using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class MineEnemySnapshot
{
    public int version = 1, stage, dueMove, lane, drillId, cycle;
    public long stoneId;
    public MineStoneStage stoneStage;
    public bool horizontal;
    public string action;
}

/// <summary>Data-selected mine choreography; all structural changes stay in BoardController.</summary>
[DisallowMultipleComponent]
public sealed class MineEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime,
    IAcceptedMoveEnemyAbility, IEnemyContinuationOwner
{
    private EnemyActor actor;
    private EnemyAutoAttack attack;
    private EnemyStagger stagger;
    private BoardController board;
    private IReadOnlyList<EnemyActor> roster;
    private MineEnemySnapshot state = new MineEnemySnapshot();
    private bool pending, disposed;
    public bool IsPreparing => state.stage == 1;
    public bool BlocksBasic => pending || IsPreparing;
    public int ResponseMoves => IsPreparing ? Mathf.Max(0, state.dueMove - board.CompletedValidPlayerMoves) : 0;
    public string CastName => state.action;
    public bool Horizontal => state.horizontal;
    public int Lane => state.lane;
    public long StoneId => state.stoneId;
    public int DrillId => state.drillId;
    public Vector2Int? TargetCell => board?.FindMineStone(state.stoneId)?.Cell;
    private EnemySpecialAbilityKind Kind => actor.Definition.SpecialAbilityKind;
    private int Move => board.CompletedValidPlayerMoves;
    private bool TargetGone => state.stoneId > 0 && (board.FindMineStone(state.stoneId) == null ||
        (Kind == EnemySpecialAbilityKind.PowderCharge && board.FindMineStone(state.stoneId)?.State.bombOwnerId != actor.PersistentId) ||
        (Kind == EnemySpecialAbilityKind.AssayVein && board.FindMineStone(state.stoneId)?.State.stage != state.stoneStage));

    public void InitializeSpecialAbility(EnemyActor owner, BoardController targetBoard, IReadOnlyList<EnemyActor> enemies)
    {
        actor = owner; board = targetBoard; roster = enemies;
        attack = actor.GetComponent<EnemyAutoAttack>(); stagger = actor.GetComponent<EnemyStagger>();
        actor.Defeated += Died;
        if (stagger != null) stagger.StaggerApplied += Interrupted;
    }
    public void ResolveAcceptedMove()
    {
        if (disposed || pending || actor.IsDefeated || !CombatMoveClock.CanOffer(actor) ||
            Time.timeScale <= 0 || board.IsBusy || actor.HasAnimationActionInProgress || stagger?.IsStaggered == true) return;
        if (IsPreparing)
        {
            if (TargetGone) { Finish(); return; }
            if (Move < state.dueMove || !actor.TryBeginSpecialAbilityAnimationAction()) return;
            pending = true; StartCoroutine(Release()); return;
        }
        if (!actor.IsSpecialReady || !Plan()) return;
        if (!actor.TryBeginSpecialAbilityAnimationAction()) { Clear(); return; }
        pending = true; StartCoroutine(Begin());
    }
    private bool Plan()
    {
        Clear();
        switch (Kind)
        {
            case EnemySpecialAbilityKind.ShareOre:
                if (!roster.Any(e => e != null && e != actor && !e.IsDefeated && e.Definition.oreWeaponEligible)) return false;
                state.action = "SHARE ORE"; break;
            case EnemySpecialAbilityKind.LayFoundation:
                if (board.MineStoneCount >= RunSession.Current.Zone.Definition.maximumMineStones) return false;
                state.action = "FOUNDATION"; break;
            case EnemySpecialAbilityKind.Faultline: state.action = "FAULTLINE"; break;
            case EnemySpecialAbilityKind.BoreDrill:
                board.LeastStoneMineLane(out state.horizontal, out state.lane); state.action = "BORE"; break;
            case EnemySpecialAbilityKind.AssayVein:
            case EnemySpecialAbilityKind.PowderCharge:
                var targets = board.MineStoneTargets().Where(t => Kind == EnemySpecialAbilityKind.AssayVein ?
                    t.State.stage != MineStoneStage.Obsidian : t.State.bombOwnerId == 0).ToArray();
                if (targets.Length == 0) return false;
                state.stoneId = targets[0].State.id; state.stoneStage = targets[0].State.stage;
                state.action = Kind == EnemySpecialAbilityKind.AssayVein ? "ASSAY" : "CHARGE"; break;
            case EnemySpecialAbilityKind.SwitchTrack:
                var drill = board.Mine?.drills?.OrderBy(d => d.id).ElementAtOrDefault(state.cycle % 2);
                if (drill == null) return false;
                state.drillId = drill.id; state.horizontal = drill.horizontal;
                int size = drill.horizontal ? board.Height : board.Width;
                state.lane = drill.lane + (drill.lane == size - 1 ? -1 : 1); state.action = "SWITCH"; break;
            default: return false;
        }
        return true;
    }
    private IEnumerator Begin()
    {
        bool instant = Kind == EnemySpecialAbilityKind.ShareOre || Kind == EnemySpecialAbilityKind.LayFoundation || Kind == EnemySpecialAbilityKind.Faultline;
        int motion = actor.StartSpecialMotion(instant ? "Ability" : "ChannelStart");
        if (motion > 0) yield return actor.WaitForSpecialMotionBeat(motion);
        if (!Valid(motion)) { Finish(); yield break; }
        bool used = true;
        switch (Kind)
        {
            case EnemySpecialAbilityKind.ShareOre:
                foreach (var ally in roster)
                    if (ally != null && ally != actor && !ally.IsDefeated) ally.GetComponent<EnemyOrePower>()?.Grant();
                board.TryQueueMineDrillPower(1 + state.cycle % 2, 1); break;
            case EnemySpecialAbilityKind.LayFoundation:
                bool placed = false; used = false;
                if (board.TryQueuePlaceMineStones(actor, 2, 6, MineStoneStage.Brittle, ok => { used = ok; placed = true; },
                    () => disposed || actor.IsDefeated || stagger?.IsStaggered == true))
                    while (!placed && !disposed) yield return null;
                break;
            case EnemySpecialAbilityKind.Faultline:
                used = attack.PlayerTarget.Statuses.Apply(actor.Definition.appliedPlayerStatus, actor); break;
            case EnemySpecialAbilityKind.PowderCharge:
                bool armed = false; used = false;
                if (board.TryQueueMineStoneOperation(actor, state.stoneId, MineStoneOperation.ArmCharge, ok => { used = ok; armed = true; }))
                    while (!armed && !disposed) yield return null;
                break;
        }
        if (used && !disposed && !actor.IsDefeated)
        {
            actor.AnnounceCommittedCast(EnemyAbilityNames.Primary(actor.Definition));
            actor.NotifySpecialAbilityUsed(); actor.ResetSpecialCounter();
            if (!instant) { state.stage = 1; state.dueMove = Move + Mathf.Max(1, actor.Definition.mineWarningMoves); }
        }
        if (motion > 0) yield return actor.WaitForSpecialMotionComplete(motion);
        pending = false; actor.EndSpecialAbilityAnimationAction();
        if (instant || !used) { state.cycle++; Finish(); }
        else Hold();
    }
    private IEnumerator Release()
    {
        actor.SpecialIdleState = null;
        int motion = actor.StartSpecialMotion("Release");
        if (motion > 0) yield return actor.WaitForSpecialMotionBeat(motion);
        if (!Valid(motion) || TargetGone) { Finish(); yield break; }
        bool done = false, queued = false;
        switch (Kind)
        {
            case EnemySpecialAbilityKind.BoreDrill:
                queued = board.TryQueueSmallMineDrill(actor, state.horizontal, state.lane,
                    edge => { if (edge && !disposed && !actor.IsDefeated) Hit(); done = true; }); break;
            case EnemySpecialAbilityKind.AssayVein:
                queued = board.TryQueueMineStoneOperation(actor, state.stoneId, MineStoneOperation.Harden, _ => done = true); break;
            case EnemySpecialAbilityKind.PowderCharge:
                queued = board.TryQueueMineStoneOperation(actor, state.stoneId, MineStoneOperation.DetonateCharge,
                    ok => { if (ok && !disposed && !actor.IsDefeated) Hit(); done = true; }); break;
            case EnemySpecialAbilityKind.SwitchTrack:
                queued = board.TryQueueMineDrillShift(actor, state.drillId, state.lane, _ => done = true); break;
        }
        if (queued) while (!done && !disposed) yield return null;
        if (motion > 0) yield return actor.WaitForSpecialMotionComplete(motion);
        state.cycle++; Finish();
    }
    private void Hit() => attack.PlayerTarget.TryTakeDamage(CombatAmounts.Round(actor.Definition.mineAbilityDamage * actor.RuntimeStats.DamageMultiplier), actor);
    private bool Valid(int motion) => !disposed && !actor.IsDefeated && stagger?.IsStaggered != true &&
        (motion <= 0 || actor.IsSpecialMotionCurrent(motion));
    private void Hold() { attack?.SetActionPaused(this, BlocksBasic); actor.SpecialIdleState = IsPreparing ? "ChannelHold" : null; }
    private void Clear() { state.stage = 0; state.stoneId = 0; state.drillId = 0; state.action = null; state.dueMove = 0; }
    private void Finish()
    {
        board.ReleaseMineCharges(actor.PersistentId); Clear(); pending = false;
        actor.ResetSpecialCounter(); actor.EndSpecialAbilityAnimationAction(); Hold();
    }
    private void Interrupted(EnemyStagger _, float duration, float remaining)
    { StopAllCoroutines(); Finish(); }
    private void Died(EnemyActor _) => Cleanup();
    private void Update()
    {
        if (!disposed && IsPreparing && !pending && Time.timeScale > 0 &&
            CombatMoveClock.Current?.IsBlockingWaveProgression != true && TargetGone) Finish();
    }
    public void CaptureContinuation(EnemyCombatSnapshot saved, Func<EnemyActor, int> slotOf)
    { saved.mineEnemy = JsonUtility.FromJson<MineEnemySnapshot>(JsonUtility.ToJson(state)); }
    public void RestoreContinuation(EnemyCombatSnapshot saved, Func<int, EnemyActor> enemyAt)
    {
        state = saved.mineEnemy == null ? new MineEnemySnapshot() : JsonUtility.FromJson<MineEnemySnapshot>(JsonUtility.ToJson(saved.mineEnemy));
        Hold();
    }
    private void Cleanup()
    {
        if (disposed) return; disposed = true; StopAllCoroutines();
        if (board != null && actor != null) board.ReleaseMineCharges(actor.PersistentId);
        if (attack != null) attack.SetActionPaused(this, false);
        if (actor != null) { actor.Defeated -= Died; actor.SpecialIdleState = null; actor.EndSpecialAbilityAnimationAction(); }
        if (stagger != null) stagger.StaggerApplied -= Interrupted;
    }
    private void OnDisable() => Cleanup();
}
