using System;
using UnityEngine;

[Serializable]
public sealed class ZoneTestEncounter
{
    public string label;
    public EnemyDefinition[] members;
    public int firstLocalWave=1,lastLocalWave=999;
}

[CreateAssetMenu(menuName="Dungeon Matcher/Zones/Zone")]
public sealed class ZoneDefinition : ScriptableObject
{
    public string zoneId;
    public string displayName;
    public bool eligibleForLiveTravel;
    public GemType affiliatedGem;
    public EnemyDefinition[] enemies;
    public ZoneTestEncounter[] developmentEncounters;
    public ZoneTestEncounter[] liveEncounters;
    public EnemyDefinition apexEnemy;
    [Min(1)] public int apexLocalWave=18;
    [Min(1)] public float affiliatedDamageMultiplier=1.15f;
    public GameplayThemeDefinition theme;
    [Tooltip("Optional zone loop; temporary music remains labelled in its source manifest.")]
    public AudioClip music;
    public bool growsVines;
    public bool crumblesTiles;
    [Min(3)] public int crumbleCadenceMoves = 4;
    [Min(1)] public int vineCadenceMoves = 2;
    [Range(4,64)] public int maximumVineOverlays = 24;
}
