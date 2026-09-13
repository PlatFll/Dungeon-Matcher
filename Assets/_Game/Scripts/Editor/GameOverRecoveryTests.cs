using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Synchronous EditMode coverage of ownership/cleanup. Death presentation state
/// is seeded to avoid running coroutines; actual revival/init APIs deliver the
/// recovery events. Timed VFX, stopped coroutine resumption and scene Retry
/// require the final combined Play Mode pass.
/// </summary>
[NonParallelizable]
public sealed class GameOverRecoveryTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    private readonly List<GameObject> objects = new List<GameObject>();
    private GameObject root;
    private Canvas canvas;
    private PlayerActor player;
    private PlayerDefinition definition;
    private GameOverPresentationController controller;
    private Image image;
    private PlayerCombatFeedback feedback;
    private float savedTimeScale;
    private UnityEngine.Random.State savedRandom;

    [SetUp]
    public void SetUp()
    {
        savedTimeScale = Time.timeScale;
        savedRandom = UnityEngine.Random.state;
        if (Application.isPlaying ||
            UnityEngine.Object.FindObjectsByType<PlayerActor>(FindObjectsInactive.Include,
                FindObjectsSortMode.None).Length > 0 ||
            UnityEngine.Object.FindObjectsByType<GameOverPresentationController>(FindObjectsInactive.Include,
                FindObjectsSortMode.None).Length > 0)
            Assert.Ignore("Run GameOverRecoveryTests in an isolated EditMode scene; do not remove live gameplay objects.");

        definition = Resources.Load<PlayerDefinition>("Players/Player_Bardley");
        Assert.That(definition, Is.Not.Null);
        Time.timeScale = 1f;
        canvas = NewObject("RecoveryTestCanvas", true, typeof(RectTransform), typeof(Canvas)).GetComponent<Canvas>();
        root = NewObject("RecoveryTestPlayer");
        player = root.AddComponent<PlayerActor>();
        player.Initialize(definition, 100);
        controller = root.AddComponent<GameOverPresentationController>();
        Set(controller, "playerActor", player);
        Set(controller, "rootCanvas", canvas);
        Call(controller, "Subscribe");
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (controller != null) Call(controller, "CleanupGameOver");
            for (int i = objects.Count - 1; i >= 0; i--)
                if (objects[i] != null) UnityEngine.Object.DestroyImmediate(objects[i]);
            objects.Clear();
        }
        finally
        {
            Time.timeScale = savedTimeScale;
            UnityEngine.Random.state = savedRandom;
        }
    }

    [TestCase(0f)]
    [TestCase(0.5f)]
    [TestCase(1f)]
    public void CleanupRestoresExactlyTheCapturedTimeScale(float scale)
    {
        Time.timeScale = scale;
        Call(controller, "FreezeGameplay");
        Call(controller, "FreezeGameplay");
        Assert.That(Time.timeScale, Is.Zero);
        Call(controller, "CleanupGameOver");
        Assert.That(Time.timeScale, Is.EqualTo(scale));
        Assert.That(Get<bool>(controller, "timeFrozen"), Is.False);
    }

    [Test]
    public void CleanupDoesNotOverwriteANewerNonzeroTimeScale()
    {
        Call(controller, "FreezeGameplay");
        Time.timeScale = 0.75f;
        Call(controller, "CleanupGameOver");
        Assert.That(Time.timeScale, Is.EqualTo(0.75f));
    }

    [Test]
    public void RepeatedCleanupDoesNotOwnTheClockAgain()
    {
        SeedDeath();
        Call(controller, "CleanupGameOver");
        Time.timeScale = 0.25f;
        Call(controller, "CleanupGameOver");
        Call(controller, "OnDestroy");
        Assert.That(Time.timeScale, Is.EqualTo(0.25f));
    }

    [Test]
    public void RealReviveEventClearsOverlayWithoutChangingRestoredHealth()
    {
        GameObject overlay = SeedDeath();
        Assert.That(player.TryRevive(37), Is.True);
        AssertClean();
        Assert.That(overlay == null, Is.True);
        Assert.That(player.CurrentHealth, Is.EqualTo(37));
        Assert.That(player.MaximumHealth, Is.EqualTo(100));
        Assert.That(player.CurrentShield, Is.Zero);
        Assert.That(player.RevivalCount, Is.EqualTo(1));
    }

    [Test]
    public void PlayerReinitializationKeepsTheNewHealthOverride()
    {
        SeedDeath();
        player.Initialize(definition, 173);
        AssertClean();
        Assert.That(player.CurrentHealth, Is.EqualTo(173));
        Assert.That(player.MaximumHealth, Is.EqualTo(173));
        Assert.That(player.RevivalCount, Is.Zero);
    }

    [Test]
    public void RejectedReviveDoesNotRemoveGameOver()
    {
        GameObject overlay = SeedDeath();
        Assert.That(player.TryRevive(0), Is.False);
        Assert.That(player.IsDefeated, Is.True);
        Assert.That(Get<bool>(controller, "sequenceStarted"), Is.True);
        Assert.That(overlay != null, Is.True);
        Assert.That(Time.timeScale, Is.Zero);
    }

    [TestCase("OnDisable")]
    [TestCase("OnDestroy")]
    public void LifecycleCleanupReleasesOnlyItsPresentation(string message)
    {
        GameObject unrelated = NewObject("UnrelatedModal", true, typeof(RectTransform));
        unrelated.transform.SetParent(canvas.transform, false);
        GameObject overlay = SeedDeath();
        Call(controller, message);
        AssertClean();
        Assert.That(overlay == null, Is.True);
        Assert.That(unrelated != null && unrelated.activeSelf, Is.True);
        Assert.That(player.IsDefeated, Is.True, "cleanup is not a revive");
    }

    [Test]
    public void DestroyingTheComponentDoesNotOrphanItsCanvasOverlay()
    {
        GameObject overlay = SeedDeath();
        UnityEngine.Object.DestroyImmediate(controller);
        Assert.That(overlay == null, Is.True);
        Assert.That(canvas != null && root != null, Is.True);
        Assert.That(Time.timeScale, Is.EqualTo(1f));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RecoveryRestoresTheImageEnabledStateItChanged(bool originallyEnabled)
    {
        CreateFeedback();
        image.enabled = originallyEnabled;
        SeedDeath();
        Assert.That(Call(controller, "BeginPlayerDeathVisual"), Is.SameAs(image));
        Assert.That(image.enabled, Is.True);
        image.enabled = false; // The sequence's later burst stage.
        Assert.That(player.TryRevive(40), Is.True);
        Assert.That(image.enabled, Is.EqualTo(originallyEnabled));
        Assert.That(Get<Image>(controller, "defeatedPlayerImage"), Is.Null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RecoveryRestoresAffinityVisibilityWithoutRecapturingHiddenState(bool originallyActive)
    {
        GameObject panelObject = NewObject("RecoveryPanel", false, typeof(RectTransform));
        PlayerPanelUI panel = panelObject.AddComponent<PlayerPanelUI>();
        Set(controller, "playerPanel", panel);
        GameObject affinity = NewObject("PlayerAffinityGem", originallyActive, typeof(RectTransform));
        affinity.transform.SetParent(panelObject.transform, false);
        SeedDeath();
        Call(controller, "HidePlayerAffinityGem");
        Call(controller, "HidePlayerAffinityGem");
        Assert.That(affinity.activeSelf, Is.False);
        player.TryRevive(40);
        Assert.That(affinity.activeSelf, Is.EqualTo(originallyActive));
    }

    [Test]
    public void DeathVisualExitDoesNotSnapBackToAnOldLayoutPosition()
    {
        CreateFeedback();
        RectTransform visual = image.rectTransform;
        Set(feedback, "visualRoot", visual);
        Set(feedback, "restingPosition", new Vector2(-20f, -40f));
        SeedDeath();
        Call(controller, "BeginPlayerDeathVisual");
        Vector2 latestPosition = new Vector2(31f, 79f);
        visual.anchoredPosition = latestPosition;
        player.TryRevive(40);
        Assert.That(visual.anchoredPosition, Is.EqualTo(latestPosition));
    }

    [Test]
    public void DestroyedVisualReferencesCannotStrandTheTimeFreeze()
    {
        CreateFeedback();
        SeedDeath();
        Call(controller, "BeginPlayerDeathVisual");
        UnityEngine.Object.DestroyImmediate(image.gameObject);
        UnityEngine.Object.DestroyImmediate(feedback.gameObject);
        Assert.DoesNotThrow(() => player.TryRevive(40));
        AssertClean();
    }

    [Test]
    public void AStaleDefeatNotificationCannotStartGameOverForALivingPlayer()
    {
        Assert.That(player.IsDefeated, Is.False);
        Call(controller, "HandlePlayerDefeated", player);
        AssertClean();
    }

    [Test]
    public void RecoveryOfAnotherPlayerDoesNotClearThisDeath()
    {
        SeedDeath();
        PlayerActor other = NewObject("OtherRecoveryPlayer").AddComponent<PlayerActor>();
        other.Initialize(definition, 90);
        Call(controller, "HandlePlayerRevived", other, 1);
        Call(controller, "HandlePlayerInitialized", other);
        Assert.That(Get<bool>(controller, "sequenceStarted"), Is.True);
        Assert.That(Time.timeScale, Is.Zero);
    }

    [Test]
    public void FiveEnableDisableMessagesAndRepeatedSubscribeKeepOneHandler()
    {
        for (int i = 0; i < 5; i++)
        {
            Call(controller, "OnDisable");
            AssertHandlerCount(0);
            Call(controller, "OnEnable");
            Call(controller, "Subscribe");
            AssertHandlerCount(1);
        }
        SeedDeath();
        player.TryRevive(31);
        AssertClean();
    }

    [Test]
    public void DisabledComponentCannotResubscribeThroughRefresh()
    {
        controller.enabled = false;
        Call(controller, "OnDisable");
        Call(controller, "Subscribe");
        AssertHandlerCount(0);
    }

    [Test]
    public void ReenableReconciliationDoesNotReopenAMissedRevival()
    {
        SeedDeath();
        Call(controller, "OnDisable");
        Assert.That(player.TryRevive(28), Is.True);
        Call(controller, "OnEnable");
        Call(controller, "SynchronizePlayerState");
        AssertClean();
        Assert.That(player.CurrentHealth, Is.EqualTo(28));
    }

    [Test]
    public void CleanupDoesNotReleaseBoardOwnershipOrOtherInputTokens()
    {
        BoardController board = NewObject("RecoveryBoard", false).AddComponent<BoardController>();
        Set(board, "isBusy", true);
        PlayerAbilityEnergy energy = root.AddComponent<PlayerAbilityEnergy>();
        energy.AddEnergy(17);
        int stored = energy.CurrentEnergy;
        using (board.AcquireExternalInputBlock())
        {
            SeedDeath();
            player.TryRevive(40);
            Assert.That(board.IsBusy, Is.True);
            Assert.That(board.IsExternalInputBlocked, Is.True);
            Assert.That(energy.CurrentEnergy, Is.EqualTo(stored));
        }
        Assert.That(board.IsExternalInputBlocked, Is.False);
        Assert.That(board.IsBusy, Is.True);
    }

    [Test]
    public void RetryGuardRequiresADeadReadyUncommittedSession()
    {
        SeedDeath();
        Button button = Get<Button>(controller, "retryButton");
        button.interactable = false;
        Assert.That((bool)Call(controller, "IsRetryAvailable"), Is.False);
        button.interactable = true;
        Assert.That((bool)Call(controller, "IsRetryAvailable"), Is.True);
        Set(controller, "retryRequested", true);
        Assert.That((bool)Call(controller, "IsRetryAvailable"), Is.False);
        Set(controller, "retryRequested", false);
        Set(controller, "sequenceStarted", false);
        Assert.That((bool)Call(controller, "IsRetryAvailable"), Is.False);
    }

    [Test]
    public void StaleRetryAfterRecoveryCannotRequestASceneLoad()
    {
        SeedDeath();
        player.TryRevive(40);
        Assert.That((bool)Call(controller, "IsRetryAvailable"), Is.False);
        Call(controller, "RetryCurrentGame"); // Guard exits before any scene API.
        AssertClean();
    }

    [TestCase(0f, 0f)]
    [TestCase(0.5f, 1.0876975f)]
    [TestCase(1f, 1f)]
    public void ExistingMenuEasingIsUnchanged(float progress, float expected)
    {
        MethodInfo method = typeof(GameOverPresentationController).GetMethod("EaseOutBack",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        float actual = (float)method.Invoke(null, new object[] { progress });
        Assert.That(actual, Is.EqualTo(expected).Within(0.00001f));
    }

    private GameObject SeedDeath()
    {
        // Suppress the timed entry while exercising the real damage/defeat API.
        Set(controller, "sequenceStarted", true);
        if (!player.IsDefeated) Assert.That(player.TryTakeDamage(player.CurrentHealth), Is.True);
        Call(controller, "FreezeGameplay");
        GameObject overlay = NewObject("RecoveryOwnedOverlay", true, typeof(RectTransform));
        overlay.transform.SetParent(canvas.transform, false);
        Set(controller, "overlayRect", overlay.GetComponent<RectTransform>());
        GameObject buttonObject = NewObject("RecoveryRetry", true, typeof(RectTransform), typeof(Button));
        buttonObject.transform.SetParent(overlay.transform, false);
        Set(controller, "retryButton", buttonObject.GetComponent<Button>());
        return overlay;
    }

    private void CreateFeedback()
    {
        // Inactive optional presentation fixture: no Awake shader setup, no hit
        // coroutine subscription, no changes to source materials or animations.
        GameObject visual = NewObject("RecoveryVisual", false, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        image = visual.GetComponent<Image>();
        GameObject owner = NewObject("RecoveryFeedback", false);
        feedback = owner.AddComponent<PlayerCombatFeedback>();
        Set(feedback, "playerImage", image);
        Set(controller, "combatFeedback", feedback);
    }

    private void AssertClean()
    {
        Assert.That(Get<bool>(controller, "sequenceStarted"), Is.False);
        Assert.That(Get<bool>(controller, "retryRequested"), Is.False);
        Assert.That(Get<bool>(controller, "timeFrozen"), Is.False);
        Assert.That(Get<RectTransform>(controller, "overlayRect"), Is.Null);
        Assert.That(Get<Button>(controller, "retryButton"), Is.Null);
        Assert.That(Time.timeScale, Is.EqualTo(1f));
    }

    private void AssertHandlerCount(int expected)
    {
        foreach (string name in new[] { "Defeated", "Revived", "Initialized" })
        {
            Delegate handlers = Get<Delegate>(player, name);
            int count = 0;
            if (handlers != null)
                foreach (Delegate handler in handlers.GetInvocationList())
                    if (ReferenceEquals(handler.Target, controller)) count++;
            Assert.That(count, Is.EqualTo(expected), name);
        }
    }

    private GameObject NewObject(string name, bool active = true, params Type[] components)
    {
        GameObject result = new GameObject(name, components);
        result.SetActive(active);
        objects.Add(result);
        return result;
    }

    private static T Get<T>(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, Flags);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        return (T)field.GetValue(target);
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
