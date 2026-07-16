using System;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TMP_Text))]
public sealed class FloatingCombatText :
    MonoBehaviour
{
    private RectTransform rectTransform;
    private TMP_Text textLabel;

    private Action<FloatingCombatText>
        releaseAction;

    private Vector2 startingPosition;
    private Vector2 travelDistance;

    private Color baseColor;

    private float lifetime;
    private float elapsedTime;

    private float fadeStartNormalized;
    private float startScale;
    private float peakScale;

    private bool isPlaying;

    private void Awake()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        if (rectTransform == null)
        {
            rectTransform =
                GetComponent<RectTransform>();
        }

        if (textLabel == null)
        {
            textLabel =
                GetComponent<TMP_Text>();
        }
    }

    public void Play(
        string displayedText,
        CombatTextStyle style,
        Vector2 localPosition,
        Action<FloatingCombatText> onFinished)
    {
        CacheReferences();

        if (style == null)
        {
            Debug.LogError(
                "FloatingCombatText requires a valid style.",
                this
            );

            onFinished?.Invoke(this);
            return;
        }

        if (rectTransform == null ||
            textLabel == null)
        {
            Debug.LogError(
                "FloatingCombatText is missing its " +
                "RectTransform or TMP_Text component.",
                this
            );

            onFinished?.Invoke(this);
            return;
        }

        releaseAction =
            onFinished;

        elapsedTime = 0f;
        isPlaying = true;

        lifetime =
            Mathf.Max(
                0.05f,
                style.Lifetime
            );

        fadeStartNormalized =
            Mathf.Clamp(
                style.FadeStartNormalized,
                0f,
                0.99f
            );

        startScale =
            Mathf.Max(
                0.01f,
                style.StartScale
            );

        peakScale =
            Mathf.Max(
                0.01f,
                style.PeakScale
            );

        startingPosition =
            localPosition;

        float horizontalJitter =
            UnityEngine.Random.Range(
                -style.HorizontalJitter,
                style.HorizontalJitter
            );

        travelDistance =
            style.TravelDistance +
            Vector2.right *
            horizontalJitter;

        baseColor =
            style.Color;

        /*
         * Prevent an accidentally transparent style
         * from making the text invisible.
         */
        baseColor.a = 1f;

        ConfigureRectTransform();
        ConfigureText(displayedText, style);

        rectTransform.anchoredPosition =
            startingPosition;

        rectTransform.localScale =
            Vector3.one *
            startScale;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        textLabel.ForceMeshUpdate();
    }

    private void ConfigureRectTransform()
    {
        rectTransform.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        rectTransform.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rectTransform.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        rectTransform.sizeDelta =
            new Vector2(
                120f,
                50f
            );

        rectTransform.localRotation =
            Quaternion.identity;
    }

    private void ConfigureText(
        string displayedText,
        CombatTextStyle style)
    {
        textLabel.enabled = true;

        textLabel.text =
            displayedText;

        textLabel.fontSize =
            Mathf.Max(
                1f,
                style.FontSize
            );

        textLabel.fontStyle =
            style.FontStyle;

        textLabel.alignment =
            TextAlignmentOptions.Center;

        textLabel.enableWordWrapping =
            false;

        textLabel.overflowMode =
            TextOverflowModes.Overflow;

        textLabel.raycastTarget =
            false;

        textLabel.color =
            baseColor;

        textLabel.alpha = 1f;

        if (textLabel.canvasRenderer != null)
        {
            textLabel.canvasRenderer.SetAlpha(
                1f
            );

            textLabel.canvasRenderer.cull =
                false;
        }
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        elapsedTime +=
            Time.deltaTime;

        float normalizedTime =
            Mathf.Clamp01(
                elapsedTime /
                lifetime
            );

        UpdatePosition(
            normalizedTime
        );

        UpdateScale(
            normalizedTime
        );

        UpdateFade(
            normalizedTime
        );

        if (elapsedTime >= lifetime)
        {
            Finish();
        }
    }

    private void UpdatePosition(
        float normalizedTime)
    {
        float movementProgress =
            EaseOutCubic(
                normalizedTime
            );

        rectTransform.anchoredPosition =
            startingPosition +
            travelDistance *
            movementProgress;
    }

    private void UpdateScale(
        float normalizedTime)
    {
        float scale;

        if (normalizedTime < 0.2f)
        {
            float progress =
                Mathf.InverseLerp(
                    0f,
                    0.2f,
                    normalizedTime
                );

            scale =
                Mathf.Lerp(
                    startScale,
                    peakScale,
                    progress
                );
        }
        else if (normalizedTime < 0.5f)
        {
            float progress =
                Mathf.InverseLerp(
                    0.2f,
                    0.5f,
                    normalizedTime
                );

            scale =
                Mathf.Lerp(
                    peakScale,
                    1f,
                    progress
                );
        }
        else
        {
            scale = 1f;
        }

        rectTransform.localScale =
            Vector3.one *
            scale;
    }

    private void UpdateFade(
        float normalizedTime)
    {
        float alpha = 1f;

        if (normalizedTime >
            fadeStartNormalized)
        {
            alpha =
                1f -
                Mathf.InverseLerp(
                    fadeStartNormalized,
                    1f,
                    normalizedTime
                );
        }

        Color currentColor =
            baseColor;

        currentColor.a =
            Mathf.Clamp01(alpha);

        textLabel.color =
            currentColor;
    }

    private static float EaseOutCubic(
        float value)
    {
        value =
            Mathf.Clamp01(value);

        float inverse =
            1f - value;

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

        isPlaying = false;

        Action<FloatingCombatText>
            callback =
                releaseAction;

        releaseAction = null;

        callback?.Invoke(this);
    }

    private void OnDisable()
    {
        isPlaying = false;
        elapsedTime = 0f;

        if (rectTransform != null)
        {
            rectTransform.localScale =
                Vector3.one;
        }

        if (textLabel != null)
        {
            Color resetColor =
                baseColor;

            resetColor.a = 1f;

            textLabel.color =
                resetColor;

            textLabel.alpha = 1f;
        }
    }
}