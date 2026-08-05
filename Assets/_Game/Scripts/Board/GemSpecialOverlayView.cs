using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class GemSpecialOverlayView :
    MonoBehaviour
{
    private static readonly int
        FlashAmountId =
            Shader.PropertyToID(
                "_FlashAmount"
            );

    [Header("Overlay Sprites")]
    [SerializeField]
    private Sprite rowBombSprite;

    [SerializeField]
    private Sprite columnBombSprite;

    [SerializeField]
    private Sprite colorCrystalSprite;

    [Header("Gem Colors")]
    [SerializeField]
    private Color rubyTint =
        new Color(1f, 0.20f, 0.20f, 1f);

    [SerializeField]
    private Color amberTint =
        new Color(1f, 0.55f, 0.12f, 1f);

    [SerializeField]
    private Color topazTint =
        new Color(1f, 0.88f, 0.18f, 1f);

    [SerializeField]
    private Color emeraldTint =
        new Color(0.20f, 1f, 0.40f, 1f);

    [SerializeField]
    private Color sapphireTint =
        new Color(0.20f, 0.55f, 1f, 1f);

    [SerializeField]
    private Color amethystTint =
        new Color(0.75f, 0.30f, 1f, 1f);

    [Header("Soft Pulse")]
    [SerializeField, Range(0f, 1f)]
    private float minimumAlpha = 0.65f;

    [SerializeField, Range(0f, 1f)]
    private float maximumAlpha = 1f;

    [SerializeField, Min(0.01f)]
    private float pulseSpeed = 1.5f;

    [SerializeField, Range(0f, 0.15f)]
    private float scalePulseAmount = 0.03f;

    [Header("Shimmer")]
    [SerializeField, Min(0.01f)]
    private float shimmerDuration = 0.14f;

    [SerializeField, Min(0f)]
    private float minimumShimmerDelay = 1.25f;

    [SerializeField, Min(0f)]
    private float maximumShimmerDelay = 2.75f;

    [SerializeField, Range(0f, 1f)]
    private float shimmerWhiteness = 0.75f;

    [Header("Crystal Materialization")]

    [SerializeField, Min(0.01f)]
    [Tooltip(
        "How long the white crystal silhouette takes to " +
        "fade from transparent to fully visible."
    )]
    private float crystalWhiteFadeDuration =
        0.16f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "How long the crystal remains fully white before " +
        "its real colors begin appearing."
    )]
    private float crystalWhiteHoldDuration =
        0.06f;

    [SerializeField, Min(0.01f)]
    [Tooltip(
        "How long the white silhouette takes to reveal " +
        "the crystal's real colors."
    )]
    private float crystalColorRevealDuration =
        0.18f;

    private SpriteRenderer overlayRenderer;

    private MaterialPropertyBlock
        materialPropertyBlock;

    private Vector3 normalScale;

    private Color currentTint = Color.white;

    private float pulseOffset;

    private float shimmerStartTime = -1f;

    private float nextShimmerTime;

    private bool isMaterializing;

    private void Awake()
    {
        overlayRenderer =
            GetComponent<SpriteRenderer>();

        normalScale =
            transform.localScale;

        pulseOffset =
            Random.Range(
                0f,
                Mathf.PI * 2f
            );

        Hide();
    }

    private void Update()
    {
        if (overlayRenderer == null ||
            !overlayRenderer.enabled ||
            isMaterializing)
        {
            return;
        }

        float currentTime =
            Time.time;

        float pulse =
            (
                Mathf.Sin(
                    currentTime *
                    pulseSpeed *
                    Mathf.PI *
                    2f +
                    pulseOffset
                ) +
                1f
            ) *
            0.5f;

        float alpha =
            Mathf.Lerp(
                minimumAlpha,
                maximumAlpha,
                pulse
            );

        float scale =
            1f +
            pulse *
            scalePulseAmount;

        transform.localScale =
            normalScale *
            scale;

        float shimmerStrength =
            UpdateShimmer(currentTime);

        Color displayedColor =
            Color.Lerp(
                currentTint,
                Color.white,
                shimmerStrength *
                shimmerWhiteness
            );

        displayedColor.a =
            Mathf.Clamp01(
                alpha +
                shimmerStrength *
                0.2f
            );

        overlayRenderer.color =
            displayedColor;
    }

    public void Show(
        GemType gemType,
        GemSpecialType specialType)
    {
        if (overlayRenderer == null)
        {
            overlayRenderer =
                GetComponent<SpriteRenderer>();
        }

        Sprite selectedSprite =
            GetSprite(specialType);

        if (selectedSprite == null)
        {
            Hide();
            return;
        }

        overlayRenderer.sprite =
            selectedSprite;

        currentTint =
            specialType ==
                GemSpecialType.ColorCrystal
                    ? Color.white
                    : GetGemTint(gemType);

        isMaterializing = false;

        SetOverlayFlashAmount(0f);

        overlayRenderer.enabled =
            true;

        transform.localScale =
            normalScale;

        shimmerStartTime = -1f;

        ScheduleNextShimmer();
    }

    public IEnumerator
        PlayColorCrystalMaterialization()
    {
        if (overlayRenderer == null)
        {
            overlayRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (overlayRenderer == null ||
            overlayRenderer.sprite !=
                colorCrystalSprite)
        {
            yield break;
        }

        isMaterializing = true;

        shimmerStartTime = -1f;

        /*
         * The crystal always remains at its normal size.
         * There is no stretching, squeezing, or popping.
         */
        transform.localScale =
            normalScale;

        overlayRenderer.enabled =
            true;

        /*
         * Stage one:
         * Keep the sprite completely white while its alpha
         * fades from invisible to fully visible.
         */
        SetOverlayFlashAmount(1f);

        float elapsedTime = 0f;

        while (elapsedTime <
               crystalWhiteFadeDuration)
        {
            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    crystalWhiteFadeDuration
                );

            float easedProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            Color whiteFade =
                Color.white;

            whiteFade.a =
                easedProgress;

            overlayRenderer.color =
                whiteFade;

            elapsedTime +=
                Time.deltaTime;

            yield return null;
        }

        overlayRenderer.color =
            Color.white;

        SetOverlayFlashAmount(1f);

        /*
         * Briefly hold the fully formed white silhouette.
         */
        if (crystalWhiteHoldDuration > 0f)
        {
            yield return new WaitForSeconds(
                crystalWhiteHoldDuration
            );
        }

        /*
         * Stage two:
         * Reduce the shader's white-flash amount from one
         * to zero. The crystal's actual sprite colors are
         * gradually revealed underneath.
         */
        elapsedTime = 0f;

        while (elapsedTime <
               crystalColorRevealDuration)
        {
            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    crystalColorRevealDuration
                );

            float easedProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            SetOverlayFlashAmount(
                1f -
                easedProgress
            );

            /*
             * Alpha stays fully visible while color enters.
             */
            overlayRenderer.color =
                Color.white;

            elapsedTime +=
                Time.deltaTime;

            yield return null;
        }

        SetOverlayFlashAmount(0f);

        overlayRenderer.color =
            Color.white;

        transform.localScale =
            normalScale;

        isMaterializing = false;

        shimmerStartTime = -1f;

        ScheduleNextShimmer();
    }

    private void SetOverlayFlashAmount(
        float amount)
    {
        if (overlayRenderer == null)
        {
            overlayRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (overlayRenderer == null)
        {
            return;
        }

        if (materialPropertyBlock == null)
        {
            materialPropertyBlock =
                new MaterialPropertyBlock();
        }

        overlayRenderer.GetPropertyBlock(
            materialPropertyBlock
        );

        materialPropertyBlock.SetFloat(
            FlashAmountId,
            Mathf.Clamp01(amount)
        );

        overlayRenderer.SetPropertyBlock(
            materialPropertyBlock
        );
    }

    public void Hide()
    {
        isMaterializing = false;

        SetOverlayFlashAmount(0f);

        if (overlayRenderer == null)
        {
            overlayRenderer =
                GetComponent<SpriteRenderer>();
        }

        overlayRenderer.enabled =
            false;

        overlayRenderer.sprite =
            null;

        transform.localScale =
            normalScale;

        shimmerStartTime = -1f;
    }

    private Sprite GetSprite(
        GemSpecialType specialType)
    {
        switch (specialType)
        {
            case GemSpecialType.RowBomb:
                return rowBombSprite;

            case GemSpecialType.ColumnBomb:
                return columnBombSprite;

            case GemSpecialType.ColorCrystal:
                return colorCrystalSprite;

            default:
                return null;
        }
    }

    private Color GetGemTint(
        GemType gemType)
    {
        switch (gemType)
        {
            case GemType.Ruby:
                return rubyTint;

            case GemType.Amber:
                return amberTint;

            case GemType.Topaz:
                return topazTint;

            case GemType.Emerald:
                return emeraldTint;

            case GemType.Sapphire:
                return sapphireTint;

            case GemType.Amethyst:
                return amethystTint;

            default:
                return Color.white;
        }
    }

    private float UpdateShimmer(
        float currentTime)
    {
        if (shimmerStartTime < 0f)
        {
            if (currentTime >=
                nextShimmerTime)
            {
                shimmerStartTime =
                    currentTime;
            }
            else
            {
                return 0f;
            }
        }

        float shimmerProgress =
            (
                currentTime -
                shimmerStartTime
            ) /
            shimmerDuration;

        if (shimmerProgress >= 1f)
        {
            shimmerStartTime = -1f;

            ScheduleNextShimmer();

            return 0f;
        }

        return Mathf.Sin(
            shimmerProgress *
            Mathf.PI
        );
    }

    private void ScheduleNextShimmer()
    {
        float minimumDelay =
            Mathf.Max(
                0f,
                minimumShimmerDelay
            );

        float maximumDelay =
            Mathf.Max(
                minimumDelay,
                maximumShimmerDelay
            );

        nextShimmerTime =
            Time.time +
            Random.Range(
                minimumDelay,
                maximumDelay
            );
    }

    private void OnValidate()
    {
        minimumAlpha =
            Mathf.Clamp01(
                minimumAlpha
            );

        maximumAlpha =
            Mathf.Clamp(
                maximumAlpha,
                minimumAlpha,
                1f
            );

        pulseSpeed =
            Mathf.Max(
                0.01f,
                pulseSpeed
            );

        shimmerDuration =
            Mathf.Max(
                0.01f,
                shimmerDuration
            );

        minimumShimmerDelay =
            Mathf.Max(
                0f,
                minimumShimmerDelay
            );

        maximumShimmerDelay =
            Mathf.Max(
                minimumShimmerDelay,
                maximumShimmerDelay
            );

        crystalWhiteFadeDuration =
            Mathf.Max(
                0.01f,
                crystalWhiteFadeDuration
            );

        crystalWhiteHoldDuration =
            Mathf.Max(
                0f,
                crystalWhiteHoldDuration
            );

        crystalColorRevealDuration =
            Mathf.Max(
                0.01f,
                crystalColorRevealDuration
            );
    }
}