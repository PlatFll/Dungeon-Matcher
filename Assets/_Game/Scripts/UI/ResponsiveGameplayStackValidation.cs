using UnityEngine;

internal static class ResponsiveGameplayStackValidation
{
    internal static bool IsFinitePositive(
        float value)
    {
        return
            !float.IsNaN(value) &&
            !float.IsInfinity(value) &&
            value > 0f;
    }

    internal static bool IsValidRectSize(
        Vector2 size)
    {
        return
            IsFinitePositive(size.x) &&
            IsFinitePositive(size.y);
    }
}
