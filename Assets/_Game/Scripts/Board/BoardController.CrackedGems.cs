using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    [Header("Cracked Gems")]
    [SerializeField]
    [Tooltip(
        "Optional cracked replacement sprites in GemType order: Ruby, Amber, " +
        "Topaz, Emerald, Sapphire, Amethyst. Gameplay still works while these " +
        "are unassigned."
    )]
    private Sprite[] crackedGemSprites =
        new Sprite[6];

    [SerializeField, Range(0f, 0.15f)]
    private float crackedShakeAmplitudeInCells =
        0.045f;

    public event Action<
        IReadOnlyList<Vector3>,
        float,
        float
    > CrackedGemTargetsSelected;

    public bool CanActivateCrackedGems(
        int requestedTargetCount)
    {
        if (requestedTargetCount <= 0 ||
            gems == null ||
            IsBusy)
        {
            return false;
        }

        return HasAnyGemForCrackedAbility();
    }

    public bool TryActivateCrackedGems(
        IReadOnlyCollection<GemType>
            preferredGemTypes,
        int requestedTargetCount,
        int fixedDamagePerCrackedGem,
        float bubbleTravelDuration,
        float bubbleHoverDuration,
        float shakeDuration,
        float burstScale,
        float whiteHoldDuration,
        Action completed)
    {
        if (!CanActivateCrackedGems(
                requestedTargetCount))
        {
            return false;
        }

        List<Gem> targets =
            SelectCrackedAbilityTargets(
                preferredGemTypes,
                requestedTargetCount
            );

        if (targets.Count == 0)
        {
            return false;
        }

        /*
         * Acquire board ownership synchronously. PlayerAbilityController spends
         * energy immediately after TryActivate succeeds, so no other board
         * mutation may slip in between acceptance and this ability coroutine.
         */
        isBusy = true;

        StartCoroutine(
            ResolveCrackedGemAbility(
                targets,
                Mathf.Max(
                    1,
                    fixedDamagePerCrackedGem
                ),
                Mathf.Max(
                    0f,
                    bubbleTravelDuration
                ),
                Mathf.Max(
                    0f,
                    bubbleHoverDuration
                ),
                Mathf.Max(
                    0f,
                    shakeDuration
                ),
                Mathf.Clamp(
                    burstScale,
                    1f,
                    1.2f
                ),
                Mathf.Max(
                    0f,
                    whiteHoldDuration
                ),
                completed
            )
        );

        return true;
    }

    private IEnumerator ResolveCrackedGemAbility(
        List<Gem> targets,
        int fixedDamagePerCrackedGem,
        float bubbleTravelDuration,
        float bubbleHoverDuration,
        float shakeDuration,
        float burstScale,
        float whiteHoldDuration,
        Action completed)
    {
        try
        {
            List<Vector3> targetPositions =
                new List<Vector3>();

            foreach (Gem target in targets)
            {
                if (target != null)
                {
                    targetPositions.Add(
                        target.transform.position
                    );
                }
            }

            CrackedGemTargetsSelected?.Invoke(
                targetPositions,
                bubbleTravelDuration,
                bubbleHoverDuration
            );

            float bubbleSequenceDuration =
                bubbleTravelDuration +
                bubbleHoverDuration;

            if (bubbleSequenceDuration > 0f)
            {
                yield return new WaitForSeconds(
                    bubbleSequenceDuration
                );
            }

            List<Gem> validTargets =
                new List<Gem>();

            foreach (Gem target in targets)
            {
                if (target == null ||
                    !IsGemStillOnBoard(target))
                {
                    continue;
                }

                SetGemCracked(target);
                validTargets.Add(target);
            }

            if (validTargets.Count == 0)
            {
                yield break;
            }

            HashSet<Gem> crackedCenters;
            List<BombTriggeredCrystalRequest>
                triggeredCrystalRequests;

            HashSet<Gem> expandedClearSet =
                BuildCrackedExpandedClearSet(
                    validTargets,
                    out crackedCenters,
                    out triggeredCrystalRequests
                );

            yield return AnimateCrackedGems(
                crackedCenters,
                shakeDuration,
                burstScale,
                whiteHoldDuration
            );

            ReportCrackedClearSetToCombat(
                expandedClearSet,
                crackedCenters,
                fixedDamagePerCrackedGem
            );

            yield return ClearMatches(
                expandedClearSet,
                null
            );

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

            float settlePause =
                GetPostFallSettlePause();

            if (settlePause > 0f)
            {
                yield return new WaitForSeconds(
                    settlePause
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
        }
        finally
        {
            isBusy = false;
            completed?.Invoke();
        }
    }

    private bool HasAnyGemForCrackedAbility()
    {
        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                if (GetGem(column, row) != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private List<Gem> SelectCrackedAbilityTargets(
        IReadOnlyCollection<GemType>
            preferredGemTypes,
        int requestedTargetCount)
    {
        HashSet<GemType> preferredTypes =
            preferredGemTypes != null
                ? new HashSet<GemType>(
                    preferredGemTypes
                )
                : new HashSet<GemType>();

        List<Gem> preferredNormalGems =
            new List<Gem>();

        List<Gem> otherNormalGems =
            new List<Gem>();

        List<Gem> specialGems =
            new List<Gem>();

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                Gem gem =
                    GetGem(column, row);

                if (gem == null)
                {
                    continue;
                }

                if (gem.SpecialType !=
                    GemSpecialType.None)
                {
                    specialGems.Add(gem);
                    continue;
                }

                if (preferredTypes.Contains(
                        gem.Type))
                {
                    preferredNormalGems.Add(gem);
                }
                else
                {
                    otherNormalGems.Add(gem);
                }
            }
        }

        List<Gem> selected =
            new List<Gem>();

        int safeTargetCount =
            Mathf.Max(
                1,
                requestedTargetCount
            );

        if (preferredNormalGems.Count > 0 ||
            otherNormalGems.Count > 0)
        {
            TakeRandomUniqueGems(
                preferredNormalGems,
                selected,
                safeTargetCount
            );

            TakeRandomUniqueGems(
                otherNormalGems,
                selected,
                safeTargetCount
            );

            return selected;
        }

        /*
         * Specials are a true last resort: if even one ordinary gem exists,
         * Bardley never chooses a bomb/crystal merely to reach five targets.
         */
        TakeRandomUniqueGems(
            specialGems,
            selected,
            safeTargetCount
        );

        return selected;
    }

    private static void TakeRandomUniqueGems(
        List<Gem> source,
        List<Gem> destination,
        int maximumTotalCount)
    {
        while (source.Count > 0 &&
               destination.Count <
                   maximumTotalCount)
        {
            int selectedIndex =
                UnityEngine.Random.Range(
                    0,
                    source.Count
                );

            destination.Add(
                source[selectedIndex]
            );

            source.RemoveAt(selectedIndex);
        }
    }

    private bool IsGemStillOnBoard(
        Gem gem)
    {
        return
            gem != null &&
            gem.Column >= 0 &&
            gem.Column < width &&
            gem.Row >= 0 &&
            gem.Row < height &&
            GetGem(
                gem.Column,
                gem.Row
            ) == gem;
    }

    private void SetGemCracked(
        Gem gem)
    {
        if (gem == null)
        {
            return;
        }

        gem.SetSpecialType(
            GemSpecialType.Cracked
        );

        int spriteIndex =
            (int)gem.Type;

        if (crackedGemSprites == null ||
            spriteIndex < 0 ||
            spriteIndex >=
                crackedGemSprites.Length ||
            crackedGemSprites[spriteIndex] == null)
        {
            return;
        }

        SpriteRenderer renderer =
            gem.GetComponent<SpriteRenderer>();

        if (renderer != null)
        {
            renderer.sprite =
                crackedGemSprites[spriteIndex];
        }
    }

    private HashSet<Gem>
        BuildCrackedExpandedClearSet(
            IEnumerable<Gem> crackedSeeds,
            out HashSet<Gem> crackedCenters,
            out List<BombTriggeredCrystalRequest>
                triggeredCrystalRequests)
    {
        HashSet<Gem> gemsToClear =
            new HashSet<Gem>();

        crackedCenters =
            new HashSet<Gem>();

        triggeredCrystalRequests =
            new List<BombTriggeredCrystalRequest>();

        Queue<Gem> pendingSpecials =
            new Queue<Gem>();

        HashSet<Gem> triggeredSpecials =
            new HashSet<Gem>();

        HashSet<Gem> crackedActivatedCrystals =
            new HashSet<Gem>();

        foreach (Gem seed in crackedSeeds)
        {
            if (seed == null)
            {
                continue;
            }

            gemsToClear.Add(seed);
            pendingSpecials.Enqueue(seed);
        }

        while (pendingSpecials.Count > 0)
        {
            Gem special =
                pendingSpecials.Dequeue();

            if (special == null ||
                !triggeredSpecials.Add(
                    special))
            {
                continue;
            }

            switch (special.SpecialType)
            {
                case GemSpecialType.Cracked:
                    crackedCenters.Add(special);

                    AddAreaToCrackedClearSet(
                        special,
                        pendingSpecials,
                        triggeredCrystalRequests,
                        crackedActivatedCrystals,
                        gemsToClear
                    );
                    break;

                case GemSpecialType.RowBomb:
                    for (int column = 0;
                         column < width;
                         column++)
                    {
                        TryAddGemToCrackedClearSet(
                            column,
                            special.Row,
                            special,
                            pendingSpecials,
                            triggeredCrystalRequests,
                            crackedActivatedCrystals,
                            gemsToClear
                        );
                    }
                    break;

                case GemSpecialType.ColumnBomb:
                    for (int row = 0;
                         row < height;
                         row++)
                    {
                        TryAddGemToCrackedClearSet(
                            special.Column,
                            row,
                            special,
                            pendingSpecials,
                            triggeredCrystalRequests,
                            crackedActivatedCrystals,
                            gemsToClear
                        );
                    }
                    break;

                case GemSpecialType.PoisonBomb:
                    ApplyPoisonBombStatus();
                    AddAreaToCrackedClearSet(
                        special,
                        pendingSpecials,
                        triggeredCrystalRequests,
                        crackedActivatedCrystals,
                        gemsToClear
                    );
                    break;

                case GemSpecialType.HealingBomb:
                    ApplyHealingBombEffect();
                    AddAreaToCrackedClearSet(
                        special,
                        pendingSpecials,
                        triggeredCrystalRequests,
                        crackedActivatedCrystals,
                        gemsToClear
                    );
                    break;

                case GemSpecialType.ShieldBomb:
                    ApplyShieldBombEffect();
                    AddAreaToCrackedClearSet(
                        special,
                        pendingSpecials,
                        triggeredCrystalRequests,
                        crackedActivatedCrystals,
                        gemsToClear
                    );
                    break;
            }
        }

        return gemsToClear;
    }

    private void AddAreaToCrackedClearSet(
        Gem center,
        Queue<Gem> pendingSpecials,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
        HashSet<Gem> crackedActivatedCrystals,
        HashSet<Gem> gemsToClear)
    {
        for (int rowOffset = -1;
             rowOffset <= 1;
             rowOffset++)
        {
            for (int columnOffset = -1;
                 columnOffset <= 1;
                 columnOffset++)
            {
                TryAddGemToCrackedClearSet(
                    center.Column + columnOffset,
                    center.Row + rowOffset,
                    center,
                    pendingSpecials,
                    triggeredCrystalRequests,
                    crackedActivatedCrystals,
                    gemsToClear
                );
            }
        }
    }

    private void TryAddGemToCrackedClearSet(
        int column,
        int row,
        Gem triggeringSpecial,
        Queue<Gem> pendingSpecials,
        List<BombTriggeredCrystalRequest>
            triggeredCrystalRequests,
        HashSet<Gem> crackedActivatedCrystals,
        HashSet<Gem> gemsToClear)
    {
        Gem gem =
            GetGem(column, row);

        if (gem == null)
        {
            return;
        }

        if (gem.SpecialType ==
            GemSpecialType.ColorCrystal)
        {
            if (triggeringSpecial != null &&
                triggeringSpecial.SpecialType ==
                    GemSpecialType.Cracked)
            {
                ActivateColorCrystalFromCrackedGem(
                    gem,
                    triggeringSpecial.Type,
                    pendingSpecials,
                    crackedActivatedCrystals,
                    gemsToClear
                );
            }
            else
            {
                TryAddBombTriggeredCrystalRequest(
                    gem,
                    triggeringSpecial,
                    triggeredCrystalRequests
                );
            }

            return;
        }

        bool wasAdded =
            gemsToClear.Add(gem);

        if (wasAdded &&
            IsCrackedResolverSpecial(
                gem.SpecialType))
        {
            pendingSpecials.Enqueue(gem);
        }
    }

    private void ActivateColorCrystalFromCrackedGem(
        Gem crystal,
        GemType triggeringGemType,
        Queue<Gem> pendingSpecials,
        HashSet<Gem> activatedCrystals,
        HashSet<Gem> gemsToClear)
    {
        if (crystal == null ||
            !activatedCrystals.Add(crystal))
        {
            return;
        }

        /*
         * Bardley's star interaction: a color crystal struck by a cracked gem
         * cracks every non-crystal gem of the triggering cracked gem's color,
         * then all of those cracked gems participate in this same deterministic
         * detonation wave.
         */
        gemsToClear.Add(crystal);

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                Gem target =
                    GetGem(column, row);

                if (target == null ||
                    target == crystal ||
                    target.Type !=
                        triggeringGemType ||
                    target.SpecialType ==
                        GemSpecialType.ColorCrystal)
                {
                    continue;
                }

                SetGemCracked(target);
                gemsToClear.Add(target);
                pendingSpecials.Enqueue(target);
            }
        }
    }

    private static bool IsCrackedResolverSpecial(
        GemSpecialType specialType)
    {
        return
            specialType == GemSpecialType.Cracked ||
            specialType == GemSpecialType.RowBomb ||
            specialType == GemSpecialType.ColumnBomb ||
            specialType == GemSpecialType.PoisonBomb ||
            specialType == GemSpecialType.HealingBomb ||
            specialType == GemSpecialType.ShieldBomb;
    }

    private IEnumerator AnimateCrackedGems(
        HashSet<Gem> crackedCenters,
        float shakeDuration,
        float burstScale,
        float whiteHoldDuration)
    {
        if (crackedCenters == null ||
            crackedCenters.Count == 0)
        {
            yield break;
        }

        List<Gem> ordered =
            new List<Gem>(crackedCenters);

        ordered.Sort(CompareGemsByGridPosition);

        Dictionary<Gem, Vector3> startPositions =
            new Dictionary<Gem, Vector3>();

        Dictionary<Gem, Vector3> startScales =
            new Dictionary<Gem, Vector3>();

        foreach (Gem gem in ordered)
        {
            if (gem == null)
            {
                continue;
            }

            startPositions[gem] =
                gem.transform.localPosition;

            startScales[gem] =
                gem.transform.localScale;
        }

        if (shakeDuration > 0f)
        {
            float elapsed = 0f;
            float amplitude =
                cellSize *
                crackedShakeAmplitudeInCells;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;

                float normalized =
                    Mathf.Clamp01(
                        elapsed /
                        shakeDuration
                    );

                for (int index = 0;
                     index < ordered.Count;
                     index++)
                {
                    Gem gem = ordered[index];

                    if (gem == null ||
                        !startPositions.ContainsKey(gem))
                    {
                        continue;
                    }

                    float phase =
                        normalized * 52f +
                        index * 1.73f;

                    Vector3 offset =
                        new Vector3(
                            Mathf.Sin(phase) * amplitude,
                            Mathf.Cos(phase * 1.31f) * amplitude,
                            0f
                        );

                    gem.transform.localPosition =
                        startPositions[gem] +
                        offset;

                    gem.transform.localScale =
                        Vector3.Lerp(
                            startScales[gem],
                            startScales[gem] *
                                burstScale,
                            normalized
                        );
                }

                yield return null;
            }
        }

        foreach (Gem gem in ordered)
        {
            if (gem == null ||
                !startPositions.ContainsKey(gem))
            {
                continue;
            }

            gem.transform.localPosition =
                startPositions[gem];

            gem.transform.localScale =
                startScales[gem] *
                burstScale;

            gem.SetVFXFlashAmount(1f);
        }

        if (whiteHoldDuration > 0f)
        {
            yield return new WaitForSeconds(
                whiteHoldDuration
            );
        }
    }

    private void ReportCrackedClearSetToCombat(
        HashSet<Gem> expandedClearSet,
        HashSet<Gem> crackedCenters,
        int fixedDamagePerCrackedGem)
    {
        if (expandedClearSet == null ||
            expandedClearSet.Count == 0)
        {
            return;
        }

        DamageBarricadesAdjacentToClears(
            expandedClearSet
        );

        ReportBombClearSetToVFX(
            expandedClearSet,
            BoardClearSource.Ability
        );

        Dictionary<GemType, int>
            ordinaryClearCounts =
                new Dictionary<GemType, int>();

        foreach (Gem gem in expandedClearSet)
        {
            if (gem == null ||
                (crackedCenters != null &&
                 crackedCenters.Contains(gem)) ||
                gem.SpecialType ==
                    GemSpecialType.ColorCrystal)
            {
                continue;
            }

            if (!ordinaryClearCounts.ContainsKey(
                    gem.Type))
            {
                ordinaryClearCounts[gem.Type] = 0;
            }

            ordinaryClearCounts[gem.Type]++;
        }

        foreach (
            KeyValuePair<GemType, int> result
            in ordinaryClearCounts)
        {
            BoardClearContext context =
                new BoardClearContext(
                    result.Key,
                    result.Value,
                    0,
                    BoardClearSource.Ability,
                    BoardMatchType.Other
                );

            ReportCrackedCombatContext(
                context,
                false,
                0
            );
        }

        if (crackedCenters == null)
        {
            return;
        }

        List<Gem> orderedCenters =
            new List<Gem>(crackedCenters);

        orderedCenters.Sort(
            CompareGemsByGridPosition
        );

        foreach (Gem crackedGem in orderedCenters)
        {
            if (crackedGem == null)
            {
                continue;
            }

            BoardClearContext context =
                new BoardClearContext(
                    crackedGem.Type,
                    1,
                    0,
                    BoardClearSource.Ability,
                    BoardMatchType.Other
                );

            ReportCrackedCombatContext(
                context,
                true,
                fixedDamagePerCrackedGem
            );
        }
    }

    private void ReportCrackedCombatContext(
        BoardClearContext context,
        bool useFixedDamage,
        int fixedDamage)
    {
        BoardClearResolved?.Invoke(context);

        bool damagedMatchingEnemy = false;

        if (combatController != null)
        {
            damagedMatchingEnemy =
                useFixedDamage
                    ? combatController
                        .ResolveFixedGemDamage(
                            context,
                            fixedDamage
                        )
                    : combatController
                        .ResolveGemClear(context);
        }

        BoardClearOutcomeResolved?.Invoke(
            new BoardClearOutcome(
                context,
                damagedMatchingEnemy
            )
        );
    }
}
