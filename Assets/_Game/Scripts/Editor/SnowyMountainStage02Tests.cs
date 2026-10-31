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

/// <summary>Disposable live-Game review. No production theme, prefab or scene is saved.</summary>
public sealed partial class ForestFoundationPlayTests
{
    const string SnowStage02="ArtSource/SnowyMountainStaged/Stage02";

    [UnityTest,Explicit] public IEnumerator SnowyMountainStage02BackdropReview()
    {
        UnityEngine.Random.InitState(291002);
        RunLaunchOptions.ForestPrototype=false;
        RunLaunchOptions.StartingZone="frostwake-peaks";
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        yield return Stable();
        foreach(string id in new[]{"frostfang_wolf","harpoon_hunter","iceborn_raider"})
            if(Run.Waves.ActiveEnemies.Count<3 && !Run.Waves.ActiveEnemies.Any(e=>e.Definition.EnemyId==id))
                Assert.That(Run.Waves.TrySummonEnemy(Run.Zone.Definition.enemies.Single(e=>e.EnemyId==id),out _),Is.True);
        yield return Until(()=>Run.Continuation.CanCapture&&Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"snow production spawn settles");
        var background=UnityEngine.Object.FindFirstObjectByType<BattleBackgroundTilemapController>();
        background.ApplyGameplayTheme(Run.Zone.Definition.theme,0);
        yield return null;
        var environment=background.ActiveEnvironment;
        Assert.That(environment,Is.Not.Null);
        var architecture=environment.transform.Find("Architecture").gameObject;
        var layer=architecture.GetComponent<TilemapRenderer>();
        var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
        Assert.That(texture.LoadImage(File.ReadAllBytes(SnowStage02+"/Exports/AlpinePass.png")),Is.True);
        texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
        var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,64,0,SpriteMeshType.FullRect);
        var preview=new GameObject("Stage02AlpinePass_PreviewOnly");
        preview.transform.SetParent(environment.transform,false);
        var renderer=preview.AddComponent<SpriteRenderer>();
        renderer.sprite=sprite;renderer.sortingLayerID=layer.sortingLayerID;renderer.sortingOrder=layer.sortingOrder;
        renderer.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;preview.SetActive(false);
        var records=new List<SnowStageRecord>();
        try
        {
            foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
            {
                Time.timeScale=1;preview.SetActive(false);architecture.SetActive(true);
                GameplayPixelLayoutController.ValidationSafeArea=null;
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
                    .Invoke(null,new object[]{new Vector2Int(size.x,size.y)});
                yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"snow viewport");
                GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                for(int n=0;n<12;n++)yield return null;
                Time.timeScale=0;Canvas.ForceUpdateCanvases();
                var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out var report),Is.Empty,report);
                var before=SnowGeometry(layout);
                var key=size.x+"x"+size.y+(size.z==1?"-safe":"");
                Directory.CreateDirectory(SnowStage02+"/Preview/Before");Directory.CreateDirectory(SnowStage02+"/Preview/After");
                yield return ForegroundScreenshot(SnowStage02+"/Preview/Before/"+key+".png");
                var mask=background.GetComponentInChildren<SpriteMask>();
                Assert.That(mask,Is.Not.Null);
                float pixel=BattleFloorAnchor.PhysicalPixelScale;
                var cam=Camera.main;
                var origin=cam.WorldToScreenPoint(background.transform.position);
                var maskBounds=mask.bounds;
                float cx=cam.WorldToScreenPoint(maskBounds.center).x;
                // Place unscaled native texels at a whole screen-pixel pitch. The existing
                // viewport masks overscan; neither the UI nor the floor is repositioned.
                float visibleHeight=(cam.WorldToScreenPoint(maskBounds.max).y-cam.WorldToScreenPoint(maskBounds.min).y)/pixel;
                float upperOverscan=Mathf.Floor(Mathf.Max(0,texture.height-visibleHeight)*.35f);
                float top=cam.WorldToScreenPoint(maskBounds.max).y+upperOverscan*pixel;
                var screenCenter=new Vector2(Mathf.Round(cx),Mathf.Round(top-texture.height*.5f*pixel));
                preview.transform.position=SnowWorld(cam,screenCenter);
                float unit=(SnowWorld(cam,screenCenter+Vector2.up*pixel)-SnowWorld(cam,screenCenter)).magnitude;
                preview.transform.localScale=Vector3.one*(unit*64);
                architecture.SetActive(false);preview.SetActive(true);
                for(int n=0;n<3;n++)yield return null;
                Canvas.ForceUpdateCanvases();
                var after=SnowGeometry(layout);
                Assert.That(JsonUtility.ToJson(after),Is.EqualTo(JsonUtility.ToJson(before)),"Backdrop-only edit preserves every measured live UI/actor rectangle");
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out report),Is.Empty,report);
                var sb=renderer.bounds;
                var lo=cam.WorldToScreenPoint(sb.min);var hi=cam.WorldToScreenPoint(sb.max);
                Assert.That((hi.x-lo.x)/texture.width,Is.EqualTo(pixel).Within(.01),"Uniform native pixel X");
                Assert.That((hi.y-lo.y)/texture.height,Is.EqualTo(pixel).Within(.01),"Uniform native pixel Y");
                var ml=cam.WorldToScreenPoint(maskBounds.min);var mh=cam.WorldToScreenPoint(maskBounds.max);
                // Record viewport coverage separately from the retained floor layer.
                records.Add(new SnowStageRecord{size=key,geometry=after,plate=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),
                    viewport=Rect.MinMaxRect(ml.x,ml.y,mh.x,mh.y),nativePixelScale=pixel,report=report});
                File.WriteAllText(SnowStage02+"/Preview/Measurements.json",JsonUtility.ToJson(new SnowStageRecords{items=records.ToArray()},true));
                yield return ForegroundScreenshot(SnowStage02+"/Preview/After/"+key+".png");
            }
        }
        finally
        {
            architecture.SetActive(true);UnityEngine.Object.Destroy(preview);
            UnityEngine.Object.Destroy(sprite);UnityEngine.Object.Destroy(texture);Time.timeScale=1;
        }
    }
    static Vector3 SnowWorld(Camera camera,Vector2 screen)
    {
        var ray=camera.ScreenPointToRay(screen);var plane=new Plane(Vector3.forward,Vector3.zero);
        Assert.That(plane.Raycast(ray,out float distance),Is.True);return ray.GetPoint(distance);
    }
    SnowGeometryRecord SnowGeometry(GameplayPixelLayoutController layout)
    {
        var slots=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).ToArray();
        Assert.That(slots.Length,Is.EqualTo(3));
        Assert.That(Run.Board.Width,Is.EqualTo(8));Assert.That(Run.Board.Height,Is.EqualTo(8));
        return new SnowGeometryRecord{
            top=GameplayPixelLayoutController.ScreenRect(layout.TopHud),board=GameplayPixelLayoutController.ScreenRect(layout.BoardArea),
            bottom=GameplayPixelLayoutController.ScreenRect(layout.BottomHud),
            portrait=GameplayPixelLayoutController.ScreenRect((RectTransform)GameObject.Find("PlayerSection").transform),
            feet=slots.Select(BattleFloorAnchor.EnemyFeetScreenY).ToArray(),
            health=slots.Select(s=>GameplayPixelLayoutController.ScreenRect(s.CombatBarRect)).ToArray(),
            controls=layout.GetComponentsInChildren<Button>().OrderBy(b=>b.name).Select(b=>new SnowControl{name=b.name,
                rect=GameplayPixelLayoutController.ScreenRect((RectTransform)b.transform),sprite=AssetDatabase.GetAssetPath(b.image?.sprite)}).ToArray()
        };
    }
    [Serializable]class SnowGeometryRecord{public Rect top,board,bottom,portrait;public float[] feet;public Rect[] health;public SnowControl[] controls;}
    [Serializable]class SnowControl{public string name,sprite;public Rect rect;}
    [Serializable]class SnowStageRecord{public string size,report;public SnowGeometryRecord geometry;public Rect plate,viewport;public float nativePixelScale;}
    [Serializable]class SnowStageRecords{public SnowStageRecord[] items;}
}
