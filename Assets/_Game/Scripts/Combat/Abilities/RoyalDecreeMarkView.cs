using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public sealed class RoyalDecreeMarkView :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private RoyalDecreeRuntime royalDecreeRuntime;

    [SerializeField]
    private RectTransform visualRoot;

    [SerializeField]
    private Graphic eyeGlowGraphic;

    [Header("Position")]
    [SerializeField]
    private Vector2 anchoredOffset =
        new Vector2(0f, -45f);

    [Header("Hit Animation")]
    [SerializeField, Min(1f)]
    private float hitPulseScale = 1.25f;

    [SerializeField, Min(0.01f)]
    private float hitPulseDuration = 0.14f;

    [Header("Expiry Blink")]
    [SerializeField, Min(0f)]
    private float blinkStartTime = 1.5f;

    [SerializeField, Min(0.1f)]
    private float blinkSpeed = 7f;

    [SerializeField, Range(0f, 1f)]
    private float minimumBlinkAlpha = 0.25f;

    private RectTransform markRect;
    private RectTransform parentRect;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private EnemyActor currentTarget;
    private Coroutine hitPulseCoroutine;
    private Vector3 normalScale = Vector3.one;
    private Color normalEyeGlowColor;
    private bool isVisible;

    private void Awake()
    {
        markRect =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        parentRect =
            markRect.parent as RectTransform;

        rootCanvas =
            GetComponentInParent<Canvas>();

        if (visualRoot == null)
        {
            visualRoot = markRect;
        }

        normalScale =
            visualRoot.localScale;

        if (eyeGlowGraphic != null)
        {
            normalEyeGlowColor =
                eyeGlowGraphic.color;

            SetEyeGlowAlpha(0f);
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        SetVisible(false);
    }

    private void OnEnable()
    {
        SubscribeToRuntime();
        RefreshFromRuntime();
    }

    private void OnDisable()
    {
        UnsubscribeFromRuntime();

        if (hitPulseCoroutine != null)
        {
            StopCoroutine(
                hitPulseCoroutine
            );

            hitPulseCoroutine = null;
        }

        ResetHitVisuals();
        SetVisible(false);
    }

    private void Update()
    {
        if (royalDecreeRuntime == null ||
            !royalDecreeRuntime.IsActive)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        if (currentTarget != null)
        {
            FollowTarget(
                currentTarget
            );
        }

        UpdateExpiryBlink();
    }

    private void SubscribeToRuntime()
    {
        if (royalDecreeRuntime == null)
        {
            return;
        }

        royalDecreeRuntime.TargetChanged -=
            HandleTargetChanged;

        royalDecreeRuntime.TargetChanged +=
            HandleTargetChanged;

        royalDecreeRuntime.HitResolved -=
            HandleHitResolved;

        royalDecreeRuntime.HitResolved +=
            HandleHitResolved;

        royalDecreeRuntime.StateChanged -=
            HandleRuntimeStateChanged;

        royalDecreeRuntime.StateChanged +=
            HandleRuntimeStateChanged;
    }

    private void UnsubscribeFromRuntime()
    {
        if (royalDecreeRuntime == null)
        {
            return;
        }

        royalDecreeRuntime.TargetChanged -=
            HandleTargetChanged;

        royalDecreeRuntime.HitResolved -=
            HandleHitResolved;

        royalDecreeRuntime.StateChanged -=
            HandleRuntimeStateChanged;
    }

    private void RefreshFromRuntime()
    {
        if (royalDecreeRuntime == null)
        {
            currentTarget = null;
            SetVisible(false);
            return;
        }

        currentTarget =
            royalDecreeRuntime.CurrentTarget;

        bool shouldBeVisible =
            royalDecreeRuntime.IsActive;

        SetVisible(
            shouldBeVisible
        );

        if (shouldBeVisible &&
            currentTarget != null)
        {
            FollowTarget(
                currentTarget
            );
        }
    }

    private void HandleTargetChanged(
        EnemyActor newTarget)
    {
        currentTarget = newTarget;

        /*
         * When newTarget is null, the mark keeps its last
         * position until Royal Decree ends or finds a new enemy.
         */
        if (currentTarget != null)
        {
            FollowTarget(
                currentTarget
            );
        }

        SetVisible(
            royalDecreeRuntime != null &&
            royalDecreeRuntime.IsActive
        );
    }

    private void HandleHitResolved(
        EnemyActor damagedEnemy,
        int damage,
        BoardMatchContext matchContext)
    {
        if (!isVisible ||
            damage <= 0)
        {
            return;
        }

        if (hitPulseCoroutine != null)
        {
            StopCoroutine(
                hitPulseCoroutine
            );
        }

        hitPulseCoroutine =
            StartCoroutine(
                PlayHitPulse()
            );
    }

    private void HandleRuntimeStateChanged()
    {
        RefreshFromRuntime();
    }

    private void FollowTarget(
        EnemyActor target)
    {
        if (target == null ||
            markRect == null ||
            parentRect == null)
        {
            return;
        }

        Camera uiCamera = null;

        if (rootCanvas != null &&
            rootCanvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            uiCamera =
                rootCanvas.worldCamera;
        }

        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                target.transform.position
            );

        bool convertedSuccessfully =
            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    parentRect,
                    screenPosition,
                    uiCamera,
                    out Vector2 localPosition
                );

        if (!convertedSuccessfully)
        {
            return;
        }

        markRect.anchoredPosition =
            localPosition +
            anchoredOffset;
    }

    private IEnumerator PlayHitPulse()
    {
        float halfDuration =
            Mathf.Max(
                0.01f,
                hitPulseDuration * 0.5f
            );

        SetEyeGlowAlpha(1f);

        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / halfDuration
                );

            visualRoot.localScale =
                Vector3.Lerp(
                    normalScale,
                    normalScale * hitPulseScale,
                    progress
                );

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / halfDuration
                );

            visualRoot.localScale =
                Vector3.Lerp(
                    normalScale * hitPulseScale,
                    normalScale,
                    progress
                );

            SetEyeGlowAlpha(
                1f - progress
            );

            yield return null;
        }

        ResetHitVisuals();
        hitPulseCoroutine = null;
    }

    private void UpdateExpiryBlink()
    {
        float remainingDuration =
            royalDecreeRuntime.RemainingDuration;

        if (remainingDuration >
            blinkStartTime)
        {
            canvasGroup.alpha = 1f;
            return;
        }

        float wave =
            (
                Mathf.Sin(
                    Time.time *
                    blinkSpeed *
                    Mathf.PI *
                    2f
                ) +
                1f
            ) *
            0.5f;

        canvasGroup.alpha =
            Mathf.Lerp(
                minimumBlinkAlpha,
                1f,
                wave
            );
    }

    private void SetVisible(
        bool visible)
    {
        isVisible = visible;

        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha =
            visible
                ? 1f
                : 0f;
    }

    private void ResetHitVisuals()
    {
        if (visualRoot != null)
        {
            visualRoot.localScale =
                normalScale;
        }

        SetEyeGlowAlpha(0f);
    }

    private void SetEyeGlowAlpha(
        float alpha)
    {
        if (eyeGlowGraphic == null)
        {
            return;
        }

        Color updatedColor =
            normalEyeGlowColor;

        updatedColor.a =
            Mathf.Clamp01(alpha);

        eyeGlowGraphic.color =
            updatedColor;
    }
}