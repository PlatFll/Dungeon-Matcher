using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Additive native art import. Kit values and other zones are not rewritten.</summary>
public static class IronveinArtImporter
{
    public static readonly string[] Ids={"pickaxe_delver","rivet_gunner","packbeetle","ore_hauler",
        "stonewright","bore_engineer","vein_surveyor","powder_sapper","rail_switcher","seismic_smith",
        "siege_machinist","obsidian_sentinel","grand_delver","rivet_turret"};
    public const string Art="Assets/_Game/Art/Ironvein/";
    private const string Source="ArtSource/Ironvein/";
    [Serializable] private sealed class Manifest { public Clip[] clips; }
    [Serializable] private sealed class Clip { public string name,state; }

    [MenuItem("Dungeon Matcher/Ironvein/Import native stills")]
    public static void ImportStills()
    {
        EditorUtility.audioMasterMute=true;
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        foreach(string id in Ids)
        {
            string source=id=="grand_delver"?"Inputs/grand_delver-tip-repaired.png":
                id=="seismic_smith"?"Corrections/smith-edge/00.png":"Inputs/"+id+".png";
            var sprite=ImportSprite(Source+source,id,true);
            var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(IronveinImporter.EnemyPath+id+".asset");
            if(definition==null) throw new InvalidDataException("Missing Ironvein definition: "+id);
            var so=new SerializedObject(definition);
            so.FindProperty("fallbackVisualSprite").objectReferenceValue=sprite;
            so.FindProperty("visualSize").vector2Value=sprite.rect.size*3;
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(definition);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Ironvein: fourteen native stills imported; action margins preserve source pixel scale.");
    }

    [MenuItem("Dungeon Matcher/Ironvein/Import reviewed motion")]
    public static void ImportMotion()
    {
        ImportStills();
        var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Source+"Motion/animation-manifest.json"));
        string[] names=manifest.clips.Select(c=>c.name).Distinct().ToArray();
        foreach(string name in names)
            if(!Ids.Contains(name) || !manifest.clips.Any(c=>c.name==name&&c.state=="Idle") ||
                !manifest.clips.Any(c=>c.name==name&&c.state=="AutoAttack"))
                throw new InvalidDataException("Motion import needs a known identity, Idle and AutoAttack: "+name);
        ForestProductionImporter.ImportMotionSet(names,Source+"Motion/",Art,
            "Assets/_Game/Animations/Ironvein/",IronveinImporter.EnemyPath);
        AssetDatabase.SaveAssets();
        Debug.Log("Ironvein: imported reviewed native motion for "+names.Length+" identities.");
    }

    internal static Sprite ImportSprite(string source,string name,bool bottom=false,int border=0,bool repeat=false)
    {
        string path=Art+name+".png";File.Copy(source,path,true);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;
        importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;
        importer.wrapMode=repeat?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
        importer.spriteBorder=new Vector4(border,border,border,border);
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;
        settings.spriteAlignment=(int)(bottom?SpriteAlignment.BottomCenter:SpriteAlignment.Center);
        importer.SetTextureSettings(settings);importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
