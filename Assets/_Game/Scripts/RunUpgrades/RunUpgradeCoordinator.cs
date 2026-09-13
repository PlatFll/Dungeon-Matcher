using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RunUpgradeCoordinator :
    MonoBehaviour,
    IWaveProgressionGate
{
    public const int WavesPerChoice = 5;

    private RunUpgradeRuntime runtime;
    private WaveController waveController;
    private BoardController boardController;
    private PlayerActor playerActor;
    private UpgradeChoiceUI choiceUI;
    private IDisposable inputBlock;
    private Coroutine openRoutine;
    private bool isHoldingProgression;
    private bool selectionCommitted;
    private int heldCompletedWave;
    private object activeChoiceSession;
    private readonly HashSet<RunUpgradeDefinition> offeredChoices =
        new HashSet<RunUpgradeDefinition>();

    public bool IsBlockingWaveProgression => isHoldingProgression;

    public static bool ShouldOfferUpgradeAfterWave(int completedWave)
    {
        return completedWave > 0 &&
               completedWave % WavesPerChoice == 0;
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void OnDisable()
    {
        CleanupIntermission();
        Unsubscribe();
    }

    public void Configure(
        RunUpgradeRuntime upgradeRuntime,
        WaveController waves,
        BoardController board,
        PlayerActor player,
        UpgradeChoiceUI ui)
    {
        // Repeated installation must preserve a visible choice and its token.
        if (runtime == upgradeRuntime && waveController == waves &&
            boardController == board && playerActor == player && choiceUI == ui)
        {
            Subscribe();
            return;
        }

        // Release the OLD board/UI/gate before replacing their references.
        CleanupIntermission();
        Unsubscribe();

        runtime = upgradeRuntime;
        waveController = waves;
        boardController = board;
        playerActor = player;
        choiceUI = ui;

        Subscribe();
    }

    private void HandleWaveCompleted(int completedWave)
    {
        if (!isActiveAndEnabled ||
            !ShouldOfferUpgradeAfterWave(completedWave) ||
            isHoldingProgression ||
            waveController == null ||
            waveController.CurrentWave != completedWave ||
            waveController.IsWaveActive ||
            playerActor == null ||
            !playerActor.IsInitialized ||
            playerActor.IsDefeated)
        {
            return;
        }

        isHoldingProgression = true;
        selectionCommitted = false;
        heldCompletedWave = completedWave;
        activeChoiceSession = new object();
        offeredChoices.Clear();

        openRoutine = StartCoroutine(
            OpenChoiceAfterBoardSettles(completedWave)
        );
    }

    private bool IsCurrentIntermission(object session, int completedWave)
    {
        return session != null && ReferenceEquals(activeChoiceSession, session) &&
               isActiveAndEnabled && isHoldingProgression &&
               heldCompletedWave == completedWave &&
               waveController != null && !waveController.IsWaveActive &&
               waveController.CurrentWave == completedWave &&
               playerActor != null && playerActor.IsInitialized && !playerActor.IsDefeated;
    }

    private IEnumerator OpenChoiceAfterBoardSettles(int completedWave)
    {
        object session = activeChoiceSession;
        while (boardController != null && boardController.IsBusy)
        {
            if (!IsCurrentIntermission(session, completedWave))
            {
                if (ReferenceEquals(activeChoiceSession, session))
                {
                    ReleaseProgression();
                    openRoutine = null;
                }
                yield break;
            }

            yield return null;
        }

        if (!IsCurrentIntermission(session, completedWave))
        {
            if (ReferenceEquals(activeChoiceSession, session))
            {
                ReleaseProgression();
                openRoutine = null;
            }
            yield break;
        }

        if (runtime == null || runtime.Catalog == null || choiceUI == null)
        {
            Debug.LogError(
                "Run upgrade intermission is missing its runtime, catalog, " +
                "or choice UI. Wave progression will continue.",
                this
            );
            ReleaseProgression();
            openRoutine = null;
            yield break;
        }

        List<RunUpgradeDefinition> choices =
            UpgradeDraftGenerator.Generate(
                runtime.Catalog,
                runtime,
                playerActor,
                completedWave,
                runtime.GetDraftRandom()
            );

        if (choices.Count == 0)
        {
            Debug.LogWarning(
                $"No legal run upgrades were available after wave " +
                $"{completedWave}. Wave progression will continue.",
                this
            );
            ReleaseProgression();
            openRoutine = null;
            yield break;
        }

        if (choices.Count < UpgradeDraftGenerator.DefaultChoiceCount)
        {
            Debug.LogWarning(
                $"Only {choices.Count} legal run upgrade choice(s) were " +
                $"available after wave {completedWave}.",
                this
            );
        }

        offeredChoices.Clear();
        offeredChoices.UnionWith(choices);
        inputBlock = boardController != null
            ? boardController.AcquireExternalInputBlock()
            : null;

        // A callback from an old choice may not apply a card to a later run or
        // intermission, even when that card would still be otherwise eligible.
        if (!choiceUI.Show(choices, definition =>
                ReferenceEquals(activeChoiceSession, session) && TrySelectUpgrade(definition)))
        {
            Debug.LogError(
                "Run upgrade UI could not present its legal choices. " +
                "Wave progression will continue.",
                this
            );
            if (ReferenceEquals(activeChoiceSession, session)) ReleaseProgression();
        }

        openRoutine = null;
    }

    private bool TrySelectUpgrade(RunUpgradeDefinition definition)
    {
        if (selectionCommitted ||
            !IsCurrentIntermission(activeChoiceSession, heldCompletedWave) ||
            runtime == null ||
            definition == null ||
            !offeredChoices.Contains(definition))
        {
            return false;
        }

        object session = activeChoiceSession;
        // TryApply publishes synchronous callbacks. Own selection BEFORE those
        // callbacks so re-entry cannot award another card from this choice.
        selectionCommitted = true;
        if (!runtime.TryApply(definition, heldCompletedWave))
        {
            if (ReferenceEquals(activeChoiceSession, session)) selectionCommitted = false;
            Debug.LogWarning(
                $"Run upgrade '{definition.UpgradeId}' was no longer legal " +
                "when selected.",
                this
            );
            return false;
        }

        // A callback may already have reset/rebound the run. Do not release a
        // different, newer choice's gate when the old application returns.
        if (ReferenceEquals(activeChoiceSession, session)) ReleaseProgression();
        return true;
    }

    private void HandlePlayerDefeated(PlayerActor defeatedPlayer)
    {
        if (defeatedPlayer == playerActor)
        {
            CleanupIntermission();
        }
    }

    private void HandlePlayerInitialized(PlayerActor initializedPlayer)
    {
        if (initializedPlayer == playerActor) CleanupIntermission();
    }

    private void HandleChoiceHidden()
    {
        // A hidden/disabled view cannot leave an invisible modal locking the
        // board. This follows the coordinator's existing cancellation policy:
        // abandon the choice without awarding anything; release only our token.
        if (isHoldingProgression) CleanupIntermission();
    }

    private void CleanupIntermission()
    {
        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }

        // Invalidate callbacks and release ownership before Hide notifies us.
        ReleaseProgression();
        if (choiceUI != null)
        {
            choiceUI.Hide();
        }
    }

    private void ReleaseProgression()
    {
        activeChoiceSession = null;
        offeredChoices.Clear();
        inputBlock?.Dispose();
        inputBlock = null;
        isHoldingProgression = false;
    }

    private void ResolveReferences()
    {
        if (waveController == null)
        {
            waveController = GetComponent<WaveController>();
        }

        if (runtime == null)
        {
            runtime = GetComponent<RunUpgradeRuntime>();
        }

        if (boardController == null)
        {
            boardController = UnityEngine.Object.FindFirstObjectByType<
                BoardController
            >();
        }

        if (playerActor == null)
        {
            playerActor = UnityEngine.Object.FindFirstObjectByType<
                PlayerActor
            >();
        }

        if (choiceUI == null)
        {
            choiceUI = UnityEngine.Object.FindFirstObjectByType<
                UpgradeChoiceUI
            >();
        }
    }

    private void Subscribe()
    {
        if (!isActiveAndEnabled) return;

        if (runtime != null)
        {
            runtime.RunReset -= CleanupIntermission;
            runtime.RunReset += CleanupIntermission;
        }

        if (choiceUI != null)
        {
            choiceUI.Hidden -= HandleChoiceHidden;
            choiceUI.Hidden += HandleChoiceHidden;
        }

        if (waveController != null)
        {
            waveController.WaveCompleted -= HandleWaveCompleted;
            waveController.WaveCompleted += HandleWaveCompleted;
            waveController.RegisterProgressionGate(this);
        }

        if (playerActor != null)
        {
            playerActor.Defeated -= HandlePlayerDefeated;
            playerActor.Defeated += HandlePlayerDefeated;
            playerActor.Initialized -= HandlePlayerInitialized;
            playerActor.Initialized += HandlePlayerInitialized;
        }
    }

    private void Unsubscribe()
    {
        if (runtime != null) runtime.RunReset -= CleanupIntermission;
        if (choiceUI != null) choiceUI.Hidden -= HandleChoiceHidden;

        if (waveController != null)
        {
            waveController.WaveCompleted -= HandleWaveCompleted;
            waveController.UnregisterProgressionGate(this);
        }

        if (playerActor != null)
        {
            playerActor.Defeated -= HandlePlayerDefeated;
            playerActor.Initialized -= HandlePlayerInitialized;
        }
    }
}
