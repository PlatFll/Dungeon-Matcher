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

// Runs the real Game scene against disposable profiles; never saves the production scene.
public sealed class SharedEnemyFloorTests
{
    const string Root="ArtSource/DrownedCourtEnvironmentV2/QA/Revision03";
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> ready){float end=Time.realtimeSinceStartup+60;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit] public IEnumerator CaptureBefore()=>Capture(false);
    [UnityTest,Explicit] public IEnumerator AllZonesShareTheApprovedDungeonFloor()=>Capture(true);
    [UnityTest,Explicit] public IEnumerator CaptureDiagnostics()=>Capture(false,true);
    static IEnumerator Capture(bool verify,bool diagnostics=false)
    {
        SessionState.SetBool("SharedEnemyFloorVerify",verify);SessionState.SetBool("SharedEnemyFloorDiagnostics",diagnostics);EditorUtility.audioMasterMute=true;Size(720,1280);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        verify=SessionState.GetBool("SharedEnemyFloorVerify",false);diagnostics=SessionState.GetBool("SharedEnemyFloorDiagnostics",false);string phase=diagnostics?(Environment.GetEnvironmentVariable("DM_FLOOR_ORIGINAL_MATRIX")=="1"?"OriginalMatrix":"Diagnostics"):verify?"After":"Before";
        string folder=Root+"/"+phase;Directory.CreateDirectory(folder);var records=new List<Record>();
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        foreach(string zone in new[]{"dungeon","magical-forest","drowned-court","ironvein-excavation","frostwake-peaks"})
        {
            string profile=Path.GetFullPath(".utmp/SharedEnemyFloor/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
            File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
            using(AccountProgression.UseDisposableProfile(profile))
            {
                RunLaunchOptions.StartingZone=zone;UnityEngine.Random.InitState(3101);SceneManager.LoadScene("Game");yield return null;
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
                var waves=RunSession.Current.Waves;
                while(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(waves.ActiveEnemies[0].Definition,out _),Is.True);
                yield return Until(()=>RunSession.Current.Continuation.CanCapture&&waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
                var sizes=(verify||diagnostics)?new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)}:new[]{new Vector3Int(720,1280,0)};
                foreach(var size in sizes)
                {
                    GameplayPixelLayoutController.ValidationSafeArea=null;Size(size.x,size.y);
                    yield return Until(()=>UnityEngine.Device.Screen.width==size.x&&UnityEngine.Device.Screen.height==size.y);
                    GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                    yield return new WaitForSecondsRealtime(1.25f);Canvas.ForceUpdateCanvases();
                    var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                    string name=zone+"-"+size.x+"x"+size.y+(size.z==1?"-safe":"");
                    var errors=GameplayPixelLayoutValidator.Validate(layout,out string report);File.WriteAllText(folder+"/"+name+".txt",report+"\n"+string.Join("\n",errors));
                    var slots=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).ToArray();Assert.That(slots.Length,Is.EqualTo(3));
                    var feet=new List<float>();var bars=new List<Rect>();
                    foreach(var slot in slots)
                    {
                        var im=slot.EnemySpawnAnchor.GetComponentsInChildren<Image>().Single(i=>i.name=="VisualRoot"&&i.sprite);
                        var r=GameplayPixelLayoutController.ScreenRect(im.rectTransform);float sourceScale=r.height/im.sprite.rect.height;
                        feet.Add(r.yMin+im.sprite.pivot.y*sourceScale);bars.Add(GameplayPixelLayoutController.ScreenRect(slot.CombatBarRect));
                        Debug.Log("FLOOR_DIAGNOSTIC "+name+" visual "+im.rectTransform.anchoredPosition+" sourceScale "+sourceScale+" actor "+((RectTransform)im.GetComponentInParent<EnemyActor>().transform).anchoredPosition+" spawn "+GameplayPixelLayoutController.ScreenRect(slot.EnemySpawnAnchor)+" spritePivot "+im.sprite.pivot+" imagePivot "+im.rectTransform.pivot);
                    }
                    var board=RunSession.Current.Board;var lo=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2)));var hi=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2)));
                    var rec=new Record{zone=zone,size=size.x+"x"+size.y+(size.z==1?"-safe":""),feet=feet.ToArray(),health=bars.ToArray(),top=GameplayPixelLayoutController.ScreenRect(layout.TopHud),board=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),portrait=GameplayPixelLayoutController.ScreenRect((RectTransform)layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection")),errors=errors.ToArray()};records.Add(rec);
                    File.WriteAllText(folder+"/Measurements.json",JsonUtility.ToJson(new Records{cases=records.ToArray()},true));
                    string png=Path.GetFullPath(folder+"/"+name+".png");ScreenCapture.CaptureScreenshot(png);yield return Until(()=>File.Exists(png));yield return null;
                    if(verify)
                    {
                        Assert.That(errors,Is.Empty,name);
                        // Independent captures of untouched HEAD preserve the approved Dungeon registration, including native pixel snapping.
                        var baseline=JsonUtility.FromJson<Records>(File.ReadAllText(Root+"/OriginalMatrix/Measurements.json"));
                        var approved=baseline.cases.Single(r=>r.zone=="dungeon"&&r.size==rec.size);
                        for(int i=0;i<feet.Count;i++)
                        {
                            Assert.That(feet[i],Is.EqualTo(approved.feet[i]).Within(.51f),name+" actor "+i+" approved source floor");
                            Assert.That(bars[i].yMin,Is.EqualTo(approved.health[i].yMin),name+" HP baseline");
                            Assert.That(bars[i].yMax,Is.EqualTo(approved.health[i].yMax),name+" HP top");
                        }
                        Assert.That(feet.Max()-feet.Min(),Is.LessThanOrEqualTo(.51f),name+" shared line");
                    }
                }
            }
        }
        if(verify)
        {
            var geometryBaseline=JsonUtility.FromJson<Records>(File.ReadAllText(Root+"/OriginalMatrix/Measurements.json"));
            foreach(var group in records.GroupBy(r=>r.size))
            {
                var reference=group.Single(r=>r.zone=="dungeon");foreach(var rec in group)
                {
                    Assert.That(rec.feet[0],Is.EqualTo(reference.feet[0]).Within(.51f),rec.zone+" "+rec.size+" cross-zone line");
                    var prior=geometryBaseline.cases.Single(r=>r.zone==rec.zone&&r.size==rec.size);
                    var canonical=geometryBaseline.cases.Single(r=>r.zone=="dungeon"&&r.size==rec.size);
                    var expectedBoard=reference.board;expectedBoard.position+=prior.board.position-canonical.board.position;expectedBoard.size+=prior.board.size-canonical.board.size;
                    var expectedPortrait=reference.portrait;expectedPortrait.position+=prior.portrait.position-canonical.portrait.position;expectedPortrait.size+=prior.portrait.size-canonical.portrait.size;
                    Assert.That(rec.board,Is.EqualTo(expectedBoard),rec.zone+" complete cross-zone board geometry with original enclosure offset");
                    Assert.That(rec.portrait,Is.EqualTo(expectedPortrait),rec.zone+" complete cross-zone portrait geometry with original enclosure offset");
                    Assert.That(rec.board,Is.EqualTo(prior.board),rec.zone+" unchanged complete board rectangle");
                    Assert.That(rec.portrait,Is.EqualTo(prior.portrait),rec.zone+" unchanged complete portrait rectangle");
                }
            }
            var before=JsonUtility.FromJson<Records>(File.ReadAllText(Root+"/Before/Measurements.json"));
            foreach(string zone in new[]{"dungeon","magical-forest"})
            {
                var a=before.cases.Single(r=>r.zone==zone);var b=records.Single(r=>r.zone==zone&&r.size=="720x1280");
                for(int i=0;i<3;i++){Assert.That(b.feet[i],Is.EqualTo(a.feet[i]).Within(.01f),zone+" approved baseline");Assert.That(b.health[i],Is.EqualTo(a.health[i]));}
            }
        }
        yield return new ExitPlayMode();
    }
    [Serializable]class Record{public string zone,size;public float[] feet;public Rect[] health;public Rect top,board,portrait;public string[] errors;}
    [Serializable]class Records{public Record[] cases;}
}
