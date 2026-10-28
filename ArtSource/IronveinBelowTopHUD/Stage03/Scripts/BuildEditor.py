from pathlib import Path
import shutil
F=Path('C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage03'
previous=(F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDStage02.cs').read_text()
utilities=previous[previous.index('    static void Import()'):previous.index('    [Serializable]class Report')].replace('ti.spritePixelsPerUnit=64','ti.spritePixelsPerUnit=100')
code=r'''using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// New art-only stage. Existing runtime frame fitting, art and earlier scenes remain untouched.
public static class IronveinBelowTopHUDStage03
{
    const string Root="ArtSource/IronveinBelowTopHUD/Stage03";
    const string Art="Assets/_Game/Art/IronveinBelowTopHUD/Stage03";
    const string Source="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage02.unity";
    const string Target="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage03.unity";
    const string FramePath="IronveinNativeFrames/BoardFrameBounds/ModularFrame";
    static Canvas canvas;static Camera camera;
    [MenuItem("Dungeon Matcher/Art/Ironvein Below Top HUD/Stage 03 Board Frame")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);
        Directory.CreateDirectory(Root+"/Preview");Directory.CreateDirectory(Root+"/QA");Import();
        EditorSceneManager.OpenScene(Source);Bind();
        int[] widths={360,720,1080};var before=widths.Select(w=>Capture(w,"Before")).ToArray();
        var geometry=Geometry();var protectedArt=ProtectedArt();
        var oldFrame=(RectTransform)canvas.transform.Find(FramePath);Rect frameBounds=WorldRect((RectTransform)oldFrame.parent);
        var existingRects=oldFrame.GetComponentsInChildren<Image>().Select(im=>WorldRect(im.rectTransform)).ToArray();
        // The fitter rounds whole-piece extents beyond the fractional opening bounds.
        // Authorize only the already-rendered frame footprint, measured before replacement.
        Rect allowed=Rect.MinMaxRect(existingRects.Min(r=>r.xMin),existingRects.Min(r=>r.yMin),existingRects.Max(r=>r.xMax),existingRects.Max(r=>r.yMax));
        Rect opening=WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement"));
        Rect portrait=WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask"));
        // Rebuild only this stage from the immutable approved source on every run.
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);Bind();
        var frame=(RectTransform)canvas.transform.Find(FramePath);var fitter=frame.GetComponent<ResponsiveModularFrameFitter>();
        string[] names={"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner","TopEdge","BottomEdge","LeftEdge","RightEdge"};
        string[] sources={"BoardCornerTL","BoardCornerTR","BoardCornerBL","BoardCornerBR","BoardRailTop","BoardRailBottom","BoardRailLeft","BoardRailRight"};
        for(int i=0;i<names.Length;i++){
            var image=frame.Find(names[i]).GetComponent<Image>();var replacement=Sprite(sources[i]);
            Assert.That(replacement.rect.size,Is.EqualTo(image.sprite.rect.size),"Keep the established frame dimensions.");
            image.sprite=replacement;
        }
        // New UI exports use the Canvas's existing 100 reference PPU. This makes
        // each tiled texel one local unit, like the native corner layout. Do not
        // change the shared Canvas or any existing 64-PPU source/import settings.
        Assert.That(canvas.referencePixelsPerUnit,Is.EqualTo(100));
        // Existing fitter remains the sole owner of corner/rail/connector placement.
        fitter.ConfigureNativePieces(Sprite("BoardJoint"),90);
        fitter.FollowOpening((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement"));
        foreach(var child in frame.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Stage03_AmberInset").ToArray())Object.DestroyImmediate(child.gameObject);
        var connectors=(RectTransform)frame.Find("NativeConnectors");
        var active=connectors.Cast<RectTransform>().Where(r=>r.gameObject.activeSelf).ToArray();
        // Center accents sit within the exact existing 9x9 connector footprint.
        foreach(var connector in active){
            var point=WorldRect(connector).center;
            bool bottomCenter=Mathf.Abs(point.x-opening.center.x)<3&&point.y<opening.yMin;
            bool sideCenter=Mathf.Abs(point.y-opening.center.y)<3&&(point.x<opening.xMin||point.x>opening.xMax);
            if(!bottomCenter&&!sideCenter)continue;
            var child=new GameObject("Stage03_AmberInset",typeof(RectTransform),typeof(Image));
            var r=child.GetComponent<RectTransform>();r.SetParent(connector,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
            var image=child.GetComponent<Image>();image.sprite=Sprite("AmberInset");image.raycastTarget=false;
        }
        Canvas.ForceUpdateCanvases();
        CollectionAssert.AreEqual(geometry,Geometry(),"All pre-existing Graphic bounds must stay unchanged.");
        CollectionAssert.AreEqual(protectedArt,ProtectedArt(),"Top, separator, cells, gems and shared UI references must remain exact.");
        Assert.That(WorldRect((RectTransform)frame.parent),Is.EqualTo(frameBounds));
        Assert.That(WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement")),Is.EqualTo(opening));
        Assert.That(WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask")),Is.EqualTo(portrait));
        foreach(var image in frame.GetComponentsInChildren<Image>()){
            Assert.That(image.raycastTarget,Is.False);Assert.That(image.color,Is.EqualTo(Color.white));
            if(image.type==Image.Type.Simple){var b=WorldRect(image.rectTransform);Assert.That(b.width/image.sprite.rect.width,Is.EqualTo(2).Within(.0001f));Assert.That(b.height/image.sprite.rect.height,Is.EqualTo(2).Within(.0001f));}
            else if(image.type==Image.Type.Tiled)Assert.That(image.sprite.pixelsPerUnit/image.canvas.referencePixelsPerUnit*image.pixelsPerUnitMultiplier,Is.EqualTo(1).Within(.0001f),"Tiled texels must share the corners' native pixel scale.");
        }
        CheckCenters(frame);Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Target),Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);
        int[] changes=new int[3];
        for(int n=0;n<widths.Length;n++){
            var after=Capture(widths[n],"Unity");int w=widths[n],h=w*1280/720;
            for(int i=0;i<after.Length;i++)if(!after[i].Equals(before[n][i])){
                Vector2 point=new Vector2((i%w+.5f)*720/w,(i/w+.5f)*720/w);
                Assert.That(allowed.Contains(point),Is.True,"Pixel outside measured original frame footprint changed: "+w+" "+point);
                Assert.That(opening.Contains(point),Is.False,"Board pixel changed: "+w+" "+point);
                Assert.That(point.y,Is.LessThan(796),"Locked top pixel changed.");changes[n]++;
            }
            Assert.That(changes[n],Is.GreaterThan(0));
        }
        File.WriteAllText(Root+"/QA/Unity.json",JsonUtility.ToJson(new Report{status="PASS",scene=Target,boardBounds=opening,frameBounds=frameBounds,renderedFrameBounds=allowed,portraitBounds=portrait,
            geometryUnchanged=true,protectedArtworkUnchanged=true,protectedPixelsUnchanged=true,sourcePixelScale=2,assets=10,widths=widths,changedPixels=changes,
            note="Actual art-only Unity camera renders; every changed pixel lies outside the unchanged grid and inside the original board-frame footprint. Top, separator source/placement, UI and all other modules remain preserved. No gameplay or device testing."},true));
        Debug.Log("IRONVEIN_LOWER_STAGE03_PASS assets=10 native_scale=2 protected_pixels=exact");
    }
    static void CheckCenters(RectTransform frame){
        var l=WorldRect((RectTransform)frame.Find("LeftEdge"));var r=WorldRect((RectTransform)frame.Find("RightEdge"));
        var t=WorldRect((RectTransform)frame.Find("TopEdge"));var b=WorldRect((RectTransform)frame.Find("BottomEdge"));
        foreach(RectTransform c in frame.Find("NativeConnectors"))if(c.gameObject.activeSelf){
            Vector2 p=WorldRect(c).center;
            Assert.That(Mathf.Min(Mathf.Abs(p.x-l.center.x),Mathf.Abs(p.x-r.center.x),Mathf.Abs(p.y-t.center.y),Mathf.Abs(p.y-b.center.y)),Is.LessThan(.001f),"Connector and rail must share a centerline.");
        }
    }
    static string[] Geometry()=>canvas.GetComponentsInChildren<Graphic>(true).Where(g=>g.name!="Stage03_AmberInset").Select(g=>PathOf(g.transform)+"|"+JsonUtility.ToJson(WorldRect(g.rectTransform))+"|"+g.color).OrderBy(s=>s).ToArray();
    static string[] ProtectedArt()=>canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!PathOf(g.transform).StartsWith(FramePath+"/")).Select(g=>PathOf(g.transform)+"|"+(g is Image im?AssetDatabase.GetAssetPath(im.sprite):"text")).OrderBy(s=>s).ToArray();
    static string PathOf(Transform t){string p=t.name;while(t.parent&&t.parent!=canvas.transform){t=t.parent;p=t.name+"/"+p;}return p;}
    static void Bind(){canvas=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Single(c=>c.isRootCanvas);camera=Object.FindFirstObjectByType<Camera>();Assert.That(canvas,Is.Not.Null);Assert.That(camera,Is.Not.Null);Canvas.ForceUpdateCanvases();}
    static Rect WorldRect(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return Rect.MinMaxRect(c[0].x,c[0].y,c[2].x,c[2].y);}
    static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/"+name+".png");
'''+utilities+r'''
    [Serializable]class Report{public string status,scene,note;public Rect boardBounds,frameBounds,renderedFrameBounds,portraitBounds;public bool geometryUnchanged,protectedArtworkUnchanged,protectedPixelsUnchanged;public int sourcePixelScale,assets;public int[] widths,changedPixels;}
}
'''
(F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDStage03.cs').write_text(code)
run=(F/'ArtSource/IronveinBelowTopHUD/Scripts/RunStage02.ps1').read_text().replace('Stage02','Stage03')
(F/'ArtSource/IronveinBelowTopHUD/Scripts/RunStage03.ps1').write_text(run)
shutil.copyfile(__file__,R/'Scripts/BuildEditor.py')
print('Stage03 preview builder written')
