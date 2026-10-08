using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Imports only Ironvein's selected native materials and compositions.</summary>
public static class IronveinThemeImporter
{
    private const string Source="ArtSource/Ironvein/";
    private const string Environments="Assets/_Game/Resources/BattleEnvironments/";
    private static Sprite S(string folder,string name,int border=0,bool repeat=false) =>
        IronveinArtImporter.ImportSprite(Source+folder+"/00.png",name,false,border,repeat);
    private static Sprite U(string name,int border=0,bool repeat=false) =>
        IronveinArtImporter.ImportSprite(Source+"UI/Prepared/"+name+".png",name,false,border,repeat);
    private static T Asset<T>(string path) where T:ScriptableObject => DrownedCourtImporter.Load<T>(path);

    [MenuItem("Dungeon Matcher/Ironvein/Import cave, UI and audio")]
    public static void ImportTheme()
    {
        EditorUtility.audioMasterMute=true;
        var theme=Asset<GameplayThemeDefinition>("Assets/_Game/Resources/Zones/IronveinTheme.asset");
        theme.minimumBattleHeight=352;
        // Housing reaches .93 cells past the grid: .25 frame plus .8 safe margin.
        theme.boardPerimeterPadding=.8f;
        theme.generalBackground=U("BackdropQuiet",0,true);theme.panelBackground=U("InsetSteel",0,true);
        theme.boardCells=new[]{S("UI/board_cell_v2","BoardCell")};
        theme.frameCorner=U("FrameCorner");theme.frameEdge=U("FrameEdge");
        theme.wavePlaque=U("WavePlaque");
        theme.buttonNormal=S("UI/button_v2","ButtonNormal",8);
        theme.buttonHighlighted=U("ButtonHighlighted",8);theme.buttonPressed=U("ButtonPressed",8);theme.buttonDisabled=U("ButtonDisabled",8);
        theme.settingsNormal=S("UI/settings","SettingsNormal");
        theme.settingsHighlighted=U("SettingsHighlighted");theme.settingsPressed=U("SettingsPressed");theme.settingsDisabled=U("SettingsDisabled");
        theme.supplyNormal=S("UI/supply_tile_v2","SupplyNormal");
        theme.supplyHighlighted=U("SupplyHighlighted");theme.supplyPressed=U("SupplyPressed");theme.supplyDisabled=U("SupplyDisabled");
        theme.panelShell=U("PanelFilled",8);theme.energyFrame=U("EnergyFrame");
        theme.abilityIcons=Array.Empty<GameplayThemeDefinition.IconReplacement>();
        theme.healthStyles=Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme").healthStyles;
        var frame=Asset<PlayerAreaFrameProfile>("Assets/_Game/Resources/Zones/IronveinPlayerFrame.asset");
        var so=new SerializedObject(frame);so.FindProperty("playerSectionWidth").floatValue=146;
        so.FindProperty("topPiece").objectReferenceValue=U("PlayerTop");
        so.FindProperty("middlePiece").objectReferenceValue=U("PlayerMiddle");
        so.FindProperty("bottomPiece").objectReferenceValue=U("PlayerBottom");
        so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(frame);theme.playerFrame=frame;
        Sprite M(string name)=>S("Mechanics/"+name,name);
        Sprite[] Frames(string name,int count)=>Enumerable.Range(0,count).Select(i=>
            IronveinArtImporter.ImportSprite(Source+"Mechanics/"+name+"/"+i.ToString("00")+".png",name+"_"+i.ToString("00"))).ToArray();
        theme.mineStoneStages=new[]{M("stone_brittle"),M("stone_hardened"),M("stone_obsidian")};
        theme.mineHardenedDamaged=M("hardened_hit1");theme.mineObsidianDamaged=new[]{M("obsidian_hit1"),M("obsidian_hit2")};
        theme.mineCore=M("core");theme.mineCoreDamaged=new[]{M("core_hit1"),M("core_hit2")};
        theme.horizontalMineDrill=M("fixed_drill");theme.verticalMineDrill=theme.horizontalMineDrill;
        theme.mineCharge=M("powder_charge");theme.mineSpark=M("ore_spark");
        theme.mineDrillFrames=Frames("fixed_drill_spin",5);theme.mineSmallDrillFrames=Frames("small_drill_spin",5);
        // Last two ore frames contain flat residual masks; omit those candidates.
        theme.mineOreFrames=Frames("ore_burst",7);theme.mineFuseFrames=Frames("charge_fuse",5);theme.mineCoreBreakFrames=Frames("core_break",9);
        var rattled=Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Rattled");rattled.icon=M("rattled");EditorUtility.SetDirty(rattled);
        foreach(string id in IronveinArtImporter.Ids)
        {
            var enemy=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(IronveinImporter.EnemyPath+id+".asset");
            enemy.mineBasicSound=id=="rivet_gunner" || id=="rivet_turret" ? CombatSoundCue.MineRivet :
                id=="grand_delver" || id=="obsidian_sentinel" ? CombatSoundCue.MinePiston : CombatSoundCue.MineHammer;
            EditorUtility.SetDirty(enemy);
        }
        ImportScenes(theme);ImportAudio();
        var zone=Resources.Load<ZoneDefinition>("Zones/ironvein-excavation");zone.theme=theme;
        zone.music=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Music/Ironvein Rail Lanterns - TEMP review.wav");
        EditorUtility.SetDirty(zone);EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();
        Debug.Log("Ironvein: native cave/UI/mechanics and original temporary audio imported; existing zones preserved.");
    }

