using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public sealed class DungeonArchitectureReviewTests
{
    const string Root="ArtSource/DungeonArchitecturePolish";
    [UnityTest,Explicit] public IEnumerator Before()=>new DungeonHybridIntegrationTests().CaptureScreens(Root,"Before");
    [UnityTest,Explicit] public IEnumerator After()=>new DungeonHybridIntegrationTests().CaptureScreens(Root,"After",null,CompareProtectedPixels);
    [UnityTearDown] public IEnumerator Cleanup()=>new DungeonHybridIntegrationTests().Cleanup();

    public static void AssertArchitecture(GameplayPixelLayoutController layout)
    {
        var root=GameObject.Find("DungeonPixelLabEnvironment").transform.Find("Side architecture");
        Assert.That(root,Is.Not.Null);Assert.That(root.childCount,Is.EqualTo(2));
        float scale=DungeonPixelLabBattleArt.PhysicalPixelScale;
        var board=GameplayPixelLayoutController.ScreenRect(layout.BoardArea);
        // The board area's frame includes transparent padding. The playable cell bounds are narrower.
        float opening=RunSession.Current.Board.LocalBoardWidth*64*layout.Current.BoardTexelRatio;
        var playable=new Rect(board.center.x-opening/2,board.center.y-opening/2,opening,opening);
        foreach(RectTransform column in root)
        {
            var r=GameplayPixelLayoutController.ScreenRect(column);
            Assert.That(column.name=="Left pillar"?r.xMin:r.xMax,Is.EqualTo(column.name=="Left pillar"?0:UnityEngine.Device.Screen.width).Within(.1f));
            Assert.That(r.yMin,Is.EqualTo(0).Within(.1f));
            var shaft=column.Find("Side_Shaft").GetComponent<Image>();
            Assert.That(shaft.type,Is.EqualTo(Image.Type.Tiled));
            Assert.That(shaft.rectTransform.anchoredPosition.y,Is.EqualTo(-20));
            Assert.That(shaft.rectTransform.rect.height+20,Is.EqualTo(column.rect.height-21).Within(.01f));
            Assert.That(column.Find("Side_Base").GetComponent<RectTransform>().anchoredPosition.y,Is.EqualTo(-(column.rect.height-23)).Within(.01f));
            Assert.That(column.GetComponentsInChildren<Image>().Count(i=>i.name.Contains("Candles_")),Is.EqualTo(2));
            Assert.That(column.GetComponentsInChildren<ParticleSystem>(),Is.Empty,"Unlit candles have no particles or glow");
            foreach(var image in column.GetComponentsInChildren<Image>())
            {
                var bounds=GameplayPixelLayoutController.ScreenRect(image.rectTransform);
                Assert.That(bounds.Overlaps(playable),Is.False,image.name+" overlaps a playable cell");
                Assert.That(image.raycastTarget,Is.False);Assert.That(image.name.Contains("Banner"),Is.False);
                if(image.name.Contains("Candles_"))
                {
                    float bottom=-image.rectTransform.anchoredPosition.y+(image.name.EndsWith("Short")?22:21);
                    var ledge=column.Find("Side_Ledge").GetComponent<RectTransform>();
                    float distance=Mathf.Min(Mathf.Abs(bottom+ledge.anchoredPosition.y),Mathf.Abs(bottom-(column.rect.height-23)));
                    Assert.That(distance,Is.LessThan(.01f),"Wax must touch a stone ledge/base");
                }
            }
            foreach(Transform chain in column.Cast<Transform>().Where(t=>t.name=="Mounted chain"))
            {
                var images=chain.GetComponentsInChildren<Image>();
                for(int i=1;i<images.Length;i++)
                    Assert.That(-images[i].rectTransform.anchoredPosition.y,Is.EqualTo(-images[i-1].rectTransform.anchoredPosition.y+images[i-1].sprite.rect.height).Within(.01f));
            }
        }
        Assert.That(Resources.Load<DungeonPixelLabBattleArt>("UI/BattlePresentation/ForestPixelLab").UsesSideArchitecture,Is.False);
    }

    static IEnumerator Capture(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));if(File.Exists(path))File.Delete(path);
        Canvas.ForceUpdateCanvases();ScreenCapture.CaptureScreenshot(Path.GetFullPath(path));
        float deadline=Time.realtimeSinceStartup+15;while(!File.Exists(path)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}
        yield return null;
    }
    static IEnumerator CompareProtectedPixels(GameplayPixelLayoutController layout,string name)
    {
        var art=DungeonPixelLabBattleArt.Current;var kit=art.sideArchitectureModules;
        var owner=layout.GetComponent<DungeonPresentationArt>();
        var geometry=typeof(DungeonPresentationArt).GetField("pixelLabGeometry",BindingFlags.Instance|BindingFlags.NonPublic);
        var animators=Object.FindObjectsByType<Animator>(FindObjectsSortMode.None).Where(a=>a.enabled).ToArray();
        var root=GameObject.Find("DungeonPixelLabEnvironment").transform.Find("Side architecture");
        var allowed=root.Cast<RectTransform>().Select(GameplayPixelLayoutController.ScreenRect).ToList();
        float scale=DungeonPixelLabBattleArt.PhysicalPixelScale;
        allowed.Add(new Rect(0,0,58*scale,86*scale));allowed.Add(new Rect(UnityEngine.Device.Screen.width-58*scale,0,58*scale,86*scale));
        string before=Root+"/Previews/PairedBefore/"+name+".png",after=Root+"/Previews/PairedAfter/"+name+".png";
        float previous=Time.timeScale;
        try
        {
            Time.timeScale=0;foreach(var a in animators)a.enabled=false;
            yield return null;
            yield return Capture(after);
            art.sideArchitectureModules=null;geometry.SetValue(owner,null);
            yield return null;yield return null;
            yield return Capture(before);
            var aTexture=new Texture2D(2,2,TextureFormat.RGBA32,false);var bTexture=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                Assert.That(aTexture.LoadImage(File.ReadAllBytes(before)),Is.True);Assert.That(bTexture.LoadImage(File.ReadAllBytes(after)),Is.True);
                var a=aTexture.GetPixels32();var b=bTexture.GetPixels32();int changed=0,outside=0;
                for(int i=0;i<a.Length;i++)if(!a[i].Equals(b[i])){changed++;var point=new Vector2(i%aTexture.width+.5f,i/aTexture.width+.5f);if(!allowed.Any(r=>r.Contains(point)))outside++;}
                File.WriteAllText(Root+"/Diagnostics/After/"+name+"-protected-pixels.txt","Changed="+changed+"; outside permitted side architecture="+outside);
                Assert.That(changed,Is.GreaterThan(0));Assert.That(outside,Is.Zero,"Protected gameplay pixels changed between frozen before/after captures");
            }
            finally {Object.DestroyImmediate(aTexture);Object.DestroyImmediate(bTexture);}
        }
        finally {art.sideArchitectureModules=kit;geometry.SetValue(owner,null);foreach(var a in animators)if(a)a.enabled=true;Time.timeScale=previous;}
        yield return null;yield return null;
    }
}
