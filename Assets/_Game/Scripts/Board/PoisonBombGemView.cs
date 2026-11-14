using UnityEngine;

[DisallowMultipleComponent]
public sealed class PoisonBombGemView : MonoBehaviour
{
    private static readonly int FlashAmountId =
        Shader.PropertyToID("_FlashAmount");

    private SpriteRenderer shellRenderer;
    private SpriteRenderer sourceIconRenderer;
    private GemSpecialType currentType;
    private Sprite still;
    private float idleTime;

    private void Update()
    {
        if (shellRenderer == null || !shellRenderer.enabled) return;
        idleTime += Time.deltaTime;
        ApplyIdle();
    }

    private void ApplyIdle()
    {
        shellRenderer.sprite = BombIdleArt.Frame(currentType, idleTime, still);
        // Native idle cels rock around the fixed socket; the view never drifts.
        transform.localPosition = Vector3.zero;
        if (sourceIconRenderer != null)
            sourceIconRenderer.transform.localPosition = BombIdleArt.SocketOffset(currentType);
    }

    private void OnDisable() { Hide(); }

    private MaterialPropertyBlock shellPropertyBlock;
    private MaterialPropertyBlock sourceIconPropertyBlock;

    public static PoisonBombGemView GetOrCreate(
        Transform gemTransform,
        SpriteRenderer templateRenderer)
    {
        if (gemTransform == null)
        {
            return null;
        }

        PoisonBombGemView existingView =
            gemTransform.GetComponentInChildren<
                PoisonBombGemView
            >(true);

        if (existingView != null)
        {
            existingView.EnsureRenderers(
                templateRenderer
            );

            return existingView;
        }

        GameObject viewObject =
            new GameObject("PoisonBombView");

        viewObject.transform.SetParent(
            gemTransform,
            false
        );

        PoisonBombGemView createdView =
            viewObject.AddComponent<
                PoisonBombGemView
            >();

        createdView.EnsureRenderers(
            templateRenderer
        );

        createdView.Hide();

        return createdView;
    }

    public void Show(
        Sprite bombSprite,
        Sprite sourceGemIcon,
        SpriteRenderer templateRenderer,
        GemSpecialType specialType = GemSpecialType.None)
    {
        EnsureRenderers(
            templateRenderer
        );

        if (shellRenderer == null)
        {
            return;
        }

        currentType = specialType;
        still = bombSprite;
        idleTime = 0f;
        shellRenderer.sprite = bombSprite;

        shellRenderer.color =
            Color.white;

        shellRenderer.enabled =
            bombSprite != null;

        if (sourceIconRenderer != null)
        {
            sourceIconRenderer.sprite =
                sourceGemIcon;

            sourceIconRenderer.color =
                Color.white;

            sourceIconRenderer.enabled =
                bombSprite != null &&
                sourceGemIcon != null;
        }

        SetFlashAmount(0f);
        ApplyIdle();
    }

    public void Hide()
    {
        currentType = GemSpecialType.None;
        still = null;
        idleTime = 0;
        transform.localPosition = Vector3.zero;
        SetFlashAmount(0f);

        if (shellRenderer != null)
        {
            shellRenderer.enabled = false;
            shellRenderer.sprite = null;
        }

        if (sourceIconRenderer != null)
        {
            sourceIconRenderer.enabled = false;
            sourceIconRenderer.sprite = null;
        }
    }

    public void SetFlashAmount(
        float amount)
    {
        float clampedAmount =
            Mathf.Clamp01(amount);

        ApplyFlashAmount(
            shellRenderer,
            ref shellPropertyBlock,
            clampedAmount
        );

        ApplyFlashAmount(
            sourceIconRenderer,
            ref sourceIconPropertyBlock,
            clampedAmount
        );
    }

    private void EnsureRenderers(
        SpriteRenderer templateRenderer)
    {
        if (shellRenderer == null)
        {
            shellRenderer =
                GetComponent<SpriteRenderer>();

            if (shellRenderer == null)
            {
                shellRenderer =
                    gameObject.AddComponent<
                        SpriteRenderer
                    >();
            }
        }

        if (sourceIconRenderer == null)
        {
            Transform existingIconTransform =
                transform.Find("SourceGemIcon");

            if (existingIconTransform != null)
            {
                sourceIconRenderer =
                    existingIconTransform
                        .GetComponent<SpriteRenderer>();
            }

            if (sourceIconRenderer == null)
            {
                GameObject iconObject =
                    new GameObject("SourceGemIcon");

                iconObject.transform.SetParent(
                    transform,
                    false
                );

                sourceIconRenderer =
                    iconObject.AddComponent<
                        SpriteRenderer
                    >();
            }
        }

        ConfigureRenderer(
            shellRenderer,
            templateRenderer,
            2
        );

        ConfigureRenderer(
            sourceIconRenderer,
            templateRenderer,
            3
        );

        transform.localPosition =
            Vector3.zero;

        transform.localRotation =
            Quaternion.identity;

        transform.localScale =
            Vector3.one;

        if (sourceIconRenderer != null)
        {
            sourceIconRenderer.transform.localPosition =
                Vector3.zero;

            sourceIconRenderer.transform.localRotation =
                Quaternion.identity;

            sourceIconRenderer.transform.localScale =
                Vector3.one;
        }
    }

    private static void ConfigureRenderer(
        SpriteRenderer renderer,
        SpriteRenderer templateRenderer,
        int sortingOrderOffset)
    {
        if (renderer == null ||
            templateRenderer == null)
        {
            return;
        }

        renderer.sortingLayerID =
            templateRenderer.sortingLayerID;

        renderer.sortingOrder =
            templateRenderer.sortingOrder +
            sortingOrderOffset;

        renderer.maskInteraction =
            templateRenderer.maskInteraction;

        renderer.sharedMaterial =
            templateRenderer.sharedMaterial;
    }

    private static void ApplyFlashAmount(
        SpriteRenderer renderer,
        ref MaterialPropertyBlock propertyBlock,
        float amount)
    {
        if (renderer == null)
        {
            return;
        }

        if (propertyBlock == null)
        {
            propertyBlock =
                new MaterialPropertyBlock();
        }

        renderer.GetPropertyBlock(
            propertyBlock
        );

        propertyBlock.SetFloat(
            FlashAmountId,
            amount
        );

        renderer.SetPropertyBlock(
            propertyBlock
        );
    }
}
