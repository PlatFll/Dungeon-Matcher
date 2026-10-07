using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ForestMilestoneSnapshot
{
    public int version, state, deadline, sequence, resolvedSequence, nextAbility, activeAbility;
    public bool heartrootsArmed;
    public string outcome;
}

/// <summary>Forest milestone casts; board owns roots/vines and EnemyStagger owns all interruption.</summary>
[DisallowMultipleComponent]
public sealed class ForestMilestoneEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility, IEnemyContinuationOwner
{
    private EnemyActor actor;
    private BoardController board;
    private IReadOnlyList<EnemyActor> roster;
    private EnemyAutoAttack attack;
    private EnemyStagger stagger;
    private BoardController.GemSetThreat warning;
    private ForestMilestoneSnapshot value=new ForestMilestoneSnapshot {version=2};
    private readonly HashSet<EnemyActor> protectedActors=new HashSet<EnemyActor>();
    private bool pending;
    private WaveController waves;
    private bool Ritual => actor.Definition.SpecialAbilityKind==EnemySpecialAbilityKind.GroveRenewal;
    public bool IsPreparing => value.state==1;
    public bool IsProtected => isActiveAndEnabled && actor!=null && !actor.IsDefeated && !Ritual && board!=null && board.OwnedRootCount(actor)>0;
    public bool BlocksBasic => pending || IsPreparing;
    public string Outcome => value.outcome;
    public int ResponseMoves => Mathf.Max(0,value.deadline-(CombatMoveClock.Current?.Tick ?? 0));
    public int ChannelMoves => !Ritual?1:value.activeAbility==2?actor.Definition.ForestHarvestChannelMoves:actor.Definition.ForestRenewalChannelMoves;
    public string CastName => !Ritual?"ROOT":value.activeAbility==0?"RENEW":value.activeAbility==1?"SURGE":"HARVEST";
    public int RootCount => board!=null?board.OwnedRootCount(actor):0;
    public event Action Changed;

