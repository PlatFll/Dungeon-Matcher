using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// EditMode branch/ordering tests, not real-time or rendered encounter tests.
/// Inactive disposable actors avoid Start/visual setup. A summon-service double
/// returns a fixture local; existing production readiness and retreat methods
/// are exercised. Coroutine yield boundaries are driven explicitly.
/// </summary>
public sealed class TownMarshalStaggerTests
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
    private readonly List<Object> disposable = new List<Object>();
    private EnemyActor marshal;
    private EnemyActor local;
    private EnemyAutoAttack localAttack;
    private EnemyStagger stagger;
    private BoardController board;
    private TownMarshalEnemyAbility ability;
    private List<EnemyActor> roster;
    private SummonService summons;
    private UnityEngine.Random.State previousRandom;
    private int casts;

    private sealed class SummonService : IEnemySummonService
    {
        public bool FreeSlot = true;
        public int Calls;
        public EnemyActor Local;
        public bool HasFreeEnemySlot => FreeSlot;
        public bool TrySummonEnemy(EnemyDefinition definition, out EnemyActor summonedEnemy)
        {
            Calls++;
            summonedEnemy = FreeSlot ? Local : null;
            return summonedEnemy != null;
        }
    }

    [SetUp]
    public void SetUp()
    {
        previousRandom = UnityEngine.Random.state;
        casts = 0;
        EnemyDefinition localData = MakeDefinition();
        EnemyDefinition marshalData = MakeDefinition();
        Set(marshalData, "hasSpecialAbility", true);
        Set(marshalData, "townMarshalSummonCandidates", new[] { localData });
        marshal = MakeActor("Marshal stagger fixture", marshalData);
        local = MakeActor("Independent local fixture", localData);
        localAttack = local.gameObject.AddComponent<EnemyAutoAttack>();
        stagger = marshal.gameObject.AddComponent<EnemyStagger>();
        Set(stagger, "enemyActor", marshal);
        board = MakeInactiveObject("Marshal fixture board").AddComponent<BoardController>();
        ability = marshal.gameObject.AddComponent<TownMarshalEnemyAbility>();
        roster = new List<EnemyActor> { marshal };
        summons = new SummonService { Local = local };
        ability.ConfigureSummonService(summons);
        ability.InitializeSpecialAbility(marshal, board, roster);
        marshal.SpecialAbilityUsed += OnCast;
        Ready();
    }

    [TearDown]
    public void TearDown()
    {
        // Detach the ability while its actor and protector still exist.
        if (ability != null) Object.DestroyImmediate(ability);
        if (marshal != null) marshal.SpecialAbilityUsed -= OnCast;
        for (int i = disposable.Count - 1; i >= 0; i--)
            if (disposable[i] != null) Object.DestroyImmediate(disposable[i]);
        disposable.Clear();
        UnityEngine.Random.state = previousRandom;
    }

    [Test]
    public void ReadinessReachedDuringStaggerDoesNotSummonOrConsumeCharge()
    {
        Set(marshal, "isSpecialReady", false);
        Set(marshal, "currentSpecialTurnCount", 2);
        SetStagger(true);
        marshal.RegisterValidPlayerTurn();
        AssertDeferred();
    }

    [Test]
    public void AutoAttackReleaseCannotBypassStagger()
    {
        SetStagger(true);
        Set(marshal, "isAutoAttackAnimationActionActive", true);
        marshal.EndAutoAttackAnimationAction();
        AssertDeferred();
    }

    [Test]
    public void ReadyPollWaitsForStaggerBoardAndActionThenSummonsOnce()
    {
        SetStagger(true);
        IEnumerator retry = Retry();
        Assert.That(retry.MoveNext(), Is.True);
        Assert.That(retry.Current, Is.Null);
        AssertDeferred();

        SetStagger(false);
        Set(board, "isBusy", true);
        Assert.That(retry.MoveNext(), Is.True, "board remains the owner after stagger ends");
        Assert.That(summons.Calls, Is.Zero);

        Set(board, "isBusy", false);
        Set(marshal, "isAutoAttackAnimationActionActive", true);
        Assert.That(retry.MoveNext(), Is.True, "another action still blocks startup");
        Assert.That(summons.Calls, Is.Zero);

        // Do not fire the action-release event here: test the poll's own gate.
        Set(marshal, "isAutoAttackAnimationActionActive", false);
        Assert.That(retry.MoveNext(), Is.False);
        Assert.That(summons.Calls, Is.EqualTo(1));
        Assert.That(casts, Is.EqualTo(1));
        Assert.That(marshal.IsSpecialReady, Is.False);
        Assert.That(marshal.CurrentSpecialTurnCount, Is.Zero);
        Assert.That(marshal.DamageRedirectTarget, Is.SameAs(local));
        Call(ability, "TryUseReadyAbility");
        Assert.That(summons.Calls, Is.EqualTo(1), "no duplicate cast after consumption");
        Assert.That(marshal.HasAnimationActionInProgress, Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void BuildupOrPostStaggerImmunityDoesNotBlockSpecial(bool immunity)
    {
        SetStagger(false);
        Set(stagger, "staggerMeterNormalized", immunity ? 0f : 0.95f);
        Set(stagger, "remainingImmunityTime", immunity ? 5f : 0f);
        Call(ability, "TryUseReadyAbility");
        Assert.That(summons.Calls, Is.EqualTo(1));
        Assert.That(casts, Is.EqualTo(1));
        Assert.That(marshal.IsSpecialReady, Is.False);
    }

    [Test]
    public void ReadyRallyCannotStartWhileStaggered()
    {
        summons.FreeSlot = false;
        roster.Add(local);
        Assert.That((bool)Call(ability, "CanUseCitizensSeizeHim"), Is.True);
        SetStagger(true);
        Call(ability, "TryUseReadyAbility");
        AssertDeferred();
        Assert.That(localAttack.RuntimeAttackSpeedMultiplier, Is.EqualTo(1f));
        Assert.That((bool)Get(ability, "isRallyActive"), Is.False);
    }

    [Test]
    public void RallyExpiryRemovesItsBuffButCannotStartAnotherCastDuringStagger()
    {
        SetStagger(true);
        Set(ability, "isRallyActive", true);
        localAttack.SetRuntimeAttackSpeedMultiplier(1.4f);
        ((List<EnemyAutoAttack>)Get(ability, "ralliedAutoAttacks")).Add(localAttack);
        IEnumerator expiry = (IEnumerator)Call(ability, "RallyDurationRoutine", 5f, 1.4f);
        Assert.That(expiry.MoveNext(), Is.True);
        Assert.That(expiry.Current, Is.TypeOf<WaitForSeconds>());
        Assert.That(localAttack.RuntimeAttackSpeedMultiplier, Is.EqualTo(1.4f));
        Assert.That(expiry.MoveNext(), Is.False);
        Assert.That(localAttack.RuntimeAttackSpeedMultiplier, Is.EqualTo(1f));
        Assert.That((bool)Get(ability, "isRallyActive"), Is.False);
        AssertDeferred();
    }

    [Test]
    public void MarshalDeathEndsDeferredPollAndRetreatWithoutRemovingLocal()
    {
        Call(ability, "BeginRetreat", local);
        SetStagger(true);
        IEnumerator retry = Retry();
        Assert.That(retry.MoveNext(), Is.True);
        Assert.That(marshal.DamageRedirectTarget, Is.SameAs(local));
        Assert.That(marshal.TryTakeDamageWithoutFeedback(marshal.CurrentHealth), Is.True);
        Assert.That(retry.MoveNext(), Is.False);
        Assert.That(marshal.IsDefeated, Is.True);
        Assert.That(Get(ability, "currentProtector"), Is.Null);
        Assert.That(local.IsDefeated, Is.False);
        Assert.That(summons.Calls, Is.Zero);
        Assert.That(casts, Is.Zero);
    }

    [Test]
    public void ProtectorDeathStillClearsRetreatAndHeldChargeDuringStagger()
    {
        Call(ability, "BeginRetreat", local);
        SetStagger(true);
        IEnumerator retry = Retry();
        Assert.That(retry.MoveNext(), Is.True);
        Assert.That(local.TryTakeDamageWithoutFeedback(local.CurrentHealth), Is.True);
        Assert.That(marshal.IsDefeated, Is.False);
        Assert.That(marshal.IsSpecialReady, Is.False, "existing breathing-room reset is preserved");
        Assert.That(Get(ability, "currentProtector"), Is.Null);
        Assert.That(retry.MoveNext(), Is.False);
        Assert.That(summons.Calls, Is.Zero);
        Assert.That(casts, Is.Zero);
    }

    private void AssertDeferred()
    {
        Assert.That(summons.Calls, Is.Zero);
        Assert.That(casts, Is.Zero);
        Assert.That(marshal.IsSpecialReady, Is.True);
        Assert.That(marshal.CurrentSpecialTurnCount, Is.EqualTo(3));
        Assert.That(marshal.HasAnimationActionInProgress, Is.False);
        Assert.That(board.IsBusy, Is.False);
    }

    private void Ready()
    {
        Set(marshal, "currentSpecialTurnCount", 3);
        Set(marshal, "isSpecialReady", true);
    }

    private void SetStagger(bool active)
    {
        Set(stagger, "isStaggered", active);
        Set(stagger, "remainingStaggerTime", active ? 2f : 0f);
        Set(stagger, "activeStaggerDuration", active ? 2f : 0f);
    }

    private IEnumerator Retry() => (IEnumerator)Call(ability, "WaitUntilReadyAbilityCanExecute");
    private void OnCast(EnemyActor actor) => casts++;

    private EnemyDefinition MakeDefinition()
    {
        var data = ScriptableObject.CreateInstance<EnemyDefinition>();
        disposable.Add(data);
        return data;
    }

    private GameObject MakeInactiveObject(string name)
    {
        var root = new GameObject(name);
        root.SetActive(false);
        disposable.Add(root);
        return root;
    }

    private EnemyActor MakeActor(string name, EnemyDefinition data)
    {
        var actor = MakeInactiveObject(name).AddComponent<EnemyActor>();
        Set(actor, "definition", data);
        Set(actor, "isInitialized", true);
        Set(actor, "currentHealth", 100);
        Set(actor, "<RuntimeStats>k__BackingField", new EnemyRuntimeStats(1, 1, 100, 1, 0, 10f, 3));
        return actor;
    }

    private static object Get(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, Fields);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        return field.GetValue(target);
    }

    private static void Set(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(name, Fields);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        field.SetValue(target, value);
    }

    private static object Call(object target, string name, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(name, Fields);
        if (method == null) throw new MissingMethodException(target.GetType().Name, name);
        return method.Invoke(target, args);
    }
}
