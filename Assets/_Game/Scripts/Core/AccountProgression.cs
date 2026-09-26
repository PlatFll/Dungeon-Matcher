using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ConsumableKind { HealthPotion, Bomb }
public enum RunChallenge { Standard, NoSupplies, BoardOnly }

[Serializable]
public sealed class ChallengeRecord
{
    public RunChallenge challenge;
    public int bestWave;
    public int victories;
}

[Serializable]
public sealed class CharacterProgress
{
    public string id;
    public int level = 1;
}

[Serializable]
public sealed class RunRewardSummary
{
    public string runId;
    public string reason;
    public int waves;
    public int progressGold;
    public int milestoneGold;
    public int bestGold;
    public int firstClearGold;
    public int supplyValue;
    public int Total => progressGold + milestoneGold + bestGold + firstClearGold;
    public int RepeatableGold => progressGold + milestoneGold;
    public string SupplyComparison => $"Used supplies: {supplyValue} gold to replace\nAfter replacement: {Total-supplyValue} gold (no extra fee)";
    public override string ToString() => $"{waves} waves: {progressGold} gold\nMilestones: {milestoneGold}\nNew best: {bestGold}   First clear: {firstClearGold}\nTotal: {Total} gold";
}

[Serializable]
public sealed class AccountRunJournal
{
    public bool supportsContinuation;
    public RunCombatSnapshot checkpoint;
    public RunReplayTape tape = new RunReplayTape();
    public List<GemMasteryReward> mastery = new List<GemMasteryReward>();
    public string id;
    public string playerId;
    public int level;
    public int completedWaves;
    public int milestoneCount;
    public bool kingCleared;
    public int potionCharges;
    public int bombCharges;
    public int initialPotionCharges;
    public int initialBombCharges;
    public int draftWave;
    public List<string> draft = new List<string>();
    public bool refinementUsed;
    public RunChallenge challenge;
}

[Serializable]
public sealed class AccountSave
{
    public int version = 1;
    public int gold;
    public List<CharacterProgress> characters = new List<CharacterProgress>();
    public List<GemSpecialType> unlocked = new List<GemSpecialType> { GemSpecialType.ColorCrystal };
    public int potions;
    public int bombs;
    public bool equipPotions;
    public bool equipBombs;
    public int bestWave;
    public bool firstKingClaimed;
    public AccountRunJournal run;
    public RunRewardSummary lastReward;
    public List<ChallengeRecord> challengeRecords = new List<ChallengeRecord>();
    public List<string> seenLessons = new List<string>();
}

/// <summary>Atomic account transactions: write the complete next snapshot before publishing it.</summary>
public sealed class AccountProgression
{
    private static AccountProgression current;
    private readonly string file;
    private AccountSave state;
    private AccountSave beforeReplay;
    public bool IsPractice { get; private set; }
    public event Action Changed;
    public event Action<GemSpecialType> Unlocked;
    public string LastError { get; private set; }
    public static AccountProgression Current => current ??= new AccountProgression(
        Path.Combine(Application.persistentDataPath, "account-v1.json"), true);

    public int Gold => state.gold;
    public int BestWave => state.bestWave;
    public bool HasClearedKing => state.firstKingClaimed;
    public ChallengeRecord Record(RunChallenge challenge) => Clone(state.challengeRecords.Find(r => r.challenge == challenge)) ?? new ChallengeRecord { challenge = challenge };
    public RunRewardSummary LastReward => Clone(state.lastReward);
    public bool HasSeenLesson(string id) => state.seenLessons != null && state.seenLessons.Contains(id);
    public bool RememberLesson(string id) => HasSeenLesson(id) || Commit(next=>
    { next.seenLessons ??= new List<string>(); next.seenLessons.Add(id); });
    public AccountRunJournal ActiveRun => Clone(state.run);
    public int HighestLevel
    {
        get { int highest = 1; foreach (var item in state.characters) highest = Math.Max(highest, item.level); return highest; }
    }

