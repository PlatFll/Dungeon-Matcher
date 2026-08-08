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
     * These defaults intentionally favor readability over raw speed. Swaps,
     * clear-to-gravity timing, deep refills and post-landing cadence all leave
     * enough time for the player to visually follow what the board just did.
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
        "swap snaps back."
    )]
    private float maximumInvalidSwapPause =
        0.06f;

    [SerializeField, Range(0.25f, 2.5f)]
    [Tooltip(
        "Controls the impact-to-gravity beat after cleared gems disappear. " +
        "The current value turns the base 0.04s post-burst delay into about " +
        "0.07s so the explosion finishes reading before gravity starts."
    )]
    private float matchPostBurstDelayMultiplier =
        1.75f;

    [Header("Board Feel - Gravity")]
    [SerializeField, Range(0.5f, 1f)]
    [Tooltip(
        "Global multiplier applied after distance-based fall " +
        "timing is calculated. One keeps the full calculated duration."
    )]
    private float fallDurationMultiplier =
        1.00f;

    [SerializeField, Range(0.35f, 1f)]
    [Tooltip(
        "Controls how strongly long drops are compressed. This higher value " +
        "keeps medium/deep refills substantially slower while still avoiding " +
        "perfectly linear fall timing. Full-column drops can reach the normal " +
        "maximum fall duration."
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
        "Readable beat after every falling gem has fully landed " +
        "before the next cascade is scanned."
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
                2.5f
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
         * Preserve more distance-based travel time than the earlier fast
         * profile. Short falls stay concise, while wiped columns and other
         * deep refills have enough screen time to remain readable.
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
         * Reserve that time here so the bounce remains inside the total fall
         * budget instead of adding extra latency after every refill.
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
     * Gravity accelerates into the landing instead of using the old symmetric
     * SmoothStep. Keeping this curve preserves the weight and responsiveness
     * of the improved motion even though the overall timing is now slower.
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
