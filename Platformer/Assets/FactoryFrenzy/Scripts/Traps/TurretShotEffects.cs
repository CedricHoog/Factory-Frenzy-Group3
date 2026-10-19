
using System.Collections;
using UnityEngine;

public class TurretShotEffects : MonoBehaviour
{
    // ==========================================
    // REFERENCES
    // ==========================================

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform recoilBody;

    // ==========================================
    // PARTICLE EFFECTS
    // ==========================================

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem smokeFX;
    [SerializeField] private ParticleSystem impactPrefab;

    // ==========================================
    // RECOIL SETTINGS
    // ==========================================

    [Header("Recoil Settings")]

    [SerializeField, Min(0f)]
    private float recoilDistance = 0.20f;

    [SerializeField, Min(0.01f)]
    private float recoilBackTime = 0.06f;

    [SerializeField, Min(0.01f)]
    private float recoilReturnTime = 0.18f;

    // ==========================================
    // AUDIO
    // ==========================================

    [Header("Optional Impact Sound")]
    [SerializeField] private AudioClip impactClip;

    [SerializeField, Range(0f, 1f)]
    private float impactVolume = 0.5f;

    // ==========================================
    // PRIVATE VARIABLES
    // ==========================================

    private Vector3 initialLocalPosition;
    private Coroutine recoilCoroutine;
    private bool recoilInitialized;

    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void Awake()
    {
        if (recoilBody != null)
        {
            initialLocalPosition = recoilBody.localPosition;
            recoilInitialized = true;
        }

        if (firePoint == null)
        {
            Debug.LogWarning(
                "TurretShotEffects: FirePoint is missing.",
                this
            );
        }
    }

    // ==========================================
    // SHOT EFFECTS
    // Called when the turret fires a projectile.
    // ==========================================

    public void PlayShot()
    {
        // 1. Muzzle flash
        PlayParticle(muzzleFlash);

        // 2. Smoke
        PlayParticle(smokeFX);

        // 3. Cannon recoil
        StartRecoil();
    }

    // ==========================================
    // PARTICLE CONTROL
    // ==========================================

    private void PlayParticle(ParticleSystem particle)
    {
        if (particle == null)
            return;

        particle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        particle.Play(true);
    }

    // ==========================================
    // RECOIL
    // ==========================================

    private void StartRecoil()
    {
        if (recoilBody == null || firePoint == null)
            return;

        if (recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
        }

        recoilBody.localPosition = initialLocalPosition;

        recoilCoroutine = StartCoroutine(
            AnimateRecoil()
        );
    }

    private IEnumerator AnimateRecoil()
    {
        // Move the cannon backwards.
        Vector3 worldOffset =
            -firePoint.forward.normalized * recoilDistance;

        // Convert world displacement into local coordinates.
        Vector3 localOffset =
            recoilBody.parent != null
            ? recoilBody.parent.InverseTransformVector(worldOffset)
            : worldOffset;

        Vector3 backPosition =
            initialLocalPosition + localOffset;

        // Fast backward movement.
        yield return MoveRecoil(
            initialLocalPosition,
            backPosition,
            recoilBackTime
        );

        // Slower return movement.
        yield return MoveRecoil(
            backPosition,
            initialLocalPosition,
            recoilReturnTime
        );

        recoilBody.localPosition = initialLocalPosition;

        recoilCoroutine = null;
    }

    private IEnumerator MoveRecoil(
        Vector3 from,
        Vector3 to,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            t = Mathf.SmoothStep(0f, 1f, t);

            recoilBody.localPosition =
                Vector3.Lerp(from, to, t);

            yield return null;
        }

        recoilBody.localPosition = to;
    }

    // ==========================================
    // PROJECTILE IMPACT
    // Called ONLY when a projectile hits something.
    // ==========================================

    public void PlayImpact(
        Vector3 position,
        Vector3 normal)
    {
        // 1. Impact particles.
        if (impactPrefab != null)
        {
            Quaternion rotation = Quaternion.identity;

            if (normal.sqrMagnitude > 0.001f)
            {
                rotation = Quaternion.LookRotation(
                    normal.normalized
                );
            }

            ParticleSystem impact = Instantiate(
                impactPrefab,
                position,
                rotation
            );

            impact.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            impact.Play(true);

            // Automatically destroy particles.
            var main = impact.main;

            float destroyDelay =
                main.duration +
                main.startLifetime.constantMax +
                0.5f;

            Destroy(
                impact.gameObject,
                destroyDelay
            );
        }

        // 2. Optional impact sound.
        if (impactClip != null)
        {
            AudioSource.PlayClipAtPoint(
                impactClip,
                position,
                impactVolume
            );
        }
    }

    // ==========================================
    // CLEANUP
    // ==========================================

    private void OnDisable()
    {
        if (recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
            recoilCoroutine = null;
        }

        if (recoilInitialized && recoilBody != null)
        {
            recoilBody.localPosition = initialLocalPosition;
        }
    }
}
