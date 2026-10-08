using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator LaunchMineKit(params string[] ids)
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/ironvein-excavation");var old=zone.liveEncounters;
        zone.liveEncounters=new[]{new ZoneTestEncounter{label="Specialist regression",members=ids.Select(id=>zone.enemies.Single(d=>d.EnemyId==id)).ToArray()}};
        try { yield return LaunchMine(); }
        finally { zone.liveEncounters=old; }
        Set(Run.Waves,"advanceWavesAutomatically",false);
        yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"mine spawn settles");
    }
    private IEnumerator ReadyMine(EnemyActor actor)
    {
        Set(actor,"currentSpecialTurnCount",actor.SpecialTurnRequirement-1);
        yield return EnvironmentMove();
    }
    private HashSet<Gem> Beside(MineStoneTarget stone)
    {
        var result=new HashSet<Gem>();
        foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right})
        {var gem=Run.Board.GetGem(stone.Cell.x+d.x,stone.Cell.y+d.y);if(gem!=null)result.Add(gem);}
        return result;
    }

    [UnityTest] public IEnumerator IronveinQueuedSmallDrillCancelsIfCasterStaggersBeforeExecution()
    {yield return MineQueuedCancellation();}
    private IEnumerator MineQueuedCancellation()
    {
        yield return LaunchMineKit("bore_engineer","pickaxe_delver");var owner=Enemy("bore_engineer");
        var before=Run.Continuation.Capture().board.cells.Where(c=>c.hasGem).Select(c=>c.identity).ToArray();
        bool? edge=null;Run.Board.TryQueueSmallMineDrill(owner,true,0,value=>edge=value);
        var stagger=owner.GetComponent<EnemyStagger>();Set(stagger,"remainingImmunityTime",0f);stagger.ApplyStagger(2,2);
        yield return Stable();Assert.That(edge,Is.False);
        Assert.That(Run.Board.Mine.drills.All(d=>d.charge==0),Is.True);
        CollectionAssert.AreEquivalent(before,Run.Continuation.Capture().board.cells.Where(c=>c.hasGem).Select(c=>c.identity));
    }

    [UnityTest] public IEnumerator IronveinAssayFizzlesIfAnotherSourceAlreadyAdvancedItsStone()
    {yield return MineAssayRace();}
    private IEnumerator MineAssayRace()
    {
        yield return LaunchMineKit("vein_surveyor","pickaxe_delver");var surveyor=Enemy("vein_surveyor");var other=Enemy("pickaxe_delver");
        Run.Board.TryQueuePlaceMineStones(other,2,6);yield return Stable();
        yield return ReadyMine(surveyor);var kit=surveyor.GetComponent<MineEnemyAbility>();Assert.That(kit.IsPreparing,Is.True);
        long target=kit.StoneId;Run.Board.TryQueueMineStoneOperation(other,target,MineStoneOperation.Harden,_=>{});yield return Stable();
        yield return EnvironmentMove();Assert.That(kit.IsPreparing,Is.False);
        Assert.That(Run.Board.FindMineStone(target)?.State.stage,Is.EqualTo(MineStoneStage.Hardened));
        Assert.That(surveyor.GetComponent<EnemyStagger>().IsStaggered,Is.False);
    }

    [UnityTest] public IEnumerator IronveinSmallDrillBreaksFirstStoneButNeverTouchesSecond()
    {yield return MineSmallStop();}
    private IEnumerator MineSmallStop()
    {
        yield return LaunchMine();var board=Run.Board;int row=3;var owner=Run.Waves.ActiveEnemies[0];
        for(int y=0;y<board.Height;y++)for(int x=0;x<board.Width;x++)
            board.GetGem(x,y).SetSpecialType(y==row&&(x==1||x==5)?GemSpecialType.None:GemSpecialType.ColorCrystal);
        Assert.That(board.TryQueuePlaceMineStones(owner,2,6,MineStoneStage.Obsidian),Is.True);yield return Stable();
        var stones=board.MineStoneTargets().OrderBy(t=>t.Cell.x).ToArray();Assert.That(stones.Length,Is.EqualTo(2));
        Call(board,"DamageMineStoneExact",stones[0].Cell,2);
        bool? edge=null;int reports=0;board.BoardClearResolved+=_=>reports++;
        int preserved=board.GetGem(0,row).BoardIdentity;
        Assert.That(board.TryQueueSmallMineDrill(owner,true,row,value=>edge=value),Is.True);yield return Stable();
        Assert.That(edge,Is.False);Assert.That(board.FindMineStone(stones[0].State.id),Is.Null);
        Assert.That(board.FindMineStone(stones[1].State.id)?.Durability,Is.EqualTo(3));
        Assert.That(board.GetGem(0,row).BoardIdentity,Is.EqualTo(preserved),"portable drill skips specials");
        Assert.That(board.Mine.drills[0].charge,Is.EqualTo(1));Assert.That(reports,Is.Zero);
    }

    [UnityTest] public IEnumerator IronveinBoreChoosesEmptyLaneResumesThenHitsOnceAndReleasesBasics()
    {yield return MineBoreDeadline();}
    private IEnumerator MineBoreDeadline()
    {
        yield return LaunchMineKit("bore_engineer","pickaxe_delver");var actor=Enemy("bore_engineer");
        yield return ReadyMine(actor);var kit=actor.GetComponent<MineEnemyAbility>();
        Assert.That(kit.IsPreparing,Is.True);Assert.That(kit.Horizontal,Is.True);Assert.That(kit.Lane,Is.Zero);
        Assert.That(kit.ResponseMoves,Is.EqualTo(2));Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.True);
        yield return ResumeRoster();actor=Enemy("bore_engineer");kit=actor.GetComponent<MineEnemyAbility>();
        Assert.That(kit.ResponseMoves,Is.EqualTo(2));int hp=Run.Player.CurrentHealth;
        yield return EnvironmentMove();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        yield return EnvironmentMove();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp-20));
        Assert.That(kit.IsPreparing,Is.False);Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.False);
        Assert.That(Run.Board.Mine.drills[0].charge,Is.EqualTo(1));
        Assert.That(Run.Continuation.Capture().enemies.Single(e=>e.persistentId==actor.PersistentId).mineEnemy.stage,Is.Zero);
    }

    [UnityTest] public IEnumerator IronveinSapperDefusesWithoutHostDamageAndExplosionAlwaysHitsTwice()
    {yield return MineBombDurability();}
    private IEnumerator MineBombDurability()
    {
        yield return LaunchMineKit("powder_sapper","pickaxe_delver");var board=Run.Board;var actor=Enemy("powder_sapper");
        Assert.That(board.TryQueuePlaceMineStones(actor,1,6,MineStoneStage.Obsidian),Is.True);yield return Stable();
        var stone=board.MineStoneTargets().Single();bool armed=false;
        board.TryQueueMineStoneOperation(actor,stone.State.id,MineStoneOperation.ArmCharge,ok=>armed=ok);yield return Stable();Assert.That(armed,Is.True);
        Call(board,"DamageBarricadesFromSource",Beside(stone),null,true,false);
        Assert.That(board.FindMineStone(stone.State.id)?.Durability,Is.EqualTo(3));
        Assert.That(board.FindMineStone(stone.State.id)?.State.bombOwnerId,Is.Zero);
        board.TryQueueMineStoneOperation(actor,stone.State.id,MineStoneOperation.ArmCharge,_=>{});yield return Stable();
        Call(board,"DamageBarricadesFromSource",Beside(stone),null,false,true);
        Assert.That(board.FindMineStone(stone.State.id)?.Durability,Is.EqualTo(3),"special also safely defuses");
        for(int stage=0;stage<3;stage++)
        {
            // Use the real placement path for each material, avoiding a fabricated tier.
            Call(board,"DamageMineStoneExact",stone.Cell,3);yield return Stable();
            board.TryQueueMineDrillPower(0,4);yield return Stable();
            Assert.That(board.TryQueuePlaceMineStones(actor,1,6,(MineStoneStage)stage),Is.True);yield return Stable();
            stone=board.MineStoneTargets().Single();board.TryQueueMineStoneOperation(actor,stone.State.id,MineStoneOperation.ArmCharge,_=>{});yield return Stable();
            bool expired=false;board.TryQueueMineStoneOperation(actor,stone.State.id,MineStoneOperation.DetonateCharge,ok=>expired=ok);yield return Stable();
            Assert.That(expired,Is.True);
            Assert.That(board.FindMineStone(stone.State.id)?.Durability??0,Is.EqualTo(stage==2?1:0));
            if(stage<2) {Assert.That(board.TryQueuePlaceMineStones(actor,1,6,MineStoneStage.Obsidian),Is.True);yield return Stable();stone=board.MineStoneTargets().Single();}
        }
    }

    [UnityTest] public IEnumerator IronveinSapperFuseSurvivesContinueAndDealsOnePlayerPacket()
    {yield return MineBombFuse();}
    private IEnumerator MineBombFuse()
    {
        yield return LaunchMineKit("powder_sapper","pickaxe_delver");var actor=Enemy("powder_sapper");
        Run.Board.TryQueuePlaceMineStones(actor,1,6,MineStoneStage.Obsidian);yield return Stable();
        yield return ReadyMine(actor);Assert.That(actor.GetComponent<MineEnemyAbility>().ResponseMoves,Is.EqualTo(2));
        yield return ResumeRoster();actor=Enemy("powder_sapper");int hp=Run.Player.CurrentHealth;
        yield return EnvironmentMove();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        yield return EnvironmentMove();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp-CombatAmounts.Round(12)));
        Assert.That(actor.GetComponent<MineEnemyAbility>().IsPreparing,Is.False);
    }

    [UnityTest] public IEnumerator IronveinHaulerStonewrightAssayAndSwitchUseOwnedBoardQueue()
    {yield return MineSupportAndGeology();}
    private IEnumerator MineSupportAndGeology()
    {
        yield return LaunchMineKit("ore_hauler","stonewright","vein_surveyor");var board=Run.Board;
        yield return ReadyMine(Enemy("ore_hauler"));Assert.That(Enemy("stonewright").GetComponent<EnemyOrePower>().IsPowered,Is.True);
        Assert.That(board.Mine.drills[0].charge,Is.EqualTo(1));
        yield return ReadyMine(Enemy("stonewright"));Assert.That(board.MineStoneCount,Is.EqualTo(2));
        Assert.That(Enemy("stonewright").GetComponent<MineEnemyAbility>().IsPreparing,Is.False);
        yield return ReadyMine(Enemy("vein_surveyor"));var kit=Enemy("vein_surveyor").GetComponent<MineEnemyAbility>();long id=kit.StoneId;
        Assert.That(kit.ResponseMoves,Is.EqualTo(1));yield return EnvironmentMove();
        Assert.That(board.FindMineStone(id)?.State.stage,Is.EqualTo(MineStoneStage.Hardened));
        Enemy("ore_hauler").TryTakeDamage(20000);yield return Stable();
        var def=Run.Zone.Definition.enemies.Single(d=>d.EnemyId=="rail_switcher");Assert.That(Run.Waves.TrySummonEnemy(def,out var switcher),Is.True);
        yield return Stable();QuietKitFixture();int lane=board.Mine.drills[0].lane;
        board.TryQueueMineDrillPower(1,1);yield return Stable();int charge=board.Mine.drills[0].charge;
        yield return ReadyMine(switcher);Assert.That(board.Mine.drills[0].lane,Is.EqualTo(lane));
        yield return EnvironmentMove();Assert.That(board.Mine.drills[0].lane,Is.EqualTo(lane+1));
        Assert.That(board.Mine.drills[0].charge,Is.EqualTo(charge));
    }

    [UnityTest] public IEnumerator IronveinRattledHalvesNewBuildupSurvivesDrillsAndExpiresAfterTwoMoves()
    {yield return MineRattled();}
    private IEnumerator MineRattled()
    {
        yield return LaunchMineKit("seismic_smith","pickaxe_delver");var target=Enemy("pickaxe_delver");var stagger=target.GetComponent<EnemyStagger>();
        Set(stagger,"remainingImmunityTime",0f);float normal=stagger.RegisterDamage(5);Set(stagger,"staggerMeterNormalized",0f);
        yield return ReadyMine(Enemy("seismic_smith"));
        Assert.That(Run.Player.Statuses.Remaining(PlayerStatusKind.Rattled),Is.EqualTo(2));
        Set(stagger,"remainingImmunityTime",0f);Set(stagger,"staggerMeterNormalized",0f);
        Assert.That(stagger.RegisterDamage(5),Is.EqualTo(normal*.5f).Within(.001f));
        Run.Board.TryQueuePlaceMineStones(Enemy("seismic_smith"),1,6,MineStoneStage.Obsidian);yield return Stable();
        var stone=Run.Board.MineStoneTargets().Single();Call(Run.Board,"DamageMineStoneExact",stone.Cell,1);
        Run.Board.TryQueueMineDrillPower(0,4);yield return Stable();
        Assert.That(Run.Player.Statuses.Remaining(PlayerStatusKind.Rattled),Is.EqualTo(2));
        yield return ResumeRoster();yield return EnvironmentMove();Assert.That(Run.Player.Statuses.Remaining(PlayerStatusKind.Rattled),Is.EqualTo(1));
        yield return EnvironmentMove();Assert.That(Run.Player.Statuses.Has(PlayerStatusKind.Rattled),Is.False);
    }

    [UnityTest] public IEnumerator IronveinChannelStaggerAndDefeatCancelWithoutDamageOrStaleBoardWork()
    {yield return MineInterruptions();}
    private IEnumerator MineInterruptions()
    {
        yield return LaunchMineKit("bore_engineer","powder_sapper","pickaxe_delver");var bore=Enemy("bore_engineer");
        yield return ReadyMine(bore);int hp=Run.Player.CurrentHealth;
        var stagger=bore.GetComponent<EnemyStagger>();Set(stagger,"remainingImmunityTime",0f);stagger.ApplyStagger(2,2);
        Assert.That(bore.GetComponent<MineEnemyAbility>().IsPreparing,Is.False);
        var sapper=Enemy("powder_sapper");Run.Board.TryQueuePlaceMineStones(sapper,1,6,MineStoneStage.Obsidian);yield return Stable();
        yield return ReadyMine(sapper);Assert.That(sapper.GetComponent<MineEnemyAbility>().IsPreparing,Is.True);
        sapper.TryTakeDamage(20000);yield return Stable();
        Assert.That(Run.Board.MineStoneTargets().All(t=>t.State.bombOwnerId==0),Is.True);
        yield return EnvironmentMove();yield return EnvironmentMove();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
    }
}
