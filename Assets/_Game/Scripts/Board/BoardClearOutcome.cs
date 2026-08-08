public readonly struct BoardClearOutcome
{
    public BoardClearContext ClearContext
    {
        get;
    }

    public bool DamagedMatchingEnemy
    {
        get;
    }

    public BoardClearOutcome(
        BoardClearContext clearContext,
        bool damagedMatchingEnemy)
    {
        ClearContext =
            clearContext;

        DamagedMatchingEnemy =
            damagedMatchingEnemy;
    }
}