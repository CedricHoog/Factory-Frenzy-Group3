
using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0.1f)]
    private float speed = 15f;

    [SerializeField, Min(0.1f)]
    private float lifetime = 4f;

    [Header("Collision")]
    [SerializeField, Min(0.01f)]
    private float collisionRadius = 0.12f;

    [SerializeField]
    private LayerMask collisionMask = ~0;

    private Vector3 direction;
    private Transform shooterRoot;
    private TurretShotEffects shotEffects;

    private float horizontalLaunch;
    private float verticalLaunch;

    private bool initialized;
    private bool hasHit;

    // Called by TrapShooterFSM after Instantiate.
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

        transform.rotation =
            Quaternion.LookRotation(direction);

        initialized = true;

        // Prevent projectiles from existing forever.
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized || hasHit)
            return;

        float distance = speed * Time.deltaTime;

        if (distance <= 0f)
            return;

        // Detect every collision along the movement path.
        // SphereCast prevents tunneling through thin objects.
        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,
            collisionRadius,
            direction,
            distance,
            collisionMask,
            QueryTriggerInteraction.Ignore
        );

        // Process the nearest obstacle first.
        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            // Do not hit our own turret.
            if (shooterRoot != null &&
                hit.collider.transform.IsChildOf(shooterRoot))
            {
                continue;
            }

            // Move to the actual contact position.
            transform.position +=
                direction * hit.distance;

            HandleImpact(hit);

            return;
        }

        // No collision: continue in a straight line.
        transform.position += direction * distance;
    }

    private void HandleImpact(RaycastHit hit)
    {
        if (hasHit)
            return;

        hasHit = true;

        // Find a player through the collider hierarchy.
        Transform player = FindPlayerRoot(
            hit.collider.transform
        );

        if (player != null)
        {
            PlayerKnockback knockback =
                player.GetComponent<PlayerKnockback>();

            if (knockback != null)
            {
                // Push the player away from the impact.
                Vector3 sourcePosition =
                    transform.position - direction;

                knockback.Launch(
                    sourcePosition,
                    horizontalLaunch,
                    verticalLaunch
                );
            }
            else
            {
                Debug.LogWarning(
                    "TurretProjectile: PlayerKnockback missing.",
                    player
                );
            }
        }

        // Play impact particles at the real contact point.
        // Works on players and environmental obstacles.
        if (shotEffects != null)
        {
            shotEffects.PlayImpact(
                hit.point,
                hit.normal
            );
        }

        Destroy(gameObject);
    }

    private Transform FindPlayerRoot(Transform candidate)
    {
        Transform current = candidate;

        while (current != null)
        {
            if (current.CompareTag("Player"))
                return current;

            current = current.parent;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            collisionRadius
        );
    }
}
