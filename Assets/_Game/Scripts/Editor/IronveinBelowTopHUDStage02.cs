using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Preview-only separator assembly. The approved Stage 01 and production scenes are never saved.
public static class IronveinBelowTopHUDStage02
{
    const string Root="ArtSource/IronveinBelowTopHUD/Stage02";
    const string Art="Assets/_Game/Art/IronveinBelowTopHUD/Stage02";
    const string Source="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage01.unity";
    const string Target="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage02.unity";
    const string Group="Stage02_Separator_PreviewOnly";
    static Canvas canvas;
    static Camera camera;
    static RectTransform native;
    static Rect allowed;

    [MenuItem("Dungeon Matcher/Art/Ironvein Below Top HUD/Stage 02 Separator")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);
        Directory.CreateDirectory(Root+"/Preview");Directory.CreateDirectory(Root+"/QA");Import();
        EditorSceneManager.OpenScene(Source);Bind();
        int[] widths={360,720,1080};var before=widths.Select(w=>Capture(w,"Before")).ToArray();
        var originalGeometry=Geometry();
        Rect boardBefore=WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement"));
        Rect portraitBefore=WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask"));
        if(!File.Exists(Target))Assert.That(AssetDatabase.CopyAsset(Source,Target),Is.True);
        EditorSceneManager.OpenScene(Target);Bind();
        var prior=canvas.transform.Find(Group);if(prior)Object.DestroyImmediate(prior.gameObject);
        // Use the established lower boundary and the actual board opening, never move either to fit art.
        Rect limit=WorldRect((RectTransform)canvas.transform.Find("LOWER_ONLY_StageBoundary_NoArtwork"));
        Rect board=WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement"));
        float top=(1280-limit.yMax)/2;
        float left=Mathf.Round(board.xMin/2)+3,right=Mathf.Round(board.xMax/2)-3;
        float center=Mathf.Round(board.center.x/2);
        Assert.That(top+18,Is.LessThan((1280-board.yMax)/2),"The entire socket must clear the unchanged board opening.");
        var root=Rect(canvas.transform,Group,limit);root.gameObject.AddComponent<RectMask2D>();
        native=Rect(root,"Native2x_ModularLedge",new Rect(0,0,360,limit.height/2));native.localScale=new Vector3(2,2,1);
        Tile("RepeatLeft",left+6,top+4,center-20-(left+6));
        Tile("RepeatRight",center+20,top+4,right-6-(center+20));
        Stamp("BeamCenter",center-20,top+3);
        Stamp("EndTransition",left,top+3);
        Stamp("EndTransitionRight",right-10,top+3);
        Stamp("CrystalNode",center-9,top);
        allowed=new Rect(left,top,right-left,18);
        Canvas.ForceUpdateCanvases();
        CollectionAssert.AreEqual(originalGeometry,Geometry(),"Existing graphics and shared UI geometry must remain unchanged.");
        Assert.That(WorldRect((RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement")),Is.EqualTo(boardBefore));
        Assert.That(WorldRect((RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask")),Is.EqualTo(portraitBefore));
        foreach(var im in native.GetComponentsInChildren<Image>()){
            Assert.That(im.raycastTarget,Is.False);Assert.That(im.color,Is.EqualTo(Color.white));
            Assert.That(im.transform.lossyScale,Is.EqualTo(new Vector3(2,2,1)));
            if(im.type==Image.Type.Simple)Assert.That(im.rectTransform.rect.size,Is.EqualTo(im.sprite.rect.size));
        }
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Target),Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);
        var changed=new int[widths.Length];
        for(int n=0;n<widths.Length;n++){
            var after=Capture(widths[n],"Unity");int w=widths[n],h=w*1280/720;
            for(int i=0;i<after.Length;i++)if(!after[i].Equals(before[n][i])){
                float x=(i%w+.5f)*360/w,y=(h-1-i/w+.5f)*360/w;
                Assert.That(allowed.Contains(new Vector2(x,y)),Is.True,"Changed pixel outside separator: "+w+" "+x+","+y);
                changed[n]++;
            }
            Assert.That(changed[n],Is.GreaterThan(0));
        }
        File.WriteAllText(Root+"/QA/Unity.json",JsonUtility.ToJson(new Report{status="PASS",scene=Target,
            protectedPixelsUnchanged=true,geometryUnchanged=true,sceneExcludedFromBuild=true,
            nativeSeparatorBounds=allowed,boardBounds=boardBefore,portraitBounds=portraitBefore,
            widths=widths,changedPixels=changed,importedAssets=5,
            note="Actual Unity art-only renders. Every pixel outside the separator rectangle matches Stage 01 at all three sizes. No production integration or phone test."},true));
        Debug.Log("IRONVEIN_LOWER_STAGE02_PASS assets=5 locked_pixels=exact widths=360,720,1080");
    }
    static string[] Geometry()=>canvas.GetComponentsInChildren<Graphic>(true).Where(g=>!PathOf(g.transform).StartsWith(Group+"/")).Select(g=>PathOf(g.transform)+"|"+JsonUtility.ToJson(WorldRect(g.rectTransform))+"|"+g.color+"|"+(g is Image im?AssetDatabase.GetAssetPath(im.sprite):"text")).OrderBy(s=>s).ToArray();
    static string PathOf(Transform t){string p=t.name;while(t.parent&&t.parent!=canvas.transform){t=t.parent;p=t.name+"/"+p;}return p;}
    static void Bind(){canvas=Object.FindFirstObjectByType<Canvas>();camera=Object.FindFirstObjectByType<Camera>();Assert.That(canvas,Is.Not.Null);Assert.That(camera,Is.Not.Null);Canvas.ForceUpdateCanvases();}
    static Rect WorldRect(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return UnityEngine.Rect.MinMaxRect(c[0].x,c[0].y,c[2].x,c[2].y);}
    static RectTransform Rect(Transform parent,string name,Rect b){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=b.position;r.sizeDelta=b.size;return r;}
    static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/"+name+".png");
    static Image Piece(string objectName,string source,float x,float top,float width,float height){var im=Rect(native,objectName,new Rect(x,640-top-height,width,height)).gameObject.AddComponent<Image>();im.sprite=Sprite(source);Assert.That(im.sprite,Is.Not.Null,source);im.raycastTarget=false;return im;}
    static void Stamp(string name,float x,float y){var s=Sprite(name);Piece(name,name,x,y,s.rect.width,s.rect.height);}
    static void Tile(string name,float x,float y,float width){var im=Piece(name,"BeamRepeat",x,y,width,10);im.type=Image.Type.Tiled;im.pixelsPerUnitMultiplier=canvas.referencePixelsPerUnit/im.sprite.pixelsPerUnit;}
    static void Import()
    {
        Directory.CreateDirectory(Art);
        foreach(var source in Directory.GetFiles(Root+"/Exports","*.png")){
            string path=Art+"/"+Path.GetFileName(source);File.Copy(source,path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Single;ti.spritePixelsPerUnit=64;
            ti.filterMode=FilterMode.Point;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.mipmapEnabled=false;ti.npotScale=TextureImporterNPOTScale.None;ti.maxTextureSize=2048;
            ti.alphaSource=TextureImporterAlphaSource.FromInput;ti.alphaIsTransparency=true;ti.isReadable=true;ti.wrapMode=TextureWrapMode.Clamp;
            var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(settings);ti.SaveAndReimport();
            var expected=new Texture2D(2,2);expected.LoadImage(File.ReadAllBytes(source));var actual=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Assert.That(actual.width,Is.EqualTo(expected.width));Assert.That(actual.height,Is.EqualTo(expected.height));
            var a=actual.GetPixels32();var b=expected.GetPixels32();for(int i=0;i<a.Length;i++)Assert.That(a[i].a==b[i].a&&(b[i].a==0||a[i].Equals(b[i])),Is.True,path+" pixel "+i);
            Object.DestroyImmediate(expected);
        }
    }
    static Color32[] Capture(int w,string prefix){int h=w*1280/720;Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);rt.filterMode=FilterMode.Point;rt.antiAliasing=1;
        var prior=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var png=new Texture2D(w,h,TextureFormat.RGBA32,false);png.ReadPixels(new Rect(0,0,w,h),0,0);png.Apply();
        File.WriteAllBytes(Root+"/Preview/"+prefix+"-"+w+"x"+h+".png",png.EncodeToPNG());var pixels=png.GetPixels32();RenderTexture.active=prior;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(png);return pixels;}
    [Serializable]class Report{public string status,scene,note;public bool protectedPixelsUnchanged,geometryUnchanged,sceneExcludedFromBuild;public Rect nativeSeparatorBounds,boardBounds,portraitBounds;public int[] widths,changedPixels;public int importedAssets;}
}
