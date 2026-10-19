using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

/// <summary>Isolated modular portrait frame: native rails, current source character readability and recess containment.</summary>
public sealed class DungeonPixelPipelineStage10Tests
{
    const string Root="ArtSource/DungeonPixelLab";
    const string Assets="Assets/_Game/Art/DungeonPixelLab";
    const string Frames="Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Frames_Player";
    const string ScenePath="Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity";
    static void SetSize(Vector2Int s)=>typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{s});
    static string Hash(string path){using(var h=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path)));}
    static void Prepare()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(var file in Directory.GetFiles(Assets+"/PortraitFrame","*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;
            importer.wrapMode=TextureWrapMode.Clamp;importer.alphaIsTransparency=false;importer.ClearPlatformTextureSettings("Android");
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteExtrude=0;
            importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        var p=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();Assert.That(p,Is.Not.Null);
        p.name="Dungeon PixelLab Stage 10 modular portrait frame - candidate";
        p.modules=new[]{Assets+"/Wall",Assets+"/PortraitRecess",Assets+"/FloorTop",Assets+"/Fascia",Assets+"/Gate",Assets+"/Support",Assets+"/Torch",Assets+"/Hanging",Assets+"/Clutter",Assets+"/PortraitFrame"}
            .SelectMany(d=>Directory.GetFiles(d,"*.png"))
            .Select(f=>AssetDatabase.LoadAssetAtPath<Sprite>(f.Replace('\\','/'))).ToArray();
        p.placementManifest=AssetDatabase.LoadAssetAtPath<TextAsset>(Assets+"/Stage10PortraitPlacement.json");
        p.width=360;p.height=640;p.safeArea=new Rect(0,0,360,640);p.Rebuild();
        Assert.That(p.Pieces.Count,Is.EqualTo(250));EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
    }
    static IEnumerator Capture(string name,int scale)
    {
        Canvas.ForceUpdateCanvases();yield return null;yield return null;
        var path=Path.GetFullPath(Root+"/Previews/Stage10/UnityPortrait-"+name+"-"+scale+"x.png");
        if(File.Exists(path))File.Delete(path);ScreenCapture.CaptureScreenshot(path);float limit=Time.realtimeSinceStartup+20;
        while(!File.Exists(path)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(limit));yield return null;}
        yield return null;yield return null;
    }
    static RectTransform Marker(Transform parent,string name,int x,int y,int w,int h)
    {
        var r=new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();r.SetParent(parent,false);
        r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
        var i=r.GetComponent<Image>();i.color=new Color32(0,255,255,255);i.raycastTarget=false;return r;
    }
    [UnityTearDown]public IEnumerator Cleanup(){if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit("Stage10 isolated portrait frame with positive and negative recess spill probes; no gameplay.")]
    public IEnumerator Stage10PortraitNativePreview()
    {
        Directory.CreateDirectory(Root+"/Diagnostics/Stage10");Directory.CreateDirectory(Root+"/Previews/Stage10");
        File.WriteAllText(Root+"/Diagnostics/Stage10/BuildSettings.sha256",Hash("ProjectSettings/EditorBuildSettings.asset"));
        File.WriteAllText(Root+"/Diagnostics/Stage10/GameScene.sha256",Hash("Assets/_Game/Scenes/Game.unity"));
        Prepare();SetSize(new Vector2Int(360,640));yield return new EnterPlayMode();
        foreach(int scale in new[]{1,3})
        {
            SetSize(new Vector2Int(360*scale,640*scale));yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Screen.width,Is.EqualTo(360*scale));Assert.That(Screen.height,Is.EqualTo(640*scale));
            var p=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();p.width=Screen.width;p.height=Screen.height;
            p.safeArea=new Rect(0,0,Screen.width,Screen.height);p.Rebuild();yield return null;Canvas.ForceUpdateCanvases();
            foreach(var r in p.Pieces)
            {
                var i=r.GetComponent<Image>();var b=GameplayPixelLayoutController.ScreenRect(r);
                Assert.That(b.x,Is.EqualTo(Mathf.Round(b.x)).Within(.01f));Assert.That(b.y,Is.EqualTo(Mathf.Round(b.y)).Within(.01f));
                Assert.That(b.width,Is.EqualTo(i.sprite.rect.width*scale).Within(.01f));Assert.That(b.height,Is.EqualTo(i.sprite.rect.height*scale).Within(.01f));
                Assert.That(i.sprite.pixelsPerUnit,Is.EqualTo(64));Assert.That(i.color,Is.EqualTo(Color.white));
                Assert.That(b.xMin,Is.GreaterThanOrEqualTo(-.01f));Assert.That(b.yMin,Is.GreaterThanOrEqualTo(-.01f));
                Assert.That(b.xMax,Is.LessThanOrEqualTo(Screen.width+.01f));Assert.That(b.yMax,Is.LessThanOrEqualTo(Screen.height+.01f));
            }
            var floors=p.Pieces.Where(r=>r.name.StartsWith("Floor_")).ToArray();Assert.That(floors.Length,Is.EqualTo(8));
            foreach(var floor in floors){var b=GameplayPixelLayoutController.ScreenRect(floor);Assert.That(b.y,Is.EqualTo((640-190)*scale).Within(.01f));Assert.That(b.height,Is.EqualTo(12*scale).Within(.01f));}
            var faces=p.Pieces.Where(r=>r.name.StartsWith("Fascia_")).ToArray();Assert.That(faces.Length,Is.EqualTo(6));
            foreach(var face in faces){var b=GameplayPixelLayoutController.ScreenRect(face);Assert.That(b.y,Is.EqualTo((640-214)*scale).Within(.01f));Assert.That(b.height,Is.EqualTo(24*scale).Within(.01f));}
            Assert.That(faces.Sum(r=>r.GetComponent<Image>().sprite.rect.width),Is.EqualTo(360));
            var gates=p.Pieces.Where(r=>r.name.StartsWith("Gate_")).ToArray();Assert.That(gates.Length,Is.EqualTo(3));
            foreach(var gate in gates){var b=GameplayPixelLayoutController.ScreenRect(gate);Assert.That(b.x,Is.EqualTo(177*scale).Within(.01f));Assert.That(b.y,Is.EqualTo((640-184)*scale).Within(.01f));Assert.That(b.width,Is.EqualTo(96*scale).Within(.01f));Assert.That(b.height,Is.EqualTo(112*scale).Within(.01f));}
            var supports=p.Pieces.Where(r=>r.name.StartsWith("Support_")).ToArray();Assert.That(supports.Length,Is.EqualTo(56));
            Assert.That(supports.Count(r=>r.name.StartsWith("Support_Core")),Is.EqualTo(15));
            Assert.That(supports.Count(r=>r.name.StartsWith("Support_Corner_")),Is.EqualTo(30));
            var feet=supports.Where(r=>r.name.StartsWith("Support_Foot")).ToArray();Assert.That(feet.Length,Is.EqualTo(3));
            foreach(var foot in feet){var b=GameplayPixelLayoutController.ScreenRect(foot);Assert.That(b.yMin,Is.EqualTo((640-180)*scale).Within(.01f));}
            var torches=p.Pieces.Where(r=>r.name.StartsWith("Torch_")).ToArray();Assert.That(torches.Length,Is.EqualTo(6));
            var lights=torches.Where(r=>r.name.StartsWith("Torch_Light_")).ToArray();Assert.That(lights.Length,Is.EqualTo(2));
            foreach(var torch in torches.Where(r=>!r.name.StartsWith("Torch_Light_"))){var b=GameplayPixelLayoutController.ScreenRect(torch);Assert.That(b.width,Is.EqualTo(32*scale).Within(.01f));Assert.That(b.height,Is.EqualTo(56*scale).Within(.01f));Assert.That(b.yMin,Is.EqualTo((640-142)*scale).Within(.01f));}
            var hanging=p.Pieces.Where(r=>r.name.StartsWith("Hanging_")).ToArray();Assert.That(hanging.Length,Is.EqualTo(15));
            Assert.That(hanging.Count(r=>r.name.StartsWith("Hanging_Chain_Repeat")),Is.EqualTo(8));
            var banners=hanging.Where(r=>r.name.StartsWith("Hanging_Banner")).ToArray();Assert.That(banners.Length,Is.EqualTo(3));
            foreach(var banner in banners){var b=GameplayPixelLayoutController.ScreenRect(banner);Assert.That(b.width,Is.EqualTo(36*scale).Within(.01f));Assert.That(b.height,Is.EqualTo(64*scale).Within(.01f));}
            var clutter=p.Pieces.Where(r=>r.name.StartsWith("Clutter_")).ToArray();Assert.That(clutter.Length,Is.EqualTo(3));
            foreach(var prop in clutter){var b=GameplayPixelLayoutController.ScreenRect(prop);Assert.That(b.yMin,Is.EqualTo((640-182)*scale).Within(.01f));}
            string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
            Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
            var frame=p.Pieces.Where(r=>r.name.StartsWith("Portrait_")).ToArray();Assert.That(frame.Length,Is.EqualTo(40));
            Assert.That(frame.Count(r=>r.name.StartsWith("Portrait_Rail_")),Is.EqualTo(22));
            foreach(var r in frame.Where(r=>r.name.Contains("Rail_Left")||r.name.Contains("Rail_Right")))
                Assert.That(r.sizeDelta.y,Is.EqualTo(20));
            var panel=p.Pieces.Single(r=>r.name=="Recess_Inset").GetComponent<Image>();
            var mask=panel.transform.parent.GetComponent<RectMask2D>();Assert.That(mask,Is.Not.Null);
            var mb=GameplayPixelLayoutController.ScreenRect((RectTransform)mask.transform);
            Assert.That(mb.x,Is.EqualTo(18*scale).Within(.01f));
            Assert.That(mb.y,Is.EqualTo((640-197)*scale).Within(.01f));
            Assert.That(mb.width,Is.EqualTo(80*scale).Within(.01f));
            Assert.That(mb.height,Is.EqualTo(167*scale).Within(.01f));
            yield return Capture("Normal",scale);
            panel.enabled=false;yield return Capture("Baseline",scale);
            var probe=new GameObject("Temporary oversized magenta recess probe",typeof(RectTransform),typeof(Image));
            var pr=(RectTransform)probe.transform;pr.SetParent(mask.transform,false);
            pr.anchorMin=pr.anchorMax=pr.pivot=new Vector2(0,1);pr.anchoredPosition=new Vector2(-40,40);
            pr.sizeDelta=new Vector2(160,247);probe.GetComponent<Image>().color=Color.magenta;probe.GetComponent<Image>().raycastTarget=false;
            yield return Capture("MaskedProbe",scale);
            mask.enabled=false;yield return Capture("UnmaskedControl",scale);
            mask.enabled=true;probe.SetActive(false);Object.Destroy(probe);panel.enabled=true;
            // Source-only readability specimen; no actor component or changed character import.
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);tex.LoadImage(File.ReadAllBytes("Docs/ArtDirection/Audit2026-10/media/skeleton-still.png"));
            tex.filterMode=FilterMode.Point;tex.wrapMode=TextureWrapMode.Clamp;
            var spr=Sprite.Create(tex,new Rect(0,0,64,64),new Vector2(.5f,.5f),64,0,SpriteMeshType.FullRect);
            var actor=Marker(p.Content,"Temporary unchanged Rattlebones source",26,116,64,64);
            actor.GetComponent<Image>().sprite=spr;actor.GetComponent<Image>().color=Color.white;
            // Temporary dynamic fill, two values. The delivered shell contains no fill/text.
            var hp=Marker(p.Content,"Temporary dynamic HP fill",29,188,60,4);
            hp.GetComponent<Image>().color=new Color32(198,38,91,255);
            yield return Capture("PortraitFull",scale);
            hp.sizeDelta=new Vector2(30,4);yield return Capture("PortraitHalf",scale);
            actor.gameObject.SetActive(false);hp.gameObject.SetActive(false);
            Object.Destroy(actor.gameObject);Object.Destroy(hp.gameObject);Object.Destroy(spr);Object.Destroy(tex);
            yield return Capture("Restored",scale);

        }
        yield return new ExitPlayMode();
        Assert.That(Hash("ProjectSettings/EditorBuildSettings.asset"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage10/BuildSettings.sha256")));
        Assert.That(Hash("Assets/_Game/Scenes/Game.unity"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage10/GameScene.sha256")));
    }
}
