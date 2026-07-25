using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private List<SpecialGemCreationRequest>
        BuildSpecialGemCreationRequests(
            HashSet<Gem> matches,
            Gem preferredGem,
            Gem fallbackGem)
    {
        List<SpecialGemCreationRequest> requests =
            new List<SpecialGemCreationRequest>();

        if (matches == null ||
            matches.Count < 4)
        {
            return requests;
        }

        List<List<Gem>> matchGroups =
            BuildConnectedMatchGroups(matches);

        foreach (List<Gem> group in matchGroups)
        {
            BoardMatchType matchType =
                DetermineMatchType(group);

            GemSpecialType specialType =
                GemSpecialType.None;

            switch (matchType)
            {
                case BoardMatchType.StraightFour:
                    specialType =
                        GetStraightFourSpecialType(
                            group
                        );

                    break;

                case BoardMatchType.StraightFive:
                case BoardMatchType.LShape:
                case BoardMatchType.TShape:
                    specialType =
                        GemSpecialType.ColorCrystal;

                    break;
            }

            if (specialType ==
                GemSpecialType.None)
            {
                continue;
            }

            Gem gemToPreserve =
                SelectGemToPreserve(
                    group,
                    preferredGem,
                    fallbackGem
                );

            if (gemToPreserve == null)
            {
                continue;
            }

            requests.Add(
                new SpecialGemCreationRequest(
                    gemToPreserve,
                    specialType
                )
            );
        }

        return requests;
    }

    private static Gem SelectGemToPreserve(
        List<Gem> group,
        Gem preferredGem,
        Gem fallbackGem)
    {
        if (group == null ||
            group.Count == 0)
        {
            return null;
        }

        /*
         * For a player-created match, prioritize
         * the gem the player moved.
         */
        if (preferredGem != null &&
            group.Contains(preferredGem))
        {
            return preferredGem;
        }

        /*
         * The other swapped gem is used when it
         * is the one belonging to the four-match.
         */
        if (fallbackGem != null &&
            group.Contains(fallbackGem))
        {
            return fallbackGem;
        }

        /*
         * Cascade-created four-matches have no
         * moved gem, so preserve a central gem.
         */
        int automaticIndex =
            (group.Count - 1) / 2;

        return group[automaticIndex];
    }

    private static GemSpecialType
        GetStraightFourSpecialType(
            List<Gem> group)
    {
        if (group == null ||
            group.Count != 4 ||
            group[0] == null)
        {
            return GemSpecialType.None;
        }

        int firstRow =
            group[0].Row;

        int firstColumn =
            group[0].Column;

        bool allSameRow = true;
        bool allSameColumn = true;

        for (int index = 1;
             index < group.Count;
             index++)
        {
            Gem gem =
                group[index];

            if (gem == null)
            {
                return GemSpecialType.None;
            }

            if (gem.Row != firstRow)
            {
                allSameRow = false;
            }

            if (gem.Column != firstColumn)
            {
                allSameColumn = false;
            }
        }

        if (allSameRow)
        {
            return GemSpecialType.RowBomb;
        }

        if (allSameColumn)
        {
            return GemSpecialType.ColumnBomb;
        }

        return GemSpecialType.None;
    }

    private bool TryBuildColorCrystalClearSet(
        Gem first,
        Gem second,
        out HashSet<Gem> gemsToClear,
        out GemType targetGemType,
        out GemSpecialType targetSpecialType)
    {
        gemsToClear =
            new HashSet<Gem>();

        targetGemType =
            default(GemType);

        targetSpecialType =
            GemSpecialType.None;

        if (first == null ||
            second == null)
        {
            return false;
        }

        bool firstIsCrystal =
            first.SpecialType ==
            GemSpecialType.ColorCrystal;

        bool secondIsCrystal =
            second.SpecialType ==
            GemSpecialType.ColorCrystal;

        /*
         * Exactly one of the swapped gems must currently
         * be a color crystal.
         *
         * Crystal + crystal will be handled separately later.
         */
        if (firstIsCrystal ==
            secondIsCrystal)
        {
            return false;
        }

        Gem crystalGem =
            firstIsCrystal
                ? first
                : second;

        Gem targetGem =
            firstIsCrystal
                ? second
                : first;

        targetGemType =
            targetGem.Type;

        targetSpecialType =
            targetGem.SpecialType;

        /*
         * The activated crystal always destroys itself.
         */
        gemsToClear.Add(
            crystalGem
        );

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                Gem gem =
                    GetGem(
                        column,
                        row
                    );

                if (gem == null ||
                    gem == crystalGem ||
                    gem.Type != targetGemType)
                {
                    continue;
                }

                /*
                 * Other crystals do not count as colored gems.
                 * Row and column bombs of the selected color
                 * are included and can chain-react later.
                 */
                if (gem.SpecialType ==
                    GemSpecialType.ColorCrystal)
                {
                    continue;
                }

                gemsToClear.Add(
                    gem
                );
            }
        }

        return gemsToClear.Count > 1;
    }

    private static List<Gem>
        BuildOrderedCrystalTargets(
            HashSet<Gem> crystalClearSet,
            out Gem crystalGem)
    {
        crystalGem = null;

        List<Gem> orderedTargets =
            new List<Gem>();

        if (crystalClearSet == null)
        {
            return orderedTargets;
        }

        foreach (Gem gem in crystalClearSet)
        {
            if (gem == null)
            {
                continue;
            }

            if (gem.SpecialType ==
                GemSpecialType.ColorCrystal)
            {
                crystalGem = gem;
                continue;
            }

            orderedTargets.Add(gem);
        }

        if (crystalGem == null)
        {
            return orderedTargets;
        }

        Gem sequenceOrigin =
            crystalGem;

        orderedTargets.Sort(
            (left, right) =>
            {
                int leftDistance =
                    Mathf.Abs(
                        left.Column -
                        sequenceOrigin.Column
                    ) +
                    Mathf.Abs(
                        left.Row -
                        sequenceOrigin.Row
                    );

                int rightDistance =
                    Mathf.Abs(
                        right.Column -
                        sequenceOrigin.Column
                    ) +
                    Mathf.Abs(
                        right.Row -
                        sequenceOrigin.Row
                    );

                int distanceComparison =
                    leftDistance.CompareTo(
                        rightDistance
                    );

                if (distanceComparison != 0)
                {
                    return distanceComparison;
                }

                int rowComparison =
                    left.Row.CompareTo(
                        right.Row
                    );

                if (rowComparison != 0)
                {
                    return rowComparison;
                }

                return left.Column.CompareTo(
                    right.Column
                );
            }
        );

        return orderedTargets;
    }

    private IEnumerator ResolveColorCrystalActivation(
        HashSet<Gem> crystalClearSet,
        GemType targetGemType,
        GemSpecialType targetSpecialType)
    {
        if (crystalClearSet == null ||
            crystalClearSet.Count == 0)
        {
            yield break;
        }

        /*
         * Selected-color bombs are included in the initial
         * set. Expanding it here allows them to chain-react.
         */
        HashSet<Gem> expandedClearSet =
            BuildBombExpandedClearSet(
                crystalClearSet
            );

        /*
         * The crystal has an underlying GemType from the
         * match that created it, but it should not grant
         * damage or energy for that hidden color.
         */
        HashSet<Gem> rewardExclusions =
            new HashSet<Gem>();

        foreach (Gem gem in crystalClearSet)
        {
            if (gem != null &&
                gem.SpecialType ==
                    GemSpecialType.ColorCrystal)
            {
                rewardExclusions.Add(gem);
            }
        }

        /*
         * All actual colored gems use the existing
         * per-cleared-gem damage and energy rules.
         */
        ReportBombClearsToCombat(
            rewardExclusions,
            expandedClearSet,
            0
        );

        Debug.Log(
            $"Color crystal clearing " +
            $"{targetGemType} gems. " +
            $"{expandedClearSet.Count} total gems " +
            $"will be destroyed."
        );

        yield return ClearMatches(
            expandedClearSet,
            null
        );

        if (cascadePause > 0f)
        {
            yield return new WaitForSeconds(
                cascadePause
            );
        }

        yield return CollapseAndRefillBoard();

        if (cascadePause > 0f)
        {
            yield return new WaitForSeconds(
                cascadePause
            );
        }

        HashSet<Gem> resultingMatches =
            FindAllMatches();

        if (resultingMatches.Count > 0)
        {
            yield return ResolveCascades(
                resultingMatches,
                null,
                null
            );
        }
        else if (!HasAvailableMove())
        {
            yield return ReshuffleBoard();
        }

        Debug.Log(
            "Color crystal activation complete."
        );
    }

    private HashSet<Gem> BuildBombExpandedClearSet(
        HashSet<Gem> matchedGems)
    {
        HashSet<Gem> gemsToClear =
            new HashSet<Gem>();

        Queue<Gem> pendingBombs =
            new Queue<Gem>();

        HashSet<Gem> triggeredBombs =
            new HashSet<Gem>();

        if (matchedGems == null)
        {
            return gemsToClear;
        }

        foreach (Gem gem in matchedGems)
        {
            if (gem == null)
            {
                continue;
            }

            gemsToClear.Add(gem);

            if (gem.SpecialType !=
                GemSpecialType.None)
            {
                pendingBombs.Enqueue(gem);
            }
        }

        while (pendingBombs.Count > 0)
        {
            Gem bomb =
                pendingBombs.Dequeue();

            if (bomb == null ||
                !triggeredBombs.Add(bomb))
            {
                continue;
            }

            switch (bomb.SpecialType)
            {
                case GemSpecialType.RowBomb:
                    for (int column = 0;
                         column < width;
                         column++)
                    {
                        TryAddGemToBombClearSet(
                            column,
                            bomb.Row,
                            gemsToClear,
                            pendingBombs
                        );
                    }

                    break;

                case GemSpecialType.ColumnBomb:
                    for (int row = 0;
                         row < height;
                         row++)
                    {
                        TryAddGemToBombClearSet(
                            bomb.Column,
                            row,
                            gemsToClear,
                            pendingBombs
                        );
                    }

                    break;
            }
        }

        return gemsToClear;
    }

    private void TryAddGemToBombClearSet(
        int column,
        int row,
        HashSet<Gem> gemsToClear,
        Queue<Gem> pendingBombs)
    {
        Gem gem =
            GetGem(
                column,
                row
            );

        if (gem == null)
        {
            return;
        }

        bool wasAdded =
            gemsToClear.Add(gem);

        if (wasAdded &&
            gem.SpecialType !=
                GemSpecialType.None)
        {
            pendingBombs.Enqueue(gem);
        }
    }
}