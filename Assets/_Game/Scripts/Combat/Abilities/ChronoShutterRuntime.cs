using System;
using System.Collections.Generic;
using UnityEngine;

public enum BoardMemoryPhase { Inactive, Casting, Holding, AwaitingSettlement, Rewinding, Recovering }

[Serializable]
public sealed class BoardMemoryAbilitySnapshot
{
    public BoardMemoryPhase phase;
    public int remainingMoves, lastAcceptedMove, wave;
    public float phaseRemaining;
    public BoardCombatSnapshot photograph;
}

[DisallowMultipleComponent]
public sealed class ChronoShutterRuntime : MonoBehaviour, IPlayerAbilityRuntime, IPlayerAbilityPresentation
{
    public const float CastDuration = .85f, RewindDuration = .3f, RecoveryDuration = .37f;
    private PlayerActor player;
    private BoardController board;
    private WaveController waves;
    private BoardCombatSnapshot photograph;
    private readonly Dictionary<int, EnemyActor> photographedOwners = new Dictionary<int, EnemyActor>();
    private IDisposable inputBlock;
    private int lastAcceptedMove, wave;
    private float phaseRemaining;
    public event Action StateChanged;
    public event Action Rewound;
    public BoardMemoryPhase Phase { get; private set; }
    public int RemainingMoves { get; private set; }
    public float PhaseRemaining => phaseRemaining;
    public bool IsActive => Phase != BoardMemoryPhase.Inactive;
    public string AnimationState => Phase == BoardMemoryPhase.Inactive ? "Idle" :
        Phase == BoardMemoryPhase.Casting ? "Ability" : Phase == BoardMemoryPhase.Recovering ? "Recovery" : "Hold";
    public float AnimationNormalizedTime => Phase == BoardMemoryPhase.Casting
        ? 1f - phaseRemaining / CastDuration : Phase == BoardMemoryPhase.Recovering ? 1f - phaseRemaining / RecoveryDuration : 0;
    public bool ShowMoveCounter => IsActive && Phase != BoardMemoryPhase.Casting && Phase != BoardMemoryPhase.Recovering;
    public int RemainingMoveCount => RemainingMoves;
    public bool Supports(CharacterAbilityDefinition definition) => definition is ChronoShutterAbilityDefinition;

    private void Resolve()
    {
        if (player == null) player = GetComponent<PlayerActor>();
        if (board == null) board = RunSession.Current?.Board ?? FindFirstObjectByType<BoardController>();
        if (waves == null) waves = RunSession.Current?.Waves ?? FindFirstObjectByType<WaveController>();
    }
    public bool CanActivate(CharacterAbilityDefinition definition)
    {
        Resolve();
        return isActiveAndEnabled && !IsActive && Supports(definition) && player != null &&
            player.IsInitialized && !player.IsDefeated && board != null && board.CanCaptureContinuation &&
            !board.IsExternalInputBlocked && board.RefillRandomState != 0 && waves != null &&
            waves.IsWaveActive && waves.CanCaptureContinuation && Time.timeScale > 0;
    }
    public bool TryActivate(CharacterAbilityDefinition definition)
    {
        if (!CanActivate(definition)) return false;
        photograph = board.CaptureBoardMemory(id => id);
        if (!board.IsValidBoardMemory(photograph)) { photograph = null; return false; }
        photographedOwners.Clear();
        foreach (var enemy in waves.ActiveEnemies)
            if (enemy != null && !enemy.IsDefeated) photographedOwners[enemy.GetInstanceID()] = enemy;
        RemainingMoves = RunUpgradeResolver.ResolveBoardMemoryMoves(((ChronoShutterAbilityDefinition)definition).ManualMoves);
        lastAcceptedMove = board.CompletedValidPlayerMoves;
        wave = waves.CurrentWave;
        Subscribe();
        inputBlock = board.AcquireExternalInputBlock();
        if (board.GetComponent<BoardStateRestoreVFX>() == null) board.gameObject.AddComponent<BoardStateRestoreVFX>();
        SetPhase(BoardMemoryPhase.Casting, CastDuration);
        return true;
    }
    private void Subscribe()
    {
        board.ValidPlayerMoveAccepted -= MoveAccepted;
        board.ValidPlayerMoveAccepted += MoveAccepted;
        waves.WaveCompleted -= EncounterCompleted;
        waves.WaveCompleted += EncounterCompleted;
    }
    private void MoveAccepted(int move)
    {
        if (Phase != BoardMemoryPhase.Holding || move <= lastAcceptedMove) return;
        lastAcceptedMove = move;
        RemainingMoves--;
        if (RemainingMoves == 0)
        {
            inputBlock = board.AcquireExternalInputBlock();
            SetPhase(BoardMemoryPhase.AwaitingSettlement);
        }
        else StateChanged?.Invoke();
    }
    private void Update()
    {
        if (!IsActive) return;
        if (player == null || player.IsDefeated || waves == null || waves.CurrentWave != wave ||
            !waves.IsWaveActive || (RunSession.Current != null && RunSession.Current.IsFinished))
        { Cancel(); return; }
        if (Time.timeScale <= 0) return;
        if (Phase == BoardMemoryPhase.AwaitingSettlement)
        {
            // Completed-turn subscribers and enemy animation impacts may enqueue
            // further mutations. Drain that same queue before replacing state.
            if (!board.CanCaptureContinuation || !waves.CanCaptureContinuation ||
                (CombatMoveClock.Current != null && CombatMoveClock.Current.IsBlockingWaveProgression)) return;
            if (!board.TryRestoreBoardMemory(photograph, Owner)) { Cancel(); return; }
            photograph = null;
            SetPhase(BoardMemoryPhase.Rewinding, RewindDuration);
            player.GrantShield(RunUpgradeResolver.ResolveBoardMemoryRewindShield());
            Rewound?.Invoke();
            return;
        }
        if (Phase == BoardMemoryPhase.Holding) return;
        phaseRemaining = Mathf.Max(0, phaseRemaining - Time.deltaTime);
        if (phaseRemaining > 0) return;
        switch (Phase)
        {
            case BoardMemoryPhase.Casting:
                inputBlock?.Dispose(); inputBlock = null;
                SetPhase(BoardMemoryPhase.Holding); break;
            case BoardMemoryPhase.Rewinding:
                SetPhase(BoardMemoryPhase.Recovering, RecoveryDuration); break;
            case BoardMemoryPhase.Recovering: Cancel(); break;
        }
    }
    private EnemyActor Owner(int id) => photographedOwners.TryGetValue(id, out var owner) &&
        owner != null && !owner.IsDefeated ? owner : null;
    private void SetPhase(BoardMemoryPhase phase, float duration = 0)
    { Phase = phase; phaseRemaining = duration; StateChanged?.Invoke(); }
    private void EncounterCompleted(int _) => Cancel();
    public void Cancel()
    {
        if (board != null) board.ValidPlayerMoveAccepted -= MoveAccepted;
        if (waves != null) waves.WaveCompleted -= EncounterCompleted;
        inputBlock?.Dispose(); inputBlock = null;
        photograph = null; photographedOwners.Clear(); RemainingMoves = 0;
        SetPhase(BoardMemoryPhase.Inactive);
    }
    private void OnDisable() => Cancel();

