using System.Collections.Generic;
using FactoryFrenzy.Gameplay;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TrapShooter : NetworkBehaviour
{
    public enum TurretState { Search, Orientation, Fire }

    [Header("Current State")]
    [SerializeField] private TurretState currentState = TurretState.Search;

    [Header("Turret References")]
    [SerializeField] private Transform rotationPivot;
    [SerializeField] private Transform firePoint;

    [Header("Visual Effects")]
    [SerializeField] private TurretShotEffects shotEffects;

    [Header("Projectile Settings")]
    [SerializeField] private TurretProjectile projectilePrefab;
    [SerializeField, Min(0f)] private float spawnOffset = 0.35f;

    [Header("Activation Settings")]
    [Tooltip("Secondes après le départ de la course.")]
    [SerializeField, Min(0f)] private float activationDelay = 5f;

    [Header("Detection Settings")]
    [SerializeField, Min(0.1f)] private float detectionRadius = 12f;

    [Header("Search Settings")]
    [SerializeField, Range(0f, 180f)] private float searchAngle = 70f;
    [SerializeField, Min(1f)] private float searchSpeed = 50f;

    [Header("Orientation Settings")]
    [SerializeField, Min(1f)] private float orientationSpeed = 120f;
    [SerializeField, Range(1f, 90f)] private float firingAngle = 6f;

    [Header("Shooting Settings")]
    [SerializeField, Min(0.1f)] private float shotInterval = 1.5f;

    [Header("Knockback Settings")]
    [SerializeField] private float horizontalLaunch = 18f;
    [SerializeField] private float verticalLaunch = 9f;

    [Header("Modern Pistols SFX")]
    [SerializeField] private AudioClip shotClip;

    private AudioSource audioSource;
    private Quaternion initialRotation;
    private int searchDirection = 1;
    private float nextShotTime;
    private PlayerData currentTarget;

    private readonly HashSet<PlayerData> playersInRange = new();
    private readonly Dictionary<PlayerData, long> entryOrder = new();
    private readonly List<PlayerData> playersToRemove = new();
    private long nextEntryNumber;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (shotEffects == null)
            shotEffects = GetComponent<TurretShotEffects>();

        if (rotationPivot == null || firePoint == null)
        {
            Debug.LogError("TrapShooter : RotationPivot ou FirePoint manquant !", this);
            enabled = false;
            return;
        }

        if (projectilePrefab == null)
            Debug.LogWarning("TrapShooter : Projectile Prefab non assigné !", this);

        initialRotation = rotationPivot.rotation;
    }

    private void Update()
    {
        // Toute la logique tourne uniquement sur le serveur
        if (!IsServer) return;

        // Activation : activationDelay secondes après le départ de la course
        var race = RaceTimer.Instance;
        if (race == null || !race.IsRunning || race.Elapsed < activationDelay)
        {
            currentState = TurretState.Search;
            return;
        }

        DetectPlayers();
        currentTarget = SelectTarget();

        // --- SEARCH ---
        if (currentTarget == null)
        {
            currentState = TurretState.Search;
            SearchForPlayers();
            return;
        }

        Vector3 aim = GetAimPoint(currentTarget);
        Vector3 direction = aim - firePoint.position;
        direction.y = 0f;

        Vector3 forward = firePoint.forward;
        forward.y = 0f;

        if (direction.sqrMagnitude < 0.001f || forward.sqrMagnitude < 0.001f)
            return;

        float signedAngle = Vector3.SignedAngle(forward, direction, Vector3.up);

        // --- ORIENTATION ---
        if (Mathf.Abs(signedAngle) > firingAngle)
        {
            currentState = TurretState.Orientation;

            float max = orientationSpeed * Time.deltaTime;
            rotationPivot.Rotate(Vector3.up, Mathf.Clamp(signedAngle, -max, max), Space.World);
            return;
        }

        // --- FIRE ---
        currentState = TurretState.Fire;

        if (Time.time >= nextShotTime)
        {
            nextShotTime = Time.time + shotInterval;

            Vector3 shotDirection = (aim - firePoint.position).normalized;
            Vector3 spawnPosition = firePoint.position + shotDirection * spawnOffset;

            FireRpc(spawnPosition, shotDirection);
        }
    }

    private void SearchForPlayers()
    {
        Quaternion limit = Quaternion.AngleAxis(searchDirection * searchAngle, Vector3.up) * initialRotation;

        rotationPivot.rotation = Quaternion.RotateTowards(
            rotationPivot.rotation, limit, searchSpeed * Time.deltaTime);

        if (Quaternion.Angle(rotationPivot.rotation, limit) < 0.5f)
            searchDirection *= -1;
    }

    // Joueurs dans le rayon, avec leur ordre d'entrée (le premier entré est ciblé)
    private void DetectPlayers()
    {
        playersInRange.Clear();

        float radiusSqr = detectionRadius * detectionRadius;

        foreach (var p in PlayerData.All)
        {
            if (p == null) continue;

            if ((p.transform.position - rotationPivot.position).sqrMagnitude > radiusSqr)
                continue;

            if (playersInRange.Add(p) && !entryOrder.ContainsKey(p))
                entryOrder.Add(p, ++nextEntryNumber);
        }

        playersToRemove.Clear();
        foreach (var entry in entryOrder)
        {
            if (entry.Key == null || !playersInRange.Contains(entry.Key))
                playersToRemove.Add(entry.Key);
        }
        foreach (var p in playersToRemove)
            entryOrder.Remove(p);
    }

    private PlayerData SelectTarget()
    {
        PlayerData selected = null;
        long oldest = long.MaxValue;

        foreach (var entry in entryOrder)
        {
            if (entry.Key != null && entry.Value < oldest)
            {
                selected = entry.Key;
                oldest = entry.Value;
            }
        }
        return selected;
    }

    private Vector3 GetAimPoint(PlayerData player)
    {
        var col = player.GetComponentInChildren<Collider>();
        return col != null ? col.bounds.center : player.transform.position + Vector3.up;
    }

    // Exécuté chez tout le monde : projectile, effets et son sont locaux
    [Rpc(SendTo.Everyone)]
    private void FireRpc(Vector3 spawnPosition, Vector3 shotDirection)
    {
        if (projectilePrefab != null)
        {
            var projectile = Instantiate(
                projectilePrefab, spawnPosition, Quaternion.LookRotation(shotDirection));

            projectile.Initialize(
                shotDirection, transform, shotEffects, horizontalLaunch, verticalLaunch);
        }

        if (shotEffects != null)
            shotEffects.PlayShot();

        if (shotClip != null && audioSource != null)
            audioSource.PlayOneShot(shotClip);
    }

    private void OnDrawGizmosSelected()
    {
        if (rotationPivot == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(rotationPivot.position, detectionRadius);

        if (firePoint == null) return;

        Vector3 forward = firePoint.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) return;
        forward.Normalize();

        Vector3 left = Quaternion.AngleAxis(-firingAngle, Vector3.up) * forward;
        Vector3 right = Quaternion.AngleAxis(firingAngle, Vector3.up) * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(firePoint.position, left * detectionRadius);
        Gizmos.DrawRay(firePoint.position, right * detectionRadius);

        if (Application.isPlaying && currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, GetAimPoint(currentTarget));
        }
    }
}