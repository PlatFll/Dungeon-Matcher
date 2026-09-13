using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Synchronous EditMode fixtures. These run the actual board reward reporters
// and combat dispatch, not timed explosions, physical destruction or spawning.
[NonParallelizable]
public sealed class CrackedCenterRewardTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private readonly List<UnityEngine.Object> temporaryAssets = new List<UnityEngine.Object>();
    private GameObject root;
    private PlayerActor player;
    private PlayerAbilityEnergy energy;
    private BoardController board;
    private CombatController combat;
    private WaveController waves;
    private RunUpgradeRuntime runtime;
    private RunUpgradeGameplayHooks hooks;
    private Gem[,] grid;
    private UnityEngine.Random.State randomState;

    [SetUp]
    public void SetUp()
    {
        randomState = UnityEngine.Random.state;
        Assert.That(RunUpgradeRuntime.Current, Is.Null, "Use an isolated EditMode scene; do not delete live run state.");
        Assert.That(RunUpgradeGameplayHooks.Current, Is.Null);
        root = new GameObject("CrackedRewardFixture");
        player = Child("Player").AddComponent<PlayerActor>();
        PlayerDefinition definition = Resources.Load<PlayerDefinition>("Players/Player_Bardley");
        Assert.That(definition, Is.Not.Null);
        player.Initialize(definition, 100);
        Assert.That(player.ActiveAbility, Is.InstanceOf<CrackedGemsAbilityDefinition>());
        energy = player.gameObject.AddComponent<PlayerAbilityEnergy>();
        board = Child("Board").AddComponent<BoardController>();
        grid = new Gem[7, 8];
        Set(board, "width", 7);
        Set(board, "height", 8);
        Set(board, "gems", grid);
        waves = Child("Waves").AddComponent<WaveController>();
        Set(waves, "currentWave", 5);
        Set(waves, "advanceWavesAutomatically", false);
        combat = Child("Combat").AddComponent<CombatController>();
        Set(combat, "playerActor", player);
        Set(combat, "waveController", waves);
        Set(board, "combatController", combat);
        runtime = waves.gameObject.AddComponent<RunUpgradeRuntime>();
        RunUpgradeCatalog catalog = Resources.Load<RunUpgradeCatalog>("RunUpgrades/PrototypeRunUpgradeCatalog");
        Assert.That(catalog, Is.Not.Null);
        runtime.Configure(catalog, player, waves);
        hooks = waves.gameObject.AddComponent<RunUpgradeGameplayHooks>();
        hooks.Configure(runtime, board, combat, player, waves);
    }

    [TearDown]
    public void TearDown()
    {
        if (root != null)
        {
            root.SetActive(false);
            UnityEngine.Object.DestroyImmediate(root);
        }
        for (int i = temporaryAssets.Count - 1; i >= 0; i--)
            if (temporaryAssets[i] != null) UnityEngine.Object.DestroyImmediate(temporaryAssets[i]);
        temporaryAssets.Clear();
        UnityEngine.Random.state = randomState;
    }

    [TestCase(false, 3, 5)]
    [TestCase(true, 3, 5)]
    [TestCase(false, 6, 10)]
    [TestCase(true, 6, 10)]
    public void BoardReportedCentersRefundEveryThirdRegardlessOfEncounterAvailability(bool active, int count, int expected)
    {
        ApplyResonant();
        Set(waves, "<IsWaveActive>k__BackingField", active);
        // No matching enemy is required. This asserts the card bonus only;
        // the ordinary per-gem energy producer is deliberately absent.
        ReportBatch(count);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(expected));
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.Zero);
    }

    [Test]
    public void EncounterEndingBetweenCentersDoesNotLoseTheThirdCenter()
    {
        ApplyResonant();
        Set(waves, "<IsWaveActive>k__BackingField", true);
        ReportBatch(2);
        Assert.That(energy.CurrentEnergy, Is.Zero);
        waves.ClearCurrentWave();
        Assert.That(waves.IsWaveActive, Is.False);
        ReportBatch(1);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(5));
        Assert.That(waves.IsWaveActive, Is.False, "a refund never reopens an encounter");
    }

    [Test]
    public void ReporterTagsOnlyFixedCentersAndExcludesCrystalHiddenColor()
    {
        ApplyResonant();
        var outcomes = new List<BoardClearOutcome>();
        board.BoardClearOutcomeResolved += outcomes.Add;
        ReportBatch(3, includeCollateralAndCrystal: true);
        Assert.That(outcomes.Count, Is.EqualTo(4));
        int centers = 0, collateral = 0, rewardableCount = 0;
        foreach (BoardClearOutcome outcome in outcomes)
        {
            BoardClearContext clear = outcome.ClearContext;
            Assert.That(clear.Source, Is.EqualTo(BoardClearSource.Ability));
            Assert.That(clear.GrantsSpecialEnergy, Is.True);
            Assert.That(clear.GemCount, Is.EqualTo(1));
            rewardableCount += clear.GemCount;
            if (clear.IsFixedDamageExplosionCenter) centers++;
            else collateral++;
        }
        Assert.That(centers, Is.EqualTo(3));
        Assert.That(collateral, Is.EqualTo(1));
        Assert.That(rewardableCount, Is.EqualTo(4), "the crystal is not a rewardable colored center");
        Assert.That(energy.CurrentEnergy, Is.EqualTo(5));
    }

    [TestCase(BoardClearSource.Ability, false, true, 1)]
    [TestCase(BoardClearSource.Ability, true, false, 1)]
    [TestCase(BoardClearSource.Ability, true, true, 0)]
    [TestCase(BoardClearSource.Ability, true, true, 2)]
    [TestCase(BoardClearSource.Match, true, true, 1)]
    [TestCase(BoardClearSource.Bomb, true, true, 1)]
    [TestCase(BoardClearSource.ColorCrystal, true, true, 1)]
    [TestCase(BoardClearSource.DoubleColorCrystal, true, true, 1)]
    public void UnentitledOrNonCenterOutcomesDoNotAdvanceResonance(
        BoardClearSource source, bool entitled, bool center, int gemCount)
    {
        ApplyResonant();
        var context = new BoardClearContext(GemType.Ruby, gemCount, 0, source,
            grantsSpecialEnergy: entitled, isFixedDamageExplosionCenter: center);
        for (int i = 0; i < 3; i++) DeliverOutcome(context);
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.Zero);
        Assert.That(energy.CurrentEnergy, Is.Zero);
    }

    [Test]
    public void ClassificationAloneDoesNotGrantEnergyEntitlement()
    {
        var ordinary = new BoardClearContext(GemType.Ruby, 1, 0, BoardClearSource.Ability);
        var classified = new BoardClearContext(GemType.Ruby, 1, 0, BoardClearSource.Ability,
            isFixedDamageExplosionCenter: true);
        Assert.That(ordinary.IsFixedDamageExplosionCenter, Is.False);
        Assert.That(classified.IsFixedDamageExplosionCenter, Is.True);
        Assert.That(classified.GrantsSpecialEnergy, Is.False);
        Assert.That(default(BoardClearContext).IsFixedDamageExplosionCenter, Is.False);
    }

    [Test]
    public void MissingResonantCardGrantsNoBonus()
    {
        ReportBatch(6);
        Assert.That(energy.CurrentEnergy, Is.Zero);
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.Zero);
    }

    [Test]
    public void DamageDispatchAloneCannotAwardOrAdvanceDetonationRefund()
    {
        ApplyResonant();
        Set(waves, "<IsWaveActive>k__BackingField", true);
        var context = CenterContext();
        for (int i = 0; i < 3; i++) combat.ResolveFixedGemDamage(context, 50);
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.Zero);
        Assert.That(energy.CurrentEnergy, Is.Zero);
        for (int i = 0; i < 3; i++) DeliverOutcome(context);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(5));
    }

    [Test]
    public void DamageModifiersUseCenterIdentityNotCoincidentDamageAmounts()
    {
        RunUpgradeDefinition strongerCenters = ScriptableObject.CreateInstance<RunUpgradeDefinition>();
        temporaryAssets.Add(strongerCenters);
        Set(strongerCenters, "upgradeId", "audit_stronger_centers");
        var modifier = new RunUpgradeModifier();
        Set(modifier, "stat", RunUpgradeStat.CrackedGemDamage);
        Set(modifier, "operation", RunUpgradeModifierOperation.AdditivePercent);
        Set(modifier, "value", 0.5f);
        Set(strongerCenters, "modifiers", new[] { modifier });
        Assert.That(runtime.TryApply(strongerCenters, 5), Is.True);
        Set(waves, "<IsWaveActive>k__BackingField", true);
        int reportedDamage = 0;
        combat.BeforeGemDamage += context => reportedDamage = context.Damage;

        int configuredBase = ((CrackedGemsAbilityDefinition)player.ActiveAbility).CrackedGemDamage;
        var collateral = new BoardClearContext(GemType.Ruby, 1, 0, BoardClearSource.Ability,
            grantsSpecialEnergy: true);
        combat.ResolveFixedGemDamage(collateral, configuredBase);
        Assert.That(reportedDamage, Is.EqualTo(configuredBase), "equal numbers do not make collateral a center");

        // A legitimate center keeps its modifier even with a different supplied
        // fixed amount; this checks classification, not a production rebalance.
        combat.ResolveFixedGemDamage(CenterContext(), 40);
        Assert.That(reportedDamage, Is.EqualTo(60));
        Assert.That(energy.CurrentEnergy, Is.Zero);
    }

    [Test]
    public void CappedRefundIsConsumedAndNotReplayedAfterEnergyIsSpent()
    {
        ApplyResonant();
        energy.AddEnergy(energy.MaximumEnergy);
        ReportBatch(3);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(energy.MaximumEnergy));
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.Zero);
        Assert.That(energy.TrySpendEnergy(10), Is.True);
        ReportBatch(1);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(energy.MaximumEnergy - 10));
    }

    [Test]
    public void DefeatedPlayerDoesNotGainRefundOrAdvanceCounter()
    {
        ApplyResonant();
        player.TryTakeDamage(player.MaximumHealth);
        Assert.That(player.IsDefeated, Is.True);
        ReportBatch(3);
        Assert.That(energy.CurrentEnergy, Is.Zero);
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.Zero);
    }

    [Test]
    public void ReenableRetainsTwoCentersWithoutDuplicateSubscriptionAndResetClearsThem()
    {
        ApplyResonant();
        ReportBatch(2);
        for (int i = 0; i < 5; i++) { hooks.enabled = false; hooks.enabled = true; }
        ReportBatch(1);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(5));
        ReportBatch(2);
        runtime.ResetRun();
        ApplyResonant();
        ReportBatch(1);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(5), "reset is not a refund and clears prior partial count");
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.EqualTo(1));
    }

    [Test]
    public void ChromaticConductorRemainsSeparateFromCenterRefund()
    {
        ApplyResonant();
        RunUpgradeDefinition conductor = Resources.Load<RunUpgradeDefinition>("RunUpgrades/RunUpgrade_ChromaticConductor");
        Assert.That(conductor, Is.Not.Null);
        Assert.That(runtime.TryApply(conductor, 5), Is.True);
        DeliverOutcome(new BoardClearContext(GemType.Ruby, 4, 0, BoardClearSource.ColorCrystal));
        Assert.That(energy.CurrentEnergy, Is.EqualTo(4));
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.Zero);
        ReportBatch(3);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(9));
    }

    private void ApplyResonant()
    {
        RunUpgradeDefinition card = Resources.Load<RunUpgradeDefinition>("RunUpgrades/RunUpgrade_ResonantCracks");
        Assert.That(card, Is.Not.Null);
        Assert.That(runtime.TryApply(card, 5), Is.True);
    }

    private static BoardClearContext CenterContext() => new BoardClearContext(
        GemType.Ruby, 1, 0, BoardClearSource.Ability, grantsSpecialEnergy: true,
        isFixedDamageExplosionCenter: true);

    private void DeliverOutcome(BoardClearContext context)
    {
        // Test-only event delivery for defensive cases that the real board does
        // not emit. Positive-path cases above use the real set reporter instead.
        var handlers = Get<Action<BoardClearOutcome>>(board, "BoardClearOutcomeResolved");
        Assert.That(handlers, Is.Not.Null);
        handlers.Invoke(new BoardClearOutcome(context, false));
    }

    private void ReportBatch(int count, bool includeCollateralAndCrystal = false)
    {
        var centers = new HashSet<Gem>();
        for (int i = 0; i < count; i++) centers.Add(CreateGem(i, 1, GemType.Ruby, GemSpecialType.Cracked));
        var expanded = new HashSet<Gem>(centers);
        if (includeCollateralAndCrystal)
        {
            expanded.Add(CreateGem(0, 3, GemType.Sapphire, GemSpecialType.None));
            expanded.Add(CreateGem(1, 3, GemType.Ruby, GemSpecialType.ColorCrystal));
        }
        Call(board, "ReportCrackedClearSetToCombat", expanded, centers, 50);
    }

    private Gem CreateGem(int x, int y, GemType type, GemSpecialType special)
    {
        GameObject go = Child("FixtureGem");
        go.SetActive(false);
        Gem gem = go.AddComponent<Gem>();
        gem.Initialize(board, x, y, type, null, 1f);
        gem.SetSpecialType(special);
        grid[x, y] = gem;
        return gem;
    }

    private GameObject Child(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root.transform, false);
        return go;
    }
    private static T Get<T>(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, Flags);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        return (T)field.GetValue(target);
    }
    private static void Set(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(name, Flags);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        field.SetValue(target, value);
    }
    private static object Call(object target, string name, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(name, Flags);
        if (method == null) throw new MissingMethodException(target.GetType().Name, name);
        return method.Invoke(target, args);
    }
}
