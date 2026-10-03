using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ForestRosterSnapshot
{
    public bool enraged, ragePresented, shieldArmed;
    public int cycle, buffExpiresMove;
    public long summonId;
    public float buffSeconds;
    public List<long> buffTargets = new List<long>();
}

/// <summary>Data-selected rage, independent summon and ally rhythm; no board mutation.</summary>
[DisallowMultipleComponent]
public sealed class ForestCombatAbility : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility, IEnemyContinuationOwner
{
    private EnemyActor actor;
    private EnemyAutoAttack attack;
    private BoardController board;
    private IReadOnlyList<EnemyActor> roster;
    private IEnemySummonService summons;
    private ForestRosterSnapshot state = new ForestRosterSnapshot();
    private bool pending, released;
    private readonly HashSet<EnemyAutoAttack> buffed = new HashSet<EnemyAutoAttack>();
    // All Drummers share one modifier key: simultaneous rhythms refresh strength,
    // rather than multiplying one another. A living caster maintains its lease.
    private static readonly object RhythmKey = new object();
    private static readonly HashSet<ForestCombatAbility> rhythms = new HashSet<ForestCombatAbility>();
    public bool IsEnraged => state.enraged;
    public bool HasLivingSummon => Find(state.summonId) != null;
    public void ConfigureSummonService(IEnemySummonService service) => summons = service;
    private EnemySpecialAbilityKind Kind => actor.Definition.SpecialAbilityKind;

