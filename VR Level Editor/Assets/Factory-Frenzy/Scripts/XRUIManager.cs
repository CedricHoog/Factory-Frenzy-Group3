using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class XRUIManager : MonoBehaviour
{
    [FormerlySerializedAs("XRUI")]
    [SerializeField] private GameObject xrUIPrefab;
    [SerializeField] private Transform playerHead;
    [SerializeField] private InputActionProperty moveAction;
    [SerializeField, Min(0.1f)] private float distance = 1f;
    [SerializeField] private float heightOffset = -0.1f;

    private GameObject instance;

    private void Awake()
    {
        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;

        if (playerHead == null || xrUIPrefab == null)
        {
            Debug.LogError("XRUIManager: playerHead and/or xrUIPrefab not assigned.", this);
            enabled = false;
        }
    }

    private void Start() => ShowXRUI();

    private void OnEnable()
    {
        if (moveAction.action == null) return;
        moveAction.action.Enable();
        moveAction.action.performed += OnMovePressed;
    }

    private void OnDisable()
    {
        if (moveAction.action == null) return;
        moveAction.action.performed -= OnMovePressed;
        moveAction.action.Disable();
    }

    private void OnDestroy() => DestroyXRUI();

    private void OnMovePressed(InputAction.CallbackContext ctx) => ShowXRUI();

    /// Spawns the UI the first time, then only repositions it.
    public void ShowXRUI()
    {
        if (!enabled) return;

        GetTargetPose(out Vector3 position, out Quaternion rotation);

        if (instance == null)
            instance = Instantiate(xrUIPrefab, position, rotation);
        else
            instance.transform.SetPositionAndRotation(position, rotation);
    }

    public void DestroyXRUI()
    {
        if (instance != null) Destroy(instance);
        instance = null;
    }

    private void GetTargetPose(out Vector3 position, out Quaternion rotation)
    {
        Vector3 forward = Vector3.ProjectOnPlane(playerHead.forward, Vector3.up);

        // Looking straight up/down: the flattened forward vanishes, so fall back to head.up
        if (forward.sqrMagnitude < 0.0001f)
        {
            Vector3 up = playerHead.forward.y > 0f ? -playerHead.up : playerHead.up;
            forward = Vector3.ProjectOnPlane(up, Vector3.up);
        }
        forward.Normalize();

        Vector3 headPos = playerHead.position;
        position = headPos + forward * distance;
        position.y = headPos.y + heightOffset;
        rotation = Quaternion.LookRotation(forward);
    }
}