using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Owns the run journal, consumable acceptance and final settlement, never board/combat resolution.</summary>
[DefaultExecutionOrder(-80)]
public sealed class RunSession : MonoBehaviour, IWaveProgressionGate
{
    public static RunSession Current { get; private set; }
    public string RunId { get; private set; }
    public bool IsFinished { get; private set; }
    public bool IsVictory { get; private set; }
    public bool NeedsSaveRetry => journalWriteFailed;
    public bool RewardFinalized => account != null && account.ActiveRun == null && account.LastReward?.runId == RunId;
    public bool IsBlockingWaveProgression => IsFinished || journalWriteFailed;
    public PlayerActor Player { get; private set; }
    public BoardController Board { get; private set; }
    public WaveController Waves { get; private set; }
    public event Action Finished;
    private AccountProgression account;
    private readonly float[] cooldowns = new float[2];
    private bool hasMilestone, hasKing, journalWriteFailed, transitioning;
    private int pendingWave;
    private float waveStartedAt, runStartedAt, waveDamage;
    private int movesAtStart;
    private string metricsPath;

    private void Awake()
    {
        Current = this;
        Player = FindFirstObjectByType<PlayerActor>();
        Board = FindFirstObjectByType<BoardController>();
        Waves = FindFirstObjectByType<WaveController>();
        account = AccountProgression.Current;
        RunId = account.BeginRun(CharacterSelectionSettings.SelectedPlayerId);
        journalWriteFailed = string.IsNullOrEmpty(RunId);
        if (Waves != null)
        {
            Waves.RegisterProgressionGate(this);
            Waves.WaveStarted += OnWaveStarted;
            Waves.WaveCompleted += OnWaveCompleted;
        }
        if (Player != null) { Player.Defeated += OnDefeated; Player.DamageTaken += OnDamage; }
        runStartedAt = Time.unscaledTime;
        if (Debug.isDebugBuild)
        {
            metricsPath = Path.Combine(Application.persistentDataPath, "balance-v1-metrics.csv");
            if (!File.Exists(metricsPath)) AppendMetric("run,character,level,wave,seconds,run_seconds,moves,hp_lost,health,gold_preview,ability_cost");
        }
    }

    private void Update()
    {
        for (int i = 0; i < cooldowns.Length; i++) cooldowns[i] = Mathf.Max(0, cooldowns[i] - Time.deltaTime);
    }

    public int Charges(ConsumableKind kind) => IsFinished ? 0 : account.Charges(RunId, kind);
    public float Cooldown(ConsumableKind kind) => cooldowns[(int)kind];
    public bool CanUse(ConsumableKind kind) => !IsFinished && !transitioning && !journalWriteFailed &&
        Player != null && Player.IsInitialized && !Player.IsDefeated && Waves != null && Waves.IsWaveActive &&
        Board != null && !Board.IsBusy && !Board.IsExternalInputBlocked && Time.timeScale > 0 &&
        Charges(kind) > 0 && Cooldown(kind) <= 0;

    public bool TryUsePotion()
    {
        if (!CanUse(ConsumableKind.HealthPotion) || Player.CurrentHealth >= Player.MaximumHealth) return false;
        if (!CommitUse(ConsumableKind.HealthPotion)) return false;
        Player.Heal(RunUpgradeResolver.ResolveHealing(Mathf.CeilToInt(Player.MaximumHealth * BalanceV1.Current.potionHealthFraction)));
        return true;
    }

    public bool ToggleBombTargeting()
    {
        if (Board != null && Board.IsSelectingTarget) { CancelTargeting(); return true; }
        if (!CanUse(ConsumableKind.Bomb)) return false;
        Board.SetTargetSelection(TryUseBomb);
        return true;
    }

