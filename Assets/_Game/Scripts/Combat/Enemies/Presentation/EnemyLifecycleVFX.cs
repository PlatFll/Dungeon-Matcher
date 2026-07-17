using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class EnemyLifecycleVFX :
    MonoBehaviour
{
    private static readonly int FlashAmountId =
        Shader.PropertyToID(
            "_FlashAmount"
        );

    [Header("Runtime Sources")]
    [SerializeField]
    private EnemyActor enemyActor;

    [SerializeField]
    private EnemyAutoAttack enemyAutoAttack;

    [Header("Enemy Visual")]
    [SerializeField]
    private RectTransform visualRoot;

    [SerializeField]
    private Image enemyImage;

    [SerializeField]
    private RectTransform attackTimerRoot;

    [Header("Spawn Portal Visuals")]
    [SerializeField]
    private RectTransform spawnCircleFlash;

    [SerializeField]
    private Image spawnCircleImage;

    [SerializeField]
    private RectTransform spawnBeam;

    [SerializeField]
    private Image spawnBeamImage;

    [Header("Death Particles")]
    [SerializeField]
    private RectTransform particleContainer;

    [SerializeField]
    private UIPixelParticle pixelParticlePrefab;

    [SerializeField, Min(1)]
    private int deathParticleCount = 16;

    [SerializeField]
    private Vector2 particleSizeRange =
        new Vector2(2f, 4f);

    [SerializeField]
    private Vector2 particleLifetimeRange =
        new Vector2(0.22f, 0.38f);

    [SerializeField]
    private Vector2 horizontalVelocityRange =
        new Vector2(-28f, 28f);

    [SerializeField]
    private Vector2 verticalVelocityRange =
        new Vector2(35f, 80f);

    [Header("Spawn Timing")]
    [SerializeField, Min(0.05f)]
    private float spawnDuration = 0.38f;

    [SerializeField, Min(0f)]
    private float spawnRiseDistance = 10f;

    [Header("Death Timing")]
    [SerializeField, Min(0f)]
    private float deathWhiteHoldDuration = 0.08f;

    [SerializeField, Min(0.05f)]
    private float deathEffectDuration = 0.42f;

    public event Action<EnemyLifecycleVFX>
        SpawnFinished;

    public event Action<EnemyLifecycleVFX>
        DeathFinished;

    private Vector2 visualRestingPosition;

    private Vector3 circleRestingScale;
    private Vector3 beamRestingScale;

    private Color enemyOriginalColor;
    private Color circleOriginalColor;
    private Color beamOriginalColor;

    private Material flashMaterial;

    private Coroutine spawnCoroutine;
    private Coroutine deathCoroutine;

    private Action deathCompletionCallback;

    public bool IsSpawning =>
        spawnCoroutine != null;

    public bool IsDying =>
        deathCoroutine != null;

    private void Awake()
    {
        ResolveReferences();
        CacheOriginalValues();
    }

    private void ResolveReferences()
    {
        if (enemyActor == null)
        {
            enemyActor =
                GetComponent<EnemyActor>();
        }

        if (enemyAutoAttack == null)
        {
            enemyAutoAttack =
                GetComponent<EnemyAutoAttack>();
        }
    }

    private void CacheOriginalValues()
    {
        if (visualRoot != null)
        {
            visualRestingPosition =
                visualRoot.anchoredPosition;
        }

        if (enemyImage != null)
        {
            enemyOriginalColor =
                enemyImage.color;
        }

        if (spawnCircleFlash != null)
        {
            circleRestingScale =
                spawnCircleFlash.localScale;
        }

        if (spawnCircleImage != null)
        {
            circleOriginalColor =
                spawnCircleImage.color;
        }

        if (spawnBeam != null)
        {
            beamRestingScale =
                spawnBeam.localScale;
        }

        if (spawnBeamImage != null)
        {
            beamOriginalColor =
                spawnBeamImage.color;
        }
    }

    public void PlaySpawnEffect()
    {
        if (!isActiveAndEnabled ||
            visualRoot == null ||
            enemyImage == null)
        {
            return;
        }

        StopRunningEffects();

        RefreshFlashMaterial();

        if (enemyAutoAttack != null)
        {
            /*
             * The attack interval begins after the enemy
             * has completely entered the battle.
             */
            enemyAutoAttack.StopAttacking();
        }

        spawnCoroutine =
            StartCoroutine(
                SpawnRoutine()
            );
    }

    public bool PlayDeathEffect(
        Action onFinished = null)
    {
        if (!isActiveAndEnabled ||
            deathCoroutine != null)
        {
            return false;
        }

        if (spawnCoroutine != null)
        {
            StopCoroutine(
                spawnCoroutine
            );

            spawnCoroutine = null;
        }

        deathCompletionCallback =
            onFinished;

        RefreshFlashMaterial();

        deathCoroutine =
            StartCoroutine(
                DeathRoutine()
            );

        return true;
    }

    private IEnumerator SpawnRoutine()
    {
        if (spawnCircleFlash != null)
        {
            spawnCircleFlash.gameObject.SetActive(
                true
            );
        }

        if (spawnBeam != null)
        {
            spawnBeam.gameObject.SetActive(
                true
            );
        }

        if (attackTimerRoot != null)
        {
            attackTimerRoot.gameObject.SetActive(
                false
            );
        }

        enemyImage.enabled = true;

        visualRoot.anchoredPosition =
            visualRestingPosition +
            Vector2.down *
            spawnRiseDistance;

        SetEnemyAlpha(0f);
        SetFlashAmount(1f);

        SetCircleAlpha(0f);
        SetBeamAlpha(0f);

        float elapsedTime = 0f;

        while (elapsedTime <
               spawnDuration)
        {
            float normalizedTime =
                Mathf.Clamp01(
                    elapsedTime /
                    spawnDuration
                );

            UpdateSpawnCircle(
                normalizedTime
            );

            UpdateSpawnBeam(
                normalizedTime
            );

            UpdateSpawnEnemy(
                normalizedTime
            );

            elapsedTime +=
                Time.deltaTime;

            yield return null;
        }

        RestoreSpawnVisuals();

        if (attackTimerRoot != null)
        {
            attackTimerRoot.gameObject.SetActive(
                true
            );
        }

        if (enemyAutoAttack != null)
        {
            enemyAutoAttack.TryStartAttacking();
        }

        spawnCoroutine = null;

        SpawnFinished?.Invoke(this);
    }

    private void UpdateSpawnCircle(
        float normalizedTime)
    {
        if (spawnCircleFlash == null ||
            spawnCircleImage == null)
        {
            return;
        }

        float expansionProgress =
            Mathf.Clamp01(
                normalizedTime /
                0.28f
            );

        float fadeProgress =
            Mathf.InverseLerp(
                0.45f,
                1f,
                normalizedTime
            );

        float scale =
            Mathf.Lerp(
                0.35f,
                1.15f,
                SmoothProgress(
                    expansionProgress
                )
            );

        spawnCircleFlash.localScale =
            circleRestingScale *
            scale;

        SetCircleAlpha(
            1f - fadeProgress
        );
    }

    private void UpdateSpawnBeam(
        float normalizedTime)
    {
        if (spawnBeam == null ||
            spawnBeamImage == null)
        {
            return;
        }

        float appearProgress =
            Mathf.InverseLerp(
                0.08f,
                0.35f,
                normalizedTime
            );

        float disappearProgress =
            Mathf.InverseLerp(
                0.48f,
                0.82f,
                normalizedTime
            );

        float alpha =
            Mathf.Clamp01(
                appearProgress -
                disappearProgress
            );

        float verticalScale =
            Mathf.Lerp(
                0.12f,
                1f,
                SmoothProgress(
                    appearProgress
                )
            );

        spawnBeam.localScale =
            new Vector3(
                beamRestingScale.x,
                beamRestingScale.y *
                verticalScale,
                beamRestingScale.z
            );

        SetBeamAlpha(alpha);
    }

    private void UpdateSpawnEnemy(
        float normalizedTime)
    {
        float appearanceProgress =
            Mathf.InverseLerp(
                0.18f,
                0.48f,
                normalizedTime
            );

        float colorRevealProgress =
            Mathf.InverseLerp(
                0.42f,
                0.82f,
                normalizedTime
            );

        float movementProgress =
            SmoothProgress(
                appearanceProgress
            );

        visualRoot.anchoredPosition =
            Vector2.Lerp(
                visualRestingPosition +
                Vector2.down *
                spawnRiseDistance,
                visualRestingPosition,
                movementProgress
            );

        SetEnemyAlpha(
            appearanceProgress
        );

        SetFlashAmount(
            1f -
            colorRevealProgress
        );
    }

    private IEnumerator DeathRoutine()
    {
        if (enemyAutoAttack != null)
        {
            enemyAutoAttack.StopAttacking();
        }

        if (attackTimerRoot != null)
        {
            attackTimerRoot.gameObject.SetActive(
                false
            );
        }

        HideSpawnPortal();

        visualRoot.anchoredPosition =
            visualRestingPosition;

        enemyImage.enabled = true;

        SetEnemyAlpha(1f);
        SetFlashAmount(1f);

        if (deathWhiteHoldDuration > 0f)
        {
            yield return new WaitForSeconds(
                deathWhiteHoldDuration
            );
        }

        SpawnDeathParticles();

        SetEnemyAlpha(0f);
        enemyImage.enabled = false;

        float remainingDuration =
            Mathf.Max(
                0f,
                deathEffectDuration -
                deathWhiteHoldDuration
            );

        if (remainingDuration > 0f)
        {
            yield return new WaitForSeconds(
                remainingDuration
            );
        }

        SetFlashAmount(0f);

        deathCoroutine = null;

        DeathFinished?.Invoke(this);

        Action callback =
            deathCompletionCallback;

        deathCompletionCallback = null;

        callback?.Invoke();
    }

    private void SpawnDeathParticles()
    {
        if (particleContainer == null ||
            pixelParticlePrefab == null)
        {
            return;
        }

        Vector2 particleCenter =
            particleContainer
                .InverseTransformPoint(
                    visualRoot.position
                );

        float availableWidth =
            Mathf.Max(
                8f,
                visualRoot.rect.width *
                0.62f
            );

        float availableHeight =
            Mathf.Max(
                8f,
                visualRoot.rect.height *
                0.70f
            );

        for (int index = 0;
             index < deathParticleCount;
             index++)
        {
            UIPixelParticle particle =
                Instantiate(
                    pixelParticlePrefab,
                    particleContainer,
                    false
                );

            Vector2 randomOffset =
                new Vector2(
                    UnityEngine.Random.Range(
                        -availableWidth * 0.5f,
                        availableWidth * 0.5f
                    ),
                    UnityEngine.Random.Range(
                        -availableHeight * 0.5f,
                        availableHeight * 0.5f
                    )
                );

            Vector2 velocity =
                new Vector2(
                    UnityEngine.Random.Range(
                        horizontalVelocityRange.x,
                        horizontalVelocityRange.y
                    ),
                    UnityEngine.Random.Range(
                        verticalVelocityRange.x,
                        verticalVelocityRange.y
                    )
                );

            float lifetime =
                UnityEngine.Random.Range(
                    particleLifetimeRange.x,
                    particleLifetimeRange.y
                );

            float size =
                UnityEngine.Random.Range(
                    particleSizeRange.x,
                    particleSizeRange.y
                );

            particle.Play(
                particleCenter +
                randomOffset,
                velocity,
                lifetime,
                size,
                Color.white
            );
        }
    }

    private void RestoreSpawnVisuals()
    {
        visualRoot.anchoredPosition =
            visualRestingPosition;

        enemyImage.enabled = true;

        SetEnemyAlpha(1f);
        SetFlashAmount(0f);

        HideSpawnPortal();
    }

    private void HideSpawnPortal()
    {
        if (spawnCircleFlash != null)
        {
            spawnCircleFlash.localScale =
                circleRestingScale;

            spawnCircleFlash.gameObject.SetActive(
                false
            );
        }

        if (spawnBeam != null)
        {
            spawnBeam.localScale =
                beamRestingScale;

            spawnBeam.gameObject.SetActive(
                false
            );
        }

        SetCircleAlpha(0f);
        SetBeamAlpha(0f);
    }

    private void RefreshFlashMaterial()
    {
        if (enemyImage == null)
        {
            flashMaterial = null;
            return;
        }

        flashMaterial =
            enemyImage.material;

        if (flashMaterial == null ||
            !flashMaterial.HasProperty(
                FlashAmountId
            ))
        {
            Debug.LogError(
                $"{name}'s Enemy Image must use the " +
                "Dungeon Matcher/UI/White Flash shader.",
                this
            );

            flashMaterial = null;
        }
    }

    private void SetFlashAmount(
        float amount)
    {
        if (flashMaterial == null)
        {
            return;
        }

        flashMaterial.SetFloat(
            FlashAmountId,
            Mathf.Clamp01(amount)
        );
    }

    private void SetEnemyAlpha(
        float alpha)
    {
        if (enemyImage == null)
        {
            return;
        }

        Color color =
            enemyOriginalColor;

        color.a *=
            Mathf.Clamp01(alpha);

        enemyImage.color =
            color;
    }

    private void SetCircleAlpha(
        float alpha)
    {
        if (spawnCircleImage == null)
        {
            return;
        }

        Color color =
            circleOriginalColor;

        color.a *=
            Mathf.Clamp01(alpha);

        spawnCircleImage.color =
            color;
    }

    private void SetBeamAlpha(
        float alpha)
    {
        if (spawnBeamImage == null)
        {
            return;
        }

        Color color =
            beamOriginalColor;

        color.a *=
            Mathf.Clamp01(alpha);

        spawnBeamImage.color =
            color;
    }

    private static float SmoothProgress(
        float value)
    {
        value =
            Mathf.Clamp01(value);

        return
            value *
            value *
            (3f - 2f * value);
    }

    private void StopRunningEffects()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(
                spawnCoroutine
            );

            spawnCoroutine = null;
        }

        if (deathCoroutine != null)
        {
            StopCoroutine(
                deathCoroutine
            );

            deathCoroutine = null;
        }

        deathCompletionCallback = null;

        RestoreSpawnVisuals();
    }

    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(
                spawnCoroutine
            );

            spawnCoroutine = null;
        }

        if (deathCoroutine != null)
        {
            StopCoroutine(
                deathCoroutine
            );

            deathCoroutine = null;
        }
    }

    private void OnValidate()
    {
        deathParticleCount =
            Mathf.Max(
                1,
                deathParticleCount
            );

        particleSizeRange.x =
            Mathf.Max(
                1f,
                particleSizeRange.x
            );

        particleSizeRange.y =
            Mathf.Max(
                particleSizeRange.x,
                particleSizeRange.y
            );

        particleLifetimeRange.x =
            Mathf.Max(
                0.05f,
                particleLifetimeRange.x
            );

        particleLifetimeRange.y =
            Mathf.Max(
                particleLifetimeRange.x,
                particleLifetimeRange.y
            );

        spawnDuration =
            Mathf.Max(
                0.05f,
                spawnDuration
            );

        spawnRiseDistance =
            Mathf.Max(
                0f,
                spawnRiseDistance
            );

        deathWhiteHoldDuration =
            Mathf.Max(
                0f,
                deathWhiteHoldDuration
            );

        deathEffectDuration =
            Mathf.Max(
                0.05f,
                deathEffectDuration
            );
    }
}