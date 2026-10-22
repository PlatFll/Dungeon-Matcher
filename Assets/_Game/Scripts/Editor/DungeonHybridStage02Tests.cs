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
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Editor-only gate study. Opens a copy of Stage 01 and imports only the new Stage 02 modules.</summary>
public sealed class DungeonHybridStage02Tests
{
    const string Root = "ArtSource/DungeonHybrid/Stage02";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage02.unity";
    [Serializable] public class Piece { public string name, path; public int width, height, layer; public int[] offset; }
    [Serializable] public class Assembly { public int width, height; public int[] scene_origin; public Piece[] parts; }
    [Serializable] public class SourceEvidence { public string path, sprite; public Rect nativeRect, screenRect; }
    [Serializable] public class CaptureEvidence { public int width, height, scale; public bool probes; public SourceEvidence[] sources; }

    static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
        return rect;
    }
    static Image AddImage(string name, Transform parent, Sprite sprite, int x, int y)
    {
        Assert.That(sprite, Is.Not.Null, name);
        var rect = Rect(name, parent, x, y, sprite.rect.width, sprite.rect.height);
        var image = rect.gameObject.AddComponent<Image>(); image.sprite = sprite;
        image.color = Color.white; image.raycastTarget = false;
        return image;
    }
    [MenuItem("Dungeon Matcher/Art Preview/Hybrid Stage 02 gate study")]
    public static void Build()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var m = JsonUtility.FromJson<Assembly>(File.ReadAllText(Root + "/Assembly.json"));
        Assert.That(m.parts.Length,Is.EqualTo(10));
        foreach(var p in m.parts)
        {
            Assert.That(p.path.StartsWith("Assets/_Game/Art/DungeonHybrid/Stage02/"),Is.True);
            var importer=(TextureImporter)AssetImporter.GetAtPath(p.path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=64; importer.filterMode=FilterMode.Point; importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.npotScale=TextureImporterNPOTScale.None;
            importer.wrapMode=TextureWrapMode.Clamp; importer.alphaIsTransparency=false; importer.maxTextureSize=2048;
            var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        // Stage 01 is a read-only base. Save the new scene under its own path only.
        var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage01.unity",OpenSceneMode.Single);
        var canvas=Object.FindFirstObjectByType<Canvas>();
        var root=(RectTransform)canvas.transform;
        int floorIndex=Enumerable.Range(0,root.childCount).First(i=>root.GetChild(i).name.StartsWith("Floor_"));
        var gate=Rect("Stage 02 gate - ten separate native modules",root,m.scene_origin[0],m.scene_origin[1],m.width,m.height);
        gate.SetSiblingIndex(floorIndex);
        foreach(var p in m.parts.OrderBy(p=>p.layer))
        {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(p.path);
            Assert.That(sprite.rect.size,Is.EqualTo(new Vector2(p.width,p.height)));
            AddImage(p.name,gate,sprite,p.offset[0],p.offset[1]);
        }
        Assert.That(gate.GetComponentsInChildren<Image>().Length,Is.EqualTo(10));
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
        EditorSceneManager.SaveScene(scene,ScenePath);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
        .Invoke(null,new object[]{new Vector2Int(360*scale,240*scale)});
    [UnityTearDown] public IEnumerator Cleanup()
    { if(Application.isPlaying) yield return new ExitPlayMode(); }

    [UnityTest, Explicit("Builds only the isolated hybrid Stage 02 scene and captures actual Game View.")]
    public IEnumerator GateAssemblyNativeScreens()
    {
        Directory.CreateDirectory(Root+"/Previews"); Directory.CreateDirectory(Root+"/Diagnostics");
        Build(); SetSize(1); yield return new EnterPlayMode();
        var probes=GameObject.Find("Approved cast - static scale probes");
        var evidence=new List<CaptureEvidence>();
        foreach(bool showProbes in new[]{false,true})
        foreach(int scale in new[]{1,2,3})
        {
            probes.SetActive(showProbes); SetSize(scale); yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Screen.width,Is.EqualTo(360*scale)); Assert.That(Screen.height,Is.EqualTo(240*scale));
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            var images=Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
            var sources=new List<SourceEvidence>();
            foreach(var image in images)
            {
                var sprite=image.sprite; var rect=GameplayPixelLayoutController.ScreenRect(image.rectTransform);
                Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                Assert.That(image.color,Is.EqualTo(Color.white)); Assert.That(image.raycastTarget,Is.False);
                Assert.That(rect.width,Is.EqualTo(sprite.rect.width*scale).Within(.01f));
                Assert.That(rect.height,Is.EqualTo(sprite.rect.height*scale).Within(.01f));
                Assert.That(rect.x,Is.EqualTo(Mathf.Round(rect.x)).Within(.01f));
                Assert.That(rect.y,Is.EqualTo(Mathf.Round(rect.y)).Within(.01f));
                if(!image.GetComponentInParent<RectMask2D>())
                {
                    Assert.That(rect.xMin,Is.GreaterThanOrEqualTo(-.01f)); Assert.That(rect.yMin,Is.GreaterThanOrEqualTo(-.01f));
                    Assert.That(rect.xMax,Is.LessThanOrEqualTo(Screen.width+.01f)); Assert.That(rect.yMax,Is.LessThanOrEqualTo(Screen.height+.01f));
                }
                sources.Add(new SourceEvidence{path=AssetDatabase.GetAssetPath(sprite),sprite=sprite.name,nativeRect=sprite.rect,screenRect=rect});
            }
            string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
            Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
            evidence.Add(new CaptureEvidence{width=Screen.width,height=Screen.height,scale=scale,probes=showProbes,sources=sources.ToArray()});
            string label=(showProbes?"Unity-cast-":"Unity-structure-")+Screen.width+"x"+Screen.height;
            File.WriteAllText(Root+"/Diagnostics/"+label+".json",JsonUtility.ToJson(evidence.Last(),true));
            string output=Path.GetFullPath(Root+"/Previews/"+label+".png");
            if(File.Exists(output)) File.Delete(output);
            ScreenCapture.CaptureScreenshot(output);
            float deadline=Time.realtimeSinceStartup+20;
            while(!File.Exists(output)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
        }
        yield return new ExitPlayMode();
    }
}
