using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    /*
     * Physical gem destruction is the common denominator for every clear path:
     * normal matches, cascades, bombs, color crystals, abilities, and enemy
     * board mutations all eventually destroy a Gem. This one notification is
     * therefore also the deterministic signal used by Royal Standard gravity.
     *
     * Crossbow Guard bolts additionally break when an orthogonally adjacent gem
     * is physically destroyed. The existing match-time pin release remains
     * harmless and makes ordinary matches react slightly earlier; by the time
     * this notification arrives that bolt has already been removed and cannot
     * be processed twice.
     */
    internal void NotifyGemDestroyedForPins(
        Gem destroyedGem)
    {
        if (ReferenceEquals(
                destroyedGem,
                null))
        {
            return;
        }

        int destroyedColumn =
            destroyedGem.Column;

        int destroyedRow =
            destroyedGem.Row;

        /*
         * Royal standards react to actual cleared gem identities, not to an
         * Update-time scan of transient null cells. Structural exclusions in
         * the banner runtime make mined holes and reserved barricades remain
         * non-destinations exactly like normal gravity.
         */
        NotifyGemDestroyedForRoyalBanners(
            destroyedColumn,
            destroyedRow
        );

        if (pinnedGemOwners.Count == 0)
        {
            return;
        }

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

            /*
             * Captain chains move with their gem and Court Mage freezes are
             * intentionally not broken by neighboring destruction. Only the
             * Crossbow Guard's fixed bolt uses the orthogonal-clear break rule.
             */
            if (movablePinnedGems.Contains(pinnedGem) ||
                frozenPinnedGems.Contains(pinnedGem))
            {
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

            if (orthogonalDistance == 1)
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
