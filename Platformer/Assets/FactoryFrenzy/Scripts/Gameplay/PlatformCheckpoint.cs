using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlatformCheckpoint : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool isStartingCheckpoint;

    [Tooltip("Distance between the two player respawn positions, in world units.")]
    [SerializeField, Min(0.5f)] private float respawnSlotSpacing = 2f;

    [Header("Visuals (not required for the starting platform)")]
    [SerializeField] private Renderer checkpointPadRenderer;
    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material activeMaterial;

    [Header("Runtime state - do not set these manually")]
    [SerializeField] private bool isActive;
    [SerializeField] private bool hasBeenReached;

    public bool IsActive => isActive;
    public bool HasBeenReached => hasBeenReached;
    public bool IsStartingCheckpoint => isStartingCheckpoint;
    public Transform RespawnPoint => respawnPoint != null ? respawnPoint : transform;

    // This implementation assigns separate slots for TWO players.
    // For three or more players, replace this with a spawn-slot allocator.
    public Vector3 GetRespawnPosition(ulong ownerClientId)
    {
        float side = (ownerClientId % 2UL == 0UL) ? -0.5f : 0.5f;
        return RespawnPoint.position + RespawnPoint.right * (side * respawnSlotSpacing);
    }

    private void Reset()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
    }

    private void Awake()
    {
        isActive = false;
        hasBeenReached = false;
        UpdatePadMaterial();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryRegister(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Also detects players spawned already inside the starting zone.
        TryRegister(other);
    }

    private void TryRegister(Collider other)
    {
        PlayerRespawn player = other.GetComponentInParent<PlayerRespawn>();
        if (player == null)
            return;

        player.TryActivateCheckpoint(this);
    }

    public void SetVisualActive(bool active)
    {
        isActive = active;
        if (active)
            hasBeenReached = true;

        UpdatePadMaterial();
    }

    public void ClearVisualProgress()
    {
        isActive = false;
        hasBeenReached = false;
        UpdatePadMaterial();
    }

    private void UpdatePadMaterial()
    {
        if (checkpointPadRenderer == null)
            return;

        Material wanted = hasBeenReached ? activeMaterial : baseMaterial;
        if (wanted != null)
            checkpointPadRenderer.sharedMaterial = wanted;
    }

    private void OnDrawGizmosSelected()
    {
        Transform point = RespawnPoint;
        float halfSpacing = respawnSlotSpacing * 0.5f;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(point.position, 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(point.position - point.right * halfSpacing, 0.25f);
        Gizmos.DrawWireSphere(point.position + point.right * halfSpacing, 0.25f);
        Gizmos.DrawLine(
            point.position - point.right * halfSpacing,
            point.position + point.right * halfSpacing);
    }
}
