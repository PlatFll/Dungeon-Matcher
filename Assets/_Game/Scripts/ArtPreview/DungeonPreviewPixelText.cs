using TMPro;
using UnityEngine;

/// <summary>Align bitmap glyph geometry to this isolated preview's native pixel grid.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public sealed class DungeonPreviewPixelText : MonoBehaviour
{
    [SerializeField] private RectTransform nativeRoot;
    private TMP_Text label;

    public void Initialize(RectTransform root)
    {
        nativeRoot = root;
        if (!label) label = GetComponent<TMP_Text>();
        label.havePropertiesChanged = true;
    }

    private void OnEnable()
    {
        label = GetComponent<TMP_Text>();
        label.OnPreRenderText += Snap;
    }

    private void OnDisable()
    {
        if (label) label.OnPreRenderText -= Snap;
    }

    private void Snap(TMP_TextInfo text)
    {
        if (!nativeRoot) return;
        foreach (var mesh in text.meshInfo)
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            var point = nativeRoot.InverseTransformPoint(transform.TransformPoint(mesh.vertices[i]));
            // Use the same tie direction for both edges so integer glyph widths
            // survive half-pixel centering on odd-sized plaque rectangles.
            point.x = Mathf.Floor(point.x + .5f);
            point.y = Mathf.Floor(point.y + .5f);
            mesh.vertices[i] = transform.InverseTransformPoint(nativeRoot.TransformPoint(point));
        }
    }
}
