using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator OpenZoneMenu()
    {
        RunLaunchOptions.StartingZone=null;
        RunLaunchOptions.Practice=false;
        RunLaunchOptions.Challenge=RunChallenge.Standard;
        SceneManager.LoadScene("MainMenu");yield return null;yield return null;
    }
    private static void ClickZoneMenuButton(Button button)
    {
        Assert.That(button,Is.Not.Null);Assert.That(button.IsInteractable(),Is.True);
        Canvas.ForceUpdateCanvases();
        var pointer=new PointerEventData(EventSystem.current)
        {position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Assert.That(hits.Count,Is.GreaterThan(0));
        Assert.That(hits[0].gameObject.GetComponentInParent<Button>(),Is.SameAs(button),"button receives actual UI input");
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
    }
    private static Button ZoneMenuButton(string name)=>GameObject.Find(name)?.GetComponent<Button>();
    private static Button MainPlayButton()=> (Button)Get(UnityEngine.Object.FindFirstObjectByType<MainMenuController>(),"playButton");
    [UnityTest] public IEnumerator DungeonFirstMenuStartsImmediatelyWithoutPicker()
    {
        yield return OpenZoneMenu();
        // Ordinary navigation discards a stale diagnostic request, even in Editor.
        RunLaunchOptions.StartingZone="drowned-court";
        RunLaunchOptions.ForestPrototype=true;
        ClickZoneMenuButton(MainPlayButton());yield return Stable();
        Assert.That(RunLaunchOptions.TestingZonePickerEnabled,Is.False);
        Assert.That(GameObject.Find("TestingZonePicker"),Is.Null);
        Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("dungeon"));
        Assert.That(Run.Waves.CurrentWave,Is.EqualTo(1));
        Assert.That(Run.Travel.State.enabled,Is.True);
        Assert.That(Run.MoveClock.Capture().profile,Is.EqualTo(CombatClockSnapshot.UnifiedProfile));
        Assert.That(RunLaunchOptions.StartingZone,Is.Null);
    }

    [UnityTest] public IEnumerator DungeonFirstEditorZonesContinueExactlyAndRetryDungeon()
    {
        foreach(string zone in new[]{"magical-forest","drowned-court","ironvein-excavation","frostwake-peaks"})
        {
            RunLaunchOptions.StartingZone=zone;SceneManager.LoadScene("Game");yield return Stable();
            Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo(zone),"explicit Editor-only launch");
            Assert.That(Run.Travel.State.enabled,Is.True);
            Assert.That(RunLaunchOptions.StartingZone,Is.Null);
            string id=Run.RunId;var before=Run.Continuation.Capture();
            Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
            // Bypass menu clearing to prove the checkpoint itself wins over a developer request.
            RunLaunchOptions.StartingZone="dungeon";SceneManager.LoadScene("Game");
            yield return Until(()=>Run?.InitialStateReady==true&&!Run.Continuation.IsRestoring,"saved zone resumes");
            Run.GetComponent<RunControlsUI>().Close();yield return Stable();
            Assert.That(Run.RunId,Is.EqualTo(id));Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo(zone));
            var after=Run.Continuation.Capture();AssertBoardCarryover(before.board,after.board);
            Assert.That(after.player.health,Is.EqualTo(before.player.health));
            Assert.That(after.player.shield,Is.EqualTo(before.player.shield));
            var comparable=after.resonance.Copy();
            // Continue opens paused, then this fixture resumes to a stable board.
            // Observe that elapsed clocks advance normally; every non-clock field is exact.
            foreach(var field in typeof(CrystalResonanceSnapshot).GetFields().Where(f=>f.FieldType==typeof(float)))
            {
                float old=(float)field.GetValue(before.resonance),current=(float)field.GetValue(comparable);
                Assert.That(current,Is.InRange(old,old+5),field.Name);
                field.SetValue(comparable,old);
            }
            Assert.That(JsonUtility.ToJson(comparable),Is.EqualTo(JsonUtility.ToJson(before.resonance)));
            RunLaunchOptions.StartingZone=zone;
            Assert.That(Run.ExitTo("Game"),Is.True);yield return Stable();
            Assert.That(Run.RunId,Is.Not.EqualTo(id));Assert.That(Run.Waves.CurrentWave,Is.EqualTo(1));
            Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("dungeon"));
            Assert.That(RunLaunchOptions.StartingZone,Is.Null);
            Assert.That(Run.ExitTo("MainMenu"),Is.True);yield return null;yield return null;
        }
    }

    [UnityTest] public IEnumerator DungeonFirstPracticePreservesSavedOtherZoneAndRetriesDungeon()
    {
        RunLaunchOptions.StartingZone="drowned-court";SceneManager.LoadScene("Game");yield return Stable();
        string id=Run.RunId;Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
        string original=File.ReadAllText(path);
        ClickZoneMenuButton(ZoneMenuButton("PracticeButton"));yield return Stable();
        Assert.That(Run.IsPractice,Is.True);Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("dungeon"));
        Assert.That(File.ReadAllText(path),Is.EqualTo(original));
        Assert.That(Run.ExitTo("Game"),Is.True);yield return Stable();
        Assert.That(Run.IsPractice,Is.True);Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("dungeon"));
        Assert.That(Run.ExitTo("MainMenu"),Is.True);yield return null;yield return null;
        Assert.That(AccountProgression.Current.ActiveRun.id,Is.EqualTo(id));
        Assert.That(File.ReadAllText(path),Is.EqualTo(original));
        ClickZoneMenuButton(MainPlayButton());yield return null;
        yield return Until(()=>Run?.InitialStateReady==true&&!Run.Continuation.IsRestoring,"Continue through menu");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();
        Assert.That(Run.RunId,Is.EqualTo(id));Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("drowned-court"));
        Assert.That(Run.IsPractice,Is.False);
    }

    [UnityTest] public IEnumerator DungeonFirstChallengesKeepRulesAcrossRetry()
    {
        profile.Dispose();File.WriteAllText(path,JsonUtility.ToJson(new AccountSave{firstKingClaimed=true}));
        profile=AccountProgression.UseDisposableProfile(path);
        foreach(int mode in new[]{1,2})
        {
            yield return OpenZoneMenu();ClickZoneMenuButton(ZoneMenuButton("ChallengesButton"));yield return null;
            ClickZoneMenuButton(ZoneMenuButton("Challenge"+mode));yield return Stable();
            Assert.That(Run.Challenge,Is.EqualTo((RunChallenge)mode));
            Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("dungeon"));
            if(mode==2)Assert.That(Run.Player.GetComponent<PlayerAbilityController>().CanActivate,Is.False);
            Assert.That(Run.ExitTo("Game"),Is.True);yield return Stable();
            Assert.That(Run.Challenge,Is.EqualTo((RunChallenge)mode));
            Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("dungeon"));
            Assert.That(Run.ExitTo("MainMenu"),Is.True);yield return null;yield return null;
        }
    }

    [UnityTest] public IEnumerator DungeonFirstInvalidDeveloperZoneIsConsumed()
    {
        RunLaunchOptions.StartingZone="aquatic";SceneManager.LoadScene("Game");yield return Stable();
        Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("dungeon"));
        Assert.That(RunLaunchOptions.StartingZone,Is.Null);Assert.That(Run.MoveClock,Is.Not.Null);
        Assert.That(Run.Travel.State.enabled,Is.True);
    }
}
