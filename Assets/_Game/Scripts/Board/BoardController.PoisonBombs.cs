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
                   GemSpecialType.PoisonBomb ||
               specialType ==
                   GemSpecialType.HealingBomb ||
               specialType ==
                   GemSpecialType.ShieldBomb;
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

        AddSpecialBombAreaToClearSet(
            poisonBomb,
            preserveTriggeredCrystals,
            triggeredCrystalRequests,
            gemsToClear,
            pendingBombs
        );
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

        AddSpecialBombAreaToConvertedClearSet(
            poisonBomb,
            activatedBomb,
            pendingConvertedBombs,
            triggeredCrystalRequests,
            gemsToClear,
            pendingBombs
        );
    }

    private void AddHealingBombAreaToClearSet(
        Gem healingBomb,
        bool preserveTriggeredCrystals,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
        HashSet<Gem> gemsToClear,
        Queue<Gem> pendingBombs)
    {
        if (healingBomb == null ||
            healingBomb.SpecialType !=
                GemSpecialType.HealingBomb)
        {
            return;
        }

        ApplyHealingBombEffect();

        AddSpecialBombAreaToClearSet(
            healingBomb,
            preserveTriggeredCrystals,
            triggeredCrystalRequests,
            gemsToClear,
            pendingBombs
        );
    }

    private void AddHealingBombAreaToConvertedClearSet(
        Gem healingBomb,
        Gem activatedBomb,
        HashSet<Gem> pendingConvertedBombs,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
        HashSet<Gem> gemsToClear,
        Queue<Gem> pendingBombs)
    {
        if (healingBomb == null ||
            healingBomb.SpecialType !=
                GemSpecialType.HealingBomb)
        {
            return;
        }

        ApplyHealingBombEffect();

        AddSpecialBombAreaToConvertedClearSet(
            healingBomb,
            activatedBomb,
            pendingConvertedBombs,
            triggeredCrystalRequests,
            gemsToClear,
            pendingBombs
        );
    }

    private void AddShieldBombAreaToClearSet(
        Gem shieldBomb,
        bool preserveTriggeredCrystals,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
        HashSet<Gem> gemsToClear,
        Queue<Gem> pendingBombs)
    {
        if (shieldBomb == null ||
            shieldBomb.SpecialType !=
                GemSpecialType.ShieldBomb)
        {
            return;
        }

        ApplyShieldBombEffect();

        AddSpecialBombAreaToClearSet(
            shieldBomb,
            preserveTriggeredCrystals,
            triggeredCrystalRequests,
            gemsToClear,
            pendingBombs
        );
    }

    private void AddShieldBombAreaToConvertedClearSet(
        Gem shieldBomb,
        Gem activatedBomb,
        HashSet<Gem> pendingConvertedBombs,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
        HashSet<Gem> gemsToClear,
        Queue<Gem> pendingBombs)
    {
        if (shieldBomb == null ||
            shieldBomb.SpecialType !=
                GemSpecialType.ShieldBomb)
        {
            return;
        }

        ApplyShieldBombEffect();

        AddSpecialBombAreaToConvertedClearSet(
            shieldBomb,
            activatedBomb,
            pendingConvertedBombs,
            triggeredCrystalRequests,
            gemsToClear,
            pendingBombs
        );
    }

    private void AddSpecialBombAreaToClearSet(
        Gem bomb,
        bool preserveTriggeredCrystals,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
        HashSet<Gem> gemsToClear,
        Queue<Gem> pendingBombs)
    {
        if (bomb == null)
        {
            return;
        }

        /*
         * Socket-style special bombs share one compact centered 3x3 blast.
         * Every affected cell still enters the authoritative bomb-clear set,
         * so color damage, obstacles, crystals and chain reactions keep using
         * the same deterministic resolution pipeline.
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
                    bomb.Column + columnOffset,
                    bomb.Row + rowOffset,
                    bomb,
                    preserveTriggeredCrystals,
                    triggeredCrystalRequests,
                    gemsToClear,
                    pendingBombs
                );
            }
        }
    }

    private void AddSpecialBombAreaToConvertedClearSet(
        Gem bomb,
        Gem activatedBomb,
        HashSet<Gem> pendingConvertedBombs,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
        HashSet<Gem> gemsToClear,
        Queue<Gem> pendingBombs)
    {
        if (bomb == null)
        {
            return;
        }

        /*
         * Color-crystal conversion sequences protect converted bombs until
         * their own activation turn. All socket-style bombs route through the
         * same converted-bomb helper so that sequencing rule stays intact.
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
                    bomb.Column + columnOffset,
                    bomb.Row + rowOffset,
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

    private void ApplyHealingBombEffect()
    {
        if (combatController == null)
        {
            return;
        }

        combatController.HealPlayerFromBomb();
    }

    private void ApplyShieldBombEffect()
    {
        if (combatController == null)
        {
            return;
        }

        combatController.GrantPlayerShieldFromBomb();
    }
}
