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

/// <summary>Explicit fascia-only addition and native ground-contact/portrait overlap evidence.</summary>
public sealed class DungeonPixelPipelineStage04Tests
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
        foreach(var file in Directory.GetFiles(Assets+"/Fascia","*.png"))
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
        p.name="Dungeon PixelLab Stage 04 continuous front platform fascia - candidate";
        p.modules=new[]{Assets+"/Wall",Assets+"/PortraitRecess",Assets+"/FloorTop",Assets+"/Fascia",Frames}
            .SelectMany(d=>Directory.GetFiles(d,"*.png"))
            .Select(f=>AssetDatabase.LoadAssetAtPath<Sprite>(f.Replace('\\','/'))).ToArray();
        p.placementManifest=AssetDatabase.LoadAssetAtPath<TextAsset>(Assets+"/Stage04FasciaPlacement.json");
        p.width=360;p.height=640;p.safeArea=new Rect(0,0,360,640);p.Rebuild();
        Assert.That(p.Pieces.Count,Is.EqualTo(135));EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
    }
    static IEnumerator Capture(string name,int scale)
    {
        Canvas.ForceUpdateCanvases();yield return null;yield return null;
        var path=Path.GetFullPath(Root+"/Previews/Stage04/UnityFascia-"+name+"-"+scale+"x.png");
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
    [UnityTest,Explicit("Stage04 isolated front fascia and temporary silhouette/contact markers; no gameplay.")]
    public IEnumerator Stage04FasciaNativePreview()
    {
        Directory.CreateDirectory(Root+"/Diagnostics/Stage04");Directory.CreateDirectory(Root+"/Previews/Stage04");
        File.WriteAllText(Root+"/Diagnostics/Stage04/BuildSettings.sha256",Hash("ProjectSettings/EditorBuildSettings.asset"));
        File.WriteAllText(Root+"/Diagnostics/Stage04/GameScene.sha256",Hash("Assets/_Game/Scenes/Game.unity"));
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
            string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
            Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
            yield return Capture("Normal",scale);
            var root=new GameObject("Temporary contact markers - never saved",typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(p.Content,false);root.anchorMin=root.anchorMax=root.pivot=new Vector2(0,1);root.sizeDelta=new Vector2(360,640);root.anchoredPosition=Vector2.zero;
            foreach(int x in new[]{154,234,314})
            {
                Marker(root,"Silhouette head",x-3,154,7,7);
                Marker(root,"Silhouette body",x-5,161,11,13);
                Marker(root,"Left foot",x-4,174,3,6);
                Marker(root,"Right foot",x+2,174,3,6);
                var foot=Marker(root,"Contact tick",x-7,179,15,1);
                var b=GameplayPixelLayoutController.ScreenRect(foot);Assert.That(b.yMin,Is.EqualTo((640-180)*scale).Within(.01f));
            }
            yield return Capture("Contact",scale);root.gameObject.SetActive(false);Object.Destroy(root.gameObject);yield return Capture("Restored",scale);
        }
        yield return new ExitPlayMode();
        Assert.That(Hash("ProjectSettings/EditorBuildSettings.asset"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage04/BuildSettings.sha256")));
        Assert.That(Hash("Assets/_Game/Scenes/Game.unity"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage04/GameScene.sha256")));
    }
}
