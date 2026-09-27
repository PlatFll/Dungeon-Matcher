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
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Opt-in graphics-enabled production scene review, following CombatIdleValidation.
[InitializeOnLoad]
public static class GideonGlassVisualValidation
{
    private const string Key = "DungeonMatcher.GideonVisualReview";
    private static readonly string Output = Path.GetFullPath(".utmp/GideonVisuals");
    private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    private static IDisposable profile, selection;
    private static string error;
    private static int checks;
    private static readonly Dictionary<string, Rect> CharacterRects = new Dictionary<string, Rect>();

    static GideonGlassVisualValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                error = null; checks = 0; Steps.Clear(); CharacterRects.Clear();
                Application.logMessageReceived += Log;
                Steps.Push(Cases()); EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Key, false);
                EditorApplication.Exit(SessionState.GetInt(Key + ".result", 1));
            }
        };
    }
    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use an isolated graphics-enabled batch editor.");
        Directory.CreateDirectory(Output);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetInt(Key + ".result", 1); SessionState.SetBool(Key, true);
        SetSize(720, 1280); EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        try
        {
            Check(error == null, error);
            if (Steps.Count == 0) { Finish(true); return; }
            var step = Steps.Peek();
            if (!step.MoveNext()) { Steps.Pop(); return; }
            if (step.Current is IEnumerator nested) Steps.Push(nested);
            EditorApplication.QueuePlayerLoopUpdate();
        }
        catch (Exception exception) { error = exception.ToString(); Debug.LogException(exception); Finish(false); }
    }
    private static void Finish(bool success)
    {
        EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
        Time.captureDeltaTime = 0; Time.timeScale = 1;
        File.WriteAllText(Path.Combine(Output, "validation.txt"), success
            ? "PASS: " + checks + " checks. Three characters and current enemies at 720x1280 / 1080x2400; live idle/cast, real five-move hold/rewind/recovery, native frame sampling and menu.\n"
            : "FAIL: " + error);
        SessionState.SetInt(Key + ".result", success ? 0 : 1);
        profile?.Dispose(); selection?.Dispose(); profile = selection = null;
        EditorApplication.ExitPlaymode();
    }
    private static void Log(string text, string trace, LogType type)
    {
        if (text.StartsWith("Pixel layout FAILED:"))
        { File.AppendAllText(Path.Combine(Output, "startup-layout.txt"), text + "\n"); return; }
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) error = text;
    }
    private static IEnumerator Cases()
    {
        Application.runInBackground = true;
        foreach (string playerId in new[] { "skeleton", "bardley", "gideon_glass" })
        {
            string path = Path.Combine(Output, playerId + "-profile.json");
            File.WriteAllText(path, JsonUtility.ToJson(new AccountSave { gold = 200 }));
            profile = AccountProgression.UseDisposableProfile(path);
            selection = CharacterSelectionSettings.UseTemporarySelection(playerId);
            SceneManager.LoadScene("Game"); yield return Stable();
            var run = RunSession.Current;
            foreach (string enemyName in new[] { "Farmer", "PanVillager" })
            {
                var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_" + enemyName + ".asset");
                if (!run.Waves.ActiveEnemies.Any(e => e.Definition == definition))
                    Check(run.Waves.TrySummonEnemy(definition, out _), "summon " + enemyName);
            }
            yield return Stable();
            foreach (var enemy in run.Waves.ActiveEnemies)
            {
                enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
                typeof(EnemyActor).GetField("currentHealth", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(enemy, 100000);
            }
            var actor = Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Single(i => i.name == "PlayerCharacter");
            var animator = actor.GetComponent<Animator>();
            var seen = new HashSet<Sprite>(); float until = Time.realtimeSinceStartup + 1.5f;
            while (Time.realtimeSinceStartup < until) { seen.Add(actor.sprite); yield return null; }
            Check(seen.Count >= 7, playerId + " live idle animates");
            foreach (var size in new[] { new Vector2Int(720,1280), new Vector2Int(1080,2400) })
            {
                SetSize(size.x, size.y); yield return Wait(.3f);
                string prefix = playerId + "-" + size.y;
                Time.timeScale = 0; animator.Play("Idle", 0, 0); animator.Update(0);
                Canvas.ForceUpdateCanvases(); var rect = ScreenRect(actor.rectTransform);
                Check(Mathf.Abs(rect.width / 64 - Mathf.Round(rect.width / 64)) < .001f, prefix + " integer pixels");
                var hp = Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Single(i => i.name == "PlayerHPBarBackground");
                Check(rect.yMin >= ScreenRect(hp.rectTransform).yMax, prefix + " planted contact clears health bar");
                string sizeKey = size.y.ToString();
                if (CharacterRects.TryGetValue(sizeKey, out var prior))
                    Check((prior.position - rect.position).sqrMagnitude < .01f && (prior.size - rect.size).sqrMagnitude < .01f, prefix + " shared cast scale and baseline");
                else CharacterRects.Add(sizeKey, rect);
                File.AppendAllText(Path.Combine(Output, "rectangles.txt"), prefix + " " + rect + "\n");
                yield return Shot(prefix + "-idle");
                Time.timeScale = 1;
                if (playerId != "gideon_glass") continue;
                run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
                Check(run.Player.GetComponent<PlayerAbilityController>().TryActivate(), "real cast accepted");
                var ability = run.Player.GetComponent<ChronoShutterRuntime>();
                var castFrames = new HashSet<string>();
                while (ability.Phase == BoardMemoryPhase.Casting) { castFrames.Add(actor.sprite.name); yield return null; }
                Check(castFrames.Contains("Gideon_Cast_05"), "live cast displays the single flash frame");
                Check(ability.Phase == BoardMemoryPhase.Holding, "cast enters dedicated hold");
                var counter = actor.GetComponentInChildren<Text>();
                Check(counter != null && counter.name == "AbilityMoveCounter" && counter.text == "5", "counter starts at five");
                var affinity = Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Single(i => i.name == "PlayerAffinityGem");
                var counterRect = ScreenRect(counter.rectTransform);
                Check(!counterRect.Overlaps(ScreenRect(affinity.rectTransform)), "counter clears affinity icon");
                Check(counterRect.yMin >= rect.yMax && counterRect.xMin >= 0 && counterRect.xMax <= size.x && counterRect.yMax <= size.y, "counter sits above head inside screen");
                for (int remaining = 5; remaining >= 0; remaining--)
                {
                    Check(counter.text == remaining.ToString(), "real counter value " + remaining);
                    Check(actor.sprite.name == "Gideon_Hold_00", "hold pose remains through countdown");
                    Time.timeScale = 0; yield return Shot(prefix + "-hold-" + remaining); Time.timeScale = 1;
                    if (remaining == 0) break;
                    Check(run.Board.TryGetRandomHintMove(out var a, out var b), "real playable move");
                    run.Board.ReplayPlayerSwap(a.Column,a.Row,b.Column,b.Row);
                    int next = remaining - 1;
                    yield return Until(() => ability.RemainingMoves == next, "move accepted");
                    if (next > 0) yield return Stable();
                }
                yield return Until(() => ability.Phase == BoardMemoryPhase.Rewinding, "fifth move settles before rewind");
                Time.timeScale = 0; yield return Shot(prefix + "-rewind"); Time.timeScale = 1;
                yield return Until(() => !ability.IsActive, "recovery returns to idle");
                Check(!counter.gameObject.activeSelf, "counter hides after recovery");
                Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"), "idle state returns");
                yield return Shot(prefix + "-returned-idle");

                // Sample the imported presentation frames in the real scene for visual QA.
                Time.timeScale = 0;
                foreach (var sample in new[] { new Vector2(.43f,5), new Vector2(.49f,6), new Vector2(.59f,7) })
                {
                    animator.Play("Ability",0,sample.x / .85f); animator.Update(0);
                    Check(actor.sprite.name == "Gideon_Cast_" + ((int)sample.y).ToString("00"), "imported flash/steam frame");
                    yield return Shot(prefix + "-cast-frame" + (int)sample.y);
                }
                animator.Play("Idle",0,0); animator.Update(0); Time.timeScale = 1;
            }
            Check(run.ExitTo("MainMenu"), "exit disposable run"); yield return Wait(.25f);
            if (playerId == "gideon_glass")
            {
                Object.FindFirstObjectByType<MainMenuController>().ShowCharacterSelect(); yield return Wait(.2f);
                foreach (var size in new[] { new Vector2Int(720,1280), new Vector2Int(1080,2400) })
                { SetSize(size.x,size.y); yield return Wait(.3f); yield return Shot("gideon-menu-" + size.y); }
            }
            selection.Dispose(); profile.Dispose(); selection = profile = null;
        }
    }
    private static IEnumerator Shot(string name)
    { yield return Wait(.08f); ScreenCapture.CaptureScreenshot(Path.Combine(Output,name + ".png")); yield return Wait(.16f); }
    private static IEnumerator Stable() => Until(() => RunSession.Current != null && RunSession.Current.Continuation.CanCapture && RunSession.Current.Waves.IsWaveActive, "settled combat");
    private static IEnumerator Until(Func<bool> predicate, string label)
    { float end = Time.realtimeSinceStartup + 45; while (!predicate()) { Check(Time.realtimeSinceStartup < end,label); yield return null; } }
    private static IEnumerator Wait(float duration)
    { float end = Time.realtimeSinceStartup + duration; while (Time.realtimeSinceStartup < end) yield return null; }
    private static void Check(bool value, string label)
    { if (!value) throw new InvalidOperationException("Gideon visual review: " + label); checks++; }
    private static Rect ScreenRect(RectTransform rect)
    {
        var corners = new Vector3[4]; rect.GetWorldCorners(corners);
        var a = RectTransformUtility.WorldToScreenPoint(null,corners[0]);
        var b = RectTransformUtility.WorldToScreenPoint(null,corners[2]);
        return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
    }
    private static void SetSize(int width, int height)
    {
        var assembly = typeof(Editor).Assembly; var type = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
        var groupType = type.GetProperty("currentGroupType",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(sizes);
        var group = type.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});
        var size = Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"),new object[]{Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType"),1),width,height,"Gideon review"});
        group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
        var viewType = assembly.GetType("UnityEditor.GameView"); var view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(view,count-1);
        view.Show(); view.Focus(); view.Repaint();
    }
}
