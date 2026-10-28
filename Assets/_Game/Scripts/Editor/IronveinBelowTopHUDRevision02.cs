using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// User-authorized local art revision. Source scenes/assets and production remain intact.
public static class IronveinBelowTopHUDRevision02
{
    const string Root="ArtSource/IronveinBelowTopHUD/Revision02";
    const string Art="Assets/_Game/Art/IronveinBelowTopHUD/Revision02";
    const string Source="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision01.unity";
    const string Target="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision02.unity";
    const string Prefix="Revision02_";
    static Canvas canvas;static Camera camera;static RectTransform native;

    [MenuItem("Dungeon Matcher/Art/Ironvein Below Top HUD/Revision 02 cave floor and block separator")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);Directory.CreateDirectory(Root+"/Preview");Directory.CreateDirectory(Root+"/QA");Import();
        EditorSceneManager.OpenScene(Source);Bind();
        int[] widths={360,720,1080};var before=widths.Select(w=>Capture(w,"Before")).ToArray();
        var masks=widths.Select(w=>ProtectedMask(w)).ToArray();
        var geometry=Geometry();var artwork=Artwork();
        var originalGraphics=canvas.GetComponentsInChildren<Graphic>(true);
        var states=originalGraphics.Select(g=>g.gameObject.activeSelf).ToArray();
        Rect board=WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement"));
        Rect portrait=WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask"));
        var oldFloor=canvas.transform.Find("Revision01_CombatFloor");Assert.That(oldFloor,Is.Not.Null);
        var removed=new[]{oldFloor};
        var changeRects=new[]{new Rect(0,(640-225)*2,720,(225-186)*2)};
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);
        oldFloor.gameObject.SetActive(false);
        native=NewRect(canvas.transform,Prefix+"CombatFloor",new Rect(0,0,360,640));native.localScale=new Vector3(2,2,1);
        native.SetSiblingIndex(oldFloor.GetSiblingIndex()+1);
        // More depth behind the existing feet, with the front edge and fascia bottom fixed.
        for(int i=0;i<12;i++){
            float x=i*32,w=Mathf.Min(32,360-x);if(w<=0)continue;
            Tile("WalkingSurface"+i,"FloorSurface"+((i%3==1)?"B":"A"),x,186,w,20);
        }
        for(int i=0;i<9;i++)Stamp("StoneBlock"+i,"SeparatorBlock"+(i%3==1?"B":"A"),i*40,206);
        Assert.That(oldFloor.gameObject.activeSelf,Is.False);
        Canvas.ForceUpdateCanvases();
        CollectionAssert.AreEqual(geometry,Geometry(),"Original Graphic geometry/color must remain exact.");
        CollectionAssert.AreEqual(artwork,Artwork(),"Original asset references must remain exact.");
        for(int i=0;i<originalGraphics.Length;i++){
            var g=originalGraphics[i];bool expected=removed.Any(t=>t.gameObject==g.gameObject)?false:states[i];
            Assert.That(g.gameObject.activeSelf,Is.EqualTo(expected),"Unexpected active-state change: "+PathOf(g.transform));
        }
        Assert.That(WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement")),Is.EqualTo(board));
        Assert.That(WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask")),Is.EqualTo(portrait));
        Assert.That(canvas.transform.Find("PortraitInterior_PlaceholderMask").GetComponent<RectMask2D>(),Is.Not.Null);
        var added=canvas.GetComponentsInChildren<Image>(true).Where(im=>PathOf(im.transform).StartsWith(Prefix)).ToArray();
        foreach(var im in added){
            Assert.That(im.raycastTarget,Is.False);Assert.That(im.color,Is.EqualTo(Color.white));Assert.That(im.transform.lossyScale,Is.EqualTo(new Vector3(2,2,1)));
            if(im.type==Image.Type.Simple)Assert.That(im.rectTransform.rect.size,Is.EqualTo(im.sprite.rect.size));
            else Assert.That(im.sprite.pixelsPerUnit/im.canvas.referencePixelsPerUnit*im.pixelsPerUnitMultiplier,Is.EqualTo(1));
            var b=WorldRect(im.rectTransform);Assert.That(b.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(b.xMax,Is.LessThanOrEqualTo(720));
            Assert.That(b.yMin,Is.GreaterThanOrEqualTo(0));Assert.That(b.yMax,Is.LessThanOrEqualTo(1280));
            Assert.That(b.Overlaps(board),Is.False,"New decoration must stay outside the board.");
        }
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Target),Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);
        var changes=new int[3];var protectedCounts=new int[3];
        for(int n=0;n<widths.Length;n++){
            int w=widths[n];var after=Capture(w,"Unity");
            for(int i=0;i<after.Length;i++){
                if(masks[n][i].a==255){Assert.That(after[i],Is.EqualTo(before[n][i]),"Protected UI/actor/grid pixel changed at "+w+" index "+i);protectedCounts[n]++;}
                if(after[i].Equals(before[n][i]))continue;
                Vector2 world=new Vector2((i%w+.5f)*720/w,(i/w+.5f)*720/w);
                Assert.That(changeRects.Any(r=>r.Contains(world)),Is.True,"Changed pixel outside authorized floor/props/crystal rectangles: "+w+" "+world);
                Assert.That(board.Contains(world),Is.False,"Playable grid changed.");changes[n]++;
            }
            Assert.That(changes[n],Is.GreaterThan(0));Assert.That(protectedCounts[n],Is.GreaterThan(1000));
        }
        File.WriteAllText(Root+"/QA/Unity.json",JsonUtility.ToJson(new Report{status="PASS",scene=Target,assets=4,disabledPlacements=removed.Select(t=>PathOf(t)).ToArray(),geometryUnchanged=true,originalReferencesUnchanged=true,protectedPixelsUnchanged=true,sourcePixelScale=2,widths=widths,changedPixels=changes,protectedOpaquePixels=protectedCounts,boardBounds=board,portraitBounds=portrait,
            note="Actual static art-only Unity renders. All original Graphic geometry and references remain exact, including actor floor line. Only the previous floor group is hidden. Surface depth increases from 8 to 20 native pixels; front edge and enemy feet remain fixed. Shared opaque UI/actors/grid exact. Point/native additions. Original source scene and production Game remain unchanged; not live combat or device testing."},true));
        Debug.Log("IRONVEIN_REVISION02_PASS assets=4 hidden=1 protected_pixels=exact actor_floor_unchanged");
    }
    static Color32[] ProtectedMask(int w){
        var gs=canvas.GetComponentsInChildren<Graphic>(true);var enabled=gs.Select(g=>g.enabled).ToArray();var flags=camera.clearFlags;var color=camera.backgroundColor;
        try{
            foreach(var g in gs){var p=PathOf(g.transform);g.enabled=g.enabled&&(p.StartsWith("IronveinNativeFrames/")||p.StartsWith("Stage04_Drills_PreviewOnly/")||p.StartsWith("Stage02_Separator_PreviewOnly/")||p.StartsWith("UnchangedSharedUIShells_StaticReviewOnly/")||p.StartsWith("LockedActorsAndBattleUI/")||p.StartsWith("LockedBoardCellsAndGems/"));}
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;return Capture(w,"ProtectedMask");
        }finally{for(int i=0;i<gs.Length;i++)gs[i].enabled=enabled[i];camera.clearFlags=flags;camera.backgroundColor=color;Canvas.ForceUpdateCanvases();}
    }
    static string[] Geometry()=>canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!PathOf(g.transform).StartsWith(Prefix)).Select(g=>PathOf(g.transform)+"|"+JsonUtility.ToJson(WorldRect(g.rectTransform))+"|"+g.color).OrderBy(s=>s).ToArray();
    static string[] Artwork()=>canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!PathOf(g.transform).StartsWith(Prefix)).Select(g=>PathOf(g.transform)+"|"+(g is Image im?AssetDatabase.GetAssetPath(im.sprite):"text")).OrderBy(s=>s).ToArray();
    static string PathOf(Transform t){string p=t.name;while(t.parent&&t.parent!=canvas.transform){t=t.parent;p=t.name+"/"+p;}return p;}
    static RectTransform NewRect(Transform parent,string name,Rect b){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=b.position;r.sizeDelta=b.size;return r;}
    static void Stamp(string name,string source,float x,float top){var s=Sprite(source);Piece(name,source,x,top,s.rect.width,s.rect.height);}
    static Image Piece(string name,string source,float x,float top,float w,float h){var im=NewRect(native,name,new Rect(x,640-top-h,w,h)).gameObject.AddComponent<Image>();im.sprite=Sprite(source);Assert.That(im.sprite,Is.Not.Null);im.raycastTarget=false;return im;}
    static void Tile(string name,string source,float x,float top,float w,float h){var im=Piece(name,source,x,top,w,h);im.type=Image.Type.Tiled;im.pixelsPerUnitMultiplier=1;}
    static void Bind(){canvas=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Single(c=>c.isRootCanvas);camera=Object.FindFirstObjectByType<Camera>();Assert.That(canvas,Is.Not.Null);Assert.That(camera,Is.Not.Null);Canvas.ForceUpdateCanvases();}
    static Rect WorldRect(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return Rect.MinMaxRect(c[0].x,c[0].y,c[2].x,c[2].y);}
    static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/"+name+".png");
    static void Import(){
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
    [Serializable]class Report{public string status,scene,note;public int assets,sourcePixelScale;public bool geometryUnchanged,originalReferencesUnchanged,protectedPixelsUnchanged;public string[] disabledPlacements;public int[] widths,changedPixels,protectedOpaquePixels;public Rect boardBounds,portraitBounds;}
}
