using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class DungeonSideViewPreviewTests
{
    [System.Serializable] sealed class NativeAsset
    {
        public string name, group;
        public int width, height, canvasPadding;
    }
    [System.Serializable] sealed class NativeManifest
    {
        public NativeAsset[] assets;
        public DungeonSideViewArtPreview.Placement[] placements;
    }

    [Test] public void ContinuousWallAndCompletePropsRenderBehindUi()
    {
        var manifest = JsonUtility.FromJson<NativeManifest>(File.ReadAllText(DungeonSideViewPreviewTools.Kit + "/AssetManifest.json"));
        string[] retired = {"LowerWall", "LeftSideMasonry", "RightSideMasonry", "PlatformSupportA", "PlatformSupportB", "PlatformSupportC", "PlatformSupportD", "PlatformSupportE"};
        Assert.That(manifest.assets.Any(a => retired.Contains(a.name)), Is.False);
        Assert.That(manifest.placements.Where(p => p.layer == 0).All(p => p.name == "BackWallReference"), Is.True);
        Assert.That(manifest.placements.Where(p => p.name.StartsWith("Cell") || p.name == "PortraitWall").All(p => p.layer > 30), Is.True);
        foreach (var name in new[] {"LowerLeftConnector", "LowerRightConnector"})
        {
            var asset = manifest.assets.Single(a => a.name == name);
            Assert.That(asset.width, Is.GreaterThanOrEqualTo(68));
            Assert.That(asset.height, Is.GreaterThanOrEqualTo(164));
        }
        foreach (var asset in manifest.assets.Where(a => a.canvasPadding > 0))
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(texture.LoadImage(File.ReadAllBytes(DungeonSideViewPreviewTools.Kit + "/Exports/" + asset.group + "/" + asset.name + ".png")), Is.True);
                var pixels = texture.GetPixels32();
                for (int y = 0; y < texture.height; y++)
                    for (int x = 0; x < texture.width; x++)
                        if (x < asset.canvasPadding || y < asset.canvasPadding || x >= texture.width - asset.canvasPadding || y >= texture.height - asset.canvasPadding)
                            Assert.That(pixels[y * texture.width + x].a, Is.Zero, asset.name + " must keep transparent contour clearance.");
            }
            finally { Object.DestroyImmediate(texture); }
        }
        EditorSceneManager.OpenScene(DungeonSideViewPreviewTools.ScenePath);
        var preview = Object.FindFirstObjectByType<DungeonSideViewArtPreview>();
        preview.Rebuild();
        foreach (var piece in preview.Pieces.Where(p => p.name.StartsWith("Cell") || p.name == "PortraitWall"))
        {
            var layer = piece.transform;
            while (layer.parent != preview.Content) layer = layer.parent;
            Assert.That(layer.GetSiblingIndex(), Is.GreaterThan(3), piece.name + " must cover expanded scenery.");
        }
    }

    [Test] public void NativeImportsMatchLibreSpriteExports()
    {
        var files=Directory.GetFiles(DungeonSideViewPreviewTools.AssetsRoot,"*.png",SearchOption.AllDirectories);
        Assert.That(files.Length,Is.GreaterThan(100));
        foreach(var f in files)
        {
            var path=f.Replace('\\','/');var i=(TextureImporter)AssetImporter.GetAtPath(path);
            Assert.That(i.filterMode,Is.EqualTo(FilterMode.Point),path);Assert.That(i.mipmapEnabled,Is.False,path);
            Assert.That(i.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed),path);
            Assert.That(i.spritePixelsPerUnit,Is.EqualTo(64),path);Assert.That(i.npotScale,Is.EqualTo(TextureImporterNPOTScale.None),path);
            var source=DungeonSideViewPreviewTools.Kit+"/Exports/"+path.Substring((DungeonSideViewPreviewTools.AssetsRoot+"/").Length);
            CollectionAssert.AreEqual(File.ReadAllBytes(source),File.ReadAllBytes(path),path);
        }
    }
    [UnityTest] public IEnumerator ReferenceCompositionAndIntegerPixelCaptures()
    {
        var settings=File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset");
        var sceneBytes=File.ReadAllBytes(DungeonSideViewPreviewTools.ScenePath);
        EditorSceneManager.OpenScene(DungeonSideViewPreviewTools.ScenePath);
        var preview=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();Assert.That(preview,Is.Not.Null);
        string output=DungeonSideViewPreviewTools.Kit+"/Review/Unity";Directory.CreateDirectory(output);
        var sizes=new[]{new Vector2Int(360,640),new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400),new Vector2Int(1080,2400)};
        for(int index=0;index<sizes.Length;index++)
        {
            var size=sizes[index];preview.width=size.x;preview.height=size.y;
            preview.safeArea=index==4?new Rect(24,80,1032,2200):new Rect(0,0,size.x,size.y);
            var target=new RenderTexture(size.x,size.y,24){antiAliasing=1,filterMode=FilterMode.Point};preview.previewCamera.targetTexture=target;
            preview.Rebuild();for(int frame=0;frame<4;frame++)yield return null;Canvas.ForceUpdateCanvases();
            Assert.That(preview.Content.sizeDelta,Is.EqualTo(new Vector2(360,640)));
            Assert.That(preview.Content.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("Cell")),Is.EqualTo(64));
            Assert.That(preview.ContentPixels.xMin,Is.GreaterThanOrEqualTo(preview.safeArea.xMin));
            Assert.That(preview.ContentPixels.xMax,Is.LessThanOrEqualTo(preview.safeArea.xMax));
            Assert.That(preview.ContentPixels.yMin,Is.GreaterThanOrEqualTo(preview.safeArea.yMin));
            Assert.That(preview.ContentPixels.yMax,Is.LessThanOrEqualTo(preview.safeArea.yMax));
            foreach(var piece in preview.Pieces)
            {
                var c=new Vector3[4];piece.GetWorldCorners(c);var a=preview.previewCamera.WorldToScreenPoint(c[0]);var b=preview.previewCamera.WorldToScreenPoint(c[2]);
                Assert.That(b.x-a.x,Is.EqualTo(piece.sizeDelta.x*preview.PixelScale).Within(.02),piece.name);
                Assert.That(b.y-a.y,Is.EqualTo(piece.sizeDelta.y*preview.PixelScale).Within(.02),piece.name);
                Assert.That(a.x,Is.EqualTo(Mathf.Round(a.x)).Within(.02),piece.name);Assert.That(a.y,Is.EqualTo(Mathf.Round(a.y)).Within(.02),piece.name);
            }
            string[] forbidden={"BoardController","BoardVisuals","CombatController","WaveController","RunSession","PlayerActor","EnemyActor","EventSystem"};
            Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>c&&forbidden.Contains(c.GetType().Name)),Is.Empty);
            var previous=RenderTexture.active;preview.previewCamera.Render();RenderTexture.active=target;
            var image=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
            File.WriteAllBytes(output+"/"+size.x+"x"+size.y+(index==4?"-safe-area":"")+".png",image.EncodeToPNG());
            RenderTexture.active=previous;preview.previewCamera.targetTexture=null;Object.DestroyImmediate(image);Object.DestroyImmediate(target);
        }
        CollectionAssert.AreEqual(settings,File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset"));
        CollectionAssert.AreEqual(sceneBytes,File.ReadAllBytes(DungeonSideViewPreviewTools.ScenePath));
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==DungeonSideViewPreviewTools.ScenePath),Is.False);
    }
}
