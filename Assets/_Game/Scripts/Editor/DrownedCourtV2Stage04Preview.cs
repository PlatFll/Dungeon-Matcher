using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Candidate-only scene. Production layout and all accepted scenes remain read-only.
public static class DrownedCourtV2Stage04Preview
{
    const string Root = "ArtSource/DrownedCourtEnvironmentV2";
    const string Art = "Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage04";
    const string Scene = "Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage04.unity";
    static int captures;
    [MenuItem("Dungeon Matcher/Art/Drowned Court V2/Build Stage 04 Preview")]
    public static void Build()
    {
        Assert.That(Application.isPlaying, Is.False); captures = 0;
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Root + "/Native/Stage04/Manifest.json"));
        Import(manifest);
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage03.unity");
        var canvas = Object.FindFirstObjectByType<Canvas>();
        var camera = Object.FindFirstObjectByType<Camera>();
        var actors = canvas.transform.Find("Stage01_ReadOnlyActorContrast");
        var rattlebones = actors.GetChild(0);
        Assert.That(rattlebones.name.ToLowerInvariant(), Does.Contain("rattlebones"));
        actors.gameObject.SetActive(true);
        Render(camera, 720, 1280, "Unity-Before-720x1280.png");
        var hud = BuildHud(canvas.transform);
        rattlebones.gameObject.SetActive(false);
        Render(camera, 720, 1280, "Unity-Before-Hud-720x1280.png");
        hud.gameObject.SetActive(false); rattlebones.gameObject.SetActive(true);

        var portrait = (RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask");
        Assert.That(portrait.rect.size, Is.EqualTo(new Vector2(195, 406)));
        Assert.That(portrait.anchoredPosition, Is.EqualTo(new Vector2(21, 825)));
        portrait.Find("OverfilledBlankRecess").gameObject.SetActive(false);
        var mask = Rect(portrait, "Stage04_InsetRecessMask", new Rect(20, 20, 155, 366));
        mask.gameObject.AddComponent<RectMask2D>();
        var fill = Rect(mask, "OversizedRecess_NoLeaks", new Rect(-128, -128, 411, 622));
        fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one;
        fill.offsetMin = Vector2.one * -128; fill.offsetMax = Vector2.one * 128;
        var recess = fill.gameObject.AddComponent<Image>(); recess.sprite = Sprite("RecessA");
        recess.type = Image.Type.Tiled; recess.pixelsPerUnitMultiplier = .5f; recess.raycastTarget = false;

        var frame = Rect(canvas.transform, "Stage04_Portrait_Frame", new Rect(21, 825, 195, 406));
        frame.SetSiblingIndex(portrait.GetSiblingIndex() + 1);
        var native = Rect(frame, "NativeLayout", new Rect(0, 0, 97.5f, 203));
        native.localScale = new Vector3(2, 2, 1);
        var modular = Rect(native, "ModularFrame", new Rect(0, 0, 97.5f, 203));
        modular.anchorMax = Vector2.one; modular.offsetMin = modular.offsetMax = Vector2.zero;
        var names = new[] { "TopLeftCorner", "TopRightCorner", "BottomLeftCorner", "BottomRightCorner", "TopEdge", "BottomEdge", "LeftEdge", "RightEdge" };
        var sprites = new[] { "CornerTL", "CornerTR", "CornerBL", "CornerBR", "RailTop", "RailBottom", "RailLeft", "RailRight" };
        for (int i = 0; i < names.Length; i++)
        {
            var child = Rect(modular, names[i], new Rect(0, 0, 15, 15));
            var im = child.gameObject.AddComponent<Image>(); im.sprite = Sprite(sprites[i]); im.raycastTarget = false;
        }
        var fitter = modular.gameObject.AddComponent<ResponsiveModularFrameFitter>();
        fitter.ConfigureNativePieces(Sprite("Clamp"), 45);
        // Exercise the existing shared fitter rather than introducing a separate layout owner.
        foreach (var size in new[] { new Vector2(85, 180), new Vector2(110, 250), new Vector2(97.5f, 203) })
        {
            native.sizeDelta = size; fitter.RefreshFrame(); CheckFrame(modular, size);
        }
        foreach (var d in manifest.decorations)
        {
            var sprite = Sprite(d.name);
            var child = Rect(native, d.name, new Rect(d.x, 203 - d.y - sprite.rect.height, sprite.rect.width, sprite.rect.height));
            var im = child.gameObject.AddComponent<Image>(); im.sprite = sprite; im.raycastTarget = false;
            var corners = new Vector3[4]; child.GetWorldCorners(corners);
            Assert.That(corners[0].x, Is.GreaterThanOrEqualTo(0)); Assert.That(corners[2].x, Is.LessThanOrEqualTo(720));
            Assert.That(corners[0].y, Is.GreaterThanOrEqualTo(0)); Assert.That(corners[2].y, Is.LessThanOrEqualTo(1280));
        }
        Assert.That(frame.GetSiblingIndex(), Is.LessThan(actors.GetSiblingIndex()));
        Assert.That(frame.GetSiblingIndex(), Is.LessThan(hud.GetSiblingIndex()));
        var board = (RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement");
        Assert.That(board.rect.size, Is.EqualTo(new Vector2(545.099f,545.099f)).Using(Vector2Comparer.Instance));
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None), Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == Scene), Is.False);
        actors.gameObject.SetActive(false); hud.gameObject.SetActive(false);
        Canvas.ForceUpdateCanvases(); EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene, Scene), Is.True);
        Render(camera, 720, 1280, "Unity-Layers-720x1280.png");
        // A deliberately oversized, solid magenta image must remain inside the inset mask.
        var savedSprite = recess.sprite; recess.sprite = null; recess.type = Image.Type.Simple; recess.color = Color.magenta;
        Render(camera, 720, 1280, "Unity-Mask-Probe-720x1280.png");
        recess.sprite = savedSprite; recess.type = Image.Type.Tiled; recess.color = Color.white;
        actors.gameObject.SetActive(true);
        CaptureVariants(camera, actors, rattlebones, hud, 1280);
        // Measured Stage00 tall layout offsets: review-only, never saved over the shared geometry.
        ((RectTransform)canvas.transform).sizeDelta = new Vector2(720, 1600);
        foreach (Transform t in canvas.transform)
        {
            var r = t as RectTransform; if (!r) continue;
            if (t.name == "BlankOceanExtent") r.sizeDelta = new Vector2(720, 1600);
            else if (t.name == "BlankLowerSurroundings") r.sizeDelta = new Vector2(712, 240);
            else if (t.name == "Stage01_SeparateBackgroundLayers" || t.name == "Stage03_Platform_Candidates" || t.name == "Stage02_Architecture_Candidates" || t.name.StartsWith("PortraitInterior") || t.name == "ExistingBattleFloorGuide" || t.name == "Stage01_ReadOnlyActorContrast" || t.name.StartsWith("Stage04_"))
                r.anchoredPosition += Vector2.up * 320;
            else if (t.name.StartsWith("GridGuide") || t.name.StartsWith("PlayableBoardBounds")) r.anchoredPosition += Vector2.up * 194;
            else if (t.name.StartsWith("UnchangedSharedUIShells"))
                foreach (RectTransform child in t) if (child.anchoredPosition.y > 796) child.anchoredPosition += Vector2.up * 320;
        }
        camera.orthographicSize = 800; camera.transform.position = new Vector3(360, 800, -10);
        CaptureVariants(camera, actors, rattlebones, hud, 1600);
        File.WriteAllText(Root + "/QA/Stage04/UnityPreview.json", JsonUtility.ToJson(new Result {
            status="PASS", importedAssets=manifest.assets.Length, captures=captures, resizeCases=3,
            importedOpaquePixelsAndAlphaExact=true, sharedFrameFitterUnmodified=true, sceneExcludedFromBuild=true,
            sharedPortraitSize=portrait.rect.size, sharedBoardSize=board.rect.size,
            maskScreenRect=new Rect(41,69,155,366), oversizePadding=128,
            note="Static art-only Unity renders. HUD snapshot uses exact production graphic bindings and measured positions; actors-only mode retains Stage03 contrast references. Not a live combat or device integration test. Shield was not visible in the captured starting profile. Magenta probe pixel bounds independently checked by VerifyStage04.py."
        },true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); EditorSceneManager.OpenScene(Scene);
        Debug.Log("DROWNED_COURT_STAGE04_PREVIEW_PASS");
    }
    static void CaptureVariants(Camera camera, Transform actors, Transform player, RectTransform hud, int height)
    {
        actors.gameObject.SetActive(true); player.gameObject.SetActive(true); hud.gameObject.SetActive(false);
        foreach (int w in new[]{360,720,1080}) Render(camera,w,height*w/720,"Unity-Actors-"+w+"x"+(height*w/720)+".png");
        player.gameObject.SetActive(false); hud.gameObject.SetActive(true);
        foreach (int w in new[]{360,720,1080}) Render(camera,w,height*w/720,"Unity-Hud-"+w+"x"+(height*w/720)+".png");
        player.gameObject.SetActive(true); hud.gameObject.SetActive(false);
    }
    static RectTransform BuildHud(Transform canvas)
    {
        var snapshot = JsonUtility.FromJson<DrownedCourtV2Stage04HudCapture.Snapshot>(File.ReadAllText(Root+"/QA/Stage04/PlayerHudSnapshot.json"));
        var root = Rect(canvas,"Stage04_ReadOnlyProductionPortraitHud",new Rect(0,0,720,1280));
        foreach(var p in snapshot.pieces)
        {
            var r=Rect(root,Path.GetFileName(p.path),p.rect);
            if(p.kind=="image")
            {
                var im=r.gameObject.AddComponent<Image>();
                im.sprite=string.IsNullOrEmpty(p.asset)?null:AssetDatabase.LoadAllAssetsAtPath(p.asset).OfType<Sprite>().First(s=>s.name==p.sprite);
                im.type=(Image.Type)p.imageType;im.fillAmount=p.fill;im.fillMethod=(Image.FillMethod)p.fillMethod;im.fillOrigin=p.fillOrigin;
                im.preserveAspect=p.preserveAspect;im.pixelsPerUnitMultiplier=p.ppuMultiplier;im.color=p.color;im.raycastTarget=false;
            }
            else
            {
                var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p.asset);
                t.text=p.text;t.fontSize=p.fontSize;t.alignment=(TextAlignmentOptions)p.alignment;t.fontStyle=(FontStyles)p.fontStyle;
                t.color=p.color;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Overflow;t.raycastTarget=false;
            }
            Assert.That(r.rect.size,Is.EqualTo(p.rect.size)); Assert.That(r.anchoredPosition,Is.EqualTo(p.rect.position));
        }
        Assert.That(snapshot.pieces.Length,Is.EqualTo(9));return root;
    }
    static void CheckFrame(RectTransform root, Vector2 size)
    {
        foreach(var n in new[]{"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner"})
        {
            var r=(RectTransform)root.Find(n);Assert.That(r.sizeDelta,Is.EqualTo(new Vector2(15,15)));
            Assert.That(r.localScale,Is.EqualTo(Vector3.one));Assert.That(r.localRotation,Is.EqualTo(Quaternion.identity));
        }
        var left=(RectTransform)root.Find("LeftEdge");var right=(RectTransform)root.Find("RightEdge");
        Assert.That(left.rect.width,Is.EqualTo(7));Assert.That(right.rect.width,Is.EqualTo(7));
        foreach(RectTransform r in root.Find("NativeConnectors"))
        {
            if(!r.gameObject.activeSelf)continue;
            float x=r.anchoredPosition.x+4,y=-r.anchoredPosition.y+4;
            Assert.That(x==7 || x==Mathf.Round(size.x)-8 || y==7 || y==Mathf.Round(size.y)-8,Is.True,"Connector centered on a rail");
        }
        foreach(var im in root.GetComponentsInChildren<Image>()) Assert.That(im.rectTransform.rect.width,Is.GreaterThan(0));
    }
    static void Import(Manifest data)
    {
        Directory.CreateDirectory(Art);
        foreach(var a in data.assets)
        {
            string path=Art+"/"+a.name+".png";File.Copy(Root+"/Exports/Stage04/"+a.name+".png",path,true);
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
        File.WriteAllBytes(Root+"/Preview/Stage04/"+filename,png.EncodeToPNG());captures++;
        RenderTexture.active=prior;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(png);
    }
    sealed class Vector2Comparer:System.Collections.Generic.IEqualityComparer<Vector2>
    { public static readonly Vector2Comparer Instance=new Vector2Comparer();public bool Equals(Vector2 a,Vector2 b)=>(a-b).sqrMagnitude<.001f;public int GetHashCode(Vector2 a)=>a.GetHashCode(); }
    [Serializable] class Manifest {public Asset[] assets;public Decoration[] decorations;}
    [Serializable] class Asset {public string name;public int width,height;}
    [Serializable] class Decoration {public string name;public int x,y;}
    [Serializable] class Result {public string status,note;public int importedAssets,captures,resizeCases,oversizePadding;public bool importedOpaquePixelsAndAlphaExact,sharedFrameFitterUnmodified,sceneExcludedFromBuild;public Vector2 sharedPortraitSize,sharedBoardSize;public Rect maskScreenRect;}
}
