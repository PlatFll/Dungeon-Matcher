using System;
using System.Collections.Generic;
using UnityEngine;

public enum ResonanceMastery { None = 0, DeliberateMatch = 1, SpecialSetup = 2, TacticalCounter = 4 }
public enum ResonanceCapstonePhase { Unselected, Scheduled, InProgress, Defeated }

[Serializable]
public sealed class CrystalResonanceTuning
{
    public int capacity = 100;
    public int[] milestones = { 12, 40, 70 };
    public int normalCharge = 4, pressureCharge = 5, minibossCharge = 10, bossCharge = 14;
    public int approachMinimum = 78, approachMaximum = 88;
    public int matchGemScore = 10, specialGemScore = 15, masteryScore = 50, victoryScore = 100;
    public int maximumBoardScorePerEncounter = 600;
    public int BaseCharge(EnemyCategory category, ZoneDefinition zone = null)
    {
        if (category == EnemyCategory.Boss) return bossCharge;
        if (category == EnemyCategory.Miniboss) return minibossCharge;
        if (category == EnemyCategory.Special)
            return zone != null && zone.resonancePressureChargeOverride > 0 ? zone.resonancePressureChargeOverride : pressureCharge;
        return zone != null && zone.resonanceNormalChargeOverride > 0 ? zone.resonanceNormalChargeOverride : normalCharge;
    }
    private static CrystalResonanceTuning current;
    public static CrystalResonanceTuning Current => current ??= Load();
    private static CrystalResonanceTuning Load()
    {
        var json = Resources.Load<TextAsset>("Balance/CrystalResonance");
        return json != null ? JsonUtility.FromJson<CrystalResonanceTuning>(json.text) : new CrystalResonanceTuning();
    }
}

/// <summary>Version zero is an absent legacy object, not an opted-in run.</summary>
[Serializable]
public sealed class CrystalResonanceSnapshot
{
    public int version;
    public long score, lastReceipt;
    public int charge, masteryMask, boardScore, lastBankedWave, activeWave, pendingWave, visit;
    public string encounterId, zoneId;
    public bool active, migratedLegacy;
    public int triggeredMask, claimedMask, activeNotch;
    public List<int> pendingNotches = new List<int>();
    public uint capstoneRandom;
    public int approachThreshold;
    public ResonanceCapstonePhase capstonePhase;
    public string capstoneId;
    public uint formationSeed;
    public List<string> capstoneMembers = new List<string>();
    public List<string> seenLeaders = new List<string>();
    // Additive v1 save fields. Older snapshots begin with an empty visit history.
    public List<EncounterHistoryEntry> formationHistory = new List<EncounterHistoryEntry>();
    public List<string> taughtZones = new List<string>();
    public List<string> recentRecipes = new List<string>();
    public List<string> seenMechanics = new List<string>();
    public int capstoneWave, capstoneKind, lastMajorVisit = -10;
    public string capstoneEncounterId;
    public List<string> recentCapstones = new List<string>();
    public float visitSeconds, combatSeconds, draftSeconds, travelSeconds;
    public int encountersThisVisit, masteryThisVisit, movesThisVisit, damageThisVisit;
    public float runSeconds, pausedSeconds;
    public int PendingMastery => CountBits(masteryMask);
    public static int CountBits(int bits) => ((bits & 1) != 0 ? 1 : 0) + ((bits & 2) != 0 ? 1 : 0) + ((bits & 4) != 0 ? 1 : 0);
    public CrystalResonanceSnapshot Copy() => JsonUtility.FromJson<CrystalResonanceSnapshot>(JsonUtility.ToJson(this));

