using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ConsumableKind { HealthPotion, Bomb }

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
    public int Total => progressGold + milestoneGold + bestGold + firstClearGold;
    public override string ToString() => $"{waves} waves: {progressGold} gold\nMilestones: {milestoneGold}\nNew best: {bestGold}   First clear: {firstClearGold}\nTotal: {Total} gold";
}

[Serializable]
public sealed class AccountRunJournal
{
    public string id;
    public string playerId;
    public int level;
    public int completedWaves;
    public int milestoneCount;
    public bool kingCleared;
    public int potionCharges;
    public int bombCharges;
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
}

/// <summary>Atomic account transactions: write the complete next snapshot before publishing it.</summary>
public sealed class AccountProgression
{
    private static AccountProgression current;
    private readonly string file;
    private AccountSave state;
    public event Action Changed;
    public event Action<GemSpecialType> Unlocked;
    public string LastError { get; private set; }
    public static AccountProgression Current => current ??= new AccountProgression(
        Path.Combine(Application.persistentDataPath, "account-v1.json"), true);

    public int Gold => state.gold;
    public int BestWave => state.bestWave;
    public RunRewardSummary LastReward => Clone(state.lastReward);
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
        // The journal only contains completed waves. Interrupted partial waves have no payout.
        if (state.run != null) FinalizeRun(state.run.id, "Interrupted");
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
    public int Owned(ConsumableKind kind) => kind == ConsumableKind.HealthPotion ? state.potions : state.bombs;
    public bool Equipped(ConsumableKind kind) => kind == ConsumableKind.HealthPotion ? state.equipPotions : state.equipBombs;
    public bool IsUnlocked(GemSpecialType type) => type == GemSpecialType.ColorCrystal || state.unlocked.Contains(type);
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

    public string BeginRun(string playerId)
    {
        if (state.run != null || !CharacterSelectionSettings.IsKnownCharacter(playerId)) return null;
        string id = Guid.NewGuid().ToString("N");
        int cap = Math.Min(3, Math.Max(0, BalanceV1.Current.maximumRunCharges));
        return Commit(next => next.run = new AccountRunJournal
        {
            id = id, playerId = playerId, level = Level(playerId),
            potionCharges = next.equipPotions ? Math.Min(cap, next.potions) : 0,
            bombCharges = next.equipBombs ? Math.Min(cap, next.bombs) : 0
        }) ? id : null;
    }

    public bool RecordWave(string runId, int wave, bool milestone, bool king)
    {
        if (state.run == null || state.run.id != runId || wave != state.run.completedWaves + 1) return false;
        return Commit(next => { next.run.completedWaves = wave; if (milestone) next.run.milestoneCount++; next.run.kingCleared |= king; });
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
        for (int wave = 1; wave <= r.completedWaves; wave++) summary.progressGold += balance.WaveGold(wave);
        summary.milestoneGold = r.milestoneCount * balance.milestoneGold + (r.kingCleared ? balance.kingGold : 0);
        summary.bestGold = Math.Max(0, r.completedWaves - state.bestWave) * balance.bestWaveGold;
        summary.firstClearGold = r.kingCleared && !state.firstKingClaimed ? balance.firstKingGold : 0;
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
            next.run = null;
        });
    }

    private bool Commit(Action<AccountSave> change)
    {
        var next = Clone(state);
        change(next);
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
            LastError = "Could not save account. No purchase, upgrade or item use was committed.";
            Debug.LogError(LastError + " " + exception.Message);
            return false;
        }
        // Presentation callbacks cannot turn a durable success into a rejected use.
        if (Changed != null) foreach (Action callback in Changed.GetInvocationList())
            try { callback(); } catch (Exception exception) { Debug.LogException(exception); }
        return true;
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
