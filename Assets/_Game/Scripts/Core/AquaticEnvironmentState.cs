using System;
using System.Collections.Generic;

public enum TidePhase { Dry, Pending, Flooded }

// Persisted rules, independent of water animation and wall-clock time. The
// board owns the instance; only the accepted-action coordinator advances it.
[Serializable]
public sealed class AquaticEnvironmentState
{
    public const int CurrentVersion = 2;
    public int version = CurrentVersion;
    public TidePhase phase;
    public int air, wetMoves, dryMoves, nextSupplyMove, lastSettledMove;
    public int floodCount;
    public int dryEncounter = -1, protectedThroughEncounter = 1;
    public int acceptedMove, acceptedAir, collectedAir, snareLoss;
    public bool acceptedWet;
    public bool reserveExhausted;
    public List<int> collectedAmounts = new List<int>();
    public List<int> bubbles = new List<int>();
    public List<AquaticSnareState> snares = new List<AquaticSnareState>();
    public AquaticCofferState coffer;
    public int nextCofferId = 1;

    public void EncounterStarted(int encounter)
    {
        if (dryEncounter == -1) dryEncounter = encounter;
    }

    public bool CanFlood(int encounter, bool completeFormationCompatible) =>
        phase == TidePhase.Pending && encounter > protectedThroughEncounter && completeFormationCompatible;

    public void StartFlood(int duration, int move)
    {
        phase = TidePhase.Flooded;
        floodCount++;
        air = 5;
        wetMoves = Math.Max(1, duration);
        reserveExhausted = false;
        nextSupplyMove = 0;
        dryMoves = 0;
    }

    public void Accept(int move)
    {
        if (move <= lastSettledMove || acceptedMove == move) return;
        acceptedMove = move;
        acceptedWet = phase == TidePhase.Flooded;
        acceptedAir = air;
        collectedAir = snareLoss = 0;
        collectedAmounts.Clear();
    }

    public void Collect(int blocks)
    {
        if (phase != TidePhase.Flooded || blocks <= 0) return;
        if (acceptedWet && acceptedMove > lastSettledMove)
        { collectedAir += blocks; collectedAmounts.Add(blocks); }
        else air = ClampAir(air + blocks);
    }

    public void SnaredManualClear()
    {
        if (acceptedWet && acceptedMove > lastSettledMove) snareLoss++;
    }

    // Returns true only for a final wet move, before any suffocation decision.
    public bool Settle(int move, int encounter)
    {
        if (move <= lastSettledMove) return false;
        if (acceptedMove != move) throw new InvalidOperationException("Aquatic move was not accepted.");
        lastSettledMove = move;
        if (!acceptedWet)
        {
            if (phase == TidePhase.Dry && ++dryMoves >= 6) phase = TidePhase.Pending;
            return false;
        }
        air = ClampAir(Math.Max(0, acceptedAir - 1 - snareLoss) + collectedAir);
        wetMoves = Math.Max(0, wetMoves - 1);
        if (wetMoves != 0) return false;
        Drain(encounter);
        return true;
    }

    public void Drain(int encounter)
    {
        phase = TidePhase.Dry;
        air = wetMoves = dryMoves = 0;
        reserveExhausted = false; nextSupplyMove = 0;
        bubbles.Clear();
        coffer = null;
        // The next complete encounter is dry; the current remainder does not count.
        protectedThroughEncounter = encounter + 1;
    }

    public bool NeedsSuffocation(int move) => acceptedWet && lastSettledMove == move &&
        phase == TidePhase.Flooded && air == 0;

    private static int ClampAir(int amount) => Math.Max(0, Math.Min(5, amount));
}

[Serializable]
public sealed class AquaticSnareState
{
    public int gemId, expiresMove;
    public long ownerId;
}

[Serializable]
public sealed class AquaticCofferState
{
    public int id, x, y, charges;
    public long ownerId;
}
