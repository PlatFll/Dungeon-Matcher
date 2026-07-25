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
         * The crystal moved by the player explodes first.
         * It receives no damage or energy reward for its
         * hidden underlying GemType.
         */
        yield return ClearCrystalWithoutRewards(
            firstCrystal
        );

        /*
         * The second crystal stays anchored in its current
         * board cell while the first board clear occurs.
         */
        isDoubleCrystalCharging = true;

        Coroutine chargeRoutine =
            StartCoroutine(
                AnimateDoubleCrystalCharge(
                    waitingCrystal
                )
            );

        /*
         * Record combat rewards once for the entire first
         * sweep, rather than once per individual column.
         */
        HashSet<Gem> firstSweepTargets =
            BuildCurrentGemSetExcept(
                waitingCrystal
            );

        ReportDoubleCrystalSweepToCombat(
            firstSweepTargets
        );

        /*
         * First explosion clears the board from left
         * to right, one complete column at a time.
         */
        yield return ClearBoardColumnByColumn(
            waitingCrystal
        );

        /*
         * Normal collapse cannot be used here because it
         * would move the waiting crystal. Instead, fill
         * every null cell directly while leaving its
         * current cell occupied and anchored.
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

        if (chargeRoutine != null)
        {
            yield return chargeRoutine;
        }

        /*
         * The charged crystal now performs the second
         * explosion.
         */
        yield return ClearCrystalWithoutRewards(
            waitingCrystal
        );

        HashSet<Gem> secondSweepTargets =
            BuildCurrentGemSetExcept(null);

        ReportDoubleCrystalSweepToCombat(
            secondSweepTargets
        );

        /*
         * Second explosion clears from the top row down.
         */
        yield return ClearBoardRowByRow();

        if (cascadePause > 0f)
        {
            yield return new WaitForSeconds(
                cascadePause
            );
        }

        /*
         * The entire board is empty now, so the normal
         * collapse and refill method is safe again.
         */
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
            "Double color crystal activation complete."
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
        Gem gemToPreserve)
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

                if (gem == null ||
                    gem == gemToPreserve)
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

    private IEnumerator ClearBoardRowByRow()
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

                if (gem != null)
                {
                    rowGems.Add(gem);
                }
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

    private HashSet<Gem>
        BuildCurrentGemSetExcept(
            Gem gemToExclude)
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

                if (gem == null ||
                    gem == gemToExclude)
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