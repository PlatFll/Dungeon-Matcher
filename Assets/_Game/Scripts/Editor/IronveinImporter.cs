using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Writes only additive Ironvein assets. Never rebuilds another zone.</summary>
public static class IronveinImporter
{
    public const string ZonePath = "Assets/_Game/Resources/Zones/ironvein-excavation.asset";
    [MenuItem("Dungeon Matcher/Ironvein/Import foundation stub")]
    public static void ImportFoundation()
    {
        EditorUtility.audioMasterMute = true;
        var zone = Load<ZoneDefinition>(ZonePath);
        var theme = Load<GameplayThemeDefinition>("Assets/_Game/Resources/Zones/IronveinTheme.asset");
        // Explicit development placeholder. Native cave composition is Phase 09.
        if (theme.battleEnvironment == null)
            theme.battleEnvironment = Resources.Load<GameObject>("BattleEnvironments/Dungeon_Finalized");
        theme.minimumBattleHeight = 384;
        zone.zoneId = "ironvein-excavation"; zone.displayName = "Ironvein (prototype)";
        zone.eligibleForTesting = true; zone.eligibleForLiveTravel = false;
        zone.affiliatedGem = GemType.Amber; zone.affiliatedDamageMultiplier = 1.15f;
        zone.maturesStone = true; zone.mineMovesPerStage = 3; zone.maximumMineStones = 6;
        zone.theme = theme;
        if (zone.enemies == null || zone.enemies.Length == 0)
        {
            var fixture = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_Farmer.asset");
            zone.enemies = new[] { fixture };
            zone.developmentEncounters = new[] { new ZoneTestEncounter {
                label = "Foundation fixture — existing Farmer placeholder", members = new[] { fixture } } };
            zone.liveEncounters = zone.developmentEncounters;
        }
        EditorUtility.SetDirty(zone); EditorUtility.SetDirty(theme); AssetDatabase.SaveAssets();
    }
    private static T Load<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
    }
}
