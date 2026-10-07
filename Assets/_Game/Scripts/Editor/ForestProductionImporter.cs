using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>Explicit, repeatable forest production imports; never rewrites approved stills.</summary>
public static class ForestProductionImporter
{
    public static readonly string[] Names={"Elven_Scout","Orc_Trailguard","Elven_Mender","Orc_Rootbinder","Barkhide_Warden","Briar_Matriarch"};
    private const string Art="Assets/_Game/Art/Forest/Production/";
    private const string Source="ArtSource/Forest/Production/";
    private const string Data="Assets/_Game/Data/Enemies/Forest/";
    private const string Animation="Assets/_Game/Animations/Forest/";
    [Serializable] private sealed class ClipSpec
    {
        public string name,state;
        public int width,height,frameCount,impactFrame;
        public int[] durationsMs;
        public bool loop;
        public bool special;
    }
    [Serializable] private sealed class Manifest { public ClipSpec[] clips; }
    [MenuItem("Dungeon Matcher/Forest/Import production art and kits")]
    public static void Run()
    {
        Directory.CreateDirectory(Art);Directory.CreateDirectory(Animation);
        AssetDatabase.Refresh();
        ImportKits();ImportAnimations();ImportTheme();
        var music=(AudioImporter)AssetImporter.GetAtPath("Assets/_Game/Audio/Music/Forest Lanterns - TEMP review.wav");
        var audio=music.defaultSampleSettings;audio.loadType=AudioClipLoadType.Streaming;
        audio.compressionFormat=AudioCompressionFormat.Vorbis;audio.quality=.8f;
        audio.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;music.defaultSampleSettings=audio;
        music.forceToMono=false;music.SaveAndReimport();
        AssetDatabase.SaveAssets();
        Debug.Log("Forest production art imported: native motion, modular woodland, timber UI, canonical vine art and temporary music.");
    }
    private static void ImportAnimations()
        => ImportMotionSet(Names,Source+"Selected/",Art,Animation);

