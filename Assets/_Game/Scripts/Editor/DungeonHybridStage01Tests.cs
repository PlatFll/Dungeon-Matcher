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

/// <summary>Editor-only structural study; reuses approved sprites without importing replacements.</summary>
public sealed class DungeonHybridStage01Tests
{
    const string Root = "ArtSource/DungeonHybrid/Stage01";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage01.unity";
    [Serializable] public class Piece { public string name, path; public int x, y, layer; }
    [Serializable] public class Layout { public int width, height; public Piece[] placements; public int[] portrait_mask; public string portrait_repeat; }
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
    static Sprite Ready(string name) => AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/CombatIdles/" + name + "_Idle.png")
        .OfType<Sprite>().Single(s => s.name == name + "_Idle_00");

    [MenuItem("Dungeon Matcher/Art Preview/Hybrid Stage 01 structural study")]
    public static void Build()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var m = JsonUtility.FromJson<Layout>(File.ReadAllText(Root + "/Placement.json"));
        Assert.That(new Vector2Int(m.width,m.height),Is.EqualTo(new Vector2Int(360,240)));
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Stage 01 native study camera").AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = m.height / 128f;
        camera.aspect = (float)m.width / m.height; camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(10,13,17,255);
        camera.allowHDR = false; camera.allowMSAA = false;
        var canvas = new GameObject("Structural study - separate native pieces", typeof(RectTransform)).AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = camera;
        canvas.referencePixelsPerUnit = 64; canvas.pixelPerfect = true;
        var root = (RectTransform)canvas.transform; root.sizeDelta = new Vector2(m.width,m.height);
        root.pivot = Vector2.zero; root.position = new Vector3(-m.width/128f,-m.height/128f,0); root.localScale = Vector3.one/64;
        foreach (var p in m.placements.OrderBy(p => p.layer))
            AddImage(p.name,root,AssetDatabase.LoadAssetAtPath<Sprite>(p.path),p.x,p.y);
        var bounds = m.portrait_mask;
        var mask = Rect("Portrait recess - overfilled repeat clipped to interior",root,bounds[0],bounds[1],bounds[2],bounds[3]);
        mask.gameObject.AddComponent<RectMask2D>();
        var tile = AssetDatabase.LoadAssetAtPath<Sprite>(m.portrait_repeat);
        for(int y=-(int)tile.rect.height;y<bounds[3]+tile.rect.height;y+=(int)tile.rect.height)
            for(int x=-(int)tile.rect.width;x<bounds[2]+tile.rect.width;x+=(int)tile.rect.width)
                AddImage("Recess repeat",mask,tile,x,y);
        // Reference figures only. No actors, controllers, motion or gameplay owners.
        var probes = Rect("Approved cast - static scale probes",root,0,0,m.width,m.height);
        AddImage("Rattlebones reference",probes,Ready("Rattlebones"),27,117);
        AddImage("Farmer reference",probes,Ready("Farmer"),105,100);
        AddImage("Pan reference",probes,Ready("PanVillager"),225,100);
        EditorSceneManager.SaveScene(scene,ScenePath);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
        .Invoke(null,new object[]{new Vector2Int(360*scale,240*scale)});
    [UnityTearDown] public IEnumerator Cleanup()
    { if(Application.isPlaying) yield return new ExitPlayMode(); }

    [UnityTest, Explicit("Builds only the isolated hybrid Stage 01 scene and captures actual Game View.")]
    public IEnumerator StructuralStudyNativeScreens()
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
