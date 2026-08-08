using UnityEngine;

public readonly struct BoardClearContext
{
    public GemType GemType { get; }

    /*
     * Number of rewardable colored gems that were
     * genuinely destroyed.
     */
    public int GemCount { get; }

    /*
     * Number of gems involved in the action that caused
     * this clear.
     *
     * Example:
     * straight-four match:
     * TriggerGemCount = 4
     * GemCount = 3 because one gem survives as a bomb.
     */
    public int TriggerGemCount { get; }

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
            BoardMatchType.Other,
        int triggerGemCount = -1)
    {
        GemType =
            gemType;

        GemCount =
            Mathf.Max(
                0,
                gemCount
            );

        TriggerGemCount =
            triggerGemCount < 0
                ? GemCount
                : Mathf.Max(
                    0,
                    triggerGemCount
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