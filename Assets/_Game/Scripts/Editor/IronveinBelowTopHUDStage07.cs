using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Assembly checkpoint only. Reuse approved modules and the existing frame fitter.
// No art import/rewrite, runtime binding or production scene changes.
public static class IronveinBelowTopHUDStage07
{
    const string Root="ArtSource/IronveinBelowTopHUD/Stage07";
    const string Source="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage06.unity";
    const string Baseline="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage01.unity";
    const string Target="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage07.unity";
    const string Frame="IronveinNativeFrames/BoardFrameBounds/ModularFrame";
    const string ArtPrefix="Assets/_Game/Art/IronveinBelowTopHUD/";
    static Canvas canvas;static Camera camera;
    [MenuItem("Dungeon Matcher/Art/Ironvein Below Top HUD/Stage 07 Full Assembly")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);
        Directory.CreateDirectory(Root+"/Preview");Directory.CreateDirectory(Root+"/QA");
        int[] widths={360,720,1080};
        EditorSceneManager.OpenScene(Baseline);Bind();
        var initial=widths.Select(w=>Capture(w,"Initial")).ToArray();
        EditorSceneManager.OpenScene(Source);Bind();
        var approved=widths.Select(w=>Capture(w,"Approved")).ToArray();var geometry=GraphicsState();
        // All families were assembled incrementally in Stages 02–06. Promote that
        // exact approved assembly to its own review scene instead of rebuilding it.
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);Bind();
        CollectionAssert.AreEqual(geometry,GraphicsState(),"Approved Graphic states must remain exact.");
        Rect board=WorldRect(R("PlayableBoardBounds_NoTileReplacement"));
        Rect portrait=WorldRect(R("PortraitInterior_PlaceholderMask"));
        var grid=R("LockedBoardCellsAndGems").GetComponentsInChildren<Image>();
        Assert.That(grid.Count(im=>im.name.StartsWith("CellTile_")),Is.EqualTo(64));
        Assert.That(grid.Count(im=>im.name.StartsWith("Gem_")),Is.EqualTo(64));
        var frame=R(Frame);Assert.That(frame.GetComponent<ResponsiveModularFrameFitter>(),Is.Not.Null);
        var l=WorldRect((RectTransform)frame.Find("LeftEdge"));var r=WorldRect((RectTransform)frame.Find("RightEdge"));
        var t=WorldRect((RectTransform)frame.Find("TopEdge"));var b=WorldRect((RectTransform)frame.Find("BottomEdge"));
        // Keep the established native fitter's sub-texel rounding at the far edges.
        var joins=new[]{Mathf.Abs(l.xMax-board.xMin),Mathf.Abs(r.xMin-board.xMax),Mathf.Abs(t.yMin-board.yMax),Mathf.Abs(b.yMax-board.yMin)};
        foreach(float gap in joins)Assert.That(gap,Is.LessThanOrEqualTo(1.001f),"Frame must meet the grid within its existing half-native-pixel rounding.");
        foreach(RectTransform c in frame.Find("NativeConnectors"))if(c.gameObject.activeSelf){
            Vector2 p=WorldRect(c).center;
            Assert.That(Mathf.Min(Mathf.Abs(p.x-l.center.x),Mathf.Abs(p.x-r.center.x),Mathf.Abs(p.y-t.center.y),Mathf.Abs(p.y-b.center.y)),Is.LessThan(.001f),"Frame connector centerline.");
        }
        var drills=R("Stage04_Drills_PreviewOnly");Assert.That(drills.GetComponentsInChildren<Image>().Length,Is.EqualTo(24));
        var stations=new Rect[6];
        for(int n=0;n<3;n++){
            var lm=WorldRect((RectTransform)drills.Find("Left"+n+"_Mount"));var rm=WorldRect((RectTransform)drills.Find("Right"+n+"_Mount"));
            Assert.That(lm.yMin,Is.EqualTo(rm.yMin));Assert.That(lm.height,Is.EqualTo(rm.height));
            Assert.That(lm.xMax,Is.LessThanOrEqualTo(board.xMin+.001f));Assert.That(rm.xMin,Is.GreaterThanOrEqualTo(board.xMax-.001f));
            Assert.That(lm.xMax-9,Is.EqualTo(l.center.x).Within(.001f));Assert.That(rm.xMin+9,Is.EqualTo(r.center.x).Within(.001f));
            foreach(string side in new[]{"Left","Right"}){
                // The source has one transparent column at the tip. Check the
                // actual opaque sprite, not its intentional transparent padding.
                var nose=OpaqueWorldRect(drills.Find(side+n+"_Nose").GetComponent<Image>());
                Assert.That(side=="Left"?nose.xMax<=board.xMin+.001f:nose.xMin>=board.xMax-.001f,Is.True,"Drill tip must remain outside gems.");
            }
            stations[n*2]=lm;stations[n*2+1]=rm;
        }
        Assert.That(stations[0].yMin-stations[2].yMin,Is.EqualTo(stations[2].yMin-stations[4].yMin).Within(.001f),"Even drill station spacing.");
        Assert.That(R("Stage05_Surroundings_PreviewOnly").GetComponentsInChildren<Image>().Length,Is.EqualTo(31));
        var seating=R("Stage06_HUDSeating_PreviewOnly");Assert.That(seating.GetComponentsInChildren<Image>().Length,Is.EqualTo(10));
        var shared=R("UnchangedSharedUIShells_StaticReviewOnly");Assert.That(seating.GetSiblingIndex(),Is.LessThan(shared.GetSiblingIndex()));
        var nativeImages=canvas.GetComponentsInChildren<Image>().Where(im=>im.sprite&&AssetDatabase.GetAssetPath(im.sprite).StartsWith(ArtPrefix)).ToArray();
        var paths=nativeImages.Select(im=>AssetDatabase.GetAssetPath(im.sprite)).Distinct().OrderBy(p=>p).ToArray();Assert.That(paths.Length,Is.EqualTo(36));
        foreach(var path in paths){
            var ti=(TextureImporter)AssetImporter.GetAtPath(path);Assert.That(ti.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(ti.mipmapEnabled,Is.False);
            Assert.That(ti.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);Assert.That(tex.GetPixels32().All(p=>p.a==0||p.a==255),Is.True,path+" binary alpha");
        }
        foreach(var im in nativeImages){
            Assert.That(im.raycastTarget,Is.False);Assert.That(im.color,Is.EqualTo(Color.white));
            var bounds=WorldRect(im.rectTransform);
            Assert.That(bounds.xMin,Is.GreaterThanOrEqualTo(-.001f));Assert.That(bounds.xMax,Is.LessThanOrEqualTo(720.001f));
            Assert.That(bounds.yMin,Is.GreaterThanOrEqualTo(-.001f));Assert.That(bounds.yMax,Is.LessThanOrEqualTo(796.001f));
            if(im.type==Image.Type.Simple){Assert.That(bounds.width/im.sprite.rect.width,Is.EqualTo(2).Within(.001f));Assert.That(bounds.height/im.sprite.rect.height,Is.EqualTo(2).Within(.001f));}
            else if(im.type==Image.Type.Tiled){Assert.That(im.sprite.pixelsPerUnit/im.canvas.referencePixelsPerUnit*im.pixelsPerUnitMultiplier,Is.EqualTo(1).Within(.001f));Assert.That(im.transform.lossyScale.x,Is.EqualTo(2).Within(.001f));Assert.That(im.transform.lossyScale.y,Is.EqualTo(2).Within(.001f));}
            else Assert.Fail("Unexpected native module scaling mode: "+im.name);
        }
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Target),Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);
        for(int n=0;n<widths.Length;n++){
            int w=widths[n];var after=Capture(w,"Unity");CollectionAssert.AreEqual(approved[n],after,"Every pixel of the approved Stage06 assembly must remain exact.");
            for(int i=0;i<after.Length;i++){
                Vector2 point=new Vector2((i%w+.5f)*720/w,(i/w+.5f)*720/w);
                if(point.y>=796||board.Contains(point))Assert.That(after[i],Is.EqualTo(initial[n][i]),"Initial approved top and grid pixel changed.");
            }
        }
        File.WriteAllText(Root+"/QA/Assembly.json",JsonUtility.ToJson(new Report{status="PASS",scene=Target,approvedArtUnchanged=true,approvedPixelsUnchanged=true,initialTopAndGridUnchanged=true,excludedFromBuilds=true,newPixelLabGenerations=0,uniqueNativeAssets=paths.Length,nativeImagePlacements=nativeImages.Length,cells=64,gems=64,drillModules=6,widths=widths,boardBounds=board,portraitBounds=portrait,frameJoinOffsetsWorldUnits=joins,drillMounts=stations,assets=paths,
            note="Actual Unity art-only assembly checkpoint. Approved Stage06 rendering is exact at all three sizes. All 36 source assets remain separate; native module scale is uniform and integer. Existing shared UI/grid geometry is retained. No gameplay, safe-area/device or production integration test."},true));
        Debug.Log("IRONVEIN_LOWER_STAGE07_PASS assets=36 drills=6 cells=64 gems=64 approved_render=exact");
    }
    static RectTransform R(string path){var r=canvas.transform.Find(path) as RectTransform;Assert.That(r,Is.Not.Null,path);return r;}
    static void Bind(){canvas=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Single(c=>c.isRootCanvas);camera=Object.FindFirstObjectByType<Camera>();Canvas.ForceUpdateCanvases();}
    static Rect WorldRect(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return Rect.MinMaxRect(c[0].x,c[0].y,c[2].x,c[2].y);}
    static Rect OpaqueWorldRect(Image im){
        Assert.That(im.type,Is.EqualTo(Image.Type.Simple));Assert.That(im.preserveAspect,Is.False);
        var s=im.sprite;var sr=s.rect;var pixels=s.texture.GetPixels32();int x0=(int)sr.width,y0=(int)sr.height,x1=-1,y1=-1;
        for(int y=0;y<(int)sr.height;y++)for(int x=0;x<(int)sr.width;x++)if(pixels[((int)sr.y+y)*s.texture.width+(int)sr.x+x].a>0){x0=Mathf.Min(x0,x);y0=Mathf.Min(y0,y);x1=Mathf.Max(x1,x);y1=Mathf.Max(y1,y);}
        Assert.That(x1,Is.GreaterThanOrEqualTo(x0));var b=WorldRect(im.rectTransform);
        return Rect.MinMaxRect(b.xMin+x0*b.width/sr.width,b.yMin+y0*b.height/sr.height,b.xMin+(x1+1)*b.width/sr.width,b.yMin+(y1+1)*b.height/sr.height);
    }
    static string PathOf(Transform t){string p=t.name;while(t.parent&&t.parent!=canvas.transform){t=t.parent;p=t.name+"/"+p;}return p;}
    static string[] GraphicsState()=>canvas.GetComponentsInChildren<Graphic>(true).Select(g=>PathOf(g.transform)+"|"+JsonUtility.ToJson(WorldRect(g.rectTransform))+"|"+g.color+"|"+g.gameObject.activeSelf+"|"+g.enabled+"|"+(g is Image im?AssetDatabase.GetAssetPath(im.sprite):"text")).OrderBy(s=>s).ToArray();
    static Color32[] Capture(int w,string prefix){int h=w*1280/720;Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);rt.filterMode=FilterMode.Point;rt.antiAliasing=1;
        var prior=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var png=new Texture2D(w,h,TextureFormat.RGBA32,false);png.ReadPixels(new Rect(0,0,w,h),0,0);png.Apply();
        File.WriteAllBytes(Root+"/Preview/"+prefix+"-"+w+"x"+h+".png",png.EncodeToPNG());var pixels=png.GetPixels32();RenderTexture.active=prior;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(png);return pixels;}
    [Serializable]class Report{public string status,scene,note;public bool approvedArtUnchanged,approvedPixelsUnchanged,initialTopAndGridUnchanged,excludedFromBuilds;public int newPixelLabGenerations,uniqueNativeAssets,nativeImagePlacements,cells,gems,drillModules;public int[] widths;public Rect boardBounds,portraitBounds;public float[] frameJoinOffsetsWorldUnits;public Rect[] drillMounts;public string[] assets;}
}
