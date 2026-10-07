using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator LegacyMenderRecoveryMigratesWithoutHealingOrHoldingBasics()
    {
        yield return Launch();
        var actor=Enemy("elven_mender");var ally=Enemy("orc_trailguard");
        var channel=actor.GetComponent<EnemyChannelRuntime>();
        ally.ResolveDamageWithoutFeedback(30);int hp=ally.CurrentHealth;
        channel.RestoreContinuation(new EnemyCombatSnapshot {channel=new EnemyChannelSnapshot {
            state=2,sequence=8,lastOutcomeSequence=8,recoveryUntil=500,targetId=ally.PersistentId,outcome="Healed"}},_=>ally);
        Assert.That(channel.BlocksBasic,Is.False);Assert.That(channel.Target,Is.Null);
        Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.False);
        Assert.That(actor.CurrentSpecialTurnCount,Is.Zero);Assert.That(ally.CurrentHealth,Is.EqualTo(hp));
        yield return ResumeCourtCheckpoint();
        actor=Enemy("elven_mender");channel=actor.GetComponent<EnemyChannelRuntime>();
        Assert.That(channel.BlocksBasic,Is.False);Assert.That(channel.IsChanneling,Is.False);
        yield return Move();Assert.That(actor.CurrentSpecialTurnCount,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator LegacyMilestoneRecoveryPreservesCycleAndUsesNormalStaggerOnly()
    {
        yield return Launch(8,true);QuietKitFixture();
        var actor=Enemy("briar_matriarch");var kit=actor.GetComponent<ForestMilestoneEnemyAbility>();
        kit.RestoreContinuation(new EnemyCombatSnapshot {forestMilestone=new ForestMilestoneSnapshot {
            version=2,state=4,deadline=999,sequence=5,resolvedSequence=5,nextAbility=2}},_=>null);
        Assert.That(kit.BlocksBasic,Is.False);Assert.That(actor.SpecialTurnRequirement,Is.EqualTo(3));
        var snapshot=new EnemyCombatSnapshot();kit.CaptureContinuation(snapshot,_=>0);
        Assert.That(snapshot.forestMilestone.state,Is.Zero);Assert.That(snapshot.forestMilestone.nextAbility,Is.EqualTo(2));
        kit.RestoreContinuation(new EnemyCombatSnapshot {forestMilestone=new ForestMilestoneSnapshot {
            version=2,state=1,deadline=Run.MoveClock.Tick+3,sequence=6,resolvedSequence=5,activeAbility=2}},_=>null);
        var stagger=actor.GetComponent<EnemyStagger>();stagger.RestoreContinuation(new EnemyCombatSnapshot());stagger.ApplyStagger(1,1);
        Assert.That(stagger.IsStaggered,Is.True);Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.BlocksBasic,Is.False);
        yield return Move();Assert.That(stagger.IsStaggered,Is.False);Assert.That(kit.BlocksBasic,Is.False);
        yield return Move();Assert.That(actor.CurrentSpecialTurnCount,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator LegacyCourtRecoveryPreservesRotationAndDoesNotReplayCast()
    {
        yield return LaunchCourt("queen_nacre","shellback_porter");
        var actor=Enemy("queen_nacre");var kit=actor.GetComponent<AquaticEnemyAbility>();int announcements=0;
        actor.AbilityCastCommitted+=(_,__)=>announcements++;
        kit.RestoreContinuation(new EnemyCombatSnapshot {aquaticEnemy=new AquaticEnemySnapshot {
            stage=2,cycle=2,recoveryUntil=999,action="DEPTHS"}},_=>null);
        Assert.That(kit.BlocksBasic,Is.False);Assert.That(actor.CurrentSpecialTurnCount,Is.Zero);
        var saved=new EnemyCombatSnapshot();kit.CaptureContinuation(saved,_=>0);
        Assert.That(saved.aquaticEnemy.stage,Is.Zero);Assert.That(saved.aquaticEnemy.cycle,Is.EqualTo(2));
        Assert.That(saved.aquaticEnemy.recoveryUntil,Is.Zero);Assert.That(announcements,Is.Zero);
        yield return ResumeCourtCheckpoint();actor=Enemy("queen_nacre");kit=actor.GetComponent<AquaticEnemyAbility>();
        Assert.That(kit.BlocksBasic,Is.False);yield return Move();Assert.That(actor.CurrentSpecialTurnCount,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator CourtTargetLossFizzlesAndStaggerEndsWithoutRecovery()
    {
        yield return LaunchCourt("breakwater_captain","shellback_porter","reef_spearman");
        var actor=Enemy("breakwater_captain");var target=Enemy("shellback_porter");
        var kit=actor.GetComponent<AquaticEnemyAbility>();var stagger=actor.GetComponent<EnemyStagger>();
        kit.RestoreContinuation(new EnemyCombatSnapshot {aquaticEnemy=new AquaticEnemySnapshot {
            stage=1,cycle=1,dueMove=10,targetId=target.PersistentId,action="BOARDING"}},_=>target);
        target.ResolveDirectDamage(9999);yield return Stable();
        Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.BlocksBasic,Is.False);Assert.That(stagger.IsStaggered,Is.False);
        Assert.That(actor.CurrentSpecialTurnCount,Is.Zero);
        kit.RestoreContinuation(new EnemyCombatSnapshot {aquaticEnemy=new AquaticEnemySnapshot {
            stage=1,cycle=1,dueMove=10,targetId=Enemy("reef_spearman").PersistentId,action="BOARDING"}},_=>null);
        stagger.ApplyStagger(1,1);Assert.That(stagger.IsStaggered,Is.True);
        Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.BlocksBasic,Is.False);
        float end=Time.time+1.2f;yield return Until(()=>Time.time>=end,"ordinary seconds Stagger finishes");
        Assert.That(stagger.IsStaggered,Is.False);Assert.That(kit.BlocksBasic,Is.False);
        yield return Move();Assert.That(actor.CurrentSpecialTurnCount,Is.EqualTo(1));
    }
}
