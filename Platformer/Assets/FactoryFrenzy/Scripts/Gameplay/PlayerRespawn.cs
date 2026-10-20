
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
public class PlayerRespawn : NetworkBehaviour
{
    // ==========================================
    // NETWORK AUTHORITY
    // ==========================================

    public enum RespawnAuthority
    {
        Server,
        Owner
    }

    [Header("Network Settings")]

    [Tooltip("Must match the player's NetworkTransform authority.")]
    [SerializeField]
    private RespawnAuthority respawnAuthority =
        RespawnAuthority.Server;


    // ==========================================
    // RESPAWN SETTINGS
    // ==========================================

    [Header("Respawn Settings")]

    [SerializeField, Min(0f)]
    private float respawnCooldown = 0.5f;


    // ==========================================
    // CHECKPOINT PROGRESSION
    // ==========================================

    private int lastCheckpointOrder = -1;

    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;

    private bool hasCheckpoint;
    private float nextRespawnTime;

    public int LastCheckpointOrder =>
        lastCheckpointOrder;


    // ==========================================
    // COMPONENT REFERENCES
    // ==========================================

    private CharacterController controller;
    private Rigidbody rb;
    private NetworkTransform networkTransform;


    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        networkTransform =
            GetComponent<NetworkTransform>();
    }

    private void Start()
    {
        // Initial fallback before the player
        // reaches the first Game checkpoint.
        if (!hasCheckpoint)
        {
            checkpointPosition = transform.position;
            checkpointRotation = transform.rotation;
            hasCheckpoint = true;
        }
    }


    // ==========================================
    // MOVEMENT AUTHORITY
    // ==========================================

    public bool HasMovementAuthority()
    {
        // Allow standalone tests without Netcode.
        if (!IsSpawned)
        {
            var manager =
                Unity.Netcode.NetworkManager.Singleton;

            return manager == null ||
                   !manager.IsListening;
        }

        // In network mode, respect the configured
        // movement authority.
        if (respawnAuthority == RespawnAuthority.Server)
        {
            return IsServer;
        }

        return IsOwner;
    }


    // ==========================================
    // INITIAL SPAWN FALLBACK
    // ==========================================

    // Compatible with the optional call from
    // PlayerSpawnManager.
    public void SetCheckpoint(Transform point)
    {
        if (point == null || !HasMovementAuthority())
            return;

        // Do not overwrite an actual Game checkpoint.
        if (lastCheckpointOrder >= 0)
            return;

        checkpointPosition = point.position;
        checkpointRotation = point.rotation;

        hasCheckpoint = true;
    }


    // ==========================================
    // ACTIVATE A CHECKPOINT
    // ==========================================

    public bool TryActivateCheckpoint(
        int checkpointOrder,
        Transform respawnPoint)
    {
        if (!HasMovementAuthority())
            return false;

        if (respawnPoint == null || checkpointOrder < 0)
            return false;

        // Never downgrade checkpoint progression.
        if (checkpointOrder <= lastCheckpointOrder)
            return false;

        lastCheckpointOrder = checkpointOrder;

        checkpointPosition = respawnPoint.position;
        checkpointRotation = respawnPoint.rotation;

        hasCheckpoint = true;

        Debug.Log(
            $"Checkpoint {lastCheckpointOrder} activated " +
            $"by {gameObject.name}. " +
            $"Respawn position: {checkpointPosition}",
            this
        );

        return true;
    }


    // ==========================================
    // RESPAWN
    // ==========================================

    public void Respawn()
    {
        if (!HasMovementAuthority())
            return;

        if (!hasCheckpoint)
        {
            Debug.LogWarning(
                "PlayerRespawn: No checkpoint saved.",
                this
            );
            return;
        }

        if (Time.time < nextRespawnTime)
            return;

        nextRespawnTime =
            Time.time + respawnCooldown;

        // Disable CharacterController temporarily.
        bool controllerWasEnabled =
            controller != null &&
            controller.enabled;

        if (controllerWasEnabled)
            controller.enabled = false;

        // Reset Rigidbody velocity if dynamic.
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (IsSpawned && networkTransform != null)
        {
            // Authoritative network teleportation.
            networkTransform.Teleport(
                checkpointPosition,
                checkpointRotation,
                transform.localScale
            );
        }
        else
        {
            // Standalone fallback.
            transform.SetPositionAndRotation(
                checkpointPosition,
                checkpointRotation
            );
        }

        Physics.SyncTransforms();

        if (controllerWasEnabled)
            controller.enabled = true;

        Debug.Log(
            $"{gameObject.name} respawned at checkpoint " +
            $"{lastCheckpointOrder}",
            this
        );
    }


    // ==========================================
    // RESET FOR ANOTHER RACE
    // ==========================================

    public void ResetCheckpointProgress()
    {
        if (!HasMovementAuthority())
            return;

        lastCheckpointOrder = -1;

        checkpointPosition = transform.position;
        checkpointRotation = transform.rotation;

        hasCheckpoint = true;
        nextRespawnTime = 0f;
    }


    // ==========================================
    // DEBUG
    // ==========================================

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !hasCheckpoint)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            checkpointPosition,
            0.4f
        );
    }
}
