using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator BombSwapAlsoClearsTheColorMatchAtBothNewPositions()
    {
        yield return LaunchDungeonHazards(); var b=Run.Board; PrepareSafeMove();
        var sprites=(Sprite[])Get(b,"gemSprites");
        foreach(var cell in new[]{new Vector2Int(3,2),new Vector2Int(3,4),new Vector2Int(4,3)})
            b.GetGem(cell.x,cell.y).SetType(GemType.Emerald,sprites[(int)GemType.Emerald]);
        foreach(var cell in new[]{new Vector2Int(4,2),new Vector2Int(4,4),new Vector2Int(3,3)})
            b.GetGem(cell.x,cell.y).SetType(GemType.Ruby,sprites[(int)GemType.Ruby]);
        var a=b.GetGem(3,3);var z=b.GetGem(4,3);
        a.SetSpecialType(GemSpecialType.RowBomb);z.SetSpecialType(GemSpecialType.RowBomb);
        var targets=new[]{b.GetGem(3,2),b.GetGem(3,4),b.GetGem(4,2),b.GetGem(4,4)}.Select(g=>g.BoardIdentity).ToArray();
        int moves=b.CompletedValidPlayerMoves;bool firstClear=false;
        b.BoardClearResolved+=context=>{
            if(firstClear)return; firstClear=true;
            Assert.That(context.Source,Is.EqualTo(BoardClearSource.Match),"ordinary matches report before bomb collateral");
        };
        b.ReplayPlayerSwap(3,3,4,3);yield return Stable();
        Assert.That(firstClear,Is.True);Assert.That(b.CompletedValidPlayerMoves,Is.EqualTo(moves+1));
        Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.hasGem&&targets.Contains(c.identity)),Is.False,
            "the four off-row gems made two vertical matches and must clear too");
    }

    [UnityTest] public IEnumerator DrillContactActivatesEachBombFamilyAndChainsOnlyOnce()
    {
        yield return LaunchMine();var b=Run.Board;b.InitializeMine();
        foreach(var special in new[]{GemSpecialType.RowBomb,GemSpecialType.ColumnBomb,
            GemSpecialType.PoisonBomb,GemSpecialType.HealingBomb,GemSpecialType.ShieldBomb})
        {
            PrepareSafeMove();
            int row=b.Mine.drills[0].lane;var bomb=b.GetGem(1,row);bomb.SetSpecialType(special);
            // Off-row column bomb is reached by every area/directional bomb,
            // except a row bomb which reaches its counterpart in the same row.
            var chained=b.GetGem(2,row);chained.SetSpecialType(GemSpecialType.ColumnBomb);
            int offRow=b.GetGem(2,(row+2)%b.Height).BoardIdentity;
            int activated=0,reports=0;
            System.Action<RunUpgradeSpecialClearContext> onBomb=context=>activated+=context.SpecialCount;
            System.Action<BoardClearContext> onClear=_=>reports++;
            b.RunUpgradeSpecialClearPrepared+=onBomb;b.BoardClearResolved+=onClear;
            Assert.That(b.TryQueueMineDrillPower(1,4),Is.True);yield return Stable();
            Assert.That(reports,Is.GreaterThan(0),special.ToString());
            Assert.That(activated,Is.EqualTo(2),"each of the two bombs detonates once");
            Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.hasGem&&c.identity==offRow),Is.False);
            Assert.That(b.Mine.drills.All(d=>d.charge==0),Is.True,"bombs cannot recursively feed a drill");
            b.RunUpgradeSpecialClearPrepared-=onBomb;b.BoardClearResolved-=onClear;
        }
    }

    [UnityTest] public IEnumerator PortableDrillActivatesBombsAndKeepsItsSinglePowerFeed()
    {
        yield return LaunchMine();var b=Run.Board;b.InitializeMine();PrepareSafeMove();
        int row=3;var owner=Run.Waves.ActiveEnemies[0];
        b.GetGem(2,row).SetSpecialType(GemSpecialType.ColumnBomb);
        int target=b.GetGem(2,6).BoardIdentity;int activated=0;bool? edge=null;
        b.RunUpgradeSpecialClearPrepared+=context=>activated+=context.SpecialCount;
        Assert.That(b.TryQueueSmallMineDrill(owner,true,row,value=>edge=value),Is.True);
        yield return Stable();Assert.That(edge,Is.True);Assert.That(activated,Is.EqualTo(1));
        Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.hasGem&&c.identity==target),Is.False);
        Assert.That(b.Mine.drills[0].charge,Is.EqualTo(1),"portable feed is once; bomb collateral cannot feed again");
    }

    [UnityTest] public IEnumerator DrillStarWaitsUntilTheReservedLaunchBatchFinishes()
    {
        yield return LaunchMine();var b=Run.Board;b.InitializeMine();PrepareSafeMove();
        int row=b.Mine.drills[0].lane;var star=b.GetGem(0,row);star.SetSpecialType(GemSpecialType.ColorCrystal);
        int identity=star.BoardIdentity;bool exited=false;
        b.MineDrillExploded+=(id,_)=>{if(id==1){exited=true;Assert.That(b.GetGem(0,row),Is.SameAs(star));}};
        Assert.That(b.TryQueueMineDrillPower(1,4),Is.True);yield return Stable();
        Assert.That(exited,Is.True);Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.identity==identity),Is.False);
    }

    [UnityTest] public IEnumerator CrumbleWarningDoesNotConsumeThinkingOrFreeActionsAndRechecksSpecials()
    {
        yield return LaunchDungeonHazards();var b=Run.Board;PrepareSafeMove();
        var before=b.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);
        b.QueueZoneEnvironment(4);yield return Stable();
        var warnings=b.CrumblingTileWarnings.Keys.ToArray();Assert.That(warnings.Length,Is.InRange(1,2));
        yield return new WaitForSeconds(.7f);
        b.QueueZoneEnvironment(4);yield return Stable();
        Assert.That(b.CrumblingTileWarnings.Keys,Is.EquivalentTo(warnings));
        Assert.That(b.TryRestoreBoardMemory(before,Run.Waves.ContinuationEnemy),Is.True);
        Assert.That(b.CrumblingTileWarnings.Keys,Is.EquivalentTo(warnings),"photo cannot erase the current warning");
        foreach(var cell in warnings)b.GetGem(cell.x,cell.y).SetSpecialType(GemSpecialType.ShieldBomb);
        b.QueueZoneEnvironment(5);yield return Stable();
        Assert.That(b.CrumblingTileWarnings,Is.Empty);
        foreach(var cell in warnings){Assert.That(b.IsCellPlayable(cell.x,cell.y),Is.True);Assert.That(b.GetGem(cell.x,cell.y).SpecialType,Is.EqualTo(GemSpecialType.ShieldBomb));}
        var snapshot=b.CaptureContinuation(Run.Waves.ContinuationOwnerSlot);
        b.QueueZoneEnvironment(8);yield return Stable();snapshot=b.CaptureContinuation(Run.Waves.ContinuationOwnerSlot);
        Assert.That(snapshot.cells.Any(c=>c.crumbleBreakMove>0),Is.True);
        b.PrepareZoneArrival(snapshot,Resources.Load<ZoneDefinition>("Zones/magical-forest"));
        Assert.That(snapshot.cells.Any(c=>c.crumbleBreakMove>0),Is.False,"travel cancels old-zone warnings");
    }
}
