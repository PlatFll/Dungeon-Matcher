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

/// <summary>Additive prop studies on saved, approved Forest scenes. Never rebuilds or overwrites those scenes.</summary>
public sealed class ForestHybridStage04Tests
{
    const string Root = "ArtSource/ForestHybrid/Stage04";
    const string Art = "Assets/_Game/Art/ForestHybrid/Stage04/";
    [Serializable] public class Placement { public string asset, study, role; public int x, y; }
    [Serializable] public class Plan { public Placement[] placements; }
    static string ScenePath(string study) => "Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage04_" + study + ".unity";
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
            var s = new TextureImporterSettings(); i.ReadTextureSettings(s); s.spriteMeshType = SpriteMeshType.FullRect;
            s.spriteAlignment = (int)SpriteAlignment.Center; s.spritePivot = new Vector2(.5f, .5f); i.SetTextureSettings(s); i.SaveAndReimport();
        }
    }
    static void Build(string study)
    {
        Import();
        var source = "Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage0" + (study == "Upper" ? "2" : "3") + ".unity";
        var scene = EditorSceneManager.OpenScene(source, OpenSceneMode.Single);
        // Save a separate scene before making any additions. The approved serialized source stays untouched.
        Assert.That(EditorSceneManager.SaveScene(scene, ScenePath(study)), Is.True);
        var canvas = Object.FindFirstObjectByType<Canvas>(); Assert.That(canvas, Is.Not.Null);
        var group = new GameObject("Stage04 separate props", typeof(RectTransform)).GetComponent<RectTransform>();
        group.SetParent(canvas.transform, false); group.anchorMin = group.anchorMax = group.pivot = new Vector2(0, 1);
        group.anchoredPosition = Vector2.zero; group.sizeDelta = ((RectTransform)canvas.transform).rect.size;
        foreach (var p in JsonUtility.FromJson<Plan>(File.ReadAllText(Root + "/Placements.json")).placements)
        {
            if (p.study != study) continue;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + p.asset + ".png"); Assert.That(sprite, Is.Not.Null, p.asset);
            var r = new GameObject(p.asset + " " + p.role, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(group, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(p.x, -p.y); r.sizeDelta = sprite.rect.size;
            var image = r.gameObject.AddComponent<Image>(); image.sprite = sprite; image.raycastTarget = false;
        }
        Assert.That(EditorSceneManager.SaveScene(scene, ScenePath(study)), Is.True);
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath(study)), Is.False);
    }
    [MenuItem("Dungeon Matcher/Art Preview/Forest Hybrid Stage 04 upper props")]
    public static void BuildUpper() => Build("Upper");
    [MenuItem("Dungeon Matcher/Art Preview/Forest Hybrid Stage 04 board props")]
    public static void BuildBoard() => Build("Board");
    static void SetSize(int w, int h) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { new Vector2Int(w, h) });
    static IEnumerator Capture(string name)
    {
        var p = Path.GetFullPath(Root + "/Previews/" + name + ".png"); if (File.Exists(p)) File.Delete(p);
        ScreenCapture.CaptureScreenshot(p); var until = Time.realtimeSinceStartup + 20;
        while (!File.Exists(p)) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(until)); yield return null; }
    }
    [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }
    [UnityTest, Explicit("Additive prop studies, exact native geometry, unclipped canvases and on/off comparison captures.")]
    public IEnumerator PropsNativeScreens()
    {
        Directory.CreateDirectory(Root + "/Previews");
        BuildUpper(); SetSize(470, 288); yield return new EnterPlayMode(); yield return null; yield return null;
        yield return CheckAndCapture("Upper", 470, 288);
        yield return new ExitPlayMode();
        BuildBoard(); SetSize(608, 608); yield return new EnterPlayMode(); yield return null; yield return null;
        yield return CheckAndCapture("Board", 608, 608);
        yield return new ExitPlayMode();
    }
    static IEnumerator CheckAndCapture(string study, int w, int h)
    {
        var group = GameObject.Find("Stage04 separate props"); Assert.That(group, Is.Not.Null);
        foreach (int scale in new[] { 1, 2, 3 })
        {
            SetSize(w * scale, h * scale); var until = Time.realtimeSinceStartup + 20;
            while (Screen.width != w * scale || Screen.height != h * scale) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(until)); yield return null; }
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            foreach (var image in group.GetComponentsInChildren<Image>())
            {
                Assert.That(image.raycastTarget, Is.False); Assert.That(image.color, Is.EqualTo(Color.white));
                Assert.That(image.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(image.rectTransform.localScale, Is.EqualTo(Vector3.one));
                var r = GameplayPixelLayoutController.ScreenRect(image.rectTransform);
                Assert.That(r.width, Is.EqualTo(image.sprite.rect.width * scale).Within(.02), image.name);
                Assert.That(r.height, Is.EqualTo(image.sprite.rect.height * scale).Within(.02), image.name);
                Assert.That(r.x, Is.EqualTo(Mathf.Round(r.x)).Within(.02), image.name);
                Assert.That(r.y, Is.EqualTo(Mathf.Round(r.y)).Within(.02), image.name);
                Assert.That(r.xMin, Is.GreaterThanOrEqualTo(-.02)); Assert.That(r.yMin, Is.GreaterThanOrEqualTo(-.02));
                Assert.That(r.xMax, Is.LessThanOrEqualTo(Screen.width + .02)); Assert.That(r.yMax, Is.LessThanOrEqualTo(Screen.height + .02));
                if (study == "Board") Assert.That(r.Overlaps(new Rect(48 * scale, 48 * scale, 512 * scale, 512 * scale)), Is.False, image.name);
            }
            yield return Capture("Unity-" + study + "-" + Screen.width + "x" + Screen.height);
            if (scale == 1) { group.SetActive(false); yield return null; yield return null; yield return Capture("Unity-" + study + "-NoProps"); group.SetActive(true); yield return null; }
        }
        string[] forbidden = { "BoardController", "EnemyActor", "PlayerActor", "RunSession", "EventSystem" };
        foreach (var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (c != null) Assert.That(Array.IndexOf(forbidden, c.GetType().Name), Is.EqualTo(-1), c.name);
    }
}
