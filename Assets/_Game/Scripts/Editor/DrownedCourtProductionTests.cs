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

public sealed class DrownedCourtProductionTests
{
    const string Root="ArtSource/DrownedCourtEnvironmentV2/Integration";
    const string Binding="Assets/_Game/Resources/UI/BattlePresentation/CourtAssembly.asset";
    const string DisabledBinding="Assets/_Game/Art/DrownedCourtEnvironmentV2/CourtAssembly_LegacyTest.asset";
    static void RestoreBinding(){if(AssetDatabase.LoadAssetAtPath<DrownedCourtBattleAssembly>(DisabledBinding))Assert.That(AssetDatabase.MoveAsset(DisabledBinding,Binding),Is.Empty);}
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> ready){float end=Time.realtimeSinceStartup+60;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;if(Application.isPlaying)yield return new ExitPlayMode();RestoreBinding();}
    [UnityTest,Explicit]public IEnumerator CaptureBefore()=>Capture(false);
    [UnityTest,Explicit]public IEnumerator VerifyProduction()=>Capture(true);
    static IEnumerator Capture(bool verify)
    {
        // Baseline uses the identical gameplay source with only the new optional art resource disconnected.
        if(!verify)Assert.That(AssetDatabase.MoveAsset(Binding,DisabledBinding),Is.Empty);
        SessionState.SetBool("CourtProductionVerify",verify);EditorUtility.audioMasterMute=true;Size(720,1280);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        verify=SessionState.GetBool("CourtProductionVerify",false);string folder=Root+(verify?"/After":"/Before");Directory.CreateDirectory(folder);
        var records=new List<Record>();
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        foreach(string zone in verify?new[]{"drowned-court","dungeon","magical-forest","ironvein-excavation","frostwake-peaks"}:new[]{"drowned-court"})
        {
            string profile=Path.GetFullPath(".utmp/CourtProduction/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
            File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
            using(AccountProgression.UseDisposableProfile(profile))
            {
                RunLaunchOptions.StartingZone=zone;UnityEngine.Random.InitState(3101);SceneManager.LoadScene("Game");yield return null;
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
                // Decorative variants deliberately use Environment.TickCount in production. Freeze the
                // fixture's cosmetic seed in BOTH captures so exact artwork comparisons are meaningful.
                var visuals=Object.FindFirstObjectByType<BoardVisuals>();
                typeof(BoardVisuals).GetField("randomizeTileLayoutEachRun",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(visuals,false);
                typeof(BoardVisuals).GetField("tileLayoutSeed",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(visuals,12345);
                visuals.RefreshPresentationFrame();
                var waves=RunSession.Current.Waves;
                if(zone=="drowned-court")foreach(string id in new[]{"hammerhead_bruiser","pearl_thief"})
                    if(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(RunSession.Current.Zone.Definition.enemies.Single(e=>e.EnemyId==id),out _),Is.True);
                while(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(waves.ActiveEnemies[0].Definition,out _),Is.True);
                yield return Until(()=>RunSession.Current.Continuation.CanCapture&&waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
                foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
                {
                    GameplayPixelLayoutController.ValidationSafeArea=null;Size(size.x,size.y);yield return Until(()=>UnityEngine.Device.Screen.width==size.x&&UnityEngine.Device.Screen.height==size.y);
                    GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                    yield return new WaitForSecondsRealtime(1.25f);Canvas.ForceUpdateCanvases();
                    var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();string label=size.x+"x"+size.y+(size.z==1?"-safe":""),name=zone+"-"+label;
                    var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);File.WriteAllText(folder+"/"+name+".txt",report+"\n"+string.Join("\n",errors));
                    var board=RunSession.Current.Board;var lo=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2)));var hi=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2)));
                    var slots=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).ToArray();Assert.That(slots.Length,Is.EqualTo(3));
                    var rec=new Record{zone=zone,size=label,board=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),portrait=GameplayPixelLayoutController.ScreenRect((RectTransform)layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection")),feet=slots.Select(BattleFloorAnchor.EnemyFeetScreenY).ToArray(),health=slots.Select(s=>GameplayPixelLayoutController.ScreenRect(s.CombatBarRect)).ToArray(),errors=errors.ToArray()};
                    rec.controls=layout.GetComponentsInChildren<UnityEngine.UI.Button>().Select(b=>new Control{name=b.name,rect=GameplayPixelLayoutController.ScreenRect((RectTransform)b.transform),sprite=AssetDatabase.GetAssetPath(b.image?.sprite)}).OrderBy(b=>b.name).ToArray();
                    rec.cells=board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.gameObject.name.StartsWith("CellTile")).Select(r=>AssetDatabase.GetAssetPath(r.sprite)+":"+r.sprite.name).OrderBy(s=>s).ToArray();
                    records.Add(rec);File.WriteAllText(folder+"/Measurements.json",JsonUtility.ToJson(new Records{cases=records.ToArray()},true));
                    string png=Path.GetFullPath(folder+"/"+name+".png");ScreenCapture.CaptureScreenshot(png);yield return Until(()=>File.Exists(png));yield return null;
                    Assert.That(errors,Is.Empty,name);
                    if(verify&&zone=="drowned-court")
                    {
                        var before=JsonUtility.FromJson<Records>(File.ReadAllText(Root+"/Before/Measurements.json")).cases.Single(r=>r.size==label);
                        Assert.That(rec.board,Is.EqualTo(before.board));Assert.That(rec.portrait,Is.EqualTo(before.portrait));Assert.That(rec.feet,Is.EqualTo(before.feet));Assert.That(rec.health,Is.EqualTo(before.health));
                        Assert.That(JsonUtility.ToJson(new Controls{items=rec.controls}),Is.EqualTo(JsonUtility.ToJson(new Controls{items=before.controls})),"Shared controls unchanged");Assert.That(rec.cells,Is.EqualTo(before.cells));
                        AssertCourt(layout);
                    }
                }
                if(verify&&zone=="drowned-court")
                {
                    var motion=Object.FindFirstObjectByType<DrownedCourtAmbientMotion>();Assert.That(motion.tracks.Count,Is.EqualTo(15));
                    motion.automatic=false;motion.Evaluate(0,false);var clam=motion.tracks.Single(t=>t.image.name=="Clam");var closed=clam.image.sprite;
                    motion.Evaluate(6,false);Assert.That(clam.image.sprite,Is.Not.SameAs(closed));motion.Evaluate(24,false);Assert.That(clam.image.sprite,Is.SameAs(closed));
                    motion.Evaluate(1,true);var a=State(motion);motion.Evaluate(9,true);Assert.That(State(motion),Is.EqualTo(a));
                    bool reduced=PresentationPreferences.ReducedMotion;var controls=Object.FindFirstObjectByType<RunControlsUI>();
                    try
                    {
                        PresentationPreferences.SetReducedMotion(false);motion.automatic=true;yield return null;var moving=State(motion);yield return new WaitForSecondsRealtime(.5f);Assert.That(State(motion),Is.Not.EqualTo(moving));
                        controls.OpenSettings();yield return null;Assert.That(Time.timeScale,Is.Zero);var paused=State(motion);yield return new WaitForSecondsRealtime(.3f);Assert.That(State(motion),Is.EqualTo(paused));controls.Close();
                        controls.OpenGuide(CombatGuide.Basics);yield return null;Assert.That(controls.IsModalOpen,Is.True);controls.Close();
                    }finally{PresentationPreferences.SetReducedMotion(reduced);}
                    var board=RunSession.Current.Board;var response=board.GetImmediateResponses().First();int tick=RunSession.Current.MoveClock.Tick;
                    board.ReplayPlayerSwap(response.Source.Column,response.Source.Row,response.Target.Column,response.Target.Row);yield return null;yield return Until(()=>RunSession.Current.Continuation.CanCapture);Assert.That(RunSession.Current.MoveClock.Tick,Is.GreaterThan(tick));
                }
            }
        }
        // Different existing zone camera matrices differ by 0.000061 screen pixels; require sub-thousandth-pixel equality.
        if(verify)foreach(var group in records.GroupBy(r=>r.size))
        {var d=group.Single(r=>r.zone=="dungeon");foreach(var r in group){Assert.That(r.board.width,Is.EqualTo(d.board.width).Within(.001f));Assert.That(r.board.height,Is.EqualTo(d.board.height).Within(.001f));Assert.That(r.portrait.size,Is.EqualTo(d.portrait.size));Assert.That(r.feet[0],Is.EqualTo(d.feet[0]).Within(.51f));}}
        yield return new ExitPlayMode();RestoreBinding();
    }
    static string State(DrownedCourtAmbientMotion m)=>string.Join("|",m.tracks.Select(t=>t.image.sprite.name+":"+t.image.color.a+":"+t.image.rectTransform.anchoredPosition));
    static void AssertCourt(GameplayPixelLayoutController layout)
    {
        var world=GameObject.Find("ApprovedDrownedCourtEnvironment");Assert.That(world,Is.Not.Null);Assert.That(GameObject.Find("ReferenceKitScenery"),Is.Null);
        Assert.That(Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ActiveEnvironment,Is.Null);
        Assert.That(layout.BoardArea.Find("CourtBoardFrame"),Is.Not.Null);
        Assert.That(layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection/CourtPortraitBackdrop/PortraitMask").GetComponent<RectMask2D>(),Is.Not.Null);
        foreach(var im in world.GetComponentsInChildren<Image>())
        {Assert.That(im.raycastTarget,Is.False);Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));if(im.type==Image.Type.Tiled)continue;var r=GameplayPixelLayoutController.ScreenRect(im.rectTransform);Assert.That(r.width/im.sprite.rect.width,Is.EqualTo(BattleFloorAnchor.PhysicalPixelScale).Within(.01f),im.name+" X");Assert.That(r.height/im.sprite.rect.height,Is.EqualTo(BattleFloorAnchor.PhysicalPixelScale).Within(.01f),im.name+" Y");}
    }
    [Serializable]class Control{public string name,sprite;public Rect rect;}
    [Serializable]class Controls{public Control[] items;}
    [Serializable]class Record{public string zone,size;public Rect board,portrait;public float[] feet;public Rect[] health;public string[] errors,cells;public Control[] controls;}
    [Serializable]class Records{public Record[] cases;}
}
