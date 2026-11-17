using System;
using UnityEngine;

/// <summary>Owns score/charge receipts and settlement, not combat or encounter construction.</summary>
[DisallowMultipleComponent]
public sealed class CrystalResonanceRuntime : MonoBehaviour, IWaveProgressionGate
{
    public CrystalResonanceSnapshot State { get; private set; }
    public CrystalResonanceTuning Tuning => CrystalResonanceTuning.Current;
    public bool Enabled => State?.version == 1;
    public long RunScore => State?.score ?? 0;
    public int Committed => State?.charge ?? 0;
    public int Pending => State?.PendingMastery ?? 0;
    public bool HasPendingDrafts => Enabled && (State.pendingNotches.Count > 0 || State.activeNotch != 0);
    public bool IsSavePending { get; private set; }
    public bool IsBlockingWaveProgression => Enabled && (State.pendingWave > 0 || HasPendingDrafts || IsSavePending || PortalReady);
    public bool PortalReady => Enabled && Committed >= Tuning.capacity &&
        State.capstonePhase == ResonanceCapstonePhase.Defeated && !HasPendingDrafts && State.pendingWave == 0 && !State.active && !IsSavePending;
    public bool Sealed => Enabled && Committed >= Tuning.capacity && State.capstonePhase != ResonanceCapstonePhase.Defeated;
    public event Action CapstoneApproaching;
    public event Action Changed;
    public event Action<ResonanceMastery> MasteryEarned;
    public event Action<int> EncounterBanked;
    private RunSession run;

