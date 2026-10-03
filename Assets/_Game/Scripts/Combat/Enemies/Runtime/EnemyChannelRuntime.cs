using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EnemyChannelSnapshot
{
    public int state, sequence, triggerMove, deadlineMove, recoveryUntil, lastOutcomeSequence;
    public long targetId;
    public string outcome;
}

/// <summary>Cancelable fixed-target heal; animation never authorizes completion.</summary>
[DisallowMultipleComponent]
public sealed class EnemyChannelRuntime : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility, IEnemyContinuationOwner
{
    private EnemyActor actor;
    private IReadOnlyList<EnemyActor> roster;
    private EnemyStagger stagger;
    private EnemyAutoAttack attack;
    private EnemyActor target;
    private EnemyChannelSnapshot value = new EnemyChannelSnapshot();
    public event Action Changed;
    public bool IsChanneling => value.state == 1;
    public bool BlocksBasic => value.state != 0;
    public int ResponseMoves => IsChanneling ? Mathf.Max(0,value.deadlineMove-(CombatMoveClock.Current?.Tick ?? 0)) : 0;
    public EnemyActor Target => target;
    public string Outcome => value.outcome;

    public void InitializeSpecialAbility(EnemyActor owner, BoardController board, IReadOnlyList<EnemyActor> enemies)
    {
        Cleanup(); actor=owner; roster=enemies; value=new EnemyChannelSnapshot();
        stagger=actor.GetComponent<EnemyStagger>(); attack=actor.GetComponent<EnemyAutoAttack>();
        actor.Defeated+=CasterDied;
        if(stagger!=null) stagger.StaggerApplied+=Staggered;
        var view=GetComponent<EnemyMoveIntentView>() ?? gameObject.AddComponent<EnemyMoveIntentView>();
        view.Initialize(actor);
    }

    public void ResolveAcceptedMove()
    {
        if(!CombatMoveClock.Active || actor==null || actor.IsDefeated) return;
        int move=CombatMoveClock.Current.Tick;
        if(value.state==2)
        {
            if(move>=value.recoveryUntil) { value.state=0; attack?.SetActionPaused(this,false); Changed?.Invoke(); }
            return;
        }
        if(IsChanneling)
        {
            if(!ValidTarget()) { Finish("Target lost",false); return; }
            if(stagger!=null && stagger.IsStaggered) { Finish("Interrupted",false); return; }
            if(move>=value.deadlineMove) Finish("Healed",true);
            return;
        }
        if(!actor.IsSpecialReady || (stagger!=null && stagger.IsStaggered)) return;
        EnemyActor selected=null;
        foreach(var candidate in roster)
        {
            if(candidate==null || candidate==actor || candidate.IsDefeated || !candidate.IsInitialized ||
                candidate.CurrentHealth*4L>=candidate.MaxHealth*3L) continue;
            if(selected==null || candidate.CurrentHealth*(long)selected.MaxHealth < selected.CurrentHealth*(long)candidate.MaxHealth ||
                (candidate.CurrentHealth*(long)selected.MaxHealth == selected.CurrentHealth*(long)candidate.MaxHealth && candidate.PersistentId<selected.PersistentId)) selected=candidate;
        }
        if(selected==null || !actor.TryBeginSpecialAbilityAnimationAction()) return;
        target=selected; target.Defeated+=RecipientDied;
        value.state=1; value.sequence++; value.targetId=target.PersistentId;
        value.triggerMove=move; value.deadlineMove=move+2; value.outcome=null;
        attack?.SetActionPaused(this,true);
        actor.NotifySpecialAbilityUsed(); actor.ResetSpecialCounter(); actor.EndSpecialAbilityAnimationAction();
        Changed?.Invoke();
    }

    private bool ValidTarget()
    {
        if(target==null || !target.isActiveAndEnabled || !target.IsInitialized || target.IsDefeated || target.PersistentId!=value.targetId) return false;
        foreach(var member in roster) if(member==target) return true;
        return false;
    }
    private void LateUpdate() {if(IsChanneling && !ValidTarget()) Finish("Target lost",false);}
    private void Finish(string outcome, bool heal)
    {
        if(!IsChanneling || value.lastOutcomeSequence==value.sequence) return;
        value.lastOutcomeSequence=value.sequence;
        value.state=2; value.outcome=outcome;
        value.recoveryUntil=CombatMoveClock.EffectAction+2;
        // Terminal state precedes actor events; reentrant removal cannot heal twice.
        if(heal && actor!=null && !actor.IsDefeated && ValidTarget()) target.RestoreHealth(20);
        if(target!=null) target.Defeated-=RecipientDied;
        target=null;
        Changed?.Invoke();
    }
    private void Staggered(EnemyStagger source,float duration,float remaining) => Finish("Interrupted",false);
    private void RecipientDied(EnemyActor source) => Finish("Target lost",false);
    private void CasterDied(EnemyActor source) { Finish("Caster defeated",false); attack?.SetActionPaused(this,false); }

    public void CaptureContinuation(EnemyCombatSnapshot saved, Func<EnemyActor,int> slotOf)
    { saved.channel=JsonUtility.FromJson<EnemyChannelSnapshot>(JsonUtility.ToJson(value)); }
    public void RestoreContinuation(EnemyCombatSnapshot saved, Func<int,EnemyActor> enemyAt)
    {
        if(target!=null) target.Defeated-=RecipientDied;
        value=saved.channel==null ? new EnemyChannelSnapshot() :
            JsonUtility.FromJson<EnemyChannelSnapshot>(JsonUtility.ToJson(saved.channel)); target=null;
        if(IsChanneling)
        {
            foreach(var candidate in roster) if(candidate!=null && candidate.PersistentId==value.targetId && !candidate.IsDefeated) target=candidate;
            if(!ValidTarget()) Finish("Target lost",false);
            else target.Defeated+=RecipientDied;
        }
        attack?.SetActionPaused(this,BlocksBasic); Changed?.Invoke();
    }
    private void Cleanup()
    {
        if(actor!=null) actor.Defeated-=CasterDied;
        if(stagger!=null) stagger.StaggerApplied-=Staggered;
        if(target!=null) target.Defeated-=RecipientDied;
        attack?.SetActionPaused(this,false);
    }
    private void OnDisable() { Finish("Removed",false); Cleanup(); }
}
