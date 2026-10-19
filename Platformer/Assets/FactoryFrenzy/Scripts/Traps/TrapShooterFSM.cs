
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TrapShooterFSM : MonoBehaviour
{
    // ==========================================
    // FINITE STATE MACHINE
    // ==========================================

    public enum TurretState
    {
        Search,
        Orientation,
        Fire
    }

    [Header("Current State")]
    [SerializeField]
    private TurretState currentState = TurretState.Search;


    // ==========================================
    // REFERENCES
    // ==========================================

    [Header("Turret References")]

    [SerializeField]
    private Transform rotationPivot;

    [SerializeField]
    private Transform firePoint;

    [Header("Visual Effects")]

    [SerializeField]
    private TurretShotEffects shotEffects;


    // ==========================================
    // PROJECTILE
    // ==========================================

    [Header("Projectile Settings")]

    [SerializeField]
    private TurretProjectile projectilePrefab;

    [SerializeField, Min(0f)]
    private float spawnOffset = 0.35f;


    // ==========================================
    // DETECTION
    // ==========================================

    [Header("Detection Settings")]

    [SerializeField, Min(0.1f)]
    private float detectionRadius = 12f;


    // ==========================================
    // SEARCH
    // ==========================================

    [Header("Search Settings")]

    [SerializeField, Range(0f, 180f)]
    private float searchAngle = 70f;

    [SerializeField, Min(1f)]
    private float searchSpeed = 50f;


    // ==========================================
    // ORIENTATION
    // ==========================================

    [Header("Orientation Settings")]

    [SerializeField, Min(1f)]
    private float orientationSpeed = 120f;

    [SerializeField, Range(1f, 90f)]
    private float firingAngle = 6f;


    // ==========================================
    // SHOOTING
    // ==========================================

    [Header("Shooting Settings")]

    [SerializeField, Min(0.1f)]
    private float shotInterval = 1.5f;


    // ==========================================
    // KNOCKBACK
    // ==========================================

    [Header("Knockback Settings")]

    [SerializeField]
    private float horizontalLaunch = 18f;

    [SerializeField]
    private float verticalLaunch = 9f;


    // ==========================================
    // AUDIO
    // ==========================================

    [Header("Modern Pistols SFX")]

    [SerializeField]
    private AudioClip shotClip;

    private AudioSource audioSource;


    // ==========================================
    // PRIVATE VARIABLES
    // ==========================================

    private Quaternion initialRotation;

    private int searchDirection = 1;

    private float nextShotTime;

    private Transform currentTarget;

    // Players currently inside detection radius.
    private readonly HashSet<Transform> playersInRange =
        new HashSet<Transform>();

    // Keep track of the order players enter the zone.
    private readonly Dictionary<Transform, long> entryOrder =
        new Dictionary<Transform, long>();

    private readonly List<Transform> playersToRemove =
        new List<Transform>();

    private long nextEntryNumber;


    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Automatically find shot effects if needed.
        if (shotEffects == null)
        {
            shotEffects =
                GetComponent<TurretShotEffects>();
        }

        // Check required references.
        if (rotationPivot == null || firePoint == null)
        {
            Debug.LogError(
                "TrapShooterFSM: RotationPivot or FirePoint missing!",
                this
            );

            enabled = false;
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning(
                "TrapShooterFSM: Projectile Prefab not assigned!",
                this
            );
        }

        if (shotEffects == null)
        {
            Debug.LogWarning(
                "TrapShooterFSM: TurretShotEffects missing!",
                this
            );
        }

        initialRotation = rotationPivot.rotation;

        currentState = TurretState.Search;
    }


    // ==========================================
    // MAIN FSM UPDATE
    // ==========================================

    private void Update()
    {
        // Detect nearby players.
        DetectPlayers();

        // Select player with highest entry priority.
        currentTarget = SelectTarget();


        // ======================================
        // STATE 1 : SEARCH
        // ======================================

        if (currentTarget == null)
        {
            currentState = TurretState.Search;

            SearchForPlayers();

            return;
        }


        // ======================================
        // CALCULATE TARGET DIRECTION
        // ======================================

        Vector3 targetPosition =
            GetAimPoint(currentTarget);

        Vector3 direction =
            targetPosition - firePoint.position;

        // Only rotate horizontally.
        direction.y = 0f;

        Vector3 forward = firePoint.forward;
        forward.y = 0f;

        if (direction.sqrMagnitude < 0.001f ||
            forward.sqrMagnitude < 0.001f)
        {
            return;
        }

        float signedAngle = Vector3.SignedAngle(
            forward,
            direction,
            Vector3.up
        );


        // ======================================
        // STATE 2 : ORIENTATION
        // ======================================

        if (Mathf.Abs(signedAngle) > firingAngle)
        {
            currentState = TurretState.Orientation;

            float rotationStep = Mathf.Clamp(
                signedAngle,
                -orientationSpeed * Time.deltaTime,
                orientationSpeed * Time.deltaTime
            );

            rotationPivot.Rotate(
                Vector3.up,
                rotationStep,
                Space.World
            );

            return;
        }


        // ======================================
        // STATE 3 : FIRE
        // ======================================

        currentState = TurretState.Fire;

        // Respect shooting cooldown.
        if (Time.time >= nextShotTime)
        {
            FireAtPlayer(currentTarget);

            nextShotTime =
                Time.time + shotInterval;
        }
    }


    // ==========================================
    // SEARCH STATE
    // ==========================================

    private void SearchForPlayers()
    {
        // Search limit on one side.
        Quaternion searchLimit =
            Quaternion.AngleAxis(
                searchDirection * searchAngle,
                Vector3.up
            ) * initialRotation;

        // Smooth rotation.
        rotationPivot.rotation =
            Quaternion.RotateTowards(
                rotationPivot.rotation,
                searchLimit,
                searchSpeed * Time.deltaTime
            );

        // Reverse search direction at limit.
        if (Quaternion.Angle(
            rotationPivot.rotation,
            searchLimit
        ) < 0.5f)
        {
            searchDirection *= -1;
        }
    }


    // ==========================================
    // PLAYER DETECTION
    // ==========================================

    private void DetectPlayers()
    {
        playersInRange.Clear();

        Collider[] colliders = Physics.OverlapSphere(
            rotationPivot.position,
            detectionRadius,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in colliders)
        {
            Transform player =
                FindPlayerRoot(hit.transform);

            if (player == null)
                continue;

            // Register new players in entry order.
            if (playersInRange.Add(player) &&
                !entryOrder.ContainsKey(player))
            {
                entryOrder.Add(
                    player,
                    ++nextEntryNumber
                );
            }
        }

        // Remove players that left detection range.
        playersToRemove.Clear();

        foreach (var entry in entryOrder)
        {
            if (entry.Key == null ||
                !playersInRange.Contains(entry.Key))
            {
                playersToRemove.Add(entry.Key);
            }
        }

        foreach (Transform player in playersToRemove)
        {
            entryOrder.Remove(player);
        }
    }


    // ==========================================
    // FIND PLAYER ROOT
    // ==========================================

    private Transform FindPlayerRoot(Transform candidate)
    {
        Transform current = candidate;

        while (current != null)
        {
            if (current.CompareTag("Player"))
            {
                return current;
            }

            current = current.parent;
        }

        return null;
    }


    // ==========================================
    // TARGET SELECTION
    // ==========================================

    private Transform SelectTarget()
    {
        Transform selected = null;

        long oldestEntry = long.MaxValue;

        foreach (var entry in entryOrder)
        {
            if (entry.Key != null &&
                entry.Value < oldestEntry)
            {
                selected = entry.Key;

                oldestEntry = entry.Value;
            }
        }

        return selected;
    }


    // ==========================================
    // AIM POINT
    // ==========================================

    private Vector3 GetAimPoint(Transform player)
    {
        // Aim at the center of the player's collider.
        Collider playerCollider =
            player.GetComponent<Collider>();

        if (playerCollider == null)
        {
            playerCollider =
                player.GetComponentInChildren<Collider>();
        }

        if (playerCollider != null)
        {
            return playerCollider.bounds.center;
        }

        // Fallback when the player has no collider.
        return player.position + Vector3.up;
    }


    // ==========================================
    // FIRE PROJECTILE
    // ==========================================

    private void FireAtPlayer(Transform player)
    {
        if (player == null || projectilePrefab == null)
        {
            return;
        }

        // ======================================
        // 1. CALCULATE SHOT DIRECTION
        // ======================================

        Vector3 targetPosition =
            GetAimPoint(player);

        Vector3 shotDirection =
            targetPosition - firePoint.position;

        if (shotDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        shotDirection.Normalize();


        // ======================================
        // 2. CALCULATE SPAWN POSITION
        // ======================================

        Vector3 spawnPosition =
            firePoint.position +
            shotDirection * spawnOffset;


        // ======================================
        // 3. CREATE PROJECTILE
        // ======================================

        TurretProjectile projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(shotDirection)
        );


        // ======================================
        // 4. INITIALIZE PROJECTILE
        // ======================================

        projectile.Initialize(
            shotDirection,
            transform,
            shotEffects,
            horizontalLaunch,
            verticalLaunch
        );

        // The projectile now handles:
        // - Movement
        // - Collision detection
        // - Player knockback on impact
        // - Impact particle effect
        // - Automatic destruction


        // ======================================
        // 5. VISUAL SHOT EFFECTS
        // ======================================

        if (shotEffects != null)
        {
            // Triggers:
            // - Muzzle Flash
            // - Cannon Recoil
            // - Smoke

            shotEffects.PlayShot();
        }


        // ======================================
        // 6. MODERN PISTOLS SFX
        // ======================================

        if (shotClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(shotClip);
        }
    }


    // ==========================================
    // DEBUG GIZMOS
    // ==========================================

    private void OnDrawGizmosSelected()
    {
        if (rotationPivot == null)
        {
            return;
        }

        // Detection radius.
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            rotationPivot.position,
            detectionRadius
        );

        if (firePoint == null)
        {
            return;
        }

        // Horizontal firing direction.
        Vector3 forward = firePoint.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
        {
            return;
        }

        forward.Normalize();

        // Left firing limit.
        Vector3 left = Quaternion.AngleAxis(
            -firingAngle,
            Vector3.up
        ) * forward;

        // Right firing limit.
        Vector3 right = Quaternion.AngleAxis(
            firingAngle,
            Vector3.up
        ) * forward;

        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(
            firePoint.position,
            left * detectionRadius
        );

        Gizmos.DrawRay(
            firePoint.position,
            right * detectionRadius
        );

        // Current target direction.
        if (Application.isPlaying &&
            currentTarget != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(
                firePoint.position,
                GetAimPoint(currentTarget)
            );
        }
    }
}
