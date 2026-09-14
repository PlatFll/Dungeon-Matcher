using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class KingEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime, IEnemyContinuationOwner
{
    private EnemyActor actor;
    private EnemyAutoAttack ownAttack;
    private BoardController board;
    private IReadOnlyList<EnemyActor> roster;
    private IEnemySummonService summons;
    private EnemySpecialActionAvailability availability;
    private BoardController.GemSetThreat judgment;
    private BoardController.LaneThreat bombardment;
    private readonly List<EnemyAutoAttack> participants = new List<EnemyAutoAttack>();
    private readonly HashSet<EnemyActor> locks = new HashSet<EnemyActor>();
    private readonly Queue<int> thresholds = new Queue<int>();
    private bool crossedHalf, crossedQuarter, released = true, pending;
    private int cycle, retryAfterMove = -1;
    public bool IsEnraged => crossedHalf;
    public void CaptureContinuation(EnemyCombatSnapshot saved, Func<EnemyActor,int> slotOf)
    {
        saved.crossedHalf=crossedHalf; saved.crossedQuarter=crossedQuarter; saved.cycle=cycle;
        saved.retryAfterMove=retryAfterMove; saved.thresholds.AddRange(thresholds);
    }
    public void RestoreContinuation(EnemyCombatSnapshot saved, Func<int,EnemyActor> enemyAt)
    {
        crossedHalf=saved.crossedHalf; crossedQuarter=saved.crossedQuarter; cycle=saved.cycle;
        retryAfterMove=saved.retryAfterMove; thresholds.Clear(); foreach(int threshold in saved.thresholds) thresholds.Enqueue(threshold);
        judgment=board.RestoredSet(actor); bombardment=board.RestoredLanes(actor);
        if(crossedHalf)
        {
            ownAttack?.SetNormalAttackModifiers(this,actor.Definition.EnrageDamageMultiplier,actor.Definition.EnrageSpeedMultiplier);
            Enraged?.Invoke(this);
        }
    }
    public event Action<KingEnemyAbility> Enraged;
    public event Action<KingEnemyAbility> CommandIssued;
    public event Action<KingEnemyAbility> HeavyStrike;
    public void ConfigureSummonService(IEnemySummonService service) => summons = service;
    public void InitializeSpecialAbility(EnemyActor enemy, BoardController initializedBoard, IReadOnlyList<EnemyActor> enemies)
    {
        Cleanup(); actor = enemy; board = initializedBoard; roster = enemies;
        ownAttack = actor.GetComponent<EnemyAutoAttack>();
        released = false; pending = crossedHalf = crossedQuarter = false; cycle = 0; retryAfterMove = -1;
        actor.SurvivedHealthDamage += OnHealthDamage; actor.Defeated += Defeated;
        availability = new EnemySpecialActionAvailability(this, actor, board, TryCast);
        var phase = gameObject.GetComponent<EnemyRoyalPhaseView>();
        if (phase == null) phase = gameObject.AddComponent<EnemyRoyalPhaseView>();
        phase.Initialize(this);
    }
    private void OnHealthDamage(EnemyActor enemy, int before, int after)
    {
        if (released || enemy.IsDefeated || after <= 0) return;
        if (!crossedHalf && before * 2L >= enemy.MaxHealth && after * 2L < enemy.MaxHealth)
        {
            crossedHalf = true; thresholds.Enqueue(50);
            ownAttack?.SetNormalAttackModifiers(this, actor.Definition.EnrageDamageMultiplier, actor.Definition.EnrageSpeedMultiplier);
            actor.SetSpecialTurnRequirement(actor.Definition.EnragedSpecialMoves);
            Enraged?.Invoke(this);
        }
        if (!crossedQuarter && before * 4L >= enemy.MaxHealth && after * 4L < enemy.MaxHealth)
        { crossedQuarter = true; thresholds.Enqueue(25); }
    }
    private bool CanAct() => Time.timeScale>0 && !released && !pending && actor != null && !actor.IsDefeated &&
        board != null && !board.IsBusy && !actor.HasAnimationActionInProgress &&
        (actor.GetComponent<EnemyStagger>() == null || !actor.GetComponent<EnemyStagger>().IsStaggered);
    private void Update()
    {
        if (!CanAct()) return;
        if (thresholds.Count > 0)
        {
            thresholds.Dequeue(); Reinforce(); return;
        }
        if (judgment != null && !judgment.Ended && board.CompletedValidPlayerMoves >= judgment.DueMove)
        {
            if (!BeginAction()) return;
            if (!board.TryQueueResolveGemSet(judgment, () => Strike(actor.Definition.JudgmentBaseDamage),
                success => EndAction(), () => released)) EndAction();
            return;
        }
        if (bombardment != null && !bombardment.Ended && board.CompletedValidPlayerMoves >= bombardment.DueMove)
        {
            if (!BeginAction()) return;
            if (!board.TryQueueResolveLanes(bombardment, () => Strike(actor.Definition.BombardmentBaseDamage),
                success => EndAction(), () => released)) EndAction();
            return;
        }
        if (actor.IsSpecialReady && board.CompletedValidPlayerMoves > retryAfterMove) availability.RequestExecution();
    }
    private void Reinforce()
    {
        if (summons == null) return;
        bool specialAdded = false;
        var candidates = new List<EnemyDefinition>();
        foreach (var data in actor.Definition.RoyalReinforcements)
        {
            if (data == null || data.EnemyPrefab == null ||
                (data.Category != EnemyCategory.Normal && data.Category != EnemyCategory.Special)) continue;
            bool duplicateSpecial = false;
            foreach (var ally in roster)
                if (ally != null && !ally.IsDefeated && ally.Definition == data && data.Category == EnemyCategory.Special) duplicateSpecial = true;
            if (!duplicateSpecial) candidates.Add(data);
        }
        while (!released && !actor.IsDefeated && summons.HasFreeEnemySlot && candidates.Count > 0)
        {
            int index = GameplayRandom.Range(0, candidates.Count);
            var data = candidates[index];
            if ((specialAdded && data.Category == EnemyCategory.Special) || !summons.TrySummonEnemy(data, out _))
            { candidates.RemoveAt(index); continue; }
            if (data.Category == EnemyCategory.Special) specialAdded = true;
        }
    }
    private bool TryCast()
    {
        if (!CanAct() || !actor.IsSpecialReady) return false;
        if (cycle == 1)
        {
            if (TryAssault()) return true;
            retryAfterMove = board.CompletedValidPlayerMoves; return false;
        }
        if ((cycle == 0 && judgment != null && !judgment.Ended) ||
            (cycle == 2 && bombardment != null && !bombardment.Ended)) return false;
        if (!BeginAction()) return false;
        bool accepted = cycle == 0
            ? board.TryQueueMarkGemSet(actor, actor.Definition.RoyalMarkCount, actor.Definition.RoyalMarkMoves,
                false, result => { judgment = result; FinishCast(result != null); }, () => released)
            : board.TryQueueMarkLanes(actor, actor.Definition.BombardmentWarningMoves,
                result => { bombardment = result; FinishCast(result != null); }, () => released);
        if (!accepted) FinishCast(false);
        return accepted;
    }
    private bool TryAssault()
    {
        if (ownAttack == null || !ownAttack.TryReserveCommand(this, makeReady: true)) return false;
        participants.Add(ownAttack);
        // The roster owns order (configured slots, then summons); no visual coordinates.
        foreach (var ally in roster)
        {
            if (ally == null || ally == actor || ally.IsDefeated || ally.Definition == null || !ally.Definition.RoyalAssaultParticipant) continue;
            var attack = ally.GetComponent<EnemyAutoAttack>();
            if (attack == null || !attack.TryReserveCommand(this, makeReady: true)) continue;
            if (ally.TryBeginSpecialAbilityAnimationAction()) { locks.Add(ally); participants.Add(attack); }
            else attack.ReleaseCommand(this);
        }
        if (!BeginAction()) { ReleaseParticipants(); return false; }
        locks.Add(actor); actor.NotifySpecialAbilityUsed();
        CommandIssued?.Invoke(this);
        StartCoroutine(Assault()); return true;
    }
    private IEnumerator Assault()
    {
        yield return new WaitForSeconds(actor.Definition.RoyalCommandWindup);
        foreach (var attack in participants)
        {
            if (released || actor == null || actor.IsDefeated) yield break;
            if (attack == null || attack.EnemyActor == null || attack.EnemyActor.IsDefeated) continue;
            // A player resolution or new stagger during the windup must finish first.
            // The waiting participant can die and finish its death presentation
            // during that resolution. Never retain an unchecked actor across a yield.
            while (!released && attack != null && attack.EnemyActor != null &&
                   !attack.EnemyActor.IsDefeated &&
                   (board.IsBusy || attack.IsPausedByStagger)) yield return null;
            if (released || actor == null || actor.IsDefeated) yield break;
            if (attack == null || attack.EnemyActor == null || attack.EnemyActor.IsDefeated) continue;
            attack.EnemyActor.EndSpecialAbilityAnimationAction(); locks.Remove(attack.EnemyActor);
            if (attack.PerformCommandStrike(this, actor.Definition.AssaultDamageMultiplier))
            {
                while (!released && attack != null && attack.IsAttackSequenceInProgress) yield return null;
                if (attack != null) attack.ReleaseCommand(this);
                yield return new WaitForSeconds(actor.Definition.RoyalCommandSpacing);
            }
        }
        ReleaseParticipants(); FinishCast(true, false);
    }
    private void Strike(int baseDamage)
    {
        if (released || actor.IsDefeated || ownAttack == null || ownAttack.PlayerTarget == null || baseDamage <= 0) return;
        ownAttack.PlayerTarget.TryTakeDamage(Mathf.RoundToInt(baseDamage * actor.RuntimeStats.DamageMultiplier), actor);
        HeavyStrike?.Invoke(this);
    }
    private bool BeginAction()
    {
        if (!actor.TryBeginSpecialAbilityAnimationAction()) return false;
        pending = true; return true;
    }
    private void EndAction() { pending = false; if (actor != null) actor.EndSpecialAbilityAnimationAction(); }
    private void FinishCast(bool success, bool announce = true)
    {
        EndAction(); if (released) return;
        if (success) { if (announce) actor.NotifySpecialAbilityUsed(); actor.ResetSpecialCounter(); cycle = (cycle + 1) % 3; }
        else retryAfterMove = board.CompletedValidPlayerMoves;
    }
    private void ReleaseParticipants()
    {
        foreach (var locked in locks) if (locked != null) locked.EndSpecialAbilityAnimationAction();
        locks.Clear();
        foreach (var attack in participants) if (attack != null) attack.ReleaseCommand(this);
        participants.Clear();
    }
    private void Defeated(EnemyActor enemy) => Cleanup();
    private void Cleanup()
    {
        released = true; StopAllCoroutines(); availability?.Dispose(); availability = null;
        ReleaseParticipants(); thresholds.Clear();
        if (board != null) { board.CancelGemSetThreat(judgment); board.CancelLaneThreat(bombardment); }
        judgment = null; bombardment = null;
        ownAttack?.RemoveNormalAttackModifiers(this);
        if (actor != null) { actor.SurvivedHealthDamage -= OnHealthDamage; actor.Defeated -= Defeated; actor.EndSpecialAbilityAnimationAction(); }
    }
    private void OnDisable() => Cleanup();
    private void OnDestroy() => Cleanup();
}