    // A distinct file permits disposable automated/Play Mode profiles without touching real preferences.
    public AccountProgression(string saveFile, bool migrateLegacy = false)
    {
        file = saveFile;
        state = Load(saveFile) ?? Load(saveFile + ".bak");
        if (state == null)
        {
            if (File.Exists(saveFile) || File.Exists(saveFile + ".bak"))
                throw new IOException("Account save and backup could not be read; originals were preserved.");
            state = new AccountSave();
            if (migrateLegacy) MigrateLegacySelections(state,PlayerPrefs.HasKey,key=>PlayerPrefs.GetInt(key));
        }
        if (state.version != 1) throw new IOException("This account was saved by an unsupported game version.");
        Normalize(state);
        // Older saves have rewards/charges but no combat state. Settle that
        // legacy journal once; current runs survive process interruption.
        if (state.run != null && !state.run.supportsContinuation) FinalizeRun(state.run.id, "Interrupted (legacy run)");
    }

    public static IDisposable UseDisposableProfile(string path)
    {
        var prior = current;
        current = new AccountProgression(path);
        return new RestoreProfile(() => current = prior);
    }

    private sealed class RestoreProfile : IDisposable
    {
        private Action restore;
        public RestoreProfile(Action restore) { this.restore = restore; }
        public void Dispose() { restore?.Invoke(); restore = null; }
    }

    public int Level(string id)
    {
        var entry = state.characters.Find(p => p.id == id);
        return entry != null ? entry.level : 1;
    }

    private AccountProgression(AccountSave practiceState)
    {
        state = Clone(practiceState);
        state.run = null; state.lastReward = null;
        state.potions = state.bombs = 3; state.equipPotions = state.equipBombs = true;
        IsPractice = true;
    }

    public static IDisposable UsePracticeProfile()
    {
        var prior = Current;
        current = new AccountProgression(prior.state);
        return new RestoreProfile(() => current = prior);
    }
    public int Owned(ConsumableKind kind) => kind == ConsumableKind.HealthPotion ? state.potions : state.bombs;
    public bool Equipped(ConsumableKind kind) => kind == ConsumableKind.HealthPotion ? state.equipPotions : state.equipBombs;
    public bool IsUnlocked(GemSpecialType type) => type == GemSpecialType.ColorCrystal ||
        type == GemSpecialType.RowBomb || type == GemSpecialType.ColumnBomb || state.unlocked.Contains(type);
    public bool IsUnlocked(GemMasteryReward reward) => GemMasteryRuntimeResolver.TryGetSpecialType(reward, out var type) && IsUnlocked(type);
    public int Charges(string runId, ConsumableKind kind) => state.run == null || state.run.id != runId ? 0 :
        kind == ConsumableKind.HealthPotion ? state.run.potionCharges : state.run.bombCharges;

    public bool TryLevelUp(string id)
    {
        if (!CharacterSelectionSettings.IsKnownCharacter(id) || state.run != null) return false;
        int level = Level(id), price = BalanceV1.Current.UpgradeCost(level);
        if (level >= BalanceV1.Current.levelCap || Gold < price) return false;
        var previous = new HashSet<GemSpecialType>(state.unlocked);
        if (!Commit(next =>
        {
            var entry = next.characters.Find(p => p.id == id);
            if (entry == null) { entry = new CharacterProgress { id = id }; next.characters.Add(entry); }
            entry.level++;
            next.gold -= price;
            UnlockMilestones(next);
        })) return false;
        foreach (var type in state.unlocked) if (!previous.Contains(type)) Unlocked?.Invoke(type);
        return true;
    }

    public bool TryResetLevel(string id)
    {
        if (!CharacterSelectionSettings.IsKnownCharacter(id) || state.run != null || Level(id) <= 1) return false;
        // Earned account unlocks and paid upgrade costs are retained. A saved
        // run keeps its original level snapshot until explicitly finished.
        return Commit(next => next.characters.Find(p => p.id == id).level = 1);
    }

    public bool TryPurchase(ConsumableKind kind)
    {
        if (!Enum.IsDefined(typeof(ConsumableKind), kind)) return false;
        int price = kind == ConsumableKind.HealthPotion ? BalanceV1.Current.potionPrice : BalanceV1.Current.bombPrice;
        if (state.run != null || price < 1 || Gold < price || Owned(kind) >= 9999) return false;
        return Commit(next => { next.gold -= price; if (kind == ConsumableKind.HealthPotion) next.potions++; else next.bombs++; });
    }

