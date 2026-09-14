using System;

/// <summary>Generation budget; does not store or spend the player's energy.</summary>
public sealed class AbilityRefundBudget
{
    public bool IsLimited { get; private set; }
    public int Remaining { get; private set; }

    public void Begin(int acceptedCost, float fraction)
    {
        IsLimited = fraction >= 0;
        Remaining = IsLimited ? Math.Max(0, (int)Math.Floor(acceptedCost * Math.Min(1f, fraction))) : 0;
    }

    public int Take(int requested)
    {
        int granted = Math.Max(0, requested);
        if (!IsLimited) return granted;
        granted = Math.Min(granted, Remaining);
        Remaining -= granted;
        return granted;
    }

    public void End() { IsLimited = false; Remaining = 0; }
}
