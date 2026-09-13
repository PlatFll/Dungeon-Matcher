using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode queue/lifecycle fixtures, not timed reshuffle or pin-art tests.
// Inactive objects prevent automatic scene startup. No Resources, preferences,
// live run singleton, authored scene or clock is changed; RNG is restored.
[NonParallelizable]
public sealed class ReshufflePinReservationTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private GameObject root;
    private BoardController board;
    private Gem[,] grid;
    private EnemyActor firstOwner, secondOwner;
    private UnityEngine.Random.State randomState;
    private int clearReports, outcomeReports, moveReports;

    [SetUp]
    public void SetUp()
    {
        randomState = UnityEngine.Random.state;
        clearReports = outcomeReports = moveReports = 0;
        root = new GameObject("ReshufflePinReservationFixture");
        root.SetActive(false);
        board = Child("Board").AddComponent<BoardController>();
        grid = new Gem[7, 8];
        Set(board, "width", 7);
        Set(board, "height", 8);
        Set(board, "gems", grid);
        Set(board, "reshufflePause", 0.25f);
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 7; x++)
            {
                Gem gem = Child("Gem").AddComponent<Gem>();
                gem.Initialize(board, x, y, (GemType)((x + 2 * y) % 6), null, 1f);
                grid[x, y] = gem;
            }
        // A real ordinary swap at (1,0)/(1,1) makes Ruby at (0,0)..(2,0).
        grid[0, 0].SetType(GemType.Ruby, null);
        grid[1, 0].SetType(GemType.Sapphire, null);
        grid[2, 0].SetType(GemType.Ruby, null);
        grid[1, 1].SetType(GemType.Ruby, null);
        firstOwner = Owner("FirstOwner");
        secondOwner = Owner("SecondOwner");
        board.BoardClearResolved += _ => clearReports++;
        board.BoardClearOutcomeResolved += _ => outcomeReports++;
        board.ValidPlayerMoveCompleted += _ => moveReports++;
    }

    [TearDown]
    public void TearDown()
    {
        if (root != null) UnityEngine.Object.DestroyImmediate(root);
        UnityEngine.Random.state = randomState;
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ReleasedReservationCannotExecuteOrChangeItsOldTarget(bool frozen)
    {
        object request = Reserve(firstOwner, frozen);
        Gem target = Get<Gem>(request, "TargetGem");
        int completed = 0;
        Set(request, "Completed", new Action<bool>(success =>
        {
            Assert.That(success, Is.False);
            completed++;
        }));
        Call(board, "ReleaseAllPinsForEmergencyReshuffle");
        Assert.That(Get<Gem>(request, "TargetGem"), Is.Null);
        Assert.That(board.IsGemPinned(target), Is.False);
        Assert.That(board.IsGemFrozen(target), Is.False);
        Assert.That(board.HasPendingBoardMutation, Is.True, "normal queue drain still owns completion");

        // The same object can survive reshuffling and later become a special.
        // The invalid request must no longer be able to pin either version.
        if (frozen) target.SetSpecialType(GemSpecialType.ColorCrystal);
        GemSpecialType retainedType = target.SpecialType;
        DrainCancelledRequests();
        Assert.That(completed, Is.EqualTo(1));
        Assert.That(board.IsGemPinned(target), Is.False);
        Assert.That(board.IsGemFrozen(target), Is.False);
        Assert.That(target.SpecialType, Is.EqualTo(retainedType));
        Assert.That(target.GetComponent<PinnedGemOverlayView>(), Is.Null);
        Assert.That(target.GetComponent<FrozenGemOverlayView>(), Is.Null);
        Assert.That(board.GetPinnedGemCountForOwner(firstOwner.GetInstanceID()), Is.Zero);
        Assert.That(board.IsBusy, Is.False);
        AssertNoRewards();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ActualReshuffleEntryCancelsBeforeItsFirstAnimationWait(bool frozen)
    {
        object request = Reserve(firstOwner, frozen);
        Set(board, "isBusy", true);
        IEnumerator reshuffle = (IEnumerator)Call(board, "ReshuffleBoard");
        try
        {
            Assert.That(reshuffle.MoveNext(), Is.True);
            Assert.That(reshuffle.Current, Is.InstanceOf<WaitForSeconds>());
            Assert.That(Get<Gem>(request, "TargetGem"), Is.Null);
            Assert.That(Get<bool>(board, "isBusy"), Is.True, "cancellation cannot release the accepted resolution");
            Assert.That(Get<Dictionary<Gem, int>>(board, "pendingPinTargetOwners"), Is.Empty);
            Assert.That(Get<HashSet<Gem>>(board, "pendingFrozenPinTargets"), Is.Empty);
            AssertNoRewards();
        }
        finally { (reshuffle as IDisposable)?.Dispose(); }
        // Layout generation, animation and the outer resolution are deliberately
        // not driven by this boundary test; they require final Play Mode tests.
    }

    [Test]
    public void BothOwnersAreCancelledEvenWhenNoPinHasMaterializedYet()
    {
        object bolt = Reserve(firstOwner, false);
        object freeze = Reserve(secondOwner, true);
        Assert.That(Get<Dictionary<Gem, int>>(board, "pinnedGemOwners"), Is.Empty);
        Call(board, "ReleaseAllPinsForEmergencyReshuffle");
        Assert.That(Get<Gem>(bolt, "TargetGem"), Is.Null);
        Assert.That(Get<Gem>(freeze, "TargetGem"), Is.Null);
        Assert.That(board.GetPinnedGemCountForOwner(firstOwner.GetInstanceID()), Is.Zero);
        Assert.That(board.GetFrozenGemCountForOwner(secondOwner.GetInstanceID()), Is.Zero);
        Assert.That(Requests().Length, Is.EqualTo(2), "do not discard queue entries or callbacks");
        DrainCancelledRequests();
        Assert.That(board.HasPendingBoardMutation, Is.False);
        AssertNoRewards();
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void ExistingBoltChainAndFreezeReleaseRulesArePreserved(int kind)
    {
        Gem applied = grid[6, 7];
        Get<Dictionary<Gem, int>>(board, "pinnedGemOwners")[applied] = firstOwner.GetInstanceID();
        if (kind == 1) Get<HashSet<Gem>>(board, "movablePinnedGems").Add(applied);
        if (kind == 2) Get<HashSet<Gem>>(board, "frozenPinnedGems").Add(applied);
        object pending = Reserve(secondOwner, true);
        Call(board, "ReleaseAllPinsForEmergencyReshuffle");
        Assert.That(Get<Gem>(pending, "TargetGem"), Is.Null);
        Assert.That(board.IsGemPinned(applied), Is.False);
        Assert.That(board.IsGemFrozen(applied), Is.False);
        Assert.That(Get<HashSet<Gem>>(board, "movablePinnedGems"), Is.Empty);
        Assert.That(Get<Dictionary<Gem, int>>(board, "pinnedGemOwners"), Is.Empty);
        Assert.That(grid[6, 7], Is.SameAs(applied), "release does not remove the gem");
        AssertNoRewards();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void NewLegalReservationSurvivesExecutionOfAnOlderCancelledRequest(bool frozen)
    {
        object old = Reserve(firstOwner, frozen);
        Call(board, "ReleaseAllPinsForEmergencyReshuffle");
        object fresh = Reserve(firstOwner, frozen);
        Gem target = Get<Gem>(fresh, "TargetGem");
        IEnumerator stale = (IEnumerator)Call(board, "ExecutePinRequest", old);
        try { Assert.That(stale.MoveNext(), Is.False); }
        finally { (stale as IDisposable)?.Dispose(); }
        Assert.That(Get<Gem>(fresh, "TargetGem"), Is.SameAs(target));
        Assert.That(board.IsGemPinned(target), Is.True);
        Assert.That(board.IsGemFrozen(target), Is.EqualTo(frozen));
        Assert.That(board.GetPinnedGemCountForOwner(firstOwner.GetInstanceID()), Is.EqualTo(1));
        AssertNoRewards();
        // This checks fresh reservation integrity, not successful visual impact.
    }

    [Test]
    public void ExecutionTimeTopUpAndOtherQueuedMutationsAreNotCancelled()
    {
        object pin = Reserve(firstOwner, true);
        Assert.That(board.TryQueueTopUpMovablePins(secondOwner, 3, _ => { }, () => false), Is.True);
        Assert.That(board.TryQueueMineRandomCell(secondOwner, 3), Is.True);
        board.QueueRestoreMinedCells(secondOwner.GetInstanceID());
        object[] before = Requests();
        Assert.That(before.Length, Is.EqualTo(4));
        Call(board, "ReleaseAllPinsForEmergencyReshuffle");
        CollectionAssert.AreEqual(before, Requests(), "preserve FIFO order and request identity");
        Assert.That(Get<Gem>(pin, "TargetGem"), Is.Null);
        Assert.That(Get<bool>(before[1], "MovablePin"), Is.True);
        Assert.That(Get<Func<bool>>(before[1], "IsCancelled")(), Is.False);
        Assert.That(Get<EnemyActor>(before[2], "OwnerActor"), Is.SameAs(secondOwner));
        Assert.That(Get<HashSet<int>>(board, "pendingRestoreOwners"), Does.Contain(secondOwner.GetInstanceID()));
        Assert.That(board.HasPendingBoardMutation, Is.True);
        AssertNoRewards();
    }

    [Test]
    public void RepeatedEmptyReleaseDoesNotMutateGemsOrGrantRewards()
    {
        var before = (Gem[,])grid.Clone();
        for (int i = 0; i < 5; i++) Call(board, "ReleaseAllPinsForEmergencyReshuffle");
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 7; x++) Assert.That(grid[x, y], Is.SameAs(before[x, y]));
        Assert.That(board.HasPendingBoardMutation, Is.False);
        Assert.That(board.IsBusy, Is.False);
        AssertNoRewards();
    }

    private object Reserve(EnemyActor owner, bool frozen)
    {
        bool accepted = frozen ? board.TryQueueFreezeRandomGem(owner, 3) : board.TryQueuePinRandomGem(owner, 2);
        Assert.That(accepted, Is.True, "fixture must offer a safe ordinary target");
        object[] pending = Requests();
        object request = pending[pending.Length - 1];
        Gem target = Get<Gem>(request, "TargetGem");
        Assert.That(target, Is.Not.Null);
        Assert.That(board.IsGemPinned(target), Is.True);
        Assert.That(board.IsGemFrozen(target), Is.EqualTo(frozen));
        return request;
    }

    private void DrainCancelledRequests()
    {
        var steps = new Stack<IEnumerator>();
        steps.Push((IEnumerator)Call(board, "ProcessBoardMutations"));
        try
        {
            for (int budget = 100; steps.Count > 0; budget--)
            {
                Assert.That(budget, Is.GreaterThan(0), "cancelled requests must not stall");
                IEnumerator current = steps.Peek();
                if (!current.MoveNext())
                {
                    steps.Pop();
                    (current as IDisposable)?.Dispose();
                }
                else if (current.Current is IEnumerator child) steps.Push(child);
                else Assert.That(current.Current, Is.Null, "cancelled requests must not start presentation waits");
            }
        }
        finally
        {
            while (steps.Count > 0) (steps.Pop() as IDisposable)?.Dispose();
        }
    }

    private object[] Requests()
    {
        var items = new List<object>();
        foreach (object request in Get<IEnumerable>(board, "pendingBoardMutations")) items.Add(request);
        return items.ToArray();
    }

    private void AssertNoRewards()
    {
        Assert.That(clearReports, Is.Zero);
        Assert.That(outcomeReports, Is.Zero);
        Assert.That(moveReports, Is.Zero);
    }

    private EnemyActor Owner(string name)
    {
        EnemyActor actor = Child(name).AddComponent<EnemyActor>();
        Set(actor, "isInitialized", true);
        return actor;
    }

    private GameObject Child(string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(root.transform, false);
        return child;
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
        foreach (MethodInfo method in target.GetType().GetMethods(Flags))
            if (method.Name == name && method.GetParameters().Length == args.Length)
                return method.Invoke(target, args);
        throw new MissingMethodException(target.GetType().Name, name);
    }
}
