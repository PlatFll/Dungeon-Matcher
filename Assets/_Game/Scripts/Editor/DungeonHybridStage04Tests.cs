using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Isolated Stage 04 prop dressing. No production scene or runtime changes.</summary>
public sealed class DungeonHybridStage04Tests
{
    const string Root="ArtSource/DungeonHybrid/Stage04";
    const string Art="Assets/_Game/Art/DungeonHybrid/Stage04/";
    const string ScenePath="Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage04.unity";
    static RectTransform Rect(string name,Transform parent,int x,int y,int w,int h)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
        r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
    }
    static Image Img(string name,Transform parent,int x,int y)
    {
        var s=AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".png");Assert.That(s,Is.Not.Null,name);
        var r=Rect(name,parent,x,y,(int)s.rect.width,(int)s.rect.height);
        var i=r.gameObject.AddComponent<Image>();i.sprite=s;i.color=Color.white;i.raycastTarget=false;return i;
    }
    static void Chain(Transform parent,int x,int y,int repeats)
    {
        var group=Rect("Complete modular chain",parent,x,y,14,39+14*repeats);
        Img("Chain_Anchor",group,0,0);
        for(int n=0;n<repeats;n++)Img("Chain_Repeat",group,0,25+n*14);
        Img("Chain_End",group,0,25+repeats*14);
    }
    static void Import()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(string file in Directory.GetFiles(Art,"*.png"))
        {
            var i=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
            i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=64;
            i.filterMode=FilterMode.Point;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;
            i.npotScale=TextureImporterNPOTScale.None;i.wrapMode=TextureWrapMode.Clamp;i.alphaIsTransparency=false;i.maxTextureSize=2048;
            var s=new TextureImporterSettings();i.ReadTextureSettings(s);s.spriteMeshType=SpriteMeshType.FullRect;i.SetTextureSettings(s);i.SaveAndReimport();
        }
    }
    [MenuItem("Dungeon Matcher/Art Preview/Hybrid Stage 04 prop dressing")]
    public static void Build()
    {
        Import();var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage03_Top.unity",OpenSceneMode.Single);
        var root=(RectTransform)Object.FindFirstObjectByType<Canvas>().transform;
        var cast=root.Find("Approved cast - static scale probes");Assert.That(cast,Is.Not.Null);
        var props=Rect("Stage 04 props - separate native sprites",root,0,0,360,240);props.SetSiblingIndex(cast.GetSiblingIndex());
        var wall=Rect("Wall dressing",props,0,0,360,240);
        Chain(wall,110,3,2);Chain(wall,231,3,1);
        Img("Banner",wall,40,42);Img("Banner",wall,143,64);Img("Banner",wall,289,63);
        Img("Torch",wall,111,74);Img("Torch",wall,333,75);
        // Floor occupies native y178..189. Ground contact is y183, with 2px
        // transparent clearance below every prop and no changes to the floor.
        var classic=Rect("Classic ground dressing",props,0,0,360,240);
        Img("Skull",classic,110,156);Img("Bones",classic,210,158);Img("Barrel",classic,314,141);
        var alternate=Rect("Alternate jar and rubble",props,0,0,360,240);
        Img("Jar",alternate,314,149);Img("Rubble",alternate,205,157);
        alternate.gameObject.SetActive(false);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
        EditorSceneManager.SaveScene(scene,ScenePath);
    }
    static void SetSize(int scale)=>typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(360*scale,240*scale)});
    [UnityTearDown]public IEnumerator Cleanup(){if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit("Builds Stage 04 isolated art scene and captures native and integer-zoom Unity Game View.")]
    public IEnumerator PropsAtNativeScaleAndClearance()
    {
        Build();SetSize(1);yield return new EnterPlayMode();yield return Capture();yield return new ExitPlayMode();
    }
    static IEnumerator Capture()
    {
        var root=Object.FindFirstObjectByType<Canvas>().transform;
        var props=root.Find("Stage 04 props - separate native sprites");
        var cast=root.Find("Approved cast - static scale probes");
        Assert.That(props.GetSiblingIndex(),Is.LessThan(cast.GetSiblingIndex()));
        Assert.That(props.GetSiblingIndex(),Is.LessThan(root.Find("Portrait retained").GetSiblingIndex()));
        Assert.That(root.GetComponent<CanvasScaler>().referencePixelsPerUnit,Is.EqualTo(64));
        foreach(string mode in new[]{"baseline","classic","classic-cast","alternate","alternate-cast"})
        {
            props.gameObject.SetActive(mode!="baseline");
            cast.gameObject.SetActive(mode=="baseline"||mode.EndsWith("-cast"));
            props.Find("Classic ground dressing").gameObject.SetActive(!mode.StartsWith("alternate"));
            props.Find("Alternate jar and rubble").gameObject.SetActive(mode.StartsWith("alternate"));
            foreach(int scale in new[]{1,2,3})
            {
                SetSize(scale);yield return new WaitForSecondsRealtime(.4f);Canvas.ForceUpdateCanvases();yield return null;yield return null;
                Assert.That(Screen.width,Is.EqualTo(360*scale));Assert.That(Screen.height,Is.EqualTo(240*scale));
                foreach(var i in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
                {
                    Assert.That(i.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(i.color,Is.EqualTo(Color.white));Assert.That(i.raycastTarget,Is.False);
                    var r=GameplayPixelLayoutController.ScreenRect(i.rectTransform);
                    Assert.That(r.x,Is.EqualTo(Mathf.Round(r.x)).Within(.01f));Assert.That(r.y,Is.EqualTo(Mathf.Round(r.y)).Within(.01f));
                    if(i.type==Image.Type.Simple)
                    {Assert.That(r.width,Is.EqualTo(i.sprite.rect.width*scale).Within(.01f));Assert.That(r.height,Is.EqualTo(i.sprite.rect.height*scale).Within(.01f));}
                    else Assert.That(i.pixelsPerUnit*i.pixelsPerUnitMultiplier,Is.EqualTo(1).Within(.001f));
                    if(!i.GetComponentInParent<RectMask2D>())
                    {Assert.That(r.xMin,Is.GreaterThanOrEqualTo(-.01f));Assert.That(r.yMin,Is.GreaterThanOrEqualTo(-.01f));Assert.That(r.xMax,Is.LessThanOrEqualTo(Screen.width+.01f));Assert.That(r.yMax,Is.LessThanOrEqualTo(Screen.height+.01f));}
                }
                foreach(var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)){t.ForceMeshUpdate();Assert.That(t.isTextTruncated,Is.False,t.text);}
                string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
                Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
                string output=Path.GetFullPath(Root+"/Previews/Unity-"+mode+"-"+Screen.width+"x"+Screen.height+".png");
                if(File.Exists(output))File.Delete(output);ScreenCapture.CaptureScreenshot(output);float deadline=Time.realtimeSinceStartup+20;
                while(!File.Exists(output)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
            }
        }
    }
}
