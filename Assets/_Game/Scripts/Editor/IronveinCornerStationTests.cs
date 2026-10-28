using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    const string CornerRoot="ArtSource/IronveinCornerStations";
    [UnityTest,Explicit] public IEnumerator IronveinCornerBaseline()=>CornerStationProof(false);
    [UnityTest,Explicit] public IEnumerator IronveinCornerRevised()=>CornerStationProof(true);
    IEnumerator CornerStationProof(bool verify)
    {
        UnityEngine.Random.InitState(3101);
        // Use the actual production launch, not the mechanic-isolation fixture
        // which intentionally gives enemies 999-countdown placeholders.
        RunLaunchOptions.ForestPrototype=false;RunLaunchOptions.StartingZone="ironvein-excavation";
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");yield return Stable();
        foreach(string id in new[]{"rivet_gunner","stonewright"})
            if(Run.Waves.ActiveEnemies.Count<3)Assert.That(Run.Waves.TrySummonEnemy(Run.Zone.Definition.enemies.Single(e=>e.EnemyId==id),out _),Is.True);
        yield return Until(()=>Run.Continuation.CanCapture&&Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"corner production spawn settles");
        var visuals=UnityEngine.Object.FindFirstObjectByType<BoardVisuals>();
        Set(visuals,"randomizeTileLayoutEachRun",false);Set(visuals,"tileLayoutSeed",12345);visuals.RefreshPresentationFrame();
        string folder=CornerRoot+"/Preview/"+(verify?"After":"Before");Directory.CreateDirectory(folder);
        var records=new List<CornerRecord>();
        foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
        {
            GameplayPixelLayoutController.ValidationSafeArea=null;
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(size.x,size.y)});
            yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"corner viewport");
            GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
            for(int n=0;n<12;n++)yield return null;
            Time.timeScale=0;Canvas.ForceUpdateCanvases();
            var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
            var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);Assert.That(errors,Is.Empty);
            var board=Run.Board;var cam=Camera.main;
            var lo=cam.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2)));
            var hi=cam.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2)));
            var slots=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).ToArray();
            Assert.That(slots.Length,Is.EqualTo(3));
            var rec=new CornerRecord{size=size.x+"x"+size.y+(size.z==1?"-safe":""),board=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),
                portrait=GameplayPixelLayoutController.ScreenRect((RectTransform)GameObject.Find("PlayerSection").transform),
                feet=slots.Select(BattleFloorAnchor.EnemyFeetScreenY).ToArray(),health=slots.Select(s=>GameplayPixelLayoutController.ScreenRect(s.CombatBarRect)).ToArray(),
                controls=layout.GetComponentsInChildren<Button>().OrderBy(b=>b.name).Select(b=>new CornerControl{name=b.name,rect=GameplayPixelLayoutController.ScreenRect((RectTransform)b.transform),sprite=AssetDatabase.GetAssetPath(b.image?.sprite)}).ToArray()};
            var environment=GameObject.Find("ApprovedIronveinEnvironment");Assert.That(environment,Is.Not.Null);
            if(verify)
            {
                var before=JsonUtility.FromJson<CornerRecords>(File.ReadAllText(CornerRoot+"/Preview/Before/Measurements.json")).items.Single(r=>r.size==rec.size);
                Assert.That(rec.board,Is.EqualTo(before.board));Assert.That(rec.portrait,Is.EqualTo(before.portrait));
                Assert.That(rec.feet,Is.EqualTo(before.feet));Assert.That(rec.health,Is.EqualTo(before.health));
                Assert.That(JsonUtility.ToJson(new CornerControls{items=rec.controls}),Is.EqualTo(JsonUtility.ToJson(new CornerControls{items=before.controls})),"All shared controls remain fixed");
                var parts=environment.GetComponentsInChildren<Image>().Where(i=>i.name.Contains("CornerStation_")).ToArray();
                Assert.That(parts.Length,Is.GreaterThanOrEqualTo(8));
                foreach(var im in parts)
                {
                    var r=GameplayPixelLayoutController.ScreenRect(im.rectTransform);var scale=BattleFloorAnchor.PhysicalPixelScale;
                    Assert.That(r.width/im.sprite.rect.width,Is.EqualTo(scale).Within(.01f),im.name+" native X");
                    Assert.That(r.height/im.sprite.rect.height,Is.EqualTo(scale).Within(.01f),im.name+" native Y");
                    Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(im.raycastTarget,Is.False);
                    Assert.That(r.xMin,Is.GreaterThanOrEqualTo(-.1f));Assert.That(r.xMax,Is.LessThanOrEqualTo(Screen.width+.1f));
                    Assert.That(r.yMin,Is.GreaterThanOrEqualTo(-.1f));
                }
                Assert.That(board.Mine.drills.Count,Is.EqualTo(6));
                var rim=environment.transform.Find("IronveinBoardFrame").GetComponent<Canvas>();
                Assert.That(rim.sortingLayerName,Is.EqualTo("Effects"));Assert.That(rim.sortingOrder,Is.EqualTo(100));
            }
            records.Add(rec);File.WriteAllText(folder+"/Measurements.json",JsonUtility.ToJson(new CornerRecords{items=records.ToArray()},true));
            File.WriteAllText(folder+"/"+rec.size+".txt",report);
            yield return ForegroundScreenshot(folder+"/"+rec.size+".png");Time.timeScale=1;
        }
    }
    [Serializable]class CornerRecord{public string size;public Rect board,portrait;public float[] feet;public Rect[] health;public CornerControl[] controls;}
    [Serializable]class CornerRecords{public CornerRecord[] items;}
    [Serializable]class CornerControl{public string name,sprite;public Rect rect;}
    [Serializable]class CornerControls{public CornerControl[] items;}
}
