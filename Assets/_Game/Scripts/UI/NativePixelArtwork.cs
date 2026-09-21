using UnityEngine;

/// <summary>Sizes decorative menu art at an integer physical source-pixel ratio.</summary>
[DisallowMultipleComponent]
public sealed class NativePixelArtwork : MonoBehaviour
{
    private Sprite sprite;
    private bool cover;
    private float verticalPosition;
    private RectTransform rect;
    private Canvas canvas;

    public void Initialize(Sprite value, bool fillViewport, float y)
    {
        sprite = value; cover = fillViewport; verticalPosition = y;
        rect = (RectTransform)transform; canvas = GetComponentInParent<Canvas>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
        Refresh();
    }

    private void LateUpdate() => Refresh();
    private void Refresh()
    {
        if (sprite == null || canvas == null || rect == null) return;
        var parent = rect.parent as RectTransform;
        if (parent == null) return;
        float scale = Mathf.Max(.01f,canvas.scaleFactor);
        Vector2 size = sprite.rect.size;
        Vector2 available = parent.rect.size * scale;
        float ratio = cover ? Mathf.Ceil(Mathf.Max(available.x/size.x,available.y/size.y))
            : Mathf.Max(1,Mathf.Floor(Mathf.Min(available.x*.9f/size.x,420*scale/size.x)));
        rect.sizeDelta = size * ratio / scale;
        rect.anchoredPosition = new Vector2(0,Mathf.Round(verticalPosition*scale)/scale);
    }
}
