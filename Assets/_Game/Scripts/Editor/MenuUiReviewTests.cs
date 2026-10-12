using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public sealed class MenuUiReviewTests
{
    private IDisposable profile, selection;
    private string output;
    [UnitySetUp] public IEnumerator SetUp()
    {
        EditorUtility.audioMasterMute = true;
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
            .Invoke(null,new object[]{new Vector2Int(1080,1920)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        output = Path.GetFullPath(".utmp/MenuUI/" + (Resources.Load<Sprite>("UI/GlobalMenu/Hall") == null ? "before" : "after"));
        Directory.CreateDirectory(output);
        string path = Path.Combine(output, "profile-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, JsonUtility.ToJson(new AccountSave { gold=100, potions=3, bombs=3, equipPotions=true, equipBombs=true }));
        profile=AccountProgression.UseDisposableProfile(path);
        selection=CharacterSelectionSettings.UseTemporarySelection("skeleton");
    }
    [UnityTearDown] public IEnumerator TearDown()
    {
        GlobalMenuPixelRoot.ValidationSafeArea=null;
        Time.timeScale=1; SceneManager.LoadScene("MainMenu"); yield return null;
        profile?.Dispose(); selection?.Dispose();
        RunLaunchOptions.StartingZone=null; RunLaunchOptions.Practice=false;
        yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator HomeNavigationPreferencesAndSavedRunContinue()
    {
        SceneManager.LoadScene("MainMenu");yield return new WaitForSecondsRealtime(.6f);
        var menu=Object.FindFirstObjectByType<MainMenuController>();
        Click("CharactersButton");yield return null;Assert.That(Object.FindFirstObjectByType<CharacterSelectMenuController>(),Is.Not.Null);menu.ShowHome();yield return null;
        Click("ShopButton");yield return null;Assert.That(Object.FindFirstObjectByType<ShopMenuController>(),Is.Not.Null);menu.ShowHome();yield return null;
        menu.ShowGemMastery();yield return null;Assert.That(Object.FindFirstObjectByType<GemMasteryMenuController>(),Is.Not.Null);menu.ShowHome();yield return null;
        bool music=AudioPreferences.MusicMuted,sfx=AudioPreferences.SfxMuted,vibration=AudioPreferences.VibrationMuted,motion=PresentationPreferences.ReducedMotion;
        try
        {
            Click("MenuSettings");yield return new WaitForSecondsRealtime(.3f);
            var dimHits=new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=new Vector2(2,2)},dimHits);
            Assert.That(dimHits.Count,Is.GreaterThan(0));
            Assert.That(dimHits[0].gameObject.name,Is.EqualTo("MenuSettingsOverlay"),"modal covers the full screen outside inset HomeScreen");
            yield return Shot("menu-settings");
            Click("Music");Assert.That(AudioPreferences.MusicMuted,Is.EqualTo(!music));
            Click("SFX");Assert.That(AudioPreferences.SfxMuted,Is.EqualTo(!sfx));
            Click("Vibration");Assert.That(AudioPreferences.VibrationMuted,Is.EqualTo(!vibration));
            Click("ReducedMotion");Assert.That(PresentationPreferences.ReducedMotion,Is.EqualTo(!motion));
            Click("CloseMenuSettings");yield return null;Assert.That(GameObject.Find("MenuSettingsOverlay"),Is.Null);
        }
        finally
        {
            AudioPreferences.SetMusicMuted(music);AudioPreferences.SetSfxMuted(sfx);
            AudioPreferences.SetVibrationMuted(vibration);PresentationPreferences.SetReducedMotion(motion);
        }
        Click("PlayButton");yield return null;Assert.That(GameObject.Find("TestingZonePicker"),Is.Not.Null);
        Click("StartZone_dungeon");yield return Until(()=>RunSession.Current!=null&&RunSession.Current.Continuation.CanCapture&&RunSession.Current.Waves.IsWaveActive);
        string id=AccountProgression.Current.ActiveRun.id;
        var controls=Object.FindFirstObjectByType<RunControlsUI>();controls.OpenSettings();yield return null;
        Click("Retry");yield return null;Assert.That(GameObject.Find("Confirm"),Is.Not.Null);Assert.That(Time.timeScale,Is.Zero);
        Click("Cancel");yield return null;Assert.That(GameObject.Find("Suspend"),Is.Not.Null);
        Click("Suspend");yield return Until(()=>Object.FindFirstObjectByType<MainMenuController>()!=null);
        yield return new WaitForSecondsRealtime(.3f);yield return Shot("saved-run");
        Assert.That(AccountProgression.Current.ActiveRun.id,Is.EqualTo(id));
        Click("PlayButton");yield return Until(()=>RunSession.Current!=null&&RunSession.Current.Continuation.CanCapture&&RunSession.Current.Waves.IsWaveActive);
        // Successful restore deliberately opens the existing paused build recap.
        Assert.That(GameObject.Find("ResumeGuide"),Is.Not.Null);
        Assert.That(Time.timeScale,Is.Zero);Click("ResumeGuide");yield return null;
        Assert.That(AccountProgression.Current.ActiveRun.id,Is.EqualTo(id));Assert.That(Time.timeScale,Is.EqualTo(1));
    }
    [UnityTest] public IEnumerator NativeMenuLayoutsFitSafeAreasAndEveryPlayable()
    {
        SceneManager.LoadScene("MainMenu");yield return new WaitForSecondsRealtime(.4f);
        foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2400),new Vector2Int(720,1280)})
        {
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
            yield return Until(()=>Screen.width==size.x&&Screen.height==size.y);
            GlobalMenuPixelRoot.ValidationSafeArea=new Rect(0,40,size.x,size.y-80);
            yield return new WaitForSecondsRealtime(.6f);
            ValidateNativeStage();yield return Shot(size.y+"-safe-area");
        }
        GlobalMenuPixelRoot.ValidationSafeArea=null;
        foreach(string id in new[]{"skeleton","bardley",CharacterSelectionSettings.GideonPlayerId,CharacterSelectionSettings.MinotaurPlayerId,CharacterSelectionSettings.KoboldPlayerId})
        {
            selection.Dispose();selection=CharacterSelectionSettings.UseTemporarySelection(id);
            Object.FindFirstObjectByType<MainMenuController>().ShowHome();yield return new WaitForSecondsRealtime(.3f);
            var actor=GameObject.Find("MenuCharacter").GetComponent<Image>();
            Assert.That(actor.sprite,Is.Not.Null);Assert.That(actor.rectTransform.rect.size,Is.EqualTo(actor.sprite.rect.size));
            ValidateNativeStage();yield return Shot("hero-"+id);
        }
    }
    private static void Click(string name)
    {
        var button=GameObject.Find(name)?.GetComponent<Button>();
        Assert.That(button,Is.Not.Null,name);Assert.That(button.interactable,Is.True,name);
        Canvas.ForceUpdateCanvases();
        var rect=(RectTransform)button.transform;
        var canvas=button.GetComponentInParent<Canvas>().rootCanvas;
        var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        var pointer=new PointerEventData(EventSystem.current)
        {position=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center)),button=PointerEventData.InputButton.Left};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Assert.That(hits.Count,Is.GreaterThan(0),name+" hit area");
        Assert.That(hits[0].gameObject.GetComponentInParent<Button>(),Is.EqualTo(button),name+" unobstructed hit area");
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
    }
    private static void ValidateNativeStage()
    {
        foreach(var stage in Object.FindObjectsByType<GlobalMenuPixelRoot>(FindObjectsSortMode.None))
        {
            var corners=new Vector3[4];((RectTransform)stage.transform).GetWorldCorners(corners);
            float ratio=(corners[3].x-corners[0].x)/((RectTransform)stage.transform).rect.width;
            Assert.That(ratio,Is.EqualTo(Mathf.Round(ratio)).Within(.001f),"integer native stage scale");
            foreach(var text in stage.GetComponentsInChildren<TMP_Text>())
            {text.ForceMeshUpdate();Assert.That(text.isTextTruncated,Is.False,text.name+": "+text.text);Assert.That(text.isTextOverflowing,Is.False,text.name+": "+text.text);}
            foreach(var image in stage.GetComponentsInChildren<Image>())
                if(image.sprite!=null)Assert.That(image.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            foreach(var button in stage.GetComponentsInChildren<Button>().Where(b=>b.name=="Music"||b.name=="SFX"||b.name=="ReducedMotion"||b.name=="Vibration"))
                Assert.That(button.transform.Find("Label").GetComponent<TMP_Text>().fontSize,Is.EqualTo(16).Within(.01f),button.name+" keeps the readable row size");
        }
    }
    [UnityTest] public IEnumerator CaptureMenusAndPreservePause()
    {
        foreach (int height in new[]{1920,2400})
        {
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
                .Invoke(null,new object[]{new Vector2Int(1080,height)});
            yield return Until(()=>Screen.width==1080 && Screen.height==height);
            SceneManager.LoadScene("MainMenu"); yield return new WaitForSecondsRealtime(.7f);
            Assert.That(Object.FindFirstObjectByType<MainMenuController>(),Is.Not.Null);
            yield return Shot(height+"-main-menu");
            RunLaunchOptions.StartingZone="dungeon";
            SceneManager.LoadScene("Game");
            yield return Until(()=>RunSession.Current!=null && RunSession.Current.Continuation.CanCapture && RunSession.Current.Waves.IsWaveActive);
            // The second viewport resumes the attempt suspended by the first.
            // Dismiss its intentional recap before opening the settings modal.
            if(GameObject.Find("ResumeGuide")!=null) {Click("ResumeGuide");yield return null;}
            yield return new WaitForSecondsRealtime(.7f);
            var controls=Object.FindFirstObjectByType<RunControlsUI>();
            controls.OpenSettings(); yield return new WaitForSecondsRealtime(.3f);
            if(Resources.Load<Sprite>("UI/GlobalMenu/Panel")!=null)
                Assert.That(GameObject.Find("NativeSettingsStage"),Is.Not.Null,"capture the settings panel, not the continuation guide");
            Assert.That(Time.timeScale,Is.Zero); int tick=RunSession.Current.MoveClock.Tick;
            yield return Shot(height+"-settings");
            Assert.That(RunSession.Current.MoveClock.Tick,Is.EqualTo(tick));
            controls.Close(); yield return null; Assert.That(Time.timeScale,Is.EqualTo(1));
            Assert.That(RunSession.Current.SuspendToMenu(),Is.True); yield return null;
        }
    }
    private IEnumerator Shot(string name)
    {
        Canvas.ForceUpdateCanvases(); yield return new WaitForSecondsRealtime(.3f);
        if(Resources.Load<Sprite>("UI/GlobalMenu/Hall")!=null)ValidateNativeStage();
        File.WriteAllLines(Path.Combine(output,name+".text.txt"),Object.FindObjectsByType<GlobalMenuPixelRoot>(FindObjectsSortMode.None)
            .SelectMany(s=>s.GetComponentsInChildren<TMP_Text>()).Select(t=>$"{t.transform.parent.name}/{t.name}: em={t.fontSize}; rect={t.rectTransform.rect.size}; preferred={t.GetPreferredValues(t.text)}; text={t.text.Replace('\n',' ')}"));
        ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));
        yield return new WaitForSecondsRealtime(.3f);
    }
    private static IEnumerator Until(Func<bool> ready)
    { float end=Time.realtimeSinceStartup+40; while(!ready()) {Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;} }
}
