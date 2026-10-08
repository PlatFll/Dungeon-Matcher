using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class AquaticEnemySnapshot
{
    public int version;
    public int stage, cycle, dueMove, answers, retaliatedMove = -1;
    public int recoveryUntil; // Legacy save field; retired post-cast delay.
    public int summonSlot = -1;
    public long targetId, summonId;
    public string action;
    public bool wetMode;
    public Vector2Int cofferSite;
    public List<int> bubbleTargets = new List<int>();
    public List<Vector2Int> marks = new List<Vector2Int>();
    public List<int> answeredIdentities = new List<int>();
    public List<long> rallyTargets = new List<long>();
    public float rallySeconds;
    public int rallyExpiresMove;
}

/// <summary>Data-selected court actions. Board placement and clear rewards remain board-owned.</summary>
[DisallowMultipleComponent]
public sealed class AquaticEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility, IEnemyContinuationOwner
{
    private EnemyActor actor;
    private EnemyAutoAttack attack;
    private EnemyStagger stagger;
    private BoardController board;
    private IReadOnlyList<EnemyActor> roster;
    private IEnemySummonService summons;
    private CombatController combat;
    private AquaticEnemySnapshot state = new AquaticEnemySnapshot{version=2};
    private bool pending, disposed;
    private readonly HashSet<EnemyAutoAttack> buffed = new HashSet<EnemyAutoAttack>();
    private static readonly HashSet<AquaticEnemyAbility> rallies = new HashSet<AquaticEnemyAbility>();
    private static readonly object RallyKey = new object();
    public bool IsPreparing => state.stage == 1;
    public bool BlocksBasic => pending || (IsPreparing && Kind != EnemySpecialAbilityKind.SpineGuard);
    public int ResponseMoves => IsPreparing ? Mathf.Max(0, state.dueMove - board.CompletedValidPlayerMoves) : 0;
    public string CastName => state.action;
    public IReadOnlyList<Vector2Int> ResponseCells => state.marks;
    public IReadOnlyList<int> MarkedBubbles => state.bubbleTargets;
    public Vector2Int? CofferTarget => IsPreparing && IsTheft ? state.cofferSite : (Vector2Int?)null;
    public int Answers => state.answers;
    public bool Rallies(EnemyActor target) => target != null && state.rallySeconds > 0 && buffed.Contains(target.GetComponent<EnemyAutoAttack>());
    public int RallyRemaining => state.rallySeconds <= 0 ? 0 : CombatMoveClock.Unified
        ? Mathf.Max(0, state.rallyExpiresMove - CombatMoveClock.EffectAction) : Mathf.CeilToInt(state.rallySeconds);
    public EnemyActor Target => Find(state.targetId);
    private EnemySpecialAbilityKind Kind => actor.Definition.SpecialAbilityKind;
    private int Move => board.CompletedValidPlayerMoves;
    private bool Queen => Kind == EnemySpecialAbilityKind.AbyssalRegent;
    private bool IsTheft => state.action=="THEFT" || state.action=="AIR LEVY" || state.action=="SEIZURE";
    private bool InvalidPressure => state.action=="PRESSURE" && (state.answers>0 || !board.IsFlooded ||
        state.bubbleTargets.Count!=2 || state.bubbleTargets.Any(id=>!board.Aquatic.bubbles.Contains(id) || board.FindAquaticGem(id)==null));
    private bool InvalidTribute => state.action=="TRIBUTE" && (!board.IsFlooded ||
        !state.bubbleTargets.Any(id=>board.Aquatic.bubbles.Contains(id) && board.FindAquaticGem(id)!=null));
    public void ConfigureSummonService(IEnemySummonService service) => summons = service;

    public void InitializeSpecialAbility(EnemyActor owner, BoardController initializedBoard, IReadOnlyList<EnemyActor> enemies)
    {
        actor = owner; board = initializedBoard; roster = enemies;
        attack = actor.GetComponent<EnemyAutoAttack>(); stagger = actor.GetComponent<EnemyStagger>();
        actor.Defeated += Died;
        if (stagger != null) stagger.StaggerApplied += Interrupted;
        board.AquaticAnswer += Answer;
        combat = FindFirstObjectByType<CombatController>();
        if (combat != null) combat.EnemyDamagedByGemClear += Damaged;
        rallies.Add(this);
        (GetComponent<EnemyMoveIntentView>() ?? gameObject.AddComponent<EnemyMoveIntentView>()).Initialize(actor);
    }

