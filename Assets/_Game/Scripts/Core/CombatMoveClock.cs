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
    public static bool MoveBasics => Active && Current.state.profile == CombatClockSnapshot.MoveProfile;
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
                bool held = (channel != null && channel.BlocksBasic) || actor.GetComponent<AquaticEnemyAbility>()?.BlocksBasic == true;
                var attack = actor.GetComponent<EnemyAutoAttack>();
                if (stagger == null || !stagger.IsStaggered)
                {
                    if (!held) actor.RegisterValidPlayerTurn();
                    foreach (var ability in actor.GetComponents<IAcceptedMoveEnemyAbility>()) ability.ResolveAcceptedMove();
                    Opportunity?.Invoke(actor);
                    actor.ResumeContinuationReadiness();
                }
                yield return WaitForActions();
                if (MoveBasics && Living(actor) && !run.Player.IsDefeated && !held &&
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
            foreach (var actor in acceptedActors)
            {
                if (!Living(actor)) continue;
                actor.GetComponent<EnemyStagger>()?.ExpireAcceptedMove(Tick);
                actor.GetComponent<TownMarshalEnemyAbility>()?.ExpireAcceptedMove(Tick);
                actor.GetComponent<ForestMilestoneEnemyAbility>()?.ExpireAcceptedMove(Tick);
            }
            run.Player.GetComponent<RoyalDecreeRuntime>()?.ExpireAcceptedMove(Tick);
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
                    actor.GetComponent<EnemyAutoAttack>()?.IsAttackSequenceInProgress == true ||
                    actor.GetComponent<EnemyAutoAttack>()?.HasCommandReservation == true);
            if (!busy) yield break;
            yield return null;
        }
    }

    private static bool Living(EnemyActor actor) => actor != null && actor.IsInitialized && !actor.IsDefeated;
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
