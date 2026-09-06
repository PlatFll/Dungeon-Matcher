using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Fixtures modify runtime objects only. Run from the Game scene outside Play
// Mode; real Unity coroutines advance all production animations and board work.
[InitializeOnLoad]
public static class GameplayEdgeCaseValidation
{
    private const string Pending = "DungeonMatcher.EdgeCases";
    private const string LogPath = "Logs/GameplayEdgeCaseValidation.log";
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();
    private static BoardController board;
    private static WaveController waves;
    private static PlayerActor player;
    private static EnemyActor enemy;
    private static bool supplementary;

    static GameplayEdgeCaseValidation() { EditorApplication.playModeStateChanged += StateChanged; }

    [MenuItem("Dungeon Matcher/Validation/Gameplay Edge Cases %#F9")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Start outside Play Mode.");
        Directory.CreateDirectory("Logs");
        File.WriteAllText(LogPath, "Gameplay edge cases started\n");
        SessionState.SetBool(Pending, true);
        EditorApplication.EnterPlaymode();
    }

    [MenuItem("Dungeon Matcher/Validation/Gameplay Supplementary Cases %#F10")]
    public static void RunSupplementary()
    {
        SessionState.SetBool(Pending + ".Supplementary", true);
        Run();
    }

    private static void StateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
        {
            SessionState.SetBool(Pending, false);
            supplementary = SessionState.GetBool(Pending + ".Supplementary", false);
            SessionState.SetBool(Pending + ".Supplementary", false);
            steps.Clear(); steps.Push(Scenarios());
            EditorApplication.update -= Tick; EditorApplication.update += Tick;
        }
        if (state == PlayModeStateChange.ExitingPlayMode)
        { EditorApplication.update -= Tick; steps.Clear(); }
    }

    private static void Tick()
    {
        try
        {
            while (steps.Count > 0)
            {
                var step = steps.Peek();
                if (!step.MoveNext()) { steps.Pop(); continue; }
                if (step.Current is IEnumerator nested) { steps.Push(nested); continue; }
                return;
            }
            Note("PASSED");
            EditorApplication.update -= Tick;
            EditorApplication.ExitPlaymode();
        }
        catch (Exception exception)
        {
            Note("FAILED: " + exception);
            Debug.LogException(exception);
            EditorApplication.update -= Tick; steps.Clear();
            EditorApplication.ExitPlaymode();
        }
    }

    private static IEnumerator Scenarios()
    {
        yield return Delay(2);
        board = UnityEngine.Object.FindFirstObjectByType<BoardController>();
        waves = UnityEngine.Object.FindFirstObjectByType<WaveController>();
        player = UnityEngine.Object.FindFirstObjectByType<PlayerActor>();
        Check(board != null && waves != null && player != null, "Game scene fixture");
        Set(waves, "advanceWavesAutomatically", false);
        yield return Until(() => !board.IsBusy && waves.ActiveEnemies.Count > 0, "initial settle");
        foreach (var actor in waves.ActiveEnemies)
        {
            var attack = actor.GetComponent<EnemyAutoAttack>();
            Set(attack, "attackAutomatically", false); attack.StopAttacking();
            actor.SetSpecialTurnRequirement(10000); actor.ResetSpecialCounter();
            Set(actor, "currentHealth", 100000);
        }
        enemy = waves.ActiveEnemies[0];
        Set(player, "maximumHealth", 100000); Set(player, "currentHealth", 50000);
        if (supplementary)
        {
            yield return SupplementaryCases();
            yield break;
        }
        yield return ShieldCases();
        yield return FlashCases();
        EnergyRules();
        EncounterCases();
        yield return PinCases();

        var variants = new[] { GemSpecialType.RowBomb, GemSpecialType.ColumnBomb,
            GemSpecialType.PoisonBomb, GemSpecialType.HealingBomb, GemSpecialType.ShieldBomb };
        foreach (var variant in variants)
        {
            yield return CrystalCombo(variant);
            yield return CrackedChain(variant);
            yield return RemoteCrystal(variant);
        }
        foreach (var variant in new[] { GemSpecialType.PoisonBomb, GemSpecialType.HealingBomb, GemSpecialType.ShieldBomb })
            yield return BombCommit(variant);
        yield return CrackedCrystal();
        yield return EnvironmentalRemoval();
        for (int i = 0; i < 6; i++)
        {
            Check(board.TryGetRandomHintMove(out Gem a, out Gem b), "legal move after chains");
            int before = board.CompletedValidPlayerMoves;
            yield return BoardRoutine((IEnumerator)Call(board, "TrySwap", a, b));
            Check(!board.IsBusy && board.CompletedValidPlayerMoves == before + 1, "single move completion");
        }
        Note("Six real player swaps, cascade/refill/reshuffle and ownership regression passed");
        yield return DeathDuringFlash();
    }

    private static IEnumerator ShieldCases()
    {
        var panel = UnityEngine.Object.FindFirstObjectByType<PlayerPanelUI>();
        Check(Get(panel, "playerShieldBar") == null, "no shield overlay at initialization");
        int shieldLoss = 0, hpLoss = 0;
        Action<PlayerActor, int> shield = (p, n) => shieldLoss += n;
        Action<PlayerActor, int> hp = (p, n) => hpLoss += n;
        player.ShieldDamaged += shield; player.DamageTaken += hp;
        try
        {
            player.GrantShield(10);
            Check(((GameObject)Get(panel, "playerShieldBar")).activeSelf, "shield appears");
            player.TryTakeDamage(4);
            Check(shieldLoss == 3 && hpLoss == 0 && player.CurrentShield == 7, "mitigated absorption");
            player.TryTakeDamage(12);
            Check(shieldLoss == 10 && hpLoss == 2 && player.CurrentShield == 0, "shield overflow exact feedback");
            Check(Get(panel, "playerShieldBar") == null, "zero shield hides immediately");
            player.GrantShield(3); player.TryTakeDamage(4);
            Check(shieldLoss == 13 && hpLoss == 2, "exact shield break");
            Check(!player.TryTakeDamage(0), "zero event ignored");
            yield return Delay(0.3f);
            Check(panel.transform.Find("PlayerHPBarBackground/PlayerShieldBar") == null, "no orphan overlay");
            player.GrantShield(4);
            player.TryTakeDamage(8);
            yield return Delay(0.2f);
            Check(Get(panel, "playerShieldBar") == null, "regrant and break lifecycle");
            Note("Shield: initialization, mitigation, absorption, overflow, exact break, zero damage and regrant passed");
        }
        finally { player.ShieldDamaged -= shield; player.DamageTaken -= hp; }
    }

    private static IEnumerator SupplementaryCases()
    {
        foreach (bool doubleCrystal in new[] { false, true })
        {
            ResetBoard();
            Gem first = At(2, 3), second = At(3, 3);
            first.SetSpecialType(GemSpecialType.ColorCrystal);
            if (doubleCrystal) second.SetSpecialType(GemSpecialType.ColorCrystal);
            yield return BoardRoutine((IEnumerator)Call(board, "TrySwap", first, second));
            Check(first == null && second == null && !board.IsBusy, "normal/double crystal swap completes");
            Check((bool)Call(board, "HasAvailableMove"), "normal/double crystal leaves playable board");
            Note(doubleCrystal ? "Double-crystal sweep passed" : "Crystal plus ordinary gem passed");
        }

        ResetBoard();
        var controller = player.GetComponent<PlayerAbilityController>();
        var energy = player.GetComponent<PlayerAbilityEnergy>();
        Check(controller.ActiveAbility is CrackedGemsAbilityDefinition, "Bardley selected for actual ability fixture");
        energy.ResetEnergy(); energy.AddEnergy(controller.RequiredEnergy);
        Check(controller.TryActivate(), "real ability accepted");
        Check(energy.CurrentEnergy == 0 && board.IsBusy, "energy spent after acceptance with board locked");
        Check(!controller.TryActivate(), "cannot activate twice during resolution");
        yield return Until(() => !controller.IsAbilityActive && !board.IsBusy, "real Bardley ability");
        Check(energy.CurrentEnergy > 0, "real Bardley explosion energy while ability active");
        Note("Real Bardley runtime: acceptance, spending, duplicate rejection, explosion energy and completion passed");

        ResetBoard();
        Gem crystal = At(3, 3);
        crystal.SetSpecialType(GemSpecialType.ColorCrystal);
        bool done = false;
        Set(board, "isBusy", true);
        board.StartCoroutine((IEnumerator)Call(board, "ResolveCrackedGemAbility", new List<Gem> { crystal },
            50, 0f, 0f, 0.2f, 1.05f, 0.04f, (Action)(() => done = true)));
        yield return Until(() => done && !board.IsBusy, "direct crystal fallback");
        Check(crystal == null, "direct crystal activated rather than overwritten");
        Note("Directly selected special crystal fallback activation passed");

        ResetBoard();
        Gem stale = At(2, 3);
        yield return BoardRoutine((IEnumerator)Call(board, "ClearMatches", new HashSet<Gem> { stale }, null, false));
        yield return BoardRoutine((IEnumerator)Call(board, "ResolveEnvironmentalBoardChange"));
        Gem center = At(3, 3); done = false;
        Set(board, "isBusy", true);
        board.StartCoroutine((IEnumerator)Call(board, "ResolveCrackedGemAbility", new List<Gem> { stale, center, center },
            50, 0.1f, 0f, 0.2f, 1.05f, 0.04f, (Action)(() => done = true)));
        yield return Until(() => done && !board.IsBusy, "stale and overlapping cracked seeds");
        Check(center == null, "surviving cracked target resolves");
        Note("Missing scheduled target and duplicate cracked seeds settle without softlock");
    }

    [MenuItem("Dungeon Matcher/Validation/Encounter Repeat History")]
    public static void RunEncounterHistory()
    {
        var root = new GameObject("Encounter history validation"); root.SetActive(false);
        try
        {
            var fixture = root.AddComponent<WaveController>();
            var database = AssetDatabase.LoadAssetAtPath<EnemyDatabase>("Assets/_Game/Data/Enemies/EnemyDatabase_Main.asset");
            var profile = AssetDatabase.LoadAssetAtPath<WaveSpawnProfile>("Assets/_Game/Data/Balance/WaveSpawnProfile_Standard.asset");
            var bishop = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_RoyalArchbishop.asset");
            var king = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_King.asset");
            Set(fixture, "enemyDatabase", database); Set(fixture, "waveSpawnProfile", profile);
            Set(fixture, "currentWave", 26); Set(fixture, "selectedMilestoneLeader", king);
            var previous = (HashSet<EnemyDefinition>)Get(fixture, "previousEncounterLeaders");
            var seen = (HashSet<EnemyDefinition>)Get(fixture, "seenMilestoneLeaders");
            previous.Add(bishop); seen.Add(bishop);
            fixture.GetType().GetProperty("CurrentPlan").SetValue(fixture,
                new WaveSpawnPlan(26, "Required escort repeat fixture", new List<EnemyCategory> { EnemyCategory.Boss, EnemyCategory.Normal }));
            var encounter = (List<EnemyDefinition>)Call(fixture, "BuildEncounter", 2);
            Check(!encounter.Contains(bishop) && !encounter.Contains(king) && encounter.Exists(d => d != null),
                "required escort repeat defers leader with nonempty fallback");
            previous.Clear(); Set(fixture, "currentWave", 27);
            var milestone = profile.SelectMilestone(27, new System.Random(1), seen, out int count);
            Check(milestone == king && count == 2, "deferred guaranteed milestone remains eligible");
            encounter = (List<EnemyDefinition>)Call(fixture, "BuildEncounter", 2);
            Check(encounter.Contains(king) && encounter.Contains(bishop), "required pairing restored after different encounter");
            seen.Add(king); previous.Add(king); previous.Add(bishop);
            encounter = (List<EnemyDefinition>)Call(fixture, "BuildEncounter", 2);
            Check(!encounter.Contains(king) && !encounter.Contains(bishop), "seen boss and previous escort excluded");
            Debug.Log("Encounter repeat history validation PASSED: required escort, fallback, deferred milestone and seen Boss.");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static IEnumerator FlashCases()
    {
        var feedback = enemy.GetComponent<EnemyCombatFeedback>();
        var poison = enemy.GetComponent<EnemyPoisonStatus>();
        if (poison == null) poison = enemy.gameObject.AddComponent<EnemyPoisonStatus>();
        var poisonView = EnemyPoisonStatusPresenter.EnsureInstalled(enemy.gameObject, poison);
        var stagger = enemy.GetComponent<EnemyStagger>();
        for (int i = 0; i < 8; i++)
        {
            enemy.TryTakeDamage(1);
            stagger.ApplyStagger(0.06f, 0.3f);
            Call(poisonView, "PlayPoisonTickWhiteFlash");
            yield return Delay(0.03f);
        }
        yield return Delay(1);
        var material = (Material)Get(feedback, "runtimeFlashMaterial");
        Check(material.GetFloat("_FlashAmount") == 0, "overlapping flashes restore zero");
        feedback.RefreshHitFlash(0.3f); feedback.enabled = false;
        Check(material.GetFloat("_FlashAmount") == 0, "disable clears temporary white");
        feedback.enabled = true;
        Note("Rapid direct/poison presentation, stagger overlap, expiry and disable flash checks passed");
    }

    private static void EncounterCases()
    {
        int oldWave = waves.CurrentWave;
        var oldPlan = waves.CurrentPlan;
        var captain = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_KnightCaptain.asset");
        var previous = (HashSet<EnemyDefinition>)Get(waves, "previousEncounterLeaders");
        var seen = (HashSet<EnemyDefinition>)Get(waves, "seenMilestoneLeaders");
        var seenSnapshot = new HashSet<EnemyDefinition>(seen);
        var previousSnapshot = new HashSet<EnemyDefinition>(previous);
        Set(waves, "currentWave", 22);
        waves.GetType().GetProperty("CurrentPlan").SetValue(waves,
            new WaveSpawnPlan(22, "Regression fixture", new List<EnemyCategory> { EnemyCategory.Miniboss, EnemyCategory.Normal }));
        Set(waves, "selectedMilestoneLeader", null);
        int captainRolls = 0;
        for (int i = 0; i < 300; i++)
        {
            previous.Clear(); previous.Add(captain);
            var next = (List<EnemyDefinition>)Call(waves, "BuildEncounter", 2);
            Check(!next.Contains(captain) && next.Exists(d => d != null), "repeat guard and fallback");
            previous.Clear();
            var later = (List<EnemyDefinition>)Call(waves, "BuildEncounter", 2);
            if (later.Contains(captain)) captainRolls++;
        }
        Check(captainRolls > 0, "Captain eligible after a different encounter");
        seen.Clear(); seen.UnionWith(seenSnapshot);
        previous.Clear(); previous.UnionWith(previousSnapshot);
        Set(waves, "currentWave", oldWave);
        waves.GetType().GetProperty("CurrentPlan").SetValue(waves, oldPlan);
        Note("600 weighted encounter compositions: no adjacent Captain, nonempty fallback, later eligible (" + captainRolls + " rolls)");
    }

    private static IEnumerator PinCases()
    {
        int[] rows = new int[board.Height];
        for (int i = 0; i < 60; i++)
        {
            bool done = false;
            Check(board.TryQueueTopUpMovablePins(enemy, 3, ok => { Check(ok, "pins accepted"); done = true; }, () => false), "queue pins");
            yield return Until(() => done && !board.IsBusy, "pin top-up");
            var pins = (HashSet<Gem>)Get(board, "movablePinnedGems");
            Check(pins.Count == 3, "three ordinary pins");
            foreach (var gem in pins) { Check(gem.SpecialType == GemSpecialType.None, "ordinary pin"); rows[gem.Row]++; }
            Check((bool)Call(board, "HasAvailableMove"), "pins preserve legal moves");
            board.QueueReleasePinnedGems(enemy.GetInstanceID());
            yield return Until(() => !board.IsBusy && pins.Count == 0, "pin cleanup");
        }
        foreach (int count in rows) Check(count > 0, "every row represented");
        Note("60 Captain casts / 180 pins: row distribution " + string.Join(",", rows));
    }

    private static IEnumerator CrystalCombo(GemSpecialType variant)
    {
        ResetBoard();
        Gem crystal = At(2, 3), bomb = At(3, 3);
        crystal.SetSpecialType(GemSpecialType.ColorCrystal); bomb.SetSpecialType(variant);
        var targets = new List<Gem>();
        for (int y = 0; y < board.Height; y++) for (int x = 0; x < board.Width; x++)
            if (At(x, y) != crystal && At(x, y).Type == bomb.Type) targets.Add(At(x, y));
        // Verify conversion while protected bombs are still waiting to detonate.
        var converted = new HashSet<Gem>();
        int before = board.CompletedValidPlayerMoves;
        board.StartCoroutine((IEnumerator)Call(board, "TrySwap", crystal, bomb));
        yield return Until(() =>
        {
            foreach (var gem in targets) if (gem != null && gem.SpecialType == variant) converted.Add(gem);
            return !board.IsBusy && board.CompletedValidPlayerMoves == before + 1;
        }, "crystal combo " + variant);
        Check(converted.Count == targets.Count, "every target converted to " + variant);
        foreach (var gem in targets) Check(gem == null, "all converted targets detonated " + variant);
        Check((bool)Call(board, "HasAvailableMove"), "combo settles playable");
        Note("Real crystal swap / conversion / all targets destroyed / settle: " + variant);
    }

    private static IEnumerator CrackedChain(GemSpecialType variant)
    {
        ResetBoard(); Set(player, "currentShield", 0);
        Gem center = At(3, 3), special = At(4, 3);
        special.SetSpecialType(variant);
        var energy = player.GetComponent<PlayerAbilityEnergy>(); energy.ResetEnergy();
        Set(energy, "maximumEnergy", 100000);
        var controller = player.GetComponent<PlayerAbilityController>();
        object previousRuntime = Get(controller, "activeRuntime");
        Set(controller, "activeRuntime", new ActiveAbilityFixture());
        int expectedEnergy = 0;
        var gain = player.GetComponent<PlayerAbilityMatchEnergyGain>();
        Action<BoardClearOutcome> outcome = o =>
        { if (o.ClearContext.GrantsSpecialEnergy) expectedEnergy += gain.CalculateEnergyGain(o); };
        board.BoardClearOutcomeResolved += outcome;
        int rewards = 0;
        center.SetSpecialType(GemSpecialType.Cracked);
        object[] expansionArgs = { new List<Gem> { center }, null, null };
        int expectedRewards = ((HashSet<Gem>)Call(board, "BuildCrackedExpandedClearSet", expansionArgs)).Count;
        Check(player.CurrentShield == 0, "planning is effect-free");
        Action<BoardClearContext> cleared = context => { if (context.Source == BoardClearSource.Ability) rewards += context.GemCount; };
        board.BoardClearResolved += cleared;
        bool done = false;
        try
        {
            Set(board, "isBusy", true);
            board.StartCoroutine((IEnumerator)Call(board, "ResolveCrackedGemAbility", new List<Gem> { center },
                50, 0f, 0f, 0.5f, 1.05f, 0.04f, (Action)(() => done = true)));
            yield return Delay(0.15f);
            Check(player.CurrentShield == 0, "no queued shield effect");
            Check(special != null, "bomb survives preparation");
            yield return Until(() => done && !board.IsBusy, "cracked chain " + variant);
            Check(center == null && special == null, "chain destroys center and bomb");
            Check(rewards == expectedRewards && energy.CurrentEnergy > 0, "one reward per physical gem");
            Check(energy.CurrentEnergy == expectedEnergy, "exact explosion energy during active ability");
            if (variant == GemSpecialType.ShieldBomb) Check(player.CurrentShield > 0, "shield committed");
            Note("Cracked chain timing, rewards and settle: " + variant + " (ability cleared " + rewards + ")");
        }
        finally
        {
            board.BoardClearResolved -= cleared; board.BoardClearOutcomeResolved -= outcome;
            Set(controller, "activeRuntime", previousRuntime);
        }
    }

    private static void EnergyRules()
    {
        var gain = player.GetComponent<PlayerAbilityMatchEnergyGain>();
        var energy = player.GetComponent<PlayerAbilityEnergy>();
        var controller = player.GetComponent<PlayerAbilityController>();
        object previous = Get(controller, "activeRuntime");
        Set(controller, "activeRuntime", new ActiveAbilityFixture()); energy.ResetEnergy();
        try
        {
            var ability = new BoardClearOutcome(new BoardClearContext(GemType.Ruby, 4, 0, BoardClearSource.Ability), true);
            Call(gain, "HandleBoardClearOutcomeResolved", ability);
            Check(energy.CurrentEnergy == 0, "unentitled ability destruction excluded");
            var bomb = new BoardClearOutcome(new BoardClearContext(GemType.Ruby, 4, 0, BoardClearSource.Bomb), true);
            Call(gain, "HandleBoardClearOutcomeResolved", bomb);
            Check(energy.CurrentEnergy == gain.CalculateEnergyGain(bomb), "bomb energy while active");
            var empty = new BoardClearOutcome(new BoardClearContext(GemType.Ruby, 0, 0, BoardClearSource.Bomb), true);
            Check(gain.CalculateEnergyGain(empty) == 0, "effect-only activation has no physical clear energy");
        }
        finally { Set(controller, "activeRuntime", previous); }
        Note("Energy ownership: active player bomb rewarded, ordinary ability and zero physical clear excluded");
    }

    private sealed class ActiveAbilityFixture : IPlayerAbilityRuntime
    {
        public event Action StateChanged { add { } remove { } }
        public bool IsActive => true;
        public bool Supports(CharacterAbilityDefinition definition) => false;
        public bool CanActivate(CharacterAbilityDefinition definition) => false;
        public bool TryActivate(CharacterAbilityDefinition definition) => false;
        public void Cancel() { }
    }

    private static IEnumerator CrackedCrystal()
    {
        ResetBoard();
        Gem center = At(3, 3), crystal = At(4, 3);
        crystal.SetSpecialType(GemSpecialType.ColorCrystal);
        var sameColor = new List<Gem>();
        for (int y = 0; y < board.Height; y++) for (int x = 0; x < board.Width; x++)
            if (At(x, y) != crystal && At(x, y).Type == center.Type) sameColor.Add(At(x, y));
        bool done = false;
        Set(board, "isBusy", true);
        board.StartCoroutine((IEnumerator)Call(board, "ResolveCrackedGemAbility", new List<Gem> { center },
            50, 0f, 0f, 0.2f, 1.05f, 0.04f, (Action)(() => done = true)));
        yield return Until(() => done && !board.IsBusy, "cracked crystal");
        Check(crystal == null, "crystal activated");
        foreach (var gem in sameColor) Check(gem == null, "approved same-color cracked expansion");
        Note("Cracked crystal: all same-color gems activated and destroyed");
    }

    private static IEnumerator EnvironmentalRemoval()
    {
        ResetBoard(); Set(player, "currentShield", 0);
        Gem target = At(3, 3); target.SetSpecialType(GemSpecialType.ShieldBomb);
        int rewards = 0; Action<BoardClearContext> cleared = c => rewards += c.GemCount;
        board.BoardClearResolved += cleared;
        try
        {
            yield return BoardRoutine((IEnumerator)Call(board, "ClearMatches", new HashSet<Gem> { target }, null, false));
            Check(rewards == 0 && player.CurrentShield == 0, "environment neither rewards nor activates");
        }
        finally { board.BoardClearResolved -= cleared; }
        yield return BoardRoutine((IEnumerator)Call(board, "ResolveEnvironmentalBoardChange"));
        Note("Environmental physical removal excludes rewards and bomb effects");
    }

    private static IEnumerator DeathDuringFlash()
    {
        var feedback = enemy.GetComponent<EnemyCombatFeedback>();
        var material = (Material)Get(feedback, "runtimeFlashMaterial");
        feedback.RefreshHitFlash(1);
        enemy.TryTakeDamage(enemy.CurrentHealth + 100);
        Check((float)Get(feedback, "hitFlashUntil") == 0, "death cancels hit flash (lifecycle has its own death flash)");
        yield return Delay(1.5f);
        Note("Lethal hit during flash and actor destruction completed");
    }

    private static IEnumerator RemoteCrystal(GemSpecialType variant)
    {
        ResetBoard();
        Gem bomb = At(3, 3), crystal = At(4, 3);
        if (variant == GemSpecialType.ColumnBomb) crystal = At(3, 4);
        bomb.SetSpecialType(variant); crystal.SetSpecialType(GemSpecialType.ColorCrystal);
        Set(board, "isBusy", true);
        yield return BoardRoutine((IEnumerator)Call(board, "ResolveCascades", new HashSet<Gem> { bomb }, null, null));
        Set(board, "isBusy", false);
        Check(crystal == null && bomb == null, "remote crystal activated " + variant);
        Check((bool)Call(board, "HasAvailableMove"), "remote chain leaves legal board");
        Note("Bomb into protected/refilled crystal chain: " + variant);
    }

    private static IEnumerator BombCommit(GemSpecialType variant)
    {
        ResetBoard(); Set(player, "currentShield", 0); Set(player, "currentHealth", 50000);
        Gem first = At(3, 3), second = At(4, 3);
        first.SetSpecialType(variant); second.SetSpecialType(variant);
        int effects = 0;
        void Record()
        {
            effects++;
            foreach (var gem in new[] { first, second })
                foreach (var renderer in gem.GetComponentsInChildren<SpriteRenderer>())
                    Check(!renderer.enabled, "effect occurs only after bomb shatter");
        }
        Action<PlayerActor, int> healed = (p, n) => Record();
        Action<PlayerActor, int, int> shield = (p, n, max) => { Record(); Set(player, "currentShield", 0); };
        Action<EnemyPoisonStatus, bool> poison = (p, refreshed) => Record();
        var status = enemy.GetComponent<EnemyPoisonStatus>();
        if (variant == GemSpecialType.HealingBomb) player.Healed += healed;
        if (variant == GemSpecialType.ShieldBomb) player.ShieldChanged += shield;
        if (variant == GemSpecialType.PoisonBomb) status.PoisonApplied += poison;
        try
        {
            object[] args = { new HashSet<Gem> { first }, true, null };
            var expanded = (HashSet<Gem>)Call(board, "BuildBombExpandedClearSet", args);
            Call(board, "BuildBombExpandedClearSet", args);
            Check(effects == 0, "repeated planning never commits effects");
            yield return BoardRoutine((IEnumerator)Call(board, "ClearMatches", expanded, null, true));
            Check(effects == 2, "two chained bombs activate exactly once each");
        }
        finally
        {
            player.Healed -= healed; player.ShieldChanged -= shield; status.PoisonApplied -= poison;
        }
        yield return BoardRoutine((IEnumerator)Call(board, "ResolveEnvironmentalBoardChange"));
        Note("Effect-free planning and two exact shatter-time commits: " + variant);
    }

    private static void ResetBoard()
    {
        Check(!board.IsBusy, "fixture requires idle board");
        var sprites = (Sprite[])Get(board, "gemSprites");
        for (int y = 0; y < board.Height; y++) for (int x = 0; x < board.Width; x++)
        {
            Gem gem = At(x, y); Check(gem != null, "filled board");
            gem.SetSpecialType(GemSpecialType.None);
            int type = (x + 2 * y) % 6; gem.SetType((GemType)type, sprites[type]);
        }
        foreach (var actor in waves.ActiveEnemies) actor.GetComponent<EnemyPoisonStatus>()?.ClearPoison();
    }

    private static IEnumerator BoardRoutine(IEnumerator routine)
    {
        bool done = false;
        board.StartCoroutine(Complete());
        IEnumerator Complete() { yield return routine; done = true; }
        yield return Until(() => done, "production coroutine");
    }
    private static IEnumerator Until(Func<bool> condition, string label)
    {
        double deadline = EditorApplication.timeSinceStartup + 60;
        while (!condition()) { Check(EditorApplication.timeSinceStartup < deadline, "timeout: " + label); yield return null; }
    }
    private static IEnumerator Delay(float seconds)
    { float end = Time.time + seconds; while (Time.time < end) yield return null; }
    private static Gem At(int x, int y) => (Gem)Call(board, "GetGem", x, y);
    private static object Get(object target, string name) => target.GetType().GetField(name, Flags).GetValue(target);
    private static void Set(object target, string name, object value) => target.GetType().GetField(name, Flags).SetValue(target, value);
    private static object Call(object target, string name, params object[] args)
    {
        foreach (var method in target.GetType().GetMethods(Flags))
            if (method.Name == name && method.GetParameters().Length == args.Length) return method.Invoke(target, args);
        throw new MissingMethodException(name);
    }
    private static void Check(bool value, string label) { if (!value) throw new Exception(label); }
    private static void Note(string message) { File.AppendAllText(LogPath, message + "\n"); Debug.Log("Edge cases: " + message); }
}
