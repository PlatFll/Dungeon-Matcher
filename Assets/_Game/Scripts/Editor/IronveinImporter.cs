using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Writes only additive Ironvein assets. Never rebuilds another zone.</summary>
public static class IronveinImporter
{
    public const string ZonePath = "Assets/_Game/Resources/Zones/ironvein-excavation.asset";
    public const string EnemyPath = "Assets/_Game/Data/Enemies/Ironvein/";
    [MenuItem("Dungeon Matcher/Ironvein/Import normal kits")]
    public static void ImportNormals()
    {
        ImportFoundation();
        string[] ids = { "pickaxe_delver", "rivet_gunner", "packbeetle" };
        int[] hp = { 65, 55, 85 }, damage = { 9, 7, 6 };
        float[] seconds = { 4.7f, 4.2f, 4.8f };
        string[] descriptions = {
            "One mechanical pickaxe chop. Ore-Powered strengthens the next whole basic attack once.",
            "One pneumatic rivet shot. Ore-Powered strengthens the next whole basic attack once.",
            "On defeat, releases ore once: powers eligible living allies and gives every fixed drill one charge." };
        var shell = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_Farmer.asset");
        var definitions = new EnemyDefinition[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            var def = Load<EnemyDefinition>(EnemyPath + ids[i] + ".asset"); definitions[i] = def;
            var so = new SerializedObject(def);
            so.FindProperty("enemyId").stringValue = ids[i];
            so.FindProperty("displayName").stringValue = string.Join(" ", ids[i].Split('_').Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)));
            so.FindProperty("description").stringValue = descriptions[i];
            so.FindProperty("race").stringValue = i == 2 ? "Cave beetle" : "Dwarf";
            so.FindProperty("faction").stringValue = "Ironvein Expedition";
            so.FindProperty("combatRole").stringValue = i == 2 ? "Death support" : "Attacker";
            var zones = so.FindProperty("eligibleZones"); zones.arraySize = 1;
            zones.GetArrayElementAtIndex(0).stringValue = "ironvein-excavation";
            so.FindProperty("category").intValue = 0;
            so.FindProperty("enemyPrefab").objectReferenceValue = shell.EnemyPrefab;
            // Temporary existing shell art only. Phase 08 replaces it with native mine art.
            if (def.StaticVisualSprite == null) so.FindProperty("fallbackVisualSprite").objectReferenceValue = shell.StaticVisualSprite;
            so.FindProperty("baseMaxHealth").intValue = hp[i];
            so.FindProperty("baseDamage").intValue = damage[i];
            so.FindProperty("baseAttackInterval").floatValue = seconds[i];
            so.FindProperty("hasSpecialAbility").boolValue = false;
            so.FindProperty("threatCost").floatValue = 1.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            def.oreWeaponEligible = i < 2; def.releasesOreOnDefeat = i == 2;
            def.oreAttackMultiplier = 1.3f; EditorUtility.SetDirty(def);
        }
        var zone = Load<ZoneDefinition>(ZonePath); zone.enemies = definitions;
        zone.developmentEncounters = new[] { new ZoneTestEncounter { label = "Ore network fixture", members = definitions } };
        zone.liveEncounters = zone.developmentEncounters;
        EditorUtility.SetDirty(zone); AssetDatabase.SaveAssets();
    }
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
        theme.boardPerimeterPadding = .55f;
        zone.zoneId = "ironvein-excavation"; zone.displayName = "Ironvein (prototype)";
        zone.eligibleForTesting = true; zone.eligibleForLiveTravel = false;
        zone.affiliatedGem = GemType.Amber; zone.affiliatedDamageMultiplier = 1.15f;
        zone.maturesStone = true; zone.mineMovesPerStage = 3; zone.maximumMineStones = 6;
        zone.mineDrillCapacity = 4;
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
