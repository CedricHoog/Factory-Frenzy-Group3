using System.Collections;
using FactoryFrenzy.Gameplay;
using Unity.Netcode;
using UnityEngine;

public class Bumper : NetworkBehaviour
{
    [Header("Pouss�e")]
    [SerializeField] private float bumpForce = 8f;     // vitesse horizontale en m/s
    [SerializeField] private float upwardBoost = 2f;   // petit saut pour d�coller du sol
    [SerializeField] private float cooldown = 0.3f;

    [Header("Animation (�tirement en Z)")]
    [SerializeField] private Transform target;
    [SerializeField] private float stretchMultiplier = 1.3f;
    [SerializeField] private float stretchDuration = 0.2f;

    private Coroutine _anim;
    private Vector3 _baseScale;
    private float _lastBump;

    private void Awake()
    {
        if (target == null) target = transform;
        _baseScale = target.localScale;
    }

    private void OnTriggerEnter(Collider other) => TryBump(other);

    private void TryBump(Collider other)
    {
        // Seul le propri�taire du joueur applique la force
        var player = other.GetComponentInParent<PlayerData>();
        if (player == null || !player.IsOwner) return;

        if (Time.time - _lastBump < cooldown) return;
        _lastBump = Time.time;

        if (!player.TryGetComponent<Rigidbody>(out var rb)) return;

        // Direction horizontale : du centre du bumper vers le joueur
        Vector3 dir = rb.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = -player.transform.forward;
        dir.Normalize();

        // On remplace la vitesse actuelle pour une pouss�e constante
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dir * bumpForce + Vector3.up * upwardBoost, ForceMode.VelocityChange);

        PlayAnimRpc();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayAnimRpc()
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(StretchAndReturn());
    }

    private IEnumerator StretchAndReturn()
    {
        Vector3 from = target.localScale;
        Vector3 peak = new Vector3(
            _baseScale.x * stretchMultiplier,
            _baseScale.y * stretchMultiplier,
            _baseScale.z * stretchMultiplier);
        float half = stretchDuration / 2f;

        for (float t = 0f; t < 1f; t += Time.deltaTime / half)
        {
            target.localScale = Vector3.Lerp(from, peak, t);
            yield return null;
        }
        for (float t = 0f; t < 1f; t += Time.deltaTime / half)
        {
            target.localScale = Vector3.Lerp(peak, _baseScale, t);
            yield return null;
        }

        target.localScale = _baseScale;
        _anim = null;
    }

    private void SetScaleZ(float z)
    {
        Vector3 s = target.localScale;
        s.z = z;
        target.localScale = s;
    }
}