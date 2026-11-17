using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class RunUpgradeCoordinator
{
    private int heldResonanceNotch;
    private CrystalResonanceRuntime Resonance => RunSession.Current?.Resonance;
    private void Update()
    {
        var run = RunSession.Current;
        if (Resonance?.Enabled != true || run.IsFinished || run.Continuation?.IsRestoring == true ||
            run.NeedsSaveRetry || playerActor == null || playerActor.IsDefeated || Time.timeScale <= 0) return;
        if (isHoldingProgression)
        {
            if (heldResonanceNotch != 0 && !selectionCommitted && offeredChoices.Count > 0 &&
                choiceUI != null && choiceUI.isActiveAndEnabled && !choiceUI.IsOpen && openRoutine == null)
            {
                object session = activeChoiceSession;
                choiceUI.Show(new List<RunUpgradeDefinition>(offeredChoices),
                    card => ReferenceEquals(activeChoiceSession, session) && TrySelectUpgrade(card));
            }
            return;
        }
        if (Resonance.IsSavePending || !Resonance.HasPendingDrafts || waveController == null || waveController.IsWaveActive ||
            boardController == null || boardController.IsBusy || !waveController.CanCaptureContinuation ||
            run.MoveClock?.IsBlockingWaveProgression == true || run.GetComponent<RunControlsUI>()?.IsModalOpen == true ||
            runtime?.Catalog == null || choiceUI == null || !choiceUI.isActiveAndEnabled) return;
        heldResonanceNotch = Resonance.BeginNextDraft();
        if (heldResonanceNotch == 0) return;
        isHoldingProgression = true; selectionCommitted = false;
        heldCompletedWave = waveController.CurrentWave;
        activeChoiceSession = new object(); offeredChoices.Clear();
        openRoutine = StartCoroutine(OpenChoiceAfterBoardSettles(heldCompletedWave));
    }
    private List<RunUpgradeDefinition> SavedResonanceOffer()
    {
        var journal = AccountProgression.Current.ActiveRun;
        if (heldResonanceNotch == 0 || journal?.draftWave != heldCompletedWave || journal.draft.Count == 0) return null;
        var result = new List<RunUpgradeDefinition>();
        foreach (string id in journal.draft)
        {
            RunUpgradeDefinition found = null;
            foreach (var definition in runtime.Catalog.Upgrades)
                if (definition != null && definition.UpgradeId == id) { found = definition; break; }
            if (found == null) throw new InvalidOperationException("Saved Resonance offer is unavailable: " + id);
            result.Add(found);
        }
        return result;
    }
}
