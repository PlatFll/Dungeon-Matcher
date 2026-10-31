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
    const string SnowStage05="ArtSource/SnowyMountainStaged/Stage05";

    // Disposable production-Game review. This does not register or replace a live zone kit.
    [UnityTest,Explicit] public IEnumerator SnowyMountainStage05BoardFrameReview()
    {
        UnityEngine.Random.InitState(291002);
        RunLaunchOptions.ForestPrototype=false;RunLaunchOptions.StartingZone="frostwake-peaks";
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");yield return Stable();
        foreach(string id in new[]{"frostfang_wolf","harpoon_hunter","iceborn_raider"})
            if(Run.Waves.ActiveEnemies.Count<3&&!Run.Waves.ActiveEnemies.Any(e=>e.Definition.EnemyId==id))
                Assert.That(Run.Waves.TrySummonEnemy(Run.Zone.Definition.enemies.Single(e=>e.EnemyId==id),out _),Is.True);
        yield return Until(()=>Run.Continuation.CanCapture&&Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"snow board spawn settles");
        var background=UnityEngine.Object.FindFirstObjectByType<BattleBackgroundTilemapController>();
        background.ApplyGameplayTheme(Run.Zone.Definition.theme,0);yield return null;
        var env=background.ActiveEnvironment;Assert.That(env,Is.Not.Null);
        var architecture=env.transform.Find("Architecture").gameObject;
        var oldFloor=env.transform.Find("Floor").gameObject;
        var sourceLayer=architecture.GetComponent<TilemapRenderer>();
        var plate=new GameObject("ApprovedStage02Scenery_PreviewOnly");plate.transform.SetParent(env.transform,false);
        var vista=plate.AddComponent<SpriteRenderer>();vista.sprite=SnowPreviewSprite(SnowStage02+"/Exports/AlpinePass.png",64);
        vista.sortingLayerID=sourceLayer.sortingLayerID;vista.sortingOrder=sourceLayer.sortingOrder;
        architecture.SetActive(false);oldFloor.SetActive(false);
        var art=ScriptableObject.CreateInstance<DungeonPixelLabBattleArt>();snowPortraitTemporary.Add(art);
        art.modules=Directory.GetFiles(SnowStage03+"/Exports","*.png").Concat(Directory.GetFiles(SnowStage05+"/Exports","Board_*.png"))
            .OrderBy(p=>p).Select(p=>SnowPreviewSprite(p,100)).ToArray();
        var ledge=Directory.GetFiles(SnowRevision+"/Exports","*.png").ToDictionary(Path.GetFileNameWithoutExtension,p=>SnowPreviewSprite(p,64));
        var mutedWall=SnowPreviewSprite(SnowStage05+"/Exports/CobbleWall_Muted.png",64);
        var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        var bottomBacking=layout.BottomHud.Find("BottomDungeonBackdrop").gameObject;
        var arena=layout.TopHud.Find("GeneratedTopBattleLayout/BattleArenaFrame").gameObject;
        var bottomEnclosure=layout.BottomHud.Find("GeneratedBottomHudFrame").gameObject;
        var oldSeparator=new[]{arena.transform.Find("BottomEdge").gameObject,arena.transform.Find("BottomLeftCorner").gameObject,arena.transform.Find("BottomRightCorner").gameObject};
        var player=(RectTransform)GameObject.Find("PlayerSection").transform;
        var oldPortrait=new[]{player.Find("PlayerAreaThreeSliceFrame").gameObject,player.Find("PlayerDungeonBackdrop").gameObject};
        var boardVisuals=Run.Board.GetComponent<BoardVisuals>();
        var oldBoardFrame=Run.Board.transform.Find("BoardFrame").gameObject;
        var originals=oldPortrait.Concat(oldSeparator).Concat(new[]{bottomBacking,arena,bottomEnclosure,oldBoardFrame,architecture,oldFloor}).Distinct().ToArray();
        var activeStates=originals.Select(g=>g.activeSelf).ToArray();bool visualState=boardVisuals.enabled;
        foreach(var g in oldPortrait.Concat(oldSeparator))g.SetActive(false);
        bottomBacking.SetActive(false);
        RectTransform portraitFrame=null,portraitMask=null,portraitDressing=null,boardTarget=null;
        GameObject floorRoot=null;bool asyncShaders=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
        var records=new List<SnowBoardRecord>();
        try
        {
            foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
            {
                Time.timeScale=1;
                foreach(var rt in new[]{portraitFrame,portraitMask,portraitDressing,boardTarget})if(rt)rt.gameObject.SetActive(false);
                if(floorRoot){floorRoot.SetActive(false);UnityEngine.Object.Destroy(floorRoot);}
                arena.SetActive(true);bottomEnclosure.SetActive(true);oldBoardFrame.SetActive(true);boardVisuals.enabled=visualState;
                vista.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
                GameplayPixelLayoutController.ValidationSafeArea=null;
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(size.x,size.y)});
                yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"snow board viewport");
                GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                for(int i=0;i<12;i++)yield return null;
                Time.timeScale=0;Canvas.ForceUpdateCanvases();
                var cam=Camera.main;var mb=background.GetComponentInChildren<SpriteMask>().bounds;
                float pixel=BattleFloorAnchor.PhysicalPixelScale;
                float h=vista.sprite.rect.height;
                float visibleH=(cam.WorldToScreenPoint(mb.max).y-cam.WorldToScreenPoint(mb.min).y)/pixel;
                float top=cam.WorldToScreenPoint(mb.max).y+Mathf.Floor(Mathf.Max(0,h-visibleH)*.35f)*pixel;
                var center=new Vector2(Mathf.Round(cam.WorldToScreenPoint(mb.center).x),Mathf.Round(top-h*.5f*pixel));
                plate.transform.position=SnowWorld(cam,center);
                plate.transform.localScale=Vector3.one*((SnowWorld(cam,center+Vector2.up*pixel)-SnowWorld(cam,center)).magnitude*64);
                art.Panel(player,"Portrait","Backing",9,50);
                portraitFrame=(RectTransform)player.Find("PixelLabPortraitFrame");portraitMask=(RectTransform)player.Find("PixelLabInteriorMask");
                portraitFrame.gameObject.SetActive(true);portraitMask.gameObject.SetActive(true);
                portraitFrame.GetComponent<ResponsiveModularFrameFitter>().RefreshFrame();
                if(portraitDressing)UnityEngine.Object.Destroy(portraitDressing.gameObject);
                portraitDressing=SnowPortraitDressing(art,player);
                var before=SnowGeometry(layout);float feet=before.feet[0],sourceTop=feet+34*pixel;
                floorRoot=SnowBoardEnvironment(ledge,ledge["CobbleWall"],cam,sourceTop,pixel);
                for(int i=0;i<3;i++)yield return null;
                string key=size.x+"x"+size.y+(size.z==1?"-safe":"");
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out var report),Is.Empty,report);
                var lockedRenderers=Run.Board.GetComponentsInChildren<SpriteRenderer>().Where(r=>!r.transform.IsChildOf(oldBoardFrame.transform)).ToArray();
                var lockedState=SnowBoardSpriteState(lockedRenderers);
                yield return ForegroundScreenshot(SnowStage05+"/Preview/Before/"+key+".png");

                floorRoot.SetActive(false);UnityEngine.Object.Destroy(floorRoot);
                floorRoot=SnowBoardEnvironment(ledge,mutedWall,cam,sourceTop,pixel);
                arena.SetActive(false);bottomEnclosure.SetActive(false);
                // Reveal the existing overscan around the removed enclosure, without moving the scenery.
                vista.maskInteraction=SpriteMaskInteraction.None;
                boardVisuals.enabled=false;oldBoardFrame.SetActive(false);
                if(boardTarget)UnityEngine.Object.Destroy(boardTarget.gameObject);
                boardTarget=DungeonPixelLabBattleArt.Rect("SnowStage05BoardFrame_PreviewOnly",layout.BoardArea,0,0,0,0);
                float unit=DungeonPixelLabBattleArt.Unit(layout.GetComponentInParent<Canvas>());
                float opening=Run.Board.LocalBoardWidth*64*layout.Current.BoardTexelRatio/layout.Current.Scale;
                float outside=Mathf.Ceil(opening/unit)*unit+26*unit;
                DungeonPixelLabBattleArt.TopRect(boardTarget,(layout.BoardArea.rect.width-outside)/2,(layout.BoardArea.rect.height-outside)/2,outside,outside);
                GameplayPixelGrid.Snap(boardTarget);
                var frame=art.Frame(boardTarget,"Board",100,unit);
                SnowBoardDressing(art,boardTarget,unit);
                for(int i=0;i<3;i++)yield return null;Canvas.ForceUpdateCanvases();
                var after=SnowGeometry(layout);
                Assert.That(JsonUtility.ToJson(after),Is.EqualTo(JsonUtility.ToJson(before)),"No changes to shared layout, feet, bars or controls");
                Assert.That(SnowBoardSpriteState(lockedRenderers),Is.EqualTo(lockedState),"All actual gems/cells retain sprite, position, scale and color");
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out report),Is.Empty,report);
                Assert.That(arena.activeInHierarchy||bottomEnclosure.activeInHierarchy,Is.False,"Both outer HUD enclosures removed");
                Assert.That(portraitMask.GetComponent<RectMask2D>(),Is.Not.Null);
                Assert.That(after.feet.All(f=>Mathf.Abs(f-feet)<.01f),Is.True);
                foreach(var image in boardTarget.GetComponentsInChildren<Image>())
                {
                    Assert.That(image.raycastTarget,Is.False);Assert.That(image.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                    var r=GameplayPixelLayoutController.ScreenRect(image.rectTransform);
                    if(image.type==Image.Type.Simple){Assert.That(r.width/image.sprite.rect.width,Is.EqualTo(pixel).Within(.01),image.name+" X");Assert.That(r.height/image.sprite.rect.height,Is.EqualTo(pixel).Within(.01),image.name+" Y");}
                }
                foreach(var sr in floorRoot.GetComponentsInChildren<SpriteRenderer>())
                {
                    var lo=cam.WorldToScreenPoint(sr.bounds.min);var hi=cam.WorldToScreenPoint(sr.bounds.max);
                    Assert.That((hi.x-lo.x)/sr.sprite.rect.width,Is.EqualTo(pixel).Within(.01));
                    Assert.That((hi.y-lo.y)/sr.sprite.rect.height,Is.EqualTo(pixel).Within(.01));
                }
                var fr=GameplayPixelLayoutController.ScreenRect(boardTarget);
                Assert.That(fr.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(fr.xMax,Is.LessThanOrEqualTo(Screen.width));
                records.Add(new SnowBoardRecord{size=key,geometry=after,boardFrame=fr,nativePixelScale=pixel,wallTop=sourceTop-84*pixel,report=report});
                File.WriteAllText(SnowStage05+"/Preview/Measurements.json",JsonUtility.ToJson(new SnowBoardRecords{items=records.ToArray()},true));
                yield return ForegroundScreenshot(SnowStage05+"/Preview/After/"+key+".png");
            }
        }
        finally
        {
            ShaderUtil.allowAsyncCompilation=asyncShaders;boardVisuals.enabled=visualState;
            for(int i=0;i<originals.Length;i++)if(originals[i])originals[i].SetActive(activeStates[i]);
            architecture.SetActive(true);oldFloor.SetActive(true);
            if(floorRoot)UnityEngine.Object.Destroy(floorRoot);UnityEngine.Object.Destroy(plate);
            foreach(var r in new[]{portraitFrame,portraitMask,portraitDressing,boardTarget})if(r)UnityEngine.Object.Destroy(r.gameObject);
            foreach(var o in snowPortraitTemporary)if(o)UnityEngine.Object.Destroy(o);snowPortraitTemporary.Clear();Time.timeScale=1;
            GameplayPixelLayoutController.ValidationSafeArea=null;
        }
    }
    GameObject SnowBoardEnvironment(Dictionary<string,Sprite> ledge,Sprite wall,Camera cam,float top,float pixel)
    {
        var root=new GameObject("SnowStage05Environment_PreviewOnly");
        for(float y=top-84*pixel;y>0;y-=wall.rect.height*pixel)
            for(float x=0;x<Screen.width;x+=wall.rect.width*pixel)SnowRevisionTile(root.transform,wall,cam,x,y,pixel,-93);
        for(int i=0;i<4;i++)SnowRevisionTile(root.transform,ledge["Ledge_"+i],cam,i*64*pixel,top,pixel,-92);
        float right=Screen.width-64*pixel;
        for(float x=256*pixel;x<right;x+=64*pixel)SnowRevisionTile(root.transform,ledge["Ledge_3"],cam,x,top,pixel,-92);
        SnowRevisionTile(root.transform,ledge["Ledge_4"],cam,right,top,pixel,-91);return root;
    }
    void SnowBoardDressing(DungeonPixelLabBattleArt art,RectTransform target,float unit)
    {
        float w=Mathf.Round(target.rect.width/unit),h=Mathf.Round(target.rect.height/unit);
        var snow=art.Image("Board_SnowRail",target,18*unit,-2*unit,unit);
        snow.type=Image.Type.Tiled;snow.pixelsPerUnitMultiplier=1/unit;snow.rectTransform.sizeDelta=new Vector2(w-36,8)*unit;
        art.Image("Board_SnowCorner",target,0,-4*unit,unit);
        art.Image("Board_SnowCorner",target,(w-18)*unit,-4*unit,unit);
        // Tiny ice accents on the outer bottom corner ledges, entirely outside the opening.
        art.Image("Board_Icicles",target,unit,(h-1)*unit,unit);
        art.Image("Board_Icicles",target,(w-17)*unit,(h-1)*unit,unit);
    }
    string SnowBoardSpriteState(SpriteRenderer[] renderers)=>string.Join("\n",renderers.Select(r=>r.GetInstanceID()+" "+(r.sprite?r.sprite.GetInstanceID():0)+" "+r.transform.position.ToString("R")+" "+r.transform.lossyScale.ToString("R")+" "+r.color.ToString()));
    [Serializable]class SnowBoardRecord{public string size,report;public SnowGeometryRecord geometry;public Rect boardFrame;public float nativePixelScale,wallTop;}
    [Serializable]class SnowBoardRecords{public SnowBoardRecord[] items;}
}
