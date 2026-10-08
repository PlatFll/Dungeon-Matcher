using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator LaunchMine()
    {
        RunLaunchOptions.ForestPrototype = false; RunLaunchOptions.StartingZone = "ironvein-excavation";
        SceneManager.LoadScene("Game"); yield return Stable(); PreserveRoster();
        Assert.That(Run.Zone.Definition.zoneId, Is.EqualTo("ironvein-excavation"));
        Assert.That(Run.MoveClock, Is.Not.Null);
        Assert.That(CombatMoveClock.MoveBasics, Is.False);
        preserveCounterplayCrystal = true;
        Run.Board.GetGem(Run.Board.Width - 1, 0).SetSpecialType(GemSpecialType.ColorCrystal);
    }
    private BoardCellSnapshot[] MineCells() => Run.Continuation.Capture().board.cells
        .Where(c => c.barricade && c.barricadeStyle == EnemyBarricadeStyle.MineStone).ToArray();

    [UnityTest] public IEnumerator IronveinStonesRespectCapsSpecialsAndMatureWithStableIdentity()
    { yield return MinePlacementAndAge(); }
    private IEnumerator MinePlacementAndAge()
    {
        yield return LaunchMine(); var board = Run.Board; var owner = Run.Waves.ActiveEnemies[0];
        int crystal = board.GetGem(board.Width - 1, 0).BoardIdentity;
        Assert.That(board.TryQueuePlaceMineStones(owner, 2, 2), Is.True); yield return Stable();
        Assert.That(MineCells().Length, Is.EqualTo(2));
        var ids = MineCells().Select(c => c.mineStone.id).ToArray();
        Assert.That(board.TryQueuePlaceMineStones(owner, 2, 2), Is.False);
        for (int move = 1; move <= 3; move++) { board.QueueZoneEnvironment(move); yield return Stable(); }
        Assert.That(MineCells().Select(c => c.mineStone.id), Is.EquivalentTo(ids));
        Assert.That(MineCells().All(c => c.mineStone.stage == MineStoneStage.Hardened && c.durability == 2), Is.True);
        board.QueueZoneEnvironment(3); yield return Stable();
        Assert.That(MineCells().All(c => c.mineStone.ignoredMoves == 0), Is.True);
        Assert.That(board.TryQueuePlaceMineStones(owner, 20, 20), Is.True); yield return Stable();
        Assert.That(MineCells().Length, Is.InRange(2, 6));
        Assert.That(Run.Continuation.Capture().board.cells.Any(c => c.identity == crystal && c.special == GemSpecialType.ColorCrystal), Is.True);
        Assert.That(board.GetImmediateResponses().Count, Is.GreaterThan(0));
    }

    [UnityTest] public IEnumerator IronveinStoneHitIsDeduplicatedAndKeepsObsidianMaterial()
    { yield return MineHitAndPhoto(); }
    private IEnumerator MineHitAndPhoto()
    {
        yield return LaunchMine(); var board = Run.Board; var owner = Run.Waves.ActiveEnemies[0];
        var before = board.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);
        Assert.That(board.TryQueuePlaceMineStones(owner, 1, 6, MineStoneStage.Obsidian), Is.True); yield return Stable();
        var cell = MineCells().Single();
        var adjacent = new HashSet<Gem>();
        foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        { var gem = board.GetGem(cell.x + d.x, cell.y + d.y); if (gem != null) adjacent.Add(gem); }
        Assert.That(adjacent.Count, Is.GreaterThan(0));
        Call(board, "DamageBarricadesForClear", adjacent, null, false);
        Assert.That(MineCells().Single().durability, Is.EqualTo(2), "one canonical clear, not one hit per adjacent gem");
        Assert.That(MineCells().Single().mineStone.stage, Is.EqualTo(MineStoneStage.Obsidian));
        Assert.That(board.TryRestoreBoardMemory(before, Run.Waves.ContinuationEnemy), Is.True);
        Assert.That(MineCells().Single().durability, Is.EqualTo(2), "photo cannot remove a new mine stone or repair it");
        var saved = board.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);
        adjacent.Clear();
        foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        { var gem = board.GetGem(cell.x + d.x, cell.y + d.y); if (gem != null) adjacent.Add(gem); }
        for (int hit = 0; hit < 2; hit++) Call(board, "DamageBarricadesForClear", adjacent, null, false);
        Assert.That(board.MineStoneCount, Is.Zero);
        Assert.That(board.TryRestoreBoardMemory(saved, Run.Waves.ContinuationEnemy), Is.True);
        Assert.That(board.MineStoneCount, Is.Zero, "photo cannot resurrect a broken stone");
        Assert.That(board.GetGem(cell.x, cell.y), Is.Not.Null);
    }

    [UnityTest] public IEnumerator IronveinAcceptedMovesResumeAndTravelCleanStoneState()
    { yield return MineResumeAndTravel(); }
    private IEnumerator MineResumeAndTravel()
    {
        yield return LaunchMine(); var board = Run.Board;
        Assert.That(board.TryQueuePlaceMineStones(Run.Waves.ActiveEnemies[0], 1, 6, MineStoneStage.Obsidian), Is.True);
        yield return Stable(); yield return EnvironmentMove();
        var before = MineCells(); long nextId = board.Mine.nextStoneId;
        yield return ResumeRoster(); board = Run.Board;
        Assert.That(board.Mine.lastSettledMove, Is.EqualTo(1)); Assert.That(board.Mine.nextStoneId, Is.EqualTo(nextId));
        Assert.That(MineCells().Select(c => c.mineStone.id), Is.EquivalentTo(before.Select(c => c.mineStone.id)));
        Assert.That(board.TryQueuePlaceMineStones(Run.Waves.ActiveEnemies[0], 1, 1), Is.False,
            "Continue must retain the living owner's stone cap");
        var saved = Run.Continuation.Capture().board;
        board.PrepareZoneArrival(saved, Resources.Load<ZoneDefinition>("Zones/dungeon"));
        Assert.That(saved.mine, Is.Null); Assert.That(saved.cells.Any(c => c.barricade && c.barricadeStyle == EnemyBarricadeStyle.MineStone), Is.False);
        foreach (var cell in before) Assert.That(saved.cells.Single(c => c.x == cell.x && c.y == cell.y).hasGem, Is.True);
        Assert.That(board.MineStoneCount, Is.EqualTo(before.Length), "detached cleanup never mutates the live source");
    }
}
