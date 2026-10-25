using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Stage 02 candidates stay in an additive art-only scene; the accepted Stage 01 scene is read-only.
public static class DrownedCourtV2Stage02Preview
{
    const string Root = "ArtSource/DrownedCourtEnvironmentV2";
    const string Art = "Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage02";
    const string SourceScene = "Assets/_Game/Scenes/ArtPreview_DrownedCourtV2.unity";
    const string PreviewScene = "Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage02.unity";
    [MenuItem("Dungeon Matcher/Art/Drowned Court V2/Build Stage 02 Preview")]
    public static void Build()
    {
        Assert.That(Application.isPlaying, Is.False);
        var data = JsonUtility.FromJson<Manifest>(File.ReadAllText(Root + "/Native/Stage02/Manifest.json"));
        Directory.CreateDirectory(Art);
        foreach (var a in data.assets)
        {
            string path = Art + "/" + a.name + ".png";
            File.Copy(Root + "/Exports/Stage02/" + a.name + ".png", path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 64; ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed; ti.mipmapEnabled = false;
            ti.npotScale = TextureImporterNPOTScale.None; ti.maxTextureSize = 2048;
            ti.alphaSource = TextureImporterAlphaSource.FromInput; ti.alphaIsTransparency = true;
            ti.isReadable = true; ti.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings(); ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; ti.SetTextureSettings(settings); ti.SaveAndReimport();
            var expected = new Texture2D(2, 2); expected.LoadImage(File.ReadAllBytes(path));
            var actual = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Assert.That(actual.width, Is.EqualTo(a.width)); Assert.That(actual.height, Is.EqualTo(a.height));
            var pixels = actual.GetPixels32(); var original = expected.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
                Assert.That(pixels[i].a == original[i].a && (original[i].a == 0 || pixels[i].Equals(original[i])), Is.True, a.name + " import pixel " + i);
            Object.DestroyImmediate(expected);
        }
        EditorSceneManager.OpenScene(SourceScene);
        var canvas = Object.FindFirstObjectByType<Canvas>(); var camera = Object.FindFirstObjectByType<Camera>();
        Assert.That(canvas, Is.Not.Null); Assert.That(camera, Is.Not.Null);
        var actors = canvas.transform.Find("Stage01_ReadOnlyActorContrast"); Assert.That(actors, Is.Not.Null);
        actors.gameObject.SetActive(true); Render(camera, 720, 1280, "Unity-Before-720x1280.png");
        var environment = Rect(canvas.transform, "Stage02_Architecture_Candidates", new Rect(0, 796, 720, 480));
        environment.SetSiblingIndex(canvas.transform.Find("Stage01_SeparateBackgroundLayers").GetSiblingIndex() + 1);
        foreach (var p in data.placements)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "/" + p.name + ".png"); Assert.That(sprite, Is.Not.Null);
            var r = Rect(environment, p.name, new Rect(p.x * 2, 480 - (p.y + sprite.rect.height) * 2, sprite.rect.width * 2, sprite.rect.height * 2));
            var image = r.gameObject.AddComponent<Image>(); image.sprite = sprite; image.raycastTarget = false;
            Assert.That(r.rect.width / sprite.rect.width, Is.EqualTo(2)); Assert.That(r.rect.height / sprite.rect.height, Is.EqualTo(2));
            Assert.That(r.anchoredPosition.x, Is.GreaterThanOrEqualTo(0));
            Assert.That(r.anchoredPosition.x + r.rect.width, Is.LessThanOrEqualTo(720));
            Assert.That(r.anchoredPosition.y, Is.GreaterThanOrEqualTo(0));
            Assert.That(r.anchoredPosition.y + r.rect.height, Is.LessThanOrEqualTo(480));
        }
        Assert.That(environment.GetSiblingIndex(), Is.LessThan(actors.GetSiblingIndex()));
        Assert.That(environment.GetSiblingIndex(), Is.LessThan(canvas.transform.Find("UnchangedSharedUIShells_StaticReviewOnly").GetSiblingIndex()));
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None), Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == PreviewScene), Is.False);
        actors.gameObject.SetActive(false); Canvas.ForceUpdateCanvases();
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene, PreviewScene), Is.True);
        Render(camera, 720, 1280, "Unity-Layers-720x1280.png");
        actors.gameObject.SetActive(true);
        Render(camera, 360, 640, "Unity-Actors-360x640.png");
        Render(camera, 720, 1280, "Unity-Actors-720x1280.png");
        Render(camera, 1080, 1920, "Unity-Actors-1080x1920.png");
        // Apply the measured Stage 00 tall-anchor deltas only in this unsaved review view.
        ((RectTransform)canvas.transform).sizeDelta = new Vector2(720, 1600);
        foreach (Transform t in canvas.transform)
        {
            var r = t as RectTransform; if (!r) continue;
            if (t.name == "BlankOceanExtent") r.sizeDelta = new Vector2(720, 1600);
            else if (t.name == "BlankLowerSurroundings") r.sizeDelta = new Vector2(712, 240);
            else if (t.name == "Stage01_SeparateBackgroundLayers" || t.name == "Stage02_Architecture_Candidates" || t.name.StartsWith("PortraitInterior") || t.name == "ExistingBattleFloorGuide" || t.name == "Stage01_ReadOnlyActorContrast")
                r.anchoredPosition += Vector2.up * 320;
            else if (t.name.StartsWith("GridGuide") || t.name.StartsWith("PlayableBoardBounds")) r.anchoredPosition += Vector2.up * 194;
            else if (t.name.StartsWith("UnchangedSharedUIShells"))
                foreach (RectTransform child in t) if (child.anchoredPosition.y > 796) child.anchoredPosition += Vector2.up * 320;
        }
        camera.orthographicSize = 800; camera.transform.position = new Vector3(360, 800, -10);
        Render(camera, 360, 800, "Unity-Actors-360x800.png");
        Render(camera, 720, 1600, "Unity-Actors-720x1600.png");
        Render(camera, 1080, 2400, "Unity-Actors-1080x2400.png");
        File.WriteAllText(Root + "/QA/Stage02/UnityPreview.json", JsonUtility.ToJson(new Result {
            status = "PASS", importedAssets = data.assets.Length, placedModules = data.placements.Length,
            importedOpaquePixelsAndAlphaExact = true, integerUniformSourceScale = true, allPlacedCanvasBoundsInsideUpperStage = true,
            actorsAndSharedUIAboveArchitecture = true, sourcePixelScales = new[] { 1, 2, 3 }, artOnly = true, sceneExcludedFromBuild = true,
            actorsAreUnchangedReadOnlyOverlays = true, sharedBoardSize = new Vector2(545.099f, 545.099f),
            sharedPortraitSize = new Vector2(195, 406), captures = 8,
            limitations = "Static art-only camera renders. No new gameplay, live health/countdown UI, safe-area interaction or device test. Tall upper placement uses measured Stage00 anchor deltas; bottom shells retain baseline."
        }, true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); EditorSceneManager.OpenScene(PreviewScene);
        Debug.Log("DROWNED_COURT_STAGE02_PREVIEW_PASS");
    }
    static RectTransform Rect(Transform parent, string name, Rect b)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = r.pivot = Vector2.zero; r.anchoredPosition = b.position; r.sizeDelta = b.size; return r;
    }
    static void Render(Camera camera, int w, int h, string filename)
    {
        Canvas.ForceUpdateCanvases();
        var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32); rt.filterMode = FilterMode.Point; rt.antiAliasing = 1;
        var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
        var png = new Texture2D(w, h, TextureFormat.RGBA32, false); png.ReadPixels(new Rect(0, 0, w, h), 0, 0); png.Apply();
        File.WriteAllBytes(Root + "/Preview/Stage02/" + filename, png.EncodeToPNG());
        RenderTexture.active = prior; camera.targetTexture = null; RenderTexture.ReleaseTemporary(rt); Object.DestroyImmediate(png);
    }
    [Serializable] class Manifest { public Asset[] assets; public Placement[] placements; }
    [Serializable] class Asset { public string name; public int width, height; }
    [Serializable] class Placement { public string name; public int x, y; }
    [Serializable] class Result
    {
        public string status, limitations; public int[] sourcePixelScales; public int importedAssets, placedModules, captures;
        public bool importedOpaquePixelsAndAlphaExact, integerUniformSourceScale, allPlacedCanvasBoundsInsideUpperStage, actorsAndSharedUIAboveArchitecture, artOnly, sceneExcludedFromBuild, actorsAreUnchangedReadOnlyOverlays;
        public Vector2 sharedBoardSize, sharedPortraitSize;
    }
}
