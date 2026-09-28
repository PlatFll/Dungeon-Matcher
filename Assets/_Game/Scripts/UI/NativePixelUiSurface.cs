using UnityEngine;
using UnityEngine.UI;

/// <summary>Keeps UI frame texels at integer physical pixels across the two canvas conventions.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(Image))]
public sealed class NativePixelUiSurface : MonoBehaviour
{
    private Image image;
    private Vector2 requestedSize;
    private void Awake()
    {
        image = GetComponent<Image>();
        requestedSize = image.rectTransform.rect.size;
    }
    private void LateUpdate()
    {
        if (image == null || image.sprite == null || image.canvas == null) return;
        float scale = Mathf.Max(.01f, image.canvas.rootCanvas.scaleFactor);
        if (image.type == Image.Type.Simple)
        {
            GameplayPixelGrid.FitImage(image, requestedSize);
            return;
        }
        int physicalPixels = Mathf.Max(1, Mathf.FloorToInt(scale + .00001f));
        image.pixelsPerUnitMultiplier = scale * image.canvas.referencePixelsPerUnit /
            (image.sprite.pixelsPerUnit * physicalPixels);
        GameplayPixelGrid.Snap(image.rectTransform);
    }
}
