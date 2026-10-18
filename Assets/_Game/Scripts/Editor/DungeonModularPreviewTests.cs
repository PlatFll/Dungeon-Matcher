using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class DungeonModularPreviewTests
{
    static Dictionary<string,string> SaveSnapshot()
    {
        var result = new Dictionary<string,string>();
        if (!Directory.Exists(Application.persistentDataPath)) return result;
        using (var sha = SHA256.Create())
            foreach (var file in Directory.GetFiles(Application.persistentDataPath,"*",SearchOption.AllDirectories))
                result[file] = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(file)));
        return result;
    }
    static Texture2D Capture(DungeonModularArtPreview preview)
    {
        Canvas.ForceUpdateCanvases();
        var previous = RenderTexture.active;
        preview.previewCamera.Render();
        RenderTexture.active = preview.previewCamera.targetTexture;
        var image = new Texture2D(preview.width,preview.height,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,preview.width,preview.height),0,0); image.Apply();
        RenderTexture.active = previous;
        return image;
    }
    static Rect PhysicalBounds(RectTransform rect, Camera camera)
    {
        var corners = new Vector3[4]; rect.GetWorldCorners(corners);
        Vector2 min = camera.WorldToScreenPoint(corners[0]), max = camera.WorldToScreenPoint(corners[2]);
        return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
    }
    static void CheckOwners()
    {
        var forbidden = new HashSet<string>{"BoardController","BoardVisuals","CombatController","WaveController","RunSession",
            "PlayerActor","EnemyActor","RunProgression","RunUpgradeController","EventSystem","SpriteRenderer"};
        var found = Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None)
            .Where(c=>c!=null && forbidden.Contains(c.GetType().Name)).Select(c=>c.GetType().Name).ToArray();
        Assert.That(found,Is.Empty,"Art preview must not instantiate gameplay, characters or gems.");
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==DungeonModularPreviewTools.ScenePath),Is.False);
    }
    [Test] public void NativeImportSettingsAndSourceBytesArePreserved()
    {
        var files=Directory.GetFiles(DungeonModularPreviewTools.AssetsRoot,"*.png",SearchOption.AllDirectories);
        Assert.That(files.Length,Is.GreaterThanOrEqualTo(80));
        foreach(var path in files)
        {
            string normalized=path.Replace('\\','/');
            var importer=(TextureImporter)AssetImporter.GetAtPath(normalized);
            Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point),path);
            Assert.That(importer.mipmapEnabled,Is.False,path);
            Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed),path);
            Assert.That(importer.spritePixelsPerUnit,Is.EqualTo(64),path);
            if(Path.GetFileName(path).StartsWith("EmptyCell",StringComparison.Ordinal))Assert.That(importer.spriteBorder,Is.EqualTo(new Vector4(8,8,8,8)),path);
            var source=DungeonModularPreviewTools.Kit+"/Exports/"+normalized.Substring((DungeonModularPreviewTools.AssetsRoot+"/").Length);
            CollectionAssert.AreEqual(File.ReadAllBytes(source),File.ReadAllBytes(path),path);
        }
    }
    [UnityTest] public IEnumerator PortraitCapturesAndActualFrameClipping()
    {
        var saves=SaveSnapshot();
        string buildSettings=File.ReadAllText("ProjectSettings/EditorBuildSettings.asset");
        EditorSceneManager.OpenScene(DungeonModularPreviewTools.ScenePath);
        var preview=Object.FindFirstObjectByType<DungeonModularArtPreview>();
        Assert.That(preview,Is.Not.Null);
        string output=DungeonModularPreviewTools.Kit+"/Review/Unity";
        Directory.CreateDirectory(output);
        var evidence=new List<string>();
        var sizes=new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400),new Vector2Int(1080,2400)};
        for(int index=0;index<sizes.Length;index++)
        {
            var size=sizes[index];preview.width=size.x;preview.height=size.y;
            preview.safeArea=index==3?new Rect(24,80,1032,2200):new Rect(0,0,size.x,size.y);
            var target=new RenderTexture(size.x,size.y,24){antiAliasing=1,filterMode=FilterMode.Point};
            preview.previewCamera.targetTexture=target;
            preview.Rebuild();CheckOwners();
            Assert.That(preview.BoardInterior.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("EmptyCell",StringComparison.Ordinal)),Is.EqualTo(64));
            for(int frame=0;frame<5;frame++)yield return null;
            var used=preview.previewCanvas.GetComponentsInChildren<Image>().Where(i=>i.sprite!=null).Select(i=>i.sprite.name).ToHashSet();
            foreach(var module in preview.modules)Assert.That(used.Contains(module.name),Is.True,"Module missing from scene: "+module.name);
            foreach(var label in preview.previewCanvas.GetComponentsInChildren<TMPro.TMP_Text>())
            {
                label.ForceMeshUpdate();
                Assert.That(label.textInfo.lineCount,Is.EqualTo(1),"Preview plaque label wrapped: "+label.text);
                Assert.That(label.textBounds.size.x,Is.LessThanOrEqualTo(label.rectTransform.rect.width+.01f));
            }
            foreach(var piece in preview.FixedPieces)
            {
                var actual=PhysicalBounds(piece,preview.previewCamera);
                Assert.That(actual.width,Is.EqualTo(piece.sizeDelta.x*preview.Geometry.Scale).Within(.01),piece.name);
                Assert.That(actual.height,Is.EqualTo(piece.sizeDelta.y*preview.Geometry.Scale).Within(.01),piece.name);
                Assert.That(actual.x,Is.EqualTo(Mathf.Round(actual.x)).Within(.01),piece.name);
                Assert.That(actual.y,Is.EqualTo(Mathf.Round(actual.y)).Within(.01),piece.name);
            }
            var baseline=Capture(preview);
            var baselinePixels=baseline.GetPixels32();
            Assert.That(baselinePixels.Distinct().Count(),Is.GreaterThan(50),"Actual rendered art, not a blank camera.");
            string name=$"{size.x}x{size.y}"+(index==3?"-safe-area":"");
            File.WriteAllBytes(output+"/"+name+".png",baseline.EncodeToPNG());
            foreach(var interior in new[]{preview.PortraitInterior,preview.BoardInterior})
            {
                Assert.That(interior.GetComponent<RectMask2D>(),Is.Not.Null);
                var bounds=PhysicalBounds(interior,preview.previewCamera);
                var probe=GameUi.Rect("TemporaryContainmentProbe",interior,interior.sizeDelta+Vector2.one*200,interior.sizeDelta/2);
                var fill=probe.gameObject.AddComponent<Image>();fill.color=Color.green;fill.raycastTarget=false;
                for(int frame=0;frame<3;frame++)yield return null;
                var tested=Capture(preview);var pixels=tested.GetPixels32();int changed=0;
                for(int i=0;i<pixels.Length;i++)
                    if(!pixels[i].Equals(baselinePixels[i]))
                    {
                        changed++;
                        Assert.That(bounds.Contains(new Vector2(i%size.x+.5f,i/size.x+.5f)),Is.True,
                            $"{name}: {interior.name} fill escaped at {i%size.x},{i/size.x}");
                    }
                Assert.That(changed,Is.GreaterThan(1000),"Probe must actually render inside the mask.");
                Object.DestroyImmediate(probe.gameObject);Object.DestroyImmediate(tested);
                for(int frame=0;frame<3;frame++)yield return null;
                evidence.Add(name+": "+interior.name+" oversized fill clipped exactly; "+changed+" visible test pixels.");
            }
            Object.DestroyImmediate(baseline);preview.previewCamera.targetTexture=null;
            target.Release();Object.DestroyImmediate(target);
        }
        CheckOwners();CollectionAssert.AreEquivalent(saves,SaveSnapshot(),"Preview must not alter persistent saves.");
        Assert.That(File.ReadAllText("ProjectSettings/EditorBuildSettings.asset"),Is.EqualTo(buildSettings));
        evidence.Add("Four actual Unity camera captures; fixed sprites on integer physical grid; no gameplay owners; persistent files unchanged; release build settings unchanged.");
        File.WriteAllLines(DungeonModularPreviewTools.Kit+"/Evidence/UnityChecks.txt",evidence);
        EditorSceneManager.OpenScene(DungeonModularPreviewTools.ScenePath);
    }
}
