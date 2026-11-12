using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Uses disposable accounts and the actual Game scene. The fixture supplies
// controlled charge levels; activation/spending use the production controller.
public sealed class AbilityReadinessTests
{
    const string Root=".utmp/AbilityReadiness";
    IDisposable profile,selection;
    int oldCapture,oldMotion;bool hadMotion,prefsCaptured;
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> ready){float end=Time.realtimeSinceStartup+100;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;yield return Until(()=>Time.realtimeSinceStartup>=end);}
    [UnitySetUp] public IEnumerator Setup()
    {
        Size(720,1280);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        oldCapture=Time.captureFramerate;hadMotion=PlayerPrefs.HasKey(PresentationPreferences.ReducedMotionKey);oldMotion=PlayerPrefs.GetInt(PresentationPreferences.ReducedMotionKey);prefsCaptured=true;
        PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,0);
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;Time.captureFramerate=oldCapture;
        if(prefsCaptured){if(hadMotion)PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,oldMotion);else PlayerPrefs.DeleteKey(PresentationPreferences.ReducedMotionKey);}
        profile?.Dispose();selection?.Dispose();RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;
        if(Application.isPlaying)yield return new ExitPlayMode();
    }
    IEnumerator Load(string id,string zone)
    {
        profile?.Dispose();selection?.Dispose();
        string path=Path.GetFullPath(Root+"/Profiles/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        profile=AccountProgression.UseDisposableProfile(path);selection=CharacterSelectionSettings.UseTemporarySelection(id);
        RunLaunchOptions.StartingZone=zone;SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
        yield return Until(()=>RunSession.Current.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
        Assert.That(RunSession.Current.Player.Definition.PlayerId,Is.EqualTo(id));
    }
    static Rect BarBounds(StoneAbilityButtonView view)=>GameplayPixelLayoutController.ScreenRect((RectTransform)view.transform.Find("AbilityEnergyBar"));
    static RectTransform Mask(StoneAbilityButtonView view)=>(RectTransform)view.transform.Find("AbilityEnergyBar/EnergyFillMask");
    static void Structure(StoneAbilityButtonView view)
    {
        var root=view.transform.Find("AbilityEnergyBar");var shell=root.Find("EnergyFrame").GetComponent<Image>();
        Assert.That(shell.sprite.name,Is.EqualTo("EnergyFrameSilver"));
        Assert.That(shell.sprite.rect.size,Is.EqualTo(new Vector2(147,24)));
        Assert.That(shell.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
        for(int y=8;y<16;y++)for(int x=15;x<132;x++)Assert.That(shell.sprite.texture.GetPixel(x,y).a,Is.Zero,"The rim must not cover live fill");
        var fill=Mask(view).GetComponentInChildren<Image>();Assert.That(fill.gameObject.activeInHierarchy&&fill.enabled,Is.True);
        Assert.That(fill.rectTransform.rect.size,Is.EqualTo(new Vector2(117,8)));
        Assert.That(fill.raycastTarget,Is.False);
    }
    // Read rendered pixels, not just fillAmount/mask values: the former bug
    // updated those values correctly while an opaque frame hid the result.
    static void AssertVisibleFill(string path,Rect bar,float charge)
    {
        var shot=new Texture2D(2,2,TextureFormat.RGBA32,false);shot.LoadImage(File.ReadAllBytes(path));
        float scale=bar.width/147;int width=Mathf.RoundToInt(117*scale),left=Mathf.RoundToInt(bar.xMin+15*scale),y=Mathf.FloorToInt(bar.yMin+12*scale);
        int expected=Mathf.RoundToInt(Mathf.Round(charge*117)*scale);int count=0;
        for(int x=0;x<width;x++)
        {
            Color c=shot.GetPixel(left+x,y);bool purple=c.r>.3f&&c.b>.35f&&c.b>c.g*1.4f&&c.r>c.g*1.3f;
            if(purple)count++;
            if(x<expected-1)Assert.That(purple,Is.True,"Missing rendered charge pixel "+x+" in "+path);
            if(x>expected+1)
            {
                Assert.That(purple,Is.False,"Charge leaked past its mask in "+path);
                Assert.That(c.r,Is.EqualTo(28f/255).Within(1.1f/255),"Empty track red");
                Assert.That(c.g,Is.EqualTo(36f/255).Within(1.1f/255),"Empty track green");
                Assert.That(c.b,Is.EqualTo(56f/255).Within(1.1f/255),"Empty track blue");
            }
        }
        Object.Destroy(shot);Assert.That(count,Is.EqualTo(expected).Within(1),"Visible fill width: "+path);
    }
    [UnityTest] public IEnumerator FourCharactersWhiteChargingColorReadyAndVisibleMeter()
    {
        foreach(string id in new[]{"skeleton","dungeon_attender","kobold_gunner","bardley"})
        {
            yield return Load(id,"drowned-court");var run=RunSession.Current;
            var energy=run.Player.GetComponent<PlayerAbilityEnergy>();var controller=run.Player.GetComponent<PlayerAbilityController>();
            energy.ResetEnergy();yield return Wait(.8f);
            var view=Object.FindFirstObjectByType<StoneAbilityButtonView>();Assert.That(view,Is.Not.Null);Structure(view);
            Assert.That(view.Engraving.sprite,Is.SameAs(controller.ActiveAbility.StoneEngraving));
            Assert.That(view.IsRaised,Is.False);Assert.That(view.Glow.color.a,Is.EqualTo(1));
            energy.AddEnergy(controller.RequiredEnergy);yield return Wait(.8f);
            Assert.That(view.Slab.anchoredPosition,Is.EqualTo(new Vector2(-1,3)));Assert.That(view.Glow.color.a,Is.Zero);
            Time.timeScale=0;var position=view.Slab.anchoredPosition;yield return Wait(.15f);
            Assert.That(view.Slab.anchoredPosition,Is.EqualTo(position));Assert.That(view.Glow.color.a,Is.Zero);Time.timeScale=1;
            PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,1);yield return null;yield return null;
            Assert.That(view.Slab.anchoredPosition,Is.EqualTo(Vector2.zero));Assert.That(view.Glow.color.a,Is.Zero);
            PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,0);
            energy.ResetEnergy();yield return Wait(.8f);
            Rect bar=BarBounds(view),button=GameplayPixelLayoutController.ScreenRect(view.GetComponent<Button>().image.rectTransform);
            Rect union=Rect.MinMaxRect(Mathf.Min(bar.xMin,button.xMin)-16,Mathf.Min(bar.yMin,button.yMin)-12,Mathf.Max(bar.xMax,button.xMax)+16,Mathf.Max(bar.yMax,button.yMax)+12);
            string dir=Path.GetFullPath(Root+"/Raw/"+id);Directory.CreateDirectory(dir);
            File.WriteAllText(dir+"/Crop.json",JsonUtility.ToJson(new Crop{x=Mathf.RoundToInt(union.xMin),y=Mathf.RoundToInt(union.yMin),width=Mathf.RoundToInt(union.width),height=Mathf.RoundToInt(union.height),screenWidth=720,screenHeight=1280,fps=30,frames=240,icon=view.Engraving.sprite.name}));
            var rows=new List<string>{"frame,phase,liftX,liftY,whiteAlpha,energy,fillWidth"};var lifts=new HashSet<Vector2>();
            var checks=new List<(int frame,float charge)>();Time.captureFramerate=30;
            for(int f=0;f<240;f++)
            {
                if(f==30||f==60||f==90||f==120){int target=Mathf.RoundToInt(controller.RequiredEnergy*(f/30)*.25f);energy.AddEnergy(target-energy.CurrentEnergy);}
                if(f==180){int before=energy.CurrentEnergy;Assert.That(controller.TryActivate(),Is.True);Assert.That(energy.CurrentEnergy,Is.EqualTo(before-controller.RequiredEnergy));}
                ScreenCapture.CaptureScreenshot(dir+"/Frame"+f.ToString("D3")+".png");
                string phase=f<30?"empty":f<60?"quarter":f<90?"half":f<120?"three-quarter":f<180?"ready":"spent";
                rows.Add(string.Join(",",f,phase,view.Slab.anchoredPosition.x.ToString(CultureInfo.InvariantCulture),view.Slab.anchoredPosition.y.ToString(CultureInfo.InvariantCulture),view.Glow.color.a.ToString(CultureInfo.InvariantCulture),energy.CurrentEnergy,Mask(view).rect.width));
                if(f>=120&&f<135)lifts.Add(view.Slab.anchoredPosition);
                if(new[]{29,59,89,119,179,239}.Contains(f))
                {
                    Assert.That(Mask(view).rect.width,Is.EqualTo(Mathf.Round(controller.ChargeNormalized*117)));
                    Assert.That(view.Glow.color.a,Is.EqualTo(controller.CanActivate?0:1));
                    Assert.That(view.IsRaised,Is.EqualTo(controller.CanActivate));checks.Add((f,controller.ChargeNormalized));
                }
                yield return null;
            }
            Assert.That(lifts.Count,Is.GreaterThanOrEqualTo(3),"Crisp native pop-out steps");
            Time.captureFramerate=oldCapture;yield return Until(()=>File.Exists(dir+"/Frame239.png"));
            foreach(var check in checks)AssertVisibleFill(dir+"/Frame"+check.frame.ToString("D3")+".png",bar,check.charge);
            File.WriteAllLines(dir+"/Frames.csv",rows);
            yield return Until(()=>run.Continuation.CanCapture);
        }
    }
    [UnityTest] public IEnumerator SharedMeterRendersAcrossFiveZonesAndPhoneLayouts()
    {
        string dir=Path.GetFullPath(Root+"/Layouts");Directory.CreateDirectory(dir);
        foreach(string zone in new[]{"dungeon","magical-forest","drowned-court","ironvein-excavation","frostwake-peaks"})
        {
            yield return Load("skeleton",zone);var run=RunSession.Current;run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(50);
            foreach(var size in new[]{new Vector3Int(360,640,0),new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,1)})
            {
                GameplayPixelLayoutController.ValidationSafeArea=null;Size(size.x,size.y);
                yield return Until(()=>UnityEngine.Device.Screen.width==size.x&&UnityEngine.Device.Screen.height==size.y);
                GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                yield return Wait(.6f);Canvas.ForceUpdateCanvases();var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);Assert.That(errors,Is.Empty,report);
                var view=Object.FindFirstObjectByType<StoneAbilityButtonView>();Structure(view);var bar=BarBounds(view);
                Assert.That(bar.width/147,Is.EqualTo(layout.Current.Scale).Within(.001));Assert.That(bar.height/24,Is.EqualTo(layout.Current.Scale).Within(.001));
                string path=dir+"/"+zone+"-"+size.x+"x"+size.y+".png";ScreenCapture.CaptureScreenshot(path);yield return Until(()=>File.Exists(path));yield return null;
                AssertVisibleFill(path,bar,run.Player.GetComponent<PlayerAbilityController>().ChargeNormalized);
            }
            GameplayPixelLayoutController.ValidationSafeArea=null;
        }
    }
    [UnityTest] public IEnumerator OrdinaryMatchChargesTheVisibleMeter()
    {
        yield return Load("skeleton","dungeon");var run=RunSession.Current;
        var energy=run.Player.GetComponent<PlayerAbilityEnergy>();energy.ResetEnergy();yield return Wait(.8f);
        Assert.That(run.Board.TryGetRandomHintMove(out var from,out var to),Is.True);
        int moves=run.Board.CompletedValidPlayerMoves;
        var action=(IEnumerator)typeof(BoardController).GetMethod("TrySwap",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(run.Board,new object[]{from,to});
        run.Board.StartCoroutine(action);
        yield return Until(()=>run.Board.CompletedValidPlayerMoves>moves&&run.Continuation.CanCapture);
        yield return Wait(.8f);Assert.That(energy.CurrentEnergy,Is.GreaterThan(0),"Production matching grants energy");
        var view=Object.FindFirstObjectByType<StoneAbilityButtonView>();
        float charge=run.Player.GetComponent<PlayerAbilityController>().ChargeNormalized;
        Assert.That(Mask(view).rect.width,Is.EqualTo(Mathf.Round(charge*117)));
        string path=Path.GetFullPath(Root+"/OrdinaryMatch.png");ScreenCapture.CaptureScreenshot(path);
        yield return Until(()=>File.Exists(path));yield return null;AssertVisibleFill(path,BarBounds(view),charge);
    }
    [Serializable]public class Crop{public int x,y,width,height,screenWidth,screenHeight,fps,frames;public string icon;}
}
