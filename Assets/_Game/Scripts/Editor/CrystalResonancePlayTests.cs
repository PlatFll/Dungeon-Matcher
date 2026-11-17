#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Diagnostic state fixtures, NEVER used as natural pacing/skill evidence.
public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator CrystalResonanceThreeQueuedDraftsResumeExactOffersAfterSettlement()
    {
        yield return LaunchUnified();
        var state=Run.Resonance.State;state.charge=99;
        bool busySawDraft=false;
        Action<BoardClearContext> during=_=>
        { busySawDraft |= Run.Waves.GetComponent<RunUpgradeCoordinator>().IsBlockingWaveProgression; };
        Run.Board.BoardClearResolved+=during;
        // A legal accepted swap executes; fixture defeat during that action tests the barrier.
        yield return Move(()=>KillEncounter());
        Run.Board.BoardClearResolved-=during;
        Assert.IsFalse(busySawDraft,"no draft during board reporting/cascades");
        yield return Until(()=>Run.Continuation.CanCapture && Run.Continuation.Capture().draft.Count>0,"first safe notch");
        Assert.AreEqual(100,Run.Resonance.Committed);Assert.IsTrue(Run.Resonance.Sealed);
        CollectionAssert.AreEqual(new[]{12,40,70},Run.Resonance.State.pendingNotches);
        var offer=Run.Continuation.Capture().draft.ToArray();int depth=Run.Waves.CurrentWave;
        Assert.IsTrue(Run.Continuation.SaveNow());var old=Run;
        SceneManager.LoadScene("Game");
        yield return Until(()=>Run!=null && Run!=old && !Run.Continuation.IsRestoring && Run.Continuation.CanCapture &&
            Run.Continuation.Capture().draft.Count>0,"restored offer");
        CollectionAssert.AreEqual(offer,Run.Continuation.Capture().draft);
        Run.GetComponent<RunControlsUI>().Close(); // Continue intentionally resumes behind a paused confirmation.
        foreach(int notch in new[]{12,40,70})
        {
            yield return Until(()=>Run.Continuation.CanCapture && Run.Resonance.State.activeNotch==notch &&
                Run.Continuation.Capture().draft.Count>0,"queued notch "+notch);
            Assert.AreEqual(depth,Run.Waves.CurrentWave);Assert.AreEqual(0,Run.Travel.State.stage);
            var coordinator=Run.Waves.GetComponent<RunUpgradeCoordinator>();
            string choice=Run.Continuation.Capture().draft[0];
            Assert.IsTrue(coordinator.SelectRecordedCard(choice));
            Assert.IsFalse(coordinator.SelectRecordedCard(choice),"duplicate callback cannot apply twice");
        }
        yield return Until(()=>Run.Waves.CurrentWave==depth+1 && Run.Continuation.CanCapture,"locked capstone spawns");
        Assert.AreEqual(7,Run.Resonance.State.claimedMask);
        Assert.AreEqual(ResonanceCapstonePhase.InProgress,Run.Resonance.State.capstonePhase);
        Assert.IsTrue(Run.Resonance.Sealed);Assert.AreEqual(0,Run.Travel.State.stage);
    }
    [UnityTest] public IEnumerator CrystalResonanceFullMeterWaitsForEscortThenTravelsWithScore()
    {
        yield return LaunchUnified();
        if(Run.Waves.ActiveEnemies.Count<2)
            Assert.IsTrue(Run.Waves.TrySummonEnemy(Run.Zone.Definition.enemies.First(e=>e.Category==EnemyCategory.Normal),out _));
        yield return Until(()=>Run.Continuation.CanCapture,"fixture escort entrance");
        var state=Run.Resonance.State;state.charge=100;state.triggeredMask=state.claimedMask=7;
        state.capstonePhase=ResonanceCapstonePhase.InProgress;state.capstoneWave=Run.Waves.CurrentWave;
        state.capstoneEncounterId=state.encounterId;state.capstoneId="diagnostic-formation";
        state.capstoneKind=1;state.formationSeed=33;state.capstoneMembers.Add(Run.Waves.ActiveEnemies[0].Definition.EnemyId);
        string source=Run.Zone.Definition.zoneId,runId=Run.RunId;state.score=1234;
        Assert.IsTrue(RunContinuation.SupportsSnapshot(Run.Continuation.Capture()));
        var leader=Run.Waves.ActiveEnemies[0];leader.ResolveDamageWithoutFeedback(999999);leader.ResolveDamageWithoutFeedback(999999);
        yield return Until(()=>Run.Continuation.CanCapture,"leader death settled");
        Assert.IsTrue(Run.Resonance.Sealed);Assert.IsFalse(Run.Resonance.PortalReady);
        Assert.AreEqual(source,Run.Zone.Definition.zoneId);Assert.AreEqual(0,Run.Travel.State.stage);
        KillEncounter();
        yield return Until(()=>Run!=null && Run.Zone.Definition.zoneId!=source && Run.Travel.State.stage==0 &&
            Run.Waves.IsWaveActive && Run.Continuation.CanCapture,"atomic capstone travel");
        Assert.AreEqual(runId,Run.RunId);Assert.AreEqual(0,Run.Resonance.Committed);
        Assert.That(Run.Resonance.RunScore,Is.GreaterThanOrEqualTo(1234));
        Assert.AreEqual(CombatClockSnapshot.UnifiedProfile,Run.Continuation.Capture().clock.profile);
        Assert.AreEqual(ResonanceCapstonePhase.Unselected,Run.Resonance.State.capstonePhase);
    }
}
#endif
