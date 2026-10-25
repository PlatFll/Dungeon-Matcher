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

/// <summary>Stage 3 decoration-only review; approved scene and shared runtime remain untouched.</summary>
public sealed class EnchantedForestStage03Tests
{
    const string Root = "ArtSource/EnchantedForestStaged/Stage03";
    const string Art = "Assets/_Game/Art/EnchantedForestStaged/Stage03/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage03.unity";
    [Serializable] public class Placement { public string name, file; public int x, y; }
    [Serializable] public class Layout { public Placement[] pieces; }
    [Serializable] public class Use { public string name, path; public Rect rect; public bool decoration; }
    [Serializable] public class Evidence { public int width, height; public Use[] images; }
    [MenuItem("Dungeon Matcher/Art Preview/Enchanted Forest staged 03")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage02_Foliage.unity", OpenSceneMode.Single);
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        root.name = "Enchanted Forest stage 03 magical decor review";
        var top = root.Find("Approved forest top modules 2x");
        var layer = new GameObject("Stage03 magical accents", typeof(RectTransform)).GetComponent<RectTransform>();
        layer.SetParent(top, false); layer.anchorMin = layer.anchorMax = layer.pivot = new Vector2(0, 1);
        layer.sizeDelta = new Vector2(360, 240); layer.anchoredPosition = Vector2.zero;
        layer.SetSiblingIndex(1); // Above the approved environment, below characters and all UI.
        Assert.That(top.GetChild(0).name, Is.EqualTo("Stage01 environment"));
        var layout = JsonUtility.FromJson<Layout>(File.ReadAllText(Root + "/Scripts/Placements.json"));
        foreach (var item in layout.pieces)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + item.file + ".png"); Assert.That(sprite, Is.Not.Null, item.file);
            var rt = new GameObject(item.name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            rt.SetParent(layer, false); rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(item.x, -item.y); rt.sizeDelta = sprite.rect.size;
            var im = rt.GetComponent<Image>(); im.sprite = sprite; im.raycastTarget = false;
        }
        Assert.That(((RectTransform)top.Find("ForestPortrait")).rect.size, Is.EqualTo(new Vector2(98, 220)));
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath), Is.False);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(360 * scale, 240 * scale) });
    static IEnumerator Capture(string name)
    {
        var path = Path.GetFullPath(Root + "/Previews/" + name + ".png"); if (File.Exists(path)) File.Delete(path);
        ScreenCapture.CaptureScreenshot(path); var deadline = Time.realtimeSinceStartup + 20;
        while (!File.Exists(path)) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null; }
    }
    [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }
    [UnityTest, Explicit("Stage 3 separate native decoration and preserved approved scene.")]
    public IEnumerator DecorationReview()
    {
        Directory.CreateDirectory(Root + "/Previews"); Directory.CreateDirectory(Root + "/Diagnostics");
        Build(); SetSize(2); yield return new EnterPlayMode(); yield return null; yield return null;
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        var decor = root.Find("Approved forest top modules 2x/Stage03 magical accents");
        foreach (int scale in new[] { 2, 4 })
        {
            SetSize(scale); var deadline = Time.realtimeSinceStartup + 20;
            while (Screen.width != 360 * scale || Screen.height != 240 * scale) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null; }
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            var uses = new List<Use>();
            foreach (var im in Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i => i.enabled))
            {
                Assert.That(im.color, Is.EqualTo(Color.white), im.name); Assert.That(im.raycastTarget, Is.False, im.name);
                Assert.That(im.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point), im.name);
                Assert.That(im.sprite.texture.mipmapCount, Is.EqualTo(1), im.name);
                Assert.That(im.transform.lossyScale.x, Is.EqualTo(im.transform.lossyScale.y), im.name);
                var rect = GameplayPixelLayoutController.ScreenRect(im.rectTransform);
                Assert.That(rect.x, Is.EqualTo(Mathf.Round(rect.x)).Within(.02), im.name);
                Assert.That(rect.y, Is.EqualTo(Mathf.Round(rect.y)).Within(.02), im.name);
                bool added = im.transform.IsChildOf(decor);
                if (added) {
                    Assert.That(rect.width, Is.EqualTo(im.sprite.rect.width * scale).Within(.02), im.name);
                    Assert.That(rect.height, Is.EqualTo(im.sprite.rect.height * scale).Within(.02), im.name);
                    Assert.That(rect.xMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                    Assert.That(rect.yMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                    Assert.That(rect.xMax, Is.LessThanOrEqualTo(Screen.width + .02), im.name);
                    Assert.That(rect.yMax, Is.LessThanOrEqualTo(Screen.height + .02), im.name);
                }
                uses.Add(new Use {name=im.name,path=AssetDatabase.GetAssetPath(im.sprite),rect=rect,decoration=added});
            }
            foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) { text.ForceMeshUpdate(); Assert.That(text.isTextTruncated, Is.False, text.text); }
            foreach (var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if(c != null) Assert.That(new[]{"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"},Does.Not.Contain(c.GetType().Name));
            File.WriteAllText(Root + "/Diagnostics/Unity-" + Screen.width + "x" + Screen.height + ".json",JsonUtility.ToJson(new Evidence{width=Screen.width,height=Screen.height,images=uses.ToArray()},true));
            yield return Capture("Unity-Stage03-"+Screen.width+"x"+Screen.height);
        }
        SetSize(2);var restoreDeadline=Time.realtimeSinceStartup+20;
        while(Screen.width!=720 || Screen.height!=480){Assert.That(Time.realtimeSinceStartup,Is.LessThan(restoreDeadline));yield return null;}
        decor.gameObject.SetActive(false);Canvas.ForceUpdateCanvases();yield return null;yield return null;
        yield return Capture("Unity-Approved-Scene-Preserved");
        yield return new ExitPlayMode();
    }
}
