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
        if (!ShouldOfferUpgradeAfterWave(completedWave) ||
            isHoldingProgression ||
            playerActor == null ||
            playerActor.IsDefeated)
        {
            return;
        }

        isHoldingProgression = true;
        selectionCommitted = false;
        heldCompletedWave = completedWave;

        openRoutine = StartCoroutine(
            OpenChoiceAfterBoardSettles(completedWave)
        );
    }

    private IEnumerator OpenChoiceAfterBoardSettles(int completedWave)
    {
        while (boardController != null && boardController.IsBusy)
        {
            if (playerActor == null || playerActor.IsDefeated)
            {
                ReleaseProgression();
                openRoutine = null;
                yield break;
            }

            yield return null;
        }

        if (!isActiveAndEnabled ||
            !isHoldingProgression ||
            heldCompletedWave != completedWave ||
            playerActor == null ||
            playerActor.IsDefeated)
        {
            ReleaseProgression();
            openRoutine = null;
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

        inputBlock = boardController != null
            ? boardController.AcquireExternalInputBlock()
            : null;

        if (!choiceUI.Show(choices, TrySelectUpgrade))
        {
            Debug.LogError(
                "Run upgrade UI could not present its legal choices. " +
                "Wave progression will continue.",
                this
            );
            ReleaseProgression();
        }

        openRoutine = null;
    }

    private bool TrySelectUpgrade(RunUpgradeDefinition definition)
    {
        if (selectionCommitted ||
            !isHoldingProgression ||
            runtime == null ||
            definition == null)
        {
            return false;
        }

        if (!runtime.TryApply(definition, heldCompletedWave))
        {
            Debug.LogWarning(
                $"Run upgrade '{definition.UpgradeId}' was no longer legal " +
                "when selected.",
                this
            );
            return false;
        }

        selectionCommitted = true;
        ReleaseProgression();
        return true;
    }

    private void HandlePlayerDefeated(PlayerActor defeatedPlayer)
    {
        if (defeatedPlayer == playerActor)
        {
            CleanupIntermission();
        }
    }

    private void CleanupIntermission()
    {
        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }

        if (choiceUI != null)
        {
            choiceUI.Hide();
        }

        ReleaseProgression();
    }

    private void ReleaseProgression()
    {
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
        }
    }

    private void Unsubscribe()
    {
        if (waveController != null)
        {
            waveController.WaveCompleted -= HandleWaveCompleted;
            waveController.UnregisterProgressionGate(this);
        }

        if (playerActor != null)
        {
            playerActor.Defeated -= HandlePlayerDefeated;
        }
    }
}
