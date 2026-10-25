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

/// <summary>Forest final art-only assembly. Reads approved assets; never writes production scenes or bindings.</summary>
public sealed class ForestHybridStage05Tests
{
    const string Root = "ArtSource/ForestHybrid/Stage05";
    const string Art = "Assets/_Game/Art/ForestHybrid/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage05.unity";
    const int Width = 720, Height = 1360;
    [Serializable] public class Use { public string path, name, type; public Rect nativeRect, screenRect; public float scale; }
    [Serializable] public class Evidence { public int width, height; public Use[] images; }
    static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h, int scale = 1)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); r.localScale = Vector3.one * scale; return r;
    }
    static Sprite Load(string p) { var s = AssetDatabase.LoadAssetAtPath<Sprite>(p); Assert.That(s, Is.Not.Null, p); return s; }
    static Image Img(string name, Transform parent, Sprite sprite, int x, int y, int w = 0, int h = 0)
    {
        var r = Rect(name, parent, x, y, w > 0 ? w : sprite.rect.width, h > 0 ? h : sprite.rect.height);
        var i = r.gameObject.AddComponent<Image>(); i.sprite = sprite; i.raycastTarget = false;
        if (w > 0 || h > 0) i.type = Image.Type.Tiled; return i;
    }
    static Image Module(string stage, string name, Transform parent, int x, int y, int w = 0, int h = 0) =>
        Img(name, parent, Load(Art + stage + "/" + name + ".png"), x, y, w, h);
    static RectTransform Copy(Transform source, Transform parent, string name)
    {
        var r = Object.Instantiate(source.gameObject, parent, false).GetComponent<RectTransform>(); r.name = name; return r;
    }
    static void Position(Transform child, int x, int y) { ((RectTransform)child).anchoredPosition = new Vector2(x, -y); }
    static void Label(Transform parent, RectTransform grid, string text, int x, int y, int w, int h, int size)
    {
        var box = Rect("Static text " + text, parent, x, y, w, h);
        var t = GameUi.Label("Label", box, text, new Vector2(w, h), Vector2.zero, size);
        t.alignment = TextAlignmentOptions.CenterGeoAligned; t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Truncate; PixelTextFitter.ApplyNativeMenu(t, size);
        t.gameObject.AddComponent<DungeonPreviewPixelText>().Initialize(grid);
    }
    static void Health(Transform parent, RectTransform grid, string kind, string value, int x, int y, int width)
    {
        var s = Resources.Load<ModularHealthBarStyle>("UI/Finalized/Health/" + kind); Assert.That(s, Is.Not.Null);
        var r = Rect(kind + " static HP", parent, x, y, width, 24);
        Img("Track", r, s.EmptyTrack, (int)s.FillInsetLeft, 8, width - (int)s.FillInsetLeft - (int)s.FillInsetRight, 8);
        Img("Fill", r, s.FillStrip, (int)s.FillInsetLeft, 8, width - (int)s.FillInsetLeft - (int)s.FillInsetRight, 8);
        Img("Start", r, s.StartPiece, 14, 0); Img("Middle", r, s.MiddlePiece, 30, 0, width - 46, 24);
        Img("End", r, s.EndPiece, width - 16, 0); Img("Badge", r, s.Badge, 0, 0);
        Label(r, grid, value, 27, 3, width - 40, 18, 16);
    }
    static Canvas SourceCanvas(Scene scene) => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>()).Single();
    static void Portrait(RectTransform top, Scene destination)
    {
        var source = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage02.unity", OpenSceneMode.Additive);
        SceneManager.SetActiveScene(destination);
        var panel = Copy(SourceCanvas(source).transform.Find("ForestPortrait"), top, "ForestPortrait");
        panel.anchoredPosition = new Vector2(8, -8); panel.sizeDelta = new Vector2(98, 220);
        var contents = panel.Find("InteriorMask/PortraitContents");
        ((RectTransform)contents).sizeDelta = panel.sizeDelta;
        Position(contents.Find("Antler pennant candidate"), 35, 12);
        Position(contents.Find("Existing affinity gem"), 33, 66);
        Position(contents.Find("Rattlebones unchanged"), 17, 110);
        Object.DestroyImmediate(contents.Find("Existing player health presentation").gameObject);
        Position(panel.Find("Lower ivy accent"), 85, 192);
        EditorSceneManager.CloseScene(source, true);
    }
    static void Board(RectTransform root, Scene destination)
    {
        var source = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage03.unity", OpenSceneMode.Additive);
        SceneManager.SetActiveScene(destination); var sourceRoot = SourceCanvas(source).transform;
        var board = Rect("Board contents native 64px", root, 104, 528, 512, 512);
        var cells = Copy(sourceRoot.Find("Existing native cells"), board, "Existing native cells"); cells.anchoredPosition = Vector2.zero;
        var gems = Copy(sourceRoot.Find("Existing native gems"), board, "Existing native gems"); gems.anchoredPosition = Vector2.zero;
        var shell = Rect("Board shell 2x native modules", root, 0, 0, 360, Height / 2, 2);
        foreach (int x in new[] { 18, 319 })
        {
            Module("Stage03", "Side_PostRepeat", shell, x, 252, 23, 270);
            Module("Stage03", "Side_PostCap", shell, x - 2, 236);
            Module("Stage03", "Side_PostFoot", shell, x - 4, 510);
        }
        var target = Copy(sourceRoot.Find("FrameAssembly/BoardFrameTarget"), shell, "BoardFrameTarget");
        target.anchoredPosition = new Vector2(28, -240); target.sizeDelta = new Vector2(304, 304);
        foreach (int y in new[] { 326, 460 })
        { Module("Stage03", "Side_Brace", shell, 26, y); Module("Stage03", "Side_Brace", shell, 311, y); }
        Module("Stage03", "Board_Ivy_Upper", shell, 15, 263);
        Module("Stage03", "Board_Ivy_Lower", shell, 326, 458);
        Module("Stage03", "Board_Ivy_Upper", shell, 105, 227);
        Module("Stage03", "Board_Ivy_Lower", shell, 240, 535);
        var props = Rect("Board props 2x", root, 0, 0, 360, Height / 2, 2);
        Module("Stage04", "Lantern_Hanging", props, 8, 286);
        Module("Stage04", "Lantern_Hanging", props, 316, 407);
        Module("Stage04", "Rune_Plaque", props, 17, 420);
        Module("Stage04", "Mushroom_Cluster", props, 7, 513);
        Module("Stage04", "Mushroom_Cluster", props, 317, 513);
        EditorSceneManager.CloseScene(source, true);
    }
    static void SharedHud(RectTransform root, RectTransform top, Scene destination)
    {
        // Reuse the approved Dungeon assembly's shared controls and lower HUD, with all native artwork untouched.
        var source = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage05.unity", OpenSceneMode.Additive);
        SceneManager.SetActiveScene(destination); var sourceRoot = SourceCanvas(source).transform;
        var sourceTop = sourceRoot.Find("Approved top assembly - 2x");
        foreach (string name in new[] { "Wave candidate", "Guide candidate", "Settings candidate" })
            Copy(sourceTop.Find(name), top, "Shared " + name);
        var bottom = Copy(sourceRoot.Find("Approved bottom HUD - 2x"), root, "Shared bottom HUD");
        bottom.anchoredPosition = new Vector2(100, -1104);
        foreach (var t in bottom.GetComponentsInChildren<DungeonPreviewPixelText>(true)) t.Initialize(bottom);
        foreach (var t in top.GetComponentsInChildren<DungeonPreviewPixelText>(true)) t.Initialize(top);
        EditorSceneManager.CloseScene(source, true);
    }
    [MenuItem("Dungeon Matcher/Art Preview/Forest Hybrid Stage 05 full assembly")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Forest full assembly camera").AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = Height / 128f; camera.aspect = (float)Width / Height;
        camera.transform.position = new Vector3(0, 0, -10); camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.magenta; camera.allowHDR = camera.allowMSAA = false;
        var canvas = new GameObject("Forest Stage 05 art-only assembly", typeof(RectTransform)).AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = camera; canvas.pixelPerfect = true;
        var scaler = canvas.gameObject.AddComponent<CanvasScaler>(); scaler.referencePixelsPerUnit = 64; scaler.dynamicPixelsPerUnit = 1;
        var root = (RectTransform)canvas.transform; root.sizeDelta = new Vector2(Width, Height); root.pivot = Vector2.zero;
        root.position = new Vector3(-Width / 128f, -Height / 128f, 0); root.localScale = Vector3.one / 64;
        var backing = Rect("Continuous quiet timber", root, 0, 0, Width / 2, Height / 2, 2);
        Module("Stage02", "Portrait_Inset", backing, 0, 0, Width / 2, Height / 2);
        var top = Rect("Approved forest top modules 2x", root, 0, 0, 360, 240, 2);
        top.gameObject.AddComponent<RectMask2D>();
        var layout = JsonUtility.FromJson<ForestHybridStage01Tests.Layout>(File.ReadAllText("ArtSource/ForestHybrid/Stage01/Placement.json"));
        foreach (var p in layout.placements.OrderBy(p => p.layer))
        {
            var sprite = Load(p.path);
            // The repeat surface ends at the viewport, while every independent sprite keeps its complete canvas.
            bool edgeRepeat = p.name.Contains("Repeat") && p.x + sprite.rect.width > 360;
            Img(p.name, top, sprite, p.x, p.y, edgeRepeat ? 360 - p.x : 0, edgeRepeat ? (int)sprite.rect.height : 0);
        }
        // The left bracket is hidden behind the portrait. Lamps stay clear of the shared control row and actors.
        Module("Stage04", "Lantern_Hanging", top, 110, 52);
        Module("Stage04", "Lantern_Hanging", top, 323, 57);
        Module("Stage04", "Rune_Plaque", top, 331, 129);
        var cast = Rect("Existing Forest roster samples 2x", top, 0, 0, 360, 240);
        string[] names = { "Elven_Scout", "Orc_Trailguard", "Elven_Mender" };
        int[] positions = { 102, 175, 248 };
        for (int n = 0; n < names.Length; n++)
        {
            var s = Load("Assets/_Game/Art/Forest/" + names[n] + ".png");
            Assert.That(s.rect.size, Is.EqualTo(new Vector2(112, 88)));
            Img(names[n] + " unchanged", cast, s, positions[n], 126);
        }
        Portrait(top, scene);
        var stats = Rect("Static readability samples", root, 0, 0, Width, 480);
        Health(stats, root, "Player", "100 / 100", 50, 394, 138);
        Health(stats, root, "Normal", "50 / 50", 256, 426, 124);
        Health(stats, root, "Normal", "70 / 70", 402, 426, 124);
        Health(stats, root, "Normal", "60 / 60", 548, 426, 124);
        foreach (int x in new[] { 294, 440, 586 })
        {
            Img("Existing attack countdown icon", stats, Load("Assets/_Game/Resources/UI/CombatStatuses/Sword.png"), x, 244);
            Label(stats, root, "2", x + 20, 240, 24, 24, 16);
        }
        Board(root, scene); SharedHud(root, top, scene);
        foreach (var f in root.GetComponentsInChildren<ResponsiveModularFrameFitter>(true)) f.RefreshFrame();
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath), Is.False);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.Static | BindingFlags.NonPublic)
        .Invoke(null, new object[] { new Vector2Int(Width * scale, Height * scale) });
    static IEnumerator Screenshot(string name)
    {
        var p = Path.GetFullPath(Root + "/Previews/" + name + ".png"); if (File.Exists(p)) File.Delete(p);
        ScreenCapture.CaptureScreenshot(p); var until = Time.realtimeSinceStartup + 20;
        while (!File.Exists(p)) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(until)); yield return null; }
    }
    [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }
    [UnityTest, Explicit("Final Forest art-only assembly, exact whole-pixel scales and shared HUD preservation.")]
    public IEnumerator FullAssemblyNativeScreens()
    {
        Directory.CreateDirectory(Root + "/Previews"); Directory.CreateDirectory(Root + "/Diagnostics");
        Build(); SetSize(1); yield return new EnterPlayMode(); yield return null; yield return null;
        yield return CaptureAndCheck(); yield return new ExitPlayMode();
    }
    static IEnumerator CaptureAndCheck()
    {
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        foreach (int scale in new[] { 1, 2 })
        {
            SetSize(scale); var until = Time.realtimeSinceStartup + 20;
            while (Screen.width != Width * scale || Screen.height != Height * scale) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(until)); yield return null; }
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            var uses = new List<Use>();
            foreach (var im in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
            {
                Assert.That(im.color, Is.EqualTo(Color.white), im.name); Assert.That(im.raycastTarget, Is.False);
                Assert.That(im.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point), im.name);
                var r = GameplayPixelLayoutController.ScreenRect(im.rectTransform);
                float factor = im.transform.lossyScale.x / root.lossyScale.x * scale;
                Assert.That(factor, Is.EqualTo(Mathf.Round(factor)).Within(.001));
                Assert.That(im.transform.lossyScale.x, Is.EqualTo(im.transform.lossyScale.y));
                Assert.That(r.x, Is.EqualTo(Mathf.Round(r.x)).Within(.02), im.name);
                Assert.That(r.y, Is.EqualTo(Mathf.Round(r.y)).Within(.02), im.name);
                if (im.type == Image.Type.Simple)
                {
                    Assert.That(r.width, Is.EqualTo(im.sprite.rect.width * factor).Within(.02), im.name);
                    Assert.That(r.height, Is.EqualTo(im.sprite.rect.height * factor).Within(.02), im.name);
                    Assert.That(r.xMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                    Assert.That(r.yMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                    Assert.That(r.xMax, Is.LessThanOrEqualTo(Screen.width + .02), im.name);
                    Assert.That(r.yMax, Is.LessThanOrEqualTo(Screen.height + .02), im.name);
                }
                else Assert.That(im.pixelsPerUnit * im.pixelsPerUnitMultiplier, Is.EqualTo(1).Within(.001), im.name);
                uses.Add(new Use { path = AssetDatabase.GetAssetPath(im.sprite), name = im.name, type = im.type.ToString(), nativeRect = im.sprite.rect, screenRect = r, scale = factor });
            }
            foreach (var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) { t.ForceMeshUpdate(); Assert.That(t.isTextTruncated, Is.False, t.text); }
            foreach (var im in root.Find("Board props 2x").GetComponentsInChildren<Image>())
            {
                var r = GameplayPixelLayoutController.ScreenRect(im.rectTransform);
                Assert.That(r.Overlaps(new Rect(104 * scale, (Height - 1040) * scale, 512 * scale, 512 * scale)), Is.False, im.name);
            }
            string[] forbidden = { "BoardController", "EnemyActor", "PlayerActor", "RunSession", "EventSystem" };
            foreach (var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (c != null) Assert.That(Array.IndexOf(forbidden, c.GetType().Name), Is.EqualTo(-1), c.name);
            File.WriteAllText(Root + "/Diagnostics/Unity-" + Screen.width + "x" + Screen.height + ".json",
                JsonUtility.ToJson(new Evidence { width = Screen.width, height = Screen.height, images = uses.ToArray() }, true));
            yield return Screenshot("Unity-Full-" + Screen.width + "x" + Screen.height);
        }
        SetSize(1); yield return null; yield return null;
        var mask = root.Find("Approved forest top modules 2x/ForestPortrait/InteriorMask").GetComponent<RectTransform>();
        Assert.That(mask.rect.size, Is.EqualTo(new Vector2(74, 196)));
        var backing = mask.Find("OverfilledBacking").GetComponent<Image>();
        backing.color = Color.magenta; yield return null; yield return null;
        yield return Screenshot("Unity-PortraitMaskProbe-720x1360");
        backing.color = Color.white; yield return null; yield return null;
        root.Find("Board contents native 64px/Existing native gems").gameObject.SetActive(false);
        root.Find("Approved forest top modules 2x/Existing Forest roster samples 2x").gameObject.SetActive(false);
        root.Find("Approved forest top modules 2x/ForestPortrait/InteriorMask/PortraitContents/Rattlebones unchanged").gameObject.SetActive(false);
        root.Find("Static readability samples").gameObject.SetActive(false);
        Canvas.ForceUpdateCanvases(); yield return null; yield return null; yield return Screenshot("Unity-Structure-720x1360");
    }
}
