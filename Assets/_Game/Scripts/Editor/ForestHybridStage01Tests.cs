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

/// <summary>Candidate Forest foundation only. No production scene or gameplay binding is changed.</summary>
public sealed class ForestHybridStage01Tests
{
    const string Root = "ArtSource/ForestHybrid/Stage01";
    const string Art = "Assets/_Game/Art/ForestHybrid/Stage01";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestHybrid_Stage01.unity";
    [Serializable] public class Piece { public string name, path; public int x, y, layer; }
    [Serializable] public class Probe { public string name; public int x, y; }
    [Serializable] public class Layout { public int width, height, standing_y; public Piece[] placements; public Probe[] probes; }
    [Serializable] public class SourceEvidence { public string path, sprite; public Rect nativeRect, screenRect; }
    [Serializable] public class CaptureEvidence { public int width, height, scale; public bool probes; public SourceEvidence[] sources; }

    static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
        return rect;
    }
    static Image AddImage(string name, Transform parent, Sprite sprite, int x, int y)
    {
        Assert.That(sprite, Is.Not.Null, name);
        var image = Rect(name, parent, x, y, sprite.rect.width, sprite.rect.height).gameObject.AddComponent<Image>();
        image.sprite = sprite; image.color = Color.white; image.raycastTarget = false;
        return image;
    }
    static Sprite Ready(string name) =>
        AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/CombatIdles/" + name + "_Idle.png")
        .OfType<Sprite>().Single(s => s.name == name + "_Idle_00");

    [MenuItem("Dungeon Matcher/Art Preview/Forest Hybrid Stage 01 foundation")]
    public static void Build()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        // Strictly scoped candidate import. Existing Forest, Dungeon, cast and shared UI importers stay untouched.
        foreach (string path in Directory.GetFiles(Art, "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(.5f,.5f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
        var m = JsonUtility.FromJson<Layout>(File.ReadAllText(Root + "/Placement.json"));
        Assert.That(new Vector2Int(m.width,m.height), Is.EqualTo(new Vector2Int(360,240)));
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Forest Stage 01 native study camera").AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = m.height/128f;
        camera.aspect = (float)m.width/m.height; camera.transform.position = new Vector3(0,0,-10);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(20,39,45,255);
        camera.allowHDR = false; camera.allowMSAA = false;
        var canvas = new GameObject("Forest foundation - separate native layers",typeof(RectTransform)).AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = camera;
        canvas.referencePixelsPerUnit = 64; canvas.pixelPerfect = true;
        var root = (RectTransform)canvas.transform;
        root.sizeDelta = new Vector2(m.width,m.height); root.pivot = Vector2.zero;
        root.position = new Vector3(-m.width/128f,-m.height/128f,0); root.localScale = Vector3.one/64;
        foreach(var p in m.placements.OrderBy(p=>p.layer))
            AddImage(p.name,root,AssetDatabase.LoadAssetAtPath<Sprite>(p.path),p.x,p.y);
        var probes = Rect("Approved cast - static scale probes",root,0,0,m.width,m.height);
        foreach(var p in m.probes) AddImage(p.name+" reference",probes,Ready(p.name),p.x,p.y);
        EditorSceneManager.SaveScene(scene,ScenePath);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False,
            "Candidate scene must not enter the production build.");
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
        .Invoke(null,new object[]{new Vector2Int(360*scale,240*scale)});
    [UnityTearDown] public IEnumerator Cleanup()
    { if(Application.isPlaying) yield return new ExitPlayMode(); }

    [UnityTest, Explicit("Builds the isolated Forest Stage 01 scene and captures actual Unity Game View.")]
    public IEnumerator FoundationNativeScreens()
    {
        Directory.CreateDirectory(Root+"/Previews"); Directory.CreateDirectory(Root+"/Diagnostics");
        Build(); SetSize(1); yield return new EnterPlayMode();
        var probes=GameObject.Find("Approved cast - static scale probes");
        foreach(bool showProbes in new[]{false,true})
        foreach(int scale in new[]{1,2,3})
        {
            probes.SetActive(showProbes); SetSize(scale);
            float sizeDeadline=Time.realtimeSinceStartup+20;
            while(Screen.width!=360*scale || Screen.height!=240*scale)
            { Assert.That(Time.realtimeSinceStartup,Is.LessThan(sizeDeadline),"Game View size timed out."); yield return null; }
            Canvas.ForceUpdateCanvases(); yield return null; yield return null;
            var sources=new List<SourceEvidence>();
            foreach(var image in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
            {
                var sprite=image.sprite;
                var rect=GameplayPixelLayoutController.ScreenRect(image.rectTransform);
                Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                Assert.That(sprite.texture.mipmapCount,Is.EqualTo(1));
                Assert.That(image.color,Is.EqualTo(Color.white));
                Assert.That(image.raycastTarget,Is.False);
                Assert.That(rect.width,Is.EqualTo(sprite.rect.width*scale).Within(.01f));
                Assert.That(rect.height,Is.EqualTo(sprite.rect.height*scale).Within(.01f));
                Assert.That(rect.x,Is.EqualTo(Mathf.Round(rect.x)).Within(.01f));
                Assert.That(rect.y,Is.EqualTo(Mathf.Round(rect.y)).Within(.01f));
                // Only repeat scenery may extend past the viewport; independent tree/anchor silhouettes must fit.
                bool repeating=image.name.Contains("Repeat") || image.name.Contains("Extension");
                if(!repeating)
                {
                    Assert.That(rect.xMin,Is.GreaterThanOrEqualTo(-.01f));
                    Assert.That(rect.yMin,Is.GreaterThanOrEqualTo(-.01f));
                    Assert.That(rect.xMax,Is.LessThanOrEqualTo(Screen.width+.01f));
                    Assert.That(rect.yMax,Is.LessThanOrEqualTo(Screen.height+.01f));
                }
                sources.Add(new SourceEvidence{path=AssetDatabase.GetAssetPath(sprite),sprite=sprite.name,
                    nativeRect=sprite.rect,screenRect=rect});
            }
            string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem"};
            Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
            var evidence=new CaptureEvidence{width=Screen.width,height=Screen.height,scale=scale,probes=showProbes,sources=sources.ToArray()};
            string label=(showProbes?"Unity-cast-":"Unity-structure-")+Screen.width+"x"+Screen.height;
            File.WriteAllText(Root+"/Diagnostics/"+label+".json",JsonUtility.ToJson(evidence,true));
            string output=Path.GetFullPath(Root+"/Previews/"+label+".png");
            if(File.Exists(output)) File.Delete(output);
            ScreenCapture.CaptureScreenshot(output);
            float deadline=Time.realtimeSinceStartup+20;
            while(!File.Exists(output)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
        }
        yield return new ExitPlayMode();
    }
}
