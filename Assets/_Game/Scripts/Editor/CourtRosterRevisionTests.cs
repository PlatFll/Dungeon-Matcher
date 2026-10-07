using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator CourtRevisionFiniteReserveAndEmergencyCadence()
    {yield return FiniteCourtReserve();}
    private IEnumerator FiniteCourtReserve()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;var state=board.Aquatic;state.StartFlood(18,board.CompletedValidPlayerMoves);
        Call(board,"EnsureAquaticSupply",5);Assert.That(state.bubbles.Count,Is.EqualTo(5));
        int[] ids=state.bubbles.ToArray();state.air=1;
        Call(board,"AdvanceEmergencyAir",100);Assert.That(state.bubbles,Is.EqualTo(ids));
        Assert.That(state.reserveExhausted,Is.False);
        // Captured charges are still reserve oxygen, including when no bubble is free.
        state.bubbles.Clear();state.coffer=new AquaticCofferState{id=1,charges=5};
        Call(board,"AdvanceEmergencyAir",101);Call(board,"EnsureReachableAir");
        Assert.That(board.RemainingOxygenReserve,Is.EqualTo(5));Assert.That(state.bubbles,Is.Empty);
        Assert.That(state.reserveExhausted,Is.False);
        state.coffer=null;Call(board,"AdvanceEmergencyAir",102);
        Assert.That(state.reserveExhausted,Is.True);Assert.That(state.nextSupplyMove,Is.EqualTo(105));
        Call(board,"AdvanceEmergencyAir",104);Assert.That(state.bubbles,Is.Empty);
        state.air=3;Call(board,"AdvanceEmergencyAir",105);Assert.That(state.bubbles.Count,Is.EqualTo(1));
        state.air=1;Call(board,"AdvanceEmergencyAir",108);Assert.That(state.bubbles.Count,Is.EqualTo(2));
        Call(board,"AdvanceEmergencyAir",111);Assert.That(state.bubbles.Count,Is.EqualTo(2));
        var before=Run.Continuation.Capture();yield return ResumeRoster();
        Assert.That(Run.Board.Aquatic.reserveExhausted,Is.True);
        Assert.That(Run.Board.Aquatic.nextSupplyMove,Is.EqualTo(before.board.aquatic.nextSupplyMove));
        Assert.That(Run.Board.Aquatic.bubbles,Is.EqualTo(before.board.aquatic.bubbles));
    }

    [UnityTest] public IEnumerator CourtRevisionLowAirRelocatesWithoutMintingResources()
    {yield return CourtAirRelocation();}
    private IEnumerator CourtAirRelocation()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;var state=board.Aquatic;state.StartFlood(18,Run.MoveClock.Tick);state.air=0;
        Call(board,"EnsureReachableAir");Assert.That(state.bubbles,Is.Empty,"no free rescue during stolen reserve");
        var reachable=(HashSet<Gem>)Call(board,"ImmediatelyClearableOrdinaryGems");
        var blocked=Enumerable.Range(0,board.Width).SelectMany(x=>Enumerable.Range(0,board.Height).Select(y=>board.GetGem(x,y)))
            .First(g=>g!=null&&!reachable.Contains(g));
        state.bubbles.Add(blocked.BoardIdentity);Call(board,"EnsureReachableAir");
        Assert.That(state.bubbles.Count,Is.EqualTo(1));Assert.That(state.air,Is.Zero);
        Assert.That(reachable.Any(g=>state.bubbles.Contains(g.BoardIdentity)),Is.True);
    }

    [UnityTest] public IEnumerator CourtRevisionAirReceiptShowsSpendThenActualTwoBlockGain()
    {yield return CourtAirReceipt();}
    private IEnumerator CourtAirReceipt()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;var state=board.Aquatic;state.StartFlood(18,Run.MoveClock.Tick);state.air=3;
        var collected=board.GetGem(safeMoveFrom.x,safeMoveFrom.y);state.bubbles.Add(collected.BoardIdentity);
        int opening=-1,cost=0;int[] gains=null;
        board.AirReceipt+=(before,spend,adds)=>{opening=before;cost=spend;gains=adds;};
        var view=board.GetComponent<AquaticEnvironmentView>();var beats=new List<string>();
        int tick=board.CompletedValidPlayerMoves;
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",collected,board.GetGem(safeMoveTo.x,safeMoveTo.y)));
        yield return Until(()=>
        {
            var label=Get(view,"airReturn") as TMPro.TMP_Text;
            if(label!=null && label.gameObject.activeInHierarchy && !beats.Contains(label.text))beats.Add(label.text);
            return board.CompletedValidPlayerMoves>tick && Run.Continuation.CanCapture && beats.Contains("+2");
        },"distinct AIR spend and gain beats");
        Assert.That(opening,Is.EqualTo(3));Assert.That(cost,Is.EqualTo(1));Assert.That(gains,Is.EqualTo(new[]{2}));
        Assert.That(state.air,Is.EqualTo(4),"bubble remains +2, move remains -1");
        Assert.That(beats.IndexOf("-1"),Is.GreaterThanOrEqualTo(0));
        Assert.That(beats.IndexOf("-1"),Is.LessThan(beats.IndexOf("+2")));
    }

    [UnityTest] public IEnumerator CourtRevisionLegacyFloodPreservesRemainingTimeAndReserve()
    {yield return CourtLegacyReserve();}
    private IEnumerator CourtLegacyReserve()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();
        var board=Run.Board;var legacy=new AquaticEnvironmentState{version=1,phase=TidePhase.Flooded,air=2,wetMoves=7,
            bubbles=new List<int>{board.GetGem(0,0).BoardIdentity},nextSupplyMove=1};
        Call(board,"RestoreAquatic",legacy);
        Assert.That(board.Aquatic.version,Is.EqualTo(2));Assert.That(board.Aquatic.air,Is.EqualTo(2));
        Assert.That(board.Aquatic.wetMoves,Is.EqualTo(7));Assert.That(board.Aquatic.bubbles,Is.EqualTo(legacy.bubbles));
        Call(board,"AdvanceEmergencyAir",100);Assert.That(board.Aquatic.bubbles.Count,Is.EqualTo(1));
        Assert.That(board.Aquatic.reserveExhausted,Is.False);
    }
}
