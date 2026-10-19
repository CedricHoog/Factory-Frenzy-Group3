using Unity.Netcode;
using UnityEngine;
using StarterAssets;

public class PlayerBump : NetworkBehaviour
{
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private CharacterController characterController;

    [Tooltip("How fast the horizontal push fades (units/sec²)")]
    [SerializeField] private float deceleration = 15f;

    private static readonly System.Reflection.FieldInfo VerticalVelocityField =
        typeof(ThirdPersonController).GetField("_verticalVelocity",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    private Vector3 bumpVelocity; // horizontal only

    public void Bump(Vector3 direction, float velocity)
    {
        if (!IsOwner) return;
        if (direction.sqrMagnitude < 0.0001f) return;

        Vector3 impulse = direction * velocity;

        // Horizontal part: handled by this script
        bumpVelocity = new Vector3(impulse.x, 0f, impulse.z);

        // Vertical part: handed to the controller, like your Bounce
        if (!Mathf.Approximately(impulse.y, 0f))
            VerticalVelocityField.SetValue(thirdPersonController, impulse.y);
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (bumpVelocity == Vector3.zero) return;

        characterController.Move(bumpVelocity * Time.deltaTime);

        bumpVelocity = Vector3.MoveTowards(bumpVelocity, Vector3.zero, deceleration * Time.deltaTime);
    }
}