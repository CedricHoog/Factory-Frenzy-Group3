
using FactoryFrenzy.Gameplay;
using UnityEngine;

public class Fan : MonoBehaviour
{
    [Header("Wind")]
    [SerializeField] private float windAcceleration = 50f;

    [Header("Fan Rotation")]
    [SerializeField] private Transform _fanRotation;
    [SerializeField] private float _fanRotateSpeed = 1000f;

    private void OnTriggerStay(Collider other)
    {
        var player = other.GetComponentInParent<PlayerData>();

        if (player == null || !player.IsOwner)
            return;

        if (!player.TryGetComponent<Rigidbody>(out var rb))
            return;

        Vector3 dir = transform.forward;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        dir.Normalize();
        rb.AddForce(dir * windAcceleration, ForceMode.Acceleration);
    }

    private void Update()
    {
        if (_fanRotation != null)
        {
            _fanRotation.Rotate(
                Vector3.up,
                _fanRotateSpeed * Time.deltaTime,
                Space.Self
            );
        }
    }
}
