using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator RosterFinalRoyalSlideReplayKeepsRouteAndRefill() {yield return RoyalSlideReplay();}
    private IEnumerator RoyalSlideReplay()
    {
        yield return RoyalCofferFixture();var board=Run.Board;PrepareSafeMove();
        var c=board.Aquatic.coffer;
        var hit=new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down}
            .Select(d=>board.GetGem(c.x+d.x,c.y+d.y)).First(g=>g!=null);
        var hitCell=new Vector2Int(hit.Column,hit.Row);var original=Run.Continuation.Capture();string runId=Run.RunId;
        Assert.That(board.TryClearPlayerArea(hit,0,()=>true),Is.True);yield return Stable();
        var expected=board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot);
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
        Assert.That(AccountProgression.Current.StoreCheckpoint(runId,original),Is.True);
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run?.Continuation!=null&&!Run.Continuation.IsRestoring,"restore pre-hit boundary");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();board=Run.Board;
        Assert.That(board.TryClearPlayerArea(board.GetGem(hitCell.x,hitCell.y),0,()=>true),Is.True);yield return Stable();
        var actual=board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot);
        Assert.That(actual.aquatic.coffer.x,Is.EqualTo(expected.aquatic.coffer.x));
        Assert.That(actual.aquatic.coffer.y,Is.EqualTo(expected.aquatic.coffer.y));
        Assert.That(actual.refillRandom,Is.EqualTo(expected.refillRandom));
        Assert.That(actual.cells.Select(JsonUtility.ToJson),Is.EqualTo(expected.cells.Select(JsonUtility.ToJson)));
    }
    [UnityTest] public IEnumerator RosterFinalRoyalRoutesCoverBothLengthsAndAllCardinals() {yield return RoyalRoutes();}
    private IEnumerator RoyalRoutes()
    {
        yield return LaunchCourt("shellback_porter");PrepareSafeMove();var board=Run.Board;var origin=new Vector2Int(3,3);
        var routes=(List<List<Vector2Int>>)Call(board,"CofferSlideRoutes",origin);
        Assert.That(routes.Count,Is.EqualTo(8));
        foreach(var d in new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down})
        foreach(int length in new[]{2,3})Assert.That(routes.Any(r=>r.Count==length+1&&r.Last()==origin+d*length),Is.True);
        var blocked=origin+Vector2Int.right;var holes=(Dictionary<Vector2Int,int>)Get(board,"minedCellOwners");
        holes[blocked]=123;
        routes=(List<List<Vector2Int>>)Call(board,"CofferSlideRoutes",origin);
        Assert.That(routes.Count,Is.EqualTo(6));Assert.That(routes.Any(r=>r.Contains(blocked)),Is.False);
        holes.Clear();
    }
    [UnityTest] public IEnumerator RosterFinalTributeCasterDeathDoesNotConsumeBubblesOrGrantBuffs() {yield return TributeCasterDeath();}
    private IEnumerator TributeCasterDeath()
    {
        yield return TributeFixture();var board=Run.Board;var queen=Enemy("queen_nacre");
        var bubbles=board.Aquatic.bubbles.ToArray();int air=board.Aquatic.air;
        queen.ResolveDamageWithoutFeedback(99999);yield return Stable();
        Assert.That(board.Aquatic.bubbles,Is.EquivalentTo(bubbles));Assert.That(board.Aquatic.air,Is.EqualTo(air));
        Assert.That(Run.Waves.ActiveEnemies.Where(e=>e!=null).Sum(e=>e.FortifiedStacks),Is.Zero);
        var marks=new List<CasterBoardTarget>();board.CollectCasterTargets(marks);
        Assert.That(marks.All(m=>m.Owner!=queen),Is.True);
    }
}
