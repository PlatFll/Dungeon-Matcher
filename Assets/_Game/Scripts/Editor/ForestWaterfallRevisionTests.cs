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

/// <summary>Focused correction on a copy: remove two comet wisps and animate the existing water plane.</summary>
public sealed class ForestWaterfallRevisionTests
{
    const string Root = "ArtSource/ForestSurroundRevision/WaterfallCorrection";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestSurround_Waterfall.unity";
    const string BeforeScene = "Assets/_Game/Scenes/ArtPreview_ForestSurround_Living.unity";
    const string Art = "Assets/_Game/Art/ForestSurroundRevision/WaterfallCorrection/";
    [Serializable] public class Evidence
    {
        public int samplesPerVersion, removedComets, remainingTracks, waterPoses;
        public bool viewportClear, uniformNativeScale, reducedMotionStable;
    }
    static bool IsComet(LivingForestReviewMotion.Track track) => track.frames.Any(s => s.name.StartsWith("Wisp_Comet_", StringComparison.Ordinal));
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[] { new Vector2Int(720 * scale, 1360 * scale) });

    [MenuItem("Dungeon Matcher/Art Preview/Forest waterfall correction")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(BeforeScene, OpenSceneMode.Single);
        var motion = Object.FindFirstObjectByType<LivingForestReviewMotion>();
        Assert.That(motion, Is.Not.Null);
        var comets = motion.tracks.Where(IsComet).ToArray();
        Assert.That(comets.Length, Is.EqualTo(2));
        foreach (var comet in comets) comet.image.gameObject.SetActive(false);
        motion.tracks = motion.tracks.Where(t => !IsComet(t)).ToArray();
        var water = motion.tracks.Single(t => t.kind == LivingForestReviewMotion.MotionKind.Water);
        water.frames = Enumerable.Range(0, 24).Select(i => AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Waterfall_Flow_" + i.ToString("D2") + ".png")).ToArray();
        Assert.That(water.frames.All(s => s != null), Is.True);
        water.frameRate = 12;
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
        while (!File.Exists(path))
        {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline));
            yield return null;
        }
    }

    [UnityTearDown] public IEnumerator Cleanup()
    {
        if (Application.isPlaying) yield return new ExitPlayMode();
    }

    [UnityTest, Explicit("Actual matched-time Unity captures: water flow and removal of both comet placements only.")]
    public IEnumerator WaterfallReview()
    {
        EditorSceneManager.OpenScene(BeforeScene, OpenSceneMode.Single);
        SetSize(1);
        yield return new EnterPlayMode(); yield return null; yield return null;
        var before = Object.FindFirstObjectByType<LivingForestReviewMotion>();
        before.automatic = false;
        for (int n = 0; n < 96; n++)
        {
            before.Evaluate(n / 12f, false);
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            yield return Capture(".utmp/forest-waterfall/before/Frame-" + n.ToString("D3") + ".png");
        }
        yield return new ExitPlayMode();
        Build(); SetSize(1);
        yield return new EnterPlayMode(); yield return null; yield return null;
        var motion = Object.FindFirstObjectByType<LivingForestReviewMotion>();
        motion.automatic = false;
        Assert.That(motion.tracks.Length, Is.EqualTo(19));
        Assert.That(motion.tracks.Any(IsComet), Is.False);
        Assert.That(Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Any(i => i.sprite != null && i.sprite.name.StartsWith("Wisp_Comet_", StringComparison.Ordinal)), Is.False);
        var water = motion.tracks.Single(t => t.kind == LivingForestReviewMotion.MotionKind.Water);
        var poses = new HashSet<Sprite>();
        for (int n = 0; n < 96; n++)
        {
            motion.Evaluate(n / 12f, false);
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            foreach (var t in motion.tracks)
            {
                var rect = GameplayPixelLayoutController.ScreenRect(t.image.rectTransform);
                Assert.That(rect.width, Is.EqualTo(t.image.sprite.rect.width * 2).Within(.02));
                Assert.That(rect.height, Is.EqualTo(t.image.sprite.rect.height * 2).Within(.02));
                Assert.That(rect.x, Is.EqualTo(Mathf.Round(rect.x)).Within(.02));
                Assert.That(rect.y, Is.EqualTo(Mathf.Round(rect.y)).Within(.02));
                Assert.That(rect.xMin, Is.GreaterThanOrEqualTo(-.02));
                Assert.That(rect.xMax, Is.LessThanOrEqualTo(720.02));
                Assert.That(rect.yMin, Is.GreaterThanOrEqualTo(-.02));
                Assert.That(rect.yMax, Is.LessThanOrEqualTo(1360.02));
                Assert.That(t.image.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(t.image.sprite.texture.mipmapCount, Is.EqualTo(1));
                Assert.That(t.image.raycastTarget, Is.False);
            }
            poses.Add(water.image.sprite);
            yield return Capture(".utmp/forest-waterfall/after/Frame-" + n.ToString("D3") + ".png");
        }
        Assert.That(poses.Count, Is.EqualTo(24));
        // Hold independent fireflies still for the water-only loop detail.
        // Their longer fade periods intentionally do not share the water's two-second cycle.
        for (int n = 0; n < 24; n++)
        {
            motion.Evaluate(1.25f, false);
            water.image.sprite = water.frames[n];
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            yield return Capture(".utmp/forest-waterfall/water-detail/Frame-" + n.ToString("D3") + ".png");
        }
        motion.Evaluate(1.25f, false);
        Canvas.ForceUpdateCanvases(); yield return null; yield return null;
        yield return Capture(Root + "/Previews/Unity-Final-720x1360.png");
        SetSize(2);
        float deadline = Time.realtimeSinceStartup + 20;
        while (Screen.width != 1440 || Screen.height != 2720)
        {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline)); yield return null;
        }
        Canvas.ForceUpdateCanvases(); yield return null; yield return null;
        yield return Capture(Root + "/Previews/Unity-Final-1440x2720.png");
        motion.Evaluate(0, true);
        var still = water.image.sprite;
        motion.Evaluate(13, true);
        Assert.That(water.image.sprite, Is.SameAs(still));
        File.WriteAllText(Root + "/Diagnostics/UnityEvidence.json", JsonUtility.ToJson(new Evidence
        {
            samplesPerVersion = 96, removedComets = 2, remainingTracks = motion.tracks.Length,
            waterPoses = poses.Count, viewportClear = true, uniformNativeScale = true, reducedMotionStable = true
        }, true));
        yield return new ExitPlayMode();
    }
}
