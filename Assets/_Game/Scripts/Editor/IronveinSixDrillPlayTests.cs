using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public sealed partial class ForestFoundationPlayTests
{
    const string DrillReview="ArtSource/IronveinBelowTopHUD/SixDrills";
    [UnityTest] public IEnumerator IronveinSixRowsMigrateLegacyFuelAndResume()
    {
        yield return LaunchMine();var b=Run.Board;
        Assert.That(b.Mine.drills.Count,Is.EqualTo(6));
        Assert.That(b.Mine.drills.Select(d=>d.lane).Distinct().Count(),Is.EqualTo(6));
        Assert.That(b.Mine.drills.All(d=>d.horizontal),Is.True);
        Assert.That(b.Mine.drills.Count(d=>d.fromRight),Is.EqualTo(3));
        var legacy=new MineEnvironmentState{version=2,drills=new List<MineDrillState>{
            new MineDrillState{id=1,horizontal=true,lane=2,charge=3,lastPlayerAction=0},
            new MineDrillState{id=2,horizontal=false,lane=5,charge=2,lastPlayerAction=0}}};
        var saved=Run.Continuation.Capture().board;saved.mine=legacy;
        Assert.That(MineEnvironmentState.Supports(saved),Is.True);
        Call(b,"RestoreMine",legacy);
        Assert.That(b.Mine.version,Is.EqualTo(3));Assert.That(b.Mine.drills.Count,Is.EqualTo(6));
        CollectionAssert.AreEqual(new[]{3,2,0,0,0,0},b.Mine.drills.Select(d=>d.charge));
        Assert.That(b.Mine.drills[0].lane,Is.EqualTo(2));Assert.That(b.Mine.drills[1].lane,Is.EqualTo(5));
        Assert.That(b.Mine.drills[1].horizontal&&b.Mine.drills[1].fromRight,Is.True);
        Assert.That(legacy.drills[1].horizontal,Is.False,"migration must not mutate the saved source");
        string expected=JsonUtility.ToJson(b.Mine);yield return ResumeRoster();
        Assert.That(JsonUtility.ToJson(Run.Board.Mine),Is.EqualTo(expected));
        var invalid=Run.Continuation.Capture().board;invalid.mine.drills[5].id=7;
        Assert.That(MineEnvironmentState.Supports(invalid),Is.False);
    }
    [UnityTest] public IEnumerator IronveinRightIntakeFeedsOnlyItsOwnEdgeOnce()
    {
        yield return LaunchMine();var b=Run.Board;var d=b.Mine.drills[1];
        Call(b,"RegisterMinePlayerMatch",new HashSet<Gem>{b.GetGem(0,d.lane),b.GetGem(1,d.lane)},true);yield return Stable();
        Assert.That(d.charge,Is.Zero);
        var match=new HashSet<Gem>{b.GetGem(b.Width-1,d.lane),b.GetGem(b.Width-2,d.lane)};
        Call(b,"RegisterMinePlayerMatch",match,false);yield return Stable();Assert.That(d.charge,Is.Zero);
        Call(b,"RegisterMinePlayerMatch",match,true);Call(b,"RegisterMinePlayerMatch",match,true);yield return Stable();
        Assert.That(d.charge,Is.EqualTo(1));Assert.That(b.Mine.drills.Where(x=>x.id!=d.id).All(x=>x.charge==0),Is.True);
    }
    [UnityTest] public IEnumerator IronveinRowProjectileClearsInOrderPausesAndReloadsAfterBurst()
    {
        yield return LaunchMine();var b=Run.Board;var d=b.Mine.drills[1];int row=d.lane;
        var visited=new List<int>();bool exploded=false,reloaded=false;float explosionTime=0;
        b.MineDrillTravelling+=(id,cell,seconds)=>{
            if(id!=2)return;Assert.That(b.IsBusy,Is.True);Assert.That(b.IsMineDrillInFlight(id),Is.True);
            if(cell.x<0)return;
            if(visited.Count>0)Assert.That(b.GetGem(visited.Last(),row),Is.Null,"previous tile cleared before next travel");
            visited.Add(cell.x);
        };
        b.MineDrillExploded+=(id,cell)=>{if(id!=2)return;exploded=true;explosionTime=Time.time;
            Assert.That(b.IsMineDrillInFlight(id),Is.True);Assert.That(cell.x,Is.EqualTo(-1));};
        b.MineDrillReloading+=id=>{if(id!=2)return;reloaded=true;Assert.That(exploded,Is.True);
            Assert.That(Time.time-explosionTime,Is.GreaterThanOrEqualTo(BoardController.MineDrillBurstSeconds-.015f));
            Assert.That(b.IsMineDrillInFlight(id),Is.False);};
        b.TryQueueMineDrillPower(2,4);yield return Until(()=>visited.Count>=2,"second impact");
        var bit=b.transform.Find("MineDrill_2/LoadedBit").GetComponent<SpriteRenderer>();Assert.That(bit.enabled,Is.False);
        var shot=b.transform.Find("RowDrillProjectile_2");Assert.That(shot,Is.Not.Null);
        Time.timeScale=0;yield return null;Vector3 point=shot.localPosition;var sprite=shot.GetComponent<SpriteRenderer>().sprite;int count=visited.Count;
        yield return new WaitForSecondsRealtime(.2f);
        Assert.That(visited.Count,Is.EqualTo(count));Assert.That(shot.localPosition,Is.EqualTo(point));Assert.That(shot.GetComponent<SpriteRenderer>().sprite,Is.SameAs(sprite));
        Time.timeScale=1;yield return Stable();yield return null;
        CollectionAssert.AreEqual(Enumerable.Range(0,b.Width).Reverse(),visited);Assert.That(exploded&&reloaded,Is.True);Assert.That(bit.enabled,Is.True);
        Assert.That(b.transform.Find("RowDrillProjectile_2"),Is.Null);
    }
    [UnityTest] public IEnumerator IronveinSixDrillGameplayDoesNotDependOnTheView()
    {
        yield return LaunchMine();var b=Run.Board;b.GetComponent<MineEnvironmentView>().enabled=false;
        var targets=Enumerable.Range(0,b.Width).Select(x=>b.GetGem(x,b.Mine.drills[4].lane).BoardIdentity).ToArray();
        int fired=0;b.MineDrillFired+=(_,__,___)=>fired++;
        Assert.That(b.TryQueueMineDrillPower(5,4),Is.True);yield return Stable();
        Assert.That(fired,Is.EqualTo(1));Assert.That(b.IsMineDrillInFlight(5),Is.False);
        Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.hasGem&&targets.Contains(c.identity)),Is.False);
    }
    [UnityTest] public IEnumerator IronveinSixLaunchersAlignAtPortraitSizes()
    {
        yield return LaunchMine();var b=Run.Board;Directory.CreateDirectory(DrillReview+"/Preview");
        foreach(var size in new[]{new Vector2Int(360,640),new Vector2Int(720,1280),new Vector2Int(1080,2400)}){
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
            yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"drill viewport");for(int n=0;n<12;n++)yield return null;
            foreach(var d in b.Mine.drills){
                var m=b.transform.Find("MineDrill_"+d.id);Assert.That(m,Is.Not.Null);
                Assert.That(m.localPosition.y,Is.EqualTo(b.GetCellLocalPosition(0,d.lane).y).Within(.0001f));
                foreach(var r in m.GetComponentsInChildren<SpriteRenderer>())foreach(var p in new[]{r.bounds.min,r.bounds.max}){
                    var s=Camera.main.WorldToScreenPoint(p);Assert.That(s.x,Is.InRange(-.5f,Screen.width+.5f),r.name+" horizontal clipping");
                    Assert.That(s.y,Is.InRange(0f,(float)Screen.height),r.name+" vertical clipping");
                }
            }
            ScreenCapture.CaptureScreenshot(DrillReview+"/Preview/Game-"+size.x+"x"+size.y+".png");yield return null;yield return null;
        }
    }
    [UnityTest,Explicit] public IEnumerator IronveinSixDrillApprovedAssemblyMotionPreview()
    {
        yield return LaunchMine();var b=Run.Board;QuietKitFixture();
        foreach(var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))c.enabled=false;
        foreach(var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))c.enabled=false;
        foreach(var l in Object.FindObjectsByType<GameplayPixelLayoutController>(FindObjectsSortMode.None))l.enabled=false;
        foreach(var l in Object.FindObjectsByType<BoardLayoutController>(FindObjectsSortMode.None))l.enabled=false;
        var scene=EditorSceneManager.LoadSceneInPlayMode("Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision04.unity",new LoadSceneParameters(LoadSceneMode.Additive));
        yield return null;
        var canvas=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Canvas>()).Single(c=>c.isRootCanvas);
        var cam=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Camera>()).Single();cam.enabled=true;
        canvas.transform.Find("LockedBoardCellsAndGems").gameObject.SetActive(false);
        canvas.transform.Find("Stage04_Drills_PreviewOnly").gameObject.SetActive(false);
        var bounds=(RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement");var corners=new Vector3[4];bounds.GetWorldCorners(corners);
        b.transform.position=(corners[0]+corners[2])/2;b.transform.localScale=Vector3.one*(corners[2].x-corners[0].x)/b.LocalBoardWidth;
        b.GetComponent<BoardVisuals>().enabled=false;b.transform.Find("BoardFrame").gameObject.SetActive(false);
        // Real BoardController, gems, sockets and VFX against the approved modular
        // art-review assembly. Actor/HUD snapshot stays unchanged in this proof.
        for(int i=0;i<3;i++)yield return null;
        Directory.CreateDirectory(DrillReview+"/Preview/Motion");CaptureDrillCamera(cam,DrillReview+"/Preview/Aligned.png",720,1280);
        b.TryQueueMineDrillPower(1,4);b.TryQueueMineDrillPower(2,4);
        int frame=0;float next=0;
        while(b.IsBusy||frame==0){
            if(Time.time>=next){CaptureDrillCamera(cam,DrillReview+"/Preview/Motion/"+(frame++).ToString("D4")+".png",720,1280);next=Time.time+.05f;}
            Assert.That(frame,Is.LessThan(600),"bounded real resolver capture");yield return null;
        }
        for(int i=0;i<10;i++){CaptureDrillCamera(cam,DrillReview+"/Preview/Motion/"+(frame++).ToString("D4")+".png",720,1280);yield return new WaitForSeconds(.05f);}
        File.WriteAllText(DrillReview+"/Preview/MotionEvidence.json",JsonUtility.ToJson(new DrillMotionEvidence{frames=frame,scene=scene.path},true));
    }
    static void CaptureDrillCamera(Camera cam,string path,int w,int h){
        var rt=RenderTexture.GetTemporary(w,h,24);rt.filterMode=FilterMode.Point;rt.antiAliasing=1;var prev=RenderTexture.active;
        cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var t=new Texture2D(w,h,TextureFormat.RGBA32,false);
        t.ReadPixels(new Rect(0,0,w,h),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=prev;RenderTexture.ReleaseTemporary(rt);Object.Destroy(t);
    }
    [Serializable]class DrillMotionEvidence{public int frames;public string scene;public string note="Real board-owned clears and drill lifecycle in approved modular art-review assembly; actor/HUD snapshot preserved. Production Game separately tested.";}
}
