using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class CrystalBoardValidityTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private readonly List<GameObject> roots = new List<GameObject>();
    private UnityEngine.Random.State randomState;
    private BoardController board;
    private Gem[,] grid;

    [SetUp]
    public void SetUp() => randomState = UnityEngine.Random.state;

    [TearDown]
    public void TearDown()
    {
        for (int i = roots.Count - 1; i >= 0; i--)
            if (roots[i] != null) UnityEngine.Object.DestroyImmediate(roots[i]);
        roots.Clear();
        UnityEngine.Random.state = randomState;
        board = null;
        grid = null;
    }

    [Test]
    public void PinnedCrystalDoesNotManufactureAnOrdinaryMove()
    {
        MakeBoard("RRS", "TMR"); // Rows listed bottom first.
        grid[0, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        Pins[grid[0, 0]] = 1;

        // R* R S / T M R looks like a right-column swap would make RRR,
        // but the pinned crystal is colorless in the real match collector.
        Assert.That(ActualMove(2, 0, 2, 1), Is.False);
        Assert.That(LiveHasMove(), Is.False);
        Assert.That(board.TryGetRandomHintMove(out _, out _), Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void InaccessibleCrystalDoesNotUseItsHiddenColor(bool pendingReservations)
    {
        MakeBoard("RRS", "TMR");
        grid[0, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        var pins = pendingReservations
            ? (Dictionary<Gem, int>)Get("pendingPinTargetOwners")
            : Pins;
        pins[grid[1, 0]] = 1;
        pins[grid[0, 1]] = 2;
        Assert.That(board.IsGemPinned(grid[0, 0]), Is.False);
        Assert.That(LiveHasMove(), Is.False);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void PinnedOrdinaryColorsStillParticipateInMatches(int pinKind)
    {
        MakeBoard("RRS", "TMR");
        Pins[grid[0, 0]] = 1;
        if (pinKind == 1) ((HashSet<Gem>)Get("movablePinnedGems")).Add(grid[0, 0]);
        if (pinKind == 2) ((HashSet<Gem>)Get("frozenPinnedGems")).Add(grid[0, 0]);
        Assert.That(ActualMove(2, 0, 2, 1), Is.True);
        Assert.That(LiveHasMove(), Is.True);
    }

    [TestCase(GemSpecialType.None)]
    [TestCase(GemSpecialType.RowBomb)]
    [TestCase(GemSpecialType.ColorCrystal)]
    public void CrystalSwapIsLegalEvenWhenHiddenColorsAreEqual(GemSpecialType partner)
    {
        MakeBoard("RR");
        grid[0, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        grid[1, 0].SetSpecialType(partner);
        Assert.That(LiveHasMove(), Is.True);
        Assert.That(board.IsHintMoveStillValid(grid[0, 0], grid[1, 0]), Is.True);
        Assert.That((bool)Call("HasAnyMatchesInSnapshot", Types(), Crystals()), Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ACrystalSwapCannotMoveEitherPinnedEndpoint(bool pinCrystal)
    {
        MakeBoard("RS");
        grid[0, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        Pins[grid[pinCrystal ? 0 : 1, 0]] = 1;
        Assert.That(LiveHasMove(), Is.False);
    }

    [TestCase("minedCellOwners")]
    [TestCase("barricadeCells")]
    [TestCase("royalBannerCells")]
    public void BlockedCellsAreNotCrystalSwapPartners(string obstacleField)
    {
        MakeBoard("RST");
        grid[0, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        BlockCell(obstacleField, 1, 0);
        Assert.That(LiveHasMove(), Is.False);
        Assert.That(board.TryGetRandomHintMove(out _, out _), Is.False);
    }

    [Test]
    public void PinSafetyCannotSealTheLastCrystalExitUsingAPhantomMatch()
    {
        MakeBoard("RRS", "TMR");
        grid[0, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        Pins[grid[0, 1]] = 1;
        Assert.That(LiveHasMove(), Is.True);

        List<Gem> safe = (List<Gem>)Call("BuildSafePinnableGemList");
        Assert.That(safe, Does.Not.Contain(grid[1, 0]));
        Assert.That(Pins.Count, Is.EqualTo(1), "Simulation must restore temporary pin state.");
        Assert.That(LiveHasMove(), Is.True);
    }

    [Test]
    public void CandidateCrystalMaskUsesDestinationPositionsAndSkipsBlockedCells()
    {
        MakeBoard("RST");
        grid[0, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        BlockCell("minedCellOwners", 1, 0);
        Gem crystal = grid[0, 0];
        var candidate = new List<Gem> { grid[2, 0], crystal };
        bool[,] mask = (bool[,])Call("BuildCrystalGrid", candidate);
        Assert.That(mask[0, 0], Is.False);
        Assert.That(mask[1, 0], Is.False);
        Assert.That(mask[2, 0], Is.True);
        Assert.That(crystal.Column, Is.EqualTo(0), "Inspecting a candidate must not move a live gem.");
        Assert.That(grid[0, 0], Is.SameAs(crystal));
    }

    [Test]
    public void AHiddenCrystalTripleIsNotAnExistingMatch()
    {
        MakeBoard("RRR");
        grid[1, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        Assert.That(((HashSet<Gem>)Call("FindAllMatches")).Count, Is.Zero);
        Assert.That((bool)Call("HasAnyMatchesInSnapshot", Types(), Crystals()), Is.False);
        Assert.That((bool)Call("HasAnyMatches", (object)Types()), Is.True,
            "The compatibility helper still describes a hypothetical all-colored grid.");
    }

    [Test]
    public void ARealOrdinaryTripleStillRejectsTheReshuffleCandidate()
    {
        MakeBoard("RRR");
        Set("maximumReshuffleAttempts", 1);
        Assert.That((bool)Call("HasAnyMatchesInSnapshot", Types(), Crystals()), Is.True);
        object[] arguments = { null };
        Assert.That((bool)Call("TryCreateShuffledLayout", arguments), Is.False);
        Assert.That(arguments[0], Is.Null);
    }

    [Test]
    public void ACrystalOnlyReshuffleMoveKeepsExistingIdentitiesAndColors()
    {
        MakeBoard("RR");
        Gem crystal = grid[0, 0];
        Gem partner = grid[1, 0];
        crystal.SetSpecialType(GemSpecialType.ColorCrystal);
        Set("maximumReshuffleAttempts", 1);
        object[] arguments = { null };
        Assert.That((bool)Call("TryCreateShuffledLayout", arguments), Is.True);
        var candidate = (List<Gem>)arguments[0];
        CollectionAssert.AreEquivalent(new[] { crystal, partner }, candidate);
        Assert.That(crystal.SpecialType, Is.EqualTo(GemSpecialType.ColorCrystal));
        Assert.That(crystal.Type, Is.EqualTo(GemType.Ruby));
        Assert.That(partner.Type, Is.EqualTo(GemType.Ruby));
        Assert.That(grid[0, 0], Is.SameAs(crystal));
        Assert.That(grid[1, 0], Is.SameAs(partner));
    }

    [Test]
    public void RecolorFallbackCanValidateACrystalOnlyMove()
    {
        MakeBoard("RS");
        grid[0, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        bool[,] mask = (bool[,])Call("BuildCrystalGrid", new List<Gem> { grid[1, 0], grid[0, 0] });
        GemType[,] generated = (GemType[,])Call("GeneratePlayableTypeGridInSnapshot", mask);
        Assert.That((bool)Call("HasAvailableMoveInSnapshot", generated, mask), Is.True);
        Assert.That((bool)Call("HasAnyMatchesInSnapshot", generated, mask), Is.False);
        foreach (GemType type in generated) Assert.That(Enum.IsDefined(typeof(GemType), type), Is.True);
        Assert.That(grid[0, 0].Column, Is.Zero);
        Assert.That(grid[0, 0].Type, Is.EqualTo(GemType.Ruby));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ProspectiveSwapRestoresTheTypeSnapshot(bool withCrystal)
    {
        MakeBoard("RRS", "TMR");
        if (withCrystal) grid[0, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        GemType[,] types = Types();
        GemType[,] before = (GemType[,])types.Clone();
        bool[,] crystals = Crystals();
        bool[,] beforeCrystals = (bool[,])crystals.Clone();
        bool legal = (bool)Call("SwapCreatesMoveInSnapshot", types, crystals, 2, 0, 2, 1);
        Assert.That(legal, Is.EqualTo(!withCrystal));
        CollectionAssert.AreEqual(before, types);
        CollectionAssert.AreEqual(beforeCrystals, crystals);
        Assert.That(grid[2, 0].Type, Is.EqualTo(GemType.Sapphire));
        Assert.That(grid[2, 1].Type, Is.EqualTo(GemType.Ruby));
    }

    [Test]
    public void LiveMoveQueryAgreesWithActualCollectionAndHintsAcrossSeededFixtures()
    {
        MakeBoard("RST", "TEM", "MRA");
        var random = new System.Random(47181);
        for (int sample = 0; sample < 64; sample++)
        {
            Pins.Clear();
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                {
                    Gem gem = grid[x, y];
                    gem.SetSpecialType(GemSpecialType.None);
                    GemType type;
                    do { type = (GemType)random.Next(6); }
                    while ((x >= 2 && grid[x - 1, y].Type == type && grid[x - 2, y].Type == type) ||
                           (y >= 2 && grid[x, y - 1].Type == type && grid[x, y - 2].Type == type));
                    gem.SetType(type, null);
                    if (random.Next(5) == 0) gem.SetSpecialType(GemSpecialType.ColorCrystal);
                    if (random.Next(4) == 0) Pins[gem] = 1;
                }

            bool expected = false;
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                {
                    if (x + 1 < 3) expected |= ActualMove(x, y, x + 1, y);
                    if (y + 1 < 3) expected |= ActualMove(x, y, x, y + 1);
                }
            Assert.That(LiveHasMove(), Is.EqualTo(expected), "fixture " + sample);
            Assert.That(board.TryGetRandomHintMove(out _, out _), Is.EqualTo(expected), "hint fixture " + sample);
        }
    }

    [Test]
    public void ReadOnlyInspectionDoesNotEmitClearRewards()
    {
        MakeBoard("RRS", "TMR");
        grid[0, 0].SetSpecialType(GemSpecialType.ColorCrystal);
        int clears = 0, outcomes = 0;
        board.BoardClearResolved += _ => clears++;
        board.BoardClearOutcomeResolved += _ => outcomes++;
        for (int i = 0; i < 5; i++)
        {
            Assert.That(LiveHasMove(), Is.True);
            Call("BuildSafePinnableGemList");
            object[] arguments = { null };
            Call("TryCreateShuffledLayout", arguments);
        }
        Assert.That(clears, Is.Zero);
        Assert.That(outcomes, Is.Zero);
        Assert.That(board.IsBusy, Is.False);
        Assert.That(grid[0, 0].SpecialType, Is.EqualTo(GemSpecialType.ColorCrystal));
    }

    private Dictionary<Gem, int> Pins => (Dictionary<Gem, int>)Get("pinnedGemOwners");
    private GemType[,] Types() => (GemType[,])Call("BuildCurrentTypeGrid");
    private bool[,] Crystals() => (bool[,])Call("BuildCurrentCrystalGrid");
    private bool LiveHasMove() => (bool)Call("HasAvailableMove");

    private void MakeBoard(params string[] rows)
    {
        var root = new GameObject("CrystalBoardValidityFixture");
        root.SetActive(false); // No gameplay Start, coroutines or live scene mutation.
        roots.Add(root);
        board = root.AddComponent<BoardController>();
        int width = rows[0].Length, height = rows.Length;
        Set("width", width);
        Set("height", height);
        grid = new Gem[width, height];
        Set("gems", grid);
        Set("gemSprites", new Sprite[6]);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var go = new GameObject("GemFixture");
                go.SetActive(false);
                go.transform.SetParent(root.transform, false);
                Gem gem = go.AddComponent<Gem>();
                GemType type;
                switch (rows[y][x])
                {
                    case 'R': type = GemType.Ruby; break;
                    case 'A': type = GemType.Amber; break;
                    case 'T': type = GemType.Topaz; break;
                    case 'E': type = GemType.Emerald; break;
                    case 'S': type = GemType.Sapphire; break;
                    case 'M': type = GemType.Amethyst; break;
                    default: throw new InvalidOperationException("Unknown fixture gem.");
                }
                gem.Initialize(board, x, y, type, null, 1f);
                grid[x, y] = gem;
            }
    }

    private void BlockCell(string field, int x, int y)
    {
        IDictionary obstacles = (IDictionary)Get(field);
        Type valueType = obstacles.GetType().GetGenericArguments()[1];
        obstacles[new Vector2Int(x, y)] = valueType == typeof(int)
            ? (object)1 : Activator.CreateInstance(valueType, true);
        grid[x, y] = null;
    }

    private bool ActualMove(int x1, int y1, int x2, int y2)
    {
        Gem first = grid[x1, y1], second = grid[x2, y2];
        if (first == null || second == null || board.IsGemPinned(first) || board.IsGemPinned(second) ||
            !board.IsCellPlayable(x1, y1) || !board.IsCellPlayable(x2, y2)) return false;
        if (first.SpecialType == GemSpecialType.ColorCrystal || second.SpecialType == GemSpecialType.ColorCrystal)
            return true;
        if (first.Type == second.Type) return false;
        grid[x1, y1] = second; grid[x2, y2] = first;
        first.SetGridPosition(x2, y2); second.SetGridPosition(x1, y1);
        try { return ((HashSet<Gem>)Call("FindMatchesFrom", first, second)).Count > 0; }
        finally
        {
            grid[x1, y1] = first; grid[x2, y2] = second;
            first.SetGridPosition(x1, y1); second.SetGridPosition(x2, y2);
        }
    }

    private object Get(string name)
    {
        FieldInfo field = typeof(BoardController).GetField(name, Flags);
        if (field == null) throw new MissingFieldException(typeof(BoardController).Name, name);
        return field.GetValue(board);
    }
    private void Set(string name, object value)
    {
        FieldInfo field = typeof(BoardController).GetField(name, Flags);
        if (field == null) throw new MissingFieldException(typeof(BoardController).Name, name);
        field.SetValue(board, value);
    }
    private object Call(string name, params object[] arguments)
    {
        foreach (MethodInfo method in typeof(BoardController).GetMethods(Flags))
            if (method.Name == name && method.GetParameters().Length == arguments.Length)
                return method.Invoke(board, arguments);
        throw new MissingMethodException(typeof(BoardController).Name, name);
    }
}