    public void InitializeSpecialAbility(EnemyActor owner,BoardController initializedBoard,IReadOnlyList<EnemyActor> enemies)
    {
        actor=owner;board=initializedBoard;roster=enemies;
        attack=actor.GetComponent<EnemyAutoAttack>();stagger=actor.GetComponent<EnemyStagger>();
        actor.Defeated+=Died;board.RootsChanged+=RootsChanged;
        if(stagger!=null) stagger.StaggerApplied+=Interrupted;
        waves=RunSession.Current?.Waves;
        if(waves!=null) waves.EnemySpawned+=ProtectSpawned;
        BindProtection();
    }
    private void ProtectSpawned(EnemyActor member)
    {
        if(!Ritual && member!=null && protectedActors.Add(member))
            member.SetSharedDamageReduction(this,()=>IsProtected?.75f:1f);
    }
    private void BindProtection()
    {
        if(Ritual) return;
        ProtectSpawned(actor);
        foreach(var member in roster) ProtectSpawned(member);
    }
    private void LateUpdate()
    {
        if(actor==null || actor.IsDefeated) return;
        BindProtection();
        if(!board.IsBusy && !pending) CheckPreparation();
    }
    private void RootsChanged()
    {
        if(actor==null || actor.IsDefeated || pending) return;
        if(Ritual && value.heartrootsArmed && RootCount==0)
        {
            value.heartrootsArmed=false;
            if(IsPreparing) Finish("Interrupted",true);
            else { value.outcome="Heartroots broken";stagger?.ApplyStagger(1,1);Publish(); }
        }
        if(!Ritual && value.state==2 && RootCount==0)
        { value.state=0;actor.ResetSpecialCounter();Publish(); }
    }
    private void CheckPreparation()
    {
        RootsChanged();
        if(!Ritual && IsPreparing && (warning==null || warning.Ended))
            Finish(warning?.PlayerInterrupted==true?"Interrupted":"Target lost",warning?.PlayerInterrupted==true);
    }
    public void ResolveAcceptedMove()
    {
        if(actor==null || actor.IsDefeated || !CombatMoveClock.CanOffer(actor) || pending) return;
        CheckPreparation();
        if(value.state==2 || (stagger!=null && stagger.IsStaggered)) return;
        if(IsPreparing)
        {
            if(CombatMoveClock.Current.Tick<value.deadline) return;
            if(!Ritual)
            {
                pending=true;Publish();
                if(!board.TryQueueResolveVines(warning,ok=>
                {
                    pending=false;warning=null;
                    if(actor==null || actor.IsDefeated) return;
                    if(ok && RootCount>0) {value.state=2;value.outcome="Planted";value.resolvedSequence=value.sequence;Publish();}
                    else Finish("Target lost",false);
                })) {pending=false;Finish("Target lost",false);}
            }
            else if(value.activeAbility==0)
            {
                int amount=actor.Definition.ForestRenewalBaseHeal+actor.Definition.ForestHeartrootHealBonus*RootCount;
                Finish("Renewed",false); // terminal identity before any heal callback
                foreach(var member in new List<EnemyActor>(roster))
                    if(member!=null && member.IsInitialized && !member.IsDefeated) member.RestoreHealth(amount);
            }
            else if(value.activeAbility==2)
            {
                pending=true;Publish();int sequence=value.sequence;
                if(!board.QueueVineHarvest(actor,count=>
                {
                    if(actor==null || actor.IsDefeated || !IsPreparing || value.sequence!=sequence) return;
                    Finish("Harvested",false);
                    int damage=CombatAmounts.Round(actor.Definition.ForestHarvestBaseDamage*actor.RuntimeStats.DamageMultiplier)+
                        actor.Definition.ForestHarvestDamagePerVine*count;
                    attack?.PlayerTarget?.TryTakeDamage(damage,actor);
                },ok=>{pending=false;if(!ok && IsPreparing) Finish("Fizzled",false);Publish();}))
                {pending=false;Finish("Fizzled",false);}
            }
            return;
        }
        if(!actor.IsSpecialReady || !actor.TryBeginSpecialAbilityAnimationAction()) return;
        value.sequence++;value.outcome=null;value.activeAbility=Ritual?value.nextAbility:0;
        pending=true;Publish();
        if(!Ritual)
        {
            if(!board.TryQueueRootWarning(actor,1,2,false,true,w=>Started(w!=null,w))) Started(false,null);
        }
        else if(value.activeAbility==0 && RootCount==0)
        {
            if(!board.TryQueuePlantRoots(actor,2,2,true,true,ok=>Started(ok,null))) Started(false,null);
        }
        else Started(true,null);
    }
    private void Started(bool success,BoardController.GemSetThreat result)
    {
        pending=false;actor.EndSpecialAbilityAnimationAction();
        if(actor.IsDefeated) {board.RemoveVineSource(actor);return;}
        if(!success || (stagger!=null && stagger.IsStaggered))
        {
            value.state=1;Finish(stagger?.IsStaggered==true?"Interrupted":"Fizzled",false);return;
        }
        warning=result;value.state=1;
        if(Ritual && RootCount==2) value.heartrootsArmed=true;
        value.deadline=!Ritual?warning.DueMove:CombatMoveClock.EffectAction+ChannelMoves;
        if(!Ritual || value.activeAbility!=1) actor.AnnounceCommittedCast(!Ritual?"Guarding Roots":value.activeAbility==0?"Renew the Grove":"Thorn Harvest");
        actor.NotifySpecialAbilityUsed();actor.ResetSpecialCounter();Publish();
        if(Ritual && value.activeAbility==1)
        {
            pending=true;
            if(!board.QueueVineSurge(actor,ok=>{if(ok) actor.AnnounceCommittedCast("Verdant Surge");pending=false;Finish(ok?"Surged":"Fizzled",false);}))
            {pending=false;Finish("Fizzled",false);}
        }
    }
    private void Finish(string outcome,bool applyStagger)
    {
        if(!IsPreparing || value.resolvedSequence==value.sequence) return;
        value.resolvedSequence=value.sequence;value.outcome=outcome;
        value.state=0;value.deadline=0;
        if(Ritual) value.nextAbility=(value.activeAbility+1)%3;
        if(warning!=null) board.CancelGemSetThreat(warning);warning=null;
        actor.ResetSpecialCounter();actor.SetSpecialTurnRequirement(3);
        // Existing rank duration, immunity, meter and presentation remain authoritative.
        if(applyStagger) stagger?.ApplyStagger(1,1);
        Publish();
    }
    private void Publish() {attack?.SetActionPaused(this,BlocksBasic);Changed?.Invoke();}
    private void Interrupted(EnemyStagger source,float duration,float remaining)
    {if(IsPreparing) Finish("Interrupted",false);}
    private void Died(EnemyActor owner) {board.RemoveVineSource(actor);attack?.SetActionPaused(this,false);}
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf)
    {saved.forestMilestone=JsonUtility.FromJson<ForestMilestoneSnapshot>(JsonUtility.ToJson(value));}
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt)
    {
        value=saved.forestMilestone==null?new ForestMilestoneSnapshot {version=2}:
            JsonUtility.FromJson<ForestMilestoneSnapshot>(JsonUtility.ToJson(saved.forestMilestone));
        if(value.version<2)
        {
            // Previous anchors/exposure are retired. Preserve the attempt, with
            // an old pending milestone safely fizzled rather than applying it twice.
            value.version=2;value.state=4;value.outcome="Rules updated";
            value.resolvedSequence=value.sequence;
            value.nextAbility=0;value.heartrootsArmed=false;
        }
        if(value.state==4)
        {
            // A legacy terminal cast already consumed its effect and rotation.
            value.state=0;value.deadline=0;value.resolvedSequence=value.sequence;
            actor.ResetSpecialCounter();actor.SetSpecialTurnRequirement(3);
        }
        warning=board.RestoredVineCast(actor);BindProtection();Publish();
    }
    private void OnDisable()
    {
        if(actor!=null) {actor.Defeated-=Died;board?.RemoveVineSource(actor);}
        if(board!=null) board.RootsChanged-=RootsChanged;
        if(stagger!=null) stagger.StaggerApplied-=Interrupted;
        if(waves!=null) waves.EnemySpawned-=ProtectSpawned;
        foreach(var member in protectedActors) if(member!=null) member.RemoveSharedDamageReduction(this);
        protectedActors.Clear();attack?.SetActionPaused(this,false);
    }
}
