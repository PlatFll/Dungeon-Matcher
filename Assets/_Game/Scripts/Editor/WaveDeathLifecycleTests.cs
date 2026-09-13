using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

// Synchronous EditMode fixtures. Real staged spawning, death animation timing,
// next-wave resumption and summons must also be tested in the combined Play Mode pass.
[NonParallelizable]
public sealed class WaveDeathLifecycleTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private readonly List<GameObject> objects = new List<GameObject>();
    private WaveController waves;

    [SetUp]
    public void SetUp()
    {
        Assert.That(Application.isPlaying, Is.False, "Run these fixtures in EditMode.");
        waves = New<WaveController>("WaveLifecycle");
        Set(waves, "enemySlots", new EnemySlotUI[0]);
        Set(waves, "spawnWaveOnStart", false);
        Set(waves, "advanceWavesAutomatically", false);
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = objects.Count - 1; i >= 0; i--)
            if (objects[i] != null) UnityEngine.Object.DestroyImmediate(objects[i]);
        objects.Clear();
    }

    [Test]
    public void DeathCompletionAcknowledgesExactlyOnce()
    {
        Action first = TrackDeath();
        Action second = TrackDeath();
        Assert.That(Pending, Is.EqualTo(2));
        first(); first();
        Assert.That(Pending, Is.EqualTo(1));
        second(); second();
        Assert.That(Pending, Is.Zero);
    }

    [Test]
    public void OldCompletionDoesNotConsumeNewEncounterDeath()
    {
        Action old = TrackDeath();
        waves.ClearCurrentWave();
        Action current = TrackDeath();
        old(); old();
        Assert.That(Pending, Is.EqualTo(1));
        current();
        Assert.That(Pending, Is.Zero);
    }

    [Test]
    public void ClearingSameWaveNumberStillInvalidatesOldCompletion()
    {
        int number = waves.CurrentWave;
        object identity = Get(waves, "encounterIdentity");
        Action old = TrackDeath();
        waves.ClearCurrentWave();
        Assert.That(waves.CurrentWave, Is.EqualTo(number));
        Assert.That(Get(waves, "encounterIdentity"), Is.Not.SameAs(identity));
        ActivateWaitingWave();
        int completions = 0;
        waves.WaveCompleted += _ => completions++;
        old();
        Assert.That(waves.IsWaveActive, Is.True);
        Assert.That(completions, Is.Zero);
    }

    [Test]
    public void LastCurrentDeathCompletesWaveExactlyOnce()
    {
        ActivateWaitingWave();
        Action first = TrackDeath(), last = TrackDeath();
        int completions = 0;
        waves.WaveCompleted += _ => completions++;
        first();
        Assert.That(completions, Is.Zero);
        last(); last(); first();
        Assert.That(completions, Is.EqualTo(1));
        Assert.That(waves.IsWaveActive, Is.False);
    }

    [Test]
    public void LivingRosterMemberPreventsDeathCompletionFromEndingWave()
    {
        ActivateWaitingWave();
        EnemyActor survivor = New<EnemyActor>("Survivor");
        var roster = (List<EnemyActor>)Get(waves, "activeEnemies");
        roster.Add(survivor);
        TrackDeath()();
        Assert.That(Pending, Is.Zero);
        Assert.That(waves.IsWaveActive, Is.True);
        Assert.That(roster, Does.Contain(survivor));
    }

    [Test]
    public void PlannedSpawnsPreventPrematureCompletion()
    {
        ActivateWaitingWave();
        Set(waves, "isSpawningWave", true);
        int completions = 0;
        waves.WaveCompleted += _ => completions++;
        TrackDeath()();
        Assert.That(completions, Is.Zero);
        Assert.That(waves.IsWaveActive, Is.True);
        Set(waves, "isSpawningWave", false);
        Call(waves, "TryCompleteWaveAfterDeaths");
        Assert.That(completions, Is.EqualTo(1));
    }

    [Test]
    public void PublicClearRevokesSpawnOwnershipAndPendingDeathState()
    {
        ActivateWaitingWave();
        object identity = Get(waves, "encounterIdentity");
        Set(waves, "isSpawningWave", true);
        TrackDeath();
        waves.ClearCurrentWave();
        Assert.That((bool)Get(waves, "isSpawningWave"), Is.False);
        Assert.That(Get(waves, "waveSpawnCoroutine"), Is.Null);
        Assert.That((bool)Get(waves, "waitingForDeathEffects"), Is.False);
        Assert.That(Pending, Is.Zero);
        Assert.That(waves.IsWaveActive, Is.False);
        Assert.That((bool)Call(waves, "IsCurrentSpawn", identity, waves.CurrentWave), Is.False);
    }

    [Test]
    public void SpawnGuardRejectsOldIdentityAndDifferentWaveNumber()
    {
        object identity = Get(waves, "encounterIdentity");
        Set(waves, "isSpawningWave", true);
        Assert.That((bool)Call(waves, "IsCurrentSpawn", identity, waves.CurrentWave), Is.True);
        Assert.That((bool)Call(waves, "IsCurrentSpawn", new object(), waves.CurrentWave), Is.False);
        Assert.That((bool)Call(waves, "IsCurrentSpawn", identity, waves.CurrentWave + 1), Is.False);
        Set(waves, "isSpawningWave", false);
        Assert.That((bool)Call(waves, "IsCurrentSpawn", identity, waves.CurrentWave), Is.False);
    }

    [Test]
    public void ForeignOrNullDefeatCannotAddDeathDebtOrCompleteWave()
    {
        ActivateWaitingWave();
        int completions = 0;
        waves.WaveCompleted += _ => completions++;
        EnemyActor foreign = New<EnemyActor>("ForeignEnemy");
        Call(waves, "HandleEnemyDefeated", null, foreign);
        Call(waves, "HandleEnemyDefeated", null, null);
        Assert.That(Pending, Is.Zero);
        Assert.That(completions, Is.Zero);
        Assert.That(waves.IsWaveActive, Is.True);
    }

    [Test]
    public void OldTransitionCannotAdvanceReplacedEncounterWithSameWaveNumber()
    {
        AddLivePlayer();
        waves.gameObject.SetActive(true);
        int number = waves.CurrentWave;
        IEnumerator transition = (IEnumerator)Call(waves, "AdvanceToNextWaveWhenReady", number);
        try
        {
            Assert.That(transition.MoveNext(), Is.True, "Reach cleanup-frame yield.");
            waves.ClearCurrentWave();
            Assert.That(transition.MoveNext(), Is.False);
            Assert.That(waves.CurrentWave, Is.EqualTo(number));
        }
        finally { (transition as IDisposable)?.Dispose(); }
    }

    [Test]
    public void CompletionListenerClearPreventsSchedulingOldAutomaticAdvance()
    {
        AddLivePlayer();
        waves.gameObject.SetActive(true);
        Set(waves, "advanceWavesAutomatically", true);
        Set(waves, "<IsWaveActive>k__BackingField", true);
        waves.WaveCompleted += _ => waves.ClearCurrentWave();
        Call(waves, "CompleteCurrentWave");
        Assert.That(Get(waves, "advanceWaveCoroutine"), Is.Null);
        Assert.That(waves.IsWaveActive, Is.False);
    }

    [Test]
    public void NormalDeathFinishesPresentationAndAcknowledgesOnce()
    {
        EnemyLifecycleVFX effect = New<EnemyLifecycleVFX>("DeathEffect");
        int acknowledgements = 0, finished = 0;
        ArmDeath(effect, () => acknowledgements++);
        effect.DeathFinished += _ =>
        {
            finished++;
            Assert.That(effect.IsDying, Is.False);
            Assert.That(Get(effect, "deathCompletionCallback"), Is.Null);
        };
        Call(effect, "CompleteDeathEffect", true);
        Call(effect, "CompleteDeathEffect", true);
        Call(effect, "OnDisable");
        Assert.That(acknowledgements, Is.EqualTo(1));
        Assert.That(finished, Is.EqualTo(1));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void InterruptedDeathAcknowledgesWithoutClaimingFinishedAnimation(bool disableObject)
    {
        EnemyLifecycleVFX effect = New<EnemyLifecycleVFX>("InterruptedDeath");
        effect.gameObject.SetActive(true);
        int acknowledgements = 0, finished = 0;
        ArmDeath(effect, () => acknowledgements++);
        effect.DeathFinished += _ => finished++;
        if (disableObject) effect.gameObject.SetActive(false);
        else effect.enabled = false;
        // Repeated cleanup cannot acknowledge a second time.
        Call(effect, "OnDisable");
        Assert.That(acknowledgements, Is.EqualTo(1));
        Assert.That(finished, Is.Zero);
        Assert.That(effect.IsDying, Is.False);
    }

    [Test]
    public void ThrowingPresentationObserverStillAcknowledgesGameplay()
    {
        EnemyLifecycleVFX effect = New<EnemyLifecycleVFX>("ThrowingObserver");
        int acknowledgements = 0;
        ArmDeath(effect, () => acknowledgements++);
        effect.DeathFinished += _ => throw new InvalidOperationException("Synthetic observer failure");
        var exception = Assert.Throws<TargetInvocationException>(
            () => Call(effect, "CompleteDeathEffect", true));
        Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
        Assert.That(acknowledgements, Is.EqualTo(1));
        Call(effect, "OnDisable");
        Assert.That(acknowledgements, Is.EqualTo(1));
    }

    [Test]
    public void ReentrantDisableDuringDeathFinishedCannotLoseCallback()
    {
        EnemyLifecycleVFX effect = New<EnemyLifecycleVFX>("ReentrantDeath");
        int acknowledgements = 0;
        ArmDeath(effect, () => acknowledgements++);
        effect.DeathFinished += _ => Call(effect, "OnDisable");
        Call(effect, "CompleteDeathEffect", true);
        Assert.That(acknowledgements, Is.EqualTo(1));
        Assert.That(effect.IsDying, Is.False);
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void MissingDeathVisualsReturnFailureWithoutTakingCompletionOwnership(bool hasRoot, bool hasImage)
    {
        EnemyLifecycleVFX effect = New<EnemyLifecycleVFX>("MissingDeathArt");
        AddVisuals(effect, hasRoot, hasImage);
        effect.gameObject.SetActive(true);
        Assert.That(effect.isActiveAndEnabled, Is.True);
        int acknowledgements = 0;
        Assert.That(effect.PlayDeathEffect(() => acknowledgements++), Is.False);
        Assert.That(effect.IsDying, Is.False);
        Assert.That(acknowledgements, Is.Zero, "Caller owns immediate fallback on false.");
        Assert.That(Get(effect, "deathCompletionCallback"), Is.Null);
    }

    [Test]
    public void SpawnRequestCannotDiscardAnAcceptedDeath()
    {
        EnemyLifecycleVFX effect = New<EnemyLifecycleVFX>("DeathBeforeSpawn");
        AddVisuals(effect, true, true);
        effect.gameObject.SetActive(true);
        int acknowledgements = 0;
        ArmDeath(effect, () => acknowledgements++);
        effect.PlaySpawnEffect();
        Assert.That(effect.IsDying, Is.True);
        Assert.That(acknowledgements, Is.Zero);
        Call(effect, "CompleteDeathEffect", false);
        Assert.That(acknowledgements, Is.EqualTo(1));
    }

    [Test]
    public void DeathCoroutineToleratesVisualsRemovedAfterAcceptance()
    {
        EnemyLifecycleVFX effect = New<EnemyLifecycleVFX>("RemovedDeathArt");
        int acknowledgements = 0;
        ArmDeath(effect, () => acknowledgements++);
        Set(effect, "deathWhiteHoldDuration", 0f);
        Set(effect, "deathEffectDuration", 0f);
        IEnumerator death = (IEnumerator)Call(effect, "DeathRoutine");
        try { Assert.That(death.MoveNext(), Is.False); }
        finally { (death as IDisposable)?.Dispose(); }
        Assert.That(acknowledgements, Is.EqualTo(1));
    }

    [Test]
    public void StagedSpawnDoesNotAdvertiseItsEmptySlotsAsFreeForSummons()
    {
        AddEmptySlot();
        waves.gameObject.SetActive(true);
        Set(waves, "<IsWaveActive>k__BackingField", true);
        Assert.That(waves.HasFreeEnemySlot, Is.True);
        Set(waves, "isSpawningWave", true);
        Assert.That(waves.HasFreeEnemySlot, Is.False);
        Set(waves, "isSpawningWave", false);
        Assert.That(waves.HasFreeEnemySlot, Is.True);
    }

    [Test]
    public void DisabledWaveControllerDoesNotAdvertiseSummonAvailability()
    {
        AddEmptySlot();
        waves.gameObject.SetActive(true);
        Set(waves, "<IsWaveActive>k__BackingField", true);
        Assert.That(waves.HasFreeEnemySlot, Is.True);
        waves.enabled = false;
        Assert.That(waves.HasFreeEnemySlot, Is.False);
    }

    [Test]
    public void DyingObjectUnderAnchorStillBlocksSummonSlotReuse()
    {
        EnemySlotUI slot = AddEmptySlot();
        GameObject dying = new GameObject("DyingAnchorChild");
        dying.transform.SetParent(slot.EnemySpawnAnchor, false);
        waves.gameObject.SetActive(true);
        Set(waves, "<IsWaveActive>k__BackingField", true);
        Assert.That(waves.HasFreeEnemySlot, Is.False);
        UnityEngine.Object.DestroyImmediate(dying);
        Assert.That(waves.HasFreeEnemySlot, Is.True);
    }

    private int Pending => (int)Get(waves, "pendingDeathEffects");
    private Action TrackDeath() => (Action)Call(waves, "TrackEnemyDeathCompletion", (object)null);
    private void ActivateWaitingWave()
    {
        Set(waves, "<IsWaveActive>k__BackingField", true);
        Set(waves, "waitingForDeathEffects", true);
    }
    private void AddLivePlayer()
    {
        PlayerActor player = New<PlayerActor>("WavePlayer");
        Set(player, "isInitialized", true);
        Set(player, "isDefeated", false);
        Set(waves, "playerActor", player);
    }
    private EnemySlotUI AddEmptySlot()
    {
        EnemySlotUI slot = New<EnemySlotUI>("EmptySlot");
        var anchor = new GameObject("SpawnAnchor", typeof(RectTransform));
        anchor.transform.SetParent(slot.transform, false);
        Set(slot, "enemySpawnAnchor", (RectTransform)anchor.transform);
        Set(waves, "enemySlots", new[] { slot });
        return slot;
    }
    private void AddVisuals(EnemyLifecycleVFX effect, bool root, bool image)
    {
        var go = new GameObject("SyntheticVisual", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(effect.transform, false);
        if (root) Set(effect, "visualRoot", go.GetComponent<RectTransform>());
        if (image) Set(effect, "enemyImage", go.GetComponent<Image>());
    }
    private static void ArmDeath(EnemyLifecycleVFX effect, Action callback)
    {
        Set(effect, "deathCompletionPending", true);
        Set(effect, "deathCompletionCallback", callback);
    }
    private T New<T>(string name) where T : Component
    {
        var go = new GameObject(name);
        objects.Add(go);
        go.SetActive(false);
        return go.AddComponent<T>();
    }
    private static object Get(object target, string field)
    {
        FieldInfo info = target.GetType().GetField(field, Flags);
        Assert.That(info, Is.Not.Null, target.GetType().Name + "." + field);
        return info.GetValue(target);
    }
    private static void Set(object target, string field, object value)
    {
        FieldInfo info = target.GetType().GetField(field, Flags);
        Assert.That(info, Is.Not.Null, target.GetType().Name + "." + field);
        info.SetValue(target, value);
    }
    private static object Call(object target, string method, params object[] args)
    {
        MethodInfo info = target.GetType().GetMethod(method, Flags);
        Assert.That(info, Is.Not.Null, target.GetType().Name + "." + method);
        return info.Invoke(target, args);
    }
}
