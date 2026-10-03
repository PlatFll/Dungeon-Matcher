using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ForestVineArtImporter
{
    private const string Source="ArtSource/Forest/VinesAndTransition/";
    private const string Art="Assets/_Game/Art/Forest/Vines/";
    [MenuItem("Dungeon Matcher/Forest/Import root and vine art")]
    public static void Run()
    {
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        var theme=Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme");
        theme.rootLevelOne=Import("Root_Level_1");theme.rootLevelTwo=Import("Root_Level_2");
        theme.vineOverlay=Import("Vine_Weave");
        theme.vineSpreadFrames=ImportClip("Vine_Spread");theme.vineHitFrames=ImportClip("Vine_Hit");
        Import("Transition_Smoke","Assets/_Game/Resources/UI/Transition/Smoke.png");
        ConfigureTravel();
        EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();
    }
    private static void ConfigureTravel()
    {
        var forest=Resources.Load<ZoneDefinition>("Zones/magical-forest");
        var dungeon=Resources.Load<ZoneDefinition>("Zones/dungeon");
        if(dungeon==null) {dungeon=ScriptableObject.CreateInstance<ZoneDefinition>();AssetDatabase.CreateAsset(dungeon,"Assets/_Game/Resources/Zones/dungeon.asset");}
        var enemies=AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/_Game/Data/Enemies"})
            .Select(g=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        EnemyDefinition Find(string id)=>enemies.Single(e=>e.EnemyId==id);
        ZoneTestEncounter Encounter(string name,int first,int last,params string[] ids)=>new ZoneTestEncounter{
            label=name,firstLocalWave=first,lastLocalWave=last,members=ids.Select(Find).ToArray()};
        dungeon.zoneId="dungeon";dungeon.displayName="The Dungeon";dungeon.eligibleForLiveTravel=true;
        dungeon.affiliatedGem=GemType.Amethyst;dungeon.affiliatedDamageMultiplier=1;
        dungeon.enemies=enemies.Where(e=>!e.EligibleZones.Contains("magical-forest")).ToArray();
        dungeon.apexEnemy=Find("king");dungeon.apexLocalWave=18;
        dungeon.liveEncounters=new[]{
            Encounter("Royal return: command escort",18,999,"king","royal_arcanist"),
            Encounter("Royal return: spear escort",18,999,"king","royal_lancer"),
            Encounter("Royal return: ranged escort",18,999,"king","royal_arbalist")};
        forest.eligibleForLiveTravel=true;forest.apexEnemy=Find("briar_matriarch");forest.apexLocalWave=18;
        forest.liveEncounters=new[]{
            Encounter("Outer path patrol",1,4,"elven_scout","orc_trailguard"),
            Encounter("Mender escort",2,6,"elven_mender","orc_trailguard"),
            Encounter("Thicket roots",3,7,"orc_rootbinder","elven_scout"),
            Encounter("Roving woodland party",5,7,"elven_mender","elven_scout","orc_trailguard"),
            Encounter("Warden crossing",8,8,"barkhide_warden","elven_scout"),
            Encounter("Quiet trail",9,10,"orc_trailguard","elven_scout"),
            Encounter("Deep grove sentries",11,13,"orc_rootbinder","elven_mender","orc_trailguard"),
            Encounter("Deep grove hunters",11,13,"elven_scout","orc_trailguard","orc_trailguard"),
            Encounter("Warden ritual",14,14,"barkhide_warden","orc_rootbinder"),
            Encounter("Grove relief",15,15,"elven_scout","elven_mender"),
            Encounter("Heartgrove approach",16,17,"orc_rootbinder","elven_scout","orc_trailguard"),
            Encounter("Matriarch's grove",18,999,"briar_matriarch","elven_scout","orc_trailguard"),
            Encounter("Matriarch's renewal",18,999,"briar_matriarch","elven_mender","orc_trailguard")};
        EditorUtility.SetDirty(forest);EditorUtility.SetDirty(dungeon);
    }
    private static Sprite[] ImportClip(string name) => Directory.GetFiles(Source+name,"*.png").OrderBy(p=>p)
        .Select(p=>Import(name+"/"+Path.GetFileNameWithoutExtension(p))).ToArray();
    private static Sprite Import(string name,string overridePath=null)
    {
        string path=overridePath??Art+name+".png";Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.Copy(Source+name+".png",path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;
        importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
