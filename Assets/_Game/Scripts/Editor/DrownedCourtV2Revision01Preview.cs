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

// Isolated visual revision: exact background recolors and connected board-side modules only.
public static class DrownedCourtV2Revision01Preview
{
    const string Root="ArtSource/DrownedCourtEnvironmentV2";
    const string Art="Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01";
    const string Scene="Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision01.unity";
    static int captures;
    static Manifest data;
    static Canvas canvas;
    static RectTransform sides;
    static readonly Dictionary<Image,Sprite> originalSprites=new Dictionary<Image,Sprite>();
    static readonly Dictionary<Image,Color> originalColors=new Dictionary<Image,Color>();
    [MenuItem("Dungeon Matcher/Art/Drowned Court V2/Build Focused Revision 01")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);captures=0;originalSprites.Clear();originalColors.Clear();
        data=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Revision01/Manifest.json"));Import(data);
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage08.unity");
        canvas=Object.FindFirstObjectByType<Canvas>();var camera=Object.FindFirstObjectByType<Camera>();
        var actors=canvas.transform.Find("Stage01_ReadOnlyActorContrast");
        var board=(RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement");
        var oldSize=board.rect.size;var oldPosition=board.anchoredPosition;
        var oldRects=canvas.GetComponentsInChildren<RectTransform>(true).ToDictionary(t=>t,t=>JsonUtility.ToJson(new BoundsRecord{anchor=t.anchoredPosition,size=t.sizeDelta,scale=t.localScale,rotation=t.localRotation,amin=t.anchorMin,amax=t.anchorMax,pivot=t.pivot}));
        sides=Rect(canvas.transform,"Revision01_IntegratedSides",new Rect(0,0,360,640));sides.localScale=new Vector3(2,2,1);
        sides.SetSiblingIndex(canvas.transform.Find("Stage05_BoardReview").GetSiblingIndex());Populate(sides,data.placements,640);
        Apply(true);
        foreach(var pair in oldRects)Assert.That(JsonUtility.ToJson(new BoundsRecord{anchor=pair.Key.anchoredPosition,size=pair.Key.sizeDelta,scale=pair.Key.localScale,rotation=pair.Key.localRotation,amin=pair.Key.anchorMin,amax=pair.Key.anchorMax,pivot=pair.Key.pivot}),Is.EqualTo(pair.Value),pair.Key.name+" layout changed");
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Scene),Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Scene),Is.True);
        actors.gameObject.SetActive(true);Capture(camera,1280);
        ((RectTransform)canvas.transform).sizeDelta=new Vector2(720,1600);
        foreach(Transform t in canvas.transform)
        {
            var r=t as RectTransform;if(!r)continue;
            if(t.name=="BlankOceanExtent")r.sizeDelta=new Vector2(720,1600);
            else if(t.name=="BlankLowerSurroundings")r.sizeDelta=new Vector2(712,240);
            else if(t.name=="Stage01_SeparateBackgroundLayers"||t.name=="Stage03_Platform_Candidates"||t.name=="Stage02_Architecture_Candidates"||t.name.StartsWith("PortraitInterior")||t.name=="ExistingBattleFloorGuide"||t.name=="Stage01_ReadOnlyActorContrast"||t.name.StartsWith("Stage04_"))r.anchoredPosition+=Vector2.up*320;
            else if(t.name.StartsWith("GridGuide")||t.name.StartsWith("PlayableBoardBounds")||t.name=="Stage05_BoardReview"||t.name=="Stage06_SideArchitecture")r.anchoredPosition+=Vector2.up*194;
            else if(t.name.StartsWith("UnchangedSharedUIShells"))foreach(RectTransform child in t)if(child.anchoredPosition.y>796)child.anchoredPosition+=Vector2.up*320;
        }
        foreach(var f in canvas.GetComponentsInChildren<ResponsiveModularFrameFitter>())f.RefreshFrame();
        // Reuse the prior tall-device mapping unchanged, including reef and old-side registration.
        Apply(false);
        var reef=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Stage07/Manifest.json"));
        Populate((RectTransform)canvas.transform.Find("Stage07_LowerReef"),reef.placementsTall,800,"Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage07");
        var detail=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Stage08/Manifest.json"));
        Populate((RectTransform)canvas.transform.Find("Stage08_MarineDetails"),detail.placementsTall,800,"Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage08");
        originalSprites.Clear();originalColors.Clear();Populate(sides,data.placementsTall,800);
        camera.orthographicSize=800;camera.transform.position=new Vector3(360,800,-10);Capture(camera,1600);
        File.WriteAllText(Root+"/QA/Revision01/UnityPreview.json",JsonUtility.ToJson(new Result{status="PASS",captures=captures,importedAssets=data.assets.Length,placedModules=data.placements.Length,importedOpaquePixelsAndAlphaExact=true,sceneExcludedFromBuild=true,boardPosition=oldPosition,boardSize=oldSize,note="Static art-only Unity renders at exact 1/2/3 scale. Existing object transforms preserved. Exact background recolor copies plus revised side modules only. Not gameplay/device testing; no production asset replacement."},true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.OpenScene(Scene);
        Debug.Log("DROWNED_COURT_REVISION01_PREVIEW_PASS");
    }
    static bool IsSide(Image im) {for(Transform t=im.transform;t&&t!=canvas.transform;t=t.parent)if(t.name=="Stage06_SideArchitecture"||t.name=="Stage08_MarineDetails"||t.name=="Revision01_IntegratedSides")return true;return false;}
    static bool IsBackground(Image im) {return originalSprites.ContainsKey(im)||originalColors.ContainsKey(im)||IsSide(im);}
    static void Apply(bool revised)
    {
        // Cache only explicitly authorized environment bindings. Actors, frames and controls never enter this map.
        foreach(var im in canvas.GetComponentsInChildren<Image>(true))
        {
            if(im.sprite&&!originalSprites.ContainsKey(im))
            {
                var path=AssetDatabase.GetAssetPath(im.sprite);
                if(data.recolors.Any(r=>r.oldAsset==path))originalSprites[im]=im.sprite;
            }
            if((im.name=="BlankOceanExtent"||im.name=="BlankLowerSurroundings")&&!originalColors.ContainsKey(im))originalColors[im]=im.color;
        }
        foreach(var p in originalSprites.Where(p=>p.Key).ToArray())
        {
            var job=data.recolors.First(r=>r.oldAsset==AssetDatabase.GetAssetPath(p.Value));
            p.Key.sprite=revised?Sprite(job.name):p.Value;
        }
        foreach(var p in originalColors.Where(p=>p.Key).ToArray())p.Key.color=revised?new Color32(4,37,51,255):p.Value;
        canvas.transform.Find("Stage06_SideArchitecture").gameObject.SetActive(!revised);
        canvas.transform.Find("Stage08_MarineDetails").gameObject.SetActive(!revised);
        sides.gameObject.SetActive(revised);
    }
    static void Protected(Camera camera,int w,int h,string name)
    {
        var hidden=canvas.GetComponentsInChildren<Image>().Where(im=>im.enabled&&IsBackground(im)).ToArray();
        foreach(var im in hidden)im.enabled=false;
        var prior=camera.backgroundColor;camera.backgroundColor=Color.clear;Render(camera,w,h,name);camera.backgroundColor=prior;
        foreach(var im in hidden)im.enabled=true;
    }
    static void Capture(Camera camera,int height)
    {
        foreach(int w in new[]{360,720,1080})
        {
            var dim=w+"x"+(height*w/720);Apply(false);
            Render(camera,w,height*w/720,"Unity-Before-"+dim+".png");Protected(camera,w,height*w/720,"Protected-Before-"+dim+".png");
            Apply(true);Render(camera,w,height*w/720,"Unity-Revised-"+dim+".png");Protected(camera,w,height*w/720,"Protected-Revised-"+dim+".png");
            var hide=canvas.transform.Cast<Transform>().Where(t=>t!=sides&&t.gameObject.activeSelf).ToArray();foreach(var t in hide)t.gameObject.SetActive(false);
            var prior=camera.backgroundColor;camera.backgroundColor=Color.clear;Render(camera,w,height*w/720,"Unity-SidesOnly-"+dim+".png");camera.backgroundColor=prior;foreach(var t in hide)t.gameObject.SetActive(true);
        }
    }
    static void Populate(RectTransform root,Placement[] placements,int nativeHeight,string artPath=Art)
    {
        for(int i=root.childCount-1;i>=0;i--)Object.DestroyImmediate(root.GetChild(i).gameObject);
        root.sizeDelta=new Vector2(360,nativeHeight);
        foreach(var p in placements)
        {
            var r=Rect(root,p.name+"_"+p.x+"_"+p.y,new Rect(p.x,nativeHeight-p.y-p.height,p.width,p.height));
            var im=r.gameObject.AddComponent<Image>();im.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(artPath+"/"+p.name+".png");Assert.That(im.sprite,Is.Not.Null);im.raycastTarget=false;
            Assert.That(im.sprite.rect.size,Is.EqualTo(r.rect.size));Assert.That(r.localScale,Is.EqualTo(Vector3.one));
            Assert.That(r.localRotation,Is.EqualTo(Quaternion.identity));
        }
    }
    static void Import(Manifest data)
    {
        Directory.CreateDirectory(Art);
        foreach(var a in data.assets)
        {
            string path=Art+"/"+a.name+".png";File.Copy(Root+"/Exports/Revision01/"+a.name+".png",path,true);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Single;
            ti.spritePixelsPerUnit=64;ti.filterMode=FilterMode.Point;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.mipmapEnabled=false;
            ti.npotScale=TextureImporterNPOTScale.None;ti.maxTextureSize=2048;ti.alphaSource=TextureImporterAlphaSource.FromInput;ti.alphaIsTransparency=true;
            ti.isReadable=true;ti.wrapMode=TextureWrapMode.Clamp;
            var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(settings);ti.SaveAndReimport();
            var original=new Texture2D(2,2);original.LoadImage(File.ReadAllBytes(path));var actual=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Assert.That(actual.width,Is.EqualTo(a.width));Assert.That(actual.height,Is.EqualTo(a.height));
            var expected=original.GetPixels32();var pixels=actual.GetPixels32();
            for(int i=0;i<pixels.Length;i++)Assert.That(pixels[i].a==expected[i].a&&(expected[i].a==0||pixels[i].Equals(expected[i])),Is.True,a.name+" pixel "+i);
            Object.DestroyImmediate(original);
        }
    }
    static Sprite Sprite(string name) { var s=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/"+name+".png");Assert.That(s,Is.Not.Null);return s; }
    static RectTransform Rect(Transform parent,string name,Rect b)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
        r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=b.position;r.sizeDelta=b.size;return r;
    }
    static void Render(Camera camera,int w,int h,string filename)
    {
        Canvas.ForceUpdateCanvases();
        var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);rt.filterMode=FilterMode.Point;rt.antiAliasing=1;
        var prior=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
        var png=new Texture2D(w,h,TextureFormat.RGBA32,false);png.ReadPixels(new Rect(0,0,w,h),0,0);png.Apply();
        File.WriteAllBytes(Root+"/Preview/Revision01/"+filename,png.EncodeToPNG());captures++;
        RenderTexture.active=prior;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(png);
    }
    [Serializable] class Manifest {public Asset[] assets;public Placement[] placements,placementsTall;public Recolor[] recolors;}
    [Serializable] class Recolor {public string name,oldAsset;}
    [Serializable] class Asset {public string name;public int width,height;}
    [Serializable] class Placement {public string name;public int x,y,width,height;}
    [Serializable] class BoundsRecord {public Vector2 anchor,size,amin,amax,pivot;public Vector3 scale;public Quaternion rotation;}
    [Serializable] class Result {public string status,note;public int importedAssets,captures,placedModules;public bool importedOpaquePixelsAndAlphaExact,sceneExcludedFromBuild;public Vector2 boardPosition,boardSize;}
}
