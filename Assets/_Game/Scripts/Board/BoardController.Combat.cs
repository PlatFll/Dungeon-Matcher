using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    public bool IsBusy =>
        isBusy ||
        HasPendingBoardMutation;

    [Header("Combat")]
    [SerializeField]
    [Tooltip(
        "Receives real board matches and converts them " +
        "into damage against enemies."
    )]
    private CombatController combatController;

    /*
     * Unified events used by all new gameplay systems.
     */
    public event Action<BoardClearContext>
        BoardClearResolved;

    public event Action<BoardClearOutcome>
        BoardClearOutcomeResolved;


    private void ReportMatchesToCombat(
        HashSet<Gem> matches,
        int cascadeDepth,
        List<SpecialGemCreationRequest>
            specialGemCreationRequests)
    {
        if (matches == null ||
            matches.Count == 0)
        {
            return;
        }

        /*
         * Board obstacles react to the authoritative resolved match set before
         * clear rewards/presentation. Released cells then participate in the
         * normal gravity/refill that follows this resolution.
         */
        BreakPinsAdjacentToMatches(
            matches
        );

        DamageBarricadesAdjacentToClears(
            matches
        );

        List<List<Gem>> matchGroups =
            BuildConnectedMatchGroups(
                matches
            );

        foreach (List<Gem> group
                 in matchGroups)
        {
            if (group == null ||
                group.Count < 3)
            {
                continue;
            }

            Gem firstGem =
                group[0];

            if (firstGem == null)
            {
                continue;
            }

            GemType gemType =
                firstGem.Type;

            int triggerGemCount =
                group.Count;

            /*
             * Count gems in this match that survive because
             * they are becoming newly created specials.
             */
            int preservedGemCount = 0;

            if (specialGemCreationRequests != null)
            {
                foreach (
                    SpecialGemCreationRequest request
                    in specialGemCreationRequests)
                {
                    if (!request.IsValid ||
                        request.GemToPreserve == null ||
                        !group.Contains(
                            request.GemToPreserve
                        ))
                    {
                        continue;
                    }

                    preservedGemCount++;
                }
            }

            int destroyedGemCount =
                Mathf.Max(
                    0,
                    triggerGemCount -
                    preservedGemCount
                );

            if (destroyedGemCount <= 0)
            {
                continue;
            }

            int safeCascadeDepth =
                Mathf.Max(
                    0,
                    cascadeDepth
                );

            BoardMatchType matchType =
                DetermineMatchType(
                    group,
                    true
                );

            BoardClearContext clearContext =
                new BoardClearContext(
                    gemType,
                    destroyedGemCount,
                    safeCascadeDepth,
                    BoardClearSource.Match,
                    matchType,
                    triggerGemCount
                );

            BoardClearResolved?.Invoke(
                clearContext
            );

            bool damagedMatchingEnemy = false;

            if (combatController != null)
            {
                damagedMatchingEnemy =
                    combatController.ResolveGemClear(
                        clearContext
                    );
            }

            BoardClearOutcome clearOutcome =
                new BoardClearOutcome(
                    clearContext,
                    damagedMatchingEnemy
                );

            BoardClearOutcomeResolved?.Invoke(
                clearOutcome
            );
        }
    }

    private void ReportBombClearsToCombat(
        HashSet<Gem> originalMatches,
        HashSet<Gem> expandedClearSet,
        int cascadeDepth,
        BoardClearSource clearSource =
            BoardClearSource.Bomb)
    {
        if (expandedClearSet == null ||
            expandedClearSet.Count == 0)
        {
            return;
        }

        /*
         * The original match already applied one obstacle impact through the
         * normal match reporter. Only additional bomb/crystal-cleared gems are
         * considered here, preventing the same physical match from damaging a
         * stone barricade twice merely because it also triggered a special.
         */
        DamageBarricadesAdjacentToClears(
            expandedClearSet,
            originalMatches
        );

        /*
         * Presentation consumes the same final expanded set as
         * combat, while every bomb gem still has its real world
         * position and special type. This keeps VFX out of the
         * bomb-expansion algorithm itself.
         */
        ReportBombClearSetToVFX(
            expandedClearSet,
            clearSource
        );

        Dictionary<GemType, int>
            clearedGemCounts =
                new Dictionary<GemType, int>();

        foreach (Gem gem in expandedClearSet)
        {
            if (gem == null)
            {
                continue;
            }

            /*
             * These gems were already reported by the normal
             * match reporter or were explicitly excluded because
             * their hidden color must not grant rewards.
             */
            if (originalMatches != null &&
                originalMatches.Contains(gem))
            {
                continue;
            }

            /*
             * A crystal itself has a hidden original GemType,
             * but that hidden color must never cause damage,
             * healing, energy, poison or Royal Decree damage.
             */
            if (gem.SpecialType ==
                GemSpecialType.ColorCrystal)
            {
                continue;
            }

            if (!clearedGemCounts.ContainsKey(
                    gem.Type))
            {
                clearedGemCounts[
                    gem.Type
                ] = 0;
            }

            clearedGemCounts[
                gem.Type
            ]++;
        }

        int safeCascadeDepth =
            Mathf.Max(
                0,
                cascadeDepth
            );

        foreach (
            KeyValuePair<GemType, int> result
            in clearedGemCounts)
        {
            BoardClearContext clearContext =
                new BoardClearContext(
                    result.Key,
                    result.Value,
                    safeCascadeDepth,
                    clearSource,
                    BoardMatchType.Other
                );

            BoardClearResolved?.Invoke(
                clearContext
            );

            bool damagedMatchingEnemy = false;

            if (combatController != null)
            {
                damagedMatchingEnemy =
                    combatController.ResolveGemClear(
                        clearContext
                    );
            }

            BoardClearOutcome clearOutcome =
                new BoardClearOutcome(
                    clearContext,
                    damagedMatchingEnemy
                );

            BoardClearOutcomeResolved?.Invoke(
                clearOutcome
            );
        }
    }

    private List<List<Gem>>
        BuildConnectedMatchGroups(
            HashSet<Gem> matches)
    {
        List<Gem> orderedMatches =
            new List<Gem>();

        foreach (Gem gem in matches)
        {
            if (gem != null)
            {
                orderedMatches.Add(gem);
            }
        }

        orderedMatches.Sort(
            CompareGemsByGridPosition
        );

        HashSet<Gem> unvisited =
            new HashSet<Gem>(
                orderedMatches
            );

        List<List<Gem>> groups =
            new List<List<Gem>>();

        foreach (Gem startingGem
                 in orderedMatches)
        {
            if (!unvisited.Remove(
                    startingGem))
            {
                continue;
            }

            List<Gem> group =
                new List<Gem>();

            Queue<Gem> pending =
                new Queue<Gem>();

            pending.Enqueue(startingGem);

            while (pending.Count > 0)
            {
                Gem current =
                    pending.Dequeue();

                group.Add(current);

                TryQueueMatchingNeighbour(
                    current.Column - 1,
                    current.Row,
                    current.Type,
                    unvisited,
                    pending
                );

                TryQueueMatchingNeighbour(
                    current.Column + 1,
                    current.Row,
                    current.Type,
                    unvisited,
                    pending
                );

                TryQueueMatchingNeighbour(
                    current.Column,
                    current.Row - 1,
                    current.Type,
                    unvisited,
                    pending
                );

                TryQueueMatchingNeighbour(
                    current.Column,
                    current.Row + 1,
                    current.Type,
                    unvisited,
                    pending
                );
            }

            group.Sort(
                CompareGemsByGridPosition
            );

            groups.Add(group);
        }

        return groups;
    }

    private void TryQueueMatchingNeighbour(
        int column,
        int row,
        GemType requiredType,
        HashSet<Gem> unvisited,
        Queue<Gem> pending)
    {
        Gem neighbour =
            GetGem(column, row);

        if (neighbour == null ||
            neighbour.Type != requiredType)
        {
            return;
        }

        if (!unvisited.Remove(neighbour))
        {
            return;
        }

        pending.Enqueue(neighbour);
    }

    private static BoardMatchType DetermineMatchType(
        List<Gem> group,
        bool distinguishCrossShape = false)
    {
        if (group == null ||
            group.Count < 3)
        {
            return BoardMatchType.Other;
        }

        HashSet<Vector2Int> matchedPositions =
            new HashSet<Vector2Int>();

        foreach (Gem gem in group)
        {
            if (gem == null)
            {
                return BoardMatchType.Other;
            }

            matchedPositions.Add(
                new Vector2Int(
                    gem.Column,
                    gem.Row
                )
            );
        }

        /*
         * Highest priority: search for any gem that belongs to both a
         * horizontal line of at least three and a vertical line of at least
         * three. This detects L, T, cross, and extended variants.
         */
        foreach (Gem intersection in group)
        {
            Vector2Int intersectionPosition =
                new Vector2Int(
                    intersection.Column,
                    intersection.Row
                );

            int leftCount =
                CountConnectedMatchPositions(
                    matchedPositions,
                    intersectionPosition,
                    Vector2Int.left
                );

            int rightCount =
                CountConnectedMatchPositions(
                    matchedPositions,
                    intersectionPosition,
                    Vector2Int.right
                );

            int belowCount =
                CountConnectedMatchPositions(
                    matchedPositions,
                    intersectionPosition,
                    Vector2Int.down
                );

            int aboveCount =
                CountConnectedMatchPositions(
                    matchedPositions,
                    intersectionPosition,
                    Vector2Int.up
                );

            int horizontalCount =
                1 +
                leftCount +
                rightCount;

            int verticalCount =
                1 +
                belowCount +
                aboveCount;

            if (horizontalCount < 3 ||
                verticalCount < 3)
            {
                continue;
            }

            bool isHorizontalMiddle =
                leftCount > 0 &&
                rightCount > 0;

            bool isVerticalMiddle =
                belowCount > 0 &&
                aboveCount > 0;

            if (!isHorizontalMiddle &&
                !isVerticalMiddle)
            {
                return BoardMatchType.LShape;
            }

            if (distinguishCrossShape &&
                isHorizontalMiddle &&
                isVerticalMiddle)
            {
                return BoardMatchType.CrossShape;
            }

            /*
             * Compatibility default: until Gem Mastery owns special creation,
             * existing callers continue treating a cross like the old T-shape
             * path so the current Poison Bomb reward cannot disappear.
             */
            return BoardMatchType.TShape;
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

            if (gem.Row != firstRow)
            {
                allSameRow = false;
            }

            if (gem.Column != firstColumn)
            {
                allSameColumn = false;
            }
        }

        bool isStraight =
            allSameRow ||
            allSameColumn;

        if (!isStraight)
        {
            return BoardMatchType.Other;
        }

        if (group.Count >= 5)
        {
            return BoardMatchType.StraightFive;
        }

        if (group.Count == 4)
        {
            return BoardMatchType.StraightFour;
        }

        if (group.Count == 3)
        {
            return BoardMatchType.NormalThree;
        }

        return BoardMatchType.Other;
    }

    private static int CountConnectedMatchPositions(
        HashSet<Vector2Int> matchedPositions,
        Vector2Int startingPosition,
        Vector2Int direction)
    {
        int count = 0;

        Vector2Int currentPosition =
            startingPosition +
            direction;

        while (matchedPositions.Contains(
                   currentPosition))
        {
            count++;

            currentPosition +=
                direction;
        }

        return count;
    }

    private static int CompareGemsByGridPosition(
        Gem first,
        Gem second)
    {
        if (ReferenceEquals(first, second))
        {
            return 0;
        }

        if (first == null)
        {
            return 1;
        }

        if (second == null)
        {
            return -1;
        }

        int rowComparison =
            first.Row.CompareTo(second.Row);

        if (rowComparison != 0)
        {
            return rowComparison;
        }

        return first.Column.CompareTo(
            second.Column
        );
    }
}
