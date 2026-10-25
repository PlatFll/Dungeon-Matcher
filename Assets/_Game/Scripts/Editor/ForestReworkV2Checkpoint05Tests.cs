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

/// <summary>Read-only assembly of approved Forest review scenes. No production binding changes.</summary>
public sealed class ForestReworkV2Checkpoint05Tests
{
    const string Root = "ArtSource/ForestReworkV2/Checkpoint05";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint05.unity";
    const int Width = 720, Height = 1360;
    [Serializable] public class Use { public string name, path, type; public Rect nativeRect, screenRect; public float scale; }
    [Serializable] public class Evidence { public int width, height, rosterCount; public Use[] images; public Rect portraitMask; }
    static Transform SourceRoot(Scene scene) => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>()).Single().transform;
    static RectTransform Copy(Transform from, Transform parent)
    {
        var r = Object.Instantiate(from.gameObject, parent, false).GetComponent<RectTransform>(); r.name = from.name; return r;
    }
    [MenuItem("Dungeon Matcher/Art Preview/Forest rework v2 checkpoint 05 full assembly")]
    public static void Build()
    {
        // Open the original full review, then save only under a new path. Earlier builders are never invoked.
        var destination = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage05.unity", OpenSceneMode.Single);
        var root = SourceRoot(destination); root.name = "Forest v2 checkpoint 05 full art-only assembly";
        var topSource = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint03.unity", OpenSceneMode.Additive);
        SceneManager.SetActiveScene(destination); var approvedTop = SourceRoot(topSource);
        foreach (string name in new[] { "Approved forest top modules 2x", "Static readability samples" })
        {
            var prior = root.Find(name); int index = prior.GetSiblingIndex(); Object.DestroyImmediate(prior.gameObject);
            var clone = Copy(approvedTop.Find(name), root); clone.SetSiblingIndex(index);
            var grid = name.StartsWith("Approved") ? clone : (RectTransform)root;
            foreach (var label in clone.GetComponentsInChildren<DungeonPreviewPixelText>(true)) label.Initialize(grid);
        }
        EditorSceneManager.CloseScene(topSource, true);
        var boardSource = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint04.unity", OpenSceneMode.Additive);
        SceneManager.SetActiveScene(destination);
        Object.DestroyImmediate(root.Find("Board shell 2x native modules").gameObject);
        Object.DestroyImmediate(root.Find("Board props 2x").gameObject);
        var frame = Copy(SourceRoot(boardSource).Find("Revised board frame 2x"), root);
        // The isolated board review has a 432-screen-pixel vertical offset from this full layout.
        frame.anchoredPosition += new Vector2(0, -432);
        frame.SetSiblingIndex(root.Find("Shared bottom HUD").GetSiblingIndex());
        EditorSceneManager.CloseScene(boardSource, true);
        foreach (var f in root.GetComponentsInChildren<ResponsiveModularFrameFitter>(true)) f.RefreshFrame();
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath), Is.False);
        EditorSceneManager.SaveScene(destination, ScenePath);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(Width * scale, Height * scale) });
    static IEnumerator Capture(string name)
    {
        string file = Path.GetFullPath(Root + "/Previews/" + name + ".png"); if (File.Exists(file)) File.Delete(file);
        ScreenCapture.CaptureScreenshot(file); float until = Time.realtimeSinceStartup + 20;
        while (!File.Exists(file)) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(until)); yield return null; }
    }
    static Rect ClippedRect(Image image, Rect raw)
    {
        if (image.type != Image.Type.Tiled) return raw;
        foreach (var mask in image.GetComponentsInParent<RectMask2D>())
        {
            var clip = GameplayPixelLayoutController.ScreenRect(mask.rectTransform);
            raw = UnityEngine.Rect.MinMaxRect(Mathf.Max(raw.xMin, clip.xMin), Mathf.Max(raw.yMin, clip.yMin), Mathf.Min(raw.xMax, clip.xMax), Mathf.Min(raw.yMax, clip.yMax));
        }
        return raw;
    }
    [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }
    [UnityTest, Explicit("Approved full Forest assembly with source preservation and native screenshot checks.")]
    public IEnumerator FullAssemblyReview()
    {
        Directory.CreateDirectory(Root + "/Previews"); Directory.CreateDirectory(Root + "/Diagnostics");
        Build(); SetSize(1); yield return new EnterPlayMode(); yield return null; yield return null;
        yield return CaptureAndCheck(); yield return new ExitPlayMode();
    }
    static IEnumerator CaptureAndCheck()
    {
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        var top = root.Find("Approved forest top modules 2x");
        var roster = top.Find("Existing Forest roster samples 2x");
        var portrait = top.Find("ForestPortrait/InteriorMask");
        Assert.That(roster.childCount, Is.EqualTo(3));
        Assert.That(((RectTransform)portrait).rect.size, Is.EqualTo(new Vector2(74, 196)));
        Assert.That(((RectTransform)portrait.Find("OverfilledBacking")).rect.size, Is.EqualTo(new Vector2(138, 260)));
        Assert.That(((RectTransform)root.Find("Board contents native 64px")).anchoredPosition, Is.EqualTo(new Vector2(104, -528)));
        foreach (int scale in new[] { 1, 2 })
        {
            SetSize(scale); float until = Time.realtimeSinceStartup + 20;
            while (Screen.width != Width * scale || Screen.height != Height * scale) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(until)); yield return null; }
            Canvas.ForceUpdateCanvases(); yield return null; yield return null; var uses = new List<Use>();
            foreach (var im in Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i => i.enabled))
            {
                Assert.That(im.sprite, Is.Not.Null, im.name); Assert.That(im.color, Is.EqualTo(Color.white), im.name); Assert.That(im.raycastTarget, Is.False, im.name);
                Assert.That(im.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point), im.name);
                var r = GameplayPixelLayoutController.ScreenRect(im.rectTransform); var visible = ClippedRect(im, r);
                float factor = im.transform.lossyScale.x / root.lossyScale.x * scale;
                Assert.That(factor, Is.EqualTo(Mathf.Round(factor)).Within(.001), im.name);
                Assert.That(im.transform.lossyScale.x, Is.EqualTo(im.transform.lossyScale.y), im.name);
                Assert.That(r.x, Is.EqualTo(Mathf.Round(r.x)).Within(.02), im.name); Assert.That(r.y, Is.EqualTo(Mathf.Round(r.y)).Within(.02), im.name);
                if (im.type == Image.Type.Simple)
                {
                    Assert.That(r.width, Is.EqualTo(im.sprite.rect.width * factor).Within(.02), im.name);
                    Assert.That(r.height, Is.EqualTo(im.sprite.rect.height * factor).Within(.02), im.name);
                }
                else Assert.That(im.pixelsPerUnit * im.pixelsPerUnitMultiplier, Is.EqualTo(1).Within(.001), im.name);
                Assert.That(visible.xMin, Is.GreaterThanOrEqualTo(-.02), im.name); Assert.That(visible.yMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                Assert.That(visible.xMax, Is.LessThanOrEqualTo(Screen.width + .02), im.name); Assert.That(visible.yMax, Is.LessThanOrEqualTo(Screen.height + .02), im.name);
                uses.Add(new Use { name = im.name, path = AssetDatabase.GetAssetPath(im.sprite), type = im.type.ToString(), nativeRect = im.sprite.rect, screenRect = r, scale = factor });
            }
            foreach (var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) { t.ForceMeshUpdate(); Assert.That(t.isTextTruncated, Is.False, t.text); }
            foreach (var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (c != null) Assert.That(new[] { "BoardController", "EnemyActor", "PlayerActor", "RunSession", "EventSystem" }, Does.Not.Contain(c.GetType().Name));
            File.WriteAllText(Root + "/Diagnostics/Unity-" + Screen.width + "x" + Screen.height + ".json", JsonUtility.ToJson(new Evidence { width = Screen.width, height = Screen.height, rosterCount = roster.childCount, images = uses.ToArray(), portraitMask = GameplayPixelLayoutController.ScreenRect((RectTransform)portrait) }, true));
            yield return Capture("Unity-Full-" + Screen.width + "x" + Screen.height);
        }
        SetSize(1); yield return null; yield return null;
        var backing = portrait.Find("OverfilledBacking").GetComponent<Image>(); backing.color = Color.magenta;
        yield return null; yield return null; yield return Capture("Unity-MaskProbe-720x1360"); backing.color = Color.white;
        root.Find("Board contents native 64px/Existing native gems").gameObject.SetActive(false);
        roster.gameObject.SetActive(false); portrait.Find("PortraitContents/Rattlebones unchanged").gameObject.SetActive(false);
        root.Find("Static readability samples").gameObject.SetActive(false);
        Canvas.ForceUpdateCanvases(); yield return null; yield return null; yield return Capture("Unity-Structure-720x1360");
    }
}
