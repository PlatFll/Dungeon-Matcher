using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Explicit, disposable preflight fixture. No production bindings or gameplay owners.
public sealed class IronveinV2PreflightReview
{
    const string Root = "ArtSource/IronveinEnvironmentV2";
    const string PreviewScene = "Assets/_Game/Scenes/ArtPreview_IronveinV2.unity";
    static void Size(int w, int h) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(w, h) });
    static IEnumerator Until(Func<bool> ready)
    {
        float end = Time.realtimeSinceStartup + 60;
        while (!ready()) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(end)); yield return null; }
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale = 1; RunLaunchOptions.StartingZone = null;
        GameplayPixelLayoutController.ValidationSafeArea = null;
        if (Application.isPlaying) yield return new ExitPlayMode();
    }
    [UnityTest, Explicit] public IEnumerator CaptureBaselinesAndBuildScaffold()
    {
        Directory.CreateDirectory(Root + "/Preview"); Directory.CreateDirectory(Root + "/QA");
        Size(720, 1280); EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        var cases = new List<Measurement>();
        using (CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        foreach (string zone in new[] { "ironvein-excavation", "dungeon", "magical-forest" })
        {
            string profile = Path.GetFullPath(".utmp/CourtV2/" + Guid.NewGuid() + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(profile));
            File.WriteAllText(profile, JsonUtility.ToJson(new AccountSave { potions = 3, bombs = 3, equipPotions = true, equipBombs = true }));
            using (AccountProgression.UseDisposableProfile(profile))
            {
                RunLaunchOptions.StartingZone = zone; UnityEngine.Random.InitState(3101);
                SceneManager.LoadScene("Game"); yield return null;
                yield return Until(() => RunSession.Current?.Continuation?.CanCapture == true && RunSession.Current.Waves.IsWaveActive);
                var waves = RunSession.Current.Waves;
                while (waves.ActiveEnemies.Count < 3) Assert.That(waves.TrySummonEnemy(waves.ActiveEnemies[0].Definition, out _), Is.True);
                yield return Until(() => RunSession.Current.Continuation.CanCapture && waves.ActiveEnemies.All(e => e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning != true));
                foreach (int height in new[] { 1280, 1600 })
                {
                    Size(720, height); yield return Until(() => UnityEngine.Device.Screen.width == 720 && UnityEngine.Device.Screen.height == height);
                    yield return new WaitForSecondsRealtime(1.5f); Canvas.ForceUpdateCanvases();
                    var layout = Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                    var errors = GameplayPixelLayoutValidator.Validate(layout, out string report);
                    var board = RunSession.Current.Board;
                    var lo = Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth / 2, -board.LocalBoardHeight / 2)));
                    var hi = Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth / 2, board.LocalBoardHeight / 2)));
                    var portrait = (RectTransform)layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection");
                    var floor = (RectTransform)layout.TopHud.Find("BattleFloorAnchor");
                    var c = new Measurement { zone = zone, height = height, board = Rect.MinMaxRect(lo.x, lo.y, hi.x, hi.y),
                        portrait = GameplayPixelLayoutController.ScreenRect(portrait), top = GameplayPixelLayoutController.ScreenRect(layout.TopHud),
                        bottom = GameplayPixelLayoutController.ScreenRect(layout.BottomHud), floorY = floor ? GameplayPixelLayoutController.ScreenRect(floor).y : -1,
                        errors = errors.ToArray(), legacyScenery = GameObject.Find("ReferenceKitScenery") != null,
                        masks = layout.TopHud.GetComponentsInChildren<RectMask2D>(true).Select(m => PathOf(m.transform)).ToArray() };
                    cases.Add(c);
                    string name = zone + "-720x" + height;
                    File.WriteAllText(Root + "/QA/" + name + ".txt", report + "\n" + string.Join("\n", errors));
                    File.WriteAllText(Root + "/QA/Geometry.json", JsonUtility.ToJson(new Measurements { cases = cases.ToArray() }, true));
                    if (zone == "ironvein-excavation" && height == 1280) SaveShellSnapshot(layout, c);
                    string png = Path.GetFullPath(Root + "/Preview/" + name + ".png");
                    if (File.Exists(png)) File.Delete(png);
                    ScreenCapture.CaptureScreenshot(png); yield return Until(() => File.Exists(png)); yield return null;
                }
            }
        }
        foreach (var group in cases.GroupBy(c => c.height))
        {
            var first = group.First();
            foreach (var c in group)
            {
                Assert.That(c.board.size.x, Is.EqualTo(first.board.size.x).Within(.1f), c.zone + " board width");
                Assert.That(c.board.size.y, Is.EqualTo(first.board.size.y).Within(.1f), c.zone + " board height");
                Assert.That(c.portrait.size.x, Is.EqualTo(first.portrait.size.x).Within(.1f), c.zone + " portrait width");
                Assert.That(c.portrait.size.y, Is.EqualTo(first.portrait.size.y).Within(.1f), c.zone + " portrait height");
            }
        }
        yield return new ExitPlayMode();
        BuildScaffold();
    }
    static string PathOf(Transform t) => t.parent ? PathOf(t.parent) + "/" + t.name : t.name;
    // The live Court canvas is 1 unit per screen pixel at this baseline.
    // Preserve it; a nominal 360px layout would shrink the existing UI artwork.
    static Rect Native(Rect r) => r;
    static void SaveShellSnapshot(GameplayPixelLayoutController layout, Measurement c)
    {
        var pieces = new List<GraphicSnapshot>();
        var roots = new List<Transform> { layout.BottomHud };
        foreach (string name in new[] { "WaveTracker", "CombatGuide", "Settings" })
        {
            var go = GameObject.Find(name); Assert.That(go, Is.Not.Null, name); roots.Add(go.transform);
        }
        foreach (Transform root in roots)
        foreach (Graphic g in root.GetComponentsInChildren<Graphic>())
        {
            string path = PathOf(g.transform);
            if (!g.enabled || g.color.a == 0 || path.Contains("ReferenceKit") || path.Contains("Backdrop") ||
                path.Contains("GeneratedBottomHudFrame") || path.Contains("ZoneResonance") || g is TMP_SubMeshUI) continue;
            var s = new GraphicSnapshot { name = path, rect = Native(GameplayPixelLayoutController.ScreenRect(g.rectTransform)), color = g.color };
            if (g is Image im)
            {
                s.kind = "image"; s.asset = im.sprite ? AssetDatabase.GetAssetPath(im.sprite) : "";
                s.sprite = im.sprite ? im.sprite.name : ""; s.imageType = (int)im.type; s.fill = im.fillAmount;
                s.fillMethod = (int)im.fillMethod; s.fillOrigin = im.fillOrigin; s.preserveAspect = im.preserveAspect;
                s.ppuMultiplier = im.pixelsPerUnitMultiplier / im.canvas.scaleFactor;
                if (im.sprite && string.IsNullOrEmpty(s.asset)) continue;
            }
            else if (g is TMP_Text text)
            {
                s.kind = "text"; s.text = text.text; s.asset = AssetDatabase.GetAssetPath(text.font);
                s.fontSize = text.fontSize * s.rect.height / text.rectTransform.rect.height;
                s.alignment = (int)text.alignment; s.fontStyle = (int)text.fontStyle;
            }
            else continue;
            pieces.Add(s);
        }
        File.WriteAllText(Root + "/QA/SharedShellSnapshot.json", JsonUtility.ToJson(new ShellSnapshot { geometry = c, pieces = pieces.ToArray() }, true));
    }
    [MenuItem("Dungeon Matcher/Art/Ironvein V2/Build Stage 00 Scaffold")]
    public static void BuildScaffold()
    {
        Assert.That(Application.isPlaying, Is.False);
        var data = JsonUtility.FromJson<ShellSnapshot>(File.ReadAllText(Root + "/QA/SharedShellSnapshot.json"));
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Stage00PreviewCamera").AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = 640; camera.transform.position = new Vector3(360, 640, -10);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(8, 14, 21, 255);
        camera.allowHDR = false; camera.allowMSAA = false;
        var canvas = new GameObject("Stage00_LayoutOnly_NoProductionBindings", typeof(RectTransform)).AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = camera; canvas.referencePixelsPerUnit = 64;
        var rect = (RectTransform)canvas.transform; rect.sizeDelta = new Vector2(720, 1280); rect.pivot = Vector2.zero;
        // Deliberately plain staging blocks, not new Court artwork or textures.
        Block(canvas.transform, "BlankOceanExtent", new Rect(0, 0, 720, 1280), new Color32(14, 28, 38, 255));
        Block(canvas.transform, "BlankUpperEnvironment", Native(data.geometry.top), new Color32(24, 45, 54, 255));
        Block(canvas.transform, "BlankLowerSurroundings", Native(data.geometry.bottom), new Color32(23, 36, 43, 255));
        var portrait = MakeRect(canvas.transform, "PortraitInterior_PlaceholderMask", Native(data.geometry.portrait));
        portrait.gameObject.AddComponent<RectMask2D>();
        var fill = Block(portrait, "OverfilledBlankRecess", new Rect(-16, -16, portrait.rect.width + 32, portrait.rect.height + 32), new Color32(13, 22, 29, 255));
        Assert.That(fill.raycastTarget, Is.False);
        var grid = Native(data.geometry.board);
        Block(canvas.transform, "PlayableBoardBounds_NoTileReplacement", grid, new Color32(11, 18, 26, 255));
        for (int i = 0; i <= 8; i++)
        {
            Block(canvas.transform, "GridGuideVertical" + i, new Rect(Mathf.Round(grid.x + i * grid.width / 8), Mathf.Round(grid.y), 1, Mathf.Round(grid.height)), new Color32(45, 58, 65, 255));
            Block(canvas.transform, "GridGuideHorizontal" + i, new Rect(Mathf.Round(grid.x), Mathf.Round(grid.y + i * grid.height / 8), Mathf.Round(grid.width), 1), new Color32(45, 58, 65, 255));
        }
        if (data.geometry.floorY >= 0)
            Block(canvas.transform, "ExistingBattleFloorGuide", new Rect(Mathf.Round(Native(data.geometry.portrait).xMax), Mathf.Round(data.geometry.floorY), 720 - Mathf.Round(Native(data.geometry.portrait).xMax), 1), new Color32(78, 104, 110, 255));
        var shells = MakeRect(canvas.transform, "UnchangedSharedUIShells_StaticReviewOnly", new Rect(0, 0, 720, 1280));
        foreach (var s in data.pieces)
        {
            var r = MakeRect(shells, s.name.Replace('/', '_'), s.rect);
            if (s.kind == "image")
            {
                var im = r.gameObject.AddComponent<Image>(); im.color = s.color; im.raycastTarget = false;
                if (!string.IsNullOrEmpty(s.asset)) im.sprite = AssetDatabase.LoadAllAssetsAtPath(s.asset).OfType<Sprite>().FirstOrDefault(p => p.name == s.sprite);
                Assert.That(string.IsNullOrEmpty(s.asset) || im.sprite != null, Is.True, s.asset + ":" + s.sprite);
                im.type = (Image.Type)s.imageType; im.fillAmount = s.fill; im.fillMethod = (Image.FillMethod)s.fillMethod;
                im.fillOrigin = s.fillOrigin; im.preserveAspect = s.preserveAspect; im.pixelsPerUnitMultiplier = s.ppuMultiplier;
            }
            else
            {
                var text = r.gameObject.AddComponent<TextMeshProUGUI>(); text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(s.asset);
                text.text = s.text; text.fontSize = s.fontSize; text.color = s.color; text.raycastTarget = false;
                text.alignment = (TextAlignmentOptions)s.alignment; text.fontStyle = (FontStyles)s.fontStyle;
                text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Overflow;
            }
        }
        Canvas.ForceUpdateCanvases(); EditorSceneManager.MarkSceneDirty(scene);
        Assert.That(EditorSceneManager.SaveScene(scene, PreviewScene), Is.True);
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == PreviewScene), Is.False, "Art-only scene must stay outside build settings");
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None), Is.Empty);
        Render(camera, 720, 1280, "Scaffold-Native.png"); Render(camera, 1440, 2560, "Scaffold-2x.png");
        File.WriteAllText(Root + "/QA/Scaffold.json", JsonUtility.ToJson(new ScaffoldCheck { nativeWidth = 720, nativeHeight = 1280, shellGraphics = data.pieces.Length,
            noActorsOrGameplay = true, excludedFromBuild = true, placeholderGridOnly = true }, true));
    }
    static RectTransform MakeRect(Transform parent, string name, Rect bounds)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = r.pivot = Vector2.zero; r.anchoredPosition = bounds.position; r.sizeDelta = bounds.size; return r;
    }
    static Image Block(Transform parent, string name, Rect bounds, Color color)
    { var image = MakeRect(parent, name, bounds).gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false; return image; }
    static void Render(Camera camera, int w, int h, string filename)
    {
        var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32); rt.filterMode = FilterMode.Point;
        var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
        var png = new Texture2D(w, h, TextureFormat.RGBA32, false); png.ReadPixels(new Rect(0, 0, w, h), 0, 0); png.Apply();
        File.WriteAllBytes(Root + "/Preview/" + filename, png.EncodeToPNG());
        RenderTexture.active = prior; camera.targetTexture = null; RenderTexture.ReleaseTemporary(rt); Object.DestroyImmediate(png);
    }
    [Serializable] public class Measurement { public string zone; public int height; public Rect board, portrait, top, bottom; public float floorY; public bool legacyScenery; public string[] errors, masks; }
    [Serializable] class Measurements { public Measurement[] cases; }
    [Serializable] class GraphicSnapshot { public string name, kind, asset, sprite, text; public Rect rect; public Color color; public int imageType, fillMethod, fillOrigin, alignment, fontStyle; public float fill, ppuMultiplier, fontSize; public bool preserveAspect; }
    [Serializable] class ShellSnapshot { public Measurement geometry; public GraphicSnapshot[] pieces; }
    [Serializable] class ScaffoldCheck { public int nativeWidth, nativeHeight, shellGraphics; public bool noActorsOrGameplay, excludedFromBuild, placeholderGridOnly; }
}
