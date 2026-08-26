using System.Collections.Generic;

public partial class BoardController
{
    private static bool IsChainReactiveBomb(
        GemSpecialType specialType)
    {
        return specialType ==
                   GemSpecialType.RowBomb ||
               specialType ==
                   GemSpecialType.ColumnBomb ||
               specialType ==
                   GemSpecialType.PoisonBomb;
    }

    private void AddPoisonBombAreaToClearSet(
        Gem poisonBomb,
        bool preserveTriggeredCrystals,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
        HashSet<Gem> gemsToClear,
        Queue<Gem> pendingBombs)
    {
        if (poisonBomb == null ||
            poisonBomb.SpecialType !=
                GemSpecialType.PoisonBomb)
        {
            return;
        }

        ApplyPoisonBombStatus();

        /*
         * Poison Bombs use a compact 3x3 blast centered on the
         * bomb. Every cell still enters the same authoritative
         * bomb-clear set as row/column bombs, so normal gem color
         * damage, obstacles and future chain reactions remain
         * owned by the existing board-resolution pipeline.
         */
        for (int rowOffset = -1;
             rowOffset <= 1;
             rowOffset++)
        {
            for (int columnOffset = -1;
                 columnOffset <= 1;
                 columnOffset++)
            {
                TryAddGemToBombClearSet(
                    poisonBomb.Column +
                        columnOffset,
                    poisonBomb.Row +
                        rowOffset,
                    poisonBomb,
                    preserveTriggeredCrystals,
                    triggeredCrystalRequests,
                    gemsToClear,
                    pendingBombs
                );
            }
        }
    }

    private void AddPoisonBombAreaToConvertedClearSet(
        Gem poisonBomb,
        Gem activatedBomb,
        HashSet<Gem> pendingConvertedBombs,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
        HashSet<Gem> gemsToClear,
        Queue<Gem> pendingBombs)
    {
        if (poisonBomb == null ||
            poisonBomb.SpecialType !=
                GemSpecialType.PoisonBomb)
        {
            return;
        }

        ApplyPoisonBombStatus();

        /*
         * Color-crystal conversion sequences deliberately protect
         * converted bombs until their own activation turn. Route
         * Poison Bomb cells through that same converted-bomb helper
         * so the established sequencing rules remain intact.
         */
        for (int rowOffset = -1;
             rowOffset <= 1;
             rowOffset++)
        {
            for (int columnOffset = -1;
                 columnOffset <= 1;
                 columnOffset++)
            {
                TryAddGemToConvertedBombClearSet(
                    poisonBomb.Column +
                        columnOffset,
                    poisonBomb.Row +
                        rowOffset,
                    activatedBomb,
                    pendingConvertedBombs,
                    triggeredCrystalRequests,
                    gemsToClear,
                    pendingBombs
                );
            }
        }
    }

    private void ApplyPoisonBombStatus()
    {
        if (combatController == null)
        {
            return;
        }

        combatController.ApplyPoisonToAllEnemies();
    }
}
