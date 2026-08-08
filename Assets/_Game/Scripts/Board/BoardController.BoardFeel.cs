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
     * These defaults intentionally target a readable "normal" match-3 pace:
     * keep the responsive gravity/bounce architecture from the previous pass,
     * but leave slightly more time for the eye to follow swaps and landings.
     */
    [Header("Board Feel - Responsiveness")]
    [SerializeField, Range(0.5f, 1f)]
    [Tooltip(
        "Multiplier applied to the existing swap duration. " +
        "A value below one makes swaps react faster without " +
        "changing the serialized base timing."
    )]
    private float swapDurationMultiplier =
        0.90f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Upper limit for the dead pause before an invalid " +
        "swap snaps back."
    )]
    private float maximumInvalidSwapPause =
        0.04f;

    [SerializeField, Range(0.25f, 1f)]
    [Tooltip(
        "Reduces normal match post-burst dead time so gravity " +
        "starts sooner after the visual impact."
    )]
    private float matchPostBurstDelayMultiplier =
        0.50f;

    [Header("Board Feel - Gravity")]
    [SerializeField, Range(0.5f, 1f)]
    [Tooltip(
        "Global multiplier applied after distance-based fall " +
        "timing is calculated. The landing rebound is included " +
        "inside this total budget rather than added afterward."
    )]
    private float fallDurationMultiplier =
        0.94f;

    [SerializeField, Range(0.35f, 1f)]
    [Tooltip(
        "Compresses long fall durations. 0.5 is approximately " +
        "square-root scaling, keeping deep refills fast."
    )]
    private float fallDistanceExponent =
        0.52f;

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
        "tiny rebound back to the exact cell center."
    )]
    private float landingSettleDuration =
        0.05f;

    [SerializeField, Range(1f, 1.10f)]
    private float landingSquashX =
        1.025f;

    [SerializeField, Range(0.90f, 1f)]
    private float landingSquashY =
        0.97f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Tiny readable beat after every falling gem has fully " +
        "landed before the next cascade is scanned."
    )]
    private float postFallSettlePause =
        0.04f;

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
                1f
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
         * Linear distance makes deep columns feel dramatically slower than
         * short falls. Compressing distance preserves the visual difference
         * while preventing long refills from stalling the board.
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
                1f
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
         * Reserve that time here so the bounce is part of the same total fall
         * budget instead of making every refill slower just to add polish.
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
     * Gravity should accelerate into the landing rather than using the old
     * symmetric SmoothStep, which eased both into and out of a fall and made
     * gems feel floaty just before impact. The 1.45 exponent keeps that
     * acceleration while still giving the first frames visible movement.
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
