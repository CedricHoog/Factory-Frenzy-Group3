using System.Collections;
using FactoryFrenzy.Gameplay;
using Unity.Netcode;
using UnityEngine;

public class Trampoline : NetworkBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float stretchMultiplier = 2f;
    [SerializeField] private float stretchDuration = 0.2f;

    private Coroutine _anim;
    private Vector3 _baseScale;

    private void Awake()
    {
        if (target == null) target = transform;
        _baseScale = target.localScale;
    }

    [SerializeField] private float bounceVelocity = 10f;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerData>();
        if (player == null || !player.IsOwner) return;

        var rb = player.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;

        rb.AddForce(Vector3.up * bounceVelocity, ForceMode.VelocityChange);

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
        float bot = target.localScale.y;
        float top = _baseScale.y * stretchMultiplier;

        for (float t = 0f; t < 1f; t += Time.deltaTime / stretchDuration / 2)
        {
            SetScaleY(Mathf.Lerp(bot, top, t));
            yield return null;
        }
        SetScaleY(top);

        for (float t = 0f; t < 1f; t += Time.deltaTime / stretchDuration / 2)
        {
            SetScaleY(Mathf.Lerp(top, _baseScale.y, t));
            yield return null;
        }
        SetScaleY(_baseScale.y);

        _anim = null;
    }

    private void SetScaleY(float y)
    {
        Vector3 s = target.localScale;
        s.y = y;
        target.localScale = s;
    }
}