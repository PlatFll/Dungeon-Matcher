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

    private HashSet<Gem>
        BuildBombTriggeredCrystalTargetSet(
            BombTriggeredCrystalRequest request)
    {
        HashSet<Gem> targetSet =
            new HashSet<Gem>();

        if (!request.IsValid)
        {
            return targetSet;
        }

        Gem crystalGem =
            request.CrystalGem;

        /*
         * The triggered crystal must also be destroyed as part
         * of its activation, but its hidden GemType will not
         * receive damage or energy rewards.
         */
        targetSet.Add(
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
                    gem.Type !=
                        request.TriggerGemType)
                {
                    continue;
                }

                /*
                 * Other crystals remain colorless and are not
                 * selected through their hidden original type.
                 */
                if (gem.SpecialType ==
                    GemSpecialType.ColorCrystal)
                {
                    continue;
                }

                targetSet.Add(
                    gem
                );
            }
        }

        return targetSet;
    }

    private static HashSet<Gem>
        ConvertCrystalTargetsToRandomBombs(
            List<Gem> orderedTargets)
    {
        HashSet<Gem> pendingConvertedBombs =
            new HashSet<Gem>();

        if (orderedTargets == null)
        {
            return pendingConvertedBombs;
        }

        foreach (Gem targetGem in orderedTargets)
        {
            if (targetGem == null)
            {
                continue;
            }

            GemSpecialType randomBombType =
                Random.Range(0, 2) == 0
                    ? GemSpecialType.RowBomb
                    : GemSpecialType.ColumnBomb;

            targetGem.SetSpecialType(
                randomBombType
            );

            pendingConvertedBombs.Add(
                targetGem
            );
        }

        return pendingConvertedBombs;
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

    private IEnumerator
        ResolveNormalColorCrystalSequence(
            HashSet<Gem> crystalClearSet,
            GemType targetGemType)
    {
        Gem crystalGem;

        List<Gem> orderedTargets =
            BuildOrderedCrystalTargets(
                crystalClearSet,
                out crystalGem
            );

        HashSet<Gem> alreadyCleared =
            new HashSet<Gem>();

        /*
         * Remove the crystal first without granting rewards
         * for its hidden underlying color.
         */
        if (crystalGem != null)
        {
            HashSet<Gem> crystalOnly =
                new HashSet<Gem>
                {
                crystalGem
                };

            alreadyCleared.Add(
                crystalGem
            );

            yield return ClearMatches(
                crystalOnly,
                null
            );
        }

        HashSet<Gem> noRewardExclusions =
            new HashSet<Gem>();

        for (int index = 0;
             index < orderedTargets.Count;
             index++)
        {
            Gem targetGem =
                orderedTargets[index];

            if (targetGem == null ||
                alreadyCleared.Contains(
                    targetGem
                ))
            {
                continue;
            }

            HashSet<Gem> activationSeed =
                new HashSet<Gem>
                {
                targetGem
                };

            HashSet<Gem> activationSet;

            List<BombTriggeredCrystalRequest>
                triggeredCrystalRequests = null;

            /*
             * Existing row or column bombs of the selected
             * color still activate normally when reached.
             *
             * Any crystal crossed by the bomb is preserved and
             * queued instead of being silently removed.
             */
            if (targetGem.SpecialType ==
                    GemSpecialType.RowBomb ||
                targetGem.SpecialType ==
                    GemSpecialType.ColumnBomb)
            {
                activationSet =
                    BuildBombExpandedClearSet(
                        activationSeed,
                        true,
                        out triggeredCrystalRequests
                    );
            }
            else
            {
                activationSet =
                    activationSeed;
            }

            activationSet.RemoveWhere(
                gem =>
                    gem == null ||
                    alreadyCleared.Contains(gem)
            );

            if (activationSet.Count == 0)
            {
                continue;
            }

            /*
             * Reward only gems that are actually being
             * destroyed during this individual activation.
             */
            ReportBombClearsToCombat(
                noRewardExclusions,
                activationSet,
                0
            );

            foreach (Gem clearedGem in activationSet)
            {
                if (clearedGem != null)
                {
                    alreadyCleared.Add(
                        clearedGem
                    );
                }
            }

            yield return ClearMatches(
                activationSet,
                null
            );

            /*
             * The bomb explosion has finished, so activate any
             * crystals that it crossed before continuing the
             * original crystal sequence.
             */
            yield return
                ResolveBombTriggeredCrystalRequests(
                    triggeredCrystalRequests
                );

            if (crystalActivationStagger > 0f &&
                            index <
                    orderedTargets.Count - 1)
            {
                yield return new WaitForSeconds(
                    crystalActivationStagger
                );
            }
        }

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
            $"Color crystal sequentially cleared " +
            $"{targetGemType} gems."
        );
    }

    private HashSet<Gem>
        BuildConvertedCrystalBombActivationSet(
            Gem activatedBomb,
            HashSet<Gem> pendingConvertedBombs,
            List<BombTriggeredCrystalRequest>
                triggeredCrystalRequests)
    {
        HashSet<Gem> gemsToClear =
            new HashSet<Gem>();

        Queue<Gem> pendingBombs =
            new Queue<Gem>();

        HashSet<Gem> triggeredBombs =
            new HashSet<Gem>();

        if (activatedBomb == null)
        {
            return gemsToClear;
        }

        gemsToClear.Add(
            activatedBomb
        );

        pendingBombs.Enqueue(
            activatedBomb
        );

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
                        TryAddGemToConvertedBombClearSet(
                            column,
                            bomb.Row,
                            activatedBomb,
                            pendingConvertedBombs,
                            triggeredCrystalRequests,
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
                        TryAddGemToConvertedBombClearSet(
                            bomb.Column,
                            row,
                            activatedBomb,
                            pendingConvertedBombs,
                            triggeredCrystalRequests,
                            gemsToClear,
                            pendingBombs
                        );
                    }

                    break;
            }
        }

        return gemsToClear;
    }

    private void TryAddGemToConvertedBombClearSet(
        int column,
        int row,
        Gem activatedBomb,
        HashSet<Gem> pendingConvertedBombs,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
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

        /*
         * A crystal hit by this explosion must not be added
         * to the clear set. Keep it on the board and queue
         * its own activation instead.
         */
        if (gem.SpecialType ==
            GemSpecialType.ColorCrystal)
        {
            TryAddBombTriggeredCrystalRequest(
                gem,
                activatedBomb,
                triggeredCrystalRequests
            );

            return;
        }

        /*
         * Converted bombs that have not reached their own
         * activation turn remain protected when an earlier
         * converted bomb crosses them.
         */
        bool isProtectedConvertedBomb =
            gem != activatedBomb &&
            pendingConvertedBombs != null &&
            pendingConvertedBombs.Contains(gem);

        if (isProtectedConvertedBomb)
        {
            return;
        }

        bool wasAdded =
            gemsToClear.Add(gem);

        /*
         * Ordinary pre-existing bombs may still join the
         * explosion chain.
         */
        if (wasAdded &&
            (
                gem.SpecialType ==
                    GemSpecialType.RowBomb ||
                gem.SpecialType ==
                    GemSpecialType.ColumnBomb
            ))
        {
            pendingBombs.Enqueue(gem);
        }
    }

    private IEnumerator
        ResolveBombColorCrystalSequence(
            HashSet<Gem> crystalClearSet,
            GemType targetGemType,
            GemSpecialType convertedBombType)
    {
        if (convertedBombType !=
                GemSpecialType.RowBomb &&
            convertedBombType !=
                GemSpecialType.ColumnBomb)
        {
            yield break;
        }

        Gem crystalGem;

        List<Gem> orderedTargets =
            BuildOrderedCrystalTargets(
                crystalClearSet,
                out crystalGem
            );

        HashSet<Gem> alreadyCleared =
            new HashSet<Gem>();

        /*
         * Remove the crystal without rewarding its hidden
         * underlying gem color.
         */
        if (crystalGem != null)
        {
            HashSet<Gem> crystalOnly =
                new HashSet<Gem>
                {
                crystalGem
                };

            alreadyCleared.Add(
                crystalGem
            );

            yield return ClearMatches(
                crystalOnly,
                null
            );
        }

        /*
         * Convert every gem of the selected color before
         * any of the converted bombs begin detonating.
         */
        HashSet<Gem> pendingConvertedBombs =
            new HashSet<Gem>();

        foreach (Gem targetGem in orderedTargets)
        {
            if (targetGem == null ||
                alreadyCleared.Contains(
                    targetGem
                ))
            {
                continue;
            }

            targetGem.SetSpecialType(
                convertedBombType
            );

            pendingConvertedBombs.Add(
                targetGem
            );
        }

        if (crystalActivationStagger > 0f)
        {
            yield return new WaitForSeconds(
                crystalActivationStagger
            );
        }

        HashSet<Gem> noRewardExclusions =
            new HashSet<Gem>();

        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests =
                new List<
                    BombTriggeredCrystalRequest
                >();

        for (int index = 0;
             index < orderedTargets.Count;
             index++)
        {
            Gem activatedBomb =
                orderedTargets[index];

            if (activatedBomb == null ||
                alreadyCleared.Contains(
                    activatedBomb
                ) ||
                !pendingConvertedBombs.Contains(
                    activatedBomb
                ))
            {
                continue;
            }

            /*
             * It is no longer protected because its own
             * activation turn has now begun.
             */
            pendingConvertedBombs.Remove(
                activatedBomb
            );

            HashSet<Gem> activationSet =
                BuildConvertedCrystalBombActivationSet(
                    activatedBomb,
                    pendingConvertedBombs,
                    triggeredCrystalRequests
                );

            activationSet.RemoveWhere(
                gem =>
                    gem == null ||
                    alreadyCleared.Contains(gem)
            );

            if (activationSet.Count == 0)
            {
                continue;
            }

            ReportBombClearsToCombat(
                noRewardExclusions,
                activationSet,
                0
            );

            foreach (Gem clearedGem in activationSet)
            {
                if (clearedGem != null)
                {
                    alreadyCleared.Add(
                        clearedGem
                    );
                }
            }

            yield return ClearMatches(
                activationSet,
                null
            );

            if (crystalActivationStagger > 0f &&
                pendingConvertedBombs.Count > 0)
            {
                yield return new WaitForSeconds(
                    crystalActivationStagger
                );
            }
        }

        /*
         * All converted bombs have finished. Crystals
         * crossed by those explosions now activate in
         * the order in which they were reached.
         */
        yield return
            ResolveBombTriggeredCrystalRequests(
                triggeredCrystalRequests
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
            $"Color crystal converted all " +
            $"{targetGemType} gems into " +
            $"{convertedBombType} gems."
        );
    }

    private IEnumerator
        ResolveBombTriggeredCrystalRequests(
            List<BombTriggeredCrystalRequest>
                triggeredCrystalRequests)
    {
        if (triggeredCrystalRequests == null ||
            triggeredCrystalRequests.Count == 0)
        {
            yield break;
        }

        for (int index = 0;
             index < triggeredCrystalRequests.Count;
             index++)
        {
            BombTriggeredCrystalRequest request =
                triggeredCrystalRequests[index];

            /*
             * Another crystal activation may already have
             * destroyed this crystal before its queued turn.
             */
            if (!request.IsValid)
            {
                continue;
            }

            yield return
                ResolveBombTriggeredCrystalSequence(
                    request
                );
        }
    }

    private IEnumerator
        ResolveBombTriggeredCrystalSequence(
            BombTriggeredCrystalRequest request)
    {
        if (!request.IsValid)
        {
            yield break;
        }

        HashSet<Gem> crystalTargetSet =
            BuildBombTriggeredCrystalTargetSet(
                request
            );

        Gem crystalGem;

        List<Gem> orderedTargets =
            BuildOrderedCrystalTargets(
                crystalTargetSet,
                out crystalGem
            );

        HashSet<Gem> alreadyCleared =
            new HashSet<Gem>();

        /*
         * Destroy the triggered crystal without rewarding its
         * hidden original GemType.
         */
        if (crystalGem != null)
        {
            HashSet<Gem> crystalOnly =
                new HashSet<Gem>
                {
                crystalGem
                };

            alreadyCleared.Add(
                crystalGem
            );

            yield return ClearMatches(
                crystalOnly,
                null
            );
        }

        /*
         * Every remaining gem matching the triggering bomb's
         * color becomes a randomly oriented bomb.
         */
        HashSet<Gem> pendingConvertedBombs =
            ConvertCrystalTargetsToRandomBombs(
                orderedTargets
            );

        if (crystalActivationStagger > 0f &&
            pendingConvertedBombs.Count > 0)
        {
            yield return new WaitForSeconds(
                crystalActivationStagger
            );
        }

        HashSet<Gem> noRewardExclusions =
            new HashSet<Gem>();

        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests =
                new List<
                    BombTriggeredCrystalRequest
                >();

        for (int index = 0;
             index < orderedTargets.Count;
             index++)
        {
            Gem activatedBomb =
                orderedTargets[index];

            if (activatedBomb == null ||
                alreadyCleared.Contains(
                    activatedBomb
                ) ||
                !pendingConvertedBombs.Contains(
                    activatedBomb
                ))
            {
                continue;
            }

            /*
             * This converted bomb has reached its activation
             * turn and no longer needs protection.
             */
            pendingConvertedBombs.Remove(
                activatedBomb
            );

            HashSet<Gem> activationSet =
                BuildConvertedCrystalBombActivationSet(
                    activatedBomb,
                    pendingConvertedBombs,
                    triggeredCrystalRequests
                );

            activationSet.RemoveWhere(
                gem =>
                    gem == null ||
                    alreadyCleared.Contains(gem)
            );

            if (activationSet.Count == 0)
            {
                continue;
            }

            ReportBombClearsToCombat(
                noRewardExclusions,
                activationSet,
                0
            );

            foreach (Gem clearedGem in activationSet)
            {
                if (clearedGem != null)
                {
                    alreadyCleared.Add(
                        clearedGem
                    );
                }
            }

            yield return ClearMatches(
                activationSet,
                null
            );

            if (crystalActivationStagger > 0f &&
                pendingConvertedBombs.Count > 0)
            {
                yield return new WaitForSeconds(
                    crystalActivationStagger
                );
            }
        }

        /*
         * Continue the chain if any of these converted
         * bombs crossed another crystal.
         */
        yield return
            ResolveBombTriggeredCrystalRequests(
                triggeredCrystalRequests
            );

        Debug.Log(
            $"Bomb-triggered crystal converted all " +
            $"{request.TriggerGemType} gems into " +
            $"random row and column bombs."
        );
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

        if (targetSpecialType ==
            GemSpecialType.None)
        {
            yield return
                ResolveNormalColorCrystalSequence(
                    crystalClearSet,
                    targetGemType
                );

            yield break;
        }

        if (targetSpecialType ==
                GemSpecialType.RowBomb ||
            targetSpecialType ==
                GemSpecialType.ColumnBomb)
        {
            yield return
                ResolveBombColorCrystalSequence(
                    crystalClearSet,
                    targetGemType,
                    targetSpecialType
                );

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
        List<BombTriggeredCrystalRequest>
            ignoredCrystalRequests;

        /*
         * Existing callers retain their current behaviour:
         * crystals caught in explosions are cleared normally.
         *
         * A later caller can enable preservation and receive
         * activation requests instead.
         */
        return BuildBombExpandedClearSet(
            matchedGems,
            false,
            out ignoredCrystalRequests
        );
    }

    private HashSet<Gem> BuildBombExpandedClearSet(
        HashSet<Gem> matchedGems,
        bool preserveTriggeredCrystals,
        out List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests)
    {
        HashSet<Gem> gemsToClear =
            new HashSet<Gem>();

        Queue<Gem> pendingBombs =
            new Queue<Gem>();

        HashSet<Gem> triggeredBombs =
            new HashSet<Gem>();

        triggeredCrystalRequests =
            new List<BombTriggeredCrystalRequest>();

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

            if (gem.SpecialType ==
                    GemSpecialType.RowBomb ||
                gem.SpecialType ==
                    GemSpecialType.ColumnBomb)
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
                            bomb,
                            preserveTriggeredCrystals,
                            triggeredCrystalRequests,
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
                            bomb,
                            preserveTriggeredCrystals,
                            triggeredCrystalRequests,
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
        Gem triggeringBomb,
        bool preserveTriggeredCrystals,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
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

        /*
         * When requested, a crystal hit by this bomb remains
         * on the board temporarily and produces an activation
         * request containing the bomb's color.
         */
        if (preserveTriggeredCrystals &&
            gem.SpecialType ==
                GemSpecialType.ColorCrystal)
        {
            TryAddBombTriggeredCrystalRequest(
                gem,
                triggeringBomb,
                triggeredCrystalRequests
            );

            return;
        }

        bool wasAdded =
            gemsToClear.Add(gem);

        if (wasAdded &&
            (
                gem.SpecialType ==
                    GemSpecialType.RowBomb ||
                gem.SpecialType ==
                    GemSpecialType.ColumnBomb
            ))
        {
            pendingBombs.Enqueue(gem);
        }
    }

    private static void
        TryAddBombTriggeredCrystalRequest(
            Gem crystalGem,
            Gem triggeringBomb,
            List<BombTriggeredCrystalRequest>
                triggeredCrystalRequests)
    {
        if (crystalGem == null ||
            triggeringBomb == null ||
            triggeredCrystalRequests == null)
        {
            return;
        }

        /*
         * A crystal may be crossed by several explosions in
         * the same chain. Only create one request for it.
         */
        foreach (
            BombTriggeredCrystalRequest request
            in triggeredCrystalRequests)
        {
            if (request.CrystalGem ==
                crystalGem)
            {
                return;
            }
        }

        triggeredCrystalRequests.Add(
            new BombTriggeredCrystalRequest(
                crystalGem,
                triggeringBomb.Type
            )
        );
    }
}