    public void Initialize(RunSession owner, RunCombatSnapshot saved)
    {
        run = owner;
        // Isolated historical fixtures preserve their old progression contract.
        if (run.Travel?.State.enabled != true) return;
        if (saved?.resonance?.version == 1) State = saved.resonance.Copy();
        else
        {
            State = new CrystalResonanceSnapshot { version = 1, capstoneRandom = CrystalResonanceSnapshot.Seed(run.RunId),
                lastBankedWave = saved == null ? 0 : (saved.waveActive ? Math.Max(0, saved.wave - 1) : saved.wave), migratedLegacy = saved != null };
            State.StartVisit(run.Zone.Definition.zoneId, run.Travel.State.visit, Tuning);
            // A durable legacy trip has already been earned. Finish it, then reset at arrival.
            if (saved?.travel?.stage == 1) { State.charge = Tuning.capacity; State.capstonePhase = ResonanceCapstonePhase.Defeated; }
            if (saved?.waveActive == true) State.BeginEncounter(run.RunId, saved.wave);
            if (saved?.seenEnemies != null)
                foreach (var zone in Resources.LoadAll<ZoneDefinition>("Zones"))
                    foreach (var enemy in zone.enemies)
                        if (enemy != null && saved.seenEnemies.Contains(enemy.name))
                        {
                            if (!State.seenMechanics.Contains(enemy.EnemyId)) State.seenMechanics.Add(enemy.EnemyId);
                            if ((enemy.Category == EnemyCategory.Miniboss || enemy.Category == EnemyCategory.Boss) &&
                                !State.seenLeaders.Contains(enemy.EnemyId)) State.seenLeaders.Add(enemy.EnemyId);
                        }
        }
        run.Board.RestoreProgressionReceiptSequence(State.lastReceipt);
        run.Board.ProgressionReceipt += Receive;
        run.Waves.WaveCompleted += WaveEnded;
        run.Waves.RegisterProgressionGate(this);
        run.Player.Defeated += Defeated;
    }
    public void BeginEncounter(int wave)
    {
        if (!Enabled) return;
        State.BeginEncounter(run.RunId, wave); Changed?.Invoke();
    }
    private void Receive(BoardProgressionReceipt receipt)
    {
        if (!Enabled || run.IsFinished || run.Player.IsDefeated) return;
        if (!State.Receive(receipt, Tuning, out var earned)) return;
        if (earned != ResonanceMastery.None) MasteryEarned?.Invoke(earned);
        Changed?.Invoke();
    }
    private void WaveEnded(int wave)
    {
        if (Enabled && State.active && wave == State.activeWave && wave > State.lastBankedWave)
            State.pendingWave = wave; // Synchronous gate prevents next-wave leakage.
    }
    private void Update()
    {
        if (!Enabled || run.IsFinished || run.Player.IsDefeated || Time.timeScale <= 0 ||
            run.Continuation?.IsRestoring == true) return;
        if (IsSavePending)
        {
            if (run.Continuation?.CanCapture == true && run.Continuation.SaveNow()) IsSavePending = false;
            else return;
        }
        if (State.pendingWave == 0 || run.Waves.IsWaveActive || run.Board.IsBusy ||
            !run.Waves.CanCaptureContinuation || run.MoveClock?.IsBlockingWaveProgression == true || run.NeedsSaveRetry) return;
        int wave = State.pendingWave;
        int charge = Tuning.normalCharge;
        foreach (var enemy in run.Waves.OriginalEncounterDefinitions)
        {
            if (enemy == null) continue;
            charge = Math.Max(charge, enemy.Category == EnemyCategory.Boss ? Tuning.bossCharge :
                enemy.Category == EnemyCategory.Miniboss ? Tuning.minibossCharge :
                enemy.Category == EnemyCategory.Special ? Tuning.pressureCharge : Tuning.normalCharge);
        }
        if (!State.Bank(wave, charge, Tuning)) return;
        if (State.capstonePhase == ResonanceCapstonePhase.InProgress && State.capstoneWave == wave &&
            State.encounterId == State.capstoneEncounterId)
            State.capstonePhase = ResonanceCapstonePhase.Defeated;
        ScheduleCapstone(wave + 1);
        EncounterBanked?.Invoke(wave); Changed?.Invoke();
        IsSavePending = run.Continuation != null;
        if (IsSavePending && run.Continuation.CanCapture && run.Continuation.SaveNow()) IsSavePending = false;
    }
    private void ScheduleCapstone(int nextWave)
    {
        if (State.capstonePhase != ResonanceCapstonePhase.Unselected || Committed < State.approachThreshold) return;
        // Only this independent stream selects capstones; no board/card/destination RNG is consumed.
        var random = new SavedRandom(State.capstoneRandom);
        State.formationSeed = (uint)random.Next(1, int.MaxValue);
        State.capstoneRandom = random.State;
        var recipe = WaveController.ChooseResonanceCapstone(run.Zone.Definition, nextWave, State, new SavedRandom(State.formationSeed));
        State.capstoneId = recipe.label; State.capstoneKind = (int)recipe.resonanceKind;
        State.capstoneMembers.Clear(); foreach (var member in recipe.members) State.capstoneMembers.Add(member.EnemyId);
        State.capstoneWave = nextWave; State.capstoneEncounterId = run.RunId + ":" + State.visit + ":" + nextWave;
        State.capstonePhase = ResonanceCapstonePhase.Scheduled;
        string key = State.zoneId + ":" + recipe.label;
        State.recentCapstones.Add(key); while (State.recentCapstones.Count > 4) State.recentCapstones.RemoveAt(0);
        if (recipe.resonanceKind == ZoneEncounterKind.MajorBoss) State.lastMajorVisit = State.visit;
        CapstoneApproaching?.Invoke();
    }
    public int BeginNextDraft()
    {
        if (!Enabled || State.pendingNotches.Count == 0) return 0;
        State.activeNotch = State.pendingNotches[0]; return State.activeNotch;
    }
    public bool CompleteDraft(int notch)
    {
        if (!Enabled || notch == 0 || State.activeNotch != notch ||
            State.pendingNotches.Count == 0 || State.pendingNotches[0] != notch) return false;
        int index = Array.IndexOf(Tuning.milestones, notch);
        if (index < 0) return false;
        State.claimedMask |= 1 << index; State.pendingNotches.RemoveAt(0); State.activeNotch = 0;
        IsSavePending = run.Continuation != null; Changed?.Invoke(); return true;
    }
    public CrystalResonanceSnapshot Capture() => Enabled ? State.Copy() : null;
    public void Restore(CrystalResonanceSnapshot saved)
    {
        if (saved?.version != 1) return;
        State = saved.Copy(); IsSavePending = false; run.Board.RestoreProgressionReceiptSequence(State.lastReceipt); Changed?.Invoke();
    }
    private void Defeated(PlayerActor player)
    { if (Enabled) { State.DiscardPending(); Changed?.Invoke(); } }
    private void OnDestroy()
    {
        if (run == null) return;
        if (run.Board != null) run.Board.ProgressionReceipt -= Receive;
        if (run.Player != null) run.Player.Defeated -= Defeated;
        if (run.Waves != null) { run.Waves.WaveCompleted -= WaveEnded; run.Waves.UnregisterProgressionGate(this); }
    }
}
