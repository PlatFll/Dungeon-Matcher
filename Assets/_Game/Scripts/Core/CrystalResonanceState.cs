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
    public int normalCharge = 5, pressureCharge = 6, minibossCharge = 10, bossCharge = 14;
    public int approachMinimum = 78, approachMaximum = 88;
    public int matchGemScore = 10, specialGemScore = 15, masteryScore = 50, victoryScore = 100;
    public int maximumBoardScorePerEncounter = 600;
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
    public List<string> recentRecipes = new List<string>();
    public List<string> seenMechanics = new List<string>();
    public int capstoneWave;
    public float visitSeconds, combatSeconds, draftSeconds, travelSeconds;
    public int encountersThisVisit, masteryThisVisit;
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
        capstonePhase = ResonanceCapstonePhase.Unselected; capstoneId = null;
        capstoneMembers.Clear(); capstoneWave = 0; formationSeed = 0;
        var random = new SavedRandom(capstoneRandom);
        approachThreshold = random.Next(tuning.approachMinimum, tuning.approachMaximum + 1);
        capstoneRandom = random.State;
        visitSeconds = combatSeconds = draftSeconds = travelSeconds = 0;
        encountersThisVisit = masteryThisVisit = 0;
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
    public void DiscardPending() { active = false; masteryMask = pendingWave = 0; }
}
