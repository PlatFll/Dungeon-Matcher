using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Adds the seven locked forest designs without rebuilding existing approved content.</summary>
public static class ForestRosterImporter
{
    public static readonly string[] Names={"Elven_Thornkeeper","Orc_Berserker","Orc_Bloomcaller","Snapvine","Orc_Drummer","Briar_Archer","Ancient_Treant"};
    private const string Data="Assets/_Game/Data/Enemies/Forest/";
    private const string Art="Assets/_Game/Art/Forest/Roster/";
    private const string Source="ArtSource/Forest/RosterProduction/";
    [MenuItem("Dungeon Matcher/Forest/Import expanded roster kits")]
    public static void ImportKits()
    {
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        var shell=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(Data+"Elven_Scout.asset").EnemyPrefab;
        int[] hp={75,100,70,20,80,75,180},damage={10,15,5,5,5,10,15},moves={4,999,4,999,4,4,3};
        float[] interval={5,4.5f,6,2.5f,6,5,6};
        var kinds=new[]{EnemySpecialAbilityKind.Barricade,EnemySpecialAbilityKind.Bloodrage,EnemySpecialAbilityKind.CallSnapvine,
            EnemySpecialAbilityKind.None,EnemySpecialAbilityKind.WarRhythm,EnemySpecialAbilityKind.ThornVolley,EnemySpecialAbilityKind.AncientBough};
        var defs=new EnemyDefinition[Names.Length];
        for(int i=0;i<Names.Length;i++)
        {
            string name=Names[i],path=Data+name+".asset";
            var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if(definition==null) {definition=ScriptableObject.CreateInstance<EnemyDefinition>();AssetDatabase.CreateAsset(definition,path);}
            defs[i]=definition;
            var sprite=ImportStill(name);
            var so=new SerializedObject(definition);
            so.FindProperty("enemyId").stringValue=name.ToLowerInvariant();so.FindProperty("displayName").stringValue=name.Replace('_',' ');
            so.FindProperty("description").stringValue=new[]{
                "Bramble Barricade: one hit; three thorned sides retaliate against deliberate clears. The marked safe side, cascades and specials are harmless.",
                "Bloodrage: once below half health, stronger and faster basic attacks.",
                "Call Snapvine: one living owned summon at a time. Waits for a free slot. The summon survives its caller.",
                "Fragile summoned plant with fast bite attacks.",
                "War Rhythm: temporarily speeds other living enemies' basic attacks. Refreshes without stacking.",
                "Thorn Volley: up to three existing vines marked for two moves. Remove a marked vine to cancel its shot.",
                "Bark Armor grants ordinary shield; breaking it staggers the Treant. Answer a Falling Bough mark within two moves to weaken its heavy hit."}[i];
            so.FindProperty("race").stringValue=i==0||i==5?"Elf":i==3||i==6?"Plant":"Orc";
            so.FindProperty("faction").stringValue=i==0||i==5?"Briar Wardens":"Woodland Clans";
            so.FindProperty("combatRole").stringValue=new[]{"Disruptor","Berserker","Summoner","Summon attacker","Support","Counterplay archer","Miniboss"}[i];
            var zones=so.FindProperty("eligibleZones");zones.arraySize=1;zones.GetArrayElementAtIndex(0).stringValue="magical-forest";
            so.FindProperty("category").intValue=i==3?0:i==6?2:1;
            so.FindProperty("enemyPrefab").objectReferenceValue=shell;so.FindProperty("fallbackVisualSprite").objectReferenceValue=sprite;
            so.FindProperty("visualSize").vector2Value=i==6?new Vector2(224,196):new Vector2(168,140);
            so.FindProperty("baseMaxHealth").intValue=hp[i];so.FindProperty("baseDamage").intValue=damage[i];
            so.FindProperty("baseAttackInterval").floatValue=interval[i];so.FindProperty("baseFollowUpDamage").intValue=0;
            so.FindProperty("firstAttackMoves").intValue=i==3?1:3;so.FindProperty("attackMoves").intValue=i==3?1:3;
            so.FindProperty("hasSpecialAbility").boolValue=i!=3;so.FindProperty("specialAbilityKind").intValue=(int)kinds[i];
            so.FindProperty("baseSpecialTurnRequirement").intValue=moves[i];so.FindProperty("lockSpecialTurnRequirement").boolValue=true;
            so.FindProperty("isSupport").boolValue=i==2||i==4;so.FindProperty("isBoardDisruptor").boolValue=i==0||i==5||i==6;
            so.FindProperty("threatCost").floatValue=i==3?1:i==6?5:2.5f;
            if(i==0)
            {so.FindProperty("barricadeStyle").intValue=(int)EnemyBarricadeStyle.Thorn;so.FindProperty("barricadeDurability").intValue=1;
             so.FindProperty("barricadesPerUse").intValue=1;so.FindProperty("maximumOwnedBarricades").intValue=2;}
            if(i==1) {so.FindProperty("enrageDamageMultiplier").floatValue=1.5f;so.FindProperty("enrageSpeedMultiplier").floatValue=1.4f;}
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(definition);
        }
        var caller=new SerializedObject(defs[2]);caller.FindProperty("forestSummon").objectReferenceValue=defs[3];caller.ApplyModifiedPropertiesWithoutUndo();
        var zone=Resources.Load<ZoneDefinition>("Zones/magical-forest");
        zone.enemies=zone.enemies.Concat(defs).Distinct().ToArray();
        var scout=zone.enemies.First(e=>e.name=="Elven_Scout");var root=zone.enemies.First(e=>e.name=="Orc_Rootbinder");
        var tests=zone.developmentEncounters.Where(e=>!e.label.StartsWith("Roster · ")).ToList();
        for(int i=0;i<defs.Length;i++) tests.Add(new ZoneTestEncounter{label="Roster · "+Names[i],members=i==3?new[]{defs[i]}:new[]{defs[i],i==5?root:scout}});
        tests.Add(new ZoneTestEncounter{label="Roster · Drummer alone",members=new[]{defs[4]}});
        tests.Add(new ZoneTestEncounter{label="Roster · Full summon slots",members=new[]{defs[2],scout,defs[1]}});
        zone.developmentEncounters=tests.ToArray();
        var live=zone.liveEncounters.Where(e=>!e.label.StartsWith("Roster · ")).ToList();
        live.Add(Encounter("Thorn sentry",3,7,defs[0],scout));
        live.Add(Encounter("Berserker patrol",5,7,defs[1],scout));
        live.Add(Encounter("Bloomcaller patrol",5,7,defs[2],scout));
        live.Add(Encounter("War party",9,13,defs[4],defs[1],scout));
        live.Add(Encounter("Vine hunters",9,13,defs[5],root));
        live.Add(Encounter("Deep thorns",11,13,defs[0],defs[1]));
        live.Add(Encounter("Ancient crossing",14,14,defs[6],scout));
        live.Add(Encounter("Bloom and briar",16,17,defs[2],defs[5]));
        zone.liveEncounters=live.ToArray();EditorUtility.SetDirty(zone);AssetDatabase.SaveAssets();
        Debug.Log("Expanded forest roster imported: seven approved stills, six specials, independent Snapvine, nine development fixtures and eight live formations. Blocker concepts remain unbound.");
    }
    [MenuItem("Dungeon Matcher/Forest/Import expanded roster animations")]
    public static void ImportAnimations()
    {
        foreach(string name in Names) ImportStill(name);
        ForestProductionImporter.ImportMotionSet(Names,Source+"Selected/",Art,"Assets/_Game/Animations/Forest/Roster/");
        AssetDatabase.SaveAssets();
    }
    private static ZoneTestEncounter Encounter(string label,int first,int last,params EnemyDefinition[] members) =>
        new ZoneTestEncounter{label="Roster · "+label,firstLocalWave=first,lastLocalWave=last,members=members};
    private static Sprite ImportStill(string name)
    {
        string path=Art+name+".png",grounded=Source+"Selected/"+name+"/Ready.png";
        File.Copy(File.Exists(grounded)?grounded:Source+"Inputs/"+name+".png",path,true);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;
        importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.npotScale=TextureImporterNPOTScale.None;importer.alphaIsTransparency=true;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.BottomCenter;
        importer.SetTextureSettings(settings);importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
