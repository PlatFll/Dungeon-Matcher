using NUnit.Framework;
using UnityEngine;

public sealed class IronveinFoundationTests
{
    [Test] public void BossSnapshotRejectsFutureAndImpossibleRemountButAcceptsPreviousMineSchema()
    {
        Assert.That(MineEnemyAbility.Supports(new MineEnemySnapshot{version=1}),Is.True);
        Assert.That(MineEnemyAbility.Supports(new MineEnemySnapshot{version=3}),Is.False);
        Assert.That(MineEnemyAbility.Supports(new MineEnemySnapshot{bossPhase=MineBossPhase.SecondMech}),Is.False);
        Assert.That(MineEnemyAbility.Supports(new MineEnemySnapshot{bossPhase=MineBossPhase.SecondMech,remountUsed=true}),Is.True);
    }
    [Test] public void StoneMaturesOncePerAcceptedMoveAndStopsAtObsidian()
    {
        var stone = new MineStoneState { id = 4, ownerId = 8, bornMove = 10, lastAdvanceMove = 10 };
        Assert.That(stone.Advance(10, 3), Is.False);
        Assert.That(stone.Advance(11, 3), Is.False);
        Assert.That(stone.Advance(11, 3), Is.False);
        Assert.That(stone.Advance(12, 3), Is.False);
        Assert.That(stone.Advance(13, 3), Is.True);
        Assert.That(stone.stage, Is.EqualTo(MineStoneStage.Hardened));
        for (int move = 14; move <= 30; move++) stone.Advance(move, 3);
        Assert.That(stone.stage, Is.EqualTo(MineStoneStage.Obsidian));
        Assert.That(stone.id, Is.EqualTo(4)); Assert.That(stone.ownerId, Is.EqualTo(8));
    }

    [Test] public void StoneHitResetsAgeAndCannotHardenOnThatAction()
    {
        var stone = new MineStoneState(); stone.Advance(1, 3); stone.Advance(2, 3);
        stone.RecordHit(3); Assert.That(stone.Advance(3, 3), Is.False);
        Assert.That(stone.ignoredMoves, Is.Zero);
        stone.Advance(4, 3); stone.Advance(5, 3); Assert.That(stone.Advance(6, 3), Is.True);
    }

    [Test] public void StoneContinuationKeepsAgeAndHitWithoutReplayingAnAction()
    {
        var stone = new MineStoneState { id = 17, stage = MineStoneStage.Hardened, ignoredMoves = 2, lastAdvanceMove = 5, lastHitMove = 3 };
        var resumed = JsonUtility.FromJson<MineStoneState>(JsonUtility.ToJson(stone));
        Assert.That(resumed.Advance(5, 3), Is.False);
        Assert.That(resumed.Advance(6, 3), Is.True);
        Assert.That(resumed.stage, Is.EqualTo(MineStoneStage.Obsidian));
        Assert.That(stone.stage, Is.EqualTo(MineStoneStage.Hardened), "snapshot is detached");
    }

    [Test] public void MinePayloadRejectsFutureVersionWithoutRejectingOlderZones()
    {
        var old = new RunCombatSnapshot { version = 1, board = new BoardCombatSnapshot() };
        Assert.That(RunContinuation.SupportsSnapshot(old), Is.True);
        old.board.mine = new MineEnvironmentState { version = MineEnvironmentState.CurrentVersion + 1 };
        Assert.That(RunContinuation.SupportsSnapshot(old), Is.False);
    }

    [Test] public void MineStubIsTestingOnlyAndUsesProvisionalSerializedTuning()
    {
        var zone = Resources.Load<ZoneDefinition>("Zones/ironvein-excavation");
        Assert.That(zone, Is.Not.Null); Assert.That(zone.maturesStone, Is.True);
        Assert.That(zone.mineMovesPerStage, Is.EqualTo(3)); Assert.That(zone.maximumMineStones, Is.EqualTo(6));
        Assert.That(ZoneTravelController.TestingReady(zone), Is.True);
        Assert.That(ZoneTravelController.DestinationReady(zone), Is.False);
    }
}
