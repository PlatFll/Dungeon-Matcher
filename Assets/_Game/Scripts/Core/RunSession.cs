using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Owns the run journal, consumable acceptance and final settlement, never board/combat resolution.</summary>
[DefaultExecutionOrder(-80)]
public sealed partial class RunSession : MonoBehaviour, IWaveProgressionGate
{
    public static RunSession Current { get; private set; }
    public string RunId { get; private set; }
    public bool IsFinished { get; private set; }
    public bool IsVictory { get; private set; }
    public bool NeedsSaveRetry => journalWriteFailed || (Continuation!=null && Continuation.Error!=null);
    public bool RewardFinalized => account != null && account.ActiveRun == null && account.LastReward?.runId == RunId;
    public bool IsBlockingWaveProgression => IsFinished || journalWriteFailed || (Continuation!=null && Continuation.IsRestoring && !Continuation.IsReplaying);
    public RunContinuation Continuation { get; private set; }
    public bool InitialStateReady { get; private set; }
    private IDisposable resumedCharacter, resumedMastery;
    public PlayerActor Player { get; private set; }
    public BoardController Board { get; private set; }
    public WaveController Waves { get; private set; }
    public event Action Finished;
    private AccountProgression account;
    private IDisposable practiceProfile;
    public bool IsPractice => account != null && account.IsPractice;
    public RunChallenge Challenge { get; private set; }
    private readonly float[] cooldowns = new float[2];
    private bool hasMilestone, hasKing, journalWriteFailed, transitioning;
    private int pendingWave;
    private float waveStartedAt, runStartedAt, waveDamage;
    private int movesAtStart;
    private string metricsPath;
    public bool HasBombPreview { get; private set; }
    public Vector2Int BombPreviewCell { get; private set; }

    private void Awake()
    {
        Current = this;
        Player = FindFirstObjectByType<PlayerActor>();
        Board = FindFirstObjectByType<BoardController>();
        Waves = FindFirstObjectByType<WaveController>();
        if (Board != null) Board.UsefulResponseValidator = options => CounterplayGuard.HasUsefulResponse(options, Player, Waves);
        if (RunLaunchOptions.Practice) practiceProfile = AccountProgression.UsePracticeProfile();
        RunLaunchOptions.Practice = false;
        account = AccountProgression.Current;
        Challenge = RunLaunchOptions.Challenge;
        RunLaunchOptions.Challenge = RunChallenge.Standard;
        var continued=account.ActiveRun;
        if(continued!=null)
        {
            Challenge=continued.challenge;
            resumedCharacter=CharacterSelectionSettings.UseTemporarySelection(continued.playerId);
            if(continued.mastery!=null && continued.mastery.Count==4)
                resumedMastery=GemMasterySettings.UseTemporaryLoadout(new GemMasteryLoadout(
                    continued.mastery[0],continued.mastery[1],continued.mastery[2],continued.mastery[3]));
            RunId=continued.id;
        }
        else RunId = account.BeginRun(CharacterSelectionSettings.SelectedPlayerId, Challenge);
        journalWriteFailed = string.IsNullOrEmpty(RunId);
        if (Waves != null)
        {
            Waves.RegisterProgressionGate(this);
            Waves.WaveStarted += OnWaveStarted;
            Waves.WaveCompleted += OnWaveCompleted;
        }
        if (Player != null) { Player.Defeated += OnDefeated; Player.DamageTaken += OnDamage; }
        runStartedAt = Time.unscaledTime;
        Continuation=gameObject.AddComponent<RunContinuation>();
        Continuation.Initialize(this,account,continued?.checkpoint,continued?.tape);
        gameObject.AddComponent<RunFrameRecorder>();
        if (Debug.isDebugBuild)
        {
            metricsPath = Path.Combine(Application.persistentDataPath, "balance-v1-metrics.csv");
            if (!File.Exists(metricsPath)) AppendMetric("run,character,level,wave,seconds,run_seconds,moves,hp_lost,health,gold_preview,ability_cost");
        }
    }

    private IEnumerator Start()
    {
        // Wave 1 can spawn before PlayerActor.Start selects its definition.
        // Wait for scene initialization so the opening charge is never skipped.
        yield return null;
        while(Player!=null && !Player.IsInitialized) yield return null;
        if(!Continuation.IsRestoring && Player?.ActiveAbility!=null)
            Player.GetComponent<PlayerAbilityEnergy>()?.AddEnergy(Mathf.FloorToInt(Player.ActiveAbility.EnergyCost*.2f));
        InitialStateReady=true;
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
        if (!CommitUse(ConsumableKind.HealthPotion,new RunRecordedAction {kind=RunActionKind.Potion})) return false;
        Player.Heal(RunUpgradeResolver.ResolveHealing(Mathf.CeilToInt(Player.MaximumHealth * BalanceV1.Current.potionHealthFraction)));
        return true;
    }

    public bool ToggleBombTargeting()
    {
        if (Board != null && Board.IsSelectingTarget) { CancelTargeting(); return true; }
        if (!CanUse(ConsumableKind.Bomb)) return false;
        Board.SetTargetSelection(PreviewBomb);
        return true;
    }

    private bool PreviewBomb(Gem gem)
    {
        if (!CanUse(ConsumableKind.Bomb) || gem == null || Board.GetGem(gem.Column, gem.Row) != gem) return false;
        BombPreviewCell = new Vector2Int(gem.Column, gem.Row);
        HasBombPreview = true;
        return true;
    }

    public bool ConfirmBomb()
    {
        if (!HasBombPreview || !Board.IsSelectingTarget) return false;
        bool accepted = TryUseBomb(Board.GetGem(BombPreviewCell.x, BombPreviewCell.y));
        if (accepted) HasBombPreview = false;
        return accepted;
    }

    public bool TryUseBomb(Gem gem)
    {
        if(!CanUse(ConsumableKind.Bomb) || gem==null || Board.GetGem(gem.Column,gem.Row)!=gem) return false;
        var action=new RunRecordedAction {kind=RunActionKind.Bomb,x=gem.Column,y=gem.Row};
        return Board.TryClearPlayerArea(gem, BalanceV1.Current.consumableBombRadius, () => CommitUse(ConsumableKind.Bomb,action));
    }
    public void CancelTargeting() { HasBombPreview = false; if (Board != null) Board.SetTargetSelection(null); }
    private bool CommitUse(ConsumableKind kind,RunRecordedAction action)
    {
        bool committed=Continuation==null || Continuation.ExecutingReplayAction
            ? account.TrySpendCharge(RunId,kind) : Continuation.RecordAction(action);
        if(!committed) return false;
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
            if (account.ActiveRun != null) return false;
            RunId = account.BeginRun(CharacterSelectionSettings.SelectedPlayerId,Challenge);
            journalWriteFailed = string.IsNullOrEmpty(RunId);
            return !journalWriteFailed;
        }
        if (pendingWave > 0) { TryRecordPendingWave(); if (journalWriteFailed) return false; }
        if (!IsFinished) return !journalWriteFailed && (Continuation==null || Continuation.SaveNow());
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
        if (scene == "Game") { RunLaunchOptions.Practice = IsPractice; RunLaunchOptions.Challenge = Challenge; }
        CancelTargeting();
        // Scene unload disposes the existing actor/board/ability/attack owners.
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
        return true;
    }
    private void OnApplicationQuit()
    {
        Continuation?.SaveNow();
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
        if (Board != null) Board.UsefulResponseValidator = null;
        practiceProfile?.Dispose(); practiceProfile = null;
        resumedMastery?.Dispose(); resumedCharacter?.Dispose();
    }
}
