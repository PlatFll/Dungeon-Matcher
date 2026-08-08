using UnityEngine;

public readonly struct BoardClearContext
{
    public GemType GemType { get; }

    public int GemCount { get; }

    public int CascadeDepth { get; }

    public BoardClearSource Source { get; }

    public BoardMatchType MatchType { get; }

    public bool IsMatchClear =>
        Source == BoardClearSource.Match;

    public bool IsSpecialClear =>
        Source == BoardClearSource.Bomb ||
        Source == BoardClearSource.ColorCrystal ||
        Source == BoardClearSource.DoubleColorCrystal;

    public BoardClearContext(
        GemType gemType,
        int gemCount,
        int cascadeDepth,
        BoardClearSource source,
        BoardMatchType matchType =
            BoardMatchType.Other)
    {
        GemType =
            gemType;

        GemCount =
            Mathf.Max(
                0,
                gemCount
            );

        CascadeDepth =
            Mathf.Max(
                0,
                cascadeDepth
            );

        Source =
            source;

        MatchType =
            matchType;
    }
}