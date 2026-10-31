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
    const string SnowStage08="ArtSource/SnowyMountainStaged/Stage08";

    // Disposable production-Game review. This does not register or replace a live zone kit.
    [UnityTest,Explicit] public IEnumerator SnowyMountainStage08AtmosphereReview()
    { yield return SnowyMountainAssembledReview(SnowStage08,false); }

    IEnumerator SnowyMountainAssembledReview(string reviewRoot,bool finalAssembly,bool sideCornerAssembly=false,bool sidePeek=false)
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
        SpriteRenderer skyExtension=null;
        if(finalAssembly)
        {
            var sky=new GameObject("SnowStage09NativeSkyOverscan_PreviewOnly");sky.transform.SetParent(plate.transform,false);
            skyExtension=sky.AddComponent<SpriteRenderer>();skyExtension.sprite=SnowPreviewSprite(SnowStage09+"/Exports/SkyExtension.png",64);
            skyExtension.sortingLayerID=vista.sortingLayerID;skyExtension.sortingOrder=vista.sortingOrder;
            sky.transform.localPosition=new Vector3(0,(vista.sprite.rect.height+skyExtension.sprite.rect.height)/128f,0);
        }
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
        GameObject floorRoot=null,sideRoot=null,lowerRoot=null; SnowAtmosphereReview atmosphere=null;bool asyncShaders=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
        var records=new List<SnowBoardRecord>();
        try
        {
            foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
            {
                Time.timeScale=1;
                if(atmosphere!=null){atmosphere.root.SetActive(false);UnityEngine.Object.Destroy(atmosphere.root);atmosphere=null;}
                if(lowerRoot){lowerRoot.SetActive(false);UnityEngine.Object.Destroy(lowerRoot);}
                if(sideRoot){sideRoot.SetActive(false);UnityEngine.Object.Destroy(sideRoot);}
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


                floorRoot.SetActive(false);UnityEngine.Object.Destroy(floorRoot);
                floorRoot=SnowBoardEnvironment(ledge,mutedWall,cam,sourceTop,pixel);
                arena.SetActive(false);bottomEnclosure.SetActive(false);
                // Reveal the existing overscan around the removed enclosure, without moving the scenery.
                vista.maskInteraction=SpriteMaskInteraction.None;
                if(skyExtension)
                    Assert.That(cam.WorldToScreenPoint(skyExtension.bounds.max).y,Is.GreaterThanOrEqualTo(Screen.height),"Native sky overscan covers the physical top beyond the safe area");
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

                sideRoot=SnowStage06Sides(cam,fr,pixel);
                for(int i=0;i<3;i++)yield return null;
                Assert.That(JsonUtility.ToJson(SnowGeometry(layout)),Is.EqualTo(JsonUtility.ToJson(after)),"Dressing cannot move the shared UI or actors");
                Assert.That(SnowBoardSpriteState(lockedRenderers),Is.EqualTo(lockedState));
                lowerRoot=SnowStage07Lower(cam,fr,pixel,layout);
                for(int i=0;i<3;i++)yield return null;
                var sharedState=SnowStage07UiState(layout);
                if(!sideCornerAssembly)yield return ForegroundScreenshot(reviewRoot+"/Preview/Before/"+key+".png");
                atmosphere=SnowStage08Atmosphere(cam,fr,pixel,feet,after.portrait,sideRoot,lowerRoot);
                var rng=UnityEngine.Random.state;
                for(int n=0;n<96;n++){atmosphere.Sample(n*.125f,false);atmosphere.Validate(fr,pixel);}
                Assert.That(UnityEngine.Random.state,Is.EqualTo(rng),"Cosmetic paths do not use gameplay RNG");
                atmosphere.Sample(0,false);
                var first=atmosphere.State();atmosphere.Sample(12,false);
                Assert.That(atmosphere.State(),Is.EqualTo(first),"Authored 12-second loop closes exactly");
                atmosphere.Sample(0,true);var reduced=atmosphere.State();atmosphere.Sample(7,true);
                Assert.That(atmosphere.State(),Is.EqualTo(reduced),"Reduced Motion is stable");
                Assert.That(atmosphere.root.GetComponentsInChildren<SpriteRenderer>().All(r=>!r.enabled),Is.True);
                if(!sideCornerAssembly)yield return ForegroundScreenshot(reviewRoot+"/Preview/Reduced/"+key+".png");
                atmosphere.Sample(2.5f,false);for(int i=0;i<2;i++)yield return null;
                if(sideCornerAssembly)
                {
                    if(!sidePeek)yield return ForegroundScreenshot(reviewRoot+"/Preview/Before/"+key+".png");
                    sideRoot.SetActive(false);UnityEngine.Object.Destroy(sideRoot);
                    lowerRoot.SetActive(false);UnityEngine.Object.Destroy(lowerRoot);
                    sideRoot=SnowPolishSides(cam,fr,pixel);
                    lowerRoot=SnowPolishCorners(cam,fr,pixel);
                    SnowPolishGlints(atmosphere,cam,pixel,sideRoot,lowerRoot);
                    atmosphere.Sample(2.5f,false);
                    for(int i=0;i<3;i++)yield return null;
                    if(sidePeek)
                    {
                        yield return ForegroundScreenshot(reviewRoot+"/Preview/Before/"+key+".png");
                        SnowPolishMoveSides(cam,fr,pixel,sideRoot,lowerRoot);
                        SnowPolishGlints(atmosphere,cam,pixel,sideRoot,lowerRoot);
                        atmosphere.Sample(2.5f,false);
                        for(int i=0;i<3;i++)yield return null;
                    }
                    SnowPolishValidate(cam,fr,pixel,sidePeek,sideRoot,lowerRoot);
                    atmosphere.Validate(fr,pixel);
                }
                Assert.That(SnowStage07UiState(layout),Is.EqualTo(sharedState));
                Assert.That(JsonUtility.ToJson(SnowGeometry(layout)),Is.EqualTo(JsonUtility.ToJson(after)));
                Assert.That(SnowBoardSpriteState(lockedRenderers),Is.EqualTo(lockedState));
                records.Add(new SnowBoardRecord{size=key,geometry=after,boardFrame=fr,nativePixelScale=pixel,wallTop=sourceTop-84*pixel,report=report});
                File.WriteAllText(reviewRoot+"/Preview/Measurements.json",JsonUtility.ToJson(new SnowBoardRecords{items=records.ToArray()},true));
                yield return ForegroundScreenshot(reviewRoot+"/Preview/After/"+key+".png");
                if(finalAssembly)
                    SnowStage09RecordAssembly(reviewRoot,key,cam,plate,floorRoot,sideRoot,lowerRoot,portraitFrame,portraitMask,portraitDressing,boardTarget,atmosphere.root);
                if(key=="720x1280"&&!sideCornerAssembly)
                {
                    for(int n=0;n<96;n++)
                    {
                        atmosphere.Sample(n*.125f,false);
                        yield return null;
                        yield return ForegroundScreenshot(reviewRoot+"/Preview/Motion/"+n.ToString("D3")+".png");
                    }
                    if(finalAssembly)yield return SnowStage09LiveReview(reviewRoot,cam,atmosphere,layout,fr,pixel);
                }
            }
        }
        finally
        {
            if(atmosphere!=null)UnityEngine.Object.Destroy(atmosphere.root);
            ShaderUtil.allowAsyncCompilation=asyncShaders;boardVisuals.enabled=visualState;
            for(int i=0;i<originals.Length;i++)if(originals[i])originals[i].SetActive(activeStates[i]);
            architecture.SetActive(true);oldFloor.SetActive(true);
            if(lowerRoot)UnityEngine.Object.Destroy(lowerRoot);if(sideRoot)UnityEngine.Object.Destroy(sideRoot);if(floorRoot)UnityEngine.Object.Destroy(floorRoot);UnityEngine.Object.Destroy(plate);
            foreach(var r in new[]{portraitFrame,portraitMask,portraitDressing,boardTarget})if(r)UnityEngine.Object.Destroy(r.gameObject);
            foreach(var o in snowPortraitTemporary)if(o)UnityEngine.Object.Destroy(o);snowPortraitTemporary.Clear();Time.timeScale=1;
            GameplayPixelLayoutController.ValidationSafeArea=null;
        }
    }


    // Preview sampler only. Future production binding must tick this cosmetic clock with
    // pause-aware delta time and PresentationPreferences.ReducedMotion; no board ownership.
    sealed class SnowAtmosphereReview
    {
        public GameObject root;public Camera camera;public float pixel;
        public readonly List<SnowAtmosphereDetail> details=new();
        public void Sample(float time,bool reduced)
        {
            foreach(var d in details)
            {
                if(reduced){d.renderer.enabled=false;continue;}
                float t=Mathf.Repeat(time+d.phase,12f);
                if(d.glint)
                {
                    int frame=Mathf.FloorToInt(t/.125f);
                    d.renderer.enabled=frame<d.frames.Length;
                    d.renderer.sprite=d.frames[Mathf.Min(frame,d.frames.Length-1)];
                    d.renderer.color=new Color(1,1,1,.65f);
                }
                else
                {
                    float u=t/12f;
                    float x=Mathf.Round((d.area.center.x+Mathf.Sin(u*Mathf.PI*2)*3*pixel)/pixel)*pixel;
                    float y=Mathf.Round(Mathf.Lerp(d.area.yMax,d.area.yMin,u)/pixel)*pixel;
                    d.renderer.transform.position=SnowWorld(camera,new Vector2(x,y));
                    float fade=Mathf.Min(Mathf.Clamp01(u/.12f),Mathf.Clamp01((1-u)/.12f));
                    d.renderer.color=new Color(1,1,1,d.opacity*fade);d.renderer.enabled=fade>0;
                }
            }
        }
        public string State()=>string.Join(";",details.Select(d=>d.renderer.enabled+" "+d.renderer.transform.position.ToString("F3")+" "+d.renderer.color.ToString("F3")+" "+d.renderer.sprite.name));
        public void Validate(Rect board,float unit)
        {
            foreach(var d in details)
            {
                var sr=d.renderer;var lo=camera.WorldToScreenPoint(sr.bounds.min);var hi=camera.WorldToScreenPoint(sr.bounds.max);
                Assert.That((hi.x-lo.x)/sr.sprite.rect.width,Is.EqualTo(unit).Within(.01));
                Assert.That((hi.y-lo.y)/sr.sprite.rect.height,Is.EqualTo(unit).Within(.01));
                Assert.That(lo.x,Is.GreaterThanOrEqualTo(-.01));Assert.That(hi.x,Is.LessThanOrEqualTo(Screen.width+.01));
                Assert.That(lo.y,Is.GreaterThanOrEqualTo(-.01));Assert.That(hi.y,Is.LessThanOrEqualTo(Screen.height+.01));
                Assert.That(hi.x<=board.xMin||lo.x>=board.xMax||hi.y<=board.yMin||lo.y>=board.yMax,Is.True,"Atmosphere never crosses the board frame or gems");
            }
        }
    }
    sealed class SnowAtmosphereDetail
    {public SpriteRenderer renderer;public Rect area;public float phase,opacity;public bool glint;public Sprite[] frames;}

    SnowAtmosphereReview SnowStage08Atmosphere(Camera cam,Rect frame,float pixel,float feet,Rect portrait,GameObject sides,GameObject lower)
    {
        var a=new SnowAtmosphereReview{root=new GameObject("SnowStage08Atmosphere_PreviewOnly"),camera=cam,pixel=pixel};
        var sprites=Directory.GetFiles(SnowStage08+"/Exports","*.png").ToDictionary(Path.GetFileNameWithoutExtension,p=>SnowPreviewSprite(p,64));
        var names=new[]{"MoteSmall","MoteWide","Snowflake"};
        void AddSnow(float x,float low,float high,int id,float opacity)
        {
            if(high-low<12*pixel)return;
            x=Mathf.Round(x/pixel)*pixel;low=Mathf.Round(low/pixel)*pixel;high=Mathf.Round(high/pixel)*pixel;
            var sp=sprites[names[id%names.Length]];
            SnowRevisionTile(a.root.transform,sp,cam,x-sp.rect.width*pixel/2,high+sp.rect.height*pixel/2,pixel,-64);
            var sr=a.root.transform.GetChild(a.root.transform.childCount-1).GetComponent<SpriteRenderer>();
            a.details.Add(new SnowAtmosphereDetail{renderer=sr,area=Rect.MinMaxRect(x,low,x,high),phase=(id*2.371f)%12,opacity=opacity});
        }
        // Keep snow behind actors and controls. Top-band placement leaves labels quiet.
        float x0=portrait.xMax+14*pixel,x1=Screen.width-14*pixel;
        for(int i=0;i<12;i++)
        {
            float upper=Screen.height-(55+(i%3)*20)*pixel;
            float bottom=Mathf.Max(feet+66*pixel,upper-(48+(i%4)*9)*pixel);
            AddSnow(Mathf.Lerp(x0,x1,(i+.5f)/12),bottom,upper,i,.35f+(i%3)*.09f);
        }
        for(int side=0;side<2;side++)for(int i=0;i<4;i++)
        {
            float x=side==0?Mathf.Max(9*pixel,frame.xMin*.4f):Screen.width-Mathf.Max(9*pixel,(Screen.width-frame.xMax)*.4f);
            float top=Mathf.Lerp(frame.yMax-50*pixel,frame.yMin+95*pixel,i/4f);
            AddSnow(x,top-36*pixel,top,12+side*4+i,.26f);
        }
        // Glints belong to existing ice tips, not floating decorative objects.
        var glints=Enumerable.Range(0,6).Select(i=>sprites["Glint_"+i.ToString("D2")]).ToArray();
        var anchors=sides.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.sprite.name=="IceCluster").Concat(lower.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.sprite.name=="LowerIce")).ToArray();
        for(int i=0;i<anchors.Length;i++)
        {
            var b=anchors[i].bounds;var lo=cam.WorldToScreenPoint(b.min);var hi=cam.WorldToScreenPoint(b.max);
            float x=Mathf.Round((lo.x+18*pixel)/pixel)*pixel,y=Mathf.Round((hi.y-7*pixel)/pixel)*pixel;
            SnowRevisionTile(a.root.transform,glints[0],cam,x-4*pixel,y+4*pixel,pixel,-63);
            var sr=a.root.transform.GetChild(a.root.transform.childCount-1).GetComponent<SpriteRenderer>();
            a.details.Add(new SnowAtmosphereDetail{renderer=sr,glint=true,frames=glints,phase=i*4+1.25f});
        }
        Assert.That(a.root.GetComponentsInChildren<UnityEngine.EventSystems.UIBehaviour>(),Is.Empty);
        return a;
    }
}
