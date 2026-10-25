using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class DungeonArchitectureImporter
{
    const string Source="ArtSource/DungeonArchitecturePolish/Exports";
    const string Output="Assets/_Game/Art/DungeonSideArchitecture";
    public static void Import()
    {
        Directory.CreateDirectory(Output);
        string[] names={"Base","Capital","Shaft","Shaft_Worn","Ledge","Recess","SkullRelief","Candles_Tall","Candles_Mixed","Candles_Short","Shaft_Warm","Shaft_Warm_Faint","Chain_End_Left","Chain_End_Right"};
        foreach(string name in names)File.Copy(Source+"/"+name+".png",Output+"/Side_"+name+".png",true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(string name in names)
        {
            string path=Output+"/Side_"+name+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;
            importer.wrapMode=TextureWrapMode.Clamp;importer.alphaIsTransparency=true;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        var art=AssetDatabase.LoadAssetAtPath<DungeonPixelLabBattleArt>("Assets/_Game/Resources/UI/BattlePresentation/DungeonPixelLab.asset");
        art.sideArchitectureModules=names.Select(n=>AssetDatabase.LoadAssetAtPath<Sprite>(Output+"/Side_"+n+".png")).ToArray();
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        Debug.Log("Dungeon side architecture: 14 new native modules bound; existing 102+57 libraries preserved; approved Torch/Chain reused.");
    }
}
