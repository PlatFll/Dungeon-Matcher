using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Full art-only assembly. Reuses approved scenes/assets without rewriting them.</summary>
public sealed class DungeonHybridStage05Tests
{
    const string Root="ArtSource/DungeonHybrid/Stage05";
    const string ScenePath="Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage05.unity";
    const string OldArt="Assets/_Game/Art/DungeonPixelLab/";
    [Serializable] public class SpriteUse { public string path,name;public Rect nativeRect;public float displayScale; }
    [Serializable] public class Usage { public int width=720,height=1280;public SpriteUse[] sprites; }
    static RectTransform Rect(string name,Transform parent,int x,int y,int w,int h,int scale=1)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
        r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
        r.localScale=Vector3.one*scale;return r;
    }
    static Sprite Load(string path){var s=AssetDatabase.LoadAssetAtPath<Sprite>(path);Assert.That(s,Is.Not.Null,path);return s;}
    static Image Img(string name,Transform parent,Sprite sprite,int x,int y,int width=0,int height=0)
    {
        var r=Rect(name,parent,x,y,width>0?width:(int)sprite.rect.width,height>0?height:(int)sprite.rect.height);
        var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.raycastTarget=false;i.color=Color.white;
        if(width>0||height>0)i.type=Image.Type.Tiled;return i;
    }
    static void Move(Transform child,Transform parent)
    {
        var r=(RectTransform)child;var pos=r.anchoredPosition;var size=r.sizeDelta;var scale=r.localScale;
        r.SetParent(parent,false);r.anchoredPosition=pos;r.sizeDelta=size;r.localScale=scale;
    }
    static void Label(RectTransform parent,string text,int x,int y,int w,int h,int size,RectTransform grid)
    {
        var box=Rect("Static value - "+text,parent,x,y,w,h);
        var t=GameUi.Label("LiveLabel",box,text,new Vector2(w,h),Vector2.zero,size);
        t.alignment=TextAlignmentOptions.CenterGeoAligned;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Truncate;
        PixelTextFitter.ApplyNativeMenu(t,size);t.gameObject.AddComponent<DungeonPreviewPixelText>().Initialize(grid);
    }
    static void Health(Transform parent,RectTransform grid,string kind,string text,int x,int y,int width)
    {
        // Static art sample follows the current modular health style geometry.
        // No health owner, binding, damage or animation component is created.
        var style=Resources.Load<ModularHealthBarStyle>("UI/Finalized/Health/"+kind);Assert.That(style,Is.Not.Null);
        var root=Rect(kind+" static health sample",parent,x,y,width,24);
        Img("Native empty track",root,style.EmptyTrack,(int)style.FillInsetLeft,8,width-(int)style.FillInsetLeft-(int)style.FillInsetRight,8);
        Img("Native full health",root,style.FillStrip,(int)style.FillInsetLeft,8,width-(int)style.FillInsetLeft-(int)style.FillInsetRight,8);
        Img("Start",root,style.StartPiece,14,0);Img("Middle repeat",root,style.MiddlePiece,30,0,width-46,24);
        Img("End",root,style.EndPiece,width-16,0);Img("Badge",root,style.Badge,0,0);
        Label(root,text,27,3,width-40,18,16,grid);
    }
    static void ExtendedWall(RectTransform canvas)
    {
        var backing=Rect("Continuous modular lower wall",canvas,0,480,360,400,2);backing.gameObject.AddComponent<RectMask2D>();backing.SetAsFirstSibling();
        var source=JsonUtility.FromJson<DungeonHybridStage01Tests.Layout>(File.ReadAllText("ArtSource/DungeonHybrid/Stage01/Placement.json"));
        // Continue at native wall row 240; use complete approved 48px tiles.
        // The viewport mask only trims the repeat surface at the screen edge.
        for(int y=0;y<400;y+=48)
        {
            int row=(y/48)%5*48;
            foreach(var p in source.placements.Where(p=>p.layer==0&&p.y==row))Img(p.name,backing,Load(p.path),p.x,y);
        }
        var sides=Rect("Lower side masonry",canvas,0,480,360,400,2);
        foreach(bool right in new[]{false,true})
        {
            var tile=Load(OldArt+"Support/Support_Corner_"+(right?"Right":"Left")+".png");
            for(int y=0;y<376;y+=28)Img("Repeated edge masonry",sides,tile,right?332:0,y);
            Img("Complete masonry foot",sides,Load(OldArt+"Support/Support_Foot.png"),right?332:0,376);
        }
    }
    [MenuItem("Dungeon Matcher/Art Preview/Hybrid Stage 05 full assembled preview")]
    public static void Build()
    {
        // Open approved Stage04 as a read-only source and save only to Stage05.
        var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage04.unity",OpenSceneMode.Single);
        var canvas=Object.FindFirstObjectByType<Canvas>();var root=(RectTransform)canvas.transform;var camera=canvas.worldCamera;
        var prior=root.Cast<Transform>().ToArray();var top=Rect("Approved top assembly - 2x",root,0,0,360,240,2);
        foreach(var child in prior)Move(child,top);
        root.sizeDelta=new Vector2(720,1280);root.pivot=Vector2.zero;root.position=new Vector3(-720/128f,-1280/128f,0);
        camera.name="Full assembly preview camera";camera.orthographicSize=1280/128f;camera.aspect=720f/1280;
        canvas.name="Stage 05 art-only native assembly";
        foreach(var t in top.GetComponentsInChildren<DungeonPreviewPixelText>(true))t.Initialize(top);
        ExtendedWall(root);
        var boardSource=EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage03_BoardHud.unity",OpenSceneMode.Additive);
        var sourceCanvas=boardSource.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Canvas>()).Single();
        SceneManager.SetActiveScene(scene);
        var board=Object.Instantiate(sourceCanvas.transform.Find("Board retained 64px cells").gameObject,root,false).GetComponent<RectTransform>();
        board.name="Approved native 64px board";board.anchoredPosition=new Vector2(91,-493);
        // Keep the 512px gem grid at 1x. Fit the existing frame pieces around it
        // at 2x, matching the surrounding UI without enlarging any cell.
        Object.DestroyImmediate(board.Find("ModularFrame").gameObject);
        var boardFrame=Object.Instantiate(sourceCanvas.transform.Find("Board retained 64px cells").gameObject,root,false).GetComponent<RectTransform>();
        boardFrame.name="Approved modular board frame - 2x";
        Object.DestroyImmediate(boardFrame.Find("InteriorMask").gameObject);
        boardFrame.anchoredPosition=new Vector2(78,-480);boardFrame.sizeDelta=new Vector2(282,282);boardFrame.localScale=Vector3.one*2;
        var recess=Rect("Board backing mask",boardFrame,9,9,264,264);recess.gameObject.AddComponent<RectMask2D>();
        Img("Overfilled approved dark inset",recess,Load("Assets/_Game/Art/DungeonHybrid/Stage03/Control_Inset.png"),-16,-16,296,296);
        recess.SetAsFirstSibling();boardFrame.SetSiblingIndex(board.GetSiblingIndex());
        boardFrame.GetComponentInChildren<ResponsiveModularFrameFitter>().RefreshFrame();
        var bottom=Rect("Approved bottom HUD - 2x",root,100,1056,260,104,2);
        foreach(Transform child in sourceCanvas.transform)
        {
            if(child.name=="Board retained 64px cells")continue;
            var copy=Object.Instantiate(child.gameObject,bottom,false).GetComponent<RectTransform>();
            copy.name=child.name;
            copy.anchoredPosition+=new Vector2(-158,566);
        }
        foreach(var t in bottom.GetComponentsInChildren<DungeonPreviewPixelText>(true))t.Initialize(bottom);
        EditorSceneManager.CloseScene(boardSource,true);
        var progress=bottom.Find("Progress retained").gameObject.AddComponent<DungeonPreviewNativeMeter>();
        progress.Initialize(Load(OldArt+"BottomHud/Progress_Fill.png"),4);progress.SetFill(.65f);
        progress.transform.Find("NativeFillMask").SetSiblingIndex(1);
        var stats=Rect("Static health readability samples",root,0,0,720,480);
        Health(stats,root,"Player","100 / 100",50,370,138);
        Health(stats,root,"Normal","35 / 35",256,376,132);
        Health(stats,root,"Normal","40 / 40",496,376,132);
        // Keep lower wall beneath the approved top; all new overlays are static UI.
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
        EditorSceneManager.SaveScene(scene,ScenePath);
        var uses=root.GetComponentsInChildren<Image>(true).Select(i=>new SpriteUse{path=AssetDatabase.GetAssetPath(i.sprite),
            name=i.sprite.name,nativeRect=i.sprite.rect,displayScale=i.transform.lossyScale.x/root.lossyScale.x}).ToArray();
        File.WriteAllText(Root+"/AssemblyUsage.json",JsonUtility.ToJson(new Usage{sprites=uses},true));
    }
    static void SetSize(int scale)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
        .Invoke(null,new object[]{new Vector2Int(720*scale,1280*scale)});
    [UnityTearDown]public IEnumerator Cleanup(){if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit("Builds the full art-only dungeon and captures Unity Game View at native and doubled resolution.")]
    public IEnumerator FullAssemblyNativeScreens()
    {
        Build();SetSize(1);yield return new EnterPlayMode();yield return Capture();yield return new ExitPlayMode();
    }
    static IEnumerator Capture()
    {
        var root=Object.FindFirstObjectByType<Canvas>().transform;
        var top=root.Find("Approved top assembly - 2x");var props=top.Find("Stage 04 props - separate native sprites");
        var board=root.Find("Approved native 64px board");var cast=top.Find("Approved cast - static scale probes");
        var stats=root.Find("Static health readability samples");var meter=Object.FindFirstObjectByType<DungeonPreviewNativeMeter>();
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
        Assert.That(board.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("Cell_")),Is.EqualTo(64));
        Assert.That(board.GetComponentsInChildren<Image>().Count(i=>i.name=="Gem native scale probe"),Is.EqualTo(64));
        foreach(string mode in new[]{"SourceCheck","Classic","Alternate","Structure"})
        {
            bool structure=mode=="Structure",alternate=mode=="Alternate";
            cast.gameObject.SetActive(!structure);stats.gameObject.SetActive(mode=="Classic"||alternate);
            props.Find("Classic ground dressing").gameObject.SetActive(!alternate);props.Find("Alternate jar and rubble").gameObject.SetActive(alternate);
            foreach(var gem in board.GetComponentsInChildren<Image>(true).Where(i=>i.name=="Gem native scale probe"))gem.gameObject.SetActive(!structure);
            meter.SetFill(mode=="SourceCheck"||structure?0:.65f);
            foreach(int scale in new[]{1,2})
            {
                SetSize(scale);yield return new WaitForSecondsRealtime(.4f);Canvas.ForceUpdateCanvases();yield return null;yield return null;
                Assert.That(Screen.width,Is.EqualTo(720*scale));Assert.That(Screen.height,Is.EqualTo(1280*scale));
                foreach(var i in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
                {
                    Assert.That(i.color,Is.EqualTo(Color.white));Assert.That(i.raycastTarget,Is.False);Assert.That(i.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                    var r=GameplayPixelLayoutController.ScreenRect(i.rectTransform);
                    Assert.That(r.x,Is.EqualTo(Mathf.Round(r.x)).Within(.02f),i.name);Assert.That(r.y,Is.EqualTo(Mathf.Round(r.y)).Within(.02f),i.name);
                    float factor=i.transform.lossyScale.x/root.lossyScale.x*scale;
                    Assert.That(factor,Is.EqualTo(Mathf.Round(factor)));Assert.That(i.transform.lossyScale.x,Is.EqualTo(i.transform.lossyScale.y));
                    if(i.type==Image.Type.Simple)
                    {Assert.That(r.width,Is.EqualTo(i.sprite.rect.width*factor).Within(.02f),i.name);Assert.That(r.height,Is.EqualTo(i.sprite.rect.height*factor).Within(.02f),i.name);}
                    else Assert.That(i.pixelsPerUnit*i.pixelsPerUnitMultiplier,Is.EqualTo(1).Within(.001f),i.name);
                    if(!i.GetComponentInParent<RectMask2D>())
                    {Assert.That(r.xMin,Is.GreaterThanOrEqualTo(-.02f),i.name);Assert.That(r.yMin,Is.GreaterThanOrEqualTo(-.02f),i.name);Assert.That(r.xMax,Is.LessThanOrEqualTo(Screen.width+.02f),i.name);Assert.That(r.yMax,Is.LessThanOrEqualTo(Screen.height+.02f),i.name);}
                }
                foreach(var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)){t.ForceMeshUpdate();Assert.That(t.isTextTruncated,Is.False,t.text);}
                foreach(var f in Object.FindObjectsByType<ResponsiveModularFrameFitter>(FindObjectsSortMode.None))
                {
                    f.RefreshFrame();var target=(RectTransform)f.transform.parent;
                    foreach(var i in f.GetComponentsInChildren<Image>()){var r=i.rectTransform;Assert.That(i.enabled,Is.True);
                        Assert.That(r.anchoredPosition.x,Is.GreaterThanOrEqualTo(0));Assert.That(-r.anchoredPosition.y,Is.GreaterThanOrEqualTo(0));
                        Assert.That(r.anchoredPosition.x+r.rect.width,Is.LessThanOrEqualTo(target.rect.width));
                        Assert.That(-r.anchoredPosition.y+r.rect.height,Is.LessThanOrEqualTo(target.rect.height));}
                }
                string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
                Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
                string path=Path.GetFullPath(Root+"/Previews/Unity-"+mode+"-"+Screen.width+"x"+Screen.height+".png");
                if(File.Exists(path))File.Delete(path);ScreenCapture.CaptureScreenshot(path);float deadline=Time.realtimeSinceStartup+20;
                while(!File.Exists(path)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
            }
        }
    }
}
