using System;
using System.Collections;
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

// Actual Game-scene comparison against the original shallow-floor fallback.
// Only the optional surface lookup changes; production data and actors stay fixed.
public sealed class DungeonFloorDepthTests
{
    const string Root="ArtSource/DungeonFloorDepth/Revision02";
    // World-canvas projection uses float matrices; below 1/20 of a screen pixel.
    // Captured pixel comparisons remain exact, with no color/position tolerance.
    const float ProjectionEpsilon=.05f;
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> condition){float end=Time.realtimeSinceStartup+60;while(!condition()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;yield return Until(()=>Time.realtimeSinceStartup>=end);}
    [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit]public IEnumerator StandingDepthPreservesGameLayout()
    {
        Directory.CreateDirectory(Root+"/Preview");Directory.CreateDirectory(Root+"/QA");
        EditorUtility.audioMasterMute=true;Size(720,1280);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        foreach(string zone in new[]{"dungeon","drowned-court"})
        {
            Time.timeScale=1;
            string profile=Path.GetFullPath(".utmp/DungeonFloorDepth/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
            File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
            using(AccountProgression.UseDisposableProfile(profile))
            {
                RunLaunchOptions.StartingZone=zone;UnityEngine.Random.InitState(3101);SceneManager.LoadScene("Game");yield return null;
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
                var waves=RunSession.Current.Waves;
                foreach(string id in zone=="dungeon"?new[]{"pan_villager","knight"}:Array.Empty<string>())
                    if(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(RunSession.Current.Zone.Definition.enemies.Single(e=>e.EnemyId==id),out _),Is.True);
                while(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(waves.ActiveEnemies[0].Definition,out _),Is.True);
                yield return Until(()=>RunSession.Current.Continuation.CanCapture&&waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
                var profiles=zone=="dungeon"?new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)}:new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,2400,1)};
                foreach(var size in profiles)
                {
                    Time.timeScale=1;GameplayPixelLayoutController.ValidationSafeArea=null;Size(size.x,size.y);
                    yield return Until(()=>UnityEngine.Device.Screen.width==size.x&&UnityEngine.Device.Screen.height==size.y);
                    GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                    yield return Wait(.6f);Time.timeScale=0;yield return Wait(.6f);Canvas.ForceUpdateCanvases();
                    var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                    string key=zone+"-"+size.x+"x"+size.y+(size.z==1?"-safe":"");
                    var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);Assert.That(errors,Is.Empty,report);
                    var feet=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).Select(BattleFloorAnchor.EnemyFeetScreenY).ToArray();
                    Assert.That(feet.Max()-feet.Min(),Is.LessThanOrEqualTo(.51f));
                    File.WriteAllText(Root+"/QA/"+key+".txt",report+"\nFeet="+string.Join(",",feet));
                    if(zone=="dungeon")
                    {
                        DungeonHybridIntegrationTests.AssertNativeLayers(layout);
                        var art=DungeonPixelLabBattleArt.Current;
                        var selected=art.Sprite("Floor_Standing");Assert.That(selected,Is.Not.Null);
                        var lookup=(System.Collections.Generic.Dictionary<string,Sprite>)typeof(DungeonPixelLabBattleArt)
                            .GetField("lookup",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(art);
                        var owner=layout.GetComponent<DungeonPresentationArt>();
                        string beforeGeometry=Geometry(layout);
                        try
                        {
                            lookup["Floor_Standing"]=null;
                            Rebuild(owner);yield return null;yield return null;
                            yield return Capture(Root+"/Preview/"+key+"-before.png");
                        }
                        finally {lookup["Floor_Standing"]=selected;Rebuild(owner);}
                        yield return null;yield return null;
                        yield return Capture(Root+"/Preview/"+key+"-after.png");
                        DungeonHybridIntegrationTests.AssertNativeLayers(layout);
                        var world=GameObject.Find("DungeonPixelLabEnvironment").transform;
                        Assert.That(world.Find("Standing floor rear courses"),Is.Null,"Rejected stacked courses must be absent");
                        var surface=(RectTransform)world.Find("Standing platform/Connected standing floor");
                        Assert.That(surface,Is.Not.Null);
                        var rect=GameplayPixelLayoutController.ScreenRect(surface);
                        float scale=BattleFloorAnchor.PhysicalPixelScale;
                        Assert.That(rect.height,Is.EqualTo(32*scale).Within(ProjectionEpsilon));
                        Assert.That(rect.xMin,Is.EqualTo(layout.Current.Viewport.xMin+106*scale).Within(ProjectionEpsilon));
                        Assert.That(rect.xMax,Is.EqualTo(size.x).Within(ProjectionEpsilon));
                        // Enemy soles can fall between scenery texels. Scenery retains the
                        // existing nearest-native-pixel anchor, never moves the actor.
                        float anchored = layout.Current.Viewport.yMax - Mathf.Round(
                            (layout.Current.Viewport.yMax-feet[0]-BattleFloorAnchor.EnemyFloorInset*scale)/scale)*scale;
                        Assert.That(rect.yMax,Is.EqualTo(anchored+20*scale).Within(ProjectionEpsilon));
                        Assert.That(rect.yMin,Is.EqualTo(anchored-12*scale).Within(ProjectionEpsilon));
                        Assert.That(feet[0],Is.InRange(rect.yMin+5*scale,rect.yMin+7*scale));
                        var floorImages=surface.GetComponentsInChildren<Image>();
                        Assert.That(floorImages.All(i=>i.sprite==selected),Is.True);
                        var slabs=floorImages.Select(i=>GameplayPixelLayoutController.ScreenRect(i.rectTransform)).OrderBy(r=>r.xMin).ToArray();
                        for(int n=1;n<slabs.Length;n++)Assert.That(slabs[n].xMin,Is.EqualTo(slabs[n-1].xMax).Within(ProjectionEpsilon),"No module gap or overlap");
                        var fascia=world.Find("Standing platform").GetComponentsInChildren<Image>().First(i=>i.name=="Fascia_Left");
                        var fasciaRect=GameplayPixelLayoutController.ScreenRect(fascia.rectTransform);
                        Assert.That(fasciaRect.yMax,Is.EqualTo(rect.yMin).Within(ProjectionEpsilon),"Fascia starts directly below floor");
                        foreach(var original in world.Find("Standing platform").GetComponentsInChildren<Image>()
                            .Where(i=>i.name.StartsWith("Floor_Mid")||i.name=="Floor_Left"))
                            Assert.That(GameplayPixelLayoutController.ScreenRect(original.rectTransform).xMax,
                                Is.LessThanOrEqualTo(rect.xMin+ProjectionEpsilon),"Original portrait-side course must never overlap the replacement");
                        var portrait=GameplayPixelLayoutController.ScreenRect((RectTransform)GameObject.Find("PlayerSection").transform.Find("PixelLabPortraitBackdrop/PortraitMask"));
                        Assert.That(Geometry(layout),Is.EqualTo(beforeGeometry),"Actor feet, health, board and UI must remain exact");
                        File.WriteAllText(Root+"/QA/"+key+"-geometry.txt",beforeGeometry);
                        // The authorized correction changes the floor and the grounding of
                        // gate/column feet/props behind it, never UI, actors, or lower art.
                        var gateBounds=world.Find("Approved hybrid gate").GetComponentsInChildren<Image>()
                            .Select(i=>GameplayPixelLayoutController.ScreenRect(i.rectTransform)).ToArray();
                        float highestGate=gateBounds.Max(r=>r.yMax);
                        var corbels=world.Find("Structure").GetComponentsInChildren<Image>().Where(i=>i.name=="Support_Corbel");
                        float lowestCorbel=corbels.Min(i=>GameplayPixelLayoutController.ScreenRect(i.rectTransform).yMin);
                        var allowed=Rect.MinMaxRect(0,lowestCorbel,size.x,highestGate);
                        CheckPixels(key,allowed,portrait);

                    }
                    else
                    {
                        Assert.That(GameObject.Find("Connected standing floor"),Is.Null,"The underwater zone is unchanged");
                        yield return Capture(Root+"/Preview/"+key+".png");
                    }
                }
                Time.timeScale=1;GameplayPixelLayoutController.ValidationSafeArea=null;Size(720,1280);yield return Wait(.4f);
                var board=RunSession.Current.Board;int moves=board.CaptureContinuation(id=>id).moves;
                var response=board.GetImmediateResponses().First();board.ReplayPlayerSwap(response.Source.Column,response.Source.Row,response.Target.Column,response.Target.Row);
                yield return Until(()=>RunSession.Current.Continuation.CanCapture);
                Assert.That(board.CaptureContinuation(id=>id).moves,Is.EqualTo(moves+1));
                var controls=Object.FindFirstObjectByType<RunControlsUI>();controls.OpenSettings();yield return null;Assert.That(controls.IsModalOpen,Is.True);controls.Close();yield return null;
            }
        }
        yield return new ExitPlayMode();
    }
    static void Rebuild(DungeonPresentationArt owner)
    {
        typeof(DungeonPresentationArt).GetField("pixelLabGeometry",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(owner,null);
    }
    static IEnumerator Capture(string path)
    {
        string full=Path.GetFullPath(path);if(File.Exists(full))File.Delete(full);
        Canvas.ForceUpdateCanvases();ScreenCapture.CaptureScreenshot(full);yield return Until(()=>File.Exists(full));yield return null;
    }
    static string Geometry(GameplayPixelLayoutController layout)
    {
        var images=layout.GetComponentsInChildren<Image>().Where(i=>i.name!="VisualRoot"&&i.name!="CharacterImage")
            .Select(i=>i.name+":"+GameplayPixelLayoutController.ScreenRect(i.rectTransform)).OrderBy(s=>s);
        var feet=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).Select(BattleFloorAnchor.EnemyFeetScreenY);
        var cells=RunSession.Current.Board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.StartsWith("CellTile_")).Select(r=>r.name+":"+r.bounds).OrderBy(s=>s);
        return "Feet="+string.Join(",",feet)+"\n"+string.Join("\n",images)+"\n"+string.Join("\n",cells);
    }
    static void CheckPixels(string key,Rect allowed,Rect portrait)
    {
        var a=new Texture2D(2,2);var b=new Texture2D(2,2);
        try
        {
            Assert.That(a.LoadImage(File.ReadAllBytes(Root+"/Preview/"+key+"-before.png")),Is.True);
            Assert.That(b.LoadImage(File.ReadAllBytes(Root+"/Preview/"+key+"-after.png")),Is.True);
            var ap=a.GetPixels32();var bp=b.GetPixels32();int changed=0,outside=0,portraitChanged=0;
            for(int i=0;i<ap.Length;i++)if(!ap[i].Equals(bp[i]))
            {
                changed++;var point=new Vector2(i%a.width+.5f,i/a.width+.5f);
                if(!allowed.Contains(point))outside++;
                if(portrait.Contains(point))portraitChanged++;
            }
            File.WriteAllText(Root+"/QA/"+key+"-pixels.txt",$"Changed={changed}; outside environment correction={outside}; portrait={portraitChanged}; allowed={allowed}; portraitRect={portrait}");
            Assert.That(changed,Is.GreaterThan(0));Assert.That(outside,Is.Zero);Assert.That(portraitChanged,Is.Zero);
        }
        finally{Object.DestroyImmediate(a);Object.DestroyImmediate(b);}
    }
}
