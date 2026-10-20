using System;
using System.Collections;
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

public sealed class DungeonPixelPipelineStage13Tests
{
    const string Root = "ArtSource/DungeonPixelLab", Art = "Assets/_Game/Art/DungeonPixelLab";
    const string Scene = "Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity";
    static void SetSize(int w, int h) => typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { new Vector2Int(w,h) });
    static string Hash(string p) { using (var h = System.Security.Cryptography.SHA256.Create()) return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))); }
    static void Prepare()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string file in Directory.GetFiles(Art + "/BottomHud", "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=64; importer.filterMode=FilterMode.Point; importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.npotScale=TextureImporterNPOTScale.None;
            importer.wrapMode=TextureWrapMode.Clamp; importer.alphaIsTransparency=false; importer.ClearPlatformTextureSettings("Android");
            var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType=SpriteMeshType.FullRect; settings.spriteExtrude=0;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
        var p=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();
        string[] families={"Wall","PortraitRecess","FloorTop","Fascia","Gate","Support","Torch","Hanging","Clutter","PortraitFrame","BoardFrame","TopPlaques","BottomHud"};
        var paths=families.SelectMany(d=>Directory.GetFiles(Art+"/"+d,"*.png")).Select(f=>f.Replace('\\','/')).Concat(new[]{
            "Assets/_Game/Resources/UI/Consumables/Potion.png","Assets/_Game/Resources/UI/Consumables/Bomb.png","Assets/_Game/Resources/UI/GlobalMenu/CrownedSkull.png"});
        p.modules=paths.Select(AssetDatabase.LoadAssetAtPath<Sprite>).ToArray();
        p.placementManifest=AssetDatabase.LoadAssetAtPath<TextAsset>(Art+"/Stage13BottomHudPlacement.json");
        p.width=360;p.height=640;p.safeArea=new Rect(0,0,360,640);p.Rebuild();
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Scene),Is.False);
    }
    static IEnumerator Capture(string name)
    {
        Canvas.ForceUpdateCanvases();yield return null;yield return null;
        string path=Path.GetFullPath(Root+"/Previews/Stage13/"+name+".png");
        if(File.Exists(path))File.Delete(path);
        ScreenCapture.CaptureScreenshot(path);float end=Time.realtimeSinceStartup+20;
        while(!File.Exists(path)){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}
        yield return null;yield return null;
    }
    static void CheckPanel(RectTransform panel,int inset)
    {
        var frame=panel.Find("ModularFrame");
        foreach(var image in frame.GetComponentsInChildren<Image>())
        {
            var r=image.rectTransform; Assert.That(image.raycastTarget,Is.False);Assert.That(image.enabled,Is.True);
            Assert.That(image.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(image.sprite.pixelsPerUnit,Is.EqualTo(64));
            Assert.That(r.localScale,Is.EqualTo(Vector3.one));
            Assert.That(r.anchoredPosition.x,Is.EqualTo(Mathf.Round(r.anchoredPosition.x)).Within(.001));
            Assert.That(r.anchoredPosition.y,Is.EqualTo(Mathf.Round(r.anchoredPosition.y)).Within(.001));
            Assert.That(r.anchoredPosition.x,Is.GreaterThanOrEqualTo(0));Assert.That(-r.anchoredPosition.y,Is.GreaterThanOrEqualTo(0));
            Assert.That(r.anchoredPosition.x+r.rect.width,Is.LessThanOrEqualTo(panel.rect.width));
            Assert.That(-r.anchoredPosition.y+r.rect.height,Is.LessThanOrEqualTo(panel.rect.height));
            if(image.type==Image.Type.Simple)Assert.That(r.rect.size,Is.EqualTo(image.sprite.rect.size));
            else Assert.That(image.pixelsPerUnitMultiplier,Is.EqualTo(1));
        }
        var mask=(RectTransform)panel.Find("InteriorMask");Assert.That(mask.GetComponent<RectMask2D>(),Is.Not.Null);
        Assert.That(mask.rect.size,Is.EqualTo(panel.rect.size-Vector2.one*2*inset));
        var backing=(RectTransform)mask.Find("OverfilledBacking");Assert.That(backing.offsetMin.x,Is.LessThan(0));Assert.That(backing.offsetMax.x,Is.GreaterThan(0));
        var icon=panel.Find("NativeIcon")?.GetComponent<Image>();
        if(icon)
        {
            var r=icon.rectTransform; Assert.That(r.rect.size,Is.EqualTo(icon.sprite.rect.size));Assert.That(icon.raycastTarget,Is.False);
            Assert.That(r.anchoredPosition,Is.EqualTo(new Vector2(Mathf.Floor((panel.rect.width-r.rect.width)/2),-Mathf.Floor((panel.rect.height-r.rect.height)/2))));
            Assert.That(r.anchoredPosition.x,Is.GreaterThanOrEqualTo(0));Assert.That(-r.anchoredPosition.y,Is.GreaterThanOrEqualTo(0));
            Assert.That(r.anchoredPosition.x+r.rect.width,Is.LessThanOrEqualTo(panel.rect.width));Assert.That(-r.anchoredPosition.y+r.rect.height,Is.LessThanOrEqualTo(panel.rect.height));
        }
    }
    [UnityTest,Explicit("Stage13 bottom HUD native geometry, meter masks and preservation.")]
    public IEnumerator BottomHudPreview()
    {
        File.WriteAllText(Root+"/Diagnostics/Stage13/GameScene.sha256",Hash("Assets/_Game/Scenes/Game.unity"));
        File.WriteAllText(Root+"/Diagnostics/Stage13/BuildSettings.sha256",Hash("ProjectSettings/EditorBuildSettings.asset"));
        Prepare();SetSize(360,640);yield return new EnterPlayMode();
        foreach(var screen in new[]{new Vector2Int(360,640),new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400)})
        {
            SetSize(screen.x,screen.y);yield return new WaitForSecondsRealtime(.4f);
            var p=Object.FindFirstObjectByType<DungeonSideViewArtPreview>();p.width=Screen.width;p.height=Screen.height;
            p.safeArea=screen.y==2400?new Rect(0,90,1080,2220):new Rect(0,0,p.width,p.height);p.Rebuild();yield return null;yield return null;
            var bottom=p.Content.Find("06 Bottom HUD shells");
            foreach(string n in new[]{"Resonance","Potion","Bomb","Progress"})CheckPanel((RectTransform)bottom.Find(n+" panel"),n=="Progress"?4:7);
            var panels=new[]{"Resonance","Potion","Bomb","Progress"}.Select(n=>GameplayPixelLayoutController.ScreenRect((RectTransform)bottom.Find(n+" panel"))).ToArray();
            for(int a=0;a<panels.Length;a++)for(int b=a+1;b<panels.Length;b++)Assert.That(panels[a].Overlaps(panels[b]),Is.False);
            var label=bottom.Find("Resonance label").GetComponentInChildren<TMP_Text>();label.ForceMeshUpdate();
            Assert.That(label.text,Is.EqualTo("PHASE RESONANCE"));Assert.That(label.isTextTruncated,Is.False);Assert.That(label.font,Is.SameAs(GameUi.TmpFont));
            Assert.That(p.Content.GetComponentsInChildren<TMP_Text>().Length,Is.EqualTo(3));
            Assert.That(p.Content.GetComponentsInChildren<Button>().Length,Is.EqualTo(0));
            string[] forbidden={"BoardController","EnemyActor","PlayerActor","RunSession","EventSystem","AbilityButtonUI","RunControlsUI"};
            Assert.That(Object.FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(c=>c&&forbidden.Contains(c.GetType().Name)),Is.False);
            var progress=(RectTransform)bottom.Find("Progress panel");var meter=progress.GetComponent<DungeonPreviewNativeMeter>();
            var mask=(RectTransform)progress.Find("NativeFillMask");Assert.That(mask.gameObject.activeSelf,Is.False);
            yield return Capture("BottomHud-"+screen.x+"x"+screen.y);
            if(screen.x==360)
            {
                meter.SetFill(.5f);yield return null;Assert.That(mask.rect.width,Is.EqualTo(45));yield return Capture("Meter-Half-Test");
                meter.SetFill(1);yield return null;Assert.That(mask.rect.width,Is.EqualTo(90));yield return Capture("Meter-Full-Test");
                foreach(var size in new[]{new Vector2(99,15),new Vector2(133,17),new Vector2(48,12)})
                {progress.sizeDelta=size;yield return null;yield return null;CheckPanel(progress,4);Assert.That(mask.rect.width,Is.EqualTo(size.x-8));}
                progress.sizeDelta=new Vector2(98,14);meter.SetFill(-1);yield return null;Assert.That(mask.gameObject.activeSelf,Is.False);
                meter.SetFill(2);yield return null;Assert.That(mask.rect.width,Is.EqualTo(90));meter.SetFill(0);
                var supply=(RectTransform)bottom.Find("Potion panel");var original=supply.rect.size;
                foreach(var size in new[]{new Vector2(46,44),new Vector2(71,65),new Vector2(38,38)}){supply.sizeDelta=size;yield return null;yield return null;CheckPanel(supply,7);}
                supply.sizeDelta=original;
            }
        }
        yield return new ExitPlayMode();
        Assert.That(Hash("Assets/_Game/Scenes/Game.unity"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage13/GameScene.sha256")));
        Assert.That(Hash("ProjectSettings/EditorBuildSettings.asset"),Is.EqualTo(File.ReadAllText(Root+"/Diagnostics/Stage13/BuildSettings.sha256")));
    }
}
