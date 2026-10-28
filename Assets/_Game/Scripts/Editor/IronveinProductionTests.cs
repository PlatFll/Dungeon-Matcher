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

public sealed class IronveinProductionTests
{
    const string Root="ArtSource/IronveinBelowTopHUD/Integration";
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> ready){float end=Time.realtimeSinceStartup+60;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;yield return Until(()=>Time.realtimeSinceStartup>=end);}
    [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit]public IEnumerator IronveinIntegrationBaseline()=>Capture(false);
    [UnityTest,Explicit]public IEnumerator IronveinIntegrationProduction()=>Capture(true);
    static IEnumerator Capture(bool verify)
    {
        SessionState.SetBool("IronveinIntegrationVerify",verify);EditorUtility.audioMasterMute=true;Size(720,1280);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        verify=SessionState.GetBool("IronveinIntegrationVerify",false);string folder=Root+(verify?"/After":"/Before");Directory.CreateDirectory(folder);
        var records=new List<Record>();
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        foreach(string zone in new[]{"ironvein-excavation","drowned-court","dungeon","magical-forest","frostwake-peaks"})
        {
            string profile=Path.GetFullPath(".utmp/IronveinIntegration/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
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
                        if(zone!="ironvein-excavation")Assert.That(JsonUtility.ToJson(new Controls{items=rec.controls}),Is.EqualTo(JsonUtility.ToJson(new Controls{items=before.controls})),"Other-zone controls unchanged");
                        else AssertApprovedControls(layout);
                        Assert.That(rec.cells,Is.EqualTo(before.cells));
                        AssertProduction(zone,layout,board);
                    }
                }
                if(verify&&(zone=="ironvein-excavation"||zone=="drowned-court"))
                {
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
    static void AssertApprovedControls(GameplayPixelLayoutController layout)
    {
        float unit=BattleFloorAnchor.Unit(layout.GetComponentInParent<Canvas>());
        foreach(var b in layout.GetComponentsInChildren<Button>())
        {
            Vector2 expected=b.name switch { "HealthPotion" or "Bomb"=>new Vector2(47,43), "AbilityButton"=>new Vector2(118,44), _=>Vector2.zero };
            if(expected==Vector2.zero)continue;
            Assert.That(((RectTransform)b.transform).rect.size,Is.EqualTo(expected*unit));
            Assert.That(b.GetComponentInChildren<ResponsiveModularFrameFitter>(),Is.Not.Null,b.name+" approved native frame");
        }
        Assert.That(DungeonPixelLabBattleArt.Controls,Is.SameAs(Resources.Load<DungeonPixelLabBattleArt>("UI/BattlePresentation/DungeonPixelLab")));
    }
    static void AssertProduction(string zone,GameplayPixelLayoutController layout,BoardController board)
    {
        var iron=GameObject.Find("ApprovedIronveinEnvironment");var court=GameObject.Find("ApprovedDrownedCourtEnvironment");
        Assert.That(iron!=null,Is.EqualTo(zone=="ironvein-excavation"));Assert.That(court!=null,Is.EqualTo(zone=="drowned-court"));
        if(zone!="ironvein-excavation"&&zone!="drowned-court")return;
        Assert.That(Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ActiveEnvironment,Is.Null);
        var world=iron?iron:court;
        foreach(var im in world.GetComponentsInChildren<Image>())
        {
            Assert.That(im.raycastTarget,Is.False);Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            if(im.type==Image.Type.Tiled)continue;
            var r=GameplayPixelLayoutController.ScreenRect(im.rectTransform);Assert.That(Mathf.Abs(r.width/im.sprite.rect.width),Is.EqualTo(BattleFloorAnchor.PhysicalPixelScale).Within(.01f),im.name+" X");Assert.That(Mathf.Abs(r.height/im.sprite.rect.height),Is.EqualTo(BattleFloorAnchor.PhysicalPixelScale).Within(.01f),im.name+" Y");
        }
        if(court){Assert.That(Object.FindFirstObjectByType<DrownedCourtAmbientMotion>().tracks.Count,Is.EqualTo(15));return;}
        var rim=iron.transform.Find("IronveinBoardFrame").GetComponent<Canvas>();
        Assert.That(rim.renderMode,Is.EqualTo(RenderMode.WorldSpace));
        Assert.That(rim.overrideSorting,Is.True);
        Assert.That(rim.sortingLayerName,Is.EqualTo("Effects"));Assert.That(rim.sortingOrder,Is.EqualTo(100));
        Assert.That(layout.BoardArea.Find("IronveinBoardFrame"),Is.Null,"No overlay rim may obscure mine projectiles");
        Assert.That(layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection/IronveinPortraitBackdrop/PortraitMask").GetComponent<RectMask2D>(),Is.Not.Null);
        Assert.That(GameObject.Find("GlobalGear"),Is.Null,"No old settings plate behind the approved gear");
        foreach(var bar in layout.GetComponentsInChildren<ModularHealthBarUI>())
        {
            var label=bar.GetComponentInChildren<TMPro.TMP_Text>();if(!label)continue;
            label.ForceMeshUpdate();Assert.That(label.textInfo.lineCount,Is.LessThanOrEqualTo(1),"Native health label remains one line");
        }
        if(UnityEngine.Device.Screen.width<448)
        {
            var hint=GameObject.Find("ConsumableHint").GetComponent<TMPro.TMP_Text>();hint.ForceMeshUpdate();
            Assert.That(hint.textInfo.lineCount,Is.EqualTo(1),"Compact caption fits a single line");
            Assert.That(hint.isTextOverflowing,Is.False,"Compact caption is not clipped");
        }
        Assert.That(board.Mine.drills.Count,Is.EqualTo(6));
        foreach(var d in board.Mine.drills)
        {var mount=board.transform.Find("MineDrill_"+d.id);Assert.That(mount,Is.Not.Null);Assert.That(mount.localPosition.y,Is.EqualTo(board.GetCellLocalPosition(0,d.lane).y).Within(.001f));}
    }
    [Serializable]class Control{public string name,sprite;public Rect rect;}
    [Serializable]class Controls{public Control[] items;}
    [Serializable]class Record{public string zone,size;public Rect board,portrait;public float[] feet;public Rect[] health;public string[] errors,cells;public Control[] controls;}
    [Serializable]class Records{public Record[] cases;}
}
