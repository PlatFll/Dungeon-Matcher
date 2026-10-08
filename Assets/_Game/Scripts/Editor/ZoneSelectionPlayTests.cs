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
    private static IEnumerator CaptureZonePicker(string name)
    {
        string folder=Path.GetFullPath(".utmp/ForestValidation/ZonePicker");Directory.CreateDirectory(folder);
        yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return null;yield return null;
    }
    private static void CheckZonePickerLayout()
    {
        var picker=GameObject.Find("TestingZonePicker").GetComponent<RectTransform>();
        var corners=new Vector3[4];picker.GetWorldCorners(corners);
        foreach(var corner in corners)
        {
            var screen=RectTransformUtility.WorldToScreenPoint(null,corner);
            Assert.That(screen.x,Is.InRange(0f,(float)Screen.width));Assert.That(screen.y,Is.InRange(0f,(float)Screen.height));
        }
        foreach(var text in picker.GetComponentsInChildren<TMP_Text>())
        {
            text.ForceMeshUpdate();Assert.That(text.isTextOverflowing,Is.False,text.name);
            Assert.That(text.font,Is.SameAs(GameUi.TmpFont));
        }
    }

    [UnityTest] public IEnumerator ZonePickerCancelCreatesNoRunAndFitsPortraitScreens()
    {
        yield return OpenZoneMenu();string original=File.ReadAllText(path);
        ClickZoneMenuButton(MainPlayButton());yield return null;
        var menu=UnityEngine.Object.FindFirstObjectByType<MainMenuController>();menu.PlayGame();
        Assert.That(UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None).Count(t=>t.name=="TestingZonePicker"),Is.EqualTo(1));
        var zoneButtons=GameObject.Find("TestingZonePicker").GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("StartZone_")).ToArray();
        CollectionAssert.AreEquivalent(new[]{"StartZone_dungeon","StartZone_magical-forest","StartZone_drowned-court","StartZone_ironvein-excavation"},zoneButtons.Select(b=>b.name));
        foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,2400)})
        {
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
            yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"menu resized");
            for(int frame=0;frame<6;frame++)yield return null;
            CheckZonePickerLayout();yield return CaptureZonePicker("picker-"+size.x+"x"+size.y);
        }
        ClickZoneMenuButton(ZoneMenuButton("Back"));yield return null;
        Assert.That(GameObject.Find("TestingZonePicker"),Is.Null);
        Assert.That(MainPlayButton().gameObject.activeInHierarchy,Is.True);
        Assert.That(AccountProgression.Current.ActiveRun,Is.Null);
        Assert.That(File.ReadAllText(path),Is.EqualTo(original));
        Assert.That(RunLaunchOptions.StartingZone,Is.Null);
    }

    [UnityTest] public IEnumerator ZonePickerReturnsToCharacterSelectionOnBack()
    {
        yield return OpenZoneMenu();var menu=UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
        menu.ShowCharacterSelect();yield return null;menu.PlayGame();yield return null;
        ClickZoneMenuButton(ZoneMenuButton("Back"));yield return null;
        Assert.That(GameObject.Find("CharacterSelectScreen"),Is.Not.Null);
        Assert.That(AccountProgression.Current.ActiveRun,Is.Null);
    }

    [UnityTest] public IEnumerator ZonePickerForestStartsLiveContinuesExactlyAndRetriesForest()
    {
        yield return OpenZoneMenu();ClickZoneMenuButton(MainPlayButton());yield return null;
        ClickZoneMenuButton(ZoneMenuButton("StartZone_magical-forest"));yield return Stable();
        Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("magical-forest"));
        Assert.That(Run.Travel.State.enabled,Is.True);Assert.That(Run.Travel.LocalWave,Is.EqualTo(1));
        Assert.That(Run.Travel.State.visit,Is.Zero);Assert.That(Run.Waves.CurrentWave,Is.EqualTo(1));
        Assert.That(Run.MoveClock.Capture().profile,Is.EqualTo(CombatClockSnapshot.LegacyEffectsProfile));
        Assert.That(CombatMoveClock.MoveBasics,Is.False);Assert.That(CombatMoveClock.MoveEffects,Is.False);
        Assert.That(RunLaunchOptions.StartingZone,Is.Null);Assert.That(Run.IsPractice,Is.False);
        Assert.That(Run.Waves.ActiveEnemies.All(e=>Run.Zone.Definition.enemies.Contains(e.Definition)),Is.True);
        Assert.That(UnityEngine.Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ActiveEnvironment.EnvironmentId,Is.EqualTo("forest-woodland"));
        yield return CaptureZonePicker("forest-start");
        string id=Run.RunId;var before=Run.Continuation.Capture();
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
        RunLaunchOptions.StartingZone="dungeon"; // A stale request must never replace a Continue.
        ClickZoneMenuButton(MainPlayButton());yield return null;
        yield return Until(()=>Run?.InitialStateReady==true && !Run.Continuation.IsRestoring,"continued forest");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();
        Assert.That(GameObject.Find("TestingZonePicker"),Is.Null);Assert.That(Run.RunId,Is.EqualTo(id));
        Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("magical-forest"));
        AssertBoardCarryover(before.board,Run.Continuation.Capture().board);
        Assert.That(Run.Travel.State.enabled,Is.True);
        Assert.That(Run.ExitTo("Game"),Is.True);yield return Stable();
        Assert.That(Run.RunId,Is.Not.EqualTo(id));Assert.That(Run.Waves.CurrentWave,Is.EqualTo(1));
        Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("magical-forest"));
        Assert.That(Run.MoveClock.Capture().profile,Is.EqualTo(CombatClockSnapshot.LegacyEffectsProfile));
    }

    [UnityTest] public IEnumerator ZonePickerDungeonKeepsOriginalOpening()
    {
        yield return OpenZoneMenu();ClickZoneMenuButton(MainPlayButton());yield return null;
        ClickZoneMenuButton(ZoneMenuButton("StartZone_dungeon"));yield return Stable();
        Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("dungeon"));Assert.That(Run.MoveClock,Is.Null);
        Assert.That(Run.Travel.State.enabled,Is.True);Assert.That(Run.Travel.State.visit,Is.Zero);
        Assert.That(Run.Zone.Encounter(1,new System.Random(1)),Is.Null,"existing kingdom generator owns the opening");
        Assert.That(Run.Waves.CurrentWave,Is.EqualTo(1));Assert.That(RunLaunchOptions.StartingZone,Is.Null);
    }

    [UnityTest] public IEnumerator ZonePickerPracticeCanCancelOrStartWithoutReplacingSavedRun()
    {
        SceneManager.LoadScene("Game");yield return Stable();string id=Run.RunId;
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
        string original=File.ReadAllText(path);
        ClickZoneMenuButton(ZoneMenuButton("PracticeButton"));yield return null;
        ClickZoneMenuButton(ZoneMenuButton("Back"));yield return null;
        Assert.That(RunLaunchOptions.Practice,Is.False);Assert.That(AccountProgression.Current.ActiveRun.id,Is.EqualTo(id));
        ClickZoneMenuButton(ZoneMenuButton("PracticeButton"));yield return null;
        ClickZoneMenuButton(ZoneMenuButton("StartZone_magical-forest"));yield return Stable();
        Assert.That(Run.IsPractice,Is.True);Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("magical-forest"));
        Assert.That(File.ReadAllText(path),Is.EqualTo(original));
        Assert.That(Run.ExitTo("MainMenu"),Is.True);yield return null;yield return null;
        Assert.That(AccountProgression.Current.ActiveRun.id,Is.EqualTo(id));
        Assert.That(File.ReadAllText(path),Is.EqualTo(original));Assert.That(RunLaunchOptions.Practice,Is.False);
        ClickZoneMenuButton(MainPlayButton());yield return null;
        yield return Until(()=>Run?.InitialStateReady==true && !Run.Continuation.IsRestoring,"saved dungeon resumes");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();
        Assert.That(Run.RunId,Is.EqualTo(id));Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("dungeon"));
        Assert.That(Run.IsPractice,Is.False);
    }

    [UnityTest] public IEnumerator ZonePickerPreservesChallengeAndCancelsPendingMode()
    {
        profile.Dispose();File.WriteAllText(path,JsonUtility.ToJson(new AccountSave{firstKingClaimed=true}));
        profile=AccountProgression.UseDisposableProfile(path);
        yield return OpenZoneMenu();ClickZoneMenuButton(ZoneMenuButton("ChallengesButton"));yield return null;
        ClickZoneMenuButton(ZoneMenuButton("Challenge2"));yield return null;
        ClickZoneMenuButton(ZoneMenuButton("Back"));yield return null;
        Assert.That(GameObject.Find("Challenges"),Is.Not.Null);Assert.That(RunLaunchOptions.Challenge,Is.EqualTo(RunChallenge.Standard));
        ClickZoneMenuButton(ZoneMenuButton("Challenge2"));yield return null;
        ClickZoneMenuButton(ZoneMenuButton("StartZone_magical-forest"));yield return Stable();
        Assert.That(Run.Challenge,Is.EqualTo(RunChallenge.BoardOnly));Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("magical-forest"));
        Assert.That(Run.Player.GetComponent<PlayerAbilityController>().CanActivate,Is.False);
    }

    [UnityTest] public IEnumerator ZonePickerUnavailableRequestFallsBackAndIsConsumed()
    {
        RunLaunchOptions.StartingZone="aquatic";SceneManager.LoadScene("Game");yield return Stable();
        Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("dungeon"));Assert.That(RunLaunchOptions.StartingZone,Is.Null);
        Assert.That(Run.MoveClock,Is.Null);Assert.That(Run.Travel.State.enabled,Is.True);
    }
}