    public BoardMemoryAbilitySnapshot CaptureContinuation()
    {
        if (!IsActive) return null;
        var copy = photograph == null ? null : JsonUtility.FromJson<BoardCombatSnapshot>(JsonUtility.ToJson(photograph));
        if (copy != null) RemapOwners(copy, id => waves.ContinuationSlot(Owner(id)));
        return new BoardMemoryAbilitySnapshot { phase = Phase, remainingMoves = RemainingMoves,
            lastAcceptedMove = lastAcceptedMove, wave = wave, phaseRemaining = phaseRemaining, photograph = copy };
    }
    public void RestoreContinuation(BoardMemoryAbilitySnapshot saved)
    {
        Cancel(); Resolve();
        if (saved == null || saved.phase == BoardMemoryPhase.Inactive) return;
        if (!Supports(player.ActiveAbility) || saved.wave != waves.CurrentWave || !waves.IsWaveActive ||
            !Enum.IsDefined(typeof(BoardMemoryPhase), saved.phase) || saved.remainingMoves < 0 ||
            saved.remainingMoves > 20 || saved.phaseRemaining < 0 || saved.phaseRemaining > CastDuration ||
            float.IsNaN(saved.phaseRemaining) || float.IsInfinity(saved.phaseRemaining) ||
            ((saved.phase == BoardMemoryPhase.Casting || saved.phase == BoardMemoryPhase.Holding) && saved.remainingMoves == 0) ||
            (saved.phase >= BoardMemoryPhase.AwaitingSettlement && saved.remainingMoves != 0))
            throw new InvalidOperationException("Invalid saved board-memory ability.");
        bool needsPhoto = saved.phase <= BoardMemoryPhase.AwaitingSettlement;
        if (needsPhoto && !board.IsValidBoardMemory(saved.photograph))
            throw new InvalidOperationException("Saved board photograph is invalid.");
        photograph = saved.photograph == null ? null : JsonUtility.FromJson<BoardCombatSnapshot>(JsonUtility.ToJson(saved.photograph));
        if (photograph != null) RemapOwners(photograph, slot =>
        {
            var enemy = waves.ContinuationEnemy(slot);
            if (enemy == null || enemy.IsDefeated) return 0;
            int id = enemy.GetInstanceID(); photographedOwners[id] = enemy; return id;
        });
        RemainingMoves = saved.remainingMoves; lastAcceptedMove = saved.lastAcceptedMove; wave = saved.wave;
        Subscribe();
        if (saved.phase != BoardMemoryPhase.Holding) inputBlock = board.AcquireExternalInputBlock();
        if (board.GetComponent<BoardStateRestoreVFX>() == null) board.gameObject.AddComponent<BoardStateRestoreVFX>();
        SetPhase(saved.phase, saved.phaseRemaining);
    }
    private static void RemapOwners(BoardCombatSnapshot saved, Func<int,int> map)
    {
        foreach (var cell in saved.cells)
        {
            if (cell.pinned) cell.pinOwner = map(cell.pinOwner);
            if (cell.mined) cell.mineOwner = map(cell.mineOwner);
            if (cell.barricade) cell.barricadeOwner = map(cell.barricadeOwner);
            if (cell.banner) cell.bannerOwner = map(cell.bannerOwner);
        }
    }
}
