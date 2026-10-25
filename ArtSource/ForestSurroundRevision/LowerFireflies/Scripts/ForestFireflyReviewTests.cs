using System;
using System.Collections;
using System.Collections.Generic;
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

/// <summary>Adds the existing warm mote to a copy of the approved forest review.</summary>
public sealed class ForestFireflyReviewTests
{
    const string Root = "ArtSource/ForestSurroundRevision/LowerFireflies";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestSurround_Fireflies.unity";
    const string BeforeScene = "Assets/_Game/Scenes/ArtPreview_ForestSurround_Waterfall.unity";
    const string Layer = "Lower forest fireflies";
    const string SpritePath = "Assets/_Game/Art/EnchantedForestStaged/Stage03/Firefly_Mote.png";
    static readonly Vector2[] Positions = {
        new Vector2(22,300), new Vector2(337,345), new Vector2(10,423), new Vector2(329,482),
        new Vector2(118,539), new Vector2(249,557), new Vector2(15,646), new Vector2(335,633)
    };
    static bool IsNew(LivingForestReviewMotion.Track t) => t.image.transform.parent.name == Layer;
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(720 * scale, 1360 * scale) });

    [MenuItem("Dungeon Matcher/Art Preview/Forest lower fireflies")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(BeforeScene, OpenSceneMode.Single);
        var motion = Object.FindFirstObjectByType<LivingForestReviewMotion>();
        Assert.That(motion, Is.Not.Null);
        Assert.That(motion.tracks.Length, Is.EqualTo(19));
        var root = motion.transform.parent;
        var board = root.Find("Board contents native 64px");
        Assert.That(board, Is.Not.Null);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        Assert.That(sprite, Is.Not.Null);
        Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(8,8)));
        var layer = new GameObject(Layer, typeof(RectTransform)).GetComponent<RectTransform>();
        layer.SetParent(root, false);
        layer.anchorMin = layer.anchorMax = layer.pivot = new Vector2(0,1);
        layer.anchoredPosition = Vector2.zero;
        layer.sizeDelta = new Vector2(360,680);
        layer.localScale = new Vector3(2,2,1);
        var hud = root.Find("Shared bottom HUD");
        Assert.That(hud, Is.Not.Null);
        layer.SetSiblingIndex(hud.GetSiblingIndex());
        var tracks = motion.tracks.ToList();
        float[] periods = { 2.6f,4.1f,3.3f,5f,2.8f,4.7f,3.8f,5.6f };
        float[] phases = { .2f,2.6f,1.8f,.6f,1.7f,3.3f,.8f,4.6f };
        float[] peaks = { .78f,.82f,.70f,.85f,.68f,.76f,.84f,.80f };
        for (int i = 0; i < Positions.Length; i++)
        {
            var image = new GameObject("Warm lower firefly " + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            var rect = image.rectTransform;
            rect.SetParent(layer, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1);
            rect.anchoredPosition = new Vector2(Positions[i].x, -Positions[i].y);
            rect.sizeDelta = sprite.rect.size;
            image.sprite = sprite;
            image.raycastTarget = false;
            tracks.Add(new LivingForestReviewMotion.Track {
                image = image, frames = new[] { sprite }, kind = LivingForestReviewMotion.MotionKind.Light,
                origin = rect.anchoredPosition, amplitude = new Vector2(i % 2 == 0 ? 2 : 1, i % 3 == 0 ? 1 : 2),
                period = periods[i], phase = phases[i], peak = peaks[i], onFraction = .46f + .04f * (i % 4),
                driftPeriod = 5.2f + i * .47f, frameRate = 1
            });
        }
        motion.tracks = tracks.ToArray();
        motion.Evaluate(1.25f, false);
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath), Is.False);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    static IEnumerator Capture(string path)
    {
        path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        if (File.Exists(path)) File.Delete(path);
        ScreenCapture.CaptureScreenshot(path);
        float deadline = Time.realtimeSinceStartup + 20;
        while (!File.Exists(path)) {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null;
        }
    }
    [UnityTearDown] public IEnumerator Cleanup() {
        if (Application.isPlaying) yield return new ExitPlayMode();
    }

    [UnityTest, Explicit("Matched-time Unity rendering of eight added lower fireflies, preserving all previous art and motion.")]
    public IEnumerator FireflyReview()
    {
        EditorSceneManager.OpenScene(BeforeScene, OpenSceneMode.Single); SetSize(1);
        yield return new EnterPlayMode(); yield return null; yield return null;
        var before = Object.FindFirstObjectByType<LivingForestReviewMotion>();
        before.automatic = false;
        for (int n=0; n<96; n++) {
            before.Evaluate(n/12f, false); Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            yield return Capture(".utmp/forest-fireflies/before/Frame-" + n.ToString("D3") + ".png");
        }
        yield return new ExitPlayMode(); Build(); SetSize(1);
        yield return new EnterPlayMode(); yield return null; yield return null;
        var motion = Object.FindFirstObjectByType<LivingForestReviewMotion>();
        motion.automatic = false;
        Assert.That(motion.tracks.Length, Is.EqualTo(27));
        var added = motion.tracks.Where(IsNew).ToArray(); Assert.That(added.Length, Is.EqualTo(8));
        var positions = Enumerable.Range(0,8).Select(i => new HashSet<Vector2>()).ToArray();
        var minAlpha = Enumerable.Repeat(1f,8).ToArray(); var maxAlpha = new float[8];
        var root = motion.transform.parent;
        var protectedRects = new[] {
            new Rect(104,1360-1040,512,512), new Rect(262,1360-1136,196,36),
            new Rect(236,1360-1248,248,88), new Rect(112,1360-1248,90,88),
            new Rect(522,1360-1248,88,88), new Rect(220,1360-1308,281,30)
        };
        Assert.That(added[0].image.transform.parent.GetSiblingIndex(), Is.GreaterThan(root.Find("Stage04 forest board frame 2x").GetSiblingIndex()));
        Assert.That(added[0].image.transform.parent.GetSiblingIndex(), Is.LessThan(root.Find("Shared bottom HUD").GetSiblingIndex()));
        for (int n=0; n<96; n++) {
            motion.Evaluate(n/12f, false); Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            for (int i=0; i<8; i++) {
                var t=added[i]; var rect=GameplayPixelLayoutController.ScreenRect(t.image.rectTransform);
                Assert.That(rect.width, Is.EqualTo(16).Within(.02));
                Assert.That(rect.height, Is.EqualTo(16).Within(.02));
                Assert.That(rect.x, Is.EqualTo(Mathf.Round(rect.x)).Within(.02));
                Assert.That(rect.y, Is.EqualTo(Mathf.Round(rect.y)).Within(.02));
                Assert.That(rect.xMin, Is.GreaterThanOrEqualTo(0)); Assert.That(rect.xMax, Is.LessThanOrEqualTo(720));
                Assert.That(rect.yMin, Is.GreaterThanOrEqualTo(0)); Assert.That(rect.yMax, Is.LessThanOrEqualTo(1360-480));
                Assert.That(protectedRects.Any(r => r.Overlaps(rect)), Is.False, t.image.name);
                Assert.That(t.image.raycastTarget, Is.False);
                Assert.That(t.image.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(t.image.sprite.texture.mipmapCount, Is.EqualTo(1));
                positions[i].Add(t.image.rectTransform.anchoredPosition);
                minAlpha[i]=Mathf.Min(minAlpha[i],t.image.color.a); maxAlpha[i]=Mathf.Max(maxAlpha[i],t.image.color.a);
            }
            yield return Capture(".utmp/forest-fireflies/after/Frame-" + n.ToString("D3") + ".png");
        }
        for(int i=0;i<8;i++) {
            Assert.That(positions[i].Count, Is.GreaterThan(2));
            Assert.That(minAlpha[i], Is.EqualTo(0)); Assert.That(maxAlpha[i], Is.GreaterThan(.6f));
        }
        motion.Evaluate(1.25f,false); Canvas.ForceUpdateCanvases(); yield return null; yield return null;
        yield return Capture(Root + "/Previews/Unity-Final-720x1360.png");
        SetSize(2); float deadline=Time.realtimeSinceStartup+20;
        while(Screen.width!=1440||Screen.height!=2720) {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null;
        }
        Canvas.ForceUpdateCanvases(); yield return null; yield return null;
        yield return Capture(Root + "/Previews/Unity-Final-1440x2720.png");
        motion.Evaluate(0,true);
        var stillPositions=added.Select(t=>t.image.rectTransform.anchoredPosition).ToArray();
        var stillAlphas=added.Select(t=>t.image.color.a).ToArray(); motion.Evaluate(23,true);
        for(int i=0;i<8;i++) {
            Assert.That(added[i].image.rectTransform.anchoredPosition, Is.EqualTo(stillPositions[i]));
            Assert.That(added[i].image.color.a, Is.EqualTo(stillAlphas[i]));
        }
        File.WriteAllText(Root+"/Diagnostics/UnityEvidence.json", "{\"result\":\"Passed\",\"addedFireflies\":8,\"totalTracks\":27,\"samplesPerVersion\":96,\"nativeScale\":2,\"uiClear\":true,\"reducedMotionStable\":true}");
        yield return new ExitPlayMode();
    }
}
