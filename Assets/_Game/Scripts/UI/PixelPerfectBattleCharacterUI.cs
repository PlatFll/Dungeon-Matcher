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

    private void Awake() => root = (RectTransform)transform;

    private void LateUpdate()
    {
        bool isPlayer = name == "PlayerCharacter";
        Image image = isPlayer ? GetComponent<Image>() : FindCharacterImage();
        if (image == null || image.sprite == null || image.canvas == null) return;
        RectTransform visual = image.rectTransform;
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
            GameplayPixelGrid.FitImage(image, available, EnemyReferenceCanvasSize);
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
