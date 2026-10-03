using System;

/// <summary>Value-only action identity. The board still decides which input is legal.</summary>
[Serializable]
public sealed class AcceptedMoveState
{
    public int completed;
    public int pending;
    public long nextActorId = 1;
    public bool IsPending => pending > completed;

    public bool Accept(int action)
    {
        if (action <= completed || IsPending) return false;
        if (action != completed + 1) throw new InvalidOperationException("Nonsequential accepted move.");
        pending = action;
        return true;
    }

    public bool Commit(int action)
    {
        if (action <= completed) return false;
        if (!IsPending || pending != action) throw new InvalidOperationException("Completion without acceptance.");
        completed = action;
        pending = 0;
        return true;
    }

    public long AllocateActor() => nextActorId++;
}

[Serializable]
public sealed class CombatClockSnapshot
{
    public const string MoveProfile = "accepted-moves-v1";
    public const string HybridProfile = "seconds-basics-move-abilities-v1";
    public const string LegacyEffectsProfile = "seconds-effects-move-abilities-v1";
    public static bool IsSupported(string value) => value == MoveProfile || value == HybridProfile || value == LegacyEffectsProfile;
    public string profile = MoveProfile;
    public string zoneId = "magical-forest";
    public int testEncounterOffset;
    public AcceptedMoveState actions = new AcceptedMoveState();
}
