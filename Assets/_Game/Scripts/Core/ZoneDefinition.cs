using System;
using UnityEngine;

[Serializable]
public sealed class ZoneTestEncounter
{
    public string label;
    public EnemyDefinition[] members;
    public int firstLocalWave=1,lastLocalWave=999;
    [Min(1)] public int weight=1;
    // 0 either, 1 dry only, 2 flooded only. Whole-cast eligibility still applies.
    public int requiredTide;
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
