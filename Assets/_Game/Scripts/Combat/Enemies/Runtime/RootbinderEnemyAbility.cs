using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RootbinderEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility, IEnemyContinuationOwner
{
    private EnemyActor actor;
    private BoardController board;
    private EnemyStagger stagger;
    private BoardController.GemSetThreat warning;
    private bool pending;
    public bool IsWarning => warning!=null && !warning.Ended;
    public int ResponseMoves => IsWarning ? Mathf.Max(0,warning.DueMove-board.CompletedValidPlayerMoves) : 0;
    public void InitializeSpecialAbility(EnemyActor owner, BoardController initializedBoard, IReadOnlyList<EnemyActor> enemies)
    {
        actor=owner;board=initializedBoard;stagger=actor.GetComponent<EnemyStagger>();
        actor.Defeated+=Died; if(stagger!=null) stagger.StaggerApplied+=Interrupted;
    }
    public void ResolveAcceptedMove()
    {
        if(!CombatMoveClock.CanOffer(actor) || pending || actor.IsDefeated) return;
        if(warning!=null && !warning.Ended)
        {
            if(board.CompletedValidPlayerMoves<warning.DueMove || !actor.TryBeginSpecialAbilityAnimationAction()) return;
            pending=true;
            if(!board.TryQueueResolveVines(warning,success=>Finish(success))) Finish(false);
            return;
        }
        if(!actor.IsSpecialReady || !actor.TryBeginSpecialAbilityAnimationAction()) return;
        pending=true;
        if(!board.TryQueueVineWarning(actor,2,3,result=>
        {
            warning=result;pending=false;
            if(result!=null) actor.NotifySpecialAbilityUsed();
            actor.EndSpecialAbilityAnimationAction();
        })) { pending=false;actor.EndSpecialAbilityAnimationAction(); }
    }
    private void Finish(bool success)
    {
        pending=false;warning=null;
        if(actor==null || actor.IsDefeated) return;
        if(success) { actor.ResetSpecialCounter();actor.SetSpecialTurnRequirement(4); }
        actor.EndSpecialAbilityAnimationAction();
    }
    private void Interrupted(EnemyStagger source,float duration,float remaining)
    { if(warning!=null) board.CancelGemSetThreat(warning); warning=null; pending=false;actor.ResetSpecialCounter(); }
    private void Died(EnemyActor owner) => board.RemoveVineSource(owner);
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf) { }
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt) { warning=board.RestoredVineCast(actor); }
    private void OnDisable()
    {
        if(actor!=null) { actor.Defeated-=Died;board?.RemoveVineSource(actor); }
        if(stagger!=null) stagger.StaggerApplied-=Interrupted;
    }
}
