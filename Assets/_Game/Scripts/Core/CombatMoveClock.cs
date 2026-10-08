using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAcceptedMoveEnemyAbility
{
    void ResolveAcceptedMove();
}

/// <summary>Coordinates accepted consequences; never resolves or mutates the gem grid.</summary>
[DisallowMultipleComponent]
public sealed class CombatMoveClock : MonoBehaviour, IWaveProgressionGate
{
    public static CombatMoveClock Current { get; private set; }
    public static bool Active => Current != null;
    public static bool MoveEffects => Active && Current.state.profile != CombatClockSnapshot.LegacyEffectsProfile;
    public static bool Unified => Active && Current.state.profile == CombatClockSnapshot.UnifiedProfile;
    public static bool MoveBasics => Unified || Active && Current.state.profile == CombatClockSnapshot.MoveProfile;
    public static bool PausesTimedBasics => Active && !MoveBasics && Current.IsBlockingWaveProgression;
    public static int EffectAction => Current == null ? 0 : Math.Max(Current.state.actions.completed, Current.state.actions.pending);
    public int Tick => state.actions.completed;
    public int TestEncounterOffset => state.testEncounterOffset;
    public bool IsResolving { get; private set; }
    public bool IsBlockingWaveProgression => IsResolving || state.actions.IsPending;
    public EnemyActor ActingEnemy { get; private set; }
    public event Action<EnemyActor> Opportunity;
    public event Action<int> ActionSettled;
    private RunSession run;
    private CombatClockSnapshot state;
    private IDisposable input;
    private readonly List<EnemyActor> acceptedActors = new List<EnemyActor>();
    private readonly HashSet<long> spentSpecial = new HashSet<long>();
    private readonly HashSet<long> dueBasics = new HashSet<long>();
    private readonly HashSet<long> spentBasics = new HashSet<long>();
    public static void RecordCommandedBasic(EnemyActor actor)
    {
        if (Unified && actor != null) Current.spentBasics.Add(actor.PersistentId);
    }

    public void Initialize(RunSession owner, CombatClockSnapshot saved = null, string newProfile = CombatClockSnapshot.MoveProfile)
    {
        if (Current != null && Current != this) throw new InvalidOperationException("Duplicate combat clock.");
        run = owner;
        state = saved == null
            ? new CombatClockSnapshot { profile = newProfile, testEncounterOffset = RunLaunchOptions.ForestEncounterOffset }
            : JsonUtility.FromJson<CombatClockSnapshot>(JsonUtility.ToJson(saved));
        RunLaunchOptions.ForestEncounterOffset = 0;
        if (!CombatClockSnapshot.IsSupported(state.profile) || state.actions == null || state.actions.IsPending)
            throw new InvalidOperationException("Unsupported or unstable combat clock snapshot.");
        Current = this;
        state.zoneId = run.Zone.Definition.zoneId;
        run.Board.ValidPlayerMoveAccepted += Accepted;
        run.Board.ValidPlayerMoveCompleted += Completed;
        run.Waves.RegisterProgressionGate(this);
    }

    public CombatClockSnapshot Capture()
    {
        if (IsBlockingWaveProgression) throw new InvalidOperationException("Clock has unresolved work.");
        return JsonUtility.FromJson<CombatClockSnapshot>(JsonUtility.ToJson(state));
    }

    public long AllocateActor() => run.Continuation?.IsRestoring == true && !run.Continuation.IsReplaying ? 0 : state.actions.AllocateActor();
    public static bool CanOffer(EnemyActor actor) => !Active ||
        (actor != null && Current.IsResolving && Current.ActingEnemy == actor && !Current.spentSpecial.Contains(actor.PersistentId));

    public static bool ClaimSpecial(EnemyActor actor)
    {
        if (!Active) return true;
        // Existing commands may reserve and lock a participant as part of their
        // one sequence; that does not grant the participant a second basic.
        if (Current.IsResolving && actor != null && Current.acceptedActors.Contains(actor) &&
            actor.GetComponent<EnemyAutoAttack>()?.HasCommandReservation == true)
            return Current.spentSpecial.Add(actor.PersistentId);
        return CanOffer(actor) && Current.spentSpecial.Add(actor.PersistentId);
    }

    private void Accepted(int action)
    {
        if (!state.actions.Accept(action)) return;
        run.Board.AcceptAquaticMove(action);
        input = run.Board.AcquireExternalInputBlock();
        acceptedActors.Clear();
        foreach (var actor in run.Waves.ActiveEnemies)
            if (Living(actor)) acceptedActors.Add(actor);
        acceptedActors.Sort((a,b) => a.PersistentId.CompareTo(b.PersistentId));
    }

