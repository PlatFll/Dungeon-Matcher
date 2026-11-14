using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator DungeonCornerAndPillarActualGamePhonePreview()
    {
        RunLaunchOptions.StartingZone="dungeon";RunLaunchOptions.ForestPrototype=false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");yield return Stable();
        foreach(string id in new[]{"pan_villager","knight"})
            if(Run.Waves.ActiveEnemies.Count<3)Assert.That(Run.Waves.TrySummonEnemy(
                Resources.Load<ZoneDefinition>("Zones/dungeon").enemies.First(e=>e.EnemyId==id),out _),Is.True);
        yield return Stable();
        yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"all preview enemies materialized");
        yield return new WaitForSeconds(.3f);
        string root=Path.GetFullPath(".utmp/BombDungeonFixes");Directory.CreateDirectory(root);
        foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),
            new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
        {
            Time.timeScale=1;GameplayPixelLayoutController.ValidationSafeArea=null;
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
                .Invoke(null,new object[]{new Vector2Int(size.x,size.y)});
            yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"preview size");
            GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
            yield return new WaitForSeconds(.5f);Time.timeScale=0;yield return null;Canvas.ForceUpdateCanvases();
            var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
            Assert.That(GameplayPixelLayoutValidator.Validate(layout,out var report),Is.Empty,report);
            DungeonHybridIntegrationTests.AssertNativeLayers(layout);
            var world=GameObject.Find("DungeonPixelLabEnvironment").transform;
            var structure=world.Find("Structure");
            var capitals=structure.GetComponentsInChildren<Image>().Where(i=>i.name=="Support_Capital").ToArray();
            Assert.That(capitals.Length,Is.EqualTo(3));
            foreach(var cap in capitals)Assert.That(GameplayPixelLayoutController.ScreenRect(cap.rectTransform).yMin,
                Is.GreaterThan(Screen.height),"pillar ends beyond the physical screen");
            var corners=world.Find("Side architecture").Cast<Transform>().Where(t=>t.name.EndsWith("corner dressing")).ToArray();
            Assert.That(corners.Length,Is.EqualTo(2));
            var buttons=layout.BottomHud.GetComponentsInChildren<Button>().Select(b=>GameplayPixelLayoutController.ScreenRect((RectTransform)b.transform)).ToArray();
            yield return CaptureDungeonTurnScreen(root+"/diagnostic-"+size.x+"x"+size.y+".png");
            foreach(var corner in corners)foreach(var image in corner.GetComponentsInChildren<Image>())
            {
                var rect=GameplayPixelLayoutController.ScreenRect(image.rectTransform);
                foreach(var button in buttons)Assert.That(rect.Overlaps(button),Is.False,image.name+" "+rect+" stays outside live button "+button);
            }
            string geometry=JsonUtility.ToJson(new DungeonHybridIntegrationTests.Geometry{
                top=GameplayPixelLayoutController.ScreenRect(layout.TopHud),board=GameplayPixelLayoutController.ScreenRect(layout.BoardArea),
                bottom=GameplayPixelLayoutController.ScreenRect(layout.BottomHud),ratio=layout.Current.BoardTexelRatio,
                health=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).Select(s=>GameplayPixelLayoutController.ScreenRect(s.CombatBarRect)).ToArray()});
            string name=size.x+"x"+size.y+(size.z==1?"-safe":"");
            File.WriteAllText(root+"/"+name+".txt",report+"\n"+geometry);
            yield return CaptureDungeonTurnScreen(root+"/"+name+"-after.png");
            // Reproduce the exact old decoration arrangement for a same-frame
            // comparison. UI, characters, gems and native art remain untouched.
            foreach(var corner in corners)corner.gameObject.SetActive(false);
            var upper=structure.GetComponentsInChildren<Image>().Where(i=>i.name=="Support_Core"&&i.rectTransform.anchoredPosition.y>-16).ToArray();
            foreach(var shaft in upper)shaft.gameObject.SetActive(false);
            foreach(var cap in capitals){var p=cap.rectTransform.anchoredPosition;cap.rectTransform.anchoredPosition=new Vector2(p.x,0);}
            yield return CaptureDungeonTurnScreen(root+"/"+name+"-before.png");
            Set(layout.GetComponent<DungeonPresentationArt>(),"pixelLabGeometry",null);
            Call(layout.GetComponent<DungeonPresentationArt>(),"UpdatePixelLabDungeon");
            yield return null;
            Assert.That(GameplayPixelLayoutController.ScreenRect(layout.BoardArea),Is.EqualTo(JsonUtility.FromJson<DungeonHybridIntegrationTests.Geometry>(geometry).board));
        }
        Time.timeScale=1;
    }
    static IEnumerator CaptureDungeonTurnScreen(string path)
    {
        if(File.Exists(path))File.Delete(path);
        yield return null;ScreenCapture.CaptureScreenshot(path);
        yield return Until(()=>File.Exists(path),"screenshot");yield return null;
    }
}
