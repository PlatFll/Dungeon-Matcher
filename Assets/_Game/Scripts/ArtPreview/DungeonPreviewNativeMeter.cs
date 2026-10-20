using UnityEngine;
using UnityEngine.UI;

/// <summary>Explicit art-preview fill states; never reads or owns gameplay energy.</summary>
[DisallowMultipleComponent]
public sealed class DungeonPreviewNativeMeter : MonoBehaviour
{
    [SerializeField] private RectTransform clip, fill;
    [SerializeField] private int inset;
    [SerializeField, Range(0, 1)] private float amount;
    private Vector2 lastSize;
    public void Initialize(Sprite sprite, int padding)
    {
        inset = padding;
        clip = new GameObject("NativeFillMask", typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();
        clip.SetParent(transform, false); clip.anchorMin = clip.anchorMax = clip.pivot = new Vector2(0, 1);
        fill = new GameObject("NativeFill", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        fill.SetParent(clip, false); fill.anchorMin = fill.anchorMax = fill.pivot = new Vector2(0, 1);
        var image = fill.GetComponent<Image>(); image.sprite = sprite; image.type = Image.Type.Tiled;
        image.raycastTarget = false; image.pixelsPerUnitMultiplier = 1;
        SetFill(0);
    }
    public void SetFill(float value) { amount = Mathf.Clamp01(value); Refresh(); }
    private void LateUpdate() { if (((RectTransform)transform).rect.size != lastSize) Refresh(); }
    private void Refresh()
    {
        if (!clip || !fill) return;
        lastSize = ((RectTransform)transform).rect.size;
        int width = Mathf.Max(0, Mathf.FloorToInt(lastSize.x) - 2 * inset);
        int height = (int)fill.GetComponent<Image>().sprite.rect.height;
        clip.anchoredPosition = new Vector2(inset, -Mathf.Floor((lastSize.y - height) / 2));
        clip.sizeDelta = new Vector2(Mathf.Floor(width * amount + .5f), height);
        clip.gameObject.SetActive(clip.sizeDelta.x > 0);
        fill.sizeDelta = new Vector2(width + 16, height); fill.anchoredPosition = Vector2.zero;
    }
}
