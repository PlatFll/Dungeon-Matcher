using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Isolated EditMode fixtures: no scene loading, board mutations, saved mastery
// edits or writes to serialized character assets. Unity execution is deferred
// to the combined gameplay-audit validation pass.
public sealed class PlayerAbilityLifecycleTests
{
    private GameObject root;
    private PlayerDefinition definition;
    private CharacterAbilityDefinition ability;
    private PlayerActor player;
    private PlayerAbilityEnergy energy;
    private PlayerAbilityController controller;
    private PlayerAbilityLifecycleProbe runtime;

    [SetUp]
    public void SetUp()
    {
        Assert.That(RunUpgradeRuntime.Current, Is.Null,
            "Run these isolated EditMode fixtures outside an active run.");
        PlayerDefinition template = Resources.Load<PlayerDefinition>("Players/Player_Skeleton");
        Assert.That(template, Is.Not.Null);
        Assert.That(template.ActiveAbility, Is.InstanceOf<RoyalDecreeAbilityDefinition>());
        definition = UnityEngine.Object.Instantiate(template);
        ability = UnityEngine.Object.Instantiate(template.ActiveAbility);
        Assert.That(ability.RuntimeType, Is.Null, "Probe fixture must not install a production runtime.");
        SetField(definition, "activeAbility", ability);

        root = new GameObject("Player ability lifecycle fixture");
        root.SetActive(false);
        player = root.AddComponent<PlayerActor>();
        SetField(player, "initializeOnStart", false);
        energy = root.AddComponent<PlayerAbilityEnergy>();
        runtime = root.AddComponent<PlayerAbilityLifecycleProbe>();
        controller = root.AddComponent<PlayerAbilityController>();
        SetField(controller, "playerActor", player);
        SetField(controller, "playerAbilityEnergy", energy);
        player.Initialize(definition);
        root.SetActive(true);
        controller.RefreshRuntime();
        energy.ResetEnergy();
        energy.AddEnergy(energy.MaximumEnergy);
        Assert.That(controller.RequiredEnergy, Is.GreaterThan(0).And.LessThanOrEqualTo(energy.MaximumEnergy));
        Assert.That(controller.CanActivate, Is.True, "Fixture starts with a usable funded runtime: " +
            $"energy={energy.CurrentEnergy}/{controller.RequiredEnergy}, controller={controller.isActiveAndEnabled}, " +
            $"player={player.IsInitialized}/{player.IsDefeated}, probe={runtime.isActiveAndEnabled}/{runtime.IsActive}, " +
            $"selected={typeof(PlayerAbilityController).GetField("activeRuntime",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(controller)}, run={RunSession.Current}");
    }

    [TearDown]
    public void TearDown()
    {
        if (root != null) UnityEngine.Object.DestroyImmediate(root);
        if (definition != null) UnityEngine.Object.DestroyImmediate(definition);
        if (ability != null) UnityEngine.Object.DestroyImmediate(ability);
        root = null;
        definition = null;
        ability = null;
        player = null;
        energy = null;
        controller = null;
        runtime = null;
    }

