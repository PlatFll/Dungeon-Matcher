using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ForestMilestoneSnapshot
{
    public int state, deadline, exposureStarted, sequence, resolvedSequence;
    public long targetId;
    public string outcome;
}

/// <summary>Two reviewed kits sharing board-owned, nonspreading vine anchors.</summary>
[DisallowMultipleComponent]
public sealed class ForestMilestoneEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility, IEnemyContinuationOwner
{
    private EnemyActor actor, target;
    private BoardController board;
    private IReadOnlyList<EnemyActor> roster;
    private EnemyAutoAttack attack;
    private EnemyStagger stagger;
    private BoardController.GemSetThreat warning;
    private ForestMilestoneSnapshot value = new ForestMilestoneSnapshot();
    private bool pending, removing;
    private bool Ritual => actor.Definition.SpecialAbilityKind == EnemySpecialAbilityKind.GroveRenewal;
    public bool IsPreparing => value.state == 1;
    public bool IsProtected => !Ritual && value.state == 2 && board.OwnedVineCount(actor)>0;
    public bool IsExposed => value.state == 3;
    public bool BlocksBasic => value.state == 1 || value.state == 3 || value.state == 4;
    public EnemyActor Target => target;
    public string Outcome => value.outcome;
    public int ResponseMoves => Mathf.Max(0,value.deadline-(CombatMoveClock.Current?.Tick ?? 0));
    public float WeaknessMultiplier => IsExposed &&
        (CombatMoveClock.EffectAction>value.exposureStarted || !board.IsBusy) ? 1.25f : 1f;
    public event Action Changed;

