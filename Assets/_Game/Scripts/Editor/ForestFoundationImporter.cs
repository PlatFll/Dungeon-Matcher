using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Explicit import of approved stills and the accepted small visual proof.</summary>
public static class ForestFoundationImporter
{
    private const string Art = "Assets/_Game/Art/Forest/";
    private const string Data = "Assets/_Game/Data/Enemies/Forest/";
    [MenuItem("Dungeon Matcher/Forest/Import approved foundation assets")]
    public static void Run()
    {
        Directory.CreateDirectory(Art); Directory.CreateDirectory(Data);
        Directory.CreateDirectory("Assets/_Game/Resources/Zones");
        AssetDatabase.Refresh();
        string[] names = {"Elven_Scout","Orc_Trailguard","Elven_Mender","Orc_Rootbinder","Barkhide_Warden","Briar_Matriarch"};
        int[] health={60,90,75,75,150,240}, damage={5,15,5,5,15,10}, cadence={2,4,4,5,4,4};
        float[] threat={1.5f,2,2.5f,3,5,7};
        string[] factions={"Path Wardens","Woodland Clans","Grove Wardens","Woodland Clans","Grove Sentinels","Deep-Grove Court"};
        string[] roles={"Ranged attacker","Heavy attacker","Ally healer","Disruptor","Protector concept","Ritual leader concept"};
        var shell=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_SpearGuard.asset").EnemyPrefab;
        var definitions=new EnemyDefinition[6];
        for(int i=0;i<names.Length;i++)
        {
            var sprite=Import("ArtSource/Forest/Approved/"+names[i]+".png",names[i],true);
            var definition=LoadOrCreate<EnemyDefinition>(Data+names[i]+".asset"); definitions[i]=definition;
            var so=new SerializedObject(definition);
            so.FindProperty("enemyId").stringValue=names[i].ToLowerInvariant();
            so.FindProperty("displayName").stringValue=names[i].Replace('_',' ');
            so.FindProperty("description").stringValue=i<4 ? "Phase 4 forest test kit. Prototype balance." : "Approved static art. Milestone kit remains a later phase; excluded from this test roster.";
            so.FindProperty("race").stringValue=i==0||i==2||i==5?"Elf":"Orc";
            so.FindProperty("faction").stringValue=factions[i];so.FindProperty("combatRole").stringValue=roles[i];
            var zones=so.FindProperty("eligibleZones");zones.arraySize=1;zones.GetArrayElementAtIndex(0).stringValue="magical-forest";
            so.FindProperty("category").enumValueIndex=i<2?0:i<4?1:i==4?2:3;
            so.FindProperty("enemyPrefab").objectReferenceValue=shell;
            so.FindProperty("fallbackVisualSprite").objectReferenceValue=sprite;
            so.FindProperty("animationControllerOverride").objectReferenceValue=null;
            so.FindProperty("visualSize").vector2Value=i==5?new Vector2(168,168):new Vector2(112,112);
            so.FindProperty("baseMaxHealth").intValue=health[i];so.FindProperty("baseDamage").intValue=damage[i];
            so.FindProperty("baseFollowUpDamage").intValue=0;
            so.FindProperty("firstAttackMoves").intValue=cadence[i];so.FindProperty("attackMoves").intValue=cadence[i];
            so.FindProperty("hasSpecialAbility").boolValue=i==2||i==3;
            so.FindProperty("specialAbilityKind").intValue=i==2?(int)EnemySpecialAbilityKind.ChannelHeal:i==3?(int)EnemySpecialAbilityKind.SpreadingVines:0;
            so.FindProperty("baseSpecialTurnRequirement").intValue=i==2?2:3;
            so.FindProperty("lockSpecialTurnRequirement").boolValue=true;
            so.FindProperty("threatCost").floatValue=threat[i];
            so.FindProperty("isSupport").boolValue=i==2;so.FindProperty("isBoardDisruptor").boolValue=i==3;
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(definition);
        }
        var theme=LoadOrCreate<GameplayThemeDefinition>("Assets/_Game/Resources/Zones/ForestTheme.asset");
        theme.frameCorner=Import("ArtSource/Forest/UI/Wood_Corner.png","Wood_Corner",false);
        theme.frameEdge=Import("ArtSource/Forest/UI/Wood_Edge.png","Wood_Edge",false);
        theme.wavePlaque=Import("ArtSource/Forest/UI/Wave_Wood.png","Wave_Wood",false);
        var playerFrame=LoadOrCreate<PlayerAreaFrameProfile>("Assets/_Game/Resources/Zones/ForestPlayerFrame.asset");
        var playerData=new SerializedObject(playerFrame);
        playerData.FindProperty("playerSectionWidth").floatValue=146;
        playerData.FindProperty("topPiece").objectReferenceValue=Import("ArtSource/Forest/UI/Player_Top.png","Player_Top",false);
        playerData.FindProperty("middlePiece").objectReferenceValue=Import("ArtSource/Forest/UI/Player_Middle.png","Player_Middle",false);
        playerData.FindProperty("bottomPiece").objectReferenceValue=Import("ArtSource/Forest/UI/Player_Bottom.png","Player_Bottom",false);
        playerData.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(playerFrame);theme.playerFrame=playerFrame;
        var hpStart=Import("ArtSource/Forest/UI/Health_Start.png","Health_Start",false);
        var hpMiddle=Import("ArtSource/Forest/UI/Health_Middle.png","Health_Middle",false);
        var hpEnd=Import("ArtSource/Forest/UI/Health_End.png","Health_End",false);
        theme.healthStyles=new ModularHealthBarStyle[5];
        string[] ranks={"Normal","Special","Miniboss","Boss","Player"};
        for(int i=0;i<5;i++)
        {
            var style=LoadOrCreate<ModularHealthBarStyle>("Assets/_Game/Resources/Zones/ForestHealth_"+ranks[i]+".asset");
            EditorUtility.CopySerialized(Resources.Load<ModularHealthBarStyle>("UI/Finalized/Health/"+ranks[i]),style);
            var data=new SerializedObject(style);
            data.FindProperty("startPiece").objectReferenceValue=hpStart;data.FindProperty("middlePiece").objectReferenceValue=hpMiddle;data.FindProperty("endPiece").objectReferenceValue=hpEnd;
            data.FindProperty("fillInsetLeft").floatValue=6;data.FindProperty("fillInsetRight").floatValue=6;data.FindProperty("fillInsetVertical").floatValue=7;
            data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(style);theme.healthStyles[i]=style;
        }
        theme.generalBackground=Import("ArtSource/Forest/UI/General_Timber_v1.png","General_Timber",false,true);
        theme.panelBackground=Import("ArtSource/Forest/UI/Panel_Timber_v1.png","Panel_Timber",false,true);
        theme.boardCells=Enumerable.Range(1,3).Select(n=>Import("ArtSource/Forest/UI/Log_Cell_0"+n+"_v1.png","Log_Cell_0"+n,false)).ToArray();
        Import("ArtSource/Forest/UI/Woodland_Study_Accepted.png","Woodland",false);
        // Exact native-scale study crops fill the existing masked renderer.
        // Repetition is a development placeholder, not a production tileset.
        var tile=LoadOrCreate<Tile>(Art+"WoodlandTile.asset");
        tile.sprite=Import("ArtSource/Forest/UI/Woodland_Canopy.png","Woodland_Canopy",false);
        tile.colliderType=Tile.ColliderType.None;EditorUtility.SetDirty(tile);
        var floorTile=LoadOrCreate<Tile>(Art+"WoodlandFloorTile.asset");
        floorTile.sprite=Import("ArtSource/Forest/UI/Woodland_Floor.png","Woodland_Floor",false);
        floorTile.colliderType=Tile.ColliderType.None;EditorUtility.SetDirty(floorTile);
        string prefab="Assets/_Game/Resources/BattleEnvironments/Forest_Prototype.prefab";
        var root=PrefabUtility.LoadPrefabContents("Assets/_Game/Resources/BattleEnvironments/Dungeon_Default.prefab");
        try
        {
            root.name="Forest_Prototype";
            var environment=new SerializedObject(root.GetComponent<BattleEnvironmentRoot>());
            environment.FindProperty("environmentId").stringValue="forest-prototype";environment.ApplyModifiedPropertiesWithoutUndo();
            foreach(var map in root.GetComponentsInChildren<Tilemap>()) map.ClearAllTiles();
            var back=root.transform.Find("BackWall").GetComponent<Tilemap>();
            back.transform.localPosition=new Vector3(-.5f,-.5f,0);
            for(int x=-12;x<=12;x+=4)
            for(int row=-8;row<=4;row++)
            {
                var cell=new Vector3Int(x,row,0);
                back.SetTile(cell,row>0?tile:floorTile);
                back.SetTileFlags(cell,TileFlags.None);
                float center=row>0?1+(row-1)*1.5f:row*.5f;
                back.SetTransformMatrix(cell,Matrix4x4.Translate(new Vector3(0,center-row,0)));
            }
            PrefabUtility.SaveAsPrefabAsset(root,prefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        theme.battleEnvironment=AssetDatabase.LoadAssetAtPath<GameObject>(prefab);EditorUtility.SetDirty(theme);
        var zone=LoadOrCreate<ZoneDefinition>("Assets/_Game/Resources/Zones/magical-forest.asset");
        zone.zoneId="magical-forest";zone.displayName="Magical Forest · Development";zone.eligibleForLiveTravel=false;
        zone.affiliatedGem=GemType.Emerald;zone.theme=theme;zone.enemies=definitions;
        zone.developmentEncounters=new[]{
            new ZoneTestEncounter{label="Forest · Heal response",members=new[]{definitions[1],definitions[2],definitions[0]}},
            new ZoneTestEncounter{label="Forest · Vine response",members=new[]{definitions[3],definitions[0],definitions[1]}},
            new ZoneTestEncounter{label="Forest · Combined pressure",members=new[]{definitions[2],definitions[3],definitions[1]}}
        };EditorUtility.SetDirty(zone);
        AssetDatabase.SaveAssets(); Debug.Log("Forest foundation: six approved stills; four playable kits; forest excluded from live selection.");
    }
    private static T LoadOrCreate<T>(string path) where T:ScriptableObject
    {
        var asset=AssetDatabase.LoadAssetAtPath<T>(path);
        if(asset==null) {asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}return asset;
    }
    private static Sprite Import(string source,string name,bool bottom,bool repeat=false)
    {
        string path=Art+name+".png";File.Copy(source,path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;
        importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;
        importer.wrapMode=repeat?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)(bottom?SpriteAlignment.BottomCenter:SpriteAlignment.Center);
        importer.SetTextureSettings(settings);importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
