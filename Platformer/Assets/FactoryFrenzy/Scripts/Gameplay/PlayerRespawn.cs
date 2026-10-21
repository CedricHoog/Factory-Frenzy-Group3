using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
public class PlayerRespawn : NetworkBehaviour
{
    public enum RespawnAuthority
    {
        Server,
        Owner
    }

    [Header("Network settings")]
    [SerializeField] private RespawnAuthority respawnAuthority = RespawnAuthority.Server;

    [Header("Respawn settings")]
    [SerializeField, Min(0f)] private float respawnCooldown = 0.5f;

    [Header("Runtime checkpoint progression")]
    [SerializeField] private PlatformCheckpoint currentCheckpoint;

    private readonly HashSet<PlatformCheckpoint> visitedCheckpoints = new();
    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;
    private bool hasCheckpoint;
    private float nextRespawnTime;

    private CharacterController controller;
    private Rigidbody rb;
    private NetworkTransform networkTransform;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        networkTransform = GetComponent<NetworkTransform>();
    }

    private void Start()
    {
        // Fallback until the starting checkpoint is detected in Game.
        if (!hasCheckpoint)
        {
            checkpointPosition = transform.position;
            checkpointRotation = transform.rotation;
            hasCheckpoint = true;
        }
    }

    public bool HasMovementAuthority()
    {
        if (!IsSpawned)
        {
            var manager = Unity.Netcode.NetworkManager.Singleton;
            return manager == null || !manager.IsListening;
        }

        return respawnAuthority == RespawnAuthority.Server ? IsServer : IsOwner;
    }

    // Optional initial spawn fallback; it never replaces an activated checkpoint.
    public void SetCheckpoint(Transform point)
    {
        if (point == null || !HasMovementAuthority() || currentCheckpoint != null)
            return;

        checkpointPosition = point.position;
        checkpointRotation = point.rotation;
        hasCheckpoint = true;
    }

    // No checkpoint numbers: the first visit to any new platform advances progress.
    public bool TryActivateCheckpoint(PlatformCheckpoint checkpoint)
    {
        if (checkpoint == null)
            return false;

        // Only the server or the local owner tracks this player's progression.
        if (IsSpawned && !IsServer && !IsOwner)
            return false;

        // Reaching Start again must not override a checkpoint further into the race.
        if (checkpoint.IsStartingCheckpoint && visitedCheckpoints.Count > 0)
            return false;

        // Do not reactivate an older checkpoint when the player revisits it.
        if (!visitedCheckpoints.Add(checkpoint))
            return false;

        // Each client changes visuals for its OWN player only.
        // On the server, remote players' checkpoints should not recolor the host's view.
        if (!IsSpawned || IsOwner)
        {
            if (currentCheckpoint != null)
                currentCheckpoint.SetVisualActive(false);

            checkpoint.SetVisualActive(true);
        }

        currentCheckpoint = checkpoint;

        // Save the destination only where the NetworkTransform can be teleported.
        if (HasMovementAuthority())
        {
            checkpointPosition = checkpoint.RespawnPoint.position;
            checkpointRotation = checkpoint.RespawnPoint.rotation;
            hasCheckpoint = true;
        }

        Debug.Log($"[CHECKPOINT] {gameObject.name} reached {checkpoint.name}", this);
        return true;
    }

    public void Respawn()
    {
        if (!HasMovementAuthority() || !hasCheckpoint || Time.time < nextRespawnTime)
            return;

        nextRespawnTime = Time.time + respawnCooldown;

        bool controllerEnabled = controller != null && controller.enabled;
        if (controllerEnabled)
            controller.enabled = false;

        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (IsSpawned && networkTransform != null)
        {
            networkTransform.Teleport(
                checkpointPosition,
                checkpointRotation,
                transform.localScale
            );
        }
        else
        {
            transform.SetPositionAndRotation(checkpointPosition, checkpointRotation);
        }

        Physics.SyncTransforms();

        if (controllerEnabled)
            controller.enabled = true;

        Debug.Log($"[RESPAWN] {gameObject.name} at {checkpointPosition}", this);
    }

    // Call when a new race starts and the same player object is reused.
    public void ResetCheckpointProgress()
    {
        if (IsSpawned && !IsServer && !IsOwner)
            return;

        if (!IsSpawned || IsOwner)
        {
            foreach (PlatformCheckpoint checkpoint in visitedCheckpoints)
                if (checkpoint != null)
                    checkpoint.ClearVisualProgress();
        }

        visitedCheckpoints.Clear();
        currentCheckpoint = null;
        checkpointPosition = transform.position;
        checkpointRotation = transform.rotation;
        hasCheckpoint = true;
        nextRespawnTime = 0f;
    }
}
