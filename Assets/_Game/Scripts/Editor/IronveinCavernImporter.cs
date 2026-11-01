using System.IO;
using UnityEditor;
using UnityEngine;

// Explicit importer for the reviewed wide cave only. Does not re-run historic
// environment or character importers, or overwrite the original 240x160 source.
public static class IronveinCavernImporter
{
    public static void Import()
    {
        const string path="Assets/_Game/Art/IronveinTopCoverage/CavernWide.png";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.Copy("ArtSource/IronveinTopCoverage/NewCavern/Exports/CavernWide.png",path,true);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=64;
        importer.filterMode=FilterMode.Point;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled=false;
        importer.isReadable=true;
        importer.npotScale=TextureImporterNPOTScale.None;
        importer.wrapMode=TextureWrapMode.Clamp;
        importer.maxTextureSize=2048;
        importer.alphaIsTransparency=true;
        importer.ClearPlatformTextureSettings("Android");
        importer.ClearPlatformTextureSettings("Standalone");
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        var assembly=AssetDatabase.LoadAssetAtPath<IronveinBattleAssembly>("Assets/_Game/Resources/UI/BattlePresentation/IronveinAssembly.asset");
        assembly.cavernBackdrop=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        EditorUtility.SetDirty(assembly);AssetDatabase.SaveAssets();
        Debug.Log("Ironvein full native cave imported; original modular library preserved.");
    }
}