    public void InitializeSpecialAbility(EnemyActor owner, BoardController initializedBoard, IReadOnlyList<EnemyActor> enemies)
    {
        actor=owner;board=initializedBoard;roster=enemies;
        attack=actor.GetComponent<EnemyAutoAttack>();stagger=actor.GetComponent<EnemyStagger>();
        actor.Defeated+=Died;
        if(stagger!=null) stagger.StaggerApplied+=Interrupted;
        actor.IncomingDamageMultiplier=ProtectionMultiplier;
        actor.WeaknessDamageMultiplier=()=>WeaknessMultiplier;
    }
    private float ProtectionMultiplier() => IsProtected?.75f:1f;
    private void LateUpdate()
    {
        // Free skills may solve anchors without advancing the accepted clock.
        // Observe their settled result before the next input or stable save.
        if(actor==null || actor.IsDefeated || board.IsBusy || pending || removing) return;
        CheckSolved();
    }
    private void CheckSolved()
    {
        if((value.state==2 || (Ritual && value.state==1)) && board.OwnedVineCount(actor)==0)
            Finish("Anchors cleared",true,false);
        else if(Ritual && value.state==1 && !Living(target)) Finish("Target lost",false,false);
    }
    public void ResolveAcceptedMove()
    {
        if(actor==null || actor.IsDefeated || !CombatMoveClock.CanOffer(actor) || pending) return;
        CheckSolved();
        int tick=CombatMoveClock.Current.Tick;
        if(value.state==3 || value.state==4) return;
        if(value.state==2) return;
        if(value.state==1)
        {
            if(tick<value.deadline) return;
            if(Ritual) { Finish("Renewed",false,true);return; }
            if(warning==null || warning.Ended) { Finish("Anchors cleared",true,false);return; }
            if(!actor.TryBeginSpecialAbilityAnimationAction()) return;
            pending=true;
            if(!board.TryQueueResolveVines(warning,success=>
            {
                pending=false;warning=null;
                actor.EndSpecialAbilityAnimationAction();
                if(actor.IsDefeated) return;
                if(success && board.OwnedVineCount(actor)>0) { value.state=2;value.outcome="Planted";Publish(); }
                else Finish("Anchors cleared",true,false);
            })) { pending=false;actor.EndSpecialAbilityAnimationAction();Finish("Anchors cleared",true,false); }
            return;
        }
        if(!actor.IsSpecialReady || (stagger!=null && stagger.IsStaggered)) return;
        var selected=Ritual?SelectRecipient():null;
        if(Ritual && !Living(selected)) return;
        if(!actor.TryBeginSpecialAbilityAnimationAction()) return;
        pending=true;value.sequence++;value.outcome=null;
        if(Ritual)
        {
            target=selected;value.targetId=target.PersistentId;target.Defeated+=RecipientDied;
            if(!board.TryQueueVineAnchors(actor,2,success=>Started(success,null))) Started(false,null);
        }
        else if(!board.TryQueueVineWarning(actor,2,2,result=>Started(result!=null,result),true)) Started(false,null);
    }
    private void Started(bool success, BoardController.GemSetThreat result)
    {
        pending=false;actor.EndSpecialAbilityAnimationAction();
        if(actor.IsDefeated) { board.RemoveVineSource(actor);return; }
        if(!success || (stagger!=null && stagger.IsStaggered) || (Ritual && !Living(target)))
        {
            Unlink();board.RemoveVineSource(actor);actor.ResetSpecialCounter();
            value.state=4;value.deadline=CombatMoveClock.EffectAction+2;Publish();return;
        }
        warning=result;value.state=1;
        value.deadline=Ritual?CombatMoveClock.EffectAction+2:warning.DueMove;
        actor.NotifySpecialAbilityUsed();actor.ResetSpecialCounter();Publish();
    }
    private EnemyActor SelectRecipient()
    {
        EnemyActor best=null;
        foreach(var candidate in roster)
        {
            if(!Living(candidate) || candidate==actor || candidate.CurrentHealth*4L>=candidate.MaxHealth*3L) continue;
            if(best==null || candidate.CurrentHealth*(long)best.MaxHealth<best.CurrentHealth*(long)candidate.MaxHealth ||
                (candidate.CurrentHealth*(long)best.MaxHealth==best.CurrentHealth*(long)candidate.MaxHealth && candidate.PersistentId<best.PersistentId)) best=candidate;
        }
        return best ?? actor;
    }
    private static bool Living(EnemyActor enemy) => enemy!=null && enemy.IsInitialized && !enemy.IsDefeated;
    private void Finish(string outcome,bool exposure,bool heal)
    {
        if(value.state!=1 && value.state!=2) return;
        if(value.resolvedSequence==value.sequence) return;
        int amount=heal && Living(target)?Mathf.Min(2,board.OwnedVineCount(actor))*10:0;
        value.resolvedSequence=value.sequence;value.outcome=outcome;
        value.state=exposure?3:4;value.exposureStarted=CombatMoveClock.EffectAction;
        value.deadline=value.exposureStarted+2;
        // Consume before callbacks; neither the release clip nor reentrancy can heal twice.
        if(amount>0 && !actor.IsDefeated) target.RestoreHealth(amount);
        Unlink();removing=true;board.RemoveVineSource(actor);removing=false;warning=null;
        Publish();
    }
    public void ExpireAcceptedMove(int tick)
    {
        if(actor==null || actor.IsDefeated) return;
        if((value.state==3 || value.state==4) && tick>=value.deadline)
        {
            value.state=0;actor.ResetSpecialCounter();actor.SetSpecialTurnRequirement(3);Publish();
        }
    }
    private void Publish() { attack?.SetActionPaused(this,BlocksBasic);Changed?.Invoke(); }
    private void Unlink() { if(target!=null) target.Defeated-=RecipientDied;target=null; }
    private void RecipientDied(EnemyActor owner) { if(!pending) Finish("Target lost",false,false); }
    private void Interrupted(EnemyStagger source,float duration,float remaining)
    { if(IsPreparing) Finish("Interrupted",true,false); }
    private void Died(EnemyActor owner)
    { Unlink();board.RemoveVineSource(actor);attack?.SetActionPaused(this,false); }
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf)
    { saved.forestMilestone=JsonUtility.FromJson<ForestMilestoneSnapshot>(JsonUtility.ToJson(value)); }
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt)
    {
        Unlink();value=saved.forestMilestone==null?new ForestMilestoneSnapshot():
            JsonUtility.FromJson<ForestMilestoneSnapshot>(JsonUtility.ToJson(saved.forestMilestone));
        warning=board.RestoredVineCast(actor);
        if(Ritual && IsPreparing)
        {
            foreach(var enemy in roster) if(Living(enemy) && enemy.PersistentId==value.targetId) target=enemy;
            if(target==null) throw new InvalidOperationException("Saved ritual target is missing.");
            target.Defeated+=RecipientDied;
        }
        Publish();
    }
    private void OnDisable()
    {
        if(actor!=null) { actor.Defeated-=Died;actor.IncomingDamageMultiplier=null;actor.WeaknessDamageMultiplier=null;board?.RemoveVineSource(actor); }
        if(stagger!=null) stagger.StaggerApplied-=Interrupted;
        Unlink();attack?.SetActionPaused(this,false);
    }
}
