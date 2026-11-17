#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [Serializable]private sealed class RecordedCourtCoffer
    {public string source,journalSha256,runId;public RunCombatSnapshot checkpoint;}

    [UnityTest] public IEnumerator CrystalResonanceRecordedCourtCofferHasARealAcceptedAnswer()
    {
        // An actual played boundary, restored unchanged. This diagnostic is not
        // a natural completed visit and awards no pacing credit.
        var sample=JsonUtility.FromJson<RecordedCourtCoffer>(File.ReadAllText(
            Path.Combine(Application.dataPath,"_Game/Scripts/Editor/Fixtures/CrystalResonanceCourtCoffer.json")));
        var cp=sample.checkpoint;
        Assert.IsTrue(RunContinuation.SupportsSnapshot(cp));
        var save=new AccountSave {potions=3,bombs=3,equipPotions=true,equipBombs=true};
        save.characters.Add(new CharacterProgress{id="skeleton",level=1});
        save.run=new AccountRunJournal {id=sample.runId,playerId="skeleton",level=1,supportsContinuation=true,
            checkpoint=cp,tape=new RunReplayTape{boardFeelVersion=1},completedWaves=cp.completedWaves,
            milestoneCount=cp.milestones,refinementUsed=cp.refinementUsed,kingCleared=cp.kingCleared,
            potionCharges=cp.potionCharges,bombCharges=cp.bombCharges,initialPotionCharges=3,initialBombCharges=3};
        File.WriteAllText(path,JsonUtility.ToJson(save));profile.Dispose();profile=AccountProgression.UseDisposableProfile(path);
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run!=null && Run.InitialStateReady && !Run.Continuation.IsRestoring,"recorded Court restore");
        Assert.IsNull(Run.Continuation.Error);Run.GetComponent<RunControlsUI>().Close();yield return Stable();
        Assert.AreEqual(2,Run.Board.Aquatic.coffer.charges);Assert.AreEqual(5,Run.Board.Aquatic.air);
        var option=Run.Board.GetImmediateResponses().FirstOrDefault(o=>o.Source.Column==0 && o.Source.Row==2 && o.Target.Column==1 && o.Target.Row==2);
        Assert.IsNotNull(option,"the observed ordinary-match answer must be genuinely legal in Unity");
        Assert.IsTrue(Run.Board.IsHintMoveStillValid(option.Source,option.Target));
        int before=Run.Board.CompletedValidPlayerMoves;
        Run.Board.ReplayPlayerSwap(0,2,1,2);
        yield return Until(()=>Run.Board.CompletedValidPlayerMoves==before+1 && (Run.Continuation.CanCapture || Run.IsFinished),"real coffer counter settles");
        Assert.IsTrue(Run.Board.Aquatic.coffer==null || Run.Board.Aquatic.coffer.charges==0,"adjacent accepted match releases stored oxygen");
        Assert.AreEqual(5,Run.Board.Aquatic.air,"stored oxygen repays this move's cost, capped at full AIR");
    }
}
#endif
