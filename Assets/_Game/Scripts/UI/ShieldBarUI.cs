using UnityEngine;
using UnityEngine.UI;

/// <summary>A compact shield track below the HP frame, sharing its content insets.</summary>
[DisallowMultipleComponent]
public sealed class ShieldBarUI : MonoBehaviour
{
    public const float TrackHeight = 6f;
    public const float ReservedSpaceBelowHealthBar = TrackHeight + 4f;
    private RectTransform track, fill, glint;
    private float normalized;
    public bool IsVisible => track != null && track.gameObject.activeSelf;

    public static void Show(GameObject healthBar, int current, int maximum)
    {
        if (healthBar == null) return;
        var view = healthBar.GetComponent<ShieldBarUI>();
        if (view == null && current > 0) view = healthBar.AddComponent<ShieldBarUI>();
        if (view != null) view.SetValue(current, maximum);
    }

    private void SetValue(int current, int maximum)
    {
        normalized = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0;
        if (current > 0 && track == null)
        {
            track = Make("ShieldTrack", transform, new Color32(10, 20, 35, 255));
            fill = Make("ShieldFill", track, new Color32(39, 155, 238, 255));
            glint = Make("ShieldHighlight", fill, new Color32(140, 239, 255, 255));
        }
        if (track != null) track.gameObject.SetActive(current > 0);
        RefreshGeometry();
    }

    private void LateUpdate() => RefreshGeometry();
    private void OnDisable() { if (track != null) track.gameObject.SetActive(false); }

    private void RefreshGeometry()
    {
        if (!IsVisible) return;
        var modular = GetComponent<ModularHealthBarUI>();
        Vector2 insets = modular != null ? modular.ContentInsets : new Vector2(4, 4);
        track.anchorMin = Vector2.zero; track.anchorMax = new Vector2(1, 0);
        track.offsetMin = new Vector2(insets.x - 1, -TrackHeight);
        track.offsetMax = new Vector2(1 - insets.y, 0);
        fill.anchorMin = fill.anchorMax = Vector2.zero;
        fill.pivot = Vector2.zero; fill.anchoredPosition = Vector2.one;
        fill.sizeDelta = new Vector2(Mathf.Round(Mathf.Max(0, track.rect.width - 2) * normalized), TrackHeight - 2);
        glint.anchorMin = new Vector2(0, 1); glint.anchorMax = Vector2.one;
        glint.offsetMin = new Vector2(0, -1); glint.offsetMax = Vector2.zero;
    }

    private static RectTransform Make(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
        return image.rectTransform;
    }
}
