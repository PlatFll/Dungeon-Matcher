using System;
using System.Collections;
using UnityEngine;

/// <summary>Coordinates value snapshots; gameplay owners capture and restore their own state.</summary>
[DefaultExecutionOrder(10000)]
public sealed class RunContinuation : MonoBehaviour
{
    private RunSession session;
    private AccountProgression account;
    private RunCombatSnapshot checkpoint;
    private IDisposable restoreInput;
    private float nextCheckpoint;
    private RunReplayTape tape=new RunReplayTape();
    private int replayFrame;
    private bool replayFrameOpened;
    public SavedRandom Random { get; private set; }
    public bool IsRestoring { get; private set; }
    public bool IsReplaying { get; private set; }
    public bool ExecutingReplayAction { get; private set; }
    public string Error { get; private set; }
    public bool CanCapture => session!=null && session.InitialStateReady && !session.IsFinished && !IsRestoring &&
        session.Player!=null && session.Player.IsInitialized && !session.Player.IsDefeated &&
        session.Board.CanCaptureContinuation && session.Waves.CanCaptureContinuation && RunUpgradeRuntime.Current!=null;

    public void Initialize(RunSession run, AccountProgression progression, RunCombatSnapshot saved,RunReplayTape savedTape)
    {
        session=run; account=progression;
        Random=new SavedRandom(UnityEngine.Random.Range(1,int.MaxValue));
        session.Board.BeforePlayerSwap=(a,b)=>RecordAction(new RunRecordedAction {kind=RunActionKind.Swap,x=a.Column,y=a.Row,targetX=b.Column,targetY=b.Row});
        bool hasBoard=saved?.board?.cells!=null && saved.board.cells.Count>0;
        if((hasBoard && saved.version!=1) || (!hasBoard && (progression.ActiveRun?.completedWaves>0 || savedTape?.frames?.Count>0)))
        {
            IsRestoring=true;Time.timeScale=0;session.Board.PrepareContinuation();session.Waves.PrepareContinuation();
            Error="The saved combat state is incomplete or from an unsupported version. Your run was preserved.";
            return;
        }
        if(hasBoard)
        {
            checkpoint=saved; IsRestoring=true; Time.timeScale=0;
            tape=savedTape ?? new RunReplayTape(); Random=new SavedRandom(saved.gameplayRandom);
            session.Board.PrepareContinuation(); session.Waves.PrepareContinuation();
            restoreInput=session.Board.AcquireExternalInputBlock();
            StartCoroutine(Restore(saved));
        }
    }
    private void LateUpdate()
    {
        if(IsReplaying)
        {
            if(!replayFrameOpened) return;
            replayFrameOpened=false; replayFrame++;
            if(replayFrame<tape.frames.Count) SetReplayDelta(tape.frames[replayFrame].delta);
            else FinishReplay();
            return;
        }
        if(IsRestoring || Time.timeScale<=0 || Time.unscaledTime<nextCheckpoint || (!CanCapture&&checkpoint==null)) return;
        SaveNow(); nextCheckpoint=Time.unscaledTime+.25f;
    }
    public RunCombatSnapshot Capture()
    {
        if(!CanCapture) throw new InvalidOperationException("Combat is not at a snapshot boundary.");
        var saved=new RunCombatSnapshot { sequence=(checkpoint?.sequence ?? 0)+1,player=session.Player.CaptureContinuation() };
        saved.gameplayRandom=Random.State;
        session.Waves.CaptureContinuation(saved);
        saved.board=session.Board.CaptureContinuation(session.Waves.ContinuationOwnerSlot);
        RunUpgradeRuntime.Current.CaptureContinuation(saved);
        RunUpgradeGameplayHooks.Current?.CaptureContinuation(saved);
        session.CaptureContinuation(saved);
        session.Waves.GetComponent<RunUpgradeCoordinator>()?.CaptureContinuation(saved);
        var decree=session.Player.GetComponent<RoyalDecreeRuntime>();
        if(decree!=null) { saved.decreeRemaining=decree.RemainingDuration; saved.decreeTarget=session.Waves.ContinuationSlot(decree.CurrentTarget); }
        return saved;
    }
    public bool SaveNow()
    {
        if(IsRestoring || session==null || session.IsFinished) return false;
        if(!CanCapture)
        {
            bool stored=checkpoint!=null && account.StoreReplayTape(session.RunId,tape);
            if(!stored) Error=account.LastError;
            return stored;
        }
        var saved=Capture();
        if(!account.StoreCheckpoint(session.RunId,saved)) { Error=account.LastError; return false; }
        checkpoint=saved; tape=new RunReplayTape(); Error=null; return true;
    }
    public void BeginFrame()
    {
        if(IsReplaying)
        {
            replayFrameOpened=true;
            foreach(var action in tape.frames[replayFrame].actions)
            {
                ExecutingReplayAction=true;
                try { Execute(action); }
                catch(Exception exception) { FailReplay(exception); return; }
                finally { ExecutingReplayAction=false; }
            }
            return;
        }
        if(IsRestoring || session==null || session.IsFinished || checkpoint==null || Time.deltaTime<=0) return;
        tape.frames.Add(new RunReplayFrame { delta=Time.deltaTime });
    }
    public bool RecordAction(RunRecordedAction action)
    {
        if(ExecutingReplayAction) return true;
        if(IsRestoring || session==null || session.IsFinished) return false;
        bool newBoundary=CanCapture;
        if(newBoundary && !SaveNow()) return false;
        if(checkpoint==null) return false;
        if(tape.frames.Count==0) tape.frames.Add(new RunReplayFrame {delta=newBoundary?0:Time.deltaTime});
        var actions=tape.frames[tape.frames.Count-1].actions; actions.Add(action);
        ConsumableKind? supply=action.kind==RunActionKind.Potion?ConsumableKind.HealthPotion:
            action.kind==RunActionKind.Bomb?ConsumableKind.Bomb:(ConsumableKind?)null;
        if(account.AcceptRecordedAction(session.RunId,tape,supply)) return true;
        actions.RemoveAt(actions.Count-1); Error=account.LastError; return false;
    }
    private void Execute(RunRecordedAction action)
    {
        switch(action.kind)
        {
            case RunActionKind.Swap:
                session.Board.ReplayPlayerSwap(action.x,action.y,action.targetX,action.targetY); break;
            case RunActionKind.Ability:
                if(!session.Player.GetComponent<PlayerAbilityController>().TryActivate()) throw new InvalidOperationException("Saved ability was not accepted."); break;
            case RunActionKind.Potion:
                if(!session.TryUsePotion()) throw new InvalidOperationException("Saved Potion was not accepted."); break;
            case RunActionKind.Bomb:
                if(!session.TryUseBomb(session.Board.GetGem(action.x,action.y))) throw new InvalidOperationException("Saved Bomb was not accepted."); break;
            case RunActionKind.ChooseCard:
                if(!session.Waves.GetComponent<RunUpgradeCoordinator>().SelectRecordedCard(action.card)) throw new InvalidOperationException("Saved card choice was not accepted."); break;
            case RunActionKind.RefineDraft:
                if(!session.Waves.GetComponent<RunUpgradeCoordinator>().TryRefine(action.theme)) throw new InvalidOperationException("Saved refinement was not accepted."); break;
        }
    }
    public void DiscardRejectedAction(RunRecordedAction action)
    {
        if(IsReplaying || checkpoint==null) return;
        foreach(var frame in tape.frames) frame.actions.Remove(action);
        if(!account.StoreReplayTape(session.RunId,tape)) Error=account.LastError;
    }
    private void SetReplayDelta(float delta)
    {
        // A zero delta denotes a synchronous input boundary, not a paused user
        // action. Keep acceptance enabled while advancing only a negligible tick.
        Time.timeScale=1;
        Time.captureDeltaTime=Mathf.Max(.000001f,delta);
    }
    private IEnumerator Restore(RunCombatSnapshot saved)
    {
        while(!session.Player.IsInitialized || RunUpgradeRuntime.Current==null || RunUpgradeRuntime.Current.Catalog==null) yield return null;
        // Let all scene Start callbacks bind presentation before publishing state.
        yield return null;
        try
        {
            RunUpgradeRuntime.Current.RestoreContinuation(saved);
            session.Player.RestoreContinuation(saved.player);
            session.Waves.RestoreContinuationActors(saved);
            session.Board.RestoreContinuation(saved.board,session.Waves.ContinuationEnemy);
            session.Waves.RestoreContinuationState(saved);
            RunUpgradeGameplayHooks.Current?.RestoreContinuation(saved);
            session.RestoreContinuation(saved);
            session.Player.GetComponent<RoyalDecreeRuntime>()?.RestoreContinuation(saved.decreeRemaining,
                session.Waves.ContinuationEnemy(saved.decreeTarget),session.Player.ActiveAbility);
            var coordinator=session.Waves.GetComponent<RunUpgradeCoordinator>();
            coordinator?.RestoreContinuation(saved);
            account.BeginContinuationReplay(saved);
        }
        catch(Exception exception)
        {
            Error="Could not continue this run. The saved run was preserved. "+exception.Message;
            Debug.LogException(exception); yield break;
        }
        restoreInput?.Dispose(); restoreInput=null;
        IsReplaying=true; replayFrame=0; replayFrameOpened=false;
        session.Waves.ResumeContinuationProgression();
        foreach(var enemy in session.Waves.ActiveEnemies) enemy.ResumeContinuationReadiness();
        if(tape.frames.Count>0) SetReplayDelta(tape.frames[0].delta);
        else FinishReplay();
    }
    private void FinishReplay()
    {
        IsReplaying=false; Time.captureDeltaTime=0; Time.timeScale=0;
        if(!account.EndContinuationReplay(true)) { Error=account.LastError; return; }
        IsRestoring=false;
        Time.timeScale=1;
        var controls=session.GetComponent<RunControlsUI>();
        controls?.Close();
        controls?.OpenGuide(CombatGuide.Continue(session));
    }
    private void FailReplay(Exception exception)
    {
        IsReplaying=false; Time.captureDeltaTime=0; Time.timeScale=0;
        account.EndContinuationReplay(false);
        Error="Could not continue. The saved run was preserved. "+exception.Message;
        Debug.LogException(exception);
    }
    private void OnDestroy()
    {
        restoreInput?.Dispose(); restoreInput=null;
        if(IsRestoring) { account.EndContinuationReplay(false); Time.captureDeltaTime=0; Time.timeScale=1; }
    }
    private void OnApplicationPause(bool paused)
    {
        if(!paused || session==null || session.IsFinished || IsRestoring) return;
        SaveNow(); session.GetComponent<RunControlsUI>()?.OpenSettings();
    }
}
