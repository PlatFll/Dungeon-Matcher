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

/// <summary>Stage 4 art-only frame review. Existing shared layout and production assets are unchanged.</summary>
public sealed class EnchantedForestStage04Tests
{
    const string Root = "ArtSource/EnchantedForestStaged/Stage04";
    const string Art = "Assets/_Game/Art/EnchantedForestStaged/Stage04/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage04.unity";
    const string NewFrame = "Stage04 forest board frame 2x";
    const string Baseline = "Stage04 baseline board frame 2x";
    [Serializable] public class Placement { public string name, file; public float anchorX, anchorY; public int x, y; }
    [Serializable] public class Layout { public Placement[] pieces; }
    [Serializable] public class Use { public string name, path, type; public Rect rect; public float scale; public bool frame; }
    [Serializable] public class Evidence { public int width, height; public Rect board, portrait, frame; public Use[] images; }
    static Transform SourceRoot(Scene s) => s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>()).Single().transform;
    static Sprite Load(string file) { var s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + file + ".png"); Assert.That(s, Is.Not.Null, file); return s; }
    static RectTransform Copy(Transform from, Transform to)
    {
        var r = Object.Instantiate(from.gameObject, to, false).GetComponent<RectTransform>(); r.name = from.name; return r;
    }
    static void Revision(Transform root, bool enabled)
    {
        root.Find(Baseline).gameObject.SetActive(!enabled); root.Find(NewFrame).gameObject.SetActive(enabled);
    }
    [MenuItem("Dungeon Matcher/Art Preview/Enchanted Forest staged 04")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestReworkV2_FrameFit.unity", OpenSceneMode.Single);
        var root = SourceRoot(scene); root.name = "Enchanted Forest stage 04 board frame review";
        var topScene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage03.unity", OpenSceneMode.Additive);
        var source = SourceRoot(topScene); SceneManager.SetActiveScene(scene);
        foreach (string name in new[] { "Approved forest top modules 2x", "Static readability samples" })
        {
            var old = root.Find(name); int index = old.GetSiblingIndex(); Object.DestroyImmediate(old.gameObject);
            var copied = Copy(source.Find(name), root); copied.SetSiblingIndex(index);
            var grid = name.StartsWith("Approved") ? copied : (RectTransform)root;
            foreach (var text in copied.GetComponentsInChildren<DungeonPreviewPixelText>(true)) text.Initialize(grid);
        }
        EditorSceneManager.CloseScene(topScene, true);
        var oldFrame = root.Find("Revised board frame 2x"); oldFrame.name = Baseline;
        var replacement = Copy(oldFrame, root); replacement.name = NewFrame; replacement.SetSiblingIndex(oldFrame.GetSiblingIndex() + 1);
        var target = replacement.Find("BoardFrameTarget");
        foreach (string name in new[] { "Upper connected foliage", "Lower connected foliage" }) Object.DestroyImmediate(target.Find(name).gameObject);
        var fitter = target.Find("ModularFrame").GetComponent<ResponsiveModularFrameFitter>();
        string[] names = { "TopLeftCorner", "TopRightCorner", "BottomLeftCorner", "BottomRightCorner", "TopEdge", "BottomEdge", "LeftEdge", "RightEdge" };
        string[] files = { "Corner_TL", "Corner_TR", "Corner_BL", "Corner_BR", "Rail_Top", "Rail_Bottom", "Rail_Left", "Rail_Right" };
        for (int i = 0; i < names.Length; i++) fitter.transform.Find(names[i]).GetComponent<Image>().sprite = Load("Board_" + files[i]);
        fitter.ConfigureNativePieces(null, 150); fitter.FollowOpening((RectTransform)root.Find("Board contents native 64px"));
        var layout = JsonUtility.FromJson<Layout>(File.ReadAllText(Root + "/Scripts/Placements.json"));
        foreach (var q in layout.pieces)
        {
            var sprite = Load(q.file); var rt = new GameObject(q.name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            rt.SetParent(target, false); rt.pivot = new Vector2(0, 1); rt.anchorMin = rt.anchorMax = new Vector2(q.anchorX, q.anchorY);
            rt.anchoredPosition = new Vector2(q.x, -q.y); rt.sizeDelta = sprite.rect.size;
            var image = rt.GetComponent<Image>(); image.sprite = sprite; image.raycastTarget = false;
        }
        Revision(root, true); Canvas.ForceUpdateCanvases(); fitter.RefreshFrame();
        Assert.That(((RectTransform)target).rect.size, Is.EqualTo(new Vector2(290, 290)));
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath), Is.False);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(720 * scale, 1360 * scale) });
    static IEnumerator Capture(string name)
    {
        var file = Path.GetFullPath(Root + "/Previews/" + name + ".png"); if (File.Exists(file)) File.Delete(file);
        ScreenCapture.CaptureScreenshot(file); var deadline = Time.realtimeSinceStartup + 20;
        while (!File.Exists(file)) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null; }
    }
    static void CheckFit(Transform root)
    {
        var board = GameplayPixelLayoutController.ScreenRect((RectTransform)root.Find("Board contents native 64px"));
        var fitter = root.Find(NewFrame + "/BoardFrameTarget/ModularFrame").GetComponent<ResponsiveModularFrameFitter>();
        Rect Edge(string name) => GameplayPixelLayoutController.ScreenRect((RectTransform)fitter.transform.Find(name));
        Assert.That(Edge("LeftEdge").xMax, Is.EqualTo(board.xMin).Within(.02), "Left rail must meet tiles");
        Assert.That(Edge("RightEdge").xMin, Is.EqualTo(board.xMax).Within(.02), "Right rail must meet tiles");
        Assert.That(Edge("TopEdge").yMin, Is.EqualTo(board.yMax).Within(.02), "Top rail must meet tiles");
        Assert.That(Edge("BottomEdge").yMax, Is.EqualTo(board.yMin).Within(.02), "Bottom rail must meet tiles");
    }
    static void CheckResize(Transform root)
    {
        var board = (RectTransform)root.Find("Board contents native 64px"); var saved = board.sizeDelta;
        var fitter = root.Find(NewFrame + "/BoardFrameTarget/ModularFrame").GetComponent<ResponsiveModularFrameFitter>();
        // Geometry probes only; the saved/rendered review always uses the original 512px board.
        foreach (var size in new[] { new Vector2(128,128), new Vector2(512,512), new Vector2(514,510), new Vector2(640,512), new Vector2(512,768) })
        {
            board.sizeDelta = size; Canvas.ForceUpdateCanvases(); fitter.RefreshFrame(); CheckFit(root);
            foreach (var im in fitter.GetComponentsInChildren<Image>())
            {
                Assert.That(im.transform.localScale, Is.EqualTo(Vector3.one));
                if (im.type == Image.Type.Simple) Assert.That(im.rectTransform.rect.size, Is.EqualTo(new Vector2(23,23)));
                else Assert.That(im.pixelsPerUnit * im.pixelsPerUnitMultiplier, Is.EqualTo(1).Within(.001));
            }
        }
        board.sizeDelta = saved; Canvas.ForceUpdateCanvases(); fitter.RefreshFrame(); CheckFit(root);
    }
    static Rect VisibleRect(Image im, Rect rect)
    {
        if (im.type != Image.Type.Tiled) return rect;
        foreach (var mask in im.GetComponentsInParent<RectMask2D>())
        {
            var clip = GameplayPixelLayoutController.ScreenRect(mask.rectTransform);
            rect = UnityEngine.Rect.MinMaxRect(Mathf.Max(rect.xMin,clip.xMin),Mathf.Max(rect.yMin,clip.yMin),Mathf.Min(rect.xMax,clip.xMax),Mathf.Min(rect.yMax,clip.yMax));
        }
        return rect;
    }
    [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }
    [UnityTest, Explicit("Stage 4 board frame: no gaps, exact approved art, native pixels and resize checks.")]
    public IEnumerator BoardFrameReview()
    {
        Directory.CreateDirectory(Root + "/Diagnostics"); Directory.CreateDirectory(Root + "/Previews");
        Build(); SetSize(1); yield return new EnterPlayMode(); yield return null; yield return null;
        var root = Object.FindFirstObjectByType<Canvas>().transform; Revision(root, false); Canvas.ForceUpdateCanvases();
        yield return null; yield return null; yield return Capture("Unity-Before-720x1360"); Revision(root, true); CheckResize(root);
        foreach (int scale in new[] { 1, 2 })
        {
            SetSize(scale); float until = Time.realtimeSinceStartup + 20;
            while (Screen.width != 720*scale || Screen.height != 1360*scale) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(until)); yield return null; }
            Canvas.ForceUpdateCanvases(); yield return null; yield return null; CheckFit(root);
            var board = (RectTransform)root.Find("Board contents native 64px");
            Assert.That(board.rect.size, Is.EqualTo(new Vector2(512,512)));
            Assert.That(board.anchoredPosition, Is.EqualTo(new Vector2(104,-528)));
            var portrait = (RectTransform)root.Find("Approved forest top modules 2x/ForestPortrait");
            Assert.That(portrait.rect.size, Is.EqualTo(new Vector2(98,220)));
            var frame = root.Find(NewFrame); var uses = new List<Use>();
            foreach (var im in Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i => i.enabled))
            {
                Assert.That(im.color, Is.EqualTo(Color.white), im.name); Assert.That(im.raycastTarget, Is.False, im.name);
                Assert.That(im.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point), im.name);
                Assert.That(im.sprite.texture.mipmapCount, Is.EqualTo(1), im.name);
                var rect = GameplayPixelLayoutController.ScreenRect(im.rectTransform); var visible = VisibleRect(im, rect);
                float factor = im.transform.lossyScale.x / root.lossyScale.x * scale;
                Assert.That(factor, Is.EqualTo(Mathf.Round(factor)).Within(.001), im.name);
                Assert.That(im.transform.lossyScale.x, Is.EqualTo(im.transform.lossyScale.y), im.name);
                Assert.That(rect.x, Is.EqualTo(Mathf.Round(rect.x)).Within(.02), im.name); Assert.That(rect.y, Is.EqualTo(Mathf.Round(rect.y)).Within(.02), im.name);
                if (im.type == Image.Type.Simple) { Assert.That(rect.width, Is.EqualTo(im.sprite.rect.width*factor).Within(.02), im.name); Assert.That(rect.height, Is.EqualTo(im.sprite.rect.height*factor).Within(.02), im.name); }
                else Assert.That(im.pixelsPerUnit*im.pixelsPerUnitMultiplier, Is.EqualTo(1).Within(.001), im.name);
                Assert.That(visible.xMin, Is.GreaterThanOrEqualTo(-.02), im.name); Assert.That(visible.yMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                Assert.That(visible.xMax, Is.LessThanOrEqualTo(Screen.width+.02), im.name); Assert.That(visible.yMax, Is.LessThanOrEqualTo(Screen.height+.02), im.name);
                uses.Add(new Use {name=im.name,path=AssetDatabase.GetAssetPath(im.sprite),type=im.type.ToString(),rect=rect,scale=factor,frame=im.transform.IsChildOf(frame)});
            }
            foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) { text.ForceMeshUpdate(); Assert.That(text.isTextTruncated, Is.False, text.text); }
            foreach (var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(c != null) Assert.That(new[]{"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"},Does.Not.Contain(c.GetType().Name));
            File.WriteAllText(Root+"/Diagnostics/Unity-"+Screen.width+"x"+Screen.height+".json",JsonUtility.ToJson(new Evidence {width=Screen.width,height=Screen.height,board=GameplayPixelLayoutController.ScreenRect(board),portrait=GameplayPixelLayoutController.ScreenRect(portrait),frame=GameplayPixelLayoutController.ScreenRect((RectTransform)frame.Find("BoardFrameTarget")),images=uses.ToArray()},true));
            yield return Capture("Unity-Stage04-"+Screen.width+"x"+Screen.height);
        }
        yield return new ExitPlayMode();
    }
}
