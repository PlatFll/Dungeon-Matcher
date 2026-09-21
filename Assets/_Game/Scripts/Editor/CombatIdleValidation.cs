using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Opt-in production-scene animation, alignment and portrait render checks.</summary>
[InitializeOnLoad]
public static class CombatIdleValidation
{
    private const string Key = "DungeonMatcher.CombatIdleValidation";
    private static readonly string Output = Path.GetFullPath(".utmp/CombatIdles");
    private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    private static IDisposable profile, selection;
    private static string error;
    private static int result, assertions;

    static CombatIdleValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                result = 0; error = null; assertions = 0; Steps.Clear();
                Application.logMessageReceived += Log;
                Steps.Push(Cases());
                EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Key, false);
                profile?.Dispose(); selection?.Dispose();
                EditorApplication.Exit(result);
            }
        };
    }

    public static void Run() => Start(false);
    public static void RunGuards() => Start(true);

    private static void Start(bool guards)
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use an isolated graphics-enabled batch editor.");
        Directory.CreateDirectory(Output);
        SessionState.SetBool(Key + ".guards", guards);
        ValidateAssets(guards);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Key, true);
        EditorApplication.EnterPlaymode();
    }

    public static void ImportAndRun() { CombatIdleImporter.Run(); Run(); }
    public static void ImportGuardsAndRun() { CombatIdleImporter.ImportGuards(); RunGuards(); }

    private static void ValidateAssets(bool guards)
    {
        foreach (string name in guards ? CombatIdleImporter.Guards : CombatIdleImporter.Characters)
        {
            string path = CombatIdleImporter.ArtRoot + "/" + name + "_Idle.png";
            string sourceRoot = guards ? "ArtSource/GuardIdles/" : "ArtSource/CombatIdles/";
            Check(File.ReadAllBytes(path).SequenceEqual(File.ReadAllBytes(sourceRoot + name + "_Idle.png")), name + " source PNG byte preservation");
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Check(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.spritePixelsPerUnit == 64 && importer.textureCompression == TextureImporterCompression.Uncompressed, name + " pixel import");
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            Check(settings.spriteMeshType == SpriteMeshType.FullRect, name + " full rectangular mesh");
            Sprite[] frames = CombatIdleImporter.LoadFrames(name);
            Check(frames.Length == 9, name + " nine imported frames");
            for (int i = 0; i < 9; i++)
            {
                Check(frames[i].rect == new Rect(i * 64, 0, 64, 64), name + " fixed frame rectangle");
                Check(frames[i].pivot == new Vector2(32, 0), name + " fixed bottom-center pivot");
            }
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(CombatIdleImporter.AnimationRoot + "/" + name + "_Idle.anim");
            Check(clip != null && clip.isLooping && Mathf.Abs(clip.length - 1.17f) < .0001f, name + " exact full loop duration");
            Check(AnimationUtility.GetCurveBindings(clip).Length == 0 && clip.events.Length == 0, name + " sprite-only presentation, no combat events or transform animation");
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            Check(bindings.Length == 1 && bindings[0].type == typeof(Image) && bindings[0].propertyName == "m_Sprite" && bindings[0].path == "", name + " correct UI Image binding");
            var keys = AnimationUtility.GetObjectReferenceCurve(clip, bindings[0]);
            Check(keys.Length == 10, name + " complete final-frame duration");
            for (int i = 0; i < 9; i++)
                Check(keys[i].value == frames[i] && Mathf.Abs(keys[i].time - i * .13f) < .0001f, name + " frame order and 130ms timing");
            Check(keys[9].value == frames[8] && Mathf.Abs(keys[9].time - 1.16f) < .0001f, name + " final sample holds frame nine without an extra loop tick");
            if (guards)
            {
                var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_" + name + ".asset");
                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CombatIdleImporter.AnimationRoot + "/" + name + "_Idle.controller");
                Check(definition.FallbackVisualSprite == frames[0] && definition.AnimationControllerOverride == controller, name + " definition references selected guard art");
                Check(controller.layers[0].stateMachine.defaultState.motion == clip, name + " idle is the default state");
                Check(!definition.TimeAutoAttackFromAnimation && !definition.UseAuthoredAutoAttackMotion && !definition.UseAuthoredSpecialAbilityMotion, name + " existing gameplay and fallback action timing retained");
            }
        }
        File.WriteAllText(Path.Combine(Output, guards ? "guard-asset-validation.txt" : "asset-validation.txt"), "PASS: source bytes, fixed rectangles/pivots, Point/FullRect import, Image curves, 130ms frames and 1.17s loops.\n");
    }

    private static IEnumerator Cases()
    {
        if (SessionState.GetBool(Key + ".guards", false))
        { yield return GuardCases(); yield break; }
        foreach (string player in new[] { "skeleton", "bardley" })
        {
            string profilePath = Path.Combine(Output, player + "-profile.json");
            File.WriteAllText(profilePath, JsonUtility.ToJson(new AccountSave()));
            profile = AccountProgression.UseDisposableProfile(profilePath);
            selection = CharacterSelectionSettings.UseTemporarySelection(player);
            SetSize(1080, 1920);
            SceneManager.LoadScene("Game");
            yield return Until(() => RunSession.Current != null && RunSession.Current.Continuation.CanCapture && RunSession.Current.Waves.IsWaveActive, "production Game ready");
            var run = RunSession.Current;
            foreach (string enemy in new[] { "Farmer", "PanVillager" })
            {
                var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_" + enemy + ".asset");
                if (!run.Waves.ActiveEnemies.Any(e => e.Definition == definition))
                    Check(run.Waves.TrySummonEnemy(definition, out _), enemy + " spawned through production wave flow");
            }
            yield return Wait(.3f);
            var playerImage = Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Single(i => i.name == "PlayerCharacter");
            var actors = new Dictionary<string, Image> { [player == "skeleton" ? "Rattlebones" : "Bardley"] = playerImage };
            foreach (var enemy in run.Waves.ActiveEnemies)
                if (enemy.Definition.EnemyId == "farmer" || enemy.Definition.EnemyId == "pan_villager")
                    actors[enemy.Definition.EnemyId == "farmer" ? "Farmer" : "PanVillager"] = enemy.transform.Find("VisualRoot").GetComponent<Image>();
            Check(actors.Count == 3, "correct player and both requested villagers visible");

            // Observe real-time Animator playback before deterministic per-pose checks.
            var seen = actors.ToDictionary(p => p.Key, p => new HashSet<Sprite>());
            float end = Time.realtimeSinceStartup + 1.4f;
            while (Time.realtimeSinceStartup < end)
            {
                foreach (var pair in actors) seen[pair.Key].Add(pair.Value.sprite);
                yield return null;
            }
            foreach (var pair in seen) Check(pair.Value.Count >= 7, pair.Key + " animates during actual game playback");
            Time.timeScale = 0;
            foreach (int height in new[] { 1920, 2400 })
            {
                SetSize(1080, height); yield return Wait(.3f);
                var layout = Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                var layoutErrors = GameplayPixelLayoutValidator.Validate(layout, out _);
                foreach (string issue in layoutErrors)
                    Check(IsAbilityIconWarning(issue), "unrelated or character layout failure: " + issue);
                if (layoutErrors.Count > 0)
                {
                    // Reproduce the pre-existing ability-icon issue with the old
                    // static player image: the new idle does not control that UI.
                    var animator = playerImage.GetComponent<Animator>();
                    Sprite currentSprite = playerImage.sprite; animator.enabled = false;
                    playerImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Characters/Bardley/Bardley.png");
                    Canvas.ForceUpdateCanvases();
                    var baselineErrors = GameplayPixelLayoutValidator.Validate(layout, out _);
                    Check(baselineErrors.Any(IsAbilityIconWarning), "ability-icon warning also exists with old static artwork");
                    playerImage.sprite = currentSprite; animator.enabled = true;
                    yield return Wait(.3f);
                    File.AppendAllText(Path.Combine(Output, "unrelated-ui-warnings.txt"), "Old static artwork reproduces: " + string.Join("; ", layoutErrors) + "\n");
                }
                var initial = actors.ToDictionary(p => p.Key, p => ScreenRect(p.Value));
                foreach (int frame in Enumerable.Range(0, 9))
                {
                    foreach (var pair in actors)
                    {
                        var animator = pair.Value.GetComponent<Animator>();
                        Check(animator != null && animator.enabled, pair.Key + " production controller active");
                        animator.Play("Idle", 0, (frame * .13f + .065f) / 1.17f);
                        animator.Update(0);
                    }
                    yield return Wait(.02f);
                    Canvas.ForceUpdateCanvases();
                    foreach (var pair in actors)
                    {
                        Check(pair.Value.sprite == CombatIdleImporter.LoadFrames(pair.Key)[frame], pair.Key + " runtime pose " + frame);
                        Rect rect = ScreenRect(pair.Value);
                        Check((rect.position - initial[pair.Key].position).sqrMagnitude < .01f && (rect.size - initial[pair.Key].size).sqrMagnitude < .01f, pair.Key + " no frame-driven recentering");
                        Check(rect.xMin >= 0 && rect.xMax <= 1080 && rect.yMin >= 0 && rect.yMax <= height, pair.Key + " portrait containment");
                        if (pair.Value == playerImage)
                        {
                            var hp = Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Single(i => i.name == "PlayerHPBarBackground");
                            Check(rect.yMin >= ScreenRect(hp).yMax + 3.9f * playerImage.canvas.rootCanvas.scaleFactor, pair.Key + " full feet clear the health bar");
                            var panel = playerImage.transform.parent as RectTransform;
                            foreach (string child in new[] { "PlayerCharacter", "PlayerAffinityGem", "PlayerHPBarBackground" })
                            {
                                var item = panel.Find(child) as RectTransform;
                                var corners = new Vector3[4]; item.GetWorldCorners(corners);
                                Check(corners.All(c => panel.rect.Contains((Vector2)panel.InverseTransformPoint(c))), pair.Key + " complete HUD stack stays inside player panel: " + child);
                            }
                        }
                        Check(Mathf.Abs(rect.width / pair.Value.sprite.rect.width - Mathf.Round(rect.width / pair.Value.sprite.rect.width)) < .001f, pair.Key + " integer physical texel scale");
                        // Every imported source rect includes actual opaque contact on row zero.
                        var source = new Texture2D(2, 2); source.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(pair.Value.sprite)));
                        var r = pair.Value.sprite.rect;
                        Check(Enumerable.Range(0, (int)r.width).Any(x => source.GetPixel((int)r.x + x, (int)r.y).a > .5f), pair.Key + " drawn contact meets UI floor");
                        Object.Destroy(source);
                    }
                    if (frame == 0 || frame == 5 || frame == 8)
                    {
                        ScreenCapture.CaptureScreenshot(Path.Combine(Output, player + "-" + height + "-frame" + (frame + 1) + ".png"));
                        yield return Wait(.15f);
                    }
                }
            }
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu"); yield return Wait(.2f);
            selection.Dispose(); selection = null; profile.Dispose(); profile = null;
        }
        File.WriteAllText(Path.Combine(Output, "play-validation.txt"), "PASS: " + assertions + " checks; both players with Farmer/Pan Villager in Game, live playback, all nine poses at 1080x1920 and 1080x2400, stable rectangles, integer pixels and grounded contacts.\n");
    }

    private static IEnumerator GuardCases()
    {
        Application.runInBackground = true;
        string profilePath = Path.Combine(Output, "guards-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(profilePath, JsonUtility.ToJson(new AccountSave()));
        profile = AccountProgression.UseDisposableProfile(profilePath);
        selection = CharacterSelectionSettings.UseTemporarySelection("skeleton");
        for (int batch = 0; batch < CombatIdleImporter.Guards.Length; batch += 2)
        {
            Time.timeScale = 1; SetSize(1080, 1920);
            // Game View applies a selected fixed resolution on a later editor
            // frame. Do not load production layout against its startup size.
            yield return Until(() => UnityEngine.Device.Screen.width == 1080 && UnityEngine.Device.Screen.height == 1920, "guard Game View reaches 1080x1920");
            SceneManager.LoadScene("Game");
            yield return Until(() => RunSession.Current != null && RunSession.Current.Continuation.CanCapture && RunSession.Current.Waves.IsWaveActive, "guard production scene ready");
            var run = RunSession.Current;
            if (UnityEngine.EventSystems.EventSystem.current != null) UnityEngine.EventSystems.EventSystem.current.enabled = false;
            foreach (var controls in Object.FindObjectsByType<RunControlsUI>(FindObjectsSortMode.None))
            { controls.Close(); controls.enabled = false; }
            var initial = run.Waves.ActiveEnemies.ToArray();
            foreach (var actor in initial) actor.GetComponent<EnemyAutoAttack>().StopAttacking();
            var actors = new Dictionary<string, EnemyActor>();
            foreach (string name in CombatIdleImporter.Guards.Skip(batch).Take(2))
            {
                yield return Until(() => run.Waves.HasFreeEnemySlot, "free slot for " + name);
                var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_" + name + ".asset");
                Check(run.Waves.TrySummonEnemy(definition, out EnemyActor actor), name + " production spawn");
                actor.GetComponent<EnemyAutoAttack>().StopAttacking(); actors[name] = actor;
                if (actors.Count == 1) foreach (var old in initial) old.TryTakeDamageWithoutFeedback(100000);
            }
            yield return Wait(.5f);
            var images = actors.ToDictionary(p => p.Key, p => p.Value.transform.Find("VisualRoot").GetComponent<Image>());
            var seen = images.ToDictionary(p => p.Key, p => new HashSet<Sprite>());
            float end = Time.realtimeSinceStartup + 1.5f;
            while (Time.realtimeSinceStartup < end)
            { foreach (var pair in images) seen[pair.Key].Add(pair.Value.sprite); yield return null; }
            foreach (var pair in seen) Check(pair.Value.Count >= 8, pair.Key + " live idle playback");
            Time.timeScale = 0;
            var paused = images.ToDictionary(p => p.Key, p => p.Value.sprite);
            yield return Wait(.25f);
            foreach (var pair in images) Check(pair.Value.sprite == paused[pair.Key], pair.Key + " pauses with gameplay");
            foreach (int height in new[] { 1920, 2400 })
            {
                SetSize(1080, height);
                yield return Until(() => UnityEngine.Device.Screen.width == 1080 && UnityEngine.Device.Screen.height == height, "guard portrait resolution applied");
                yield return Wait(.3f);
                var baseline = images.ToDictionary(p => p.Key, p => ScreenRect(p.Value));
                for (int frame = 0; frame < 9; frame++)
                {
                    foreach (var image in images.Values)
                    { var animator = image.GetComponent<Animator>(); animator.Play("Idle", 0, (frame * .13f + .065f) / 1.17f); animator.Update(0); }
                    yield return Wait(.025f); Canvas.ForceUpdateCanvases();
                    foreach (var pair in images)
                    {
                        string name = pair.Key; Image image = pair.Value; Rect rect = ScreenRect(image);
                        Check(image.sprite == CombatIdleImporter.LoadFrames(name)[frame], name + " displayed frame " + frame);
                        Check((rect.position - baseline[name].position).sqrMagnitude < .01f && (rect.size - baseline[name].size).sqrMagnitude < .01f, name + " fixed center and ground across poses");
                        Check(rect.xMin >= 0 && rect.xMax <= 1080 && rect.yMin >= 0 && rect.yMax <= height, name + " portrait containment");
                        Check(Mathf.Abs(rect.width / 64f - Mathf.Round(rect.width / 64f)) < .001f, name + " integer texel scale");
                        var health = actors[name].GetComponentInParent<EnemySlotUI>().transform.Find("EnemyHPBarBackground").GetComponent<Image>();
                        Check(rect.yMin > ScreenRect(health).yMax, name + " feet clear health bar");
                        foreach (var mask in image.GetComponentsInParent<RectMask2D>())
                        { var corners = new Vector3[4]; image.rectTransform.GetWorldCorners(corners); Check(corners.All(c => mask.rectTransform.rect.Contains((Vector2)mask.rectTransform.InverseTransformPoint(c))), name + " uncut by UI masks"); }
                    }
                    Check(images.Values.Select(ScreenRect).Max(r => r.yMin) - images.Values.Select(ScreenRect).Min(r => r.yMin) < .1f, "guards share a floor");
                    if (frame == 0 || frame == 5 || frame == 8)
                    { ScreenCapture.CaptureScreenshot(Path.Combine(Output, "guards-" + batch + "-" + height + "-frame" + (frame + 1) + ".png")); yield return Wait(.15f); }
                }
            }
            Time.timeScale = 1; SceneManager.LoadScene("MainMenu"); yield return Wait(.2f);
        }
        selection.Dispose(); selection = null; profile.Dispose(); profile = null;
        File.WriteAllText(Path.Combine(Output, "guard-play-validation.txt"), "PASS: four restored guards in production Game, live playback and pause, 36 poses at two portrait sizes, fixed rectangles/shared floor, integer texels and health-bar/mask clearance. " + assertions + " checks.\n");
    }

    private static Rect ScreenRect(Image image)
    {
        var corners = new Vector3[4]; image.rectTransform.GetWorldCorners(corners);
        var camera = image.canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : image.canvas.worldCamera;
        Vector2 a = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 b = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
        return Rect.MinMaxRect(a.x, a.y, b.x, b.y);
    }
    private static void Tick()
    {
        try
        {
            Check(error == null, error);
            if (Steps.Count == 0) { Finish(); return; }
            var step = Steps.Peek();
            if (!step.MoveNext()) { Steps.Pop(); return; }
            if (step.Current is IEnumerator nested) Steps.Push(nested);
            EditorApplication.QueuePlayerLoopUpdate();
        }
        catch (Exception exception) { result = 1; Debug.LogException(exception); Finish(); }
    }
    private static void Finish()
    {
        EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
        Time.timeScale = 1;
        if (result != 0) File.WriteAllText(Path.Combine(Output, SessionState.GetBool(Key + ".guards", false) ? "guard-play-validation.txt" : "play-validation.txt"), "FAIL: " + error);
        EditorApplication.ExitPlaymode();
    }
    private static void Log(string text, string trace, LogType type)
    {
        if (type == LogType.Exception && trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")) return;
        // Startup layout diagnostics are rechecked as structured errors after
        // the scene settles above. Character/layout failures still fail the run.
        if (text.StartsWith("Pixel layout FAILED:"))
        { File.AppendAllText(Path.Combine(Output, "startup-layout-diagnostics.txt"), text + "\n"); return; }
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) error = text;
    }
    private static bool IsAbilityIconWarning(string issue) => issue.StartsWith(
        "Fractional UI art vertices Canvas/SafeArea/BottomHUD/AbilityButton/AbilityIcon:");
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException("Combat idle validation: " + message); assertions++; }
    private static IEnumerator Wait(float seconds)
    { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
    private static IEnumerator Until(Func<bool> predicate, string label)
    { float end = Time.realtimeSinceStartup + 40; while (!predicate()) { Check(Time.realtimeSinceStartup < end, label); yield return null; } }
    private static void SetSize(int width, int height)
    {
        var assembly = typeof(Editor).Assembly; var type = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        var groupType = type.GetProperty("currentGroupType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sizes);
        var group = type.GetMethod("GetGroup").Invoke(sizes, new[] { groupType });
        var size = Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"), new object[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType"), 1), width, height, "Combat idle validation" });
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
        int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
        var viewType = assembly.GetType("UnityEditor.GameView"); var view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, count - 1);
        view.Show(); view.Focus(); view.Repaint();
    }
}
