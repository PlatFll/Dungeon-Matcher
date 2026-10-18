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

/// <summary>Explicit art-only Stage 01 import and real Game View evidence.</summary>
public sealed class DungeonPixelPipelineStage01Tests
{
    const string Root = "ArtSource/DungeonPixelLab";
    const string Assets = "Assets/_Game/Art/DungeonPixelLab";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity";
    [Serializable] public class Evidence
    {
        public string unityVersion;
        public int width, height, sourceScale, pieces;
        public bool integerAligned, exactNativeSizes, noGameplayOwners;
    }
    static void SetSize(Vector2Int size) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize", BindingFlags.Static | BindingFlags.NonPublic)
        .Invoke(null, new object[] {size});
    static string Hash(string path)
    {
        using(var sha=System.Security.Cryptography.SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)));
    }
    static void Prepare()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(var path in Directory.GetFiles(Assets+"/Wall", "*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=64; importer.filterMode=FilterMode.Point;
            importer.mipmapEnabled=false; importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.npotScale=TextureImporterNPOTScale.None; importer.wrapMode=TextureWrapMode.Clamp;
            importer.alphaIsTransparency=false;
            importer.ClearPlatformTextureSettings("Android");
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteExtrude=0;
            importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        var preview=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();
        Assert.That(preview,Is.Not.Null);
        preview.name="Dungeon PixelLab Stage 01 wall material - review candidate";
        preview.modules=Directory.GetFiles(Assets+"/Wall","*.png")
            .Select(p=>AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\','/'))).ToArray();
        preview.placementManifest=AssetDatabase.LoadAssetAtPath<TextAsset>(Assets+"/Stage01WallPlacement.json");
        preview.width=360;preview.height=640;preview.safeArea=new Rect(0,0,360,640);
        preview.Rebuild();
        Assert.That(preview.Pieces.Count,Is.EqualTo(112));
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
    }
    [UnityTearDown] public IEnumerator Cleanup()
    { if(Application.isPlaying)yield return new ExitPlayMode(); }
    [UnityTest, Explicit("Stage 01 art-only capture; rebuilds only the candidate preview.")]
    public IEnumerator Stage01WallNativePreview()
    {
        Directory.CreateDirectory(Root+"/Previews/Stage01");
        Directory.CreateDirectory(Root+"/Diagnostics/Stage01");
        File.WriteAllText(Root+"/Diagnostics/Stage01/BuildSettings.sha256",Hash("ProjectSettings/EditorBuildSettings.asset"));
        File.WriteAllText(Root+"/Diagnostics/Stage01/GameScene.sha256",Hash("Assets/_Game/Scenes/Game.unity"));
        Prepare();SetSize(new Vector2Int(360,640));
        yield return new EnterPlayMode();
        foreach(var size in new[]{new Vector2Int(360,640),new Vector2Int(1080,1920)})
        {
            SetSize(size);yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Screen.width,Is.EqualTo(size.x));Assert.That(Screen.height,Is.EqualTo(size.y));
            var preview=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();
            preview.width=size.x;preview.height=size.y;preview.safeArea=new Rect(0,0,size.x,size.y);preview.Rebuild();
            yield return null;yield return null;Canvas.ForceUpdateCanvases();
            int scale=size.x/360;
            bool aligned=true,native=true;
            foreach(var piece in preview.Pieces)
            {
                var image=piece.GetComponent<Image>();var source=image.sprite;
                var bounds=GameplayPixelLayoutController.ScreenRect(piece);
                aligned&=Mathf.Abs(bounds.x-Mathf.Round(bounds.x))<.01f&&Mathf.Abs(bounds.y-Mathf.Round(bounds.y))<.01f;
                native&=Mathf.Abs(bounds.width-source.rect.width*scale)<.01f&&Mathf.Abs(bounds.height-source.rect.height*scale)<.01f;
                Assert.That(source.pixelsPerUnit,Is.EqualTo(64));
                Assert.That(image.color,Is.EqualTo(Color.white));
                Assert.That(bounds.xMin,Is.GreaterThanOrEqualTo(-.01f));Assert.That(bounds.yMin,Is.GreaterThanOrEqualTo(-.01f));
                Assert.That(bounds.xMax,Is.LessThanOrEqualTo(size.x+.01f));Assert.That(bounds.yMax,Is.LessThanOrEqualTo(size.y+.01f));
            }
            string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
            bool clean=!Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Any(c=>c&&forbidden.Contains(c.GetType().Name));
            Assert.That(aligned&&native&&clean,Is.True);
            Assert.That(preview.PixelScale,Is.EqualTo(scale));
            var evidence=new Evidence{unityVersion=Application.unityVersion,width=size.x,height=size.y,
                sourceScale=scale,pieces=preview.Pieces.Count,integerAligned=aligned,exactNativeSizes=native,noGameplayOwners=clean};
            string label="UnityWall-"+size.x+"x"+size.y;
            File.WriteAllText(Root+"/Diagnostics/Stage01/"+label+".json",JsonUtility.ToJson(evidence,true));
            string output=Path.GetFullPath(Root+"/Previews/Stage01/"+label+".png");
            if(File.Exists(output))File.Delete(output);
            ScreenCapture.CaptureScreenshot(output);
            float deadline=Time.realtimeSinceStartup+20;
            while(!File.Exists(output)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
        }
        yield return new ExitPlayMode();
        Assert.That(Hash("ProjectSettings/EditorBuildSettings.asset"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage01/BuildSettings.sha256")));
        Assert.That(Hash("Assets/_Game/Scenes/Game.unity"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage01/GameScene.sha256")));
    }
}
