using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[NonParallelizable]
public sealed class EnemyDamageResultTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();

    [SetUp]
    public void SetUp()
    {
        Assert.That(RunUpgradeRuntime.Current, Is.Null,
            "These isolated damage fixtures require no live run-upgrade singleton.");
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = created.Count - 1; i >= 0; i--)
            if (created[i] != null) UnityEngine.Object.DestroyImmediate(created[i]);
        created.Clear();
    }

    [TestCase(0, 20, 20, 0)]
    [TestCase(30, 8, 0, 6)]
    [TestCase(10, 20, 5, 10)]
    [TestCase(30, 40, 0, 30)]
    public void DirectDamageKeepsShieldRoundingAndSeparateEvents(
        int shield, int amount, int expectedHpLoss, int expectedShieldLoss)
    {
        EnemyActor actor = Enemy("Target");
        actor.GrantShield(shield);
        int hpEvents = 0, shieldEvents = 0, reportedHp = 0, reportedShield = 0;
        actor.DamageReceived += (_, n) => { hpEvents++; reportedHp += n; };
        actor.ShieldDamaged += (_, n) => { shieldEvents++; reportedShield += n; };

        EnemyDamageResult result = actor.ResolveDirectDamage(amount);

        AssertResult(result, actor, expectedHpLoss, expectedShieldLoss);
        Assert.That(actor.CurrentHealth, Is.EqualTo(100 - expectedHpLoss));
        Assert.That(actor.CurrentShield, Is.EqualTo(shield - expectedShieldLoss));
        Assert.That(hpEvents, Is.EqualTo(expectedHpLoss > 0 ? 1 : 0));
        Assert.That(shieldEvents, Is.EqualTo(expectedShieldLoss > 0 ? 1 : 0));
        Assert.That(reportedHp, Is.EqualTo(expectedHpLoss));
        Assert.That(reportedShield, Is.EqualTo(expectedShieldLoss));
    }

    [Test]
    public void BreakingShieldDoesNotReduceTheFollowingHit()
    {
        EnemyActor actor = Enemy("Target");
        actor.GrantShield(10);
        AssertResult(actor.ResolveDirectDamage(20), actor, 5, 10);
        AssertResult(actor.ResolveDirectDamage(20), actor, 20, 0);
        Assert.That(actor.CurrentHealth, Is.EqualTo(75));
    }

    [Test]
    public void ConditionalDefencePrecedesShieldExactlyOnce()
    {
        EnemyActor actor = Enemy("Defended");
        int evaluations = 0;
        actor.IncomingDamageMultiplier = () => { evaluations++; return 0.8f; };
        actor.GrantShield(10);
        AssertResult(actor.ResolveDirectDamage(20), actor, 2, 10);
        Assert.That(evaluations, Is.EqualTo(1));
    }

    [Test]
    public void InterceptionReportsProtectorAndStopsAfterOneHop()
    {
        EnemyActor original = Enemy("Original");
        EnemyActor protector = Enemy("Protector", GemType.Sapphire);
        EnemyActor third = Enemy("Third", GemType.Topaz);
        Assert.That(original.SetDamageRedirectTarget(protector), Is.True);
        Assert.That(protector.SetDamageRedirectTarget(third), Is.True);
        int originalDefenceCalls = 0, protectorDefenceCalls = 0;
        original.IncomingDamageMultiplier = () => { originalDefenceCalls++; return 0.5f; };
        protector.IncomingDamageMultiplier = () => { protectorDefenceCalls++; return 0.8f; };
        protector.GrantShield(10);

        AssertResult(original.ResolveDirectDamage(20), protector, 2, 10);
        Assert.That(original.CurrentHealth, Is.EqualTo(100));
        Assert.That(third.CurrentHealth, Is.EqualTo(100));
        Assert.That(originalDefenceCalls, Is.Zero);
        Assert.That(protectorDefenceCalls, Is.EqualTo(1));
    }

    [Test]
    public void LethalInterceptionRetainsRecipientAfterRetreatCleanup()
    {
        EnemyActor original = Enemy("Original");
        EnemyActor protector = Enemy("Protector", GemType.Sapphire, 5);
        original.SetDamageRedirectTarget(protector);
        int deaths = 0;
        protector.Defeated += _ => { deaths++; original.ClearDamageRedirectTarget(protector); };

        EnemyDamageResult result = original.ResolveDirectDamage(20);
        AssertResult(result, protector, 5, 0);
        Assert.That(deaths, Is.EqualTo(1));
        Assert.That(original.CurrentHealth, Is.EqualTo(100), "Overkill does not spill into the protected actor.");
        Assert.That(original.DamageRedirectTarget, Is.Null);
        AssertResult(original.ResolveDirectDamage(20), original, 20, 0);
        Assert.That(protector.TryTakeDamage(1), Is.False);
        Assert.That(deaths, Is.EqualTo(1));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void NonPositiveDamageReturnsEmptyResult(int amount)
    {
        EnemyActor actor = Enemy("Target");
        EnemyDamageResult result = actor.ResolveDirectDamage(amount);
        Assert.That(result.Applied, Is.False);
        Assert.That(result.Recipient, Is.Null);
        Assert.That(result.HealthDamage, Is.Zero);
        Assert.That(result.ShieldDamage, Is.Zero);
        Assert.That(actor.TryTakeDamage(amount), Is.False);
        Assert.That(actor.CurrentHealth, Is.EqualTo(100));
    }

    [Test]
    public void UninitializedAndDefeatedActorsRejectDamage()
    {
        EnemyActor actor = Enemy("Target");
        Set(actor, "isInitialized", false);
        Assert.That(actor.ResolveDirectDamage(20).Applied, Is.False);
        Set(actor, "isInitialized", true);
        Set(actor, "isDefeated", true);
        Assert.That(actor.ResolveDamageWithoutFeedback(20).Applied, Is.False);
        Assert.That(actor.CurrentHealth, Is.EqualTo(100));
    }

    [Test]
    public void LegacyBoolWrappersApplyOneHitAndRetainDotBypass()
    {
        EnemyActor original = Enemy("Original");
        EnemyActor protector = Enemy("Protector", GemType.Sapphire);
        original.SetDamageRedirectTarget(protector);
        Assert.That(original.TryTakeDamage(7), Is.True);
        Assert.That(protector.CurrentHealth, Is.EqualTo(93));
        Assert.That(original.TryTakeDamageWithoutFeedback(9), Is.True);
        Assert.That(original.CurrentHealth, Is.EqualTo(91));
        Assert.That(protector.CurrentHealth, Is.EqualTo(93));
    }

    [Test]
    public void SnapshotDoesNotSubtractSynchronousHealing()
    {
        EnemyActor actor = Enemy("Target");
        actor.SurvivedHealthDamage += (_, __, ___) => actor.RestoreHealth(5);
        AssertResult(actor.ResolveDirectDamage(20), actor, 20, 0);
        Assert.That(actor.CurrentHealth, Is.EqualTo(85));
    }

    [Test]
    public void SnapshotDoesNotIncludeASeparateSynchronousHit()
    {
        EnemyActor actor = Enemy("Target");
        bool appliedExtra = false;
        EnemyDamageResult extra = default;
        actor.DamageReceived += (_, __) =>
        {
            if (appliedExtra) return;
            appliedExtra = true;
            extra = actor.ResolveDirectDamage(3);
        };
        EnemyDamageResult first = actor.ResolveDirectDamage(20);
        AssertResult(first, actor, 20, 0);
        AssertResult(extra, actor, 3, 0);
        Assert.That(actor.CurrentHealth, Is.EqualTo(77));
    }

    [Test]
    public void SuppressedDamageBypassesProtectorAndDoesNotEmitStaggerInputs()
    {
        EnemyActor actor = Enemy("Poisoned");
        EnemyActor protector = Enemy("Protector", GemType.Sapphire);
        actor.SetDamageRedirectTarget(protector);
        actor.GrantShield(10);
        int hpFeedback = 0, shieldFeedback = 0, healthChanges = 0, shieldChanges = 0;
        actor.DamageReceived += (_, __) => hpFeedback++;
        actor.ShieldDamaged += (_, __) => shieldFeedback++;
        actor.HealthChanged += (_, __, ___) => healthChanges++;
        actor.ShieldChanged += (_, __, ___) => shieldChanges++;

        AssertResult(actor.ResolveDamageWithoutFeedback(20), actor, 5, 10);
        Assert.That(protector.CurrentHealth, Is.EqualTo(100));
        Assert.That(hpFeedback + shieldFeedback, Is.Zero);
        Assert.That(healthChanges, Is.EqualTo(1));
        Assert.That(shieldChanges, Is.EqualTo(1));
    }

    [TestCase(0, 20)]
    [TestCase(10, 5)]
    [TestCase(30, 0)]
    public void CombatReportsActualRecipientAndPreservesOriginalClearContext(int shield, int hpLoss)
    {
        EnemyActor original = Enemy("Original");
        EnemyActor protector = Enemy("Protector", GemType.Sapphire);
        original.SetDamageRedirectTarget(protector);
        protector.GrantShield(shield);
        CombatController combat = Combat(original, protector);
        int reports = 0, successfulTargets = -1;
        var context = new BoardClearContext(GemType.Ruby, 1, 0, BoardClearSource.Bomb);
        combat.EnemyDamagedByGemClear += (recipient, damage, hp) =>
        {
            reports++;
            Assert.That(recipient, Is.SameAs(protector));
            Assert.That(hp, Is.EqualTo(hpLoss));
            Assert.That(damage.GemType, Is.EqualTo(GemType.Ruby));
            Assert.That(damage.ClearSource, Is.EqualTo(BoardClearSource.Bomb));
        };
        combat.GemDamageResolved += (_, count) => successfulTargets = count;
        Assert.That(combat.ResolveFixedGemDamage(context, 20), Is.True);
        Assert.That(reports, Is.EqualTo(1));
        Assert.That(successfulTargets, Is.EqualTo(1), "Shield-only absorption remains a successful hit.");
        Assert.That(original.CurrentHealth, Is.EqualTo(100));
        Assert.That(protector.CurrentHealth, Is.EqualTo(100 - hpLoss));
    }

    [Test]
    public void CombatStillRejectsUnmatchedAndInactiveWaveDamage()
    {
        EnemyActor actor = Enemy("Target", GemType.Sapphire);
        CombatController combat = Combat(actor);
        var context = new BoardClearContext(GemType.Ruby, 1, 0, BoardClearSource.Match);
        Assert.That(combat.ResolveFixedGemDamage(context, 20), Is.False);
        actor.AssignGemType(GemType.Ruby);
        Set(combat.WaveController, "<IsWaveActive>k__BackingField", false);
        Assert.That(combat.ResolveFixedGemDamage(context, 20), Is.False);
        Assert.That(actor.CurrentHealth, Is.EqualTo(100));
    }

    [Test]
    public void CombatReportsHitRatherThanNetHealthChangeAfterCallbacks()
    {
        EnemyActor actor = Enemy("Target");
        actor.SurvivedHealthDamage += (_, __, ___) => actor.RestoreHealth(5);
        CombatController combat = Combat(actor);
        int reported = -1;
        combat.EnemyDamagedByGemClear += (_, __, hp) => reported = hp;
        Assert.That(combat.ResolveFixedGemDamage(
            new BoardClearContext(GemType.Ruby, 1, 0, BoardClearSource.Match), 20), Is.True);
        Assert.That(reported, Is.EqualTo(20));
        Assert.That(actor.CurrentHealth, Is.EqualTo(85));
    }

    [TestCase(0, 20, 1)]
    [TestCase(30, 0, 0)]
    public void RoyalDecreeKeepsItsMarkButReportsInterceptedHpHits(int shield, int hpLoss, int expectedReports)
    {
        EnemyActor original = Enemy("Marked");
        EnemyActor protector = Enemy("Protector", GemType.Sapphire);
        original.SetDamageRedirectTarget(protector);
        protector.GrantShield(shield);
        RoyalDecreeRuntime decree = ObjectWith<RoyalDecreeRuntime>("Decree");
        RoyalDecreeAbilityDefinition ability = Data<RoyalDecreeAbilityDefinition>();
        Set(ability, "damagePerGem", 20);
        Set(decree, "activeDefinition", ability);
        Set(decree, "<IsActive>k__BackingField", true);
        Call(decree, "SetTarget", original);
        int reports = 0;
        decree.HitResolved += (recipient, damage, context) =>
        {
            reports++;
            Assert.That(recipient, Is.SameAs(protector));
            Assert.That(damage, Is.EqualTo(hpLoss));
            Assert.That(context.GemType, Is.EqualTo(GemType.Topaz));
        };
        try
        {
            Call(decree, "HandleBoardClearResolved",
                new BoardClearContext(GemType.Topaz, 1, 0, BoardClearSource.Match));
            Assert.That(reports, Is.EqualTo(expectedReports));
            Assert.That(decree.CurrentTarget, Is.SameAs(original));
            Assert.That(protector.CurrentHealth, Is.EqualTo(100 - hpLoss));
            Assert.That(original.CurrentHealth, Is.EqualTo(100));
        }
        finally { decree.Cancel(); }
    }

    [Test]
    public void PoisonReportsItsOwnHpLossAndKeepsInterceptionBypass()
    {
        EnemyActor actor = Enemy("Poisoned");
        EnemyActor protector = Enemy("Protector", GemType.Sapphire);
        actor.SetDamageRedirectTarget(protector);
        actor.SurvivedHealthDamage += (_, __, ___) => actor.RestoreHealth(4);
        EnemyPoisonStatus poison = actor.gameObject.AddComponent<EnemyPoisonStatus>();
        poison.Apply(7f, 1f, 10);
        int damageReported = 0, normalFeedback = 0;
        poison.TickDamageApplied += (_, n) => damageReported += n;
        actor.DamageReceived += (_, __) => normalFeedback++;
        Call(poison, "ApplyTick");
        Assert.That(damageReported, Is.EqualTo(10));
        Assert.That(actor.CurrentHealth, Is.EqualTo(94));
        Assert.That(protector.CurrentHealth, Is.EqualTo(100));
        Assert.That(normalFeedback, Is.Zero);
    }

    [Test]
    public void ShieldOnlyPoisonKeepsHpOnlyFeedbackContract()
    {
        EnemyActor actor = Enemy("Poisoned");
        actor.GrantShield(30);
        EnemyPoisonStatus poison = actor.gameObject.AddComponent<EnemyPoisonStatus>();
        poison.Apply(7f, 1f, 8);
        int reports = 0;
        poison.TickDamageApplied += (_, __) => reports++;
        Call(poison, "ApplyTick");
        Assert.That(actor.CurrentShield, Is.EqualTo(24));
        Assert.That(actor.CurrentHealth, Is.EqualTo(100));
        Assert.That(reports, Is.Zero);
    }

    private EnemyActor Enemy(string name, GemType color = GemType.Ruby, int hp = 100)
    {
        EnemyActor actor = ObjectWith<EnemyActor>(name);
        // Establish only isolated gameplay state. Do not install visual assets,
        // launch an encounter, or mutate saved definitions/preferences.
        Set(actor, "definition", Data<EnemyDefinition>());
        Set(actor, "<RuntimeStats>k__BackingField", new EnemyRuntimeStats(1, 1, hp, 1, 0, 10f, 3));
        Set(actor, "assignedGemType", color);
        Set(actor, "currentHealth", hp);
        Set(actor, "isInitialized", true);
        return actor;
    }

    private CombatController Combat(params EnemyActor[] actors)
    {
        PlayerActor player = ObjectWith<PlayerActor>("Player");
        Set(player, "isInitialized", true);
        Set(player, "maximumHealth", 100);
        Set(player, "currentHealth", 100);
        WaveController waves = ObjectWith<WaveController>("Waves");
        ((List<EnemyActor>)Get(waves, "activeEnemies")).AddRange(actors);
        Set(waves, "<IsWaveActive>k__BackingField", true);
        CombatController combat = ObjectWith<CombatController>("Combat");
        Set(combat, "playerActor", player);
        Set(combat, "waveController", waves);
        return combat;
    }

    private T ObjectWith<T>(string name) where T : Component
    {
        GameObject go = new GameObject(name);
        go.SetActive(false);
        created.Add(go);
        return go.AddComponent<T>();
    }

    private T Data<T>() where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        created.Add(asset);
        return asset;
    }

    private static void AssertResult(EnemyDamageResult result, EnemyActor recipient, int hp, int shield)
    {
        Assert.That(result.Recipient, Is.SameAs(recipient));
        Assert.That(result.HealthDamage, Is.EqualTo(hp));
        Assert.That(result.ShieldDamage, Is.EqualTo(shield));
        Assert.That(result.Applied, Is.EqualTo(hp > 0 || shield > 0));
    }

    private static object Get(object target, string name) => Field(target, name).GetValue(target);
    private static void Set(object target, string name, object value) => Field(target, name).SetValue(target, value);
    private static FieldInfo Field(object target, string name) => target.GetType().GetField(name, Flags)
        ?? throw new MissingFieldException(target.GetType().Name, name);
    private static object Call(object target, string name, params object[] args) =>
        (target.GetType().GetMethod(name, Flags) ?? throw new MissingMethodException(name)).Invoke(target, args);
}
