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
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

/// <summary>Forest Stage 2 review only: existing responsive frame and HP presentation, no gameplay owner.</summary>
public sealed class ForestHybridStage02Tests
{
    const string Root="ArtSource/ForestHybrid/Stage02";
    const string Art="Assets/_Game/Art/ForestHybrid/Stage02/";
    const string Foundation="Assets/_Game/Art/ForestHybrid/Stage01/";
    const string ScenePath="Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage02.unity";
    const int Width=470,Height=288;
    [Serializable] public class CaptureEvidence { public int width,height; public Rect portrait,mask; public int images; public bool tiledMask,integerGeometry; }
    static RectTransform Rect(string n,Transform parent,float x,float y,float w,float h)
    {
        var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
        r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
    }
    static Sprite Load(string p) {var s=AssetDatabase.LoadAssetAtPath<Sprite>(p);Assert.That(s,Is.Not.Null,p);return s;}
    static Image Img(string n,Transform parent,Sprite s,float x,float y)
    {var i=Rect(n,parent,x,y,s.rect.width,s.rect.height).gameObject.AddComponent<Image>();i.sprite=s;i.color=Color.white;i.raycastTarget=false;return i;}
    static Sprite Ready(string n)=>AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/CombatIdles/"+n+"_Idle.png").OfType<Sprite>().Single(s=>s.name==n+"_Idle_00");
    static void Import()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(var p in Directory.GetFiles(Art,"*.png"))
        {
            var i=(TextureImporter)AssetImporter.GetAtPath(p.Replace('\\','/'));i.textureType=TextureImporterType.Sprite;
            i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=64;i.filterMode=FilterMode.Point;
            i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;i.npotScale=TextureImporterNPOTScale.None;
            i.alphaIsTransparency=false;i.maxTextureSize=2048;i.wrapMode=TextureWrapMode.Clamp;
            var settings=new TextureImporterSettings();i.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;
            settings.spriteAlignment=(int)SpriteAlignment.Center;settings.spritePivot=new Vector2(.5f,.5f);i.SetTextureSettings(settings);i.SaveAndReimport();
        }
    }
    static void GroundStudy(RectTransform root)
    {
        // Reassemble unchanged Stage 1 modules to the current Forest top reference extent.
        // Scene 01 and all its artwork/placement data remain read-only.
        for(int y=0;y<=48;y+=24)for(int x=0;x<Width;x+=48)Img("Foundation Far_CanopyRepeat",root,Load(Foundation+"Far_CanopyRepeat.png"),x,y);
        foreach(int x in new[]{0,52,104,156,418})Img("Foundation Far_SideExtension",root,Load(Foundation+"Far_SideExtension.png"),x,72);
        Img("Foundation Far_Forest",root,Load(Foundation+"Far_Forest.png"),162,72);
        foreach(int y in new[]{232,240})for(int x=0;x<Width;x+=48)Img("Foundation Understory_Repeat",root,Load(Foundation+"Understory_Repeat.png"),x,y);
        Img("Foundation Tree_Left",root,Load(Foundation+"Tree_Left.png"),4,48);
        Img("Foundation Tree_Right",root,Load(Foundation+"Tree_Right.png"),402,48);
        for(int x=0;x<Width;x+=60)Img("Foundation Canopy_FringeRepeat",root,Load(Foundation+"Canopy_FringeRepeat.png"),x,48);
        Img("Foundation Canopy_Left",root,Load(Foundation+"Canopy_Left.png"),0,48);
        Img("Foundation Canopy_Right",root,Load(Foundation+"Canopy_Right.png"),374,48);
        Img("Foundation Support_Left",root,Load(Foundation+"Support_Left.png"),171,91);
        Img("Foundation Support_Right",root,Load(Foundation+"Support_Right.png"),382,91);
        for(int x=0;x<Width;x+=60)
        {
            string suffix=(x/60)%2==0?"A":"B";
            Img("Foundation Ground_Top"+suffix,root,Load(Foundation+"Ground_Top"+suffix+".png"),x,248);
            Img("Foundation Ground_Face"+suffix,root,Load(Foundation+"Ground_Face"+suffix+".png"),x,264);
        }
        Img("Farmer static reference",root,Ready("Farmer"),213,174);
        Img("Pan static reference",root,Ready("PanVillager"),320,174);
    }
    [MenuItem("Dungeon Matcher/Art Preview/Forest Hybrid Stage 02 portrait")]
    public static void Build()
    {
        Import();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Forest portrait native study camera").AddComponent<Camera>();
        camera.orthographic=true;camera.orthographicSize=Height/128f;camera.aspect=(float)Width/Height;
        camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color32(20,39,45,255);camera.allowHDR=false;camera.allowMSAA=false;
        var canvas=new GameObject("Forest portrait study - independent native pieces",typeof(RectTransform)).AddComponent<Canvas>();
        canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;canvas.pixelPerfect=true;
        var scaler=canvas.gameObject.AddComponent<CanvasScaler>();scaler.referencePixelsPerUnit=64;scaler.dynamicPixelsPerUnit=1;
        var root=(RectTransform)canvas.transform;root.sizeDelta=new Vector2(Width,Height);root.pivot=Vector2.zero;
        root.position=new Vector3(-Width/128f,-Height/128f,0);root.localScale=Vector3.one/64;
        GroundStudy(root);
        var panel=Rect("ForestPortrait",root,8,24,135,258);
        var mask=Rect("InteriorMask",panel,12,12,111,234);mask.anchorMin=Vector2.zero;mask.anchorMax=Vector2.one;
        mask.offsetMin=Vector2.one*12;mask.offsetMax=-mask.offsetMin;mask.gameObject.AddComponent<RectMask2D>();
        var backing=Img("OverfilledBacking",mask,Load(Art+"Portrait_Inset.png"),0,0);
        backing.type=Image.Type.Tiled;backing.pixelsPerUnitMultiplier=1;
        backing.rectTransform.anchorMin=Vector2.zero;backing.rectTransform.anchorMax=Vector2.one;
        backing.rectTransform.offsetMin=-Vector2.one*32;backing.rectTransform.offsetMax=Vector2.one*32;
        // Art-only contents use current production sprites and config without changing owners or assets.
        var contents=Rect("PortraitContents",mask,-12,-12,135,258);
        Img("Antler pennant candidate",contents,Load(Art+"Portrait_AntlerPennant.png"),53,15);
        var config=AssetDatabase.LoadAssetAtPath<EnemyWeaknessIndicatorConfig>("Assets/_Game/Resources/UI/EnemyWeaknessIndicatorConfig.asset");
        var serialized=new SerializedObject(config);
        var gem=(Sprite)serialized.FindProperty("emeraldSprite").objectReferenceValue;
        Assert.That(gem.rect.size,Is.EqualTo(new Vector2(32,32)));
        Img("Existing affinity gem",contents,gem,51,78);
        Img("Rattlebones unchanged",contents,Ready("Rattlebones"),35,124);
        var hp=Rect("Existing player health presentation",contents,15,211,104,24);
        var value=Rect("PlayerHPBarFill",hp,0,0,104,24).gameObject.AddComponent<Image>();
        value.sprite=Load("Assets/_Game/Resources/UI/Finalized/HealthFill.png");value.type=Image.Type.Filled;value.fillMethod=Image.FillMethod.Horizontal;value.fillAmount=1;value.raycastTarget=false;
        var label=GameUi.Label("HealthText",hp,"100 / 100",new Vector2(76,24),new Vector2(8,0),10);
        label.alignment=TextAlignmentOptions.Center;label.textWrappingMode=TextWrappingModes.NoWrap;
        label.overflowMode=TextOverflowModes.Truncate;PixelTextFitter.ApplyNativeMenu(label,10);
        label.gameObject.AddComponent<DungeonPreviewPixelText>().Initialize(root);
        var health = hp.gameObject.AddComponent<ModularHealthBarUI>();
        var healthStyle = AssetDatabase.LoadAssetAtPath<ModularHealthBarStyle>("Assets/_Game/Resources/UI/Finalized/Health/Player.asset");
        Assert.That(healthStyle, Is.Not.Null, "Existing player health style");
        Assert.That(healthStyle.HasCompleteFrame, Is.True, "Existing player health frame references");
        var healthData = new SerializedObject(health);
        healthData.FindProperty("styleOverride").objectReferenceValue = healthStyle;
        healthData.ApplyModifiedPropertiesWithoutUndo();
        var frame=Rect("ModularFrame",panel,0,0,135,258);frame.anchorMin=Vector2.zero;frame.anchorMax=Vector2.one;frame.offsetMin=frame.offsetMax=Vector2.zero;
        string[] names={"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner","TopEdge","BottomEdge","LeftEdge","RightEdge"};
        string[] files={"Corner_TL","Corner_TR","Corner_BL","Corner_BR","Rail_Top","Rail_Bottom","Rail_Left","Rail_Right"};
        for(int n=0;n<8;n++)Img(names[n],frame,Load(Art+"Portrait_"+files[n]+".png"),0,0);
        frame.gameObject.AddComponent<ResponsiveModularFrameFitter>().ConfigureNativePieces(Load(Art+"Portrait_Clamp.png"),58);
        Img("Upper ivy accent",panel,Load(Art+"Portrait_Leaves_Upper.png"),14,-3);
        Img("Lower ivy accent",panel,Load(Art+"Portrait_Leaves_Lower.png"),122,230);
        EditorSceneManager.SaveScene(scene,ScenePath);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
    }
    static void SetSize(int s)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(Width*s,Height*s)});
    static void CheckShell(ResponsiveModularFrameFitter f)
    {
        f.RefreshFrame();var panel=(RectTransform)f.transform.parent;
        const float center=7,halfRail=4;
        foreach(var i in f.GetComponentsInChildren<Image>())
        {
            var r=i.rectTransform;Assert.That(i.enabled,Is.True);Assert.That(r.localScale,Is.EqualTo(Vector3.one));
            Assert.That(r.anchoredPosition.x,Is.EqualTo(Mathf.Round(r.anchoredPosition.x)).Within(.001));
            Assert.That(r.anchoredPosition.y,Is.EqualTo(Mathf.Round(r.anchoredPosition.y)).Within(.001));
            if(i.type==Image.Type.Simple)Assert.That(r.rect.size,Is.EqualTo(i.sprite.rect.size));
            else Assert.That(i.pixelsPerUnit*i.pixelsPerUnitMultiplier,Is.EqualTo(1).Within(.001));
        }
        var l=(RectTransform)f.transform.Find("LeftEdge");var rgt=(RectTransform)f.transform.Find("RightEdge");
        var top=(RectTransform)f.transform.Find("TopEdge");var bottom=(RectTransform)f.transform.Find("BottomEdge");
        Assert.That(l.anchoredPosition.x+halfRail,Is.EqualTo(center));
        Assert.That(rgt.anchoredPosition.x+halfRail,Is.EqualTo(panel.rect.width-1-center));
        Assert.That(-top.anchoredPosition.y+halfRail,Is.EqualTo(center));
        Assert.That(-bottom.anchoredPosition.y+halfRail,Is.EqualTo(panel.rect.height-1-center));
        foreach(RectTransform j in f.transform.Find("NativeConnectors"))if(j.gameObject.activeSelf)
        {
            var c=j.anchoredPosition+new Vector2(3,-3);
            Assert.That(Mathf.Approximately(c.x,center)||Mathf.Approximately(c.x,panel.rect.width-1-center)||Mathf.Approximately(-c.y,center)||Mathf.Approximately(-c.y,panel.rect.height-1-center),Is.True);
        }
    }
    static IEnumerator Capture(string name)
    {
        var p=Path.GetFullPath(Root+"/Previews/"+name+".png");if(File.Exists(p))File.Delete(p);
        ScreenCapture.CaptureScreenshot(p);float until=Time.realtimeSinceStartup+20;
        while(!File.Exists(p)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(until));yield return null;}
    }
    [UnityTearDown] public IEnumerator Cleanup(){if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit("Forest portrait shell with existing gem/HP art, responsive joins and clipping evidence.")]
    public IEnumerator PortraitNativeScreens()
    {
        Directory.CreateDirectory(Root+"/Diagnostics");Directory.CreateDirectory(Root+"/Previews");
        Build();SetSize(1);yield return new EnterPlayMode();yield return null;yield return null;
        var panel=(RectTransform)GameObject.Find("ForestPortrait").transform;
        var mask=(RectTransform)panel.Find("InteriorMask");var backing=mask.Find("OverfilledBacking").GetComponent<Image>();
        var f=panel.Find("ModularFrame").GetComponent<ResponsiveModularFrameFitter>();var original=panel.sizeDelta;
        foreach(var size in new[]{new Vector2(30,30),new Vector2(128,224),new Vector2(135,258),new Vector2(163,287),new Vector2(129,257)})
        {
            panel.sizeDelta=size;Canvas.ForceUpdateCanvases();CheckShell(f);
            Assert.That(mask.rect.size,Is.EqualTo(size-Vector2.one*24));
            Assert.That(backing.rectTransform.rect.size,Is.EqualTo(mask.rect.size+Vector2.one*64));
        }
        panel.sizeDelta=new Vector2(29,29);f.RefreshFrame();Assert.That(f.GetComponentsInChildren<Image>().All(i=>!i.enabled),Is.True);
        panel.sizeDelta=original;Canvas.ForceUpdateCanvases();CheckShell(f);yield return null;yield return null;
        foreach(int scale in new[]{1,2,3})
        {
            SetSize(scale);float until=Time.realtimeSinceStartup+20;
            while(Screen.width!=Width*scale||Screen.height!=Height*scale){Assert.That(Time.realtimeSinceStartup,Is.LessThan(until));yield return null;}
            Canvas.ForceUpdateCanvases();yield return null;yield return null;
            foreach(var i in Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.enabled))
            {
                Assert.That(i.sprite,Is.Not.Null,i.name);Assert.That(i.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point),i.name);
                var r=GameplayPixelLayoutController.ScreenRect(i.rectTransform);
                Assert.That(r.x,Is.EqualTo(Mathf.Round(r.x)).Within(.02),i.name);
                Assert.That(r.y,Is.EqualTo(Mathf.Round(r.y)).Within(.02),i.name);
                if(i.type==Image.Type.Simple)
                {
                    Assert.That(r.width,Is.EqualTo(i.sprite.rect.width*scale).Within(.02),i.name);
                    Assert.That(r.height,Is.EqualTo(i.sprite.rect.height*scale).Within(.02),i.name);
                }
            }
            var text=panel.GetComponentInChildren<TMP_Text>();text.ForceMeshUpdate();Assert.That(text.isTextTruncated,Is.False);
            var bar=panel.GetComponentInChildren<ModularHealthBarUI>();Assert.That(bar,Is.Not.Null);
            var generatedBar = bar.transform.Find("GeneratedModularHealthBar");
            Assert.That(generatedBar, Is.Not.Null, "Existing HP component must build its full native frame");
            Assert.That(generatedBar.GetComponent<RectTransform>().rect.width,Is.EqualTo(104));
            Assert.That(bar.transform.Find("PlayerHPBarFill").GetComponent<Image>().enabled, Is.False,
                "Legacy value source must not cover the existing native HP frame");
            var evidence=new CaptureEvidence{width=Screen.width,height=Screen.height,portrait=GameplayPixelLayoutController.ScreenRect(panel),mask=GameplayPixelLayoutController.ScreenRect(mask),
                images=Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Length,tiledMask=true,integerGeometry=true};
            File.WriteAllText(Root+"/Diagnostics/Unity-"+Screen.width+"x"+Screen.height+".json",JsonUtility.ToJson(evidence,true));
            yield return Capture("Unity-portrait-"+Screen.width+"x"+Screen.height);
            if(scale==1){backing.color=new Color(1,0,1,1);yield return null;yield return null;yield return Capture("Unity-mask-probe");backing.color=Color.white;yield return null;}
        }
        string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
        foreach (var component in Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if (component != null)
                Assert.That(Array.IndexOf(forbidden, component.GetType().Name), Is.EqualTo(-1), component.name);
        yield return new ExitPlayMode();
    }
}
