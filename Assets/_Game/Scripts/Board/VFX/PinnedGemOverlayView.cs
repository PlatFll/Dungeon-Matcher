using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PinnedGemOverlayView :
    MonoBehaviour
{
    private static readonly int FlashAmountId =
        Shader.PropertyToID(
            "_FlashAmount"
        );

    private const string OverlayObjectName =
        "PinnedBoltOverlay";

    private Gem gem;
    private BoardController boardController;
    private int ownerInstanceId;

    private GameObject overlayObject;
    private SpriteRenderer overlayRenderer;
    private MaterialPropertyBlock propertyBlock;

    /*
     * Store the latest undimmed color separately from the color we applied.
     * If another system changes a gem's visual while it is pinned (for example
     * a Color Crystal converts it into a bomb), LateUpdate detects that new
     * source color and reapplies the pin dim instead of letting it pop bright.
     */
    private readonly Dictionary<SpriteRenderer, Color>
        sourceRendererColors =
            new Dictionary<SpriteRenderer, Color>();

    private readonly Dictionary<SpriteRenderer, Color>
        lastDimmedRendererColors =
            new Dictionary<SpriteRenderer, Color>();

    private readonly List<SpriteRenderer>
        trackedRenderers =
            new List<SpriteRenderer>();

    private GemSpecialOverlayView specialOverlayView;
    private bool specialOverlayWasEnabled;

    private float dimBrightness = 0.55f;
    private float materializeDuration = 0.10f;
    private float shakeDistance = 0.04f;
    private float shakeDuration = 0.12f;

    private bool released;

    public int OwnerInstanceId =>
        ownerInstanceId;

    public void Initialize(
        Gem targetGem,
        BoardController board,
        int ownerId,
        Sprite boltSprite,
        float brightness,
        float flashDuration,
        float horizontalShakeDistance,
        float horizontalShakeDuration)
    {
        gem = targetGem;
        boardController = board;
        ownerInstanceId = ownerId;

        dimBrightness =
            Mathf.Clamp(
                brightness,
                0.15f,
                1f
            );

        materializeDuration =
            Mathf.Max(
                0.02f,
                flashDuration
            );

        shakeDistance =
            Mathf.Max(
                0f,
                horizontalShakeDistance
            );

        shakeDuration =
            Mathf.Max(
                0.02f,
                horizontalShakeDuration
            );

        CacheAndDimGemVisuals();
        CreateOverlay(boltSprite);
    }

    private void LateUpdate()
    {
        if (released ||
            gem == null ||
            trackedRenderers.Count == 0)
        {
            return;
        }

        for (int index = 0;
             index < trackedRenderers.Count;
             index++)
        {
            SpriteRenderer renderer =
                trackedRenderers[index];

            if (renderer == null)
            {
                continue;
            }

            Color currentColor =
                renderer.color;

            if (lastDimmedRendererColors.TryGetValue(
                    renderer,
                    out Color lastDimmed) &&
                ColorsApproximatelyEqual(
                    currentColor,
                    lastDimmed))
            {
                continue;
            }

            /*
             * Something intentionally changed the renderer while pinned.
             * Treat that as the new real source color, then dim that state.
             */
            sourceRendererColors[renderer] =
                currentColor;

            ApplyDimmedColor(
                renderer,
                currentColor
            );
        }
    }

    public IEnumerator PlayBoltImpact()
    {
        if (gem == null)
        {
            yield break;
        }

        Vector3 restingPosition =
            gem.transform.localPosition;

        Vector3 normalOverlayScale =
            overlayRenderer != null
                ? overlayRenderer.transform.localScale
                : Vector3.one;

        float duration =
            Mathf.Max(
                materializeDuration,
                shakeDuration
            );

        float elapsed = 0f;

        while (elapsed < duration &&
               gem != null)
        {
            if (overlayRenderer != null)
            {
                float materializeProgress =
                    Mathf.Clamp01(
                        elapsed /
                        materializeDuration
                    );

                SetOverlayFlashAmount(
                    1f - materializeProgress
                );

                overlayRenderer.transform.localScale =
                    Vector3.Lerp(
                        normalOverlayScale * 1.10f,
                        normalOverlayScale,
                        Mathf.SmoothStep(
                            0f,
                            1f,
                            materializeProgress
                        )
                    );
            }

            float shakeProgress =
                Mathf.Clamp01(
                    elapsed /
                    shakeDuration
                );

            float shakeEnvelope =
                1f - shakeProgress;

            float horizontalOffset =
                Mathf.Sin(
                    shakeProgress *
                    Mathf.PI *
                    8f
                ) *
                shakeDistance *
                shakeEnvelope;

            gem.transform.localPosition =
                restingPosition +
                Vector3.right *
                horizontalOffset;

            elapsed +=
                Time.deltaTime;

            yield return null;
        }

        if (gem != null)
        {
            gem.transform.localPosition =
                restingPosition;
        }

        if (overlayRenderer != null)
        {
            overlayRenderer.transform.localScale =
                normalOverlayScale;

            SetOverlayFlashAmount(0f);
        }
    }

    public void ReleaseVisual()
    {
        if (released)
        {
            return;
        }

        released = true;
        RestoreGemVisuals();

        if (overlayObject != null)
        {
            Destroy(overlayObject);
            overlayObject = null;
            overlayRenderer = null;
        }
    }

    private void CacheAndDimGemVisuals()
    {
        if (gem == null)
        {
            return;
        }

        trackedRenderers.Clear();

        SpriteRenderer[] renderers =
            gem.GetComponentsInChildren<SpriteRenderer>(
                true
            );

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            trackedRenderers.Add(renderer);

            Color sourceColor =
                renderer.color;

            sourceRendererColors[renderer] =
                sourceColor;

            ApplyDimmedColor(
                renderer,
                sourceColor
            );
        }

        specialOverlayView =
            gem.GetComponentInChildren<
                GemSpecialOverlayView
            >(true);

        if (specialOverlayView != null)
        {
            specialOverlayWasEnabled =
                specialOverlayView.enabled;

            /*
             * Prevent pulse/shimmer Update from continuously overwriting the
             * dim. Direct Show/Hide calls can still change its SpriteRenderer;
             * LateUpdate above detects those changes and dims the new state.
             */
            specialOverlayView.enabled = false;
        }
    }

    private void ApplyDimmedColor(
        SpriteRenderer renderer,
        Color sourceColor)
    {
        if (renderer == null)
        {
            return;
        }

        Color dimmed =
            sourceColor;

        dimmed.r *= dimBrightness;
        dimmed.g *= dimBrightness;
        dimmed.b *= dimBrightness;

        renderer.color = dimmed;

        lastDimmedRendererColors[renderer] =
            dimmed;
    }

    private void RestoreGemVisuals()
    {
        foreach (
            KeyValuePair<SpriteRenderer, Color> entry
            in sourceRendererColors)
        {
            if (entry.Key != null)
            {
                entry.Key.color =
                    entry.Value;
            }
        }

        sourceRendererColors.Clear();
        lastDimmedRendererColors.Clear();
        trackedRenderers.Clear();

        if (specialOverlayView != null)
        {
            specialOverlayView.enabled =
                specialOverlayWasEnabled;
        }

        if (gem != null)
        {
            /*
             * Rebuild from the Gem's current state instead of assuming it has
             * the same special type/color it had when the bolt was fired.
             */
            gem.SetSelected(false);
            gem.SetSpecialType(
                gem.SpecialType
            );
        }
    }

    private static bool ColorsApproximatelyEqual(
        Color first,
        Color second)
    {
        const float epsilon = 0.001f;

        return
            Mathf.Abs(first.r - second.r) <= epsilon &&
            Mathf.Abs(first.g - second.g) <= epsilon &&
            Mathf.Abs(first.b - second.b) <= epsilon &&
            Mathf.Abs(first.a - second.a) <= epsilon;
    }

    private void CreateOverlay(
        Sprite boltSprite)
    {
        overlayObject =
            new GameObject(
                OverlayObjectName
            );

        overlayObject.transform.SetParent(
            transform,
            false
        );

        overlayObject.transform.localPosition =
            Vector3.zero;

        overlayObject.transform.localRotation =
            Quaternion.identity;

        overlayObject.transform.localScale =
            Vector3.one;

        overlayRenderer =
            overlayObject.AddComponent<
                SpriteRenderer
            >();

        overlayRenderer.sprite =
            boltSprite;

        overlayRenderer.color =
            Color.white;

        overlayRenderer.sortingLayerName =
            "Gems";

        overlayRenderer.sortingOrder = 8;

        overlayRenderer.maskInteraction =
            SpriteMaskInteraction
                .VisibleInsideMask;

        SpriteRenderer gemRenderer =
            gem != null
                ? gem.GetComponent<SpriteRenderer>()
                : null;

        if (gemRenderer != null)
        {
            overlayRenderer.sharedMaterial =
                gemRenderer.sharedMaterial;
        }

        overlayRenderer.enabled =
            boltSprite != null;

        SetOverlayFlashAmount(1f);
    }

    private void SetOverlayFlashAmount(
        float amount)
    {
        if (overlayRenderer == null)
        {
            return;
        }

        if (propertyBlock == null)
        {
            propertyBlock =
                new MaterialPropertyBlock();
        }

        overlayRenderer.GetPropertyBlock(
            propertyBlock
        );

        propertyBlock.SetFloat(
            FlashAmountId,
            Mathf.Clamp01(amount)
        );

        overlayRenderer.SetPropertyBlock(
            propertyBlock
        );
    }

    private void OnDestroy()
    {
        RestoreGemVisuals();

        if (overlayObject != null)
        {
            Destroy(overlayObject);
            overlayObject = null;
            overlayRenderer = null;
        }

        /*
         * A bomb, crystal, Miner or ordinary clear can destroy a pinned gem
         * directly. Notify the board so owner counts never retain a dead Gem.
         */
        if (!released &&
            boardController != null)
        {
            boardController.NotifyPinnedGemDestroyed(
                gem,
                ownerInstanceId
            );
        }
    }
}
