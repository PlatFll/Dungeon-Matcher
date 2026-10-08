using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator IronveinMachinistsNeverBorrowOwnershipOrReplaceOccupiedSlots()
    {yield return MineSeparateBuilders();}
    private IEnumerator MineSeparateBuilders()
    {
        yield return LaunchMineKit("siege_machinist","siege_machinist","pickaxe_delver");
        var builders=Run.Waves.ActiveEnemies.Where(e=>e.Definition.EnemyId=="siege_machinist").ToArray();
        yield return ReadyMine(builders[0]);Assert.That(builders[0].IsSpecialReady,Is.True);
        Assert.That(builders[0].GetComponent<MineEnemyAbility>().OwnedTurrets,Is.Empty);
        Enemy("pickaxe_delver").TryTakeDamage(20000);yield return Stable();yield return EnvironmentMove();QuietKitFixture();
        Assert.That(builders[0].GetComponent<MineEnemyAbility>().OwnedTurrets.Count(),Is.EqualTo(1));
        yield return ReadyMine(builders[1]);Assert.That(builders[1].IsSpecialReady,Is.True);
        Assert.That(builders[1].GetComponent<MineEnemyAbility>().OwnedTurrets,Is.Empty);
        Assert.That(Run.Waves.ActiveEnemies.Count,Is.EqualTo(3));
    }

    [UnityTest] public IEnumerator IronveinExtractionReturnsEachActualTierWithoutPlayerRewards()
    {yield return MineExtractionTiers();}
    private IEnumerator MineExtractionTiers()
    {
        yield return LaunchMine();var board=Run.Board;var actor=Run.Waves.ActiveEnemies[0];
        int reports=0;board.BoardClearResolved+=_=>reports++;
        for(int stage=0;stage<3;stage++)
        {
            board.TryQueuePlaceMineStones(actor,1,6,(MineStoneStage)stage);yield return Stable();
            long id=board.MineStoneTargets().Single().State.id;MineStoneStage? extracted=null;
            Assert.That(board.TryQueueExtractMineStone(actor,id,(ok,value)=>{Assert.That(ok,Is.True);extracted=value;}),Is.True);
            yield return Stable();Assert.That(extracted,Is.EqualTo((MineStoneStage)stage));Assert.That(board.FindMineStone(id),Is.Null);
        }
        Assert.That(reports,Is.Zero);Assert.That(actor.GetComponent<EnemyOrePower>().IsPowered,Is.False,"board extraction never grants an out-of-band actor buff");
    }
    [UnityTest] public IEnumerator IronveinMachinistCapsOwnedTurretsAndTheySurviveBuilderContinue()
    {yield return MineTurretLifetime();}
    private IEnumerator MineTurretLifetime()
    {
        yield return LaunchMineKit("siege_machinist");var builder=Enemy("siege_machinist");
        yield return ReadyMine(builder);QuietKitFixture();var kit=builder.GetComponent<MineEnemyAbility>();
        Assert.That(kit.OwnedTurrets.Count(),Is.EqualTo(1));
        yield return ReadyMine(builder);QuietKitFixture();
        Assert.That(kit.OwnedTurrets.Single().GetComponent<EnemyOrePower>().IsPowered,Is.True);
        yield return ReadyMine(builder);QuietKitFixture();Assert.That(kit.OwnedTurrets.Count(),Is.EqualTo(2));
        yield return ReadyMine(builder);QuietKitFixture();
        Assert.That(Run.Waves.ActiveEnemies.Count,Is.EqualTo(3));
        Assert.That(kit.OwnedTurrets.All(t=>t.GetComponent<EnemyOrePower>().IsPowered),Is.True);
        long[] ids=kit.OwnedTurrets.Select(t=>t.PersistentId).ToArray();
        yield return ResumeRoster();builder=Enemy("siege_machinist");kit=builder.GetComponent<MineEnemyAbility>();
        CollectionAssert.AreEquivalent(ids,kit.OwnedTurrets.Select(t=>t.PersistentId));
        builder.TryTakeDamage(20000);yield return Stable();
        Assert.That(Run.Waves.IsWaveActive,Is.True);Assert.That(Run.Waves.ActiveEnemies.Count,Is.EqualTo(2));
        Assert.That(Run.Waves.ActiveEnemies.All(t=>t.Definition.EnemyId=="rivet_turret"&&t.GetComponent<EnemyOrePower>().IsPowered),Is.True);
        yield return ResumeRoster();CollectionAssert.AreEquivalent(ids,Run.Waves.ActiveEnemies.Select(t=>t.PersistentId));
        int completed=0;Run.Waves.WaveCompleted+=_=>completed++;
        var turrets=Run.Waves.ActiveEnemies.ToArray();turrets[0].TryTakeDamage(20000);yield return Stable();Assert.That(completed,Is.Zero);
        turrets[1].TryTakeDamage(20000);yield return Until(()=>!Run.Waves.IsWaveActive,"final independent turret completes formation");
        Assert.That(completed,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator IronveinSentinelExtractsCurrentMaterialAndKeepsPowerThroughContinue()
    {yield return MineSentinelExtraction();}
    private IEnumerator MineSentinelExtraction()
    {
        yield return LaunchMineKit("obsidian_sentinel","pickaxe_delver");var actor=Enemy("obsidian_sentinel");var board=Run.Board;
        board.TryQueuePlaceMineStones(actor,1,6);yield return Stable();yield return ReadyMine(actor);
        var kit=actor.GetComponent<MineEnemyAbility>();long id=kit.StoneId;Assert.That(kit.CastName,Is.EqualTo("DEVOUR"));
        var other=Enemy("pickaxe_delver");board.TryQueueMineStoneOperation(other,id,MineStoneOperation.Harden,_=>{});yield return Stable();
        board.TryQueueMineStoneOperation(other,id,MineStoneOperation.Harden,_=>{});yield return Stable();
        yield return ResumeRoster();actor=Enemy("obsidian_sentinel");board=Run.Board;kit=actor.GetComponent<MineEnemyAbility>();
        yield return EnvironmentMove();yield return EnvironmentMove();
        Assert.That(board.FindMineStone(id),Is.Null);Assert.That(kit.HasObsidianSlam,Is.True);
        Assert.That(actor.FortifiedStacks,Is.EqualTo(1));
        Assert.That(Run.Continuation.Capture().enemies.Single(e=>e.persistentId==actor.PersistentId).oreNextMultiplier,Is.EqualTo(2f));
        yield return ResumeRoster();actor=Enemy("obsidian_sentinel");kit=actor.GetComponent<MineEnemyAbility>();
        Assert.That(kit.HasObsidianSlam,Is.True);Assert.That(actor.FortifiedStacks,Is.EqualTo(1));
        yield return ReadyMine(actor);int hp=Run.Player.CurrentHealth;Assert.That(kit.CastName,Is.EqualTo("SLAM"));
        yield return EnvironmentMove();yield return EnvironmentMove();
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp-45));Assert.That(kit.HasObsidianSlam,Is.False);
    }

    [UnityTest] public IEnumerator IronveinSentinelOnlyActualDrillFiringInterruptsEvenOnLastResponseMove()
    {yield return MineSlamCounter();}
    private IEnumerator MineSlamCounter()
    {
        yield return LaunchMineKit("obsidian_sentinel","pickaxe_delver");var actor=Enemy("obsidian_sentinel");
        yield return ReadyMine(actor);var kit=actor.GetComponent<MineEnemyAbility>();Assert.That(kit.CastName,Is.EqualTo("SLAM"));
        Run.Board.TryQueueMineDrillPower(1,1);yield return Stable();Assert.That(kit.IsPreparing,Is.True,"charge itself never interrupts");
        yield return ResumeRoster();actor=Enemy("obsidian_sentinel");kit=actor.GetComponent<MineEnemyAbility>();
        yield return EnvironmentMove();Assert.That(kit.ResponseMoves,Is.EqualTo(1));
        Set(actor.GetComponent<EnemyStagger>(),"remainingImmunityTime",0f);int hp=Run.Player.CurrentHealth;
        Run.MoveClock.Opportunity+=e=>{if(e==actor)Run.Board.TryQueueMineDrillPower(1,3);};
        int fired=0;Run.Board.MineDrillFired+=(_,__,___)=>fired++;
        yield return EnvironmentMove();Assert.That(fired,Is.EqualTo(1));
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));Assert.That(kit.IsPreparing,Is.False);
        Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.True);
        Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.False,"no custom recovery hold after interruption");
    }

    [UnityTest] public IEnumerator IronveinSentinelLostExtractionTargetFizzlesWithoutStaggerOrPower()
    {yield return MineLostExtraction();}
    private IEnumerator MineLostExtraction()
    {
        yield return LaunchMineKit("obsidian_sentinel","pickaxe_delver");var actor=Enemy("obsidian_sentinel");var board=Run.Board;
        board.TryQueuePlaceMineStones(actor,1,6,MineStoneStage.Obsidian);yield return Stable();yield return ReadyMine(actor);
        var kit=actor.GetComponent<MineEnemyAbility>();var target=board.FindMineStone(kit.StoneId).Value;
        board.Mine.drills[0].lane=target.Cell.y;board.TryQueueMineDrillPower(1,4);yield return Stable();yield return null;
        Assert.That(kit.IsPreparing,Is.False);Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.False);
        Assert.That(actor.GetComponent<EnemyOrePower>().IsPowered,Is.False);Assert.That(actor.FortifiedStacks,Is.Zero);
    }

    [UnityTest] public IEnumerator IronveinMinibossProofShowsOwnedTurretsAndReadableOreStatus()
    {yield return MineMinibossProof();}
    private IEnumerator MineMinibossProof()
    {
        yield return LaunchMineKit("siege_machinist");var actor=Enemy("siege_machinist");
        for(int cast=0;cast<4;cast++){yield return ReadyMine(actor);QuietKitFixture();}
        string dir=Path.GetFullPath(".utmp/Ironvein/Captures");Directory.CreateDirectory(dir);
        yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(dir,"phase06-turret-proof.png"));yield return null;yield return null;
        Assert.That(actor.GetComponent<MineEnemyAbility>().OwnedTurrets.Count(),Is.EqualTo(2));
    }
}
