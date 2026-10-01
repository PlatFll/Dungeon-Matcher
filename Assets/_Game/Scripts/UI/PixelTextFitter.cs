using TMPro;
using UnityEngine;

/// <summary>Fit Thaleah at whole screen-pixel multiples of its native 16-point em.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public sealed class PixelTextFitter : MonoBehaviour
{
    private TMP_Text label;
    private float requestedSize, appliedSize = -1, lastScale;
    private Vector2 lastRect;
    private string lastText;
    private TextWrappingModes lastWrapping;

    public static void Apply(TMP_Text text, float requested = -1)
    {
        if (text == null || GameUi.TmpFont == null) return;
        var fitter = text.GetComponent<PixelTextFitter>();
        if (fitter == null) fitter = text.gameObject.AddComponent<PixelTextFitter>();
        fitter.label = text;
        if (requested > 0) { fitter.requestedSize = requested; fitter.appliedSize = -1; }
        fitter.Fit();
    }

    private void LateUpdate() => Fit();

    private void Fit()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (label == null || GameUi.TmpFont == null) return;
        bool externalSize = appliedSize >= 0 && !Mathf.Approximately(label.fontSize, appliedSize);
        if (requestedSize <= 0 || externalSize) requestedSize = label.fontSize;
        var rect = label.rectTransform.rect.size;
        float scale = label.canvas != null ? Mathf.Max(.01f, label.canvas.rootCanvas.scaleFactor) : 1;
        if (label.font == GameUi.TmpFont && appliedSize >= 0 && !externalSize && rect == lastRect &&
            lastText == label.text && lastScale == scale && lastWrapping == label.textWrappingMode) return;

        label.font = GameUi.TmpFont;
        label.fontSharedMaterial = GameUi.TmpFont.material;
        label.enableAutoSizing = false;
        label.fontStyle = FontStyles.Normal;
        float step = 16 / scale;
        // Thaleah's seven-pixel capitals occupy less of the em than the old face.
        bool scrollContent = label.name == "GuideText" || label.name == "BuildRecap";
        float target = Mathf.Max(step, requestedSize * (scrollContent ? 1.2f : 2));
        int multiples = Mathf.Max(1, Mathf.FloorToInt(target / step));
        Vector4 margin = label.margin;
        float width = Mathf.Max(1, rect.x - margin.x - margin.z);
        float height = Mathf.Max(1, rect.y - margin.y - margin.w);
        for (; multiples >= 1; multiples--)
        {
            label.fontSize = multiples * step;
            Vector2 preferred = label.GetPreferredValues(label.text, width, float.PositiveInfinity);
            if (preferred.x <= width + .1f && (scrollContent || preferred.y <= height + .1f))
            {
                if (scrollContent) label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferred.y + margin.y + margin.w);
                break;
            }
        }
        appliedSize = label.fontSize; lastRect = label.rectTransform.rect.size;
        lastText = label.text; lastScale = scale; lastWrapping = label.textWrappingMode;
    }
}
