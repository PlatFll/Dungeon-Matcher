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
using Object = UnityEngine.Object;

/// <summary>Explicit isolated portrait-recess render and mask regression evidence.</summary>
public sealed class DungeonPixelPipelineStage02Tests
{
    const string Root="ArtSource/DungeonPixelLab";
    const string Assets="Assets/_Game/Art/DungeonPixelLab";
    const string Frames="Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Frames_Player";
    const string ScenePath="Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity";
    static void SetSize(Vector2Int size) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
    static string Hash(string path)
    {
        using(var sha=System.Security.Cryptography.SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)));
    }
    static void Prepare()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(var file in Directory.GetFiles(Assets+"/PortraitRecess","*.png"))
        {
            var path=file.Replace('\\','/');var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;
            importer.alphaIsTransparency=false;importer.ClearPlatformTextureSettings("Android");
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteExtrude=0;
            importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        var preview=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();Assert.That(preview,Is.Not.Null);
        preview.name="Dungeon PixelLab Stage 02 separate portrait recess - candidate";
        preview.modules=new[]{Assets+"/Wall",Assets+"/PortraitRecess",Frames}
            .SelectMany(d=>Directory.GetFiles(d,"*.png"))
            .Select(p=>AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\','/'))).ToArray();
        preview.placementManifest=AssetDatabase.LoadAssetAtPath<TextAsset>(Assets+"/Stage02RecessPlacement.json");
        preview.width=360;preview.height=640;preview.safeArea=new Rect(0,0,360,640);preview.Rebuild();
        Assert.That(preview.Pieces.Count,Is.EqualTo(121));
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
    }
    static IEnumerator Capture(string label,int scale)
    {
        Canvas.ForceUpdateCanvases();yield return null;yield return null;
        string output=Path.GetFullPath(Root+"/Previews/Stage02/UnityRecess-"+label+"-"+scale+"x.png");
        if(File.Exists(output))File.Delete(output);
        ScreenCapture.CaptureScreenshot(output);float deadline=Time.realtimeSinceStartup+20;
        while(!File.Exists(output)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
        yield return null;yield return null;
    }
    [UnityTearDown] public IEnumerator Cleanup(){if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit("Stage 02 isolated native preview plus oversized clipping positive/negative controls.")]
    public IEnumerator Stage02RecessMaskPreview()
    {
        Directory.CreateDirectory(Root+"/Previews/Stage02");Directory.CreateDirectory(Root+"/Diagnostics/Stage02");
        File.WriteAllText(Root+"/Diagnostics/Stage02/BuildSettings.sha256",Hash("ProjectSettings/EditorBuildSettings.asset"));
        File.WriteAllText(Root+"/Diagnostics/Stage02/GameScene.sha256",Hash("Assets/_Game/Scenes/Game.unity"));
        Prepare();SetSize(new Vector2Int(360,640));yield return new EnterPlayMode();
        foreach(int scale in new[]{1,3})
        {
            var size=new Vector2Int(360*scale,640*scale);SetSize(size);yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Screen.width,Is.EqualTo(size.x));Assert.That(Screen.height,Is.EqualTo(size.y));
            var preview=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();
            preview.width=size.x;preview.height=size.y;preview.safeArea=new Rect(0,0,size.x,size.y);preview.Rebuild();
            yield return null;Canvas.ForceUpdateCanvases();
            foreach(var piece in preview.Pieces)
            {
                var image=piece.GetComponent<Image>();var bounds=GameplayPixelLayoutController.ScreenRect(piece);
                Assert.That(bounds.x,Is.EqualTo(Mathf.Round(bounds.x)).Within(.01f));
                Assert.That(bounds.y,Is.EqualTo(Mathf.Round(bounds.y)).Within(.01f));
                Assert.That(bounds.width,Is.EqualTo(image.sprite.rect.width*scale).Within(.01f));
                Assert.That(bounds.height,Is.EqualTo(image.sprite.rect.height*scale).Within(.01f));
                Assert.That(image.sprite.pixelsPerUnit,Is.EqualTo(64));Assert.That(image.color,Is.EqualTo(Color.white));
            }
            string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
            Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
            var panel=preview.Pieces.Single(p=>p.name=="Recess_Inset").GetComponent<Image>();
            var mask=panel.transform.parent.GetComponent<RectMask2D>();Assert.That(mask,Is.Not.Null);
            var rect=GameplayPixelLayoutController.ScreenRect((RectTransform)mask.transform);
            Assert.That(rect.x,Is.EqualTo(18*scale).Within(.01f));
            Assert.That(rect.y,Is.EqualTo((640-197)*scale).Within(.01f));
            Assert.That(rect.width,Is.EqualTo(80*scale).Within(.01f));
            Assert.That(rect.height,Is.EqualTo(167*scale).Within(.01f));
            yield return Capture("Normal",scale);
            panel.enabled=false;yield return Capture("Baseline",scale);
            var probe=new GameObject("Diagnostic excess fill - never saved",typeof(RectTransform),typeof(Image));
            var pr=(RectTransform)probe.transform;pr.SetParent(mask.transform,false);
            pr.anchorMin=pr.anchorMax=pr.pivot=new Vector2(0,1);pr.anchoredPosition=new Vector2(-40,40);
            pr.sizeDelta=new Vector2(160,247);var pi=probe.GetComponent<Image>();pi.color=new Color32(255,0,255,255);pi.raycastTarget=false;
            yield return Capture("MaskedProbe",scale);
            mask.enabled=false;yield return Capture("UnmaskedControl",scale);
            mask.enabled=true;probe.SetActive(false);Object.Destroy(probe);panel.enabled=true;
            yield return Capture("Restored",scale);
        }
        yield return new ExitPlayMode();
        Assert.That(Hash("ProjectSettings/EditorBuildSettings.asset"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage02/BuildSettings.sha256")));
        Assert.That(Hash("Assets/_Game/Scenes/Game.unity"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage02/GameScene.sha256")));
    }
}
