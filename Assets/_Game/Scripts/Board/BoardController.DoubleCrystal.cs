using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    [Header("Double Color Crystal")]

    [SerializeField, Min(0f)]
    [Tooltip(
        "Pause between each cleared column and row."
    )]
    private float doubleCrystalSweepDelay =
        0.08f;

    [SerializeField, Range(1f, 1.5f)]
    [Tooltip(
        "How large the waiting crystal becomes."
    )]
    private float doubleCrystalChargeScale =
        1.18f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "How long the waiting crystal takes to grow."
    )]
    private float doubleCrystalGrowDuration =
        0.22f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "How far the waiting crystal shakes."
    )]
    private float doubleCrystalShakeDistance =
        0.035f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "How quickly the waiting crystal shakes."
    )]
    private float doubleCrystalShakeSpeed =
        34f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Small pause after the first refill settles."
    )]
    private float doubleCrystalSettlePause =
        0.18f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Stagger between gems entering during the " +
        "middle refill."
    )]
    private float doubleCrystalRefillStagger =
        0.015f;

    private bool isDoubleCrystalCharging;

    private readonly HashSet<Gem>
    chargingQueuedDoubleCrystals =
        new HashSet<Gem>();

    private readonly Dictionary<Gem, Coroutine>
        queuedDoubleCrystalChargeRoutines =
            new Dictionary<Gem, Coroutine>();

    private static bool
        IsDoubleColorCrystalSwap(
            Gem first,
            Gem second)
    {
        return first != null &&
               second != null &&
               first.SpecialType ==
                   GemSpecialType.ColorCrystal &&
               second.SpecialType ==
                   GemSpecialType.ColorCrystal;
    }

    private IEnumerator
        ResolveDoubleColorCrystalActivation(
            Gem firstCrystal,
            Gem waitingCrystal)
    {
        if (!IsDoubleColorCrystalSwap(
                firstCrystal,
                waitingCrystal))
        {
            yield break;
        }

        /*
         * Capture every additional crystal before any sweep
         * begins. These crystals will remain protected until
         * their own full-board activation turn.
         */
        List<Gem> queuedCrystals =
            BuildAdditionalColorCrystalQueue(
                firstCrystal,
                waitingCrystal
            );

        HashSet<Gem> preservedCrystals =
            new HashSet<Gem>(
                queuedCrystals
            );

        /*
         * The second crystal in the original swap must also
         * survive the first sweep.
         */
        preservedCrystals.Add(
            waitingCrystal
        );

        /*
         * The crystal moved by the player activates first.
         */
        yield return ClearCrystalWithoutRewards(
            firstCrystal
        );

        /*
         * The second swapped crystal performs the existing
         * charge animation while waiting for sweep two.
         */
        isDoubleCrystalCharging = true;

        Coroutine waitingCrystalChargeRoutine =
            StartCoroutine(
                AnimateDoubleCrystalCharge(
                    waitingCrystal
                )
            );

        /*
         * Every additional crystal begins charging now and
         * remains charged until its own turn.
         */
        foreach (Gem queuedCrystal in queuedCrystals)
        {
            StartQueuedDoubleCrystalCharge(
                queuedCrystal
            );
        }

        /*
         * First sweep: clear everything except the waiting
         * swapped crystal and all additional queued crystals.
         */
        HashSet<Gem> firstSweepTargets =
            BuildCurrentGemSetExcept(
                preservedCrystals
            );

        ReportDoubleCrystalSweepToCombat(
            firstSweepTargets
        );

        yield return ClearBoardColumnByColumn(
            preservedCrystals
        );

        /*
         * Fill every cleared position without moving any of
         * the preserved crystals.
         */
        yield return
            RefillEmptyCellsAroundCrystal();

        if (doubleCrystalSettlePause > 0f)
        {
            yield return new WaitForSeconds(
                doubleCrystalSettlePause
            );
        }

        isDoubleCrystalCharging = false;

        if (waitingCrystalChargeRoutine != null)
        {
            yield return
                waitingCrystalChargeRoutine;
        }

        /*
         * The second crystal from the original swap now
         * activates.
         */
        yield return ClearCrystalWithoutRewards(
            waitingCrystal
        );

        preservedCrystals.Remove(
            waitingCrystal
        );

        /*
         * Second sweep: clear the refilled board while still
         * protecting every additional queued crystal.
         */
        HashSet<Gem> secondSweepTargets =
            BuildCurrentGemSetExcept(
                preservedCrystals
            );

        ReportDoubleCrystalSweepToCombat(
            secondSweepTargets
        );

        yield return ClearBoardRowByRow(
            preservedCrystals
        );

        if (cascadePause > 0f)
        {
            yield return new WaitForSeconds(
                cascadePause
            );
        }

        /*
         * Every additional crystal now receives one complete
         * full-board activation. The board refills before each
         * queued crystal activates.
         */
        yield return
            ResolveQueuedDoubleCrystalChain(
                queuedCrystals
            );

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
            $"Double color crystal activation complete. " +
            $"{queuedCrystals.Count} additional crystal(s) " +
            $"joined the chain."
        );
    }

    private IEnumerator ClearCrystalWithoutRewards(
        Gem crystal)
    {
        if (crystal == null)
        {
            yield break;
        }

        HashSet<Gem> crystalOnly =
            new HashSet<Gem>
            {
                crystal
            };

        /*
         * Do not call ReportBombClearsToCombat here.
         * A crystal's hidden original GemType must not
         * grant combat damage or energy.
         */
        yield return ClearMatches(
            crystalOnly,
            null
        );
    }

    private IEnumerator ClearBoardColumnByColumn(
        HashSet<Gem> gemsToPreserve)
    {
        for (int column = 0;
             column < width;
             column++)
        {
            HashSet<Gem> columnGems =
                new HashSet<Gem>();

            for (int row = 0;
                 row < height;
                 row++)
            {
                Gem gem =
                    GetGem(
                        column,
                        row
                    );

                if (gem == null)
                {
                    continue;
                }

                /*
                 * Waiting and queued crystals survive the
                 * sweep instead of entering ClearMatches.
                 */
                if (gemsToPreserve != null &&
                    gemsToPreserve.Contains(gem))
                {
                    continue;
                }

                columnGems.Add(gem);
            }

            if (columnGems.Count > 0)
            {
                yield return ClearMatches(
                    columnGems,
                    null
                );
            }

            if (doubleCrystalSweepDelay > 0f &&
                column < width - 1)
            {
                yield return new WaitForSeconds(
                    doubleCrystalSweepDelay
                );
            }
        }
    }

    private IEnumerator ClearBoardRowByRow(
        HashSet<Gem> gemsToPreserve)
    {
        /*
         * Internal row zero is the bottom of the board,
         * so iterating downward from height - 1 produces
         * a visible top-to-bottom sweep.
         */
        for (int row = height - 1;
             row >= 0;
             row--)
        {
            HashSet<Gem> rowGems =
                new HashSet<Gem>();

            for (int column = 0;
                 column < width;
                 column++)
            {
                Gem gem =
                    GetGem(
                        column,
                        row
                    );

                if (gem == null)
                {
                    continue;
                }

                if (gemsToPreserve != null &&
                    gemsToPreserve.Contains(gem))
                {
                    continue;
                }

                rowGems.Add(gem);
            }

            if (rowGems.Count > 0)
            {
                yield return ClearMatches(
                    rowGems,
                    null
                );
            }

            if (doubleCrystalSweepDelay > 0f &&
                row > 0)
            {
                yield return new WaitForSeconds(
                    doubleCrystalSweepDelay
                );
            }
        }
    }

    private IEnumerator
        RefillEmptyCellsAroundCrystal()
    {
        List<GemMove> refillMoves =
            new List<GemMove>();

        for (int column = 0;
             column < width;
             column++)
        {
            int spawnedGemIndex = 0;

            for (int row = 0;
                 row < height;
                 row++)
            {
                if (gems[column, row] != null)
                {
                    continue;
                }

                Vector3 targetPosition =
                    GetLocalPosition(
                        column,
                        row
                    );

                Vector3 topPosition =
                    GetLocalPosition(
                        column,
                        height - 1
                    );

                Vector3 startPosition =
                    topPosition +
                    Vector3.up *
                    (
                        (spawnedGemIndex + 1) *
                        cellSize
                    );

                Gem newGem =
                    CreateGem(
                        column,
                        row,
                        GetRandomGemType(),
                        startPosition
                    );

                refillMoves.Add(
                    new GemMove
                    {
                        Gem = newGem,

                        StartPosition =
                            startPosition,

                        TargetPosition =
                            targetPosition,

                        Delay =
                            spawnedGemIndex *
                            doubleCrystalRefillStagger,

                        Duration =
                            CalculateFallDuration(
                                startPosition,
                                targetPosition
                            )
                    }
                );

                spawnedGemIndex++;
            }
        }

        yield return AnimateGemMoves(
            refillMoves
        );
    }

    private IEnumerator
        AnimateDoubleCrystalCharge(
            Gem crystal)
    {
        if (crystal == null)
        {
            yield break;
        }

        Vector3 originalPosition =
            crystal.transform.localPosition;

        Vector3 originalScale =
            crystal.transform.localScale;

        Vector3 chargedScale =
            originalScale *
            doubleCrystalChargeScale;

        float elapsedTime = 0f;

        while (isDoubleCrystalCharging &&
               crystal != null)
        {
            float growthProgress;

            if (doubleCrystalGrowDuration <= 0f)
            {
                growthProgress = 1f;
            }
            else
            {
                growthProgress =
                    Mathf.Clamp01(
                        elapsedTime /
                        doubleCrystalGrowDuration
                    );
            }

            crystal.transform.localScale =
                Vector3.Lerp(
                    originalScale,
                    chargedScale,
                    SmoothStep(growthProgress)
                );

            float horizontalShake =
                Mathf.Sin(
                    elapsedTime *
                    doubleCrystalShakeSpeed
                ) *
                doubleCrystalShakeDistance;

            float verticalShake =
                Mathf.Sin(
                    elapsedTime *
                    doubleCrystalShakeSpeed *
                    1.37f
                ) *
                doubleCrystalShakeDistance;

            crystal.transform.localPosition =
                originalPosition +
                new Vector3(
                    horizontalShake,
                    verticalShake,
                    0f
                );

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        if (crystal != null)
        {
            crystal.transform.localPosition =
                originalPosition;

            /*
             * Keep it enlarged until ClearMatches performs
             * the second crystal explosion.
             */
            crystal.transform.localScale =
                chargedScale;
        }
    }

    private List<Gem>
    BuildAdditionalColorCrystalQueue(
        Gem firstCrystal,
        Gem waitingCrystal)
    {
        List<Gem> queuedCrystals =
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
                    GetGem(
                        column,
                        row
                    );

                if (gem == null ||
                    gem == firstCrystal ||
                    gem == waitingCrystal ||
                    gem.SpecialType !=
                        GemSpecialType.ColorCrystal)
                {
                    continue;
                }

                queuedCrystals.Add(gem);
            }
        }

        /*
         * Use a stable board order so repeated tests produce
         * understandable activation sequencing.
         */
        queuedCrystals.Sort(
            CompareGemsByGridPosition
        );

        return queuedCrystals;
    }

    private void StartQueuedDoubleCrystalCharge(
    Gem crystal)
    {
        if (crystal == null ||
            queuedDoubleCrystalChargeRoutines
                .ContainsKey(crystal))
        {
            return;
        }

        chargingQueuedDoubleCrystals.Add(
            crystal
        );

        Coroutine chargeRoutine =
            StartCoroutine(
                AnimateQueuedDoubleCrystalCharge(
                    crystal
                )
            );

        queuedDoubleCrystalChargeRoutines.Add(
            crystal,
            chargeRoutine
        );
    }


    private IEnumerator
    StopQueuedDoubleCrystalCharge(
        Gem crystal)
    {
        if (crystal == null)
        {
            yield break;
        }

        chargingQueuedDoubleCrystals.Remove(
            crystal
        );

        if (!queuedDoubleCrystalChargeRoutines
                .TryGetValue(
                    crystal,
                    out Coroutine chargeRoutine))
        {
            yield break;
        }

        queuedDoubleCrystalChargeRoutines.Remove(
            crystal
        );

        if (chargeRoutine != null)
        {
            yield return chargeRoutine;
        }
    }

    private IEnumerator
    AnimateQueuedDoubleCrystalCharge(
        Gem crystal)
    {
        if (crystal == null)
        {
            yield break;
        }

        Vector3 originalScale =
            crystal.transform.localScale;

        while (crystal != null &&
               chargingQueuedDoubleCrystals.Contains(
                   crystal))
        {
            float pulse =
                (
                    Mathf.Sin(
                        Time.time *
                        doubleCrystalShakeSpeed *
                        0.18f
                    ) +
                    1f
                ) *
                0.5f;

            float scaleMultiplier =
                Mathf.Lerp(
                    1f,
                    doubleCrystalChargeScale,
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

    private bool IsQueuedCrystalValid(
    Gem crystal)
    {
        if (crystal == null ||
            crystal.SpecialType !=
                GemSpecialType.ColorCrystal)
        {
            return false;
        }

        return GetGem(
            crystal.Column,
            crystal.Row
        ) == crystal;
    }

    private IEnumerator
    ResolveQueuedDoubleCrystalChain(
        List<Gem> queuedCrystals)
    {
        HashSet<Gem> remainingCrystals =
            new HashSet<Gem>();

        if (queuedCrystals != null)
        {
            foreach (Gem crystal in queuedCrystals)
            {
                if (IsQueuedCrystalValid(crystal))
                {
                    remainingCrystals.Add(
                        crystal
                    );
                }
            }
        }

        int completedExtraSweeps = 0;

        if (queuedCrystals != null)
        {
            foreach (Gem queuedCrystal
                     in queuedCrystals)
            {
                if (!remainingCrystals.Contains(
                        queuedCrystal))
                {
                    continue;
                }

                if (!IsQueuedCrystalValid(
                        queuedCrystal))
                {
                    remainingCrystals.Remove(
                        queuedCrystal
                    );

                    yield return
                        StopQueuedDoubleCrystalCharge(
                            queuedCrystal
                        );

                    continue;
                }

                /*
                 * The preceding sweep left the board empty
                 * except for queued crystals. Refill first so
                 * this crystal receives a rewarding full board
                 * to destroy.
                 */
                yield return
                    RefillEmptyCellsAroundCrystal();

                if (doubleCrystalSettlePause > 0f)
                {
                    yield return new WaitForSeconds(
                        doubleCrystalSettlePause
                    );
                }

                yield return
                    StopQueuedDoubleCrystalCharge(
                        queuedCrystal
                    );

                /*
                 * Consume this crystal without granting rewards
                 * for its hidden original color.
                 */
                yield return
                    ClearCrystalWithoutRewards(
                        queuedCrystal
                    );

                remainingCrystals.Remove(
                    queuedCrystal
                );

                HashSet<Gem> sweepTargets =
                    BuildCurrentGemSetExcept(
                        remainingCrystals
                    );

                ReportDoubleCrystalSweepToCombat(
                    sweepTargets
                );

                /*
                 * Alternate sweep direction so consecutive
                 * crystal activations are visually distinct.
                 */
                if (completedExtraSweeps % 2 == 0)
                {
                    yield return
                        ClearBoardColumnByColumn(
                            remainingCrystals
                        );
                }
                else
                {
                    yield return
                        ClearBoardRowByRow(
                            remainingCrystals
                        );
                }

                completedExtraSweeps++;

                if (cascadePause > 0f)
                {
                    yield return new WaitForSeconds(
                        cascadePause
                    );
                }
            }
        }

        /*
         * The last queued crystal has now emptied the board.
         * Perform the normal final refill.
         */
        yield return CollapseAndRefillBoard();
    }

    private HashSet<Gem>
        BuildCurrentGemSetExcept(
            HashSet<Gem> gemsToExclude)
    {
        HashSet<Gem> currentGems =
            new HashSet<Gem>();

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

                if (gem == null)
                {
                    continue;
                }

                if (gemsToExclude != null &&
                    gemsToExclude.Contains(gem))
                {
                    continue;
                }

                currentGems.Add(gem);
            }
        }

        return currentGems;
    }

    private void ReportDoubleCrystalSweepToCombat(
        HashSet<Gem> sweepTargets)
    {
        if (sweepTargets == null ||
            sweepTargets.Count == 0)
        {
            return;
        }

        /*
         * Any additional crystals caught in the sweep
         * must not grant rewards for their hidden color.
         */
        HashSet<Gem> rewardExclusions =
            new HashSet<Gem>();

        foreach (Gem gem in sweepTargets)
        {
            if (gem != null &&
                gem.SpecialType ==
                    GemSpecialType.ColorCrystal)
            {
                rewardExclusions.Add(gem);
            }
        }

        ReportBombClearsToCombat(
            rewardExclusions,
            sweepTargets,
            0
        );
    }
}