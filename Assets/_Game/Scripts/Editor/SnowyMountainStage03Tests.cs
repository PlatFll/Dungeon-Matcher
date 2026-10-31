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
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    const string SnowStage03="ArtSource/SnowyMountainStaged/Stage03";
    readonly List<UnityEngine.Object> snowPortraitTemporary=new List<UnityEngine.Object>();
    Sprite SnowPreviewSprite(string path,float ppu)
    {
        var t=new Texture2D(2,2,TextureFormat.RGBA32,false);
        Assert.That(t.LoadImage(File.ReadAllBytes(path)),Is.True);
        t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;
        var s=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,ppu,0,SpriteMeshType.FullRect);
        s.name=Path.GetFileNameWithoutExtension(path);snowPortraitTemporary.Add(t);snowPortraitTemporary.Add(s);return s;
    }
    [UnityTest,Explicit] public IEnumerator SnowyMountainStage03PortraitReview()
    {
        UnityEngine.Random.InitState(291002);
        RunLaunchOptions.ForestPrototype=false;RunLaunchOptions.StartingZone="frostwake-peaks";
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");yield return Stable();
        foreach(string id in new[]{"frostfang_wolf","harpoon_hunter","iceborn_raider"})
            if(Run.Waves.ActiveEnemies.Count<3&&!Run.Waves.ActiveEnemies.Any(e=>e.Definition.EnemyId==id))
                Assert.That(Run.Waves.TrySummonEnemy(Run.Zone.Definition.enemies.Single(e=>e.EnemyId==id),out _),Is.True);
        yield return Until(()=>Run.Continuation.CanCapture&&Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"snow portrait spawn settles");
        var background=UnityEngine.Object.FindFirstObjectByType<BattleBackgroundTilemapController>();
        background.ApplyGameplayTheme(Run.Zone.Definition.theme,0);yield return null;
        var environment=background.ActiveEnvironment;Assert.That(environment,Is.Not.Null);
        var architecture=environment.transform.Find("Architecture").gameObject;
        var sourceLayer=architecture.GetComponent<TilemapRenderer>();
        var plate=new GameObject("ApprovedStage02Scenery_PreviewOnly");
        plate.transform.SetParent(environment.transform,false);
        var renderer=plate.AddComponent<SpriteRenderer>();
        renderer.sprite=SnowPreviewSprite(SnowStage02+"/Exports/AlpinePass.png",64);
        renderer.sortingLayerID=sourceLayer.sortingLayerID;renderer.sortingOrder=sourceLayer.sortingOrder;
        renderer.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;architecture.SetActive(false);
        var art=ScriptableObject.CreateInstance<DungeonPixelLabBattleArt>();
        art.modules=Directory.GetFiles(SnowStage03+"/Exports","*.png").OrderBy(p=>p).Select(p=>SnowPreviewSprite(p,100)).ToArray();
        snowPortraitTemporary.Add(art);
        var records=new List<SnowPortraitRecord>();
        var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        var player=(RectTransform)GameObject.Find("PlayerSection").transform;
        var originals=new[]{player.Find("PlayerAreaThreeSliceFrame").gameObject,player.Find("PlayerDungeonBackdrop").gameObject};
        RectTransform frame=null,mask=null,dressing=null;
        try
        {
            foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
            {
                Time.timeScale=1;
                foreach(var go in originals)go.SetActive(true);
                if(frame)frame.gameObject.SetActive(false);if(mask)mask.gameObject.SetActive(false);if(dressing)dressing.gameObject.SetActive(false);
                GameplayPixelLayoutController.ValidationSafeArea=null;
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(size.x,size.y)});
                yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"snow portrait viewport");
                GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                for(int n=0;n<12;n++)yield return null;
                Time.timeScale=0;Canvas.ForceUpdateCanvases();
                var viewportMask=background.GetComponentInChildren<SpriteMask>();var mb=viewportMask.bounds;var cam=Camera.main;
                float pixel=BattleFloorAnchor.PhysicalPixelScale;
                float height=renderer.sprite.rect.height;
                float visibleHeight=(cam.WorldToScreenPoint(mb.max).y-cam.WorldToScreenPoint(mb.min).y)/pixel;
                float top=cam.WorldToScreenPoint(mb.max).y+Mathf.Floor(Mathf.Max(0,height-visibleHeight)*.35f)*pixel;
                var center=new Vector2(Mathf.Round(cam.WorldToScreenPoint(mb.center).x),Mathf.Round(top-height*.5f*pixel));
                plate.transform.position=SnowWorld(cam,center);
                plate.transform.localScale=Vector3.one*((SnowWorld(cam,center+Vector2.up*pixel)-SnowWorld(cam,center)).magnitude*64);
                for(int n=0;n<3;n++)yield return null;
                var before=SnowGeometry(layout);
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out var report),Is.Empty,report);
                var key=size.x+"x"+size.y+(size.z==1?"-safe":"");
                Directory.CreateDirectory(SnowStage03+"/Preview/Before");Directory.CreateDirectory(SnowStage03+"/Preview/After");
                yield return ForegroundScreenshot(SnowStage03+"/Preview/Before/"+key+".png");
                foreach(var go in originals)go.SetActive(false);
                art.Panel(player,"Portrait","Backing",9,50);
                frame=(RectTransform)player.Find("PixelLabPortraitFrame");mask=(RectTransform)player.Find("PixelLabInteriorMask");
                frame.gameObject.SetActive(true);mask.gameObject.SetActive(true);
                frame.GetComponent<ResponsiveModularFrameFitter>().RefreshFrame();
                if(dressing){dressing.gameObject.SetActive(false);UnityEngine.Object.Destroy(dressing.gameObject);}
                dressing=SnowPortraitDressing(art,player);
                for(int n=0;n<3;n++)yield return null;
                Canvas.ForceUpdateCanvases();
                var after=SnowGeometry(layout);
                Assert.That(JsonUtility.ToJson(after),Is.EqualTo(JsonUtility.ToJson(before)),"Portrait art cannot alter shared layout or characters");
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out report),Is.Empty,report);
                // Negative probes ensure native recognition does not hide distorted corners or tile density.
                var probeEdge=frame.Find("TopEdge").GetComponent<Image>();
                float correctDensity=probeEdge.pixelsPerUnitMultiplier;
                probeEdge.pixelsPerUnitMultiplier=correctDensity*1.25f;
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out _).Any(e=>e.StartsWith("Native frame tile density mismatch:")),Is.True);
                probeEdge.pixelsPerUnitMultiplier=correctDensity;
                var probeCorner=(RectTransform)frame.Find("TopLeftCorner");
                Vector2 correctSize=probeCorner.sizeDelta;
                probeCorner.sizeDelta=correctSize+new Vector2(BattleFloorAnchor.Unit(player.GetComponentInParent<Canvas>()),0);
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out _).Any(e=>e.StartsWith("Native frame pixels distorted:")),Is.True);
                probeCorner.sizeDelta=correctSize;
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out _),Is.Empty);
                Assert.That(mask.GetComponent<RectMask2D>(),Is.Not.Null);
                var fill=mask.GetComponentInChildren<Image>();
                Assert.That(fill.type,Is.EqualTo(Image.Type.Tiled));
                Assert.That(fill.rectTransform.offsetMin.x,Is.LessThan(0));Assert.That(fill.rectTransform.offsetMax.x,Is.GreaterThan(0));
                var maskRect=GameplayPixelLayoutController.ScreenRect(mask);
                var frameRect=GameplayPixelLayoutController.ScreenRect(player);
                Assert.That(frameRect.Contains(maskRect.min)&&frameRect.Contains(maskRect.max),Is.True,"Backing clipped inside portrait");
                foreach(var im in frame.GetComponentsInChildren<Image>())
                {
                    Assert.That(im.raycastTarget,Is.False);Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                    var bounds=GameplayPixelLayoutController.ScreenRect(im.rectTransform);
                    if(im.type==Image.Type.Simple)
                    {
                        Assert.That(bounds.width/im.sprite.rect.width,Is.EqualTo(pixel).Within(.01),im.name+" native X");
                        Assert.That(bounds.height/im.sprite.rect.height,Is.EqualTo(pixel).Within(.01),im.name+" native Y");
                    }
                }
                var left=GameplayPixelLayoutController.ScreenRect((RectTransform)frame.Find("LeftEdge")).center.x;
                var right=GameplayPixelLayoutController.ScreenRect((RectTransform)frame.Find("RightEdge")).center.x;
                var upper=GameplayPixelLayoutController.ScreenRect((RectTransform)frame.Find("TopEdge")).center.y;
                var lower=GameplayPixelLayoutController.ScreenRect((RectTransform)frame.Find("BottomEdge")).center.y;
                foreach(Transform clamp in frame.Find("NativeConnectors"))
                {
                    if(!clamp.gameObject.activeInHierarchy)continue;
                    var c=GameplayPixelLayoutController.ScreenRect((RectTransform)clamp).center;
                    Assert.That(Mathf.Min(Mathf.Min(Mathf.Abs(c.x-left),Mathf.Abs(c.x-right)),Mathf.Min(Mathf.Abs(c.y-upper),Mathf.Abs(c.y-lower))),Is.LessThan(.01),"Clamp centered on a rail");
                }
                records.Add(new SnowPortraitRecord{size=key,geometry=after,mask=maskRect,frame=frameRect,nativePixelScale=pixel,report=report});
                File.WriteAllText(SnowStage03+"/Preview/Measurements.json",JsonUtility.ToJson(new SnowPortraitRecords{items=records.ToArray()},true));
                yield return ForegroundScreenshot(SnowStage03+"/Preview/After/"+key+".png");
            }
        }
        finally
        {
            foreach(var go in originals)go.SetActive(true);
            foreach(var r in new[]{frame,mask,dressing})if(r)UnityEngine.Object.Destroy(r.gameObject);
            architecture.SetActive(true);UnityEngine.Object.Destroy(plate);
            foreach(var o in snowPortraitTemporary)if(o)UnityEngine.Object.Destroy(o);snowPortraitTemporary.Clear();Time.timeScale=1;
        }
    }
    [UnityTest,Explicit] public IEnumerator SnowyMountainNativeFrameRegression()
    {
        // Check the shared fitter and validator against all production zone kits.
        // No art or production objects are replaced and no historical captures are overwritten.
        foreach(string zone in new[]{"dungeon","magical-forest","drowned-court","ironvein-excavation","frostwake-peaks"})
        {
            RunLaunchOptions.ForestPrototype=false;RunLaunchOptions.StartingZone=zone;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");yield return Stable();
            foreach(var size in new[]{new Vector2Int(360,640),new Vector2Int(720,1280),new Vector2Int(1080,1920)})
            {
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
                yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"native frame regression viewport");
                for(int n=0;n<12;n++)yield return null;
                var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);
                Assert.That(errors,Is.Empty,zone+" "+size+"\n"+report);
            }
        }
    }
    RectTransform SnowPortraitDressing(DungeonPixelLabBattleArt art,RectTransform player)
    {
        float unit=BattleFloorAnchor.Unit(player.GetComponentInParent<Canvas>());
        float w=Mathf.Round(player.rect.width/unit),h=Mathf.Round(player.rect.height/unit);
        var root=DungeonPixelLabBattleArt.Rect("SnowPortraitDressing",player,0,0,player.rect.width,player.rect.height);
        art.Image("SnowCap_Left",root,0,-4*unit,unit);
        art.Image("SnowCap_Right",root,(w-32)*unit,-4*unit,unit);
        var middle=art.Image("SnowCap_Middle",root,32*unit,-4*unit,unit);
        middle.type=Image.Type.Tiled;middle.pixelsPerUnitMultiplier=1/unit;middle.rectTransform.sizeDelta=new Vector2(Mathf.Max(0,w-64),16)*unit;
        art.Image("SnowFoot_Left",root,9*unit,(h-18)*unit,unit);
        art.Image("SnowFoot_Right",root,(w-33)*unit,(h-18)*unit,unit);
        root.SetAsLastSibling();return root;
    }
    [Serializable]class SnowPortraitRecord{public string size,report;public SnowGeometryRecord geometry;public Rect frame,mask;public float nativePixelScale;}
    [Serializable]class SnowPortraitRecords{public SnowPortraitRecord[] items;}
}
