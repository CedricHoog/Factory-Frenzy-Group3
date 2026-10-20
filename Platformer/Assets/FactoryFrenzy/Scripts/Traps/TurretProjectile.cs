using FactoryFrenzy.Gameplay;
using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float speed = 15f;
    [SerializeField, Min(0.1f)] private float lifetime = 4f;

    [Header("Collision")]
    [SerializeField, Min(0.01f)] private float collisionRadius = 0.12f;
    [SerializeField] private LayerMask collisionMask = ~0;

    private Vector3 direction;
    private Transform shooterRoot;
    private TurretShotEffects shotEffects;
    private float horizontalLaunch;
    private float verticalLaunch;
    private bool initialized;
    private bool hasHit;

    public void Initialize(
        Vector3 launchDirection,
        Transform shooter,
        TurretShotEffects effects,
        float horizontalForce,
        float verticalForce)
    {
        direction = launchDirection.normalized;
        shooterRoot = shooter;
        shotEffects = effects;
        horizontalLaunch = horizontalForce;
        verticalLaunch = verticalForce;

        transform.rotation = Quaternion.LookRotation(direction);
        initialized = true;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized || hasHit) return;

        float distance = speed * Time.deltaTime;
        if (distance <= 0f) return;

        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position, collisionRadius, direction,
            distance, collisionMask, QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null) continue;

            if (shooterRoot != null && hit.collider.transform.IsChildOf(shooterRoot))
                continue;

            transform.position += direction * hit.distance;
            HandleImpact(hit);
            return;
        }

        transform.position += direction * distance;
    }

    private void HandleImpact(RaycastHit hit)
    {
        if (hasHit) return;
        hasHit = true;

        var player = hit.collider.GetComponentInParent<PlayerData>();

        if (player != null && player.IsOwner &&
            player.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 push = direction;
            push.y = 0f;
            if (push.sqrMagnitude < 0.0001f) push = Vector3.forward;
            push.Normalize();

            rb.linearVelocity = Vector3.zero;
            rb.AddForce(push * horizontalLaunch + Vector3.up * verticalLaunch,
                        ForceMode.VelocityChange);
        }

        if (shotEffects != null)
            shotEffects.PlayImpact(hit.point, hit.normal);

        Destroy(gameObject);
    }
}