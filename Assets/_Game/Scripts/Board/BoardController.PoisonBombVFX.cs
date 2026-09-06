using UnityEngine;

public partial class BoardController
{
    /*
     * Presentation-only queries used by PoisonBombBurstVFX. The effect never
     * changes board state; it only asks whether a residue cell is still a valid
     * empty visual destination and whether the authoritative gem assigned to
     * that cell has visibly finished its fall.
     */
    internal bool IsCellEligibleForPoisonResidue(
        int column,
        int row)
    {
        return IsCellPlayable(column, row);
    }

    internal bool IsGemVisuallySettledInCell(
        int column,
        int row)
    {
        if (!IsCellPlayable(column, row))
        {
            return false;
        }

        Gem gem = GetGem(column, row);

        if (gem == null)
        {
            return false;
        }

        Vector3 targetPosition =
            GetLocalPosition(column, row);

        float settleTolerance =
            Mathf.Max(
                0.005f,
                cellSize * 0.025f
            );

        return
            (gem.transform.localPosition - targetPosition)
                .sqrMagnitude <=
            settleTolerance * settleTolerance;
    }
}
