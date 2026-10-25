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
using Object = UnityEngine.Object;

/// <summary>Isolated board-frame review using existing gem/cell art and the native frame fitter.</summary>
public sealed class ForestReworkV2Checkpoint04Tests
{
    const string Root = "ArtSource/ForestReworkV2/Checkpoint04";
    const string Art = "Assets/_Game/Art/ForestReworkV2/Checkpoint04/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint04.unity";
    const int Width = 720, Height = 704;
    [Serializable] public class Use { public string name, path, type; public Rect rect; public float scale; }
    [Serializable] public class Evidence { public int width, height; public Use[] images; public Vector2[] sizes; }
    static readonly Vector2[] Sizes = { new Vector2(46,46), new Vector2(304,304), new Vector2(305,303), new Vector2(360,280), new Vector2(259,411) };
    static Sprite Load(string p) { var s = AssetDatabase.LoadAssetAtPath<Sprite>(p); Assert.That(s, Is.Not.Null, p); return s; }
    static RectTransform Rect(string name, Transform parent, int x, int y, int w, int h)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0,1); r.anchoredPosition = new Vector2(x,-y); r.sizeDelta = new Vector2(w,h); return r;
    }
    static Image Img(string name, Transform parent, string file, int x, int y)
    {
        var s = Load(file); var i = Rect(name,parent,x,y,(int)s.rect.width,(int)s.rect.height).gameObject.AddComponent<Image>(); i.sprite=s; i.raycastTarget=false; return i;
    }
    static Transform Copy(Transform source, Transform root)
    {
        var copy=Object.Instantiate(source.gameObject,root,false).transform;copy.name=source.name;return copy;
    }
    static void Revision(Transform root, bool enabled)
    {
        root.Find("Board shell 2x native modules").gameObject.SetActive(!enabled);
        root.Find("Board props 2x").gameObject.SetActive(!enabled);
        root.Find("Revised board frame 2x").gameObject.SetActive(enabled);
    }
    [MenuItem("Dungeon Matcher/Art Preview/Forest rework v2 checkpoint 04")]
    public static void Build()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Forest board review camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=Height/128f;camera.aspect=(float)Width/Height;
        camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.magenta;camera.allowHDR=camera.allowMSAA=false;
        var canvas=new GameObject("Forest checkpoint 04 board-only review",typeof(RectTransform)).AddComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;canvas.pixelPerfect=true;
        var scaler=canvas.gameObject.AddComponent<CanvasScaler>();scaler.referencePixelsPerUnit=64;scaler.dynamicPixelsPerUnit=1;
        var root=(RectTransform)canvas.transform;root.sizeDelta=new Vector2(Width,Height);root.pivot=Vector2.zero;root.position=new Vector3(-Width/128f,-Height/128f,0);root.localScale=Vector3.one/64;
        var backing=Rect("Unchanged quiet timber",root,0,0,360,352);backing.localScale=Vector3.one*2;
        var bg=Img("Existing backing",backing,"Assets/_Game/Art/ForestHybrid/Stage02/Portrait_Inset.png",0,0);bg.type=Image.Type.Tiled;bg.rectTransform.sizeDelta=new Vector2(360,352);
        var source=EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage05.unity",OpenSceneMode.Additive);SceneManager.SetActiveScene(scene);
        var src=source.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Canvas>()).Single().transform;
        foreach(string name in new[]{"Board contents native 64px","Board shell 2x native modules","Board props 2x"})
        {
            var c=(RectTransform)Copy(src.Find(name),root);c.anchoredPosition+=new Vector2(0,432);
        }
        var revised=Rect("Revised board frame 2x",root,0,0,360,352);revised.localScale=Vector3.one*2;
        var target=(RectTransform)Copy(src.Find("Board shell 2x native modules/BoardFrameTarget"),revised);target.anchoredPosition=new Vector2(28,-24);
        EditorSceneManager.CloseScene(source,true);
        var frame=target.Find("ModularFrame");string[] names={"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner","TopEdge","BottomEdge","LeftEdge","RightEdge"};
        string[] files={"Corner_TL","Corner_TR","Corner_BL","Corner_BR","Rail_Top","Rail_Bottom","Rail_Left","Rail_Right"};
        for(int n=0;n<8;n++)frame.Find(names[n]).GetComponent<Image>().sprite=Load(Art+"Board_"+files[n]+".png");
        frame.GetComponent<ResponsiveModularFrameFitter>().ConfigureNativePieces(null,150);
        Img("Upper connected foliage",target,Art+"Board_Foliage_UL.png",-2,-2);
        var lower=Img("Lower connected foliage",target,Art+"Board_Foliage_LR.png",0,0).rectTransform;
        lower.anchorMin=lower.anchorMax=new Vector2(1,0);lower.anchoredPosition=new Vector2(-63,66);
        Revision(root,true);Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);EditorSceneManager.SaveScene(scene,ScenePath);
    }
    static void SetSize(int scale)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(Width*scale,Height*scale)});
    static IEnumerator Capture(string name)
    {
        string p=Path.GetFullPath(Root+"/Previews/"+name+".png");if(File.Exists(p))File.Delete(p);ScreenCapture.CaptureScreenshot(p);float end=Time.realtimeSinceStartup+20;
        while(!File.Exists(p)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}
    }
    static void CheckResize(Transform root)
    {
        var target=(RectTransform)root.Find("Revised board frame 2x/BoardFrameTarget");var f=target.Find("ModularFrame").GetComponent<ResponsiveModularFrameFitter>();
        foreach(var size in Sizes)
        {
            target.sizeDelta=size;Canvas.ForceUpdateCanvases();f.RefreshFrame();
            foreach(var im in f.GetComponentsInChildren<Image>())
            {
                var r=im.rectTransform;Assert.That(im.enabled,Is.True);Assert.That(r.localScale,Is.EqualTo(Vector3.one));
                Assert.That(r.anchoredPosition.x,Is.EqualTo(Mathf.Round(r.anchoredPosition.x)).Within(.001));Assert.That(r.anchoredPosition.y,Is.EqualTo(Mathf.Round(r.anchoredPosition.y)).Within(.001));
                if(im.type==Image.Type.Simple)Assert.That(r.rect.size,Is.EqualTo(im.sprite.rect.size));else Assert.That(im.pixelsPerUnit*im.pixelsPerUnitMultiplier,Is.EqualTo(1).Within(.001));
            }
            Assert.That(((RectTransform)f.transform.Find("LeftEdge")).anchoredPosition.x+5,Is.EqualTo(11));
            Assert.That(((RectTransform)f.transform.Find("RightEdge")).anchoredPosition.x+5,Is.EqualTo(size.x-12));
            Assert.That(-((RectTransform)f.transform.Find("TopEdge")).anchoredPosition.y+5,Is.EqualTo(11));
            Assert.That(-((RectTransform)f.transform.Find("BottomEdge")).anchoredPosition.y+5,Is.EqualTo(size.y-12));
        }
        target.sizeDelta=new Vector2(45,45);f.RefreshFrame();Assert.That(f.GetComponentsInChildren<Image>().All(i=>!i.enabled),Is.True);
        target.sizeDelta=new Vector2(304,304);Canvas.ForceUpdateCanvases();f.RefreshFrame();
    }
    [UnityTearDown] public IEnumerator Cleanup(){if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit("Checkpoint4 board frame, native gem exclusion and modular resize review.")]
    public IEnumerator BoardReview()
    {
        Directory.CreateDirectory(Root+"/Previews");Directory.CreateDirectory(Root+"/Diagnostics");Build();SetSize(1);yield return new EnterPlayMode();yield return null;yield return null;
        yield return CaptureAndCheck();yield return new ExitPlayMode();
    }
    static IEnumerator CaptureAndCheck()
    {
        var root=Object.FindFirstObjectByType<Canvas>().transform;Revision(root,false);Canvas.ForceUpdateCanvases();yield return null;yield return null;yield return Capture("Unity-Before-720x704");
        Revision(root,true);CheckResize(root);
        foreach(int scale in new[]{1,2})
        {
            SetSize(scale);float end=Time.realtimeSinceStartup+20;while(Screen.width!=Width*scale||Screen.height!=Height*scale){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}
            Canvas.ForceUpdateCanvases();yield return null;yield return null;var uses=new List<Use>();
            foreach(var im in Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.enabled))
            {
                var r=GameplayPixelLayoutController.ScreenRect(im.rectTransform);float factor=im.transform.lossyScale.x/root.lossyScale.x*scale;
                Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point),im.name);Assert.That(im.color,Is.EqualTo(Color.white));Assert.That(im.raycastTarget,Is.False);
                Assert.That(factor,Is.EqualTo(Mathf.Round(factor)).Within(.001));Assert.That(im.transform.lossyScale.x,Is.EqualTo(im.transform.lossyScale.y));
                Assert.That(r.x,Is.EqualTo(Mathf.Round(r.x)).Within(.02),im.name);Assert.That(r.y,Is.EqualTo(Mathf.Round(r.y)).Within(.02),im.name);
                if(im.type==Image.Type.Simple){Assert.That(r.width,Is.EqualTo(im.sprite.rect.width*factor).Within(.02),im.name);Assert.That(r.height,Is.EqualTo(im.sprite.rect.height*factor).Within(.02),im.name);}
                Assert.That(r.xMin,Is.GreaterThanOrEqualTo(-.02),im.name);Assert.That(r.yMin,Is.GreaterThanOrEqualTo(-.02),im.name);
                Assert.That(r.xMax,Is.LessThanOrEqualTo(Screen.width+.02),im.name);Assert.That(r.yMax,Is.LessThanOrEqualTo(Screen.height+.02),im.name);
                uses.Add(new Use{name=im.name,path=AssetDatabase.GetAssetPath(im.sprite),rect=r,scale=factor,type=im.type.ToString()});
            }
            foreach(var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(c!=null)Assert.That(new[]{"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"},Does.Not.Contain(c.GetType().Name));
            File.WriteAllText(Root+"/Diagnostics/Unity-"+Screen.width+"x"+Screen.height+".json",JsonUtility.ToJson(new Evidence{width=Screen.width,height=Screen.height,images=uses.ToArray(),sizes=Sizes},true));
            yield return Capture("Unity-After-"+Screen.width+"x"+Screen.height);
        }
        SetSize(1);yield return null;yield return null;root.Find("Revised board frame 2x").gameObject.SetActive(false);yield return null;yield return null;yield return Capture("Unity-NoFrame-720x704");
        root.Find("Revised board frame 2x").gameObject.SetActive(true);root.Find("Board contents native 64px/Existing native gems").gameObject.SetActive(false);yield return null;yield return null;yield return Capture("Unity-Empty-720x704");
    }
}
