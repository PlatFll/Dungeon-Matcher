using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class UnifiedMoveCombatTests
{
    [TestCase(4, 1f, 4)]
    [TestCase(4, 1.4f, 3)]
    [TestCase(5, 1.25f, 4)]
    [TestCase(2, 5f, 2)]
    [TestCase(6, 1.2f, 5)]
    public void HasteUsesWholeIntervalsWithTwoMoveFloor(int basis, float speed, int expected)
        => Assert.That(EnemyAutoAttack.MoveInterval(basis, speed), Is.EqualTo(expected));

    [Test] public void UnifiedSavesNeverInterpretLegacyFloatCountdowns()
    {
        var saved = new RunCombatSnapshot { version = 2, clock = new CombatClockSnapshot { profile = CombatClockSnapshot.UnifiedProfile } };
        saved.clock.actions.nextActorId = 2;
        saved.enemies.Add(new EnemyCombatSnapshot { persistentId = 1, attackRemaining = 3.75f });
        Assert.That(RunContinuation.SupportsSnapshot(saved), Is.False);
        saved.enemies[0].unifiedAttackRemaining = 3;
        Assert.That(RunContinuation.SupportsSnapshot(saved), Is.True);
        var copy = JsonUtility.FromJson<RunCombatSnapshot>(JsonUtility.ToJson(saved));
        Assert.That(copy.enemies[0].unifiedAttackRemaining, Is.EqualTo(3));
        copy.clock.profile = CombatClockSnapshot.LegacyEffectsProfile;
        copy.enemies[0].unifiedAttackRemaining = -1;
        Assert.That(RunContinuation.SupportsSnapshot(copy), Is.True);
        Assert.That(copy.enemies[0].attackRemaining, Is.EqualTo(3.75f));
    }
}

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator LaunchUnified(string zone = "dungeon")
    {
        RunLaunchOptions.StartingZone = zone;
        SceneManager.LoadScene("Game");
        yield return Stable();
        Assert.That(CombatMoveClock.Unified, Is.True);
        Assert.That(Run.Continuation.Capture().clock.profile, Is.EqualTo(CombatClockSnapshot.UnifiedProfile));
    }

    [UnityTest] public IEnumerator UnifiedBasicsUseAcceptedMovesAndResumeExactIntegers()
    {
        yield return LaunchUnified();
        var enemy = Run.Waves.ActiveEnemies.First();
        var attack = enemy.GetComponent<EnemyAutoAttack>();
        Set(attack, "unifiedRemaining", 4);
        int hits = 0;
        attack.AttackStarted += _ => hits++;
        yield return new WaitForSeconds(1.2f);
        Assert.That(attack.RemainingAttackTime, Is.EqualTo(4));
        for (int expected = 3; expected >= 1; expected--)
        {
            yield return Move();
            Assert.That(attack.RemainingAttackTime, Is.EqualTo(expected));
            Assert.That(hits, Is.Zero);
        }
        var saved = new EnemyCombatSnapshot();
        attack.CaptureContinuation(saved);
        attack.RestoreContinuation(saved);
        Assert.That(attack.RemainingAttackTime, Is.EqualTo(1));
        yield return Move();
        Assert.That(hits, Is.EqualTo(1));
        Assert.That(attack.RemainingAttackTime, Is.EqualTo(attack.EffectiveMoveInterval));
    }

    [UnityTest] public IEnumerator UnifiedHasteNeverGrantsImmediateOrFractionalBasic()
    {
        yield return LaunchUnified();
        var attack = Run.Waves.ActiveEnemies.First().GetComponent<EnemyAutoAttack>();
        Set(attack, "unifiedRemaining", 2);
        object first = new object(), second = new object();
        attack.SetNormalAttackModifiers(first, 1, 1.4f);
        Assert.That(attack.RemainingAttackTime, Is.EqualTo(1));
        attack.SetNormalAttackModifiers(second, 1, 1.25f);
        Assert.That(attack.EffectiveMoveInterval, Is.EqualTo(3), "haste sources do not multiply");
        yield return new WaitForSeconds(.5f);
        Assert.That(attack.RemainingAttackTime, Is.EqualTo(1));
        attack.RemoveNormalAttackModifiers(first);
        attack.RemoveNormalAttackModifiers(second);
        Assert.That(attack.RemainingAttackTime, Is.EqualTo(1), "expiry does not refund progress");
        yield return Move();
        Assert.That(attack.RemainingAttackTime, Is.EqualTo(4));
    }

    private void DurableUnifiedFixture()
    {
        Set(Run.Player, "maximumHealth", 9995); Set(Run.Player, "currentHealth", 9995);
        foreach (var actor in Run.Waves.ActiveEnemies)
        {
            typeof(EnemyActor).GetProperty("RuntimeStats", Flags).SetValue(actor,
                new EnemyRuntimeStats(1, 1, 9995, 5, 0, 10, actor.SpecialTurnRequirement));
            Set(actor, "currentHealth", 9995);
        }
    }

    [UnityTest] public IEnumerator UnifiedStaggerProtectsBirthAndTwoFullFutureMoves()
    {
        yield return LaunchUnified(); DurableUnifiedFixture();
        var actor = Run.Waves.ActiveEnemies.First();
        var stagger = actor.GetComponent<EnemyStagger>();
        var attack = actor.GetComponent<EnemyAutoAttack>();
        Set(attack, "unifiedRemaining", 1);
        int hits = 0; attack.AttackStarted += _ => hits++;
        yield return Move(() => stagger.RegisterDamage(Mathf.CeilToInt(stagger.DamageThreshold)));
        Assert.That(stagger.RemainingStaggerTime, Is.EqualTo(2)); Assert.That(hits, Is.Zero);
        yield return new WaitForSeconds(.3f);
        Assert.That(stagger.RemainingStaggerTime, Is.EqualTo(2));
        yield return Move(); Assert.That(stagger.RemainingStaggerTime, Is.EqualTo(1)); Assert.That(hits, Is.Zero);
        yield return Move(); Assert.That(stagger.IsStaggered, Is.False); Assert.That(hits, Is.Zero);
        Assert.That(stagger.RemainingImmunityTime, Is.EqualTo(2));
        yield return Move(); Assert.That(hits, Is.EqualTo(1)); Assert.That(stagger.RemainingImmunityTime, Is.EqualTo(1));
        Assert.That(stagger.ApplyStagger(1, 1), Is.Zero, "forced Stagger respects immunity");
        yield return Move(); Assert.That(stagger.RemainingImmunityTime, Is.Zero);
        Assert.That(stagger.ApplyStagger(.5f, .5f), Is.EqualTo(2), "forced Stagger uses the same explicit future-move duration");
        yield return Move(); Assert.That(stagger.RemainingStaggerTime, Is.EqualTo(1));
        yield return Move(); Assert.That(stagger.IsStaggered, Is.False);
    }

    [UnityTest] public IEnumerator UnifiedPartialStaggerDecaysOnlyOnceAfterAnUnansweredMove()
    {
        yield return LaunchUnified(); DurableUnifiedFixture();
        var actor = Run.Waves.ActiveEnemies.First(); var stagger = actor.GetComponent<EnemyStagger>();
        stagger.RegisterDamage(60); float initial = stagger.StaggerMeterNormalized;
        yield return new WaitForSeconds(.4f);
        Assert.That(stagger.StaggerMeterNormalized, Is.EqualTo(initial));
        yield return Move(); float after = stagger.StaggerMeterNormalized;
        Assert.That(after, Is.EqualTo(initial - stagger.OffColorDecayDamage / stagger.DamageThreshold).Within(.00001f));
        stagger.ExpireAcceptedMove(Run.MoveClock.Tick);
        Assert.That(stagger.StaggerMeterNormalized, Is.EqualTo(after), "same action cannot drain twice");
        yield return Move(() => { stagger.RegisterDamage(5); stagger.RegisterDamage(5); });
        Assert.That(stagger.StaggerMeterNormalized, Is.EqualTo(after + 10 / stagger.DamageThreshold).Within(.00001f));
    }

    [UnityTest] public IEnumerator UnifiedDecreeAffectsFiveCompleteMovesAndSurvivesContinue()
    {
        yield return LaunchUnified(); DurableUnifiedFixture();
        Run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
        Assert.That(Run.Player.GetComponent<PlayerAbilityController>().TryActivate(), Is.True);
        var decree = Run.Player.GetComponent<RoyalDecreeRuntime>();
        Assert.That(decree.RemainingMoves, Is.EqualTo(5));
        yield return new WaitForSeconds(.3f); Assert.That(decree.RemainingMoves, Is.EqualTo(5));
        for (int n = 4; n >= 1; n--) { yield return Move(); Assert.That(decree.RemainingMoves, Is.EqualTo(n)); }
        var snapshot = Run.Continuation.Capture();
        Assert.That(snapshot.decreeRemaining, Is.EqualTo(1));
        Assert.That(Run.SuspendToMenu(), Is.True); yield return null;
        SceneManager.LoadScene("Game");
        yield return Until(() => Run?.Continuation != null && !Run.Continuation.IsRestoring, "unified Continue");
        Run.GetComponent<RunControlsUI>().Close(); yield return Stable(); DurableUnifiedFixture();
        decree = Run.Player.GetComponent<RoyalDecreeRuntime>();
        Assert.That(CombatMoveClock.Unified, Is.True); Assert.That(decree.RemainingMoves, Is.EqualTo(1));
        yield return Move(); Assert.That(decree.IsActive, Is.False);
    }

    [UnityTest] public IEnumerator UnifiedAllSpecialsPrecedeBasicsByRankThenSlot()
    {
        yield return LaunchUnified();
        var shield = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_ShieldKnight.asset");
        Assert.That(Run.Waves.TrySummonEnemy(shield, out var left), Is.True);
        Assert.That(Run.Waves.TrySummonEnemy(shield, out var right), Is.True);
        yield return Stable(); DurableUnifiedFixture();
        var boss = UnityEngine.Object.Instantiate(shield);
        try
        {
            Set(boss, "category", EnemyCategory.Boss); Set(right, "definition", boss);
            var order = new List<string>();
            foreach (var actor in Run.Waves.ActiveEnemies)
            {
                int slot = Run.Waves.ContinuationSlot(actor);
                actor.SpecialAbilityUsed += _ => order.Add("S" + slot);
                actor.GetComponent<EnemyAutoAttack>().AttackStarted += _ => order.Add("B" + slot);
                Set(actor.GetComponent<EnemyAutoAttack>(), "unifiedRemaining", 1);
                if (actor.HasSpecialAbility) actor.SetSpecialTurnRequirement(1);
            }
            yield return Move();
            CollectionAssert.AreEqual(new[] { "S2", "S1", "B0", "B1", "B2" }, order);
        }
        finally { Set(right, "definition", shield); UnityEngine.Object.Destroy(boss); }
    }

    [UnityTest] public IEnumerator UnifiedSummonDoesNotReceiveItsBirthActionTick()
    {
        yield return LaunchUnified(); DurableUnifiedFixture();
        var data = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_ShieldKnight.asset");
        EnemyActor summoned = null;
        yield return Move(() => Assert.That(Run.Waves.TrySummonEnemy(data, out summoned), Is.True));
        yield return Stable();
        Assert.That(summoned.CurrentSpecialTurnCount, Is.Zero);
        Assert.That(summoned.GetComponent<EnemyAutoAttack>().RemainingAttackTime, Is.EqualTo(data.UnifiedFirstAttackMoves));
        yield return Move();
        Assert.That(summoned.CurrentSpecialTurnCount, Is.EqualTo(1));
        Assert.That(summoned.GetComponent<EnemyAutoAttack>().RemainingAttackTime, Is.EqualTo(data.UnifiedFirstAttackMoves - 1));
    }

    [UnityTest] public IEnumerator UnifiedConchRallyUsesThreeFutureMovesAndPreservesContinue()
    {
        yield return LaunchUnified("drowned-court");
        var data = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(
            AssetDatabase.FindAssets("conch_marshal t:EnemyDefinition").First()));
        Assert.That(Run.Waves.TrySummonEnemy(data, out var conch), Is.True);
        yield return Stable(); DurableUnifiedFixture();
        var kit = conch.GetComponent<AquaticEnemyAbility>();
        Call(kit, "ApplyRally");
        Assert.That(kit.RallyRemaining, Is.EqualTo(3));
        yield return new WaitForSeconds(.4f); Assert.That(kit.RallyRemaining, Is.EqualTo(3));
        yield return Move(); Assert.That(kit.RallyRemaining, Is.EqualTo(2));
        var saved = new EnemyCombatSnapshot(); kit.CaptureContinuation(saved, Run.Waves.ContinuationSlot);
        kit.RestoreContinuation(saved, Run.Waves.ContinuationEnemy);
        Assert.That(kit.RallyRemaining, Is.EqualTo(2));
        yield return Move(); Assert.That(kit.RallyRemaining, Is.EqualTo(1));
        yield return Move(); Assert.That(kit.RallyRemaining, Is.Zero);
    }
}