    private EnemyActor Find(long id) => id <= 0 ? null : roster.FirstOrDefault(e =>
        e != null && e.IsInitialized && !e.IsDefeated && e.PersistentId == id);
    private List<EnemyActor> Allies() => roster.Where(e => e != null && e != actor && e.IsInitialized && !e.IsDefeated)
        .OrderBy(e => e.PersistentId).ToList();
    private EnemyActor ShieldTarget() => Allies().OrderBy(e => e.CurrentShield).ThenBy(e => e.PersistentId).FirstOrDefault() ?? actor;

    public void ResolveAcceptedMove()
    {
        if (disposed || pending || actor.IsDefeated || !CombatMoveClock.CanOffer(actor) ||
            Time.timeScale <= 0 || board.IsBusy || actor.HasAnimationActionInProgress || stagger?.IsStaggered == true) return;
        if (IsPreparing)
        {
            if (InvalidPressure || InvalidTribute) { FinishCast(); return; }
            if (state.targetId > 0 && Target == null) { FinishCast(); return; }
            if (Move < state.dueMove) return;
            if (!actor.TryBeginSpecialAbilityAnimationAction()) return;
            pending = true; StartCoroutine(Release()); return;
        }
        if (!actor.IsSpecialReady) return;
        if (!Plan()) return;
        if (!actor.TryBeginSpecialAbilityAnimationAction()) { ClearPlan(); return; }
        pending = true; StartCoroutine(Begin());
    }

    private bool Plan()
    {
        ClearPlan(); state.wetMode = board.IsFlooded;
        switch (Kind)
        {
            case EnemySpecialAbilityKind.PearlTheft:
                if (!board.TryPlanAirTheft(2, false, out var thiefTargets, out var thiefSite)) return false;
                state.bubbleTargets = thiefTargets; state.cofferSite = thiefSite; state.action = "THEFT"; break;
            case EnemySpecialAbilityKind.RallyingConch:
                if (Allies().Count == 0) return false;
                state.action = "RALLY"; break;
            case EnemySpecialAbilityKind.ThornySnare: state.action = "SNARE"; break;
            case EnemySpecialAbilityKind.SpineGuard: state.action = "SPINES"; break;
            case EnemySpecialAbilityKind.MoraySiphon: state.action = "SIPHON"; break;
            case EnemySpecialAbilityKind.BreakwaterCommand:
                if (state.cycle == 0) { state.action = "SHELLGUARD"; state.targetId = ShieldTarget().PersistentId; }
                else
                {
                    var ally = Allies().FirstOrDefault(e => e.GetComponent<EnemyStagger>()?.IsStaggered != true &&
                        e.GetComponent<EnemyAutoAttack>()?.IsPausedByAction != true);
                    if (ally == null) return false;
                    state.action = "BOARDING"; state.targetId = ally.PersistentId;
                }
                break;
            case EnemySpecialAbilityKind.LanternPressure:
            case EnemySpecialAbilityKind.AbyssalRegent:
                if (state.cycle == 0)
                {
                    if (board.IsFlooded)
                    {
                        if (!board.TryPlanAirTheft(3, Queen, out var targets, out var site)) return false;
                        state.bubbleTargets = targets; state.cofferSite = site; state.action = Queen ? "SEIZURE" : "AIR LEVY";
                    }
                    else { state.action = Queen ? "ROYAL WARD" : "DEEPGUARD"; state.targetId = actor.PersistentId; }
                }
                else if (state.cycle == 1)
                {
                    state.action = Queen ? "TRIBUTE" : "PRESSURE";
                    state.bubbleTargets=board.AvailableAirTargets().Take(Queen?Mathf.Clamp(actor.Definition.tributeTargetCount,1,3):2).ToList();
                    if(state.bubbleTargets.Count<(Queen?1:2))return false;
                }
                else
                {
                    if (summons is IEnemyFixedSlotSummonService slots && slots.FirstFreeSummonSlot >= 0 && Find(state.summonId) == null)
                    { state.action = "MUSTER"; state.summonSlot = slots.FirstFreeSummonSlot; }
                    else { state.action = "ROYAL GUARD"; state.targetId = ShieldTarget().PersistentId; }
                }
                break;
            default: return false;
        }
        return true;
    }

