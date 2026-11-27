#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Controlled board fixtures validate attribution through real accepted swaps;
// they are excluded from natural play/pacing samples.
public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator CrystalResonanceDeliberateFourAndBombPairProduceBoundedReceipts()
    {
        yield return LaunchUnified();PrepareSafeMove();var board=Run.Board;
        var sprites=(Sprite[])Get(board,"gemSprites");
        var type=board.GetGem(safeMoveTo.x-1,safeMoveTo.y).Type;
        board.GetGem(safeMoveTo.x+1,safeMoveTo.y).SetType(type,sprites[(int)type]);
        var earned=new List<ResonanceMastery>();Run.Resonance.MasteryEarned+=earned.Add;
        int before=board.CompletedValidPlayerMoves;
        board.ReplayPlayerSwap(safeMoveFrom.x,safeMoveFrom.y,safeMoveTo.x,safeMoveTo.y);
        yield return Until(()=>board.CompletedValidPlayerMoves==before+1 && Run.Continuation.CanCapture,"deliberate four settles");
        Assert.AreEqual(1,earned.Count(e=>(e&ResonanceMastery.DeliberateMatch)!=0));
        Assert.AreEqual(0,Run.Resonance.Committed,"mastery remains provisional during combat");
        PrepareSafeMove();board.GetGem(3,3).SetSpecialType(GemSpecialType.RowBomb);board.GetGem(4,3).SetSpecialType(GemSpecialType.ColumnBomb);
        before=board.CompletedValidPlayerMoves;board.ReplayPlayerSwap(3,3,4,3);
        yield return Until(()=>board.CompletedValidPlayerMoves==before+1 && Run.Continuation.CanCapture,"two bombs settle once");
        Assert.AreEqual(1,earned.Count(e=>(e&ResonanceMastery.SpecialSetup)!=0));
        Assert.That(Run.Resonance.Pending,Is.LessThanOrEqualTo(3));
        Run.Resonance.MasteryEarned-=earned.Add;
    }
    [UnityTest] public IEnumerator CrystalResonanceAutomaticFourCascadeIsNotDeliberateMastery()
    {
        yield return LaunchUnified();PrepareSafeMove();var board=Run.Board;
        var sprites=(Sprite[])Get(board,"gemSprites");
        // No pre-existing line: an initial vertical three clears column 1;
        // the emerald above then falls into a gap to complete a horizontal four.
        for(int x=0;x<board.Width;x++)for(int y=0;y<board.Height;y++)
            board.GetGem(x,y).SetType((GemType)((x+2*y)%6),sprites[(x+2*y)%6]);
        void Color(int x,int y,GemType t)=>board.GetGem(x,y).SetType(t,sprites[(int)t]);
        Color(1,0,GemType.Ruby);Color(1,1,GemType.Ruby);Color(1,2,GemType.Sapphire);Color(2,2,GemType.Ruby);
        Color(1,3,GemType.Emerald);foreach(int x in new[]{0,2,3})Color(x,0,GemType.Emerald);
        var receipts=new List<BoardProgressionReceipt>();var clears=new List<BoardClearContext>();
        board.ProgressionReceipt+=receipts.Add;board.BoardClearResolved+=clears.Add;
        int before=board.CompletedValidPlayerMoves;board.ReplayPlayerSwap(2,2,1,2);
        yield return Until(()=>board.CompletedValidPlayerMoves==before+1 && Run.Continuation.CanCapture,"automatic four cascade settles");
        Assert.IsTrue(clears.Any(c=>c.TriggerGemCount>=4),"fixture must actually produce the secondary four");
        Assert.IsFalse(receipts.Any(r=>(r.Mastery&ResonanceMastery.DeliberateMatch)!=0),"cascade is score/combat, not deliberate mastery");
        board.ProgressionReceipt-=receipts.Add;board.BoardClearResolved-=clears.Add;
    }
    [UnityTest] public IEnumerator CrystalResonancePendingSwapReplayKeepsScoreAndMasteryExactlyOnce()
    {
        yield return LaunchUnified();PrepareSafeMove();var board=Run.Board;
        var sprites=(Sprite[])Get(board,"gemSprites");var color=board.GetGem(safeMoveTo.x-1,safeMoveTo.y).Type;
        board.GetGem(safeMoveTo.x+1,safeMoveTo.y).SetType(color,sprites[(int)color]);
        Assert.IsTrue(Run.Continuation.SaveNow());
        board.ReplayPlayerSwap(safeMoveFrom.x,safeMoveFrom.y,safeMoveTo.x,safeMoveTo.y);
        Assert.IsTrue(Run.Continuation.SaveNow());string pending=File.ReadAllText(path);
        yield return Until(()=>Run.MoveClock.Tick==1 && Run.Continuation.CanCapture,"uninterrupted mastery action");
        var expected=Run.Continuation.Capture();Assert.That(expected.resonance.masteryMask,Is.Not.Zero);
        SceneManager.LoadScene("MainMenu");yield return null;
        File.WriteAllText(path,pending);profile.Dispose();profile=AccountProgression.UseDisposableProfile(path);
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run!=null && !Run.Continuation.IsRestoring,"mastery action replay");
        Assert.IsNull(Run.Continuation.Error);Run.GetComponent<RunControlsUI>().Close();yield return Stable();
        var actual=Run.Continuation.Capture();
        Assert.AreEqual(expected.resonance.score,actual.resonance.score);
        Assert.AreEqual(expected.resonance.masteryMask,actual.resonance.masteryMask);
        Assert.AreEqual(expected.resonance.charge,actual.resonance.charge);
        Assert.AreEqual(expected.resonance.lastReceipt,actual.resonance.lastReceipt);
        Assert.AreEqual(JsonUtility.ToJson(expected.board),JsonUtility.ToJson(actual.board));
        Assert.AreEqual(1,actual.clock.actions.completed);
    }
}
#endif
