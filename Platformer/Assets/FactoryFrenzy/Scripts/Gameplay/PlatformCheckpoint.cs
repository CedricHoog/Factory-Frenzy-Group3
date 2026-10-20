
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlatformCheckpoint : MonoBehaviour
{
    // ==========================================
    // CHECKPOINT SETTINGS
    // ==========================================

    [Header("Checkpoint Settings")]

    [Tooltip("0 = Start, 1 = First checkpoint, etc.")]
    [SerializeField, Min(0)]
    private int checkpointOrder = 1;

    [Tooltip("Where the player respawns.")]
    [SerializeField]
    private Transform respawnPoint;


    // ==========================================
    // DEFAULT PHYSICS CONFIGURATION
    // ==========================================

    private void Reset()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();

        body.useGravity = false;
        body.isKinematic = true;
    }


    // ==========================================
    // TRIGGER EVENTS
    // ==========================================

    private void OnTriggerEnter(Collider other)
    {
        RegisterCheckpoint(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Also handles a player already standing
        // inside the trigger on scene load.
        RegisterCheckpoint(other);
    }


    // ==========================================
    // SAVE PLAYER PROGRESSION
    // ==========================================

    private void RegisterCheckpoint(Collider other)
    {
        PlayerRespawn player =
            other.GetComponentInParent<PlayerRespawn>();

        if (player == null)
            return;

        if (!player.HasMovementAuthority())
            return;

        Transform point =
            respawnPoint != null
                ? respawnPoint
                : transform;

        player.TryActivateCheckpoint(
            checkpointOrder,
            point
        );
    }


    // ==========================================
    // EDITOR GIZMOS
    // ==========================================

    private void OnDrawGizmosSelected()
    {
        Transform point =
            respawnPoint != null
                ? respawnPoint
                : transform;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            point.position,
            0.3f
        );
    }
}
