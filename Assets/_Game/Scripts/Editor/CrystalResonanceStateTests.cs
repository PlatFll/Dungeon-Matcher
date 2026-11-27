#if UNITY_EDITOR
using NUnit.Framework;

public sealed class CrystalResonanceStateTests
{
    private readonly CrystalResonanceTuning tuning = new CrystalResonanceTuning();
    private CrystalResonanceSnapshot NewState()
    {
        var state = new CrystalResonanceSnapshot { version = 1, capstoneRandom = 19 };
        state.StartVisit("dungeon", 0, tuning); state.BeginEncounter("test", 1); return state;
    }
    [Test] public void DuplicateReceiptsAndVictoriesCannotMultiplyRewards()
    {
        var state = NewState();
        var receipt = new BoardProgressionReceipt(1, 1, 3, false, ResonanceMastery.DeliberateMatch);
        Assert.IsTrue(state.Receive(receipt, tuning, out _));
        long score = state.score;
        Assert.IsFalse(state.Receive(receipt, tuning, out _)); Assert.AreEqual(score, state.score);
        Assert.AreEqual(0, state.charge); Assert.AreEqual(1, state.PendingMastery);
        Assert.IsTrue(state.Bank(1, 5, tuning)); Assert.AreEqual(6, state.charge);
        Assert.IsFalse(state.Bank(1, 5, tuning)); Assert.AreEqual(6, state.charge);
    }
    [Test] public void LongCascadeCannotFarmMasteryOrUnlimitedScore()
    {
        var state = NewState();
        for (int i = 1; i <= 100; i++) state.Receive(new BoardProgressionReceipt(i, 1, 10, false, ResonanceMastery.None), tuning, out _);
        Assert.AreEqual(0, state.PendingMastery); Assert.AreEqual(tuning.maximumBoardScorePerEncounter, state.score);
    }
    [Test] public void AllMasteryCapsAtThreeAndLossDoesNotBank()
    {
        var state = NewState();
        for (int i = 1; i <= 20; i++) state.Receive(new BoardProgressionReceipt(i, 1, 0, false, (ResonanceMastery)7), tuning, out _);
        Assert.AreEqual(3, state.PendingMastery); state.DiscardPending();
        Assert.IsFalse(state.Bank(1, 5, tuning)); Assert.AreEqual(0, state.charge);
    }
    [Test] public void OverflowQueuesOnlyThreeNotchesAndTravelPreservesScore()
    {
        var state = NewState(); state.Bank(1, 250, tuning);
        Assert.AreEqual(100, state.charge); CollectionAssert.AreEqual(new[] {12,40,70}, state.pendingNotches);
        long score = state.score; state.StartVisit("magical-forest", 1, tuning);
        Assert.AreEqual(score, state.score); Assert.AreEqual(0, state.charge);
        Assert.That(state.approachThreshold, Is.InRange(78,88));
    }
}
#endif
