using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Opt-in graphics validation using disposable state and the production scenes/resolver.</summary>
[InitializeOnLoad]
public static class DungeonPresentationValidation
{
    private const string Key = "DungeonMatcher.DungeonPresentationValidation";
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly string Output = Path.GetFullPath(".utmp/DungeonPresentation");
    private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    private static readonly StringBuilder Report = new StringBuilder();
    private static IDisposable profile, selection;
    private static string error;
    private static int result, assertions;
    private const string SfxKey = "DungeonMatcher.Audio.SfxMuted";
    private static bool audioPreferenceSaved, hadSfxPreference;
    private static int originalSfxPreference, viewportHeight;
    private static Component audioController;
    private static EventInfo audioEvent;
    private static Delegate audioHandler;
    private static readonly List<string> PlayedCues = new List<string>();

    static DungeonPresentationValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                result = 0; error = null; assertions = 0; Steps.Clear(); Report.Clear();
                Application.logMessageReceived += Log;
                Steps.Push(Cases()); EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Key, false);
                profile?.Dispose(); selection?.Dispose(); profile = selection = null;
                EditorApplication.Exit(SessionState.GetInt(Key + ".exitCode", 1));
            }
        };
    }

    // Use -batchmode -screen-width 1080 -screen-height 1920, without -quit or -nographics.
    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use an isolated graphics-enabled batch editor.");
        Directory.CreateDirectory(Output);
        File.WriteAllText(Path.Combine(Output, "production-validation.txt"), "RUNNING " + DateTime.UtcNow.ToString("O") + "\n");
        File.WriteAllText(Path.Combine(Output, "asset-validation.txt"), "RUNNING\n");
        File.WriteAllText(Path.Combine(Output, "runtime-events.tsv"), "height\tfamily\ttiles\ttime\n");
        File.WriteAllText(Path.Combine(Output, "audio-cues.tsv"), "height\tcue\ttime\n");
        File.WriteAllText(Path.Combine(Output, "startup-layout-diagnostics.txt"), "");
        File.WriteAllText(Path.Combine(Output, "editor-search-diagnostics.txt"), "");
        try
        {
            Check(SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null, "graphics device available");
            Type importer = typeof(DungeonPresentationValidation).Assembly.GetType("DungeonPresentationArtImporter");
            Check(importer != null, "dungeon presentation importer exists");
            importer.GetMethod("Run", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);
            TileBurstArtImporter.Run();
            ValidateAssets();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetSize(1080, 1920);
            SessionState.SetInt(Key + ".exitCode", 1);
            SessionState.SetBool(Key, true);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception)
        {
            File.WriteAllText(Path.Combine(Output, "production-validation.txt"), "FAIL before Play Mode\n" + exception);
            Debug.LogException(exception); EditorApplication.Exit(1);
        }
    }

    private static void ValidateAssets()
    {
        foreach (string name in new[] { "DungeonBackdropTile", "Torch", "TorchAlt", "RoyalBanner", "Barrel", "SkullPile", "DungeonMatcherLogo", "MenuDungeon" })
            ValidateSprite(Resources.Load<Sprite>("UI/DungeonPresentation/" + name), name);
        foreach (string name in new[] { "Potion", "Bomb" })
            ValidateSprite(Resources.Load<Sprite>("UI/Consumables/" + name), name);
        var library = Resources.Load<TileBurstLibrary>("VFX/TileBursts");
        Check(library != null, "runtime tile burst library exists");
        foreach (TileBurstKind kind in Enum.GetValues(typeof(TileBurstKind)))
        {
            var sequence = library.Find(kind);
            Check(sequence != null, kind + " frames and timings usable");
            foreach (var sprite in sequence.frames)
            {
                ValidateSprite(sprite, kind.ToString());
                Check(sprite.rect.size == new Vector2(64, 64) && sprite.pivot == new Vector2(32, 32), kind + " native 64x64 center pivot");
            }
        }
        File.WriteAllText(Path.Combine(Output, "asset-validation.txt"), "PASS: four burst families plus dungeon, logo and supply assets; Point, Full Rect, 64 PPU, no mipmaps/compression.\n");
    }

    private static void ValidateSprite(Sprite sprite, string label)
    {
        Check(sprite != null, label + " sprite exists");
        var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
        Check(importer != null && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled &&
            importer.textureCompression == TextureImporterCompression.Uncompressed, label + " crisp import settings");
        Check(sprite.pixelsPerUnit == 64 && sprite.vertices.Length == 4, label + " native density and Full Rect");
    }

    private static IEnumerator Cases()
    {
        Application.runInBackground = true;
        Time.captureDeltaTime = 1f / 60f;
        hadSfxPreference = PlayerPrefs.HasKey(SfxKey);
        originalSfxPreference = PlayerPrefs.GetInt(SfxKey, 0);
        audioPreferenceSaved = true;
        PlayerPrefs.SetInt(SfxKey, 0); // In-memory fixture; restored before leaving Play Mode.
        selection = CharacterSelectionSettings.UseTemporarySelection("skeleton");
        foreach (int height in new[] { 1920, 2400 })
        {
            viewportHeight = height;
            string profilePath = Path.Combine(Output, "test-" + height + "-" + Guid.NewGuid().ToString("N") + "-profile.json");
            File.WriteAllText(profilePath, JsonUtility.ToJson(new AccountSave { gold = 100, potions = 3, bombs = 3, equipPotions = true, equipBombs = true }));
            profile = AccountProgression.UseDisposableProfile(profilePath);
            SetSize(1080, height); yield return Wait(.2f);
            SceneManager.LoadScene("Game");
            yield return Until(() => RunSession.Current != null && RunSession.Current.Continuation.CanCapture && RunSession.Current.Waves.IsWaveActive, "production Game settles");
            StopEnemies();
            if (UnityEngine.EventSystems.EventSystem.current != null) UnityEngine.EventSystems.EventSystem.current.enabled = false;
            foreach (var controls in Object.FindObjectsByType<RunControlsUI>(FindObjectsSortMode.None)) { controls.Close(); controls.enabled = false; }
            yield return Until(() => GameObject.Find("PlayerDungeonBackdrop") != null && GameObject.Find("BottomDungeonBackdrop") != null, "dungeon panels installed");
            yield return Wait(.15f);
            ValidateBattle(height);
            ObserveAudio(RunSession.Current.Board);
            yield return DelayedMatchPauseCase(RunSession.Current.Board);
            Time.timeScale = 0; yield return Shot(height + "-game"); Time.timeScale = 1;
            foreach (TileBurstKind kind in Enum.GetValues(typeof(TileBurstKind)))
                yield return BurstCase(height, kind);
            StopObservingAudio();
            SceneManager.LoadScene("MainMenu");
            yield return Until(() => GameObject.Find("DungeonMatcherLogo") != null && GameObject.Find("MenuDungeonArtwork") != null, "production MainMenu artwork installed");
            yield return Wait(.15f);
            ValidateMenu(height);
            yield return Shot(height + "-menu");
            Object.FindFirstObjectByType<MainMenuController>().ShowHome();
            Object.FindFirstObjectByType<MainMenuController>().ShowHome();
            ValidateMenu(height);
            profile.Dispose(); profile = null;
        }
        selection.Dispose(); selection = null;
        Report.AppendLine("LIMITS: automated scene/renderer/cue evidence; screenshots require visual review. No physical-device vibration, Android deployment, or listening/loudness judgment was performed.");
    }

    private static void ValidateBattle(int height)
    {
        Check(Screen.width == 1080 && Screen.height == height, "actual Game viewport " + height);
        var layout = Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        Check(layout != null, "existing gameplay layout owner installed");
        var issues = GameplayPixelLayoutValidator.Validate(layout, out string layoutReport);
        File.WriteAllText(Path.Combine(Output, height + "-layout.txt"), layoutReport + string.Join("\n", issues));
        Check(issues.Count == 0, "settled production layout: " + string.Join("; ", issues));
        var surroundings = GameObject.Find("DungeonSurroundingMasonry")?.GetComponent<SpriteRenderer>();
        Check(surroundings != null && surroundings.enabled && surroundings.sprite != null && surroundings.sortingLayerName == "Default" && surroundings.sortingOrder == -200,
            "surrounding masonry below battle and board");
        var camera = Camera.main;
        Vector3 lo = camera.ViewportToWorldPoint(new Vector3(0, 0, surroundings.transform.position.z - camera.transform.position.z));
        Vector3 hi = camera.ViewportToWorldPoint(new Vector3(1, 1, surroundings.transform.position.z - camera.transform.position.z));
        Check(surroundings.bounds.min.x <= lo.x && surroundings.bounds.min.y <= lo.y && surroundings.bounds.max.x >= hi.x && surroundings.bounds.max.y >= hi.y,
            "masonry covers viewport gutters");
        foreach (string name in new[] { "PlayerDungeonBackdrop", "BottomDungeonBackdrop" })
        {
            var image = GameObject.Find(name)?.GetComponent<Image>();
            Check(image != null && image.enabled && image.sprite != null && image.type == Image.Type.Tiled && !image.raycastTarget, name + " tiled and noninteractive");
            Check(image.transform.GetSiblingIndex() == 0, name + " remains behind existing controls/frame");
        }
        var environment = Object.FindFirstObjectByType<BattleBackgroundTilemapController>()?.ActiveEnvironment;
        Check(environment != null, "existing tilemap environment remains the battle scenery owner");
        var props = environment.transform.Find("AtmosphereProps")?.GetComponent<Tilemap>();
        Check(props != null, "native battle props imported into existing environment");
        var renderer = props.GetComponent<TilemapRenderer>();
        Check(renderer != null && renderer.enabled && !renderer.forceRenderingOff && renderer.maskInteraction == SpriteMaskInteraction.VisibleInsideMask &&
            renderer.sortingLayerName == "Default" && renderer.sortingOrder == -96, "props use the authored battle viewport mask");
        var tiles = props.GetTilesBlock(props.cellBounds).Where(t => t != null).ToArray();
        Check(tiles.Length >= 7, "all seven battle prop placements exist");
        foreach (string name in new[] { "Torch", "RoyalBanner", "Barrel", "SkullPile" })
            Check(tiles.Any(t => t.name == name), name + " present on battle tilemap");
    }

    private static IEnumerator BurstCase(int height, TileBurstKind kind)
    {
        var run = RunSession.Current;
        yield return Until(() => !run.Board.IsBusy && run.Waves.IsWaveActive, kind + " board ready");
        StopEnemies();
        var board = run.Board;
        Check(board.Width >= 5 && board.Height >= 5, "board supports centered fixture");
        var sprites = (Sprite[])typeof(BoardController).GetField("gemSprites", Flags).GetValue(board);
        for (int y = 0; y < board.Height; y++) for (int x = 0; x < board.Width; x++)
        {
            var gem = board.GetGem(x, y);
            if (gem == null) continue;
            gem.SetSpecialType(GemSpecialType.None);
            int color = (x + y * 2) % 6; gem.SetType((GemType)color, sprites[color]);
        }
        int column = board.Width / 2, row = board.Height / 2;
        var center = board.GetGem(column, row);
        Check(center != null, "center fixture gem exists");
        center.SetSpecialType(kind == TileBurstKind.Poison ? GemSpecialType.PoisonBomb : kind == TileBurstKind.Healing ? GemSpecialType.HealingBomb
            : kind == TileBurstKind.Shield ? GemSpecialType.ShieldBomb : GemSpecialType.None);
        var expected = new List<Vector3>();
        for (int y = row - 1; y <= row + 1; y++) for (int x = column - 1; x <= column + 1; x++)
        {
            Check(board.GetGem(x, y) != null, "fixture footprint is populated");
            expected.Add(board.transform.TransformPoint(board.GetCellLocalPosition(x, y)));
        }
        TileBurstVFXContext observed = default; int cues = 0, commits = 0;
        int audioStart = PlayedCues.Count;
        string expectedCue = kind == TileBurstKind.Poison ? "PoisonBurst" : kind == TileBurstKind.Shield ? "ShieldGain" :
            kind == TileBurstKind.Healing ? "Healing" : "Explosion";
        Action<TileBurstVFXContext> handler = context =>
        {
            File.AppendAllText(Path.Combine(Output, "runtime-events.tsv"), height + "\t" + context.Kind + "\t" + context.TileCount + "\t" + Time.time.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + "\n");
            if (context.Kind == kind && cues++ == 0) observed = context;
        };
        board.TileBurstVFXRequested += handler;
        Check(board.TryClearPlayerArea(center, 1, () => { commits++; return true; }), kind + " accepted through production area resolver");
        Check(commits == 1, kind + " acceptance commits once");
        yield return Until(() => observed.TileCount > 0 && ActiveBursts(board).Length > 0 && PlayedCues.Skip(audioStart).Contains(expectedCue),
            kind + " actual shatter cue, rendered frames and mapped audio request");
        Check(PlayedCues.Skip(audioStart).Contains(expectedCue), kind + " real audio controller requested " + expectedCue);
        Time.timeScale = 0; yield return Wait(.08f);
        Check(observed.Kind == kind && observed.TileCount == expected.Count, kind + " complete affected-cell snapshot");
        Check(expected.All(p => observed.WorldPositions.Any(q => Vector3.Distance(p, q) < .0001f)), kind + " snapshot matches actual board centers");
        var library = Resources.Load<TileBurstLibrary>("VFX/TileBursts");
        var sequence = library.Find(kind);
        var effects = ActiveBursts(board);
        Check(effects.Length == expected.Count, kind + " one renderer per affected tile");
        Check(expected.All(p => effects.Any(effect => Vector3.Distance(p, effect.transform.position) < .0001f)), kind + " every affected tile has a renderer");
        foreach (var effect in effects)
        {
            Check(sequence.frames.Contains(effect.sprite), kind + " correct native family frame");
            Check(expected.Any(p => Vector3.Distance(p, effect.transform.position) < .0001f), kind + " centered renderer");
            Vector2 size = effect.sprite.rect.size / effect.sprite.pixelsPerUnit * (Vector2)effect.transform.localScale;
            Check(Vector2.Distance(size, Vector2.one * board.CellSize) < .0001f, kind + " covers one complete native tile");
            Check(effect.maskInteraction == SpriteMaskInteraction.VisibleInsideMask && effect.sortingLayerName == "Gems" && effect.sortingOrder == 12,
                kind + " remains inside board and under frame");
        }
        var pausedFrames = effects.Select(e => e.sprite).ToArray();
        int pausedAudioCount = PlayedCues.Count;
        yield return Wait(.15f);
        Check(effects.Select(e => e.sprite).SequenceEqual(pausedFrames), kind + " pause freezes frames");
        Check(PlayedCues.Count == pausedAudioCount, kind + " pause emits no new audio play requests");
        Check(board.IsBusy, kind + " production resolver retains ownership while paused");
        Time.timeScale = 1;
        int peak = Mathf.Min(4, sequence.frames.Length - 2);
        yield return Until(() => ActiveBursts(board).Any(r => Array.IndexOf(sequence.frames, r.sprite) >= peak), kind + " expanded peak frame");
        Time.timeScale = 0; yield return Wait(.04f);
        Check(ActiveBursts(board).Length == expected.Count, kind + " full footprint still visible at capture");
        Report.AppendLine(kind + " capture sprites: " + string.Join(",", ActiveBursts(board).Select(r => r.sprite.name).Distinct()));
        yield return Shot(height + "-burst-" + kind);
        Time.timeScale = 1;
        yield return Until(() => !board.IsBusy && ActiveBursts(board).Length == 0, kind + " cleanup and authoritative settlement");
        board.TileBurstVFXRequested -= handler;
        Check(cues >= 1, kind + " actual event observed");
        StopEnemies();
    }

    private static SpriteRenderer[] ActiveBursts(BoardController board) => board.GetComponentsInChildren<SpriteRenderer>()
        .Where(r => r.name == "TileBurst" && r.enabled && r.sprite != null).ToArray();

    private static void ObserveAudio(BoardController board)
    {
        Type type = typeof(BoardController).Assembly.GetType("CombatAudioController");
        Check(type != null, "production combat audio type exists");
        audioController = board.GetComponent(type);
        Check(audioController != null, "production scene installs its combat audio observer");
        // Batch editors may not own OS focus; this only supplies a test fixture for that platform input.
        type.GetField("focused", Flags).SetValue(audioController, true);
        Report.AppendLine("AUDIO FIXTURE: simulated application focus and temporary SFX unmute; CuePlayed proves an AudioSource.Play request, not audible output or physical haptics.");
        audioEvent = type.GetEvent("CuePlayed");
        Check(audioEvent != null, "audio exposes played-cue observation");
        Type cueType = audioEvent.EventHandlerType.GetGenericArguments()[0];
        MethodInfo observer = typeof(DungeonPresentationValidation).GetMethod(nameof(AudioCue), Flags).MakeGenericMethod(cueType);
        audioHandler = Delegate.CreateDelegate(audioEvent.EventHandlerType, observer);
        audioEvent.AddEventHandler(audioController, audioHandler);
    }

    private static void AudioCue<T>(T cue)
    {
        string name = cue.ToString(); PlayedCues.Add(name);
        File.AppendAllText(Path.Combine(Output, "audio-cues.tsv"), viewportHeight + "\t" + name + "\t" +
            Time.time.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + "\n");
    }

    private static void StopObservingAudio()
    {
        if (audioController != null && audioEvent != null && audioHandler != null) audioEvent.RemoveEventHandler(audioController, audioHandler);
        audioController = null; audioEvent = null; audioHandler = null;
    }

    private static IEnumerator DelayedMatchPauseCase(BoardController board)
    {
        var matchAudio = board.GetComponent<GemBreakAudioController>();
        Check(matchAudio != null, "production match audio controller exists");
        var delayField = typeof(GemBreakAudioController).GetField("breakSoundDelay", Flags);
        var delayedPlay = typeof(GemBreakAudioController).GetMethod("PlayBreakSoundAfterDelay", Flags);
        float originalDelay = (float)delayField.GetValue(matchAudio);
        delayField.SetValue(matchAudio, .1f);
        Report.AppendLine("AUDIO FIXTURE: invoking the real delayed match coroutine independently of board mutation to isolate its pause cancellation.");
        int start = PlayedCues.Count(cue => cue == "GemMatch");
        matchAudio.StartCoroutine((IEnumerator)delayedPlay.Invoke(matchAudio, new object[] { 3, 0 }));
        yield return Until(() => PlayedCues.Count(cue => cue == "GemMatch") > start, "delayed match positive playback control");
        Check(PlayedCues.Count(cue => cue == "GemMatch") == start + 1, "delayed match positive control emits one cue");
        int beforePause = PlayedCues.Count(cue => cue == "GemMatch");
        matchAudio.StartCoroutine((IEnumerator)delayedPlay.Invoke(matchAudio, new object[] { 3, 0 }));
        Time.timeScale = 0;
        yield return Wait(.15f);
        Check(PlayedCues.Count(cue => cue == "GemMatch") == beforePause, "delayed match remains silent during pause");
        Time.timeScale = 1;
        float resumed = Time.time;
        yield return Until(() => Time.time - resumed > .25f, "delayed match cancellation observation window");
        delayField.SetValue(matchAudio, originalDelay);
        Check(PlayedCues.Count(cue => cue == "GemMatch") == beforePause, "pause cancels delayed match; no stale cue after resume");
    }

    private static void StopEnemies()
    {
        foreach (var attack in Object.FindObjectsByType<EnemyAutoAttack>(FindObjectsSortMode.None)) attack.StopAttacking();
    }

    private static void ValidateMenu(int height)
    {
        Check(Screen.width == 1080 && Screen.height == height, "actual MainMenu viewport " + height);
        var images = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        var logo = images.Single(i => i.name == "DungeonMatcherLogo");
        var backdrop = images.Single(i => i.name == "MenuDungeonArtwork");
        foreach (var image in new[] { logo, backdrop })
        {
            Check(image.enabled && image.sprite != null && !image.raycastTarget, image.name + " unique visible noninteractive artwork");
            Rect bounds = ScreenRect(image.rectTransform);
            float ratio = bounds.width / image.sprite.rect.width;
            Check(Mathf.Abs(ratio - Mathf.Round(ratio)) < .001f && Mathf.Abs(bounds.height / image.sprite.rect.height - ratio) < .001f,
                image.name + " integer physical source pixels");
        }
        var names = new[] { "PlayButton", "CharactersButton", "ShopButton", "GemMasteryButton", "PracticeButton", "ChallengesButton" };
        var controls = new List<Rect>();
        foreach (string name in names)
        {
            var button = GameObject.Find(name)?.GetComponent<Button>();
            Check(button != null && button.isActiveAndEnabled, name + " still present");
            Rect bounds = ScreenRect((RectTransform)button.transform);
            Check(Contains(new Rect(0, 0, 1080, height), bounds), name + " inside portrait viewport");
            Check(!bounds.Overlaps(ScreenRect(logo.rectTransform)), name + " clear of logo");
            Check(controls.All(r => !r.Overlaps(bounds)), name + " clear of other controls");
            controls.Add(bounds);
        }
        var summary = GameObject.Find("AccountSummary")?.transform as RectTransform;
        Check(summary != null && !ScreenRect(summary).Overlaps(ScreenRect(logo.rectTransform)), "logo clears account summary");
        Check(Contains(new Rect(0, 0, 1080, height), ScreenRect(logo.rectTransform)), "logo contained in portrait viewport");
    }

    private static Rect ScreenRect(RectTransform rect)
    {
        var corners = new Vector3[4]; rect.GetWorldCorners(corners);
        var canvas = rect.GetComponentInParent<Canvas>();
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 a = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 b = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
        return Rect.MinMaxRect(a.x, a.y, b.x, b.y);
    }

    private static bool Contains(Rect outer, Rect inner) => inner.xMin >= outer.xMin - .1f && inner.yMin >= outer.yMin - .1f && inner.xMax <= outer.xMax + .1f && inner.yMax <= outer.yMax + .1f;

    private static IEnumerator Shot(string name)
    {
        string path = Path.Combine(Output, name + ".png");
        if (File.Exists(path)) File.Delete(path); // exact validation output only; never a source asset
        ScreenCapture.CaptureScreenshot(path);
        yield return Until(() => CompletePng(path), "complete screenshot written: " + name);
        byte[] bytes = File.ReadAllBytes(path);
        int width = bytes[16] << 24 | bytes[17] << 16 | bytes[18] << 8 | bytes[19];
        int height = bytes[20] << 24 | bytes[21] << 16 | bytes[22] << 8 | bytes[23];
        Check(width == Screen.width && height == Screen.height, name + " native screenshot dimensions");
        Report.AppendLine("SCREENSHOT " + path);
    }

    private static bool CompletePng(string path)
    {
        if (!File.Exists(path)) return false;
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            return bytes.Length > 32 && bytes[0] == 137 && bytes[1] == 80 && bytes[2] == 78 && bytes[3] == 71 &&
                Encoding.ASCII.GetString(bytes, bytes.Length - 8, 4) == "IEND";
        }
        catch (IOException) { return false; }
    }

    private static void Tick()
    {
        try
        {
            if (error != null) throw new InvalidOperationException(error);
            if (Steps.Count == 0) { Finish(); return; }
            var step = Steps.Peek();
            if (!step.MoveNext()) { Steps.Pop(); return; }
            if (step.Current is IEnumerator nested) Steps.Push(nested);
            EditorApplication.QueuePlayerLoopUpdate();
        }
        catch (Exception exception) { result = 1; error = exception.ToString(); Finish(); }
    }

    private static void Finish()
    {
        EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
        StopObservingAudio();
        if (audioPreferenceSaved)
        {
            if (hadSfxPreference) PlayerPrefs.SetInt(SfxKey, originalSfxPreference); else PlayerPrefs.DeleteKey(SfxKey);
            audioPreferenceSaved = false;
        }
        selection?.Dispose(); selection = null; profile?.Dispose(); profile = null;
        Time.captureDeltaTime = 0; Time.timeScale = 1;
        File.WriteAllText(Path.Combine(Output, "production-validation.txt"), (result == 0 ? "PASS" : "FAIL") + " " + DateTime.UtcNow.ToString("O") + "\n" +
            assertions + " assertions\n" + Report + (error == null ? "" : "\n" + error));
        if (result != 0) Debug.LogError("Dungeon presentation validation failed: " + error);
        else Debug.Log("Dungeon presentation validation PASSED. Evidence: " + Output);
        SessionState.SetInt(Key + ".exitCode", result);
        EditorApplication.ExitPlaymode();
    }

    private static void Log(string text, string trace, LogType type)
    {
        if (text.StartsWith("Pixel layout FAILED:"))
        { File.AppendAllText(Path.Combine(Output, "startup-layout-diagnostics.txt"), text + "\n"); return; }
        // Settled layout is explicitly rechecked by ValidateBattle; transient startup messages remain in evidence.
        if (type == LogType.Exception && trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup"))
        { File.AppendAllText(Path.Combine(Output, "editor-search-diagnostics.txt"), text + "\n"); return; }
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) error ??= text + "\n" + trace;
    }

    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException("Dungeon presentation validation: " + message); assertions++; if (!string.IsNullOrEmpty(message)) Report.AppendLine("PASS " + message); }
    private static IEnumerator Wait(float seconds)
    { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
    private static IEnumerator Until(Func<bool> predicate, string label)
    { float end = Time.realtimeSinceStartup + 40f; while (!predicate()) { if (Time.realtimeSinceStartup >= end) throw new TimeoutException(label); yield return null; } }

    private static void SetSize(int width, int height)
    {
        var assembly = typeof(Editor).Assembly; var type = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        var groupType = type.GetProperty("currentGroupType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sizes);
        var group = type.GetMethod("GetGroup").Invoke(sizes, new[] { groupType });
        var size = Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"), new object[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType"), 1), width, height, "Dungeon presentation validation" });
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
        int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
        var viewType = assembly.GetType("UnityEditor.GameView"); var view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, count - 1);
        view.Show(); view.Focus(); view.Repaint();
    }
}
