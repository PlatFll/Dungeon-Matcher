using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Background-only production scene checks and screenshots, using disposable account state.</summary>
[InitializeOnLoad]
public static class DungeonBackgroundValidation
{
    private const string Key = "DungeonMatcher.BackgroundValidation";
    private static readonly string Output = Path.GetFullPath(".utmp/Backgrounds");
    private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    private static IDisposable profile, selection;
    private static string failure;
    private static int exitCode;

    static DungeonBackgroundValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                failure = null; exitCode = 0; Steps.Clear();
                Application.logMessageReceived += Log; Steps.Push(Cases()); EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Key, false); profile?.Dispose(); selection?.Dispose();
                EditorApplication.Exit(exitCode);
            }
        };
    }

    public static void ImportAndRun()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a graphics-enabled batch editor.");
        Directory.CreateDirectory(Output);
        File.WriteAllText(Path.Combine(Output, "validation.txt"), "RUNNING\n");
        DungeonBackgroundImporter.Run();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Key, true); EditorApplication.EnterPlaymode();
    }

    private static IEnumerator Cases()
    {
        Application.runInBackground = true; Time.captureDeltaTime = 1f / 60f;
        string path = Path.Combine(Output, "profile-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, JsonUtility.ToJson(new AccountSave { gold = 100, potions = 3, bombs = 3, equipPotions = true, equipBombs = true }));
        profile = AccountProgression.UseDisposableProfile(path);
        selection = CharacterSelectionSettings.UseTemporarySelection("skeleton");
        foreach (int height in new[] { 1920, 2400 })
        {
            SetSize(1080, height); yield return Wait(.3f);
            SceneManager.LoadScene("Game");
            yield return Until(() => RunSession.Current != null && RunSession.Current.Waves.IsWaveActive && RunSession.Current.Continuation.CanCapture, "game settles");
            foreach (var enemy in RunSession.Current.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
            yield return Until(() => GameObject.Find("BottomDungeonBackdrop") != null, "background panels installed");
            // The second case resumes the disposable run and opens its normal guide.
            // Close it through the existing API so screenshots show the background.
            Object.FindFirstObjectByType<RunControlsUI>()?.Close();
            yield return Wait(.6f);
            Check(Screen.width == 1080 && Screen.height == height, "requested viewport");
            var layout = Object.FindFirstObjectByType<GameplayPixelLayoutController>();
            var issues = GameplayPixelLayoutValidator.Validate(layout, out string report);
            File.WriteAllText(Path.Combine(Output, height + "-layout.txt"), report + string.Join("\n", issues));
            Check(issues.Count == 0, "production layout: " + string.Join("; ", issues));
            var controller = Object.FindFirstObjectByType<BattleBackgroundTilemapController>();
            var environment = controller.ActiveEnvironment;
            Check(environment != null && environment.transform.localScale == Vector3.one, "existing environment and native scale");
            var back = environment.transform.Find("BackWall").GetComponent<Tilemap>();
            var ground = environment.transform.Find("Floor").GetComponent<Tilemap>();
            Check(back.GetUsedTilesCount() == 32 && ground.GetUsedTilesCount() == 9, "complete selected wall and floor tiles");
            foreach (var map in new[] { back, ground })
            {
                var renderer = map.GetComponent<TilemapRenderer>();
                Check(renderer.enabled && !renderer.forceRenderingOff && renderer.maskInteraction == SpriteMaskInteraction.VisibleInsideMask, "background uses existing viewport mask");
                foreach (var tile in map.GetTilesBlock(map.cellBounds).OfType<Tile>().Distinct()) ValidateSprite(tile.sprite);
            }
            foreach (string name in new[] { "Architecture", "BackDecor", "AtmosphereProps" })
                Check(environment.transform.Find(name).GetComponent<Tilemap>().GetUsedTilesCount() == 0, "previous dressing not doubled: " + name);
            var surround = GameObject.Find("DungeonSurroundingMasonry").GetComponent<SpriteRenderer>();
            Check(surround.sortingOrder == -200 && surround.drawMode == SpriteDrawMode.Tiled, "surrounds stay behind gameplay");
            ValidateSprite(surround.sprite);
            var camera = Camera.main;
            var low = camera.ViewportToWorldPoint(new Vector3(0, 0, surround.transform.position.z - camera.transform.position.z));
            var high = camera.ViewportToWorldPoint(new Vector3(1, 1, surround.transform.position.z - camera.transform.position.z));
            Check(surround.bounds.min.x <= low.x && surround.bounds.min.y <= low.y && surround.bounds.max.x >= high.x && surround.bounds.max.y >= high.y, "no uncovered viewport gutters");
            foreach (string name in new[] { "PlayerDungeonBackdrop", "BottomDungeonBackdrop" })
            {
                var image = GameObject.Find(name).GetComponent<Image>();
                Check(image.type == Image.Type.Tiled && !image.raycastTarget && image.transform.GetSiblingIndex() == 0, "panel fill behind controls: " + name);
            }
            Time.timeScale = 0;
            ScreenCapture.CaptureScreenshot(Path.Combine(Output, height + "-game.png")); yield return Wait(.4f);
            Time.timeScale = 1; SceneManager.LoadScene("MainMenu"); yield return Wait(.2f);
        }
        profile.Dispose(); profile = null; selection.Dispose(); selection = null;
        File.WriteAllText(Path.Combine(Output, "validation.txt"), "PASS: production Game at 1080x1920 and 1080x2400; complete selected tile maps, native pixel imports, unchanged layout, background mask, viewport coverage and panel layering. Screenshots captured for visual review.\n");
    }
    private static void ValidateSprite(Sprite sprite)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite));
        Check(sprite.pixelsPerUnit == 64 && sprite.vertices.Length == 4 && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "native crisp full-rect background sprite");
    }
    private static void Tick()
    {
        try
        {
            Check(failure == null, failure);
            if (Steps.Count == 0) { Finish(); return; }
            var step = Steps.Peek(); if (!step.MoveNext()) Steps.Pop(); else if (step.Current is IEnumerator nested) Steps.Push(nested);
            EditorApplication.QueuePlayerLoopUpdate();
        }
        catch (Exception error) { exitCode = 1; failure = error.ToString(); Finish(); }
    }
    private static void Finish()
    {
        EditorApplication.update -= Tick; Application.logMessageReceived -= Log; Time.timeScale = 1;
        if (exitCode != 0) File.WriteAllText(Path.Combine(Output, "validation.txt"), "FAIL: " + failure);
        EditorApplication.ExitPlaymode();
    }
    private static void Log(string message, string trace, LogType type)
    {
        if (type == LogType.Exception && trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")) return;
        if (message.StartsWith("Pixel layout FAILED:")) { File.AppendAllText(Path.Combine(Output, "startup-layout.txt"), message + "\n"); return; }
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failure = message;
    }
    private static void Check(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); }
    private static IEnumerator Wait(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
    private static IEnumerator Until(Func<bool> test, string label) { float end = Time.realtimeSinceStartup + 40; while (!test()) { Check(Time.realtimeSinceStartup < end, label); yield return null; } }
    private static void SetSize(int width, int height)
    {
        var assembly = typeof(Editor).Assembly; var type = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        var groupType = type.GetProperty("currentGroupType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sizes);
        var group = type.GetMethod("GetGroup").Invoke(sizes, new[] { groupType });
        var size = Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"), new object[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType"), 1), width, height, "Background review" });
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
        int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
        var viewType = assembly.GetType("UnityEditor.GameView"); var view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, count - 1);
        view.Show(); view.Focus(); view.Repaint();
    }
}
