using System.Collections.Generic;

public partial class BoardController
{
    /*
     * ClearMatches makes cleared grid slots authoritative before its short
     * visual flash finishes. That gives this board-owned coordinator a safe
     * window to move standards through exactly the gravity slots opened below
     * them before CollapseAndRefillBoard compacts the remaining gems.
     *
     * RoyalBannerState.MoveRoutine is also the one-frame/multi-frame guard: a
     * banner prepared for the current clear is not advanced again while the
     * same unresolved gap is still visible in the grid.
     */
    private void Update()
    {
        if (gems == null ||
            royalBannerCells.Count == 0)
        {
            return;
        }

        List<RoyalBannerState> activeBanners =
            new List<RoyalBannerState>(
                royalBannerCells.Values
            );

        /*
         * Lower standards move first. This keeps the behavior deterministic if
         * future encounter rules ever permit more than one banner in a column.
         */
        activeBanners.Sort(
            (left, right) =>
            {
                if (left == null)
                {
                    return right == null ? 0 : 1;
                }

                if (right == null)
                {
                    return -1;
                }

                return left.Cell.y.CompareTo(
                    right.Cell.y
                );
            }
        );

        foreach (RoyalBannerState state
                 in activeBanners)
        {
            if (state == null ||
                state.ReachedBottom ||
                state.MoveRoutine != null)
            {
                continue;
            }

            int openedGravitySlots =
                CountOpenRoyalBannerGravitySlotsBelow(
                    state
                );

            for (int step = 0;
                 step < openedGravitySlots &&
                 !state.ReachedBottom;
                 step++)
            {
                AdvanceRoyalBannerOneGravitySlot(
                    state
                );
            }
        }
    }

    private int CountOpenRoyalBannerGravitySlotsBelow(
        RoyalBannerState state)
    {
        if (state == null ||
            state.ReachedBottom)
        {
            return 0;
        }

        int count = 0;
        int column = state.Cell.x;

        for (int row = 0;
             row < state.Cell.y;
             row++)
        {
            if (!IsStructurallyOpenForRoyalBannerGravity(
                    column,
                    row) ||
                IsCellRoyalBanner(
                    column,
                    row))
            {
                continue;
            }

            Gem gem = GetGem(column, row);

            /*
             * Fixed bolts and Court Mage freezes are not gravity destinations.
             * They therefore do not count as an open slot and other gems (and
             * the banner's gravity ordering) pass through their height.
             */
            if (gem != null &&
                IsGemFixedByPin(gem))
            {
                continue;
            }

            if (gem == null)
            {
                count++;
            }
        }

        return count;
    }
}
