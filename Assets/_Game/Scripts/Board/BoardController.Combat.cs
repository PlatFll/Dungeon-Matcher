using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    public bool IsBusy => isBusy;

    [Header("Combat")]
    [SerializeField]
    [Tooltip(
        "Receives real board matches and converts them " +
        "into damage against enemies."
    )]
    private CombatController combatController;

    public event Action<BoardMatchContext>
    BoardMatchResolved;

    public event Action<BoardMatchOutcome>
    BoardMatchOutcomeResolved;

    public event Action<BoardBombClearOutcome>
    BoardBombClearOutcomeResolved;


    private void ReportMatchesToCombat(
        HashSet<Gem> matches,
        int cascadeDepth)
    {
        if (matches == null ||
            matches.Count == 0)
        {
            return;
        }

        List<List<Gem>> matchGroups =
            BuildConnectedMatchGroups(matches);

        foreach (List<Gem> group in matchGroups)
        {
            if (group == null ||
                group.Count < 3)
            {
                continue;
            }

            Gem firstGem = group[0];

            if (firstGem == null)
            {
                continue;
            }

            GemType gemType =
                firstGem.Type;

            int gemCount =
                group.Count;

            int safeCascadeDepth =
                Mathf.Max(0, cascadeDepth);

            BoardMatchType matchType =
                DetermineMatchType(group);

            BoardMatchContext matchContext =
                new BoardMatchContext(
                    gemType,
                    gemCount,
                    safeCascadeDepth,
                    matchType
                );

            BoardMatchResolved?.Invoke(
                matchContext
            );

            bool damagedMatchingEnemy = false;

            if (combatController != null)
            {
                damagedMatchingEnemy =
                    combatController.ResolveGemMatch(
                        gemType,
                        gemCount,
                        safeCascadeDepth
                    );
            }

            BoardMatchOutcome outcome =
                new BoardMatchOutcome(
                    matchContext,
                    damagedMatchingEnemy
                );

            BoardMatchOutcomeResolved?.Invoke(
                outcome
            );
        }
    }

    private void ReportBombClearsToCombat(
        HashSet<Gem> originalMatches,
        HashSet<Gem> expandedClearSet,
        int cascadeDepth)
    {
        if (expandedClearSet == null ||
            expandedClearSet.Count == 0)
        {
            return;
        }

        Dictionary<GemType, int>
            explosionGemCounts =
                new Dictionary<GemType, int>();

        foreach (Gem gem in expandedClearSet)
        {
            if (gem == null)
            {
                continue;
            }

            /*
             * Gems belonging to the original match already
             * received normal match damage and energy.
             */
            if (originalMatches != null &&
                originalMatches.Contains(gem))
            {
                continue;
            }

            if (!explosionGemCounts.ContainsKey(
                    gem.Type))
            {
                explosionGemCounts[
                    gem.Type
                ] = 0;
            }

            explosionGemCounts[
                gem.Type
            ]++;
        }

        int safeCascadeDepth =
            Mathf.Max(
                0,
                cascadeDepth
            );

        foreach (
            KeyValuePair<GemType, int>
                explosionResult
            in explosionGemCounts)
        {
            bool damagedMatchingEnemy = false;

            if (combatController != null)
            {
                damagedMatchingEnemy =
                    combatController
                        .ResolveBombGemClear(
                            explosionResult.Key,
                            explosionResult.Value,
                            safeCascadeDepth
                        );
            }

            BoardBombClearOutcome outcome =
                new BoardBombClearOutcome(
                    explosionResult.Key,
                    explosionResult.Value,
                    safeCascadeDepth,
                    damagedMatchingEnemy
                );

            BoardBombClearOutcomeResolved?.Invoke(
                outcome
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
        List<Gem> group)
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
         * Highest priority:
         *
         * Search for any gem that belongs to both a
         * horizontal line of at least three and a
         * vertical line of at least three.
         *
         * This detects L, T, cross, and extended
         * versions of those shapes even when the
         * connected group contains more than five gems.
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

            /*
             * End of both lines means an L shape.
             */
            if (!isHorizontalMiddle &&
                !isVerticalMiddle)
            {
                return BoardMatchType.LShape;
            }

            /*
             * Middle of one line means a T shape.
             *
             * Middle of both lines means a cross.
             * Cross currently uses TShape because both
             * produce the same color crystal and use
             * the same energy reward.
             */
            return BoardMatchType.TShape;
        }

        /*
         * No intersection was found, so check whether
         * the entire group is one straight line.
         */
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