using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Scoped native pearl import. Does not rebuild environments or enemy art.</summary>
public static class CourtRevisionImporter
{
    public static void Run()
    {
        var theme=Resources.Load<GameplayThemeDefinition>("Zones/DrownedCourtTheme");
        theme.airCoffer=Import("PearlCoffer_L1");theme.armoredAirCoffer=Import("PearlCoffer_L2");
        theme.exposedPearl=Import("ExposedPearl");theme.tributePearl=Import("TributePearl");
        theme.shellFragment=Import("ShellFragment");theme.pearlPop=Import("PearlPop");
        theme.waterMicroBubble=Import("WaterMicroBubble");
        theme.waterRippleFrames=new[]{Import("WaterRipple_1"),Import("WaterRipple_2"),Import("WaterRipple_3")};
        EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();
        Debug.Log("Court native pearl family imported; existing scene/prefab art preserved.");
    }
    private static Sprite Import(string name)
    {
        string path="Assets/_Game/Art/DrownedCourt/"+name+".png";
        File.Copy("ArtSource/DrownedCourt/RosterRevision/"+name+".png",path,true);
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
