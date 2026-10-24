using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Stage 1 replaces only board-side dressing to an independent art review, without changing shared layout.</summary>
public sealed class ForestSurroundStage01Tests
{
    const string Root = "ArtSource/ForestSurroundRevision/Stage01";
    const string Art = "Assets/_Game/Art/ForestSurroundRevision/Stage01/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestSurround_Stage01.unity";
    const string Dressing = "Stage01 root-dominant board surrounds 2x";
    [Serializable] public class Placement { public string name, file; public int x, y; }
    [Serializable] public class Layout { public Placement[] pieces; }
    [Serializable] public class Use { public string name, path; public Rect rect; public int width, height; }
    [Serializable] public class Evidence { public int width, height, preservedImages; public Rect board, portrait, frame; public Use[] dressing; }
    [MenuItem("Dungeon Matcher/Art Preview/Forest surrounds stage 01 roots")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage05_Revision02.unity", OpenSceneMode.Single);
        var root = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>()).Single().transform;
        root.name = "Forest Surround Revision Stage 01";
        var rt = new GameObject(Dressing, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(root, false); rt.pivot = new Vector2(0,1); rt.anchorMin = rt.anchorMax = new Vector2(0,1);
        rt.sizeDelta = new Vector2(360,680); rt.anchoredPosition = Vector2.zero; rt.localScale = new Vector3(2,2,1);
        // Broad branches may extend safely behind the opaque board and HUD, without squeezing their silhouettes.
        rt.SetSiblingIndex(root.Find("Stage05 rich woodland dressing 2x").GetSiblingIndex());
        var layout = JsonUtility.FromJson<Layout>(File.ReadAllText(Root + "/Scripts/Placements.json"));
        foreach (var q in layout.pieces)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + q.file + ".png"); Assert.That(s,Is.Not.Null,q.file);
            var part = new GameObject(q.name,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();
            part.SetParent(rt,false); part.pivot = new Vector2(0,1); part.anchorMin = part.anchorMax = new Vector2(0,1);
            part.anchoredPosition = new Vector2(q.x,-q.y); part.sizeDelta = s.rect.size;
            var image = part.GetComponent<Image>(); image.sprite = s; image.raycastTarget = false;
        }
        ToggleVariant(root,true); Canvas.ForceUpdateCanvases(); Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath),Is.False);
        EditorSceneManager.SaveScene(scene,ScenePath);
    }
    static void ToggleVariant(Transform root,bool revised)
    {
        root.Find(Dressing).gameObject.SetActive(revised);
        // Only the first four boughs per side occupy the authorized board region.
        // The two lower boughs and both original rock anchors per side stay unchanged.
        var previous=root.Find("Stage05 rich woodland dressing 2x");
        foreach(string side in new[]{"Left","Right"})
            for(int n=1;n<=4;n++) previous.Find(side+" connected bough "+n).gameObject.SetActive(!revised);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static)
        .Invoke(null,new object[]{new Vector2Int(720*scale,1360*scale)});
    static Rect Screen(Transform t) => GameplayPixelLayoutController.ScreenRect((RectTransform)t);
    static IEnumerator Capture(string name)
    {
        string file = Path.GetFullPath(Root + "/Previews/" + name + ".png"); if(File.Exists(file))File.Delete(file);
        ScreenCapture.CaptureScreenshot(file); float deadline = Time.realtimeSinceStartup+20;
        while(!File.Exists(file)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
    }
    [UnityTearDown] public IEnumerator Cleanup(){if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit("Stage 1 roots: preserve approved art, shared geometry and unobstructed UI.")]
    public IEnumerator BoardSideRootReview()
    {
        Directory.CreateDirectory(Root+"/Diagnostics");Directory.CreateDirectory(Root+"/Previews");
        Build();SetSize(1);yield return new EnterPlayMode();yield return null;yield return null;
        var root = Object.FindFirstObjectByType<Canvas>().transform; var decor = root.Find(Dressing);
        ToggleVariant(root,false);Canvas.ForceUpdateCanvases();yield return null;yield return null;
        var previous = Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.enabled).ToArray();
        var savedSprites = previous.Select(i=>i.sprite).ToArray();var savedRects = previous.Select(i=>Screen(i.transform)).ToArray();
        yield return Capture("Unity-Before-720x1360");ToggleVariant(root,true);Canvas.ForceUpdateCanvases();
        for(int n=0;n<previous.Length;n++){Assert.That(previous[n].sprite,Is.SameAs(savedSprites[n]),previous[n].name);Assert.That(Screen(previous[n].transform),Is.EqualTo(savedRects[n]),previous[n].name);}
        foreach(int scale in new[]{1,2})
        {
            SetSize(scale);float deadline=Time.realtimeSinceStartup+20;
            while(ScreenWidth()!=720*scale||UnityEngine.Screen.height!=1360*scale){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
            Canvas.ForceUpdateCanvases();yield return null;yield return null;
            var board=(RectTransform)root.Find("Board contents native 64px");
            var portrait=(RectTransform)root.Find("Approved forest top modules 2x/ForestPortrait");
            var frame=(RectTransform)root.Find("Stage04 forest board frame 2x/BoardFrameTarget");
            Assert.That(board.rect.size,Is.EqualTo(new Vector2(512,512)));Assert.That(board.anchoredPosition,Is.EqualTo(new Vector2(104,-528)));
            Assert.That(portrait.rect.size,Is.EqualTo(new Vector2(98,220)));Assert.That(frame.rect.size,Is.EqualTo(new Vector2(290,290)));
            var uses = new List<Use>();
            foreach(var im in decor.GetComponentsInChildren<Image>())
            {
                var rect=Screen(im.transform);var s=im.sprite;
                Assert.That(im.color,Is.EqualTo(Color.white));Assert.That(im.raycastTarget,Is.False);
                Assert.That(s.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(s.texture.mipmapCount,Is.EqualTo(1));
                Assert.That(rect.width,Is.EqualTo(s.rect.width*2*scale).Within(.02));Assert.That(rect.height,Is.EqualTo(s.rect.height*2*scale).Within(.02));
                Assert.That(rect.x,Is.EqualTo(Mathf.Round(rect.x)).Within(.02));Assert.That(rect.y,Is.EqualTo(Mathf.Round(rect.y)).Within(.02));
                Assert.That(rect.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(rect.yMin,Is.GreaterThanOrEqualTo(0));
                Assert.That(rect.xMax,Is.LessThanOrEqualTo(720*scale));Assert.That(rect.yMax,Is.LessThanOrEqualTo(1360*scale));
                Assert.That(decor.GetSiblingIndex(),Is.LessThan(board.GetSiblingIndex()),"Dressing must remain behind the board");
                Assert.That(decor.GetSiblingIndex(),Is.LessThan(root.Find("Shared bottom HUD").GetSiblingIndex()),"Dressing must remain behind HUD");
                Assert.That(rect.yMax,Is.LessThanOrEqualTo((1360-480)*scale),im.name+" overlaps approved battle area");
                Assert.That(rect.yMin,Is.GreaterThanOrEqualTo((1360-1080)*scale),im.name+" overlaps deferred lower surroundings");
                uses.Add(new Use{name=im.name,path=AssetDatabase.GetAssetPath(s),rect=rect,width=(int)s.rect.width,height=(int)s.rect.height});
            }
            foreach(var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)){t.ForceMeshUpdate();Assert.That(t.isTextTruncated,Is.False,t.text);}
            File.WriteAllText(Root+"/Diagnostics/Unity-"+(720*scale)+"x"+(1360*scale)+".json",JsonUtility.ToJson(new Evidence{width=720*scale,height=1360*scale,preservedImages=previous.Length,board=Screen(board),portrait=Screen(portrait),frame=Screen(frame),dressing=uses.ToArray()},true));
            yield return Capture("Unity-Stage01-"+(720*scale)+"x"+(1360*scale));
        }
        yield return new ExitPlayMode();
    }
    static int ScreenWidth()=>UnityEngine.Screen.width;
}
