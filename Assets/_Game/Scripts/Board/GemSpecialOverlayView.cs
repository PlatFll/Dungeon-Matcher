using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class GemSpecialOverlayView :
    MonoBehaviour
{
    [Header("Overlay Sprites")]
    [SerializeField]
    private Sprite rowBombSprite;

    [SerializeField]
    private Sprite columnBombSprite;

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

    private SpriteRenderer overlayRenderer;

    private Vector3 normalScale;

    private Color currentTint = Color.white;

    private float pulseOffset;

    private float shimmerStartTime = -1f;

    private float nextShimmerTime;

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
            !overlayRenderer.enabled)
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
            GetGemTint(gemType);

        overlayRenderer.enabled =
            true;

        transform.localScale =
            normalScale;

        shimmerStartTime = -1f;

        ScheduleNextShimmer();
    }

    public void Hide()
    {
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
    }
}