using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Explicit additive import, preserving the existing zones and source art.</summary>
public static class DrownedCourtImporter
{
    public static readonly string[] Ids = { "reef_spearman", "hammerhead_bruiser", "needlefin_skirmisher", "shellback_porter",
        "pearl_thief", "pearl_cantor", "conch_marshal", "moray_siphoner", "reef_netweaver", "puffer_sentinel",
        "breakwater_captain", "lantern_warden", "queen_nacre", "skittercrab" };
    private const string Source = "ArtSource/DrownedCourt/Production/Selected/";
    private const string Art = "Assets/_Game/Art/DrownedCourt/";
    private const string Data = "Assets/_Game/Data/Enemies/DrownedCourt/";
    private const string Animation = "Assets/_Game/Animations/DrownedCourt/";
    public static string CultureFor(string id) => id == "pearl_cantor" || id == "conch_marshal" ||
        id == "lantern_warden" || id == "queen_nacre" ? "Nacre Court" : "Reef Clans";
    [Serializable] private sealed class Recipes { public Recipe[] recipes; }
    [Serializable] private sealed class Recipe { public string id, name; public string[] members; public int first, last, tide, weight = 1; }

    [MenuItem("Dungeon Matcher/Drowned Court/Import stills and kits")]
    public static void ImportKits()
    {
        Directory.CreateDirectory(Art); Directory.CreateDirectory(Data); AssetDatabase.Refresh();
        var shell = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Forest/Elven_Scout.asset").EnemyPrefab;
        int[] hp = { 60,110,50,120,65,75,90,80,85,100,180,210,320,25 };
        int[] damage = { 10,20,5,10,5,5,10,5,5,10,20,15,15,5 };
        float[] seconds = { 4.8f,6,4.2f,6.5f,6,5.5f,6,6,6,5.8f,7,6.5f,6.5f,2.8f };
        var kinds = new[] { EnemySpecialAbilityKind.None, EnemySpecialAbilityKind.None, EnemySpecialAbilityKind.None,
            EnemySpecialAbilityKind.None, EnemySpecialAbilityKind.PearlTheft, EnemySpecialAbilityKind.ChannelHeal,
            EnemySpecialAbilityKind.RallyingConch, EnemySpecialAbilityKind.MoraySiphon, EnemySpecialAbilityKind.ThornySnare,
            EnemySpecialAbilityKind.SpineGuard, EnemySpecialAbilityKind.BreakwaterCommand, EnemySpecialAbilityKind.LanternPressure,
            EnemySpecialAbilityKind.AbyssalRegent, EnemySpecialAbilityKind.None };
        string[] descriptions = {
            "One trident thrust. Can fight in dry or flooded chambers.", "One slow, heavy two-handed club strike.",
            "Two quick dart hits. Each hit resolves shields separately.", "Sturdy shell porter with a single mallet strike; no hidden armor.",
            "Pearl Theft: two moves to pop marked bubbles before a one-hit coffer captures them. Break the coffer or defeat its owner to recover air.",
            "Pearl Hymn: two-move heal channel on one fixed other ally below 75% HP. Stagger or defeat the cantor to interrupt.",
            "Rallying Conch: other living allies gain 30% basic DAMAGE for five seconds. Refreshes; never stacks with another conch.",
            "Siphon: two-move channel for 20 damage. Heals only actual player HP lost; shields deny the heal.",
            "Thorny Snare: two gems marked for three moves. An opening manual match costs one AIR per snared gem while flooded; cascades, abilities and specials clear safely.",
            "Spine Guard: inflated for two moves. Opening manual-match damage retaliates once for five damage per move; cascades and specials are safe.",
            "Alternates Shellguard on one ally and a two-move Boarding Order for one fixed ally's normal attack sequence. Stagger interrupts the order.",
            "Alternates Air Levy (dry: Deepguard) and Pressure Lance. Pop air or break a coffer to reduce a wet lance; dry lances mark a gem. Stagger cancels.",
            "Royal Seizure, Crushing Depths and Court Muster. Answer two ripple marks or recover air to weaken Depths. Always damageable. Defeat the whole formation to travel.",
            "Small independent summon with fast nips. Survives its summoner; grants no independent farming reward." };
        var defs = new EnemyDefinition[Ids.Length];
        for (int i = 0; i < Ids.Length; i++)
        {
            string id = Ids[i]; var def = Load<EnemyDefinition>(Data + id + ".asset"); defs[i] = def;
            var so = new SerializedObject(def);
            so.FindProperty("enemyId").stringValue = id;
            so.FindProperty("displayName").stringValue = string.Join(" ", id.Split('_').Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)));
            so.FindProperty("description").stringValue = descriptions[i];
            so.FindProperty("race").stringValue = i == 13 ? "Reef crustacean" : "Marine folk";
            so.FindProperty("faction").stringValue = CultureFor(id);
            so.FindProperty("combatRole").stringValue = i == 13 ? "Summon" : kinds[i].ToString();
            var zones = so.FindProperty("eligibleZones"); zones.arraySize = 1; zones.GetArrayElementAtIndex(0).stringValue = "drowned-court";
            so.FindProperty("category").intValue = i == 12 ? 3 : i == 10 || i == 11 ? 2 : i >= 4 && i <= 9 ? 1 : 0;
            so.FindProperty("enemyPrefab").objectReferenceValue = shell;
            so.FindProperty("fallbackVisualSprite").objectReferenceValue = ImportSprite(Source + id + ".png", id, true);
            so.FindProperty("visualSize").vector2Value = new Vector2(192,192);
            so.FindProperty("baseMaxHealth").intValue = hp[i]; so.FindProperty("baseDamage").intValue = damage[i];
            so.FindProperty("baseFollowUpDamage").intValue = i == 2 ? 5 : 0;
            so.FindProperty("followUpAttackDelay").floatValue = i == 2 ? .12f : 0;
            so.FindProperty("baseAttackInterval").floatValue = seconds[i];
            so.FindProperty("hasSpecialAbility").boolValue = kinds[i] != EnemySpecialAbilityKind.None;
            so.FindProperty("specialAbilityKind").intValue = (int)kinds[i];
            so.FindProperty("baseSpecialTurnRequirement").intValue = i == 5 ? 2 : i >= 10 ? 3 : 4;
            so.FindProperty("lockSpecialTurnRequirement").boolValue = true;
            so.FindProperty("isSupport").boolValue = i == 5 || i == 6;
            so.FindProperty("isBoardDisruptor").boolValue = i == 4 || i == 8 || i == 11 || i == 12;
            so.FindProperty("threatCost").floatValue = i == 13 ? 1 : i >= 10 ? 5 : i >= 4 ? 2.5f : 1.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            def.canFightFlooded = true; def.aquaticAbilityDamage = 20;
            def.aquaticChannelMoves = 2;
            def.aquaticShield = i == 12 ? 20 : 25; def.aquaticRallyDamage = 1.3f; def.aquaticRallySeconds = 5;
            EditorUtility.SetDirty(def);
        }
        defs[12].aquaticSummon = defs[13]; EditorUtility.SetDirty(defs[12]);
        var zone = Load<ZoneDefinition>("Assets/_Game/Resources/Zones/drowned-court.asset");
        zone.zoneId = "drowned-court"; zone.displayName = "The Drowned Court"; zone.affiliatedGem = GemType.Sapphire;
        zone.affiliatedDamageMultiplier = 1.15f; zone.periodicallyFloods = true; zone.enemies = defs; zone.apexEnemy = defs[12]; zone.apexLocalWave = 20;
        zone.floodMinimumMoves=16;zone.floodMaximumMoves=18;zone.initialAirBubbles=5;
        zone.emergencyAirCadenceMoves=3;zone.emergencyAirSupply=1;zone.criticalAirSupply=2;zone.criticalAirThreshold=1;
        zone.developmentEncounters = defs.Select(d => new ZoneTestEncounter { label = "Court fixture: " + d.DisplayName,
            members = d == defs[0] ? new[] { d } : new[] { d, defs[0] } }).ToArray();
        var recipes = JsonUtility.FromJson<Recipes>(File.ReadAllText(Source + "encounters.json"));
        zone.liveEncounters = recipes.recipes.Select(r => new ZoneTestEncounter { label = r.id + ": " + r.name,
            firstLocalWave = r.first, lastLocalWave = r.last, requiredTide = r.tide, weight = r.weight,
            members = r.members.Select(id => defs[Array.IndexOf(Ids, id)]).ToArray() }).ToArray();
        EditorUtility.SetDirty(zone); AssetDatabase.SaveAssets();
    }

    [MenuItem("Dungeon Matcher/Drowned Court/Import reviewed motion")]
    public static void ImportMotion()
    {
        ForestProductionImporter.ImportMotionSet(Ids, Source + "Motion/", Art, Animation, Data);
        AssetDatabase.SaveAssets();
    }

    public static void ImportPresentation()
    {
        ImportMotion();
        ImportTheme();
    }

    [MenuItem("Dungeon Matcher/Drowned Court/Import gameplay materials")]
    public static void ImportTheme()
    {
        Directory.CreateDirectory(Art); AssetDatabase.Refresh();
        var theme=Load<GameplayThemeDefinition>("Assets/_Game/Resources/Zones/DrownedCourtTheme.asset");
        Sprite S(string n,int border=0,bool repeat=false)=>ImportSprite(Source+"UI/"+n+".png",n,false,border,repeat);
        theme.minimumBattleHeight=352;
        theme.generalBackground=S("general_stone",0,true);theme.panelBackground=S("inset_panel",0,true);
        theme.frameCorner=S("FrameCorner");theme.frameEdge=S("FrameEdge");theme.wavePlaque=S("WavePlaque");
        theme.boardCells=Enumerable.Range(0,3).Select(i=>S("cell_"+i)).ToArray();
        theme.buttonNormal=S("ButtonNormal",8);theme.buttonHighlighted=S("ButtonHighlighted",8);
        theme.buttonPressed=S("ButtonPressed",8);theme.buttonDisabled=S("ButtonDisabled",8);
        theme.settingsNormal=S("SettingsNormal");theme.settingsHighlighted=S("SettingsHighlighted");
        theme.settingsPressed=S("SettingsPressed");theme.settingsDisabled=S("SettingsDisabled");
        theme.supplyNormal=S("SupplyNormal");theme.supplyHighlighted=S("SupplyHighlighted");
        theme.supplyPressed=S("SupplyPressed");theme.supplyDisabled=S("SupplyDisabled");
        theme.panelShell=S("PanelShell",16);
        theme.energyFrame=S("EnergyFrame");
        var forestTheme=Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme");
        string[] glyphs={"AbilityRoyal","AbilityHarmony","AbilityCamera"};
        theme.abilityIcons=forestTheme.abilityIcons.Select((icon,i)=>new GameplayThemeDefinition.IconReplacement
            {source=icon.source,themed=S(glyphs[i])}).ToArray();
        theme.airBubble=S("air_bubble");theme.thornySnare=S("thorny_snare");
        theme.airCoffer=S("air_coffer");theme.pressureSeal=S("pressure_seal");
        // Use the game's actual Sapphire identity as the affiliation badge.
        theme.resonanceIcon=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Gems/Small gems/Gems32/Sapphire32.png");
        theme.healthStyles=Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme").healthStyles;
        var player=Load<PlayerAreaFrameProfile>("Assets/_Game/Resources/Zones/DrownedCourtPlayerFrame.asset");
        var so=new SerializedObject(player);so.FindProperty("playerSectionWidth").floatValue=146;
        so.FindProperty("topPiece").objectReferenceValue=S("PlayerTop");
        so.FindProperty("middlePiece").objectReferenceValue=S("PlayerMiddle");
        so.FindProperty("bottomPiece").objectReferenceValue=S("PlayerBottom");
        so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(player);theme.playerFrame=player;
        Tile T(string name)
        {var t=Load<Tile>(Art+name+"Tile.asset");t.sprite=S(name);t.colliderType=Tile.ColliderType.None;EditorUtility.SetDirty(t);return t;}
        var back=T("court_background");var floor=T("court_floor");var column=T("coral_column");var coral=T("coral_foreground");
        string path="Assets/_Game/Resources/BattleEnvironments/DrownedCourt.prefab";
        var root=PrefabUtility.LoadPrefabContents("Assets/_Game/Resources/BattleEnvironments/Dungeon_Default.prefab");
        try
        {
            root.name="DrownedCourt";var env=new SerializedObject(root.GetComponent<BattleEnvironmentRoot>());
            env.FindProperty("environmentId").stringValue="drowned-court";env.ApplyModifiedPropertiesWithoutUndo();
            foreach(var map in root.GetComponentsInChildren<Tilemap>())
            {map.ClearAllTiles();map.transform.localPosition=Vector3.zero;map.tileAnchor=Vector3.zero;}
            var distant=root.transform.Find("BackWall").GetComponent<Tilemap>();
            var ground=root.transform.Find("Floor").GetComponent<Tilemap>();
            var architecture=root.transform.Find("Architecture").GetComponent<Tilemap>();
            var foreground=root.transform.Find("AtmosphereProps").GetComponent<Tilemap>();
            void Place(Tilemap map,Tile tile,float x,float y)
            {var cell=new Vector3Int(Mathf.FloorToInt(x),Mathf.FloorToInt(y),0);map.SetTile(cell,tile);map.SetTileFlags(cell,TileFlags.None);
             map.SetTransformMatrix(cell,Matrix4x4.Translate(new Vector3(x-cell.x,y-cell.y,0)));}
            for(int x=-16;x<=16;x+=4)for(int y=-12;y<=12;y+=2)Place(distant,back,x,y+1.75f);
            for(int x=-16;x<=16;x+=2)for(int y=0;y>=-12;y--)Place(ground,floor,x,y+.25f);
            foreach(int side in new[]{-1,1}) {Place(architecture,column,side*3.6f,1.1f);Place(foreground,coral,side*3.4f,-.6f);}
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally {PrefabUtility.UnloadPrefabContents(root);}
        theme.battleEnvironment=AssetDatabase.LoadAssetAtPath<GameObject>(path);EditorUtility.SetDirty(theme);
        theme.battleEnvironmentVariants=new GameObject[3];theme.battleEnvironmentVariants[0]=theme.battleEnvironment;
        for(int variant=1;variant<=2;variant++)
        {
            var composition=PrefabUtility.LoadPrefabContents(path);
            try
            {
                string name=variant==1?"DrownedCourt_CoralCloister":"DrownedCourt_Throne";
                composition.name=name;
                var backdrop=composition.transform.Find("BackWall").GetComponent<Tilemap>();
                backdrop.transform.localPosition=new Vector3(variant==1?1.5f:0,variant==1?.5f:1,0);
                var pillars=composition.transform.Find("Architecture").GetComponent<Tilemap>();pillars.ClearAllTiles();
                foreach(int side in new[]{-1,1})
                {
                    int x=side*(variant==1?2:3);var cell=new Vector3Int(x,0,0);
                    pillars.SetTile(cell,column);pillars.SetTileFlags(cell,TileFlags.None);
                    pillars.SetTransformMatrix(cell,Matrix4x4.Translate(new Vector3(0,1.2f,0)));
                    if(variant==2){var second=new Vector3Int(x+side,0,0);pillars.SetTile(second,column);pillars.SetTileFlags(second,TileFlags.None);
                        pillars.SetTransformMatrix(second,Matrix4x4.Translate(new Vector3(0,1.2f,0)));}
                }
                string destination="Assets/_Game/Resources/BattleEnvironments/"+name+".prefab";
                PrefabUtility.SaveAsPrefabAsset(composition,destination);
                theme.battleEnvironmentVariants[variant]=AssetDatabase.LoadAssetAtPath<GameObject>(destination);
            }
            finally{PrefabUtility.UnloadPrefabContents(composition);}
        }
        var zone=Resources.Load<ZoneDefinition>("Zones/drowned-court");zone.theme=theme;
        zone.music=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Music/Drowned Court - TEMP review.wav");
        var music=(AudioImporter)AssetImporter.GetAtPath("Assets/_Game/Audio/Music/Drowned Court - TEMP review.wav");
        var audio=music.defaultSampleSettings;audio.loadType=AudioClipLoadType.Streaming;
        audio.compressionFormat=AudioCompressionFormat.Vorbis;audio.quality=.8f;
        audio.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;music.defaultSampleSettings=audio;
        music.forceToMono=false;music.SaveAndReimport();
        EditorUtility.SetDirty(zone);AssetDatabase.SaveAssets();
    }

    internal static T Load<T>(string path) where T : ScriptableObject
    {
        var result = AssetDatabase.LoadAssetAtPath<T>(path);
        if (result == null) { result = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(result, path); }
        return result;
    }
    internal static Sprite ImportSprite(string source, string name, bool bottom = false, int border = 0, bool repeat = false)
    {
        string path = Art + name + ".png"; File.Copy(source, path, true);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 64; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None; importer.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        importer.spriteBorder = new Vector4(border,border,border,border);
        if (name.StartsWith("Button", StringComparison.Ordinal)) importer.spriteBorder = new Vector4(16,8,16,8);
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)(bottom ? SpriteAlignment.BottomCenter : SpriteAlignment.Center);
        importer.SetTextureSettings(settings); importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
