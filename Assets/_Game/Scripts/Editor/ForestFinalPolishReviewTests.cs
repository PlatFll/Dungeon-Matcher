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

public sealed class ForestFinalPolishReviewTests
{
    const string Root = "ArtSource/ForestSurroundRevision/PolishFinal";
    const string Art = "Assets/_Game/Art/ForestSurroundRevision/PolishFinal/";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestSurround_FinalPolish.unity";
    [Serializable] public class Placement { public string name, file; public int x, y, stage; }
    [Serializable] public class Layout { public Placement[] pieces; }
    [Serializable] public class Use { public string name, path; public Rect rect; public int width, height, stage; public float alpha; }
    [Serializable] public class Evidence { public int width, height, preservedImages; public Rect board, portrait, frame; public Use[] dressing; }
    [Serializable] public class MotionEvidence { public bool reducedMotionStable, loopCloses, animatedStatesDiffer; public int frames; }
    static string Layer(int n) => "Forest final polish stage " + n;
    [MenuItem("Dungeon Matcher/Art Preview/Forest final surround polish")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_ForestSurround_RootJoins.unity", OpenSceneMode.Single);
        var root = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>()).Single().transform;
        var layout = JsonUtility.FromJson<Layout>(File.ReadAllText(Root + "/Scripts/Placements.json"));
        foreach (int stage in new[] { 4, 3, 5 })
        {
            var rt = new GameObject(Layer(stage), typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(root, false); rt.pivot = new Vector2(0,1); rt.anchorMin = rt.anchorMax = new Vector2(0,1);
            rt.sizeDelta = new Vector2(360,680); rt.anchoredPosition = Vector2.zero; rt.localScale = new Vector3(2,2,1);
            rt.SetSiblingIndex(root.Find("Board contents native 64px").GetSiblingIndex());
            foreach (var q in layout.pieces.Where(q => q.stage == stage))
            {
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + q.file + ".png"); Assert.That(s, Is.Not.Null, q.file);
                var part = new GameObject(q.name,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();
                part.SetParent(rt,false); part.pivot = new Vector2(0,1); part.anchorMin = part.anchorMax = new Vector2(0,1);
                part.anchoredPosition = new Vector2(q.x,-q.y); part.sizeDelta = s.rect.size;
                var image = part.GetComponent<Image>(); image.sprite = s; image.raycastTarget = false;
            }
        }
        var lights = root.Find(Layer(5)).GetComponentsInChildren<Image>();
        var motion = root.Find(Layer(5)).gameObject.AddComponent<ForestPolishPreviewMotion>();
        motion.lights = lights; motion.origins = lights.Select(i => i.rectTransform.anchoredPosition).ToArray(); motion.Evaluate(1,false);
        SetStage(root,5); Canvas.ForceUpdateCanvases();
        Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ScenePath),Is.False);
        EditorSceneManager.SaveScene(scene,ScenePath);
    }
    static void SetStage(Transform root, int stage)
    {
        root.Find("Continuous lower root joins 2x").gameObject.SetActive(stage < 3);
        root.Find("Unified lower corner formations 2x").gameObject.SetActive(stage < 3);
        foreach (int n in new[] { 3,4,5 }) root.Find(Layer(n)).gameObject.SetActive(stage >= n);
    }
    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(720*scale,1360*scale)});
    static Rect Screen(Transform t) => GameplayPixelLayoutController.ScreenRect((RectTransform)t);
    static IEnumerator Capture(string name)
    {
        string file = Path.GetFullPath(Root + "/Previews/" + name + ".png"); if(File.Exists(file))File.Delete(file);
        ScreenCapture.CaptureScreenshot(file); float deadline=Time.realtimeSinceStartup+20;
        while(!File.Exists(file)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
    }
    [UnityTearDown] public IEnumerator Cleanup(){if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit("Stages 3-6: integrated corners, enrichment and restrained lights with locked shared UI.")]
    public IEnumerator FinalReview()
    {
        Build();SetSize(1);yield return new EnterPlayMode();yield return null;yield return null;
        var root=Object.FindFirstObjectByType<Canvas>().transform;
        var motion=root.Find(Layer(5)).GetComponent<ForestPolishPreviewMotion>();motion.automatic=false;
        SetStage(root,2);Canvas.ForceUpdateCanvases();yield return null;yield return null;
        var previous=Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.enabled).ToArray();
        var sprites=previous.Select(i=>i.sprite).ToArray();var rects=previous.Select(i=>Screen(i.transform)).ToArray();
        yield return Capture("Unity-Before-720x1360");
        foreach (int stage in new[] {3,4,5})
        {
            SetStage(root,stage);motion.Evaluate(1,false);Canvas.ForceUpdateCanvases();yield return null;yield return null;
            for(int n=0;n<previous.Length;n++){Assert.That(previous[n].sprite,Is.SameAs(sprites[n]),previous[n].name);Assert.That(Screen(previous[n].transform),Is.EqualTo(rects[n]),previous[n].name);}
            yield return Capture("Unity-Stage"+stage+"-720x1360");
        }
        foreach(int scale in new[]{1,2})
        {
            SetSize(scale);float deadline=Time.realtimeSinceStartup+20;
            while(UnityEngine.Screen.width!=720*scale||UnityEngine.Screen.height!=1360*scale){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
            motion.Evaluate(1,false);Canvas.ForceUpdateCanvases();yield return null;yield return null;
            var board=(RectTransform)root.Find("Board contents native 64px");var portrait=(RectTransform)root.Find("Approved forest top modules 2x/ForestPortrait");var frame=(RectTransform)root.Find("Stage04 forest board frame 2x/BoardFrameTarget");
            Assert.That(board.rect.size,Is.EqualTo(new Vector2(512,512)));Assert.That(board.anchoredPosition,Is.EqualTo(new Vector2(104,-528)));
            Assert.That(portrait.rect.size,Is.EqualTo(new Vector2(98,220)));Assert.That(frame.rect.size,Is.EqualTo(new Vector2(290,290)));
            var uses=new List<Use>();
            foreach(int stage in new[]{3,4,5})
            {
                var layer=root.Find(Layer(stage));
                Assert.That(layer.GetSiblingIndex(),Is.LessThan(board.GetSiblingIndex()));Assert.That(layer.GetSiblingIndex(),Is.LessThan(root.Find("Shared bottom HUD").GetSiblingIndex()));
                foreach(var im in layer.GetComponentsInChildren<Image>())
                {
                    var rect=Screen(im.transform);var s=im.sprite;Assert.That(im.raycastTarget,Is.False);
                    if(stage!=5)Assert.That(im.color,Is.EqualTo(Color.white));
                    Assert.That(s.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(s.texture.mipmapCount,Is.EqualTo(1));
                    Assert.That(rect.width,Is.EqualTo(s.rect.width*2*scale).Within(.02));Assert.That(rect.height,Is.EqualTo(s.rect.height*2*scale).Within(.02));
                    Assert.That(rect.x,Is.EqualTo(Mathf.Round(rect.x)).Within(.02));Assert.That(rect.y,Is.EqualTo(Mathf.Round(rect.y)).Within(.02));
                    Assert.That(Mathf.RoundToInt(rect.xMin),Is.GreaterThanOrEqualTo(0));Assert.That(Mathf.RoundToInt(rect.yMin),Is.GreaterThanOrEqualTo(0));
                    Assert.That(Mathf.RoundToInt(rect.xMax),Is.LessThanOrEqualTo(720*scale));Assert.That(Mathf.RoundToInt(rect.yMax),Is.LessThanOrEqualTo(860*scale));
                    uses.Add(new Use{name=im.name,path=AssetDatabase.GetAssetPath(s),rect=rect,width=(int)s.rect.width,height=(int)s.rect.height,stage=stage,alpha=im.color.a});
                }
            }
            foreach(var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)){t.ForceMeshUpdate();Assert.That(t.isTextTruncated,Is.False,t.text);}
            File.WriteAllText(Root+"/Diagnostics/Unity-"+(720*scale)+"x"+(1360*scale)+".json",JsonUtility.ToJson(new Evidence{width=720*scale,height=1360*scale,preservedImages=previous.Length,board=Screen(board),portrait=Screen(portrait),frame=Screen(frame),dressing=uses.ToArray()},true));
            yield return Capture("Unity-Final-"+(720*scale)+"x"+(1360*scale));
        }
        SetSize(1);yield return null;yield return null;
        motion.Evaluate(0,true);var stillPositions=motion.lights.Select(i=>i.rectTransform.anchoredPosition).ToArray();var stillColors=motion.lights.Select(i=>i.color).ToArray();
        motion.Evaluate(3,true);Assert.That(motion.lights.Select(i=>i.rectTransform.anchoredPosition).ToArray(),Is.EqualTo(stillPositions));Assert.That(motion.lights.Select(i=>i.color).ToArray(),Is.EqualTo(stillColors));
        motion.Evaluate(0,false);var first=motion.lights.Select(i=>i.color).ToArray();var firstPositions=motion.lights.Select(i=>i.rectTransform.anchoredPosition).ToArray();
        motion.Evaluate(6,false);Assert.That(motion.lights.Select(i=>i.color.a).ToArray(),Is.EqualTo(first.Select(c=>c.a).ToArray()).Within(.0001));Assert.That(motion.lights.Select(i=>i.rectTransform.anchoredPosition).ToArray(),Is.EqualTo(firstPositions));
        motion.Evaluate(3,false);Assert.That(motion.lights.Select(i=>i.color).ToArray(),Is.Not.EqualTo(first));
        for(int n=0;n<12;n++){motion.Evaluate(n*.5f,false);Canvas.ForceUpdateCanvases();yield return null;yield return null;yield return Capture("Unity-Motion-"+n.ToString("D2"));}
        File.WriteAllText(Root+"/Diagnostics/Motion.json",JsonUtility.ToJson(new MotionEvidence{reducedMotionStable=true,loopCloses=true,animatedStatesDiffer=true,frames=12},true));
        yield return new ExitPlayMode();
    }
}