    public static uint Seed(string runId)
    {
        uint hash = 2166136261;
        foreach (char c in runId ?? "") hash = unchecked((hash ^ c) * 16777619);
        return hash ^ 0x43a9b271u;
    }
    public void StartVisit(string zone, int visitIndex, CrystalResonanceTuning tuning)
    {
        zoneId = zone; visit = visitIndex; charge = masteryMask = boardScore = 0;
        triggeredMask = claimedMask = activeNotch = pendingWave = 0;
        active = false; pendingNotches.Clear(); recentRecipes.Clear();
        formationHistory?.Clear();
        capstonePhase = ResonanceCapstonePhase.Unselected; capstoneId = null;
        capstoneMembers.Clear(); capstoneWave = capstoneKind = 0; formationSeed = 0; capstoneEncounterId = null;
        var random = new SavedRandom(capstoneRandom);
        approachThreshold = random.Next(tuning.approachMinimum, tuning.approachMaximum + 1);
        capstoneRandom = random.State;
        visitSeconds = combatSeconds = draftSeconds = travelSeconds = 0;
        encountersThisVisit = masteryThisVisit = movesThisVisit = damageThisVisit = 0;
    }
    public void BeginEncounter(string runId, int wave)
    {
        if (activeWave == wave && active) return;
        activeWave = wave; encounterId = runId + ":" + visit + ":" + wave;
        active = true; masteryMask = boardScore = pendingWave = 0;
    }
    public bool Receive(BoardProgressionReceipt receipt, CrystalResonanceTuning tuning, out ResonanceMastery earned)
    {
        earned = ResonanceMastery.None;
        if (!active || activeWave <= lastBankedWave || receipt.Sequence <= lastReceipt) return false;
        lastReceipt = receipt.Sequence;
        int points = Math.Min(Math.Max(0, tuning.maximumBoardScorePerEncounter - boardScore),
            Math.Max(0, receipt.GemCount) * (receipt.Special ? tuning.specialGemScore : tuning.matchGemScore));
        boardScore += points; score += points;
        int bit = (int)receipt.Mastery & 7;
        int fresh = bit & ~masteryMask;
        if (fresh != 0) { masteryMask |= fresh; earned = (ResonanceMastery)fresh; score += tuning.masteryScore * CountBits(fresh); }
        return true;
    }
    public bool Bank(int wave, int baseCharge, CrystalResonanceTuning tuning)
    {
        if (!active || wave != activeWave || wave <= lastBankedWave) return false;
        int mastery = PendingMastery;
        charge = Math.Min(tuning.capacity, charge + Math.Max(0, baseCharge) + mastery);
        score += tuning.victoryScore; lastBankedWave = wave; pendingWave = 0;
        active = false; masteryMask = 0; encountersThisVisit++; masteryThisVisit += mastery;
        for (int i = 0; i < tuning.milestones.Length; i++)
            if (charge >= tuning.milestones[i] && (triggeredMask & (1 << i)) == 0)
            { triggeredMask |= 1 << i; pendingNotches.Add(tuning.milestones[i]); }
        return true;
    }
    public static bool Supports(CrystalResonanceSnapshot value, RunCombatSnapshot combat)
    {
        if (value == null || value.version == 0) return true; // Explicit legacy migration, never inferred charge.
        if (value.version != 1 || value.score < 0 || value.lastReceipt < 0 || value.charge < 0 || value.charge > 100 ||
            value.masteryMask < 0 || value.masteryMask > 7 || value.boardScore < 0 || value.visit < 0 ||
            value.lastBankedWave < 0 || value.lastBankedWave > combat.wave || value.activeWave < 0 ||
            value.approachThreshold < 78 || value.approachThreshold > 88 || value.capstoneRandom == 0 ||
            value.pendingNotches == null || value.seenLeaders == null || value.recentRecipes == null ||
            value.seenMechanics == null || value.recentCapstones == null || value.capstoneMembers == null ||
            value.triggeredMask < 0 || value.triggeredMask > 7 || value.claimedMask < 0 ||
            (value.claimedMask & ~value.triggeredMask) != 0 ||
            combat.travel?.version != 1 || value.visit != combat.travel.visit || value.zoneId != combat.travel.zoneId) return false;
        if (value.active && (value.activeWave != combat.wave || value.activeWave <= value.lastBankedWave ||
            string.IsNullOrEmpty(value.encounterId))) return false;
        if (value.pendingWave != 0 && (!value.active || value.pendingWave != value.activeWave)) return false;
        int previous = 0, queuedMask = 0;
        int[] notches = { 12, 40, 70 };
        foreach (int notch in value.pendingNotches)
        {
            int index = Array.IndexOf(notches, notch);
            if (index < 0 || notch <= previous || value.charge < notch) return false;
            previous = notch; queuedMask |= 1 << index;
        }
        if (queuedMask != (value.triggeredMask & ~value.claimedMask) ||
            (value.activeNotch != 0 && (value.pendingNotches.Count == 0 || value.activeNotch != value.pendingNotches[0]))) return false;
        if (!Enum.IsDefined(typeof(ResonanceCapstonePhase), value.capstonePhase)) return false;
        bool selected = value.capstonePhase != ResonanceCapstonePhase.Unselected;
        bool legacyTrip = value.migratedLegacy && value.capstonePhase == ResonanceCapstonePhase.Defeated && combat.travel.stage == 1;
        if (selected && !legacyTrip && (string.IsNullOrEmpty(value.capstoneId) || string.IsNullOrEmpty(value.capstoneEncounterId) ||
            value.capstoneMembers.Count < 1 || value.capstoneMembers.Count > 3 || value.formationSeed == 0 ||
            value.capstoneWave <= 0 || value.capstoneKind < 1 || value.capstoneKind > 3)) return false;
        if (value.capstonePhase == ResonanceCapstonePhase.Scheduled && value.capstoneWave != combat.wave + 1) return false;
        if (value.capstonePhase == ResonanceCapstonePhase.InProgress &&
            (value.capstoneWave != combat.wave || !value.active || value.capstoneEncounterId != value.encounterId)) return false;
        foreach (float seconds in new[] { value.visitSeconds, value.combatSeconds, value.draftSeconds, value.travelSeconds, value.runSeconds, value.pausedSeconds })
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0) return false;
        return true;
    }
    public void DiscardPending() { active = false; masteryMask = pendingWave = 0; }
}
