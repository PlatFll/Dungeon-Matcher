using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Separate native support art behind the unchanged HUD in a new review scene.
// No production scene or shared layout/runtime implementation is changed.
public static class IronveinBelowTopHUDStage06
{
    const string Root="ArtSource/IronveinBelowTopHUD/Stage06";
    const string Art="Assets/_Game/Art/IronveinBelowTopHUD/Stage06";
    const string Source="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage05.unity";
    const string Target="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage06.unity";
    const string Group="Stage06_HUDSeating_PreviewOnly";
    static Canvas canvas;static Camera camera;static RectTransform native;
    [MenuItem("Dungeon Matcher/Art/Ironvein Below Top HUD/Stage 06 HUD seating")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);Directory.CreateDirectory(Root+"/Preview");Directory.CreateDirectory(Root+"/QA");Import();
        EditorSceneManager.OpenScene(Source);Bind();
        int[] widths={360,720,1080};var before=widths.Select(w=>Capture(w,"Before")).ToArray();
        var masks=widths.Select(w=>ProtectedMask(w)).ToArray();
        var geometry=Geometry();var artwork=Artwork();
        Rect board=WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement"));
        Rect portrait=WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask"));
        var protectedStates=canvas.GetComponentsInChildren<Graphic>(true).Select(g=>PathOf(g.transform)+"|"+g.gameObject.activeSelf).OrderBy(v=>v).ToArray();
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);Bind();
        native=NewRect(canvas.transform,Group,new Rect(0,0,360,640));native.localScale=new Vector3(2,2,1);
        // Behind original lower props and the shared controls: artwork is never covered.
        native.SetSiblingIndex(canvas.transform.Find("IronveinLowerModules").GetSiblingIndex());
        Tile("UpperSeatingRail","SeatRail",64,570,232,7);
        Tile("LowerSeatingRail","SeatRail",64,611,232,7);
        Tile("LeftOuterPost","SeatPost",64,577,7,34);
        Tile("RightOuterPost","SeatPost",289,577,7,34);
        Stamp("LeftCorbel","CorbelRight",46,604);
        Stamp("RightCorbel","CorbelLeft",282,604);
        Stamp("LeftUpperBolt","SeatBolt",64,571);
        Stamp("RightUpperBolt","SeatBolt",291,571);
        Stamp("LeftLowerBolt","SeatBolt",64,611);
        Stamp("RightLowerBolt","SeatBolt",291,611);
        Canvas.ForceUpdateCanvases();
        CollectionAssert.AreEqual(geometry,Geometry(),"All original Graphic geometry/color must remain exact.");
        CollectionAssert.AreEqual(artwork,Artwork(),"All original asset references remain exact.");
        var states=canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!PathOf(g.transform).StartsWith(Group+"/")).Select(g=>PathOf(g.transform)+"|"+g.gameObject.activeSelf).OrderBy(s=>s).ToArray();
        CollectionAssert.AreEqual(protectedStates,states,"No other placement may be disabled.");
        Assert.That(WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement")),Is.EqualTo(board));
        Assert.That(WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask")),Is.EqualTo(portrait));
        foreach(var im in native.GetComponentsInChildren<Image>()){
            Assert.That(im.raycastTarget,Is.False);Assert.That(im.color,Is.EqualTo(Color.white));Assert.That(im.transform.lossyScale,Is.EqualTo(new Vector3(2,2,1)));
            if(im.type==Image.Type.Simple)Assert.That(im.rectTransform.rect.size,Is.EqualTo(im.sprite.rect.size));
            else Assert.That(im.sprite.pixelsPerUnit/im.canvas.referencePixelsPerUnit*im.pixelsPerUnitMultiplier,Is.EqualTo(1));
            var b=WorldRect(im.rectTransform);Assert.That(b.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(b.xMax,Is.LessThanOrEqualTo(720));
        }
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Target),Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);
        var changes=new int[3];var protectedCounts=new int[3];
        for(int n=0;n<widths.Length;n++){
            int w=widths[n];var after=Capture(w,"Unity");
            for(int i=0;i<after.Length;i++){
                if(masks[n][i].a==255){Assert.That(after[i],Is.EqualTo(before[n][i]),"Approved opaque UI or environment pixel changed at "+w+" index "+i);protectedCounts[n]++;}
                if(after[i].Equals(before[n][i]))continue;
                Vector2 world=new Vector2((i%w+.5f)*720/w,(i/w+.5f)*720/w);Vector2 p=new Vector2(world.x/2,(1280-world.y)/2);
                bool seating=p.x>=46&&p.x<314&&p.y>=570&&p.y<618;
                bool feet=(p.x>=46&&p.x<72||p.x>=288&&p.x<314)&&p.y>=618&&p.y<636;
                Assert.That(seating||feet,Is.True,"Changed pixel outside Stage06 seating: "+w+" "+p);
                Assert.That(board.Contains(world),Is.False,"Playable grid changed: "+p);changes[n]++;
            }
            Assert.That(changes[n],Is.GreaterThan(0));Assert.That(protectedCounts[n],Is.GreaterThan(1000));
        }
        File.WriteAllText(Root+"/QA/Unity.json",JsonUtility.ToJson(new Report{status="PASS",scene=Target,assets=5,disabledOldSupportPlacements=0,geometryUnchanged=true,originalReferencesUnchanged=true,protectedPixelsUnchanged=true,sourcePixelScale=2,widths=widths,changedPixels=changes,protectedOpaquePixels=protectedCounts,boardBounds=board,portraitBounds=portrait,
            note="Actual art-only Unity renders. Only environmental seating behind the bottom controls changes. Every existing Graphic geometry, reference and active state is unchanged. Approved opaque UI and props match pixel-for-pixel. Top and board remain exact; not live gameplay/device testing; excluded from builds."},true));
        Debug.Log("IRONVEIN_LOWER_STAGE06_PASS assets=5 protected_pixels=exact all_existing_objects_unchanged");
    }
    static Color32[] ProtectedMask(int w){
        var gs=canvas.GetComponentsInChildren<Graphic>(true);var enabled=gs.Select(g=>g.enabled).ToArray();var flags=camera.clearFlags;var color=camera.backgroundColor;
        try{
            foreach(var g in gs){var p=PathOf(g.transform);g.enabled=g.enabled&&(p.StartsWith("IronveinNativeFrames/")||p.StartsWith("Stage04_Drills_PreviewOnly/")||p.StartsWith("Stage02_Separator_PreviewOnly/")||p.StartsWith("Stage05_Surroundings_PreviewOnly/")||p.StartsWith("IronveinLowerModules/")||p.StartsWith("IronveinBoardSideModules/")||p.StartsWith("UnchangedSharedUIShells_StaticReviewOnly/")||p.StartsWith("LockedActorsAndBattleUI/"));}
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;return Capture(w,"ProtectedMask");
        }finally{for(int i=0;i<gs.Length;i++)gs[i].enabled=enabled[i];camera.clearFlags=flags;camera.backgroundColor=color;Canvas.ForceUpdateCanvases();}
    }
    static string[] Geometry()=>canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!PathOf(g.transform).StartsWith(Group+"/")).Select(g=>PathOf(g.transform)+"|"+JsonUtility.ToJson(WorldRect(g.rectTransform))+"|"+g.color).OrderBy(s=>s).ToArray();
    static string[] Artwork()=>canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!PathOf(g.transform).StartsWith(Group+"/")).Select(g=>PathOf(g.transform)+"|"+(g is Image im?AssetDatabase.GetAssetPath(im.sprite):"text")).OrderBy(s=>s).ToArray();
    static string PathOf(Transform t){string p=t.name;while(t.parent&&t.parent!=canvas.transform){t=t.parent;p=t.name+"/"+p;}return p;}
    static RectTransform NewRect(Transform parent,string name,Rect b){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=b.position;r.sizeDelta=b.size;return r;}
    static void Stamp(string name,string source,float x,float top){var s=Sprite(source);Piece(name,source,x,top,s.rect.width,s.rect.height);}
    static Image Piece(string name,string source,float x,float top,float w,float h){var im=NewRect(native,name,new Rect(x,640-top-h,w,h)).gameObject.AddComponent<Image>();im.sprite=Sprite(source);Assert.That(im.sprite,Is.Not.Null);im.raycastTarget=false;return im;}
    static void Tile(string name,string source,float x,float top,float w,float h){var im=Piece(name,source,x,top,w,h);im.type=Image.Type.Tiled;im.pixelsPerUnitMultiplier=1;}
    static void Bind(){canvas=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Single(c=>c.isRootCanvas);camera=Object.FindFirstObjectByType<Camera>();Assert.That(canvas,Is.Not.Null);Assert.That(camera,Is.Not.Null);Canvas.ForceUpdateCanvases();}
    static Rect WorldRect(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return Rect.MinMaxRect(c[0].x,c[0].y,c[2].x,c[2].y);}
    static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/"+name+".png");
    static void Import()
    {
        Directory.CreateDirectory(Art);
        foreach(var source in Directory.GetFiles(Root+"/Exports","*.png")){
            string path=Art+"/"+Path.GetFileName(source);File.Copy(source,path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Single;ti.spritePixelsPerUnit=100;
            ti.filterMode=FilterMode.Point;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.mipmapEnabled=false;ti.npotScale=TextureImporterNPOTScale.None;ti.maxTextureSize=2048;
            ti.alphaSource=TextureImporterAlphaSource.FromInput;ti.alphaIsTransparency=true;ti.isReadable=true;ti.wrapMode=TextureWrapMode.Clamp;
            var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(settings);ti.SaveAndReimport();
            var expected=new Texture2D(2,2);expected.LoadImage(File.ReadAllBytes(source));var actual=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Assert.That(actual.width,Is.EqualTo(expected.width));Assert.That(actual.height,Is.EqualTo(expected.height));
            var a=actual.GetPixels32();var b=expected.GetPixels32();for(int i=0;i<a.Length;i++)Assert.That(a[i].a==b[i].a&&(b[i].a==0||a[i].Equals(b[i])),Is.True,path+" pixel "+i);
            Object.DestroyImmediate(expected);
        }
    }
    static Color32[] Capture(int w,string prefix){int h=w*1280/720;Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);rt.filterMode=FilterMode.Point;rt.antiAliasing=1;
        var prior=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var png=new Texture2D(w,h,TextureFormat.RGBA32,false);png.ReadPixels(new Rect(0,0,w,h),0,0);png.Apply();
        File.WriteAllBytes(Root+"/Preview/"+prefix+"-"+w+"x"+h+".png",png.EncodeToPNG());var pixels=png.GetPixels32();RenderTexture.active=prior;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(png);return pixels;}



    [Serializable]class Report{public string status,scene,note;public int assets,disabledOldSupportPlacements,sourcePixelScale;public bool geometryUnchanged,originalReferencesUnchanged,protectedPixelsUnchanged;public int[] widths,changedPixels,protectedOpaquePixels;public Rect boardBounds,portraitBounds;}
}
