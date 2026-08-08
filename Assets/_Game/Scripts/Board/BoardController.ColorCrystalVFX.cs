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
     * The star glint is now the visible target-clear effect. While a color
     * crystal is active, the normal ClearMatches flash/hold is suppressed so
     * it does not compete with that star or add a second little explosion.
     * ClearMatches still owns the actual board-state removal and final yield.
     *
     * Presentation order:
     * crystal pulse -> adaptive star sweep -> lightweight board clears.
     */
    private const float
        SynchronizedCrystalFlashDuration =
            0f;

    private const float
        SynchronizedCrystalWhiteHoldDuration =
            0f;

    private const float
        SynchronizedCrystalPostBurstDelay =
            0f;

    /*
     * Stars live for about 0.24 seconds. Give the sweep the same-sized head
     * start before gameplay begins removing targets. This keeps every target
     * star readable without extending the total activation, because the VFX
     * and gameplay clear loop continue in parallel after this point.
     */
    private const float
        SynchronizedCrystalTargetClearLeadIn =
            0.24f;

    /*
     * At 90/120 FPS the one-frame ClearMatches handoff can run noticeably
     * faster than the visual sweep. Add a tiny frame-aware wait there only;
     * 30/60 FPS need no extra target delay.
     */
    private const float
        HighFrameRateThreshold =
            1f / 75f;

    private const float
        VeryHighFrameRateThreshold =
            1f / 110f;

    /*
     * This guard covers the final color-crystal clear tail, but remains short
     * enough that ordinary refill cascades recover normal match timings.
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

        BeginSynchronizedCrystalTimings();

        /*
         * Await only the source crystal pulse. The target stars then launch
         * in the background across one elapsed-time sweep, allowing the VFX
         * and gameplay clear loop to read as one continuous event.
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

        if (restoreCrystalTimingsRoutine ==
            null)
        {
            restoreCrystalTimingsRoutine =
                StartCoroutine(
                    RestoreCrystalTimingsAfterTargets()
                );
        }
    }

    private void BeginSynchronizedCrystalTimings()
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
            CalculateCrystalActivationStagger();

        synchronizedCrystalTimingsActive =
            true;
    }

    private float CalculateCrystalActivationStagger()
    {
        float frameDuration =
            Mathf.Max(
                Time.deltaTime,
                1f / 240f
            );

        if (frameDuration >=
            HighFrameRateThreshold)
        {
            return 0f;
        }

        /*
         * Request roughly one rendered frame of spacing at 90 FPS and two
         * at 120 FPS. WaitForSeconds is frame-quantized, so using fractions
         * of the current frame asks Unity for the desired number of frames
         * without slowing the normal 60 FPS path.
         */
        if (frameDuration <=
            VeryHighFrameRateThreshold)
        {
            return frameDuration * 1.5f;
        }

        return frameDuration * 0.75f;
    }

    private IEnumerator
        RestoreCrystalTimingsAfterTargets()
    {
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
