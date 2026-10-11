using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Frostwake-only native materials, scenery and original temporary music.</summary>
public static class FrostwakeThemeImporter
{
    private static Sprite E(string name,int border=0,bool repeat=false)=>FrostwakeZoneImporter.Sprite("ArtSource/Frostwake/environment/"+name+".png",name,false,border,repeat);
    private static Sprite P(string name,int border=0,bool repeat=false)=>FrostwakeZoneImporter.Sprite("ArtSource/Frostwake/theme/"+name+".png",name,false,border,repeat);
    [MenuItem("Dungeon Matcher/Frostwake/Import mountain theme")]
    public static void Run()
    {
        EditorUtility.audioMasterMute=true;
        var theme=FrostwakeZoneImporter.Load<GameplayThemeDefinition>("Assets/_Game/Resources/Zones/FrostwakeTheme.asset");
        theme.minimumBattleHeight=384;
        theme.generalBackground=P("QuietRock",0,true);theme.panelBackground=P("QuietTimber",0,true);
        theme.boardCells=new[]{E("StoneTile")};theme.frameCorner=P("FrameCorner");theme.frameEdge=P("FrameEdge");
        theme.wavePlaque=P("WavePlaque");theme.panelShell=E("Button",12);theme.energyFrame=P("EnergyFrame");
        theme.buttonNormal=E("Button",12);theme.buttonHighlighted=theme.buttonNormal;theme.buttonPressed=theme.buttonNormal;theme.buttonDisabled=theme.buttonNormal;
        theme.settingsNormal=E("SettingsGear");theme.settingsHighlighted=theme.settingsNormal;theme.settingsPressed=theme.settingsNormal;theme.settingsDisabled=theme.settingsNormal;
        theme.supplyNormal=E("SupplyTile");theme.supplyHighlighted=theme.supplyNormal;theme.supplyPressed=theme.supplyNormal;theme.supplyDisabled=theme.supplyNormal;
        var shared=Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme");
        theme.healthStyles=shared.healthStyles;theme.healEffect=shared.healEffect;theme.interruptEffect=shared.interruptEffect;
        var frame=FrostwakeZoneImporter.Load<PlayerAreaFrameProfile>("Assets/_Game/Resources/Zones/FrostwakePlayerFrame.asset");
        var so=new SerializedObject(frame);so.FindProperty("playerSectionWidth").floatValue=146;
        so.FindProperty("topPiece").objectReferenceValue=P("PlayerTop");so.FindProperty("middlePiece").objectReferenceValue=P("PlayerMiddle");
        so.FindProperty("bottomPiece").objectReferenceValue=P("PlayerBottom");so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(frame);theme.playerFrame=frame;
        var rules=Resources.Load<FrostwakeRules>("Zones/FrostwakeRules");
        Sprite Ice(string id)=>FrostwakeZoneImporter.Sprite("ArtSource/Frostwake/cleanup/"+id+".png",id);
        rules.frostOverlay=Ice("Frost");rules.heavyIceOverlay=Ice("HeavyIce");rules.bifrostOverlay=Ice("Bifrost");EditorUtility.SetDirty(rules);
        Scenes(theme);
        string audio="Assets/_Game/Audio/Music/Frostwake Wandering Halls - TEMP review.wav";
        File.Copy("ArtSource/Frostwake/audio/Frostwake Wandering Halls - TEMP review.wav",audio,true);AssetDatabase.ImportAsset(audio,ImportAssetOptions.ForceSynchronousImport);
        var importer=(AudioImporter)AssetImporter.GetAtPath(audio);var settings=importer.defaultSampleSettings;
        settings.loadType=AudioClipLoadType.Streaming;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.8f;
        settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;importer.defaultSampleSettings=settings;importer.forceToMono=false;importer.SaveAndReimport();
        var zone=Resources.Load<ZoneDefinition>("Zones/frostwake-peaks");zone.theme=theme;zone.music=AssetDatabase.LoadAssetAtPath<AudioClip>(audio);
        zone.eligibleForLiveTravel=true;EditorUtility.SetDirty(zone);EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();
        Debug.Log("Frostwake mountain pass, wandering hall, glacier gate, timber UI and temporary score imported.");
    }
    private static void Scenes(GameplayThemeDefinition theme)
    {
        Tile T(string name,Sprite sprite)
        {var tile=FrostwakeZoneImporter.Load<Tile>(FrostwakeZoneImporter.Art+name+"Tile.asset");tile.sprite=sprite;tile.colliderType=Tile.ColliderType.None;EditorUtility.SetDirty(tile);return tile;}
        var wall=T("MountainRock",P("QuietRock"));var floor=T("MountainFloor",P("Floor"));
        string[] names={"EmptyMountainPass","WanderingHall","GlacierGate"};theme.battleEnvironmentVariants=new GameObject[3];
        for(int i=0;i<names.Length;i++)
        {
            string[] sources={"FullMountainPass","FullWanderingHallV2","FullGlacierGateV2"};
            var vista=T(names[i],E(sources[i]));var root=PrefabUtility.LoadPrefabContents("Assets/_Game/Resources/BattleEnvironments/Dungeon_Default.prefab");
            try
            {
                root.name="Frostwake_"+names[i];var so=new SerializedObject(root.GetComponent<BattleEnvironmentRoot>());
                so.FindProperty("environmentId").stringValue="frostwake-"+i;so.ApplyModifiedPropertiesWithoutUndo();
                foreach(var map in root.GetComponentsInChildren<Tilemap>())
                {map.ClearAllTiles();map.transform.localPosition=Vector3.zero;map.tileAnchor=Vector3.zero;map.color=Color.white;}
                var back=root.transform.Find("BackWall").GetComponent<Tilemap>();var mid=root.transform.Find("Architecture").GetComponent<Tilemap>();
                var ground=root.transform.Find("Floor").GetComponent<Tilemap>();
                void Place(Tilemap map,Tile tile,float x,float y)
                {
                    var cell=new Vector3Int(Mathf.FloorToInt(x),Mathf.FloorToInt(y),0);
                    if(map.HasTile(cell))
                    {
                        var child=new GameObject(tile.name);child.transform.SetParent(map.transform,false);child.transform.localPosition=new Vector3(x,y,0);
                        var renderer=child.AddComponent<SpriteRenderer>();renderer.sprite=tile.sprite;renderer.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
                        var layer=map.GetComponent<TilemapRenderer>();renderer.sortingLayerID=layer.sortingLayerID;renderer.sortingOrder=layer.sortingOrder;return;
                    }
                    map.SetTile(cell,tile);map.SetTileFlags(cell,TileFlags.None);map.SetTransformMatrix(cell,Matrix4x4.Translate(new Vector3(x-cell.x,y-cell.y,0)));
                }
                for(int x=-8;x<=8;x+=2)for(int y=-4;y<=6;y+=2)Place(back,wall,x,y);
                // One composition at native scale. Rock extends beyond it; no stretched vista.
                Place(mid,vista,0,2.25f);
                // Source floor is a real rocky walking ledge, extending above the shared feet line.
                for(int x=-8;x<=8;x++)for(float y=-.25f;y>=-4;y-=.5f)Place(ground,floor,x,y);
                string path="Assets/_Game/Resources/BattleEnvironments/"+root.name+".prefab";
                PrefabUtility.SaveAsPrefabAsset(root,path);theme.battleEnvironmentVariants[i]=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        theme.battleEnvironment=theme.battleEnvironmentVariants[0];
    }
}
