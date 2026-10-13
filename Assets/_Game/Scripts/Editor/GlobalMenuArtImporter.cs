using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Explicitly imports only this task's native exports. Existing zones and assets are untouched.</summary>
public static class GlobalMenuArtImporter
{
    public static void Run()
    {
        const string source="ArtSource/MenuUI/native", target="Assets/_Game/Resources/UI/GlobalMenu";
        Directory.CreateDirectory(target);
        foreach(string file in Directory.GetFiles(source,"*.png"))
        {
            string path=target+"/"+Path.GetFileName(file);
            // Unity can retain a mapped view of a texture already imported in
            // this session. Do not rewrite identical exports; still verify
            // their import settings below so this command also repairs them.
            if(!File.Exists(path) || !File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(path)))
                File.Copy(file,path,true);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
            importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;
            importer.wrapMode=TextureWrapMode.Clamp;
            importer.spriteBorder=Path.GetFileNameWithoutExtension(file)=="Panel"?new Vector4(12,12,12,12):Vector4.zero;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();Debug.Log("GLOBAL MENU: native imports complete.");
    }
}
