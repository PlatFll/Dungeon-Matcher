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

public sealed class ForestProductionIntegrationTests
{
    const string Root="ArtSource/ForestProductionIntegration";
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> ready)
    {float end=Time.realtimeSinceStartup+45;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;
        if(Application.isPlaying)yield return new ExitPlayMode();
    }
    [UnityTest,Explicit] public IEnumerator Quick()=>Capture(false);
    [UnityTest,Explicit] public IEnumerator PhoneMatrix()=>Capture(true);
    static IEnumerator Capture(bool full)
    {
        SessionState.SetBool("ForestProductionFull",full);EditorUtility.audioMasterMute=true;
        Size(720,1360);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        full=SessionState.GetBool("ForestProductionFull",false);
        string output=Root+(full?"/PhoneMatrix":"/Quick");Directory.CreateDirectory(output);
        var records=new List<Measurement>();
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        foreach(var zone in full?new[]{"magical-forest","dungeon","drowned-court","ironvein-excavation","frostwake-peaks"}:new[]{"magical-forest"})
        {
            var profile=Path.GetFullPath(".utmp/ForestProduction/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
            File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
            using(AccountProgression.UseDisposableProfile(profile))
            {
                RunLaunchOptions.StartingZone=zone;UnityEngine.Random.InitState(3101);SceneManager.LoadScene("Game");yield return null;
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
                var waves=RunSession.Current.Waves;
                if(zone=="magical-forest")
                {
                    var enemies=RunSession.Current.Zone.Definition.enemies;
                    foreach(var token in new[]{"trailguard","mender"})
                        if(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(enemies.First(e=>e.EnemyId.Contains(token)),out _),Is.True);
                }
                while(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(waves.ActiveEnemies[0].Definition,out _),Is.True);
                yield return Until(()=>RunSession.Current.Continuation.CanCapture&&waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
                var sizes=full?new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)}:new[]{new Vector3Int(720,1360,0)};
                foreach(var size in sizes)
                {
                    GameplayPixelLayoutController.ValidationSafeArea=null;Size(size.x,size.y);
                    yield return Until(()=>UnityEngine.Device.Screen.width==size.x&&UnityEngine.Device.Screen.height==size.y);
                    GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                    yield return new WaitForSecondsRealtime(1.5f);Canvas.ForceUpdateCanvases();
                    var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                    var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);
                    string name=zone+"-"+size.x+"x"+size.y+(size.z==1?"-safe":"");
                    File.WriteAllText(output+"/"+name+".txt",report+"\n"+string.Join("\n",errors));
                    var board=RunSession.Current.Board;
                    var lo=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2)));
                    var hi=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2)));
                    var portrait=(RectTransform)layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection");
                    records.Add(new Measurement{zone=zone,size=name,board=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),portrait=GameplayPixelLayoutController.ScreenRect(portrait),errors=errors.ToArray()});
                    File.WriteAllText(output+"/Geometry.json",JsonUtility.ToJson(new Measurements{cases=records.ToArray()},true));
                    string png=Path.GetFullPath(output+"/"+name+".png");ScreenCapture.CaptureScreenshot(png);yield return Until(()=>File.Exists(png));yield return null;
                    Assert.That(errors,Is.Empty,name);
                    if(zone=="magical-forest")AssertForest(layout);
                    if(zone=="dungeon")DungeonHybridIntegrationTests.AssertNativeLayers(layout);
                }
                if(zone=="magical-forest")
                {
                    var motion=Object.FindFirstObjectByType<ForestAmbientMotion>();Assert.That(motion.tracks.Length,Is.EqualTo(27));
                    var water=motion.tracks.Single(t=>t.kind==ForestAmbientMotion.MotionKind.Water);
                    Assert.That(water.frames.Length,Is.EqualTo(24));
                    motion.Evaluate(0,false);var a=water.image.sprite;motion.Evaluate(.2f,false);Assert.That(water.image.sprite,Is.Not.SameAs(a));
                    motion.Evaluate(0,true);var state=motion.tracks.Select(t=>(t.image.sprite,t.image.rectTransform.anchoredPosition,t.image.color)).ToArray();
                    motion.Evaluate(10,true);Assert.That(motion.tracks.Select(t=>(t.image.sprite,t.image.rectTransform.anchoredPosition,t.image.color)).ToArray(),Is.EqualTo(state));
                    var controls=Object.FindFirstObjectByType<RunControlsUI>();
                    bool reducedBefore=PresentationPreferences.ReducedMotion;
                    try
                    {
                        PresentationPreferences.SetReducedMotion(false);
                        yield return null;
                        var runningWater=water.image.sprite;
                        yield return new WaitForSecondsRealtime(.28f);
                        Assert.That(Object.FindFirstObjectByType<ForestAmbientMotion>(),Is.SameAs(motion),"Layout must not rebuild motion each frame");
                        Assert.That(water.image.sprite,Is.Not.SameAs(runningWater),"Water must advance automatically in play mode");
                        controls.OpenSettings();yield return null;Assert.That(Time.timeScale,Is.Zero);
                        var pausedWater=water.image.sprite;
                        yield return new WaitForSecondsRealtime(.25f);
                        Assert.That(water.image.sprite,Is.SameAs(pausedWater),"Settings pauses ambient water");
                        controls.Close();yield return null;Assert.That(Time.timeScale,Is.GreaterThan(0));
                    }
                    finally { PresentationPreferences.SetReducedMotion(reducedBefore); }
                    Assert.That(RunSession.Current.Continuation.CanCapture,Is.True);
                    var board=RunSession.Current.Board;
                    var response=board.GetImmediateResponses().First();
                    board.ReplayPlayerSwap(response.Source.Column,response.Source.Row,response.Target.Column,response.Target.Row);
                    yield return null;
                    yield return Until(()=>RunSession.Current.Continuation.CanCapture);
                    controls.OpenGuide(CombatGuide.Basics);yield return null;Assert.That(controls.IsModalOpen,Is.True);controls.Close();
                }
            }
        }
        foreach(var group in records.GroupBy(r=>r.size.Substring(r.zone.Length)))
        {
            var first=group.First();foreach(var r in group){Assert.That(r.board.width,Is.EqualTo(first.board.width).Within(.1),r.zone);Assert.That(r.portrait.size.x,Is.EqualTo(first.portrait.size.x).Within(.1));Assert.That(r.portrait.size.y,Is.EqualTo(first.portrait.size.y).Within(.1));}
        }
        yield return new ExitPlayMode();
    }
    static void AssertForest(GameplayPixelLayoutController layout)
    {
        var world=GameObject.Find("ApprovedForestEnvironment");Assert.That(world,Is.Not.Null);
        Assert.That(GameObject.Find("ReferenceKitScenery"),Is.Null);
        Assert.That(Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ActiveEnvironment,Is.Null);
        Assert.That(Object.FindObjectsByType<LivingForestReviewMotion>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(RunSession.Current.Board.transform.Find("BoardFrame").GetComponentsInChildren<SpriteRenderer>(),Is.Empty);
        Assert.That(layout.BoardArea.Find("ForestBoardFrame"),Is.Not.Null);
        Assert.That(world.GetComponentsInChildren<Image>().Any(i=>i.name.Contains("Comet")),Is.False);
        foreach(var im in world.GetComponentsInChildren<Image>())
        {
            Assert.That(im.raycastTarget,Is.False);Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            if(im.type==Image.Type.Tiled)continue;
            var rect=GameplayPixelLayoutController.ScreenRect(im.rectTransform);
            Assert.That(rect.width/im.sprite.rect.width,Is.EqualTo(DungeonPixelLabBattleArt.PhysicalPixelScale).Within(.01),im.name);
            Assert.That(rect.height/im.sprite.rect.height,Is.EqualTo(DungeonPixelLabBattleArt.PhysicalPixelScale).Within(.01),im.name);
        }
        Assert.That(layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection/PixelLabPortraitBackdrop/PortraitMask").GetComponent<RectMask2D>(),Is.Not.Null);
    }
    [Serializable] class Measurement{public string zone,size;public Rect board,portrait;public string[] errors;}
    [Serializable] class Measurements{public Measurement[] cases;}
}
