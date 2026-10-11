using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class FrostwakeEnemySnapshot
{
    public bool armorNext=true, shieldArmed;
}

/// <summary>Data-selected rime placement and interruptible strikes; board owns every target and clear.</summary>
[DisallowMultipleComponent]
public sealed class FrostwakeEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility, IEnemyContinuationOwner
{
    private EnemyActor actor;
    private EnemyAutoAttack attack;
    private EnemyStagger stagger;
    private BoardController board;
    private BoardController.GemSetThreat warning;
    private FrostwakeEnemySnapshot state=new FrostwakeEnemySnapshot();
    private bool pending,disposed;
    public bool IsPreparing => warning!=null && !warning.Ended;
    public bool BlocksBasic => pending || IsPreparing;
    public int ResponseMoves => IsPreparing?Mathf.Max(0,warning.DueMove-board.CompletedValidPlayerMoves):0;
    private bool InvalidTarget => warning!=null && (warning.Ended || warning.Targets.Exists(g=>!board.IsEnvironmentalOrdinaryGem(g)));
    private bool Cancelled() => disposed || actor==null || actor.IsDefeated || stagger?.IsStaggered==true;

    public void InitializeSpecialAbility(EnemyActor owner,BoardController initializedBoard,IReadOnlyList<EnemyActor> enemies)
    {
        actor=owner;board=initializedBoard;attack=owner.GetComponent<EnemyAutoAttack>();stagger=owner.GetComponent<EnemyStagger>();
        actor.Defeated+=Died;actor.ShieldChanged+=ShieldChanged;
        if(stagger!=null)stagger.StaggerApplied+=Interrupted;
    }
    private void LateUpdate()
    {
        if(!disposed && !pending && board!=null && !board.IsBusy &&
            CombatMoveClock.Current?.IsBlockingWaveProgression!=true && InvalidTarget) Finish(true);
    }
    public void ResolveAcceptedMove()
    {
        if(Cancelled() || pending || board.IsBusy || Time.timeScale<=0 ||
            actor.HasAnimationActionInProgress || !CombatMoveClock.CanOffer(actor))return;
        if(InvalidTarget){Finish(true);return;}
        if(IsPreparing)
        {
            if(ResponseMoves>0 || !actor.TryBeginSpecialAbilityAnimationAction())return;
            pending=true;actor.SpecialIdleState=null;actor.PrepareSpecialMotion("Release");
            if(!board.TryQueueResolveGemSet(warning,Impact,_=>Finish(false),Cancelled,pulseBeforeClear:true))Finish(false);
            return;
        }
        if(!actor.IsSpecialReady || !actor.TryBeginSpecialAbilityAnimationAction())return;
        pending=true;attack?.SetActionPaused(this,true);
        if(actor.Definition.SpecialAbilityKind==EnemySpecialAbilityKind.Rimecarving)
        {
            actor.PrepareSpecialMotion("Ability");
            if(!board.TryQueueEncasedGem(actor,FrostStage.Frost,target=>
            {
                if(target!=null && !Cancelled()) Commit();
                pending=false;attack?.SetActionPaused(this,false);actor.EndSpecialAbilityAnimationAction();
            },Cancelled))Finish(false);
        }
        else if(actor.Definition.mountainArmorShield>0 && state.armorNext)StartCoroutine(Armor());
        else
        {
            actor.PrepareSpecialMotion("ChannelStart");
            if(!board.TryQueueMarkGemSet(actor,actor.Definition.mountainMarkedGems,actor.Definition.mountainResponseMoves,
                false,marked=>
                {
                    pending=false;warning=marked;
                    if(marked!=null && !Cancelled()) {Commit();actor.SpecialIdleState="ChannelHold";}
                    else {board.CancelGemSetThreat(marked);warning=null;}
                    attack?.SetActionPaused(this,IsPreparing);actor.EndSpecialAbilityAnimationAction();
                },Cancelled,cancelOnAnyTargetLost:true,label:EnemyAbilityNames.Primary(actor.Definition)))Finish(false);
        }
    }
    private void Commit(){actor.AnnounceCommittedCast();actor.NotifySpecialAbilityUsed();actor.ResetSpecialCounter();}
    private IEnumerator Armor()
    {
        int motion=actor.StartSpecialMotion("Ability");
        if(motion>0)yield return actor.WaitForSpecialMotionBeat(motion);
        if(!Cancelled() && (motion==0 || actor.IsSpecialMotionCurrent(motion)))
        {
            actor.GrantShield(actor.Definition.mountainArmorShield);state.shieldArmed=actor.CurrentShield>0;
            state.armorNext=false;actor.AnnounceCommittedCast("Glacial Armor");
            actor.NotifySpecialAbilityUsed();actor.ResetSpecialCounter();
        }
        if(motion>0)yield return actor.WaitForSpecialMotionComplete(motion);
        pending=false;attack?.SetActionPaused(this,false);actor.EndSpecialAbilityAnimationAction();
    }
    private void Impact()
    {
        if(Cancelled())return;
        (attack?.PlayerTarget ?? RunSession.Current?.Player)?.TryTakeDamage(
            CombatAmounts.Round(actor.Definition.mountainStrikeDamage*actor.RuntimeStats.DamageMultiplier),actor);
    }
    private void Finish(bool countered)
    {
        bool hadWarning=warning!=null;board?.CancelGemSetThreat(warning);warning=null;pending=false;
        if(hadWarning)state.armorNext=true;
        if(actor!=null){actor.SpecialIdleState=null;actor.ResetSpecialCounter();actor.EndSpecialAbilityAnimationAction();}
        attack?.SetActionPaused(this,false);
        if(countered && hadWarning)actor?.GetComponent<ForestEnemyMotion>()?.PlayInterruptRecovery();
        if(countered && hadWarning && actor!=null && !actor.IsDefeated)stagger?.ApplyStagger(1,1);
    }
    private void ShieldChanged(EnemyActor _,int current,int maximum)
    {
        if(!state.shieldArmed || current>0 || actor.IsDefeated)return;
        state.shieldArmed=false;stagger?.ApplyStagger(1,1);
    }
    private void Interrupted(EnemyStagger _,float duration,float remaining){if(warning!=null)Finish(false);}
    private void Died(EnemyActor _) => Cleanup();
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf)
    {saved.frostwakeEnemy=new FrostwakeEnemySnapshot{armorNext=state.armorNext,shieldArmed=state.shieldArmed};}
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt)
    {
        state=saved.frostwakeEnemy==null?new FrostwakeEnemySnapshot():
            new FrostwakeEnemySnapshot{armorNext=saved.frostwakeEnemy.armorNext,shieldArmed=saved.frostwakeEnemy.shieldArmed};
        warning=board.RestoredSet(actor);pending=false;
        actor.SpecialIdleState=IsPreparing?"ChannelHold":null;attack?.SetActionPaused(this,IsPreparing);
    }
    private void Cleanup()
    {
        if(disposed)return;disposed=true;StopAllCoroutines();Finish(false);
        if(actor!=null){actor.Defeated-=Died;actor.ShieldChanged-=ShieldChanged;}
        if(stagger!=null)stagger.StaggerApplied-=Interrupted;
    }
    private void OnDisable()=>Cleanup();
}
