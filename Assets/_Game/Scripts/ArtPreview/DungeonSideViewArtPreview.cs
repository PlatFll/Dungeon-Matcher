using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Isolated reference-layout art preview. It never creates gameplay or input owners.</summary>
public sealed class DungeonSideViewArtPreview : MonoBehaviour
{
    public Camera previewCamera;
    public Canvas previewCanvas;
    public Sprite[] modules;
    public TextAsset placementManifest;
    public int width = 1080, height = 1920;
    public Rect safeArea = new Rect(0, 0, 1080, 1920);
    public int PixelScale { get; private set; }
    public Rect ContentPixels { get; private set; }
    public RectTransform Content { get; private set; }
    public readonly List<RectTransform> Pieces = new List<RectTransform>();
    [Serializable] public class Placement { public string name; public int x, y, layer; public int[] clip; }
    [Serializable] public class Manifest { public int width, height, pixelsPerUnit; public Placement[] placements; public NativePanel[] panels; }

    [Serializable] public class NativePanel
    {
        public string name, prefix, backing;
        public int x, y, width, height, inset, connectorPitch, cells;
    }

    // The panel owns only preview construction; the shared fitter owns frame internals.
    void BuildPanel(NativePanel p, Transform parent, Dictionary<string, Sprite> sprites)
    {
        var root = TopRect(p.name + " panel", parent, p.x, p.y, p.width, p.height);
        var mask = TopRect("InteriorMask", root, 0, 0, 0, 0);
        mask.anchorMin = Vector2.zero; mask.anchorMax = Vector2.one;
        mask.offsetMin = new Vector2(p.inset, p.inset); mask.offsetMax = -mask.offsetMin;
        mask.gameObject.AddComponent<RectMask2D>();
        if (!string.IsNullOrEmpty(p.backing))
        {
            var backing = sprites[p.backing];
            var fill = TopRect("OverfilledBacking", mask, 0, 0, 0, 0);
            fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one;
            fill.offsetMin = -backing.rect.size; fill.offsetMax = backing.rect.size;
            var image = fill.gameObject.AddComponent<Image>(); image.sprite = backing;
            image.type = Image.Type.Tiled; image.raycastTarget = false; image.pixelsPerUnitMultiplier = 1;
        }
        if (p.cells > 0)
        {
            var tile = sprites[p.prefix + "_Cell"];
            if (p.width - p.inset * 2 != tile.rect.width * p.cells ||
                p.height - p.inset * 2 != tile.rect.height * p.cells)
                throw new InvalidOperationException("Board pitch must exactly fill the native interior.");
            for (int y = 0; y < p.cells; y++) for (int x = 0; x < p.cells; x++)
            {
                var cell = TopRect("Cell_" + x + "_" + y, mask, x * tile.rect.width, y * tile.rect.height, tile.rect.width, tile.rect.height);
                var image = cell.gameObject.AddComponent<Image>(); image.sprite = tile; image.raycastTarget = false;
            }
        }
        var frame = TopRect("ModularFrame", root, 0, 0, 0, 0);
        frame.anchorMin = Vector2.zero; frame.anchorMax = Vector2.one; frame.offsetMin = frame.offsetMax = Vector2.zero;
        string[] children = { "TopLeftCorner", "TopRightCorner", "BottomLeftCorner", "BottomRightCorner", "TopEdge", "BottomEdge", "LeftEdge", "RightEdge" };
        string[] suffixes = { "Corner_TL", "Corner_TR", "Corner_BL", "Corner_BR", "Rail_Top", "Rail_Bottom", "Rail_Left", "Rail_Right" };
        for (int n = 0; n < children.Length; n++)
        {
            var r = TopRect(children[n], frame, 0, 0, 0, 0);
            var image = r.gameObject.AddComponent<Image>(); image.sprite = sprites[p.prefix + "_" + suffixes[n]];
            image.raycastTarget = false;
        }
        frame.gameObject.AddComponent<ResponsiveModularFrameFitter>().ConfigureNativePieces(sprites[p.prefix + "_Clamp"], p.connectorPitch);
    }

