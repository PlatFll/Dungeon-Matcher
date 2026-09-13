using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode coverage of production planning methods. No coroutines, PlayerPrefs,
// scene assets or presentation are involved; live detonation still needs Play Mode.
public sealed class CrystalChainAttributionTests
{
    private const int Size = 6;
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private GameObject root;
    private BoardController board;
    private Gem[,] cells;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("CrystalChainAttributionFixture");
        root.SetActive(false);
        board = root.AddComponent<BoardController>();
        cells = new Gem[Size, Size];
        SetField(board, "width", Size);
        SetField(board, "height", Size);
        SetField(board, "gems", cells);

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                var gemObject = new GameObject($"Cell_{x}_{y}");
                gemObject.transform.SetParent(root.transform, false);
                Gem gem = gemObject.AddComponent<Gem>();
                gem.SetGridPosition(x, y);
                cells[x, y] = gem;
                Put(x, y, GemType.Amber, GemSpecialType.None);
            }
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (root != null) UnityEngine.Object.DestroyImmediate(root);
        root = null;
        board = null;
        cells = null;
    }

    [TestCase(GemSpecialType.RowBomb)]
    [TestCase(GemSpecialType.ColumnBomb)]
    [TestCase(GemSpecialType.PoisonBomb)]
    [TestCase(GemSpecialType.HealingBomb)]
    [TestCase(GemSpecialType.ShieldBomb)]
    public void ConvertedCollateralUsesTheBombThatActuallyHitsTheCrystal(GemSpecialType kind)
    {
        ArrangeCollateral(kind, out Gem first, out Gem collateral, out Gem crystal);
        Gem blue = Put(5, 5, GemType.Sapphire, GemSpecialType.None);
        Gem red = Put(4, 5, GemType.Ruby, GemSpecialType.None);
        Gem otherCrystal = Put(5, 4, GemType.Sapphire, GemSpecialType.ColorCrystal);
        var requests = new List<BombTriggeredCrystalRequest>();

        HashSet<Gem> converted = Converted(first, new HashSet<Gem>(), requests);
        HashSet<Gem> ordinary = Expanded(new HashSet<Gem> { first }, true, out var ordinaryRequests);

        Assert.That(converted.SetEquals(ordinary), Is.True, "The attribution fix must not change the blast footprint.");
        Assert.That(converted.Contains(collateral), Is.True);
        Assert.That(converted.Contains(crystal), Is.False, "The crystal waits for its own activation.");
        AssertRequest(requests, crystal, GemType.Sapphire);
        AssertRequest(ordinaryRequests, crystal, GemType.Sapphire);

        var targets = (HashSet<Gem>)Call("BuildBombTriggeredCrystalTargetSet", requests[0]);
        Assert.That(targets.Contains(crystal), Is.True);
        Assert.That(targets.Contains(blue), Is.True, "The later crystal selects the actual trigger color.");
        Assert.That(targets.Contains(red), Is.False, "The sequence root's color must not leak into targeting.");
        Assert.That(targets.Contains(otherCrystal), Is.False, "A different crystal's hidden color is not a target.");
    }

    [TestCase(GemSpecialType.RowBomb)]
    [TestCase(GemSpecialType.ColumnBomb)]
    [TestCase(GemSpecialType.PoisonBomb)]
    [TestCase(GemSpecialType.HealingBomb)]
    [TestCase(GemSpecialType.ShieldBomb)]
    public void DirectConvertedBombStillUsesItsOwnColor(GemSpecialType kind)
    {
        Gem first = Put(2, 2, GemType.Ruby, kind);
        Gem crystal = kind == GemSpecialType.ColumnBomb
            ? Put(2, 4, GemType.Topaz, GemSpecialType.ColorCrystal)
            : Put(kind == GemSpecialType.RowBomb ? 4 : 3, 2, GemType.Topaz, GemSpecialType.ColorCrystal);
        var requests = new List<BombTriggeredCrystalRequest>();

        HashSet<Gem> clear = Converted(first, null, requests);

        Assert.That(clear.Contains(first), Is.True);
        Assert.That(clear.Contains(crystal), Is.False);
        AssertRequest(requests, crystal, GemType.Ruby);
    }

    [Test]
    public void PendingConvertedBombRemainsProtectedUntilItsOwnTurn()
    {
        Gem first = Put(0, 0, GemType.Ruby, GemSpecialType.RowBomb);
        Put(2, 0, GemType.Sapphire, GemSpecialType.ColumnBomb);
        Gem crystal = Put(2, 3, GemType.Topaz, GemSpecialType.ColorCrystal);
        Gem waiting = Put(4, 0, GemType.Ruby, GemSpecialType.ColumnBomb);
        Gem underWaiting = cells[4, 5];
        // Keeping the root in this set also exercises the existing root exemption.
        var pending = new HashSet<Gem> { first, waiting };
        var requests = new List<BombTriggeredCrystalRequest>();

        HashSet<Gem> firstClear = Converted(first, pending, requests);

        Assert.That(firstClear.Contains(first), Is.True);
        Assert.That(firstClear.Contains(waiting), Is.False);
        Assert.That(firstClear.Contains(underWaiting), Is.False, "A protected bomb must not expand early.");
        Assert.That(pending.SetEquals(new[] { first, waiting }), Is.True, "Planning must not consume the caller's queue.");
        AssertRequest(requests, crystal, GemType.Sapphire);

        var laterRequests = new List<BombTriggeredCrystalRequest>();
        HashSet<Gem> laterClear = Converted(waiting, new HashSet<Gem>(), laterRequests);
        Assert.That(laterClear.Contains(waiting), Is.True);
        Assert.That(laterClear.Contains(underWaiting), Is.True, "The waiting bomb still expands on its own turn.");
    }

    [Test]
    public void OverlappingCollateralQueuesOneCrystalRequestAndPlanningChangesNoState()
    {
        Gem first = Put(0, 0, GemType.Ruby, GemSpecialType.RowBomb);
        Put(2, 0, GemType.Sapphire, GemSpecialType.PoisonBomb);
        Put(3, 0, GemType.Emerald, GemSpecialType.ShieldBomb);
        Gem crystal = Put(2, 1, GemType.Topaz, GemSpecialType.ColorCrystal);
        var requests = new List<BombTriggeredCrystalRequest>();
        var originalTypes = new GemType[Size, Size];
        var originalSpecials = new GemSpecialType[Size, Size];
        var originalCells = (Gem[,])cells.Clone();
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                originalTypes[x, y] = cells[x, y].Type;
                originalSpecials[x, y] = cells[x, y].SpecialType;
            }

        int clearReports = 0;
        int outcomes = 0;
        Action<BoardClearContext> onClear = _ => clearReports++;
        Action<BoardClearOutcome> onOutcome = _ => outcomes++;
        board.BoardClearResolved += onClear;
        board.BoardClearOutcomeResolved += onOutcome;
        try
        {
            HashSet<Gem> firstPlan = Converted(first, new HashSet<Gem>(), requests);
            HashSet<Gem> secondPlan = Converted(first, new HashSet<Gem>(), requests);
            Assert.That(firstPlan.SetEquals(secondPlan), Is.True);
            AssertRequest(requests, crystal, GemType.Sapphire);
            Assert.That(clearReports, Is.Zero);
            Assert.That(outcomes, Is.Zero);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Assert.That(cells[x, y], Is.SameAs(originalCells[x, y]));
                    Assert.That(cells[x, y].Column, Is.EqualTo(x));
                    Assert.That(cells[x, y].Row, Is.EqualTo(y));
                    Assert.That(cells[x, y].Type, Is.EqualTo(originalTypes[x, y]));
                    Assert.That(cells[x, y].SpecialType, Is.EqualTo(originalSpecials[x, y]));
                }
        }
        finally
        {
            board.BoardClearResolved -= onClear;
            board.BoardClearOutcomeResolved -= onOutcome;
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void SimultaneousSeedsChooseTheSameTriggerInGridOrder(bool reverseInsertion)
    {
        Gem earlier = Put(3, 0, GemType.Sapphire, GemSpecialType.ColumnBomb);
        Gem later = Put(0, 1, GemType.Ruby, GemSpecialType.RowBomb);
        Gem crystal = Put(3, 1, GemType.Topaz, GemSpecialType.ColorCrystal);
        var seeds = reverseInsertion
            ? new HashSet<Gem> { later, null, earlier }
            : new HashSet<Gem> { earlier, null, later };

        HashSet<Gem> clear = Expanded(seeds, true, out var requests);

        AssertRequest(requests, crystal, GemType.Sapphire);
        Assert.That(clear.Contains(crystal), Is.False);
        Assert.That(seeds.Count, Is.EqualTo(3), "Seed sorting must not alter the caller's set.");
    }

    [Test]
    public void NonPreservingExpansionAndNullSeedsRetainTheirExistingSemantics()
    {
        Gem first = Put(0, 0, GemType.Ruby, GemSpecialType.RowBomb);
        Gem crystal = Put(2, 0, GemType.Topaz, GemSpecialType.ColorCrystal);
        HashSet<Gem> clear = Expanded(new HashSet<Gem> { first }, false, out var requests);
        Assert.That(clear.Contains(crystal), Is.True);
        Assert.That(requests, Is.Empty);

        Assert.That(Expanded(null, true, out requests), Is.Empty);
        Assert.That(requests, Is.Empty);
        Assert.That(Converted(null, null, requests), Is.Empty);
    }

    private void ArrangeCollateral(GemSpecialType kind, out Gem first, out Gem collateral, out Gem crystal)
    {
        if (kind == GemSpecialType.RowBomb)
        {
            first = Put(0, 0, GemType.Ruby, GemSpecialType.ColumnBomb);
            collateral = Put(0, 3, GemType.Sapphire, kind);
            crystal = Put(3, 3, GemType.Topaz, GemSpecialType.ColorCrystal);
        }
        else
        {
            first = Put(0, 0, GemType.Ruby, GemSpecialType.RowBomb);
            collateral = Put(3, 0, GemType.Sapphire, kind);
            crystal = Put(3, kind == GemSpecialType.ColumnBomb ? 3 : 1,
                GemType.Topaz, GemSpecialType.ColorCrystal);
        }
    }

    private Gem Put(int x, int y, GemType type, GemSpecialType special)
    {
        Gem gem = cells[x, y];
        // Populate identity/type data without constructing optional bomb art.
        // This intentionally tests planning only, not Gem presentation setup.
        SetField(gem, "<Type>k__BackingField", type);
        SetField(gem, "<SpecialType>k__BackingField", special);
        return gem;
    }

    private HashSet<Gem> Converted(Gem first, HashSet<Gem> pending, List<BombTriggeredCrystalRequest> requests)
    {
        return (HashSet<Gem>)Call("BuildConvertedCrystalBombActivationSet", first, pending, requests);
    }

    private HashSet<Gem> Expanded(HashSet<Gem> seeds, bool preserve, out List<BombTriggeredCrystalRequest> requests)
    {
        object[] args = { seeds, preserve, null };
        var result = (HashSet<Gem>)Call("BuildBombExpandedClearSet", args);
        requests = (List<BombTriggeredCrystalRequest>)args[2];
        return result;
    }

    private static void AssertRequest(List<BombTriggeredCrystalRequest> requests, Gem crystal, GemType color)
    {
        Assert.That(requests.Count, Is.EqualTo(1));
        Assert.That(requests[0].IsValid, Is.True);
        Assert.That(requests[0].CrystalGem, Is.SameAs(crystal));
        Assert.That(requests[0].TriggerGemType, Is.EqualTo(color));
    }

    private object Call(string name, params object[] args)
    {
        foreach (MethodInfo method in typeof(BoardController).GetMethods(Flags))
            if (method.Name == name && method.GetParameters().Length == args.Length)
                return method.Invoke(board, args);
        throw new MissingMethodException(typeof(BoardController).Name, name);
    }

    private static void SetField(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(name, Flags);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        field.SetValue(target, value);
    }
}
