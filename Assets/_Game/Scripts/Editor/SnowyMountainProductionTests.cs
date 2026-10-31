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

public sealed class SnowyMountainProductionTests
{
    const string Root="ArtSource/SnowyMountainStaged/Integration";
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> ready){float end=Time.realtimeSinceStartup+60;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;yield return Until(()=>Time.realtimeSinceStartup>=end);}
    [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit]public IEnumerator SnowIntegrationBaseline()=>Capture(false);
    [UnityTest,Explicit]public IEnumerator SnowIntegrationProduction()=>Capture(true);
    static IEnumerator Capture(bool verify)
    {
        SessionState.SetBool("SnowIntegrationVerify",verify);EditorUtility.audioMasterMute=true;Size(720,1280);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        verify=SessionState.GetBool("SnowIntegrationVerify",false);string folder=Root+(verify?"/After":"/Before");Directory.CreateDirectory(folder);
        var records=new List<Record>();
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        foreach(string zone in new[]{"ironvein-excavation","drowned-court","dungeon","magical-forest","frostwake-peaks"})
        {
            string profile=Path.GetFullPath(".utmp/SnowIntegration/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
            File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
            using(AccountProgression.UseDisposableProfile(profile))
            {
                RunLaunchOptions.StartingZone=zone;UnityEngine.Random.InitState(3101);SceneManager.LoadScene("Game");yield return null;
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
                var visuals=Object.FindFirstObjectByType<BoardVisuals>();
                typeof(BoardVisuals).GetField("randomizeTileLayoutEachRun",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(visuals,false);
                typeof(BoardVisuals).GetField("tileLayoutSeed",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(visuals,12345);visuals.RefreshPresentationFrame();
                var waves=RunSession.Current.Waves;
                foreach(string id in zone=="ironvein-excavation"?new[]{"rivet_gunner","stonewright"}:zone=="drowned-court"?new[]{"hammerhead_bruiser","pearl_thief"}:Array.Empty<string>())
                    if(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(RunSession.Current.Zone.Definition.enemies.Single(e=>e.EnemyId==id),out _),Is.True);
                while(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(waves.ActiveEnemies[0].Definition,out _),Is.True);
                yield return Until(()=>RunSession.Current.Continuation.CanCapture&&waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
                foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
                {
                    GameplayPixelLayoutController.ValidationSafeArea=null;Size(size.x,size.y);yield return Until(()=>UnityEngine.Device.Screen.width==size.x&&UnityEngine.Device.Screen.height==size.y);
                    GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                    yield return Wait(.35f);Canvas.ForceUpdateCanvases();
                    var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();string label=size.x+"x"+size.y+(size.z==1?"-safe":""),name=zone+"-"+label;
                    var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);File.WriteAllText(folder+"/"+name+".txt",report+"\n"+string.Join("\n",errors));
                    var board=RunSession.Current.Board;var lo=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2)));var hi=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2)));
                    var slots=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).ToArray();Assert.That(slots.Length,Is.EqualTo(3));
                    var rec=new Record{zone=zone,size=label,board=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),portrait=GameplayPixelLayoutController.ScreenRect((RectTransform)layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection")),feet=slots.Select(BattleFloorAnchor.EnemyFeetScreenY).ToArray(),health=slots.Select(s=>GameplayPixelLayoutController.ScreenRect(s.CombatBarRect)).ToArray(),errors=errors.ToArray()};
                    rec.controls=layout.GetComponentsInChildren<Button>().Select(b=>new Control{name=b.name,rect=GameplayPixelLayoutController.ScreenRect((RectTransform)b.transform),sprite=AssetDatabase.GetAssetPath(b.image?.sprite)}).OrderBy(b=>b.name).ToArray();
                    rec.cells=board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.gameObject.name.StartsWith("CellTile")).Select(r=>AssetDatabase.GetAssetPath(r.sprite)+":"+r.sprite.name).OrderBy(s=>s).ToArray();
                    records.Add(rec);File.WriteAllText(folder+"/Measurements.json",JsonUtility.ToJson(new Records{cases=records.ToArray()},true));
                    string png=Path.GetFullPath(folder+"/"+name+".png");if(File.Exists(png))File.Delete(png);ScreenCapture.CaptureScreenshot(png);yield return Until(()=>File.Exists(png));yield return null;
                    Assert.That(errors,Is.Empty,name);
                    if(verify)
                    {
                        var before=JsonUtility.FromJson<Records>(File.ReadAllText(Root+"/Before/Measurements.json")).cases.Single(r=>r.zone==zone&&r.size==label);
                        Assert.That(rec.board,Is.EqualTo(before.board));Assert.That(rec.portrait,Is.EqualTo(before.portrait));Assert.That(rec.feet,Is.EqualTo(before.feet));Assert.That(rec.health,Is.EqualTo(before.health));
                        Assert.That(JsonUtility.ToJson(new Controls{items=rec.controls}),Is.EqualTo(JsonUtility.ToJson(new Controls{items=before.controls})),"Existing controls unchanged");

                        if(zone!="frostwake-peaks")Assert.That(rec.cells,Is.EqualTo(before.cells));
                        else Assert.That(board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.StartsWith("CellTile")).All(r=>r.sprite==SnowyMountainBattleAssembly.Current.boardCell),Is.True,"Same native cell as Dungeon/Forest");
                        AssertProduction(zone,layout,board);
                    }
                }
                if(verify&&zone=="frostwake-peaks")
                {
                    foreach(var extra in new[]{new Vector2Int(720,1600),new Vector2Int(800,1800),new Vector2Int(1440,3200)})
                    {
                        GameplayPixelLayoutController.ValidationSafeArea=null;Size(extra.x,extra.y);yield return Until(()=>UnityEngine.Device.Screen.width==extra.x&&UnityEngine.Device.Screen.height==extra.y);yield return Wait(.4f);
                        var responsive=Object.FindFirstObjectByType<GameplayPixelLayoutController>();Assert.That(GameplayPixelLayoutValidator.Validate(responsive,out var extraReport),Is.Empty,extraReport);AssertProduction(zone,responsive,RunSession.Current.Board);
                        string extraPng=Path.GetFullPath(folder+"/frostwake-peaks-"+extra.x+"x"+extra.y+".png");ScreenCapture.CaptureScreenshot(extraPng);yield return Wait(.2f);
                    }
                    var controls=Object.FindFirstObjectByType<RunControlsUI>();controls.OpenSettings();yield return null;Assert.That(Time.timeScale,Is.Zero);controls.Close();
                    controls.OpenGuide(CombatGuide.Basics);yield return null;Assert.That(controls.IsModalOpen,Is.True);controls.Close();
                    var b=RunSession.Current.Board;var response=b.GetImmediateResponses().First();int tick=RunSession.Current.MoveClock.Tick;
                    b.ReplayPlayerSwap(response.Source.Column,response.Source.Row,response.Target.Column,response.Target.Row);yield return null;yield return Until(()=>RunSession.Current.Continuation.CanCapture);Assert.That(RunSession.Current.MoveClock.Tick,Is.GreaterThan(tick));
                }
            }
        }
        foreach(var group in records.GroupBy(r=>r.size))
        {var d=group.Single(r=>r.zone=="dungeon");foreach(var r in group){Assert.That(r.board.size.x,Is.EqualTo(d.board.size.x).Within(.001f));Assert.That(r.portrait.size,Is.EqualTo(d.portrait.size));Assert.That(r.feet[0],Is.EqualTo(d.feet[0]).Within(.51f));}}
        yield return new ExitPlayMode();
    }
    static void AssertProduction(string zone,GameplayPixelLayoutController layout,BoardController board)
    {
        var world=GameObject.Find("ApprovedSnowyMountainEnvironment");
        Assert.That(world!=null,Is.EqualTo(zone=="frostwake-peaks"));
        if(!world)return;
        Assert.That(Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ActiveEnvironment,Is.Null,"Legacy mountain scenery bypassed");
        float p=BattleFloorAnchor.PhysicalPixelScale;var cam=Camera.main;
        var structures=world.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.EndsWith("_ConnectedCliff")).ToArray();
        Assert.That(structures.Length,Is.EqualTo(2));
        Assert.That(world.GetComponentsInChildren<UnityEngine.EventSystems.UIBehaviour>(),Is.Empty);
        foreach(var sr in world.GetComponentsInChildren<SpriteRenderer>())
        {
            var lo=cam.WorldToScreenPoint(sr.bounds.min);var hi=cam.WorldToScreenPoint(sr.bounds.max);
            Assert.That((hi.x-lo.x)/sr.sprite.rect.width,Is.EqualTo(p).Within(.02),sr.name+" X native scale");
            Assert.That((hi.y-lo.y)/sr.sprite.rect.height,Is.EqualTo(p).Within(.02),sr.name+" Y native scale");
            Assert.That(sr.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
        }
        var frame=GameplayPixelLayoutController.ScreenRect((RectTransform)layout.BoardArea.Find("SnowyMountainBoardFrame"));
        var opening=new Rect(frame.x+13*p,frame.y+13*p,frame.width-26*p,frame.height-26*p);
        foreach(var sr in structures)
        {
            bool left=sr.name.StartsWith("Left");var lo=cam.WorldToScreenPoint(sr.bounds.min);var t=sr.sprite.texture;
            int edge=Mathf.FloorToInt(((left?.5f:Screen.width-.5f)-lo.x)/p);
            int join=Mathf.FloorToInt(((left?frame.xMin+.5f:frame.xMax-.5f)-lo.x)/p);
            var pixels=t.GetPixels32();
            for(int y=0;y<t.height;y++)
            {
                Assert.That(pixels[y*t.width+edge].a,Is.EqualTo(255),"No physical edge gutter");
                if(lo.y+(y+.5f)*p>=frame.yMin)Assert.That(pixels[y*t.width+join].a,Is.EqualTo(255),"No board-side gutter");
                for(int x=0;x<t.width;x++)if(pixels[y*t.width+x].a>0)Assert.That(opening.Contains(new Vector2(lo.x+(x+.5f)*p,lo.y+(y+.5f)*p)),Is.False,"No cliff pixels over gems");
            }
        }
        var portrait=layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection");
        Assert.That(portrait.Find("PixelLabInteriorMask").GetComponent<RectMask2D>(),Is.Not.Null);
        Assert.That(layout.TopHud.Find("GeneratedTopBattleLayout/BattleArenaFrame"),Is.Null);
        Assert.That(layout.BottomHud.Find("GeneratedBottomHudFrame"),Is.Null);
        var motion=world.GetComponent<SnowyMountainAmbientMotion>();
        // Upper snow lanes are deliberately omitted when shorter viewports have no safe flight path.
        float feet=BattleFloorAnchor.EnemyFeetScreenY(layout.TopHud.GetComponentsInChildren<EnemySlotUI>().First(s=>s.CurrentEnemy));
        int upperCount=Enumerable.Range(0,12).Count(i=>Screen.height-(55+i%3*20)*p-feet-66*p>=12*p);
        Assert.That(motion.tracks.Count(t=>t.frames!=null),Is.EqualTo(3),"Three authored ice-tip glints");
        Assert.That(motion.tracks.Count(t=>t.frames==null),Is.EqualTo(8+upperCount),"Safe upper lanes and eight side snow tracks");
        string profile=Screen.width+"x"+Screen.height+(GameplayPixelLayoutController.ValidationSafeArea.HasValue?"-safe":"");
        foreach(var sr in structures)
        {
            string path="ArtSource/SnowyMountainStaged/SideCornerPolish/Stage04/ConnectedSides/Exports/"+(sr.name.StartsWith("Left")?"Left":"Right")+"_"+profile+".png";
            if(!File.Exists(path))continue;
            var reference=new Texture2D(2,2);reference.LoadImage(File.ReadAllBytes(path));
            Assert.That(sr.sprite.texture.width,Is.EqualTo(reference.width));Assert.That(sr.sprite.texture.height,Is.EqualTo(reference.height));
            var expected=reference.GetPixels32();var actual=sr.sprite.texture.GetPixels32();
            for(int i=0;i<expected.Length;i++)Assert.That(actual[i].a==expected[i].a&&(expected[i].a==0||actual[i].Equals(expected[i])),Is.True,"Exact approved cliff pixel "+profile+" "+sr.name+" "+i);
            Object.Destroy(reference);
        }
        var rng=UnityEngine.Random.state;
        string State()=>string.Join(";",motion.tracks.Select(t=>t.renderer.enabled+":"+t.renderer.transform.localPosition+":"+t.renderer.color+":"+t.renderer.sprite.name));
        motion.Evaluate(0,false);var loop=State();motion.Evaluate(12,false);Assert.That(State(),Is.EqualTo(loop),"Exact ambient loop");
        motion.Evaluate(3,false);Assert.That(UnityEngine.Random.state,Is.EqualTo(rng));
        motion.Evaluate(3,true);Assert.That(motion.tracks.All(t=>!t.renderer.enabled),Is.True);motion.Evaluate(3,false);
    }
    [Serializable]class Control{public string name,sprite;public Rect rect;}
    [Serializable]class Controls{public Control[] items;}
    [Serializable]class Record{public string zone,size;public Rect board,portrait;public float[] feet;public Rect[] health;public string[] errors,cells;public Control[] controls;}
    [Serializable]class Records{public Record[] cases;}
}
