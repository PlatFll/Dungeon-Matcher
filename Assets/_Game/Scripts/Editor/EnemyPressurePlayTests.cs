using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator PressureThresholdsIncreaseByRankAndHitsDoNotMoveSprites()
    {
        yield return LaunchUnified(); DurableUnifiedFixture();
        var enemy = Run.Waves.ActiveEnemies[0];
        var original = enemy.Definition;
        var copy = UnityEngine.Object.Instantiate(original);
        Set(enemy, "definition", copy);
        try
        {
            var stagger = enemy.GetComponent<EnemyStagger>();
            var expected = new[] { .60f, .70f, .80f, .90f };
            for (int rank = 0; rank < expected.Length; rank++)
            {
                Set(copy, "category", (EnemyCategory)rank);
                Assert.That(stagger.DamageThreshold, Is.EqualTo(enemy.MaxHealth * expected[rank]).Within(.01f));
            }
            var feedback = enemy.GetComponent<EnemyCombatFeedback>();
            var root = (RectTransform)Get(feedback, "visualRoot");
            Vector2 position = root.anchoredPosition;
            enemy.ResolveDirectDamage(20);
            Assert.That(stagger.IsStaggered, Is.False);
            Assert.That((float)Get(feedback, "hitFlashUntil"), Is.GreaterThan(Time.time));
            for (float until = Time.time + .6f; Time.time < until;)
            {
                Assert.That(root.anchoredPosition, Is.EqualTo(position), "hit feedback cannot translate the sprite");
                yield return null;
            }
        }
        finally { Set(enemy, "definition", original); UnityEngine.Object.Destroy(copy); }
    }

    [UnityTest] public IEnumerator PressureSpecialReadinessAndImmunitySurviveContinue()
    {
        yield return LaunchUnified(); DurableUnifiedFixture();
        var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_ShieldKnight.asset");
        Assert.That(Run.Waves.TrySummonEnemy(definition, out var knight), Is.True);
        yield return Stable(); DurableUnifiedFixture();
        Assert.That(knight.SpecialTurnRequirement, Is.EqualTo(4), "six-move shield charge becomes four");
        int casts = 0; knight.SpecialAbilityUsed += _ => casts++;
        for (int move = 0; move < 3; move++) { yield return Move(); Assert.That(casts, Is.Zero); }
        yield return Move(); Assert.That(casts, Is.EqualTo(1));
        var target = Run.Waves.ActiveEnemies[0];
        var stagger = target.GetComponent<EnemyStagger>();
        stagger.ApplyStagger(1, 1);
        yield return Move(); Assert.That(stagger.RemainingImmunityTime, Is.EqualTo(5));
        yield return Move(); Assert.That(stagger.RemainingImmunityTime, Is.EqualTo(4));
        Assert.That(Run.SuspendToMenu(), Is.True); yield return null;
        SceneManager.LoadScene("Game");
        yield return Until(() => Run?.Continuation != null && !Run.Continuation.IsRestoring, "pressure Continue");
        Run.GetComponent<RunControlsUI>().Close(); yield return Stable();
        stagger = Run.Waves.ActiveEnemies[0].GetComponent<EnemyStagger>();
        Assert.That(CombatMoveClock.RevisedEnemyPressure, Is.True);
        Assert.That(Run.Board.SpecialPairSwapsEnabled, Is.True);
        Assert.That(stagger.RemainingImmunityTime, Is.EqualTo(4));
        Assert.That(stagger.ApplyStagger(1, 1), Is.Zero);
    }

    [UnityTest] public IEnumerator PressureLegacyUnifiedSaveRetainsOldRules()
    {
        yield return LaunchUnified(); DurableUnifiedFixture();
        ((CombatClockSnapshot)Get(Run.MoveClock, "state")).profile = CombatClockSnapshot.LegacyUnifiedProfile;
        Run.Board.SpecialPairSwapsEnabled = false;
        var enemy = Run.Waves.ActiveEnemies[0];
        var stagger = enemy.GetComponent<EnemyStagger>();
        Set(enemy.GetComponent<EnemyAutoAttack>(), "unifiedRemaining", 4);
        Assert.That(stagger.DamageThreshold, Is.EqualTo(enemy.MaxHealth * .30f).Within(.01f));
        Assert.That(stagger.ApplyStagger(1, 1), Is.EqualTo(2));
        Assert.That(Run.SuspendToMenu(), Is.True); yield return null;
        SceneManager.LoadScene("Game");
        yield return Until(() => Run?.Continuation != null && !Run.Continuation.IsRestoring, "legacy unified Continue");
        Run.GetComponent<RunControlsUI>().Close(); yield return Stable(); DurableUnifiedFixture();
        enemy = Run.Waves.ActiveEnemies[0]; stagger = enemy.GetComponent<EnemyStagger>();
        Assert.That(CombatMoveClock.Unified, Is.True);
        Assert.That(CombatMoveClock.RevisedEnemyPressure, Is.False);
        Assert.That(Run.Board.SpecialPairSwapsEnabled, Is.False);
        Assert.That(enemy.GetComponent<EnemyAutoAttack>().EffectiveMoveInterval, Is.EqualTo(4));
        yield return Move(); yield return Move();
        Assert.That(stagger.RemainingImmunityTime, Is.EqualTo(2));
    }

    [UnityTest] public IEnumerator PressureEveryBombPairActivatesOnceAtSwappedOrigins()
    {
        yield return LaunchUnified(); DurableUnifiedFixture();
        Time.timeScale = 6;
        var board = Run.Board;
        // Exercise all pairs on the same full board; crumbling has its own tests.
        Set(board, "nextCrumbleMove", int.MaxValue);
        var sprites = (Sprite[])Get(board, "gemSprites");
        var kinds = new[] { GemSpecialType.RowBomb, GemSpecialType.ColumnBomb, GemSpecialType.PoisonBomb,
            GemSpecialType.HealingBomb, GemSpecialType.ShieldBomb };
        foreach (var firstKind in kinds) foreach (var secondKind in kinds)
        {
            for (int x = 0; x < board.Width; x++) for (int y = 0; y < board.Height; y++)
            {
                var gem = board.GetGem(x, y);
                Assert.That(gem, Is.Not.Null);
                gem.SetSpecialType(GemSpecialType.None);
                gem.SetType((GemType)((x + 2 * y) % 6), sprites[(x + 2 * y) % 6]);
            }
            bool vertical = ((int)firstKind + (int)secondKind) % 2 == 0;
            var a = board.GetGem(2, 3); var b = board.GetGem(vertical ? 2 : 3, vertical ? 4 : 3);
            var aOrigin = new Vector2Int(a.Column, a.Row); var bOrigin = new Vector2Int(b.Column, b.Row);
            a.SetSpecialType(firstKind); b.SetSpecialType(secondKind);
            Vector3 aWorld = a.transform.position, bWorld = b.transform.position;
            var presented = new List<(TileBurstKind kind, Vector3 position)>();
            var lines = new List<BombVFXContext>();
            int accepted = 0, rewarded = 0, detonated = 0; bool landed = false;
            Action<BombVFXContext> line = context =>
            { if (!landed && context.IsDirectionalBomb) lines.Add(context); };
            Action<RunUpgradeSpecialClearContext> detonation = context =>
            { if (!landed) detonated += context.SpecialCount; };
            Action<TileBurstVFXContext> bomb = context =>
            { if (!landed) foreach (var position in context.WorldPositions) presented.Add((context.Kind, position)); };
            Action<int> accept = _ => accepted++;
            Action<int> landing = _ => landed = true;
            Action<BoardClearContext> clear = context => { if (!landed) rewarded += context.GemCount; };
            board.BombVFXRequested += line; board.RunUpgradeSpecialClearPrepared += detonation;
            board.TileBurstVFXRequested += bomb; board.ValidPlayerMoveAccepted += accept;
            board.GemsLanded += landing; board.BoardClearResolved += clear;
            int before = Run.MoveClock.Tick;
            try
            {
                Assert.That(board.PreviewManualSwap(a, b).IsLegal, Is.True);
                board.StartCoroutine((IEnumerator)Call(board, "TrySwap", a, b));
                yield return Until(() => Run.MoveClock.Tick == before + 1 && Run.Continuation.CanCapture, "bomb pair " + firstKind + "/" + secondKind);
                Assert.That(accepted, Is.EqualTo(1));
                Assert.That(detonated, Is.EqualTo(2), "two specials in the initial gameplay clear");
                bool Directional(GemSpecialType kind) => kind == GemSpecialType.RowBomb || kind == GemSpecialType.ColumnBomb;
                void AssertOrigin(GemSpecialType kind, Vector2Int cell, Vector3 position)
                {
                    if (Directional(kind))
                    {
                        Assert.That(lines.Count(c => c.SpecialType == kind && c.HasGridPosition && c.Column == cell.x && c.Row == cell.y
                            && Vector3.Distance(c.WorldPosition, position) < .0001f), Is.EqualTo(1), kind + " travelling blast origin");
                        return;
                    }
                    var material = kind == GemSpecialType.PoisonBomb ? TileBurstKind.Poison :
                        kind == GemSpecialType.HealingBomb ? TileBurstKind.Healing : TileBurstKind.Shield;
                    Assert.That(presented.Count(c => c.kind == material && Vector3.Distance(c.position, position) < .0001f),
                        Is.EqualTo(1), kind + " material at new origin");
                }
                AssertOrigin(firstKind, bOrigin, bWorld); AssertOrigin(secondKind, aOrigin, aWorld);
                Assert.That(lines.Count, Is.EqualTo((Directional(firstKind) ? 1 : 0) + (Directional(secondKind) ? 1 : 0)));
                var utilityCells = new HashSet<Vector2Int>();
                var cells = new HashSet<Vector2Int>();
                Action<GemSpecialType, Vector2Int> footprint = (kind, origin) =>
                {
                    for (int x = 0; x < board.Width; x++) for (int y = 0; y < board.Height; y++)
                        if (kind == GemSpecialType.RowBomb ? y == origin.y : kind == GemSpecialType.ColumnBomb ? x == origin.x :
                            Math.Abs(x - origin.x) <= 1 && Math.Abs(y - origin.y) <= 1)
                        {
                            var cell = new Vector2Int(x, y);
                            cells.Add(cell);
                            if (!Directional(kind)) utilityCells.Add(cell);
                        }
                };
                footprint(firstKind, bOrigin); footprint(secondKind, aOrigin);
                Assert.That(rewarded, Is.EqualTo(cells.Count), "overlap rewards exactly once, without a synthetic match");
                Assert.That(presented.Any(c => c.kind == TileBurstKind.Explosion), Is.False, "directional bombs use their travelling fire");
                Assert.That(presented.Count, Is.EqualTo(utilityCells.Count), "one material burst per utility cell");
                foreach (var cell in utilityCells)
                {
                    Vector3 position = board.transform.TransformPoint(board.GetCellLocalPosition(cell.x, cell.y));
                    Assert.That(presented.Count(c => Vector3.Distance(c.position, position) < .0001f), Is.EqualTo(1), cell.ToString());
                }
                Assert.That(board.IsBusy, Is.False); Assert.That(Run.MoveClock.IsResolving, Is.False);
            }
            finally
            {
                board.BombVFXRequested -= line; board.RunUpgradeSpecialClearPrepared -= detonation;
                board.TileBurstVFXRequested -= bomb; board.ValidPlayerMoveAccepted -= accept;
                board.GemsLanded -= landing; board.BoardClearResolved -= clear;
            }
        }
    }

    [UnityTest] public IEnumerator PressureExtendedSwapActivatesFinalContactAtRotatedOrigins()
    {
        yield return LaunchUnified(); DurableUnifiedFixture(); SlideFixture();
        var board = Run.Board;
        board.ExtraManualSwapStep = () => true;
        var a = board.GetGem(1, 4); var middle = board.GetGem(2, 4); var last = board.GetGem(3, 4);
        a.SetSpecialType(GemSpecialType.PoisonBomb); last.SetSpecialType(GemSpecialType.ShieldBomb);
        Vector3 aDestination = last.transform.position, lastDestination = middle.transform.position;
        int accepted = 0; bool landed = false;
        var bursts = new List<(TileBurstKind kind, Vector3 position)>();
        board.ValidPlayerMoveAccepted += _ =>
        {
            accepted++;
            Assert.That(board.GetGem(3, 4), Is.SameAs(a));
            Assert.That(board.GetGem(1, 4), Is.SameAs(middle));
            Assert.That(board.GetGem(2, 4), Is.SameAs(last));
        };
        board.GemsLanded += _ => landed = true;
        board.TileBurstVFXRequested += context =>
        { if (!landed) foreach (var position in context.WorldPositions) bursts.Add((context.Kind, position)); };
        var plan = board.PreviewManualSwap(a, middle);
        Assert.That(plan.IsExtended && plan.IsLegal, Is.True);
        int before = Run.MoveClock.Tick;
        board.StartCoroutine((IEnumerator)Call(board, "TrySwap", a, middle));
        yield return Until(() => Run.MoveClock.Tick == before + 1 && Run.Continuation.CanCapture, "extended special pair");
        Assert.That(accepted, Is.EqualTo(1));
        Assert.That(bursts.Count(c => c.kind == TileBurstKind.Poison && Vector3.Distance(c.position, aDestination) < .0001f), Is.EqualTo(1));
        Assert.That(bursts.Count(c => c.kind == TileBurstKind.Shield && Vector3.Distance(c.position, lastDestination) < .0001f), Is.EqualTo(1));
    }
}
