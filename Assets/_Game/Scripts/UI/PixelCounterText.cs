using TMPro;
using UnityEngine;

/// <summary>One logical pixel outline for the project's bitmap font.</summary>
public sealed class PixelCounterText : MonoBehaviour
{
    private TMP_Text face;
    private readonly TMP_Text[] outline = new TMP_Text[8];
    public TMP_Text Face => face;
    public static PixelCounterText Create(string name, Transform parent, Vector2 size, Vector2 position, int fontSize)
    {
        var root = GameUi.Rect(name, parent, size, position);
        var counter = root.gameObject.AddComponent<PixelCounterText>();
        int i = 0;
        for (int y = -1; y <= 1; y++) for (int x = -1; x <= 1; x++)
        {
            if (x == 0 && y == 0) continue;
            var label = GameUi.Label("Outline", root, "", size, new Vector2(x,y), fontSize);
            label.color = new Color32(10,13,17,255);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            counter.outline[i++] = label;
        }
        counter.face = GameUi.Label("Value", root, "", size, Vector2.zero, fontSize);
        counter.face.color = new Color32(253,245,229,255);
        counter.face.textWrappingMode = TextWrappingModes.NoWrap;
        return counter;
    }
    public void Set(string value, Color color)
    {
        face.text = value; face.color = color;
        foreach (var label in outline) { label.text = value; label.alignment = face.alignment; }
    }
}
