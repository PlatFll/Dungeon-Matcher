using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Opt-in Play Mode validation. Uses disposable, unregistered actor fixtures;
/// executes real EnemyAutoAttack coroutines and real damage/revive APIs.
/// Does not paint/save scenes, change PlayerPrefs or modify source assets.
/// </summary>
[InitializeOnLoad]
public static class EnemyAttackLifecycleValidation
{
    private const string Pending = "DungeonMatcher.EnemyAttackLifecycleValidation";
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static string runtimeError;
    private static int scenarios;

    static EnemyAttackLifecycleValidation()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem("Dungeon Matcher/Validation/Enemy Attack Lifecycle")]
    public static void Run()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Start Enemy Attack Lifecycle outside Play Mode.");
        if (SceneManager.GetActiveScene().name != "Game")
            throw new InvalidOperationException("Open Game before running Enemy Attack Lifecycle.");
        if (EditorSettings.enterPlayModeOptionsEnabled &&
            (EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableSceneReload) != 0)
            throw new InvalidOperationException("Enable scene reload for isolated Play Mode validation.");
        SessionState.SetBool(Pending, true);
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
        {
            SessionState.SetBool(Pending, false);
            runtimeError = null; scenarios = 0;
            steps.Push(Validate());
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;
        }
        else if (state == PlayModeStateChange.ExitingPlayMode) Stop();
    }
    private static void OnLog(string message, string trace, LogType type)
    {
        if (runtimeError == null && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
            runtimeError = message;
    }
    private static void Tick()
    {
        try
        {
            Check(runtimeError == null, "runtime error: " + runtimeError);
            while (steps.Count > 0)
            {
                IEnumerator step = steps.Peek();
                if (!step.MoveNext())
                {
                    steps.Pop(); (step as IDisposable)?.Dispose(); continue;
                }
                if (step.Current is IEnumerator nested) { steps.Push(nested); continue; }
                return;
            }
            Check(runtimeError == null, "runtime error: " + runtimeError);
            Check(scenarios == 14, "all 14 scenarios completed");
            Debug.Log($"Enemy Attack Lifecycle PASSED ({scenarios} scenarios).");
            Stop(); EditorApplication.ExitPlaymode();
        }
        catch (Exception exception)
        {
            Stop(); Debug.LogException(exception); EditorApplication.ExitPlaymode();
        }
    }
    private static void Stop()
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        while (steps.Count > 0)
        {
            try { (steps.Pop() as IDisposable)?.Dispose(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }
    }

    private static IEnumerator Validate()
    {
        yield return Frames(2);
        Check(Time.timeScale > 0f, "requires an unpaused scene");
        yield return Reenable(false);
        yield return Reenable(true);
        yield return ManualMode();
        yield return InitializeDisabled();
        yield return RepeatedEnable();
        yield return Reservation(false);
        yield return Reservation(true);
        yield return FullStopReset();
        StopRejectsReentrantAttack();
        NullOwnerIsNotAReservation();
        yield return PlayerDeathAndRevive();
        yield return EnemyDeath();
        yield return CompletedCommand(false);
        yield return CompletedCommand(true);
    }

    internal static IEnumerator ValidateForAutomation()
    {
        runtimeError = null;
        scenarios = 0;
        yield return Validate();
        Check(scenarios == 14, "all 14 lifecycle scenarios completed");
    }

    private static IEnumerator Reenable(bool wholeRoot)
    {
        using (var f = new Fixture(true))
        {
            Check(f.Attack.IsRunning, "automatic loop starts on enable after inactive initialization");
            if (wholeRoot) f.Root.SetActive(false); else f.Attack.enabled = false;
            Check(!f.Attack.IsRunning && Get(f.Attack, "attackCoroutine") == null, "disable stops loop");
            int hp = f.Player.CurrentHealth;
            yield return Frames(2);
            if (wholeRoot) f.Root.SetActive(true); else f.Attack.enabled = true;
            Check(f.Attack.IsRunning && f.Attack.RemainingAttackTime > 0f, "enable restores full cooldown loop");
            Check(f.Player.CurrentHealth == hp, "no immediate damage on re-enable with wait-before-first enabled");
            scenarios++;
        }
    }
    private static IEnumerator ManualMode()
    {
        using (var f = new Fixture(false))
        {
            f.Attack.enabled = false; f.Attack.enabled = true;
            f.Root.SetActive(false); f.Root.SetActive(true);
            yield return Frames(2);
            Check(!f.Attack.IsRunning && Get(f.Attack, "attackCoroutine") == null, "manual mode stays stopped");
            f.Attack.TryStartAttacking();
            Check(f.Attack.IsRunning, "explicit manual start remains supported");
            f.Attack.StopAttacking();
            yield return Frames(2);
            Check(!f.Attack.IsRunning, "explicit Stop does not auto-restart every frame");
            scenarios++;
        }
    }
    private static IEnumerator InitializeDisabled()
    {
        using (var f = new Fixture(false))
        {
            f.Attack.enabled = false;
            Set(f.Attack, "attackAutomatically", true);
            f.Attack.Initialize(f.Actor, f.Player);
            Check(!f.Attack.IsRunning, "initialization while disabled does not start coroutine");
            f.Attack.enabled = true;
            yield return Frames(1);
            Check(f.Attack.IsRunning, "first subsequent enable starts initialized automatic attacker");
            scenarios++;
        }
    }
    private static IEnumerator RepeatedEnable()
    {
        using (var f = new Fixture(true))
        {
            for (int cycle = 0; cycle < 5; cycle++)
            {
                f.Attack.enabled = false; f.Attack.enabled = true;
                object loop = Get(f.Attack, "attackCoroutine");
                Check(loop != null, "one restored loop");
                f.Attack.TryStartAttacking(); f.Attack.TryStartAttacking();
                Check(ReferenceEquals(loop, Get(f.Attack, "attackCoroutine")), "repeated starts cannot duplicate loop");
                yield return Frames(1);
            }
            scenarios++;
        }
    }
    private static IEnumerator Reservation(bool makeReady)
    {
        using (var f = new Fixture(true))
        {
            object owner = new object();
            float stored = f.Attack.RemainingAttackTime;
            Check(f.Attack.TryReserveCommand(owner, makeReady), "command reserved");
            float reserved = f.Attack.RemainingAttackTime;
            f.Attack.TryStartAttacking();
            Check(Get(f.Attack, "attackCoroutine") == null, "start cannot run normal countdown under reservation");
            yield return Frames(2);
            Check(f.Attack.RemainingAttackTime == reserved, "reserved timer remains frozen");
            f.Attack.ReleaseCommand(new object());
            Check(f.Attack.IsCommandReservedBy(owner), "wrong owner cannot release");
            f.Attack.ReleaseCommand(owner);
            Check(f.Attack.IsRunning && !f.Attack.IsCommandReservedBy(owner), "matching release resumes loop");
            Check(f.Attack.RemainingAttackTime > 0f && f.Attack.RemainingAttackTime <= stored,
                "unspent release resumes stored cooldown rather than reset or immediate extra hit");
            scenarios++;
        }
    }
    private static IEnumerator FullStopReset()
    {
        using (var f = new Fixture(false))
        {
            object owner = new object();
            Set(f.Attack, "remainingAttackTime", 4f);
            Check(f.Attack.TryReserveCommand(owner, true), "manual-mode reservation accepted");
            f.Attack.ReleaseCommand(owner);
            Check((bool)Get(f.Attack, "resumeCooldown"), "release staged legitimate cooldown resume");
            f.Attack.StopAttacking();
            foreach (string field in new[] { "resumeCooldown", "commandStrike", "commandMadeReady", "commandedAttackStarting" })
                Check(!(bool)Get(f.Attack, field), "full stop clears " + field);
            Check((float)Get(f.Attack, "reservedAttackTime") == 0f, "full stop clears saved timer");
            Check((float)Get(f.Attack, "commandDamageMultiplier") == 1f, "temporary command multiplier reset");
            Set(f.Attack, "attackAutomatically", true);
            f.Attack.Initialize(f.Actor, f.Player);
            Check(f.Attack.RemainingAttackTime > 4f, "new initialization uses full interval, not staged zero/stored resume");
            yield return Frames(1);
            scenarios++;
        }
    }
    private static void StopRejectsReentrantAttack()
    {
        using (var f = new Fixture(false))
        {
            Check(f.Actor.TryBeginAutoAttackAnimationAction(), "fixture holds an actor action");
            bool called = false, reserved = false, attacked = false;
            Action<EnemyActor> callback = _ =>
            {
                called = true;
                f.Attack.TryStartAttacking();
                reserved = f.Attack.TryReserveCommand(new object());
                attacked = f.Attack.PerformAttackImmediately();
                f.Attack.StopAttacking();
            };
            f.Actor.AnimationActionReleased += callback;
            int before = f.Player.CurrentHealth;
            try { f.Attack.StopAttacking(); }
            finally { f.Actor.AnimationActionReleased -= callback; }
            Check(called && !reserved && !attacked, "synchronous release cannot start or reserve during Stop");
            Check(!f.Attack.IsRunning && Get(f.Attack, "attackCoroutine") == null &&
                f.Player.CurrentHealth == before, "stop leaves no reentrant loop or damage");
            scenarios++;
        }
    }
    private static void NullOwnerIsNotAReservation()
    {
        using (var f = new Fixture(false))
        {
            Check(!f.Attack.IsCommandReservedBy(null), "null is not an owner");
            Check(!f.Attack.PerformCommandStrike(null), "null cannot perform unreserved command");
            f.Attack.ReleaseCommand(null);
            Check(!(bool)Get(f.Attack, "resumeCooldown"), "null release cannot stage a resume");
            scenarios++;
        }
    }
    private static IEnumerator PlayerDeathAndRevive()
    {
        using (var f = new Fixture(false))
        {
            object owner = new object();
            Check(f.Attack.TryReserveCommand(owner) && f.Attack.PerformCommandStrike(owner), "pending command accepted");
            yield return Until(() => f.Attack.ActiveAttackPresentationId > 0, "command impact wait");
            int stale = f.Attack.ActiveAttackPresentationId;
            f.Player.TryTakeDamage(f.Player.CurrentHealth + 100);
            Check(!f.Attack.IsAttackSequenceInProgress && !f.Attack.IsCommandReservedBy(owner), "defeat cancels sequence/reservation");
            Check(!f.Attack.ResolvePresentationImpact(stale) && !f.Attack.CompleteAttackPresentation(stale), "old callbacks rejected");
            Set(f.Attack, "attackAutomatically", true);
            Check(f.Player.TryRevive(100), "real revive accepted");
            Check(f.Attack.IsRunning && f.Attack.RemainingAttackTime > 0f, "revive restarts configured countdown");
            int health = f.Player.CurrentHealth;
            Check(!f.Attack.ResolvePresentationImpact(stale), "pre-death callback cannot hit revived player");
            yield return Frames(2);
            Check(f.Player.CurrentHealth == health, "no immediate stored command hit after revive");
            scenarios++;
        }
    }
    private static IEnumerator EnemyDeath()
    {
        using (var f = new Fixture(false))
        {
            object owner = new object();
            Check(f.Attack.TryReserveCommand(owner) && f.Attack.PerformCommandStrike(owner), "enemy pending command");
            yield return Until(() => f.Attack.ActiveAttackPresentationId > 0, "enemy impact wait");
            int stale = f.Attack.ActiveAttackPresentationId;
            f.Actor.TryTakeDamage(1000);
            Set(f.Attack, "attackAutomatically", true);
            f.Attack.enabled = false; f.Attack.enabled = true;
            f.Attack.TryStartAttacking();
            Check(f.Actor.IsDefeated && !f.Attack.IsRunning, "dead enemy cannot restart on enable");
            Check(!f.Attack.ResolvePresentationImpact(stale), "dead enemy's impact rejected");
            scenarios++;
        }
    }
    private static IEnumerator CompletedCommand(bool followUp)
    {
        using (var f = new Fixture(true, followUp ? 7 : 0))
        {
            object owner = new object();
            Check(f.Attack.TryReserveCommand(owner, true) && f.Attack.PerformCommandStrike(owner), "completed command starts");
            int hp = f.Player.CurrentHealth, previousId = 0;
            foreach (int damage in followUp ? new[] { 5, 7 } : new[] { 5 })
            {
                int old = previousId;
                yield return Until(() => f.Attack.ActiveAttackPresentationId > 0 && f.Attack.ActiveAttackPresentationId != old,
                    "next command hit");
                previousId = f.Attack.ActiveAttackPresentationId;
                int before = f.Player.CurrentHealth;
                Check(f.Attack.ResolvePresentationImpact(previousId), "one real impact accepted");
                Check(f.Player.CurrentHealth == before - damage, "separate expected damage instance");
                Check(!f.Attack.ResolvePresentationImpact(previousId), "duplicate impact rejected");
                Check(f.Attack.IsAttackSequenceInProgress, "command owns action until return acknowledgement");
                Check(f.Attack.CompleteAttackPresentation(previousId), "return acknowledgement accepted");
            }
            yield return Until(() => !f.Attack.IsAttackSequenceInProgress, "full command completion");
            f.Attack.ReleaseCommand(owner);
            Check(f.Attack.IsRunning && f.Attack.RemainingAttackTime > 0f, "consumed command starts full normal cooldown");
            yield return Frames(2);
            Check(f.Player.CurrentHealth == hp - (followUp ? 12 : 5), "no stored immediate extra normal attack");
            scenarios++;
        }
    }

    private sealed class Fixture : IDisposable
    {
        public readonly GameObject Root;
        public readonly PlayerActor Player;
        public readonly EnemyActor Actor;
        public readonly EnemyAutoAttack Attack;
        private readonly EnemyDefinition definition;
        public Fixture(bool automatic, int followUp = 0)
        {
            Root = new GameObject("AttackLifecycleFixture"); Root.SetActive(false);
            definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            var playerObject = new GameObject("FixturePlayer"); playerObject.transform.SetParent(Root.transform);
            Player = playerObject.AddComponent<PlayerActor>();
            Set(Player, "initializeOnStart", false); Set(Player, "isInitialized", true);
            Set(Player, "maximumHealth", 100); Set(Player, "currentHealth", 100);
            var enemyObject = new GameObject("FixtureEnemy"); enemyObject.transform.SetParent(Root.transform);
            Actor = enemyObject.AddComponent<EnemyActor>();
            // Isolated actor snapshot avoids installing optional visuals. The
            // authority under test is EnemyAutoAttack.Initialize + its coroutines.
            Set(Actor, "definition", definition); Set(Actor, "isInitialized", true); Set(Actor, "currentHealth", 100);
            Set(Actor, "<RuntimeStats>k__BackingField", new EnemyRuntimeStats(1, 1, 100, 5, followUp, 10f, 3));
            enemyObject.AddComponent<EnemyStagger>().enabled = false;
            Attack = enemyObject.AddComponent<EnemyAutoAttack>();
            Set(Attack, "attackAutomatically", automatic);
            Set(Attack, "animationImpactTimeout", 1000f); // No visual presenter; tests explicitly acknowledge each hit.
            Attack.Initialize(Actor, Player);
            Root.SetActive(true);
        }
        public void Dispose()
        {
            if (Root != null) { Root.SetActive(false); UnityEngine.Object.DestroyImmediate(Root); }
            if (definition != null) UnityEngine.Object.DestroyImmediate(definition);
        }
    }
    private static IEnumerator Frames(int count)
    {
        int target = Time.frameCount + count;
        yield return Until(() => Time.frameCount >= target, "rendered frames");
    }
    private static IEnumerator Until(Func<bool> condition, string label)
    {
        double deadline = EditorApplication.timeSinceStartup + 30d;
        while (!condition())
        {
            Check(runtimeError == null, "runtime error: " + runtimeError);
            Check(EditorApplication.timeSinceStartup < deadline, "timeout: " + label);
            yield return null;
        }
    }
    private static object Get(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, Flags);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        return field.GetValue(target);
    }
    private static void Set(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(name, Flags);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        field.SetValue(target, value);
    }
    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("Enemy Attack Lifecycle: " + label);
    }
}