    private IEnumerator Begin()
    {
        bool instant = state.action == "RALLY" || state.action == "SNARE" || state.action == "SHELLGUARD" ||
            state.action == "ROYAL WARD" || state.action == "DEEPGUARD" || IsTheft;
        int motion = actor.StartSpecialMotion(instant || state.action == "SPINES" ? "Ability" : "ChannelStart");
        if (motion > 0) yield return actor.WaitForSpecialMotionBeat(motion);
        if (!ValidMotion(motion)) { pending = false; actor.ResetSpecialCounter();actor.EndSpecialAbilityAnimationAction();ClearPlan();SetHeld(false);yield break; }
        bool used = true;
        if (state.action == "RALLY") ApplyRally();
        else if (state.action == "SNARE") used = board.TryApplyAquaticSnares(actor);
        else if(IsTheft)
        {
            bool done=false;used=false;
            if(board.TryQueueAirTheft(actor,state.bubbleTargets,state.cofferSite,
                Kind==EnemySpecialAbilityKind.PearlTheft?1:2,Queen,ok=>{used=ok;done=true;}))
                while(!done && !disposed)yield return null;
        }
        else if (instant) Target?.GrantShield(Queen ? 20 : actor.Definition.aquaticShield);
        if (used)
        {
            actor.AnnounceCommittedCast(EnemyAbilityNames.Aquatic(state.action));actor.NotifySpecialAbilityUsed(); actor.ResetSpecialCounter();
            if (instant) { AdvanceRotation(); state.stage = 0; }
            else
            {
                state.stage = 1; state.dueMove = Move + (state.action == "TRIBUTE" ? Mathf.Max(1,actor.Definition.tributeChannelMoves) : actor.Definition.aquaticChannelMoves);
                SetHeld(BlocksBasic);
            }
        }
        if (motion > 0) yield return actor.WaitForSpecialMotionComplete(motion);
        pending = false; actor.EndSpecialAbilityAnimationAction();
        if(instant) {if(!used)actor.ResetSpecialCounter();ClearPlan();SetHeld(false);}
        else SetHeld(BlocksBasic); // Inflation permits basics once its cast motion ends.
        actor.SpecialIdleState = IsPreparing ? (state.action == "SPINES" ? "InflatedIdle" : "ChannelHold") : null;
        actor.SpecialAutoAttackState = IsPreparing && state.action == "SPINES" ? "InflatedAttack" : null;
    }

    private IEnumerator Release()
    {
        actor.SpecialIdleState = null;
        actor.SpecialAutoAttackState = null;
        int motion = actor.StartSpecialMotion(state.action == "TRIBUTE" ? "DepthsRelease" : "Release"); // Reuse approved royal motion; no old damage behavior.
        if (motion > 0) yield return actor.WaitForSpecialMotionBeat(motion);
        if (ValidMotion(motion))
        {
            switch (state.action)
            {
                case "SIPHON":
                    int before = attack.PlayerTarget.CurrentHealth;
                    Hit(actor.Definition.aquaticAbilityDamage);
                    if (!actor.IsDefeated) actor.RestoreHealth(Mathf.Max(0, before - attack.PlayerTarget.CurrentHealth));
                    break;
                case "PRESSURE": if(!InvalidPressure) Hit(actor.Definition.aquaticAbilityDamage); break;
                case "TRIBUTE":
                    bool tributeDone=false;int pearls=0;
                    if(board.TryQueueConsumeAirBubbles(actor,state.bubbleTargets,n=>{pearls=n;tributeDone=true;}))
                        while(!tributeDone&&!disposed)yield return null;
                    if(!disposed && !actor.IsDefeated)DistributeTribute(pearls);
                    break;
                case "BOARDING":
                    var commanded = Target?.GetComponent<EnemyAutoAttack>();
                    if (commanded != null && commanded.TryReserveCommand(this))
                    {
                        try
                        {
                            if (CombatMoveClock.ClaimSpecial(Target) && commanded.PerformCommandStrike(this))
                                while (commanded != null && commanded.IsAttackSequenceInProgress && !disposed) yield return null;
                        }
                        finally { commanded?.ReleaseCommand(this); }
                    }
                    break;
                case "MUSTER":
                    if (summons is IEnemyFixedSlotSummonService slots && Find(state.summonId) == null &&
                        actor.Definition.aquaticSummon != null &&
                        slots.TrySummonEnemyAt(actor.Definition.aquaticSummon, state.summonSlot, out var summoned)) state.summonId = summoned.PersistentId;
                    break;
                case "ROYAL GUARD": Target?.GrantShield(20); break;
            }
        }
        if (motion > 0) yield return actor.WaitForSpecialMotionComplete(motion);
        pending = false; actor.EndSpecialAbilityAnimationAction();
        if (!disposed) FinishCast();
    }

