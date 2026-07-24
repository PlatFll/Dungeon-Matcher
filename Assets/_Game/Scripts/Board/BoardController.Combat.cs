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
            group.Count < 3 ||
            group[0] == null)
        {
            return BoardMatchType.Other;
        }

        int gemCount = group.Count;
        int firstRow = group[0].Row;
        int firstColumn = group[0].Column;

        bool allSameRow = true;
        bool allSameColumn = true;

        for (int index = 1;
             index < group.Count;
             index++)
        {
            Gem gem = group[index];

            if (gem == null)
            {
                return BoardMatchType.Other;
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

        bool isStraight =
            allSameRow ||
            allSameColumn;

        if (isStraight)
        {
            if (gemCount == 3)
            {
                return BoardMatchType.NormalThree;
            }

            if (gemCount == 4)
            {
                return BoardMatchType.StraightFour;
            }

            if (gemCount >= 5)
            {
                return BoardMatchType.StraightFive;
            }
        }

        /*
         * L and T matches currently require exactly
         * five gems: two lines of three sharing one gem.
         */
        if (gemCount != 5)
        {
            return BoardMatchType.Other;
        }

        for (int candidateIndex = 0;
             candidateIndex < group.Count;
             candidateIndex++)
        {
            Gem intersection =
                group[candidateIndex];

            int sameRowCount = 0;
            int sameColumnCount = 0;

            int minimumColumn = int.MaxValue;
            int maximumColumn = int.MinValue;

            int minimumRow = int.MaxValue;
            int maximumRow = int.MinValue;

            for (int gemIndex = 0;
                 gemIndex < group.Count;
                 gemIndex++)
            {
                Gem gem = group[gemIndex];

                if (gem.Row == intersection.Row)
                {
                    sameRowCount++;

                    minimumColumn =
                        Mathf.Min(
                            minimumColumn,
                            gem.Column
                        );

                    maximumColumn =
                        Mathf.Max(
                            maximumColumn,
                            gem.Column
                        );
                }

                if (gem.Column ==
                    intersection.Column)
                {
                    sameColumnCount++;

                    minimumRow =
                        Mathf.Min(
                            minimumRow,
                            gem.Row
                        );

                    maximumRow =
                        Mathf.Max(
                            maximumRow,
                            gem.Row
                        );
                }
            }

            if (sameRowCount != 3 ||
                sameColumnCount != 3)
            {
                continue;
            }

            bool isHorizontalMiddle =
                intersection.Column >
                    minimumColumn &&
                intersection.Column <
                    maximumColumn;

            bool isVerticalMiddle =
                intersection.Row >
                    minimumRow &&
                intersection.Row <
                    maximumRow;

            /*
             * Middle of one line and end of the other
             * creates a T.
             */
            if (isHorizontalMiddle !=
                isVerticalMiddle)
            {
                return BoardMatchType.TShape;
            }

            /*
             * End of both lines creates an L.
             */
            if (!isHorizontalMiddle &&
                !isVerticalMiddle)
            {
                return BoardMatchType.LShape;
            }

            /*
             * Middle of both lines is a plus shape,
             * which is not classified yet.
             */
            return BoardMatchType.Other;
        }

        return BoardMatchType.Other;
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