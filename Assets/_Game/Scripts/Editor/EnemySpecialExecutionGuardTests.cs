using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Synchronous EditMode fixtures. Board requests and readiness callbacks are real;
// coroutine yield boundaries are advanced explicitly, not by Unity's scheduler.
[NonParallelizable]
public sealed class EnemySpecialExecutionGuardTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private GameObject root;
    private EnemyDefinition definition;
    private EnemyActor owner;
    private BoardController board;
    private CourtMageEnemyAbility host;
    private readonly List<object> availability = new List<object>();
    private readonly List<IEnumerator> routines = new List<IEnumerator>();
    private UnityEngine.Random.State randomState;
    private int executions;
    private int mineEvents;
    private int clearRewards;
    private int completedMoves;

    [SetUp]
    public void SetUp()
    {
        executions = mineEvents = clearRewards = completedMoves = 0;
        randomState = UnityEngine.Random.state;
        root = new GameObject("SpecialExecutionFixture");
        root.SetActive(false);
        definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        Set(definition, "hasSpecialAbility", true);
        Set(definition, "allyShieldAmount", 10);
        Set(definition, "selfShieldAmount", 15);
        owner = CreateActor("Owner");
        host = owner.gameObject.AddComponent<CourtMageEnemyAbility>();
        board = Child("Board").AddComponent<BoardController>();
        // Never start the real board/entrance coroutines from these fixtures.
        board.gameObject.SetActive(false);
        // One mineable cell plus protected crystal responses. A 1x1 board has
        // no legal response and must now be rejected by placement safety.
        Set(board, "width", 4);
        Set(board, "height", 4);
        var grid=new Gem[4,4];
        for(int y=0;y<4;y++)for(int x=0;x<4;x++)
        {
            Gem gem=Child("Gem",board.transform).AddComponent<Gem>();
            gem.Initialize(board,x,y,(GemType)((x+y)%6),null,1f);
            if(x!=0||y!=0)gem.SetSpecialType(GemSpecialType.ColorCrystal);
            grid[x,y]=gem;
        }
        Set(board,"gems",grid);
        board.CellMiningStarted += (_, __, ___) => mineEvents++;
        board.BoardClearResolved += _ => clearRewards++;
        board.ValidPlayerMoveCompleted += _ => completedMoves++;
        root.SetActive(true);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (IEnumerator routine in routines) (routine as IDisposable)?.Dispose();
        routines.Clear();
        foreach (object helper in availability) Call(helper, "Dispose");
        availability.Clear();
        if (root != null) UnityEngine.Object.DestroyImmediate(root);
        if (definition != null) UnityEngine.Object.DestroyImmediate(definition);
        UnityEngine.Random.state = randomState;
    }

    [TestCase(false)]
    [TestCase(true)]
    public void DisabledOrInactiveHostCannotExecuteOrConsumeReadyCharge(bool inactiveRoot)
    {
        object helper = MakeAvailability();
        owner.RegisterValidPlayerTurn();
        Assert.That(owner.IsSpecialReady, Is.True);
        SetUnavailable(inactiveRoot);
        Call(helper, "RequestExecution");
        Assert.That(executions, Is.Zero);
        Assert.That(owner.IsSpecialReady, Is.True);
        Assert.That(owner.HasAnimationActionInProgress, Is.False);
        Assert.That(board.HasPendingBoardMutation, Is.False);
    }

    [TestCase(1)]
    [TestCase(5)]
    public void ReenableAllowsOneExecutionOnTheNextLegitimateRequest(int cycles)
    {
        object helper = MakeAvailability();
        for (int i = 0; i < cycles; i++)
        {
            owner.RegisterValidPlayerTurn();
            host.enabled = false;
            Call(helper, "RequestExecution");
            Assert.That(executions, Is.EqualTo(i));
            host.enabled = true;
            Call(helper, "RequestExecution");
            Call(helper, "RequestExecution");
            Assert.That(executions, Is.EqualTo(i + 1));
            Assert.That(owner.IsSpecialReady, Is.False);
        }
    }

    [TestCase("disposed")]
    [TestCase("defeated")]
    [TestCase("destroyedHost")]
    [TestCase("uninitialized")]
    [TestCase("notReady")]
    public void ExistingUnavailableOwnerGuardsStillRejectExecution(string state)
    {
        object helper = MakeAvailability();
        owner.RegisterValidPlayerTurn();
        switch (state)
        {
            case "disposed": Call(helper, "Dispose"); break;
            case "defeated": Assert.That(owner.TryTakeDamageWithoutFeedback(1000), Is.True); break;
            case "destroyedHost": UnityEngine.Object.DestroyImmediate(host); break;
            case "uninitialized": Set(owner, "isInitialized", false); break;
            case "notReady": owner.ResetSpecialCounter(); break;
        }
        Call(helper, "RequestExecution");
        Assert.That(executions, Is.Zero);
    }

    [Test]
    public void AlreadyCreatedDeferredPollDoesNotExecuteAfterHostDisable()
    {
        object helper = MakeAvailability();
        owner.RegisterValidPlayerTurn();
        Set(helper, "wasDeferredByStagger", true);
        IEnumerator poll = Track((IEnumerator)Call(helper, "RetryAfterBoardBecomesIdle"));
        Assert.That(poll.MoveNext(), Is.True, "initial next-frame yield");
        host.enabled = false;
        Assert.That(poll.MoveNext(), Is.False);
        Assert.That(executions, Is.Zero);
        Assert.That(owner.IsSpecialReady, Is.True);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ActualShieldingRuntimeRejectsDisabledReadinessThenResumesOnActionRelease(bool inactiveRoot)
    {
        EnemyActor ally = CreateActor("Ally");
        var shielding = owner.gameObject.AddComponent<ShieldingAlliesEnemyAbility>();
        shielding.InitializeSpecialAbility(owner, board, new List<EnemyActor> { owner, ally });
        int casts = 0;
        owner.SpecialAbilityUsed += _ => casts++;
        if (inactiveRoot) owner.gameObject.SetActive(false);
        else shielding.enabled = false;

        // This is the actor's actual readiness event, not a direct shield call.
        owner.RegisterValidPlayerTurn();
        Assert.That(owner.CurrentShield, Is.Zero);
        Assert.That(ally.CurrentShield, Is.Zero);
        Assert.That(owner.IsSpecialReady, Is.True);
        Assert.That(casts, Is.Zero);

        if (inactiveRoot) owner.gameObject.SetActive(true);
        else shielding.enabled = true;
        Assert.That(owner.TryBeginAutoAttackAnimationAction(), Is.True);
        owner.EndAutoAttackAnimationAction();
        Assert.That(owner.CurrentShield, Is.EqualTo(15));
        Assert.That(ally.CurrentShield, Is.EqualTo(10));
        Assert.That(casts, Is.EqualTo(1));
        Assert.That(owner.IsSpecialReady, Is.False);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void MineRechecksDeadOrDestroyedOwnerAfterImpactWait(bool destroyOwner, bool impactReached)
    {
        int ownerId = owner.GetInstanceID();
        IEnumerator execution = QueueMine(waitForImpact: true);
        Assert.That(execution.MoveNext(), Is.True);
        Assert.That(execution.Current, Is.Null, "still waiting for impact");
        if (impactReached) Assert.That(board.NotifyMineAnimationImpact(owner), Is.True);
        if (destroyOwner) UnityEngine.Object.DestroyImmediate(owner.gameObject);
        else Assert.That(owner.TryTakeDamageWithoutFeedback(1000), Is.True);

        Assert.That(execution.MoveNext(), Is.False, "no posthumous cell mutation");
        Assert.That(board.GetMinedCellCountForOwner(ownerId), Is.Zero);
        Assert.That(board.IsCellPlayable(0, 0), Is.True);
        Assert.That(Get<Gem[,]>(board, "gems")[0, 0], Is.Not.Null);
        Assert.That(mineEvents, Is.Zero);
        AssertNoRewardsOrOwnershipRelease();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void LivingOwnerStillCommitsOneHoleAtTheExistingBoundary(bool waitForImpact)
    {
        int ownerId = owner.GetInstanceID();
        IEnumerator execution = QueueMine(waitForImpact);
        if (waitForImpact)
        {
            Assert.That(execution.MoveNext(), Is.True);
            Assert.That(mineEvents, Is.Zero);
            Assert.That(board.NotifyMineAnimationImpact(owner), Is.True);
            Assert.That(board.NotifyMineAnimationImpact(owner), Is.True);
        }
        Assert.That(execution.MoveNext(), Is.True);
        Assert.That(execution.Current, Is.InstanceOf<IEnumerator>(), "existing ClearMatches continuation");
        Assert.That(board.GetMinedCellCountForOwner(ownerId), Is.EqualTo(1));
        Assert.That(board.IsCellMined(0, 0), Is.True);
        Assert.That(mineEvents, Is.EqualTo(1));
        AssertNoRewardsOrOwnershipRelease();
        // Do not simulate shatter/refill here; the combined Play Mode pass owns it.
    }

    [Test]
    public void AnotherActorsImpactDoesNotReleaseTheWaitingMine()
    {
        IEnumerator execution = QueueMine(waitForImpact: true);
        Assert.That(execution.MoveNext(), Is.True);
        EnemyActor other = CreateActor("Other");
        Assert.That(board.NotifyMineAnimationImpact(other), Is.False);
        Assert.That(execution.MoveNext(), Is.True);
        Assert.That(execution.Current, Is.Null);
        Assert.That(mineEvents, Is.Zero);
        Assert.That(board.NotifyMineAnimationImpact(owner), Is.True);
        Assert.That(execution.MoveNext(), Is.True);
        Assert.That(mineEvents, Is.EqualTo(1));
    }

    private void AssertNoRewardsOrOwnershipRelease()
    {
        Assert.That(clearRewards, Is.Zero);
        Assert.That(completedMoves, Is.Zero);
        Assert.That(Get<bool>(board, "isBusy"), Is.True,
            "the outer processor, not this request, owns unlocking");
    }

    private object MakeAvailability()
    {
        Type type = typeof(EnemyActor).Assembly.GetType("EnemySpecialActionAvailability", throwOnError: true);
        object helper = Activator.CreateInstance(type, new object[]
        {
            host, owner, board, new Func<bool>(() =>
            {
                executions++;
                owner.ResetSpecialCounter();
                return true;
            })
        });
        availability.Add(helper);
        return helper;
    }

    private IEnumerator QueueMine(bool waitForImpact)
    {
        Assert.That(board.TryQueueMineRandomCell(owner, 3, waitForImpact), Is.True);
        object queue = Get<object>(board, "pendingBoardMutations");
        object request = Call(queue, "Dequeue");
        Set(board, "activeBoardMutationRequest", request);
        Set(board, "isBusy", true);
        return Track((IEnumerator)Call(board, "ExecuteMineRequest", request));
    }

    private IEnumerator Track(IEnumerator routine)
    {
        routines.Add(routine);
        return routine;
    }

    private void SetUnavailable(bool inactiveRoot)
    {
        if (inactiveRoot) owner.gameObject.SetActive(false);
        else host.enabled = false;
    }

    private EnemyActor CreateActor(string name)
    {
        EnemyActor actor = Child(name).AddComponent<EnemyActor>();
        // Seed gameplay fields only; Initialize installs unrelated visual
        // presenters. Readiness, shields and death still use actual actor APIs.
        Set(actor, "definition", definition);
        Set(actor, "<RuntimeStats>k__BackingField", new EnemyRuntimeStats(1, 1, 100, 1, 0, 10f, 1));
        Set(actor, "currentHealth", 100);
        Set(actor, "isInitialized", true);
        return actor;
    }

    private GameObject Child(string name, Transform parent = null)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent != null ? parent : root.transform, false);
        return child;
    }

    private static T Get<T>(object target, string name) => (T)Field(target, name).GetValue(target);
    private static void Set(object target, string name, object value) => Field(target, name).SetValue(target, value);
    private static FieldInfo Field(object target, string name) =>
        target.GetType().GetField(name, Flags) ?? throw new MissingFieldException(target.GetType().Name, name);
    private static object Call(object target, string name, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(name, Flags);
        if (method == null) throw new MissingMethodException(target.GetType().Name, name);
        return method.Invoke(target, args);
    }
}
