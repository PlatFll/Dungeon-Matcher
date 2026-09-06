using System;
using System.Collections.Generic;

public readonly struct RunUpgradeSpecialClearContext
{
    public int SpecialCount { get; }
    public int DirectionalBombCount { get; }

    public RunUpgradeSpecialClearContext(
        int specialCount,
        int directionalBombCount)
    {
        SpecialCount = Math.Max(0, specialCount);
        DirectionalBombCount = Math.Max(0, directionalBombCount);
    }
}

public partial class BoardController
{
    public event Action<RunUpgradeSpecialClearContext>
        RunUpgradeSpecialClearPrepared;

    public event Action RunUpgradeSpecialClearFinished;

    private void ReportRunUpgradeSpecialClearPrepared(
        HashSet<Gem> expandedClearSet)
    {
        int specialCount = 0;
        int directionalBombCount = 0;

        if (expandedClearSet != null)
        {
            foreach (Gem gem in expandedClearSet)
            {
                if (gem == null || !IsChainReactiveBomb(gem.SpecialType))
                {
                    continue;
                }

                specialCount++;

                if (gem.SpecialType == GemSpecialType.RowBomb ||
                    gem.SpecialType == GemSpecialType.ColumnBomb)
                {
                    directionalBombCount++;
                }
            }
        }

        RunUpgradeSpecialClearPrepared?.Invoke(
            new RunUpgradeSpecialClearContext(
                specialCount,
                directionalBombCount
            )
        );
    }

    private void ReportRunUpgradeSpecialClearFinished()
    {
        RunUpgradeSpecialClearFinished?.Invoke();
    }
}
