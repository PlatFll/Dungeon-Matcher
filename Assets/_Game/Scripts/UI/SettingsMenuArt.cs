using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Approved modular settings art. Existing controllers own every action and preference.</summary>
public static class SettingsMenuArt
{
    private const string Folder = "SettingsV2/";
    private static Sprite Load(string name) => GlobalMenuSkin.Load(Folder + name);
    public static bool Available => Load("BodyTile") != null;

    // Coordinates are native top-left pixels within the panel, matching the reviewed assembly.
    private static Vector2 Position(RectTransform parent, float x, float y, float w, float h) =>
        new Vector2(x + w / 2 - parent.rect.width / 2, parent.rect.height / 2 - y - h / 2);

    private static Image Art(RectTransform parent, string name, float x, float y, float w = 0, float h = 0, bool tile = false)
    {
        var sprite = Load(name);
        if (w == 0) w = sprite.rect.width;
        if (h == 0) h = sprite.rect.height;
        var image = GlobalMenuSkin.Art(name, parent, Folder + name, new Vector2(w, h), Position(parent, x, y, w, h));
        if (tile)
        {
            image.type = Image.Type.Tiled;
            image.pixelsPerUnitMultiplier = image.canvas.referencePixelsPerUnit / sprite.pixelsPerUnit;
        }
        return image;
    }

    public static void Panel(RectTransform panel, bool battle)
    {
        float height = battle ? 532 : 348;
        panel.sizeDelta = new Vector2(304, height);
        if (panel.TryGetComponent<NativePixelUiSurface>(out var native)) native.enabled = false;
        if (panel.TryGetComponent<Outline>(out var outline)) outline.enabled = false;
        if (panel.TryGetComponent<Image>(out var old)) { old.sprite = null; old.color = Color.clear; }
        var oldTitle = panel.Find("Title");
        if (oldTitle != null) oldTitle.gameObject.SetActive(false);
        Art(panel, "BodyTile", 0, 0, 304, height, true);
        Art(panel, "HeaderTile", 12, 12, 280, 48, true);
        Art(panel, "EdgeTop", 12, 0, 280, 12, true);
        Art(panel, "EdgeBottom", 12, height - 12, 280, 12, true);
        Art(panel, "EdgeLeft", 0, 12, 12, height - 24, true);
        Art(panel, "EdgeRight", 292, 12, 12, height - 24, true);
        Art(panel, "HeaderRule", 12, 60, 280, 8, true);
        Art(panel, "CornerTL", 0, 0); Art(panel, "CornerTR", 278, 0);
        Art(panel, "CornerBL", 0, height - 26); Art(panel, "CornerBR", 278, height - 26);
        Art(panel, "CrownedCrest", 120, -18);
        GlobalMenuSkin.Label("SettingsHeading", panel, "SETTINGS", new Vector2(256, 48), Position(panel, 24, 12, 256, 48), 40);
        Rows(panel);
        if (battle) Art(panel, "Divider", 32, 272);
    }

    private static void Rows(RectTransform panel)
    {
        Row(panel, "Music", "MUSIC", "Music", 70, () => !AudioPreferences.MusicMuted, () => AudioPreferences.SetMusicMuted(!AudioPreferences.MusicMuted));
        Row(panel, "SFX", "SFX", "SFX", 120, () => !AudioPreferences.SfxMuted, () => AudioPreferences.SetSfxMuted(!AudioPreferences.SfxMuted));
        Row(panel, "ReducedMotion", "REDUCED MOTION", "Motion", 170, () => PresentationPreferences.ReducedMotion, () => PresentationPreferences.SetReducedMotion(!PresentationPreferences.ReducedMotion));
        Row(panel, "Vibration", "VIBRATION", "Vibration", 220, () => !AudioPreferences.VibrationMuted, () => AudioPreferences.SetVibrationMuted(!AudioPreferences.VibrationMuted));
    }

