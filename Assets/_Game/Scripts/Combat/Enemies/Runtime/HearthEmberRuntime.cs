using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class HearthEmberSnapshot
{
    public bool active;
    public long targetId;
    public int createdMove, expiresMove, lastHealMove;
    public static bool Supports(HearthEmberSnapshot s)=>s==null ||
        (s.targetId>=0 && s.createdMove>=0 && s.lastHealMove>=0 && s.expiresMove>=0 &&
        (!s.active || (s.targetId>0 && s.expiresMove-s.createdMove==3 &&
            s.lastHealMove>=s.createdMove && s.lastHealMove<s.expiresMove)));
}

/// <summary>Fixed-recipient healing; the ordinary board structure owns occupancy and destruction.</summary>
[DisallowMultipleComponent]
public sealed class HearthEmberRuntime : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility, IEnemyContinuationOwner
{
    private EnemyActor actor,target;
    private BoardController board;
    private IReadOnlyList<EnemyActor> roster;
    private HearthEmberSnapshot state=new HearthEmberSnapshot();
    private bool pending,disposed;
    public bool HasEmber => state.active;
    public EnemyActor Target=>target;
    public int RemainingMoves=>HasEmber?Mathf.Max(0,state.expiresMove-board.CompletedValidPlayerMoves):0;
    private bool InvalidCaster=>disposed || actor==null || actor.IsDefeated;
    private bool ValidTarget=>target!=null && !target.IsDefeated && target.isActiveAndEnabled && target.PersistentId==state.targetId;
    public void InitializeSpecialAbility(EnemyActor owner,BoardController initializedBoard,IReadOnlyList<EnemyActor> enemies)
    {
        actor=owner;board=initializedBoard;roster=enemies;
        actor.Defeated+=Died;board.HearthDestroyed+=Destroyed;board.HearthMoveAdvanced+=Advance;
    }
    public void ResolveAcceptedMove()
    {
        if(InvalidCaster || pending || HasEmber || board.HasHearthEmber() || !actor.IsSpecialReady ||
            actor.GetComponent<EnemyStagger>()?.IsStaggered==true)return;
        EnemyActor selected=null;
        foreach(var candidate in roster)
            if(candidate!=null && candidate!=actor && candidate.IsInitialized && !candidate.IsDefeated &&
                candidate.CurrentHealth<candidate.MaxHealth && (selected==null ||
                candidate.CurrentHealth*(long)selected.MaxHealth<selected.CurrentHealth*(long)candidate.MaxHealth ||
                (candidate.CurrentHealth*(long)selected.MaxHealth==selected.CurrentHealth*(long)candidate.MaxHealth &&
                candidate.PersistentId<selected.PersistentId)))selected=candidate;
        if(selected==null || !actor.TryBeginSpecialAbilityAnimationAction())return;
        pending=true;target=selected;state.targetId=target.PersistentId;
        actor.PrepareSpecialMotion("Ability");
        if(!board.TryQueuePlaceBarricades(actor,1,1,1,EnemyBarricadeStyle.HearthEmber,
            protectSpecialGems:true,waitForAnimationImpact:true,
            isCancelled:()=>InvalidCaster || !ValidTarget || actor.GetComponent<EnemyStagger>()?.IsStaggered==true,
            completed:placed=>
            {
                pending=false;
                if(placed && !InvalidCaster && ValidTarget && board.HasHearthEmber(actor.GetInstanceID()))
                {
                    state.active=true;state.createdMove=board.CompletedValidPlayerMoves;
                    state.lastHealMove=state.createdMove;state.expiresMove=state.createdMove+3;
                    target.Defeated+=TargetDied;
                    actor.AnnounceCommittedCast();actor.NotifySpecialAbilityUsed();actor.ResetSpecialCounter();
                }
                else End(true);
                actor?.EndSpecialAbilityAnimationAction();
            }))
        {pending=false;target=null;actor.EndSpecialAbilityAnimationAction();}
    }
    private void Advance(int move)
    {
        if(!HasEmber || move<=state.lastHealMove)return;
        if(InvalidCaster || !ValidTarget || !board.HasHearthEmber(actor.GetInstanceID())){End(true);return;}
        // Consume this move before healing callbacks, and never replay missed moves.
        state.lastHealMove=move;
        if(move>state.createdMove && move<=state.expiresMove)target.RestoreHealth(5);
        if(move>=state.expiresMove)End(true);
    }
    private void Destroyed(int owner){if(actor!=null && actor.GetInstanceID()==owner)End(false);}
    private void Died(EnemyActor source)=>End(true);
    private void TargetDied(EnemyActor source)=>End(true);
    private void End(bool remove)
    {
        state.active=false;
        if(target!=null)target.Defeated-=TargetDied;
        target=null;state.targetId=0;
        if(remove && board!=null && actor!=null)board.QueueExtinguishHearth(actor.GetInstanceID());
        actor?.ResetSpecialCounter();
    }
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf)
        =>saved.hearthEmber=JsonUtility.FromJson<HearthEmberSnapshot>(JsonUtility.ToJson(state));
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt)
    {
        state=saved.hearthEmber==null?new HearthEmberSnapshot():
            JsonUtility.FromJson<HearthEmberSnapshot>(JsonUtility.ToJson(saved.hearthEmber));
        target=null;
        if(!HasEmber)return;
        foreach(var enemy in roster)if(enemy!=null && enemy.PersistentId==state.targetId)target=enemy;
        if(!ValidTarget || !board.HasHearthEmber(actor.GetInstanceID()))End(true);
        else target.Defeated+=TargetDied;
    }
    private void OnDisable()
    {
        disposed=true;End(true);actor?.EndSpecialAbilityAnimationAction();
        if(actor!=null)actor.Defeated-=Died;
        if(board!=null){board.HearthDestroyed-=Destroyed;board.HearthMoveAdvanced-=Advance;}
    }
}
