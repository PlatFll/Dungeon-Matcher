using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Image))]
public sealed class EnemyWeaknessIndicatorUI :
    MonoBehaviour
{
    private static readonly int FlashAmountId =
        Shader.PropertyToID("_FlashAmount");

    private static readonly int FlashColorId =
        Shader.PropertyToID("_FlashColor");

    private RectTransform indicatorRect;
    private RectTransform slotRect;
    private RectTransform healthBarRect;
    private RectTransform particleContainer;
    private Image indicatorImage;
    private CanvasGroup canvasGroup;
    private EnemyWeaknessIndicatorConfig config;
    private Material flashMaterial;
    private Coroutine animationCoroutine;

    private bool isInitialized;
    private bool isDefeating;

    public bool IsDefeating =>
        isDefeating;

    private void Awake()
    {
        indicatorRect =
            GetComponent<RectTransform>();

        indicatorImage =
            GetComponent<Image>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        indicatorImage.raycastTarget = false;
        indicatorImage.preserveAspect = true;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void OnDestroy()
    {
        if (flashMaterial != null)
        {
            Destroy(flashMaterial);
            flashMaterial = null;
        }
    }

    public void Initialize(
        RectTransform ownerSlotRect,
        RectTransform targetHealthBarRect,
        EnemyWeaknessIndicatorConfig indicatorConfig)
    {
        slotRect = ownerSlotRect;
        healthBarRect = targetHealthBarRect;
        config = indicatorConfig;

        if (slotRect == null ||
            healthBarRect == null ||
            config == null)
        {
            Debug.LogError(
                $"{name} cannot initialize without its slot, " +
                "health bar, and weakness indicator config.",
                this
            );

            HideImmediate();
            return;
        }

        indicatorRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        indicatorRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        indicatorRect.pivot =
            new Vector2(0.5f, 0.5f);

        indicatorRect.sizeDelta =
            Vector2.one *
            config.IconSize;

        Canvas rootCanvas =
            GetComponentInParent<Canvas>();

        particleContainer =
            rootCanvas != null
                ? rootCanvas.transform as RectTransform
                : slotRect;

        CreateFlashMaterial();

        isInitialized = true;
        FollowHealthBar();
        HideImmediate();
    }

    public void Show(
        GemType gemType,
        bool animate)
    {
        if (!isInitialized ||
            config == null)
        {
            return;
        }

        Sprite weaknessSprite =
            config.GetSprite(gemType);

        if (weaknessSprite == null)
        {
            Debug.LogWarning(
                $"No 16x16 weakness sprite is assigned for {gemType}.",
                config
            );

            HideImmediate();
            return;
        }

        StopAnimation();
        isDefeating = false;

        indicatorImage.sprite = weaknessSprite;
        indicatorImage.enabled = true;
        canvasGroup.alpha = 1f;

        FollowHealthBar();

        if (!animate)
        {
            ResetRestingVisual();
            return;
        }

        animationCoroutine =
            StartCoroutine(
                MaterializeRoutine()
            );
    }

    public void PlayDefeat()
    {
        if (!isInitialized ||
            !indicatorImage.enabled ||
            indicatorImage.sprite == null)
        {
            HideImmediate();
            return;
        }

        StopAnimation();
        isDefeating = true;

        animationCoroutine =
            StartCoroutine(
                DefeatRoutine()
            );
    }

    public void HideImmediate()
    {
        StopAnimation();

        isDefeating = false;

        if (indicatorImage != null)
        {
            indicatorImage.enabled = false;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (indicatorRect != null)
        {
            indicatorRect.localScale =
                Vector3.one;
        }

        SetFlashAmount(0f);
    }

    private void LateUpdate()
    {
        if (!isInitialized ||
            indicatorImage == null ||
            !indicatorImage.enabled)
        {
            return;
        }

        FollowHealthBar();
    }

    private void CreateFlashMaterial()
    {
        if (config.WhiteFlashShader == null)
        {
            Debug.LogWarning(
                $"{config.name} has no white flash shader. " +
                "The weakness sprite will still display, but its " +
                "materialize/death white-fill effect will be skipped.",
                config
            );

            return;
        }

        flashMaterial =
            new Material(
                config.WhiteFlashShader
            )
            {
                name =
                    "Enemy Weakness White Flash (Runtime)"
            };

        if (flashMaterial.HasProperty(
                FlashColorId))
        {
            flashMaterial.SetColor(
                FlashColorId,
                Color.white
            );
        }

        indicatorImage.material =
            flashMaterial;
    }

    private void FollowHealthBar()
    {
        if (slotRect == null ||
            healthBarRect == null ||
            config == null)
        {
            return;
        }

        Vector3 healthBarBottomCenter =
            healthBarRect.TransformPoint(
                new Vector3(
                    healthBarRect.rect.center.x,
                    healthBarRect.rect.yMin,
                    0f
                )
            );

        Vector3 slotLocalPosition =
            slotRect.InverseTransformPoint(
                healthBarBottomCenter
            );

        float centerDrop =
            config.GapBelowHealthBar +
            config.IconSize * 0.5f;

        indicatorRect.localPosition =
            new Vector3(
                slotLocalPosition.x,
                slotLocalPosition.y - centerDrop,
                0f
            );
    }

    private IEnumerator MaterializeRoutine()
    {
        float duration =
            Mathf.Max(
                0.01f,
                config.MaterializeDuration
            );

        float elapsed = 0f;

        indicatorRect.localScale =
            Vector3.one *
            config.MaterializeStartScale;

        canvasGroup.alpha = 0f;
        SetFlashAmount(1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration
                );

            float scale;

            if (progress < 0.68f)
            {
                float popProgress =
                    Mathf.Clamp01(
                        progress / 0.68f
                    );

                popProgress =
                    EaseOutCubic(
                        popProgress
                    );

                scale =
                    Mathf.Lerp(
                        config.MaterializeStartScale,
                        config.MaterializeOvershootScale,
                        popProgress
                    );
            }
            else
            {
                float settleProgress =
                    Mathf.InverseLerp(
                        0.68f,
                        1f,
                        progress
                    );

                settleProgress =
                    SmoothProgress(
                        settleProgress
                    );

                scale =
                    Mathf.Lerp(
                        config.MaterializeOvershootScale,
                        1f,
                        settleProgress
                    );
            }

            indicatorRect.localScale =
                Vector3.one * scale;

            canvasGroup.alpha =
                Mathf.InverseLerp(
                    0f,
                    0.18f,
                    progress
                );

            float colorRevealProgress =
                Mathf.InverseLerp(
                    0.18f,
                    0.88f,
                    progress
                );

            SetFlashAmount(
                1f -
                SmoothProgress(
                    colorRevealProgress
                )
            );

            yield return null;
        }

        ResetRestingVisual();
        animationCoroutine = null;
    }

    private IEnumerator DefeatRoutine()
    {
        canvasGroup.alpha = 1f;
        indicatorImage.enabled = true;
        SetFlashAmount(1f);

        float holdDuration =
            Mathf.Max(
                0f,
                config.DeathWhiteHoldDuration
            );

        float elapsed = 0f;

        while (elapsed < holdDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                holdDuration > 0f
                    ? Mathf.Clamp01(
                        elapsed /
                        holdDuration
                    )
                    : 1f;

            float scale =
                Mathf.Lerp(
                    1f,
                    config.DeathPopScale,
                    EaseOutCubic(progress)
                );

            indicatorRect.localScale =
                Vector3.one * scale;

            yield return null;
        }

        SpawnBurstParticles();

        canvasGroup.alpha = 0f;
        indicatorImage.enabled = false;
        indicatorRect.localScale =
            Vector3.one;

        SetFlashAmount(0f);

        isDefeating = false;
        animationCoroutine = null;
    }

    private void SpawnBurstParticles()
    {
        if (particleContainer == null ||
            config.DeathParticleCount <= 0)
        {
            return;
        }

        Vector3 worldCenter =
            indicatorRect.TransformPoint(
                indicatorRect.rect.center
            );

        Vector3 containerLocalCenter =
            particleContainer.InverseTransformPoint(
                worldCenter
            );

        for (int index = 0;
             index < config.DeathParticleCount;
             index++)
        {
            GameObject particleObject =
                new GameObject(
                    "WeaknessGemWhitePixel",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(EnemyWeaknessBurstParticle)
                );

            particleObject.transform.SetParent(
                particleContainer,
                false
            );

            float angle =
                Random.Range(
                    0f,
                    Mathf.PI * 2f
                );

            Vector2 direction =
                new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle) +
                    config.ParticleUpwardBias
                );

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector2.up;
            }

            direction.Normalize();

            float speed =
                Random.Range(
                    config.ParticleMinimumSpeed,
                    config.ParticleMaximumSpeed
                );

            float lifetime =
                Random.Range(
                    config.ParticleMinimumLifetime,
                    config.ParticleMaximumLifetime
                );

            float size =
                Random.Range(
                    config.ParticleMinimumSize,
                    config.ParticleMaximumSize
                );

            EnemyWeaknessBurstParticle particle =
                particleObject.GetComponent<
                    EnemyWeaknessBurstParticle
                >();

            particle.Play(
                new Vector2(
                    containerLocalCenter.x,
                    containerLocalCenter.y
                ),
                direction * speed,
                lifetime,
                size,
                config.ParticleEndScale,
                Color.white
            );
        }
    }

    private void StopAnimation()
    {
        if (animationCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            animationCoroutine
        );

        animationCoroutine = null;
    }

    private void ResetRestingVisual()
    {
        if (indicatorRect != null)
        {
            indicatorRect.localScale =
                Vector3.one;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        SetFlashAmount(0f);
    }

    private void SetFlashAmount(
        float amount)
    {
        if (flashMaterial == null ||
            !flashMaterial.HasProperty(
                FlashAmountId))
        {
            return;
        }

        flashMaterial.SetFloat(
            FlashAmountId,
            Mathf.Clamp01(amount)
        );
    }

    private static float SmoothProgress(
        float value)
    {
        value = Mathf.Clamp01(value);

        return value *
            value *
            (3f - 2f * value);
    }

    private static float EaseOutCubic(
        float value)
    {
        value = Mathf.Clamp01(value);

        float inverse =
            1f - value;

        return 1f -
            inverse *
            inverse *
            inverse;
    }
}
