using Unity.Netcode;
using UnityEngine;
using StarterAssets;

public class PlayerBump : NetworkBehaviour
{
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private CharacterController characterController;

    [SerializeField] private float deceleration = 15f;
    

    private static readonly System.Reflection.FieldInfo VerticalVelocityField =
        typeof(ThirdPersonController).GetField("_verticalVelocity",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    private Vector3 bumpVelocity; // horizontal only
    

    public void Bump(Vector3 direction, float velocity)
    {
        if (!IsOwner) return;
        if (direction.sqrMagnitude < 0.0001f) return;
        direction.y = Mathf.Max(direction.y, 0.5f);
        Vector3 impulse = direction * velocity;
        Debug.Log(impulse);
        
        
        bumpVelocity = new Vector3(impulse.x, 0f, impulse.z);
        
        if (!Mathf.Approximately(impulse.y, 0f))
            VerticalVelocityField.SetValue(thirdPersonController, impulse.y);
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (bumpVelocity == Vector3.zero)
        {
            return;
        }

        /*if (characterController.isGrounded)
        {
            bumpVelocity = new Vector3(0f, 0f, 0f);
        }*/

        characterController.Move(bumpVelocity * Time.deltaTime);

        bumpVelocity = Vector3.MoveTowards(bumpVelocity, Vector3.zero, deceleration * Time.deltaTime);
    }
}