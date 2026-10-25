from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
E=R/'Exports/Revision01';P=R/'Preview/Revision01';N=R/'Native/Revision01'
jobs=json.loads((N/'RecolorPlan.json').read_text());assets=[]
for p in sorted(E.glob('*.png')):
 im=Image.open(p).convert('RGBA');assets.append(dict(name=p.stem,width=im.width,height=im.height,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
placements=[]
for side,x in [('Left',0),('Right',329)]:
 names=[('Cap'+side,243,16),('Column'+side+'A',259 if side=='Left' else 323,96),('Column'+side+'B',355 if side=='Left' else 419,96),('Quiet'+side+'A',451 if side=='Left' else 259,32),('Quiet'+side+'B',483 if side=='Left' else 291,32),('Short'+side,515,18),('Base'+side,533,18)]
 for name,y,h in names:placements.append(dict(name=name,x=x,y=y,width=31,height=h))
data=dict(stage='Revision01',assets=assets,recolors=jobs,placements=placements,placementsTall=[dict(p,y=p['y']+63) for p in placements],pixelLabGenerations=0,status='REVIEW_CANDIDATE')
(N/'Manifest.json').write_text(json.dumps(data,indent=2)+'\n')
for hh in [640,800]:
 canvas=Image.new('RGBA',(360,hh));pp=data['placements' if hh==640 else 'placementsTall']
 for p in pp:canvas.alpha_composite(Image.open(E/(p['name']+'.png')).convert('RGBA'),(p['x'],p['y']))
 canvas.save(P/f'SideModules-Native-{hh}.png')
board=Image.new('RGB',(580,600),'#062b3b');d=ImageDraw.Draw(board)
for i,n in enumerate(['ColumnLeftA','ColumnLeftB','ColumnRightA','ColumnRightB']):
 im=Image.open(E/(n+'.png')).convert('RGBA').resize((124,384),Image.Resampling.NEAREST);board.paste(im,(i*143+8,35),im);d.text((i*143+8,10),n,fill='white')
board.save(P/'Native-Sides-4x.png')
template=(F/'Assets/_Game/Scripts/Editor/DrownedCourtV2Stage08Preview.cs').read_text()
helpers=template[template.index('    static void Populate'):template.index('    static void Capture')]+template[template.index('    static void Import'):template.index('    [Serializable] class Manifest')]
helpers=helpers.replace('/Stage08','/Revision01')
head='''using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Isolated visual revision: exact background recolors and connected board-side modules only.
public static class DrownedCourtV2Revision01Preview
{
    const string Root="ArtSource/DrownedCourtEnvironmentV2";
    const string Art="Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01";
    const string Scene="Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision01.unity";
    static int captures;
    static Manifest data;
    static Canvas canvas;
    static RectTransform sides;
    static readonly Dictionary<Image,Sprite> originalSprites=new Dictionary<Image,Sprite>();
    static readonly Dictionary<Image,Color> originalColors=new Dictionary<Image,Color>();
    [MenuItem("Dungeon Matcher/Art/Drowned Court V2/Build Focused Revision 01")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);captures=0;originalSprites.Clear();originalColors.Clear();
        data=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Revision01/Manifest.json"));Import(data);
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage08.unity");
        canvas=Object.FindFirstObjectByType<Canvas>();var camera=Object.FindFirstObjectByType<Camera>();
        var actors=canvas.transform.Find("Stage01_ReadOnlyActorContrast");
        var board=(RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement");
        var oldSize=board.rect.size;var oldPosition=board.anchoredPosition;
        var oldRects=canvas.GetComponentsInChildren<RectTransform>(true).ToDictionary(t=>t,t=>JsonUtility.ToJson(new BoundsRecord{anchor=t.anchoredPosition,size=t.sizeDelta,scale=t.localScale,rotation=t.localRotation,amin=t.anchorMin,amax=t.anchorMax,pivot=t.pivot}));
        sides=Rect(canvas.transform,"Revision01_IntegratedSides",new Rect(0,0,360,640));sides.localScale=new Vector3(2,2,1);
        sides.SetSiblingIndex(canvas.transform.Find("Stage05_BoardReview").GetSiblingIndex());Populate(sides,data.placements,640);
        Apply(true);
        foreach(var pair in oldRects)Assert.That(JsonUtility.ToJson(new BoundsRecord{anchor=pair.Key.anchoredPosition,size=pair.Key.sizeDelta,scale=pair.Key.localScale,rotation=pair.Key.localRotation,amin=pair.Key.anchorMin,amax=pair.Key.anchorMax,pivot=pair.Key.pivot}),Is.EqualTo(pair.Value),pair.Key.name+" layout changed");
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Scene),Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Scene),Is.True);
        actors.gameObject.SetActive(true);Capture(camera,1280);
        ((RectTransform)canvas.transform).sizeDelta=new Vector2(720,1600);
        foreach(Transform t in canvas.transform)
        {
            var r=t as RectTransform;if(!r)continue;
            if(t.name=="BlankOceanExtent")r.sizeDelta=new Vector2(720,1600);
            else if(t.name=="BlankLowerSurroundings")r.sizeDelta=new Vector2(712,240);
            else if(t.name=="Stage01_SeparateBackgroundLayers"||t.name=="Stage03_Platform_Candidates"||t.name=="Stage02_Architecture_Candidates"||t.name.StartsWith("PortraitInterior")||t.name=="ExistingBattleFloorGuide"||t.name=="Stage01_ReadOnlyActorContrast"||t.name.StartsWith("Stage04_"))r.anchoredPosition+=Vector2.up*320;
            else if(t.name.StartsWith("GridGuide")||t.name.StartsWith("PlayableBoardBounds")||t.name=="Stage05_BoardReview"||t.name=="Stage06_SideArchitecture")r.anchoredPosition+=Vector2.up*194;
            else if(t.name.StartsWith("UnchangedSharedUIShells"))foreach(RectTransform child in t)if(child.anchoredPosition.y>796)child.anchoredPosition+=Vector2.up*320;
        }
        foreach(var f in canvas.GetComponentsInChildren<ResponsiveModularFrameFitter>())f.RefreshFrame();
        // Reuse the prior tall-device mapping unchanged, including reef and old-side registration.
        Apply(false);
        var reef=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Stage07/Manifest.json"));
        Populate((RectTransform)canvas.transform.Find("Stage07_LowerReef"),reef.placementsTall,800,"Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage07");
        var detail=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Stage08/Manifest.json"));
        Populate((RectTransform)canvas.transform.Find("Stage08_MarineDetails"),detail.placementsTall,800,"Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage08");
        originalSprites.Clear();originalColors.Clear();Populate(sides,data.placementsTall,800);
        camera.orthographicSize=800;camera.transform.position=new Vector3(360,800,-10);Capture(camera,1600);
        File.WriteAllText(Root+"/QA/Revision01/UnityPreview.json",JsonUtility.ToJson(new Result{status="PASS",captures=captures,importedAssets=data.assets.Length,placedModules=data.placements.Length,importedOpaquePixelsAndAlphaExact=true,sceneExcludedFromBuild=true,boardPosition=oldPosition,boardSize=oldSize,note="Static art-only Unity renders at exact 1/2/3 scale. Existing object transforms preserved. Exact background recolor copies plus revised side modules only. Not gameplay/device testing; no production asset replacement."},true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.OpenScene(Scene);
        Debug.Log("DROWNED_COURT_REVISION01_PREVIEW_PASS");
    }
    static bool IsSide(Image im) {for(Transform t=im.transform;t&&t!=canvas.transform;t=t.parent)if(t.name=="Stage06_SideArchitecture"||t.name=="Stage08_MarineDetails"||t.name=="Revision01_IntegratedSides")return true;return false;}
    static bool IsBackground(Image im) {return originalSprites.ContainsKey(im)||originalColors.ContainsKey(im)||IsSide(im);}
    static void Apply(bool revised)
    {
        // Cache only explicitly authorized environment bindings. Actors, frames and controls never enter this map.
        foreach(var im in canvas.GetComponentsInChildren<Image>(true))
        {
            if(im.sprite&&!originalSprites.ContainsKey(im))
            {
                var path=AssetDatabase.GetAssetPath(im.sprite);
                if(data.recolors.Any(r=>r.oldAsset==path))originalSprites[im]=im.sprite;
            }
            if((im.name=="BlankOceanExtent"||im.name=="BlankLowerSurroundings")&&!originalColors.ContainsKey(im))originalColors[im]=im.color;
        }
        foreach(var p in originalSprites.Where(p=>p.Key).ToArray())
        {
            var job=data.recolors.First(r=>r.oldAsset==AssetDatabase.GetAssetPath(p.Value));
            p.Key.sprite=revised?Sprite(job.name):p.Value;
        }
        foreach(var p in originalColors.Where(p=>p.Key).ToArray())p.Key.color=revised?new Color32(4,37,51,255):p.Value;
        canvas.transform.Find("Stage06_SideArchitecture").gameObject.SetActive(!revised);
        canvas.transform.Find("Stage08_MarineDetails").gameObject.SetActive(!revised);
        sides.gameObject.SetActive(revised);
    }
    static void Protected(Camera camera,int w,int h,string name)
    {
        var hidden=canvas.GetComponentsInChildren<Image>().Where(im=>im.enabled&&IsBackground(im)).ToArray();
        foreach(var im in hidden)im.enabled=false;
        var prior=camera.backgroundColor;camera.backgroundColor=Color.clear;Render(camera,w,h,name);camera.backgroundColor=prior;
        foreach(var im in hidden)im.enabled=true;
    }
    static void Capture(Camera camera,int height)
    {
        foreach(int w in new[]{360,720,1080})
        {
            var dim=w+"x"+(height*w/720);Apply(false);
            Render(camera,w,height*w/720,"Unity-Before-"+dim+".png");Protected(camera,w,height*w/720,"Protected-Before-"+dim+".png");
            Apply(true);Render(camera,w,height*w/720,"Unity-Revised-"+dim+".png");Protected(camera,w,height*w/720,"Protected-Revised-"+dim+".png");
            var hide=canvas.transform.Cast<Transform>().Where(t=>t!=sides&&t.gameObject.activeSelf).ToArray();foreach(var t in hide)t.gameObject.SetActive(false);
            var prior=camera.backgroundColor;camera.backgroundColor=Color.clear;Render(camera,w,height*w/720,"Unity-SidesOnly-"+dim+".png");camera.backgroundColor=prior;foreach(var t in hide)t.gameObject.SetActive(true);
        }
    }
'''
tail='''    [Serializable] class Manifest {public Asset[] assets;public Placement[] placements,placementsTall;public Recolor[] recolors;}
    [Serializable] class Recolor {public string name,oldAsset;}
    [Serializable] class Asset {public string name;public int width,height;}
    [Serializable] class Placement {public string name;public int x,y,width,height;}
    [Serializable] class BoundsRecord {public Vector2 anchor,size,amin,amax,pivot;public Vector3 scale;public Quaternion rotation;}
    [Serializable] class Result {public string status,note;public int importedAssets,captures,placedModules;public bool importedOpaquePixelsAndAlphaExact,sceneExcludedFromBuild;public Vector2 boardPosition,boardSize;}
}
'''
(F/'Assets/_Game/Scripts/Editor/DrownedCourtV2Revision01Preview.cs').write_text(head+helpers+tail)
for n in ['SetupRevision01.py','BuildRevision01.py','RunLibreRevision01.ps1']:
 if (W/n).resolve()!=(R/'Scripts/Revision01'/n).resolve():shutil.copy2(W/n,R/'Scripts/Revision01'/n)
print('Built additive preview,',len(assets),'assets,',len(placements),'side modules.')
