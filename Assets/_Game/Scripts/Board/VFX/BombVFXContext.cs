using UnityEngine;

public readonly struct BombVFXContext
{
    public GemSpecialType SpecialType { get; }

    public Vector3 WorldPosition { get; }

    public float StartDelay { get; }

    public bool IsDirectionalBomb =>
        SpecialType == GemSpecialType.RowBomb ||
        SpecialType == GemSpecialType.ColumnBomb;

    public BombVFXContext(
        GemSpecialType specialType,
        Vector3 worldPosition,
        float startDelay)
    {
        SpecialType = specialType;
        WorldPosition = worldPosition;
        StartDelay = Mathf.Max(0f, startDelay);
    }
}
