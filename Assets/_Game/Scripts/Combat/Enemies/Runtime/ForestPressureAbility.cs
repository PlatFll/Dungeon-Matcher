using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Move-telegraphed forest attacks; board owns response cells, actors own damage/shield.</summary>
[DisallowMultipleComponent]
public sealed class ForestPressureAbility : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility,IEnemyContinuationOwner
{
    private EnemyActor actor;
    private EnemyAutoAttack attack;
    private EnemyStagger stagger;
    private BoardController board;
    private BoardController.CellResponseThreat warning;
    private ForestRosterSnapshot state=new ForestRosterSnapshot();
    private bool pending,released;
    public bool IsPreparing => warning!=null && !warning.Ended;
    public int ResponseMoves => IsPreparing?Mathf.Max(0,warning.DueMove-board.CompletedValidPlayerMoves):0;
    public string CastName => IsTreant?"BOUGH":"VOLLEY";
    private bool IsTreant => actor.Definition.SpecialAbilityKind==EnemySpecialAbilityKind.AncientBough;
    public void InitializeSpecialAbility(EnemyActor owner,BoardController initializedBoard,IReadOnlyList<EnemyActor> enemies)
    {
        actor=owner;board=initializedBoard;attack=actor.GetComponent<EnemyAutoAttack>();stagger=actor.GetComponent<EnemyStagger>();
        actor.Defeated+=Died;actor.ShieldChanged+=ShieldChanged;
        if(stagger!=null) stagger.StaggerApplied+=Interrupted;
    }
    private void ShieldChanged(EnemyActor owner,int current,int maximum)
    {
        if(IsTreant && state.shieldArmed && current==0 && !actor.IsDefeated)
        { state.shieldArmed=false;stagger?.ApplyStagger(1,1); }
    }
    public void ResolveAcceptedMove()
    {
        if(released || pending || actor==null || actor.IsDefeated || !CombatMoveClock.CanOffer(actor) ||
            Time.timeScale<=0 || board.IsBusy || actor.HasAnimationActionInProgress || stagger?.IsStaggered==true) return;
        if(IsPreparing)
        {
            if(board.CompletedValidPlayerMoves<warning.DueMove || !actor.TryBeginSpecialAbilityAnimationAction()) return;
            pending=true;actor.SpecialIdleState=null;actor.PrepareSpecialMotion("Release");
            if(!board.TryQueueResolveCellResponse(warning,Impact,ok=>Finish())) Finish();
            return;
        }
        if(!actor.IsSpecialReady) return;
        if(!IsTreant && board.VineCount==0) return;
        if(!actor.TryBeginSpecialAbilityAnimationAction()) return;
        pending=true;
        if(IsTreant && state.cycle==0)
        { StartCoroutine(Armor());return; }
        actor.PrepareSpecialMotion("ChannelStart");
        if(!board.TryQueueCellResponse(actor,3,2,!IsTreant,marked=>
        {
            pending=false;warning=marked;
            if(marked!=null && !released)
            {
                actor.NotifySpecialAbilityUsed();actor.ResetSpecialCounter();
                actor.SpecialIdleState="ChannelHold";attack?.SetActionPaused(this,true);
            }
            actor.EndSpecialAbilityAnimationAction();
        })) {pending=false;actor.EndSpecialAbilityAnimationAction();}
    }
    private IEnumerator Armor()
    {
        int motion=actor.StartSpecialMotion("Ability");
        if(motion>0) yield return actor.WaitForSpecialMotionBeat(motion);
        if(!released && !actor.IsDefeated && stagger?.IsStaggered!=true && (motion==0 || actor.IsSpecialMotionCurrent(motion)))
        {
            actor.GrantShield(actor.Definition.BarkArmorShield);state.shieldArmed=actor.CurrentShield>0;
            state.cycle=1;actor.NotifySpecialAbilityUsed();actor.ResetSpecialCounter();
        }
        if(motion>0) yield return actor.WaitForSpecialMotionComplete(motion);
        pending=false;actor.EndSpecialAbilityAnimationAction();
    }
    private void Impact(int shots,bool answered)
    {
        if(released || actor.IsDefeated) return;
        var player=attack?.PlayerTarget;
        if(IsTreant)
        {
            int damage=answered?actor.Definition.FallingBoughWeakenedDamage:actor.Definition.FallingBoughDamage;
            player?.TryTakeDamage(CombatAmounts.Round(damage*actor.RuntimeStats.DamageMultiplier),actor);
        }
        else for(int i=0;i<shots;i++)
        {
            if(player==null || player.IsDefeated) break;
            player.TryTakeDamage(CombatAmounts.Round(actor.Definition.ThornVolleyDamage*actor.RuntimeStats.DamageMultiplier),actor);
        }
    }
    private void Finish()
    {
        board.CancelCellResponse(warning);warning=null;pending=false;
        if(IsTreant) state.cycle=0;
        actor.SpecialIdleState=null;actor.ResetSpecialCounter();
        attack?.SetActionPaused(this,false);actor.EndSpecialAbilityAnimationAction();
    }
    private void Interrupted(EnemyStagger source,float duration,float remaining)
    { if(IsPreparing) Finish(); }
    private void Died(EnemyActor owner) => Cleanup();
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf)
    { saved.forestRoster=JsonUtility.FromJson<ForestRosterSnapshot>(JsonUtility.ToJson(state)); }
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt)
    {
        state=saved.forestRoster==null?new ForestRosterSnapshot():
            JsonUtility.FromJson<ForestRosterSnapshot>(JsonUtility.ToJson(saved.forestRoster));
        warning=board.RestoredCellResponse(actor);
        actor.SpecialIdleState=IsPreparing?"ChannelHold":null;attack?.SetActionPaused(this,IsPreparing);
    }
    private void Cleanup()
    {
        if(released) return;released=true;StopAllCoroutines();board?.CancelCellResponse(warning);
        attack?.SetActionPaused(this,false);
        if(actor!=null) {actor.Defeated-=Died;actor.ShieldChanged-=ShieldChanged;actor.SpecialIdleState=null;actor.EndSpecialAbilityAnimationAction();}
        if(stagger!=null) stagger.StaggerApplied-=Interrupted;
    }
    private void OnDisable() => Cleanup();
}
