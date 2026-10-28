using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Art-only layout review. Reuse approved sprites and the measured production actor baseline.
public static class IronveinBelowTopHUDRevision04
{
    const string Root = "ArtSource/IronveinBelowTopHUD/Revision04";
    const string Source = "Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision03.unity";
    const string Target = "Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision04.unity";
    static Canvas canvas;
    static Camera camera;

    [MenuItem("Dungeon Matcher/Art/Ironvein Below Top HUD/Revision 04 centered floor alignment")]
    public static void Build()
    {
        Assert.That(Application.isPlaying, Is.False);
        Directory.CreateDirectory(Root + "/Preview"); Directory.CreateDirectory(Root + "/QA");
        EditorSceneManager.OpenScene(Source);
        canvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Single(c => c.isRootCanvas);
        camera = Object.FindFirstObjectByType<Camera>(); Canvas.ForceUpdateCanvases();
        int[] widths = {360, 720, 1080};
        var before = widths.Select(w => Capture(w, "Before")).ToArray();
        var masks = widths.Select(ProtectedMask).ToArray();
        var rects = canvas.GetComponentsInChildren<RectTransform>(true);
        var positions = rects.Select(r => r.anchoredPosition).ToArray();
        var shapes = rects.Select(Shape).ToArray();
        var graphics = canvas.GetComponentsInChildren<Graphic>(true);
        var bindings = graphics.Select(Binding).ToArray();
        var floors = canvas.transform.Find("Revision03_WalkingSurface").GetComponentsInChildren<Image>();
        var blocks = canvas.transform.Find("Revision02_CombatFloor").GetComponentsInChildren<Image>().Where(i => i.name.StartsWith("StoneBlock")).ToArray();
        Assert.That(floors.Length, Is.EqualTo(6)); Assert.That(blocks.Length, Is.EqualTo(9));
        var moving = floors.Concat(blocks).Select(i => i.rectTransform).ToArray();
        var actors = canvas.transform.Find("LockedActorsAndBattleUI").GetComponentsInChildren<Image>()
            .Where(i => i.name.Contains("EnemySpawnAnchor") && i.name.EndsWith("VisualRoot") && i.sprite).ToArray();
        Assert.That(actors.Length, Is.EqualTo(3));
        var feet = actors.Select(FeetY).ToArray();
        Assert.That(feet.Max() - feet.Min(), Is.LessThan(.01f), "Existing shared source-floor line");
        float standingNative = (1280 - feet.Average()) / 2;
        float floorTopBefore = NativeTop(floors[0].rectTransform);
        float floorBottomBefore = NativeBottom(floors[0].rectTransform);
        Assert.That(floorTopBefore, Is.EqualTo(174)); Assert.That(floorBottomBefore, Is.EqualTo(206));
        int shift = Mathf.RoundToInt(standingNative - (floorTopBefore + floorBottomBefore) * .5f);
        Assert.That(shift, Is.GreaterThan(0));
        // As in Court/Forest/Dungeon, align scenery to feet, never shift the shared actor/HP row.
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene, Target), Is.True);
        foreach (var r in moving) r.anchoredPosition += Vector2.down * shift;
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < rects.Length; i++)
        {
            Assert.That(Shape(rects[i]), Is.EqualTo(shapes[i]), rects[i].name + " shape/state changed");
            Assert.That(rects[i].anchoredPosition, Is.EqualTo(positions[i] + (moving.Contains(rects[i]) ? Vector2.down * shift : Vector2.zero)), rects[i].name + " unexpected movement");
        }
        CollectionAssert.AreEqual(bindings, graphics.Select(Binding).ToArray(), "Every original artwork, color and visibility remains exact");
        CollectionAssert.AreEqual(feet, actors.Select(FeetY).ToArray(), "Enemy floor line must remain shared");
        float top = NativeTop(floors[0].rectTransform), bottom = NativeBottom(floors[0].rectTransform);
        Assert.That(Mathf.Abs((top + bottom) * .5f - standingNative), Is.LessThanOrEqualTo(.5f), "Feet must be centered on the floor");
        Rect board = WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement"));
        var rim = canvas.transform.Find("Stage02_Separator_PreviewOnly").GetComponentsInChildren<Image>();
        foreach (var im in floors)
        {
            Assert.That(NativeTop(im.rectTransform), Is.EqualTo(top)); Assert.That(NativeBottom(im.rectTransform), Is.EqualTo(bottom));
            Assert.That(im.rectTransform.rect.height, Is.EqualTo(32));
            Assert.That(im.sprite.rect.size, Is.EqualTo(new Vector2(64, 32)));
        }
        foreach (var im in blocks)
        {
            Assert.That(NativeTop(im.rectTransform), Is.EqualTo(bottom), "Separator must meet the front of the walking floor");
            Assert.That(im.rectTransform.rect.size, Is.EqualTo(im.sprite.rect.size));
        }
        foreach (var im in floors.Concat(blocks))
        {
            Assert.That(im.transform.lossyScale, Is.EqualTo(new Vector3(2, 2, 1)));
            Assert.That(im.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point)); Assert.That(im.raycastTarget, Is.False);
            var b = WorldRect(im.rectTransform); Assert.That(b.Overlaps(board), Is.False);
            Assert.That(rim.Any(other => WorldRect(other.rectTransform).Overlaps(b)), Is.False, "Platform must clear the existing board separator/frame");
        }
        Assert.That(canvas.transform.Find("PortraitInterior_PlaceholderMask").GetComponent<RectMask2D>(), Is.Not.Null);
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None), Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == Target), Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene, Target), Is.True);
        var changed = new int[3]; var protectedPixels = new int[3];
        float lowest = blocks.Max(im => NativeBottom(im.rectTransform));
        for (int n = 0; n < widths.Length; n++)
        {
            int w = widths[n]; var after = Capture(w, "Unity");
            for (int i = 0; i < after.Length; i++)
            {
                if (masks[n][i].a == 255) { Assert.That(after[i], Is.EqualTo(before[n][i]), "Protected pixel changed " + w + " " + i); protectedPixels[n]++; }
                if (after[i].Equals(before[n][i])) continue;
                float nativeY = 640 - (i / w + .5f) * 360 / w;
                Assert.That(nativeY, Is.InRange(floorTopBefore, lowest), "Changed pixel outside floor/separator band"); changed[n]++;
            }
            Assert.That(changed[n], Is.GreaterThan(0)); Assert.That(protectedPixels[n], Is.GreaterThan(1000));
        }
        File.WriteAllText(Root + "/QA/Unity.json", JsonUtility.ToJson(new Report {status = "PASS", scene = Target, shiftedNativePixels = shift, floorTop = top, floorBottom = bottom, separatorBottom = lowest, enemyFloorNative = standingNative, enemyFeetWorld = feet, widths = widths, changedPixels = changed, protectedPixels = protectedPixels, movedRects = moving.Select(PathOf).ToArray(), originalArtUnchanged = true, actorAndUIUnchanged = true, note = "Actual static art-only Unity renders. Fifteen existing floor/separator placements translated by whole native pixels. Source sprites, actors, HP, board, portrait, other geometry and production Game unchanged."}, true));
        Debug.Log("IRONVEIN_REVISION04_PASS floor=" + top + ".." + bottom + " feet=" + standingNative + " separator=" + bottom + ".." + lowest);
    }
    static float FeetY(Image im) { var r = WorldRect(im.rectTransform); return r.yMin + im.sprite.pivot.y * r.height / im.sprite.rect.height; }
    static float NativeTop(RectTransform r) => (1280 - WorldRect(r).yMax) / 2;
    static float NativeBottom(RectTransform r) => (1280 - WorldRect(r).yMin) / 2;
    static string Shape(RectTransform r) => r.sizeDelta + "|" + r.anchorMin + "|" + r.anchorMax + "|" + r.pivot + "|" + r.localScale + "|" + r.localRotation + "|" + r.gameObject.activeSelf;
    static string Binding(Graphic g) => PathOf(g.transform) + "|" + g.color + "|" + g.enabled + "|" + (g is Image im ? AssetDatabase.GetAssetPath(im.sprite) + ":" + (im.sprite ? im.sprite.name : "") : "text");
    static string PathOf(Transform t) { string p = t.name; while (t.parent && t.parent != canvas.transform) { t = t.parent; p = t.name + "/" + p; } return p; }
    static Rect WorldRect(RectTransform r) { var c = new Vector3[4]; r.GetWorldCorners(c); return Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y); }
    static Color32[] ProtectedMask(int w)
    {
        var gs = canvas.GetComponentsInChildren<Graphic>(true); var enabled = gs.Select(g => g.enabled).ToArray(); var flags = camera.clearFlags; var color = camera.backgroundColor;
        try
        {
            foreach (var g in gs) { var p = PathOf(g.transform); g.enabled = g.enabled && (p.StartsWith("IronveinNativeFrames/") || p.StartsWith("Stage04_Drills_PreviewOnly/") || p.StartsWith("Stage02_Separator_PreviewOnly/") || p.StartsWith("UnchangedSharedUIShells_StaticReviewOnly/") || p.StartsWith("LockedActorsAndBattleUI/") || p.StartsWith("LockedBoardCellsAndGems/")); }
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.clear; return Capture(w, "ProtectedMask");
        }
        finally { for (int i = 0; i < gs.Length; i++) gs[i].enabled = enabled[i]; camera.clearFlags = flags; camera.backgroundColor = color; Canvas.ForceUpdateCanvases(); }
    }
    static Color32[] Capture(int w, string prefix)
    {
        int h = w * 1280 / 720; Canvas.ForceUpdateCanvases(); var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32); rt.filterMode = FilterMode.Point; rt.antiAliasing = 1;
        var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt; var png = new Texture2D(w, h, TextureFormat.RGBA32, false); png.ReadPixels(new Rect(0, 0, w, h), 0, 0); png.Apply();
        File.WriteAllBytes(Root + "/Preview/" + prefix + "-" + w + "x" + h + ".png", png.EncodeToPNG()); var pixels = png.GetPixels32(); RenderTexture.active = prior; camera.targetTexture = null; RenderTexture.ReleaseTemporary(rt); Object.DestroyImmediate(png); return pixels;
    }
    [Serializable] class Report { public string status, scene, note; public int shiftedNativePixels; public float floorTop, floorBottom, separatorBottom, enemyFloorNative; public float[] enemyFeetWorld; public int[] widths, changedPixels, protectedPixels; public string[] movedRects; public bool originalArtUnchanged, actorAndUIUnchanged; }
}
