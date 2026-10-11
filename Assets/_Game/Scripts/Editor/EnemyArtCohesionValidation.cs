using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Opt-in checks for the expanded enemy sprite set and its real UI floor.</summary>
[InitializeOnLoad]
public static class EnemyArtCohesionValidation
{
    private const string Key = "DungeonMatcher.EnemyArtCohesionValidation";
    private const string ManifestPath = "ArtSource/EnemyArtCohesion/unity-import-manifest.json";
    private static readonly string Output = Path.GetFullPath(".utmp/EnemyArtCohesion/Unity");
    private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    private static readonly MethodInfo Fit = typeof(PixelPerfectBattleCharacterUI).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
    private static Manifest manifest;
    private static IDisposable profile, selection;
    private static string failure;
    private static int assertions, poses, result;

    [Serializable] private class Manifest { public TextureRow[] textures; public ClipRow[] clips; public ActorRow[] actors; }
    [Serializable] private class TextureRow { public string actor, path, sha256; public int width, height, ground; public SpriteRow[] sprites; }
    [Serializable] private class SpriteRow { public string fileID, name; public int x, y; public int[] bounds; }
    [Serializable] private class ClipRow { public string path, sha256; }
    [Serializable] private class ActorRow { public string id, definition, controller; public string[] states; }

