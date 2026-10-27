from pathlib import Path
import shutil
def copy_script(source,target):
 if Path(source).resolve()!=Path(target).resolve():shutil.copyfile(source,target)
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD'
old=(F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDStage04.cs').read_text();helpers=old[old.index('    static void Bind()'):old.index('    [Serializable]class Report')]
code='''using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// A new review-only scene replaces only the old side-support placements.
// No production scene or shared layout/runtime implementation is changed.
public static class IronveinBelowTopHUDStage05
{
    const string Root="ArtSource/IronveinBelowTopHUD/Stage05";
    const string Art="Assets/_Game/Art/IronveinBelowTopHUD/Stage05";
    const string Source="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage04.unity";
    const string Target="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage05.unity";
    const string Group="Stage05_Surroundings_PreviewOnly";
    static Canvas canvas;static Camera camera;static RectTransform native;
    [MenuItem("Dungeon Matcher/Art/Ironvein Below Top HUD/Stage 05 Surroundings")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);Directory.CreateDirectory(Root+"/Preview");Directory.CreateDirectory(Root+"/QA");Import();
        EditorSceneManager.OpenScene(Source);Bind();
        int[] widths={360,720,1080};var before=widths.Select(w=>Capture(w,"Before")).ToArray();
        var masks=widths.Select(w=>ProtectedMask(w)).ToArray();
        var geometry=Geometry();var artwork=Artwork();
        Rect board=WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement"));
        Rect portrait=WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask"));
        var side=canvas.transform.Find("IronveinBoardSideModules");var lower=canvas.transform.Find("IronveinLowerModules");
        var replaced=side.Cast<Transform>().Concat(lower.Cast<Transform>()).Where(t=>t.name.StartsWith("Support")).ToArray();
        Assert.That(replaced.Length,Is.EqualTo(14));var protectedStates=canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!replaced.Contains(g.transform)).Select(g=>PathOf(g.transform)+"|"+g.gameObject.activeSelf).OrderBy(s=>s).ToArray();
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);Bind();
        // Source objects and assets remain recoverable in the new scene. Only their
        // old support placements are hidden, as this stage replaces that family.
        foreach(var t in replaced)t.gameObject.SetActive(false);
        native=NewRect(canvas.transform,Group,new Rect(0,0,360,640));native.localScale=new Vector3(2,2,1);
        native.SetSiblingIndex(side.GetSiblingIndex());
        // Rock is the background mass. Timber carries the already-approved drills.
        for(int i=0;i<7;i++){
            Stamp("RockLeft"+i,i%2==0?"RockA":"RockB",i%2==0?0:2,244+i*44);
            Stamp("RockRight"+i,i%2==0?"RockB":"RockA",i%2==0?338:336,244+i*44);
        }
        Tile("UnderBoardRock","RockCourse",0,541,360,16);
        foreach(float x in new[]{13f,327f}){
            Stamp("TimberCap"+x,"TimberCap",x,244);
            Tile("TimberShaft"+x,"TimberRepeat",x,262,20,280);
            Stamp("TimberFoot"+x,"TimberFoot",x,542);
            foreach(float y in new[]{354f,449f,515f})Stamp("Collar"+x+"_"+y,"IronCollar",x-1,y);
        }
        Stamp("LeftFooting","RockFoot",0,548);Stamp("RightFooting","RockFoot",332,548);
        Stamp("LeftAmberDeposit","AmberDeposit",0,516);Stamp("RightAmberDeposit","AmberDeposit",336,516);
        Canvas.ForceUpdateCanvases();
        CollectionAssert.AreEqual(geometry,Geometry(),"All original Graphic geometry/color must remain exact.");
        CollectionAssert.AreEqual(artwork,Artwork(),"All original asset references remain exact.");
        var states=canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!PathOf(g.transform).StartsWith(Group+"/")&&!replaced.Contains(g.transform)).Select(g=>PathOf(g.transform)+"|"+g.gameObject.activeSelf).OrderBy(s=>s).ToArray();
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
                if(masks[n][i].a==255){Assert.That(after[i],Is.EqualTo(before[n][i]),"Approved opaque frame/drill/separator pixel changed at "+w+" index "+i);protectedCounts[n]++;}
                if(after[i].Equals(before[n][i]))continue;
                Vector2 world=new Vector2((i%w+.5f)*720/w,(i/w+.5f)*720/w);Vector2 p=new Vector2(world.x/2,(1280-world.y)/2);
                bool sides=(p.x<34||p.x>=326)&&p.y>=242&&p.y<574;
                bool course=p.y>=541&&p.y<557;
                Assert.That(sides||course,Is.True,"Changed pixel outside Stage05 environment: "+w+" "+p);
                Assert.That(board.Contains(world),Is.False,"Playable grid changed: "+p);changes[n]++;
            }
            Assert.That(changes[n],Is.GreaterThan(0));Assert.That(protectedCounts[n],Is.GreaterThan(1000));
        }
        File.WriteAllText(Root+"/QA/Unity.json",JsonUtility.ToJson(new Report{status="PASS",scene=Target,assets=9,disabledOldSupportPlacements=14,geometryUnchanged=true,originalReferencesUnchanged=true,protectedPixelsUnchanged=true,sourcePixelScale=2,widths=widths,changedPixels=changes,protectedOpaquePixels=protectedCounts,boardBounds=board,portraitBounds=portrait,
            note="Actual art-only Unity renders. Only side-support placements and the low rock course beneath the board change. Approved frame, drill and separator opaque pixels match. Top, board, HUD and original assets remain exact. Not live gameplay/device testing; excluded from builds."},true));
        Debug.Log("IRONVEIN_LOWER_STAGE05_PASS assets=9 protected_pixels=exact old_supports_hidden=14");
    }
    static Color32[] ProtectedMask(int w){
        var gs=canvas.GetComponentsInChildren<Graphic>(true);var enabled=gs.Select(g=>g.enabled).ToArray();var flags=camera.clearFlags;var color=camera.backgroundColor;
        try{
            foreach(var g in gs){var p=PathOf(g.transform);g.enabled=g.enabled&&(p.StartsWith("IronveinNativeFrames/")||p.StartsWith("Stage04_Drills_PreviewOnly/")||p.StartsWith("Stage02_Separator_PreviewOnly/"));}
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
'''+helpers+'''
    [Serializable]class Report{public string status,scene,note;public int assets,disabledOldSupportPlacements,sourcePixelScale;public bool geometryUnchanged,originalReferencesUnchanged,protectedPixelsUnchanged;public int[] widths,changedPixels,protectedOpaquePixels;public Rect boardBounds,portraitBounds;}
}
'''
(F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDStage05.cs').write_text(code,encoding='utf-8')
(R/'Scripts/RunStage05.ps1').write_text((R/'Scripts/RunStage04.ps1').read_text().replace('Stage04','Stage05'))
copy_script(__file__,R/'Stage05/Scripts/PrepareUnityHelper.py');print('Stage05 Unity helper prepared')
