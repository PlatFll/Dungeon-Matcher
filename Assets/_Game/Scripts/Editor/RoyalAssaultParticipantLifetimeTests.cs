using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode yield-boundary tests, NOT timed attack/playback tests. Inactive
// disposable actors prevent automatic loops and optional presentation setup.
// The remaining participant list and reservation state are seeded explicitly.
// No PlayerPrefs, source assets, live singleton or Unity clock is modified.
[NonParallelizable]
public sealed class RoyalAssaultParticipantLifetimeTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private GameObject root;
    private EnemyDefinition definition;
    private PlayerActor player;
    private BoardController board;
    private EnemyActor kingActor;
    private KingEnemyAbility king;
    private IEnumerator assault;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("RoyalAssaultLifetimeFixture");
        root.SetActive(false);
        definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        Set(definition, "hasSpecialAbility", true);
        board = Child("Board").AddComponent<BoardController>();
        player = Child("Player").AddComponent<PlayerActor>();
        Set(player, "isInitialized", true);
        Set(player, "maximumHealth", 200);
        Set(player, "currentHealth", 200);
        kingActor = Actor("King");
        king = kingActor.gameObject.AddComponent<KingEnemyAbility>();
        Set(king, "actor", kingActor);
        Set(king, "board", board);
        Set(king, "released", false);
        Set(king, "pending", true);
        Set(king, "cycle", 1);
        Assert.That(kingActor.TryBeginSpecialAbilityAnimationAction(), Is.True);
        Get<HashSet<EnemyActor>>(king, "locks").Add(kingActor);
        assault = (IEnumerator)Call(king, "Assault");
    }

    [TearDown]
    public void TearDown()
    {
        (assault as IDisposable)?.Dispose();
        if (king != null) Call(king, "Cleanup");
        if (root != null) UnityEngine.Object.DestroyImmediate(root);
        if (definition != null) UnityEngine.Object.DestroyImmediate(definition);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void DestroyedParticipantAfterEnteringBoardWaitIsSkipped(bool boardStillBusy)
    {
        EnemyAutoAttack attack = Participant("WaitingSoldier");
        Set(board, "isBusy", true);
        EnterWait();
        UnityEngine.Object.DestroyImmediate(attack.gameObject);
        Set(board, "isBusy", boardStillBusy);
        Assert.That(assault.MoveNext(), Is.False);
        AssertCompleted();
        Assert.That(board.IsBusy, Is.EqualTo(boardStillBusy), "command cleanup must not release board ownership");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void DefeatedParticipantDoesNotWaitForOldBoardOrStaggerBlock(bool boardWait)
    {
        EnemyAutoAttack attack = Participant("DefeatedSoldier");
        Set(board, "isBusy", boardWait);
        SetStagger(attack, !boardWait);
        EnterWait();
        Assert.That(attack.EnemyActor.TryTakeDamageWithoutFeedback(1000), Is.True);
        Assert.That(attack.EnemyActor.IsDefeated, Is.True);
        Assert.That(assault.MoveNext(), Is.False);
        AssertCompleted();
        Assert.That(attack.IsCommandReservedBy(king), Is.False);
        Assert.That(attack.EnemyActor.HasAnimationActionInProgress, Is.False);
        Assert.That(board.IsBusy, Is.EqualTo(boardWait));
    }

    [Test]
    public void MissingActorReferenceAfterEnteringStaggerWaitIsSkipped()
    {
        EnemyAutoAttack attack = Participant("UnboundSoldier");
        SetStagger(attack, true);
        EnterWait();
        Set(attack, "enemyActor", null);
        Assert.That(assault.MoveNext(), Is.False);
        AssertCompleted();
        Assert.That(attack.IsCommandReservedBy(king), Is.False);
    }

    [Test]
    public void DestroyedAttackComponentAfterEnteringStaggerWaitIsSkipped()
    {
        EnemyAutoAttack attack = Participant("RemovedAttack");
        SetStagger(attack, true);
        EnterWait();
        UnityEngine.Object.DestroyImmediate(attack);
        Assert.That(assault.MoveNext(), Is.False);
        AssertCompleted();
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void LivingParticipantStillWaitsAndKeepsItsReservation(bool busy, bool staggered)
    {
        EnemyAutoAttack attack = Participant("LivingSoldier");
        Set(board, "isBusy", busy);
        SetStagger(attack, staggered);
        EnterWait();
        for (int i = 0; i < 3; i++)
        {
            Assert.That(assault.MoveNext(), Is.True);
            Assert.That(assault.Current, Is.Null);
        }
        Assert.That(attack.IsCommandReservedBy(king), Is.True);
        Assert.That(attack.EnemyActor.IsSpecialAbilityAnimationActionActive, Is.True);
        Assert.That(attack.IsAttackSequenceInProgress, Is.False);
        Assert.That(Get<bool>(king, "pending"), Is.True);
        Assert.That(Get<int>(king, "cycle"), Is.EqualTo(1));
        Assert.That(player.CurrentHealth, Is.EqualTo(200));
    }

    [Test]
    public void LaterLivingParticipantIsReachedButMustStillWaitAfterEarlierDeath()
    {
        EnemyAutoAttack lost = Participant("FirstSoldier");
        EnemyAutoAttack survivor = Participant("LaterSoldier");
        Set(board, "isBusy", true);
        EnterWait();
        UnityEngine.Object.DestroyImmediate(lost.gameObject);
        Assert.That(assault.MoveNext(), Is.True, "continue to later participant's own board wait");
        Assert.That(assault.Current, Is.Null);
        Assert.That(survivor.IsCommandReservedBy(king), Is.True);
        Set(board, "isBusy", false);
        SetStagger(survivor, true);
        Assert.That(assault.MoveNext(), Is.True, "later participant's stagger must also end");
        Assert.That(assault.Current, Is.Null);
        int releasedActions = 0;
        survivor.EnemyActor.AnimationActionReleased += _ => releasedActions++;
        SetStagger(survivor, false);
        // This fixture's inactive actor rejects actual attack execution. We
        // verify continuation/cleanup here; successful strikes need Play Mode.
        Assert.That(assault.MoveNext(), Is.False);
        Assert.That(releasedActions, Is.EqualTo(1));
        Assert.That(survivor.IsCommandReservedBy(king), Is.False);
        Assert.That(survivor.RemainingAttackTime, Is.EqualTo(7f), "rejected/unspent reservation restores stored timer");
        AssertCompleted();
    }

    [Test]
    public void OwnerCleanupDuringWaitDoesNotCompleteOrAdvanceOldCommand()
    {
        EnemyAutoAttack attack = Participant("WaitingSoldier");
        Set(board, "isBusy", true);
        EnterWait();
        Call(king, "Cleanup");
        UnityEngine.Object.DestroyImmediate(attack.gameObject);
        Assert.That(assault.MoveNext(), Is.False);
        Assert.That(Get<bool>(king, "released"), Is.True);
        Assert.That(Get<int>(king, "cycle"), Is.EqualTo(1));
        Assert.That(kingActor.IsSpecialReady, Is.True, "cancel does not commit successful cycle advancement");
        Assert.That(Get<List<EnemyAutoAttack>>(king, "participants"), Is.Empty);
        Assert.That(Get<HashSet<EnemyActor>>(king, "locks"), Is.Empty);
        Assert.That(board.IsBusy, Is.True);
        Assert.That(player.CurrentHealth, Is.EqualTo(200));
    }

    [Test]
    public void AlreadyDefeatedParticipantIsStillSkippedBeforeAnyWait()
    {
        EnemyAutoAttack attack = Participant("AlreadyDefeated");
        Assert.That(attack.EnemyActor.TryTakeDamageWithoutFeedback(1000), Is.True);
        Set(board, "isBusy", true);
        Assert.That(assault.MoveNext(), Is.True);
        Assert.That(assault.Current, Is.InstanceOf<WaitForSeconds>());
        Assert.That(assault.MoveNext(), Is.False);
        AssertCompleted();
        Assert.That(board.IsBusy, Is.True);
    }

    [Test]
    public void AlreadyDestroyedParticipantIsStillSkippedBeforeAnyWait()
    {
        EnemyAutoAttack attack = Participant("AlreadyDestroyed");
        UnityEngine.Object.DestroyImmediate(attack.gameObject);
        Set(board, "isBusy", true);
        Assert.That(assault.MoveNext(), Is.True);
        Assert.That(assault.Current, Is.InstanceOf<WaitForSeconds>());
        Assert.That(assault.MoveNext(), Is.False);
        AssertCompleted();
        Assert.That(board.IsBusy, Is.True);
    }

    private void EnterWait()
    {
        Assert.That(assault.MoveNext(), Is.True);
        Assert.That(assault.Current, Is.InstanceOf<WaitForSeconds>(), "existing windup is retained");
        Assert.That(assault.MoveNext(), Is.True);
        Assert.That(assault.Current, Is.Null, "reached real Assault board/stagger wait boundary");
    }

    private void AssertCompleted()
    {
        Assert.That(Get<bool>(king, "pending"), Is.False);
        Assert.That(Get<int>(king, "cycle"), Is.EqualTo(2));
        Assert.That(kingActor.IsSpecialReady, Is.False);
        Assert.That(kingActor.HasAnimationActionInProgress, Is.False);
        Assert.That(Get<List<EnemyAutoAttack>>(king, "participants"), Is.Empty);
        Assert.That(Get<HashSet<EnemyActor>>(king, "locks"), Is.Empty);
        Assert.That(player.CurrentHealth, Is.EqualTo(200), "lost participants cause no attack damage");
    }

    private EnemyAutoAttack Participant(string name)
    {
        EnemyActor actor = Actor(name);
        EnemyStagger stagger = actor.gameObject.AddComponent<EnemyStagger>();
        EnemyAutoAttack attack = actor.gameObject.AddComponent<EnemyAutoAttack>();
        Set(attack, "enemyActor", actor);
        Set(attack, "enemyStagger", stagger);
        Set(attack, "playerTarget", player);
        Set(attack, "attackAutomatically", false);
        Set(attack, "commandOwner", king);
        Set(attack, "commandMadeReady", true);
        Set(attack, "reservedAttackTime", 7f);
        Set(attack, "remainingAttackTime", 0f);
        Assert.That(actor.TryBeginSpecialAbilityAnimationAction(), Is.True);
        Get<List<EnemyAutoAttack>>(king, "participants").Add(attack);
        Get<HashSet<EnemyActor>>(king, "locks").Add(actor);
        return attack;
    }

    private EnemyActor Actor(string name)
    {
        EnemyActor actor = Child(name).AddComponent<EnemyActor>();
        Set(actor, "definition", definition);
        Set(actor, "<RuntimeStats>k__BackingField", new EnemyRuntimeStats(25, 1, 100, 10, 0, 10f, 4));
        Set(actor, "isInitialized", true);
        Set(actor, "currentHealth", 100);
        Set(actor, "isSpecialReady", true);
        Set(actor, "currentSpecialTurnCount", 4);
        return actor;
    }

    private GameObject Child(string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(root.transform, false);
        return child;
    }

    private static void SetStagger(EnemyAutoAttack attack, bool active)
    {
        EnemyStagger stagger = attack.GetComponent<EnemyStagger>();
        Set(stagger, "isStaggered", active);
        Set(stagger, "remainingStaggerTime", active ? 60f : 0f);
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

    private static object Call(object target, string name)
    {
        MethodInfo method = target.GetType().GetMethod(name, Flags);
        if (method == null) throw new MissingMethodException(target.GetType().Name, name);
        return method.Invoke(target, null);
    }
}
