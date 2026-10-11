using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    private static readonly string[] NewPlayableIds={CharacterSelectionSettings.MinotaurPlayerId,CharacterSelectionSettings.KoboldPlayerId};
    private void ResetPlayableProfile()
    {
        profile.Dispose();File.WriteAllText(path,JsonUtility.ToJson(new AccountSave {potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        profile=AccountProgression.UseDisposableProfile(path);
    }
    private static IEnumerator PlayableScreen(string name)
    {
        string folder=Path.GetFullPath(".utmp/FrostwakePlayables/Runtime");Directory.CreateDirectory(folder);
        yield return null;yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return null;yield return null;
    }
    [UnityTest] public IEnumerator NewPlayablesProductionPresentationPauseAndPortraitLayouts()
    {
        foreach(string id in NewPlayableIds)
        {
            character.Dispose();character=CharacterSelectionSettings.UseTemporarySelection(id);
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(1080,1920)});
            yield return Until(()=>Screen.width==1080 && Screen.height==1920,"portrait screen before scene load");
            for(int frame=0;frame<6;frame++)yield return null;
            yield return Launch();QuietKitFixture();
            foreach(var enemy in Run.Waves.ActiveEnemies)Set(enemy,"currentHealth",10000);
            Assert.That(Run.Player.Definition.PlayerId,Is.EqualTo(id));
            var image=Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Single(i=>i.name=="PlayerCharacter");
            var animator=image.GetComponent<Animator>();
            Assert.That(animator.HasState(0,Animator.StringToHash("Base Layer.Hit")),Is.True);
            Assert.That(animator.HasState(0,Animator.StringToHash("Base Layer.Death")),Is.True);
            foreach(int height in new[]{1920,2400})
            {
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(1080,height)});
                yield return Until(()=>Screen.height==height && Object.FindFirstObjectByType<GameplayPixelLayoutController>().Current.Safe.height==height,"rendered portrait dimensions agree");
                for(int frame=0;frame<6;frame++)yield return null;
                yield return PlayableScreen(id+"-"+height);
            }
            var controller=Run.Player.GetComponent<PlayerAbilityController>();var energy=Run.Player.GetComponent<PlayerAbilityEnergy>();energy.AddEnergy(100);
            int before=energy.CurrentEnergy;Assert.That(controller.TryActivate(),Is.True);
            Assert.That(energy.CurrentEnergy,Is.EqualTo(before-controller.RequiredEnergy));
            var runtime=Run.Player.GetComponent<SequentialBoardAbilityRuntime>();
            var view=Run.Player.GetComponent<PlayerImpactPresentation>();
            Assert.That(view,Is.Not.Null);
            yield return new WaitForSeconds(.1f);
            if(id==CharacterSelectionSettings.MinotaurPlayerId)
            {Assert.That(image.GetComponent<CanvasGroup>(),Is.Not.Null);Assert.That(image.GetComponent<CanvasGroup>().alpha,Is.Zero);}
            yield return PlayableScreen(id+"-anticipation");
            yield return Until(()=>view.PresentedImpacts>0,"first production impact");
            Assert.That(Run.Board.IsBusy,Is.True);yield return PlayableScreen(id+"-impact");
            Time.timeScale=0;int impacts=view.PresentedImpacts;yield return new WaitForSecondsRealtime(.25f);
            Assert.That(view.PresentedImpacts,Is.EqualTo(impacts));Time.timeScale=1;
            yield return Stable();Assert.That(runtime.IsActive,Is.False);Assert.That(view.HasBoardAvatar,Is.False);
            Assert.That(view.PresentedImpacts,Is.EqualTo(id==CharacterSelectionSettings.MinotaurPlayerId?7:6));
            var visibility=image.GetComponent<CanvasGroup>();
            Assert.That(visibility!=null?visibility.alpha:1,Is.EqualTo(1));
            Assert.That(Run.Board.CompletedValidPlayerMoves,Is.Zero);
            Run.Player.TryTakeDamage(5);yield return new WaitForSeconds(.1f);yield return PlayableScreen(id+"-hit");
            float hitStart=Time.time;
            for(int frame=0;frame<120 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");frame++)yield return null;
            var info=animator.GetCurrentAnimatorStateInfo(0);
            Assert.That(info.IsName("Idle"),Is.True,"authored hit returns to idle: "+id+" clips="+string.Join(",",animator.GetCurrentAnimatorClipInfo(0).Select(c=>c.clip.name))+" speed="+animator.speed+" time="+info.normalizedTime+" length="+info.length+" elapsed="+(Time.time-hitStart)+" mode="+animator.updateMode+" controller="+animator.runtimeAnimatorController.name);
            energy.AddEnergy(100);Assert.That(controller.TryActivate(),Is.True);yield return new WaitForSeconds(.1f);
            Run.Player.TryTakeDamage(100000);yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Run.Player.IsDefeated,Is.True);Assert.That(runtime.IsActive,Is.False);Assert.That(view.HasBoardAvatar,Is.False);
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Death"),Is.True);
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).normalizedTime,Is.GreaterThan(.2f),"death advances while gameplay time is frozen");
            yield return PlayableScreen(id+"-defeat");Time.timeScale=1;
            SceneManager.LoadScene("MainMenu");yield return null;
            ResetPlayableProfile();
        }
    }
    [UnityTest] public IEnumerator NewPlayablesMenuAndLiveZonesKeepExistingClocksAndUi()
    {
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(1080,2400)});
        foreach(string id in NewPlayableIds)
        {
            character.Dispose();character=CharacterSelectionSettings.UseTemporarySelection(id);
            yield return OpenZoneMenu();Object.FindFirstObjectByType<MainMenuController>().ShowCharacterSelect();yield return null;
            var menu=Object.FindFirstObjectByType<CharacterSelectMenuController>();Assert.That(menu,Is.Not.Null);
            string button=id==CharacterSelectionSettings.MinotaurPlayerId?"MinotaurButton":"KoboldButton";
            ClickZoneMenuButton(GameObject.Find(button).GetComponent<Button>());yield return PlayableScreen(id+"-menu");
            foreach(var label in menu.GetComponentsInChildren<TMPro.TMP_Text>())
            {label.ForceMeshUpdate();Assert.That(label.isTextOverflowing,Is.False,label.name);}
            foreach(string zone in new[]{"dungeon","magical-forest","drowned-court","ironvein-excavation"})
            {
                RunLaunchOptions.ForestPrototype=false;RunLaunchOptions.StartingZone=zone;
                SceneManager.LoadScene("Game");yield return Stable();
                Assert.That(Run.Player.Definition.PlayerId,Is.EqualTo(id));Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo(zone));
                Assert.That(Run.MoveClock.Capture().profile,Is.EqualTo(CombatClockSnapshot.UnifiedProfile));int move=Run.MoveClock.Tick;
                Run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
                Assert.That(Run.Player.GetComponent<PlayerAbilityController>().TryActivate(),Is.True);yield return Stable();
                Assert.That(Run.MoveClock.Tick,Is.EqualTo(move));Assert.That(Run.Continuation.SaveNow(),Is.True);
                GameplayPixelLayoutController.ValidationSafeArea=new Rect(0,70,1080,2260);
                yield return new WaitForSeconds(.3f);yield return PlayableScreen(id+"-"+zone+"-inset");
                GameplayPixelLayoutController.ValidationSafeArea=null;
                SceneManager.LoadScene("MainMenu");yield return null;ResetPlayableProfile();
            }
        }
    }
    [UnityTest] public IEnumerator NewPlayablesInFlightContinueReplaysExactlyOnce()
    {
        foreach(string id in NewPlayableIds)
        {
            character.Dispose();character=CharacterSelectionSettings.UseTemporarySelection(id);
            yield return Launch();var controller=Run.Player.GetComponent<PlayerAbilityController>();
            Run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
            Assert.That(Run.Continuation.SaveNow(),Is.True);Assert.That(controller.TryActivate(),Is.True);
            Assert.That(Run.Continuation.SaveNow(),Is.True);string pending=File.ReadAllText(path);
            yield return Stable();var expected=Run.Continuation.Capture();
            SceneManager.LoadScene("MainMenu");yield return null;
            File.WriteAllText(path,pending);profile.Dispose();profile=AccountProgression.UseDisposableProfile(path);
            SceneManager.LoadScene("Game");yield return null;
            yield return Until(()=>Run!=null && Run.Continuation!=null && !Run.Continuation.IsRestoring,"new playable action replay");
            Assert.That(Run.Continuation.Error,Is.Null);Run.GetComponent<RunControlsUI>().Close();yield return Stable();
            var resumed=Run.Continuation.Capture();
            Assert.That(Run.Player.Definition.PlayerId,Is.EqualTo(id));
            Assert.That(JsonUtility.ToJson(resumed.board),Is.EqualTo(JsonUtility.ToJson(expected.board)),id+" board");
            Assert.That(JsonUtility.ToJson(resumed.player),Is.EqualTo(JsonUtility.ToJson(expected.player)),id+" player");
            Assert.That(resumed.enemies.Select(JsonUtility.ToJson).ToArray(),Is.EqualTo(expected.enemies.Select(JsonUtility.ToJson).ToArray()),id+" enemies");
            Assert.That(resumed.clock.actions.completed,Is.EqualTo(expected.clock.actions.completed));
            Assert.That(Run.Player.GetComponent<SequentialBoardAbilityRuntime>().IsActive,Is.False);
            Assert.That(Run.Continuation.SaveNow(),Is.True);
            SceneManager.LoadScene("MainMenu");yield return null;ResetPlayableProfile();
        }
    }
}
