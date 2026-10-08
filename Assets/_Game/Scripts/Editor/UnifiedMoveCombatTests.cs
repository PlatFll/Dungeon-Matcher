using System;
using System.Collections;
using System.Linq;
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
}
