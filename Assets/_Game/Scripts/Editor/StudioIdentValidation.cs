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
using Object = UnityEngine.Object;

// Isolated batch editor only; omit -quit so Play Mode checks can finish.
[InitializeOnLoad]
public static class StudioIdentValidation
{
    private const string Pending = "SmallHold.Validation.Pending";
    private const string Result = "SmallHold.Validation.Result";
    private const string AtlasPath = "Assets/_Game/Art/Branding/SmallHold_Games_Atlas.png";
    private const BindingFlags Fields = BindingFlags.NonPublic | BindingFlags.Instance;
    private static IEnumerator routine;
    private static string runtimeError;
    private static int editorSearchErrors;
    private static int menuLoads;

    static StudioIdentValidation()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use an isolated batch editor for StudioIdentValidation.");
        ValidateAssetsAndLayout();
        EditorSceneManager.OpenScene(StudioIdentPlayer.ScenePath);
        StudioIdentPlayer ident = Object.FindFirstObjectByType<StudioIdentPlayer>();
        Check(ident != null, "startup scene has its player");
        var serialized = new SerializedObject(ident);
        Check(serialized.FindProperty("frameAtlas").objectReferenceValue == AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath), "serialized atlas reference");
        Check(serialized.FindProperty("nextScenePath").stringValue == "Assets/_Game/Scenes/MainMenu.unity", "serialized menu destination");
        Check(Mathf.Approximately(ident.FinalLogoHoldSeconds, 1.5f), "additional final white-logo hold is 1.5 seconds");
        Check(Object.FindFirstObjectByType<BoardController>() == null, "startup contains no board");
        Directory.CreateDirectory(".utmp/StudioIdent");
        SetPortraitGameView();
        SessionState.SetBool(Pending, true);
        SessionState.SetInt(Result, 1);
        EditorApplication.EnterPlaymode();
    }

    private static void ValidateAssetsAndLayout()
    {
        string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        Check(scenes.Length >= 3 && scenes[0] == StudioIdentPlayer.ScenePath &&
            scenes[1] == "Assets/_Game/Scenes/MainMenu.unity" && scenes[2] == "Assets/_Game/Scenes/Game.unity", "build scene order");
        Check(!PlayerSettings.SplashScreen.show && !PlayerSettings.SplashScreen.showUnityLogo, "native Unity splash and Unity logo are disabled");
        var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
        Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
        Check(atlas != null && atlas.width == 2400 && atlas.height == 1620, "full-resolution 30-frame atlas");
        var readable = new Texture2D(2, 2);
        readable.LoadImage(File.ReadAllBytes(AtlasPath));
        Color corner = readable.GetPixel(4 * 480, 0);
        Check(corner.r > .99f && corner.g > .99f && corner.b > .99f, "held final pose has a white background");
        Object.DestroyImmediate(readable);
        Check(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && !importer.isReadable &&
            importer.npotScale == TextureImporterNPOTScale.None && importer.textureCompression == TextureImporterCompression.Uncompressed,
            "crisp, uncompressed GPU-only texture settings");
        foreach (string platform in new[] { "Standalone", "Android", "iPhone" })
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
            Check(!settings.overridden || (settings.maxTextureSize >= 4096 && settings.textureCompression == TextureImporterCompression.Uncompressed), platform + " retains atlas pixels");
        }
        foreach (Rect safe in new[] {
            new Rect(0, 0, 480, 270), new Rect(0, 0, 1920, 1080),
            new Rect(0, 0, 1080, 1920), new Rect(0, 96, 1080, 2190),
            new Rect(84, 0, 2220, 1080), new Rect(0, 0, 720, 1600),
            new Rect(0, 0, 320, 568), new Rect(0, 0, 3840, 2160) })
        {
            Rect fitted = StudioIdentPlayer.CalculateFrameRect(safe);
            Check(fitted.width > 0 && fitted.height > 0 &&
                Mathf.Abs(fitted.width / fitted.height - 16f / 9f) < .001f, "landscape aspect preserved");
            Check(fitted.xMin >= safe.xMin - .5f && fitted.yMin >= safe.yMin - .5f &&
                fitted.xMax <= safe.xMax + .5f && fitted.yMax <= safe.yMax + .5f, "fits safe area");
            Check(Vector2.Distance(fitted.center, safe.center) <= .71f, "centered to nearest device pixel");
            if (safe.width >= 480 && safe.height >= 270)
                Check(Mathf.Approximately(fitted.width / 480, Mathf.Round(fitted.width / 480)), "integer scaling");
        }
        Debug.Log("SmallHold static validation passed: build order, serialized assets, texture settings, eight layouts.");
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Pending, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            runtimeError = null;
            editorSearchErrors = 0;
            menuLoads = 0;
            Application.logMessageReceived += OnLog;
            SceneManager.sceneLoaded += CountMenuLoads;
            routine = ValidatePlayback();
            EditorApplication.update += Tick;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.SetBool(Pending, false);
            EditorApplication.Exit(SessionState.GetInt(Result, 1));
        }
    }

    private static void CountMenuLoads(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu") menuLoads++;
    }

    private static void OnLog(string message, string trace, LogType type)
    {
        // Unity 6.3's batch Search index can fail while creating its first Game
        // View. Record this editor-only failure separately; never suppress errors
        // from the ident, menu, gameplay, rendering, or any other stack.
        if (type == LogType.Exception && message.StartsWith("ArgumentOutOfRangeException:") &&
            trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup"))
        {
            editorSearchErrors++;
            return;
        }
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            runtimeError = message;
    }

    private static void Tick()
    {
        try
        {
            Check(runtimeError == null, "runtime error: " + runtimeError);
            if (routine.MoveNext()) { EditorApplication.QueuePlayerLoopUpdate(); return; }
            Finish(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            Finish(1);
        }
    }

    private static void Finish(int result)
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        SceneManager.sceneLoaded -= CountMenuLoads;
        SceneManager.sceneLoaded -= RemoveOptionalAtlas;
        (routine as IDisposable)?.Dispose();
        routine = null;
        Time.timeScale = 1f;
        SessionState.SetInt(Result, result);
        EditorApplication.ExitPlaymode();
    }

    private static IEnumerator ValidatePlayback()
    {
        StudioIdentPlayer ident = Object.FindFirstObjectByType<StudioIdentPlayer>();
        Check(ident != null, "ident active on launch");
        Time.timeScale = 0f;
        double started = EditorApplication.timeSinceStartup;
        var observed = new HashSet<int>();
        bool suspensionChecked = false;
        double pausedDuration = 0;
        double finalPoseStarted = -1;
        while (ident != null)
        {
            Check(EditorApplication.timeSinceStartup - started < 15, "ident completes without a timeScale dependency");
            Check(!SceneManager.GetSceneByName("MainMenu").isLoaded && !SceneManager.GetSceneByName("Game").isLoaded, "menu and game stay unloaded during ident");
            Check(BackgroundMusicPlayer.Instance == null, "music is deferred during ident");
            int frame = (int)typeof(StudioIdentPlayer).GetField("currentFrame", Fields).GetValue(ident);
            Check(frame >= 0 && frame < 30, "valid frame index");
            if (frame == 29 && finalPoseStarted < 0) finalPoseStarted = EditorApplication.timeSinceStartup;
            if (observed.Add(frame) && (frame == 0 || frame == 10 || frame == 16 || frame == 24 || frame == 28 || frame == 29))
                ScreenCapture.CaptureScreenshot($".utmp/StudioIdent/frame-{frame + 1:00}.png");
            if (frame >= 3 && !suspensionChecked)
            {
                suspensionChecked = true;
                ident.SendMessage("OnApplicationPause", true);
                double pauseStart = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - pauseStart < .2)
                {
                    Check((int)typeof(StudioIdentPlayer).GetField("currentFrame", Fields).GetValue(ident) == frame, "suspension freezes frame");
                    yield return null;
                }
                pausedDuration = EditorApplication.timeSinceStartup - pauseStart;
                ident.SendMessage("OnApplicationPause", false);
            }
            yield return null;
        }
        Check(EditorApplication.timeSinceStartup - started - pausedDuration >= 3.9, "animation plus additional hold precedes menu");
        Check(finalPoseStarted >= 0 && EditorApplication.timeSinceStartup - finalPoseStarted >= 1.5, "final white logo stays visible for the additional hold");
        Check(observed.Contains(29) && observed.Any(frame => frame >= 23 && frame <= 24), "white-only and final wordmark poses displayed");
        Check(suspensionChecked && menuLoads == 1 && SceneManager.GetActiveScene().name == "MainMenu", "single successful menu handoff");
        Check(Object.FindFirstObjectByType<MainMenuController>() != null && BackgroundMusicPlayer.Instance != null, "existing menu and music initialize");
        BackgroundMusicPlayer music = BackgroundMusicPlayer.Instance;
        Time.timeScale = 1f;
        AsyncOperation reload = SceneManager.LoadSceneAsync("MainMenu");
        while (!reload.isDone) yield return null;
        Check(Object.FindFirstObjectByType<StudioIdentPlayer>() == null && BackgroundMusicPlayer.Instance == music &&
            Object.FindObjectsByType<BackgroundMusicPlayer>(FindObjectsSortMode.None).Length == 1, "menu re-entry does not replay ident or duplicate music");

        // Remove only the in-memory optional reference after Awake, before Start.
        // No source scene, import setting or PlayerPrefs is written by this test.
        SceneManager.sceneLoaded += RemoveOptionalAtlas;
        reload = SceneManager.LoadSceneAsync(StudioIdentPlayer.ScenePath);
        while (!reload.isDone) yield return null;
        double deadline = EditorApplication.timeSinceStartup + 5;
        while (SceneManager.GetActiveScene().name != "MainMenu")
        {
            Check(EditorApplication.timeSinceStartup < deadline, "missing art falls through to menu");
            yield return null;
        }
        SceneManager.sceneLoaded -= RemoveOptionalAtlas;
        Check(menuLoads == 3, "missing art loads the menu exactly once");
        File.WriteAllText(".utmp/StudioIdent/validation.txt",
            "PASS: atlas/import settings; eight layouts; unscaled playback; suspension; white/name frames; menu handoff; music deferral; menu re-entry; missing-art fallback.\n" +
            "Observed frames: " + string.Join(", ", observed.OrderBy(frame => frame)) + "\n" +
            "Unrelated batch-editor Search index exceptions: " + editorSearchErrors + "\n");
        Debug.Log("SmallHold Play Mode validation PASSED.");
    }

    private static void RemoveOptionalAtlas(Scene scene, LoadSceneMode mode)
    {
        if (scene.path != StudioIdentPlayer.ScenePath) return;
        StudioIdentPlayer ident = Object.FindFirstObjectByType<StudioIdentPlayer>();
        typeof(StudioIdentPlayer).GetField("frameAtlas", Fields).SetValue(ident, null);
    }

    private static void SetPortraitGameView()
    {
        // Same fixed-size Game View mechanism as GameplayPixelLayoutTests.
        Assembly assembly = typeof(Editor).Assembly;
        Type sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        Type singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singleton.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        object currentGroup = sizesType.GetProperty("currentGroupType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sizes);
        object group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { currentGroup });
        Type sizeType = assembly.GetType("UnityEditor.GameViewSize");
        Type kind = assembly.GetType("UnityEditor.GameViewSizeType");
        object entry = Activator.CreateInstance(sizeType, new object[] { Enum.ToObject(kind, 1), 1080, 1920, "SmallHold validation" });
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { entry });
        int total = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
        Type viewType = assembly.GetType("UnityEditor.GameView");
        EditorWindow view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, total - 1);
        view.Show();
        view.Focus();
        view.Repaint();
    }

    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("SmallHold validation: " + label);
    }
}