    private static void ImportScenes(GameplayThemeDefinition theme)
    {
        Tile T(string folder,string name)
        {
            var tile=Asset<Tile>(IronveinArtImporter.Art+name+"Tile.asset");tile.sprite=S("Scene/"+folder,name);
            tile.colliderType=Tile.ColliderType.None;EditorUtility.SetDirty(tile);return tile;
        }
        var wall=T("cave_wall","CaveWall");var floor=T("cave_floor_v2","CaveFloor");var tunnel=T("cave_depth","MineTunnel");
        var timber=T("timber_support","TimberSupport");var ore=T("ore_cluster","OreCluster");var cart=T("minecart","MineCart");
        var rail=T("rail_junction","RailJunction");var lamp=T("mine_lantern","MineLantern");
        var winch=T("mine_winch","MineWinch");var pump=T("mine_pump","MinePump");var banner=T("mine_banner","MineBanner");
        theme.battleEnvironmentVariants=new GameObject[3];
        for(int variant=0;variant<3;variant++)
        {
            var root=PrefabUtility.LoadPrefabContents(Environments+"Dungeon_Default.prefab");
            try
            {
                string name=new[]{"Ironvein_Railhead","Ironvein_Pumpworks","Ironvein_DeepShaft"}[variant];root.name=name;
                var env=new SerializedObject(root.GetComponent<BattleEnvironmentRoot>());
                env.FindProperty("environmentId").stringValue="ironvein-"+variant;env.ApplyModifiedPropertiesWithoutUndo();
                foreach(var map in root.GetComponentsInChildren<Tilemap>())
                {map.ClearAllTiles();map.transform.localPosition=Vector3.zero;map.tileAnchor=Vector3.zero;map.color=Color.white;}
                var back=root.transform.Find("BackWall").GetComponent<Tilemap>();
                var ground=root.transform.Find("Floor").GetComponent<Tilemap>();
                var architecture=root.transform.Find("Architecture").GetComponent<Tilemap>();
                var props=root.transform.Find("AtmosphereProps").GetComponent<Tilemap>();
                void Place(Tilemap map,Tile tile,float x,float y)
                {
                    var cell=new Vector3Int(Mathf.FloorToInt(x),Mathf.FloorToInt(y),0);
                    // Multiple small props in a logical cell get their own child renderer;
                    // this avoids silently overwriting a lamp with a banner.
                    if(map.HasTile(cell))
                    {
                        var go=new GameObject(tile.name);go.transform.SetParent(map.transform,false);go.transform.localPosition=new Vector3(x,y,0);
                        var sr=go.AddComponent<SpriteRenderer>();sr.sprite=tile.sprite;sr.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
                        var mr=map.GetComponent<TilemapRenderer>();sr.sortingLayerID=mr.sortingLayerID;sr.sortingOrder=mr.sortingOrder;return;
                    }
                    map.SetTile(cell,tile);map.SetTileFlags(cell,TileFlags.None);
                    map.SetTransformMatrix(cell,Matrix4x4.Translate(new Vector3(x-cell.x,y-cell.y,0)));
                }
                for(int x=-16;x<=16;x+=4)for(int y=-12;y<=12;y+=2)Place(back,wall,x,y);
                back.color=new Color(.25f,.27f,.34f,1);
                for(int x=-16;x<=16;x+=2)for(int y=0;y>=-12;y--)Place(ground,floor,x,y+.25f);
                ground.color=new Color(.72f,.72f,.78f,1);
                Place(architecture,tunnel,variant==1?.3f:.85f,2.05f);
                Place(architecture,timber,-1.7f,2.55f);Place(architecture,timber,3.65f,2.55f);
                Place(architecture,lamp,-1.55f,2.8f);Place(architecture,lamp,3.35f,2.8f);
                Place(architecture,ore,-1.45f,1.1f);Place(architecture,ore,2.6f,1.75f);
                Place(props,rail,-1.1f,.55f);Place(props,cart,-1.2f,1.15f);
                Place(props,variant==1?pump:winch,3.05f,1.2f);
                Place(architecture,banner,2.05f,2.8f);
                if(variant==2){Place(props,pump,-4.3f,.1f);Place(architecture,ore,-.7f,2.0f);}
                string path=Environments+name+".prefab";PrefabUtility.SaveAsPrefabAsset(root,path);
                theme.battleEnvironmentVariants[variant]=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        theme.battleEnvironment=theme.battleEnvironmentVariants[0];
    }

    private static void ImportAudio()
    {
        foreach(string source in Directory.GetFiles(Source+"Audio","*.wav"))
        {
            bool music=Path.GetFileName(source).Contains("TEMP");
            string folder=music?"Assets/_Game/Audio/Music/":"Assets/_Game/Resources/Audio/Combat/";
            Directory.CreateDirectory(folder);string path=folder+Path.GetFileName(source);File.Copy(source,path,true);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;
            settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat=music?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;
            settings.quality=.8f;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings=settings;importer.forceToMono=!music;importer.SaveAndReimport();
        }
    }
}
