using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlatformCheckpoint : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool isStartingCheckpoint;

    [Header("Visuals (not required for the starting platform)")]
    [SerializeField] private Renderer checkpointPadRenderer;
    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material activeMaterial;

    [Header("Runtime state — do not set these manually")]
    [SerializeField] private bool isActive;
    [SerializeField] private bool hasBeenReached;

    public bool IsActive => isActive;
    public bool HasBeenReached => hasBeenReached;
    public bool IsStartingCheckpoint => isStartingCheckpoint;
    public Transform RespawnPoint => respawnPoint != null ? respawnPoint : transform;

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
        // Also detects players spawned directly inside the starting trigger.
        TryRegister(other);
    }

    private void TryRegister(Collider other)
    {
        PlayerRespawn player = other.GetComponentInParent<PlayerRespawn>();
        if (player == null)
            return;

        player.TryActivateCheckpoint(this);
    }

    // Called by PlayerRespawn for the LOCAL player's checkpoint visuals.
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

        // An old checkpoint stays green even after it is deactivated.
        Material wanted = hasBeenReached ? activeMaterial : baseMaterial;
        if (wanted != null)
            checkpointPadRenderer.sharedMaterial = wanted;
    }

    private void OnDrawGizmosSelected()
    {
        Transform point = RespawnPoint;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(point.position, 0.3f);
    }
}
