using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController

{

    [Header("Bomb-Triggered Crystal Charge")]

    [SerializeField, Range(1f, 1.5f)]
    [Tooltip(
        "Maximum scale reached while a crystal waits " +
        "for the board to refill."
    )]
    private float triggeredCrystalChargeScale =
        1.16f;

    [SerializeField, Min(0.1f)]
    [Tooltip(
        "Speed of the charging crystal pulse."
    )]
    private float triggeredCrystalChargePulseSpeed =
        4f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Small pause after the refill settles before " +
        "the queued crystal activates."
    )]
    private float triggeredCrystalSettlePause =
        0.12f;

    private readonly HashSet<Gem>
        chargingTriggeredCrystals =
            new HashSet<Gem>();

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
                    specialType =
                        GemSpecialType.ColorCrystal;

                    break;

                case BoardMatchType.LShape:
                case BoardMatchType.TShape:
                    specialType =
                        GemSpecialType.PoisonBomb;

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
                    matchType,
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
        BoardMatchType matchType,
        Gem preferredGem,
        Gem fallbackGem)
    {
        if (group == null ||
            group.Count == 0)
        {
            return null;
        }

        /*
         * Player-created specials continue to prioritize the
         * gem moved by the player.
         */
        if (preferredGem != null &&
            group.Contains(preferredGem))
        {
            return preferredGem;
        }

        /*
         * Use the other swapped gem when that is the gem that
         * belongs to the special-producing match.
         */
        if (fallbackGem != null &&
            group.Contains(fallbackGem))
        {
            return fallbackGem;
        }

        /*
         * Automatic cascades have no preferred swapped gem.
         * Choose a meaningful position based on match geometry.
         */
        switch (matchType)
        {
            case BoardMatchType.LShape:
            case BoardMatchType.TShape:
                {
                    Gem shapePivot =
                        FindShapePivotGem(group);

                    if (shapePivot != null)
                    {
                        return shapePivot;
                    }

                    break;
                }

            case BoardMatchType.StraightFive:
                return FindGemNearestGroupCenter(
                    group
                );
        }

        /*
         * Straight-four cascades and unexpected shapes use
         * the most central available gem.
         */
        return FindGemNearestGroupCenter(
            group
        );
    }

    private static Gem FindShapePivotGem(
        List<Gem> group)
    {
        if (group == null ||
            group.Count == 0)
        {
            return null;
        }

        Gem bestPivot = null;

        foreach (Gem candidate in group)
        {
            if (candidate == null)
            {
                continue;
            }

            bool hasHorizontalNeighbour = false;
            bool hasVerticalNeighbour = false;

            foreach (Gem other in group)
            {
                if (other == null ||
                    other == candidate)
                {
                    continue;
                }

                if (other.Row == candidate.Row)
                {
                    hasHorizontalNeighbour = true;
                }

                if (other.Column ==
                    candidate.Column)
                {
                    hasVerticalNeighbour = true;
                }

                if (hasHorizontalNeighbour &&
                    hasVerticalNeighbour)
                {
                    break;
                }
            }

            /*
             * The T intersection and L corner are the only gems
             * connected in both the horizontal and vertical axes.
             */
            if (!hasHorizontalNeighbour ||
                !hasVerticalNeighbour)
            {
                continue;
            }

            if (bestPivot == null ||
                IsEarlierGridPosition(
                    candidate,
                    bestPivot))
            {
                bestPivot = candidate;
            }
        }

        return bestPivot;
    }

    private static Gem FindGemNearestGroupCenter(
        List<Gem> group)
    {
        if (group == null ||
            group.Count == 0)
        {
            return null;
        }

        float totalColumn = 0f;
        float totalRow = 0f;
        int validGemCount = 0;

        foreach (Gem gem in group)
        {
            if (gem == null)
            {
                continue;
            }

            totalColumn += gem.Column;
            totalRow += gem.Row;
            validGemCount++;
        }

        if (validGemCount == 0)
        {
            return null;
        }

        float centerColumn =
            totalColumn /
            validGemCount;

        float centerRow =
            totalRow /
            validGemCount;

        Gem bestGem = null;
        float bestDistance =
            float.MaxValue;

        foreach (Gem gem in group)
        {
            if (gem == null)
            {
                continue;
            }

            float columnDistance =
                gem.Column -
                centerColumn;

            float rowDistance =
                gem.Row -
                centerRow;

            float squaredDistance =
                columnDistance *
                columnDistance +
                rowDistance *
                rowDistance;

            bool isCloser =
                squaredDistance <
                bestDistance;

            bool isEqualDistance =
                Mathf.Approximately(
                    squaredDistance,
                    bestDistance
                );

            if (bestGem == null ||
                isCloser ||
                (
                    isEqualDistance &&
                    IsEarlierGridPosition(
                        gem,
                        bestGem
                    )
                ))
            {
                bestGem = gem;
                bestDistance =
                    squaredDistance;
            }
        }

        return bestGem;
    }

    private static bool IsEarlierGridPosition(
        Gem candidate,
        Gem current)
    {
        if (candidate == null)
        {
            return false;
        }

        if (current == null)
        {
            return true;
        }

        if (candidate.Row != current.Row)
        {
            return candidate.Row <
                   current.Row;
        }

        return candidate.Column <
               current.Column;
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

    private bool
        TryCreateEarnedSpecialBeforeCrystalActivation(
            Gem first,
            Gem second,
            out Gem crystalGem,
            out Gem targetGem,
            out GemSpecialType createdSpecialType)
    {
        crystalGem = null;
        targetGem = null;

        createdSpecialType =
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
         * This helper only handles exactly one crystal.
         * Existing crystal + crystal swaps are handled by
         * ResolveDoubleColorCrystalActivation.
         */
        if (firstIsCrystal ==
            secondIsCrystal)
        {
            return false;
        }

        crystalGem =
            firstIsCrystal
                ? first
                : second;

        targetGem =
            firstIsCrystal
                ? second
                : first;

        /*
         * Do not replace an existing row bomb, column bomb,
         * or other special. Its existing crystal interaction
         * should continue normally.
         */
        if (targetGem.SpecialType !=
            GemSpecialType.None)
        {
            return false;
        }

        /*
         * The crystal itself is ignored by AddMatchesAt.
         * Only inspect the ordinary gem after the completed
         * swap.
         */
        HashSet<Gem> targetMatches =
            FindMatchesFrom(
                targetGem,
                null
            );

        if (targetMatches.Count < 4)
        {
            return false;
        }

        /*
         * Passing targetGem as the preferred gem ensures the
         * swapped gem becomes the earned special.
         */
        List<SpecialGemCreationRequest>
            creationRequests =
                BuildSpecialGemCreationRequests(
                    targetMatches,
                    targetGem,
                    null
                );

        foreach (
            SpecialGemCreationRequest request
            in creationRequests)
        {
            if (!request.IsValid ||
                request.GemToPreserve !=
                    targetGem ||
                request.SpecialType ==
                    GemSpecialType.None)
            {
                continue;
            }

            createdSpecialType =
                request.SpecialType;

            /*
             * Create the earned special immediately without
             * clearing the match yet. The following crystal
             * interaction will process the matching color.
             */
            targetGem.SetSpecialType(
                createdSpecialType
            );

            Debug.Log(
                $"Crystal swap created " +
                $"{createdSpecialType} at " +
                $"({targetGem.Column}, " +
                $"{targetGem.Row}) before activation."
            );

            return true;
        }

        return false;
    }

    /*
     * Reward-first crystal/bomb exception.
     *
     * A direct crystal + ordinary-gem swap normally activates the
     * crystal immediately. That can waste an already-existing bomb
     * when the ordinary gem simultaneously creates a normal three.
     *
     * For exactly that case, allow the ordinary three-match to resolve
     * first only when one of its existing row/column bombs will actually
     * cross the crystal's current cell. ResolveCascades already preserves
     * bomb-hit crystals, records the triggering bomb's GemType, refills,
     * then converts that color into random bombs.
     *
     * Scope this to exact three-matches and an ordinary swapped gem so
     * direct crystal+bomb swaps and the carefully-defined 4/5/L/T special
     * creation interactions keep their existing behavior.
     */
    private bool ShouldResolveMatchedBombBeforeCrystal(
        Gem crystalGem,
        Gem targetGem)
    {
        if (crystalGem == null ||
            targetGem == null ||
            targetGem.SpecialType !=
                GemSpecialType.None)
        {
            return false;
        }

        HashSet<Gem> targetMatches =
            FindMatchesFrom(
                targetGem,
                null
            );

        if (targetMatches.Count != 3)
        {
            return false;
        }

        foreach (Gem matchedGem in targetMatches)
        {
            if (matchedGem == null)
            {
                continue;
            }

            bool rowBombReachesCrystal =
                matchedGem.SpecialType ==
                    GemSpecialType.RowBomb &&
                matchedGem.Row ==
                    crystalGem.Row;

            bool columnBombReachesCrystal =
                matchedGem.SpecialType ==
                    GemSpecialType.ColumnBomb &&
                matchedGem.Column ==
                    crystalGem.Column;

            if (rowBombReachesCrystal ||
                columnBombReachesCrystal)
            {
                return true;
            }
        }

        return false;
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

        /*
         * If this crystal swap also created a normal three whose
         * existing bomb will hit the crystal, deliberately decline
         * the direct crystal activation here. TrySwap will then fall
         * through to the ordinary match path, where ResolveCascades
         * explodes the bomb first and queues this crystal from the hit.
         */
        if (ShouldResolveMatchedBombBeforeCrystal(
                crystalGem,
                targetGem))
        {
            Debug.Log(
                "Crystal swap created a three-match whose existing " +
                "bomb reaches the crystal. Resolving the match and " +
                "bomb before the crystal activation."
            );

            return false;
        }

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

            if (targetGem.SpecialType ==
                GemSpecialType.PoisonBomb)
            {
                pendingConvertedBombs.Add(
                    targetGem
                );

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
            if (IsChainReactiveBomb(
                targetGem.SpecialType))
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
                0,
                BoardClearSource.ColorCrystal
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

                case GemSpecialType.PoisonBomb:
                    AddPoisonBombAreaToConvertedClearSet(
                        bomb,
                        activatedBomb,
                        pendingConvertedBombs,
                        triggeredCrystalRequests,
                        gemsToClear,
                        pendingBombs
                    );

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
            IsChainReactiveBomb(
                gem.SpecialType))
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

            if (targetGem.SpecialType !=
                GemSpecialType.PoisonBomb)
            {
                targetGem.SetSpecialType(
                    convertedBombType
                );
            }

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
                0,
                BoardClearSource.ColorCrystal
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

            if (!request.IsValid)
            {
                continue;
            }

            Gem waitingCrystal =
                request.CrystalGem;

            /*
             * Start visibly charging before the board begins
             * collapsing and refilling.
             */
            Coroutine chargeRoutine =
                StartCoroutine(
                    AnimateTriggeredCrystalCharge(
                        waitingCrystal
                    )
                );

            /*
             * The explosion that found this crystal has already
             * created empty spaces. Refill them before selecting
             * gems of the triggering bomb's color.
             *
             * The crystal remains alive and moves normally with
             * the collapsing board.
             */
            yield return CollapseAndRefillBoard();

            if (triggeredCrystalSettlePause > 0f)
            {
                yield return new WaitForSeconds(
                    triggeredCrystalSettlePause
                );
            }

            /*
             * End the charge animation while keeping the crystal
             * at its new settled board position.
             */
            chargingTriggeredCrystals.Remove(
                waitingCrystal
            );

            if (chargeRoutine != null)
            {
                yield return chargeRoutine;
            }

            /*
             * Another part of the chain may already have consumed
             * this crystal while the board was resolving.
             */
            if (!request.IsValid)
            {
                continue;
            }

            /*
             * BuildBombTriggeredCrystalTargetSet is called inside
             * this sequence, so it now scans the newly refilled
             * board rather than the depleted board.
             */
            yield return
                ResolveBombTriggeredCrystalSequence(
                    request
                );
        }
    }

    private IEnumerator
        AnimateTriggeredCrystalCharge(
            Gem crystal)
    {
        if (crystal == null)
        {
            yield break;
        }

        Vector3 originalScale =
            crystal.transform.localScale;

        chargingTriggeredCrystals.Add(
            crystal
        );

        while (crystal != null &&
               chargingTriggeredCrystals.Contains(
                   crystal))
        {
            float pulse =
                (
                    Mathf.Sin(
                        Time.time *
                        triggeredCrystalChargePulseSpeed *
                        Mathf.PI *
                        2f
                    ) +
                    1f
                ) *
                0.5f;

            float scaleMultiplier =
                Mathf.Lerp(
                    1f,
                    triggeredCrystalChargeScale,
                    pulse
                );

            crystal.transform.localScale =
                originalScale *
                scaleMultiplier;

            yield return null;
        }

        if (crystal != null)
        {
            crystal.transform.localScale =
                originalScale;
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

        yield return
            PlayColorCrystalActivationVFX(
                crystalTargetSet
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
                0,
                BoardClearSource.ColorCrystal
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

        yield return
            PlayColorCrystalActivationVFX(
                crystalClearSet
            );

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
            0,
            BoardClearSource.ColorCrystal
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

            if (IsChainReactiveBomb(
                    gem.SpecialType))
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

                case GemSpecialType.PoisonBomb:
                    AddPoisonBombAreaToClearSet(
                        bomb,
                        preserveTriggeredCrystals,
                        triggeredCrystalRequests,
                        gemsToClear,
                        pendingBombs
                    );

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
            IsChainReactiveBomb(
                gem.SpecialType))
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
