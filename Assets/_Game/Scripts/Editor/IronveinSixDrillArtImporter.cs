using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public static class IronveinSixDrillArtImporter
{
    const string Root="ArtSource/IronveinBelowTopHUD/SixDrills";
    const string Art="Assets/_Game/Art/IronveinBelowTopHUD/SixDrills";
    public static void Import()
    {
        Directory.CreateDirectory(Art);
        foreach(var source in Directory.GetFiles(Root+"/Exports","*.png")){
            if(Path.GetFileNameWithoutExtension(source)=="RowDrillSpin")continue;
            string path=Art+"/"+Path.GetFileName(source);File.Copy(source,path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Single;
            ti.spritePixelsPerUnit=100;ti.filterMode=FilterMode.Point;ti.textureCompression=TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled=false;ti.npotScale=TextureImporterNPOTScale.None;ti.alphaIsTransparency=true;ti.isReadable=true;
            var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(settings);ti.SaveAndReimport();
            var expected=new Texture2D(2,2);expected.LoadImage(File.ReadAllBytes(source));var actual=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var a=actual.GetPixels32();var b=expected.GetPixels32();Assert.That(a.Length,Is.EqualTo(b.Length));
            for(int i=0;i<a.Length;i++)Assert.That(a[i].a==b[i].a&&(b[i].a==0||a[i].Equals(b[i])),Is.True,path+" pixel "+i);
            UnityEngine.Object.DestroyImmediate(expected);
        }
        Sprite S(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/"+name+".png");
        Sprite Old(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/IronveinBelowTopHUD/Stage04/"+name+".png");
        var theme=AssetDatabase.LoadAssetAtPath<GameplayThemeDefinition>("Assets/_Game/Resources/Zones/IronveinTheme.asset");
        theme.mineRowMount=Old("MountLeft");theme.mineRowMotor=Old("MotorLeft");theme.mineRowPower=Old("PowerBlock");
        theme.mineRowSocket=S("EmptySocket");theme.mineRowSpinFrames=Enumerable.Range(0,8).Select(i=>S("RowDrillSpin"+i.ToString("00"))).ToArray();
        Assert.That(theme.mineRowSpinFrames.All(s=>s&&s.rect.size==new Vector2(40,32)),Is.True);
        Assert.That(theme.mineRowMount&&theme.mineRowMotor&&theme.mineRowPower&&theme.mineRowSocket,Is.True);
        EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();
        File.WriteAllText(Root+"/ImportQA.txt","PASS: 8 native 40x32 spin frames, socket and existing Stage04 mount/motor/power bound only to IronveinTheme. Point filtering, no compression/mipmaps, exact opaque pixels verified.");
        Debug.Log("IRONVEIN_SIX_DRILL_IMPORT_PASS");
    }
}
