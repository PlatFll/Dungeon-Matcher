using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Isolated Forest board-shell review. Existing cell/gem pixels and runtime owners are read-only.</summary>
public sealed class ForestHybridStage03Tests
{
    const string Root = "ArtSource/ForestHybrid/Stage03";
    const string Art = "Assets/_Game/Art/ForestHybrid/Stage03/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage03.unity";
    const int Size = 608, BoardStart = 48, BoardPixels = 512;
    static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
    }
    static Sprite Load(string path)
    { var s = AssetDatabase.LoadAssetAtPath<Sprite>(path); Assert.That(s, Is.Not.Null, path); return s; }
    static Image Img(string name, Transform parent, Sprite sprite, float x, float y)
    {
        var i = Rect(name, parent, x, y, sprite.rect.width, sprite.rect.height).gameObject.AddComponent<Image>();
        i.sprite = sprite; i.color = Color.white; i.raycastTarget = false; return i;
    }
    static Image Module(string name, Transform parent, float x, float y) => Img(name, parent, Load(Art + name + ".png"), x, y);
    static void Import()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (var p in Directory.GetFiles(Art, "*.png"))
        {
            var i = (TextureImporter)AssetImporter.GetAtPath(p.Replace('\\', '/'));
            i.textureType = TextureImporterType.Sprite; i.spriteImportMode = SpriteImportMode.Single;
            i.spritePixelsPerUnit = 64; i.filterMode = FilterMode.Point; i.mipmapEnabled = false;
            i.textureCompression = TextureImporterCompression.Uncompressed; i.npotScale = TextureImporterNPOTScale.None;
            i.alphaIsTransparency = false; i.maxTextureSize = 2048; i.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings(); i.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(.5f, .5f); i.SetTextureSettings(settings); i.SaveAndReimport();
        }
    }
    [MenuItem("Dungeon Matcher/Art Preview/Forest Hybrid Stage 03 board")]
    public static void Build()
    {
        Import(); var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Forest board native camera").AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = Size / 128f; camera.aspect = 1;
        camera.transform.position = new Vector3(0, 0, -10); camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(24, 35, 33, 255); camera.allowHDR = camera.allowMSAA = false;
        var canvas = new GameObject("Forest board shell study", typeof(RectTransform)).AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = camera; canvas.pixelPerfect = true;
        var scaler = canvas.gameObject.AddComponent<CanvasScaler>(); scaler.referencePixelsPerUnit = 64; scaler.dynamicPixelsPerUnit = 1;
        var root = (RectTransform)canvas.transform; root.sizeDelta = Vector2.one * Size; root.pivot = Vector2.zero;
        root.position = new Vector3(-Size / 128f, -Size / 128f, 0); root.localScale = Vector3.one / 64;
        var backing = Img("Approved quiet timber backing", root, Load("Assets/_Game/Art/ForestHybrid/Stage02/Portrait_Inset.png"), 0, 0);
        backing.type = Image.Type.Tiled; backing.rectTransform.sizeDelta = Vector2.one * Size;
        var cells = Rect("Existing native cells", root, BoardStart, BoardStart, BoardPixels, BoardPixels);
        var gems = Rect("Existing native gems", root, BoardStart, BoardStart, BoardPixels, BoardPixels);
        // Approved 64px Dungeon cell is the shared cleanliness benchmark; no Forest binding is changed.
        var tile = Load("Assets/_Game/Art/DungeonPixelLab/Stage16/Board_Cell64.png");
        Assert.That(tile.rect.size, Is.EqualTo(Vector2.one * 64));
        string[] guids = {"9a22612233ec77441871888a44fc5a8c", "ec4caeefdd13e504db84e48b96811872", "6632d83a6c5833842a416467a47c1ff5", "679c26fd95f197c4488846d5e8f8f3d3", "3b95815b85717ba4d8dca4b502263055", "736bd1feae8605d488aa88bcf27829ba"};
        var sprites = guids.Select(g => Load(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        foreach (var s in sprites) Assert.That(s.rect.size, Is.EqualTo(Vector2.one * 64));
        for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
        {
            Img("Cell " + x + "," + y, cells, tile, x * 64, y * 64);
            Img("Gem " + x + "," + y, gems, sprites[(x * 5 + y * 2 + y / 3) % 6], x * 64, y * 64);
        }
        var assembly = Rect("FrameAssembly", root, 0, 0, Size, Size);
        var supports = Rect("Separate side supports", assembly, 0, 0, Size, Size);
        foreach (int x in new[] {14, 571})
        {
            for (int y = 40; y < 552; y += 64) Module("Side_PostRepeat", supports, x, y);
            Module("Side_PostCap", supports, x - 2, 20);
            Module("Side_PostFoot", supports, x - 4, 548);
        }
        var target = Rect("BoardFrameTarget", assembly, 24, 24, 560, 560);
        var frame = Rect("ModularFrame", target, 0, 0, 560, 560);
        frame.anchorMin = Vector2.zero; frame.anchorMax = Vector2.one; frame.offsetMin = frame.offsetMax = Vector2.zero;
        string[] names = {"TopLeftCorner", "TopRightCorner", "BottomLeftCorner", "BottomRightCorner", "TopEdge", "BottomEdge", "LeftEdge", "RightEdge"};
        string[] files = {"Corner_TL", "Corner_TR", "Corner_BL", "Corner_BR", "Rail_Top", "Rail_Bottom", "Rail_Left", "Rail_Right"};
        for (int n = 0; n < 8; n++) Img(names[n], frame, Load(Art + "Board_" + files[n] + ".png"), 0, 0);
        frame.gameObject.AddComponent<ResponsiveModularFrameFitter>().ConfigureNativePieces(Load(Art + "Board_Connector.png"), 150);
        foreach (int y in new[] {164, 431}) {Module("Side_Brace", assembly, 22, y); Module("Side_Brace", assembly, 563, y);}
        Module("Board_Ivy_Upper", assembly, 12, 74); Module("Board_Ivy_Lower", assembly, 578, 459);
        Module("Board_Ivy_Upper", assembly, 136, 10); Module("Board_Ivy_Lower", assembly, 450, 570);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath), Is.False);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] {new Vector2Int(Size * scale, Size * scale)});
    static void CheckShell(ResponsiveModularFrameFitter f)
    {
        f.RefreshFrame(); var target = (RectTransform)f.transform.parent;
        foreach (var image in f.GetComponentsInChildren<Image>())
        {
            var r = image.rectTransform; Assert.That(image.enabled, Is.True);
            Assert.That(r.localScale, Is.EqualTo(Vector3.one));
            Assert.That(r.anchoredPosition.x, Is.EqualTo(Mathf.Round(r.anchoredPosition.x)).Within(.001));
            Assert.That(r.anchoredPosition.y, Is.EqualTo(Mathf.Round(r.anchoredPosition.y)).Within(.001));
            if (image.type == Image.Type.Simple) Assert.That(r.rect.size, Is.EqualTo(image.sprite.rect.size));
            else Assert.That(image.pixelsPerUnit * image.pixelsPerUnitMultiplier, Is.EqualTo(1).Within(.001));
        }
        var left = (RectTransform)f.transform.Find("LeftEdge"); var right = (RectTransform)f.transform.Find("RightEdge");
        var top = (RectTransform)f.transform.Find("TopEdge"); var bottom = (RectTransform)f.transform.Find("BottomEdge");
        Assert.That(left.anchoredPosition.x + 5, Is.EqualTo(11));
        Assert.That(right.anchoredPosition.x + 5, Is.EqualTo(target.rect.width - 12));
        Assert.That(-top.anchoredPosition.y + 5, Is.EqualTo(11));
        Assert.That(-bottom.anchoredPosition.y + 5, Is.EqualTo(target.rect.height - 12));
        foreach (RectTransform r in f.transform.Find("NativeConnectors")) if (r.gameObject.activeSelf)
        {
            var c = r.anchoredPosition + new Vector2(5, -5);
            Assert.That(Mathf.Approximately(c.x, 11) || Mathf.Approximately(c.x, target.rect.width - 12) || Mathf.Approximately(-c.y, 11) || Mathf.Approximately(-c.y, target.rect.height - 12), Is.True);
        }
    }
    static IEnumerator Capture(string name)
    {
        string p = Path.GetFullPath(Root + "/Previews/" + name + ".png"); if (File.Exists(p)) File.Delete(p);
        ScreenCapture.CaptureScreenshot(p); float deadline = Time.realtimeSinceStartup + 20;
        while (!File.Exists(p)) {Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null;}
    }
    [UnityTearDown] public IEnumerator Cleanup() {if (Application.isPlaying) yield return new ExitPlayMode();}
    [UnityTest, Explicit("Native 64px cells/gems, dynamic Forest shell, pixel-scale and decoration exclusion captures.")]
    public IEnumerator BoardNativeScreens()
    {
        Directory.CreateDirectory(Root + "/Previews"); Directory.CreateDirectory(Root + "/Diagnostics");
        Build(); SetSize(1); yield return new EnterPlayMode(); yield return null; yield return null;
        var target = (RectTransform)GameObject.Find("BoardFrameTarget").transform;
        var fitter = target.GetComponentInChildren<ResponsiveModularFrameFitter>();
        foreach (var size in new[] {new Vector2(46,46), new Vector2(560,560), new Vector2(561,559), new Vector2(640,512), new Vector2(411,689)})
        {target.sizeDelta = size; Canvas.ForceUpdateCanvases(); CheckShell(fitter);}
        target.sizeDelta = new Vector2(45,45); fitter.RefreshFrame(); Assert.That(fitter.GetComponentsInChildren<Image>().All(i => !i.enabled), Is.True);
        target.sizeDelta = Vector2.one * 560; Canvas.ForceUpdateCanvases(); CheckShell(fitter); yield return null;
        for (int scale = 1; scale <= 3; scale++)
        {
            SetSize(scale); float deadline = Time.realtimeSinceStartup + 20;
            while (Screen.width != Size * scale || Screen.height != Size * scale) {Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null;}
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            foreach (var image in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
            {
                Assert.That(image.sprite, Is.Not.Null, image.name); Assert.That(image.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point), image.name);
                var r = GameplayPixelLayoutController.ScreenRect(image.rectTransform);
                Assert.That(r.x, Is.EqualTo(Mathf.Round(r.x)).Within(.02), image.name); Assert.That(r.y, Is.EqualTo(Mathf.Round(r.y)).Within(.02), image.name);
                if (image.type == Image.Type.Simple) {Assert.That(r.width, Is.EqualTo(image.sprite.rect.width * scale).Within(.02), image.name); Assert.That(r.height, Is.EqualTo(image.sprite.rect.height * scale).Within(.02), image.name);}
                Assert.That(r.xMin, Is.GreaterThanOrEqualTo(-.02), image.name); Assert.That(r.yMin, Is.GreaterThanOrEqualTo(-.02), image.name);
                Assert.That(r.xMax, Is.LessThanOrEqualTo(Screen.width + .02), image.name); Assert.That(r.yMax, Is.LessThanOrEqualTo(Screen.height + .02), image.name);
            }
            yield return Capture("Unity-board-" + Screen.width + "x" + Screen.height);
            if (scale == 1)
            {
                var assembly = GameObject.Find("FrameAssembly"); assembly.SetActive(false); yield return null; yield return null;
                yield return Capture("Unity-board-no-frame"); assembly.SetActive(true);
                var gems = GameObject.Find("Existing native gems"); gems.SetActive(false); yield return null; yield return null;
                yield return Capture("Unity-board-empty"); gems.SetActive(true); yield return null;
            }
        }
        string[] forbidden = {"BoardController", "EnemyActor", "PlayerActor", "RunSession", "EventSystem"};
        foreach (var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (c != null) Assert.That(Array.IndexOf(forbidden, c.GetType().Name), Is.EqualTo(-1), c.name);
        yield return new ExitPlayMode();
    }
}
