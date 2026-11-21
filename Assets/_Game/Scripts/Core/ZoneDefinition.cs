using System;
using UnityEngine;

public enum OrdinaryRole { Relief, Standard, Pressure, Introduction }
public enum EncounterPhase { Early, Mid, Late }

public enum ZoneEncounterKind { Ordinary, Elite, Miniboss, MajorBoss }
[Serializable]
public sealed class ZoneDepthMember
{
    public EnemyDefinition enemy;
    public int minimumTier, maximumTier = 4, family;
    public float[] tierWeights = { 1, 1, 1, 1, 1 };
    public bool summonOnly;
    public float Weight(int tier) => tier >= minimumTier && tier <= maximumTier && !summonOnly &&
        tierWeights != null && tier < tierWeights.Length ? Mathf.Max(0, tierWeights[tier]) : 0;
}

[Serializable]
public sealed class ZoneTestEncounter
{
    public string label;
    [Tooltip("Eligible complexity bands: Early 1, Mid 2, Late 4. Combine bits for reusable formations.")]
    public int phaseMask = 7;
    public OrdinaryRole ordinaryRole = OrdinaryRole.Standard;
    public string formationFamily;
    [Min(1)] public int familyWeight = 3;
    public int minimumDepthTier, maximumDepthTier = 4;
    public ZoneEncounterKind resonanceKind;
    public float resonanceThreatBudget = 12;
    public bool oncePerRun, allowRepeatedRequiredEscort;
    public string[] prerequisites = Array.Empty<string>();
    public EnemyDefinition[] members;
    public int firstLocalWave=1,lastLocalWave=999;
    [Min(1)] public int weight=1;
    // 0 either, 1 dry only, 2 flooded only. Whole-cast eligibility still applies.
    public int requiredTide;
    [NonSerialized] public string selectedPhase, selectedRole;
    public ZoneTestEncounter WithSelection(string phase, string role)
    {
        var copy = (ZoneTestEncounter)MemberwiseClone();
        copy.selectedPhase = phase;
        copy.selectedRole = role;
        return copy;
    }
    // Optional teaching metadata for a zone's weighted encounter library.
    public EnemyDefinition introduction;
    public int introduceByLocalWave;
    public EnemyDefinition[] requiredSeen = Array.Empty<EnemyDefinition>();
    public bool oncePerVisit;
}

[CreateAssetMenu(menuName="Dungeon Matcher/Zones/Zone")]
public sealed class ZoneDefinition : ScriptableObject
{
    public string zoneId;
    public string displayName;
    public bool eligibleForLiveTravel;
    [Tooltip("Temporary testing picker only; does not enable crystal travel into unfinished zones.")]
    public bool eligibleForTesting;
    public GemType affiliatedGem;
    public EnemyDefinition[] enemies;
    public ZoneTestEncounter[] developmentEncounters;
    public ZoneTestEncounter[] liveEncounters;
    [Header("Crystal Resonance global-depth content (legacy fixtures remain separate)")]
    [Tooltip("Fixed zone cadence overrides. Zero inherits shared awards; never derived from player power or time.")]
    [Min(0)] public int resonanceNormalChargeOverride, resonancePressureChargeOverride;
    public int[] resonanceTierStarts = { 1, 6, 13, 25, 45 };
    public float[] resonanceThreatBudgets = { 4, 6, 8, 11, 12 };
    public ZoneDepthMember[] resonancePool = Array.Empty<ZoneDepthMember>();
    public ZoneTestEncounter[] resonanceEncounters = Array.Empty<ZoneTestEncounter>();
    public int ResonanceTier(int depth)
    {
        int tier = 0;
        for (int i = 1; i < resonanceTierStarts.Length; i++) if (depth >= resonanceTierStarts[i]) tier = i;
        return Mathf.Clamp(tier, 0, 4);
    }
    public WaveSpawnProfile encounterBudget;
    public EnemyDefinition apexEnemy;
    [Min(1)] public int apexLocalWave=18;
    [Min(1)] public float affiliatedDamageMultiplier=1.15f;
    public GameplayThemeDefinition theme;
    [Tooltip("Optional zone loop; temporary music remains labelled in its source manifest.")]
    public AudioClip music;
    public bool growsVines;
    public bool periodicallyFloods;
    [Header("Flood oxygen — provisional tuning")]
    [Min(1)] public int floodMinimumMoves=16, floodMaximumMoves=18;
    [Range(1,8)] public int initialAirBubbles=5;
    [Min(1)] public int emergencyAirCadenceMoves=3;
    [Range(1,2)] public int emergencyAirSupply=1,criticalAirSupply=2;
    [Range(0,4)] public int criticalAirThreshold=1;
    public bool crumblesTiles;
    [Header("Frostwake foundation (requires a separately ready zone)")]
    public FrostwakeRules frostwakeRules;
    [Header("Ironvein — provisional stone tuning")]
    public bool maturesStone;
    [Min(1)] public int mineMovesPerStage = 3;
    [Range(1,6)] public int maximumMineStones = 6;
    [Min(1)] public int mineDrillCapacity = 4;
    [Min(3)] public int crumbleCadenceMoves = 4;
    [Min(1)] public int vineCadenceMoves = 2;
    [Range(1,4)] public int maximumVineSpreadPerPulse = 2;
    [Range(4,64)] public int maximumVineOverlays = 12;
}