    static EnemyArtCohesionValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
                failure = null; result = 0; poses = 0; Steps.Clear();
                Application.logMessageReceived += Log;
                Steps.Push(Cases()); EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Key, false); profile?.Dispose(); selection?.Dispose();
                EditorApplication.Exit(result);
            }
        };
    }

    public static void RunAssets()
    {
        Directory.CreateDirectory(Output); assertions = 0;
        manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
        Check(manifest.textures.Length == 395 && manifest.actors.Length == 64, "complete production manifest");
        int count = 0;
        foreach (var row in manifest.textures)
        {
            Check(Hash(row.path) == row.sha256, row.path + " exact native PNG");
            var importer = AssetImporter.GetAtPath(row.path) as TextureImporter;
            Check(importer != null && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled &&
                importer.textureCompression == TextureImporterCompression.Uncompressed && importer.npotScale == TextureImporterNPOTScale.None,
                row.path + " lossless point import");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(row.path);
            Check(texture.width == row.width * row.sprites.Length && texture.height == row.height, row.path + " GPU dimensions");
            var sprites = AssetDatabase.LoadAllAssetsAtPath(row.path).OfType<Sprite>().ToDictionary(LocalID);
            Check(sprites.Count == row.sprites.Length, row.path + " no missing or extra sprite subassets");
            foreach (var expected in row.sprites)
            {
                Check(sprites.TryGetValue(expected.fileID, out var sprite), row.path + " preserved fileID " + expected.fileID);
                Check(sprite.rect == new Rect(expected.x, expected.y, row.width, row.height), sprite.name + " fixed full rectangle");
                Check(Vector2.Distance(sprite.pivot, new Vector2(row.width * .5f, row.ground)) < .0001f, sprite.name + " fixed floor pivot");
                Check(Mathf.Abs(sprite.pixelsPerUnit - 64f) < .001f && sprite.vertices.Length == 4, sprite.name + " native scale and FullRect mesh");
                count++;
            }
        }
        Check(count == 2951, "all imported sprite IDs accounted for");
        foreach (var row in manifest.clips)
        {
            Check(Hash(row.path) == row.sha256, row.path + " unchanged timings/events/curves");
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(row.path);
            Check(clip != null, row.path + " imported clip");
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                foreach (var key in AnimationUtility.GetObjectReferenceCurve(clip, binding))
                    Check(key.value != null, row.path + " resolved object key at " + key.time);
        }
        foreach (var row in manifest.actors)
        {
            Check(AssetDatabase.LoadAssetAtPath<EnemyDefinition>(row.definition) != null, row.id + " definition");
            Check(AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(row.controller) != null, row.id + " controller");
        }
        File.WriteAllText(Path.Combine(Output, "assets.txt"), $"PASS: 395 textures; {count} preserved sprite IDs; 353 byte-identical clips; 64 forms. {assertions} assertions.\n");
        Debug.Log("ENEMY_ART_ASSETS_PASS " + assertions);
    }

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a graphics-enabled batch editor.");
        RunAssets(); EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Key, true); EditorApplication.EnterPlaymode();
    }

    private static IEnumerator Cases()
    {
        Application.runInBackground = true;
        string profilePath = Path.Combine(Output, "profile-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(profilePath, JsonUtility.ToJson(new AccountSave()));
        profile = AccountProgression.UseDisposableProfile(profilePath);
        selection = CharacterSelectionSettings.UseTemporarySelection("skeleton");
        SetSize(1080, 1920);
        yield return Until(() => Screen.width == 1080 && Screen.height == 1920, "initial portrait viewport");
        SceneManager.LoadScene("Game");
        yield return Until(() => RunSession.Current != null && RunSession.Current.Continuation.CanCapture && RunSession.Current.Waves.IsWaveActive, "production scene ready");
        var run = RunSession.Current;
        foreach (var controls in Object.FindObjectsByType<RunControlsUI>(FindObjectsSortMode.None)) { controls.Close(); controls.enabled = false; }
        foreach (var enemy in run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
        var host = run.Waves.ActiveEnemies.First();
        var image = host.transform.Find("VisualRoot").GetComponent<Image>();
        var fit = host.GetComponentInParent<PixelPerfectBattleCharacterUI>();
        Check(fit != null, "production spawn anchor owns pixel-grid fitter");
        host.GetComponent<EnemyCombatFeedback>().enabled = false;
        yield return Wait(.25f); Time.timeScale = 0;
        var measures = new List<string>();
        foreach (int height in new[] { 1920, 2400 })
        {
            SetSize(1080, height);
            yield return Until(() => Screen.width == 1080 && Screen.height == height, "portrait resize");
            yield return Wait(.25f);
            foreach (var actor in manifest.actors)
            {
                var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(actor.definition);
                Check(EnemyVisualPresenter.TryApply(host.gameObject, definition), actor.id + " existing presenter");
                var animator = image.GetComponent<Animator>(); animator.fireEvents = false; animator.enabled = false;
                Fit.Invoke(fit, null); Canvas.ForceUpdateCanvases();
                Rect baseline = ScreenRect(image);
                float scale = baseline.width / image.sprite.rect.width;
                float floor = baseline.yMin + image.sprite.pivot.y * scale;
                float center = baseline.center.x;
                if (poses == 0) CheckCanvasClearance(image);
                foreach (var texture in manifest.textures.Where(t => t.actor == actor.id))
                {
                    var sprites = AssetDatabase.LoadAllAssetsAtPath(texture.path).OfType<Sprite>().ToDictionary(LocalID);
                    foreach (var row in texture.sprites)
                    {
                        image.sprite = sprites[row.fileID]; Fit.Invoke(fit, null); Canvas.ForceUpdateCanvases();
                        Rect rect = ScreenRect(image); float texels = rect.width / image.sprite.rect.width;
                        Check(Mathf.Abs(texels - Mathf.Round(texels)) < .001f && Mathf.Abs(texels - scale) < .001f, actor.id + " stable integer pixels");
                        Check(Mathf.Abs(rect.center.x - center) < .1f, actor.id + " fixed horizontal anchor");
                        Check(Mathf.Abs(rect.yMin + image.sprite.pivot.y * texels - floor) < .1f, actor.id + " fixed authored floor");
                        var b = row.bounds;
                        Rect ink = new Rect(rect.xMin + b[0] * texels, rect.yMin + b[1] * texels, b[2] * texels, b[3] * texels);
                        Check(ink.yMin >= 0 && ink.yMax <= height, actor.id + " vertical portrait containment");
                        Check(ink.xMin >= 0 && ink.xMax <= 1080, actor.id + " horizontal portrait containment");
                        foreach (var mask in image.GetComponentsInParent<RectMask2D>())
                        {
                            Rect bounds = ScreenRect(mask.rectTransform, image.canvas);
                            Check(bounds.Contains(ink.min) && bounds.Contains(ink.max), actor.id + " opaque drawing clear of clipping mask");
                        }
                        poses++;
                    }
                }
                measures.Add($"{height} {actor.id}: {scale:0.###} screen pixels/texel; floor {floor:0.###}; center {center:0.###}");
                if (new[] { "puffer_sentinel", "snapvine", "seismic_smith", "royal_standard_bearer" }.Contains(actor.id))
                {
                    image.sprite = definition.FallbackVisualSprite; Fit.Invoke(fit, null); Canvas.ForceUpdateCanvases();
                    yield return Wait(.05f);
                    ScreenCapture.CaptureScreenshot(Path.Combine(Output, actor.id + "-" + height + ".png"));
                    yield return Wait(.15f);
                }
                yield return null;
            }
        }
        File.WriteAllLines(Path.Combine(Output, "grounding.txt"), measures);
        Check(poses == 5902, "all imported drawings checked at both portrait heights");
        File.WriteAllText(Path.Combine(Output, "presentation.txt"), $"PASS: {poses} pose/portrait cases in the production Game scene; fixed source floor and center; integer texel scale; horizontal/vertical screen and parent-mask containment. Transparent overhang accepted and actual artwork escape rejected by the layout validator. No physical-device claim.\n");
        Time.timeScale = 1; SceneManager.LoadScene("MainMenu"); yield return Wait(.1f);
        selection.Dispose(); selection = null; profile.Dispose(); profile = null;
    }

    private static string Hash(string path) { using var hash = SHA256.Create(); return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant(); }
    private static void CheckCanvasClearance(Image image)
    {
        var layout = Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        var method = typeof(GameplayPixelLayoutValidator).GetMethod("VisibleArtworkBounds", BindingFlags.NonPublic | BindingFlags.Static);
        var rect = image.rectTransform; Vector2 saved = rect.anchoredPosition;
        try
        {
            Rect full = ScreenRect(image), ink = (Rect)method.Invoke(null, new object[] { image });
            float margin = full.xMax - ink.xMax;
            Check(margin > 0, "fixture has transparent right clearance");
            rect.anchoredPosition += Vector2.right; Canvas.ForceUpdateCanvases();
            float pixelsPerUnit = ScreenRect(image).xMin - full.xMin;
            rect.anchoredPosition = saved + Vector2.right * ((layout.Current.Safe.xMax - full.xMax + margin * .5f) / pixelsPerUnit);
            Canvas.ForceUpdateCanvases();
            var errors = GameplayPixelLayoutValidator.Validate(layout, out _);
            Check(!errors.Any(e => e.StartsWith("UI artwork leaves safe area:") && e.EndsWith("/VisualRoot")), "transparent canvas overhang accepted");
            rect.anchoredPosition += Vector2.right * ((margin + 4) / pixelsPerUnit); Canvas.ForceUpdateCanvases();
            errors = GameplayPixelLayoutValidator.Validate(layout, out _);
            Check(errors.Any(e => e.StartsWith("UI artwork leaves safe area:") && e.EndsWith("/VisualRoot")), "actual enemy artwork outside safe area rejected");
        }
        finally { rect.anchoredPosition = saved; Canvas.ForceUpdateCanvases(); }
    }
    private static string LocalID(Sprite sprite) { AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string _, out long id); return id.ToString(); }
    private static Rect ScreenRect(Image image) => ScreenRect(image.rectTransform, image.canvas);
    private static Rect ScreenRect(RectTransform rect, Canvas canvas)
    {
        var corners = new Vector3[4]; rect.GetWorldCorners(corners);
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 a = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 b = RectTransformUtility.WorldToScreenPoint(camera, corners[2]); return Rect.MinMaxRect(a.x, a.y, b.x, b.y);
    }
    private static void Tick()
    {
        try { Check(failure == null, failure); if (Steps.Count == 0) { Finish(); return; } var step = Steps.Peek(); if (!step.MoveNext()) Steps.Pop(); else if (step.Current is IEnumerator nested) Steps.Push(nested); EditorApplication.QueuePlayerLoopUpdate(); }
        catch (Exception error) { failure = error.ToString(); result = 1; Debug.LogException(error); Finish(); }
    }
    private static void Finish()
    {
        EditorApplication.update -= Tick; Application.logMessageReceived -= Log; Time.timeScale = 1;
        if (result != 0) File.WriteAllText(Path.Combine(Output, "presentation.txt"), "FAIL: " + failure);
        EditorApplication.ExitPlaymode();
    }
    private static void Log(string text, string trace, LogType type)
    {
        if (type == LogType.Exception && trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")) return;
        if (text.StartsWith("Pixel layout FAILED:")) { File.AppendAllText(Path.Combine(Output, "startup-layout.txt"), text + "\n"); return; }
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failure = text;
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException("Enemy art validation: " + message); assertions++; }
    private static IEnumerator Wait(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
    private static IEnumerator Until(Func<bool> condition, string label) { float end = Time.realtimeSinceStartup + 40; while (!condition()) { Check(Time.realtimeSinceStartup < end, label); yield return null; } }
    private static void SetSize(int width, int height)
    {
        var assembly = typeof(Editor).Assembly; var type = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        var groupType = type.GetProperty("currentGroupType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sizes);
        var group = type.GetMethod("GetGroup").Invoke(sizes, new[] { groupType });
        var size = Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"), new object[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType"), 1), width, height, "Enemy art validation" });
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
        int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
        var viewType = assembly.GetType("UnityEditor.GameView"); var view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, count - 1);
        view.Show(); view.Focus(); view.Repaint();
    }
}
