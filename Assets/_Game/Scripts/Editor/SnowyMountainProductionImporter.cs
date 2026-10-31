using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SnowyMountainProductionImporter
{
    const string Source="ArtSource/SnowyMountainStaged/",Target="Assets/_Game/Art/SnowyMountainApproved/",Resource="Assets/_Game/Resources/UI/BattlePresentation/";
    public static void Import()
    {
        Directory.CreateDirectory(Target);
        var inputs=new List<string>{Source+"Stage02/Exports/AlpinePass.png",Source+"Stage09/Exports/SkyExtension.png",Source+"Stage06/Exports/Lantern.png"};
        foreach(var dir in new[]{"Stage03/Exports","Stage05/Exports","Stage08/Exports","SideCornerPolish/Stage04/BannerMounts/Exports"})inputs.AddRange(Directory.GetFiles(Source+dir,"*.png"));
        inputs.AddRange(Directory.GetFiles(Source+"Stage04/Revision01/Exports","Ledge_*.png"));
        foreach(var n in new[]{"Left_Complete","Right_Complete","Corner_Left","Corner_Right"})inputs.Add(Source+"SideCornerPolish/Stage04/ConnectedSides/Inputs/"+n+".png");
        foreach(var input in inputs)File.Copy(input,Target+Path.GetFileName(input),true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(var file in inputs)
        {
            string path=Target+Path.GetFileName(file);var t=(TextureImporter)AssetImporter.GetAtPath(path);
            t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.spritePixelsPerUnit=file.Contains("Stage03/")||file.Contains("Stage05/")&&Path.GetFileName(file).StartsWith("Board_")?100:64;
            t.filterMode=FilterMode.Point;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=4096;t.isReadable=true;t.alphaIsTransparency=true;t.npotScale=TextureImporterNPOTScale.None;t.wrapMode=TextureWrapMode.Clamp;
            var settings=new TextureImporterSettings();t.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;t.SetTextureSettings(settings);t.SaveAndReimport();
        }
        T Asset<T>(string name)where T:ScriptableObject{string path=Resource+name+".asset";var a=AssetDatabase.LoadAssetAtPath<T>(path);if(!a){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);}return a;}
        var sprites=inputs.Select(p=>AssetDatabase.LoadAssetAtPath<Sprite>(Target+Path.GetFileName(p))).ToArray();
        var frames=Asset<DungeonPixelLabBattleArt>("SnowyMountainFrames");frames.modules=sprites.Where(s=>s.pixelsPerUnit==100).ToArray();EditorUtility.SetDirty(frames);
        var art=Asset<SnowyMountainBattleAssembly>("SnowyMountainAssembly");art.frames=frames;art.modules=sprites;
        art.boardCell=Resources.Load<DungeonPixelLabBattleArt>("UI/BattlePresentation/DungeonPixelLab").boardCell64;
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        Debug.Log("SNOW_PRODUCTION_IMPORTED "+sprites.Length+" unchanged native sprites; shared Dungeon/Forest cell reference");
    }
}
