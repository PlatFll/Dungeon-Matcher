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

public sealed class LivingForestReviewTests
{
    const string Root = "ArtSource/ForestSurroundRevision/LivingForest";
    const string Art = "Assets/_Game/Art/ForestSurroundRevision/LivingForest/";
    const string BeforeScene = "Assets/_Game/Scenes/ArtPreview_ForestSurround_FinalPolish.unity";
    const string ScenePath = "Assets/_Game/Scenes/ArtPreview_ForestSurround_Living.unity";
    [Serializable] public class Binding { public string name, path; public Rect rect; public bool protectedImage; }
    [Serializable] public class Bindings { public Binding[] images; }
    [Serializable] public class TrackEvidence { public string name, kind; public float period; public Rect rect; public string[] paths; public float minAlpha, maxAlpha; public int positions, poses, neutralSamples; }
    [Serializable] public class Evidence { public bool reducedMotionStable, uniformPixels, pauseStable; public int samples, protectedImages; public TrackEvidence[] tracks; }

    static string Hierarchy(Transform t) => t.parent == null ? t.name : Hierarchy(t.parent) + "/" + t.name;
    static bool Editable(Image image)
    {
        string p = Hierarchy(image.transform);
        return p.Contains("Forest final polish stage 5/") || p.EndsWith("/Right integrated corner")
            || p.EndsWith("/Left hanging vine") || p.EndsWith("/Right hanging vine")
            || p.EndsWith("/Grove_Background") || p.Contains("/Wisp_")
            || p.EndsWith("/Warm firefly") || p.EndsWith("/Hanging leaves");
    }
    static Sprite[] Frames(string name, int count) => Enumerable.Range(0, count).Select(i => {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + "_" + i.ToString("D2") + ".png");
        Assert.That(s, Is.Not.Null, name); return s;
    }).ToArray();
    static Image Add(Transform parent, string name, Sprite sprite, int x, int y)
    {
        var r = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        r.SetParent(parent, false); r.pivot = r.anchorMin = r.anchorMax = new Vector2(0,1);
        r.anchoredPosition = new Vector2(x,-y); r.sizeDelta = sprite.rect.size;
        var im = r.GetComponent<Image>(); im.sprite = sprite; im.raycastTarget = false; return im;
    }
    static RectTransform Layer(Transform parent, string name, float scale)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent,false); r.pivot = r.anchorMin = r.anchorMax = new Vector2(0,1);
        r.sizeDelta = new Vector2(360,680); r.anchoredPosition = Vector2.zero; r.localScale = new Vector3(scale,scale,1); return r;
    }
    static LivingForestReviewMotion.Track Light(Image im, Sprite[] frames, float period, float phase, float peak, float on, float dx, float dy, float drift)
        => new LivingForestReviewMotion.Track { image=im, frames=frames, origin=im.rectTransform.anchoredPosition,
            kind=LivingForestReviewMotion.MotionKind.Light,period=period,phase=phase,peak=peak,onFraction=on,
            amplitude=new Vector2(dx,dy),driftPeriod=drift,frameRate=4.5f+phase*.3f };

    [MenuItem("Dungeon Matcher/Art Preview/Living forest motion review")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(BeforeScene,OpenSceneMode.Single);
        var root = Object.FindFirstObjectByType<Canvas>().transform;
        var top = root.Find("Approved forest top modules 2x");
        var env = top.Find("Stage01 environment");
        var accents = top.Find("Stage03 magical accents");
        root.Find("Forest final polish stage 5").gameObject.SetActive(false);
        root.Find("Forest final polish stage 3/Right integrated corner").GetComponent<Image>().sprite = Frames("Corner_Right_NoSprig",1)[0];
        env.Find("Grove_Background").GetComponent<Image>().sprite = Frames("Grove_Motion_Copy",1)[0];
        var lower = Layer(root,"Living forest lower spirits",2);
        lower.SetSiblingIndex(root.Find("Board contents native 64px").GetSiblingIndex());
        var water = Layer(env,"Living forest water",1); water.SetSiblingIndex(1);
        var upper = Layer(top,"Living forest upper spirits",1); upper.SetSiblingIndex(accents.GetSiblingIndex()+1);
        var tracks = new List<LivingForestReviewMotion.Track>();
        var waterFrames = Frames("Waterfall",12);
        var waterImage = Add(water,"Subtle waterfall shimmer",waterFrames[0],174,8);
        tracks.Add(new LivingForestReviewMotion.Track {image=waterImage,frames=waterFrames,origin=waterImage.rectTransform.anchoredPosition,kind=LivingForestReviewMotion.MotionKind.Water,frameRate=8});
        var oldTop = new[]{"Wisp_Far_A","Wisp_Far_B","Wisp_Near"};
        for(int i=0;i<oldTop.Length;i++)
        {
            var im=env.Find(oldTop[i]).GetComponent<Image>();
            tracks.Add(Light(im,new[]{im.sprite},3.9f+i*1.7f,.5f+i*1.3f,.85f,.73f,1+i%2,2,5.3f+i*2.1f));
        }
        foreach(string name in new[]{"Wisp_Aura","Wisp_Core"})
        {
            var im=accents.Find(name).GetComponent<Image>();
            tracks.Add(Light(im,new[]{im.sprite},8.3f,1.6f,name=="Wisp_Aura"?.55f:.85f,.8f,2,3,10.9f));
        }
        var fly=accents.Find("Warm firefly").GetComponent<Image>();
        tracks.Add(Light(fly,new[]{fly.sprite},3.2f,.9f,.88f,.5f,3,2,6.7f));
        var extraFly=Add(upper,"Distant gold firefly",fly.sprite,280,131);
        tracks.Add(Light(extraFly,new[]{fly.sprite},2.1f,.4f,.65f,.43f,2,2,5.1f));
        foreach(var item in new[]{("Upper_Mote_A",244,84,4.6f,1.1f),("Upper_Mote_B",295,97,2.7f,.3f)})
        {
            var frames=Frames(item.Item1,1);var im=Add(upper,item.Item1,frames[0],item.Item2,item.Item3);
            tracks.Add(Light(im,frames,item.Item4,item.Item5,.85f,.68f,2,2,6.1f+item.Item5));
        }
        var motes=Frames("Wisp_Mote",4);var curls=Frames("Wisp_Curl",4);var comets=Frames("Wisp_Comet",4);
        var flame=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/ForestSurroundRevision/PolishFinal/Wisp.png");
        var sprites=new[]{motes,curls,curls,motes,comets,new[]{flame},motes,comets};
        var positions=new[]{new Vector2Int(7,304),new Vector2Int(330,319),new Vector2Int(3,402),new Vector2Int(336,432),new Vector2Int(4,540),new Vector2Int(330,550),new Vector2Int(29,638),new Vector2Int(318,615)};
        float[] periods={2.8f,5.9f,7.3f,3.1f,8.9f,4.4f,2.3f,6.7f};
        for(int i=0;i<sprites.Length;i++)
        {
            var im=Add(lower,"Varied forest spirit "+i,sprites[i][0],positions[i].x,positions[i].y);
            tracks.Add(Light(im,sprites[i],periods[i],i*.63f,.6f+(i%3)*.08f,i%2==0?.6f:.76f,1+i%2,2+i%2,4.7f+i*1.13f));
        }
        var windTargets=new[]{root.Find("Forest final polish stage 4/Left hanging vine"),root.Find("Forest final polish stage 4/Right hanging vine"),accents.Find("Hanging leaves")};
        var windNames=new[]{"Vine_Left","Vine_Right","Upper_Leaves"};
        for(int i=0;i<windTargets.Length;i++)
        {
            var im=windTargets[i].GetComponent<Image>();var frames=Frames(windNames[i],5);
            im.sprite=frames[0];im.rectTransform.sizeDelta=frames[0].rect.size;im.rectTransform.anchoredPosition+=new Vector2(-4,4);
            tracks.Add(new LivingForestReviewMotion.Track { image=im,frames=frames,origin=im.rectTransform.anchoredPosition,
                kind=LivingForestReviewMotion.MotionKind.Foliage,period=9.7f+i*1.4f,phase=i*3.2f,gustDuration=3.1f });
        }
        var motion=lower.gameObject.AddComponent<LivingForestReviewMotion>();motion.tracks=tracks.ToArray();motion.Evaluate(1.25f,false);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==ScenePath),Is.False);
        EditorSceneManager.SaveScene(scene,ScenePath);
    }

    static void SetSize(int scale) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(720*scale,1360*scale)});
    static Rect ScreenRect(Transform t) => GameplayPixelLayoutController.ScreenRect((RectTransform)t);
    static IEnumerator Capture(string path)
    {
        path=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(path));if(File.Exists(path))File.Delete(path);
        ScreenCapture.CaptureScreenshot(path);float deadline=Time.realtimeSinceStartup+20;
        while(!File.Exists(path)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
    }
    static string Signature(LivingForestReviewMotion motion) => string.Join("|",motion.tracks.Select(t=>t.image.sprite.name+":"+t.image.rectTransform.anchoredPosition+":"+t.image.color.a));
    static string BindingKey(Binding b) => b.name+"|"+b.path+"|"+b.rect;
    [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit("Copied forest scene: protected artwork, varied spirits, occasional wind and water motion.")]
    public IEnumerator MotionReview()
    {
        EditorSceneManager.OpenScene(BeforeScene,OpenSceneMode.Single);SetSize(1);yield return new EnterPlayMode();yield return null;yield return null;
        var old=Object.FindFirstObjectByType<ForestPolishPreviewMotion>();old.automatic=false;old.Evaluate(1,false);
        Canvas.ForceUpdateCanvases();yield return null;yield return null;
        var baseline=Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.enabled).Select(i=>new Binding{name=Hierarchy(i.transform),path=AssetDatabase.GetAssetPath(i.sprite),rect=ScreenRect(i.transform),protectedImage=!Editable(i)}).ToArray();
        File.WriteAllText(Root+"/Diagnostics/BeforeBindings.json",JsonUtility.ToJson(new Bindings{images=baseline},true));
        yield return Capture(Root+"/Previews/Unity-Before.png");yield return new ExitPlayMode();
        Build();SetSize(1);yield return new EnterPlayMode();yield return null;yield return null;
        var motion=Object.FindFirstObjectByType<LivingForestReviewMotion>();
        Assert.That(motion,Is.Not.Null,"Copied scene motion component");
        motion.automatic=false;motion.Evaluate(1.25f,false);
        var root=motion.transform.parent;
        var protectedBindings=JsonUtility.FromJson<Bindings>(File.ReadAllText(Root+"/Diagnostics/BeforeBindings.json")).images.Where(i=>i.protectedImage).ToArray();
        // Modular frame pieces may share names. Compare the complete binding/rectangle multiset.
        var currentBindings=Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.enabled)
            .Select(i=>new Binding{name=Hierarchy(i.transform),path=AssetDatabase.GetAssetPath(i.sprite),rect=ScreenRect(i.transform)}).ToArray();
        var bindingCounts=currentBindings.GroupBy(BindingKey).ToDictionary(g=>g.Key,g=>g.Count());
        foreach(var group in protectedBindings.GroupBy(BindingKey))
        {
            Assert.That(bindingCounts.ContainsKey(group.Key),Is.True,group.Key);
            Assert.That(bindingCounts[group.Key],Is.EqualTo(group.Count()),group.Key);
        }
        var board=root.Find("Board contents native 64px");var portrait=root.Find("Approved forest top modules 2x/ForestPortrait");
        Assert.That(board,Is.Not.Null,"Board parent");Assert.That(portrait,Is.Not.Null,"Portrait parent");
        Assert.That(((RectTransform)board).rect.size,Is.EqualTo(new Vector2(512,512)));
        Assert.That(((RectTransform)portrait).rect.size,Is.EqualTo(new Vector2(98,220)));
        var records=motion.tracks.Select(t=>new TrackEvidence{name=Hierarchy(t.image.transform),kind=t.kind.ToString(),period=t.period,rect=ScreenRect(t.image.transform),paths=t.frames.Select(s=>AssetDatabase.GetAssetPath(s)).ToArray(),minAlpha=1,maxAlpha=0}).ToArray();
        foreach(int scale in new[]{1,2})
        {
            SetSize(scale);float deadline=Time.realtimeSinceStartup+20;
            while(Screen.width!=720*scale||Screen.height!=1360*scale){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
            motion.Evaluate(1.25f,false);Canvas.ForceUpdateCanvases();yield return null;yield return null;
            foreach(var t in motion.tracks)
            {
                var r=ScreenRect(t.image.transform);Assert.That(r.width,Is.EqualTo(t.image.sprite.rect.width*2*scale).Within(.02));Assert.That(r.height,Is.EqualTo(t.image.sprite.rect.height*2*scale).Within(.02));
                Assert.That(t.image.raycastTarget,Is.False);Assert.That(t.image.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(t.image.sprite.texture.mipmapCount,Is.EqualTo(1));
            }
            foreach(var t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)){t.ForceMeshUpdate();Assert.That(t.isTextTruncated,Is.False);}
            yield return Capture(Root+"/Previews/Unity-Final-"+Screen.width+"x"+Screen.height+".png");
        }
        SetSize(1);yield return null;yield return null;
        motion.Evaluate(0,true);string reduced=Signature(motion);motion.Evaluate(19,true);Assert.That(Signature(motion),Is.EqualTo(reduced));
        yield return Capture(Root+"/Previews/Unity-ReducedMotion.png");
        motion.Evaluate(2,false);Time.timeScale=0;motion.automatic=true;string paused=Signature(motion);yield return null;yield return null;
        // The automatic clock starts at zero, so establish its paused state before comparing.
        paused=Signature(motion);yield return null;yield return null;Assert.That(Signature(motion),Is.EqualTo(paused));motion.automatic=false;Time.timeScale=1;
        var seenPositions=motion.tracks.Select(_=>new HashSet<Vector2>()).ToArray();var seenSprites=motion.tracks.Select(_=>new HashSet<Sprite>()).ToArray();
        for(int n=0;n<288;n++)
        {
            motion.Evaluate(n/12f,false);Canvas.ForceUpdateCanvases();yield return null;yield return null;
            for(int i=0;i<motion.tracks.Length;i++)
            {
                var t=motion.tracks[i];var r=ScreenRect(t.image.transform);
                Assert.That(r.x,Is.EqualTo(Mathf.Round(r.x)).Within(.02));Assert.That(r.y,Is.EqualTo(Mathf.Round(r.y)).Within(.02));
                Assert.That(r.xMin,Is.GreaterThanOrEqualTo(-.02));Assert.That(r.xMax,Is.LessThanOrEqualTo(720.02));Assert.That(r.yMin,Is.GreaterThanOrEqualTo(-.02));Assert.That(r.yMax,Is.LessThanOrEqualTo(1360.02));
                seenPositions[i].Add(t.image.rectTransform.anchoredPosition);seenSprites[i].Add(t.image.sprite);
                records[i].minAlpha=Mathf.Min(records[i].minAlpha,t.image.color.a);records[i].maxAlpha=Mathf.Max(records[i].maxAlpha,t.image.color.a);
                if(t.image.sprite==t.frames[0])records[i].neutralSamples++;
            }
            yield return Capture(".utmp/living-forest/capture/Frame-"+n.ToString("D3")+".png");
        }
        for(int i=0;i<motion.tracks.Length;i++)
        {
            var t=motion.tracks[i];records[i].positions=seenPositions[i].Count;records[i].poses=seenSprites[i].Count;
            if(t.kind==LivingForestReviewMotion.MotionKind.Light){Assert.That(records[i].positions,Is.GreaterThan(1));Assert.That(records[i].maxAlpha-records[i].minAlpha,Is.GreaterThan(.4));}
            else if(t.kind==LivingForestReviewMotion.MotionKind.Foliage){Assert.That(records[i].poses,Is.GreaterThanOrEqualTo(3));Assert.That(records[i].neutralSamples,Is.GreaterThan(144));}
            else Assert.That(records[i].poses,Is.EqualTo(12));
        }
        File.WriteAllText(Root+"/Diagnostics/MotionEvidence.json",JsonUtility.ToJson(new Evidence{reducedMotionStable=true,uniformPixels=true,pauseStable=true,samples=288,protectedImages=protectedBindings.Length,tracks=records},true));
        yield return new ExitPlayMode();
    }
}
