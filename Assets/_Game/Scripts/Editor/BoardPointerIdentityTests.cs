using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

// EditMode handler tests. We deliver PointerEventData to the real Gem handlers,
// not through a hardware device/raycaster. Inactive objects avoid scene startup.
// Outward swipes intentionally have no neighbor: no swap/cascade runs here.
[NonParallelizable]
public sealed class BoardPointerIdentityTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    private GameObject root;
    private BoardController board;
    private EventSystem eventSystem;
    private Gem first, other;
    private int clearEvents, moveEvents;
    private static readonly Vector2 Origin = new Vector2(100f, 100f);

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("BoardPointerIdentityFixture");
        root.SetActive(false);
        eventSystem = Child("FixtureEventSystem").AddComponent<EventSystem>();
        board = Child("Board").AddComponent<BoardController>();
        var grid = new Gem[7, 8];
        Set(board, "gems", grid);
        first = CreateGem(0, 0);
        other = CreateGem(5, 5); // Nonadjacent selection never starts a swap.
        grid[0, 0] = first;
        grid[5, 5] = other;
        clearEvents = moveEvents = 0;
        board.BoardClearResolved += _ => clearEvents++;
        board.ValidPlayerMoveCompleted += _ => moveEvents++;
    }

    [TearDown]
    public void TearDown()
    {
        if (root != null) UnityEngine.Object.DestroyImmediate(root);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void OlderReleaseCannotCancelLatestContact(bool sameGem)
    {
        Gem latest = sameGem ? first : other;
        first.OnPointerDown(Event(11, Origin));
        latest.OnPointerDown(Event(22, Origin + Vector2.right * 10f));
        first.OnPointerUp(Event(11, Origin));
        Assert.That(Get<Gem>(board, "pointerStartGem"), Is.SameAs(latest));
        latest.OnPointerUp(Event(22, Origin + Vector2.right * 10f));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(latest));
        Assert.That(Get<Gem>(board, "pointerStartGem"), Is.Null);
        AssertNoResolution();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void OlderDragCannotUseLatestContactsOrigin(bool sameGem)
    {
        Gem latest = sameGem ? first : other;
        Vector2 latestOrigin = Origin + Vector2.right * 100f;
        first.OnPointerDown(Event(11, Origin));
        latest.OnPointerDown(Event(22, latestOrigin));
        first.OnDrag(Event(11, Origin - Vector2.right * 100f));
        Assert.That(Get<Gem>(board, "pointerStartGem"), Is.SameAs(latest));
        Assert.That(Get<Vector2>(board, "pointerStartPosition"), Is.EqualTo(latestOrigin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.Null);
        latest.OnPointerUp(Event(22, latestOrigin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(latest));
        AssertNoResolution();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void LatestAcceptedDownRetainsExistingTakeoverAndOwnTapOrigin(bool sameGem)
    {
        Gem latest = sameGem ? first : other;
        Vector2 latestOrigin = Origin + Vector2.right * 100f;
        first.OnPointerDown(Event(11, Origin));
        latest.OnPointerDown(Event(22, latestOrigin));
        Assert.That(Get<Gem>(board, "pointerStartGem"), Is.SameAs(latest));
        Assert.That(Get<int>(board, "gesturePointerId"), Is.EqualTo(22));
        Assert.That(Get<Vector2>(board, "pointerStartPosition"), Is.EqualTo(latestOrigin));
        latest.OnPointerUp(Event(22, latestOrigin));
        first.OnPointerUp(Event(11, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(latest));
        AssertNoResolution();
    }

    [Test]
    public void OwningOutwardDragConsumesOnceAndUpDoesNotBecomeATap()
    {
        first.OnPointerDown(Event(22, Origin));
        first.OnDrag(Event(22, Origin + Vector2.left * 100f));
        Assert.That(Get<Gem>(board, "pointerStartGem"), Is.Null);
        first.OnDrag(Event(22, Origin + Vector2.left * 110f));
        first.OnPointerUp(Event(22, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.Null);
        AssertNoResolution();
    }

    [Test]
    public void SubthresholdOwningDragRetainsTheGesture()
    {
        first.OnPointerDown(Event(22, Origin));
        first.OnDrag(Event(22, Origin + Vector2.left * 5f));
        Assert.That(Get<Gem>(board, "pointerStartGem"), Is.SameAs(first));
        first.OnPointerUp(Event(22, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(first));
        AssertNoResolution();
    }

    [Test]
    public void BusyRejectedDownDoesNotStealExistingPointerIdentity()
    {
        first.OnPointerDown(Event(11, Origin));
        Set(board, "isBusy", true);
        first.OnPointerDown(Event(22, Origin));
        Assert.That(Get<int>(board, "gesturePointerId"), Is.EqualTo(11));
        first.OnPointerUp(Event(22, Origin));
        Assert.That(Get<Gem>(board, "pointerStartGem"), Is.SameAs(first));
        Assert.That(board.IsBusy, Is.True, "another finger cannot release board ownership");
        Set(board, "isBusy", false);
        first.OnPointerUp(Event(11, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(first));
        AssertNoResolution();
    }

    [Test]
    public void PinnedRejectedDownDoesNotReplaceAnotherGemsOwner()
    {
        first.OnPointerDown(Event(11, Origin));
        Get<Dictionary<Gem, int>>(board, "pinnedGemOwners")[other] = 123;
        other.OnPointerDown(Event(22, Origin));
        other.OnPointerUp(Event(22, Origin));
        Assert.That(Get<Gem>(board, "pointerStartGem"), Is.SameAs(first));
        Assert.That(Get<int>(board, "gesturePointerId"), Is.EqualTo(11));
        first.OnPointerUp(Event(11, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(first));
        AssertNoResolution();
    }

    [Test]
    public void ExternalBlockInvalidatesGestureWithoutReplayingItAfterRelease()
    {
        first.OnPointerDown(Event(11, Origin));
        using (board.AcquireExternalInputBlock())
        {
            Assert.That(Get<Gem>(board, "pointerStartGem"), Is.Null);
            other.OnPointerDown(Event(22, Origin));
            Assert.That(Get<Gem>(board, "pointerStartGem"), Is.Null);
            Assert.That(board.IsExternalInputBlocked, Is.True);
        }
        first.OnPointerUp(Event(11, Origin));
        other.OnPointerUp(Event(22, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.Null);
        other.OnPointerDown(Event(33, Origin));
        other.OnPointerUp(Event(33, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(other));
        AssertNoResolution();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void OwnerCancellationOrPinInvalidatesItsRemainingEvents(bool pinOwner)
    {
        first.OnPointerDown(Event(11, Origin));
        if (pinOwner)
        {
            Get<Dictionary<Gem, int>>(board, "pinnedGemOwners")[first] = 123;
            first.OnDrag(Event(11, Origin)); // Real Gem pin guard cancels it.
            Get<Dictionary<Gem, int>>(board, "pinnedGemOwners").Remove(first);
        }
        else board.CancelPointerInteraction(first);
        first.OnDrag(Event(11, Origin + Vector2.left * 100f));
        first.OnPointerUp(Event(11, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.Null);
        first.OnPointerDown(Event(33, Origin));
        first.OnPointerUp(Event(33, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(first));
        AssertNoResolution();
    }

    [TestCase(-1)]
    [TestCase(31)]
    public void PointerIdentityDoesNotAssumePositiveTouchIdsOnly(int id)
    {
        first.OnPointerDown(Event(id, Origin));
        first.OnPointerUp(Event(id, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(first));
        first.OnPointerUp(Event(id, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(first), "duplicate up must not toggle selection");
        AssertNoResolution();
    }

    [Test]
    public void DragAndUpWithoutAnAcceptedDownCannotSelectOrResolve()
    {
        first.OnDrag(Event(11, Origin + Vector2.left * 100f));
        first.OnPointerUp(Event(11, Origin));
        Assert.That(Get<Gem>(board, "selectedGem"), Is.Null);
        Assert.That(Get<Gem>(board, "pointerStartGem"), Is.Null);
        AssertNoResolution();
    }

    [Test]
    public void FiveReplacementsDoNotLetOldUpsChangeTheCurrentSelection()
    {
        for (int i = 0; i < 5; i++)
        {
            int oldId = 100 + i * 2, latestId = oldId + 1;
            first.OnPointerDown(Event(oldId, Origin));
            other.OnPointerDown(Event(latestId, Origin));
            first.OnPointerUp(Event(oldId, Origin));
            Assert.That(Get<Gem>(board, "pointerStartGem"), Is.SameAs(other));
            other.OnPointerUp(Event(latestId, Origin));
            Gem selected = Get<Gem>(board, "selectedGem");
            first.OnPointerUp(Event(oldId, Origin));
            Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(selected));
        }
        AssertNoResolution();
    }

    [Test]
    public void ExistingDirectInputHelpersRemainAvailableForSinglePointerFixtures()
    {
        board.BeginPointer(first, Origin);
        board.UpdatePointerDrag(first, Origin + Vector2.left * 5f);
        board.EndPointer(first, Origin);
        Assert.That(Get<Gem>(board, "selectedGem"), Is.SameAs(first));
        AssertNoResolution();
    }

    private void AssertNoResolution()
    {
        Assert.That(board.IsBusy, Is.False);
        Assert.That(clearEvents, Is.Zero);
        Assert.That(moveEvents, Is.Zero);
    }

    private PointerEventData Event(int id, Vector2 position) =>
        new PointerEventData(eventSystem) { pointerId = id, position = position };

    private Gem CreateGem(int x, int y)
    {
        Gem gem = Child("FixtureGem").AddComponent<Gem>();
        gem.Initialize(board, x, y, GemType.Ruby, null, 1f);
        return gem;
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
}
