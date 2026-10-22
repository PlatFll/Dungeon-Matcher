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
using Object = UnityEngine.Object;

/// <summary>Editor-only Stage 03 UI studies. No production scene or gameplay ownership.</summary>
public sealed class DungeonHybridStage03Tests
{
    const string Root="ArtSource/DungeonHybrid/Stage03";
    const string NewArt="Assets/_Game/Art/DungeonHybrid/Stage03/";
    const string OldArt="Assets/_Game/Art/DungeonPixelLab/";
    static string Scene(bool top)=>"Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage03_"+(top?"Top":"BoardHud")+".unity";
    static Sprite Load(string path){var s=AssetDatabase.LoadAssetAtPath<Sprite>(path);Assert.That(s,Is.Not.Null,path);return s;}
    static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
        r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
    }
    static Image Img(string name,Transform parent,Sprite sprite,float x,float y)
    {
        var r=Rect(name,parent,x,y,sprite.rect.width,sprite.rect.height);var i=r.gameObject.AddComponent<Image>();
        i.sprite=sprite;i.raycastTarget=false;i.color=Color.white;return i;
    }
    static void Label(RectTransform panel,string text,int size)
    {
        var label=GameUi.Label("LiveLabel",panel,text,new Vector2(panel.rect.width-16,panel.rect.height),Vector2.zero,size);
        label.alignment=TextAlignmentOptions.CenterGeoAligned;label.textWrappingMode=TextWrappingModes.NoWrap;
        label.overflowMode=TextOverflowModes.Truncate;PixelTextFitter.ApplyNativeMenu(label,size);
        label.gameObject.AddComponent<DungeonPreviewPixelText>().Initialize((RectTransform)panel.GetComponentInParent<Canvas>().transform);
    }
    static RectTransform Panel(Transform parent,string name,string family,string directory,int x,int y,int w,int h,int inset,int pitch,bool backing=true)
    {
        var p=Rect(name,parent,x,y,w,h);
        var mask=Rect("InteriorMask",p,0,0,0,0);mask.anchorMin=Vector2.zero;mask.anchorMax=Vector2.one;
        mask.offsetMin=Vector2.one*inset;mask.offsetMax=-mask.offsetMin;mask.gameObject.AddComponent<RectMask2D>();
        if(backing){var i=Img("OverfilledBacking",mask,Load(directory+family+"_Inset.png"),0,0);var r=i.rectTransform;
            r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=-i.sprite.rect.size;r.offsetMax=i.sprite.rect.size;i.type=Image.Type.Tiled;}
        var frame=Rect("ModularFrame",p,0,0,0,0);frame.anchorMin=Vector2.zero;frame.anchorMax=Vector2.one;frame.offsetMin=frame.offsetMax=Vector2.zero;
        string[] names={"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner","TopEdge","BottomEdge","LeftEdge","RightEdge"};
        string[] suffix={"Corner_TL","Corner_TR","Corner_BL","Corner_BR","Rail_Top","Rail_Bottom","Rail_Left","Rail_Right"};
        for(int n=0;n<8;n++)Img(names[n],frame,Load(directory+family+"_"+suffix[n]+".png"),0,0);
        frame.gameObject.AddComponent<ResponsiveModularFrameFitter>().ConfigureNativePieces(Load(directory+family+"_Clamp.png"),pitch);
        return p;
    }
    static void Icon(RectTransform p,string path)
    {var s=Load(path);Img("NativeIcon",p,s,Mathf.Floor((p.rect.width-s.rect.width)/2),Mathf.Floor((p.rect.height-s.rect.height)/2));}
    static void Wings(RectTransform p,string prefix)
    {
        Img("Left ornament",p,Load(NewArt+prefix+"_Wing_Left.png"),-3,Mathf.Floor((p.rect.height-15)/2));
        Img("Right ornament",p,Load(NewArt+prefix+"_Wing_Right.png"),p.rect.width-4,Mathf.Floor((p.rect.height-15)/2));
    }
    static void Import()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(string file in Directory.GetFiles(NewArt,"*.png"))
        {
            var i=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;
            i.spritePixelsPerUnit=64;i.filterMode=FilterMode.Point;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;
            i.npotScale=TextureImporterNPOTScale.None;i.wrapMode=TextureWrapMode.Clamp;i.alphaIsTransparency=false;i.maxTextureSize=2048;
            var s=new TextureImporterSettings();i.ReadTextureSettings(s);s.spriteMeshType=SpriteMeshType.FullRect;i.SetTextureSettings(s);i.SaveAndReimport();
        }
    }
    [MenuItem("Dungeon Matcher/Art Preview/Hybrid Stage 03 top UI study")]
    public static void BuildTop()
    {
        Import();var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage02.unity",OpenSceneMode.Single);
        var root=(RectTransform)Object.FindFirstObjectByType<Canvas>().transform;
        // Canvas.referencePixelsPerUnit alone is not persisted on scene reload.
        // A serialized scaler keeps UGUI tiled images at one texel per native unit.
        var scaler=root.gameObject.AddComponent<CanvasScaler>();scaler.referencePixelsPerUnit=64;scaler.dynamicPixelsPerUnit=1;
        // Retain the approved recess, cast, gate, floor and structure from Stage02 byte-for-byte.
        Panel(root,"Portrait retained", "Portrait",OldArt+"PortraitFrame/",9,20,99,190,9,40,false);
        var wave=Panel(root,"Wave candidate","Wave",NewArt,125,12,103,32,6,1000);Label(wave,"WAVE 1",24);Wings(wave,"Wave");
        Img("Hanging skull",wave,Load(NewArt+"Wave_Skull.png"),44,28);
        var guide=Panel(root,"Guide candidate","Control",NewArt,245,18,64,31,6,1000);Label(guide,"GUIDE",16);
        var settings=Panel(root,"Settings candidate","Control",NewArt,320,18,31,31,6,1000);Icon(settings,OldArt+"TopPlaques/Plaque_Gear.png");
        EditorSceneManager.SaveScene(scene,Scene(true));
    }
    [MenuItem("Dungeon Matcher/Art Preview/Hybrid Stage 03 board and lower HUD study")]
    public static void BuildBoardHud()
    {
        Import();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        const int width=576,height=688;
        var camera=new GameObject("Native UI study camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=height/128f;
        camera.aspect=(float)width/height;camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color32(21,20,35,255);camera.allowHDR=false;camera.allowMSAA=false;
        var canvas=new GameObject("UI piece study - not final production layout",typeof(RectTransform)).AddComponent<Canvas>();
        canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;canvas.referencePixelsPerUnit=64;canvas.pixelPerfect=true;
        var scaler=canvas.gameObject.AddComponent<CanvasScaler>();scaler.referencePixelsPerUnit=64;scaler.dynamicPixelsPerUnit=1;
        var root=(RectTransform)canvas.transform;root.sizeDelta=new Vector2(width,height);root.pivot=Vector2.zero;
        root.position=new Vector3(-width/128f,-height/128f,0);root.localScale=Vector3.one/64;
        var board=Panel(root,"Board retained 64px cells","Board",OldArt+"BoardFrame/",19,10,538,538,13,80,false);
        var mask=board.Find("InteriorMask");var tile=Load(OldArt+"Stage16/Board_Cell64.png");Assert.That(tile.rect.size,Is.EqualTo(new Vector2(64,64)));
        string[] names={"Ruby","Topaz","Amethyst","Emerald","Amber_tri","Sapphire (1)"};
        for(int y=0;y<8;y++)for(int x=0;x<8;x++)
        {
            Img("Cell_"+x+"_"+y,mask,tile,x*64,y*64);
            var gem=Load("Assets/_Game/Art/Gems/"+names[(x+y*3)%6]+".png");Assert.That(gem.rect.size,Is.EqualTo(new Vector2(64,64)));
            Img("Gem native scale probe",mask,gem,x*64,y*64);
        }
        var res=Panel(root,"Resonance candidate","Resonance",NewArt,229,594,118,44,6,1000);Icon(res,"Assets/_Game/Resources/UI/GlobalMenu/CrownedSkull.png");Wings(res,"Resonance");
        // Source-derived center jewels remain separate from rail repetition.
        Img("Upper amethyst",res,Load(NewArt+"Resonance_Clamp.png"),55,1);Img("Lower amethyst",res,Load(NewArt+"Resonance_Clamp.png"),55,36);
        var potion=Panel(root,"Potion candidate","Control",NewArt,163,594,47,44,6,1000);Icon(potion,"Assets/_Game/Resources/UI/Consumables/Potion.png");
        var bomb=Panel(root,"Bomb candidate","Control",NewArt,366,594,47,44,6,1000);Icon(bomb,"Assets/_Game/Resources/UI/Consumables/Bomb.png");
        Panel(root,"Progress retained","Progress",OldArt+"BottomHud/",239,566,98,14,4,1000);
        var text=Rect("Caption",root,158,650,260,20);Label(text,"PHASE RESONANCE",16);
        EditorSceneManager.SaveScene(scene,Scene(false));
    }
    static void SetSize(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(w,h)});
    static void CheckFrame(ResponsiveModularFrameFitter fitter)
    {
        fitter.RefreshFrame();var frame=(RectTransform)fitter.transform;var p=(RectTransform)frame.parent;
        float c=frame.Find("TopLeftCorner").GetComponent<Image>().sprite.rect.width,t=frame.Find("TopEdge").GetComponent<Image>().sprite.rect.height;
        foreach(var i in frame.GetComponentsInChildren<Image>())
        {
            Assert.That(i.enabled,Is.True);var r=i.rectTransform;Assert.That(r.localScale,Is.EqualTo(Vector3.one));
            Assert.That(r.anchoredPosition.x,Is.EqualTo(Mathf.Round(r.anchoredPosition.x)).Within(.001f));
            Assert.That(r.anchoredPosition.y,Is.EqualTo(Mathf.Round(r.anchoredPosition.y)).Within(.001f));
            Assert.That(r.anchoredPosition.x,Is.GreaterThanOrEqualTo(0));Assert.That(-r.anchoredPosition.y,Is.GreaterThanOrEqualTo(0));
            Assert.That(r.anchoredPosition.x+r.rect.width,Is.LessThanOrEqualTo(p.rect.width));Assert.That(-r.anchoredPosition.y+r.rect.height,Is.LessThanOrEqualTo(p.rect.height));
            if(i.type==Image.Type.Simple)Assert.That(r.rect.size,Is.EqualTo(i.sprite.rect.size));
            else {Assert.That(i.type,Is.EqualTo(Image.Type.Tiled));Assert.That(i.pixelsPerUnit*i.pixelsPerUnitMultiplier,Is.EqualTo(1).Within(.001f));}
        }
        var left=(RectTransform)frame.Find("LeftEdge");var right=(RectTransform)frame.Find("RightEdge");
        Assert.That(left.anchoredPosition.x+(t-1)/2,Is.EqualTo((c-1)/2));
        Assert.That(right.anchoredPosition.x+(t-1)/2,Is.EqualTo(p.rect.width-1-(c-1)/2));
        foreach(RectTransform joint in frame.Find("NativeConnectors"))if(joint.gameObject.activeSelf)
        {var center=joint.anchoredPosition+new Vector2((joint.rect.width-1)/2,-(joint.rect.height-1)/2);float edge=(c-1)/2;
            Assert.That(Mathf.Approximately(center.x,edge)||Mathf.Approximately(center.x,p.rect.width-1-edge)||Mathf.Approximately(-center.y,edge)||Mathf.Approximately(-center.y,p.rect.height-1-edge),Is.True);}
    }
    static void CheckResizing()
    {
        foreach(var f in Object.FindObjectsByType<ResponsiveModularFrameFitter>(FindObjectsSortMode.None))
        {
            var p=(RectTransform)f.transform.parent;var original=p.sizeDelta;CheckFrame(f);
            float c=f.transform.Find("TopLeftCorner").GetComponent<Image>().sprite.rect.width;
            foreach(var size in new[]{new Vector2(c*2,c*2),new Vector2(105,79),new Vector2(164,104),new Vector2(311,215)}){p.sizeDelta=size;Canvas.ForceUpdateCanvases();CheckFrame(f);}
            p.sizeDelta=Vector2.one*(2*c-1);f.RefreshFrame();Assert.That(f.GetComponentsInChildren<Image>().All(i=>!i.enabled),Is.True);
            p.sizeDelta=original;Canvas.ForceUpdateCanvases();CheckFrame(f);
        }
    }
    [UnityTearDown]public IEnumerator Cleanup(){if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit("Builds the isolated top UI study and captures actual Unity Game View.")]
    public IEnumerator TopUIHeroPiecesNativeScreens()
    {
        BuildTop();SetSize(360,240);yield return new EnterPlayMode();
        yield return CaptureStudy(true);yield return new ExitPlayMode();
    }
    [UnityTest,Explicit("Builds the isolated board/lower-HUD study and captures actual Unity Game View.")]
    public IEnumerator BoardUIHeroPiecesNativeScreens()
    {
        BuildBoardHud();SetSize(576,688);yield return new EnterPlayMode();
        yield return CaptureStudy(false);yield return new ExitPlayMode();
    }
    static IEnumerator CaptureStudy(bool top)
    {
        {
            int w=top?360:576,h=top?240:688;
            Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Scene(top)),Is.False);
            CheckResizing();
            foreach(int scale in new[]{1,2})
            {
                SetSize(w*scale,h*scale);yield return new WaitForSecondsRealtime(.4f);Canvas.ForceUpdateCanvases();yield return null;yield return null;
                Assert.That(Screen.width,Is.EqualTo(w*scale));Assert.That(Screen.height,Is.EqualTo(h*scale));
                foreach(var i in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
                {
                    Assert.That(i.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(i.color,Is.EqualTo(Color.white));Assert.That(i.raycastTarget,Is.False);
                    var r=GameplayPixelLayoutController.ScreenRect(i.rectTransform);Assert.That(r.x,Is.EqualTo(Mathf.Round(r.x)).Within(.01f));Assert.That(r.y,Is.EqualTo(Mathf.Round(r.y)).Within(.01f));
                    if(i.type==Image.Type.Simple){Assert.That(r.width,Is.EqualTo(i.sprite.rect.width*scale).Within(.01f));Assert.That(r.height,Is.EqualTo(i.sprite.rect.height*scale).Within(.01f));}
                    if(!i.GetComponentInParent<RectMask2D>()){Assert.That(r.xMin,Is.GreaterThanOrEqualTo(-.01f));Assert.That(r.yMin,Is.GreaterThanOrEqualTo(-.01f));Assert.That(r.xMax,Is.LessThanOrEqualTo(Screen.width+.01f));Assert.That(r.yMax,Is.LessThanOrEqualTo(Screen.height+.01f));}
                }
                foreach(var text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)){text.ForceMeshUpdate();Assert.That(text.isTextTruncated,Is.False,text.text);}
                string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
                Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
                string file=Path.GetFullPath(Root+"/Previews/Unity-"+(top?"Top-":"BoardHud-")+Screen.width+"x"+Screen.height+".png");
                if(File.Exists(file))File.Delete(file);ScreenCapture.CaptureScreenshot(file);float deadline=Time.realtimeSinceStartup+20;
                while(!File.Exists(file)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
            }
        }
    }
}