    [Test]
    public void AcceptedCastSpendsOnceAndRejectsRepeatedAttempt()
    {
        int before = energy.CurrentEnergy;
        int cost = controller.RequiredEnergy;
        Assert.That(controller.TryActivate(), Is.True);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(before - cost));
        Assert.That(controller.IsAbilityActive, Is.True);
        Assert.That(controller.TryActivate(), Is.False);
        Assert.That(runtime.ActivationAttempts, Is.EqualTo(1));
        Assert.That(runtime.AcceptedActivations, Is.EqualTo(1));
        Assert.That(energy.CurrentEnergy, Is.EqualTo(before - cost));
    }

    [Test]
    public void InsufficientEnergyRejectsBeforeRuntimeAttempt()
    {
        energy.ResetEnergy();
        Assert.That(controller.CanActivate, Is.False);
        Assert.That(controller.TryActivate(), Is.False);
        Assert.That(runtime.ActivationAttempts, Is.Zero);
        Assert.That(energy.CurrentEnergy, Is.Zero);
    }

    [Test]
    public void RuntimeRejectionDoesNotSpendEnergy()
    {
        int before = energy.CurrentEnergy;
        runtime.RejectActivation = true;
        Assert.That(controller.TryActivate(), Is.False);
        Assert.That(runtime.ActivationAttempts, Is.EqualTo(1));
        Assert.That(runtime.AcceptedActivations, Is.Zero);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(before));
    }

    [Test]
    public void DisabledControllerRejectsWithoutSideEffects()
    {
        int before = energy.CurrentEnergy;
        controller.enabled = false;
        Assert.That(controller.CanActivate, Is.False);
        Assert.That(controller.TryActivate(), Is.False);
        Assert.That(runtime.ActivationAttempts, Is.Zero);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(before));
        controller.enabled = true;
        Assert.That(controller.CanActivate, Is.True);
        Assert.That(controller.TryActivate(), Is.True);
    }

    [Test]
    public void InactivePlayerRootRejectsWithoutSideEffects()
    {
        int before = energy.CurrentEnergy;
        root.SetActive(false);
        Assert.That(controller.CanActivate, Is.False);
        Assert.That(controller.TryActivate(), Is.False);
        Assert.That(runtime.ActivationAttempts, Is.Zero);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(before));
    }

    [Test]
    public void DisabledRuntimeRejectsWithoutSideEffects()
    {
        int before = energy.CurrentEnergy;
        runtime.enabled = false;
        // The probe itself would accept: the coordinator must reject first.
        Assert.That(runtime.CanActivate(ability), Is.True);
        Assert.That(controller.CanActivate, Is.False);
        Assert.That(controller.TryActivate(), Is.False);
        Assert.That(runtime.ActivationAttempts, Is.Zero);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(before));
    }

    [Test]
    public void ReenabledRuntimeBecomesUsableWithoutReplacingIt()
    {
        runtime.enabled = false;
        Assert.That(controller.CanActivate, Is.False);
        runtime.enabled = true;
        Assert.That(root.GetComponents<PlayerAbilityLifecycleProbe>().Length, Is.EqualTo(1));
        Assert.That(controller.CanActivate, Is.True);
        Assert.That(controller.TryActivate(), Is.True);
        Assert.That(runtime.AcceptedActivations, Is.EqualTo(1));
    }

    [Test]
    public void DestroyedInterfaceRuntimeCannotRemainCastable()
    {
        int before = energy.CurrentEnergy;
        UnityEngine.Object.DestroyImmediate(runtime);
        Assert.That(controller.IsAbilityActive, Is.False);
        Assert.That(controller.CanActivate, Is.False);
        Assert.That(controller.TryActivate(), Is.False);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(before));
        Assert.DoesNotThrow(controller.CancelActiveAbility);
    }

    [Test]
    public void ReenablingControllerRestoresOneRuntimeSubscription()
    {
        for (int cycle = 0; cycle < 5; cycle++)
        {
            controller.enabled = false;
            controller.enabled = true;
            AssertSingleNotification(runtime.NotifyStateChanged);
        }
        Assert.That(controller.TryActivate(), Is.True);
        AssertSingleNotification(runtime.Complete);
        Assert.That(controller.IsAbilityActive, Is.False);
    }

    [Test]
    public void RepeatedRefreshDoesNotDuplicateRuntimeSubscription()
    {
        for (int index = 0; index < 5; index++) controller.RefreshRuntime();
        AssertSingleNotification(runtime.NotifyStateChanged);
    }

    [Test]
    public void RefreshWhileDisabledDoesNotRestoreCallbacks()
    {
        controller.enabled = false;
        int notifications = 0;
        Action handler = () => notifications++;
        controller.StateChanged += handler;
        try
        {
            controller.RefreshRuntime();
            runtime.NotifyStateChanged();
            Assert.That(notifications, Is.Zero);
        }
        finally { controller.StateChanged -= handler; }
        controller.enabled = true;
        AssertSingleNotification(runtime.NotifyStateChanged);
    }

    [Test]
    public void DisableCancelsOnceWithoutRefundingAcceptedCast()
    {
        Assert.That(controller.TryActivate(), Is.True);
        int afterSpending = energy.CurrentEnergy;
        controller.enabled = false;
        Assert.That(runtime.IsActive, Is.False);
        Assert.That(runtime.Cancellations, Is.EqualTo(1));
        controller.CancelActiveAbility();
        Assert.That(runtime.Cancellations, Is.EqualTo(1));
        Assert.That(energy.CurrentEnergy, Is.EqualTo(afterSpending));
    }

    [Test]
    public void PlayerDefeatCancelsAndRejectsFurtherActivation()
    {
        Assert.That(controller.TryActivate(), Is.True);
        Assert.That(player.TryTakeDamage(player.CurrentHealth), Is.True);
        Assert.That(player.IsDefeated, Is.True);
        Assert.That(runtime.Cancellations, Is.EqualTo(1));
        energy.AddEnergy(energy.MaximumEnergy);
        int before = energy.CurrentEnergy;
        Assert.That(controller.CanActivate, Is.False);
        Assert.That(controller.TryActivate(), Is.False);
        Assert.That(energy.CurrentEnergy, Is.EqualTo(before));
    }

    [Test]
    public void ReinitializationCancelsAndRetainsSameRuntimeNotifications()
    {
        Assert.That(controller.TryActivate(), Is.True);
        player.Initialize(definition);
        Assert.That(runtime.Cancellations, Is.EqualTo(1));
        Assert.That(controller.IsAbilityActive, Is.False);
        AssertSingleNotification(runtime.NotifyStateChanged);
    }

    [Test]
    public void RefreshRebindsAfterRuntimeComponentReplacement()
    {
        UnityEngine.Object.DestroyImmediate(runtime);
        runtime = root.AddComponent<PlayerAbilityLifecycleProbe>();
        controller.RefreshRuntime();
        AssertSingleNotification(runtime.NotifyStateChanged);
        Assert.That(controller.CanActivate, Is.True);
        Assert.That(controller.TryActivate(), Is.True);
        Assert.That(runtime.AcceptedActivations, Is.EqualTo(1));
    }

    private void AssertSingleNotification(Action source)
    {
        int notifications = 0;
        Action handler = () => notifications++;
        controller.StateChanged += handler;
        try
        {
            source();
            Assert.That(notifications, Is.EqualTo(1));
        }
        finally { controller.StateChanged -= handler; }
    }

    private static void SetField(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Fixture field {target.GetType().Name}.{name} exists.");
        field.SetValue(target, value);
    }
}
