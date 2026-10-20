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

public sealed class DungeonPixelPipelineStage14Tests
{
    const string Root = "ArtSource/DungeonPixelLab", Scene = "Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity";
    [Serializable] class CaptureRecord { public string name; public int width,height,scale,logicalHeight; public Rect safeArea,content; public int images,uniqueSources; }
    [Serializable] class Evidence { public List<CaptureRecord> captures=new List<CaptureRecord>(); public List<SourceUse> sources=new List<SourceUse>(); }
    [Serializable] class SourceUse { public string name,path; public Rect rect; }
    static void SetSize(int w,int h) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(w,h)});
    static string Hash(string p) { using(var h=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))); }
    static IEnumerator Capture(string name)
    {
        Canvas.ForceUpdateCanvases();yield return null;yield return null;
        string path=Path.GetFullPath(Root+"/Previews/Stage14/"+name+".png");
        if(File.Exists(path))File.Delete(path);
        ScreenCapture.CaptureScreenshot(path);float end=Time.realtimeSinceStartup+20;
        while(!File.Exists(path)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}
        yield return null;yield return null;
    }
    static void Check(DungeonSideViewArtPreview p,Evidence evidence,string name)
    {
        Assert.That(p.ContentPixels.xMin,Is.GreaterThanOrEqualTo(p.safeArea.xMin));
        Assert.That(p.ContentPixels.yMin,Is.GreaterThanOrEqualTo(p.safeArea.yMin));
        Assert.That(p.ContentPixels.xMax,Is.LessThanOrEqualTo(p.safeArea.xMax));
        Assert.That(p.ContentPixels.yMax,Is.LessThanOrEqualTo(p.safeArea.yMax));
        var images=p.Content.GetComponentsInChildren<Image>();
        var top=p.Content.Find("05 Top UI shells");var bottom=p.Content.Find("06 Bottom HUD shells");
        Assert.That(p.Content.Find("03 Structural masonry").GetSiblingIndex(),Is.LessThan(p.Content.Find("02 Standing platform").GetSiblingIndex()));
        Assert.That(p.Content.Find("02 Standing platform").GetSiblingIndex(),Is.LessThan(p.Content.Find("04 Props").GetSiblingIndex()));
        Assert.That(p.Content.Find("04 Props").GetSiblingIndex(),Is.LessThan(top.GetSiblingIndex()));
        Assert.That(top.GetSiblingIndex(),Is.LessThan(bottom.GetSiblingIndex()));
        Assert.That(((RectTransform)top.Find("Board panel")).anchoredPosition.y,Is.EqualTo(-225-(p.LogicalHeight-640)/2));
        Assert.That(((RectTransform)top.Find("Wave panel")).anchoredPosition.y,Is.EqualTo(-16));
        Assert.That(((RectTransform)bottom.Find("Resonance panel")).anchoredPosition.y,Is.EqualTo(-563-(p.LogicalHeight-640)));
        Assert.That(top.Find("Board panel/InteriorMask").GetComponentsInChildren<Image>().Length,Is.EqualTo(64));
        Assert.That(top.Find("Portrait panel/InteriorMask").GetComponent<RectMask2D>(),Is.Not.Null);
        var backing=(RectTransform)top.Find("Portrait panel/InteriorMask/OverfilledBacking");
        Assert.That(backing.offsetMin.x,Is.LessThan(0));Assert.That(backing.offsetMax.x,Is.GreaterThan(0));
        foreach(var im in images)
        {
            Assert.That(im.raycastTarget,Is.False);Assert.That(im.sprite,Is.Not.Null);
            Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            // Simple images use explicit native dimensions, including the locked 100PPU heart.
            if(im.type==Image.Type.Tiled)Assert.That(im.sprite.pixelsPerUnit,Is.EqualTo(64));
            Assert.That(im.rectTransform.localScale,Is.EqualTo(Vector3.one));
            Assert.That(im.type,Is.EqualTo(Image.Type.Simple).Or.EqualTo(Image.Type.Tiled));
            if(im.type==Image.Type.Simple)Assert.That(im.rectTransform.rect.size,Is.EqualTo(im.sprite.rect.size));
            var corners=new Vector3[4];im.rectTransform.GetWorldCorners(corners);
            foreach(var v in corners)
            {
                var native=p.Content.InverseTransformPoint(v);
                Assert.That(native.x,Is.EqualTo(Mathf.Round(native.x)).Within(.002),im.name+" x grid");
                Assert.That(native.y,Is.EqualTo(Mathf.Round(native.y)).Within(.002),im.name+" y grid");
            }
        }
        foreach(var r in p.Pieces.Where(r=>r.name.StartsWith("Clutter_")||r.name.StartsWith("Hanging_")||r.name.StartsWith("Gate_")||r.name.StartsWith("Torch_")))
        {
            var b=GameplayPixelLayoutController.ScreenRect(r);
            Assert.That(b.xMin,Is.GreaterThanOrEqualTo(p.ContentPixels.xMin-.001),r.name);
            Assert.That(b.xMax,Is.LessThanOrEqualTo(p.ContentPixels.xMax+.001),r.name);
            Assert.That(b.yMin,Is.GreaterThanOrEqualTo(p.ContentPixels.yMin-.001),r.name);
            Assert.That(b.yMax,Is.LessThanOrEqualTo(p.ContentPixels.yMax+.001),r.name);
        }
        string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem","AbilityButtonUI","RunControlsUI","Button","SpriteRenderer"};
        Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
        Assert.That(p.previewCamera.targetTexture,Is.Null);
        Assert.That(p.Content.GetComponentsInChildren<RawImage>().Length,Is.EqualTo(0));
        evidence.captures.Add(new CaptureRecord{name=name,width=p.width,height=p.height,scale=p.PixelScale,logicalHeight=p.LogicalHeight,safeArea=p.safeArea,content=p.ContentPixels,images=images.Length,uniqueSources=images.Select(i=>i.sprite).Distinct().Count()});
        if(name=="Assembly-360x640")foreach(var im in images)
        {
            var corners=new Vector3[4];im.rectTransform.GetWorldCorners(corners);var tl=p.Content.InverseTransformPoint(corners[1]);
            evidence.sources.Add(new SourceUse{name=im.name,path=AssetDatabase.GetAssetPath(im.sprite),rect=new Rect(tl.x,p.LogicalHeight-tl.y,im.rectTransform.rect.width,im.rectTransform.rect.height)});
        }
    }
    [UnityTest,Explicit("Stage14 art-only assembly, full/tall safe area and source geometry.")]
    public IEnumerator AssemblyPreview()
    {
        File.WriteAllText(Root+"/Diagnostics/Stage14/GameScene.sha256",Hash("Assets/_Game/Scenes/Game.unity"));
        File.WriteAllText(Root+"/Diagnostics/Stage14/BuildSettings.sha256",Hash("ProjectSettings/EditorBuildSettings.asset"));
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
        var edit=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();
        edit.width=360;edit.height=640;edit.safeArea=new Rect(0,0,360,640);edit.fitScreenInPlayMode=true;edit.Rebuild();
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Scene),Is.False);
        SetSize(360,640);yield return new EnterPlayMode();
        var evidence=new Evidence();
        foreach(var screen in new[]{new Vector2Int(360,640),new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(360,800),new Vector2Int(1080,2400)})
        {
            SetSize(screen.x,screen.y);yield return new WaitForSecondsRealtime(.4f);
            var p=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();p.fitScreenInPlayMode=true;
            yield return null;yield return null;
            Assert.That(p.width,Is.EqualTo(screen.x));Assert.That(p.height,Is.EqualTo(screen.y));
            string name="Assembly-"+screen.x+"x"+screen.y;Check(p,evidence,name);yield return Capture(name);
        }
        foreach(var config in new[]{new[]{1080,2400,0,90,1080,2220},new[]{1084,2404,2,91,1081,2221}})
        {
            SetSize(config[0],config[1]);yield return new WaitForSecondsRealtime(.4f);
            var p=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();p.fitScreenInPlayMode=false;
            p.width=Screen.width;p.height=Screen.height;p.safeArea=new Rect(config[2],config[3],config[4],config[5]);p.Rebuild();yield return null;yield return null;
            string name="SafeArea-"+config[0]+"x"+config[1];Check(p,evidence,name);yield return Capture(name);
        }
        File.WriteAllText(Root+"/Diagnostics/Stage14/AssemblyEvidence.json",JsonUtility.ToJson(evidence,true));
        yield return new ExitPlayMode();
        Assert.That(Hash("Assets/_Game/Scenes/Game.unity"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage14/GameScene.sha256")));
        Assert.That(Hash("ProjectSettings/EditorBuildSettings.asset"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage14/BuildSettings.sha256")));
    }
}
