using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

// Scoped to the two approved playables. Never reimports existing cast art.
public static class FrostwakePlayableImporter
{
    public const string ArtRoot="Assets/_Game/Art/FrostwakePlayables";
    public const string AnimationRoot="Assets/_Game/Animations/FrostwakePlayables";
    private const string Source="ArtSource/FrostwakePlayables/production";
    [Serializable] private sealed class Manifest { public Clip[] clips; }
    [Serializable] private sealed class Clip
    { public string name,character,state; public int width,height,pivotY; public int[] durations; public bool loop; }

    public static void RunBatch()
    {
        try { Run(); EditorApplication.Exit(0); }
        catch(Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
    [MenuItem("Dungeon Matcher/Art/Import approved new playables")]
    public static void Run()
    {
        Directory.CreateDirectory(ArtRoot);Directory.CreateDirectory(AnimationRoot);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Source+"/manifest.json"));
        foreach(string character in new[]{"Minotaur","Kobold"})
        {
            string path=AnimationRoot+"/"+character+".controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path)??AnimatorController.CreateAnimatorControllerAtPath(path);
            var machine=controller.layers[0].stateMachine;Sprite ready=null;
            foreach(var source in manifest.clips.Where(c=>c.character==character))
            {
                var frames=ImportSheet(Source+"/"+source.name+".png",source.name,source.width,source.height,source.durations.Length,source.pivotY);
                var clip=MakeClip(source,frames);
                var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name==source.state)??machine.AddState(source.state);
                state.motion=clip;state.speed=1;
                foreach(var transition in state.transitions.ToArray())state.RemoveTransition(transition);
                if(source.state=="Idle"){machine.defaultState=state;ready=frames[0];}
            }
            var hit=machine.states.Single(s=>s.state.name=="Hit").state;
            var idle=machine.states.Single(s=>s.state.name=="Idle").state;
            var exit=hit.AddTransition(idle);exit.hasExitTime=true;exit.exitTime=1;exit.duration=0;
            EditorUtility.SetDirty(controller);
            ConfigurePlayer(character,ready,controller);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Frostwake playable import: two definitions, native motion, effects and separate passive assigned.");
    }
    private static T Asset<T>(string path) where T:ScriptableObject
    {
        var asset=AssetDatabase.LoadAssetAtPath<T>(path);
        if(asset==null){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}
        return asset;
    }
    private static void ConfigurePlayer(string name,Sprite ready,AnimatorController controller)
    {
        bool minotaur=name=="Minotaur";
        var portrait=ImportSheet("ArtSource/FrostwakePlayables/locked/"+name+".png",name+"_Portrait",minotaur?80:64,minotaur?80:64,1,0)[0];
        var impact=ImportSheet(Source+"/"+(minotaur?"AttenderImpact":"RicochetImpact")+".png",minotaur?"AttenderImpact":"RicochetImpact",minotaur?48:32,32,5,16);
        CharacterAbilityDefinition ability=minotaur
            ?(CharacterAbilityDefinition)Asset<DungeonAttenderAbilityDefinition>("Assets/_Game/Data/Player Abilities/Ability_DungeonAttender.asset")
            :Asset<RicochetAbilityDefinition>("Assets/_Game/Data/Player Abilities/Ability_Ricochet.asset");
        var data=new SerializedObject(ability);
        Set(data,"abilityId",minotaur?"dungeon_attender":"ricochet");
        Set(data,"displayName",minotaur?"Labyrinth Charge":"Ricochet");
        Set(data,"description",minotaur?"Leap onto the board and charge through seven spaces. Prioritize obstructions; stubborn barriers take another collision. Uses normal barrier damage and your upgrades.":"Fire six ordered shots at board targets. Each shot can ricochet to a living enemy. Repeated hits on the same enemy gain up to 15 extra damage this cast.");
        Set(data,"buttonLabel",minotaur?"CHARGE":"RICOCHET");
        data.FindProperty("icon").objectReferenceValue=portrait;
        data.FindProperty("meterDisplay").enumValueIndex=1;
        data.FindProperty("energyCost").intValue=90;
        var array=data.FindProperty("impactFrames");array.arraySize=impact.Length;
        for(int i=0;i<impact.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=impact[i];
        if(minotaur)
        {
            data.FindProperty("boardMarker").objectReferenceValue=ImportSheet(Source+"/MinotaurMarker.png","MinotaurMarker",32,32,1,16)[0];
            data.FindProperty("movementBudget").intValue=7;
            data.FindProperty("jumpSeconds").floatValue=.4f;
            data.FindProperty("landingSeconds").floatValue=.3f;
            data.FindProperty("stepSeconds").floatValue=.18f;
            data.FindProperty("recoverySeconds").floatValue=.32f;
        }
        else
        {
            data.FindProperty("shots").intValue=6;
            data.FindProperty("baseEnemyDamage").intValue=5;
            data.FindProperty("spinSeconds").floatValue=.54f;
            data.FindProperty("shotSeconds").floatValue=.12f;
            data.FindProperty("recoverySeconds").floatValue=.18f;
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        CharacterPassiveDefinition passive=null;
        if(!minotaur)
        {
            passive=Asset<RepeatRicochetPassiveDefinition>("Assets/_Game/Data/Player Abilities/Passive_RepeatRicochet.asset");
            data=new SerializedObject(passive);Set(data,"passiveId","repeat_ricochet");Set(data,"displayName","By Ear");
            Set(data,"description","During each Ricochet, an enemy takes +5 damage for each prior shot that hit it, up to +15. Resets on the next cast.");
            data.FindProperty("damagePerPriorHit").intValue=5;data.FindProperty("maximumBonus").intValue=15;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        var player=Asset<PlayerDefinition>("Assets/_Game/Resources/Players/Player_"+(minotaur?"DungeonAttender":"KoboldGunner")+".asset");
        data=new SerializedObject(player);
        Set(data,"playerId",minotaur?CharacterSelectionSettings.MinotaurPlayerId:CharacterSelectionSettings.KoboldPlayerId);
        Set(data,"displayName",minotaur?"Dungeon Attender":"Kobold Gunner");
        Set(data,"description",minotaur?"The labyrinth's kindhearted keeper. His keys open doors; his horns clear everything else.":"He listens for where a shot will end up. One old revolver, six unlikely paths, and complete confidence.");
        data.FindProperty("battleCharacterSprite").objectReferenceValue=ready;
        data.FindProperty("menuPortrait").objectReferenceValue=portrait;
        data.FindProperty("battleAnimatorController").objectReferenceValue=controller;
        data.FindProperty("battleReferenceCanvas").vector2Value=new Vector2(64,64);
        data.FindProperty("activeAbility").objectReferenceValue=ability;
        data.FindProperty("passiveAbility").objectReferenceValue=passive;
        data.FindProperty("affinityGemType").enumValueIndex=(int)(minotaur?GemType.Amber:GemType.Ruby);
        data.FindProperty("baseMaxHealth").intValue=minotaur?120:85;
        data.FindProperty("healthPerLevel").intValue=minotaur?10:8;
        data.FindProperty("baseGemDamage").floatValue=10;
        data.FindProperty("gemDamagePerLevel").floatValue=minotaur?.55f:.6f;
        data.FindProperty("baseShieldCap").intValue=minotaur?50:35;
        data.FindProperty("shieldCapPerLevel").intValue=3;
        data.ApplyModifiedPropertiesWithoutUndo();
    }
    private static void Set(SerializedObject data,string field,string value)=>data.FindProperty(field).stringValue=value;
    private static Sprite[] ImportSheet(string source,string name,int width,int height,int count,int pivotY)
    {
        string path=ArtRoot+"/"+name+".png";File.Copy(source,path,true);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.wrapMode=TextureWrapMode.Clamp;
        importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
#pragma warning disable CS0618
        importer.spritesheet=Enumerable.Range(0,count).Select(i=>new SpriteMetaData{name=name+"_"+i.ToString("00"),rect=new Rect(i*width,0,width,height),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.5f,(float)pivotY/height)}).ToArray();
#pragma warning restore CS0618
        importer.SaveAndReimport();
        var frames=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
        if(frames.Length!=count)throw new InvalidDataException("Sprite count: "+name);
        return frames;
    }
    private static AnimationClip MakeClip(Clip source,Sprite[] frames)
    {
        string path=AnimationRoot+"/"+source.name+".anim";
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,path);}
        clip.name=source.name;clip.frameRate=100;clip.ClearCurves();
        var keys=new ObjectReferenceKeyframe[frames.Length+1];int ms=0;
        for(int i=0;i<frames.Length;i++)
        {keys[i]=new ObjectReferenceKeyframe{time=ms/1000f,value=frames[i]};ms+=source.durations[i];}
        keys[frames.Length]=new ObjectReferenceKeyframe{time=(ms-10)/1000f,value=frames[frames.Length-1]};
        AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(Image),"m_Sprite"),keys);
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=source.loop;
        settings.startTime=0;settings.stopTime=ms/1000f;AnimationUtility.SetAnimationClipSettings(clip,settings);
        EditorUtility.SetDirty(clip);return clip;
    }
}
