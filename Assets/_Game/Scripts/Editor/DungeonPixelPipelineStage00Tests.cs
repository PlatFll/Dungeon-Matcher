using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Stage 00 only: additive diagnostics and an empty art-only scaffold.
// No production importer, layout, save or rendering setting is changed.
public sealed class DungeonPixelPipelineStage00Tests
{
    public const string Root = "ArtSource/DungeonPixelLab";
    public const string Candidate = "Assets/_Game/Art/DungeonPixelLab";
    public const string ScenePath = "Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity";
    [Serializable] public class SpriteSample
    {
        public string name, path, rendering, filter, format, mesh;
        public Rect screen, source;
        public Vector2 pixelsPerSourcePixel;
        public Color tint;
        public float ppu;
        public bool mipmaps, compressed, androidOverride;
    }
    [Serializable] public class CameraSample
    {
        public string name, target, pipeline;
        public float orthographicSize, renderScale;
        public bool hdr, msaa, dynamicResolution;
        public Rect pixels;
    }
    [Serializable] public class CaptureReport
    {
        public string label, unityVersion, activeBuildTarget, colorSpace;
        public int width, height, qualityAntiAliasing;
        public float canvasScale;
        public SpriteSample[] sprites;
        public CameraSample[] cameras;
    }
    static string Hash(string path)
    { using(var sha=System.Security.Cryptography.SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))); }
    static void Size(Vector2Int s) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] {s});
    static IEnumerator Until(Func<bool> f)
    {
        float end = Time.realtimeSinceStartup + 45;
        while (!f()) { Assert.That(Time.realtimeSinceStartup, Is.LessThan(end)); yield return null; }
    }
    static SpriteSample Sample(string name, Sprite sprite, Rect screen, Color tint, string rendering)
    {
        string path = AssetDatabase.GetAssetPath(sprite);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        return new SpriteSample {name=name,path=path,rendering=rendering,screen=screen,source=sprite.rect,
            pixelsPerSourcePixel=new Vector2(screen.width/sprite.rect.width,screen.height/sprite.rect.height),
            tint=tint,ppu=sprite.pixelsPerUnit,filter=importer.filterMode.ToString(),format=sprite.texture.format.ToString(),
            mesh=settings.spriteMeshType.ToString(),mipmaps=importer.mipmapEnabled,
            compressed=importer.textureCompression!=TextureImporterCompression.Uncompressed,
            androidOverride=importer.GetPlatformTextureSettings("Android").overridden};
    }
    static Rect WorldRect(SpriteRenderer renderer, Camera camera)
    {
        var a=camera.WorldToScreenPoint(renderer.bounds.min);var b=camera.WorldToScreenPoint(renderer.bounds.max);
        return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
    }
    static CaptureReport Measure(string label)
    {
        var list = new List<SpriteSample>();
        foreach(var i in Object.FindObjectsByType<Image>(FindObjectsSortMode.None)
            .Where(i=>i.isActiveAndEnabled&&i.sprite&&(i.name.StartsWith("ApprovedDungeon")||i.name.StartsWith("Stage00"))))
            list.Add(Sample(i.name,i.sprite,GameplayPixelLayoutController.ScreenRect(i.rectTransform),i.color,i.type.ToString()));
        var camera = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault(c=>c.isActiveAndEnabled);
        if(camera) foreach(var r in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&r.sprite&&(r.name=="DungeonSurroundingMasonry"||r.name.StartsWith("ApprovedDungeon"))))
            list.Add(Sample(r.name,r.sprite,WorldRect(r,camera),r.color,r.drawMode.ToString()));
        var pipeline = GraphicsSettings.currentRenderPipeline;
        float renderScale = 1;
        if(pipeline) { var prop=pipeline.GetType().GetProperty("renderScale");if(prop!=null)renderScale=Convert.ToSingle(prop.GetValue(pipeline)); }
        return new CaptureReport { label=label,unityVersion=Application.unityVersion,
            activeBuildTarget=EditorUserBuildSettings.activeBuildTarget.ToString(),colorSpace=QualitySettings.activeColorSpace.ToString(),
            width=Screen.width,height=Screen.height,qualityAntiAliasing=QualitySettings.antiAliasing,
            canvasScale=Object.FindFirstObjectByType<GameplayPixelLayoutController>()?.GetComponentInParent<Canvas>().rootCanvas.scaleFactor??0,
            sprites=list.OrderBy(s=>s.name).ToArray(),
            cameras=Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(c=>c.isActiveAndEnabled).Select(c=>new CameraSample {
                name=c.name,target=c.targetTexture?c.targetTexture.width+"x"+c.targetTexture.height+" "+c.targetTexture.filterMode:"screen",
                pipeline=pipeline?pipeline.name:"Built-in",renderScale=renderScale,orthographicSize=c.orthographicSize,
                hdr=c.allowHDR,msaa=c.allowMSAA,dynamicResolution=c.allowDynamicResolution,pixels=c.pixelRect}).ToArray()};
    }
    static IEnumerator Capture(string label)
    {
        Canvas.ForceUpdateCanvases();
        string path=Path.GetFullPath(Root+"/Previews/Stage00/"+label+".png");
        if(File.Exists(path)) File.Delete(path); // Only this test's named screenshot.
        File.WriteAllText(Root+"/Diagnostics/"+label+".json",JsonUtility.ToJson(Measure(label),true));
        ScreenCapture.CaptureScreenshot(path);
        yield return Until(()=>File.Exists(path));
    }
    static DungeonSideViewArtPreview BuildScaffold()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        string path=Candidate+"/Diagnostics/PixelScaleProbe.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;
        importer.wrapMode=TextureWrapMode.Clamp;importer.alphaIsTransparency=true;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteExtrude=0;importer.SetTextureSettings(settings);importer.SaveAndReimport();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var root=new GameObject("Dungeon PixelLab EMPTY candidate scaffold - Stage 00");
        var preview=root.AddComponent<DungeonSideViewArtPreview>();
        preview.previewCamera=new GameObject("Preview Camera").AddComponent<Camera>();
        preview.previewCamera.transform.SetParent(root.transform,false);
        preview.previewCanvas=new GameObject("Preview Canvas",typeof(RectTransform)).AddComponent<Canvas>();
        preview.previewCanvas.transform.SetParent(root.transform,false);
        preview.modules=Array.Empty<Sprite>();
        preview.placementManifest=AssetDatabase.LoadAssetAtPath<TextAsset>(Candidate+"/EmptyPlacementManifest.json");
        preview.width=360;preview.height=640;preview.safeArea=new Rect(0,0,360,640);preview.Rebuild();
        EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
        Assert.That(preview.Content.GetComponentsInChildren<Image>(),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
        return preview;
    }
    static void Probe(DungeonSideViewArtPreview preview, string name, Sprite sprite, Vector2 at)
    {
        var r=new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();
        r.SetParent(preview.Content,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;
        r.anchoredPosition=at;r.sizeDelta=sprite.rect.size;
        var i=r.GetComponent<Image>();i.sprite=sprite;i.raycastTarget=false;
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;
        if(Application.isPlaying)yield return new ExitPlayMode();
    }
    [UnityTest, Explicit("Stage00 capture rebuilds its empty preview; run only for the setup checkpoint.")] public IEnumerator Stage00BaselineAndExactPixelScaffold()
    {
        Directory.CreateDirectory(Root+"/Diagnostics");Directory.CreateDirectory(Root+"/Previews/Stage00");
        File.WriteAllText(Root+"/Diagnostics/BuildSettings.sha256",Hash("ProjectSettings/EditorBuildSettings.asset"));
        EditorUtility.audioMasterMute=true;Size(new Vector2Int(1080,1920));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        {
            string profile=Path.GetFullPath(".utmp/Stage00/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
            File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave()));
            using(AccountProgression.UseDisposableProfile(profile))
            {
                UnityEngine.Random.InitState(3101);RunLaunchOptions.StartingZone="dungeon";SceneManager.LoadScene("Game");
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
                foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920)})
                {
                    Size(size);yield return new WaitForSecondsRealtime(.6f);
                    Assert.That(Screen.width,Is.EqualTo(size.x));Assert.That(Screen.height,Is.EqualTo(size.y));
                    yield return Capture("Baseline-"+size.x+"x"+size.y);
                }
                SceneManager.LoadScene("MainMenu");yield return null;yield return null;
            }
        }
        yield return new ExitPlayMode();
        BuildScaffold();
        File.WriteAllText(Root+"/Diagnostics/EmptyScene.sha256",Hash(ScenePath));
        Size(new Vector2Int(360,640));yield return new EnterPlayMode();
        foreach(var size in new[]{new Vector2Int(360,640),new Vector2Int(1080,1920)})
        {
            Size(size);yield return new WaitForSecondsRealtime(.4f);
            var preview=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();
            preview.width=size.x;preview.height=size.y;preview.safeArea=new Rect(0,0,size.x,size.y);preview.Rebuild();
            yield return null;yield return null;
            yield return Capture("EmptyScaffold-"+size.x+"x"+size.y);
            Probe(preview,"Stage00PixelScaleProbe",AssetDatabase.LoadAssetAtPath<Sprite>(Candidate+"/Diagnostics/PixelScaleProbe.png"),new Vector2(32,32));
            Probe(preview,"Stage00ExistingWallDIAGNOSTIC_ONLY",AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Tiles_Wall/BackWallReference.png"),new Vector2(96,32));
            yield return null;yield return null;yield return Capture("PixelProbe-"+size.x+"x"+size.y);
            Assert.That(preview.PixelScale,Is.EqualTo(size.x/360));
            string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
            Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
        }
        yield return new ExitPlayMode();
        Assert.That(Hash(ScenePath),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/EmptyScene.sha256")),"Diagnostic fixtures must not be saved into the empty scene.");
        Assert.That(Hash("ProjectSettings/EditorBuildSettings.asset"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/BuildSettings.sha256")),"Preview must not modify build settings.");
    }
}
