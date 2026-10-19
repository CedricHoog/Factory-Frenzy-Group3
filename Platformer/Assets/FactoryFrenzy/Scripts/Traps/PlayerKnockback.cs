
using System.Collections;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("CharacterController Settings")]
    [SerializeField]
    private float knockbackDuration = 1.1f;

    [SerializeField]
    private float horizontalDeceleration = 11f;

    private Rigidbody rb;
    private CharacterController characterController;
    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        characterController =
            GetComponent<CharacterController>();
    }

    public void Launch(
        Vector3 sourcePosition,
        float horizontalSpeed,
        float verticalSpeed)
    {
        Vector3 direction =
            transform.position - sourcePosition;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = -transform.forward;
            direction.y = 0f;
        }

        direction.Normalize();

        Vector3 launchVelocity =
            direction * horizontalSpeed +
            Vector3.up * verticalSpeed;

        // Cas 1 : personnage avec Rigidbody dynamique.
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(
                launchVelocity,
                ForceMode.VelocityChange
            );

            return;
        }

        // Cas 2 : personnage avec CharacterController.
        if (characterController != null &&
            characterController.enabled)
        {
            if (knockbackCoroutine != null)
            {
                StopCoroutine(knockbackCoroutine);
            }

            knockbackCoroutine = StartCoroutine(
                PushCharacterController(launchVelocity)
            );

            return;
        }

        Debug.LogWarning(
            "Aucun Rigidbody dynamique ou " +
            "CharacterController actif trouve sur le joueur.",
            this
        );
    }

    private IEnumerator PushCharacterController(
        Vector3 velocity)
    {
        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            if (characterController == null ||
                !characterController.enabled)
            {
                break;
            }

            float dt = Time.deltaTime;

            characterController.Move(velocity * dt);

            velocity.y += Physics.gravity.y * dt;

            velocity.x = Mathf.MoveTowards(
                velocity.x,
                0f,
                horizontalDeceleration * dt
            );

            velocity.z = Mathf.MoveTowards(
                velocity.z,
                0f,
                horizontalDeceleration * dt
            );

            elapsed += dt;

            yield return null;
        }

        knockbackCoroutine = null;
    }
}