    private bool ValidMotion(int motion) => !disposed && !actor.IsDefeated && stagger?.IsStaggered != true &&
        (motion == 0 || actor.IsSpecialMotionCurrent(motion));
    private void Hit(int amount) => attack.PlayerTarget?.TryTakeDamage(CombatAmounts.Round(amount * actor.RuntimeStats.DamageMultiplier), actor);
    private void SetHeld(bool held) => attack?.SetActionPaused(this, held);
    private void AdvanceRotation()
    {
        if (Queen) state.cycle = (state.cycle + 1) % 3;
        else if (Kind == EnemySpecialAbilityKind.BreakwaterCommand || Kind == EnemySpecialAbilityKind.LanternPressure) state.cycle = (state.cycle + 1) % 2;
    }
    private void FinishCast()
    {
        if (state.stage == 0) return;
        AdvanceRotation(); state.stage = 0; state.recoveryUntil = 0; state.dueMove = 0;
        actor.ResetSpecialCounter(); actor.SpecialIdleState = null; SetHeld(false);
        actor.SpecialAutoAttackState = null;
        state.marks.Clear(); state.bubbleTargets.Clear(); state.targetId = 0;
    }
    private void ClearPlan()
    {
        state.action = null; state.targetId = 0; state.answers = 0;
        state.summonSlot = -1;
        state.marks.Clear(); state.bubbleTargets.Clear(); state.answeredIdentities.Clear();
    }
    private void DistributeTribute(int pearls)
    {
        // Use actual slots: creation order can put a replacement left slot last.
        var recipients=new List<EnemyActor>{actor};
        recipients.AddRange(roster.Where(e=>e!=null&&e!=actor&&e.IsInitialized&&!e.IsDefeated)
            .OrderBy(e=>combat?.WaveController?.ContinuationSlot(e)??0).ThenBy(e=>e.PersistentId));
        int cap=Mathf.Clamp(actor.Definition.tributeStackCap,1,2);
        while(pearls>0)
        {
            bool granted=false;
            foreach(var recipient in recipients)
            {
                if(pearls==0)break;
                if(recipient.GrantFortified(1,cap)>0){pearls--;granted=true;}
            }
            if(!granted)break;
        }
    }
    private void Answer(int gemId, bool bubble, bool coffer)
    {
        if (!IsPreparing) return;
        if(state.action=="PRESSURE")
        {
            if(bubble && state.bubbleTargets.Contains(gemId)) {state.answers=1;state.bubbleTargets.Clear();}
            return;
        }
        if(state.action=="TRIBUTE" && bubble)state.bubbleTargets.Remove(gemId);
    }
    private void Damaged(EnemyActor target, GemDamageContext context, int hpLost)
    {
        if (disposed || target != actor || actor.IsDefeated || Kind != EnemySpecialAbilityKind.SpineGuard || !IsPreparing ||
            !board.ReportingManualClear || context.ClearSource != BoardClearSource.Match || context.CascadeDepth != 0 ||
            CombatMoveClock.EffectAction <= state.retaliatedMove || CombatMoveClock.Current?.IsBlockingWaveProgression != true) return;
        state.retaliatedMove = CombatMoveClock.EffectAction; Hit(5);
    }
    private void Interrupted(EnemyStagger source, float duration, float remaining)
    {
        if(pending)actor.EndSpecialAbilityAnimationAction();
        if (IsPreparing) FinishCast();
    }

