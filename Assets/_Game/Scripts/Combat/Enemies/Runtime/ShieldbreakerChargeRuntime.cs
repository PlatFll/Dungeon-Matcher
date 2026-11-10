using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ShieldbreakerSnapshot
{
    public bool channeling;
    public int deadline, recoveryUntil;
    public static bool Supports(ShieldbreakerSnapshot s)=>s==null ||
        (s.deadline>=0 && s.recoveryUntil>=0 && (!s.channeling || (s.deadline>0 && s.recoveryUntil==0)));
}

/// <summary>One saved channel, two ordinary damage packets, then move-owned recovery.</summary>
[DisallowMultipleComponent]
public sealed class ShieldbreakerChargeRuntime : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility, IEnemyContinuationOwner
{
    private EnemyActor actor;
    private EnemyAutoAttack attack;
    private EnemyStagger stagger;
    private BoardController board;
    private ShieldbreakerSnapshot state = new ShieldbreakerSnapshot();
    private bool releasing, disposed;
    private int Move => CombatMoveClock.Current?.Tick ?? board?.CompletedValidPlayerMoves ?? 0;
    public bool IsChanneling => state.channeling;
    public int ResponseMoves => IsChanneling ? Mathf.Max(0,state.deadline-Move) : 0;
    public int RecoveryMoves => Mathf.Max(0,state.recoveryUntil-Move);
    public bool BlocksBasic => IsChanneling || releasing || state.recoveryUntil>0;

    public void InitializeSpecialAbility(EnemyActor owner,BoardController initializedBoard,IReadOnlyList<EnemyActor> enemies)
    {
        actor=owner;board=initializedBoard;attack=owner.GetComponent<EnemyAutoAttack>();
        stagger=owner.GetComponent<EnemyStagger>();actor.Defeated+=Died;
        if(stagger!=null)stagger.StaggerApplied+=Interrupted;
    }
    public void ResolveAcceptedMove()
    {
        if(disposed || actor.IsDefeated || releasing || stagger?.IsStaggered==true || board.IsBusy)return;
        if(state.recoveryUntil>0)
        {
            if(RecoveryMoves>0)return;
            state.recoveryUntil=0;Refresh();return;
        }
        if(IsChanneling)
        {
            if(ResponseMoves>0 || !actor.TryBeginSpecialAbilityAnimationAction())return;
            state.channeling=false;releasing=true;actor.SpecialIdleState=null;
            StartCoroutine(Release());return;
        }
        if(!actor.IsSpecialReady || !actor.TryBeginSpecialAbilityAnimationAction())return;
        state.channeling=true;state.deadline=Move+2;Refresh();
        actor.AnnounceCommittedCast();actor.NotifySpecialAbilityUsed();actor.ResetSpecialCounter();
        actor.StartSpecialMotion("ChannelStart");
        StartCoroutine(FinishTelegraphMotion());
    }
    private IEnumerator FinishTelegraphMotion()
    {
        int motion=actor.SpecialMotionId;
        if(motion>0)yield return actor.WaitForSpecialMotionComplete(motion);
        actor.EndSpecialAbilityAnimationAction();Refresh();
    }
    private bool Cancelled => disposed || actor==null || actor.IsDefeated || stagger?.IsStaggered==true;
    private IEnumerator Release()
    {
        int motion=actor.StartSpecialMotion("Release");
        if(motion>0)yield return actor.WaitForSpecialMotionBeat(motion);
        if(!Cancelled)Hit(actor.Definition.shieldbreakerFirstDamage);
        yield return new WaitForSeconds(.18f);
        if(!Cancelled)Hit(actor.Definition.shieldbreakerMainDamage);
        if(motion>0 && !Cancelled)yield return actor.WaitForSpecialMotionComplete(motion);
        releasing=false;
        if(!Cancelled)state.recoveryUntil=Move+Mathf.Max(1,actor.Definition.shieldbreakerRecoveryMoves);
        actor?.ResetSpecialCounter();actor?.EndSpecialAbilityAnimationAction();Refresh();
    }
    private void Hit(int amount) => (attack?.PlayerTarget ?? RunSession.Current?.Player)?.TryTakeDamage(
        CombatAmounts.Round(amount*actor.RuntimeStats.DamageMultiplier),actor);
    private void Refresh()
    {
        if(actor!=null)actor.SpecialIdleState=IsChanneling?"ChannelHold":null;
        attack?.SetActionPaused(this,BlocksBasic);
    }
    private void Interrupted(EnemyStagger source,float duration,float remaining)=>Cancel();
    private void Died(EnemyActor source)=>Cancel();
    private void Cancel()
    {
        StopAllCoroutines();state.channeling=false;state.recoveryUntil=0;releasing=false;
        actor?.ResetSpecialCounter();actor?.EndSpecialAbilityAnimationAction();Refresh();
    }
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf)
        =>saved.shieldbreaker=JsonUtility.FromJson<ShieldbreakerSnapshot>(JsonUtility.ToJson(state));
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt)
    {
        state=saved.shieldbreaker==null?new ShieldbreakerSnapshot():
            JsonUtility.FromJson<ShieldbreakerSnapshot>(JsonUtility.ToJson(saved.shieldbreaker));
        // Retired Hallbreaker warnings never release a stale marked-gem strike.
        board.CancelGemSetThreat(board.RestoredSet(actor));Refresh();
    }
    private void OnDisable()
    {
        disposed=true;Cancel();
        if(actor!=null)actor.Defeated-=Died;
        if(stagger!=null)stagger.StaggerApplied-=Interrupted;
    }
}
