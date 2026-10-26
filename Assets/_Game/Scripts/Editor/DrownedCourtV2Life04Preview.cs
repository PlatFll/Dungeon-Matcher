using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Review-only revision: previous art, clips, scenes and production bindings remain intact.
public static class DrownedCourtV2Life04Preview
{
    const string Root = "ArtSource/DrownedCourtEnvironmentV2";
    const string Output = Root + "/Preview/Revision04";
    const string Art = "Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision04";
    const string Motion = Art + "/Motion";
    const string Scene = "Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision04.unity";
    static Canvas canvas;
    static Camera camera;
    [MenuItem("Dungeon Matcher/Art/Drowned Court V2/Build Life Revision 04")]
    public static void Build()
    {
        Assert.That(Application.isPlaying, Is.False);
        Directory.CreateDirectory(Output + "/Motion"); Directory.CreateDirectory(Root + "/QA/Revision04");
        Import(); Directory.CreateDirectory(Motion);
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision03.unity");
        canvas = Object.FindFirstObjectByType<Canvas>(); camera = Object.FindFirstObjectByType<Camera>();
        Sample(0);
        var rects = canvas.GetComponentsInChildren<RectTransform>(true).ToDictionary(r => r, Record);
        var bindings = canvas.GetComponentsInChildren<Image>(true).ToDictionary(im => im, im => im.sprite);
        var changed = new HashSet<Image>();
        foreach (int scale in new[] {1, 2, 3}) Render(scale, "BeforeClosed");
        Sample(5.9f); foreach (int scale in new[] {1, 2, 3}) Render(scale, "BeforeOpen"); Sample(0);

        int tiles = 0;
        foreach (var course in new[] {"Front", "Rear"})
        {
            string root = course == "Front" ? "Stage03_Platform_Candidates" : "Revision03_RearWalkway";
            var group = canvas.transform.Find(root); Assert.That(group, Is.Not.Null);
            foreach (var im in group.GetComponentsInChildren<Image>().Where(i => i.sprite && i.sprite.name.Contains("FloorTop")))
            {
                string suffix = im.sprite.name.Substring(im.sprite.name.IndexOf("FloorTop", StringComparison.Ordinal) + 8);
                im.sprite = Sprite("Floor" + course + suffix); changed.Add(im); tiles++;
            }
        }
        Assert.That(tiles, Is.EqualTo(16));

        var clam = canvas.transform.Find("Revision02_Ambient/Clam").GetComponent<Image>();
        Assert.That(clam, Is.Not.Null); changed.Add(clam);
        clam.rectTransform.sizeDelta = new Vector2(64, 64);
        clam.rectTransform.anchoredPosition = new Vector2(31, 0);
        var clamClip = new AnimationClip {name = "ClamHinge", frameRate = 12};
        var timing = JsonUtility.FromJson<ClamTiming>(File.ReadAllText(Root + "/Native/Revision04/ClamTiming.json"));
        Assert.That(timing.keys.Length, Is.GreaterThan(4));
        AnimationUtility.SetObjectReferenceCurve(clamClip, EditorCurveBinding.PPtrCurve("", typeof(Image), "m_Sprite"), timing.keys.Select(k => Key(k.time, k.sprite)).ToArray());
        Bind(clam, clamClip, 12);

        int corals = 0;
        foreach (string colony in new[] {"ColumnLeftA", "ColumnRightB"})
        foreach (var im in bindings.Keys.Where(i => i.sprite && i.sprite.name == colony))
        {
            changed.Add(im); corals++;
            bool right = colony == "ColumnRightB"; float duration = right ? 24 : 12, start = right ? 11.5f : 2.5f;
            var clip = new AnimationClip {name = colony + "Current", frameRate = 12};
            float[] times = {0, start, start + .45f, start + .9f, start + 1.35f, start + 1.8f, start + 2.25f, start + 2.7f, duration - 1f / 12};
            int[] poses = {0, 0, 1, 0, 2, 0, 1, 0, 0};
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(Image), "m_Sprite"), times.Select((t, i) => Key(t, colony + "Sway" + (char)('A' + poses[i]))).ToArray());
            Bind(im, clip, duration);
        }
        Assert.That(corals, Is.EqualTo(2));
        Sample(0);
        foreach (var pair in rects) if (pair.Key != clam.rectTransform) Assert.That(Record(pair.Key), Is.EqualTo(pair.Value), pair.Key.name + " layout");
        foreach (var pair in bindings) if (!changed.Contains(pair.Key)) Assert.That(pair.Key.sprite, Is.SameAs(pair.Value), pair.Key.name + " source art");
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None), Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == Scene), Is.False);
        AssetDatabase.SaveAssets(); Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene, Scene), Is.True);
        foreach (int scale in new[] {1, 2, 3}) Render(scale, "RevisedClosed");
        Sample(5.9f); foreach (int scale in new[] {1, 2, 3}) Render(scale, "RevisedOpen");
        for (int f = 0; f < 288; f++) {Sample(f / 12f); Capture(360, 640, Output + "/Motion/Frame-" + f.ToString("000") + ".png");}
        Sample(0); Capture(360, 640, Output + "/LoopStart.png"); Sample(24); Capture(360, 640, Output + "/LoopEnd.png");
        File.WriteAllText(Root + "/QA/Revision04/UnityPreview.json", JsonUtility.ToJson(new Result {status = "PASS", floorTiles = tiles, coralTracks = corals, preservedRects = rects.Count - 1, preservedBindings = bindings.Count - changed.Count, loopFrames = 288, note = "Actual Unity art-only scene and native clip samples. Production scene, runtime code, gameplay and previous artwork unchanged."}, true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); EditorSceneManager.OpenScene(Scene);
        Debug.Log("DROWNED_COURT_LIFE04_PASS");
    }
    static ObjectReferenceKeyframe Key(float t, string name) => new ObjectReferenceKeyframe {time = t, value = Sprite(name)};
    static void Bind(Image image, AnimationClip clip, float duration)
    {
        var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true; settings.startTime = 0; settings.stopTime = duration; AnimationUtility.SetAnimationClipSettings(clip, settings);
        Assert.That(clip.length, Is.EqualTo(duration).Within(.001f));
        string path = Motion + "/" + clip.name + ".anim"; var prior = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (prior) {EditorUtility.CopySerialized(clip, prior); Object.DestroyImmediate(clip); clip = prior;} else AssetDatabase.CreateAsset(clip, path);
        string cp = Motion + "/" + clip.name + ".controller"; var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(cp);
        if (!ctrl) ctrl = AnimatorController.CreateAnimatorControllerAtPath(cp);
        var machine = ctrl.layers[0].stateMachine; var state = machine.states.Length > 0 ? machine.states[0].state : machine.AddState("Ambient"); state.motion = clip; machine.defaultState = state;
        var animator = image.GetComponent<Animator>(); if (!animator) animator = image.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = ctrl; animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }
    static string Record(RectTransform r) => r.anchoredPosition + "|" + r.sizeDelta + "|" + r.localScale + "|" + r.anchorMin + "|" + r.anchorMax + "|" + r.pivot;
    static Sprite Sprite(string name) {var s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "/" + name + ".png"); Assert.That(s, Is.Not.Null, name); return s;}
    static void Sample(float time) {foreach (var a in canvas.GetComponentsInChildren<Animator>()) {var clip = a.runtimeAnimatorController.animationClips.Single(); clip.SampleAnimation(a.gameObject, Mathf.Repeat(time, clip.length));} Canvas.ForceUpdateCanvases();}
    static void Render(int scale, string phase) => Capture(360 * scale, 640 * scale, Output + "/Unity-" + phase + "-" + (360 * scale) + "x" + (640 * scale) + ".png");
    static void Capture(int w, int h, string path)
    {
        Canvas.ForceUpdateCanvases(); var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32); rt.filterMode = FilterMode.Point; rt.antiAliasing = 1; var prior = RenderTexture.active;
        camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt; var tex = new Texture2D(w, h, TextureFormat.RGBA32, false); tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG()); camera.targetTexture = null; RenderTexture.active = prior; RenderTexture.ReleaseTemporary(rt); Object.DestroyImmediate(tex);
    }
    static void Import()
    {
        Directory.CreateDirectory(Art);
        foreach (var file in Directory.GetFiles(Root + "/Exports/Revision04", "*.png").Where(p => !Path.GetFileName(p).StartsWith("Underlay") && Path.GetFileName(p) != "ClamClosed.png"))
        {
            string p = Art + "/" + Path.GetFileName(file); File.Copy(file, p, true); AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceSynchronousImport);
            var ti = (TextureImporter)AssetImporter.GetAtPath(p); ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single; ti.spritePixelsPerUnit = 64; ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed; ti.mipmapEnabled = false; ti.npotScale = TextureImporterNPOTScale.None; ti.alphaSource = TextureImporterAlphaSource.FromInput; ti.alphaIsTransparency = true; ti.isReadable = true; ti.wrapMode = TextureWrapMode.Clamp;
            var st = new TextureImporterSettings(); ti.ReadTextureSettings(st); st.spriteMeshType = SpriteMeshType.FullRect; ti.SetTextureSettings(st); ti.SaveAndReimport();
            var expected = new Texture2D(2, 2); expected.LoadImage(File.ReadAllBytes(file)); var actual = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            Assert.That(actual.width, Is.EqualTo(expected.width)); Assert.That(actual.height, Is.EqualTo(expected.height));
            var aa = actual.GetPixels32(); var bb = expected.GetPixels32(); for (int i = 0; i < aa.Length; i++) Assert.That(aa[i].a == bb[i].a && (bb[i].a == 0 || aa[i].Equals(bb[i])), Is.True, p);
            Object.DestroyImmediate(expected);
        }
    }
    [Serializable] class Result {public string status, note; public int floorTiles, coralTracks, preservedRects, preservedBindings, loopFrames;}
    [Serializable] class ClamTiming {public SpriteKey[] keys;}
    [Serializable] class SpriteKey {public float time; public string sprite;}
}