    private void ApplyRally()
    {
        state.rallySeconds = actor.Definition.aquaticRallySeconds;
        state.rallyExpiresMove = CombatMoveClock.EffectAction + Mathf.Max(1, actor.Definition.aquaticRallyMoves);
        foreach (var ally in Allies()) if (ally.TryGetComponent<EnemyAutoAttack>(out var target))
        { buffed.Add(target); RefreshRally(target); }
    }
    private static void RefreshRally(EnemyAutoAttack target)
    {
        if (target == null) return;
        float amount = 1;
        foreach (var source in rallies) if (source != null && !source.disposed && source.state.rallySeconds > 0 && source.buffed.Contains(target))
            amount = Mathf.Max(amount, source.actor.Definition.aquaticRallyDamage);
        if (amount > 1) target.SetNormalAttackModifiers(RallyKey, amount, 1);
        else target.RemoveNormalAttackModifiers(RallyKey);
    }
    private void ClearRally()
    {
        var old = buffed.ToArray(); buffed.Clear(); state.rallySeconds = 0;
        foreach (var target in old) RefreshRally(target);
    }
    private void Update()
    {
        if (disposed || Time.timeScale <= 0) return;
        if (IsPreparing && !pending && CombatMoveClock.Current?.IsBlockingWaveProgression!=true &&
            ((state.targetId > 0 && Target == null) || InvalidPressure || InvalidTribute)) FinishCast();
        if (state.rallySeconds <= 0 || CombatMoveClock.Unified) return;
        state.rallySeconds = Mathf.Max(0, state.rallySeconds - Time.deltaTime);
        if (state.rallySeconds == 0) ClearRally();
    }
    public void ExpireAcceptedMove(int move)
    {
        if (CombatMoveClock.Unified && state.rallySeconds > 0 && move >= state.rallyExpiresMove) ClearRally();
    }
    public void CaptureContinuation(EnemyCombatSnapshot saved, Func<EnemyActor, int> slotOf)
    {
        state.rallyTargets = buffed.Where(t => t != null && !t.EnemyActor.IsDefeated).Select(t => t.EnemyActor.PersistentId).ToList();
        saved.aquaticEnemy = JsonUtility.FromJson<AquaticEnemySnapshot>(JsonUtility.ToJson(state));
    }
    public void RestoreContinuation(EnemyCombatSnapshot saved, Func<int, EnemyActor> enemyAt)
    {
        ClearRally(); state = saved.aquaticEnemy == null ? new AquaticEnemySnapshot{version=2} :
            JsonUtility.FromJson<AquaticEnemySnapshot>(JsonUtility.ToJson(saved.aquaticEnemy));
        if(state.stage==2)
        {
            // Rotation already advanced when the old cast ended. Never repeat it.
            state.stage=0;state.recoveryUntil=0;state.dueMove=0;
            ClearPlan();actor.ResetSpecialCounter();
        }
        if(state.stage==1 && (IsTheft || state.action=="DEPTHS" || (state.version<2 && state.action=="PRESSURE")))
        {state.stage=0;state.dueMove=0;ClearPlan();actor.ResetSpecialCounter();}
        state.version=2;
        if (state.rallySeconds > 0) foreach (long id in state.rallyTargets)
        {
            var target = Find(id)?.GetComponent<EnemyAutoAttack>();
            if (target != null) { buffed.Add(target); RefreshRally(target); }
        }
        actor.SpecialIdleState = IsPreparing ? (state.action == "SPINES" ? "InflatedIdle" : "ChannelHold") : null;
        actor.SpecialAutoAttackState = IsPreparing && state.action == "SPINES" ? "InflatedAttack" : null;
        SetHeld(BlocksBasic);
    }
    private void Died(EnemyActor _) { board.ReleaseAquaticOwner(actor.PersistentId); Cleanup(); }
    private void Cleanup()
    {
        if (disposed) return; disposed = true;
        StopAllCoroutines(); ClearRally(); rallies.Remove(this); SetHeld(false);
        if (board != null) board.AquaticAnswer -= Answer;
        if (combat != null) combat.EnemyDamagedByGemClear -= Damaged;
        if (actor != null) { actor.Defeated -= Died; actor.SpecialIdleState = actor.SpecialAutoAttackState = null; actor.EndSpecialAbilityAnimationAction(); }
        if (stagger != null) stagger.StaggerApplied -= Interrupted;
    }
    private void OnDisable() => Cleanup();
}
