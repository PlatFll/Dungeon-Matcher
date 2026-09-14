using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Synchronous EditMode reporting tests, not shatter/animation or scene tests.
// Resources are read-only. No preferences, live singleton or authored scene is edited.
[NonParallelizable]
public sealed class CrackedChainUpgradeScopeTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private GameObject root;
    private EnemyDefinition enemyDefinition;
    private PlayerDefinition playerDefinition;
    private CharacterAbilityDefinition abilityDefinition;
    private PlayerActor player;
    private PlayerAbilityEnergy energy;
    private BoardController board;
    private CombatController combat;
    private WaveController waves;
    private RunUpgradeRuntime runtime;
    private EnemyActor collateralVictim, centerVictim;
    private Gem[,] grid;
    private readonly List<GemDamageContext> damage = new List<GemDamageContext>();
    private readonly List<BoardClearOutcome> outcomes = new List<BoardClearOutcome>();
    private RunUpgradeSpecialClearContext prepared;
    private int preparedCalls, finishedCalls;
    private UnityEngine.Random.State randomState;

    [SetUp]
    public void SetUp()
    {
        randomState = UnityEngine.Random.state;
        Assert.That(RunUpgradeRuntime.Current, Is.Null, "Use an isolated EditMode scene; never delete a live run.");
        Assert.That(RunUpgradeGameplayHooks.Current, Is.Null);
        root = new GameObject("CrackedChainScopeFixture");
        player = Child("Player").AddComponent<PlayerActor>();
        var definition = playerDefinition = UnityEngine.Object.Instantiate(Resources.Load<PlayerDefinition>("Players/Player_Bardley"));
        Assert.That(definition, Is.Not.Null);
        abilityDefinition=UnityEngine.Object.Instantiate(definition.ActiveAbility);
        abilityDefinition.GetType().GetField("energyCost",Flags).SetValue(abilityDefinition,80);
        Set(definition,"activeAbility",abilityDefinition);
        Set(definition,"baseGemDamage",100f); // Preserve the fixture's unrounded 100-damage arithmetic.
        player.Initialize(definition, 100);
        energy = player.gameObject.AddComponent<PlayerAbilityEnergy>();
        board = Child("Board").AddComponent<BoardController>();
        grid = new Gem[7, 8];
        Set(board, "width", 7); Set(board, "height", 8); Set(board, "gems", grid);
        waves = Child("Waves").AddComponent<WaveController>();
        Set(waves, "currentWave", 5);
        Set(waves, "advanceWavesAutomatically", false);
        Set(waves, "<IsWaveActive>k__BackingField", true);
        combat = Child("Combat").AddComponent<CombatController>();
        Set(combat, "playerActor", player); Set(combat, "waveController", waves);
        // Isolate percentage arithmetic from small-integer rounding.
        Set(combat, "damagePerGem", 100);
        Set(board, "combatController", combat);
        runtime = waves.gameObject.AddComponent<RunUpgradeRuntime>();
        var catalog = Resources.Load<RunUpgradeCatalog>("RunUpgrades/PrototypeRunUpgradeCatalog");
        Assert.That(catalog, Is.Not.Null);
        runtime.Configure(catalog, player, waves);
        var hooks = waves.gameObject.AddComponent<RunUpgradeGameplayHooks>();
        hooks.Configure(runtime, board, combat, player, waves);
        enemyDefinition = ScriptableObject.CreateInstance<EnemyDefinition>();
        collateralVictim = Enemy("CollateralVictim", GemType.Emerald);
        centerVictim = Enemy("CenterVictim", GemType.Ruby);
        combat.BeforeGemDamage += damage.Add;
        board.RunUpgradeSpecialClearPrepared += context => { prepared = context; preparedCalls++; };
        board.RunUpgradeSpecialClearFinished += () => finishedCalls++;
        board.BoardClearOutcomeResolved += outcome =>
        {
            Assert.That(RunUpgradeGameplayHooks.CurrentSpecialClearCount, Is.EqualTo(prepared.SpecialCount));
            Assert.That(RunUpgradeGameplayHooks.CurrentDirectionalBombCount, Is.EqualTo(prepared.DirectionalBombCount));
            outcomes.Add(outcome);
        };
    }

    [TearDown]
    public void TearDown()
    {
        if (root != null) { root.SetActive(false); UnityEngine.Object.DestroyImmediate(root); }
        if (enemyDefinition != null) UnityEngine.Object.DestroyImmediate(enemyDefinition);
        if (playerDefinition != null) UnityEngine.Object.DestroyImmediate(playerDefinition);
        if (abilityDefinition != null) UnityEngine.Object.DestroyImmediate(abilityDefinition);
        damage.Clear(); outcomes.Clear();
        preparedCalls = finishedCalls = 0;
        UnityEngine.Random.state = randomState;
    }

    [TestCase(GemSpecialType.RowBomb, 125, 1)]
    [TestCase(GemSpecialType.ColumnBomb, 125, 1)]
    [TestCase(GemSpecialType.PoisonBomb, 100, 0)]
    [TestCase(GemSpecialType.HealingBomb, 100, 0)]
    [TestCase(GemSpecialType.ShieldBomb, 100, 0)]
    public void BombsmithUsesParticipatingDirectionalBombs(GemSpecialType bomb, int expected, int directional)
    {
        Apply("RunUpgrade_Bombsmith");
        Report(new[] { bomb });
        AssertBatch(expected, 1, directional, 3);
    }

    [TestCase(1, 100)]
    [TestCase(2, 110)]
    [TestCase(4, 130)]
    [TestCase(5, 130)]
    public void ChainReactionUsesExistingBombClassifierAndCap(int bombCount, int expected)
    {
        Apply("RunUpgrade_ChainReaction");
        var bombs = new GemSpecialType[bombCount];
        for (int i = 0; i < bombs.Length; i++) bombs[i] = GemSpecialType.HealingBomb;
        Report(bombs);
        AssertBatch(expected, bombCount, 0, bombCount + 2);
    }

    [Test]
    public void BothCardsRemainAdditiveWithoutModifyingFixedCenterDamage()
    {
        Apply("RunUpgrade_Bombsmith"); Apply("RunUpgrade_ChainReaction");
        Report(new[] { GemSpecialType.RowBomb, GemSpecialType.ShieldBomb });
        AssertBatch(135, 2, 1, 4);
    }

    [Test]
    public void CrystalHiddenColorAndCrackedCenterDoNotManufactureBombCounts()
    {
        Apply("RunUpgrade_Bombsmith"); Apply("RunUpgrade_ChainReaction");
        Report(Array.Empty<GemSpecialType>(), includeCrystal: true);
        AssertBatch(100, 0, 0, 2);
        Assert.That(outcomes.Count, Is.EqualTo(2), "only the ordinary gem and fixed center report rewards");
    }

    [Test]
    public void WithoutCardsChainMetadataDoesNotChangeDamageOrActivateUtilities()
    {
        Report(new[] { GemSpecialType.RowBomb, GemSpecialType.HealingBomb, GemSpecialType.ShieldBomb });
        AssertBatch(100, 3, 1, 5);
        Assert.That(player.CurrentHealth, Is.EqualTo(100));
        Assert.That(player.CurrentShield, Is.Zero, "reporting is not utility-bomb commitment");
        Assert.That(energy.CurrentEnergy, Is.Zero, "base energy producer and refund cards are absent");
    }

    [Test]
    public void CompletedScopeCannotBoostTheFollowingBombFreeBatch()
    {
        Apply("RunUpgrade_Bombsmith"); Apply("RunUpgrade_ChainReaction");
        Report(new[] { GemSpecialType.RowBomb, GemSpecialType.HealingBomb });
        Assert.That(ReportedCollateralDamage(), Is.EqualTo(135));
        AssertScopeClosed();
        damage.Clear(); outcomes.Clear();
        Report(Array.Empty<GemSpecialType>());
        Assert.That(ReportedCollateralDamage(), Is.EqualTo(100));
        Assert.That(preparedCalls, Is.EqualTo(2)); Assert.That(finishedCalls, Is.EqualTo(2));
        AssertScopeClosed();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ReportingObserverFailureStillReleasesModifierScope(bool failDuringPreparation)
    {
        Apply("RunUpgrade_Bombsmith");
        if (failDuringPreparation)
            board.RunUpgradeSpecialClearPrepared += _ => throw new InvalidOperationException("scope-probe");
        else
            board.BoardClearOutcomeResolved += _ => throw new InvalidOperationException("scope-probe");
        var exception = Assert.Throws<TargetInvocationException>(() => Report(new[] { GemSpecialType.RowBomb }));
        Assert.That(exception.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(exception.InnerException.Message, Is.EqualTo("scope-probe"));
        Assert.That(preparedCalls, Is.EqualTo(1)); Assert.That(finishedCalls, Is.EqualTo(1));
        AssertScopeClosed();
        // This is cleanup, not rollback of damage already applied before an observer failed.
    }

    [TestCase(false)]
    [TestCase(true)]
    public void EmptyOrNullInputDoesNotPublishScopeOrRewards(bool useNull)
    {
        Call(board, "ReportCrackedClearSetToCombat", useNull ? null : new HashSet<Gem>(), null, 50);
        Assert.That(preparedCalls, Is.Zero); Assert.That(finishedCalls, Is.Zero);
        Assert.That(damage, Is.Empty); Assert.That(outcomes, Is.Empty);
    }

    [Test]
    public void AllSpecialFallbackWithoutCentersStillFinishesScope()
    {
        Apply("RunUpgrade_Bombsmith");
        var clear = new HashSet<Gem> { GemAt(1, 1, GemType.Emerald, GemSpecialType.ColumnBomb) };
        Call(board, "ReportCrackedClearSetToCombat", clear, null, 50);
        Assert.That(ReportedCollateralDamage(), Is.EqualTo(125));
        Assert.That(outcomes.Count, Is.EqualTo(1));
        Assert.That(preparedCalls, Is.EqualTo(1)); Assert.That(finishedCalls, Is.EqualTo(1));
        AssertScopeClosed();
    }

    [Test]
    public void InactiveEncounterKeepsBoardOutcomesWithoutApplyingEnemyDamage()
    {
        Apply("RunUpgrade_Bombsmith");
        Set(waves, "<IsWaveActive>k__BackingField", false);
        Report(new[] { GemSpecialType.RowBomb });
        Assert.That(damage, Is.Empty);
        Assert.That(outcomes.Count, Is.EqualTo(3));
        Assert.That(collateralVictim.CurrentHealth, Is.EqualTo(10000));
        Assert.That(centerVictim.CurrentHealth, Is.EqualTo(10000));
        Assert.That(preparedCalls, Is.EqualTo(1)); Assert.That(finishedCalls, Is.EqualTo(1));
        AssertScopeClosed();
    }

    [Test]
    public void RealExpansionPreparesNoUpgradeScopeUntilItsResultIsReported()
    {
        Apply("RunUpgrade_Bombsmith");
        Gem center = GemAt(1, 1, GemType.Ruby, GemSpecialType.Cracked);
        GemAt(2, 1, GemType.Sapphire, GemSpecialType.RowBomb);
        GemAt(6, 1, GemType.Emerald, GemSpecialType.None);
        object[] args = { new List<Gem> { center }, null, null };
        var expanded = (HashSet<Gem>)Call(board, "BuildCrackedExpandedClearSet", args);
        Call(board, "BuildCrackedExpandedClearSet", args);
        Assert.That(preparedCalls, Is.Zero); Assert.That(damage, Is.Empty);
        Assert.That(outcomes, Is.Empty);
        Call(board, "ReportCrackedClearSetToCombat", expanded, (HashSet<Gem>)args[1], 50);
        AssertBatch(125, 1, 1, 3);
    }

    private void AssertBatch(int expectedDamage, int bombs, int directional, int rewardableCount)
    {
        Assert.That(ReportedCollateralDamage(), Is.EqualTo(expectedDamage));
        Assert.That(collateralVictim.CurrentHealth, Is.EqualTo(10000 - expectedDamage));
        Assert.That(centerVictim.CurrentHealth, Is.EqualTo(9950), "fixed-center path is not reclassified as collateral");
        Assert.That(prepared.SpecialCount, Is.EqualTo(bombs));
        Assert.That(prepared.DirectionalBombCount, Is.EqualTo(directional));
        Assert.That(preparedCalls, Is.EqualTo(1)); Assert.That(finishedCalls, Is.EqualTo(1));
        int total = 0, centers = 0;
        foreach (BoardClearOutcome outcome in outcomes)
        {
            Assert.That(outcome.ClearContext.Source, Is.EqualTo(BoardClearSource.Ability));
            Assert.That(outcome.ClearContext.GrantsSpecialEnergy, Is.True);
            total += outcome.ClearContext.GemCount;
            if (outcome.ClearContext.IsFixedDamageExplosionCenter) centers++;
        }
        Assert.That(total, Is.EqualTo(rewardableCount)); Assert.That(centers, Is.EqualTo(1));
        AssertScopeClosed();
    }

    private int ReportedCollateralDamage()
    {
        var contexts = damage.FindAll(item => item.GemType == GemType.Emerald);
        Assert.That(contexts.Count, Is.EqualTo(1), "one collateral damage report for this color");
        return contexts[0].Damage;
    }

    private static void AssertScopeClosed()
    {
        Assert.That(RunUpgradeGameplayHooks.CurrentSpecialClearCount, Is.Zero);
        Assert.That(RunUpgradeGameplayHooks.CurrentDirectionalBombCount, Is.Zero);
    }

    private void Report(GemSpecialType[] bombs, bool includeCrystal = false)
    {
        Gem center = GemAt(0, 0, GemType.Ruby, GemSpecialType.Cracked);
        var clear = new HashSet<Gem> { center, GemAt(6, 6, GemType.Emerald, GemSpecialType.None) };
        for (int i = 0; i < bombs.Length; i++)
        {
            Gem bomb = GemAt(i, 2, GemType.Sapphire, bombs[i]);
            clear.Add(bomb); clear.Add(bomb); // A repeated reach never duplicates an authoritative clear member.
        }
        if (includeCrystal) clear.Add(GemAt(6, 7, GemType.Emerald, GemSpecialType.ColorCrystal));
        Call(board, "ReportCrackedClearSetToCombat", clear, new HashSet<Gem> { center }, 50);
    }

    private void Apply(string name)
    {
        var card = Resources.Load<RunUpgradeDefinition>("RunUpgrades/" + name);
        Assert.That(card, Is.Not.Null); Assert.That(runtime.TryApply(card, 5), Is.True);
    }

    private EnemyActor Enemy(string name, GemType color)
    {
        GameObject go = Child(name); go.SetActive(false);
        EnemyActor actor = go.AddComponent<EnemyActor>();
        Set(actor, "definition", enemyDefinition);
        Set(actor, "<RuntimeStats>k__BackingField", new EnemyRuntimeStats(5, 1, 10000, 0, 0, 10f, 5));
        Set(actor, "isInitialized", true); Set(actor, "currentHealth", 10000); Set(actor, "assignedGemType", color);
        Get<List<EnemyActor>>(waves, "activeEnemies").Add(actor);
        return actor;
    }

    private Gem GemAt(int x, int y, GemType type, GemSpecialType special)
    {
        GameObject go = Child("FixtureGem"); go.SetActive(false);
        Gem gem = go.AddComponent<Gem>();
        gem.Initialize(board, x, y, type, null, 1f); gem.SetSpecialType(special);
        grid[x, y] = gem;
        return gem;
    }

    private GameObject Child(string name)
    {
        var child = new GameObject(name); child.transform.SetParent(root.transform, false); return child;
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
