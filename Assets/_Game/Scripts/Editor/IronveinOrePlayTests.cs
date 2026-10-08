using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator IronveinOreRefreshesWithoutStackingAndSurvivesContinue()
    { yield return MineOreResume(); }
    private IEnumerator MineOreResume()
    {
        yield return LaunchMine(); var pick = Enemy("pickaxe_delver");
        var token = pick.GetComponent<EnemyOrePower>();
        Assert.That(token.Grant(), Is.True); Assert.That(token.Grant(), Is.True);
        Assert.That(Enemy("packbeetle").GetComponent<EnemyOrePower>().Grant(), Is.False);
        yield return ResumeRoster(); pick = Enemy("pickaxe_delver"); token = pick.GetComponent<EnemyOrePower>();
        Assert.That(token.IsPowered, Is.True);
        var saved = Run.Continuation.Capture().enemies.Single(e => e.persistentId == pick.PersistentId);
        Assert.That(saved.oreNextMultiplier, Is.EqualTo(1.3f));
        var attack = pick.GetComponent<EnemyAutoAttack>();
        int presentations=0;
        attack.AttackStarted += _ => { presentations++; Assert.That(token.IsCurrentSequencePowered,Is.EqualTo(presentations==1)); };
        int hp = Run.Player.CurrentHealth;
        Assert.That(attack.PerformAttackImmediately(), Is.True);
        yield return Until(() => !attack.IsAttackSequenceInProgress, "powered basic finishes");
        Assert.That(token.IsPowered, Is.False);
        Assert.That(token.IsCurrentSequencePowered, Is.False);
        Assert.That(hp - Run.Player.CurrentHealth, Is.EqualTo(CombatAmounts.Round(Mathf.RoundToInt(pick.Damage * 1.3f))));
        hp = Run.Player.CurrentHealth;
        Assert.That(attack.PerformAttackImmediately(), Is.True);
        yield return Until(() => !attack.IsAttackSequenceInProgress, "ordinary basic finishes");
        Assert.That(hp - Run.Player.CurrentHealth, Is.EqualTo(CombatAmounts.Round(pick.Damage)));
    }

    [UnityTest] public IEnumerator IronveinPackbeetleDeathPowersEveryDrillAndLivingEligibleAllyOnce()
    { yield return MineDeathNetwork(); }
    private IEnumerator MineDeathNetwork()
    {
        yield return LaunchMine(); var board = Run.Board;
        Assert.That(board.TryQueuePlaceMineStones(Enemy("pickaxe_delver"), 1, 6, MineStoneStage.Obsidian), Is.True);
        yield return Stable(); var stone = MineCells().Single(); board.Mine.drills[0].lane = stone.y;
        board.TryQueueMineDrillPower(1, 3); yield return Stable();
        var beetle = Enemy("packbeetle"); var token = beetle.GetComponent<EnemyOrePower>();
        int shots = 0; board.MineDrillFired += (_, __, ___) => shots++;
        Assert.That(beetle.TryTakeDamage(20000), Is.True);
        Assert.That(token.BurstConsumed, Is.True);
        Assert.That(beetle.TryTakeDamage(20000), Is.False);
        Assert.That(Enemy("pickaxe_delver").GetComponent<EnemyOrePower>().IsPowered, Is.True);
        Assert.That(Enemy("rivet_gunner").GetComponent<EnemyOrePower>().IsPowered, Is.True);
        yield return Stable();
        Assert.That(shots, Is.EqualTo(1)); Assert.That(board.MineStoneCount, Is.Zero);
        Assert.That(board.Mine.drills.Single(d => d.id == 2).charge, Is.EqualTo(1));
        yield return ResumeRoster();
        Assert.That(Run.Waves.ActiveEnemies.Count, Is.EqualTo(2));
        Assert.That(Run.Board.Mine.drills.Single(d => d.id == 2).charge, Is.EqualTo(1), "Continue does not replay a dead beetle");
        foreach (var ally in Run.Waves.ActiveEnemies) Assert.That(ally.GetComponent<EnemyOrePower>().IsPowered, Is.True);
    }

    [UnityTest] public IEnumerator IronveinOreBoostAppliesToBothHitsOfOneSequenceOnly()
    { yield return MineOreMultiHit(); }
    private IEnumerator MineOreMultiHit()
    {
        yield return LaunchMine(); var actor = Enemy("rivet_gunner");
        // Fixture changes runtime stats only; the production Gunner remains single-shot.
        typeof(EnemyActor).GetProperty("RuntimeStats", Flags).SetValue(actor,
            new EnemyRuntimeStats(1, 1, 9995, 20, 20, 999, 3));
        Set(Run.Player, "maximumHealth", 1000); Set(Run.Player, "currentHealth", 1000);
        var token = actor.GetComponent<EnemyOrePower>(); var attack = actor.GetComponent<EnemyAutoAttack>();
        token.Grant(); token.Grant(); int hits = 0;
        int presentations=0;
        attack.AttackStarted += _ => { presentations++; Assert.That(token.IsCurrentSequencePowered,Is.True); };
        attack.AttackResolved += (_, amount, __) => { hits++; Assert.That(CombatAmounts.Round(amount), Is.EqualTo(25)); };
        Assert.That(attack.PerformAttackImmediately(), Is.True);
        yield return Until(() => !attack.IsAttackSequenceInProgress, "both ore hits finish");
        Assert.That(hits, Is.EqualTo(2)); Assert.That(Run.Player.CurrentHealth, Is.EqualTo(950));
        Assert.That(presentations,Is.EqualTo(2)); Assert.That(token.IsCurrentSequencePowered,Is.False);
        Assert.That(token.IsPowered, Is.False);
    }
}
