using System;
using System.Collections.Generic;
using UnityEngine;

// Haldra's fixed-identity response uses board-owned ice and the existing move
// coordinator. Future animation may observe this state but cannot release it.
[DisallowMultipleComponent]
public sealed class GlacierCleaveRuntime : MonoBehaviour,IEnemySpecialAbilityRuntime,IAcceptedMoveEnemyAbility,IEnemyContinuationOwner
{
    private EnemyActor actor;
    private BoardController board;
    private EnemyAutoAttack attack;
    private EnemyStagger stagger;
    private bool pending;
    private GlacierCleaveSnapshot state=new GlacierCleaveSnapshot();
    public event Action Changed;
    public bool BlocksBasic => pending || state.channeling;
    public bool IsChanneling => state.channeling;
    public int ResponseMoves => state.channeling && board!=null?Mathf.Max(0,state.deadlineMove-board.CompletedValidPlayerMoves):0;
    public Gem Target => board!=null?board.FindGemIdentity(state.targetIdentity):null;
    public string Outcome => state.outcome;
    public void InitializeSpecialAbility(EnemyActor owner,BoardController initializedBoard,IReadOnlyList<EnemyActor> enemies)
    {
        Cleanup();pending=false;state=new GlacierCleaveSnapshot();actor=owner;board=initializedBoard;
        attack=actor.GetComponent<EnemyAutoAttack>();stagger=actor.GetComponent<EnemyStagger>();
        board.FrostChanged+=CheckTarget;board.BoardStateRestored+=CheckTarget;
        actor.Defeated+=Died;if(stagger!=null)stagger.StaggerApplied+=Interrupted;
    }
    public void ResolveAcceptedMove()
    {
        if(actor==null || actor.IsDefeated || pending || stagger?.IsStaggered==true)return;
        if(state.channeling)
        {
            if(board.IceStage(Target)==FrostStage.None){Finish("Thawed",true);return;}
            if(board.CompletedValidPlayerMoves<state.deadlineMove || !actor.TryBeginSpecialAbilityAnimationAction())return;
            pending=true;actor.SpecialIdleState=null;actor.PrepareSpecialMotion("Release");Publish();
            if(!board.TryQueueConsumeEncasedGem(actor,state.targetIdentity,Impact,
                ok=>{pending=false;actor.EndSpecialAbilityAnimationAction();if(!ok)Finish("Target lost",false);Publish();},Cancelled))
            {pending=false;actor.EndSpecialAbilityAnimationAction();Finish("Target lost",false);}
            return;
        }
        if(!CombatMoveClock.CanOffer(actor) || !actor.IsSpecialReady || actor.Definition.frostwakeRules==null ||
            !actor.TryBeginSpecialAbilityAnimationAction())return;
        pending=true;actor.PrepareSpecialMotion("ChannelStart");Publish();
        if(!board.TryQueueEncasedGem(actor,FrostStage.Bifrost,target=>
        {
            pending=false;
            if(target!=null && !Cancelled())
            {
                state.channeling=true;state.targetIdentity=target.BoardIdentity;
                state.deadlineMove=board.CompletedValidPlayerMoves+3;state.sequence++;state.outcome=null;
                actor.SpecialIdleState="ChannelHold";
                actor.AnnounceCommittedCast();actor.NotifySpecialAbilityUsed();
            }
            if(actor!=null){actor.ResetSpecialCounter();actor.EndSpecialAbilityAnimationAction();}
            Publish();
        },Cancelled))
        {pending=false;actor.EndSpecialAbilityAnimationAction();Publish();}
    }
    private bool Cancelled() => actor==null || actor.IsDefeated || !isActiveAndEnabled || stagger?.IsStaggered==true;
    private void Impact(FrostStage stage)
    {
        if(!state.channeling || state.lastOutcomeSequence==state.sequence || Cancelled())return;
        var rules=actor.Definition.frostwakeRules;
        // End warning before damage/status callbacks; retries cannot strike twice.
        Finish("Struck",false);
        var player=attack?.PlayerTarget ?? RunSession.Current?.Player;
        if(player==null || player.IsDefeated || rules==null)return;
        player.TryTakeDamage(CombatAmounts.Round(rules.CleaveDamage(stage)*actor.RuntimeStats.DamageMultiplier),actor);
        if(stage==FrostStage.Bifrost && !player.IsDefeated)player.Statuses.Apply(rules.frostbite,actor);
    }
    private void CheckTarget()
    {if(state.channeling && !pending && board.IceStage(Target)==FrostStage.None)Finish("Thawed",true);}
    private void Finish(string outcome,bool countered)
    {
        if(!state.channeling)return;
        state.channeling=false;state.targetIdentity=0;state.lastOutcomeSequence=state.sequence;state.outcome=outcome;
        if(actor!=null){actor.SpecialIdleState=null;actor.ResetSpecialCounter();}Publish();
        if(outcome!="Struck")actor?.GetComponent<ForestEnemyMotion>()?.PlayInterruptRecovery();
        if(countered && actor!=null && !actor.IsDefeated)stagger?.ApplyStagger(1,1);
    }
    private void Publish(){if(attack!=null)attack.SetActionPaused(this,BlocksBasic);Changed?.Invoke();}
    private void Interrupted(EnemyStagger source,float duration,float remaining)=>Finish("Interrupted",false);
    private void Died(EnemyActor owner){pending=false;Finish("Caster defeated",false);Publish();}
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf)
    {saved.glacierCleave=JsonUtility.FromJson<GlacierCleaveSnapshot>(JsonUtility.ToJson(state));}
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt)
    {
        pending=false;state=saved.glacierCleave==null?new GlacierCleaveSnapshot():
            JsonUtility.FromJson<GlacierCleaveSnapshot>(JsonUtility.ToJson(saved.glacierCleave));
        // Restore does not pay damage or grant an interrupt reward.
        if(state.channeling && board.IceStage(Target)==FrostStage.None)Finish("Target lost",false);
        if(actor!=null)actor.SpecialIdleState=state.channeling?"ChannelHold":null;
        Publish();
    }
    private void Cleanup()
    {
        if(board!=null){board.FrostChanged-=CheckTarget;board.BoardStateRestored-=CheckTarget;}
        if(actor!=null)actor.Defeated-=Died;if(stagger!=null)stagger.StaggerApplied-=Interrupted;
        if(attack!=null)attack.SetActionPaused(this,false);
    }
    private void OnDisable(){pending=false;Finish("Removed",false);Cleanup();}
}
