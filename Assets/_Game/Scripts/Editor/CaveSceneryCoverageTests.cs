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

public sealed class CaveSceneryCoverageTests
{
    const string Root="ArtSource/IronveinTopCoverage/Preview";
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> ready){float end=Time.realtimeSinceStartup+60;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;yield return Until(()=>Time.realtimeSinceStartup>=end);}
    [UnityTearDown]public IEnumerator Cleanup(){SceneManager.sceneLoaded-=SeedScene;Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit]public IEnumerator Before()=>Capture(false);
    [UnityTest,Explicit]public IEnumerator After()=>Capture(true);
    static IEnumerator Capture(bool verify)
    {
        SessionState.SetBool("CaveCoverageVerify",verify);EditorUtility.audioMasterMute=true;Size(1080,2340);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        verify=SessionState.GetBool("CaveCoverageVerify",false);string folder=Root+(verify?"/After":"/Before");Directory.CreateDirectory(folder);
        string profile=Path.GetFullPath(".utmp/CaveCoverage/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
        File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        {
            foreach(string zone in new[]{"ironvein-excavation","dungeon","magical-forest","drowned-court","frostwake-peaks"})
            {
                // A separate disposable save prevents a prior zone continuation from
                // overriding the next zone under test.
                profile=Path.GetFullPath(".utmp/CaveCoverage/"+Guid.NewGuid()+".json");
                File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
                using(AccountProgression.UseDisposableProfile(profile))
                {
                RunLaunchOptions.StartingZone=zone;SceneManager.sceneLoaded+=SeedScene;
                SceneManager.LoadScene("Game");yield return null;SceneManager.sceneLoaded-=SeedScene;
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
                var visuals=Object.FindFirstObjectByType<BoardVisuals>();
                typeof(BoardVisuals).GetField("randomizeTileLayoutEachRun",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(visuals,false);
                typeof(BoardVisuals).GetField("tileLayoutSeed",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(visuals,12345);visuals.RefreshPresentationFrame();
                var waves=RunSession.Current.Waves;
                if(zone=="ironvein-excavation")foreach(string id in new[]{"rivet_gunner","stonewright"})
                    if(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(RunSession.Current.Zone.Definition.enemies.Single(e=>e.EnemyId==id),out _),Is.True);
                yield return Until(()=>RunSession.Current.Continuation.CanCapture&&waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
                var sizes=zone=="ironvein-excavation"?new[]{new Vector3Int(1080,2340,0),new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(800,1800,0),new Vector3Int(1440,3200,0),new Vector3Int(360,640,0)}:new[]{new Vector3Int(1080,2340,0)};
                foreach(var size in sizes)
                {
                    GameplayPixelLayoutController.ValidationSafeArea=null;Size(size.x,size.y);yield return Until(()=>UnityEngine.Device.Screen.width==size.x&&UnityEngine.Device.Screen.height==size.y);
                    GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                    yield return Wait(.4f);Canvas.ForceUpdateCanvases();
                    var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();var board=RunSession.Current.Board;
                    string key=zone+"-"+size.x+"x"+size.y+(size.z==1?"-safe":"");
                    var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);Assert.That(errors,Is.Empty,report);
                    File.WriteAllText(folder+"/"+key+".txt",report.TrimEnd()+"\n");
                    var world=GameObject.Find("ApprovedIronveinEnvironment");
                    var snap=new Snapshot{
                        images=layout.GetComponentsInChildren<Image>().Select(im=>State(im)).OrderBy(s=>s).ToArray(),
                        feet=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).Select(BattleFloorAnchor.EnemyFeetScreenY).ToArray(),
                        cells=board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.StartsWith("CellTile")).Select(r=>r.sprite.name+":"+r.bounds).OrderBy(s=>s).ToArray(),
                        scenery=world?world.GetComponentsInChildren<Image>().Where(im=>im.name!="IronveinTopModules_CavernDepth").Select(State).OrderBy(s=>s).ToArray():Array.Empty<string>()};
                    string data=JsonUtility.ToJson(snap,true);File.WriteAllText(folder+"/"+key+".json",data);
                    string png=Path.GetFullPath(folder+"/"+key+".png");ScreenCapture.CaptureScreenshot(png);yield return Wait(.15f);Assert.That(File.Exists(png),Is.True);
                    if(verify)
                    {
                        Assert.That(data,Is.EqualTo(File.ReadAllText(Root+"/Before/"+key+".json")),"Exact layout, existing art placement and board geometry retained");
                        if(world)ValidateCoverage(world);
                        else Assert.That(IronveinBattleAssembly.Current,Is.Null,"Other zones retain their own presentation");
                    }
                }
                if(verify&&zone=="ironvein-excavation")
                {
                    var controls=Object.FindFirstObjectByType<RunControlsUI>();controls.OpenSettings();yield return null;Assert.That(Time.timeScale,Is.Zero);controls.Close();controls.OpenGuide(CombatGuide.Basics);yield return null;Assert.That(controls.IsModalOpen,Is.True);controls.Close();
                }
                }
            }
        }
        yield return new ExitPlayMode();
    }
    static void SeedScene(Scene scene,LoadSceneMode mode)
    {
        // Match the encounter and weakness choices across independent processes.
        // Unity's seed alone does not control WaveController's SavedRandom.
        UnityEngine.Random.InitState(3101);
        var waves=Object.FindFirstObjectByType<WaveController>();
        typeof(WaveController).GetField("encounterSeed",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(waves,3101);
        typeof(WaveController).GetField("encounterRandom",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(waves,null);
    }
    static string State(Image im)=>im.name+":"+AssetDatabase.GetAssetPath(im.sprite)+":"+GameplayPixelLayoutController.ScreenRect(im.rectTransform)+":"+im.type+":"+im.color;
    static void ValidateCoverage(GameObject world)
    {
        var cave=world.GetComponentsInChildren<Image>().Single(im=>im.name=="IronveinTopModules_CavernDepth");
        var cr=GameplayPixelLayoutController.ScreenRect(cave.rectTransform);
        Assert.That(cave.raycastTarget,Is.False);Assert.That(cave.color,Is.EqualTo(Color.white));
        Assert.That(cave.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
        Assert.That(AssetDatabase.GetAssetPath(cave.sprite),Is.EqualTo("Assets/_Game/Art/IronveinStaged/CavernDepth.png"));
        float sx=cr.width/cave.sprite.rect.width,sy=cr.height/cave.sprite.rect.height;
        Assert.That(sx,Is.EqualTo(sy).Within(.001),"Uniform source pixel scale");
        Assert.That(sx,Is.EqualTo(Mathf.Round(sx)).Within(.001),"Whole physical pixels, no fractional sampling");
        Assert.That(cr.xMin,Is.LessThanOrEqualTo(.01));Assert.That(cr.xMax,Is.GreaterThanOrEqualTo(Screen.width-.01));
        Assert.That(cr.yMax,Is.GreaterThanOrEqualTo(Screen.height-.01),"Cover physical top beyond safe area");
        Assert.That(cr.xMin,Is.EqualTo(Mathf.Round(cr.xMin)).Within(.01));
        Assert.That(cr.yMin,Is.EqualTo(Mathf.Round(cr.yMin)).Within(.01));
        var floor=world.GetComponentsInChildren<Image>().Where(im=>im.name.StartsWith("Revision03_WalkingSurface")).ToArray();
        Assert.That(floor,Is.Not.Empty);
        Assert.That(cr.yMin,Is.LessThan(floor.Max(im=>GameplayPixelLayoutController.ScreenRect(im.rectTransform).yMax)),"Cave continues behind floor without a gap");

    }
    [Serializable]class Snapshot{public string[] images,cells,scenery;public float[] feet;}
}
