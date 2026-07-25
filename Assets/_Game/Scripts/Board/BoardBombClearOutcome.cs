public readonly struct BoardBombClearOutcome
{
    public GemType GemType { get; }

    public int GemCount { get; }

    public int CascadeDepth { get; }

    public bool DamagedMatchingEnemy { get; }

    public BoardBombClearOutcome(
        GemType gemType,
        int gemCount,
        int cascadeDepth,
        bool damagedMatchingEnemy)
    {
        GemType =
            gemType;

        GemCount =
            gemCount < 0
                ? 0
                : gemCount;

        CascadeDepth =
            cascadeDepth < 0
                ? 0
                : cascadeDepth;

        DamagedMatchingEnemy =
            damagedMatchingEnemy;
    }
}