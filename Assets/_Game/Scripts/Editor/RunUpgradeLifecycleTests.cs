using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode only. Synthetic assets/actors avoid changing player preferences or
// authored content. Event delivery below is controlled; real scene/wave timing
// and reward presentation still require the final combined Play Mode pass.
[NonParallelizable]
public sealed class RunUpgradeLifecycleTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private readonly List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
    private GameObject root;
    private PlayerDefinition playerDefinition;
    private PlayerActor player;
    private PlayerAbilityEnergy energy;
    private BoardController board;
    private CombatController combat;
    private WaveController waves;
    private RunUpgradeRuntime runtime;
    private RunUpgradeGameplayHooks hooks;
    private RunUpgradeCatalog catalog;
    private RunUpgradeDefinition health, damage, prepared, plating, conductor;

    [TearDown]
    public void TearDown()
    {
        if (root != null)
        {
            root.SetActive(false);
            UnityEngine.Object.DestroyImmediate(root);
        }
        for (int i = assets.Count - 1; i >= 0; i--)
            if (assets[i] != null) UnityEngine.Object.DestroyImmediate(assets[i]);
        assets.Clear();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ResetRestoresUnmodifiedMaximumWithoutHealingAndReapplyDoesNotCompound(bool hooksEnabled)
    {
        CreateFixture(hooksEnabled);
        Apply(health);
        Assert.That(player.MaximumHealth, Is.EqualTo(120));
        player.TryTakeDamage(40);
        Assert.That(player.CurrentHealth, Is.EqualTo(80));
        runtime.ResetRun();
        Assert.That(runtime.GetOwnedDefinitions(), Is.Empty);
        Assert.That(runtime.BaseMaximumHealth, Is.EqualTo(100));
        Assert.That(player.MaximumHealth, Is.EqualTo(100));
        Assert.That(player.CurrentHealth, Is.EqualTo(80));
        Apply(health);
        Assert.That(player.MaximumHealth, Is.EqualTo(120));
        Assert.That(player.CurrentHealth, Is.EqualTo(100));
        Assert.That(runtime.GetStackCount(health), Is.EqualTo(1));
    }

    [Test]
    public void ResetClampsHpButDoesNotAlterShieldOrEnergy()
    {
        CreateFixture();
        Apply(health);
        player.GrantShield(12);
        energy.AddEnergy(23);
        runtime.ResetRun();
        Assert.That(player.CurrentHealth, Is.EqualTo(100));
        Assert.That(player.MaximumHealth, Is.EqualTo(100));
        Assert.That(player.CurrentShield, Is.EqualTo(12));
        Assert.That(energy.CurrentEnergy, Is.EqualTo(23));
    }

    [Test]
    public void ResetDoesNotReviveDefeatedPlayer()
    {
        CreateFixture();
        Apply(health);
        player.TryTakeDamage(10000);
        Assert.That(player.IsDefeated, Is.True);
        runtime.ResetRun();
        Assert.That(player.MaximumHealth, Is.EqualTo(100));
        Assert.That(player.CurrentHealth, Is.Zero);
        Assert.That(player.IsDefeated, Is.True);
    }

    [TestCase(80, false)]
    [TestCase(175, true)]
    public void ReinitializationAdoptsNewBaselineRegardlessOfObserverOrder(int newMaximum, bool runtimeLast)
    {
        CreateFixture();
        Apply(health);
        Apply(prepared);
        if (runtimeLast)
        {
            runtime.enabled = false;
            runtime.enabled = true;
        }
        int revision = runtime.RunRevision;
        player.Initialize(playerDefinition, newMaximum);
        Assert.That(runtime.RunRevision, Is.EqualTo(revision + 1));
        Assert.That(runtime.GetOwnedDefinitions(), Is.Empty);
        Assert.That(runtime.BaseMaximumHealth, Is.EqualTo(newMaximum));
        Assert.That(player.MaximumHealth, Is.EqualTo(newMaximum));
        Apply(health);
        Assert.That(player.MaximumHealth, Is.EqualTo(newMaximum + 20));
        Assert.That(player.CurrentHealth, Is.EqualTo(newMaximum + 20));
    }

    [Test]
    public void RepeatConfigurationPreservesCardsDraftStreamAndSameWaveTracking()
    {
        CreateFixture();
        Apply(health);
        System.Random random = runtime.GetDraftRandom();
        var expected = new SavedRandom(runtime.CardSeed);
        Assert.That(random.Next(), Is.EqualTo(expected.Next()));
        int revision = runtime.RunRevision;
        Set(hooks, "emergencyPlatingUsedThisWave", true);
        Set(hooks, "resonantCrackCount", 2);
        PrepareClear(3, 2);
        for (int i = 0; i < 5; i++)
        {
            runtime.Configure(catalog, player, waves);
            hooks.Configure(runtime, board, combat, player, waves);
        }
        Assert.That(runtime.GetDraftRandom(), Is.SameAs(random));
        Assert.That(random.Next(), Is.EqualTo(expected.Next()));
        Assert.That(runtime.RunRevision, Is.EqualTo(revision));
        Assert.That(runtime.GetStackCount(health), Is.EqualTo(1));
        Assert.That(player.MaximumHealth, Is.EqualTo(120));
        Assert.That(Get<bool>(hooks, "emergencyPlatingUsedThisWave"), Is.True);
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.EqualTo(2));
        Assert.That(RunUpgradeGameplayHooks.CurrentSpecialClearCount, Is.EqualTo(3));
    }

    [Test]
    public void RebindingDifferentPlayerRestoresOldHpAndUsesNewBaseline()
    {
        CreateFixture();
        Apply(health);
        PlayerActor previous = player;
        player = Child("SecondPlayer").AddComponent<PlayerActor>();
        player.Initialize(playerDefinition, 200);
        runtime.Configure(catalog, player, waves);
        hooks.Configure(runtime, board, combat, player, waves);
        Assert.That(previous.MaximumHealth, Is.EqualTo(100));
        Assert.That(runtime.GetOwnedDefinitions(), Is.Empty);
        Assert.That(runtime.BaseMaximumHealth, Is.EqualTo(200));
        Apply(health);
        Assert.That(player.MaximumHealth, Is.EqualTo(220));
        previous.Initialize(playerDefinition, 300);
        Assert.That(runtime.GetStackCount(health), Is.EqualTo(1), "old player callbacks are disconnected");
        Assert.That(runtime.BaseMaximumHealth, Is.EqualTo(200));
    }

    [Test]
    public void ExplicitResetPublishesOnceAndReseedsOnlyTheDraftStream()
    {
        CreateFixture();
        Apply(damage);
        System.Random before = runtime.GetDraftRandom();
        int seed = runtime.CardSeed;
        before.Next();
        int notifications = 0;
        runtime.RunReset += () => notifications++;
        runtime.ResetRun();
        Assert.That(notifications, Is.EqualTo(1));
        Assert.That(runtime.CardSeed, Is.Zero);
        Assert.That(Get<object>(waves, "encounterRandom"), Is.Null);
        System.Random after = runtime.GetDraftRandom();
        Assert.That(after, Is.Not.SameAs(before));
        Assert.That(runtime.CardSeed, Is.EqualTo(seed));
        Assert.That(after.Next(), Is.EqualTo(new SavedRandom(seed).Next()));
        Assert.That(RunUpgradeResolver.ResolveGemDamage(100, default, runtime), Is.EqualTo(100));
    }

    [Test]
    public void ResetWhileHooksDisabledIsReconciledWithoutReplayingRewards()
    {
        CreateFixture();
        Apply(health);
        Set(hooks, "emergencyPlatingUsedThisWave", true);
        Set(hooks, "resonantCrackCount", 2);
        hooks.enabled = false;
        runtime.ResetRun();
        hooks.enabled = true;
        Assert.That(Get<int>(hooks, "baseMaximumHealth"), Is.EqualTo(100));
        Assert.That(Get<bool>(hooks, "emergencyPlatingUsedThisWave"), Is.False);
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.Zero);
        Assert.That(energy.CurrentEnergy, Is.Zero);
        Assert.That(player.CurrentShield, Is.Zero);
        Apply(health);
        Assert.That(player.MaximumHealth, Is.EqualTo(120));
    }

    [TestCase(1)]
    [TestCase(5)]
    public void ReenableRestoresExactlyOneWaveRewardSubscription(int cycles)
    {
        CreateFixture();
        Apply(prepared);
        for (int i = 0; i < cycles; i++)
        {
            hooks.enabled = false;
            StartWave(6 + i);
            Assert.That(energy.CurrentEnergy, Is.Zero);
            hooks.enabled = true;
            Assert.That(energy.CurrentEnergy, Is.Zero, "enable does not replay missed waves");
        }
        StartWave(20);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(15));
        hooks.Configure(runtime, board, combat, player, waves);
        StartWave(21);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(30));
    }

    [Test]
    public void ConfigureWhileDisabledDoesNotInstallRewardCallbacks()
    {
        CreateFixture();
        Apply(prepared);
        hooks.enabled = false;
        hooks.Configure(runtime, board, combat, player, waves);
        StartWave(6);
        Assert.That(energy.CurrentEnergy, Is.Zero);
        hooks.enabled = true;
        StartWave(7);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(15));
    }

    [Test]
    public void EmergencyPlatingCannotBeRearmedBySameWaveReenable()
    {
        CreateFixture();
        Apply(plating);
        StartWave(5);
        player.GrantShield(5);
        player.TryTakeDamage(7);
        Assert.That(player.CurrentShield, Is.EqualTo(10), "first break procs plating");
        hooks.enabled = false;
        hooks.enabled = true;
        player.TryTakeDamage(14);
        Assert.That(player.CurrentShield, Is.Zero, "second break in same wave does not proc");
        StartWave(6);
        player.GrantShield(5);
        player.TryTakeDamage(7);
        Assert.That(player.CurrentShield, Is.EqualTo(10), "actual new wave rearms once");
    }

    [Test]
    public void MissedClearFinishedCannotLeaveStaleChainContextAfterEnable()
    {
        CreateFixture();
        PrepareClear(4, 2);
        Assert.That(RunUpgradeGameplayHooks.CurrentSpecialClearCount, Is.EqualTo(4));
        hooks.enabled = false;
        (Get<Delegate>(board, "RunUpgradeSpecialClearFinished") as Action)?.Invoke();
        hooks.enabled = true;
        Assert.That(RunUpgradeGameplayHooks.CurrentSpecialClearCount, Is.Zero);
        Assert.That(RunUpgradeGameplayHooks.CurrentDirectionalBombCount, Is.Zero);
        PrepareClear(2, 1);
        Assert.That(RunUpgradeGameplayHooks.CurrentSpecialClearCount, Is.EqualTo(2));
        (Get<Delegate>(board, "RunUpgradeSpecialClearFinished") as Action)?.Invoke();
        Assert.That(RunUpgradeGameplayHooks.CurrentSpecialClearCount, Is.Zero);
    }

    [Test]
    public void ChromaticConductorReceivesOneBoardOutcomeAfterRepeatedEnable()
    {
        CreateFixture();
        Apply(conductor);
        for (int i = 0; i < 5; i++) { hooks.enabled = false; hooks.enabled = true; }
        var context = new BoardClearContext(GemType.Ruby, 4, 0, BoardClearSource.ColorCrystal);
        (Get<Delegate>(board, "BoardClearOutcomeResolved") as Action<BoardClearOutcome>)
            ?.Invoke(new BoardClearOutcome(context, false));
        Assert.That(energy.CurrentEnergy, Is.EqualTo(4), "only hook bonus exists in this fixture");
    }

    [Test]
    public void ResetClearsMechanicBookkeepingWithoutAnArtificialWaveStart()
    {
        CreateFixture();
        Apply(prepared);
        Set(hooks, "emergencyPlatingUsedThisWave", true);
        Set(hooks, "resonantCrackCount", 2);
        PrepareClear(4, 2);
        int wave = waves.CurrentWave;
        runtime.ResetRun();
        Assert.That(waves.CurrentWave, Is.EqualTo(wave));
        Assert.That(Get<bool>(hooks, "emergencyPlatingUsedThisWave"), Is.False);
        Assert.That(Get<int>(hooks, "resonantCrackCount"), Is.Zero);
        Assert.That(RunUpgradeGameplayHooks.CurrentSpecialClearCount, Is.Zero);
        Assert.That(RunUpgradeGameplayHooks.CurrentDirectionalBombCount, Is.Zero);
        Assert.That(energy.CurrentEnergy, Is.Zero);
    }

    [Test]
    public void RebindingHooksUsesRuntimeBaselineNotAlreadyUpgradedHp()
    {
        CreateFixture();
        Apply(health);
        BoardController previous = board;
        board = Child("ReplacementBoard").AddComponent<BoardController>();
        hooks.Configure(runtime, board, combat, player, waves);
        Apply(damage);
        Assert.That(player.MaximumHealth, Is.EqualTo(120), "unrelated card does not compound HP");
        (Get<Delegate>(previous, "RunUpgradeSpecialClearPrepared") as Action<RunUpgradeSpecialClearContext>)
            ?.Invoke(new RunUpgradeSpecialClearContext(4, 2));
        Assert.That(RunUpgradeGameplayHooks.CurrentSpecialClearCount, Is.Zero);
        PrepareClear(2, 1);
        Assert.That(RunUpgradeGameplayHooks.CurrentSpecialClearCount, Is.EqualTo(2));
    }

    [TestCase(1, 198)]
    [TestCase(2, 378)]
    public void NumericStackingOrderRemainsFlatThenAdditiveThenMultiplicative(int stacks, int expected)
    {
        CreateFixture();
        RunUpgradeDefinition mixed = Upgrade("audit_mixed", new[]
        {
            Modifier(RunUpgradeStat.GemDamage, RunUpgradeModifierOperation.Flat, 10f),
            Modifier(RunUpgradeStat.GemDamage, RunUpgradeModifierOperation.AdditivePercent, 0.2f),
            Modifier(RunUpgradeStat.GemDamage, RunUpgradeModifierOperation.Multiplicative, 1.5f)
        });
        for (int i = 0; i < stacks; i++) Apply(mixed);
        Assert.That(RunUpgradeResolver.ResolveGemDamage(100, default, runtime), Is.EqualTo(expected));
        runtime.ResetRun();
        Assert.That(RunUpgradeResolver.ResolveGemDamage(100, default, runtime), Is.EqualTo(100));
    }

    private void CreateFixture(bool hooksEnabled = true)
    {
        Assert.That(RunSession.Current, Is.Null, "Use an isolated fixture, never remove a live gameplay run.");
        Assert.That(RunUpgradeRuntime.Current == null && RunUpgradeGameplayHooks.Current == null,
            Is.True, "Requires an isolated editor fixture; do not modify another run.");
        root = new GameObject("RunUpgradeLifecycleFixture");
        root.SetActive(false);
        playerDefinition = Asset<PlayerDefinition>();
        GameObject playerObject = Child("Player");
        player = playerObject.AddComponent<PlayerActor>();
        energy = playerObject.AddComponent<PlayerAbilityEnergy>();
        player.Initialize(playerDefinition, 100);
        board = Child("Board").AddComponent<BoardController>();
        combat = Child("Combat").AddComponent<CombatController>();
        GameObject waveObject = Child("Waves");
        waves = waveObject.AddComponent<WaveController>();
        Set(waves, "encounterSeed", 3210);
        runtime = waveObject.AddComponent<RunUpgradeRuntime>();
        hooks = waveObject.AddComponent<RunUpgradeGameplayHooks>();
        hooks.enabled = hooksEnabled;
        health = Upgrade("audit_health", new[] { Modifier(RunUpgradeStat.MaximumHealth, RunUpgradeModifierOperation.Flat, 20f) });
        damage = Upgrade("audit_damage", new[] { Modifier(RunUpgradeStat.GemDamage, RunUpgradeModifierOperation.AdditivePercent, 0.15f) });
        prepared = Upgrade("audit_prepared", mechanic: RunUpgradeMechanic.PreparedCasting);
        plating = Upgrade("audit_plating", mechanic: RunUpgradeMechanic.EmergencyPlating);
        conductor = Upgrade("audit_conductor", mechanic: RunUpgradeMechanic.ChromaticConductor);
        catalog = Asset<RunUpgradeCatalog>();
        Set(catalog, "upgrades", new[] { health, damage, prepared, plating, conductor });
        runtime.Configure(catalog, player, waves);
        hooks.Configure(runtime, board, combat, player, waves);
        root.SetActive(true);
    }

    private GameObject Child(string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(root.transform, false);
        return child;
    }

    private T Asset<T>() where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        assets.Add(asset);
        return asset;
    }

    private RunUpgradeDefinition Upgrade(string id, RunUpgradeModifier[] modifiers = null,
        RunUpgradeMechanic mechanic = RunUpgradeMechanic.None)
    {
        RunUpgradeDefinition definition = Asset<RunUpgradeDefinition>();
        Set(definition, "upgradeId", id);
        Set(definition, "maxStacks", 3);
        Set(definition, "modifiers", modifiers ?? Array.Empty<RunUpgradeModifier>());
        if (mechanic != RunUpgradeMechanic.None)
        {
            var grant = new RunUpgradeMechanicGrant();
            Set(grant, "mechanic", mechanic);
            Set(definition, "mechanicGrants", new[] { grant });
        }
        return definition;
    }

    private static RunUpgradeModifier Modifier(RunUpgradeStat stat, RunUpgradeModifierOperation operation, float value)
    {
        var modifier = new RunUpgradeModifier();
        Set(modifier, "stat", stat); Set(modifier, "operation", operation); Set(modifier, "value", value);
        return modifier;
    }

    private void Apply(RunUpgradeDefinition definition) => Assert.That(runtime.TryApply(definition, 5), Is.True);
    private void StartWave(int wave)
    {
        Set(waves, "currentWave", wave);
        Set(waves, "<IsWaveActive>k__BackingField", true);
        (Get<Delegate>(waves, "WaveStarted") as Action<int>)?.Invoke(wave);
    }
    private void PrepareClear(int specials, int directional)
    {
        (Get<Delegate>(board, "RunUpgradeSpecialClearPrepared") as Action<RunUpgradeSpecialClearContext>)
            ?.Invoke(new RunUpgradeSpecialClearContext(specials, directional));
    }
    private static FieldInfo Field(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, Flags);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        return field;
    }
    private static T Get<T>(object target, string name) => (T)Field(target, name).GetValue(target);
    private static void Set(object target, string name, object value) => Field(target, name).SetValue(target, value);
}
