using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    /*
     * Physical gem destruction is the common denominator for every clear path:
     * normal matches, cascades, bombs, color crystals, abilities, and enemy
     * board mutations all eventually destroy a Gem. A Crossbow Guard chain is
     * therefore released whenever one of those destroyed gems occupied the
     * pinned gem's own cell or an orthogonally adjacent cell.
     *
     * The existing match-time pin release remains harmless and makes ordinary
     * matches react slightly earlier; by the time this notification arrives
     * that pin has already been removed and cannot be processed twice.
     */
    internal void NotifyGemDestroyedForPins(
        Gem destroyedGem)
    {
        if (ReferenceEquals(
                destroyedGem,
                null) ||
            pinnedGemOwners.Count == 0)
        {
            return;
        }

        int destroyedColumn =
            destroyedGem.Column;

        int destroyedRow =
            destroyedGem.Row;

        List<Gem> adjacentPinsToRelease =
            new List<Gem>();

        bool destroyedGemWasPinned = false;

        foreach (
            KeyValuePair<Gem, int> pin
            in pinnedGemOwners)
        {
            Gem pinnedGem =
                pin.Key;

            if (ReferenceEquals(
                    pinnedGem,
                    null))
            {
                continue;
            }

            /*
             * If the pinned gem itself is being destroyed, its GameObject and
             * overlay are already on their way out. Only forget ownership;
             * trying to play a release visual on a destroying object is both
             * unnecessary and vulnerable to Unity component-destruction order.
             */
            if (ReferenceEquals(
                    pinnedGem,
                    destroyedGem))
            {
                destroyedGemWasPinned = true;
                continue;
            }

            int orthogonalDistance =
                Mathf.Abs(
                    pinnedGem.Column -
                    destroyedColumn
                ) +
                Mathf.Abs(
                    pinnedGem.Row -
                    destroyedRow
                );

            if (orthogonalDistance == 1 && !movablePinnedGems.Contains(pinnedGem))
            {
                adjacentPinsToRelease.Add(
                    pinnedGem
                );
            }
        }

        if (destroyedGemWasPinned)
        {
            pinnedGemOwners.Remove(
                destroyedGem
            );
        }

        foreach (Gem pinnedGem
                 in adjacentPinsToRelease)
        {
            ReleasePinInternal(
                pinnedGem
            );
        }

        CleanupDestroyedPinEntries();
    }
}