    private void Completed(int action)
    {
        if (!state.actions.Commit(action)) return;
        IsResolving = true;
        StartCoroutine(Resolve());
    }

    private IEnumerator Resolve()
    {
        try
        {
            // All legacy completion listeners finish with move-profile runtime
            // acceptance gated. Their registration order cannot grant an action.
            yield return null;
            while (run.Board.IsBusy || Time.timeScale <= 0)
            {
                if (run.Player.IsDefeated) yield break;
                yield return null;
            }
            spentSpecial.Clear();
            spentBasics.Clear();
            dueBasics.Clear();
            // A timed basic accepted before the swap keeps its owned impact and
            // recovery. Drain it before offering move-timed specialist work.
            yield return WaitForActions();
            foreach (var actor in acceptedActors)
                if (Living(actor)) actor.GetComponent<EnemyPoisonStatus>()?.AdvanceAcceptedMove(Tick);
            // Normal forest growth precedes enemy casts. A harvest consumes the
            // full current frontier, and a new root never spreads on its birth action.
            if (!run.Player.IsDefeated) yield return run.Board.AdvanceZoneEnvironment(Tick);
            // Snapshot this tick's readiness before an earlier actor can apply a
            // speed buff. Newly granted buffs first accelerate a future move.
            foreach (var actor in acceptedActors)
                if (Living(actor)) actor.GetComponent<EnemyAutoAttack>()?.AdvanceAcceptedMove();
            if (Unified)
            {
                // Readiness events cannot cast while ActingEnemy is null. All
                // countdowns advance before the first special changes the roster.
                foreach (var actor in acceptedActors)
                {
                    if (!Living(actor)) continue;
                    if (actor.GetComponent<EnemyAutoAttack>()?.RemainingAttackTime <= 0) dueBasics.Add(actor.PersistentId);
                    if (actor.GetComponent<EnemyStagger>()?.IsStaggered != true && !BlocksSpecialCountdown(actor))
                        actor.RegisterValidPlayerTurn();
                }
                acceptedActors.Sort(CompareSpecialPriority);
            }
            foreach (var actor in acceptedActors)
            {
                if (!Living(actor) || run.Player.IsDefeated) continue;
                while (run.Board.IsBusy || Time.timeScale <= 0)
                {
                    if (run.Player.IsDefeated) yield break;
                    yield return null;
                }
                if (!Living(actor) || run.Player.IsDefeated) continue;
                ActingEnemy = actor;
                var stagger = actor.GetComponent<EnemyStagger>();
                var channel = actor.GetComponent<EnemyChannelRuntime>();
                bool held = (channel != null && channel.BlocksBasic) || actor.GetComponent<AquaticEnemyAbility>()?.BlocksBasic == true ||
                    actor.GetComponent<MineEnemyAbility>()?.BlocksBasic == true;
                var attack = actor.GetComponent<EnemyAutoAttack>();
                if (stagger == null || !stagger.IsStaggered)
                {
                    if (!Unified && !held) actor.RegisterValidPlayerTurn();
                    foreach (var ability in actor.GetComponents<IAcceptedMoveEnemyAbility>()) ability.ResolveAcceptedMove();
                    Opportunity?.Invoke(actor);
                    actor.ResumeContinuationReadiness();
                }
                yield return WaitForActions();
                if (!Unified && MoveBasics && Living(actor) && !run.Player.IsDefeated && !held &&
                    !spentSpecial.Contains(actor.PersistentId) && (channel == null || !channel.BlocksBasic))
                {
                    // Reserve this ordinary opportunity before callbacks can
                    // offer a special or a second command to the same actor.
                    spentSpecial.Add(actor.PersistentId);
                    if (attack == null || !attack.TryPerformAcceptedMoveAttack())
                        spentSpecial.Remove(actor.PersistentId);
                }
                yield return WaitForActions();
                ActingEnemy = null;
            }
            if (Unified)
            {
                acceptedActors.Sort(CompareSlot);
                foreach (var actor in acceptedActors)
                {
                    if (!Living(actor) || run.Player.IsDefeated || !dueBasics.Contains(actor.PersistentId) ||
                        spentBasics.Contains(actor.PersistentId) || BlocksBasic(actor)) continue;
                    // Special offers stay closed during the basic phase. Commands
                    // that already spent a participant's attack are never replayed.
                    spentBasics.Add(actor.PersistentId);
                    actor.GetComponent<EnemyAutoAttack>()?.TryPerformAcceptedMoveAttack();
                    yield return WaitForActions();
                }
            }
            if (!Unified) ExpireActorEffects();
            if (!Unified) run.Player.GetComponent<RoyalDecreeRuntime>()?.ExpireAcceptedMove(Tick);
            if (!run.Player.IsDefeated) yield return run.Board.DrainMineDrills();
            // Drill-counterable warnings stay live through the final response
            // move's actual environmental firing, then release under this hold.
            if (Unified) acceptedActors.Sort(CompareSpecialPriority);
            foreach (var actor in acceptedActors)
            {
                if (!Living(actor)) continue;
                if (run.Player.IsDefeated) continue;
                var mineAbility = actor.GetComponent<MineEnemyAbility>();
                if (mineAbility == null) continue;
                ActingEnemy = actor;
                mineAbility.ResolveAfterMineDrills();
                yield return WaitForActions();
                ActingEnemy = null;
            }
            if (Unified)
            {
                ExpireActorEffects();
                run.Player.GetComponent<RoyalDecreeRuntime>()?.ExpireAcceptedMove(Tick);
            }
            run.AdvanceSupplyCooldowns();
            run.Board.FinishAquaticMove(Tick);
            ActionSettled?.Invoke(Tick);
        }
        finally
        {
            ActingEnemy = null;
            IsResolving = false;
            input?.Dispose(); input = null;
            acceptedActors.Clear();
        }
    }

