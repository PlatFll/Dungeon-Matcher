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
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public sealed class SilverRoyalRelicProductionTests
{
    const string Root=".utmp/SilverControlSizing";
    IDisposable profile,selection;
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> ready){float end=Time.realtimeSinceStartup+90;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;yield return Until(()=>Time.realtimeSinceStartup>=end);}
    static Rect Bounds(Image im)=>GameplayPixelLayoutController.ScreenRect(im.rectTransform);
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;profile?.Dispose();selection?.Dispose();profile=null;selection=null;
        RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;
        if(Application.isPlaying)yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator ProductionControlsAcrossZonesAndCharacters()
    {
        Size(720,1280);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();Directory.CreateDirectory(Root+"/Preview");Directory.CreateDirectory(Root+"/QA");
        string[] zones={"dungeon","magical-forest","drowned-court","ironvein-excavation","frostwake-peaks","drowned-court","drowned-court","drowned-court"};
        string[] ids={"skeleton","skeleton","skeleton","skeleton","skeleton","dungeon_attender","kobold_gunner","bardley"};
        for(int c=0;c<zones.Length;c++)
        {
            string path=Path.GetFullPath(".utmp/SilverProduction/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
            profile=AccountProgression.UseDisposableProfile(path);selection=CharacterSelectionSettings.UseTemporarySelection(ids[c]);
            RunLaunchOptions.StartingZone=zones[c];SceneManager.LoadScene("Game");yield return null;
            yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
            yield return Until(()=>RunSession.Current.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
            var run=RunSession.Current;Assert.That(run.Player.Definition.PlayerId,Is.EqualTo(ids[c]));
            var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
            var controls=Object.FindFirstObjectByType<RunControlsUI>();
            var cases=c<5?new[]{new Vector3Int(360,640,0),new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,1)}:new[]{new Vector3Int(720,1280,0)};
            foreach(var size in cases)
            {
                GameplayPixelLayoutController.ValidationSafeArea=null;Size(size.x,size.y);
                yield return Until(()=>UnityEngine.Device.Screen.width==size.x&&UnityEngine.Device.Screen.height==size.y);
                GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                yield return Wait(.45f);Canvas.ForceUpdateCanvases();
                var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);Assert.That(errors,Is.Empty,report);
                ValidateControls(layout,controls,ids[c]);
                string key=zones[c]+"-"+ids[c]+"-"+size.x+"x"+size.y+(size.z==1?"-safe":"");
                File.WriteAllText(Root+"/QA/"+key+".txt",report+"\nApproved shared controls, pixel scale, centered separate icon, margins and live wave label pass.\n");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(Root+"/Preview/"+key+".png"));yield return Wait(.2f);
            }
            controls.SettingsButton.onClick.Invoke();yield return null;
            Assert.That(controls.IsModalOpen,Is.True);Assert.That(Time.timeScale,Is.Zero);controls.Close();yield return null;
            Assert.That(Time.timeScale,Is.EqualTo(1));
            var buttons=controls.GameplayCanvas.GetComponentsInChildren<Button>();
            Assert.That(buttons.Any(b=>b.name=="CombatGuide"),Is.False);
            var bomb=buttons.Single(b=>b.name=="Bomb");int stock=run.Charges(ConsumableKind.Bomb);
            bomb.onClick.Invoke();yield return null;Assert.That(run.Board.IsSelectingTarget,Is.True);
            bomb.onClick.Invoke();yield return null;Assert.That(run.Board.IsSelectingTarget,Is.False);Assert.That(run.Charges(ConsumableKind.Bomb),Is.EqualTo(stock));
            run.Player.TryTakeDamage(25);yield return null;
            var potion=buttons.Single(b=>b.name=="HealthPotion");stock=run.Charges(ConsumableKind.HealthPotion);
            potion.onClick.Invoke();yield return null;Assert.That(run.Charges(ConsumableKind.HealthPotion),Is.EqualTo(stock-1));
            var controller=run.Player.GetComponent<PlayerAbilityController>();var energy=run.Player.GetComponent<PlayerAbilityEnergy>();
            energy.AddEnergy(1000);yield return Wait(.4f);
            var view=Object.FindFirstObjectByType<StoneAbilityButtonView>();Assert.That(view.IsRaised,Is.True);
            Assert.That(view.Glow.color.a,Is.Zero,"Ready reveals the full-color icon");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath(Root+"/Preview/"+zones[c]+"-"+ids[c]+"-ready.png"));yield return Wait(.2f);
            Time.timeScale=0;var position=view.Slab.anchoredPosition;var alpha=view.Glow.color.a;yield return Wait(.15f);
            Assert.That(view.Slab.anchoredPosition,Is.EqualTo(position));Assert.That(view.Glow.color.a,Is.EqualTo(alpha));Time.timeScale=1;
            bool hadMotion=PlayerPrefs.HasKey(PresentationPreferences.ReducedMotionKey);int oldMotion=PlayerPrefs.GetInt(PresentationPreferences.ReducedMotionKey);
            try
            {
                PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,1);yield return null;yield return null;
                Assert.That(view.Slab.anchoredPosition,Is.EqualTo(Vector2.zero));Assert.That(view.Glow.color.a,Is.Zero,"Ready reveals the full-color icon");
            }
            finally{if(hadMotion)PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,oldMotion);else PlayerPrefs.DeleteKey(PresentationPreferences.ReducedMotionKey);}
            int charge=energy.CurrentEnergy;buttons.Single(b=>b.name=="AbilityButton").onClick.Invoke();yield return null;
            Assert.That(energy.CurrentEnergy,Is.EqualTo(charge-controller.RequiredEnergy));
            yield return Until(()=>run.Continuation.CanCapture);
            profile.Dispose();selection.Dispose();profile=null;selection=null;
        }
        yield return new ExitPlayMode();
    }
    static void ValidateControls(GameplayPixelLayoutController layout,RunControlsUI controls,string character)
    {
        var buttons=controls.GameplayCanvas.GetComponentsInChildren<Button>();
        var supply=buttons.Where(b=>b.name=="Bomb"||b.name=="HealthPotion").Select(b=>b.image).ToArray();
        var view=Object.FindFirstObjectByType<StoneAbilityButtonView>();Assert.That(view,Is.Not.Null);
        var plate=view.Slab.GetComponent<Image>();
        Assert.That(plate.sprite.name,Is.EqualTo("AbilityButtonSilver"));
        Assert.That(controls.SettingsButton.image.sprite.name,Is.EqualTo("SettingsButtonSilver"));
        Assert.That(supply.All(im=>im.sprite.name=="PowerUpTileSilver"),Is.True);
        Assert.That(controls.SettingsButton.image.sprite.rect.size,Is.EqualTo(new Vector2(84,84)));
        foreach(var im in supply)Assert.That(im.sprite.rect.size,Is.EqualTo(new Vector2(105,105)));
        foreach(var im in supply.Concat(new[]{plate,controls.SettingsButton.image,view.Engraving}))
        {
            Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            // The new source canvases already contain the approved enlargement.
            // Assert their actual display density independently of layout helpers.
            float requested = supply.Contains(im) || im==controls.SettingsButton.image ?
                (Screen.width>=720 ? 1 : 2f/3f) : 1;
            Assert.That(Bounds(im).width/im.sprite.rect.width,Is.EqualTo(layout.Current.Scale*requested).Within(.001));
            Assert.That(Bounds(im).height/im.sprite.rect.height,Is.EqualTo(layout.Current.Scale*requested).Within(.001));
            Assert.That(Bounds(im).xMin,Is.EqualTo(Mathf.Round(Bounds(im).xMin)).Within(.001));
            Assert.That(Bounds(im).yMin,Is.EqualTo(Mathf.Round(Bounds(im).yMin)).Within(.001));
        }
        Assert.That(view.Engraving.transform.parent,Is.EqualTo(view.Slab));Assert.That(view.Engraving.raycastTarget,Is.False);
        Assert.That(view.Engraving.sprite,Is.SameAs(RunSession.Current.Player.GetComponent<PlayerAbilityController>().ActiveAbility.StoneEngraving));
        if(character=="skeleton")Assert.That(view.Engraving.sprite.name,Is.EqualTo("RattleBonesAbilityIcon"));
        var icon=view.Engraving.sprite;var p=icon.texture.GetPixels32();var occupied=Enumerable.Range(0,p.Length).Where(i=>p[i].a>0).ToArray();
        float cx=(occupied.Min(i=>i%icon.texture.width)+occupied.Max(i=>i%icon.texture.width)+1)*.5f;
        float cy=(occupied.Min(i=>i/icon.texture.width)+occupied.Max(i=>i/icon.texture.width)+1)*.5f;
        var pos=view.Engraving.rectTransform.anchoredPosition;
        Assert.That(pos.x+cx-icon.rect.width*.5f,Is.EqualTo(0).Within(.51),"Visible icon horizontally centered");
        Assert.That(pos.y+cy-icon.rect.height*.5f,Is.EqualTo(-3).Within(.51),"Visible icon centered in inset");
        foreach(var im in supply)Assert.That(Bounds(im).Overlaps(Bounds(plate)),Is.False,"Supply/ability margin");
        var wave=layout.TopHud.Find("WaveTracker").GetComponent<Image>();
        Assert.That(wave.sprite.name,Is.EqualTo("WavePlaqueSilver"));
        Assert.That(wave.sprite.rect.size,Is.EqualTo(new Vector2(264,99)));
        Assert.That(wave.transform.Find("SharedWavePendant")?.gameObject.activeSelf??false,Is.False,"No duplicate skull");
        Assert.That(Bounds(wave).Overlaps(Bounds(controls.SettingsButton.image)),Is.False);
        var label=wave.GetComponentInChildren<TMP_Text>();Assert.That(label.text,Is.EqualTo("WAVE "+RunSession.Current.Waves.CurrentWave));
        string saved=label.text;
        foreach(string value in new[]{"WAVE 1","WAVE 99999"})
        {label.text=value;PixelTextFitter.ApplyNativeMenu(label,48*BattlePresentationSkin.TopControlScale(layout.TopHud));label.ForceMeshUpdate();Assert.That(label.textBounds.size.x,Is.LessThanOrEqualTo(label.rectTransform.rect.width+.1f));
         if(value=="WAVE 1")Assert.That(label.fontSize,Is.GreaterThanOrEqualTo(32),"Readable capitals within the new plaque");}
        label.text=saved;PixelTextFitter.ApplyNativeMenu(label,48*BattlePresentationSkin.TopControlScale(layout.TopHud));
        // Requested large-phone sizes, fixed compact fallback, and precise rail
        // joins are independent of the production helper's implementation.
        float expected = Screen.width>=720 ? 1.5f : 1;
        Assert.That(Bounds(controls.SettingsButton.image).size,Is.EqualTo(Vector2.one*56*expected*layout.Current.Scale));
        foreach(var im in supply)
        {
            Assert.That(Bounds(im).size,Is.EqualTo(Vector2.one*70*expected*layout.Current.Scale));
            var iconRect=im.transform.Find("Icon") as RectTransform;
            var ib=GameplayPixelLayoutController.ScreenRect(iconRect);
            Assert.That(Vector2.Distance(ib.center,Bounds(im).center),Is.LessThan(.8f));
            Assert.That(ib.width,Is.EqualTo(48*expected*layout.Current.Scale).Within(.01f));
            var count=im.transform.Find("CountBacking") as RectTransform;
            var cb=GameplayPixelLayoutController.ScreenRect(count);
            Assert.That(Bounds(im).Contains(cb.min)&&Bounds(im).Contains(cb.max),Is.True,"Count stays inside holder");
            var hint=layout.BottomHud.Find("ConsumableHint") as RectTransform;
            if(hint.gameObject.activeInHierarchy)Assert.That(Bounds(im).Overlaps(GameplayPixelLayoutController.ScreenRect(hint)),Is.False,"Holder clears HUD caption");
        }
        for(int i=0;i<5;i++)
        {
            var part=wave.transform.Find("SilverWavePart"+i).GetComponent<Image>();
            var pb=Bounds(part);
            Assert.That(pb.height,Is.EqualTo(66*expected*layout.Current.Scale).Within(.01));
            if(i>0)Assert.That(pb.xMin,Is.EqualTo(Bounds(wave.transform.Find("SilverWavePart"+(i-1)).GetComponent<Image>()).xMax).Within(.001),"No wave strip seam");
            if(i==1||i==3)Assert.That(part.canvas.rootCanvas.scaleFactor*part.canvas.referencePixelsPerUnit/(part.sprite.pixelsPerUnit*part.pixelsPerUnitMultiplier),Is.EqualTo(expected/1.5f*layout.Current.Scale).Within(.001));
        }
        var board=RunSession.Current.Board.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.name.StartsWith("CellTile"));
        foreach(var tile in board)
        {var lo=Camera.main.WorldToScreenPoint(tile.bounds.min);var hi=Camera.main.WorldToScreenPoint(tile.bounds.max);var b=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y);
         foreach(var im in supply.Concat(new[]{plate,controls.SettingsButton.image,wave}))Assert.That(Bounds(im).Overlaps(b),Is.False,"Control covers board");}
    }
}
