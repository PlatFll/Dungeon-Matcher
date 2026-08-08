using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private ColorCrystalVFXController
        colorCrystalVFXController;

    /*
     * Color-crystal targets used to wait for the entire glint pass,
     * then play the ordinary 0.12s ClearMatches animation one by one.
     * That made the visual selector feel much faster than the board.
     *
     * Synchronized mode keeps the quick travelling cadence, but the glint
     * now receives a short visual lead before its target begins clearing.
     * This lets the star visibly grow toward its 0.09s peak instead of the
     * gem disappearing almost immediately underneath it.
     *
     * The values below are deliberately tuned around Unity's coroutine
     * frame scheduling. ClearMatches always has a final yield, so a 0.01s
     * flash plus that final frame continues to track the 0.02s travelling
     * cadence closely at 60/90/120 FPS once the fixed lead is established.
     *
     * Keeping this override here avoids changing crystal target selection,
     * damage/reward reporting, bomb expansion, or ClearMatches itself.
     */
    private const float
        SynchronizedCrystalFlashDuration =
            0.01f;

    private const float
        SynchronizedCrystalTargetCadence =
            0.02f;

    /*
     * Stars begin immediately after the source crystal pulse. Gameplay
     * waits briefly before starting the source-crystal clear. The source
     * clear then adds a couple of frames of its own, placing the first
     * target disappearance close to the target star's 0.09s visual peak.
     */
    private const float
        SynchronizedCrystalTargetClearLeadIn =
            0.05f;

    private const float
        SynchronizedCrystalWhiteHoldDuration =
            0f;

    private const float
        SynchronizedCrystalPostBurstDelay =
            0f;

    private const float
        SynchronizedCrystalActivationStagger =
            0f;

    /*
     * Leave enough room for the final fast ClearMatches coroutine to finish
     * before restoring the normal match timings. This prevents the last
     * selected gems from suddenly reverting to the old slow animation.
     */
    private const float
        SynchronizedCrystalRestoreSafetyDelay =
            0.25f;

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

        BeginSynchronizedCrystalTimings();

        /*
         * Start the target stars immediately after the source crystal pulse.
         * The fixed lead below then gives those stars time to become readable
         * before ResolveNormalColorCrystalSequence begins destroying gems.
         */
        const float targetStartDelay =
            0f;

        yield return
            colorCrystalVFXController
                .PlaySynchronizedActivation(
                    new ColorCrystalVFXContext(
                        crystalGem,
                        validTargets.ToArray()
                    ),
                    targetStartDelay,
                    SynchronizedCrystalTargetCadence
                );

        if (SynchronizedCrystalTargetClearLeadIn >
            0f)
        {
            yield return new WaitForSeconds(
                SynchronizedCrystalTargetClearLeadIn
            );
        }

        /*
         * PlaySynchronizedActivation starts target glints in the background.
         * Start restoration only after the visual lead, when gameplay is
         * about to begin clearing the source crystal and selected targets.
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
            SynchronizedCrystalActivationStagger;

        synchronizedCrystalTimingsActive =
            true;
    }

    private IEnumerator
        RestoreCrystalTimingsAfterTargets()
    {
        /*
         * A bomb-triggered crystal can start another target sequence while
         * the current one is resolving. Keep the fast timings until no
         * synchronized target-launch sequence remains active.
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

            /*
             * A nested crystal may have started during the safety window.
             * If so, wait through that target sequence as well instead of
             * restoring timings underneath it.
             */
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
