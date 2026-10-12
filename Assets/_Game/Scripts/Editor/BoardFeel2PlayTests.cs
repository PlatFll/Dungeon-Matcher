using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator BoardFeel2MatchImpactSharesShatterFrameAndMutedAudioKeepsHaptic()
    {
        yield return LaunchUnified(); DurableUnifiedFixture(); PrepareSafeMove();
        bool muted = AudioPreferences.SfxMuted, vibration = AudioPreferences.VibrationMuted;
        var audio = Run.Board.GetComponent<CombatAudioController>();
        Set(audio, "focused", true);
        AudioPreferences.SetSfxMuted(true); AudioPreferences.SetVibrationMuted(false);
        int lastVfxFrame = -1, impacts = 0, played = 0;
        long lastId = 0;
        Run.Board.GemMatchVFXRequested += _ => lastVfxFrame = Time.frameCount;
        Run.Board.FeedbackPresented += receipt =>
        {
            Assert.That(receipt.Id, Is.GreaterThan(lastId)); lastId = receipt.Id;
            if (receipt.Kind != BoardFeedbackKind.MatchImpact) return;
            impacts++; Assert.That(lastVfxFrame, Is.EqualTo(Time.frameCount));
            Assert.That(Run.Board.IsBusy, Is.True);
            Assert.That(receipt.Count, Is.GreaterThanOrEqualTo(3));
        };
        audio.CuePlayed += _ => played++;
        int hapticBefore = MobileHaptics.DispatchAttempts;
        try
        {
            yield return Move();
            Assert.That(impacts, Is.GreaterThan(0)); Assert.That(played, Is.Zero);
            Assert.That(MobileHaptics.DispatchAttempts, Is.GreaterThan(hapticBefore));
            Assert.That(MobileHaptics.LastDispatched, Is.EqualTo(HapticStrength.Tick));
        }
        finally { AudioPreferences.SetSfxMuted(muted); AudioPreferences.SetVibrationMuted(vibration); }
    }

    [UnityTest] public IEnumerator BoardFeel2InvalidSwapReturnsWithoutRewardOrCombatOpportunity()
    {
        yield return LaunchUnified(); PrepareSafeMove();
        var board = Run.Board;
        string before = JsonUtility.ToJson(board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot));
        int clears = 0, accepted = 0; var receipts = new List<BoardFeedbackKind>();
        board.BoardClearResolved += _ => clears++;
        board.ValidPlayerMoveAccepted += _ => accepted++;
        board.FeedbackPresented += r => receipts.Add(r.Kind);
        board.StartCoroutine((IEnumerator)Call(board, "TrySwap", board.GetGem(0, 0), board.GetGem(1, 0)));
        yield return Until(() => !board.IsBusy && Run.Continuation.CanCapture, "invalid return");
        Assert.That(accepted, Is.Zero); Assert.That(clears, Is.Zero); Assert.That(Run.MoveClock.Tick, Is.Zero);
        Assert.That(JsonUtility.ToJson(board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot)), Is.EqualTo(before));
        CollectionAssert.AreEqual(new[] { BoardFeedbackKind.Swap, BoardFeedbackKind.InvalidSwap }, receipts);
    }

    [UnityTest] public IEnumerator BoardFeel2ProfilesProduceIdenticalResultsAndMeasuredPhases()
    {
        yield return LaunchUnified(); PrepareSafeMove();
        Assert.That(Run.Continuation.SaveNow(), Is.True);
        string initial = File.ReadAllText(path);
        RunCombatSnapshot expected = null;
        float oldSeconds = 0;
        string output = Path.GetFullPath(".utmp/BoardFeel2"); Directory.CreateDirectory(output);
        try
        {
            foreach (bool legacy in new[] { true, false })
            {
                if (!legacy)
                {
                    yield return ReloadBoardFeelFixture(initial);
                }
                DurableUnifiedFixture();
                var board = Run.Board;
                var original = board.BeforePlayerSwap;
                board.BeforePlayerSwap = (a, b) =>
                {
                    bool accepted = original(a, b);
                    Set(board, "<ReplayLegacyBoardFeel>k__BackingField", legacy);
                    return accepted;
                };
                var metrics = board.gameObject.AddComponent<BoardFeelMetrics>();
                Time.captureDeltaTime = 1f / 60f;
                float start = Time.time;
                // Fixture positions and SavedRandom are identical across profiles.
                yield return Move();
                float duration = Time.time - start;
                var result = Run.Continuation.Capture();
                File.WriteAllText(Path.Combine(output, legacy ? "profile-a.csv" : "profile-b.csv"), metrics.ExportCsv());
                if (legacy) { expected = result; oldSeconds = duration; }
                else
                {
                    Assert.That(JsonUtility.ToJson(result.board), Is.EqualTo(JsonUtility.ToJson(expected.board)));
                    Assert.That(JsonUtility.ToJson(result.player), Is.EqualTo(JsonUtility.ToJson(expected.player)));
                    Assert.That(result.enemies.Select(e => e.health).ToArray(), Is.EqualTo(expected.enemies.Select(e => e.health).ToArray()));
                    Assert.That(result.clock.actions.completed, Is.EqualTo(expected.clock.actions.completed));
                    Assert.That(duration, Is.LessThan(oldSeconds), "fixed 60Hz ordinary path is shorter");
                    File.WriteAllText(Path.Combine(output, "timing.txt"),
                        $"Fixed 60Hz fixture: A={oldSeconds:F3}s B={duration:F3}s; board/player/health/action outcomes equal.\n");
                }
            }
        }
        finally { Time.captureDeltaTime = 0; }
    }

    private IEnumerator ReloadBoardFeelFixture(string json)
    {
        SceneManager.LoadScene("MainMenu"); yield return null;
        File.WriteAllText(path, json); profile.Dispose(); profile = AccountProgression.UseDisposableProfile(path);
        SceneManager.LoadScene("Game"); yield return null;
        yield return Until(() => Run?.Continuation != null && !Run.Continuation.IsRestoring, "board feel continuation");
        Assert.That(Run.Continuation.Error, Is.Null);
        Run.GetComponent<RunControlsUI>().Close(); yield return Stable();
    }

    [UnityTest] public IEnumerator BoardFeel2MissingOptionalPresentationStillResolvesAndDispatchesHaptic()
    {
        yield return LaunchUnified();
        var board = Run.Board;
        var audio = board.GetComponent<CombatAudioController>();
        Set(audio, "focused", true);
        ((System.Collections.IDictionary)Get(audio, "variants")).Clear();
        board.GetComponent<GemBreakAudioController>().enabled = false;
        board.GetComponent<GemMatchVFXController>().enabled = false;
        bool vibration = AudioPreferences.VibrationMuted;
        try
        {
            AudioPreferences.SetVibrationMuted(false);
            int before = MobileHaptics.DispatchAttempts;
            yield return Move();
            Assert.That(Run.MoveClock.Tick, Is.EqualTo(1));
            Assert.That(MobileHaptics.DispatchAttempts, Is.GreaterThan(before));
        }
        finally { AudioPreferences.SetVibrationMuted(vibration); }
    }

    [UnityTest] public IEnumerator BoardFeel2CreationAndPointerPresentationCaptures()
    {
        yield return LaunchUnified(); DurableUnifiedFixture(); PrepareSafeMove();
        var board = Run.Board;
        var source = board.GetGem(safeMoveFrom.x, safeMoveFrom.y);
        var extra = board.GetGem(safeMoveTo.x + 1, safeMoveTo.y);
        extra.SetType(source.Type, ((Sprite[])Get(board, "gemSprites"))[(int)source.Type]);
        string output = Path.GetFullPath(".utmp/BoardFeel2"); Directory.CreateDirectory(output);
        board.BeginPointerGesture(source, new Vector2(100, 100), 77);
        yield return null;
        ScreenCapture.CaptureScreenshot(Path.Combine(output, "pointer.png"));
        yield return null; yield return null;
        board.CancelPointerInteraction(source);
        int creations = 0;
        board.FeedbackPresented += receipt =>
        {
            if (receipt.Kind == BoardFeedbackKind.MatchImpact)
                ScreenCapture.CaptureScreenshot(Path.Combine(output, "shatter.png"));
            if (receipt.Kind == BoardFeedbackKind.SpecialCreated)
            {
                creations++;
                ScreenCapture.CaptureScreenshot(Path.Combine(output, "creation.png"));
            }
        };
        board.StartCoroutine((IEnumerator)Call(board, "TrySwap", source, board.GetGem(safeMoveTo.x, safeMoveTo.y)));
        yield return Until(() => Run.MoveClock.Tick == 1 && Run.Continuation.CanCapture, "earned special settles");
        Assert.That(creations, Is.GreaterThan(0));
        Assert.That(board.GetComponentsInChildren<Gem>().Any(g => g.SpecialType != GemSpecialType.None), Is.True);
        ScreenCapture.CaptureScreenshot(Path.Combine(output, "ready.png"));
        yield return null; yield return null;
    }

    [UnityTest] public IEnumerator BoardFeel2ThirtyHzChainKeepsOneActionAndBoundedFeedback()
    {
        yield return LaunchUnified(); DurableUnifiedFixture();
        var board = Run.Board; var sprites = (Sprite[])Get(board, "gemSprites");
        GemType color = Run.Waves.ActiveEnemies[0].AssignedGemType;
        for (int x = 0; x < board.Width; x++) for (int y = 0; y < board.Height; y++)
        {
            var gem = board.GetGem(x, y); gem.SetSpecialType(GemSpecialType.None); gem.SetType(color, sprites[(int)color]);
        }
        board.GetGem(2, 0).SetType((GemType)(((int)color + 1) % 6), sprites[((int)color + 1) % 6]);
        board.GetGem(5, 3).SetSpecialType(GemSpecialType.RowBomb);
        board.GetGem(6, 3).SetSpecialType(GemSpecialType.ColumnBomb);
        Set(board, "refillRandom", new SavedRandom(13579));
        int accepted = 0, clearCount = 0, depth = 0;
        board.ValidPlayerMoveAccepted += _ => accepted++;
        board.BoardClearResolved += c => { clearCount += c.GemCount; depth = Math.Max(depth, c.CascadeDepth); };
        var metrics = board.gameObject.AddComponent<BoardFeelMetrics>();
        try
        {
            Time.captureDeltaTime = 1f / 30f;
            board.StartCoroutine((IEnumerator)Call(board, "TrySwap", board.GetGem(2, 0), board.GetGem(2, 1)));
            yield return Until(() => Run.MoveClock.Tick == 1 && Run.Continuation.CanCapture, "30Hz chain settles");
            Assert.That(accepted, Is.EqualTo(1)); Assert.That(clearCount, Is.GreaterThan(40));
            Assert.That(depth, Is.GreaterThanOrEqualTo(2));
            string folder = Path.GetFullPath(".utmp/BoardFeel2"); Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "chain-30hz.csv"), metrics.ExportCsv());
        }
        finally { Time.captureDeltaTime = 0; }
    }

    [UnityTest] public IEnumerator BoardFeel2OldAndNewInFlightJournalsRecoverExactlyOnce()
    {
        foreach (int version in new[] { 0, 1 })
        {
            yield return LaunchUnified(); PrepareSafeMove();
            var board = Run.Board;
            var original = board.BeforePlayerSwap;
            board.BeforePlayerSwap = (a, b) =>
            {
                bool accepted = original(a, b);
                Set(board, "<ReplayLegacyBoardFeel>k__BackingField", version == 0);
                ((RunReplayTape)Get(Run.Continuation, "tape")).boardFeelVersion = version;
                return accepted;
            };
            board.StartCoroutine((IEnumerator)Call(board, "TrySwap",
                board.GetGem(safeMoveFrom.x, safeMoveFrom.y), board.GetGem(safeMoveTo.x, safeMoveTo.y)));
            Assert.That(Run.Continuation.SaveNow(), Is.True);
            string pending = File.ReadAllText(path);
            yield return Until(() => Run.MoveClock.Tick == 1 && Run.Continuation.CanCapture, "original action");
            var expected = Run.Continuation.Capture();
            yield return ReloadBoardFeelFixture(pending);
            var actual = Run.Continuation.Capture();
            Assert.That(JsonUtility.ToJson(actual.board), Is.EqualTo(JsonUtility.ToJson(expected.board)), "journal " + version);
            Assert.That(JsonUtility.ToJson(actual.player), Is.EqualTo(JsonUtility.ToJson(expected.player)));
            Assert.That(actual.clock.actions.completed, Is.EqualTo(1));
            Assert.That(actual.enemies.Select(e => e.health).ToArray(), Is.EqualTo(expected.enemies.Select(e => e.health).ToArray()));
        }
    }

    [UnityTest] public IEnumerator BoardFeel2LegacySecondsSaveKeepsOriginalTimingWithoutMoveClock()
    {
        yield return LaunchUnified(); PrepareSafeMove();
        Assert.That(Run.Continuation.SaveNow(), Is.True);
        var saved = JsonUtility.FromJson<AccountSave>(File.ReadAllText(path));
        saved.run.checkpoint.version = 1;
        saved.run.checkpoint.clock = null;
        saved.run.tape = new RunReplayTape();
        yield return ReloadBoardFeelFixture(JsonUtility.ToJson(saved));
        Assert.That(Run.MoveClock, Is.Null);
        Assert.That(Run.Board.UsesLegacyBoardFeel, Is.True);
        Assert.That((float)Call(Run.Board, "GetResponsiveSwapDuration"), Is.EqualTo(.15f).Within(.0001f));
        Assert.That((float)Call(Run.Board, "GetPostFallSettlePause"), Is.EqualTo(.06f).Within(.0001f));
        Assert.That(Run.Continuation.SaveNow(), Is.True);
        Assert.That(((RunReplayTape)Get(Run.Continuation, "tape")).boardFeelVersion, Is.Zero);
    }

    [UnityTest] public IEnumerator BoardFeel2PauseFocusAndVibrationSettingsDiscardFeedback()
    {
        yield return LaunchUnified();
        var audio = Run.Board.GetComponent<CombatAudioController>();
        bool muted = AudioPreferences.VibrationMuted;
        Set(audio, "focused", true);
        int dispatch = MobileHaptics.DispatchAttempts;
        try
        {
            AudioPreferences.SetVibrationMuted(false);
            MobileHaptics.Request(HapticStrength.Cast);
            Time.timeScale = 0; MobileHaptics.Flush();
            Time.timeScale = 1; MobileHaptics.Flush();
            Assert.That(MobileHaptics.DispatchAttempts, Is.EqualTo(dispatch));
            Call(audio, "OnApplicationFocus", false);
            CombatAudioController.PlayMechanism(CombatSoundCue.CrystalImpact);
            MobileHaptics.Request(HapticStrength.Cast); MobileHaptics.Flush();
            Assert.That(MobileHaptics.DispatchAttempts, Is.EqualTo(dispatch));
            Call(audio, "OnApplicationFocus", true);
            AudioPreferences.SetVibrationMuted(true);
            MobileHaptics.Request(HapticStrength.Cast); MobileHaptics.Flush();
            Assert.That(MobileHaptics.DispatchAttempts, Is.EqualTo(dispatch));
        }
        finally { Time.timeScale = 1; AudioPreferences.SetVibrationMuted(muted); Call(audio, "OnApplicationFocus", true); }
    }
}
