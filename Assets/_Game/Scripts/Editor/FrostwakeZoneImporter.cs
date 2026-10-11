using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Additive mountain data; never invokes another zone's rebuilding importer.</summary>
public static class FrostwakeZoneImporter
{
    public const string Data="Assets/_Game/Data/Enemies/Frostwake/";
    public const string Art="Assets/_Game/Art/Frostwake/";
    public const string ZonePath="Assets/_Game/Resources/Zones/frostwake-peaks.asset";
    public static readonly string[] Ids={"iceborn_raider","harpoon_hunter","frostfang_wolf","rimecarver","shieldmaiden",
        "rime_skald","hearthkeeper","frostblade_reaver","glacier_troll","iceborn_champion","jarl_haldra"};
    public static T Load<T>(string path) where T:ScriptableObject
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));AssetDatabase.Refresh();
        var asset=AssetDatabase.LoadAssetAtPath<T>(path);
        if(asset==null){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}
        return asset;
    }
    public static Sprite Sprite(string source,string id,bool actor=false,int border=0,bool repeat=false)
    {
        Directory.CreateDirectory(Art);string path=Art+id+".png";File.Copy(source,path,true);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Sprite;
        t.spriteImportMode=SpriteImportMode.Single;t.spritePixelsPerUnit=64;t.filterMode=FilterMode.Point;
        t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.alphaIsTransparency=true;
        t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;t.wrapMode=repeat?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
        t.spriteBorder=new Vector4(border,border,border,border);var settings=new TextureImporterSettings();t.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)(actor?SpriteAlignment.Custom:SpriteAlignment.Center);
        if(actor)settings.spritePivot=new Vector2(.5f,4f/104f);t.SetTextureSettings(settings);t.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    [MenuItem("Dungeon Matcher/Frostwake/Import approved stills and kits")]
    public static void ImportKits()
    {
        EditorUtility.audioMasterMute=true;Directory.CreateDirectory(Data);AssetDatabase.Refresh();
        var shell=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Forest/Elven_Scout.asset").EnemyPrefab;
        var rules=Load<FrostwakeRules>("Assets/_Game/Resources/Zones/FrostwakeRules.asset");
        rules.frostbite=Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Frostbite");
        int[] hp={55,70,40,75,85,70,70,90,205,220,340};
        int[] damage={10,10,5,5,10,5,5,5,15,15,15};
        int[] moves={4,4,2,5,5,5,5,4,6,5,5};
        int[] specials={5,4,5,5,6,5,5,5,5,5,5};
        var kinds=new[]{EnemySpecialAbilityKind.None,EnemySpecialAbilityKind.MountainStrike,EnemySpecialAbilityKind.None,
            EnemySpecialAbilityKind.Rimecarving,EnemySpecialAbilityKind.ShieldingAllies,EnemySpecialAbilityKind.WarRhythm,
            EnemySpecialAbilityKind.ChannelHeal,EnemySpecialAbilityKind.ApplyPlayerStatus,EnemySpecialAbilityKind.MountainStrike,
            EnemySpecialAbilityKind.MountainStrike,EnemySpecialAbilityKind.GlacierCleave};
        string[] labels={"","Harpoon Hunt","","Carve Rime","Shieldwall","Marching Song","Hearthfire","Biting Cold","Avalanche Blow","Hallbreaker","Glacier Cleave"};
        string[] descriptions={
            "Iceborn traveller with a single axe strike. Straightforward pressure.",
            "Harpoon Hunt marks one gem for two moves. Clear or convert it to cancel and Stagger the hunter. Otherwise the gem is consumed and the harpoon deals 15 base damage. Basics pause while aiming.",
            "Low-health mountain wolf with a quick bite every two accepted moves. No board interference.",
            "Carve Rime safely encases one ordinary gem in Frost. The zone's normal thaw, escalation and global restriction cap apply. Existing ice persists after the carver dies.",
            "Shieldwall grants 10 ordinary shield to each other living ally and 15 to herself. Existing shield caps and whole-hit gating apply.",
            "Marching Song increases other living allies' basic attack speed by 35% for three accepted moves. Refreshes without stacking; does not cast alone. Defeating the skald ends its buff.",
            "Hearthfire channels for two accepted moves to heal 20 HP on one fixed other ally below 75% HP. Stagger or defeat the keeper to cancel. Basics pause while tending the flame.",
            "Biting Cold applies the shared Frostbite status for its authored duration. Normal attacks are two separate five-damage sword strikes; each contact respects shields.",
            "Alternates Glacial Armor (30 ordinary shield) with Avalanche Blow (two marked gems, two response moves, 25 base damage). Break its shield or clear either mark to Stagger it. Basics pause during the warning.",
            "Hallbreaker marks two gems for two moves. Clear either mark to cancel the full hammer blow and Stagger the champion. Otherwise both are consumed and the strike deals 30 base damage once.",
            "Glacier Cleave fully encases and marks one gem for three moves. Fully thaw or destroy it to cancel and Stagger Haldra. Otherwise she consumes it: Frost/Heavy Ice/Bifrost deals 20/35/50 base damage; Bifrost also applies Frostbite. Basics pause throughout the warning."};
        var defs=new EnemyDefinition[Ids.Length];
        for(int i=0;i<Ids.Length;i++)
        {
            string id=Ids[i];var def=Load<EnemyDefinition>(Data+id+".asset");defs[i]=def;var s=new SerializedObject(def);
            s.FindProperty("enemyId").stringValue=id;s.FindProperty("displayName").stringValue=i==10?"Jarl Haldra, the Icebreaker":string.Join(" ",id.Split('_').Select(x=>char.ToUpperInvariant(x[0])+x.Substring(1)));
            s.FindProperty("description").stringValue=descriptions[i];s.FindProperty("race").stringValue=i==2?"Wolf":i==8?"Troll":"Human";
            s.FindProperty("faction").stringValue="Iceborn Clans";s.FindProperty("combatRole").stringValue=i==2?"Fast attacker":i==4?"Shield support":i==5?"Speed support":i==6?"Healer":kinds[i].ToString();
            var zones=s.FindProperty("eligibleZones");zones.arraySize=1;zones.GetArrayElementAtIndex(0).stringValue="frostwake-peaks";
            s.FindProperty("category").intValue=i==10?3:i>=8?2:kinds[i]==EnemySpecialAbilityKind.None?0:1;
            s.FindProperty("enemyPrefab").objectReferenceValue=shell;
            s.FindProperty("fallbackVisualSprite").objectReferenceValue=Sprite("ArtSource/Frostwake/approved/"+id+".png",id,true);
            s.FindProperty("visualSize").vector2Value=new Vector2(128*3,104*3);
            s.FindProperty("baseMaxHealth").intValue=hp[i];s.FindProperty("baseDamage").intValue=damage[i];
            s.FindProperty("baseFollowUpDamage").intValue=i==7?5:0;s.FindProperty("followUpAttackDelay").floatValue=i==7?.12f:0;
            s.FindProperty("baseAttackInterval").floatValue=moves[i]*1.6f;
            s.FindProperty("unifiedFirstAttackMoves").intValue=moves[i];s.FindProperty("unifiedAttackMoves").intValue=moves[i];
            s.FindProperty("firstAttackMoves").intValue=moves[i];s.FindProperty("attackMoves").intValue=moves[i];
            s.FindProperty("hasSpecialAbility").boolValue=kinds[i]!=EnemySpecialAbilityKind.None;
            s.FindProperty("specialAbilityKind").intValue=(int)kinds[i];s.FindProperty("baseSpecialTurnRequirement").intValue=specials[i];
            s.FindProperty("lockSpecialTurnRequirement").boolValue=true;s.FindProperty("isSupport").boolValue=i>=4&&i<=6;
            s.FindProperty("isBoardDisruptor").boolValue=i==1||i==3||i>=8;
            s.FindProperty("threatCost").floatValue=i==10?7:i>=8?5:i==0||i==2?1.5f:i>=4&&i<=6?2.5f:2;
            s.FindProperty("allyShieldAmount").intValue=10;s.FindProperty("selfShieldAmount").intValue=15;
            s.FindProperty("warRhythmSpeed").floatValue=1.35f;s.FindProperty("warRhythmMoves").intValue=3;
            s.ApplyModifiedPropertiesWithoutUndo();def.abilityDisplayName=labels[i];def.frostwakeRules=rules;
            def.appliedPlayerStatus=i==7?rules.frostbite:null;def.mountainArmorShield=i==8?30:0;
            def.mountainMarkedGems=i>=8?2:1;def.mountainResponseMoves=2;def.mountainStrikeDamage=i==8?25:i==9?30:15;
            EditorUtility.SetDirty(def);
        }
        var zone=Load<ZoneDefinition>(ZonePath);zone.zoneId="frostwake-peaks";zone.displayName="Frostwake Peaks";
        zone.enemies=defs;zone.affiliatedGem=GemType.Sapphire;zone.frostwakeRules=rules;
        zone.eligibleForTesting=true;zone.apexEnemy=defs[10];zone.apexLocalWave=28;
        zone.developmentEncounters=defs.Select(d=>new ZoneTestEncounter{label=d.DisplayName+" fixture",members=d.IsSupport?new[]{d,defs[0]}:new[]{d}}).ToArray();
        BuildEncounters(zone);EditorUtility.SetDirty(rules);EditorUtility.SetDirty(zone);AssetDatabase.SaveAssets();
        Debug.Log("Frostwake: eleven approved native stills and move-based kits imported; live readiness requires completed theme/motion validation.");
    }
    private static void BuildEncounters(ZoneDefinition zone)
    {
        var budget=Load<WaveSpawnProfile>("Assets/_Game/Resources/Zones/FrostwakeEncounterBudget.asset");var s=new SerializedObject(budget);
        s.FindProperty("threatBudgetByWave").animationCurveValue=new AnimationCurve(new Keyframe(1,3),new Keyframe(4,4),new Keyframe(8,6.5f),new Keyframe(18,8),new Keyframe(26,10),new Keyframe(999,10));
        s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(budget);zone.encounterBudget=budget;
        EnemyDefinition E(string id)=>zone.enemies.Single(e=>e.EnemyId==id);
        var list=new List<ZoneTestEncounter>();
        void Add(string label,int first,int weight,params string[] ids)=>list.Add(new ZoneTestEncounter{label=label,firstLocalWave=first,weight=weight,members=ids.Select(E).ToArray()});
        Add("Raider lookout",1,3,"iceborn_raider");Add("Snow paws",1,3,"frostfang_wolf");
        Add("Pass patrol",2,4,"iceborn_raider","frostfang_wolf");Add("Travelling hunters",5,2,"iceborn_raider","iceborn_raider");
        Add("Wolf trail",5,2,"frostfang_wolf","frostfang_wolf");Add("Caravan outriders",8,2,"iceborn_raider","iceborn_raider","frostfang_wolf");
        void Lesson(string id,int first,int deadline,string prerequisite=null,bool leader=false)
        {
            foreach(string escort in E(id).IsSupport?new[]{"iceborn_raider","frostfang_wolf"}:new[]{"","iceborn_raider","frostfang_wolf"})
            {
                Add(E(id).DisplayName+(escort==""?" alone":" + "+escort),first,3,escort==""?new[]{id}:new[]{id,escort});
                var e=list.Last();e.introduction=E(id);e.introduceByLocalWave=deadline;e.oncePerVisit=leader;
                e.requiredSeen=prerequisite==null?Array.Empty<EnemyDefinition>():new[]{E(prerequisite)};
            }
        }
        Lesson("rimecarver",3,4);Lesson("harpoon_hunter",5,6,"rimecarver");Lesson("shieldmaiden",7,8,"harpoon_hunter");
        Lesson("glacier_troll",9,11,"shieldmaiden",true);Lesson("rime_skald",12,13,"glacier_troll");
        Lesson("hearthkeeper",14,15,"rime_skald");Lesson("frostblade_reaver",16,17,"hearthkeeper");
        Lesson("iceborn_champion",19,21,"frostblade_reaver",true);Lesson("jarl_haldra",26,28,"iceborn_champion",true);
        Add("Frozen pursuit",8,2,"harpoon_hunter","rimecarver");
        Add("Shielded caravan",9,3,"shieldmaiden","iceborn_raider","frostfang_wolf");
        Add("March through snow",14,2,"rime_skald","iceborn_raider","frostfang_wolf");
        Add("Warm sanctuary",16,2,"hearthkeeper","rimecarver","iceborn_raider");
        Add("Cold steel",18,2,"frostblade_reaver","harpoon_hunter","frostfang_wolf");
        Add("Clan vanguard",22,2,"shieldmaiden","frostblade_reaver","iceborn_raider");
        Add("Glacier ritual",23,2,"rimecarver","harpoon_hunter","rime_skald");
        zone.liveEncounters=list.ToArray();
    }
}
