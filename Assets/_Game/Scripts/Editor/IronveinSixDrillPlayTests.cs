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
        float pauseEnd=Time.realtimeSinceStartup+.2f;
        yield return Until(()=>Time.realtimeSinceStartup>=pauseEnd,"paused observation");
        Assert.That(visited.Count,Is.EqualTo(count));Assert.That(shot.localPosition,Is.EqualTo(point));Assert.That(shot.GetComponent<SpriteRenderer>().sprite,Is.SameAs(sprite));
        Time.timeScale=1;yield return Stable();yield return null;
        CollectionAssert.AreEqual(Enumerable.Range(0,b.Width).Reverse(),visited);Assert.That(exploded&&reloaded,Is.True);Assert.That(bit.enabled,Is.True);
        Assert.That(b.transform.Find("RowDrillProjectile_2"),Is.Null);
    }
    [UnityTest] public IEnumerator IronveinLeftDrillBoresBeforeEntryButLaunchShattersImmediately()
    { yield return CheckMineContactTiming(1); }
    [UnityTest] public IEnumerator IronveinRightDrillBoresBeforeEntryButLaunchShattersImmediately()
    { yield return CheckMineContactTiming(2); }
    IEnumerator CheckMineContactTiming(int id)
    {
        float capture=Time.captureDeltaTime;
        try { Time.captureDeltaTime=id==1?1f/60f:1f/30f; yield return CheckMineContactAtFixedRate(id); }
        finally { Time.captureDeltaTime=capture; }
    }
    IEnumerator CheckMineContactAtFixedRate(int id)
    {
        yield return LaunchMine();var b=Run.Board;var d=b.Mine.drills.Single(x=>x.id==id);
        int first=d.fromRight?b.Width-1:0,next=first+(d.fromRight?-1:1),row=d.lane;
        var originals=Enumerable.Range(0,b.Width).Select(x=>b.GetGem(x,row)).ToArray();
        // Launch must also remove a special immediately without activating it.
        originals[first].SetSpecialType(GemSpecialType.ColorCrystal);
        var contacts=new Dictionary<int,float>();var entries=new List<int>();bool launched=false;float launchTime=0;
        b.MineDrillFired+=(f,_,__)=>{if(f==id){launched=true;launchTime=Time.time;}};
        b.MineDrillBoring+=(f,cell,seconds)=>{
            if(f!=id)return;Assert.That(cell.x,Is.Not.EqualTo(first),"first tile never winds up");
            Assert.That(b.GetGem(cell.x,row),Is.SameAs(originals[cell.x]),"target intact during drilling");
            contacts.Add(cell.x,Time.time);Assert.That(b.IsBusy,Is.True);
        };
        b.MineDrillTravelling+=(f,cell,seconds)=>{
            if(f!=id||cell.x<0||cell.x>=b.Width)return;entries.Add(cell.x);
            if(cell.x==first)return;
            Assert.That(contacts.ContainsKey(cell.x),Is.True);
            Assert.That(Time.time-contacts[cell.x],Is.GreaterThanOrEqualTo(BoardController.MineDrillApproachSeconds+BoardController.MineDrillBoreSeconds-.015f));
            Assert.That(b.GetGem(cell.x,row),Is.Null,"entry waits for destruction");
            Assert.That(originals[cell.x]==null||!originals[cell.x].GetComponent<SpriteRenderer>().enabled,Is.True,"entry waits for visible shatter, not just logical removal");
        };
        int reports=0;b.BoardClearResolved+=_=>reports++;
        b.TryQueueMineDrillPower(id,4);yield return Until(()=>launched,"launch");yield return null;
        Assert.That(Time.time-launchTime,Is.LessThan(BoardController.MineDrillTravelSeconds+.02f));
        Assert.That(b.GetGem(first,row),Is.Null,"first cell removed during launch travel");
        Assert.That(originals[first]==null||!originals[first].GetComponent<SpriteRenderer>().enabled,Is.True,"first cell skips preparation flash");
        yield return Until(()=>contacts.ContainsKey(next),"contact with next gem");
        var shot=b.transform.Find("RowDrillProjectile_"+id);Assert.That(shot,Is.Not.Null);
        var contact=b.GetCellLocalPosition(next,row)+(d.fromRight?Vector3.right:Vector3.left)*b.CellSize*.7f;
        // Observe arrival rather than racing this test coroutine against the
        // renderer on the frame where the short approach completes.
        yield return Until(()=>shot.localPosition==contact,"tip reaches contact");
        Assert.That(Time.time-contacts[next],Is.LessThan(BoardController.MineDrillApproachSeconds+.05f),"approach must not consume the drilling hold");
        Vector3 point=shot.localPosition;var sprite=shot.GetComponent<SpriteRenderer>().sprite;float sampled=Time.time;
        Assert.That(Mathf.Abs(point.x-b.GetCellLocalPosition(next,row).x),Is.GreaterThan(b.CellSize*.6f),"bit body stays outside occupied cell");
        Assert.That(b.GetGem(next,row),Is.SameAs(originals[next]));
        yield return Until(()=>Time.time-sampled>=.055f,"sample multiple spinning frames");
        Assert.That(shot.localPosition,Is.EqualTo(point),"spin holds at contact without advancing");
        Assert.That(shot.GetComponent<SpriteRenderer>().sprite,Is.Not.SameAs(sprite),"bit keeps spinning during the hold");
        Assert.That(b.GetGem(next,row),Is.SameAs(originals[next]),"target still intact before shatter");
        yield return Stable();
        var order=Enumerable.Range(0,b.Width);CollectionAssert.AreEqual(d.fromRight?order.Reverse():order,entries);
        Assert.That(contacts.Count,Is.EqualTo(b.Width-1));Assert.That(reports,Is.Zero);
    }
    [UnityTest] public IEnumerator IronveinDrillBreaksStoneAtContactBeforeMovingIntoItsCell()
    {
        yield return LaunchMine();var b=Run.Board;int row=b.Mine.drills[0].lane;
        // Only these two non-special cells are eligible for controlled stones.
        for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++)
            b.GetGem(x,y).SetSpecialType(y==row&&(x==0||x==1)?GemSpecialType.None:GemSpecialType.ColorCrystal);
        Assert.That(b.TryQueuePlaceMineStones(Run.Waves.ActiveEnemies[0],2,6,MineStoneStage.Obsidian),Is.True);yield return Stable();
        var first=new Vector2Int(0,row);var next=new Vector2Int(1,row);
        var barriers=(IDictionary)Get(b,"barricadeCells");
        var firstState=barriers[first];var nextState=barriers[next];bool contact=false,entered=false,launched=false;
        b.MineDrillFired+=(id,_,__)=>{if(id==1)launched=true;};
        b.MineDrillBoring+=(id,cell,seconds)=>{if(id!=1||cell!=next)return;contact=true;
            Assert.That(b.MineStoneTargets().Any(t=>t.Cell==cell),Is.True);Assert.That(Get(nextState,"ViewObject"),Is.Not.Null);};
        b.MineDrillTravelling+=(id,cell,seconds)=>{if(id!=1||cell!=next)return;entered=true;
            Assert.That(contact,Is.True);Assert.That(b.MineStoneTargets().Any(t=>t.Cell==cell),Is.False);
            Assert.That(Get(nextState,"ViewObject"),Is.Null,"stone view is removed before entry");};
        b.TryQueueMineDrillPower(1,4);yield return Until(()=>launched,"stone launch");yield return null;
        Assert.That(b.MineStoneTargets().Any(t=>t.Cell==first),Is.False);Assert.That(Get(firstState,"ViewObject"),Is.Null,"first stone shatters on launch");
        yield return Stable();Assert.That(contact&&entered,Is.True);
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
        string motionRoot=DrillReview+"/ContactTiming";
        Directory.CreateDirectory(motionRoot+"/Motion");CaptureDrillCamera(cam,motionRoot+"/Aligned.png",720,1280);
        b.TryQueueMineDrillPower(1,4);b.TryQueueMineDrillPower(2,4);
        int frame=0;float next=0;
        while(b.IsBusy||frame==0){
            if(Time.time>=next){CaptureDrillCamera(cam,motionRoot+"/Motion/"+(frame++).ToString("D4")+".png",720,1280);next=Time.time+.05f;}
            Assert.That(frame,Is.LessThan(600),"bounded real resolver capture");yield return null;
        }
        for(int i=0;i<10;i++){CaptureDrillCamera(cam,motionRoot+"/Motion/"+(frame++).ToString("D4")+".png",720,1280);yield return new WaitForSeconds(.05f);}
        File.WriteAllText(motionRoot+"/MotionEvidence.json",JsonUtility.ToJson(new DrillMotionEvidence{frames=frame,scene=scene.path},true));
    }
    static void CaptureDrillCamera(Camera cam,string path,int w,int h){
        var rt=RenderTexture.GetTemporary(w,h,24);rt.filterMode=FilterMode.Point;rt.antiAliasing=1;var prev=RenderTexture.active;
        cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var t=new Texture2D(w,h,TextureFormat.RGBA32,false);
        t.ReadPixels(new Rect(0,0,w,h),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=prev;RenderTexture.ReleaseTemporary(rt);Object.Destroy(t);
    }
    [Serializable]class DrillMotionEvidence{public int frames;public string scene;public string note="Real board-owned clears and drill lifecycle in approved modular art-review assembly; actor/HUD snapshot preserved. Production Game separately tested.";}
}
