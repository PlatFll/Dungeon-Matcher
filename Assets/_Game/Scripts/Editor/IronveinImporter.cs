using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Writes only additive Ironvein assets. Never rebuilds another zone.</summary>
public static class IronveinImporter
{
    public const string ZonePath = "Assets/_Game/Resources/Zones/ironvein-excavation.asset";
    public const string EnemyPath = "Assets/_Game/Data/Enemies/Ironvein/";
    [MenuItem("Dungeon Matcher/Ironvein/Import specialist kits")]
    public static void ImportSpecialists()
    {
        ImportNormals();
        var rattled = Load<PlayerStatusDefinition>("Assets/_Game/Resources/PlayerStatuses/Rattled.asset");
        rattled.kind = PlayerStatusKind.Rattled; rattled.displayName = "Rattled";
        rattled.description = "New Stagger buildup is halved for two accepted moves. Existing Stagger, damage and energy are unchanged. Stone hits and drills do not cleanse it.";
        rattled.durationMoves = 2; rattled.multiplier = .5f; rattled.damagePerMove = 0;
        if (rattled.icon == null) rattled.icon = Resources.Load<Sprite>("UI/PlayerStatuses/Weakened");
        EditorUtility.SetDirty(rattled);
        string[] ids = { "ore_hauler", "stonewright", "bore_engineer", "vein_surveyor", "powder_sapper", "rail_switcher", "seismic_smith" };
        int[] hp = { 80,80,75,70,70,70,100 }, hits = { 4,8,6,5,6,7,13 };
        float[] seconds = { 6.1f,5.3f,5.3f,6.1f,5.8f,5.8f,6.1f };
        string[] descriptions = {
            "Share the Load empowers each eligible living ally's next basic sequence once and adds one charge to a fixed drill. Repeated ore refreshes without stacking.",
            "Lay the Foundation: a floor strike immediately places up to two legal Brittle Stones. No extra channel. Stones remain after this mason falls.",
            "Path of Least Resistance: marks the lane with fewest stones for two moves. A small drill clears ordinary gems, skips specials and stops at the FIRST stone after exactly one hit, even if it breaks. If no stone stops it, take 20 base damage. Launch adds one charge to the corresponding big drill.",
            "Assay the Vein: marks one existing non-Obsidian stone for one move, then advances its material one tier. Destroy that fixed stone to cancel. It never creates or retargets a stone.",
            "Set Charge: one bomb on an existing stone, with two moves to respond. A deliberate adjacent match or special defuses it without hitting the host. Destroying the host also cancels it. Ignored charge hits the current stone for exactly two durability and the player for 12 base damage.",
            "Switch Track: after a one-move warning, shifts one fixed drill by one lane. Charge remains. The marked destination cannot change during its warning.",
            "Faultline Strike: immediately applies Rattled for two accepted moves, halving only new Stagger buildup. Stone hits and drill firing do not cleanse it." };
        var shell = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(EnemyPath + "pickaxe_delver.asset");
        var defs = new EnemyDefinition[ids.Length];
        for (int i=0;i<ids.Length;i++)
        {
            var def=Load<EnemyDefinition>(EnemyPath+ids[i]+".asset"); defs[i]=def;
            var so=new SerializedObject(def);
            so.FindProperty("enemyId").stringValue=ids[i];
            so.FindProperty("displayName").stringValue=string.Join(" ",ids[i].Split('_').Select(s=>char.ToUpperInvariant(s[0])+s.Substring(1)));
            so.FindProperty("description").stringValue=descriptions[i];
            so.FindProperty("race").stringValue="Dwarf"; so.FindProperty("faction").stringValue="Ironvein Expedition";
            so.FindProperty("combatRole").stringValue=i==0?"Support":i==6?"Combat pressure":"Board disruptor";
            var zones=so.FindProperty("eligibleZones");zones.arraySize=1;zones.GetArrayElementAtIndex(0).stringValue="ironvein-excavation";
            so.FindProperty("category").intValue=1;so.FindProperty("enemyPrefab").objectReferenceValue=shell.EnemyPrefab;
            if(def.StaticVisualSprite==null)so.FindProperty("fallbackVisualSprite").objectReferenceValue=shell.StaticVisualSprite;
            so.FindProperty("baseMaxHealth").intValue=hp[i];so.FindProperty("baseDamage").intValue=hits[i];
            so.FindProperty("baseAttackInterval").floatValue=seconds[i];so.FindProperty("hasSpecialAbility").boolValue=true;
            so.FindProperty("specialAbilityKind").intValue=(int)EnemySpecialAbilityKind.ShareOre+i;
            so.FindProperty("baseSpecialTurnRequirement").intValue=i==4?5:4;
            so.FindProperty("lockSpecialTurnRequirement").boolValue=true;
            so.FindProperty("isSupport").boolValue=i==0;so.FindProperty("isBoardDisruptor").boolValue=i>=1&&i<=5;
            so.FindProperty("threatCost").floatValue=i==2||i==4||i==6?3:2.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            def.oreWeaponEligible=i==1||i==2||i==6;def.oreAttackMultiplier=1.3f;
            def.mineWarningMoves=i==3||i==5?1:2;def.mineAbilityDamage=i==4?12:20;
            def.appliedPlayerStatus=i==6?rattled:null;EditorUtility.SetDirty(def);
        }
        var zone=Load<ZoneDefinition>(ZonePath);zone.enemies=zone.enemies.Concat(defs).ToArray();
        zone.developmentEncounters=zone.developmentEncounters.Concat(defs.Select(d=>new ZoneTestEncounter {
            label=d.DisplayName+" fixture",members=new[]{d,shell} })).ToArray();
        // Keep the normal-only live test entry until encounter teaching is authored in Phase 10.
        EditorUtility.SetDirty(zone);AssetDatabase.SaveAssets();
    }
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