    public bool SetEquipped(ConsumableKind kind, bool equipped)
    {
        if (!Enum.IsDefined(typeof(ConsumableKind), kind)) return false;
        if (state.run != null || (equipped && Owned(kind) < 1)) return false;
        return Commit(next => { if (kind == ConsumableKind.HealthPotion) next.equipPotions = equipped; else next.equipBombs = equipped; });
    }

    public string BeginRun(string playerId, RunChallenge challenge = RunChallenge.Standard)
    {
        if (state.run != null || !CharacterSelectionSettings.IsKnownCharacter(playerId) || !Enum.IsDefined(typeof(RunChallenge), challenge) ||
            (challenge != RunChallenge.Standard && !HasClearedKing)) return null;
        string id = Guid.NewGuid().ToString("N");
        int cap = Math.Min(3, Math.Max(0, BalanceV1.Current.maximumRunCharges));
        return Commit(next => next.run = new AccountRunJournal
        {
            id = id, playerId = playerId, level = Level(playerId), challenge = challenge,
            supportsContinuation=true,
            mastery=new List<GemMasteryReward> { GemMasterySettings.Current.StraightFive,GemMasterySettings.Current.LShape,
                GemMasterySettings.Current.TShape,GemMasterySettings.Current.CrossShape },
            potionCharges = challenge == RunChallenge.Standard && next.equipPotions ? Math.Min(cap, next.potions) : 0,
            bombCharges = challenge == RunChallenge.Standard && next.equipBombs ? Math.Min(cap, next.bombs) : 0,
            initialPotionCharges = challenge == RunChallenge.Standard && next.equipPotions ? Math.Min(cap, next.potions) : 0,
            initialBombCharges = challenge == RunChallenge.Standard && next.equipBombs ? Math.Min(cap, next.bombs) : 0
        }) ? id : null;
    }

    public bool RecordWave(string runId, int wave, bool milestone, bool king)
    {
        if (state.run == null || state.run.id != runId || wave != state.run.completedWaves + 1) return false;
        return Commit(next => { next.run.completedWaves = wave; if (milestone) next.run.milestoneCount++; next.run.kingCleared |= king; });
    }

    public bool StoreDraft(string runId, int wave, List<string> cards, bool refined)
    {
        if (state.run == null || state.run.id != runId || cards == null || cards.Count == 0 ||
            (refined && state.run.refinementUsed)) return false;
        return Commit(next => { next.run.draftWave = wave; next.run.draft = new List<string>(cards); next.run.refinementUsed |= refined; });
    }

