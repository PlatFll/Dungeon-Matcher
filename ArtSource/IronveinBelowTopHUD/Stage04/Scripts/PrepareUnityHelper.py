from pathlib import Path
import shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher')
R=F/'ArtSource/IronveinBelowTopHUD'
old=(F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDStage03.cs').read_text()
helpers=old[old.index('    static void Bind()'):old.index('    [Serializable]class Report')]
code='''using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Stage 04 art-only assembly. Derive mounts from the existing fitted rails;
// do not change the shared frame fitter, source scene or production presentation.
public static class IronveinBelowTopHUDStage04
{
    const string Root="ArtSource/IronveinBelowTopHUD/Stage04";
    const string Art="Assets/_Game/Art/IronveinBelowTopHUD/Stage04";
    const string Source="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage03.unity";
    const string Target="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage04.unity";
    const string FramePath="IronveinNativeFrames/BoardFrameBounds/ModularFrame";
    const string Group="Stage04_Drills_PreviewOnly";
    static Canvas canvas;static Camera camera;static RectTransform native;
    [MenuItem("Dungeon Matcher/Art/Ironvein Below Top HUD/Stage 04 Drill Modules")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);
        Directory.CreateDirectory(Root+"/Preview");Directory.CreateDirectory(Root+"/QA");Import();
        EditorSceneManager.OpenScene(Source);Bind();
        int[] widths={360,720,1080};var before=widths.Select(w=>Capture(w,"Before")).ToArray();
        var geometry=Geometry();var art=Artwork();
        var frame=(RectTransform)canvas.transform.Find(FramePath);
        Rect frameBounds=WorldRect((RectTransform)frame.parent);
        Rect opening=WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement"));
        Rect portrait=WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask"));
        float leftRail=WorldRect((RectTransform)frame.Find("LeftEdge")).center.x/2;
        float rightRail=WorldRect((RectTransform)frame.Find("RightEdge")).center.x/2;
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);Bind();
        native=NewRect(canvas.transform,Group,new Rect(0,0,360,640));native.localScale=new Vector3(2,2,1);
        // Add below the protected board's cells, preserving every old sibling's order.
        var board=canvas.transform.Find("LockedBoardCellsAndGems");Assert.That(board,Is.Not.Null);
        native.SetSiblingIndex(board.GetSiblingIndex());
        var allowed=new List<Rect>();float[] stations={.15f,.5f,.85f};
        for(int n=0;n<stations.Length;n++){
            float top=Mathf.Round((1280-opening.yMax)/2+opening.height/2*stations[n]-18);
            float lx=leftRail-33.5f,rx=rightRail-6.5f;
            // Socket strips meet the rail centerline exactly. Keep the full drill
            // silhouette outside the gem opening, even at its existing fractional bounds.
            Assert.That(lx+38,Is.LessThanOrEqualTo(opening.xMin/2+.001f));
            Assert.That(rx+2,Is.GreaterThanOrEqualTo(opening.xMax/2-.001f));
            Assert.That(lx,Is.GreaterThanOrEqualTo(0));Assert.That(rx+40,Is.LessThanOrEqualTo(360));
            allowed.Add(new Rect(lx,top,40,36));allowed.Add(new Rect(rx,top,40,36));
            Stamp("Left"+n+"_Mount","MountLeft",lx,top);
            Stamp("Left"+n+"_Motor","MotorLeft",lx+7,top+4);
            Stamp("Left"+n+"_Nose","NoseRight",lx+23,top+9);
            Stamp("Left"+n+"_Power","PowerBlock",lx+13,top+13);
            Stamp("Right"+n+"_Mount","MountRight",rx+2,top);
            Stamp("Right"+n+"_Motor","MotorRight",rx+15,top+4);
            Stamp("Right"+n+"_Nose","NoseLeft",rx+1,top+9);
            Stamp("Right"+n+"_Power","PowerBlock",rx+18,top+13);
        }
        Canvas.ForceUpdateCanvases();
        CollectionAssert.AreEqual(geometry,Geometry(),"Every existing Graphic must retain its exact geometry and color.");
        CollectionAssert.AreEqual(art,Artwork(),"Existing sprites/text and active state remain unchanged.");
        Assert.That(WorldRect((RectTransform)canvas.transform.Find(FramePath).parent),Is.EqualTo(frameBounds));
        Assert.That(WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement")),Is.EqualTo(opening));
        Assert.That(WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask")),Is.EqualTo(portrait));
        var images=native.GetComponentsInChildren<Image>();Assert.That(images.Length,Is.EqualTo(24));
        foreach(var im in images){
            Assert.That(im.raycastTarget,Is.False);Assert.That(im.color,Is.EqualTo(Color.white));
            Assert.That(im.type,Is.EqualTo(Image.Type.Simple));Assert.That(im.rectTransform.rect.size,Is.EqualTo(im.sprite.rect.size));
            Assert.That(im.transform.lossyScale,Is.EqualTo(new Vector3(2,2,1)));
        }
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Target),Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);
        int[] changes=new int[widths.Length];
        for(int n=0;n<widths.Length;n++){
            var after=Capture(widths[n],"Unity");int w=widths[n],h=w*1280/720;
            for(int i=0;i<after.Length;i++)if(!after[i].Equals(before[n][i])){
                Vector2 world=new Vector2((i%w+.5f)*720/w,(i/w+.5f)*720/w);
                Vector2 point=new Vector2(world.x/2,(1280-world.y)/2);
                Assert.That(allowed.Any(r=>r.Contains(point)),Is.True,"Changed pixel outside drill footprints: "+w+" "+point);
                Assert.That(opening.Contains(world),Is.False,"Changed board pixel: "+w+" "+world);
                Assert.That(world.y,Is.LessThan(796),"Locked top pixel changed.");changes[n]++;
            }
            Assert.That(changes[n],Is.GreaterThan(0));
        }
        File.WriteAllText(Root+"/QA/Unity.json",JsonUtility.ToJson(new Report{status="PASS",scene=Target,boardBounds=opening,frameBounds=frameBounds,portraitBounds=portrait,
            geometryUnchanged=true,existingArtworkUnchanged=true,protectedPixelsUnchanged=true,sceneExcludedFromBuild=true,
            assets=7,modules=6,sourcePixelScale=2,widths=widths,changedPixels=changes,nativeDrillBounds=allowed.ToArray(),
            note="Actual art-only Unity camera renders at three resolutions. Every changed pixel lies within a drill station and outside the grid. Top, separator, lower HUD and all original asset pixels and object geometry remain preserved. No live gameplay/device testing."},true));
        Debug.Log("IRONVEIN_LOWER_STAGE04_PASS assets=7 modules=6 native_scale=2 protected_pixels=exact");
    }
    static string[] Geometry()=>canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!PathOf(g.transform).StartsWith(Group+"/")).Select(g=>PathOf(g.transform)+"|"+JsonUtility.ToJson(WorldRect(g.rectTransform))+"|"+g.color).OrderBy(s=>s).ToArray();
    static string[] Artwork()=>canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!PathOf(g.transform).StartsWith(Group+"/")).Select(g=>PathOf(g.transform)+"|"+g.gameObject.activeSelf+"|"+(g is Image im?AssetDatabase.GetAssetPath(im.sprite):"text")).OrderBy(s=>s).ToArray();
    static string PathOf(Transform t){string p=t.name;while(t.parent&&t.parent!=canvas.transform){t=t.parent;p=t.name+"/"+p;}return p;}
    static RectTransform NewRect(Transform parent,string name,Rect b){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=b.position;r.sizeDelta=b.size;return r;}
    static void Stamp(string objectName,string source,float x,float top){var s=Sprite(source);Assert.That(s,Is.Not.Null,source);var r=NewRect(native,objectName,new Rect(x,640-top-s.rect.height,s.rect.width,s.rect.height));var im=r.gameObject.AddComponent<Image>();im.sprite=s;im.raycastTarget=false;}
'''+helpers+'''
    [Serializable]class Report{public string status,scene,note;public Rect boardBounds,frameBounds,portraitBounds;public Rect[] nativeDrillBounds;public bool geometryUnchanged,existingArtworkUnchanged,protectedPixelsUnchanged,sceneExcludedFromBuild;public int assets,modules,sourcePixelScale;public int[] widths,changedPixels;}
}
'''
(F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDStage04.cs').write_text(code,encoding='utf-8')
runner=(R/'Scripts/RunStage03.ps1').read_text().replace('Stage03','Stage04')
(R/'Scripts/RunStage04.ps1').write_text(runner,encoding='utf-8')
shutil.copyfile(__file__,R/'Stage04/Scripts/PrepareUnityHelper.py')
print('Stage04 Unity helper ready')