    public static void ImportMotionSet(string[] names,string source,string art,string animation,string dataRoot=Data)
    {
        Directory.CreateDirectory(art);Directory.CreateDirectory(animation);AssetDatabase.Refresh();
        var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(source+"animation-manifest.json"));
        foreach(string name in names)
        {
            string controllerPath=animation+name+".controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) ??
                AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine=controller.layers[0].stateMachine;
            foreach(string parameter in new[]{"AutoAttack","Ability"})
                if(!controller.parameters.Any(p=>p.name==parameter)) controller.AddParameter(parameter,AnimatorControllerParameterType.Trigger);
            foreach(var spec in manifest.clips.Where(c=>c.name==name))
            {
                if(spec.durationsMs.Length!=spec.frameCount) throw new InvalidDataException(name+" "+spec.state+" timing mismatch.");
                string key=name+"_"+spec.state,path=art+key+".png";
                File.Copy(source+name+"/"+spec.state+".png",path,true);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=Configure(path);
                importer.spriteImportMode=SpriteImportMode.Multiple;
#pragma warning disable CS0618
                importer.spritesheet=Enumerable.Range(0,spec.frameCount).Select(i=>new SpriteMetaData{
                    name=key+"_"+i.ToString("00"),rect=new Rect(i*spec.width,0,spec.width,spec.height),
                    alignment=(int)SpriteAlignment.BottomCenter,pivot=new Vector2(.5f,0)}).ToArray();
#pragma warning restore CS0618
                importer.SaveAndReimport();
                var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
                if(sprites.Length!=spec.frameCount) throw new InvalidDataException(key+" frame import mismatch.");
                var clip=LoadOrCreate<AnimationClip>(animation+key+".anim");clip.name=key;clip.frameRate=100;clip.ClearCurves();
                var keys=new ObjectReferenceKeyframe[spec.frameCount+1];int elapsed=0;
                for(int i=0;i<spec.frameCount;i++) { keys[i]=new ObjectReferenceKeyframe{time=elapsed/1000f,value=sprites[i]};elapsed+=spec.durationsMs[i]; }
                keys[spec.frameCount]=new ObjectReferenceKeyframe{time=(elapsed-10)/1000f,value=sprites.Last()};
                AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(Image),"m_Sprite"),keys);
                var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=spec.loop;settings.startTime=0;settings.stopTime=elapsed/1000f;
                AnimationUtility.SetAnimationClipSettings(clip,settings);
                AnimationUtility.SetAnimationEvents(clip,(spec.state=="AutoAttack" || spec.state=="InflatedAttack")?new[]{
                    new AnimationEvent{time=spec.durationsMs.Take(spec.impactFrame).Sum()/1000f,functionName="AutoAttackImpact"},
                    new AnimationEvent{time=(elapsed-10)/1000f,functionName="AutoAttackComplete"}}:spec.special?new[]{
                    new AnimationEvent{time=spec.durationsMs.Take(spec.impactFrame).Sum()/1000f,functionName="AbilityImpact"},
                    new AnimationEvent{time=spec.durationsMs.Take(spec.impactFrame).Sum()/1000f,functionName="AbilityBeat",intParameter=1},
                    new AnimationEvent{time=(elapsed-10)/1000f,functionName="AbilityComplete"}}:Array.Empty<AnimationEvent>());
                var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name==spec.state)??machine.AddState(spec.state);
                state.motion=clip;state.speed=1;state.writeDefaultValues=true;
                foreach(var transition in state.transitions.ToArray()) state.RemoveTransition(transition);
                if(spec.state=="Idle") machine.defaultState=state;
                EditorUtility.SetDirty(clip);
            }
            var idle=machine.states.Single(s=>s.state.name=="Idle").state;
            var attack=machine.states.Single(s=>s.state.name=="AutoAttack").state;
            var exit=attack.AddTransition(idle);exit.hasExitTime=true;exit.exitTime=1;exit.duration=0;
            // Full authored casts finish in the exact ready pose. Held warnings
            // retain ChannelStart's final pose until their owner selects ChannelHold.
            foreach(var completed in machine.states.Select(s=>s.state).Where(s=>
                manifest.clips.Any(c=>c.name==name && c.state==s.name && c.special &&
                    (c.state=="Ability" || c.state.EndsWith("Release",StringComparison.Ordinal)))))
            {
                var recovery=completed.AddTransition(idle);
                recovery.hasExitTime=true;recovery.exitTime=1;recovery.duration=0;
            }
            var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(dataRoot+name+".asset");
            var so=new SerializedObject(definition);
            so.FindProperty("animationControllerOverride").objectReferenceValue=controller;
            so.FindProperty("timeAutoAttackFromAnimation").boolValue=true;
            so.FindProperty("useAuthoredAutoAttackMotion").boolValue=true;
            bool specials=manifest.clips.Any(c=>c.name==name && c.special);
            so.FindProperty("useAuthoredSpecialAbilityMotion").boolValue=specials;
            if(specials) so.FindProperty("timeSpecialAbilityFromAnimation").boolValue=true;
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(definition);EditorUtility.SetDirty(controller);
        }
    }
    private static T LoadOrCreate<T>(string path) where T:UnityEngine.Object
    {
        var value=AssetDatabase.LoadAssetAtPath<T>(path);
        if(value==null)
        {
            value=typeof(T)==typeof(AnimationClip)?new AnimationClip() as T:ScriptableObject.CreateInstance(typeof(T)) as T;
            AssetDatabase.CreateAsset(value,path);
        }
        return value;
    }
    private static TextureImporter Configure(string path)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;
        importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;
        importer.maxTextureSize=4096;importer.wrapMode=TextureWrapMode.Clamp;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;importer.SetTextureSettings(settings);
        return importer;
    }
    private static Sprite Sprite(string category,string name,int border=0)
    {
        string path=Art+name+".png";File.Copy(Source+"Selected/"+category+"/"+name+".png",path,true);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=Configure(path);importer.spriteImportMode=SpriteImportMode.Single;
        importer.spriteBorder=new Vector4(border,border,border,border);importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    private static Tile Tile(string name)
    {
        var tile=LoadOrCreate<Tile>(Art+name+"Tile.asset");tile.sprite=Sprite("Environment",name);
        tile.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.None;EditorUtility.SetDirty(tile);return tile;
    }
    private static void ImportTheme()
    {
        var theme=Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme");
        // Reserve room for the approved 96px apex at the same texel scale.
        theme.minimumBattleHeight=384; // Native 96px apex plus readable counters below top controls.
        theme.boardCells=Enumerable.Range(1,3).Select(i=>Sprite("UI","Log_Cell_0"+i)).ToArray();
        theme.buttonNormal=Sprite("UI","ButtonNormal",12);theme.buttonHighlighted=Sprite("UI","ButtonHighlighted",12);
        theme.buttonPressed=Sprite("UI","ButtonPressed",12);theme.buttonDisabled=Sprite("UI","ButtonDisabled",12);
        theme.settingsNormal=Sprite("UI","SettingsNormal");theme.settingsHighlighted=Sprite("UI","SettingsHighlighted");
        theme.settingsPressed=Sprite("UI","SettingsPressed");theme.settingsDisabled=Sprite("UI","SettingsDisabled");
        theme.panelShell=Sprite("UI","PanelShell",12);theme.energyFrame=Sprite("UI","EnergyFrame");
        theme.abilityIcons=new[]{
            new GameplayThemeDefinition.IconReplacement{source=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/UI/Ability UI/Royal_Decree_Button_new.png"),themed=Sprite("UI","AbilityRoyal")},
            new GameplayThemeDefinition.IconReplacement{source=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/UI/Ability UI/Horrible_Harmony.png"),themed=Sprite("UI","AbilityHarmony")},
            new GameplayThemeDefinition.IconReplacement{source=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/GideonGlass/Gideon_AbilityButton.png"),themed=Sprite("UI","AbilityCamera")}
        };
        if(theme.vineSpreadFrames==null || theme.vineSpreadFrames.Length==0) theme.vineOverlay=Sprite("Effects","Vine_Overlay");
        theme.anchorOverlay=Sprite("Effects","Vine_Anchor");
        theme.vineWarning=Sprite("Effects","Vine_Warning");theme.healEffect=Sprite("Effects","Leaf_Heal");
        theme.interruptEffect=Sprite("Effects","Interrupt");theme.resonanceIcon=Sprite("Effects","Resonance");
        theme.supplyNormal=Sprite("UI","SupplyNormal");theme.supplyHighlighted=Sprite("UI","SupplyHighlighted");
        theme.supplyPressed=Sprite("UI","SupplyPressed");theme.supplyDisabled=Sprite("UI","SupplyDisabled");
        var distant=Tile("Tall_Woodland");var soil=Tile("Woodland_Ground");
        var tree=Tile("Ancient_Tree");var fern=Tile("Forest_Fern");var ruin=Tile("Forest_Ruin");
        string path="Assets/_Game/Resources/BattleEnvironments/Forest_Woodland.prefab";
        var root=PrefabUtility.LoadPrefabContents("Assets/_Game/Resources/BattleEnvironments/Dungeon_Default.prefab");
        try
        {
            root.name="Forest_Woodland";
            var environment=new SerializedObject(root.GetComponent<BattleEnvironmentRoot>());
            environment.FindProperty("environmentId").stringValue="forest-woodland";environment.ApplyModifiedPropertiesWithoutUndo();
            foreach(var map in root.GetComponentsInChildren<Tilemap>())
            { map.ClearAllTiles();map.transform.localPosition=Vector3.zero;map.tileAnchor=Vector3.zero; }
            var back=root.transform.Find("BackWall").GetComponent<Tilemap>();
            var mid=root.transform.Find("Architecture").GetComponent<Tilemap>();
            var ground=root.transform.Find("Floor").GetComponent<Tilemap>();
            var detail=root.transform.Find("BackDecor").GetComponent<Tilemap>();
            var front=root.transform.Find("AtmosphereProps").GetComponent<Tilemap>();
            for(int x=-16;x<=16;x+=4)
            {
                for(int y=-12;y<=12;y+=4) Place(back,distant,x,y+1.75f,(x/4)%2!=0);
            }
            // The rendered integer-size bodies stand above the anchor by up to
            // 32 native pixels. Soil extends 48px above it, behind their feet.
            // Actor transforms and source-pixel density remain unchanged.
            for(int x=-16;x<=16;x+=2)
                for(int y=0;y>=-12;y--) Place(ground,soil,x,y+.25f,(x/2)%2!=0);
            foreach(int side in new[]{-1,1})
            {
                Place(mid,tree,side*3.5f,1,side>0);
                Place(detail,ruin,side*2.7f,.4f,side>0);
                Place(front,fern,side*3.25f,-.6f,side>0);
            }
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        theme.battleEnvironment=AssetDatabase.LoadAssetAtPath<GameObject>(path);EditorUtility.SetDirty(theme);
    }
    private static void Place(Tilemap map,Tile tile,float x,float y,bool flip=false)
    {
        var cell=new Vector3Int(Mathf.FloorToInt(x),Mathf.FloorToInt(y),0);map.SetTile(cell,tile);map.SetTileFlags(cell,TileFlags.None);
        map.SetTransformMatrix(cell,Matrix4x4.TRS(new Vector3(x-cell.x,y-cell.y,0),Quaternion.identity,new Vector3(flip?-1:1,1,1)));
    }
    [MenuItem("Dungeon Matcher/Forest/Import production kits")]
    public static void ImportKits()
    {
        var definitions=Names.Select(n=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(Data+n+".asset")).ToArray();
        float[] seconds={3,4.5f,5,5.5f,4.5f,5};
        string[] roles={"Ranged attacker","Heavy attacker","Ally healer","Disruptor","Root protector","Ritual leader"};
        for(int i=0;i<definitions.Length;i++)
        {
            var so=new SerializedObject(definitions[i]);
            so.FindProperty("baseAttackInterval").floatValue=seconds[i];
            so.FindProperty("combatRole").stringValue=roles[i];
            so.FindProperty("description").stringValue="Forest starter test kit. Seconds basics; accepted-move abilities. See the combat guide for counterplay.";
            if(i>=4)
            {
                so.FindProperty("hasSpecialAbility").boolValue=true;
                so.FindProperty("specialAbilityKind").intValue=i==4?(int)EnemySpecialAbilityKind.GuardingRoots:(int)EnemySpecialAbilityKind.GroveRenewal;
                so.FindProperty("baseSpecialTurnRequirement").intValue=i==4?2:3;
                so.FindProperty("lockSpecialTurnRequirement").boolValue=true;
                so.FindProperty("isSupport").boolValue=i==5;so.FindProperty("isBoardDisruptor").boolValue=true;
            }
            so.FindProperty("forestRenewalBaseHeal").intValue=20;
            so.FindProperty("forestHeartrootHealBonus").intValue=20;
            so.FindProperty("forestHarvestBaseDamage").intValue=20;
            so.FindProperty("forestHarvestDamagePerVine").intValue=5;
            so.FindProperty("forestRenewalChannelMoves").intValue=2;
            so.FindProperty("forestHarvestChannelMoves").intValue=3;
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(definitions[i]);
        }
        var zone=Resources.Load<ZoneDefinition>("Zones/magical-forest");
        zone.displayName="Magical Forest";
        zone.growsVines=true;zone.vineCadenceMoves=2;zone.maximumVineSpreadPerPulse=2;zone.maximumVineOverlays=12;
        zone.developmentEncounters=new[]{
            Encounter("Heal response",definitions,1,2,0),Encounter("Root response",definitions,3,0,1),
            Encounter("Combined pressure",definitions,2,3,1),
            Encounter("Scout solo",definitions,0),Encounter("Trailguard solo",definitions,1),
            Encounter("Mender solo (no self heal)",definitions,2),Encounter("Rootbinder solo",definitions,3),
            Encounter("Warden solo",definitions,4),Encounter("Matriarch solo",definitions,5),
            Encounter("Warden and Scout",definitions,4,0),Encounter("Warden and Trailguard",definitions,4,1),
            Encounter("Matriarch first lesson",definitions,5,0,1),Encounter("Matriarch with roots",definitions,5,3,0)
        };
        zone.music=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Music/Forest Lanterns - TEMP review.wav");
        EditorUtility.SetDirty(zone);AssetDatabase.SaveAssets();
        Debug.Log("Forest production: six kits, 13 deterministic fixtures, authored seconds basics, original temporary music.");
    }
    private static ZoneTestEncounter Encounter(string label,EnemyDefinition[] definitions,params int[] members) =>
        new ZoneTestEncounter{label=label,members=members.Select(i=>definitions[i]).ToArray()};
}
