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
     * Current profile: all non-swap board pacing is intentionally 1.5x longer
     * than the previous readability-first profile. The actual swap animation
     * remains at its full base duration and is not slowed by this pass.
     */
    [Header("Board Feel - Responsiveness")]
    [SerializeField, Range(0.5f, 1f)]
    [Tooltip(
        "Multiplier applied to the existing swap duration. " +
        "One uses the full serialized base timing."
    )]
    private float swapDurationMultiplier =
        1.00f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Upper limit for the dead pause before an invalid " +
        "swap snaps back. This pause is 1.5x the previous profile; " +
        "the swap travel itself remains unchanged."
    )]
    private float maximumInvalidSwapPause =
        0.09f;

    [SerializeField, Range(0.25f, 4f)]
    [Tooltip(
        "Controls the impact-to-gravity beat after cleared gems disappear. " +
        "The current multiplier makes the previous ~0.07s beat about 0.105s."
    )]
    private float matchPostBurstDelayMultiplier =
        2.625f;

    [Header("Board Feel - Gravity")]
    [SerializeField, Range(0.5f, 2f)]
    [Tooltip(
        "Global multiplier applied after distance-based fall timing is " +
        "calculated. 1.5 makes every fall budget 50% longer than the " +
        "previous profile while keeping the same gravity curve."
    )]
    private float fallDurationMultiplier =
        1.50f;

    [SerializeField, Range(0.35f, 1f)]
    [Tooltip(
        "Controls how strongly long drops are compressed. Keep the current " +
        "shape so the slowdown is proportional across short and deep falls."
    )]
    private float fallDistanceExponent =
        0.90f;

    [SerializeField, Range(0f, 0.12f)]
    [Tooltip(
        "How far a falling gem briefly travels below its cell " +
        "before settling, measured in cell sizes."
    )]
    private float landingOvershootInCells =
        0.045f;

    [SerializeField, Min(0.01f)]
    [Tooltip(
        "Time reserved inside the fall's total duration for the " +
        "tiny rebound back to the exact cell center. This is also " +
        "1.5x the previous profile."
    )]
    private float landingSettleDuration =
        0.075f;

    [SerializeField, Range(1f, 1.10f)]
    private float landingSquashX =
        1.025f;

    [SerializeField, Range(0.90f, 1f)]
    private float landingSquashY =
        0.97f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Readable beat after every falling gem has fully landed " +
        "before the next cascade is scanned. This is 1.5x the " +
        "previous profile."
    )]
    private float postFallSettlePause =
        0.09f;

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
         * Preserve the same distance curve as the previous profile. The
         * separate fall-duration multiplier now stretches the entire result
         * by 1.5x, so a wiped column receives the same proportional slowdown
         * as a short refill instead of another hand-tuned special case.
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
         * Reserve that time here so the rebound remains inside the total fall
         * budget. Both travel and landing are slower, but the rebound still
         * does not add extra time beyond the requested fall budget.
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
     * Gravity still accelerates into the landing rather than returning to the
     * old floaty SmoothStep motion. This pass changes time, not motion shape.
     */
    private static float EaseInGravity(
        float progress)
    {
        float safeProgress =
            Mathf.Clamp01(progress);

        return Mathf.Pow(
            safeProgress,
            1.45f
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
