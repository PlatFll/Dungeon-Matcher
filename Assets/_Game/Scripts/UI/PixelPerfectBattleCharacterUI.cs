using UnityEngine;
using UnityEngine.UI;

/// <summary>Character source texels use the shared gameplay physical pixel grid.</summary>
[DefaultExecutionOrder(10000)]
[DisallowMultipleComponent]
public sealed class PixelPerfectBattleCharacterUI : MonoBehaviour
{
    // Enemy body scale is authored against the normal 64x64 character canvas.
    // Larger animation frames (for example the Miner's 80x80 frames) keep the
    // same source-texel scale and are allowed to extend outside that envelope.
    private static readonly Vector2 EnemyReferenceCanvasSize = new Vector2(64f, 64f);

    private RectTransform root;
    private Image observedImage;
    private Vector2 requestedBox;
    private Vector2 appliedSize;
    private bool hasApplied;
    private RectTransform enemyRoot;
    private Vector2 enemyRestPosition;

    private void Awake() => root = (RectTransform)transform;

    private void LateUpdate()
    {
        bool isPlayer = name == "PlayerCharacter";
        Image image = isPlayer ? GetComponent<Image>() : FindCharacterImage();
        if (image == null || image.sprite == null || image.canvas == null) return;
        RectTransform visual = image.rectTransform;
        if (!isPlayer && image != observedImage)
        {
            enemyRoot = image.GetComponentInParent<EnemyActor>()?.transform as RectTransform;
            if (enemyRoot != null) enemyRestPosition = enemyRoot.anchoredPosition;
        }
        // A new definition supplies an authored box and preserveAspect=true.
        // Animation sprite changes do not replace that box with our last result.
        if (image != observedImage || !hasApplied || image.preserveAspect ||
            (visual.rect.size - appliedSize).sqrMagnitude > 0.0001f)
        {
            requestedBox = visual.rect.size;
            observedImage = image;
        }
        Vector2 available = requestedBox;
        if (!isPlayer && root.parent is RectTransform slot)
            available.x = Mathf.Min(available.x, slot.rect.width);
        root.localScale = Vector3.one;
        visual.localScale = Vector3.one;
        GameplayPixelGrid.Snap(root);
        if (isPlayer)
            GameplayPixelGrid.FitImage(image, available);
        else
        {
            GameplayPixelGrid.FitImage(image, available, EnemyReferenceCanvasSize);
            // The center-pivoted Image grows downward with taller canvases.
            // Compensate on the actor, leaving the layout-owned spawn anchor
            // and the feedback-owned VisualRoot untouched, including resizes.
            if (enemyRoot != null)
            {
                float canvasGroundOffset = Mathf.Max(0f, image.sprite.rect.height - EnemyReferenceCanvasSize.y) *
                    (visual.rect.height / image.sprite.rect.height) * visual.pivot.y;
                enemyRoot.anchoredPosition = enemyRestPosition + Vector2.up * canvasGroundOffset;
                GameplayPixelGrid.Snap(enemyRoot);
            }
        }
        appliedSize = visual.rect.size;
        hasApplied = true;
    }

    private Image FindCharacterImage()
    {
        foreach (Image image in GetComponentsInChildren<Image>(true))
            if (image.name == "VisualRoot") return image;
        return null;
    }
}
