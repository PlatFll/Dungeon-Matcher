public readonly struct BoardMatchOutcome
{
    public BoardMatchContext MatchContext { get; }

    public bool DamagedMatchingEnemy { get; }

    public BoardMatchOutcome(
        BoardMatchContext matchContext,
        bool damagedMatchingEnemy)
    {
        MatchContext = matchContext;
        DamagedMatchingEnemy =
            damagedMatchingEnemy;
    }
}