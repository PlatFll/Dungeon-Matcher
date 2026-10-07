using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RootbinderEnemyAbility : MonoBehaviour,IEnemySpecialAbilityRuntime,IAcceptedMoveEnemyAbility,IEnemyContinuationOwner
{
    private EnemyActor actor;
    private BoardController board;
    private EnemyStagger stagger;
    private bool pending;
    public event Action Changed;
    public string Outcome { get; private set; }
    public bool IsWarning => false; // Kept for presentation clients of legacy saves.
    public int ResponseMoves => 0;
    public void InitializeSpecialAbility(EnemyActor owner,BoardController initializedBoard,IReadOnlyList<EnemyActor> enemies)
    {
        actor=owner;board=initializedBoard;stagger=actor.GetComponent<EnemyStagger>();
        actor.Defeated+=Died;if(stagger!=null) stagger.StaggerApplied+=Interrupted;
    }
    public void ResolveAcceptedMove()
    {
        if(!CombatMoveClock.CanOffer(actor) || pending || actor.IsDefeated || stagger?.IsStaggered==true) return;
        if(!actor.IsSpecialReady || board.OwnedRootCount(actor)>0 || !actor.TryBeginSpecialAbilityAnimationAction()) return;
        pending=true;Outcome=null;actor.PrepareSpecialMotion("Release");Publish();
        if(!board.TryQueuePlantRoots(actor,1,1,false,false,success=>
        {
            if(success && actor!=null && !actor.IsDefeated)
            {actor.AnnounceCommittedCast();actor.NotifySpecialAbilityUsed();}
            Finish(success);
        })) Finish(false);
    }
    private void Finish(bool success)
    {
        pending=false;Outcome=success?"Planted":"Fizzled";
        if(actor!=null && !actor.IsDefeated)
        {
            actor.ResetSpecialCounter();actor.SetSpecialTurnRequirement(4);actor.EndSpecialAbilityAnimationAction();
        }
        Publish();
    }
    private void Publish() {actor?.GetComponent<EnemyAutoAttack>()?.SetActionPaused(this,pending);Changed?.Invoke();}
    private void Interrupted(EnemyStagger source,float duration,float remaining)
    {if(pending) Finish(false);}
    private void Died(EnemyActor owner) => board.RemoveVineSource(owner);
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf) {saved.rootbinderOutcome=Outcome;}
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt)
    {
        var legacy=board.RestoredVineCast(actor);board.CancelGemSetThreat(legacy);
        Outcome=legacy!=null?"Rules updated":saved.rootbinderOutcome;
        if(legacy!=null) {actor.ResetSpecialCounter();actor.SetSpecialTurnRequirement(4);}
        Publish();
    }
    private void OnDisable()
    {
        if(actor!=null) {actor.Defeated-=Died;board?.RemoveVineSource(actor);}
        if(stagger!=null) stagger.StaggerApplied-=Interrupted;
        actor?.GetComponent<EnemyAutoAttack>()?.SetActionPaused(this,false);
    }
}
