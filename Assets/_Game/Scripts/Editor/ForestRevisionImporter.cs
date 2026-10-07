using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Only the requested root variants and two existing release contacts.</summary>
public static class ForestRevisionImporter
{
    public static void Run()
    {
        var theme=Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme");
        theme.shieldRootLevelOne=Import("ShieldRoot_1");theme.shieldRootLevelTwo=Import("ShieldRoot_2");
        theme.heartrootLevelOne=Import("HeartRoot_1");theme.heartrootLevelTwo=Import("HeartRoot_2");
        EditorUtility.SetDirty(theme);
        foreach(string name in new[]{"Orc_Rootbinder","Barkhide_Warden"})
        {
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_Game/Animations/Forest/"+name+"_Release.anim");
            AnimationUtility.SetAnimationEvents(clip,new[]{
                new AnimationEvent{time=.30f,functionName="AbilityBeat",intParameter=1},
                new AnimationEvent{time=clip.length-.01f,functionName="AbilityComplete"}});
            EditorUtility.SetDirty(clip);
            var data=new SerializedObject(AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Forest/"+name+".asset"));
            data.FindProperty("timeSpecialAbilityFromAnimation").boolValue=true;
            data.FindProperty("useAuthoredSpecialAbilityMotion").boolValue=true;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Forest root variants and existing release contacts imported.");
    }
    private static Sprite Import(string name)
    {
        string path="Assets/_Game/Art/Board/ApprovedBlockers/"+name+".png";
        File.Copy("ArtSource/Forest/RosterRevision/"+name+".png",path,true);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
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
