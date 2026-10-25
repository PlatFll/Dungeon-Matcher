using System;
using System.Collections;
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

/// <summary>Stage-one separator review. Keeps the existing scene and every approved source asset intact.</summary>
public sealed class ForestSeparatorReviewTests
{
    const string Root = "ArtSource/ForestSurroundRevision/PolishStage01";
    const string Art = "Assets/_Game/Art/ForestSurroundRevision/PolishStage01/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestSurround_StoneSeparator.unity";
    const string Layer = "Mudstone separator native tiles";
    const string Environment = "Approved forest top modules 2x/Stage01 environment";
    [Serializable] public class Use { public string name, path; public Rect rect; }
    [Serializable] public class Evidence { public int width, height, preservedImages; public Use[] tiles; }

    [MenuItem("Dungeon Matcher/Art Preview/Forest stone separator")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestSurround_UnifiedCorners.unity", OpenSceneMode.Single);
        var root = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>()).Single().transform;
        var environment = root.Find(Environment);
        Assert.That(environment, Is.Not.Null);
        var layer = new GameObject(Layer, typeof(RectTransform)).GetComponent<RectTransform>();
        layer.SetParent(environment, false);
        layer.anchorMin = layer.anchorMax = layer.pivot = new Vector2(0, 1);
        layer.anchoredPosition = new Vector2(0, -224);
        layer.sizeDelta = new Vector2(360, 16);
        // This stays inside the background hierarchy. Portrait foliage, actors and
        // health bars keep their existing foreground order and placement.
        string[] rhythm = { "A", "B", "C", "B", "A", "C", "A", "B", "C" };
        for (int i = 0; i < rhythm.Length; i++)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Mudstone_" + rhythm[i] + ".png");
            Assert.That(sprite, Is.Not.Null);
            Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(40, 16)));
            var piece = new GameObject("Mudstone block " + i, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            piece.SetParent(layer, false);
            piece.anchorMin = piece.anchorMax = piece.pivot = new Vector2(0, 1);
            piece.anchoredPosition = new Vector2(i * 40, 0);
            piece.sizeDelta = sprite.rect.size;
            var image = piece.GetComponent<Image>(); image.sprite = sprite; image.raycastTarget = false;
        }
        Canvas.ForceUpdateCanvases();
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath), Is.False);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(720 * scale, 1360 * scale) });
    static Rect Bounds(Transform t) => GameplayPixelLayoutController.ScreenRect((RectTransform)t);
    static IEnumerator Capture(string name)
    {
        var path = Path.GetFullPath(Root + "/Previews/" + name + ".png");
        if (File.Exists(path)) File.Delete(path);
        ScreenCapture.CaptureScreenshot(path);
        var deadline = Time.realtimeSinceStartup + 20;
        while (!File.Exists(path)) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null; }
    }
    [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }

    [UnityTest, Explicit("Capture modular separator at 2x/4x native pixels and preserve existing UI and sprite placement.")]
    public IEnumerator SeparatorReview()
    {
        Directory.CreateDirectory(Root + "/Diagnostics"); Directory.CreateDirectory(Root + "/Previews");
        Build(); SetSize(1); yield return new EnterPlayMode(); yield return null; yield return null;
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        var layer = root.Find(Environment + "/" + Layer);
        layer.gameObject.SetActive(false); Canvas.ForceUpdateCanvases(); yield return null; yield return null;
        var oldImages = Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i => i.enabled).ToArray();
        var sprites = oldImages.Select(i => i.sprite).ToArray();
        var positions = oldImages.Select(i => Bounds(i.transform)).ToArray();
        yield return Capture("Unity-Before-720x1360");
        layer.gameObject.SetActive(true); Canvas.ForceUpdateCanvases();
        for (int i = 0; i < oldImages.Length; i++)
        {
            Assert.That(oldImages[i].sprite, Is.SameAs(sprites[i]), oldImages[i].name);
            Assert.That(Bounds(oldImages[i].transform), Is.EqualTo(positions[i]), oldImages[i].name);
        }
        foreach (int scale in new[] { 1, 2 })
        {
            SetSize(scale); var deadline = Time.realtimeSinceStartup + 20;
            while (Screen.width != 720 * scale || Screen.height != 1360 * scale)
            { Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null; }
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            var board = (RectTransform)root.Find("Board contents native 64px");
            Assert.That(board.rect.size, Is.EqualTo(new Vector2(512, 512)));
            Assert.That(board.anchoredPosition, Is.EqualTo(new Vector2(104, -528)));
            var images = layer.GetComponentsInChildren<Image>(); Assert.That(images.Length, Is.EqualTo(9));
            for (int i = 0; i < images.Length; i++)
            {
                var im = images[i]; var bounds = Bounds(im.transform);
                Assert.That(im.color, Is.EqualTo(Color.white)); Assert.That(im.raycastTarget, Is.False);
                Assert.That(im.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(im.sprite.texture.mipmapCount, Is.EqualTo(1));
                Assert.That(bounds.xMin, Is.EqualTo(i * 80 * scale).Within(.02));
                Assert.That(bounds.yMin, Is.EqualTo((1360 - 480) * scale).Within(.02));
                Assert.That(bounds.width, Is.EqualTo(80 * scale).Within(.02));
                Assert.That(bounds.height, Is.EqualTo(32 * scale).Within(.02));
            }
            foreach (var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            { t.ForceMeshUpdate(); Assert.That(t.isTextTruncated, Is.False, t.text); }
            var evidence = new Evidence { width = Screen.width, height = Screen.height, preservedImages = oldImages.Length,
                tiles = images.Select(i => new Use { name = i.name, path = AssetDatabase.GetAssetPath(i.sprite), rect = Bounds(i.transform) }).ToArray() };
            File.WriteAllText(Root + "/Diagnostics/Unity-" + Screen.width + "x" + Screen.height + ".json", JsonUtility.ToJson(evidence, true));
            yield return Capture("Unity-Separator-" + Screen.width + "x" + Screen.height);
        }
        yield return new ExitPlayMode();
    }
}
