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
public sealed class ForestReworkV2Checkpoint03Tests
{
    const string Root = "ArtSource/ForestReworkV2/Checkpoint03";
    const string Art = "Assets/_Game/Art/ForestReworkV2/Checkpoint03/";
    const string OldArt = "Assets/_Game/Art/ForestHybrid/Stage02/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint03.unity";
    const int Width = 720, Height = 480;
    static readonly string[] Names = { "TopLeftCorner", "TopRightCorner", "BottomLeftCorner", "BottomRightCorner", "TopEdge", "BottomEdge", "LeftEdge", "RightEdge" };
    static readonly string[] Files = { "Corner_TL", "Corner_TR", "Corner_BL", "Corner_BR", "Rail_Top", "Rail_Bottom", "Rail_Left", "Rail_Right" };
    [Serializable] public class Use { public string name, path; public Rect rect; public float scale; }
    [Serializable] public class Evidence { public int width, height; public Use[] images; public Vector2[] frameCases; }
    static Sprite Load(string p) { var s = AssetDatabase.LoadAssetAtPath<Sprite>(p); Assert.That(s, Is.Not.Null, p); return s; }
    static Transform Portrait(Transform root) => root.Find("Approved forest top modules 2x/ForestPortrait");
    static void Revision(Transform root, bool enabled)
    {
        var panel = Portrait(root); var frame = panel.Find("ModularFrame");
        for (int n = 0; n < Names.Length; n++) frame.Find(Names[n]).GetComponent<Image>().sprite = Load((enabled ? Art : OldArt) + "Portrait_" + Files[n] + ".png");
        frame.GetComponent<ResponsiveModularFrameFitter>().ConfigureNativePieces(Load((enabled ? Art + "Portrait_Join" : OldArt + "Portrait_Clamp") + ".png"), enabled ? 90 : 58);
        panel.Find("Upper ivy accent").gameObject.SetActive(!enabled); panel.Find("Lower ivy accent").gameObject.SetActive(!enabled);
        panel.Find("Upper connected vine").gameObject.SetActive(enabled); panel.Find("Lower connected vine").gameObject.SetActive(enabled);
    }
    static void Vine(Transform panel, string name, string file, bool lower)
    {
        var r = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>(); r.SetParent(panel, false);
        r.anchorMin = r.anchorMax = lower ? new Vector2(1, 0) : new Vector2(0, 1); r.pivot = new Vector2(0, 1);
        r.anchoredPosition = lower ? new Vector2(-14, 28) : new Vector2(10, 7);
        var i = r.GetComponent<Image>(); i.sprite = Load(Art + file + ".png"); i.raycastTarget = false; r.sizeDelta = i.sprite.rect.size;
    }
    [MenuItem("Dungeon Matcher/Art Preview/Forest rework v2 checkpoint 03")]
    public static void Build()
    {
        // Save a new scene path; never invoke an earlier stage's builder or save its scene.
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint02.unity", OpenSceneMode.Single);
        var root = Object.FindFirstObjectByType<Canvas>().transform; root.name = "Forest v2 checkpoint 03 art-only portrait";
        Vine(Portrait(root), "Upper connected vine", "Portrait_Vine_Upper", false);
        Vine(Portrait(root), "Lower connected vine", "Portrait_Vine_Lower", true);
        Revision(root, true);
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
    [UnityTest, Explicit("Checkpoint three native portrait joins, resize and mask review.")]
    public IEnumerator PortraitReview()
    {
        Directory.CreateDirectory(Root + "/Previews"); Directory.CreateDirectory(Root + "/Diagnostics");
        Build(); SetSize(1); yield return new EnterPlayMode(); yield return null; yield return null;
        yield return CaptureAndCheck(); yield return new ExitPlayMode();
    }
    static IEnumerator CaptureAndCheck()
    {
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        Revision(root, false); Canvas.ForceUpdateCanvases(); yield return null; yield return null;
        yield return Screenshot("Unity-Before-720x480"); Revision(root, true); CheckResizing(root);
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
        SetSize(1); yield return null; yield return null;
        var mask = Portrait(root).Find("InteriorMask"); var backing = mask.Find("OverfilledBacking").GetComponent<Image>();
        backing.color = Color.magenta; yield return null; yield return null; yield return Screenshot("Unity-MaskProbe-720x480"); backing.color = Color.white;
        yield return null; yield return null;
    }
}
