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

    // Explicitly opted-in player explosions retain Ability source attribution.
    public bool GrantsSpecialEnergy { get; }

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
        int triggerGemCount = -1,
        bool grantsSpecialEnergy = false)
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

        GrantsSpecialEnergy = grantsSpecialEnergy ||
            source == BoardClearSource.Bomb ||
            source == BoardClearSource.ColorCrystal ||
            source == BoardClearSource.DoubleColorCrystal;

        MatchType =
            matchType;
    }
}
