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
    const string SnowRevision="ArtSource/SnowyMountainStaged/Stage04/Revision01";
    [UnityTest,Explicit] public IEnumerator SnowyMountainStage04OriginalStoneReview()
    {
        UnityEngine.Random.InitState(291002);
        RunLaunchOptions.ForestPrototype=false;RunLaunchOptions.StartingZone="frostwake-peaks";
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");yield return Stable();
        foreach(string id in new[]{"frostfang_wolf","harpoon_hunter","iceborn_raider"})
            if(Run.Waves.ActiveEnemies.Count<3&&!Run.Waves.ActiveEnemies.Any(e=>e.Definition.EnemyId==id))
                Assert.That(Run.Waves.TrySummonEnemy(Run.Zone.Definition.enemies.Single(e=>e.EnemyId==id),out _),Is.True);
        yield return Until(()=>Run.Continuation.CanCapture&&Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"snow floor spawn settles");
        var background=UnityEngine.Object.FindFirstObjectByType<BattleBackgroundTilemapController>();
        background.ApplyGameplayTheme(Run.Zone.Definition.theme,0);yield return null;
        var env=background.ActiveEnvironment;Assert.That(env,Is.Not.Null);
        var architecture=env.transform.Find("Architecture").gameObject;
        var oldFloor=new[]{env.transform.Find("Floor").gameObject};
        var sourceLayer=architecture.GetComponent<TilemapRenderer>();
        var plate=new GameObject("ApprovedStage02Scenery_PreviewOnly");plate.transform.SetParent(env.transform,false);
        var vista=plate.AddComponent<SpriteRenderer>();vista.sprite=SnowPreviewSprite(SnowStage02+"/Exports/AlpinePass.png",64);
        vista.sortingLayerID=sourceLayer.sortingLayerID;vista.sortingOrder=sourceLayer.sortingOrder;
        vista.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;architecture.SetActive(false);
        var art=ScriptableObject.CreateInstance<DungeonPixelLabBattleArt>();snowPortraitTemporary.Add(art);
        art.modules=Directory.GetFiles(SnowStage03+"/Exports","*.png").OrderBy(p=>p).Select(p=>SnowPreviewSprite(p,100)).ToArray();
        var floorSprites=Directory.GetFiles(SnowStage04+"/Exports","*.png").ToDictionary(Path.GetFileNameWithoutExtension,p=>SnowPreviewSprite(p,64));
        var revisedSprites=Directory.GetFiles(SnowRevision+"/Exports","*.png").ToDictionary(Path.GetFileNameWithoutExtension,p=>SnowPreviewSprite(p,64));
        Directory.CreateDirectory(SnowRevision+"/Preview/Before");Directory.CreateDirectory(SnowRevision+"/Preview/After");
        var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        var bottomBacking=layout.BottomHud.Find("BottomDungeonBackdrop").gameObject;
        bool oldBackingState=bottomBacking.activeSelf;
        var player=(RectTransform)GameObject.Find("PlayerSection").transform;
        var oldPortrait=new[]{player.Find("PlayerAreaThreeSliceFrame").gameObject,player.Find("PlayerDungeonBackdrop").gameObject};
        foreach(var go in oldPortrait)go.SetActive(false);
        var arena=layout.TopHud.Find("GeneratedTopBattleLayout/BattleArenaFrame");
        var oldSeparator=new[]{arena.Find("BottomEdge").gameObject,arena.Find("BottomLeftCorner").gameObject,arena.Find("BottomRightCorner").gameObject};
        RectTransform frame=null,mask=null,dressing=null;GameObject floorRoot=null;
        var records=new List<SnowFloorRecord>();
        bool asynchronousShaders=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
        try
        {
            foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
            {
                Time.timeScale=1;bottomBacking.SetActive(oldBackingState);
                if(frame)frame.gameObject.SetActive(false);if(mask)mask.gameObject.SetActive(false);if(dressing)dressing.gameObject.SetActive(false);
                if(floorRoot){floorRoot.SetActive(false);UnityEngine.Object.Destroy(floorRoot);}
                foreach(var go in oldFloor.Concat(oldSeparator))go.SetActive(true);
                GameplayPixelLayoutController.ValidationSafeArea=null;
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(size.x,size.y)});
                yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"snow floor viewport");
                GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                for(int n=0;n<12;n++)yield return null;
                Time.timeScale=0;Canvas.ForceUpdateCanvases();
                var cam=Camera.main;var viewportMask=background.GetComponentInChildren<SpriteMask>();var mb=viewportMask.bounds;
                float pixel=BattleFloorAnchor.PhysicalPixelScale;
                float height=vista.sprite.rect.height;
                float visibleHeight=(cam.WorldToScreenPoint(mb.max).y-cam.WorldToScreenPoint(mb.min).y)/pixel;
                float top=cam.WorldToScreenPoint(mb.max).y+Mathf.Floor(Mathf.Max(0,height-visibleHeight)*.35f)*pixel;
                var center=new Vector2(Mathf.Round(cam.WorldToScreenPoint(mb.center).x),Mathf.Round(top-height*.5f*pixel));
                plate.transform.position=SnowWorld(cam,center);
                plate.transform.localScale=Vector3.one*((SnowWorld(cam,center+Vector2.up*pixel)-SnowWorld(cam,center)).magnitude*64);
                art.Panel(player,"Portrait","Backing",9,50);
                frame=(RectTransform)player.Find("PixelLabPortraitFrame");mask=(RectTransform)player.Find("PixelLabInteriorMask");
                frame.gameObject.SetActive(true);mask.gameObject.SetActive(true);
                frame.GetComponent<ResponsiveModularFrameFitter>().RefreshFrame();
                if(dressing){dressing.gameObject.SetActive(false);UnityEngine.Object.Destroy(dressing.gameObject);}
                dressing=SnowPortraitDressing(art,player);
                for(int n=0;n<3;n++)yield return null;
                var before=SnowGeometry(layout);string key=size.x+"x"+size.y+(size.z==1?"-safe":"");
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out var report),Is.Empty,report);

                foreach(var go in oldFloor.Concat(oldSeparator))go.SetActive(false);
                floorRoot=new GameObject("Stage04SnowFloor_PreviewOnly");
                float left=Mathf.Floor(cam.WorldToScreenPoint(mb.min).x),right=Mathf.Ceil(cam.WorldToScreenPoint(mb.max).x);
                float feet=before.feet[0],floorTop=feet+20*pixel,separatorTop=feet-12*pixel;
                float separatorBottom=separatorTop-24*pixel,bottom=before.top.yMin;
                Assert.That(separatorBottom,Is.GreaterThanOrEqualTo(bottom),"Complete block face fits above the board");
                // Give the ledge its own mask down to the top-HUD boundary. The vista
                // retains its approved inset; only repeated floor masonry may overscan.
                var maskObject=new GameObject("SnowLedgeViewport");maskObject.transform.SetParent(floorRoot.transform,false);
                var ledgeMask=maskObject.AddComponent<SpriteMask>();ledgeMask.sprite=viewportMask.sprite;
                ledgeMask.alphaCutoff=.01f;ledgeMask.isCustomRangeActive=true;
                ledgeMask.backSortingLayerID=ledgeMask.frontSortingLayerID=SortingLayer.NameToID("Default");
                ledgeMask.backSortingOrder=-95;ledgeMask.frontSortingOrder=-91;
                var low=SnowWorld(cam,new Vector2(left,bottom));var high=SnowWorld(cam,new Vector2(right,floorTop));
                maskObject.transform.position=(low+high)*.5f;
                maskObject.transform.localScale=new Vector3((high.x-low.x)/ledgeMask.sprite.bounds.size.x,(high.y-low.y)/ledgeMask.sprite.bounds.size.y,1);
                int count=Mathf.CeilToInt((right-left)/(64*pixel));
                for(int i=0;i<count;i++)
                {
                    string suffix="ABC"[i%3].ToString();float x=left+i*64*pixel;
                    SnowFloorTile(floorRoot.transform,floorSprites["SnowFloor_"+suffix],cam,x,floorTop,pixel,-93);
                    SnowFloorTile(floorRoot.transform,floorSprites["Separator_"+suffix],cam,x,separatorTop,pixel,-93);
                    for(float y=separatorBottom;y>bottom;y-=32*pixel)
                        SnowFloorTile(floorRoot.transform,floorSprites["Foundation"],cam,x,y,pixel,-94);
                }
                // Discrete overhangs stay inside the viewport; repeating masonry alone may overscan.
                SnowFloorTile(floorRoot.transform,floorSprites["SnowOverhang_A"],cam,left+2*pixel,separatorTop+5*pixel,pixel,-92);
                SnowFloorTile(floorRoot.transform,floorSprites["SnowOverhang_B"],cam,right-(floorSprites["SnowOverhang_B"].rect.width+2)*pixel,separatorTop+5*pixel,pixel,-92);
                for(int n=0;n<3;n++)yield return null;Canvas.ForceUpdateCanvases();
                yield return ForegroundScreenshot(SnowRevision+"/Preview/Before/"+key+".png");
                floorRoot.SetActive(false);UnityEngine.Object.Destroy(floorRoot);
                floorRoot=new GameObject("OriginalPixelLabSnowAndCobble_ReviewOnly");
                bottomBacking.SetActive(false);
                // The original upper 84 rows stay native. Source row 34 meets the
                // unchanged feet line; its stone lip begins ten pixels below it.
                float sourceTop=feet+34*pixel;
                floorTop=feet+14*pixel;separatorTop=feet-10*pixel;
                separatorBottom=sourceTop-84*pixel;bottom=0;left=0;right=Screen.width;
                var wall=revisedSprites["CobbleWall"];
                for(float y=separatorBottom;y>0;y-=wall.rect.height*pixel)
                    for(float x=0;x<right;x+=wall.rect.width*pixel)
                        SnowRevisionTile(floorRoot.transform,wall,cam,x,y,pixel,-93);
                // Retain the full left and right end pieces. Interior repeats may
                // overlap under the right cap; the icicles are never cropped.
                for(int i=0;i<4;i++)
                    SnowRevisionTile(floorRoot.transform,revisedSprites["Ledge_"+i],cam,i*64*pixel,sourceTop,pixel,-92);
                float rightCap=right-64*pixel;
                for(float x=256*pixel;x<rightCap;x+=64*pixel)
                    SnowRevisionTile(floorRoot.transform,revisedSprites["Ledge_3"],cam,x,sourceTop,pixel,-92);
                SnowRevisionTile(floorRoot.transform,revisedSprites["Ledge_4"],cam,rightCap,sourceTop,pixel,-91);
                for(int n=0;n<3;n++)yield return null;Canvas.ForceUpdateCanvases();
                var after=SnowGeometry(layout);
                var renderState=floorRoot.GetComponentsInChildren<SpriteRenderer>().Select(sr=>sr.name+" visible="+sr.isVisible+" layer="+sr.sortingLayerName+" order="+sr.sortingOrder+" material="+sr.sharedMaterial.name+" bounds="+sr.bounds).ToArray();
                File.WriteAllLines(SnowRevision+"/QA/Render-"+key+".txt",renderState);
                Assert.That(JsonUtility.ToJson(after),Is.EqualTo(JsonUtility.ToJson(before)),"Floor art cannot move actors, bars, controls or shared layout");
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out report),Is.Empty,report);
                Assert.That(after.feet.All(f=>Mathf.Abs(f-feet)<.01f),Is.True,"Common enemy standing line");
                Assert.That((floorTop-feet)/pixel,Is.EqualTo(14).Within(.01));
                Assert.That((feet-separatorTop)/pixel,Is.EqualTo(10).Within(.01),"Visible floor in front of feet");
                foreach(var sr in floorRoot.GetComponentsInChildren<SpriteRenderer>())
                {
                    var lo=cam.WorldToScreenPoint(sr.bounds.min);var hi=cam.WorldToScreenPoint(sr.bounds.max);
                    Assert.That((hi.x-lo.x)/sr.sprite.rect.width,Is.EqualTo(pixel).Within(.01),sr.name+" X source texels");
                    Assert.That((hi.y-lo.y)/sr.sprite.rect.height,Is.EqualTo(pixel).Within(.01),sr.name+" Y source texels");
                    Assert.That(sr.sortingOrder,Is.LessThan(-69),"Environment stays below board and UI");
                }
                records.Add(new SnowFloorRecord{size=key,geometry=after,nativePixelScale=pixel,floorTop=floorTop,separatorTop=separatorTop,separatorBottom=separatorBottom,viewport=Rect.MinMaxRect(left,bottom,right,cam.WorldToScreenPoint(mb.max).y),report=report});
                File.WriteAllText(SnowRevision+"/Preview/Measurements.json",JsonUtility.ToJson(new SnowFloorRecords{items=records.ToArray()},true));
                yield return ForegroundScreenshot(SnowRevision+"/Preview/After/"+key+".png");
                var capture=new Texture2D(2,2,TextureFormat.RGBA32,false);
                capture.LoadImage(File.ReadAllBytes(SnowRevision+"/Preview/After/"+key+".png"));
                var visiblePixels=capture.GetPixels32();int snowCount=0,stoneCount=0;
                for(int y=Mathf.CeilToInt(bottom);y<Mathf.FloorToInt(floorTop);y++)
                    for(int x=Mathf.CeilToInt(left);x<Mathf.FloorToInt(right);x++)
                    {
                        var c=visiblePixels[y*capture.width+x];
                        if(c.r==236&&c.g==250&&c.b==253)snowCount++;
                        if(c.r==62&&c.g==98&&c.b==120)stoneCount++;
                    }
                UnityEngine.Object.Destroy(capture);
                Assert.That(snowCount,Is.GreaterThan(50),"Native snow pixels must be visible in the actual capture");
                Assert.That(stoneCount,Is.GreaterThan(50),"Native separator pixels must be visible in the actual capture");
            }
        }
        finally
        {
            ShaderUtil.allowAsyncCompilation=asynchronousShaders;bottomBacking.SetActive(oldBackingState);
            foreach(var go in oldFloor.Concat(oldSeparator).Concat(oldPortrait))go.SetActive(true);architecture.SetActive(true);
            if(floorRoot)UnityEngine.Object.Destroy(floorRoot);UnityEngine.Object.Destroy(plate);
            foreach(var r in new[]{frame,mask,dressing})if(r)UnityEngine.Object.Destroy(r.gameObject);
            foreach(var o in snowPortraitTemporary)if(o)UnityEngine.Object.Destroy(o);snowPortraitTemporary.Clear();Time.timeScale=1;
        }
    }
    void SnowRevisionTile(Transform parent,Sprite sprite,Camera cam,float x,float top,float pixel,int order)
    {
        SnowFloorTile(parent,sprite,cam,x,top,pixel,order);
        // Below the top HUD this repeated stone wall must cover the full screen,
        // including safe-area gutters. UI and board render above this world layer.
        parent.GetChild(parent.childCount-1).GetComponent<SpriteRenderer>().maskInteraction=SpriteMaskInteraction.None;
    }
}
