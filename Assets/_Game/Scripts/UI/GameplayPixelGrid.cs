using UnityEngine;
using UnityEngine.UI;

/// <summary>Shared physical phase for UI artwork inside assigned rectangles.</summary>
public static class GameplayPixelGrid
{
    public static void Snap(RectTransform rect)
    {
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        if (canvas == null || rect.parent is not RectTransform parent) return;
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 point = RectTransformUtility.WorldToScreenPoint(camera,
            rect.TransformPoint(new Vector3(rect.rect.xMin, rect.rect.yMin, 0)));
        Vector2 rounded = new Vector2(Mathf.Round(point.x), Mathf.Round(point.y));
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, point, camera, out Vector2 before);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, rounded, camera, out Vector2 after);
        rect.anchoredPosition += after - before;
    }

    public static void FitImage(Image image, Vector2 requestedSize)
    {
        if (image == null || image.sprite == null || image.canvas == null) return;
        FitImage(image, requestedSize, image.sprite.rect.size);
    }

    /// <summary>
    /// Fits an image using a stable reference canvas to choose its integer texel scale,
    /// while still sizing the rendered rect from the sprite's real source dimensions.
    /// This lets oversized animation frames extend beyond a normal character canvas
    /// without shrinking their source pixels to fit the normal authored box.
    /// </summary>
    public static void FitImage(Image image, Vector2 requestedSize, Vector2 referenceSourceSize)
    {
        if (image == null || image.sprite == null || image.canvas == null) return;
        RectTransform rect = image.rectTransform;
        rect.localScale = Vector3.one;
        Vector2 source = image.sprite.rect.size;
        Vector2 fitSource = referenceSourceSize.x > 0f && referenceSourceSize.y > 0f
            ? referenceSourceSize
            : source;
        float canvasScale = image.canvas.rootCanvas.scaleFactor;
        int ratio = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(requestedSize.x / fitSource.x,
            requestedSize.y / fitSource.y) * canvasScale + 0.00001f));
        Vector2 size = source * ratio / canvasScale;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        image.preserveAspect = false;
        Snap(rect);
    }
}
