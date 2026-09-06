using UnityEngine;

public readonly struct BombVFXContext
{
    public GemSpecialType SpecialType { get; }

    public Vector3 WorldPosition { get; }

    public float StartDelay { get; }

    public int Column { get; }

    public int Row { get; }

    public bool HasGridPosition { get; }

    public bool IsDirectionalBomb =>
        SpecialType == GemSpecialType.RowBomb ||
        SpecialType == GemSpecialType.ColumnBomb;

    public BombVFXContext(
        GemSpecialType specialType,
        Vector3 worldPosition,
        float startDelay)
        : this(
            specialType,
            worldPosition,
            startDelay,
            -1,
            -1,
            false
        )
    {
    }

    public BombVFXContext(
        GemSpecialType specialType,
        Vector3 worldPosition,
        float startDelay,
        int column,
        int row)
        : this(
            specialType,
            worldPosition,
            startDelay,
            column,
            row,
            true
        )
    {
    }

    private BombVFXContext(
        GemSpecialType specialType,
        Vector3 worldPosition,
        float startDelay,
        int column,
        int row,
        bool hasGridPosition)
    {
        SpecialType = specialType;
        WorldPosition = worldPosition;
        StartDelay = Mathf.Max(0f, startDelay);
        Column = column;
        Row = row;
        HasGridPosition = hasGridPosition;
    }
}
