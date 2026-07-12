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

    public event Action<GemType, int, int>
        MatchResolved;

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

            MatchResolved?.Invoke(
                gemType,
                gemCount,
                safeCascadeDepth
            );

            if (combatController != null)
            {
                combatController.ResolveGemMatch(
                    gemType,
                    gemCount,
                    safeCascadeDepth
                );
            }
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