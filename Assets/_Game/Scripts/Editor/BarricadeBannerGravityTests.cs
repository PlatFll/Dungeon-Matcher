using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Tests the authoritative opening accounting without running scene/presentation
// coroutines. Real clear timing, falling, refill and aura removal remain Play
// Mode acceptance cases in BARRICADE_BANNER_GRAVITY_AUDIT.md.
public sealed class BarricadeBannerGravityTests
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private readonly List<GameObject> objects = new List<GameObject>();
    private BoardController board;
    private Gem[,] grid;
    private IDictionary barricades;
    private IDictionary banners;

    [SetUp]
    public void SetUp()
    {
        Assert.That(RunUpgradeRuntime.Current, Is.Null,
            "Use an isolated EditMode run without a live run-upgrade singleton.");
        board = CreateObject("BarricadeBannerFixture").AddComponent<BoardController>();
        grid = new Gem[board.Width, board.Height];
        Write(board, "gems", grid);
        barricades = (IDictionary)Read(board, "barricadeCells");
        banners = (IDictionary)Read(board, "royalBannerCells");
    }

    [TearDown]
    public void TearDown()
    {
        // Fixture gems are not bound to BoardController, so destroying them
        // cannot inject unrelated physical-clear notifications during teardown.
        for (int i = objects.Count - 1; i >= 0; i--)
            if (objects[i] != null) UnityEngine.Object.DestroyImmediate(objects[i]);
        objects.Clear();
    }

    [Test]
    public void SideClearBreakingBarricadeQueuesOpeningWithoutMovingDuringFlash()
    {
        object banner = Banner(2, 5);
        Block(2, 3);
        Gem side = GemAt(1, 3); // No destroyed gem in the banner's column.
        int clearReports = 0;
        board.BoardClearResolved += _ => clearReports++;
        board.BoardClearOutcomeResolved += _ => clearReports++;

        Hit(side);
        Assert.That(board.IsCellBarricaded(2, 3), Is.False);
        Assert.That(board.IsCellPlayable(2, 3), Is.True);
        Assert.That(Steps(banner), Is.EqualTo(1));
        Call("Update"); // Simulate any Update during ClearMatches' flash.
        Assert.That((Vector2Int)Read(banner, "Cell"), Is.EqualTo(new Vector2Int(2, 5)));
        Assert.That(Steps(banner), Is.EqualTo(1));
        Assert.That(Read(board, "royalBannerClearBatchReady"), Is.EqualTo(false));

        board.NotifyGemDestroyedForRoyalBanners(side.Column, side.Row);
        Assert.That(Read(board, "royalBannerClearBatchReady"), Is.EqualTo(true));
        Assert.That(Steps(banner), Is.EqualTo(1));
        Assert.That(clearReports, Is.Zero, "Opening accounting must not award a gem clear.");
    }

    [Test]
    public void SurvivingStoneBarricadeDoesNotCreateAnOpening()
    {
        object banner = Banner(2, 5);
        object stone = Block(2, 3, 2);
        Hit(GemAt(1, 3));
        Assert.That(Read(stone, "RemainingDurability"), Is.EqualTo(1));
        Assert.That(board.IsCellBarricaded(2, 3), Is.True);
        Assert.That(Steps(banner), Is.Zero);
    }

    [Test]
    public void SecondDistinctHitCreatesExactlyOneStoneOpening()
    {
        object banner = Banner(2, 5);
        Block(2, 3, 2);
        Gem side = GemAt(1, 3);
        Hit(side);
        Assert.That(Steps(banner), Is.Zero);
        Hit(side);
        Assert.That(board.IsCellBarricaded(2, 3), Is.False);
        Assert.That(Steps(banner), Is.EqualTo(1));
    }

    [Test]
    public void SeveralAdjacentGemsCountOneOpeningAndIgnoredSeedsCountNone()
    {
        object banner = Banner(2, 5);
        Block(2, 3);
        var seeds = new HashSet<Gem> { GemAt(1, 3), GemAt(3, 3), GemAt(2, 2) };
        Call("DamageBarricadesAdjacentToClears", seeds, new HashSet<Gem>(seeds));
        Assert.That(board.IsCellBarricaded(2, 3), Is.True);
        Assert.That(Steps(banner), Is.Zero);
        Call("DamageBarricadesAdjacentToClears", seeds, null);
        Assert.That(Steps(banner), Is.EqualTo(1));
    }

    [Test]
    public void RemovedBarricadeCannotQueueItsOpeningAgain()
    {
        object banner = Banner(2, 5);
        Block(2, 3);
        Gem side = GemAt(1, 3);
        Hit(side);
        Hit(side);
        Assert.That(Steps(banner), Is.EqualTo(1));
        Assert.That(barricades.Count, Is.Zero);
    }

    [Test]
    public void BarricadesAndDestroyedGemJoinSameBatchBeforeAnyBannerMoves()
    {
        object banner = Banner(2, 6);
        Block(2, 1);
        Block(2, 3);
        Gem between = GemAt(2, 2);
        Hit(between);
        Assert.That(Steps(banner), Is.EqualTo(2));
        Call("Update");
        Assert.That((Vector2Int)Read(banner, "Cell"), Is.EqualTo(new Vector2Int(2, 6)));
        Assert.That(Steps(banner), Is.EqualTo(2));

        grid[2, 2] = null;
        board.NotifyGemDestroyedForRoyalBanners(2, 2);
        Assert.That(Steps(banner), Is.EqualTo(3));
        Assert.That(Read(board, "royalBannerClearBatchReady"), Is.EqualTo(true));
    }

    [TestCase(2, 6)]
    [TestCase(4, 3)]
    public void OpeningAboveBannerOrInAnotherColumnDoesNotMoveIt(int x, int y)
    {
        object banner = Banner(2, 5);
        Block(x, y);
        Hit(GemAt(x - 1, y));
        Assert.That(board.IsCellBarricaded(x, y), Is.False);
        Assert.That(Steps(banner), Is.Zero);
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public void OrphaningEitherObstaclePreservesGravityInteraction(bool orphanBanner, bool orphanBarricade)
    {
        object banner = Banner(2, 5);
        Block(2, 3);
        if (orphanBanner) board.OrphanRoyalBannerForOwner(101);
        if (orphanBarricade) board.OrphanBarricadesForOwner(202);
        Assert.That(board.IsCellRoyalBanner(2, 5), Is.True);
        Assert.That(board.IsCellBarricaded(2, 3), Is.True);
        Hit(GemAt(1, 3));
        Assert.That(Steps(banner), Is.EqualTo(1));
    }

    [Test]
    public void OneOpeningIsCountedForEachStandardAboveIt()
    {
        object lower = Banner(2, 3);
        object upper = Banner(2, 6);
        Block(2, 1);
        Hit(GemAt(1, 1));
        Assert.That(Steps(lower), Is.EqualTo(1));
        Assert.That(Steps(upper), Is.EqualTo(1));
        Assert.That(banners.Count, Is.EqualTo(2));
    }

    [Test]
    public void BarricadeBreakWithoutStandardsKeepsOrdinaryRemovalBehavior()
    {
        Block(2, 3);
        Gem side = GemAt(1, 3);
        Hit(side);
        Assert.That(board.IsCellBarricaded(2, 3), Is.False);
        Assert.That(board.IsCellPlayable(2, 3), Is.True);
        board.NotifyGemDestroyedForRoyalBanners(side.Column, side.Row);
        Call("Update");
        Assert.That(banners.Count, Is.Zero);
        Assert.That(Read(board, "royalBannerClearBatchReady"), Is.EqualTo(false));
    }

    private GameObject CreateObject(string name)
    {
        var value = new GameObject(name);
        value.SetActive(false);
        objects.Add(value);
        return value;
    }

    private Gem GemAt(int x, int y)
    {
        Gem gem = CreateObject("ClearSeed").AddComponent<Gem>();
        gem.SetGridPosition(x, y);
        grid[x, y] = gem;
        return gem;
    }

    private object Banner(int x, int y)
    {
        object state = NewState("RoyalBannerState");
        Write(state, "Cell", new Vector2Int(x, y));
        Write(state, "OwnerInstanceId", 101);
        Write(state, "BannerId", banners.Count + 1);
        banners.Add(new Vector2Int(x, y), state);
        return state;
    }

    private object Block(int x, int y, int durability = 1)
    {
        object state = NewState("BarricadeCellState");
        Write(state, "OwnerInstanceId", 202);
        Write(state, "RemainingDurability", durability);
        Write(state, "MaximumDurability", durability);
        barricades.Add(new Vector2Int(x, y), state);
        return state;
    }

    private void Hit(params Gem[] gems) =>
        Call("DamageBarricadesAdjacentToClears", new HashSet<Gem>(gems), null);
    private static int Steps(object state) => (int)Read(state, "PendingGravitySteps");
    private static object NewState(string name)
    {
        Type type = typeof(BoardController).GetNestedType(name, BindingFlags.NonPublic);
        Assert.That(type, Is.Not.Null, name);
        return Activator.CreateInstance(type, true);
    }
    private static FieldInfo Field(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, Fields);
        Assert.That(field, Is.Not.Null, name);
        return field;
    }
    private static object Read(object target, string name) => Field(target, name).GetValue(target);
    private static void Write(object target, string name, object value) => Field(target, name).SetValue(target, value);
    private object Call(string name, params object[] args)
    {
        MethodInfo method = typeof(BoardController).GetMethod(name, Fields);
        Assert.That(method, Is.Not.Null, name);
        return method.Invoke(board, args);
    }
}
