using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator IronveinFullDrillNeverStopsAtTheFirstOfTwoObsidianStones()
    { yield return MineTwoStoneLane(); }
    private IEnumerator MineTwoStoneLane()
    {
        yield return LaunchMine(); var board=Run.Board; int row=board.Height/2;
        // Restrict the real safe-placement API to two ordinary cells in one lane.
        for(int y=0;y<board.Height;y++) for(int x=0;x<board.Width;x++)
            board.GetGem(x,y).SetSpecialType(y==row&&(x==0||x==board.Width-2)?GemSpecialType.None:GemSpecialType.ColorCrystal);
        Assert.That(board.TryQueuePlaceMineStones(Run.Waves.ActiveEnemies[0],2,6,MineStoneStage.Obsidian),Is.True);
        yield return Stable(); Assert.That(MineCells().Length,Is.EqualTo(2));
        Assert.That(MineCells().All(c=>c.y==row&&c.durability==3),Is.True);
        int reports=0;board.BoardClearResolved+=_=>reports++;
        board.Mine.drills[0].lane=row;board.TryQueueMineDrillPower(1,4);yield return Stable();
        Assert.That(board.MineStoneCount,Is.Zero,"both 3-hit stones are removed by the same penetrating drill");
        Assert.That(reports,Is.Zero,"crossed special gems do not detonate");
    }

    [UnityTest] public IEnumerator IronveinManualIntakeLaunchWaitsForActorWorkAndKeepsBoardOwned()
    { yield return MineAcceptedLaunch(); }
    private IEnumerator MineAcceptedLaunch()
    {
        yield return LaunchMine();var board=Run.Board;PrepareSafeMove();
        board.Mine.drills[0].lane=safeMoveTo.y;
        board.TryQueueMineDrillPower(1,3);yield return Stable();
        bool offered=false,fired=false;int reports=0,reportsAtFire=-1;
        Run.MoveClock.Opportunity+=_=>offered=true;board.BoardClearResolved+=_=>reports++;
        board.MineDrillFired+=(id,_,__)=>
        { if(id!=1)return;fired=true;Assert.That(offered,Is.True);Assert.That(board.IsBusy,Is.True);Assert.That(Run.MoveClock.IsResolving,Is.True);reportsAtFire=reports; };
        yield return EnvironmentMove();
        Assert.That(fired,Is.True);Assert.That(board.Mine.drills[0].charge,Is.Zero);
        Assert.That(reports,Is.EqualTo(reportsAtFire));Assert.That(board.CompletedValidPlayerMoves,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator IronveinDrillsPenetrateObsidianAndCrossOnceWithoutRewards()
    { yield return MineDrillPenetration(); }
    private IEnumerator MineDrillPenetration()
    {
        yield return LaunchMine(); var board = Run.Board;
        Assert.That(board.TryQueuePlaceMineStones(Run.Waves.ActiveEnemies[0], 5, 6, MineStoneStage.Obsidian), Is.True);
        yield return Stable(); var stones = MineCells(); Assert.That(stones.Length, Is.GreaterThan(0));
        var first = stones.OrderBy(c=>c.x).First();
        board.Mine.drills[0].lane = first.y; board.Mine.drills[1].lane = first.x;
        var before = Run.Continuation.Capture().board;
        var removed = before.cells.Where(c=>c.hasGem && (c.x==first.x || c.y==first.y)).Select(c=>c.identity).ToArray();
        int reports=0,settles=0;var launches=new List<int>();
        board.BoardClearResolved+=_=>reports++;
        board.MineDrillBatchSettled+=()=>settles++;
        board.MineDrillFired+=(id,_,__)=>launches.Add(id);
        int hp=Run.Player.CurrentHealth,energy=Run.Player.GetComponent<PlayerAbilityEnergy>().CurrentEnergy;
        // Both feeds are synchronous requests; the queue must batch both passes.
        Assert.That(board.TryQueueMineDrillPower(0,4),Is.True); yield return Stable();
        CollectionAssert.AreEqual(new[]{1,2},launches); Assert.That(settles,Is.EqualTo(1));
        Assert.That(MineCells().Any(c=>c.x==first.x || c.y==first.y),Is.False,"every intersected stone is destroyed regardless of tier");
        Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.hasGem&&removed.Contains(c.identity)),Is.False);
        Assert.That(reports,Is.Zero,"neither machine clear nor its refill cascades report player combat");
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        Assert.That(Run.Player.GetComponent<PlayerAbilityEnergy>().CurrentEnergy,Is.EqualTo(energy));
        Assert.That(board.Mine.drills.All(d=>d.charge==0),Is.True,"no recursive intake");
        Assert.That(Run.Continuation.Capture().board.cells.Count(c=>c.hasGem),Is.EqualTo(board.Width*board.Height-board.MineStoneCount));
    }

    [UnityTest] public IEnumerator IronveinDrillChargeDeduplicatesManualMatchesAndSurvivesContinue()
    { yield return MineChargeAndContinue(); }
    private IEnumerator MineChargeAndContinue()
    {
        yield return LaunchMine(); var board=Run.Board; var drill=board.Mine.drills[0];
        var match=new HashSet<Gem>{board.GetGem(0,drill.lane),board.GetGem(1,drill.lane),board.GetGem(2,drill.lane)};
        Call(board,"RegisterMinePlayerMatch",match,true);Call(board,"RegisterMinePlayerMatch",match,true);
        Call(board,"RegisterMinePlayerMatch",match,false);yield return Stable();
        Assert.That(drill.charge,Is.EqualTo(1));
        Assert.That(board.TryQueueMineDrillPower(1,2),Is.True);yield return Stable();
        Assert.That(drill.charge,Is.EqualTo(3));int lane=drill.lane;
        yield return ResumeRoster();board=Run.Board;
        Assert.That(board.Mine.drills.Single(d=>d.id==1).charge,Is.EqualTo(3));
        Assert.That(board.Mine.drills.Single(d=>d.id==1).lane,Is.EqualTo(lane));
        var photo=board.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);
        Assert.That(board.TryQueueMineDrillPower(1,1),Is.True);yield return Stable();
        Assert.That(board.TryRestoreBoardMemory(photo,Run.Waves.ContinuationEnemy),Is.True);
        Assert.That(board.Mine.drills.Single(d=>d.id==1).charge,Is.Zero,"photo cannot refund spent charge");
        int moves=board.CompletedValidPlayerMoves;
        Run.GetComponent<RunControlsUI>().OpenSettings();yield return new WaitForSecondsRealtime(.2f);
        Assert.That(board.CompletedValidPlayerMoves,Is.EqualTo(moves));Assert.That(board.Mine.drills[0].charge,Is.Zero);
        Run.GetComponent<RunControlsUI>().Close();
    }

    [UnityTest] public IEnumerator IronveinDrillProofFitsEdgesAndLeavesSigilSidesClear()
    { yield return MineDrillProof(); }
    private IEnumerator MineDrillProof()
    {
        yield return LaunchMine();var board=Run.Board;
        board.TryQueueMineDrillPower(1,3);board.TryQueueMineDrillPower(2,2);yield return Stable();
        Assert.That(board.GetComponent<MineEnvironmentView>(),Is.Not.Null);
        var view=board.GetComponent<MineEnvironmentView>();
        foreach(var renderer in view.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.transform.parent?.name.StartsWith("MineDrill_")==true))
        {
            foreach(var point in new[]{renderer.bounds.min,renderer.bounds.max})
            {var screen=Camera.main.WorldToScreenPoint(point);Assert.That(screen.x,Is.InRange(0f,(float)Screen.width));Assert.That(screen.y,Is.InRange(0f,(float)Screen.height));}
        }
        string dir=Path.GetFullPath(".utmp/Ironvein/Captures");Directory.CreateDirectory(dir);
        yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(dir,"phase03-drill-proof.png"));yield return null;yield return null;
    }
}
