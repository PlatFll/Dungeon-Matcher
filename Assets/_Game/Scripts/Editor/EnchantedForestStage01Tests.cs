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
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Stage-one art review only; production Forest and shared layout stay untouched.</summary>
public sealed class EnchantedForestStage01Tests
{
    const string Root = "ArtSource/EnchantedForestStaged/Stage01";
    const string Art = "Assets/_Game/Art/EnchantedForestStaged/Stage01/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage01.unity";
    static readonly string[] Retained = { "ForestPortrait", "Shared Wave candidate",
        "Shared Guide candidate", "Shared Settings candidate", "Existing Forest roster samples 2x" };
    [Serializable] public class Use { public string name, path; public Rect rect; }
    [Serializable] public class Evidence { public int width, height; public Use[] images; }
    static RectTransform Add(Transform parent, string file, int x, int y)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + file + ".png");
        Assert.That(s, Is.Not.Null, file);
        var r = new GameObject(file, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = s.rect.size;
        var image = r.GetComponent<Image>(); image.sprite = s; image.raycastTarget = false;
        return r;
    }
    [MenuItem("Dungeon Matcher/Art Preview/Enchanted Forest staged 01")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint03.unity", OpenSceneMode.Single);
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        root.name = "Enchanted Forest stage 01 art review";
        var top = root.Find("Approved forest top modules 2x");
        foreach (Transform child in top)
            if (!Retained.Contains(child.name)) child.gameObject.SetActive(false);
        var layer = new GameObject("Stage01 environment", typeof(RectTransform)).GetComponent<RectTransform>();
        layer.SetParent(top, false); layer.anchorMin = layer.anchorMax = layer.pivot = new Vector2(0, 1);
        layer.sizeDelta = new Vector2(360, 240); layer.anchoredPosition = Vector2.zero; layer.SetAsFirstSibling();
        Add(layer, "Grove_Background", 0, 0); Add(layer, "Combat_Ground", 0, 200);
        Add(layer, "Wisp_Far_A", 160, 121); Add(layer, "Wisp_Far_B", 258, 92); Add(layer, "Wisp_Near", 329, 161);
        foreach (var f in root.GetComponentsInChildren<ResponsiveModularFrameFitter>()) f.RefreshFrame();
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath), Is.False);
        Assert.That(root.Find("Approved forest top modules 2x/Existing Forest roster samples 2x").childCount, Is.EqualTo(3));
        EditorSceneManager.SaveScene(scene, ScenePath);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(360 * scale, 240 * scale) });
    static IEnumerator Capture(string name)
    {
        var path = Path.GetFullPath(Root + "/Previews/" + name + ".png");
        if (File.Exists(path)) File.Delete(path);
        ScreenCapture.CaptureScreenshot(path);
        var deadline = Time.realtimeSinceStartup + 20;
        while (!File.Exists(path)) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null; }
    }
    [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }
    [UnityTest, Explicit("Native Stage 1 art-only scene, three static actors, intact shared context and clean environment assembly.")]
    public IEnumerator TopBackgroundReview()
    {
        Directory.CreateDirectory(Root + "/Previews"); Directory.CreateDirectory(Root + "/Diagnostics");
        Build(); SetSize(2); yield return new EnterPlayMode(); yield return null; yield return null;
        // Reacquire all scene references after the domain reload.
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        var top = root.Find("Approved forest top modules 2x");
        // Retained UI was authored on the 720px review canvas; test it at its exact native registration.
        foreach (int scale in new[] { 2, 4 })
        {
            SetSize(scale);
            var deadline = Time.realtimeSinceStartup + 20;
            while (Screen.width != 360 * scale || Screen.height != 240 * scale) {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null;
            }
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            var uses = new List<Use>();
            foreach (var im in Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i => i.enabled))
            {
                Assert.That(im.color, Is.EqualTo(Color.white), im.name);
                Assert.That(im.raycastTarget, Is.False, im.name);
                Assert.That(im.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point), im.name);
                Assert.That(im.sprite.texture.mipmapCount, Is.EqualTo(1), im.name);
                Assert.That(im.transform.lossyScale.x, Is.EqualTo(im.transform.lossyScale.y), im.name);
                var rect = GameplayPixelLayoutController.ScreenRect(im.rectTransform);
                Assert.That(rect.x, Is.EqualTo(Mathf.Round(rect.x)).Within(.02), im.name);
                Assert.That(rect.y, Is.EqualTo(Mathf.Round(rect.y)).Within(.02), im.name);
                if (im.transform.IsChildOf(top.Find("Stage01 environment")))
                {
                    Assert.That(rect.width, Is.EqualTo(im.sprite.rect.width * scale).Within(.02), im.name);
                    Assert.That(rect.height, Is.EqualTo(im.sprite.rect.height * scale).Within(.02), im.name);
                    Assert.That(rect.xMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                    Assert.That(rect.yMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                    Assert.That(rect.xMax, Is.LessThanOrEqualTo(Screen.width + .02), im.name);
                    Assert.That(rect.yMax, Is.LessThanOrEqualTo(Screen.height + .02), im.name);
                }
                uses.Add(new Use { name = im.name, path = AssetDatabase.GetAssetPath(im.sprite), rect = rect });
            }
            foreach (var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) {
                t.ForceMeshUpdate(); Assert.That(t.isTextTruncated, Is.False, t.text);
            }
            foreach (var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (c != null) Assert.That(new[] { "BoardController", "EnemyActor", "PlayerActor", "RunSession", "EventSystem" }, Does.Not.Contain(c.GetType().Name));
            File.WriteAllText(Root + "/Diagnostics/Unity-" + Screen.width + "x" + Screen.height + ".json",
                JsonUtility.ToJson(new Evidence { width = Screen.width, height = Screen.height, images = uses.ToArray() }, true));
            yield return Capture("Unity-Stage01-" + Screen.width + "x" + Screen.height);
        }
        foreach (string n in Retained) top.Find(n).gameObject.SetActive(false);
        root.Find("Static readability samples").gameObject.SetActive(false);
        SetSize(1);
        var nativeDeadline = Time.realtimeSinceStartup + 20;
        while (Screen.width != 360 || Screen.height != 240) {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(nativeDeadline)); yield return null;
        }
        yield return null; yield return null; Canvas.ForceUpdateCanvases();
        foreach (var im in top.Find("Stage01 environment").GetComponentsInChildren<Image>()) {
            var rect = GameplayPixelLayoutController.ScreenRect(im.rectTransform);
            Assert.That(rect.width, Is.EqualTo(im.sprite.rect.width).Within(.02));
            Assert.That(rect.height, Is.EqualTo(im.sprite.rect.height).Within(.02));
            Assert.That(rect.x, Is.EqualTo(Mathf.Round(rect.x)).Within(.02));
            Assert.That(rect.y, Is.EqualTo(Mathf.Round(rect.y)).Within(.02));
        }
        yield return Capture("Unity-Environment-360x240");
        yield return new ExitPlayMode();
    }
}
