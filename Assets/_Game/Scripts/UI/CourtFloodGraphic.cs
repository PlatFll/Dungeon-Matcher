using UnityEngine;
using UnityEngine.UI;

/// <summary>Cosmetic screen water. Observes flooding; owns no board state or input.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class CourtFloodGraphic : MaskableGraphic
{
    private Texture2D atlas;
    private float amount, elapsed;
    private bool reducedMotion;
    public float FillAmount => amount;
    public float AnimationTime => elapsed;
    public override Texture mainTexture => atlas ? atlas : Texture2D.whiteTexture;
    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;
        atlas = Resources.Load<Texture2D>("UI/CourtFlood/WaterAtlas");
    }

    public void Present(bool flooded, float deltaTime, bool reduced)
    {
        reducedMotion = reduced;
        // The existing .35-second board transition is unchanged. This slower
        // cosmetic rise never holds a move, modifies AIR or delays resolution.
        amount = reduced ? (flooded ? 1 : 0) : Mathf.MoveTowards(amount, flooded ? 1 : 0, deltaTime / 1.8f);
        if (!reduced && amount > 0) elapsed += deltaTime;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (amount <= 0) return;
        Rect r = rectTransform.rect;
        float surface = r.yMin - 64 + (r.height + 128) * Mathf.SmoothStep(0, 1, amount);
        float join = surface - 42;
        Vector2 white = atlas ? new Vector2(1.5f / 512, 1 - 257.5f / 272) : Vector2.one * .5f;
        var baseTint = new Color(12 / 255f, 90 / 255f, 101 / 255f, 64 / 255f);
        if (join > r.yMin)
            Quad(mesh, new Rect(r.xMin, r.yMin, r.width, Mathf.Min(join, r.yMax) - r.yMin),
                new Rect(white, Vector2.zero), new Color(4 / 255f, 40 / 255f, 70 / 255f, .29f), baseTint);
        if (!atlas) return; // Safe quiet tint if optional effect artwork is missing.
        if (surface - 42 < r.yMax && surface + 22 > r.yMin)
        {
            int frame = reducedMotion ? 0 : Mathf.FloorToInt(elapsed * 12) % 16;
            int column = frame % 4, row = frame / 4;
            for (float x = r.xMin; x < r.xMax; x += 128)
            {
                float width = Mathf.Min(128, r.xMax - x);
                float bottom = Mathf.Max(r.yMin, surface - 42), top = Mathf.Min(r.yMax, surface + 22);
                float cropBottom = bottom - (surface - 42);
                Quad(mesh, new Rect(x, bottom, width, top - bottom),
                    new Rect(column * .25f, 1 - (row + 1) * 64 / 272f + cropBottom / 272f, width / 512f, (top - bottom) / 272f),
                    Color.white, Color.white);
            }
        }
        if (reducedMotion) return;
        // Bounded, deterministic cosmetic paths: never consume board RNG.
        for (int i = 0; i < 15; i++)
        {
            float cycle = Mathf.Repeat(elapsed * (6 + i % 4 * 2) + i * 43, r.height + 28);
            float y = r.yMin + cycle - 14;
            if (y > surface - 6 || y > r.yMax - 10) continue;
            float x = r.xMin + (i * .618034f % 1) * (r.width - 20) + 6 + Mathf.Round(Mathf.Sin(elapsed * .65f + i) * 3);
            bool large = i % 3 == 0;
            float size = large ? 8 : 4;
            float fade = Mathf.Clamp01((surface - y) / 18) * Mathf.Clamp01((cycle - 5) / 15);
            var tint = new Color(1, 1, 1, .20f * fade);
            var bubbleUV = large ? new Rect(8 / 512f, 1 - 266 / 272f, 8 / 512f, 8 / 272f)
                : new Rect(24 / 512f, 1 - 262 / 272f, 4 / 512f, 4 / 272f);
            Quad(mesh, new Rect(Mathf.Round(x), Mathf.Round(y), size, size), bubbleUV, tint, tint);
        }
    }

    private static void Quad(VertexHelper mesh, Rect r, Rect uv, Color bottom, Color top)
    {
        if (r.width <= 0 || r.height <= 0) return;
        int n = mesh.currentVertCount;
        mesh.AddVert(new Vector3(r.xMin, r.yMin), bottom, new Vector2(uv.xMin, uv.yMin));
        mesh.AddVert(new Vector3(r.xMin, r.yMax), top, new Vector2(uv.xMin, uv.yMax));
        mesh.AddVert(new Vector3(r.xMax, r.yMax), top, new Vector2(uv.xMax, uv.yMax));
        mesh.AddVert(new Vector3(r.xMax, r.yMin), bottom, new Vector2(uv.xMax, uv.yMin));
        mesh.AddTriangle(n, n + 1, n + 2); mesh.AddTriangle(n, n + 2, n + 3);
    }
}
