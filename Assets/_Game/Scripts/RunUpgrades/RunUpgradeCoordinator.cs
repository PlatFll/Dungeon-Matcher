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
    private bool refinementUsed;
    private int heldCompletedWave;
    private object activeChoiceSession;
    private readonly HashSet<RunUpgradeDefinition> offeredChoices =
        new HashSet<RunUpgradeDefinition>();

    public bool IsBlockingWaveProgression => isHoldingProgression;
    public bool CanRefine => isHoldingProgression && !selectionCommitted && !refinementUsed &&
        !(AccountProgression.Current.ActiveRun?.refinementUsed ?? false);

    public bool TryRefine(RunUpgradeTheme theme)
    {
        if (!CanRefineTheme(theme) || !IsCurrentIntermission(activeChoiceSession, heldCompletedWave)) return false;
        if(RunSession.Current?.Continuation!=null && !RunSession.Current.Continuation.RecordAction(new RunRecordedAction {kind=RunActionKind.RefineDraft,theme=theme})) return false;
        var choices = UpgradeDraftGenerator.Refine(runtime.Catalog, runtime, playerActor, heldCompletedWave,
            runtime.GetDraftRandom(), theme, offeredChoices);
        if (choices.Count == 0) return false;
        // Accepted direction is already durable in the action journal. A later
        // snapshot failure must not permit another refinement.
        PersistDraft(choices,true);
        refinementUsed = true;
        offeredChoices.Clear(); offeredChoices.UnionWith(choices);
        RunSession.Current?.Continuation?.SaveNow();
        object session = activeChoiceSession;
        return choiceUI.Show(choices, card => ReferenceEquals(activeChoiceSession, session) && TrySelectUpgrade(card));
    }
    public bool CanRefineTheme(RunUpgradeTheme theme)
    {
        if(!CanRefine || runtime?.Catalog==null) return false;
        foreach(var definition in runtime.Catalog.Upgrades)
            if(definition!=null && definition.Theme==theme && !offeredChoices.Contains(definition) && runtime.IsEligible(definition,playerActor,heldCompletedWave)) return true;
        return false;
    }

    public void CaptureContinuation(RunCombatSnapshot saved)
    {
        saved.refinementUsed=refinementUsed || (AccountProgression.Current.ActiveRun?.refinementUsed ?? false);
        saved.draft.Clear(); saved.draftWave=0;
        if(!isHoldingProgression || selectionCommitted) return;
        saved.draftWave=heldCompletedWave;
        foreach(var card in offeredChoices) saved.draft.Add(card.UpgradeId);
    }

    public void RestoreContinuation(RunCombatSnapshot saved)
    {
        CleanupIntermission(); refinementUsed=saved.refinementUsed;
        if(saved.waveActive || saved.draftWave!=saved.wave || saved.draft.Count==0) return;
        var choices=new List<RunUpgradeDefinition>();
        foreach(string id in saved.draft)
            foreach(var definition in runtime.Catalog.Upgrades)
                if(definition.UpgradeId==id) choices.Add(definition);
        if(choices.Count!=saved.draft.Count) throw new InvalidOperationException("A saved card offer is unavailable.");
        isHoldingProgression=true; selectionCommitted=false; heldCompletedWave=saved.wave;
        activeChoiceSession=new object(); offeredChoices.UnionWith(choices);
        object session=activeChoiceSession;
        inputBlock=boardController.AcquireExternalInputBlock(); choiceUI.SetRefinement(this);
        if(!choiceUI.Show(choices,card=>ReferenceEquals(activeChoiceSession,session)&&TrySelectUpgrade(card)))
            throw new InvalidOperationException("The saved card choice could not be presented.");
    }

    private bool PersistDraft(List<RunUpgradeDefinition> choices, bool refined)
    {
        var run = RunSession.Current;
        if (run == null) return true;
        return AccountProgression.Current.StoreDraft(run.RunId, heldCompletedWave,
            choices.ConvertAll(card => card.UpgradeId), refined);
    }

    public static bool ShouldOfferUpgradeAfterWave(int completedWave)
    {
        return BalanceV1.Current.OffersCard(completedWave);
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
        if (!PersistDraft(choices, false))
        {
            // Keep the progression gate; retry the same offer, never reroll on a write failure.
            while (!PersistDraft(choices, false))
            {
                if (!IsCurrentIntermission(session, completedWave)) yield break;
                yield return new WaitForSecondsRealtime(1f);
            }
        }
        choiceUI.SetRefinement(this);
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

        if(RunSession.Current?.Continuation!=null && !RunSession.Current.Continuation.RecordAction(new RunRecordedAction {kind=RunActionKind.ChooseCard,card=definition.UpgradeId})) return false;
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
        if (ReferenceEquals(activeChoiceSession, session))
        {
            var run=RunSession.Current;
            if(run!=null) { AccountProgression.Current.ClearDraft(run.RunId); run.Continuation?.SaveNow(); }
            ReleaseProgression();
        }
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
        if (initializedPlayer == playerActor) { refinementUsed = false; CleanupIntermission(); }
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

    public bool SelectRecordedCard(string id)
    {
        foreach(var definition in new List<RunUpgradeDefinition>(offeredChoices))
            if(definition.UpgradeId==id)
            {
                bool accepted=TrySelectUpgrade(definition);
                if(accepted) choiceUI.Hide();
                return accepted;
            }
        return false;
    }

    private void HandleRunReset() { refinementUsed=false; CleanupIntermission(); }

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
            runtime.RunReset -= HandleRunReset;
            runtime.RunReset += HandleRunReset;
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
        if (runtime != null) runtime.RunReset -= HandleRunReset;
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
