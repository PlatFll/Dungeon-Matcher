using UnityEngine;

public partial class BoardController
{
    /*
     * Central board-feel tuning.
     *
     * Keep presentation-only responsiveness values in this partial so future
     * tuning has one obvious rollback/debug location. None of these helpers
     * decide matches, targets, rewards, bomb chains, or cascade outcomes.
     *
     * This profile sits between the earlier fast board and the later 1.5x
     * slowdown. New refill gems also enter as a short stream instead of a
     * rigid column, while gravity keeps a gentle sense of acceleration.
     */
    [Header("Board Feel - Responsiveness")]
    [SerializeField, Range(0.5f, 1f)]
    [Tooltip(
        "Multiplier applied to the existing swap duration. " +
        "One keeps the full serialized 0.15s swap timing."
    )]
    private float swapDurationMultiplier =
        1.00f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Upper limit for the dead pause before an invalid swap snaps back. " +
        "This does not change the swap travel speed itself."
    )]
    private float maximumInvalidSwapPause =
        0.07f;

    [SerializeField, Range(0.25f, 4f)]
    [Tooltip(
        "Controls the impact-to-gravity beat after cleared gems disappear. " +
        "With the base 0.04s post-burst delay, 2.0 produces about 0.08s."
    )]
    private float matchPostBurstDelayMultiplier =
        2.00f;

    [Header("Board Feel - Gravity")]
    [SerializeField, Range(0.5f, 2f)]
    [Tooltip(
        "Global multiplier applied after distance-based fall timing is " +
        "calculated. 1.10 is the current middle-ground fall speed."
    )]
    private float fallDurationMultiplier =
        1.10f;

    [SerializeField, Range(0.35f, 1f)]
    [Tooltip(
        "Controls how strongly long drops are compressed. Lower values make " +
        "deep drops closer in duration to short drops; higher values preserve " +
        "more distance-based travel time."
    )]
    private float fallDistanceExponent =
        0.82f;

    [SerializeField, Range(1f, 1.5f)]
    [Tooltip(
        "Shape of the downward acceleration. 1 is linear motion. The current " +
        "1.18 keeps a gravity feel without saving too much distance for the " +
        "last part of the fall."
    )]
    private float gravityAccelerationExponent =
        1.18f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Delay between newly spawned refill gems in the same column. Existing " +
        "gems above holes still begin falling together, while replacement " +
        "gems enter as a readable stream."
    )]
    private float refillSpawnStagger =
        0.035f;

    [SerializeField, Range(0f, 0.12f)]
    [Tooltip(
        "How far a falling gem briefly travels below its cell before settling, " +
        "measured in cell sizes."
    )]
    private float landingOvershootInCells =
        0.045f;

    [SerializeField, Min(0.01f)]
    [Tooltip(
        "Time reserved inside the fall budget for the tiny rebound back to the " +
        "exact cell center."
    )]
    private float landingSettleDuration =
        0.06f;

    [SerializeField, Range(1f, 1.10f)]
    private float landingSquashX =
        1.025f;

    [SerializeField, Range(0.90f, 1f)]
    private float landingSquashY =
        0.97f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Readable beat after every falling gem has fully landed before the " +
        "next cascade is scanned."
    )]
    private float postFallSettlePause =
        0.06f;

    private float GetResponsiveSwapDuration()
    {
        return Mathf.Max(
            0.01f,
            swapDuration *
            Mathf.Clamp(
                swapDurationMultiplier,
                0.5f,
                1f
            )
        );
    }

    private float GetResponsiveInvalidSwapPause()
    {
        return Mathf.Min(
            Mathf.Max(0f, invalidSwapPause),
            Mathf.Max(0f, maximumInvalidSwapPause)
        );
    }

    private float GetResponsiveMatchPostBurstDelay()
    {
        return Mathf.Max(
            0f,
            matchPostBurstDelay *
            Mathf.Clamp(
                matchPostBurstDelayMultiplier,
                0.25f,
                4f
            )
        );
    }

    private float CalculateResponsiveFallDuration(
        float distanceInCells)
    {
        float safeDistance =
            Mathf.Max(
                1f,
                distanceInCells
            );

        /*
         * Keep some distance-based timing so a deep refill still has more
         * weight than a one-cell drop, but compress it enough that a large
         * clear does not become a long cutscene.
         */
        float compressedDistance =
            Mathf.Pow(
                safeDistance,
                Mathf.Clamp(
                    fallDistanceExponent,
                    0.35f,
                    1f
                )
            );

        float baseDuration =
            minimumFallDuration +
            (
                compressedDistance -
                1f
            ) *
            fallDurationPerCell;

        float safeMultiplier =
            Mathf.Clamp(
                fallDurationMultiplier,
                0.5f,
                2f
            );

        float responsiveMinimum =
            minimumFallDuration *
            safeMultiplier;

        float responsiveMaximum =
            maximumFallDuration *
            safeMultiplier;

        float totalFallBudget =
            Mathf.Clamp(
                baseDuration *
                safeMultiplier,
                responsiveMinimum,
                responsiveMaximum
            );

        /*
         * AnimateGemMoves appends the landing phase after this travel phase.
         * Reserve its duration here so the little bounce stays inside the
         * requested fall budget rather than adding hidden extra latency.
         */
        return Mathf.Max(
            0.01f,
            totalFallBudget -
            GetLandingSettleDuration()
        );
    }

    private float GetLandingSettleDuration()
    {
        return Mathf.Max(
            0.01f,
            landingSettleDuration
        );
    }

    private float GetRefillSpawnDelay(
        int spawnedGemIndex)
    {
        return Mathf.Max(
            0,
            spawnedGemIndex
        ) *
        Mathf.Max(
            0f,
            refillSpawnStagger
        );
    }

    private Vector3 GetLandingOvershootPosition(
        Vector3 targetPosition)
    {
        return targetPosition +
               Vector3.down *
               cellSize *
               Mathf.Max(
                   0f,
                   landingOvershootInCells
               );
    }

    private Vector3 GetLandingSquashScale(
        Vector3 restingScale)
    {
        return new Vector3(
            restingScale.x *
                Mathf.Max(
                    1f,
                    landingSquashX
                ),
            restingScale.y *
                Mathf.Clamp(
                    landingSquashY,
                    0.90f,
                    1f
                ),
            restingScale.z
        );
    }

    private float GetPostFallSettlePause()
    {
        return Mathf.Max(
            0f,
            postFallSettlePause
        );
    }

    /*
     * A mild exponent keeps the idea of gravity without the old 1.45 curve,
     * which held gems back early and made the final part of the fall look much
     * faster. At 1.18 the gem is already visibly moving early in the fall and
     * the final 20% no longer carries such a large share of the distance.
     */
    private float EaseInGravity(
        float progress)
    {
        float safeProgress =
            Mathf.Clamp01(progress);

        return Mathf.Pow(
            safeProgress,
            Mathf.Clamp(
                gravityAccelerationExponent,
                1f,
                1.5f
            )
        );
    }

    private static float EaseOutCubic(
        float progress)
    {
        float safeProgress =
            Mathf.Clamp01(progress);

        float inverse =
            1f -
            safeProgress;

        return 1f -
               inverse *
               inverse *
               inverse;
    }
}
