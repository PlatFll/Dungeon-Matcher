using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class ColorCrystalGlintVFX :
    MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private Gem targetGem;

    private Color startingColor;

    private float baseSize;
    private float duration;
    private float peakTime;
    private float flashDuration;
    private float elapsedTime;
    private float totalRotation;

    private bool flashStarted;
    private bool flashReset;
    private bool isPlaying;

    private Action<ColorCrystalGlintVFX>
        releaseAction;

    private void Awake()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }
    }

    public void ConfigureRendering(
        Sprite starSprite,
        string sortingLayerName,
        int sortingOrder)
    {
        CacheReferences();

        spriteRenderer.sprite =
            starSprite;

        spriteRenderer.sortingLayerName =
            sortingLayerName;

        spriteRenderer.sortingOrder =
            sortingOrder;

        spriteRenderer.maskInteraction =
            SpriteMaskInteraction.VisibleInsideMask;
    }

    public void Play(
        Gem gem,
        Vector3 localPosition,
        float size,
        float glintDuration,
        float glintPeakTime,
        float targetFlashDuration,
        float rotationDegrees,
        Color color,
        Action<ColorCrystalGlintVFX> onFinished)
    {
        CacheReferences();

        targetGem =
            gem;

        baseSize =
            Mathf.Max(
                0.005f,
                size
            );

        duration =
            Mathf.Max(
                0.05f,
                glintDuration
            );

        peakTime =
            Mathf.Clamp(
                glintPeakTime,
                0.01f,
                duration
            );

        flashDuration =
            Mathf.Max(
                0f,
                targetFlashDuration
            );

        totalRotation =
            rotationDegrees;

        startingColor =
            color;

        startingColor.a = 1f;

        releaseAction =
            onFinished;

        elapsedTime = 0f;
        flashStarted = false;
        flashReset = false;
        isPlaying = true;

        transform.localPosition =
            localPosition;

        transform.localRotation =
            Quaternion.identity;

        transform.localScale =
            Vector3.zero;

        spriteRenderer.color =
            startingColor;

        spriteRenderer.enabled = true;

        gameObject.SetActive(true);

        ApplyVisualState();
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        elapsedTime +=
            Time.deltaTime;

        UpdateTargetFlash();
        ApplyVisualState();

        if (elapsedTime >= duration)
        {
            Finish();
        }
    }

    private void UpdateTargetFlash()
    {
        if (!flashStarted &&
            elapsedTime >= peakTime)
        {
            flashStarted = true;

            if (targetGem != null)
            {
                targetGem.SetVFXFlashAmount(
                    1f
                );
            }
        }

        if (!flashStarted ||
            flashReset ||
            elapsedTime <
                peakTime +
                flashDuration)
        {
            return;
        }

        flashReset = true;

        if (targetGem != null)
        {
            targetGem.SetVFXFlashAmount(
                0f
            );
        }
    }

    private void ApplyVisualState()
    {
        float visualScale;

        const float firstGrowEnd = 0.04f;

        if (elapsedTime < firstGrowEnd)
        {
            float progress =
                Mathf.InverseLerp(
                    0f,
                    firstGrowEnd,
                    elapsedTime
                );

            visualScale =
                Mathf.Lerp(
                    0f,
                    0.6f,
                    SmoothStep(progress)
                );
        }
        else if (elapsedTime < peakTime)
        {
            float progress =
                Mathf.InverseLerp(
                    firstGrowEnd,
                    peakTime,
                    elapsedTime
                );

            visualScale =
                Mathf.Lerp(
                    0.6f,
                    1.2f,
                    EaseOutCubic(progress)
                );
        }
        else
        {
            float progress =
                Mathf.InverseLerp(
                    peakTime,
                    duration,
                    elapsedTime
                );

            visualScale =
                Mathf.Lerp(
                    1.2f,
                    0f,
                    SmoothStep(progress)
                );
        }

        transform.localScale =
            Vector3.one *
            baseSize *
            visualScale;

        float rotationProgress =
            Mathf.Clamp01(
                elapsedTime /
                duration
            );

        transform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                totalRotation *
                rotationProgress
            );

        float fadeProgress =
            Mathf.InverseLerp(
                peakTime,
                duration,
                elapsedTime
            );

        Color color =
            Color.Lerp(
                startingColor,
                Color.white,
                Mathf.Clamp01(
                    elapsedTime /
                    peakTime
                ) *
                0.35f
            );

        color.a =
            1f -
            SmoothStep(fadeProgress);

        spriteRenderer.color =
            color;
    }

    private static float SmoothStep(
        float value)
    {
        value =
            Mathf.Clamp01(value);

        return
            value *
            value *
            (3f - 2f * value);
    }

    private static float EaseOutCubic(
        float value)
    {
        value =
            Mathf.Clamp01(value);

        float inverse =
            1f -
            value;

        return
            1f -
            inverse *
            inverse *
            inverse;
    }

    private void Finish()
    {
        if (!isPlaying)
        {
            return;
        }

        ResetTargetFlash();

        isPlaying = false;

        Action<ColorCrystalGlintVFX>
            callback =
                releaseAction;

        releaseAction = null;

        callback?.Invoke(this);
    }

    private void ResetTargetFlash()
    {
        if (!flashReset &&
            targetGem != null)
        {
            targetGem.SetVFXFlashAmount(
                0f
            );
        }

        flashReset = true;
        targetGem = null;
    }

    public void StopImmediately()
    {
        ResetTargetFlash();

        isPlaying = false;
        releaseAction = null;

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (isPlaying)
        {
            ResetTargetFlash();
        }

        isPlaying = false;
        elapsedTime = 0f;

        transform.localScale =
            Vector3.zero;
    }
}
