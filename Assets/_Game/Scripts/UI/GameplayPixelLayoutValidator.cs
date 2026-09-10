#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GameplayPixelLayoutValidator
{
    private const float Epsilon = 0.02f;
    public static List<string> Validate(GameplayPixelLayoutController owner, out string report)
    {
        var errors = new List<string>();
        var g = owner.Current;
        Rect top = GameplayPixelLayoutController.ScreenRect(owner.TopHud);
        Rect board = GameplayPixelLayoutController.ScreenRect(owner.BoardArea);
        Rect bottom = GameplayPixelLayoutController.ScreenRect(owner.BottomHud);
        Canvas canvas = owner.GetComponentInParent<Canvas>();
        BoardLayoutController worldBoard = Object.FindFirstObjectByType<BoardLayoutController>();
        report = $"Screen {Screen.width}x{Screen.height}; safe={Screen.safeArea}; constraints={g.Safe}; " +
            $"logical viewport={g.Viewport.size / g.Scale}; physical viewport={g.Viewport}; " +
            $"canvas scale={canvas.scaleFactor}; logical/physical={g.Scale}; " +
            $"Top={top}; BoardArea={board}; Bottom={bottom}; " +
            $"gaps={top.yMin - board.yMax},{board.yMin - bottom.yMax}; " +
            $"bottom display clearance={bottom.yMin}; safe clearance={bottom.yMin - g.Safe.yMin}; " +
            $"board texel ratio={worldBoard?.PhysicalTexelRatio}; origin={worldBoard?.PhysicalOrigin}; " +
            $"fractional board scale={g.FractionalBoardScale}\n";
        Require(g.Fits, "No feasible layout", errors);
        Require(Near(bottom.yMin, g.Viewport.yMin), "Bottom is not anchored to safe viewport", errors);
        Require(Contains(g.Viewport, bottom), $"Bottom leaves gameplay viewport: {bottom} vs {g.Viewport}", errors);
        Require(bottom.yMin - g.Safe.yMin >= GameplayPixelLayoutController.Inset * g.Scale - Epsilon,
            "Bottom enters additional safe inset", errors);
        Require(Contains(g.Viewport, top) && Contains(g.Viewport, board), "Top/board leaves gameplay viewport", errors);
        Require(!top.Overlaps(board) && !board.Overlaps(bottom) && !top.Overlaps(bottom), "Sections overlap", errors);
        Require(Near(top.yMin - board.yMax, GameplayPixelLayoutController.Gap * g.Scale) &&
            Near(board.yMin - bottom.yMax, GameplayPixelLayoutController.Gap * g.Scale), "Incorrect section gaps", errors);
        Require(Near(canvas.scaleFactor, g.Scale) && Integral(canvas.scaleFactor), "Fractional Canvas scale", errors);
        if (worldBoard == null) errors.Add("Missing board layout consumer");
        else
        {
            Require(worldBoard.PhysicalTexelRatio > 0 && Near(worldBoard.PhysicalTexelRatio, g.BoardTexelRatio),
                "Board scale differs from assigned fit", errors);
            Require(Integral(worldBoard.PhysicalOrigin.x) && Integral(worldBoard.PhysicalOrigin.y), "Off-grid board origin", errors);
            Transform border = worldBoard.transform.Find("BoardFrame");
            Require(border != null, "Missing world board frame", errors);
            if (border != null)
            foreach (SpriteRenderer piece in border.GetComponentsInChildren<SpriteRenderer>())
            {
                if (piece.sprite == null) { errors.Add("Missing world frame sprite: " + piece.name); continue; }
                Vector3 lo = Camera.main.WorldToScreenPoint(piece.bounds.min);
                Vector3 hi = Camera.main.WorldToScreenPoint(piece.bounds.max);
                Vector2 pixels = new Vector2(Mathf.Abs(hi.x - lo.x), Mathf.Abs(hi.y - lo.y));
                Vector2 source = piece.sprite.rect.size;
                if (piece.name.StartsWith("LeftEdge") || piece.name.StartsWith("RightEdge")) source = new Vector2(source.y, source.x);
                report += $"World frame {piece.name}: min={lo}, max={hi}, source={source}, ratios={pixels / source}\n";
                Require(Near(pixels.x / source.x, worldBoard.PhysicalTexelRatio) && Near(pixels.y / source.y, worldBoard.PhysicalTexelRatio),
                    "World frame texel mismatch: " + piece.name, errors);
                Require(g.FractionalBoardScale || (Integral(lo.x) && Integral(lo.y) && Integral(hi.x) && Integral(hi.y)),
                    "Fractional world frame vertices: " + piece.name, errors);
            }
        }
        foreach (RectTransform root in new[] { (RectTransform)owner.transform, owner.TopHud, owner.BoardArea, owner.BottomHud })
        {
            Require(root.GetComponents<GameplayPixelLayoutController>().Length <= 1, "Duplicate major layout owner: " + root.name, errors);
            Require(!root.TryGetComponent(out ContentSizeFitter fitter) || !fitter.enabled, "Competing ContentSizeFitter: " + root.name, errors);
            Require(!root.TryGetComponent(out LayoutGroup group) || !group.enabled, "Competing LayoutGroup: " + root.name, errors);
            Require(!root.TryGetComponent(out SafeAreaFitter safe) || !safe.enabled, "Competing safe-area writer: " + root.name, errors);
        }
        foreach (RectTransform rect in owner.GetComponentsInChildren<RectTransform>(false))
        {
            // Transient particles and the separate shield presentation retain their own VFX policy.
            if (IsTransientPresentation(rect)) continue;
            Vector3 scale = rect.localScale;
            Require(Near(Mathf.Abs(scale.x), 1) && Near(Mathf.Abs(scale.y), 1) && Near(Mathf.Abs(scale.z), 1),
                $"Non-unit pixel UI transform {Path(rect)}: {scale}", errors);
        }
        foreach (ResponsiveModularFrameFitter frame in owner.GetComponentsInChildren<ResponsiveModularFrameFitter>(false))
        {
            string[] names = { "TopLeftCorner", "TopRightCorner", "BottomLeftCorner", "BottomRightCorner", "TopEdge", "BottomEdge", "LeftEdge", "RightEdge" };
            foreach (string name in names)
            {
                var piece = frame.transform.Find(name) as RectTransform;
                if (piece == null || !piece.TryGetComponent(out Image image) || image.sprite == null)
                { errors.Add($"Missing frame piece {Path(frame.transform)}/{name}"); continue; }
                Rect bounds = GameplayPixelLayoutController.ScreenRect(piece);
                report += $"{Path(piece)} physical={bounds}; source={image.sprite.rect.size}; tilePPU={image.pixelsPerUnitMultiplier}\n";
                Require(Integral(bounds.xMin) && Integral(bounds.yMin) && Integral(bounds.xMax) && Integral(bounds.yMax),
                    $"Fractional frame vertices {Path(piece)}: {bounds}", errors);
                bool corner = name.Contains("Corner");
                float thickness = corner ? bounds.width / image.sprite.rect.width * 16 : Mathf.Min(bounds.width, bounds.height);
                Require(Near(thickness, 16 * g.Scale), $"Mismatched native frame thickness {Path(piece)}: {thickness}, expected {16 * g.Scale}", errors);
                Require(corner || Near(image.pixelsPerUnitMultiplier, 1), $"Fractional tile density {Path(piece)}", errors);
            }
        }
        Require(owner.BottomHud.Find("GeneratedBottomHudFrame") != null, "Missing generated bottom frame", errors);
        Require(owner.TopHud.Find("GeneratedTopBattleLayout/BattleArenaFrame") != null, "Missing generated battle frame", errors);
        foreach (Image image in owner.GetComponentsInChildren<Image>(false))
        {
            if (!image.enabled || image.sprite == null || image.type != Image.Type.Simple ||
                IsTransientPresentation(image.transform) || image.name.Contains("Base") ||
                image.sprite.name == "Knob" || image.sprite.name == "UISprite" || image.sprite.name == "Background") continue;
            Rect bounds = GameplayPixelLayoutController.ScreenRect(image.rectTransform);
            Vector2 source = image.sprite.rect.size;
            float xRatio = bounds.width / source.x, yRatio = bounds.height / source.y;
            report += $"UI art {Path(image.transform)} bounds={bounds}, source={source}, ratios=({xRatio},{yRatio})\n";
            Require(Integral(bounds.xMin) && Integral(bounds.yMin) && Integral(bounds.xMax) && Integral(bounds.yMax),
                $"Fractional UI art vertices {Path(image.transform)}: {bounds}", errors);
            Require(Near(xRatio, yRatio) && Integral(xRatio) && xRatio >= 1,
                $"Fractional UI source texels {Path(image.transform)}: ({xRatio},{yRatio})", errors);
        }
        return errors;
    }
    public static bool Integral(float value) => Near(value, Mathf.Round(value));
    private static bool IsTransientPresentation(Transform target)
    {
        for (Transform node = target; node != null; node = node.parent)
            if (node.name.Contains("Particle") || node.name.Contains("Shield") || node.name == "AbilityVFXLayer" ||
                node.name == "SpawnCircleFlash" || node.name == "SpawnBeam") return true;
        return false;
    }
    private static bool Near(float a, float b) => Mathf.Abs(a - b) < Epsilon;
    private static bool Contains(Rect outer, Rect inner) => inner.xMin >= outer.xMin - Epsilon && inner.yMin >= outer.yMin - Epsilon && inner.xMax <= outer.xMax + Epsilon && inner.yMax <= outer.yMax + Epsilon;
    private static void Require(bool value, string message, List<string> errors) { if (!value) errors.Add(message); }
    private static string Path(Transform target) => target.parent == null ? target.name : Path(target.parent) + "/" + target.name;
}
#endif
