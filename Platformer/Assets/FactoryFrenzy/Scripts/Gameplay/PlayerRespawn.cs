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
    private float nextStartLookupTime;

    private CharacterController controller;
    private Rigidbody rb;
    private NetworkTransform networkTransform;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        networkTransform = GetComponent<NetworkTransform>();
    }

    public override void OnNetworkSpawn()
    {
        // Game might not yet be loaded when the player network object spawns.
        // Update() will retry finding the starting checkpoint after scene load.
        TryFindStartingCheckpoint();
    }

    private void Update()
    {
        // Never use the player's current position as an automatic fallback:
        // it may be outside the map or inside the FallZone.
        if (!HasMovementAuthority() || hasCheckpoint || Time.time < nextStartLookupTime)
            return;

        nextStartLookupTime = Time.time + 0.5f;
        TryFindStartingCheckpoint();
    }

    private void TryFindStartingCheckpoint()
    {
        if (!HasMovementAuthority() || hasCheckpoint)
            return;

        PlatformCheckpoint[] checkpoints =
            FindObjectsByType<PlatformCheckpoint>(FindObjectsSortMode.None);

        foreach (PlatformCheckpoint checkpoint in checkpoints)
        {
            if (checkpoint == null || !checkpoint.IsStartingCheckpoint)
                continue;

            checkpointPosition = checkpoint.GetRespawnPosition(GetPlayerSlotId());
            checkpointRotation = checkpoint.RespawnPoint.rotation;
            hasCheckpoint = true;

            Debug.Log(
                $"[RESPAWN] {name} initial start: {checkpointPosition} (player {GetPlayerSlotId()})",
                this);
            return;
        }
    }

    private ulong GetPlayerSlotId()
    {
        // Host = 0, first connected client = 1 in the usual two-player setup.
        return IsSpawned ? OwnerClientId : 0UL;
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

    // Optional explicit spawn fallback used by PlayerSpawnManager.
    // The provided Transform should already be a valid *individual* player slot.
    public void SetCheckpoint(Transform point)
    {
        if (point == null || !HasMovementAuthority() || currentCheckpoint != null)
            return;

        checkpointPosition = point.position;
        checkpointRotation = point.rotation;
        hasCheckpoint = true;
    }

    public bool TryActivateCheckpoint(PlatformCheckpoint checkpoint)
    {
        if (checkpoint == null)
            return false;

        // The server saves authoritative checkpoint positions; an owner-client
        // can still show its personal checkpoint visuals, as in the original code.
        if (IsSpawned && !IsServer && !IsOwner)
            return false;

        if (checkpoint.IsStartingCheckpoint &&
            currentCheckpoint != null &&
            !currentCheckpoint.IsStartingCheckpoint)
            return false;

        // Never let an old checkpoint replace a newer first-time checkpoint.
        if (!visitedCheckpoints.Add(checkpoint))
            return false;

        if (!IsSpawned || IsOwner)
        {
            if (currentCheckpoint != null)
                currentCheckpoint.SetVisualActive(false);

            checkpoint.SetVisualActive(true);
        }

        currentCheckpoint = checkpoint;

        if (HasMovementAuthority())
        {
            checkpointPosition = checkpoint.GetRespawnPosition(GetPlayerSlotId());
            checkpointRotation = checkpoint.RespawnPoint.rotation;
            hasCheckpoint = true;
        }

        Debug.Log($"[CHECKPOINT] {name} reached {checkpoint.name}", this);
        return true;
    }

    public void Respawn()
    {
        if (!HasMovementAuthority() || Time.time < nextRespawnTime)
            return;

        if (!hasCheckpoint)
            TryFindStartingCheckpoint();

        if (!hasCheckpoint)
        {
            // An unavailable starting checkpoint is preferable to teleporting
            // back into the FallZone. Log at most once per cooldown interval.
            nextRespawnTime = Time.time + Mathf.Max(1f, respawnCooldown);
            Debug.LogWarning(
                $"[RESPAWN] No valid starting checkpoint for {name}. " +
                "Add a starting PlatformCheckpoint to Game and assign RespawnPoint.",
                this);
            return;
        }

        nextRespawnTime = Time.time + respawnCooldown;

        bool controllerEnabled = controller != null && controller.enabled;
        if (controllerEnabled)
            controller.enabled = false;

        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = checkpointPosition;
            rb.rotation = checkpointRotation;
        }

        if (IsSpawned && networkTransform != null)
        {
            networkTransform.Teleport(
                checkpointPosition,
                checkpointRotation,
                transform.localScale);
        }
        else
        {
            transform.SetPositionAndRotation(checkpointPosition, checkpointRotation);
        }

        Physics.SyncTransforms();

        if (controllerEnabled)
            controller.enabled = true;

        Debug.Log(
            $"[RESPAWN] {name}, player {GetPlayerSlotId()} -> {checkpointPosition}",
            this);
    }

    public void ResetCheckpointProgress()
    {
        if (IsSpawned && !IsServer && !IsOwner)
            return;

        if (!IsSpawned || IsOwner)
        {
            foreach (PlatformCheckpoint checkpoint in visitedCheckpoints)
            {
                if (checkpoint != null)
                    checkpoint.ClearVisualProgress();
            }
        }

        visitedCheckpoints.Clear();
        currentCheckpoint = null;
        hasCheckpoint = false;
        nextRespawnTime = 0f;
        nextStartLookupTime = 0f;
        TryFindStartingCheckpoint();
    }
}
