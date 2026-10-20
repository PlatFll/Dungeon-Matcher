using UnityEngine;
using UnityEngine.UI;

/// <summary>Keep unchanged native icons centered when a preview shell is resized.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(Image))]
public sealed class DungeonPreviewNativeIcon : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    private Vector2 lastSize = new Vector2(-1, -1);
    public void Initialize(RectTransform parent) { panel = parent; Refresh(); }
    private void LateUpdate() { if (panel && panel.rect.size != lastSize) Refresh(); }
    private void Refresh()
    {
        if (!panel) return;
        lastSize = panel.rect.size;
        var image = GetComponent<Image>(); var r = image.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.sizeDelta = image.sprite.rect.size;
        r.anchoredPosition = new Vector2(Mathf.Floor((lastSize.x-r.rect.width)/2), -Mathf.Floor((lastSize.y-r.rect.height)/2));
    }
}
