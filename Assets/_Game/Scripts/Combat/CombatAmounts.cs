using System;

/// <summary>Final combat resources use five-point steps; energy and board durability do not.</summary>
public static class CombatAmounts
{
    public const int Step = 5;

    // Round once after the relevant modifiers. A positive effect remains meaningful.
    public static int Round(double value)
    {
        if (double.IsNaN(value) || value <= 0) return 0;
        const int largestMultiple = int.MaxValue - int.MaxValue % Step;
        return (int)Math.Min(largestMultiple, Math.Max(Step, Math.Floor(value / Step + .5) * Step));
    }

    public static int Health(double value) => Math.Max(Step, Round(value));
}