    static RectTransform TopRect(string name, Transform parent, float x, float y, float w, float h)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h);
        return r;
    }

    [ContextMenu("Rebuild reference art preview")]
    public void Rebuild()
    {
        if (!previewCamera || !previewCanvas || !placementManifest || modules == null)
            throw new InvalidOperationException("Preview camera, canvas, source sprites and manifest are required.");
        var manifest = JsonUtility.FromJson<Manifest>(placementManifest.text);
        if (manifest.width != 360 || manifest.height != 640 || manifest.placements == null)
            throw new InvalidOperationException("Unexpected reference art manifest.");
        var lookup = modules.ToDictionary(s => s.name, s => s);
        PixelScale = Mathf.FloorToInt(Mathf.Min(safeArea.width / 360f, safeArea.height / 640f));
        if (PixelScale < 1) throw new InvalidOperationException("Preview safe area must fit the 360x640 native reference.");
        ContentPixels = new Rect(Mathf.Floor(safeArea.x + (safeArea.width - 360 * PixelScale) / 2),
            Mathf.Floor(safeArea.y + (safeArea.height - 640 * PixelScale) / 2), 360 * PixelScale, 640 * PixelScale);
        var old = previewCanvas.transform.Find("ReferenceContent");
        if (old) { old.gameObject.SetActive(false); if (Application.isPlaying) Destroy(old.gameObject); else DestroyImmediate(old.gameObject); }
        Pieces.Clear();
        previewCamera.orthographic = true; previewCamera.orthographicSize = height / 128f;
        previewCamera.aspect = (float)width / height; previewCamera.transform.position = new Vector3(0, 0, -10);
        previewCamera.clearFlags = CameraClearFlags.SolidColor; previewCamera.backgroundColor = new Color32(10, 13, 17, 255);
        previewCamera.allowHDR = false; previewCamera.allowMSAA = false;
        previewCanvas.renderMode = RenderMode.WorldSpace; previewCanvas.worldCamera = previewCamera;
        previewCanvas.referencePixelsPerUnit = 64; previewCanvas.pixelPerfect = true;
        var canvas = (RectTransform)previewCanvas.transform;
        canvas.pivot = Vector2.zero; canvas.sizeDelta = new Vector2(width, height);
        canvas.position = new Vector3(-width / 128f, -height / 128f, 0); canvas.localScale = Vector3.one / 64;
        Content = new GameObject("ReferenceContent", typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();
        Content.SetParent(canvas, false); Content.anchorMin = Content.anchorMax = Content.pivot = Vector2.zero;
        Content.anchoredPosition = ContentPixels.position; Content.sizeDelta = new Vector2(360, 640);
        Content.localScale = Vector3.one * PixelScale;
        string[] names = {"01 Base walls", "02 Standing platform", "03 Structural masonry", "04 Props", "05 Top UI shells", "06 Bottom HUD shells"};
        var layers = names.Select(n => TopRect(n, Content, 0, 0, 360, 640)).ToArray();
        if (manifest.panels != null) foreach (var panel in manifest.panels) BuildPanel(panel, layers[4], lookup);
        var clips = new Dictionary<string, RectTransform>();
        foreach (var p in manifest.placements.OrderBy(p => p.layer))
        {
            if (!lookup.TryGetValue(p.name, out var sprite)) throw new InvalidOperationException("Missing native asset " + p.name);
            int group = p.layer <= 2 ? 0 : p.layer <= 11 ? 1 : p.layer <= 14 ? 2 : p.layer <= 30 ? 3 : p.y < 530 ? 4 : 5;
            Transform parent = layers[group]; int x = p.x, y = p.y;
            if (p.clip != null && p.clip.Length == 4)
            {
                string key = group + ":" + p.layer + ":" + string.Join(",", p.clip);
                if (!clips.TryGetValue(key, out var clip))
                {
                    clip = TopRect(p.name + " clipped field", parent, p.clip[0], p.clip[1], p.clip[2], p.clip[3]);
                    clip.gameObject.AddComponent<RectMask2D>(); clips.Add(key, clip);
                }
                parent = clip; x -= p.clip[0]; y -= p.clip[1];
            }
            var rect = TopRect(p.name, parent, x, y, sprite.rect.width, sprite.rect.height);
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = sprite; image.type = Image.Type.Simple;
            image.raycastTarget = false; image.preserveAspect = false; Pieces.Add(rect);
        }
    }
}
