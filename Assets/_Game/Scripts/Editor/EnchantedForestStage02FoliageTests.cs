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

/// <summary>Portrait-only review on the approved Forest background. No production binding changes.</summary>
public sealed class EnchantedForestStage02FoliageTests
{
    const string Root = "ArtSource/EnchantedForestStaged/Stage02/FoliageRevision";
    const string Art = "Assets/_Game/Art/EnchantedForestStaged/Stage02/FoliageRevision/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage02_Foliage.unity";
    const int Width = 720, Height = 480;
    static readonly string[] Names = { "TopLeftCorner", "TopRightCorner", "BottomLeftCorner", "BottomRightCorner", "TopEdge", "BottomEdge", "LeftEdge", "RightEdge" };
    static readonly string[] Files = { "Corner_TL", "Corner_TR", "Corner_BL", "Corner_BR", "Rail_Top", "Rail_Bottom", "Rail_Left", "Rail_Right" };
    [Serializable] public class Use { public string name, path; public Rect rect; public float scale; }
    [Serializable] public class Evidence { public int width, height; public Use[] images; public Vector2[] frameCases; }
    static Sprite Load(string p) { var s = AssetDatabase.LoadAssetAtPath<Sprite>(p); Assert.That(s, Is.Not.Null, p); return s; }
    static Transform Portrait(Transform root) => root.Find("Approved forest top modules 2x/ForestPortrait");
    static void Vine(Transform panel, string file, Vector2 anchor, Vector2 offset)
    {
        var r = new GameObject(file, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        r.SetParent(panel, false); r.anchorMin = r.anchorMax = anchor; r.pivot = new Vector2(0, 1);
        r.anchoredPosition = offset;
        var image = r.GetComponent<Image>(); image.sprite = Load(Art + file + ".png");
        image.raycastTarget = false; r.sizeDelta = image.sprite.rect.size;
    }
    [MenuItem("Dungeon Matcher/Art Preview/Enchanted Forest staged 02 fuller foliage")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage02.unity", OpenSceneMode.Single);
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        root.name = "Enchanted Forest stage 02 fuller foliage review";
        var panel = (RectTransform)Portrait(root);
        Assert.That(panel.rect.size, Is.EqualTo(new Vector2(98, 220)));
        foreach (var name in new[] { "Portrait_Ivy_UpperLeft", "Portrait_Ivy_UpperRight", "Portrait_Ivy_LeftReturn", "Portrait_Ivy_LowerRight" })
            panel.Find(name).gameObject.SetActive(false);
        Vine(panel, "Ivy_CrownLeft", new Vector2(0, 1), new Vector2(-8, 8));
        Vine(panel, "Ivy_LeftSwag", new Vector2(0, 1), new Vector2(-8, -17));
        Vine(panel, "Ivy_CrownRight", new Vector2(1, 1), new Vector2(-57, 8));
        Vine(panel, "Ivy_RightSwag", new Vector2(1, 0), new Vector2(-57, 157));
        // Existing masked contents retain their exact hierarchy and geometry.
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath), Is.False);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(Width * scale, Height * scale) });
    static IEnumerator Screenshot(string name)
    {
        var path = Path.GetFullPath(Root + "/Previews/" + name + ".png"); if (File.Exists(path)) File.Delete(path);
        ScreenCapture.CaptureScreenshot(path); var deadline = Time.realtimeSinceStartup + 20;
        while (!File.Exists(path)) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null; }
    }
    static readonly Vector2[] Cases = { new Vector2(30, 30), new Vector2(74, 180), new Vector2(98, 220), new Vector2(129, 257), new Vector2(163, 287) };
    static void CheckResizing(Transform root)
    {
        var panel = (RectTransform)Portrait(root); var original = panel.sizeDelta;
        var f = panel.Find("ModularFrame").GetComponent<ResponsiveModularFrameFitter>(); var mask = (RectTransform)panel.Find("InteriorMask");
        foreach (var size in Cases)
        {
            panel.sizeDelta = size; Canvas.ForceUpdateCanvases(); f.RefreshFrame();
            Assert.That(mask.rect.size, Is.EqualTo(size - Vector2.one * 24));
            Assert.That(((RectTransform)mask.Find("OverfilledBacking")).rect.size, Is.EqualTo(mask.rect.size + Vector2.one * 64));
            foreach (var vine in panel.GetComponentsInChildren<Image>().Where(i => i.name.StartsWith("Ivy_")))
            {
                var lowerLeft = panel.InverseTransformPoint(vine.rectTransform.TransformPoint(vine.rectTransform.rect.min));
                Assert.That(lowerLeft.x, Is.EqualTo(Mathf.Round(lowerLeft.x)).Within(.001), vine.name);
                Assert.That(lowerLeft.y, Is.EqualTo(Mathf.Round(lowerLeft.y)).Within(.001), vine.name);
                Assert.That(vine.rectTransform.rect.size, Is.EqualTo(vine.sprite.rect.size));
            }
            foreach (var i in f.GetComponentsInChildren<Image>())
            {
                var r = i.rectTransform; Assert.That(i.enabled, Is.True); Assert.That(r.localScale, Is.EqualTo(Vector3.one));
                Assert.That(r.anchoredPosition.x, Is.EqualTo(Mathf.Round(r.anchoredPosition.x)).Within(.001));
                Assert.That(r.anchoredPosition.y, Is.EqualTo(Mathf.Round(r.anchoredPosition.y)).Within(.001));
                if (i.type == Image.Type.Simple) Assert.That(r.rect.size, Is.EqualTo(i.sprite.rect.size));
                else Assert.That(i.pixelsPerUnit * i.pixelsPerUnitMultiplier, Is.EqualTo(1).Within(.001));
            }
            Assert.That(((RectTransform)f.transform.Find("LeftEdge")).anchoredPosition.x + 4, Is.EqualTo(7));
            Assert.That(((RectTransform)f.transform.Find("RightEdge")).anchoredPosition.x + 4, Is.EqualTo(size.x - 8));
            Assert.That(-((RectTransform)f.transform.Find("TopEdge")).anchoredPosition.y + 4, Is.EqualTo(7));
            Assert.That(-((RectTransform)f.transform.Find("BottomEdge")).anchoredPosition.y + 4, Is.EqualTo(size.y - 8));
            foreach (RectTransform j in f.transform.Find("NativeConnectors")) if (j.gameObject.activeSelf)
            {
                var c = j.anchoredPosition + new Vector2(3, -3);
                Assert.That(Mathf.Approximately(c.x, 7) || Mathf.Approximately(c.x, size.x - 8) || Mathf.Approximately(-c.y, 7) || Mathf.Approximately(-c.y, size.y - 8), Is.True);
            }
        }
        panel.sizeDelta = new Vector2(29, 29); f.RefreshFrame(); Assert.That(f.GetComponentsInChildren<Image>().All(i => !i.enabled), Is.True);
        panel.sizeDelta = original; Canvas.ForceUpdateCanvases(); f.RefreshFrame();
    }
    [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }
    [UnityTest, Explicit("Stage 2 fuller source foliage; preserve timber, recess, shared sizing and approved environment.")]
    public IEnumerator PortraitReview()
    {
        Directory.CreateDirectory(Root + "/Previews"); Directory.CreateDirectory(Root + "/Diagnostics");
        Build(); SetSize(1); yield return new EnterPlayMode(); yield return null; yield return null;
        yield return CaptureAndCheck(); yield return new ExitPlayMode();
    }
    static IEnumerator CaptureAndCheck()
    {
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        CheckResizing(root);
        foreach (int scale in new[] { 1, 2 })
        {
            SetSize(scale); var deadline = Time.realtimeSinceStartup + 20;
            while (Screen.width != Width * scale || Screen.height != Height * scale) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null; }
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            var uses = new List<Use>();
            foreach (var im in Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i => i.enabled))
            {
                Assert.That(im.color, Is.EqualTo(Color.white), im.name); Assert.That(im.raycastTarget, Is.False, im.name);
                Assert.That(im.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point), im.name);
                var r = GameplayPixelLayoutController.ScreenRect(im.rectTransform); float factor = im.transform.lossyScale.x / root.lossyScale.x * scale;
                Assert.That(factor, Is.EqualTo(Mathf.Round(factor)).Within(.001), im.name); Assert.That(im.transform.lossyScale.x, Is.EqualTo(im.transform.lossyScale.y), im.name);
                Assert.That(r.x, Is.EqualTo(Mathf.Round(r.x)).Within(.02), im.name); Assert.That(r.y, Is.EqualTo(Mathf.Round(r.y)).Within(.02), im.name);
                if (im.type == Image.Type.Simple)
                {
                    Assert.That(r.width, Is.EqualTo(im.sprite.rect.width * factor).Within(.02), im.name); Assert.That(r.height, Is.EqualTo(im.sprite.rect.height * factor).Within(.02), im.name);
                    Assert.That(r.xMin, Is.GreaterThanOrEqualTo(-.02), im.name); Assert.That(r.yMin, Is.GreaterThanOrEqualTo(-.02), im.name);
                    Assert.That(r.xMax, Is.LessThanOrEqualTo(Screen.width + .02), im.name); Assert.That(r.yMax, Is.LessThanOrEqualTo(Screen.height + .02), im.name);
                }
                uses.Add(new Use { name = im.name, path = AssetDatabase.GetAssetPath(im.sprite), rect = r, scale = factor });
            }
            foreach (var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) { t.ForceMeshUpdate(); Assert.That(t.isTextTruncated, Is.False, t.text); }
            foreach (var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (c != null) Assert.That(new[] { "BoardController", "EnemyActor", "PlayerActor", "RunSession", "EventSystem" }, Does.Not.Contain(c.GetType().Name));
            File.WriteAllText(Root + "/Diagnostics/Unity-" + Screen.width + "x" + Screen.height + ".json", JsonUtility.ToJson(new Evidence { width = Screen.width, height = Screen.height, images = uses.ToArray(), frameCases = Cases }, true));
            yield return Screenshot("Unity-After-" + Screen.width + "x" + Screen.height);
        }
        SetSize(1); var maskDeadline = Time.realtimeSinceStartup + 20;
        while (Screen.width != Width || Screen.height != Height) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(maskDeadline)); yield return null; }
        Canvas.ForceUpdateCanvases(); yield return null; yield return null;
        var mask = Portrait(root).Find("InteriorMask"); var backing = mask.Find("OverfilledBacking").GetComponent<Image>();
        backing.color = Color.magenta; yield return null; yield return null; yield return Screenshot("Unity-MaskProbe-720x480"); backing.color = Color.white;
        yield return null; yield return null;
    }
}