    public bool TryUseBomb(Gem gem)
    {
        return CanUse(ConsumableKind.Bomb) && Board.TryClearPlayerArea(gem, BalanceV1.Current.consumableBombRadius, () => CommitUse(ConsumableKind.Bomb));
    }
    public void CancelTargeting() { if (Board != null) Board.SetTargetSelection(null); }
    private bool CommitUse(ConsumableKind kind)
    {
        if (!account.TrySpendCharge(RunId, kind)) return false;
        cooldowns[(int)kind] = BalanceV1.Current.consumableCooldown;
        return true;
    }
    private void OnWaveStarted(int wave)
    {
        hasMilestone = hasKing = false;
        foreach (var definition in Waves.OriginalEncounterDefinitions)
        {
            if (definition == null) continue;
            hasMilestone |= definition.Category == EnemyCategory.Miniboss;
            hasKing |= definition.Category == EnemyCategory.Boss;
        }
        waveStartedAt = Time.unscaledTime;
        movesAtStart = Board.CompletedValidPlayerMoves;
        waveDamage = 0;
    }
    private void OnDamage(PlayerActor player, int amount) { waveDamage += amount; }
    private void OnWaveCompleted(int wave)
    {
        if (IsFinished) return;
        pendingWave = wave;
        TryRecordPendingWave();
    }
    private void TryRecordPendingWave()
    {
        int wave = pendingWave;
        journalWriteFailed = !account.RecordWave(RunId, wave, hasMilestone && !hasKing, hasKing);
        if (journalWriteFailed) return;
        pendingWave = 0;
        RecordMetric(wave);
        if (hasKing && !IsFinished)
        {
            IsVictory = true;
            IsFinished = true; // Gate immediately; wait for the final board clear before presentation.
            CancelTargeting();
            StartCoroutine(CompleteVictory());
        }
    }
    private IEnumerator CompleteVictory()
    {
        while (Board != null && Board.IsBusy) yield return null;
        journalWriteFailed = !account.FinalizeRun(RunId, "Victory");
        Finished?.Invoke();
    }
    private void OnDefeated(PlayerActor player)
    {
        if (IsFinished) return;
        IsFinished = true;
        CancelTargeting();
        journalWriteFailed = !RetrySave();
        Finished?.Invoke();
    }
    public bool RetrySave()
    {
        if (string.IsNullOrEmpty(RunId))
        {
            if (account.ActiveRun != null && !account.FinalizeRun(account.ActiveRun.id, "Interrupted")) return false;
            RunId = account.BeginRun(CharacterSelectionSettings.SelectedPlayerId);
            journalWriteFailed = string.IsNullOrEmpty(RunId);
            return !journalWriteFailed;
        }
        if (pendingWave > 0) { TryRecordPendingWave(); if (journalWriteFailed) return false; }
        if (!IsFinished) return !journalWriteFailed;
        journalWriteFailed = !RewardFinalized && !account.FinalizeRun(RunId, IsVictory ? "Victory" : "Defeat");
        return !journalWriteFailed;
    }
    public bool ExitTo(string scene)
    {
        if (transitioning || !Application.CanStreamedLevelBeLoaded(scene)) return false;
        if (pendingWave > 0 && !RetrySave()) return false;
        var active = account.ActiveRun;
        if (active != null && active.id == RunId && !account.FinalizeRun(RunId, scene == "Game" ? "Retry" : "Quit to Menu")) return false;
        transitioning = true;
        CancelTargeting();
        // Scene unload disposes the existing actor/board/ability/attack owners.
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
        return true;
    }
    private void OnApplicationQuit()
    {
        if (account != null && !string.IsNullOrEmpty(RunId)) account.FinalizeRun(RunId, "Quit");
    }
    private void RecordMetric(int wave)
    {
        if (!Debug.isDebugBuild || Player == null) return;
        AppendMetric(string.Join(",", RunId, Player.Definition.PlayerId, Player.PermanentLevel, wave,
            (Time.unscaledTime-waveStartedAt).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            (Time.unscaledTime-runStartedAt).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            Board.CompletedValidPlayerMoves-movesAtStart, waveDamage, Player.CurrentHealth,
            account.PreviewReward("Preview")?.Total ?? 0, Player.ActiveAbility != null ? Player.ActiveAbility.EnergyCost : 0));
    }
    private void AppendMetric(string line)
    {
        if (string.IsNullOrEmpty(metricsPath)) return;
        try { File.AppendAllText(metricsPath, line + Environment.NewLine); }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException) { /* Diagnostics never own gameplay. */ }
    }
    private void OnDestroy()
    {
        CancelTargeting();
        if (Waves != null) { Waves.WaveStarted -= OnWaveStarted; Waves.WaveCompleted -= OnWaveCompleted; Waves.UnregisterProgressionGate(this); }
        if (Player != null) { Player.Defeated -= OnDefeated; Player.DamageTaken -= OnDamage; }
        if (Current == this) Current = null;
    }
}
