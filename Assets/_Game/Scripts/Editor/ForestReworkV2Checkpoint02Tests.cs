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

/// <summary>Isolated top-background review. Never saves the source scene or production bindings.</summary>
public sealed class ForestReworkV2Checkpoint02Tests
{
    const string Root = "ArtSource/ForestReworkV2/Checkpoint02";
    const string Art = "Assets/_Game/Art/ForestReworkV2/Checkpoint02/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint02.unity";
    const int Width = 720, Height = 480;
    static readonly Dictionary<string, string> Replacements = new Dictionary<string, string> {
        { "Far_Forest", "Far_EnchantedSanctuary" }, { "Far_SideExtension", "Depth_SideRepeat" },
        { "Far_CanopyRepeat", "Depth_CanopyRepeat" }, { "Understory_Repeat", "Depth_UnderstoryRepeat" }
    };
    [Serializable] public class Use { public string name, path; public Rect rect; public float scale; }
    [Serializable] public class Evidence { public int width, height; public Use[] images; }
    static Sprite Load(string p) { var s = AssetDatabase.LoadAssetAtPath<Sprite>(p); Assert.That(s, Is.Not.Null, p); return s; }
    static RectTransform Rect(string name, Transform parent, int x, int y, int w, int h)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
    }
    static void Revision(Transform root, bool enabled)
    {
        var top = root.Find("Approved forest top modules 2x");
        foreach (var im in top.GetComponentsInChildren<Image>(true))
            if (Replacements.TryGetValue(im.name, out string replacement))
                im.sprite = Load(enabled ? Art + replacement + ".png" : "Assets/_Game/Art/ForestHybrid/Stage01/" + im.name + ".png");
        top.Find("Rune_Plaque").gameObject.SetActive(!enabled);
        top.Find("Distant magical lights").gameObject.SetActive(enabled);
    }
    [MenuItem("Dungeon Matcher/Art Preview/Forest rework v2 checkpoint 02")]
    public static void Build()
    {
        var destination = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Forest top review camera").AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = Height / 128f; camera.aspect = (float)Width / Height;
        camera.transform.position = new Vector3(0, 0, -10); camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.magenta; camera.allowHDR = camera.allowMSAA = false;
        var canvas = new GameObject("Forest v2 checkpoint 02 art-only top", typeof(RectTransform)).AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = camera; canvas.pixelPerfect = true;
        var scaler = canvas.gameObject.AddComponent<CanvasScaler>(); scaler.referencePixelsPerUnit = 64; scaler.dynamicPixelsPerUnit = 1;
        var root = (RectTransform)canvas.transform; root.sizeDelta = new Vector2(Width, Height); root.pivot = Vector2.zero;
        root.position = new Vector3(-Width / 128f, -Height / 128f, 0); root.localScale = Vector3.one / 64;
        var source = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage05.unity", OpenSceneMode.Additive);
        SceneManager.SetActiveScene(destination);
        var sourceRoot = source.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>()).Single().transform;
        // Clone only the review area; shared lower UI is neither rebuilt nor edited.
        foreach (string name in new[] { "Approved forest top modules 2x", "Static readability samples" })
        {
            var clone = Object.Instantiate(sourceRoot.Find(name).gameObject, root, false); clone.name = name;
            var grid = name.StartsWith("Approved") ? clone.GetComponent<RectTransform>() : root;
            foreach (var t in clone.GetComponentsInChildren<DungeonPreviewPixelText>(true)) t.Initialize(grid);
        }
        EditorSceneManager.CloseScene(source, true);
        var top = root.Find("Approved forest top modules 2x");
        var lights = Rect("Distant magical lights", top, 0, 0, 360, 240);
        // Place behind the unchanged near trees, well above all fighter canvases.
        lights.SetSiblingIndex(top.Find("Far_Forest").GetSiblingIndex() + 1);
        foreach (Vector2Int at in new[] { new Vector2Int(201, 100), new Vector2Int(267, 86) })
        {
            var r = Rect("Distant wisp", lights, at.x, at.y, 12, 16);
            var i = r.gameObject.AddComponent<Image>(); i.sprite = Load(Art + "Distant_Wisp.png"); i.raycastTarget = false;
        }
        Revision(root, true);
        foreach (var f in root.GetComponentsInChildren<ResponsiveModularFrameFitter>(true)) f.RefreshFrame();
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath), Is.False);
        EditorSceneManager.SaveScene(destination, ScenePath);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(Width * scale, Height * scale) });
    static IEnumerator Screenshot(string name)
    {
        var path = Path.GetFullPath(Root + "/Previews/" + name + ".png"); if (File.Exists(path)) File.Delete(path);
        ScreenCapture.CaptureScreenshot(path); var deadline = Time.realtimeSinceStartup + 20;
        while (!File.Exists(path)) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null; }
    }
    [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }
    [UnityTest, Explicit("Checkpoint two top-background blockout, whole-pixel capture and retained layout checks.")]
    public IEnumerator TopBackgroundReview()
    {
        Directory.CreateDirectory(Root + "/Previews"); Directory.CreateDirectory(Root + "/Diagnostics");
        Build(); SetSize(1); yield return new EnterPlayMode(); yield return null; yield return null;
        yield return CaptureAndCheck(); yield return new ExitPlayMode();
    }
    static IEnumerator CaptureAndCheck()
    {
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        Revision(root, false); Canvas.ForceUpdateCanvases(); yield return null; yield return null;
        yield return Screenshot("Unity-Before-720x480"); Revision(root, true);
        foreach (int scale in new[] { 1, 2 })
        {
            SetSize(scale); var deadline = Time.realtimeSinceStartup + 20;
            while (Screen.width != Width * scale || Screen.height != Height * scale) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null; }
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            var uses = new List<Use>();
            foreach (var im in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
            {
                Assert.That(im.color, Is.EqualTo(Color.white), im.name); Assert.That(im.raycastTarget, Is.False, im.name);
                Assert.That(im.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point), im.name);
                var r = GameplayPixelLayoutController.ScreenRect(im.rectTransform);
                float factor = im.transform.lossyScale.x / root.lossyScale.x * scale;
                Assert.That(factor, Is.EqualTo(Mathf.Round(factor)).Within(.001), im.name);
                Assert.That(im.transform.lossyScale.x, Is.EqualTo(im.transform.lossyScale.y), im.name);
                Assert.That(r.x, Is.EqualTo(Mathf.Round(r.x)).Within(.02), im.name);
                Assert.That(r.y, Is.EqualTo(Mathf.Round(r.y)).Within(.02), im.name);
                var visible = r;
                // Repeating backing intentionally overfills the portrait mask. Check
                // its visible extent; independent Simple sprites must fit in full.
                if (im.type == Image.Type.Tiled)
                    foreach (var mask in im.GetComponentsInParent<RectMask2D>())
                    {
                        var clip = GameplayPixelLayoutController.ScreenRect(mask.rectTransform);
                        visible = UnityEngine.Rect.MinMaxRect(Mathf.Max(visible.xMin, clip.xMin), Mathf.Max(visible.yMin, clip.yMin),
                            Mathf.Min(visible.xMax, clip.xMax), Mathf.Min(visible.yMax, clip.yMax));
                    }
                Assert.That(visible.xMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                Assert.That(visible.yMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                Assert.That(visible.xMax, Is.LessThanOrEqualTo(Screen.width + .02), im.name);
                Assert.That(visible.yMax, Is.LessThanOrEqualTo(Screen.height + .02), im.name);
                if (im.type == Image.Type.Simple)
                {
                    Assert.That(r.width, Is.EqualTo(im.sprite.rect.width * factor).Within(.02), im.name);
                    Assert.That(r.height, Is.EqualTo(im.sprite.rect.height * factor).Within(.02), im.name);
                }
                else Assert.That(im.pixelsPerUnit * im.pixelsPerUnitMultiplier, Is.EqualTo(1).Within(.001), im.name);
                if (im.name == "Distant wisp")
                    Assert.That(r.yMin, Is.GreaterThan((Height - 252) * scale), "Light must stay above fighter canvases");
                uses.Add(new Use { name = im.name, path = AssetDatabase.GetAssetPath(im.sprite), rect = r, scale = factor });
            }
            foreach (var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) { t.ForceMeshUpdate(); Assert.That(t.isTextTruncated, Is.False, t.text); }
            foreach (var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (c != null) Assert.That(new[] { "BoardController", "EnemyActor", "PlayerActor", "RunSession", "EventSystem" }, Does.Not.Contain(c.GetType().Name));
            File.WriteAllText(Root + "/Diagnostics/Unity-" + Screen.width + "x" + Screen.height + ".json",
                JsonUtility.ToJson(new Evidence { width = Screen.width, height = Screen.height, images = uses.ToArray() }, true));
            yield return Screenshot("Unity-After-" + Screen.width + "x" + Screen.height);
        }
        SetSize(1); yield return null; yield return null;
        var portraitMask = root.Find("Approved forest top modules 2x/ForestPortrait/InteriorMask");
        var backing = portraitMask.Find("OverfilledBacking").GetComponent<Image>();
        var backingRect = GameplayPixelLayoutController.ScreenRect(backing.rectTransform);
        var maskRect = GameplayPixelLayoutController.ScreenRect((RectTransform)portraitMask);
        Assert.That(backingRect.xMin, Is.LessThan(maskRect.xMin)); Assert.That(backingRect.xMax, Is.GreaterThan(maskRect.xMax));
        Assert.That(backingRect.yMin, Is.LessThan(maskRect.yMin)); Assert.That(backingRect.yMax, Is.GreaterThan(maskRect.yMax));
        backing.color = Color.magenta; yield return null; yield return null;
        yield return Screenshot("Unity-MaskProbe-720x480");
        backing.color = Color.white; yield return null; yield return null;
        root.Find("Approved forest top modules 2x/Existing Forest roster samples 2x").gameObject.SetActive(false);
        root.Find("Approved forest top modules 2x/ForestPortrait/InteriorMask/PortraitContents/Rattlebones unchanged").gameObject.SetActive(false);
        root.Find("Static readability samples").gameObject.SetActive(false);
        Canvas.ForceUpdateCanvases(); yield return null; yield return null;
        yield return Screenshot("Unity-Background-720x480");
    }
}
