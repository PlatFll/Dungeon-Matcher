using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// EditMode: disposable actors, catalog and minimal button views. Drives the
// existing open coroutine at yield boundaries; does NOT render the real UI,
// run real waves or replace the final combined Play Mode validation.
public sealed class UpgradeIntermissionLifecycleTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private PlayerActor player;
    private PlayerDefinition playerDefinition;
    private WaveController waves;
    private BoardController board;
    private RunUpgradeRuntime runtime;
    private RunUpgradeCatalog catalog;
    private RunUpgradeCoordinator coordinator;
    private UpgradeChoiceUI ui;
    private Canvas canvas;
    private RectTransform overlay;
    private UpgradeCardView[] cards;
    private RunUpgradeDefinition[] choices;

    [SetUp]
    public void SetUp()
    {
        Assume.That(RunUpgradeRuntime.Current, Is.Null,
            "Run outside Play Mode; never destroy a live run to make a fixture pass.");
        playerDefinition = Resources.Load<PlayerDefinition>("Players/Player_Skeleton");
        Assert.That(playerDefinition, Is.Not.Null);
        GameObject playerObject = NewObject("ChoiceFixture_Player");
        player = playerObject.AddComponent<PlayerActor>();
        player.Initialize(playerDefinition, 100);
        playerObject.SetActive(true);
        GameObject boardObject = NewObject("ChoiceFixture_Board");
        board = boardObject.AddComponent<BoardController>();
        boardObject.SetActive(true);
        choices = new RunUpgradeDefinition[3];
        for (int index = 0; index < choices.Length; index++)
        {
            choices[index] = NewAsset<RunUpgradeDefinition>();
            Set(choices[index], "upgradeId", "audit_choice_" + index);
            Set(choices[index], "maxStacks", 3);
        }
        catalog = NewAsset<RunUpgradeCatalog>();
        Set(catalog, "upgrades", choices);
        GameObject waveObject = NewObject("ChoiceFixture_Waves");
        waves = waveObject.AddComponent<WaveController>();
        Set(waves, "currentWave", 5);
        Set(waves, "encounterSeed", 71237);
        runtime = waveObject.AddComponent<RunUpgradeRuntime>();
        runtime.Configure(catalog, player, waves);
        waveObject.SetActive(true);

        // Supply only the view references used by Show/Bind/Hide. Avoid font
        // and artwork generation; real generated layout is a Play Mode check.
        GameObject uiObject = NewObject("ChoiceFixture_Canvas", typeof(RectTransform), typeof(Canvas));
        canvas = uiObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ui = uiObject.AddComponent<UpgradeChoiceUI>();
        GameObject overlayObject = new GameObject("FixtureOverlay", typeof(RectTransform));
        overlayObject.transform.SetParent(uiObject.transform, false);
        overlay = overlayObject.GetComponent<RectTransform>();
        overlayObject.SetActive(false);
        Set(ui, "rootCanvas", canvas);
        Set(ui, "overlayRect", overlay);
        cards = (UpgradeCardView[])Get(ui, "cardViews");
        for (int index = 0; index < cards.Length; index++)
        {
            GameObject cardObject = new GameObject("FixtureCard", typeof(RectTransform), typeof(Button));
            cardObject.transform.SetParent(overlay, false);
            cards[index] = cardObject.AddComponent<UpgradeCardView>();
            cards[index].Initialize(cardObject.GetComponent<Button>(), null, null, null);
        }
        uiObject.SetActive(true);
        GameObject coordinatorObject = NewObject("ChoiceFixture_Coordinator");
        coordinator = coordinatorObject.AddComponent<RunUpgradeCoordinator>();
        coordinator.Configure(runtime, waves, board, player, ui);
        coordinatorObject.SetActive(true);
    }

    [TearDown]
    public void TearDown()
    {
        for (int index = created.Count - 1; index >= 0; index--)
            if (created[index] != null) UnityEngine.Object.DestroyImmediate(created[index]);
        created.Clear();
    }

    [Test]
    public void OneSelectionAppliesOneCardAndCloses()
    {
        OpenChoice();
        Func<RunUpgradeDefinition, bool> callback = Callback();
        RunUpgradeDefinition chosen = CardDefinition(0);
        Click(0);
        Assert.That(runtime.GetStackCount(chosen), Is.EqualTo(1));
        Assert.That(callback(chosen), Is.False);
        AssertClosed();
    }

    [Test]
    public void SynchronousUpgradeCallbackCannotSelectASecondCard()
    {
        OpenChoice();
        Func<RunUpgradeDefinition, bool> callback = Callback();
        RunUpgradeDefinition first = CardDefinition(0), second = CardDefinition(1);
        bool attempted = false, nestedAccepted = true;
        runtime.UpgradeChanged += (_, __) =>
        {
            if (attempted) return;
            attempted = true;
            nestedAccepted = callback(second);
        };
        Click(0);
        Assert.That(attempted, Is.True);
        Assert.That(nestedAccepted, Is.False);
        Assert.That(runtime.GetStackCount(first), Is.EqualTo(1));
        Assert.That(runtime.GetStackCount(second), Is.Zero);
        AssertClosed();
    }

    [Test]
    public void EligibleButUnofferedCardIsRejected()
    {
        OpenChoice();
        RunUpgradeDefinition outsideOffer = NewAsset<RunUpgradeDefinition>();
        Set(outsideOffer, "upgradeId", "audit_not_offered");
        Assert.That(runtime.IsEligible(outsideOffer, player, 5), Is.True);
        Assert.That(Callback()(outsideOffer), Is.False);
        Assert.That(runtime.GetStackCount(outsideOffer), Is.Zero);
        Assert.That(coordinator.IsBlockingWaveProgression && ui.IsOpen, Is.True);
    }

    [Test]
    public void NewlyIneligibleChoiceDoesNotConsumeSelection()
    {
        OpenChoice();
        RunUpgradeDefinition first = CardDefinition(0), second = CardDefinition(1);
        for (int index = 0; index < first.MaxStacks; index++)
            Assert.That(runtime.TryApply(first, 5), Is.True);
        LogAssert.Expect(LogType.Warning,
            $"Run upgrade '{first.UpgradeId}' was no longer legal when selected.");
        Click(0);
        Assert.That(ui.IsOpen && coordinator.IsBlockingWaveProgression, Is.True);
        Assert.That(cards[1].GetComponent<Button>().interactable, Is.True);
        Click(1);
        Assert.That(runtime.GetStackCount(second), Is.EqualTo(1));
        AssertClosed();
    }

    [Test]
    public void RepeatConfigurationPreservesChoiceCardsAndDraftStream()
    {
        Assert.That(runtime.TryApply(choices[0], 5), Is.True);
        OpenChoice();
        object session = Get(coordinator, "activeChoiceSession");
        object uiSession = Get(ui, "choiceSession");
        System.Random random = runtime.GetDraftRandom();
        for (int index = 0; index < 5; index++)
        {
            runtime.Configure(catalog, player, waves);
            ui.Configure(canvas);
            coordinator.Configure(runtime, waves, board, player, ui);
        }
        Assert.That(Get(coordinator, "activeChoiceSession"), Is.SameAs(session));
        Assert.That(Get(ui, "choiceSession"), Is.SameAs(uiSession));
        Assert.That(runtime.GetDraftRandom(), Is.SameAs(random));
        Assert.That(runtime.GetStackCount(choices[0]), Is.EqualTo(1));
        Assert.That(ui.IsOpen && coordinator.IsBlockingWaveProgression && board.IsExternalInputBlocked, Is.True);
    }

    [Test]
    public void ChangedBindingReleasesOldBoardAndRevokesOldCallback()
    {
        OpenChoice();
        Func<RunUpgradeDefinition, bool> old = Callback();
        BoardController oldBoard = board;
        using (IDisposable otherOwner = oldBoard.AcquireExternalInputBlock())
        {
            GameObject otherBoardObject = NewObject("ChoiceFixture_ReplacementBoard");
            board = otherBoardObject.AddComponent<BoardController>();
            otherBoardObject.SetActive(true);
            coordinator.Configure(runtime, waves, board, player, ui);
            Assert.That(oldBoard.IsExternalInputBlocked, Is.True);
            AssertClosed();
            OpenChoice();
            Assert.That(old(choices[0]), Is.False);
            Assert.That(runtime.GetOwnedDefinitions().Count, Is.Zero);
            Assert.That(board.IsExternalInputBlocked && ui.IsOpen, Is.True);
        }
        Assert.That(oldBoard.IsExternalInputBlocked, Is.False);
    }

    [Test]
    public void ExplicitRunResetRevokesCallbacksEvenAfterAnotherChoiceOpens()
    {
        OpenChoice();
        Func<RunUpgradeDefinition, bool> old = Callback();
        runtime.ResetRun();
        AssertClosed();
        OpenChoice(10);
        Assert.That(old(choices[0]), Is.False);
        Assert.That(runtime.GetOwnedDefinitions().Count, Is.Zero);
        Assert.That(ui.IsOpen && coordinator.IsBlockingWaveProgression, Is.True);
    }

    [Test]
    public void PlayerReinitializationClosesOldChoice()
    {
        OpenChoice();
        Func<RunUpgradeDefinition, bool> old = Callback();
        player.Initialize(playerDefinition, 150);
        AssertClosed();
        Assert.That(old(choices[0]), Is.False);
        Assert.That(player.MaximumHealth, Is.EqualTo(150));
    }

    [Test]
    public void DefeatClosesChoiceWithoutApplyingCard()
    {
        OpenChoice();
        Func<RunUpgradeDefinition, bool> old = Callback();
        Assert.That(player.TryTakeDamage(10000), Is.True);
        Assert.That(player.IsDefeated, Is.True);
        AssertClosed();
        Assert.That(old(choices[0]), Is.False);
        Assert.That(runtime.GetOwnedDefinitions().Count, Is.Zero);
    }

    [Test]
    public void HideNotifiesOnceAndReleasesOnlyItsOwnInputToken()
    {
        using (IDisposable otherOwner = board.AcquireExternalInputBlock())
        {
            OpenChoice();
            int notifications = 0;
            ui.Hidden += () => { notifications++; ui.Hide(); };
            ui.Hide();
            ui.Hide();
            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(board.IsExternalInputBlocked, Is.True);
            Assert.That(coordinator.IsBlockingWaveProgression || ui.IsOpen, Is.False);
            Assert.That(runtime.GetOwnedDefinitions().Count, Is.Zero);
        }
        Assert.That(board.IsExternalInputBlocked, Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void DisablingViewOrItsRootReleasesChoice(bool disableRoot)
    {
        OpenChoice();
        Func<RunUpgradeDefinition, bool> old = Callback();
        if (disableRoot) ui.gameObject.SetActive(false); else ui.enabled = false;
        AssertClosed();
        Assert.That(old(choices[0]), Is.False);
        if (disableRoot) ui.gameObject.SetActive(true); else ui.enabled = true;
        AssertClosed();
    }

    [Test]
    public void ExternalOverlayDeactivationIsObservedWithoutGrantingReward()
    {
        OpenChoice();
        overlay.gameObject.SetActive(false);
        Call(ui, "Update");
        AssertClosed();
        Assert.That(runtime.GetOwnedDefinitions().Count, Is.Zero);
    }

    [Test]
    public void DestroyingViewReleasesCoordinator()
    {
        OpenChoice();
        Func<RunUpgradeDefinition, bool> old = Callback();
        UnityEngine.Object.DestroyImmediate(ui);
        Assert.That(coordinator.IsBlockingWaveProgression || board.IsExternalInputBlocked, Is.False);
        Assert.That(overlay.gameObject.activeSelf, Is.False);
        Assert.That(old(choices[0]), Is.False);
    }

    [Test]
    public void DisabledCoordinatorRejectsSelectionAndDoesNotResubscribeOnConfigure()
    {
        OpenChoice();
        Func<RunUpgradeDefinition, bool> old = Callback();
        coordinator.enabled = false;
        coordinator.Configure(runtime, waves, board, player, ui);
        Assert.That(old(choices[0]), Is.False);
        Assert.That(SubscriptionCount(runtime, "RunReset"), Is.Zero);
        Assert.That(SubscriptionCount(ui, "Hidden"), Is.Zero);
        Assert.That(((HashSet<IWaveProgressionGate>)Get(waves, "progressionGates")).Contains(coordinator), Is.False);
        AssertClosed();
    }

    [Test]
    public void FiveEnableCyclesKeepExactlyOneObserverAndGate()
    {
        for (int index = 0; index < 5; index++)
        {
            coordinator.enabled = false;
            coordinator.enabled = true;
            coordinator.Configure(runtime, waves, board, player, ui);
        }
        Assert.That(SubscriptionCount(runtime, "RunReset"), Is.EqualTo(1));
        Assert.That(SubscriptionCount(ui, "Hidden"), Is.EqualTo(1));
        Assert.That(SubscriptionCount(player, "Initialized"), Is.EqualTo(1));
        Assert.That(SubscriptionCount(waves, "WaveCompleted"), Is.EqualTo(1));
        Assert.That(((HashSet<IWaveProgressionGate>)Get(waves, "progressionGates")).Count, Is.EqualTo(1));
        OpenChoice();
        Click(0);
        Assert.That(runtime.GetOwnedDefinitions().Count, Is.EqualTo(1));
    }

    [Test]
    public void ResetWhileWaitingForBoardPreventsLateOverlayOpening()
    {
        Set(board, "isBusy", true);
        IEnumerator opening = BeginChoice(5);
        Assert.That(opening.MoveNext(), Is.True);
        Assert.That(ui.IsOpen, Is.False);
        runtime.ResetRun();
        Set(board, "isBusy", false);
        Assert.That(opening.MoveNext(), Is.False);
        (opening as IDisposable)?.Dispose();
        AssertClosed();
    }

    [Test]
    public void OldApplyReturningAfterResetCannotCloseNewIntermission()
    {
        OpenChoice();
        bool once = false;
        runtime.UpgradeChanged += (_, __) =>
        {
            if (once) return;
            once = true;
            runtime.ResetRun();
            OpenChoice(10);
        };
        Click(0);
        Assert.That(once, Is.True);
        Assert.That(runtime.GetOwnedDefinitions().Count, Is.Zero);
        Assert.That(ui.IsOpen && coordinator.IsBlockingWaveProgression && board.IsExternalInputBlocked, Is.True);
        Click(0);
        Assert.That(runtime.GetOwnedDefinitions().Count, Is.EqualTo(1));
        AssertClosed();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ReplacementViewSurvivesOldHandlerReturning(bool accepted)
    {
        int replacementCalls = 0;
        Assert.That(ui.Show(choices, _ =>
        {
            Assert.That(ui.Show(choices, __ => { replacementCalls++; return true; }), Is.True);
            return accepted;
        }), Is.True);
        Click(0);
        Assert.That(ui.IsOpen, Is.True);
        Assert.That(cards[0].GetComponent<Button>().interactable, Is.True);
        Click(0);
        Assert.That(replacementCalls, Is.EqualTo(1));
        Assert.That(ui.IsOpen, Is.False);
    }

    [Test]
    public void RetainedOldCardCallbackCannotSelectNewOffer()
    {
        int calls = 0;
        Assert.That(ui.Show(choices, _ => false), Is.True);
        Action<RunUpgradeDefinition> old = (Action<RunUpgradeDefinition>)Get(cards[0], "selected");
        Assert.That(ui.Show(choices, _ => { calls++; return true; }), Is.True);
        old(choices[0]);
        Assert.That(calls, Is.Zero);
        Assert.That(ui.IsOpen, Is.True);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void InvalidShowNeverLeavesAnEmptyInputBlocker(int invalidKind)
    {
        OpenChoice();
        IReadOnlyList<RunUpgradeDefinition> invalid = choices;
        Func<RunUpgradeDefinition, bool> handler = _ => true;
        if (invalidKind == 0) invalid = null;
        if (invalidKind == 1) invalid = Array.Empty<RunUpgradeDefinition>();
        if (invalidKind == 2) invalid = new RunUpgradeDefinition[] { null };
        if (invalidKind == 3) handler = null;
        Assert.That(ui.Show(invalid, handler), Is.False);
        AssertClosed();
        Assert.That(overlay.gameObject.activeSelf, Is.False);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void OneTwoOrThreeValidChoicesRemainSelectable(int count)
    {
        RunUpgradeDefinition[] displayed = new RunUpgradeDefinition[count];
        Array.Copy(choices, displayed, count);
        int accepted = 0;
        Assert.That(ui.Show(displayed, _ => { accepted++; return true; }), Is.True);
        for (int index = 0; index < cards.Length; index++)
            Assert.That(cards[index].gameObject.activeSelf, Is.EqualTo(index < count));
        Click(count - 1);
        Assert.That(accepted, Is.EqualTo(1));
        Assert.That(ui.IsOpen, Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ChangedWaveOrActiveEncounterRejectsOldSelection(bool activeEncounter)
    {
        OpenChoice();
        if (activeEncounter) Set(waves, "<IsWaveActive>k__BackingField", true);
        else Set(waves, "currentWave", 6);
        Assert.That(Callback()(choices[0]), Is.False);
        Assert.That(runtime.GetOwnedDefinitions().Count, Is.Zero);
    }

    [Test]
    public void RejectedUiHandlerCanBeRetried()
    {
        int attempts = 0;
        Assert.That(ui.Show(choices, _ => ++attempts > 1), Is.True);
        Click(0);
        Assert.That(ui.IsOpen && cards[0].GetComponent<Button>().interactable, Is.True);
        Click(0);
        Assert.That(attempts, Is.EqualTo(2));
        Assert.That(ui.IsOpen, Is.False);
    }

    private IEnumerator BeginChoice(int wave)
    {
        // Model the state installed by HandleWaveCompleted, without starting
        // Unity coroutines from EditMode. All subsequent open/draft/UI/selection
        // work calls the production implementation.
        Set(waves, "currentWave", wave);
        Set(coordinator, "heldCompletedWave", wave);
        Set(coordinator, "selectionCommitted", false);
        Set(coordinator, "isHoldingProgression", true);
        Set(coordinator, "activeChoiceSession", new object());
        return (IEnumerator)Call(coordinator, "OpenChoiceAfterBoardSettles", wave);
    }

    private void OpenChoice(int wave = 5)
    {
        Assert.That(coordinator.IsBlockingWaveProgression, Is.False, "previous choice must finish first");
        IEnumerator opening = BeginChoice(wave);
        Assert.That(opening.MoveNext(), Is.False, "idle fixture opens synchronously");
        (opening as IDisposable)?.Dispose();
        Assert.That(ui.IsOpen && coordinator.IsBlockingWaveProgression && board.IsExternalInputBlocked, Is.True);
    }

    private void AssertClosed()
    {
        Assert.That(ui.IsOpen, Is.False);
        Assert.That(coordinator.IsBlockingWaveProgression, Is.False);
        Assert.That(board.IsExternalInputBlocked, Is.False);
    }

    private Func<RunUpgradeDefinition, bool> Callback() =>
        (Func<RunUpgradeDefinition, bool>)Get(ui, "trySelect");
    private RunUpgradeDefinition CardDefinition(int index) =>
        (RunUpgradeDefinition)Get(cards[index], "definition");
    private void Click(int index) => cards[index].GetComponent<Button>().onClick.Invoke();
    private int SubscriptionCount(object source, string name)
    {
        Delegate callbacks = (Delegate)Get(source, name);
        int count = 0;
        if (callbacks != null)
            foreach (Delegate callback in callbacks.GetInvocationList())
                if (ReferenceEquals(callback.Target, coordinator)) count++;
        return count;
    }
    private GameObject NewObject(string name, params Type[] components)
    {
        GameObject result = new GameObject(name, components);
        result.SetActive(false);
        created.Add(result);
        return result;
    }
    private T NewAsset<T>() where T : ScriptableObject
    {
        T result = ScriptableObject.CreateInstance<T>();
        created.Add(result);
        return result;
    }
    private static object Get(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, Flags);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        return field.GetValue(target);
    }
    private static void Set(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(name, Flags);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        field.SetValue(target, value);
    }
    private static object Call(object target, string name, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(name, Flags);
        if (method == null) throw new MissingMethodException(target.GetType().Name, name);
        return method.Invoke(target, args);
    }
}
