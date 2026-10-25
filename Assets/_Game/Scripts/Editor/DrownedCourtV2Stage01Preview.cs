using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Candidate-only art preview. Never loads or saves Game, gameplay definitions or build settings.
public static class DrownedCourtV2Stage01Preview
{
    const string Root = "ArtSource/DrownedCourtEnvironmentV2";
    const string Art = "Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage01";
    const string Scene = "Assets/_Game/Scenes/ArtPreview_DrownedCourtV2.unity";
    static readonly string[] Names = { "DeepWater", "LightShafts", "FarRuins", "MidRuins", "Particles" };
    [MenuItem("Dungeon Matcher/Art/Drowned Court V2/Build Stage 01 Preview")]
    public static void Build()
    {
        Assert.That(Application.isPlaying, Is.False);
        Directory.CreateDirectory(Art);
        foreach (string name in Names)
        {
            string dest = Art + "/" + name + ".png";
            File.Copy(Root + "/Exports/Stage01/" + name + ".png", dest, true);
            AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceSynchronousImport);
            var ti = (TextureImporter)AssetImporter.GetAtPath(dest);
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 64; ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed; ti.mipmapEnabled = false;
            ti.npotScale = TextureImporterNPOTScale.None; ti.maxTextureSize = 2048;
            ti.alphaSource = TextureImporterAlphaSource.FromInput; ti.alphaIsTransparency = true;
            ti.isReadable = true; ti.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings(); ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; ti.SetTextureSettings(settings); ti.SaveAndReimport();
            var expected = new Texture2D(2, 2); expected.LoadImage(File.ReadAllBytes(dest));
            var actual = AssetDatabase.LoadAssetAtPath<Texture2D>(dest);
            Assert.That(actual.width, Is.EqualTo(expected.width)); Assert.That(actual.height, Is.EqualTo(expected.height));
            var a = actual.GetPixels32(); var e = expected.GetPixels32();
            for (int p = 0; p < a.Length; p++)
                Assert.That(a[p].a == e[p].a && (e[p].a == 0 || a[p].Equals(e[p])), Is.True, name + " imported pixel " + p);
            Object.DestroyImmediate(expected);
        }
        EditorSceneManager.OpenScene(Scene);
        var canvas = Object.FindFirstObjectByType<Canvas>();
        var camera = Object.FindFirstObjectByType<Camera>();
        Assert.That(canvas, Is.Not.Null); Assert.That(camera, Is.Not.Null);
        var old = canvas.transform.Find("Stage01_SeparateBackgroundLayers");
        if (old) Object.DestroyImmediate(old.gameObject);
        var oldActors = canvas.transform.Find("Stage01_ReadOnlyActorContrast");
        if (oldActors) Object.DestroyImmediate(oldActors.gameObject);
        var blank = canvas.transform.Find("BlankUpperEnvironment"); blank.gameObject.SetActive(true);
        Render(camera, 720, 1280, "Unity-Before-720x1280.png");
        var env = Rect(canvas.transform, "Stage01_SeparateBackgroundLayers", new Rect(0, 796, 720, 480));
        env.SetSiblingIndex(blank.GetSiblingIndex() + 1); blank.gameObject.SetActive(false);
        Layer(env, "DeepWater", 0, 0); Layer(env, "LightShafts", 0, 0);
        Layer(env, "FarRuins", 208, 96); Layer(env, "MidRuins", 208, 80); Layer(env, "Particles", 0, 0);
        var actors = Rect(canvas.transform, "Stage01_ReadOnlyActorContrast", new Rect(0, 0, 720, 1280));
        Actor(actors, "Rattlebones", "Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png", "Rattlebones_Idle_00", 120, 970, true);
        Actor(actors, "ReefSpearman", "Assets/_Game/Art/DrownedCourt/reef_spearman.png", null, 305, 865, false);
        Actor(actors, "HammerheadBruiser", "Assets/_Game/Art/DrownedCourt/hammerhead_bruiser.png", null, 470, 865, false);
        Actor(actors, "PearlThief", "Assets/_Game/Art/DrownedCourt/pearl_thief.png", null, 635, 865, false);
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None), Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == Scene), Is.False);
        var imageRects = env.GetComponentsInChildren<Image>().Select(i => i.rectTransform).ToArray();
        foreach (var r in imageRects)
        {
            var s = r.GetComponent<Image>().sprite;
            Assert.That(r.rect.width / s.rect.width, Is.EqualTo(2));
            Assert.That(r.rect.height / s.rect.height, Is.EqualTo(2));
        }
        actors.gameObject.SetActive(false); Canvas.ForceUpdateCanvases();
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene); Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene, Scene), Is.True);
        Render(camera, 720, 1280, "Unity-Layers-720x1280.png");
        actors.gameObject.SetActive(true);
        Render(camera, 360, 640, "Unity-Actors-360x640.png");
        Render(camera, 720, 1280, "Unity-Actors-720x1280.png");
        Render(camera, 1080, 1920, "Unity-Actors-1080x1920.png");
        // The Stage 00 measurements establish identical 720px-wide board/portrait dimensions on tall phones.
        // Apply those measured anchor deltas only to this temporary review view; no runtime layout modification.
        ((RectTransform)canvas.transform).sizeDelta = new Vector2(720, 1600);
        foreach (Transform t in canvas.transform)
        {
            var r = t as RectTransform; if (!r) continue;
            if (t.name == "BlankOceanExtent") r.sizeDelta = new Vector2(720, 1600);
            else if (t.name == "BlankLowerSurroundings") r.sizeDelta = new Vector2(712, 240);
            else if (t.name == "Stage01_SeparateBackgroundLayers" || t.name.StartsWith("PortraitInterior") || t.name == "ExistingBattleFloorGuide" || t.name == "Stage01_ReadOnlyActorContrast")
                r.anchoredPosition += Vector2.up * 320;
            else if (t.name.StartsWith("GridGuide") || t.name.StartsWith("PlayableBoardBounds")) r.anchoredPosition += Vector2.up * 194;
            else if (t.name.StartsWith("UnchangedSharedUIShells"))
                foreach (RectTransform child in t) if (child.anchoredPosition.y > 796) child.anchoredPosition += Vector2.up * 320;
        }
        camera.orthographicSize = 800; camera.transform.position = new Vector3(360, 800, -10);
        Render(camera, 360, 800, "Unity-Actors-360x800.png");
        Render(camera, 720, 1600, "Unity-Actors-720x1600.png");
        Render(camera, 1080, 2400, "Unity-Actors-1080x2400.png");
        File.WriteAllText(Root + "/QA/Stage01/UnityPreview.json", JsonUtility.ToJson(new Result {
            status = "PASS", layers = Names, importedOpaquePixelsAndAlphaExact = true, integerUniformSourceScale = true,
            sourcePixelScales = new[] { 1, 2, 3 }, artOnly = true, sceneExcludedFromBuild = true,
            actorsAreUnchangedReadOnlyOverlays = true, sharedBoardSize = new Vector2(545.099f, 545.099f),
            sharedPortraitSize = new Vector2(195, 406), captures = 8,
            limitations = "Static art-only camera renders, not gameplay or device tests. Tall view uses measured Stage00 anchor deltas; bottom shells retained from baseline. Safe-area input and animation not tested in this stage."
        }, true));
        // Discard temporary tall view and restore the saved normal art-only composition.
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); EditorSceneManager.OpenScene(Scene);
        Debug.Log("DROWNED_COURT_STAGE01_PREVIEW_PASS");
    }
    static RectTransform Rect(Transform parent, string name, Rect b)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = r.pivot = Vector2.zero; r.anchoredPosition = b.position; r.sizeDelta = b.size; return r;
    }
    static void Layer(Transform parent, string name, float x, float y)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "/" + name + ".png"); Assert.That(s, Is.Not.Null);
        var im = Rect(parent, name, new Rect(x, y, s.rect.width * 2, s.rect.height * 2)).gameObject.AddComponent<Image>();
        im.sprite = s; im.raycastTarget = false;
    }
    static void Actor(Transform parent, string name, string path, string spriteName, float x, float floor, bool original64)
    {
        var s = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault(v => spriteName == null || v.name == spriteName);
        Assert.That(s, Is.Not.Null, path);
        // Expanded enemy canvases use the registered four-native-pixel floor; old Rattlebones remains its 64px original.
        float bottom = floor - (original64 ? 2 : 8);
        var im = Rect(parent, name, new Rect(x - s.rect.width, bottom, s.rect.width * 2, s.rect.height * 2)).gameObject.AddComponent<Image>();
        im.sprite = s; im.raycastTarget = false;
    }
    static void Render(Camera camera, int w, int h, string filename)
    {
        Canvas.ForceUpdateCanvases();
        var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32); rt.filterMode = FilterMode.Point; rt.antiAliasing = 1;
        var prior = RenderTexture.active; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
        var png = new Texture2D(w, h, TextureFormat.RGBA32, false); png.ReadPixels(new Rect(0, 0, w, h), 0, 0); png.Apply();
        File.WriteAllBytes(Root + "/Preview/Stage01/" + filename, png.EncodeToPNG());
        RenderTexture.active = prior; camera.targetTexture = null; RenderTexture.ReleaseTemporary(rt); Object.DestroyImmediate(png);
    }
    [Serializable] class Result
    {
        public string status, limitations; public string[] layers; public int[] sourcePixelScales; public int captures;
        public bool importedOpaquePixelsAndAlphaExact, integerUniformSourceScale, artOnly, sceneExcludedFromBuild, actorsAreUnchangedReadOnlyOverlays;
        public Vector2 sharedBoardSize, sharedPortraitSize;
    }
}
