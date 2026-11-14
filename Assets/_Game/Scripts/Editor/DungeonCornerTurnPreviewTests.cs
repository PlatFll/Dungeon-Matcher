using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
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
        string root=Path.GetFullPath(".utmp/DungeonCornerFoundations");Directory.CreateDirectory(root);
        foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),
            new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
        {
            Time.timeScale=1;GameplayPixelLayoutController.ValidationSafeArea=null;
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
                .Invoke(null,new object[]{new Vector2Int(size.x,size.y)});
            yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"preview size");
            GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
            yield return new WaitForSeconds(.5f);Time.timeScale=0;yield return null;Canvas.ForceUpdateCanvases();
            // Let late-frame native anchors run after the layout rebuild before
            // recording geometry, exactly as they do for a rendered frame.
            yield return null;
            var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
            Assert.That(GameplayPixelLayoutValidator.Validate(layout,out var report),Is.Empty,report);
            DungeonHybridIntegrationTests.AssertNativeLayers(layout);
            var world=GameObject.Find("DungeonPixelLabEnvironment").transform;
            var structure=world.Find("Structure");
            var capitals=structure.GetComponentsInChildren<Image>().Where(i=>i.name=="Support_Capital").ToArray();
            Assert.That(capitals.Length,Is.EqualTo(3));
            foreach(var cap in capitals)Assert.That(GameplayPixelLayoutController.ScreenRect(cap.rectTransform).yMin,
                Is.GreaterThan(Screen.height),"pillar ends beyond the physical screen");
            var architecture=world.Find("Side architecture");
            Assert.That(architecture.Cast<Transform>().Any(t=>t.name.EndsWith("corner dressing")),Is.False,
                "Rejected small-prop group must not remain behind the new foundation");
            var corners=architecture.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Dungeon_Foundation_")).ToArray();
            Assert.That(corners.Length,Is.EqualTo(2),"Exactly one connected sprite per corner");
            var boardRect=GameplayPixelLayoutController.ScreenRect(layout.BoardArea);
            Dictionary<int,Rect> uiRects=null;
            yield return SettledDungeonButtonRects(layout,r=>uiRects=r);
            foreach(var corner in corners)
            {
                Assert.That(corner.sprite.rect.size,Is.EqualTo(new Vector2(112,128)));
                Assert.That(corner.rectTransform.sizeDelta,Is.EqualTo(corner.sprite.rect.size),"Native art must not stretch");
                Assert.That(corner.raycastTarget,Is.False);
                Assert.That(corner.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                var rect=GameplayPixelLayoutController.ScreenRect(corner.rectTransform);
                float scale=DungeonPixelLabBattleArt.PhysicalPixelScale;
                Assert.That(rect.width,Is.EqualTo(112*scale).Within(.1f));
                Assert.That(rect.height,Is.EqualTo(128*scale).Within(.1f));
                Assert.That(rect.yMin,Is.EqualTo(-2*scale).Within(.1f),"Foot ends at physical screen edge, not safe-area edge");
                bool isLeft=corner.name.EndsWith("Left");
                Assert.That(isLeft?rect.xMin:rect.xMax,Is.EqualTo(isLeft?-11*scale:Screen.width+11*scale).Within(.1f));
                var column=architecture.Find(isLeft?"Left pillar":"Right pillar");
                var shaft=column.Find("Side_Shaft").GetComponent<Image>();
                var shaftRect=GameplayPixelLayoutController.ScreenRect(shaft.rectTransform);
                Assert.That(shaftRect.yMin,Is.EqualTo(rect.yMax).Within(.1f),"No stacked foot or gap at column/foundation joint");
                Assert.That(column.Find("Side_Base"),Is.Null,"The previous foot must not be stacked on this asset");
                // Check the actual alpha silhouette, not its mostly transparent
                // bounding box: broad feet may extend below/behind the HUD,
                // while the upper column must stay outside the playable board.
                var png=new Texture2D(2,2);png.LoadImage(File.ReadAllBytes(UnityEditor.AssetDatabase.GetAssetPath(corner.sprite)));
                for(int y=0;y<png.height;y++)for(int x=0;x<png.width;x++)
                {
                    var color=png.GetPixel(x,y);
                    Assert.That(color.a==0||color.a==1,Is.True,"Binary native alpha");
                    if(color.a==0)continue;
                    var screen=new Vector2(rect.xMin+(x+.5f)*scale,rect.yMin+(y+.5f)*scale);
                    Assert.That(boardRect.Contains(screen),Is.False,"Foundation silhouette must stay outside the playable board: "+screen);
                }
                Object.Destroy(png);
            }
            foreach(var button in layout.BottomHud.GetComponentsInChildren<Button>())
            {
                var canvas=button.GetComponentInParent<Canvas>();
                Assert.That(canvas.rootCanvas.renderMode==RenderMode.ScreenSpaceOverlay ||
                    canvas.sortingOrder>world.GetComponent<Canvas>().sortingOrder,Is.True,"Live UI renders above large environment foundations");
            }
            string geometry=JsonUtility.ToJson(new DungeonHybridIntegrationTests.Geometry{
                top=GameplayPixelLayoutController.ScreenRect(layout.TopHud),board=GameplayPixelLayoutController.ScreenRect(layout.BoardArea),
                bottom=GameplayPixelLayoutController.ScreenRect(layout.BottomHud),ratio=layout.Current.BoardTexelRatio,
                health=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).Select(s=>GameplayPixelLayoutController.ScreenRect(s.CombatBarRect)).ToArray()});
            string name=size.x+"x"+size.y+(size.z==1?"-safe":"");
            File.WriteAllText(root+"/"+name+".txt",report+"\n"+geometry);
            yield return CaptureDungeonTurnScreen(root+"/"+name+"-after.png");
            // Same paused Game frame with candidate art hidden is a diagnostic
            // reference for unchanged HUD and board geometry, not historical art.
            foreach(var corner in corners)corner.gameObject.SetActive(false);
            yield return CaptureDungeonTurnScreen(root+"/"+name+"-without-foundations.png");
            foreach(var corner in corners)corner.gameObject.SetActive(true);
            yield return null;Canvas.ForceUpdateCanvases();yield return null;
            Dictionary<int,Rect> afterRects=null;
            yield return SettledDungeonButtonRects(layout,r=>afterRects=r);
            foreach(var button in layout.GetComponentsInChildren<Button>())
                Assert.That(afterRects[button.GetInstanceID()],Is.EqualTo(uiRects[button.GetInstanceID()]),button.name+" dimensions/positions remain unchanged");
            Assert.That(GameplayPixelLayoutController.ScreenRect(layout.BoardArea),Is.EqualTo(boardRect));

        }
        Time.timeScale=1;
    }
    static IEnumerator SettledDungeonButtonRects(GameplayPixelLayoutController layout,Action<Dictionary<int,Rect>> accept)
    {
        Dictionary<int,Rect> previous=null;int stableFrames=0;
        Action<ScriptableRenderContext,Camera[]> rendered=(context,cameras)=>
        {
            var current=layout.GetComponentsInChildren<Button>().ToDictionary(b=>b.GetInstanceID(),b=>GameplayPixelLayoutController.ScreenRect((RectTransform)b.transform));
            bool same=previous!=null && previous.Count==current.Count && current.All(p=>previous.TryGetValue(p.Key,out var value)&&value==p.Value);
            stableFrames=same?stableFrames+1:0;previous=current;
        };
        RenderPipelineManager.endFrameRendering+=rendered;
        try { yield return Until(()=>stableFrames>=3,"native controls settle after rendered layout");accept(previous); }
        finally { RenderPipelineManager.endFrameRendering-=rendered; }
    }
    static IEnumerator CaptureDungeonTurnScreen(string path)
    {
        if(File.Exists(path))File.Delete(path);
        yield return null;ScreenCapture.CaptureScreenshot(path);
        yield return Until(()=>File.Exists(path),"screenshot");yield return null;
    }
}
