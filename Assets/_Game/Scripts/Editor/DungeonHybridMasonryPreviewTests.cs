using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// New artwork is previewed in the live scene without selecting it in production resources.
public sealed class DungeonHybridMasonryPreviewTests
{
    const string Root="ArtSource/DungeonHybrid/SideMasonry";
    const string Art="Assets/_Game/Art/DungeonHybrid/SideMasonry/";
    static void Import()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(string file in Directory.GetFiles(Art,"*.png"))
        {
            var i=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
            i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=64;
            i.filterMode=FilterMode.Point;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;
            i.npotScale=TextureImporterNPOTScale.None;i.wrapMode=TextureWrapMode.Clamp;i.alphaIsTransparency=false;i.maxTextureSize=2048;
            var s=new TextureImporterSettings();i.ReadTextureSettings(s);s.spriteMeshType=SpriteMeshType.FullRect;i.SetTextureSettings(s);i.SaveAndReimport();
        }
    }
    static Image Piece(string name,Transform parent,float x,float y)
    {
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".png");Assert.That(sprite,Is.Not.Null);
        var r=DungeonPixelLabBattleArt.Rect(name,parent,x,y,sprite.rect.width,sprite.rect.height);
        var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.raycastTarget=false;return i;
    }
    static void Decorate(GameplayPixelLayoutController layout)
    {
        var world=GameObject.Find("DungeonPixelLabEnvironment").GetComponent<RectTransform>();
        if(world.Find("Candidate side masonry"))return;
        var structure=world.Find("Structure");
        var old=structure.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Support_Corner_")).ToArray();
        Assert.That(old,Is.Not.Empty);
        float start=old.Min(i=>-i.rectTransform.anchoredPosition.y);
        // Walls overfill with ceil dimensions. Keep standalone bases fully inside.
        float scale=DungeonPixelLabBattleArt.PhysicalPixelScale;
        float width=Mathf.Floor(layout.Current.Viewport.width/scale),height=Mathf.Floor(layout.Current.Viewport.height/scale);
        foreach(var image in old)image.gameObject.SetActive(false);
        var root=DungeonPixelLabBattleArt.Rect("Candidate side masonry",world,0,0,width,height);
        root.SetSiblingIndex(structure.GetSiblingIndex()+1);
        // One native pixel of clearance also avoids camera rounding at safe-area edges.
        float baseTop=height-87;
        var repeat=DungeonPixelLabBattleArt.Rect("Masonry repeat window",root,0,start,width,baseTop+2-start);
        repeat.gameObject.AddComponent<RectMask2D>();
        for(float y=0;y<repeat.rect.height;y+=16)
        {
            Piece("Masonry_Repeat_Left",repeat,1,y);
            Piece("Masonry_Repeat_Right",repeat,width-25,y);
        }
        Piece("Masonry_Base_Left",root,1,baseTop);
        Piece("Masonry_Base_Right",root,width-49,baseTop);
        Canvas.ForceUpdateCanvases();
        foreach(var image in root.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Masonry_Base_")))
        {
            var r=GameplayPixelLayoutController.ScreenRect(image.rectTransform);
            var v=layout.Current.Viewport;
            Assert.That(r.xMin,Is.GreaterThanOrEqualTo(v.xMin-.01f));
            Assert.That(r.yMin,Is.GreaterThanOrEqualTo(v.yMin-.01f));
            Assert.That(r.xMax,Is.LessThanOrEqualTo(v.xMax+.01f));
            Assert.That(r.yMax,Is.LessThanOrEqualTo(v.yMax+.01f));
        }
        Assert.That(DungeonPixelLabBattleArt.Current.hybridModules.Any(s=>s.name.StartsWith("Masonry_")),Is.False,"Candidate must remain unbound in production");
    }
    [UnityTearDown]public IEnumerator Cleanup()=>new DungeonHybridIntegrationTests().Cleanup();
    [UnityTest,Explicit("Review new PixelLab masonry inside actual combat without selecting it for production.")]
    public IEnumerator CandidateWithThreeLiveEnemies()
    {
        Import();
        return new DungeonHybridIntegrationTests().CaptureScreens(Root,"Candidate",Decorate);
    }
}
