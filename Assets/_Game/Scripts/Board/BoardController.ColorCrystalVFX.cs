using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private ColorCrystalVFXController
        colorCrystalVFXController;

    /*
     * Color-crystal presentation timing.
     *
     * The earlier implementation tried to keep every target on a fixed
     * 0.02-second cadence. That made small clears unreadably fast and also
     * made the VFX duration depend too heavily on coroutine frame rounding.
     *
     * The revised presentation follows one coordinated event:
     *
     * crystal pulse -> adaptive star sweep -> fast overlapping clears.
     *
     * The VFX controller owns only the visual sweep. Gameplay still owns
     * target selection, rewards, bomb expansion and the actual ClearMatches
     * calls. These temporary values only shorten the ordinary clear animation
     * while the color-crystal sequence is active.
     */
    private const float
        SynchronizedCrystalFlashDuration =
            0.015f;

    private const float
        SynchronizedCrystalWhiteHoldDuration =
            0f;

    private const float
        SynchronizedCrystalPostBurstDelay =
            0f;

    /*
     * Let each target star visibly grow before its gem starts disappearing.
     * The star currently peaks at about 0.12 seconds, so gameplay receives
     * the same lead before entering the sequential crystal-clear loop.
     */
    private const float
        SynchronizedCrystalTargetClearLeadIn =
            0.12f;

    /*
     * Small target sets need a little spacing because ClearMatches has a
     * mandatory final yielded frame. Large sets derive any required spacing
     * from the adaptive sweep and current frame duration instead.
     */
    private const int
        SmallCrystalTargetCount =
            7;

    private const float
        SmallCrystalTargetActivationStagger =
            0.01f;

    /*
     * This guard covers the final color-crystal clear tail, but remains short
     * enough that ordinary refill cascades recover the normal match timings.
     * Nested crystal sweeps are rechecked below and extend the guard safely.
     */
    private const float
        SynchronizedCrystalRestoreSafetyDelay =
            0.20f;

    private bool
        synchronizedCrystalTimingsActive;

    private float savedMatchFlashDuration;
    private float savedMatchWhiteHoldDuration;
    private float savedMatchPostBurstDelay;
    private float savedCrystalActivationStagger;

    private Coroutine
        restoreCrystalTimingsRoutine;

    private void EnsureColorCrystalVFXController()
    {
        if (colorCrystalVFXController != null)
        {
            return;
        }

        colorCrystalVFXController =
            GetComponent<
                ColorCrystalVFXController
            >();

        if (colorCrystalVFXController != null)
        {
            return;
        }

        colorCrystalVFXController =
            gameObject.AddComponent<
                ColorCrystalVFXController
            >();
    }

    private IEnumerator
        PlayColorCrystalActivationVFX(
            HashSet<Gem> crystalTargetSet)
    {
        if (crystalTargetSet == null ||
            crystalTargetSet.Count == 0)
        {
            yield break;
        }

        Gem crystalGem;

        List<Gem> orderedTargets =
            BuildOrderedCrystalTargets(
                crystalTargetSet,
                out crystalGem
            );

        if (crystalGem == null ||
            orderedTargets == null)
        {
            yield break;
        }

        List<Gem> validTargets =
            new List<Gem>(
                orderedTargets.Count
            );

        foreach (Gem targetGem
                 in orderedTargets)
        {
            if (targetGem != null)
            {
                validTargets.Add(
                    targetGem
                );
            }
        }

        EnsureColorCrystalVFXController();

        if (colorCrystalVFXController == null)
        {
            yield break;
        }

        float targetSweepDuration =
            colorCrystalVFXController
                .CalculateCoordinatedSweepDuration(
                    validTargets.Count
                );

        BeginSynchronizedCrystalTimings(
            validTargets.Count,
            targetSweepDuration
        );

        /*
         * The source crystal pulse is still awaited, but the target sweep
         * itself launches in the background. This lets the star wave and
         * the existing gameplay clear loop overlap instead of playing as
         * two separate animations back-to-back.
         */
        yield return
            colorCrystalVFXController
                .PlaySynchronizedActivation(
                    new ColorCrystalVFXContext(
                        crystalGem,
                        validTargets.ToArray()
                    ),
                    0f,
                    targetSweepDuration
                );

        if (SynchronizedCrystalTargetClearLeadIn >
            0f)
        {
            yield return new WaitForSeconds(
                SynchronizedCrystalTargetClearLeadIn
            );
        }

        /*
         * Restoration runs independently so ResolveColorCrystalActivation
         * can continue into its existing gameplay path without waiting for
         * the complete visual tail.
         */
        if (restoreCrystalTimingsRoutine ==
            null)
        {
            restoreCrystalTimingsRoutine =
                StartCoroutine(
                    RestoreCrystalTimingsAfterTargets()
                );
        }
    }

    private void BeginSynchronizedCrystalTimings(
        int targetCount,
        float targetSweepDuration)
    {
        if (synchronizedCrystalTimingsActive)
        {
            return;
        }

        savedMatchFlashDuration =
            matchFlashDuration;

        savedMatchWhiteHoldDuration =
            matchWhiteHoldDuration;

        savedMatchPostBurstDelay =
            matchPostBurstDelay;

        savedCrystalActivationStagger =
            crystalActivationStagger;

        matchFlashDuration =
            SynchronizedCrystalFlashDuration;

        matchWhiteHoldDuration =
            SynchronizedCrystalWhiteHoldDuration;

        matchPostBurstDelay =
            SynchronizedCrystalPostBurstDelay;

        crystalActivationStagger =
            CalculateCrystalActivationStagger(
                targetCount,
                targetSweepDuration
            );

        synchronizedCrystalTimingsActive =
            true;
    }

    private float CalculateCrystalActivationStagger(
        int targetCount,
        float targetSweepDuration)
    {
        if (targetCount <= 1)
        {
            return 0f;
        }

        if (targetCount <=
            SmallCrystalTargetCount)
        {
            return SmallCrystalTargetActivationStagger;
        }

        /*
         * ClearMatches spends one or more frames in its flash loop and then
         * always yields one final frame. Estimate that frame cost so very
         * high frame rates do not let the gameplay clear sequence outrun the
         * elapsed-time star sweep. At 60/90 FPS this normally resolves to
         * zero extra delay; at 120 FPS it adds only the small amount needed.
         */
        float frameDuration =
            Mathf.Max(
                Time.deltaTime,
                1f / 240f
            );

        int flashFrameCount =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    SynchronizedCrystalFlashDuration /
                    frameDuration
                )
            );

        float estimatedClearDuration =
            (
                flashFrameCount +
                1
            ) *
            frameDuration;

        float desiredTargetCadence =
            targetSweepDuration /
            (targetCount - 1);

        float requiredStagger =
            Mathf.Max(
                0f,
                desiredTargetCadence -
                estimatedClearDuration
            );

        /*
         * A sub-frame WaitForSeconds still costs a rendered frame, so only
         * request an explicit wait when the gap is large enough to justify
         * one. This avoids accidentally slowing the 60 FPS path.
         */
        if (requiredStagger <
            frameDuration * 0.75f)
        {
            return 0f;
        }

        return requiredStagger;
    }

    private IEnumerator
        RestoreCrystalTimingsAfterTargets()
    {
        /*
         * A bomb-triggered crystal can begin another sweep while the current
         * crystal is resolving. Never restore the ordinary match timings
         * underneath an active target-launch sequence.
         */
        while (true)
        {
            while (colorCrystalVFXController !=
                       null &&
                   colorCrystalVFXController
                       .IsTargetLaunchSequenceActive)
            {
                yield return null;
            }

            if (SynchronizedCrystalRestoreSafetyDelay >
                0f)
            {
                yield return new WaitForSeconds(
                    SynchronizedCrystalRestoreSafetyDelay
                );
            }

            if (colorCrystalVFXController !=
                    null &&
                colorCrystalVFXController
                    .IsTargetLaunchSequenceActive)
            {
                continue;
            }

            break;
        }

        RestoreCrystalTimings();

        restoreCrystalTimingsRoutine =
            null;
    }

    private void RestoreCrystalTimings()
    {
        if (!synchronizedCrystalTimingsActive)
        {
            return;
        }

        matchFlashDuration =
            savedMatchFlashDuration;

        matchWhiteHoldDuration =
            savedMatchWhiteHoldDuration;

        matchPostBurstDelay =
            savedMatchPostBurstDelay;

        crystalActivationStagger =
            savedCrystalActivationStagger;

        synchronizedCrystalTimingsActive =
            false;
    }
}
