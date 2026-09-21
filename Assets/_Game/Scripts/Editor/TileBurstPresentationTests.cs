using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[NonParallelizable]
public sealed class TileBurstPresentationTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private GameObject root;
    private BoardController board;
    private Gem[,] grid;
    private readonly List<UnityEngine.Object> assets = new List<UnityEngine.Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("TileBurstFixture");
        board = root.AddComponent<BoardController>();
        grid = new Gem[7, 8];
        Set(board, "width", 7); Set(board, "height", 8); Set(board, "gems", grid);
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 7; x++)
            {
                var go = new GameObject("Gem"); go.transform.SetParent(root.transform, false);
                var gem = go.AddComponent<Gem>();
                gem.Initialize(board, x, y, GemType.Ruby, null, 1f);
                go.transform.localPosition = board.GetCellLocalPosition(x, y);
                grid[x, y] = gem;
            }
    }

    [TearDown]
    public void TearDown()
    {
        if (root != null) UnityEngine.Object.DestroyImmediate(root);
        foreach (var asset in assets) if (asset != null) UnityEngine.Object.DestroyImmediate(asset);
        assets.Clear();
    }

    [TestCase(GemSpecialType.RowBomb, TileBurstKind.Explosion, 7)]
    [TestCase(GemSpecialType.ColumnBomb, TileBurstKind.Explosion, 8)]
    [TestCase(GemSpecialType.PoisonBomb, TileBurstKind.Poison, 9)]
    [TestCase(GemSpecialType.HealingBomb, TileBurstKind.Healing, 9)]
    [TestCase(GemSpecialType.ShieldBomb, TileBurstKind.Shield, 9)]
    public void ActualExpandedClearHasOneCenteredBurstPerDestroyedTile(GemSpecialType special, TileBurstKind kind, int count)
    {
        grid[3, 3].SetSpecialType(special);
        var clear = Expand(grid[3, 3]);
        var before = (Gem[,])grid.Clone();
        var contexts = Snapshot(clear);
        Assert.That(contexts.Length, Is.EqualTo(1));
        Assert.That(contexts[0].Kind, Is.EqualTo(kind));
        Assert.That(contexts[0].TileCount, Is.EqualTo(count));
        Assert.That(contexts[0].WorldPositions.Distinct().Count(), Is.EqualTo(count));
        CollectionAssert.AreEquivalent(clear.Select(g => root.transform.TransformPoint(board.GetCellLocalPosition(g.Column, g.Row))), contexts[0].WorldPositions);
        CollectionAssert.AreEqual(before, grid, "snapshot cannot change the authoritative grid");
    }

    [Test]
    public void CornerBlastExcludesHoleProtectedCrystalAndPreservedReward()
    {
        grid[0, 0].SetSpecialType(GemSpecialType.PoisonBomb);
        grid[1, 0] = null;
        grid[0, 1].SetSpecialType(GemSpecialType.ColorCrystal);
        var clear = Expand(grid[0, 0]);
        var preserved = new Dictionary<Gem, GemSpecialType> { [grid[1, 1]] = GemSpecialType.RowBomb };
        var contexts = Snapshot(clear, preserved);
        Assert.That(contexts.Single().TileCount, Is.EqualTo(1));
        Assert.That(contexts[0].WorldPositions[0], Is.EqualTo(board.GetCellLocalPosition(0, 0)));
        Assert.That(grid[0, 1].SpecialType, Is.EqualTo(GemSpecialType.ColorCrystal));
    }

    [Test]
    public void OverlappingChainIsStableDeduplicatedAndKeepsSpecialCenterIdentity()
    {
        grid[3, 3].SetSpecialType(GemSpecialType.PoisonBomb);
        grid[4, 3].SetSpecialType(GemSpecialType.ShieldBomb);
        grid[3, 4].SetSpecialType(GemSpecialType.HealingBomb);
        grid[2, 3].SetSpecialType(GemSpecialType.RowBomb);
        var clear = Expand(grid[3, 3]);
        var a = Snapshot(clear);
        var b = Snapshot(new HashSet<Gem>(clear.Reverse()));
        Assert.That(a.SelectMany(c => c.WorldPositions).Distinct().Count(), Is.EqualTo(clear.Count));
        Assert.That(a.Sum(c => c.TileCount), Is.EqualTo(clear.Count));
        for (int i = 0; i < a.Length; i++)
        {
            Assert.That(a[i].Kind, Is.EqualTo(b[i].Kind));
            CollectionAssert.AreEqual(a[i].WorldPositions, b[i].WorldPositions);
        }
        Assert.That(a.Single(c => c.Kind == TileBurstKind.Poison).WorldPositions,
            Does.Contain(board.GetCellLocalPosition(3, 3)));
        Assert.That(a.Single(c => c.Kind == TileBurstKind.Explosion).WorldPositions,
            Does.Contain(board.GetCellLocalPosition(2, 3)));
    }

    [Test]
    public void ConsumableFootprintAndCrackedCollateralUseGenericArtWithSpecialOverrides()
    {
        var targets = new HashSet<Gem>();
        for (int y = 2; y <= 4; y++) for (int x = 2; x <= 4; x++) targets.Add(grid[x, y]);
        var generic = Snapshot(targets, null, targets);
        Assert.That(generic.Single().Kind, Is.EqualTo(TileBurstKind.Explosion));
        Assert.That(generic[0].TileCount, Is.EqualTo(9));
        grid[3, 3].SetSpecialType(GemSpecialType.HealingBomb);
        var mixed = Snapshot(targets, null, targets);
        Assert.That(mixed.Single().Kind, Is.EqualTo(TileBurstKind.Healing));
        Assert.That(mixed[0].TileCount, Is.EqualTo(9));
    }

    [Test]
    public void OrdinaryMatchHasNoBombCueAndSnapshotDoesNotFollowMovedGem()
    {
        var targets = new HashSet<Gem> { grid[2, 2], grid[3, 2], grid[4, 2] };
        Assert.That(Snapshot(targets), Is.Empty);
        root.transform.position = new Vector3(5, 7, 0);
        var snapshot = Snapshot(targets, null, targets).Single();
        Vector3 first = snapshot.WorldPositions[0];
        grid[2, 2].transform.position += Vector3.up * 10f;
        Assert.That(snapshot.WorldPositions[0], Is.EqualTo(first));
        Assert.That(first, Is.EqualTo(root.transform.TransformPoint(board.GetCellLocalPosition(2, 2))));
    }

    [TestCase(true, 1)]
    [TestCase(false, 0)]
    public void CueOccursAtShatterOnlyWhenSpecialActivationIsAllowed(bool activate, int expected)
    {
        grid[3, 3].SetSpecialType(GemSpecialType.RowBomb);
        var clear = Expand(grid[3, 3]);
        Set(board, "matchFlashDuration", 0f); Set(board, "matchWhiteHoldDuration", .1f);
        Set(board, "matchPostBurstDelay", .1f);
        int cues = 0; board.TileBurstVFXRequested += _ => cues++;
        var routine = (IEnumerator)Call("ClearMatches", clear, null, activate);
        Assert.That(cues, Is.Zero);
        Assert.That(routine.MoveNext(), Is.True); // preparation white hold
        Assert.That(cues, Is.Zero);
        Assert.That(routine.MoveNext(), Is.True); // shatter, before post-burst wait
        Assert.That(cues, Is.EqualTo(expected));
        foreach (Gem gem in clear) Assert.That(grid[gem.Column, gem.Row], Is.Null);
        (routine as IDisposable)?.Dispose(); // do not invoke delayed Destroy in EditMode
    }

    [Test]
    public void MissingArtIsOptionalAndDisablingPresenterClearsAllItsRenderers()
    {
        var controller = root.AddComponent<TileBurstVFXController>();
        typeof(TileBurstVFXController).GetMethod("Awake", Flags).Invoke(controller, null);
        typeof(TileBurstVFXController).GetMethod("OnEnable", Flags).Invoke(controller, null);
        Set(controller, "library", null);
        Assert.That(controller.HasSequence(TileBurstKind.Explosion), Is.False);
        var context = new TileBurstVFXContext(TileBurstKind.Explosion, new[] { Vector3.zero, Vector3.right });
        Call("ReportTileBursts", new[] { context });
        Assert.That(root.GetComponentsInChildren<SpriteRenderer>().Count(r => r.name == "TileBurst"), Is.Zero);
        var texture = new Texture2D(64, 64); assets.Add(texture);
        var sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * .5f, 64); assets.Add(sprite);
        var library = ScriptableObject.CreateInstance<TileBurstLibrary>(); assets.Add(library);
        library.sequences = new[] { new TileBurstLibrary.Sequence { kind = TileBurstKind.Explosion,
            frames = new[] { sprite }, durations = new[] { .1f } } };
        Set(controller, "library", library);
        Call("ReportTileBursts", new[] { context });
        var effects = root.GetComponentsInChildren<SpriteRenderer>().Where(r => r.name == "TileBurst").ToArray();
        Assert.That(effects.Length, Is.EqualTo(2));
        foreach (var effect in effects)
        {
            Assert.That(effect.enabled, Is.True);
            Assert.That(effect.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(effect.maskInteraction, Is.EqualTo(SpriteMaskInteraction.VisibleInsideMask));
        }
        controller.enabled = false;
        typeof(TileBurstVFXController).GetMethod("OnDisable", Flags).Invoke(controller, null);
        Assert.That(effects.All(r => !r.enabled), Is.True);
    }

    [Test]
    public void GravityContactCoalescesOnceAndDoesNotCountOrdinaryMoves()
    {
        Type moveType = typeof(BoardController).GetNestedType("GemMove", BindingFlags.NonPublic);
        var moves = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(moveType));
        for (int i = 0; i < 3; i++)
        {
            object move = Activator.CreateInstance(moveType);
            Set(move, "Gem", grid[i, 0]);
            Set(move, "Duration", i < 2 ? 0f : .2f);
            Set(move, "UseGravityMotion", i < 2);
            moves.Add(move);
        }
        int cues = 0, count = 0;
        board.GemsLanded += n => { cues++; count += n; };
        var routine = (IEnumerator)Call("AnimateGemMoves", moves);
        Assert.That(routine.MoveNext(), Is.True);
        Assert.That(cues, Is.EqualTo(1));
        Assert.That(count, Is.EqualTo(2));
        routine.MoveNext();
        Assert.That(cues, Is.EqualTo(1), "the same landing cannot emit twice on subsequent rendered frames");
        (routine as IDisposable)?.Dispose();
    }

    [Test]
    public void ImportedLibraryContainsAllFourCenteredNativeFamilies()
    {
        var library = Resources.Load<TileBurstLibrary>("VFX/TileBursts");
        Assert.That(library, Is.Not.Null, "Run TileBurstArtImporter.Run after exporting the native sheets.");
        foreach (TileBurstKind kind in Enum.GetValues(typeof(TileBurstKind)))
        {
            var sequence = library.Find(kind);
            Assert.That(sequence, Is.Not.Null, kind + " has a complete frame/timing sequence");
            foreach (var sprite in sequence.frames)
            {
                Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(64, 64)));
                Assert.That(sprite.pivot, Is.EqualTo(new Vector2(32, 32)));
                Assert.That(sprite.pixelsPerUnit, Is.EqualTo(64));
                Assert.That(sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(sprite.texture.mipmapCount, Is.EqualTo(1));
                Assert.That(sprite.vertices.Length, Is.EqualTo(4));
            }
        }
    }

    private HashSet<Gem> Expand(Gem seed)
    {
        var method = typeof(BoardController).GetMethods(Flags).Single(m => m.Name == "BuildBombExpandedClearSet" && m.GetParameters().Length == 3);
        return (HashSet<Gem>)method.Invoke(board, new object[] { new HashSet<Gem> { seed }, true, null });
    }
    private TileBurstVFXContext[] Snapshot(HashSet<Gem> clear, Dictionary<Gem, GemSpecialType> preserved = null, HashSet<Gem> generic = null)
        => (TileBurstVFXContext[])Call("BuildTileBurstContexts", clear, preserved ?? new Dictionary<Gem, GemSpecialType>(), generic);
    private object Call(string name, params object[] args) => typeof(BoardController).GetMethod(name, Flags).Invoke(board, args);
    private static void Set(object target, string name, object value) => target.GetType().GetField(name, Flags).SetValue(target, value);
}
