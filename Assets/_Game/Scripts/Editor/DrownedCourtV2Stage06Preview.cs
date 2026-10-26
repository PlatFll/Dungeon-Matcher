using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Additive static art review. Existing scene objects and all production bindings remain untouched.
public static class DrownedCourtV2Stage06Preview
{
    const string Root="ArtSource/DrownedCourtEnvironmentV2";
    const string Art="Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage06";
    const string Scene="Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage06.unity";
    static int captures;
    [MenuItem("Dungeon Matcher/Art/Drowned Court V2/Build Stage 06 Preview")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);captures=0;
        var data=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Stage06/Manifest.json"));Import(data);
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage05.unity");
        var canvas=Object.FindFirstObjectByType<Canvas>();var camera=Object.FindFirstObjectByType<Camera>();
        var actors=canvas.transform.Find("Stage01_ReadOnlyActorContrast");
        var board=(RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement");
        var oldSize=board.rect.size;var oldPosition=board.anchoredPosition;
        var root=Rect(canvas.transform,"Stage06_SideArchitecture",new Rect(0,0,360,640));root.localScale=new Vector3(2,2,1);
        root.SetSiblingIndex(canvas.transform.Find("Stage05_BoardReview").GetSiblingIndex());
        foreach(var p in data.placements)
        {
            var r=Rect(root,p.name+"_"+p.x+"_"+p.y,new Rect(p.x,640-p.y-p.height,p.width,p.height));
            var im=r.gameObject.AddComponent<Image>();im.sprite=Sprite(p.name);im.raycastTarget=false;
            Assert.That(im.sprite.rect.size,Is.EqualTo(r.rect.size));Assert.That(r.localScale,Is.EqualTo(Vector3.one));
            Assert.That(r.localRotation,Is.EqualTo(Quaternion.identity));
        }
        Assert.That(root.childCount,Is.EqualTo(data.placements.Length));Assert.That(root.childCount,Is.EqualTo(34));
        Assert.That(board.rect.size,Is.EqualTo(oldSize));Assert.That(board.anchoredPosition,Is.EqualTo(oldPosition));
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Scene),Is.False);
        Canvas.ForceUpdateCanvases();Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Scene),Is.True);
        Render(camera,720,1280,"Unity-Layers-720x1280.png");actors.gameObject.SetActive(true);
        Capture(camera,canvas,root,1280);
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
        camera.orthographicSize=800;camera.transform.position=new Vector3(360,800,-10);Capture(camera,canvas,root,1600);
        File.WriteAllText(Root+"/QA/Stage06/UnityPreview.json",JsonUtility.ToJson(new Result{status="PASS",captures=captures,importedAssets=data.assets.Length,placedModules=data.placements.Length,importedOpaquePixelsAndAlphaExact=true,sceneExcludedFromBuild=true,boardPosition=oldPosition,boardSize=oldSize,note="Static art-only Unity renders; uniform native 1/2/3 scale. All prior scene objects preserved; Stage06 uses one additive parent behind the existing board frame/cells. No gameplay, production integration, animation or device verification."},true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.OpenScene(Scene);
        Debug.Log("DROWNED_COURT_STAGE06_PREVIEW_PASS");
    }
    static void Capture(Camera camera,Canvas canvas,RectTransform root,int height)
    {
        foreach(int w in new[]{360,720,1080})
        {
            var dimensions=w+"x"+(height*w/720);
            root.gameObject.SetActive(false);Render(camera,w,height*w/720,"Unity-Before-"+dimensions+".png");
            root.gameObject.SetActive(true);Render(camera,w,height*w/720,"Unity-Actors-"+dimensions+".png");
            var active=canvas.transform.Cast<Transform>().Where(t=>t!=root&&t.gameObject.activeSelf).ToArray();
            foreach(var t in active)t.gameObject.SetActive(false);var prior=camera.backgroundColor;camera.backgroundColor=Color.clear;
            Render(camera,w,height*w/720,"Unity-SidesOnly-"+dimensions+".png");
            camera.backgroundColor=prior;foreach(var t in active)t.gameObject.SetActive(true);
        }
    }
    static void Import(Manifest data)
    {
        Directory.CreateDirectory(Art);
        foreach(var a in data.assets)
        {
            string path=Art+"/"+a.name+".png";File.Copy(Root+"/Exports/Stage06/"+a.name+".png",path,true);
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
        File.WriteAllBytes(Root+"/Preview/Stage06/"+filename,png.EncodeToPNG());captures++;
        RenderTexture.active=prior;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(png);
    }
    [Serializable] class Manifest {public Asset[] assets;public Placement[] placements;}
    [Serializable] class Asset {public string name;public int width,height;}
    [Serializable] class Placement {public string name;public int x,y,width,height;}
    [Serializable] class Result {public string status,note;public int importedAssets,captures,placedModules;public bool importedOpaquePixelsAndAlphaExact,sceneExcludedFromBuild;public Vector2 boardPosition,boardSize;}
}
