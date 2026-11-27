#if UNITY_EDITOR
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Diagnostic controls are excluded from natural pacing and policy evidence.
public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator CrystalResonanceRealTelegraphCounterAwardsOncePerEncounter()
    {
        yield return LaunchUnified();PrepareSafeMove();var board=Run.Board;
        int awards=0;Run.Resonance.MasteryEarned+=earned=>{if((earned&ResonanceMastery.TacticalCounter)!=0)awards++;};
        for(int repeat=0;repeat<2;repeat++)
        {
            PrepareSafeMove();BoardController.GemSetThreat threat=null;
            Assert.IsTrue(board.TryQueueMarkGemSet(Run.Waves.ActiveEnemies[0],1,10,false,t=>threat=t,()=>false,cancelOnAnyTargetLost:true));
            yield return Until(()=>threat!=null && Run.Continuation.CanCapture,"warning placed");
            // Keep this deterministic fixture's target in the initial three-match.
            threat.Targets.Clear();threat.Targets.Add(board.GetGem(safeMoveTo.x-1,safeMoveTo.y));
            int before=board.CompletedValidPlayerMoves;
            board.ReplayPlayerSwap(safeMoveFrom.x,safeMoveFrom.y,safeMoveTo.x,safeMoveTo.y);
            yield return Until(()=>board.CompletedValidPlayerMoves==before+1 && Run.Continuation.CanCapture,"counter settles");
            Assert.IsTrue(threat.Ended);Assert.AreEqual(1,awards);
            Assert.AreEqual(0,Run.Resonance.Committed,"counter mastery stays provisional");
        }
    }
    [UnityTest] public IEnumerator CrystalResonanceExhaustedCatalogConsumesNotchWithoutInvisibleGate()
    {
        yield return LaunchUnified();Set(Run.Waves,"advanceWavesAutomatically",false);
        var runtime=RunUpgradeRuntime.Current;var original=runtime.Catalog;
        var exhausted=ScriptableObject.CreateInstance<RunUpgradeCatalog>();
        Set(exhausted,"upgrades",new RunUpgradeDefinition[0]);Set(runtime,"catalog",exhausted);
        try
        {
            Run.Resonance.State.charge=11;
            LogAssert.Expect(LogType.Warning,new Regex("No legal run upgrades were available"));
            KillEncounter();
            yield return Until(()=>Run.Resonance.State.claimedMask==1 && !Run.Resonance.IsSavePending && Run.Continuation.CanCapture,"empty offer safely consumed");
            Assert.IsFalse(Run.Resonance.HasPendingDrafts);
            Assert.IsFalse(Run.Waves.GetComponent<RunUpgradeCoordinator>().IsBlockingWaveProgression);
            Assert.IsFalse(Run.Board.IsExternalInputBlocked);
            Assert.AreEqual(0,Run.Continuation.Capture().draft.Count);
            Assert.IsTrue(RunContinuation.SupportsSnapshot(Run.Continuation.Capture()));
        }
        finally {Set(runtime,"catalog",original);Object.DestroyImmediate(exhausted);}
    }
}
#endif