    public bool StoreCheckpoint(string runId, RunCombatSnapshot checkpoint)
    {
        if(state.run==null || state.run.id!=runId || checkpoint==null) return false;
        return Commit(next=>
        {
            next.run.checkpoint=checkpoint; next.run.tape=new RunReplayTape();
            next.run.draftWave=checkpoint.draftWave; next.run.draft=new List<string>(checkpoint.draft);
            next.run.refinementUsed=checkpoint.refinementUsed;
        });
    }
    public bool StoreReplayTape(string runId,RunReplayTape tape)
    {
        if(state.run==null || state.run.id!=runId || state.run.checkpoint==null) return false;
        return Commit(next=>next.run.tape=Clone(tape));
    }
    public bool AcceptRecordedAction(string runId,RunReplayTape tape,ConsumableKind? supply)
    {
        if(state.run==null || state.run.id!=runId || state.run.checkpoint==null) return false;
        if(supply.HasValue && (Charges(runId,supply.Value)<=0 || Owned(supply.Value)<=0)) return false;
        return Commit(next=>
        {
            next.run.tape=Clone(tape);
            if(supply==ConsumableKind.HealthPotion)
            { next.potions--; next.run.potionCharges--; if(next.potions==0) next.equipPotions=false; }
            else if(supply==ConsumableKind.Bomb)
            { next.bombs--; next.run.bombCharges--; if(next.bombs==0) next.equipBombs=false; }
        });
    }
    public void BeginContinuationReplay(RunCombatSnapshot saved)
    {
        if(beforeReplay!=null || state.run==null) throw new InvalidOperationException("Cannot start continuation recovery.");
        beforeReplay=state; state=Clone(state);
        // Rebuild pre-action stock in memory. Durable inventory is replaced only
        // after the same accepted actions run through their gameplay owners.
        state.potions+=saved.potionCharges-state.run.potionCharges;
        state.bombs+=saved.bombCharges-state.run.bombCharges;
        state.run.potionCharges=saved.potionCharges; state.run.bombCharges=saved.bombCharges;
        state.run.completedWaves=saved.completedWaves; state.run.milestoneCount=saved.milestones;
        state.run.kingCleared=saved.kingCleared; state.run.draftWave=saved.draftWave;
        state.run.draft=new List<string>(saved.draft); state.run.refinementUsed=saved.refinementUsed;
    }
    public bool EndContinuationReplay(bool commit)
    {
        if(beforeReplay==null) return false;
        var recovered=state; state=beforeReplay; beforeReplay=null;
        if(!commit) { PublishChanged(); return true; }
        return Commit(next=>
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(recovered),next);
            if(recovered.run==null) next.run=null;
            if(recovered.lastReward==null) next.lastReward=null;
        });
    }

    public bool ClearDraft(string runId)
    {
        if(state.run==null || state.run.id!=runId) return false;
        return Commit(next=> { next.run.draftWave=0; next.run.draft.Clear(); });
    }

    public bool TrySpendCharge(string runId, ConsumableKind kind)
    {
        if (!Enum.IsDefined(typeof(ConsumableKind), kind)) return false;
        if (Charges(runId, kind) <= 0 || Owned(kind) <= 0) return false;
        return Commit(next =>
        {
            if (kind == ConsumableKind.HealthPotion) { next.potions--; next.run.potionCharges--; if (next.potions == 0) next.equipPotions = false; }
            else { next.bombs--; next.run.bombCharges--; if (next.bombs == 0) next.equipBombs = false; }
        });
    }

    public RunRewardSummary PreviewReward(string reason)
    {
        if (state.run == null) return LastReward;
        var r = state.run;
        var balance = BalanceV1.Current;
        var summary = new RunRewardSummary { runId = r.id, reason = reason, waves = r.completedWaves };
        if (IsPractice) { summary.reason = "Practice - no gold or progression"; return summary; }
        for (int wave = 1; wave <= r.completedWaves; wave++) summary.progressGold += balance.WaveGold(wave);
        summary.milestoneGold = r.milestoneCount * balance.milestoneGold + (r.kingCleared ? balance.kingGold : 0);
        summary.bestGold = Math.Max(0, r.completedWaves - state.bestWave) * balance.bestWaveGold;
        summary.firstClearGold = r.kingCleared && !state.firstKingClaimed ? balance.firstKingGold : 0;
        summary.supplyValue = Math.Max(0,r.initialPotionCharges-r.potionCharges)*balance.potionPrice +
            Math.Max(0,r.initialBombCharges-r.bombCharges)*balance.bombPrice;
        return summary;
    }

    public bool FinalizeRun(string runId, string reason)
    {
        if (state.run == null || state.run.id != runId) return false;
        var summary = PreviewReward(reason);
        return Commit(next =>
        {
            next.gold = (int)Math.Min(int.MaxValue, (long)next.gold + summary.Total);
            next.bestWave = Math.Max(next.bestWave, next.run.completedWaves);
            next.firstKingClaimed |= next.run.kingCleared;
            next.lastReward = summary;
            if (!IsPractice && next.run.challenge != RunChallenge.Standard)
            {
                var record = next.challengeRecords.Find(r => r.challenge == next.run.challenge);
                if (record == null) { record = new ChallengeRecord { challenge = next.run.challenge }; next.challengeRecords.Add(record); }
                record.bestWave = Math.Max(record.bestWave, next.run.completedWaves);
                if (next.run.kingCleared) record.victories++;
            }
            next.run = null;
        });
    }

    private bool Commit(Action<AccountSave> change)
    {
        var next = Clone(state);
        change(next);
        if (IsPractice || beforeReplay!=null) { state = next; PublishChanged(); return true; }
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file)));
            string temporary = file + ".tmp";
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(JsonUtility.ToJson(next, true)); writer.Flush(); stream.Flush(true);
            }
            if (File.Exists(file)) File.Replace(temporary, file, file + ".bak");
            else File.Move(temporary, file);
            state = next;
            LastError = null;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            LastError = "Could not save account. No account changes were committed.";
            Debug.LogError(LastError + " " + exception.Message);
            return false;
        }
        PublishChanged();
        return true;
    }

    private void PublishChanged()
    {
        // Presentation callbacks cannot turn a durable success into a rejected use.
        if (Changed != null) foreach (Action callback in Changed.GetInvocationList())
            try { callback(); } catch (Exception exception) { Debug.LogException(exception); }
    }

    private static T Clone<T>(T value) where T : class
    {
        if (value == null) return null;
        var copy = JsonUtility.FromJson<T>(JsonUtility.ToJson(value));
        // Unity serializes null inline classes as empty value objects. Preserve
        // the absent-journal meaning both in transaction snapshots and on load.
        if (value is AccountSave original && copy is AccountSave account)
        {
            if (original.run == null) account.run = null;
            if (original.lastReward == null) account.lastReward = null;
        }
        return copy;
    }
    private static AccountSave Load(string path)
    {
        if (!File.Exists(path)) return null;
        try { return JsonUtility.FromJson<AccountSave>(File.ReadAllText(path)); }
        catch (Exception exception) when (exception is IOException || exception is ArgumentException) { return null; }
    }
    private static void Normalize(AccountSave save)
    {
        save.characters ??= new List<CharacterProgress>();
        save.unlocked ??= new List<GemSpecialType>();
        save.challengeRecords ??= new List<ChallengeRecord>();
        save.gold = Math.Max(0, save.gold);
        save.potions = Mathf.Clamp(save.potions, 0, 9999);
        save.bombs = Mathf.Clamp(save.bombs, 0, 9999);
        save.characters.RemoveAll(p => p == null || !CharacterSelectionSettings.IsKnownCharacter(p.id));
        var known = new HashSet<string>();
        save.characters.RemoveAll(p => !known.Add(p.id));
        save.unlocked.RemoveAll(t => BalanceV1.Current.UnlockLevel(t) == int.MaxValue);
        save.bestWave = Math.Max(0, save.bestWave);
        if (save.run != null && string.IsNullOrEmpty(save.run.id)) save.run = null;
        if (save.lastReward != null && string.IsNullOrEmpty(save.lastReward.runId)) save.lastReward = null;
        if (save.potions == 0) save.equipPotions = false;
        if (save.bombs == 0) save.equipBombs = false;
        if (save.run != null)
        {
            save.run.completedWaves = Mathf.Clamp(save.run.completedWaves, 0, 10000);
            save.run.milestoneCount = Mathf.Clamp(save.run.milestoneCount, 0, 4);
            save.run.potionCharges = Mathf.Clamp(save.run.potionCharges, 0, Math.Min(3, save.potions));
            save.run.bombCharges = Mathf.Clamp(save.run.bombCharges, 0, Math.Min(3, save.bombs));
        }
        foreach (var p in save.characters) p.level = Mathf.Clamp(p.level, 1, BalanceV1.Current.levelCap);
        UnlockMilestones(save);
    }
    private static void UnlockMilestones(AccountSave save)
    {
        int highest = 1;
        foreach (var p in save.characters) highest = Math.Max(highest, p.level);
        foreach (GemSpecialType type in Enum.GetValues(typeof(GemSpecialType)))
            if (BalanceV1.Current.UnlockLevel(type) <= highest && !save.unlocked.Contains(type)) save.unlocked.Add(type);
    }
    private static void MigrateLegacySelections(AccountSave save,Func<string,bool> hasKey,Func<string,int> readValue)
    {
        // Explicit saved selections are evidence of ownership. Do not erase or rewrite legacy keys.
        foreach (GemMasteryShape shape in Enum.GetValues(typeof(GemMasteryShape)))
        {
            string key = "DungeonMatcher.GemMastery.v1." + shape;
            if (hasKey(key) && GemMasteryRuntimeResolver.TryGetSpecialType((GemMasteryReward)readValue(key), out var type)
                && !save.unlocked.Contains(type)) save.unlocked.Add(type);
        }
    }
}
