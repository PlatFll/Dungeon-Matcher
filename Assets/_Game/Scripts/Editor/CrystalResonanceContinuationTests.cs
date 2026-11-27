#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

public sealed class CrystalResonanceContinuationTests
{
    private RunCombatSnapshot Snapshot()
    {
        var saved = new RunCombatSnapshot {version=2,wave=1,
            clock=new CombatClockSnapshot {profile=CombatClockSnapshot.UnifiedProfile,zoneId="dungeon"},
            travel=new ZoneTravelSnapshot {version=1,enabled=true,zoneId="dungeon"},
            resonance=new CrystalResonanceSnapshot {version=1,capstoneRandom=19}};
        saved.resonance.StartVisit("dungeon",0,new CrystalResonanceTuning());
        saved.resonance.BeginEncounter("fixture",1); return saved;
    }
    [Test] public void LegacyAbsentResonanceAndRoundTripRetainClockUnits()
    {
        var saved = Snapshot(); saved.resonance=null;
        Assert.IsTrue(RunContinuation.SupportsSnapshot(saved));
        var copy=JsonUtility.FromJson<RunCombatSnapshot>(JsonUtility.ToJson(saved));
        Assert.IsTrue(RunContinuation.SupportsSnapshot(copy));
        Assert.AreEqual(CombatClockSnapshot.UnifiedProfile,copy.clock.profile);
        copy.version=1; Assert.IsTrue(RunContinuation.SupportsSnapshot(copy));
    }
    [Test] public void PendingMasteryAndReceiptCursorSurviveWithoutDoubleCredit()
    {
        var saved=Snapshot(); var tuning=new CrystalResonanceTuning();
        var receipt=new BoardProgressionReceipt(7,1,4,false,ResonanceMastery.DeliberateMatch);
        saved.resonance.Receive(receipt,tuning,out _);
        var copy=JsonUtility.FromJson<RunCombatSnapshot>(JsonUtility.ToJson(saved));
        Assert.IsTrue(RunContinuation.SupportsSnapshot(copy)); Assert.AreEqual(1,copy.resonance.PendingMastery);
        Assert.AreEqual(0,copy.resonance.charge); Assert.IsFalse(copy.resonance.Receive(receipt,tuning,out _));
        Assert.IsTrue(copy.resonance.Bank(1,5,tuning)); Assert.AreEqual(6,copy.resonance.charge);
        Assert.IsFalse(copy.resonance.Bank(1,5,tuning));
    }
    [Test] public void QueuedNotchesMustBeOrderedUniqueAndUnclaimed()
    {
        var saved=Snapshot();saved.resonance.Bank(1,80,new CrystalResonanceTuning());
        saved.resonance.activeNotch=12; Assert.IsTrue(RunContinuation.SupportsSnapshot(saved));
        saved.resonance.pendingNotches.Add(70); Assert.IsFalse(RunContinuation.SupportsSnapshot(saved));
        saved.resonance.pendingNotches.RemoveAt(3);saved.resonance.claimedMask=1;
        Assert.IsFalse(RunContinuation.SupportsSnapshot(saved));
        saved.resonance.pendingNotches.RemoveAt(0);saved.resonance.activeNotch=40;
        Assert.IsTrue(RunContinuation.SupportsSnapshot(saved));
    }
    [Test] public void SelectedCapstoneRoundTripPreservesIdentityAndSeed()
    {
        var saved=Snapshot();var state=saved.resonance;
        state.Bank(1,80,new CrystalResonanceTuning());
        state.capstonePhase=ResonanceCapstonePhase.Scheduled;state.capstoneId="Reserve";
        state.capstoneMembers.Add("dummy-member");state.capstoneKind=1;state.capstoneWave=2;
        state.capstoneEncounterId="fixture:0:2";state.formationSeed=44;
        var copy=JsonUtility.FromJson<RunCombatSnapshot>(JsonUtility.ToJson(saved));
        Assert.IsTrue(RunContinuation.SupportsSnapshot(copy));
        Assert.AreEqual(state.capstoneRandom,copy.resonance.capstoneRandom);
        Assert.AreEqual(state.approachThreshold,copy.resonance.approachThreshold);
        CollectionAssert.AreEqual(state.capstoneMembers,copy.resonance.capstoneMembers);
        copy.resonance.capstoneWave=3;Assert.IsFalse(RunContinuation.SupportsSnapshot(copy));
    }
    [Test] public void DetachedArrivalResetCannotEraseSourceOnFailedCommit()
    {
        var source=Snapshot();source.resonance.Bank(1,100,new CrystalResonanceTuning());
        var destination=JsonUtility.FromJson<RunCombatSnapshot>(JsonUtility.ToJson(source));
        destination.resonance.StartVisit("drowned-court",1,new CrystalResonanceTuning());
        Assert.AreEqual(100,source.resonance.charge);Assert.AreEqual(0,destination.resonance.charge);
        Assert.AreEqual(source.resonance.score,destination.resonance.score);
        Assert.AreEqual(source.resonance.lastReceipt,destination.resonance.lastReceipt);
    }
    [Test] public void FutureSchemaAndNonFiniteTelemetryFailClosed()
    {
        var saved=Snapshot();saved.resonance.version=2;Assert.IsFalse(RunContinuation.SupportsSnapshot(saved));
        saved.resonance.version=1;saved.resonance.runSeconds=float.NaN;
        Assert.IsFalse(RunContinuation.SupportsSnapshot(saved));
    }
}
#endif
