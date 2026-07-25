using System.Collections.Generic;

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
            if (DetermineMatchType(group) !=
                BoardMatchType.StraightFour)
            {
                continue;
            }

            GemSpecialType specialType =
                GetStraightFourSpecialType(group);

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
}