    private IEnumerator WaitForActions()
    {
        // Allow synchronous acceptance to start its existing coroutine before
        // checking recovery. No game resource is advanced by this yield.
        yield return null;
        while (run != null)
        {
            if (run.Player.IsDefeated) yield break;
            bool busy = run.Board.IsBusy || Time.timeScale <= 0;
            foreach (var actor in run.Waves.ActiveEnemies)
                busy |= Living(actor) && (actor.HasAnimationActionInProgress ||
                    actor.GetComponent<MineEnemyAbility>()?.IsResolving == true ||
                    actor.GetComponent<EnemyAutoAttack>()?.IsAttackSequenceInProgress == true ||
                    actor.GetComponent<EnemyAutoAttack>()?.HasCommandReservation == true);
            if (!busy) yield break;
            yield return null;
        }
    }

    private bool Living(EnemyActor actor) => actor != null && actor.IsInitialized && !actor.IsDefeated &&
        (!Unified || run.Waves.ContinuationSlot(actor) >= 0);
    private void ExpireActorEffects()
    {
        foreach (var actor in acceptedActors)
        {
            if (!Living(actor)) continue;
            actor.GetComponent<EnemyStagger>()?.ExpireAcceptedMove(Tick);
            actor.GetComponent<TownMarshalEnemyAbility>()?.ExpireAcceptedMove(Tick);
            if (Unified) actor.GetComponent<AquaticEnemyAbility>()?.ExpireAcceptedMove(Tick);
        }
    }
    private static bool BlocksBasic(EnemyActor actor) =>
        actor.GetComponent<EnemyChannelRuntime>()?.BlocksBasic == true ||
        actor.GetComponent<AquaticEnemyAbility>()?.BlocksBasic == true ||
        actor.GetComponent<MineEnemyAbility>()?.BlocksBasic == true ||
        actor.GetComponent<EnemyAutoAttack>()?.IsPausedByAction == true;
    private static bool BlocksSpecialCountdown(EnemyActor actor) =>
        actor.GetComponent<EnemyChannelRuntime>()?.BlocksBasic == true ||
        actor.GetComponent<AquaticEnemyAbility>()?.BlocksBasic == true ||
        actor.GetComponent<MineEnemyAbility>()?.BlocksBasic == true ||
        actor.GetComponent<ForestMilestoneEnemyAbility>()?.BlocksBasic == true ||
        actor.GetComponent<ForestPressureAbility>()?.IsPreparing == true ||
        actor.GetComponent<KingEnemyAbility>()?.BlocksBasic == true;
    private int CompareSlot(EnemyActor a, EnemyActor b)
    {
        int slots = run.Waves.ContinuationSlot(a).CompareTo(run.Waves.ContinuationSlot(b));
        return slots != 0 ? slots : a.PersistentId.CompareTo(b.PersistentId);
    }
    private int CompareSpecialPriority(EnemyActor a, EnemyActor b)
    {
        int rank = b.Definition.Category.CompareTo(a.Definition.Category);
        return rank != 0 ? rank : CompareSlot(a, b);
    }
    private void OnDestroy()
    {
        if (run != null && run.Board != null)
        {
            run.Board.ValidPlayerMoveAccepted -= Accepted;
            run.Board.ValidPlayerMoveCompleted -= Completed;
        }
        input?.Dispose(); input = null;
        if(run!=null && run.Waves!=null) run.Waves.UnregisterProgressionGate(this);
        if (Current == this) Current = null;
    }
}
