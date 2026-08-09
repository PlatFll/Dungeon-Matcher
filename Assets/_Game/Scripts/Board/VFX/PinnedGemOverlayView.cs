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

    private readonly Dictionary<SpriteRenderer, Color>
        originalRendererColors =
            new Dictionary<SpriteRenderer, Color>();

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

            originalRendererColors[renderer] =
                renderer.color;

            Color dimmed =
                renderer.color;

            dimmed.r *= dimBrightness;
            dimmed.g *= dimBrightness;
            dimmed.b *= dimBrightness;

            renderer.color = dimmed;
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
             * Prevent its pulse/shimmer Update from overwriting the dim color
             * while the bolt is attached. The renderer itself stays visible.
             */
            specialOverlayView.enabled = false;
        }
    }

    private void RestoreGemVisuals()
    {
        foreach (
            KeyValuePair<SpriteRenderer, Color> entry
            in originalRendererColors)
        {
            if (entry.Key != null)
            {
                entry.Key.color =
                    entry.Value;
            }
        }

        originalRendererColors.Clear();

        if (specialOverlayView != null)
        {
            specialOverlayView.enabled =
                specialOverlayWasEnabled;
        }
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
