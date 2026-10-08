using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator LaunchDelver(bool remount=false, params string[] escorts)
    {
        var def=Resources.Load<ZoneDefinition>("Zones/ironvein-excavation").enemies.Single(d=>d.EnemyId=="grand_delver");
        bool old=def.mineEnableRemount;def.mineEnableRemount=remount;
        try { yield return LaunchMineKit(new[]{"grand_delver"}.Concat(escorts).ToArray()); }
        finally { def.mineEnableRemount=old; }
    }
    private IEnumerator DelverCore()
    {
        var actor=Enemy("grand_delver");yield return ReadyMine(actor);yield return ReadyMine(actor);yield return ReadyMine(actor);
        Assert.That(actor.GetComponent<MineEnemyAbility>().CastName,Is.EqualTo("CORE"));
    }

    [UnityTest] public IEnumerator IronveinDelverCycleCoreContinueAndSingleHeavyImpact()
    { yield return MineBossCoreImpact(); }
    private IEnumerator MineBossCoreImpact()
    {
        yield return LaunchDelver();var actor=Enemy("grand_delver");var board=Run.Board;
        yield return ReadyMine(actor);Assert.That(board.MineStoneCount,Is.EqualTo(2));
        Assert.That(board.MineStoneTargets().All(t=>t.State.stage==MineStoneStage.Brittle),Is.True);
        yield return ReadyMine(actor);Assert.That(actor.GetComponent<EnemyOrePower>().IsPowered,Is.True);
        Assert.That(board.Mine.drills.All(d=>d.charge==1),Is.True);
        yield return ReadyMine(actor);var kit=actor.GetComponent<MineEnemyAbility>();long core=kit.StoneId;
        Assert.That(board.FindMineStone(core)?.State.isCore,Is.True);Assert.That(board.FindMineStone(core)?.Durability,Is.EqualTo(3));
        Assert.That(kit.ResponseMoves,Is.EqualTo(3));int hp=Run.Player.CurrentHealth;
        yield return ResumeRoster();actor=Enemy("grand_delver");kit=actor.GetComponent<MineEnemyAbility>();
        Assert.That(kit.StoneId,Is.EqualTo(core));
        yield return EnvironmentMove();yield return EnvironmentMove();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        yield return EnvironmentMove();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp-45));
        Assert.That(Run.Board.FindMineStone(core),Is.Null);Assert.That(kit.IsPreparing,Is.False);
        Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.False);
        yield return EnvironmentMove();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp-45));
    }

    [UnityTest] public IEnumerator IronveinDelverCoreTakesThreeOrdinaryHitsAndStaggersOnBreak()
    {yield return MineBossCoreHits();}
    private IEnumerator MineBossCoreHits()
    {
        yield return LaunchDelver();yield return DelverCore();var actor=Enemy("grand_delver");var kit=actor.GetComponent<MineEnemyAbility>();
        long id=kit.StoneId;int hp=Run.Player.CurrentHealth;Set(actor.GetComponent<EnemyStagger>(),"remainingImmunityTime",0f);
        for(int hit=1;hit<=3;hit++)
        {
            var target=Run.Board.FindMineStone(id).Value;
            Call(Run.Board,"DamageBarricadesFromSource",Beside(target),null,true,false);
            Assert.That(Run.Board.FindMineStone(id)?.Durability??0,Is.EqualTo(3-hit));
            if(hit<3)Assert.That(kit.IsPreparing,Is.True);
        }
        yield return null;yield return null;
        Assert.That(kit.IsPreparing,Is.False);Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.True);
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
    }

    [UnityTest] public IEnumerator IronveinDelverFinalResponseDrillBreaksCoreBeforeImpact()
    {yield return MineBossCoreDrill();}
    private IEnumerator MineBossCoreDrill()
    {
        yield return LaunchDelver();yield return DelverCore();var actor=Enemy("grand_delver");var kit=actor.GetComponent<MineEnemyAbility>();
        long id=kit.StoneId;Run.Board.Mine.drills[0].lane=Run.Board.FindMineStone(id).Value.Cell.y;
        yield return EnvironmentMove();yield return EnvironmentMove();Assert.That(kit.ResponseMoves,Is.EqualTo(1));
        int hp=Run.Player.CurrentHealth;Set(actor.GetComponent<EnemyStagger>(),"remainingImmunityTime",0f);
        Run.MoveClock.Opportunity+=e=>{if(e==actor)Run.Board.TryQueueMineDrillPower(1,4);};
        yield return EnvironmentMove();Assert.That(Run.Board.FindMineStone(id),Is.Null);
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));Assert.That(kit.IsPreparing,Is.False);
        Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.True);
    }

    [UnityTest] public IEnumerator IronveinDelverDefaultDefeatIsFinalWithoutExperimentalRemount()
    {yield return MineBossDefaultDeath();}
    private IEnumerator MineBossDefaultDeath()
    {
        yield return LaunchDelver();var actor=Enemy("grand_delver");int defeats=0;actor.Defeated+=_=>defeats++;
        actor.TryTakeDamage(20000);Assert.That(actor.IsDefeated,Is.True);Assert.That(defeats,Is.EqualTo(1));
        Assert.That(actor.TryTakeDamage(20000),Is.False);Assert.That(defeats,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator IronveinDelverSuitBreakCancelsPendingCoreAndDoesNotConsumeItsPower()
    {yield return MineBossEjectCore();}
    private IEnumerator MineBossEjectCore()
    {
        yield return LaunchDelver(true);yield return DelverCore();var actor=Enemy("grand_delver");var kit=actor.GetComponent<MineEnemyAbility>();
        long core=kit.StoneId;actor.TryTakeDamage(20000);yield return Stable();yield return null;
        Assert.That(kit.IsPilot,Is.True);Assert.That(kit.IsPreparing,Is.False);
        Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.False);
        Assert.That(actor.GetComponent<EnemyOrePower>().IsPowered,Is.False);
        Assert.That(Run.Board.FindMineStone(core)?.State.isCore,Is.False,"cancelled ritual becomes ordinary Obsidian");
        int hp=Run.Player.CurrentHealth;yield return ResumeRoster();yield return EnvironmentMove();
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));Assert.That(Enemy("grand_delver").GetComponent<MineEnemyAbility>().IsPreparing,Is.False);
    }

    [UnityTest] public IEnumerator IronveinDelverPilotCanDieEarlyWithoutRewardsOnSuitBreak()
    {yield return MineBossPilotDeath();}
    private IEnumerator MineBossPilotDeath()
    {
        yield return LaunchDelver(true,"rivet_turret");var actor=Enemy("grand_delver");long id=actor.PersistentId;
        int defeats=0,complete=0;actor.Defeated+=_=>defeats++;Run.Waves.WaveCompleted+=_=>complete++;
        actor.TryTakeDamage(20000);Assert.That(actor.IsDefeated,Is.False);Assert.That(defeats,Is.Zero);
        Assert.That(actor.GetComponent<MineEnemyAbility>().IsPilot,Is.True);Assert.That(actor.CurrentHealth,Is.EqualTo(70));
        Assert.That(actor.TryTakeDamage(20000),Is.False,"same ejection frame cannot double-kill");
        yield return Stable();yield return null;Assert.That(actor.CanReceiveDamage,Is.True);
        Assert.That(actor.PersistentId,Is.EqualTo(id));actor.TryTakeDamage(20000);yield return Stable();
        Assert.That(defeats,Is.EqualTo(1));Assert.That(complete,Is.Zero);Assert.That(Run.Waves.IsWaveActive,Is.True);
        Enemy("rivet_turret").TryTakeDamage(20000);yield return Until(()=>!Run.Waves.IsWaveActive,"last escort releases formation");
        Assert.That(complete,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator IronveinDelverPilotContinueAndOneWeakerReserveSuit()
    {yield return MineBossRemount();}
    private IEnumerator MineBossRemount()
    {
        yield return LaunchDelver(true);var actor=Enemy("grand_delver");long id=actor.PersistentId;actor.TryTakeDamage(20000);
        yield return Stable();yield return null;actor.TryTakeDamage(5);
        Assert.That(actor.GetComponent<MineEnemyAbility>().PilotMoves,Is.EqualTo(3));
        yield return ResumeRoster();actor=Enemy("grand_delver");var kit=actor.GetComponent<MineEnemyAbility>();
        Assert.That(actor.PersistentId,Is.EqualTo(id));Assert.That(actor.CurrentHealth,Is.EqualTo(65));Assert.That(actor.MaxHealth,Is.EqualTo(70));
        yield return EnvironmentMove();yield return EnvironmentMove();Assert.That(kit.IsPilot,Is.True);Assert.That(kit.PilotMoves,Is.EqualTo(1));
        yield return EnvironmentMove();Assert.That(kit.BossPhase,Is.EqualTo(MineBossPhase.SecondMech));Assert.That(actor.MaxHealth,Is.EqualTo(175));
        yield return ResumeRoster();actor=Enemy("grand_delver");kit=actor.GetComponent<MineEnemyAbility>();
        Assert.That(kit.BossPhase,Is.EqualTo(MineBossPhase.SecondMech));Assert.That(actor.MaxHealth,Is.EqualTo(175));
        int final=0;actor.Defeated+=_=>final++;actor.TryTakeDamage(20000);
        Assert.That(actor.IsDefeated,Is.True);Assert.That(final,Is.EqualTo(1));
    }
}
