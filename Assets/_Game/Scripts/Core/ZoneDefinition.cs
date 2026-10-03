using System;
using UnityEngine;

[Serializable]
public sealed class ZoneTestEncounter
{
    public string label;
    public EnemyDefinition[] members;
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
    public GameplayThemeDefinition theme;
    [Tooltip("Optional zone loop; temporary music remains labelled in its source manifest.")]
    public AudioClip music;
}