    private static void Row(RectTransform panel, string name, string text, string icon, float y, Func<bool> value, UnityAction toggle)
    {
        var button = GameUi.Button(name, panel, text, new Vector2(264, 44), Position(panel, 20, y, 264, 44), () => { });
        GlobalMenuSkin.Skin(button, Folder + "Row");
        var label = button.GetComponentInChildren<TMP_Text>();
        GlobalMenuSkin.Place(label.rectTransform, button.transform, new Vector2(138, 28), new Vector2(-19, 0));
        GlobalMenuSkin.StyleText(label, 22); label.alignment = TextAlignmentOptions.MidlineLeft;
        var rect = (RectTransform)button.transform;
        Art(rect, "Icon" + icon, 8, 8);
        var state = Art(rect, value() ? "ToggleOn" : "ToggleOff", 186, 7);
        state.name = "Toggle";
        var word = GlobalMenuSkin.Label("State", state.transform, "", new Vector2(34, 26), Vector2.zero, 22);
        void Refresh()
        {
            bool on = value(); state.sprite = Load(on ? "ToggleOn" : "ToggleOff"); word.text = on ? "ON" : "OFF";
            // Text occupies the exposed half, opposite the physical slider thumb.
            word.rectTransform.anchoredPosition = new Vector2(on ? -15 : 15, 0);
        }
        Refresh(); button.onClick.AddListener(() => { toggle(); Refresh(); });
    }

    public static Button Action(RectTransform panel, string name, string text, string kind, float top, UnityAction action)
    {
        bool gray = kind == "Suspend" || kind == "Retry";
        string family = "Button" + (gray ? "Gray" : kind == "EndRun" ? "Red" : "Green");
        var size = Load(family).rect.size;
        var button = GameUi.Button(name, panel, text, size, Position(panel, 28, top, size.x, size.y), action);
        GlobalMenuSkin.Skin(button, Folder + family);
        var label = button.GetComponentInChildren<TMP_Text>();
        GlobalMenuSkin.Place(label.rectTransform, button.transform, new Vector2(204, 36), Vector2.zero);
        GlobalMenuSkin.StyleText(label, kind == "Retry" ? 20 : gray ? 22 : 28);
        var icon = GlobalMenuSkin.Art("ActionIcon", button.transform, Folder + "Icon" + kind, new Vector2(24, 24), Vector2.zero);
        // Actual opaque bounds from the approved 24x24 glyph sources (top-left coordinates).
        Rect bounds = kind == "Resume" || kind == "Back" ? new Rect(5, 3, 14, 18) :
            kind == "Suspend" ? new Rect(1, 0, 21, 21) :
            kind == "Retry" ? new Rect(3, 2, 18, 19) : new Rect(2, 3, 18, 19);
        button.gameObject.AddComponent<SettingsActionContent>().Initialize(label, icon, bounds);
        return button;
    }
}

/// <summary>Centers painted icon/text bounds together, including font-size changes at integer screen scales.</summary>
public sealed class SettingsActionContent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private TMP_Text label;
    private Image icon;
    private Rect iconBounds;
    private bool pressed;
    public void Initialize(TMP_Text text, Image glyph, Rect bounds) { label = text; icon = glyph; iconBounds = bounds; }
    private void LateUpdate()
    {
        if (label == null) return;
        label.ForceMeshUpdate();
        var bounds = label.textBounds;
        float width = iconBounds.width + 12 + bounds.size.x;
        float left = -width / 2;
        float dy = pressed ? -1 : 0;
        icon.rectTransform.anchoredPosition = new Vector2(Mathf.Round(left + 12 - iconBounds.x), Mathf.Round(iconBounds.center.y - 12) + dy);
        label.rectTransform.anchoredPosition = new Vector2(Mathf.Round(left + iconBounds.width + 12 - bounds.min.x), Mathf.Round(-bounds.center.y) + dy);
    }
    public void OnPointerDown(PointerEventData data) { if (data.button == PointerEventData.InputButton.Left) pressed = true; }
    public void OnPointerUp(PointerEventData data) => pressed = false;
    public void OnPointerExit(PointerEventData data) => pressed = false;
}
