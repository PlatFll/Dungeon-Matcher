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
using Object=UnityEngine.Object;

public sealed class SharedZoneFrameTests
{
    const string Root="ArtSource/ForestReworkV2/FrameFit";
    static readonly string[] Zones={"dungeon","magical-forest","drowned-court","ironvein-excavation","frostwake-peaks"};
    [Serializable] public class Measurement { public string zone,viewport; public int width,height,enemyCount; public Rect top,boardArea,cells,portrait,bottom; public float ratio,layoutWidth; public string[] errors; }
    [Serializable] public class Measurements { public Measurement[] cases; }
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> ready)
    {
        float end=Time.realtimeSinceStartup+40;
        while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;
        if(Application.isPlaying)yield return new ExitPlayMode();
    }
    [UnityTest,Explicit("Record existing cross-zone geometry before shared frame correction.")]
    public IEnumerator Baseline()=>Capture("Baseline",false);
    [UnityTest,Explicit("Verify shared board and portrait dimensions across every live zone.")]
    public IEnumerator ConsistentZoneFrames()=>Capture("Corrected",true);
    [UnityTest,Explicit("Verify the compact shared viewport without repeating larger screen captures.")]
    public IEnumerator CompactZoneFrames()=>Capture("Compact",true);
    static IEnumerator Capture(string phase,bool enforce)
    {
        SessionState.SetString("FrameFitPhase",phase);SessionState.SetBool("FrameFitEnforce",enforce);
        EditorUtility.audioMasterMute=true;Size(1080,1920);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();var records=new List<Measurement>();
        string output=Root+"/"+SessionState.GetString("FrameFitPhase","");Directory.CreateDirectory(output);
        enforce=SessionState.GetBool("FrameFitEnforce",false);
        var sizes = enforce ? new[] { new Vector2Int(1080,1920), new Vector2Int(720,1280), new Vector2Int(1080,2400), new Vector2Int(1080,2400), new Vector2Int(360,640) } : new[] { new Vector2Int(1080,1920) };
        if(SessionState.GetString("FrameFitPhase","")=="Compact") sizes=new[]{new Vector2Int(360,640)};
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        for(int device=0;device<sizes.Length;device++)
        {
        var size=sizes[device];Size(size.x,size.y);yield return null;
        yield return Until(()=>UnityEngine.Device.Screen.width==size.x&&UnityEngine.Device.Screen.height==size.y);
        GameplayPixelLayoutController.ValidationSafeArea=device==3?new Rect(12,90,size.x-24,size.y-210):(Rect?)null;
        string viewport=size.x+"x"+size.y+(device==3?"-safe":"");
        int first=records.Count;
        foreach(string zone in Zones)
        {
            string profile=Path.GetFullPath(".utmp/FrameFit/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
            File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
            using(AccountProgression.UseDisposableProfile(profile))
            {
                UnityEngine.Random.InitState(3101);RunLaunchOptions.StartingZone=zone;SceneManager.LoadScene("Game");yield return null;
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
                Assert.That(RunSession.Current.Zone.Definition.zoneId,Is.EqualTo(zone));
                if(enforce)
                {
                    var waves=RunSession.Current.Waves;
                    while(waves.ActiveEnemies.Count<3)
                    {
                        var definition=waves.ActiveEnemies.Count==2 && RunSession.Current.Zone.Definition.apexEnemy
                            ? RunSession.Current.Zone.Definition.apexEnemy : waves.ActiveEnemies[0].Definition;
                        Assert.That(waves.TrySummonEnemy(definition,out _),Is.True,"Fill three real enemy slots");
                    }
                    yield return Until(()=>RunSession.Current.Continuation.CanCapture&&waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
                }
                yield return new WaitForSecondsRealtime(1);
                var owner=Object.FindFirstObjectByType<GameplayPixelLayoutController>();var board=RunSession.Current.Board;
                var layout=board.GetComponent<BoardLayoutController>();
                var portrait=(RectTransform)owner.TopHud.Find("GeneratedTopBattleLayout/PlayerSection");
                var errors=GameplayPixelLayoutValidator.Validate(owner,out string report);
                File.WriteAllText(output+"/"+zone+"-"+viewport+"-layout.txt",report+"\n"+string.Join("\n",errors));
                Vector3 lo=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2,0)));
                Vector3 hi=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2,0)));
                var m=new Measurement{zone=zone,viewport=viewport,enemyCount=RunSession.Current.Waves.ActiveEnemies.Count,width=UnityEngine.Device.Screen.width,height=UnityEngine.Device.Screen.height,
                    top=GameplayPixelLayoutController.ScreenRect(owner.TopHud),boardArea=GameplayPixelLayoutController.ScreenRect(owner.BoardArea),
                    cells=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),portrait=GameplayPixelLayoutController.ScreenRect(portrait),bottom=GameplayPixelLayoutController.ScreenRect(owner.BottomHud),ratio=layout.PhysicalTexelRatio,layoutWidth=board.GetComponent<BoardVisuals>().LayoutLocalWidth,errors=errors.ToArray()};
                records.Add(m);File.WriteAllText(output+"/Geometry.json",JsonUtility.ToJson(new Measurements{cases=records.ToArray()},true));
                string png=Path.GetFullPath(output+"/"+zone+"-"+viewport+".png");if(File.Exists(png))File.Delete(png);ScreenCapture.CaptureScreenshot(png);yield return Until(()=>File.Exists(png));yield return null;
                if(enforce)
                {
                    Assert.That(errors,Is.Empty,zone);
                    var expected=records[first];Assert.That(m.cells.width,Is.EqualTo(expected.cells.width).Within(.1),zone+" cell footprint");
                    Assert.That(m.cells.height,Is.EqualTo(expected.cells.height).Within(.1));Assert.That(m.ratio,Is.EqualTo(expected.ratio).Within(.001));
                    Assert.That(m.portrait.width,Is.EqualTo(expected.portrait.width).Within(.1),zone+" portrait width");Assert.That(m.portrait.height,Is.EqualTo(expected.portrait.height).Within(.1),zone+" portrait height");
                }
            }
        }
        }
        yield return new ExitPlayMode();
    }
}
