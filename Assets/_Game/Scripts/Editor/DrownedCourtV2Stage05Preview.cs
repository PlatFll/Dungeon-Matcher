using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Candidate-only scene: actual cell bindings/positions are copied from the read-only live snapshot.
// No gameplay controller or production asset is changed by this preview builder.
public static class DrownedCourtV2Stage05Preview
{
    const string Root="ArtSource/DrownedCourtEnvironmentV2";
    const string Art="Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage05";
    const string Scene="Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage05.unity";
    static int captures;
    [MenuItem("Dungeon Matcher/Art/Drowned Court V2/Build Stage 05 Preview")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);captures=0;
        var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Stage05/Manifest.json"));Import(manifest);
        var snapshot=JsonUtility.FromJson<DrownedCourtV2Stage05CellCapture.Snapshot>(File.ReadAllText(Root+"/QA/Stage05/CellSnapshot.json"));
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage04.unity");
        var canvas=Object.FindFirstObjectByType<Canvas>();var camera=Object.FindFirstObjectByType<Camera>();
        var actors=canvas.transform.Find("Stage01_ReadOnlyActorContrast");actors.gameObject.SetActive(true);
        Render(camera,720,1280,"Unity-Stage04-Before-720x1280.png");
        var board=(RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement");
        var originalPosition=board.anchoredPosition;var originalSize=board.rect.size;
        Assert.That((board.rect.size-snapshot.board.size).sqrMagnitude,Is.LessThan(.001f));
        Assert.That((board.anchoredPosition-snapshot.board.position).sqrMagnitude,Is.LessThan(.001f));
        foreach(Transform t in canvas.transform)if(t.name.StartsWith("GridGuide"))t.gameObject.SetActive(false);
        var root=Rect(canvas.transform,"Stage05_BoardReview",new Rect(0,0,720,1280));
        var surround=Rect(root,"FrameAndInset",new Rect(0,0,720,1280));
        // A two-reference-pixel tiled overlap lives underneath the cells. It closes raster gaps
        // caused by existing fractional board bounds without cropping or resizing any cell.
        var b=snapshot.board;float p=2;
        var strips=new[]{new Rect(b.x-p,b.y-p,b.width+2*p,2*p),new Rect(b.x-p,b.yMax-p,b.width+2*p,2*p),
            new Rect(b.x-p,b.y,2*p,b.height),new Rect(b.xMax-p,b.y,2*p,b.height)};
        for(int i=0;i<strips.Length;i++)
        {
            var r=Rect(surround,"UnderCellOverlap_"+i,strips[i]);var im=r.gameObject.AddComponent<Image>();
            im.sprite=Sprite("InnerShadow");im.type=Image.Type.Tiled;im.pixelsPerUnitMultiplier=.5f;im.raycastTarget=false;
        }
        var native=Rect(surround,"NativeScale2",new Rect(0,0,360,640));native.localScale=new Vector3(2,2,1);
        var bounds=Rect(native,"FrameBounds",new Rect(0,0,309,309));
        var modular=Rect(bounds,"ModularFrame",new Rect(0,0,309,309));modular.anchorMax=Vector2.one;modular.offsetMin=modular.offsetMax=Vector2.zero;
        var names=new[]{"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner","TopEdge","BottomEdge","LeftEdge","RightEdge"};
        var sprites=new[]{"CornerTL","CornerTR","CornerBL","CornerBR","RailTop","RailBottom","RailLeft","RailRight"};
        for(int i=0;i<names.Length;i++)
        {
            var r=Rect(modular,names[i],new Rect(0,0,25,25));var im=r.gameObject.AddComponent<Image>();im.sprite=Sprite(sprites[i]);im.raycastTarget=false;
        }
        var fitter=modular.gameObject.AddComponent<ResponsiveModularFrameFitter>();fitter.ConfigureNativePieces(Sprite("Joint"),120);
        // Independent disposable openings exercise the shared fitter; the captured board never moves.
        var probe=Rect(canvas.transform,"DisposableResizeProbe",b);
        foreach(float size in new[]{512f,600f,b.width})
        {
            probe.sizeDelta=new Vector2(size,size);fitter.FollowOpening(probe);CheckFrame(modular);
        }
        Object.DestroyImmediate(probe.gameObject);fitter.FollowOpening(board);CheckFrame(modular);
        var cells=Rect(root,"LockedCells_ReadOnly",new Rect(0,0,720,1280));
        foreach(var cell in snapshot.cells)
        {
            var r=Rect(cells,cell.name,cell.rect);var im=r.gameObject.AddComponent<Image>();
            im.sprite=AssetDatabase.LoadAllAssetsAtPath(cell.asset).OfType<Sprite>().First(s=>s.name==cell.sprite);
            im.color=cell.color;im.raycastTarget=false;
            Assert.That(im.sprite.rect.size,Is.EqualTo(cell.nativeSize));Assert.That(r.anchoredPosition,Is.EqualTo(cell.rect.position));
            Assert.That(r.rect.size,Is.EqualTo(cell.rect.size));
        }
        Assert.That(cells.childCount,Is.EqualTo(64));
        Assert.That(board.anchoredPosition,Is.EqualTo(originalPosition));Assert.That(board.rect.size,Is.EqualTo(originalSize));
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Scene),Is.False);
        actors.gameObject.SetActive(false);Canvas.ForceUpdateCanvases();
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Scene),Is.True);
        Render(camera,720,1280,"Unity-Layers-720x1280.png");actors.gameObject.SetActive(true);
        CaptureVariants(camera,canvas,root,surround,cells,1280);
        // Measured Stage00 tall layout offsets; disposable captures, never saved over the scene.
        ((RectTransform)canvas.transform).sizeDelta=new Vector2(720,1600);
        foreach(Transform t in canvas.transform)
        {
            var r=t as RectTransform;if(!r)continue;
            if(t.name=="BlankOceanExtent")r.sizeDelta=new Vector2(720,1600);
            else if(t.name=="BlankLowerSurroundings")r.sizeDelta=new Vector2(712,240);
            else if(t.name=="Stage01_SeparateBackgroundLayers"||t.name=="Stage03_Platform_Candidates"||t.name=="Stage02_Architecture_Candidates"||t.name.StartsWith("PortraitInterior")||t.name=="ExistingBattleFloorGuide"||t.name=="Stage01_ReadOnlyActorContrast"||t.name.StartsWith("Stage04_"))r.anchoredPosition+=Vector2.up*320;
            else if(t.name.StartsWith("GridGuide")||t.name.StartsWith("PlayableBoardBounds")||t.name=="Stage05_BoardReview")r.anchoredPosition+=Vector2.up*194;
            else if(t.name.StartsWith("UnchangedSharedUIShells"))foreach(RectTransform child in t)if(child.anchoredPosition.y>796)child.anchoredPosition+=Vector2.up*320;
        }
        fitter.RefreshFrame();CheckFrame(modular);
        camera.orthographicSize=800;camera.transform.position=new Vector3(360,800,-10);
        CaptureVariants(camera,canvas,root,surround,cells,1600);
        File.WriteAllText(Root+"/QA/Stage05/UnityPreview.json",JsonUtility.ToJson(new Result{
            status="PASS",importedAssets=manifest.assets.Length,captures=captures,resizeCases=3,lockedCells=64,
            importedOpaquePixelsAndAlphaExact=true,sharedFrameFitterUnmodified=true,sceneExcludedFromBuild=true,
            boardPosition=originalPosition,boardSize=originalSize,
            note="Actual Unity camera renders of a static art-only scene. Existing production cell sprites, colors and measured bounds are copied read-only. The existing fractional cell mapping is preserved, not fixed or recertified. New frame sprites use uniform native scale 1/2/3 at 360/720/1080 widths. No live combat/device test or production integration."
        },true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.OpenScene(Scene);
        Debug.Log("DROWNED_COURT_STAGE05_PREVIEW_PASS");
    }
    static void CaptureVariants(Camera camera,Canvas canvas,RectTransform root,RectTransform surround,RectTransform cells,int height)
    {
        foreach(int w in new[]{360,720,1080})
        {
            string dimensions=w+"x"+(height*w/720);
            surround.gameObject.SetActive(false);Render(camera,w,height*w/720,"Unity-Cells-Before-"+dimensions+".png");
            surround.gameObject.SetActive(true);Render(camera,w,height*w/720,"Unity-Actors-"+dimensions+".png");
            var active=canvas.transform.Cast<Transform>().Where(t=>t!=root&&t.gameObject.activeSelf).ToArray();
            foreach(var t in active)t.gameObject.SetActive(false);cells.gameObject.SetActive(false);
            var prior=camera.backgroundColor;camera.backgroundColor=Color.clear;
            Render(camera,w,height*w/720,"Unity-FrameOnly-"+dimensions+".png");
            camera.backgroundColor=prior;cells.gameObject.SetActive(true);foreach(var t in active)t.gameObject.SetActive(true);
        }
    }
    static void CheckFrame(RectTransform frame)
    {
        foreach(var n in new[]{"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner"})
        {
            var r=(RectTransform)frame.Find(n);Assert.That(r.sizeDelta,Is.EqualTo(new Vector2(25,25)));
            Assert.That(r.localScale,Is.EqualTo(Vector3.one));Assert.That(r.localRotation,Is.EqualTo(Quaternion.identity));
        }
        Assert.That(((RectTransform)frame.Find("TopEdge")).rect.height,Is.EqualTo(11));
        Assert.That(((RectTransform)frame.Find("LeftEdge")).rect.width,Is.EqualTo(11));
        var size=((RectTransform)frame.parent).rect.size;
        foreach(RectTransform r in frame.Find("NativeConnectors"))
        {
            if(!r.gameObject.activeSelf)continue;float x=r.anchoredPosition.x+4,y=-r.anchoredPosition.y+4;
            Assert.That(x==12||x==Mathf.Round(size.x)-13||y==12||y==Mathf.Round(size.y)-13,Is.True,"Joint centered on rail");
        }
    }
    static void Import(Manifest data)
    {
        Directory.CreateDirectory(Art);
        foreach(var a in data.assets)
        {
            string path=Art+"/"+a.name+".png";File.Copy(Root+"/Exports/Stage05/"+a.name+".png",path,true);
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
        File.WriteAllBytes(Root+"/Preview/Stage05/"+filename,png.EncodeToPNG());captures++;
        RenderTexture.active=prior;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(png);
    }
    [Serializable] class Manifest {public Asset[] assets;}
    [Serializable] class Asset {public string name;public int width,height;}
    [Serializable] class Result {public string status,note;public int importedAssets,captures,resizeCases,lockedCells;public bool importedOpaquePixelsAndAlphaExact,sharedFrameFitterUnmodified,sceneExcludedFromBuild;public Vector2 boardPosition,boardSize;}
}
