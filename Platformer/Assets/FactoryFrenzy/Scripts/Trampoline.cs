using System.Collections;
using FactoryFrenzy.Gameplay;
using Unity.Netcode;
using UnityEngine;

public class Trampoline : NetworkBehaviour
{
    [SerializeField] private float bounceVelocity = 10f;

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

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerData>();
        if (player == null || !player.IsOwner) return;

        player.Bounce(bounceVelocity);
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
        float bot = target.localScale.z;
        float top = _baseScale.z * stretchMultiplier;

        for (float t = 0f; t < 1f; t += Time.deltaTime / stretchDuration / 2)
        {
            SetScaleZ(Mathf.Lerp(bot, top, t));
            yield return null;
        }
        SetScaleZ(top);

        for (float t = 0f; t < 1f; t += Time.deltaTime / stretchDuration / 2)
        {
            SetScaleZ(Mathf.Lerp(top, _baseScale.z, t));
            yield return null;
        }
        SetScaleZ(_baseScale.z);

        _anim = null;
    }

    private void SetScaleZ(float z)
    {
        Vector3 s = target.localScale;
        s.z = z;
        target.localScale = s;
    }
}