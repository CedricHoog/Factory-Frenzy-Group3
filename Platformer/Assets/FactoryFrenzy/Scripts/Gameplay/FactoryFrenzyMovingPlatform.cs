using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// A -> B -> A moving platform, synchronized using the server clock.
/// Waypoints are LOCAL positions relative to the platform's initial transform.
/// Each peer moves its own kinematic Rigidbody so the collider follows the platform.
/// No NetworkTransform should be attached to this object.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Rigidbody))]
public sealed class FactoryFrenzyMovingPlatform : NetworkBehaviour
{
    [Header("Waypoints (local positions)")]
    [Tooltip("Initial platform position, relative to its placement in the scene.")]
    [SerializeField] private Vector3 startPosition = Vector3.zero;

    [Tooltip("Destination, relative to the same initial scene placement.")]
    [SerializeField] private Vector3 endPosition = new Vector3(0f, 0f, 8f);

    [Header("Timeline (seconds)")]
    [Tooltip("Applied once after the platform spawns on the server.")]
    [SerializeField, Min(0f)] private float activationDelay = 5f;

    [Tooltip("Complete A -> B -> A travel time; excludes the 1-second restart pause.")]
    [SerializeField, Min(0.1f)] private float cycleDuration = 8f;

    // Intentionally fixed: not exposed in the Inspector.
    public const float RestartDelaySeconds = 1f;

    [Header("Diagnostics")]
    [Tooltip("For offline tests only. Disable for Host/Client tests.")]
    [SerializeField] private bool allowOfflinePreview = false;

    [SerializeField] private bool logNetworkState = true;

    // The server publishes both world-space endpoints to all peers. These
    // fields must not be modified directly by clients or edited during a run.
    private readonly NetworkVariable<Vector3> networkStartPosition =
        new NetworkVariable<Vector3>(Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<Vector3> networkEndPosition =
        new NetworkVariable<Vector3>(Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<float> networkActivationDelay =
        new NetworkVariable<float>(0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<float> networkCycleDuration =
        new NetworkVariable<float>(8f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    // -1 means that the server has not initialized this platform yet.
    private readonly NetworkVariable<double> networkStartTime =
        new NetworkVariable<double>(-1d,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private Rigidbody platformBody;
    private Vector3 offlineStartWorld;
    private Vector3 offlineEndWorld;
    private double offlineStartTime;
    private bool initialNetworkSnapPending;

    public Vector3 CurrentVelocity { get; private set; }
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        platformBody = GetComponent<Rigidbody>();
        platformBody.isKinematic = true;
        platformBody.useGravity = false;

        // Cache the original transform BEFORE the platform starts moving.
        offlineStartWorld = transform.TransformPoint(startPosition);
        offlineEndWorld = transform.TransformPoint(endPosition);
        offlineStartTime = Time.timeAsDouble;
    }

    private void OnValidate()
    {
        activationDelay = Mathf.Max(0f, activationDelay);
        cycleDuration = Mathf.Max(0.1f, cycleDuration);
    }

    public override void OnNetworkSpawn()
    {
        initialNetworkSnapPending = true;

        if (IsServer)
        {
            // Convert the Inspector's local endpoints to world coordinates
            // once on the SERVER, then synchronize them to every client.
            networkStartPosition.Value = transform.TransformPoint(startPosition);
            networkEndPosition.Value = transform.TransformPoint(endPosition);
            networkActivationDelay.Value = activationDelay;
            networkCycleDuration.Value = Mathf.Max(0.1f, cycleDuration);
            networkStartTime.Value = NetworkManager.ServerTime.Time;
        }

        if (logNetworkState)
        {
            Debug.Log($"[FFMovingPlatform] Spawned; server={IsServer}, " +
                      $"start={networkStartPosition.Value}, end={networkEndPosition.Value}, " +
                      $"epoch={networkStartTime.Value:F3}", this);
        }
    }

    public override void OnNetworkDespawn()
    {
        CurrentVelocity = Vector3.zero;
        IsMoving = false;
        initialNetworkSnapPending = false;
    }

    private void FixedUpdate()
    {
        bool online = IsSpawned && NetworkManager != null && NetworkManager.IsListening;
        var manager = Unity.Netcode.NetworkManager.Singleton;

        // Never animate a scene object that has not yet been spawned by NGO
        // while a network session is running.
        if (!online && manager != null && manager.IsListening)
        {
            StopMovement();
            return;
        }

        if (!online && !allowOfflinePreview)
        {
            StopMovement();
            return;
        }

        if (online && networkStartTime.Value < 0d)
        {
            StopMovement();
            return;
        }

        Vector3 a = online ? networkStartPosition.Value : offlineStartWorld;
        Vector3 b = online ? networkEndPosition.Value : offlineEndWorld;
        float delay = online ? networkActivationDelay.Value : activationDelay;
        float duration = online ? networkCycleDuration.Value : cycleDuration;
        double epoch = online ? networkStartTime.Value : offlineStartTime;
        double clock = online ? NetworkManager.ServerTime.Time : Time.timeAsDouble;

        Vector3 destination = EvaluatePosition(
            a, b, clock - epoch, delay, duration, out bool moving);

        // Late join: snap to the CURRENT position once, instead of sweeping
        // a Rigidbody through the level from its initial prefab location.
        if (online && initialNetworkSnapPending)
        {
            platformBody.position = destination;
            Physics.SyncTransforms();
            initialNetworkSnapPending = false;
            CurrentVelocity = Vector3.zero;
            IsMoving = moving;
            return;
        }

        CurrentVelocity = (destination - platformBody.position) / Time.fixedDeltaTime;
        IsMoving = moving;
        platformBody.MovePosition(destination);
    }

    private void StopMovement()
    {
        CurrentVelocity = Vector3.zero;
        IsMoving = false;
    }

    /// <summary>
    /// Smooth cycle: A -> B -> A, then exactly 1 second waiting at A.
    /// A/B are world positions. One-time activation delay applies first.
    /// </summary>
    public static Vector3 EvaluatePosition(
        Vector3 a, Vector3 b, double elapsedSeconds,
        float firstDelay, float fullCycleDuration,
        out bool isMoving)
    {
        isMoving = false;
        if (elapsedSeconds <= firstDelay)
            return a;

        double activeTime = elapsedSeconds - firstDelay;
        double duration = Math.Max(0.1d, fullCycleDuration);
        double cycleWithPause = duration + RestartDelaySeconds;
        double phase = activeTime % cycleWithPause;
        if (phase < 0d)
            phase += cycleWithPause;

        // Hold at A during the fixed 1-second restart pause.
        if (phase >= duration)
            return a;

        // 0->1 on the outward leg; 1->0 on the return leg.
        double normalized = phase / duration;
        float linearProgress = (float)(normalized <= 0.5d
            ? normalized * 2d
            : (1d - normalized) * 2d);

        // Zero velocity at A and B makes direction changes gradual.
        float smoothProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(linearProgress));
        isMoving = (b - a).sqrMagnitude > 0.000001f;
        return Vector3.LerpUnclamped(a, b, smoothProgress);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 a;
        Vector3 b;

        if (Application.isPlaying && IsSpawned && networkStartTime.Value >= 0d)
        {
            a = networkStartPosition.Value;
            b = networkEndPosition.Value;
        }
        else if (Application.isPlaying)
        {
            a = offlineStartWorld;
            b = offlineEndWorld;
        }
        else
        {
            a = transform.TransformPoint(startPosition);
            b = transform.TransformPoint(endPosition);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(a, 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(b, 0.3f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
    }
}
