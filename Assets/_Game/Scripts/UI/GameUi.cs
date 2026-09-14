using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shared runtime typography and small controls, configured by Resources/UI/Typography.</summary>
public static class GameUi
{
    private static Font font;
    private static TMP_FontAsset tmpFont;
    private static UiTypography typography;
    private static UiTypography Typography => typography != null ? typography : typography = Resources.Load<UiTypography>("UI/Typography");
    public static Font Font => font != null ? font : font = Typography != null && Typography.regularFont != null ? Typography.regularFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    public static TMP_FontAsset TmpFont => tmpFont != null ? tmpFont : tmpFont = Typography != null && Typography.textMeshProFont != null ? Typography.textMeshProFont : Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    public static readonly Color Face = new Color32(28, 21, 39, 255);
    public static readonly Color Purple = new Color32(86, 49, 109, 255);

    public static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size; rect.anchoredPosition = position;
        return rect;
    }
    public static Text Label(string name, Transform parent, string value, Vector2 size, Vector2 position, int fontSize = 20)
    {
        var rect = Rect(name, parent, size, position);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = Font; text.fontSize = fontSize; text.text = value;
        text.color = Color.white; text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false; text.horizontalOverflow = HorizontalWrapMode.Wrap;
        return text;
    }
    public static Button Button(string name, Transform parent, string text, Vector2 size, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        var rect = Rect(name, parent, size, position);
        var image = rect.gameObject.AddComponent<Image>(); image.color = Purple;
        var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        var colors = button.colors; colors.highlightedColor = new Color(1.2f,1.2f,1.2f); colors.disabledColor = new Color(0.45f,0.45f,0.45f); button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.onClick.AddListener(action);
        Label("Label", rect, text, size - new Vector2(12, 4), Vector2.zero);
        return button;
    }
    public static RectTransform Panel(string name, Transform parent, Vector2 size)
    {
        var rect = Rect(name, parent, size, Vector2.zero);
        var image = rect.gameObject.AddComponent<Image>(); image.color = Face;
        var outline = rect.gameObject.AddComponent<Outline>(); outline.effectColor = Purple; outline.effectDistance = new Vector2(3, -3);
        return rect;
    }
}
