using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RootbinderEnemyAbility : MonoBehaviour,IEnemySpecialAbilityRuntime,IAcceptedMoveEnemyAbility,IEnemyContinuationOwner
{
    private EnemyActor actor;
    private BoardController board;
    private EnemyStagger stagger;
    private BoardController.GemSetThreat warning;
    private bool pending;
    public event Action Changed;
    public string Outcome { get; private set; }
    public bool IsWarning => warning!=null && !warning.Ended;
    public int ResponseMoves => IsWarning?Mathf.Max(0,warning.DueMove-board.CompletedValidPlayerMoves):0;
    public void InitializeSpecialAbility(EnemyActor owner,BoardController initializedBoard,IReadOnlyList<EnemyActor> enemies)
    {
        actor=owner;board=initializedBoard;stagger=actor.GetComponent<EnemyStagger>();
        actor.Defeated+=Died;if(stagger!=null) stagger.StaggerApplied+=Interrupted;
    }
    private void LateUpdate()
    {
        if(actor==null || actor.IsDefeated || board.IsBusy || pending || warning==null || !warning.Ended) return;
        bool interrupted=warning.PlayerInterrupted;Finish(false,interrupted);
    }
    public void ResolveAcceptedMove()
    {
        if(!CombatMoveClock.CanOffer(actor) || pending || actor.IsDefeated || stagger?.IsStaggered==true) return;
        if(warning!=null)
        {
            if(warning.Ended) {Finish(false,warning.PlayerInterrupted);return;}
            if(board.CompletedValidPlayerMoves<warning.DueMove || !actor.TryBeginSpecialAbilityAnimationAction()) return;
            pending=true;if(!board.TryQueueResolveVines(warning,ok=>Finish(ok,false))) Finish(false,false);
            return;
        }
        if(!actor.IsSpecialReady || board.OwnedRootCount(actor)>0 || !actor.TryBeginSpecialAbilityAnimationAction()) return;
        pending=true;
        if(!board.TryQueueRootWarning(actor,1,1,false,false,result=>
        {
            warning=result;pending=false;Outcome=null;actor.EndSpecialAbilityAnimationAction();
            if(result!=null) {actor.AnnounceCommittedCast();actor.NotifySpecialAbilityUsed();actor.ResetSpecialCounter();}
            Publish();
        })) {pending=false;actor.EndSpecialAbilityAnimationAction();}
    }
    private void Finish(bool success,bool interrupted)
    {
        if(warning!=null) board.CancelGemSetThreat(warning);
        pending=false;warning=null;Outcome=success?"Planted":interrupted?"Interrupted":"Target lost";
        if(actor!=null && !actor.IsDefeated)
        {
            actor.ResetSpecialCounter();actor.SetSpecialTurnRequirement(4);actor.EndSpecialAbilityAnimationAction();
            if(interrupted) stagger?.ApplyStagger(1,1);
        }
        Publish();
    }
    private void Publish() {actor?.GetComponent<EnemyAutoAttack>()?.SetActionPaused(this,IsWarning);Changed?.Invoke();}
    private void Interrupted(EnemyStagger source,float duration,float remaining)
    {if(warning!=null || pending) Finish(false,true);}
    private void Died(EnemyActor owner) => board.RemoveVineSource(owner);
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf) {saved.rootbinderOutcome=Outcome;}
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt)
    {warning=board.RestoredVineCast(actor);Outcome=saved.rootbinderOutcome;Publish();}
    private void OnDisable()
    {
        if(actor!=null) {actor.Defeated-=Died;board?.RemoveVineSource(actor);}
        if(stagger!=null) stagger.StaggerApplied-=Interrupted;
        actor?.GetComponent<EnemyAutoAttack>()?.SetActionPaused(this,false);
    }
}
