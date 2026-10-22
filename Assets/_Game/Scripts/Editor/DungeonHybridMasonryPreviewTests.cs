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
    const string Root="ArtSource/DungeonHybrid/SideMasonry/ScreenFill";
    const string Art="Assets/_Game/Art/DungeonHybrid/SideMasonry/MatchedCorners/";
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
        // Artwork reaches physical screen edges; live UI retains its safe viewport.
        float scale=DungeonPixelLabBattleArt.PhysicalPixelScale;
        var viewport=layout.Current.Viewport;
        float left=-viewport.xMin/scale,right=(UnityEngine.Device.Screen.width-viewport.xMin)/scale;
        float bottom=viewport.yMax/scale;
        var root=DungeonPixelLabBattleArt.Rect("Candidate side masonry",world,0,0,right,bottom);
        root.SetSiblingIndex(structure.GetSiblingIndex()+1);
        // The native opaque bottom is row 86, followed by two transparent rows.
        // Anchor visible stone, not padded canvas, to physical screen y=0.
        Piece("Masonry_Corner_Left",root,left,bottom-86);
        Piece("Masonry_Corner_Right",root,right-58,bottom-86);
        Assert.That(old.All(i=>i.gameObject.activeSelf),Is.True,"Preserve every original side-wall tile");
        Canvas.ForceUpdateCanvases();
        foreach(var image in root.GetComponentsInChildren<Image>())
        {
            var r=GameplayPixelLayoutController.ScreenRect(image.rectTransform);
            Assert.That(r.yMin+2*scale,Is.EqualTo(0).Within(.1f),"Opaque stone must meet the physical bottom edge");
            if(image.name.EndsWith("Left"))Assert.That(r.xMin,Is.EqualTo(0).Within(.1f));
            else Assert.That(r.xMax,Is.EqualTo(UnityEngine.Device.Screen.width).Within(.1f));
        }
        Assert.That(DungeonPixelLabBattleArt.Current.hybridModules.Any(s=>s.name.StartsWith("Masonry_")),Is.False,"Candidate must remain unbound in production");
    }
    [UnityTearDown]public IEnumerator Cleanup()=>new DungeonHybridIntegrationTests().Cleanup();
    [UnityTest,Explicit("Review matching bottom-corner additions while retaining the approved side stones.")]
    public IEnumerator CandidateWithThreeLiveEnemies()
    {
        Import();
        return new DungeonHybridIntegrationTests().CaptureScreens(Root,"Candidate",Decorate);
    }
}