    public void InitializeSpecialAbility(EnemyActor owner, BoardController initializedBoard, IReadOnlyList<EnemyActor> enemies)
    {
        actor=owner;board=initializedBoard;roster=enemies;attack=actor.GetComponent<EnemyAutoAttack>();
        actor.SurvivedHealthDamage+=Damaged;actor.Defeated+=Died;
        if(CombatMoveClock.Current!=null) CombatMoveClock.Current.ActionSettled+=Expire;
        rhythms.Add(this);
    }
    private EnemyActor Find(long id)
    {
        if(id<=0 || roster==null) return null;
        foreach(var enemy in roster)
            if(enemy!=null && !enemy.IsDefeated && enemy.PersistentId==id) return enemy;
        return null;
    }
    private void Damaged(EnemyActor owner,int before,int after)
    {
        if(Kind!=EnemySpecialAbilityKind.Bloodrage || state.enraged || after<=0 || after*2L>=actor.MaxHealth) return;
        state.enraged=true;
        attack?.SetNormalAttackModifiers(this,actor.Definition.EnrageDamageMultiplier,actor.Definition.EnrageSpeedMultiplier);
    }
    private void Update()
    {
        if(released || actor==null || actor.IsDefeated) return;
        if(!CombatMoveClock.MoveEffects && state.buffSeconds>0 && Time.timeScale>0)
        {
            state.buffSeconds=Mathf.Max(0,state.buffSeconds-Time.deltaTime);
            if(state.buffSeconds==0) ClearRhythm();
        }
        if(!CombatMoveClock.Active) ResolveAcceptedMove();
    }
    private bool Available => !released && !pending && actor!=null && !actor.IsDefeated &&
        Time.timeScale>0 && !board.IsBusy && !actor.HasAnimationActionInProgress &&
        actor.GetComponent<EnemyStagger>()?.IsStaggered!=true;
    public void ResolveAcceptedMove()
    {
        if(!Available || !CombatMoveClock.CanOffer(actor)) return;
        if(Kind==EnemySpecialAbilityKind.Bloodrage)
        {
            if(state.enraged && !state.ragePresented && actor.TryBeginSpecialAbilityAnimationAction())
            { state.ragePresented=true;pending=true;StartCoroutine(Cast("Ability",()=>{})); }
            return;
        }
        if(!actor.IsSpecialReady) return;
        if(Kind==EnemySpecialAbilityKind.CallSnapvine &&
            (HasLivingSummon || summons==null || !summons.HasFreeEnemySlot || actor.Definition.ForestSummon==null)) return;
        if(Kind==EnemySpecialAbilityKind.WarRhythm && !HasAllies()) return;
        if(!actor.TryBeginSpecialAbilityAnimationAction()) return;
        pending=true;
        StartCoroutine(Cast("Ability",()=>
        {
            bool used=false;
            if(Kind==EnemySpecialAbilityKind.CallSnapvine)
            {
                if(!HasLivingSummon && summons.TrySummonEnemy(actor.Definition.ForestSummon,out var summoned))
                { state.summonId=summoned.PersistentId;used=true; }
            }
            else if(HasAllies()) { ApplyRhythm();used=true; }
            if(used) { actor.NotifySpecialAbilityUsed();actor.ResetSpecialCounter(); }
        }));
    }
    private IEnumerator Cast(string clip,Action impact)
    {
        int motion=actor.StartSpecialMotion(clip);
        if(motion>0) yield return actor.WaitForSpecialMotionBeat(motion);
        if(!released && !actor.IsDefeated && actor.GetComponent<EnemyStagger>()?.IsStaggered!=true &&
            (motion==0 || actor.IsSpecialMotionCurrent(motion))) impact();
        if(motion>0) yield return actor.WaitForSpecialMotionComplete(motion);
        pending=false;actor.EndSpecialAbilityAnimationAction();
    }
    private bool HasAllies()
    {
        foreach(var enemy in roster) if(enemy!=null && enemy!=actor && !enemy.IsDefeated) return true;
        return false;
    }
    private void ApplyRhythm()
    {
        state.buffSeconds=actor.Definition.WarRhythmSeconds;
        state.buffExpiresMove=CombatMoveClock.EffectAction+actor.Definition.WarRhythmMoves;
        foreach(var enemy in roster)
            if(enemy!=null && enemy!=actor && !enemy.IsDefeated && enemy.TryGetComponent<EnemyAutoAttack>(out var target))
            { buffed.Add(target);RefreshRhythm(target); }
    }
    private static void RefreshRhythm(EnemyAutoAttack target)
    {
        if(target==null) return;
        float speed=1;
        foreach(var source in rhythms)
            if(source!=null && !source.released && source.buffed.Contains(target))
                speed=Mathf.Max(speed,source.actor.Definition.WarRhythmSpeed);
        if(speed>1) target.SetNormalAttackModifiers(RhythmKey,1,speed);
        else target.RemoveNormalAttackModifiers(RhythmKey);
    }
    private void ClearRhythm()
    {
        var previous=new List<EnemyAutoAttack>(buffed);buffed.Clear();state.buffSeconds=0;
        foreach(var target in previous) RefreshRhythm(target);
    }
    private void Expire(int move)
    { if(CombatMoveClock.MoveEffects && state.buffSeconds>0 && move>=state.buffExpiresMove) ClearRhythm(); }
    public void CaptureContinuation(EnemyCombatSnapshot saved,Func<EnemyActor,int> slotOf)
    {
        state.buffTargets.Clear();
        foreach(var target in buffed) if(target!=null && !target.EnemyActor.IsDefeated) state.buffTargets.Add(target.EnemyActor.PersistentId);
        saved.forestRoster=JsonUtility.FromJson<ForestRosterSnapshot>(JsonUtility.ToJson(state));
    }
    public void RestoreContinuation(EnemyCombatSnapshot saved,Func<int,EnemyActor> enemyAt)
    {
        ClearRhythm();state=saved.forestRoster==null?new ForestRosterSnapshot():
            JsonUtility.FromJson<ForestRosterSnapshot>(JsonUtility.ToJson(saved.forestRoster));
        if(state.enraged) attack?.SetNormalAttackModifiers(this,actor.Definition.EnrageDamageMultiplier,actor.Definition.EnrageSpeedMultiplier);
        if(state.buffSeconds>0) foreach(long id in state.buffTargets)
        {
            var target=Find(id)?.GetComponent<EnemyAutoAttack>();
            if(target!=null) {buffed.Add(target);RefreshRhythm(target);}
        }
    }
    private void Died(EnemyActor owner) => Cleanup();
    private void Cleanup()
    {
        if(released) return;released=true;ClearRhythm();rhythms.Remove(this);
        attack?.RemoveNormalAttackModifiers(this);StopAllCoroutines();pending=false;
        if(actor!=null) {actor.SurvivedHealthDamage-=Damaged;actor.Defeated-=Died;actor.EndSpecialAbilityAnimationAction();}
        if(CombatMoveClock.Current!=null) CombatMoveClock.Current.ActionSettled-=Expire;
        // Summons remain independent members of the wave after their producer dies.
    }
    private void OnDisable() => Cleanup();
}
