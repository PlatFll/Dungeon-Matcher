using NUnit.Framework;
using UnityEngine;

public sealed class AquaticEnvironmentStateTests
{
    private static AquaticEnvironmentState Wet(int air = 5, int duration = 10)
    {
        var state = new AquaticEnvironmentState();
        state.StartFlood(duration, 0); state.air = air;
        return state;
    }

    [Test] public void DebitAndSnareClampBeforeCollectedAir()
    {
        var s = Wet(1); s.Accept(1); s.SnaredManualClear(); s.SnaredManualClear();
        s.Collect(2); s.Settle(1, 2);
        Assert.That(s.air, Is.EqualTo(2));
        Assert.That(s.NeedsSuffocation(1), Is.False);
    }
    [Test] public void FinalWetMoveDrainsBeforeSuffocationAndProtectsNextEncounter()
    {
        var s = Wet(0); s.wetMoves = 1; s.Accept(1);
        Assert.That(s.Settle(1, 4), Is.True);
        Assert.That(s.phase, Is.EqualTo(TidePhase.Dry));
        Assert.That(s.NeedsSuffocation(1), Is.False);
        s.phase = TidePhase.Pending;
        Assert.That(s.CanFlood(5, true), Is.False);
        Assert.That(s.CanFlood(6, false), Is.False);
        Assert.That(s.CanFlood(6, true), Is.True);
    }
    [Test] public void DryAcceptanceCannotChargeTheNewFlood()
    {
        var s = new AquaticEnvironmentState(); s.Accept(1); s.Settle(1, 2);
        s.StartFlood(11, 1);
        Assert.That(s.air, Is.EqualTo(5)); Assert.That(s.wetMoves, Is.EqualTo(11));
        Assert.That(s.NeedsSuffocation(1), Is.False);
    }
    [Test] public void FreeCollectionDoesNotAdvanceTideOrSpendAir()
    {
        var s = Wet(1); s.Collect(2); s.Collect(2);
        Assert.That(s.air, Is.EqualTo(5)); Assert.That(s.wetMoves, Is.EqualTo(10));
        Assert.That(s.lastSettledMove, Is.Zero);
    }
    [Test] public void DuplicateSettlementAndAcceptanceCannotDoubleCharge()
    {
        var s = Wet(); s.Accept(1); s.Collect(2); s.Accept(1);
        s.Settle(1, 2); string snapshot = JsonUtility.ToJson(s); s.Settle(1, 2); s.Accept(1);
        Assert.That(JsonUtility.ToJson(s), Is.EqualTo(snapshot));
    }
    [Test] public void ResumePreservesPendingReceiptsAndUnits()
    {
        var s = Wet(2, 12); s.Accept(3); s.Collect(2); s.SnaredManualClear();
        var resumed = JsonUtility.FromJson<AquaticEnvironmentState>(JsonUtility.ToJson(s));
        s.Settle(3, 2); resumed.Settle(3, 2);
        Assert.That(JsonUtility.ToJson(resumed), Is.EqualTo(JsonUtility.ToJson(s)));
        Assert.That(resumed.air, Is.EqualTo(2)); Assert.That(resumed.wetMoves, Is.EqualTo(11));
    }
    [Test] public void FullDryFormationAndSixAcceptedMovesAreRequired()
    {
        var s = new AquaticEnvironmentState();
        for (int i = 1; i <= 6; i++) { s.Accept(i); s.Settle(i, 1); }
        Assert.That(s.phase, Is.EqualTo(TidePhase.Pending));
        Assert.That(s.CanFlood(1, true), Is.False);
        Assert.That(s.CanFlood(2, false), Is.False);
        Assert.That(s.CanFlood(2, true), Is.True);
    }
}
