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
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public sealed class StoneAbilityPlayTests
{
    const string Root="ArtSource/StoneAbilityUI/MaterialRevision/Preview/Characters";
    IDisposable profile,selection;
    int oldCapture;bool motionKey,preferencesCaptured;int oldMotion;
    [UnityTearDown]public IEnumerator Cleanup()
    {
        Time.timeScale=1;Time.captureFramerate=oldCapture;
        if(preferencesCaptured){if(motionKey)PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,oldMotion);else PlayerPrefs.DeleteKey(PresentationPreferences.ReducedMotionKey);}
        profile?.Dispose();selection?.Dispose();RunLaunchOptions.StartingZone=null;
        if(Application.isPlaying)yield return new ExitPlayMode();
    }
    static IEnumerator Until(Func<bool> condition){float end=Time.realtimeSinceStartup+90;while(!condition()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    static IEnumerator Pause(float seconds){float end=Time.realtimeSinceStartup+seconds;yield return Until(()=>Time.realtimeSinceStartup>=end);}
    static void Shot(string p)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath(p));
    [UnityTest]public IEnumerator RattlebonesStonePlate()=>Character("skeleton");
    [UnityTest]public IEnumerator BardleyStonePlate()=>Character("bardley");
    [UnityTest]public IEnumerator MinotaurStonePlateAndBullCharge()=>Character("dungeon_attender");
    [UnityTest]public IEnumerator KoboldStonePlateAndRapidSixShots()=>Character("kobold_gunner");
    [UnitySetUp]public IEnumerator Setup()
    {
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(720,1280)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        oldCapture=Time.captureFramerate;
        preferencesCaptured=true;motionKey=PlayerPrefs.HasKey(PresentationPreferences.ReducedMotionKey);oldMotion=PlayerPrefs.GetInt(PresentationPreferences.ReducedMotionKey);PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,0);
    }
    IEnumerator Character(string id)
    {
        string path=Path.GetFullPath(".utmp/StoneCharacters/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        profile=AccountProgression.UseDisposableProfile(path);selection=CharacterSelectionSettings.UseTemporarySelection(id);
        RunLaunchOptions.StartingZone="drowned-court";SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>SceneManager.GetActiveScene().name=="Game"&&RunSession.Current?.Player?.IsInitialized==true&&RunSession.Current.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
        var run=RunSession.Current;var controller=run.Player.GetComponent<PlayerAbilityController>();
        Assert.That(run.Player.Definition.PlayerId,Is.EqualTo(id));
        foreach(var enemy in run.Waves.ActiveEnemies)typeof(EnemyActor).GetField("currentHealth",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(enemy,10000);
        var view=Object.FindFirstObjectByType<StoneAbilityButtonView>();Assert.That(view,Is.Not.Null);
        Assert.That(view.Engraving.sprite,Is.SameAs(controller.ActiveAbility.StoneEngraving));Assert.That(view.IsRaised,Is.False);Assert.That(view.Glow.color.a,Is.Zero);
        string dir=Root+"/"+id;Directory.CreateDirectory(dir);Shot(dir+"/Charging.png");yield return Pause(.2f);
        Time.captureFramerate=30;run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(1000);
        for(int f=0;f<24;f++){Shot(dir+"/Rise"+f.ToString("D3")+".png");yield return null;}
        Time.captureFramerate=oldCapture;yield return Until(()=>view.IsRaised);
        Assert.That(view.Slab.anchoredPosition,Is.EqualTo(new Vector2(-1,3)));Assert.That(view.Glow.color.a,Is.GreaterThan(.5f));
        Shot(dir+"/Ready.png");yield return Pause(.2f);
        Time.timeScale=0;var paused=view.Slab.anchoredPosition;var alpha=view.Glow.color.a;yield return Pause(.2f);
        Assert.That(view.Slab.anchoredPosition,Is.EqualTo(paused));Assert.That(view.Glow.color.a,Is.EqualTo(alpha));Time.timeScale=1;
        PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,1);yield return null;yield return null;
        Assert.That(view.Slab.anchoredPosition,Is.EqualTo(Vector2.zero));Assert.That(view.Glow.color.a,Is.GreaterThan(.5f));
        PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,0);yield return null;yield return null;
        if(id=="kobold_gunner"||id=="dungeon_attender")
        {
            var runtime=run.Player.GetComponent<SequentialBoardAbilityRuntime>();Assert.That(runtime,Is.Not.Null);
            var hits=new List<float>();runtime.Striking+=(step,cell)=>hits.Add(Time.time);
            int energy=run.Player.GetComponent<PlayerAbilityEnergy>().CurrentEnergy,moves=run.Board.CompletedValidPlayerMoves;
            Time.captureFramerate=60;float start=Time.time;Assert.That(controller.TryActivate(),Is.True);
            Assert.That(run.Player.GetComponent<PlayerAbilityEnergy>().CurrentEnergy,Is.EqualTo(energy-controller.RequiredEnergy));
            Assert.That(controller.TryActivate(),Is.False);bool sawIcon=false;int frame=0;
            var times=new List<string>();
            while(runtime.IsActive||frame<12)
            {
                Assert.That(frame,Is.LessThan(600),"Ability finishes within ten game seconds");
                if(id=="dungeon_attender")
                {
                    var icon=GameObject.Find("AttenderBoardAvatar")?.GetComponent<SpriteRenderer>();
                    if(icon&&icon.enabled){sawIcon=true;Assert.That(icon.sprite,Is.SameAs(((DungeonAttenderAbilityDefinition)controller.ActiveAbility).BoardAvatar));Assert.That(icon.sprite.name,Is.EqualTo("MinotaurCharge"));Assert.That(icon.bounds.size.x,Is.LessThanOrEqualTo(run.Board.CellSize));}
                    Assert.That(GameObject.Find("AttenderLandingMarker"),Is.Null,"No full-character marker remains on the board");
                }
                if(frame%2==0){Shot(dir+"/Frame"+frame.ToString("D3")+".png");times.Add(frame+","+(Time.time-start).ToString("F4",System.Globalization.CultureInfo.InvariantCulture));}
                frame++;yield return null;
            }
            yield return Pause(.2f);Time.captureFramerate=oldCapture;
            Assert.That(hits.Count,Is.EqualTo(id=="kobold_gunner"?6:7));Assert.That(run.Board.CompletedValidPlayerMoves,Is.EqualTo(moves));
            if(id=="kobold_gunner")Assert.That(hits.Last()-hits.First(),Is.LessThan(.8f),"Six shots empty rapidly without ordinary tile-clear holds");else Assert.That(sawIcon,Is.True);
            File.WriteAllLines(dir+"/FrameTimes.csv",times);
            File.WriteAllText(dir+"/Timing.txt","Impacts="+hits.Count+"\nFirst-to-last seconds="+(hits.Last()-hits.First())+"\nEnergy spent="+controller.RequiredEnergy+"\nPlayer moves="+(run.Board.CompletedValidPlayerMoves-moves));
            yield return Until(()=>run.Continuation.CanCapture);Assert.That(view.IsRaised,Is.False);
        }
        else
        {
            // The same slab has one native rise and a restrained white pulse.
            Time.captureFramerate=30;for(int f=0;f<36;f++){Shot(dir+"/Frame"+f.ToString("D3")+".png");yield return null;}Time.captureFramerate=oldCapture;
        }
        yield return Pause(.2f);
    }
    [UnityTest]public IEnumerator MenuHasOnlyFourCharacterChoices()
    {
        string path=Path.GetFullPath(".utmp/StoneCharacters/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(new AccountSave()));
        profile=AccountProgression.UseDisposableProfile(path);selection=CharacterSelectionSettings.UseTemporarySelection("skeleton");
        SceneManager.LoadScene("MainMenu");yield return null;
        yield return Until(()=>Object.FindFirstObjectByType<MainMenuController>()!=null);
        Object.FindFirstObjectByType<MainMenuController>().ShowCharacterSelect();yield return Pause(.4f);
        var menu=Object.FindFirstObjectByType<CharacterSelectMenuController>();Assert.That(menu,Is.Not.Null);
        Assert.That(menu.GetComponentsInChildren<Button>(true).Any(b=>b.name=="GideonButton"),Is.False);
        Assert.That(menu.GetComponentsInChildren<Button>().Any(b=>b.name=="MinotaurButton"),Is.True);
        Assert.That(menu.GetComponentsInChildren<Button>().Any(b=>b.name=="KoboldButton"),Is.True);
        Directory.CreateDirectory(Root);Shot(Root+"/FourCharacterMenu.png");yield return Pause(.2f);
    }
    [Test]public void FourSelectableCharactersPreserveGideonIdentity()
    {
        foreach(string id in new[]{"skeleton","bardley","dungeon_attender","kobold_gunner"})Assert.That(CharacterSelectionSettings.IsSelectableCharacter(id),Is.True);
        Assert.That(CharacterSelectionSettings.IsSelectableCharacter("gideon_glass"),Is.False);
        Assert.That(CharacterSelectionSettings.IsKnownCharacter("gideon_glass"),Is.True,"Existing saves keep the fifth character identity");
        using(CharacterSelectionSettings.UseTemporarySelection("gideon_glass"))Assert.That(CharacterSelectionSettings.SelectedPlayerId,Is.EqualTo("gideon_glass"),"An existing continuation remains loadable");
    }
}
