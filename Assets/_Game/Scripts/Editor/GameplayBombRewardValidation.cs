using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runs against a fresh Game-scene Play Mode instance, never authored assets or
/// PlayerPrefs. Production board coroutines run on Unity, not the editor pump.
/// </summary>
[InitializeOnLoad]
public static class GameplayBombRewardValidation
{
    private const string Pending = "DungeonMatcher.GameplayBombRewardValidation";
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static BoardController board;
    private static CombatController combat;
    private static WaveController waves;
    private static PlayerActor player;
    private static EnemyActor enemy;
    private static string runtimeError;
    private static int scenarios;

    static GameplayBombRewardValidation()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem("Dungeon Matcher/Validation/Gameplay Bomb Rewards")]
    public static void Run()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Start Gameplay Bomb Rewards outside Play Mode.");
        if (SceneManager.GetActiveScene().name != "Game")
            throw new InvalidOperationException("Open the Game scene before running Gameplay Bomb Rewards.");
        if (EditorSettings.enterPlayModeOptionsEnabled &&
            (EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableSceneReload) != 0)
            throw new InvalidOperationException("Enable scene reload for this isolated Play Mode validation.");

        SessionState.SetBool(Pending, true);
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
        {
            SessionState.SetBool(Pending, false);
            runtimeError = null;
            scenarios = 0;
            steps.Push(Validate());
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Stop();
        }
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
                    steps.Pop();
                    (step as IDisposable)?.Dispose();
                    continue;
                }
                if (step.Current is IEnumerator nested)
                {
                    steps.Push(nested);
                    continue;
                }
                return;
            }
            Check(runtimeError == null, "runtime error: " + runtimeError);
            Debug.Log($"Gameplay bomb reward validation PASSED ({scenarios} scenarios).");
            Stop();
            EditorApplication.ExitPlaymode();
        }
        catch (Exception exception)
        {
            Stop();
            Debug.LogException(exception);
            EditorApplication.ExitPlaymode();
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
        board = null; combat = null; waves = null; player = null; enemy = null;
    }

    private static IEnumerator Validate()
    {
        yield return Until(() =>
        {
            board = UnityEngine.Object.FindFirstObjectByType<BoardController>();
            combat = UnityEngine.Object.FindFirstObjectByType<CombatController>();
            waves = UnityEngine.Object.FindFirstObjectByType<WaveController>();
            player = UnityEngine.Object.FindFirstObjectByType<PlayerActor>();
            return board != null && combat != null && waves != null && player != null &&
                   player.IsInitialized && !board.IsBusy && waves.IsWaveActive &&
                   !(bool)Get(waves, "isSpawningWave") && waves.ActiveEnemies.Count > 0;
        }, "initialized battle");
        Check(Time.timeScale > 0f, "validation requires an unpaused battle");
        Check(board.Width >= 5 && board.Height >= 5, "fixture needs a 5x5 playable area");

        // All changes below belong only to the disposable Play Mode scene.
        Set(waves, "advanceWavesAutomatically", false);
        enemy = waves.ActiveEnemies[0];
        foreach (EnemyActor actor in waves.ActiveEnemies)
        {
            actor.GetComponent<EnemyAutoAttack>()?.StopAttacking();
            actor.SetSpecialTurnRequirement(1000000);
            Set(actor, "currentHealth", 1000000);
        }

        using (board.AcquireExternalInputBlock())
        {
            ValidateCrossEnergy();
            ValidateAffinityHealingUpgrade();
            Set(player, "maximumHealth", 100000);
            Set(player, "currentHealth", 50000);

            GemSpecialType[] bombs =
            {
                GemSpecialType.HealingBomb, GemSpecialType.ShieldBomb, GemSpecialType.PoisonBomb
            };
            GemSpecialType[] rewards =
            {
                GemSpecialType.RowBomb, GemSpecialType.ColumnBomb, GemSpecialType.ColorCrystal,
                GemSpecialType.HealingBomb, GemSpecialType.ShieldBomb, GemSpecialType.PoisonBomb
            };
            foreach (GemSpecialType bomb in bombs)
            {
                // First use the player's ACTUAL mastery selection, read-only.
                foreach (BoardMatchType shape in new[] { BoardMatchType.StraightFive,
                    BoardMatchType.LShape, BoardMatchType.TShape, BoardMatchType.CrossShape })
                    yield return PreservedBomb(bomb, null, true, false, shape);
                foreach (GemSpecialType reward in rewards)
                    yield return PreservedBomb(bomb, reward, true, true);
                yield return PreservedBomb(bomb, GemSpecialType.RowBomb, false, true);
            }
            foreach (BoardMatchType shape in new[] { BoardMatchType.StraightFive,
                BoardMatchType.LShape, BoardMatchType.TShape, BoardMatchType.CrossShape })
                yield return PreservedBomb(GemSpecialType.None, null, true, false, shape);

            // End the encounter through its real damage/death completion path.
            foreach (EnemyActor actor in new List<EnemyActor>(waves.ActiveEnemies))
                actor.TryTakeDamage(10000000);
            yield return Until(() => !waves.IsWaveActive && !board.IsBusy, "wave completion and cleanup");
            Check(waves.ActiveEnemies.Count == 0, "completed encounter has no living enemies");
            enemy = null;

            yield return PreservedBomb(GemSpecialType.HealingBomb, GemSpecialType.RowBomb, true, true);
            yield return PreservedBomb(GemSpecialType.ShieldBomb, GemSpecialType.RowBomb, true, true);
            ValidateInactiveWaveAndCaps();
            yield return ValidateWaveTransition(false, 0f);
            yield return ValidateWaveTransition(false, 0.15f);
            yield return ValidateWaveTransition(true, 0f);
            yield return ValidateWaveTransition(true, 0.15f);
        }
    }

    private static void ValidateCrossEnergy()
    {
        var gain = player.GetComponent<PlayerAbilityMatchEnergyGain>();
        Check(gain != null, "energy producer exists");
        foreach (bool damaged in new[] { false, true })
        {
            var cross = new BoardClearContext(GemType.Ruby, 4, 0, BoardClearSource.Match,
                BoardMatchType.CrossShape, triggerGemCount: 5);
            var tee = new BoardClearContext(GemType.Ruby, 4, 0, BoardClearSource.Match,
                BoardMatchType.TShape, triggerGemCount: 5);
            Check(gain.CalculateEnergyGain(new BoardClearOutcome(cross, damaged)) ==
                  gain.CalculateEnergyGain(new BoardClearOutcome(tee, damaged)),
                "Cross retains its prior T-family energy, damaged=" + damaged);
        }
        var ability = new BoardClearContext(GemType.Ruby, 4, 0, BoardClearSource.Ability);
        Check(gain.CalculateEnergyGain(new BoardClearOutcome(ability, true)) == 0,
            "ordinary Ability source remains ineligible for special energy");
        scenarios++;
    }

    private static void ValidateAffinityHealingUpgrade()
    {
        var affinity = UnityEngine.Object.FindFirstObjectByType<PlayerAffinityHealing>();
        RunUpgradeRuntime runtime = RunUpgradeRuntime.Current;
        var remedy = Resources.Load<RunUpgradeDefinition>("RunUpgrades/Prototype_StrongRemedy");
        Check(affinity != null && runtime != null && remedy != null, "healing fixture dependencies");
        runtime.ResetRun();
        var context = new BoardClearContext(player.Definition.AffinityGemType, 10, 0, BoardClearSource.Match);
        int baseline = affinity.CalculateHealing(context);
        Check(baseline > 0, "positive baseline affinity healing");
        try
        {
            Check(runtime.TryApply(remedy, 5), "global healing upgrade accepted");
            int expected = RunUpgradeResolver.ResolveHealing(baseline);
            Check(expected > baseline, "global upgrade increases healing");
            Check(affinity.CalculateHealing(context) == expected, "affinity modifier applied exactly once");
            Check(player.MaximumHealth > expected, "healing fixture has enough health capacity");
            Set(player, "currentHealth", player.MaximumHealth - expected);
            int before = player.CurrentHealth;
            Call(affinity, "HandleBoardClearResolved", context);
            Check(player.CurrentHealth - before == expected, "actual affinity HP reward uses modifier");
            Check(affinity.CalculateHealing(new BoardClearContext(context.GemType, 0, 0, BoardClearSource.Match)) == 0,
                "zero cleared gems never grant healing");
            var flask = Resources.Load<RunUpgradeDefinition>("RunUpgrades/RunUpgrade_ReinforcedFlask");
            Check(flask != null && runtime.TryApply(flask, 5), "bomb-specific healing upgrade accepted");
            Check(affinity.CalculateHealing(context) == expected, "bomb-specific modifier does not affect affinity");
            int bombExpected = Mathf.RoundToInt(Mathf.RoundToInt((int)Get(combat, "healingBombHealAmount") * 1.2f) * 1.3f);
            Check(player.MaximumHealth > bombExpected, "bomb healing fixture has enough capacity");
            Set(player, "currentHealth", player.MaximumHealth - bombExpected);
            Check(combat.HealPlayerFromBomb() == bombExpected,
                "Healing Bomb retains one global and one bomb-specific modifier");
        }
        finally { runtime.ResetRun(); }
        Check(affinity.CalculateHealing(context) == baseline, "no-upgrade behavior restored");
        scenarios++;
    }

    private static IEnumerator PreservedBomb(GemSpecialType oldType, GemSpecialType? overrideReward,
        bool activateSpecials, bool chain, BoardMatchType shape = BoardMatchType.CrossShape)
    {
        ResetBoard();
        Set(player, "currentHealth", 50000);
        Set(player, "currentShield", 0);
        Gem first = At(3, 3), second = At(4, 3);
        var match = new HashSet<Gem> { first, second, At(2, 3), At(3, 2), At(3, 4) };
        if (shape == BoardMatchType.StraightFive)
            match = new HashSet<Gem> { first, second, At(0, 3), At(1, 3), At(2, 3) };
        else if (shape == BoardMatchType.LShape)
            match = new HashSet<Gem> { first, second, At(2, 3), At(2, 2), At(2, 1) };
        else if (shape == BoardMatchType.TShape)
            match = new HashSet<Gem> { first, second, At(2, 3), At(3, 2), At(3, 1) };
        Sprite[] sprites = (Sprite[])Get(board, "gemSprites");
        foreach (Gem gem in match) gem.SetType(GemType.Ruby, sprites[(int)GemType.Ruby]);
        first.SetSpecialType(oldType);
        if (chain) second.SetSpecialType(oldType);

        var requests = (List<SpecialGemCreationRequest>)Call(board, "BuildSpecialGemCreationRequests", match, first, null);
        Check(requests.Count == 1 && requests[0].GemToPreserve == first,
            "real shape selection preserves the existing bomb cell");
        Check((BoardMatchType)Call(board, "DetermineMatchType", new List<Gem>(match), true) == shape,
            "fixture classified as " + shape);
        Check(requests[0].SpecialType == GemMasteryRuntimeResolver.ResolveSpecialType(shape),
            "actual saved mastery controls " + shape + " creation");
        GemSpecialType reward = overrideReward ?? requests[0].SpecialType;
        requests[0] = new SpecialGemCreationRequest(first, reward);

        EnemyPoisonStatus status = enemy != null ? enemy.GetComponent<EnemyPoisonStatus>() : null;
        if (enemy != null && status == null) status = enemy.gameObject.AddComponent<EnemyPoisonStatus>();
        status?.ClearPoison();
        int effects = 0;
        void Record()
        {
            effects++;
            Check(first.SpecialType == oldType, "old bomb effect commits BEFORE replacement type assignment");
            if (chain)
                foreach (SpriteRenderer renderer in second.GetComponentsInChildren<SpriteRenderer>())
                    Check(!renderer.enabled, "chained bomb effect waits for the shatter moment");
        }
        Action<PlayerActor, int> healed = (_, __) => Record();
        Action<PlayerActor, int, int> shield = (_, __, ___) =>
        {
            Record();
            // Allow counting a second grant without changing the production cap.
            Set(player, "currentShield", 0);
        };
        Action<EnemyPoisonStatus, bool> poisoned = (_, __) => Record();
        // For an ordinary preserved gem, listen to all utility channels so a
        // newly created bomb cannot silently activate during its own creation.
        if (oldType == GemSpecialType.HealingBomb || oldType == GemSpecialType.None) player.Healed += healed;
        if (oldType == GemSpecialType.ShieldBomb || oldType == GemSpecialType.None) player.ShieldChanged += shield;
        if (status != null && (oldType == GemSpecialType.PoisonBomb || oldType == GemSpecialType.None))
            status.PoisonApplied += poisoned;
        try
        {
            object[] args = { match, true, null };
            var expanded = (HashSet<Gem>)Call(board, "BuildBombExpandedClearSet", args);
            Call(board, "BuildBombExpandedClearSet", args);
            Check(effects == 0, "repeated blast planning is effect-free");
            Check(expanded.Contains(first), "preserved bomb belongs to authoritative activation set");
            if (oldType != GemSpecialType.None)
                for (int y = 2; y <= 4; y++)
                    for (int x = 2; x <= 4; x++)
                        Check(expanded.Contains(At(x, y)), "old bomb retains its complete 3x3 footprint");
            yield return BoardRoutine((IEnumerator)Call(board, "ClearMatches", expanded, requests, activateSpecials));
            int expected = activateSpecials && oldType != GemSpecialType.None ? (chain ? 2 : 1) : 0;
            Check(effects == expected, $"{oldType} -> {reward}: expected {expected} commits, got {effects}");
            Check(first != null && At(3, 3) == first && first.SpecialType == reward,
                "new reward survives at the selected cell without self-activation");
        }
        finally
        {
            player.Healed -= healed;
            player.ShieldChanged -= shield;
            if (status != null) status.PoisonApplied -= poisoned;
        }
        // Remove the fixture reward before refill so random cascades cannot
        // confound a later case. This is test setup, not a gameplay mutation.
        first.SetSpecialType(GemSpecialType.None);
        yield return BoardRoutine((IEnumerator)Call(board, "ResolveEnvironmentalBoardChange"));
        scenarios++;
    }

    private static void ValidateInactiveWaveAndCaps()
    {
        Check(!waves.IsWaveActive, "utility rewards tested after encounter ended");
        Set(player, "currentHealth", 50000);
        Set(player, "currentShield", 0);
        Check(combat.HealPlayerFromBomb() > 0, "healing utility needs no living enemy");
        Check(combat.GrantPlayerShieldFromBomb() > 0, "shield utility needs no living enemy");
        Check(combat.ApplyPoisonToAllEnemies() == 0, "inactive wave cannot receive poison");
        Check(!combat.ResolveFixedGemDamage(new BoardClearContext(GemType.Ruby, 1, 0, BoardClearSource.Bomb), 10),
            "enemy damage remains encounter-bound");

        Set(player, "currentHealth", player.MaximumHealth);
        Set(player, "currentShield", player.MaximumShield);
        Check(combat.HealPlayerFromBomb() == 0 && combat.GrantPlayerShieldFromBomb() == 0,
            "full caps legitimately give zero actual reward");
        Set(player, "currentHealth", 50000);
        Set(player, "currentShield", 0);
        Set(player, "isDefeated", true);
        try
        {
            Check(combat.HealPlayerFromBomb() == 0 && combat.GrantPlayerShieldFromBomb() == 0,
                "bombs cannot revive a defeated player");
        }
        finally { Set(player, "isDefeated", false); }
        Set(player, "isInitialized", false);
        try
        {
            Check(combat.HealPlayerFromBomb() == 0 && combat.GrantPlayerShieldFromBomb() == 0,
                "uninitialized player cannot receive bomb rewards");
        }
        finally { Set(player, "isInitialized", true); }
        scenarios++;
    }

    private sealed class Gate : IWaveProgressionGate
    {
        public bool Holding;
        public bool IsBlockingWaveProgression => Holding;
    }

    private static IEnumerator ValidateWaveTransition(bool withGate, float delay)
    {
        Check(!waves.IsWaveActive && !board.IsBusy, "transition fixture starts idle between encounters");
        float originalDelay = (float)Get(waves, "delayBeforeNextWave");
        var gate = new Gate { Holding = withGate };
        waves.RegisterProgressionGate(gate);
        int completedWave = waves.CurrentWave;
        Set(waves, "delayBeforeNextWave", delay);
        int starts = 0;
        void Started(int wave) { Check(wave == completedWave + 1 && !board.IsBusy, "next wave starts on idle board"); starts++; }
        void Spawned(EnemyActor actor) { Check(!board.IsBusy, "enemy spawn never overlaps old board work"); actor.GetComponent<EnemyAutoAttack>()?.StopAttacking(); }
        waves.WaveStarted += Started;
        waves.EnemySpawned += Spawned;
        Coroutine transition = waves.StartCoroutine((IEnumerator)Call(waves, "AdvanceToNextWaveWhenReady", completedWave));
        try
        {
            // Acquire the board AFTER the transition reached its initial delay.
            Set(board, "isBusy", true);
            double deadline = EditorApplication.timeSinceStartup + delay + 0.1d;
            yield return Until(() => EditorApplication.timeSinceStartup >= deadline, "real transition delay");
            Check(starts == 0 && waves.CurrentWave == completedWave && !waves.IsWaveActive,
                "transition cannot spawn into newly acquired board ownership");
            if (withGate)
            {
                Set(board, "isBusy", false);
                yield return null;
                yield return null;
                Check(starts == 0 && !waves.IsWaveActive, "idle board still waits for held upgrade gate");
                Set(board, "isBusy", true);
            }
            gate.Holding = false;
            for (int frame = 0; frame < 3; frame++)
            {
                yield return null;
                Check(waves.CurrentWave == completedWave && !waves.IsWaveActive,
                    "no next-wave spawn during the new board resolution");
            }
            Set(board, "isBusy", false);
            yield return Until(() => starts == 1 && !(bool)Get(waves, "isSpawningWave"), "progression resumes after ownership release");
            yield return null;
            Check(starts == 1 && waves.CurrentWave == completedWave + 1, "progression resumes exactly once");
            foreach (EnemyActor actor in waves.ActiveEnemies)
                Check(actor.CurrentHealth == actor.MaxHealth &&
                    !(actor.GetComponent<EnemyPoisonStatus>()?.IsPoisoned ?? false), "new enemies have fresh HP and no old poison");
        }
        finally
        {
            waves.StopCoroutine(transition);
            waves.WaveStarted -= Started;
            waves.EnemySpawned -= Spawned;
            waves.ClearCurrentWave();
            Set(waves, "currentWave", completedWave);
            Set(board, "isBusy", false);
            Set(waves, "delayBeforeNextWave", originalDelay);
            waves.UnregisterProgressionGate(gate);
        }
        scenarios++;
    }

    private static void ResetBoard()
    {
        Check(!board.IsBusy, "board fixture requires idle ownership");
        Sprite[] sprites = (Sprite[])Get(board, "gemSprites");
        for (int y = 0; y < board.Height; y++)
            for (int x = 0; x < board.Width; x++)
            {
                Gem gem = At(x, y);
                Check(gem != null, "fixture expects filled playable cells");
                gem.SetSpecialType(GemSpecialType.None);
                int type = (x + 2 * y) % 6;
                gem.SetType((GemType)type, sprites[type]);
            }
        foreach (EnemyActor actor in waves.ActiveEnemies)
            actor.GetComponent<EnemyPoisonStatus>()?.ClearPoison();
    }

    private static IEnumerator BoardRoutine(IEnumerator routine)
    {
        BoardController owner = board;
        bool done = false;
        Check(owner != null && !owner.IsBusy, "single board coroutine owner");
        Set(owner, "isBusy", true);
        owner.StartCoroutine(Complete());
        IEnumerator Complete()
        {
            try { yield return routine; }
            finally
            {
                if (owner != null) Set(owner, "isBusy", false);
                done = true;
            }
        }
        yield return Until(() => done && (owner == null || !owner.IsBusy), "production board coroutine");
    }

    private static IEnumerator Until(Func<bool> predicate, string label)
    {
        double deadline = EditorApplication.timeSinceStartup + 60d;
        while (!predicate())
        {
            Check(EditorApplication.timeSinceStartup < deadline, "timeout: " + label);
            yield return null;
        }
    }

    private static Gem At(int x, int y) => (Gem)Call(board, "GetGem", x, y);
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
    private static object Call(object target, string name, params object[] args)
    {
        foreach (MethodInfo method in target.GetType().GetMethods(Flags | BindingFlags.Static))
            if (method.Name == name && method.GetParameters().Length == args.Length)
                return method.Invoke(target, args);
        throw new MissingMethodException(target.GetType().Name, name);
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Gameplay bomb rewards: " + message);
    }
}
