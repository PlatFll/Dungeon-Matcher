using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator KingReadinessHybridAssaultConsumesReadyCounter()
    {
        Assert.That(EditorUtility.audioMasterMute, Is.True, "automated Unity output is muted");
        yield return Launch(3, true);
        var data = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_King.asset");
        Assert.That(Run.Waves.TrySummonEnemy(data, out var king), Is.True);
        yield return Stable(); PreserveRoster();
        var ability = king.GetComponent<KingEnemyAbility>();
        Set(ability, "cycle", 1);
        int commands = 0, strikes = 0;
        ability.CommandIssued += _ => commands++;
        king.GetComponent<EnemyAutoAttack>().AttackResolved += (a, n, applied) => strikes++;
        for (int i = 0; i < king.SpecialTurnRequirement; i++) yield return Move();
        Assert.That(commands, Is.EqualTo(1), "ready King must start Assault during the move opportunity");
        Assert.That(strikes, Is.GreaterThan(0));
        Assert.That(king.IsSpecialReady, Is.False);
        Assert.That(king.CurrentSpecialTurnCount, Is.Zero);
        Assert.That((int)Get(ability, "cycle"), Is.EqualTo(2));
    }

    [UnityTest] public IEnumerator KingReadinessFullCycleUsesLiveSecondsProfile() => RoyalCycle(true);
    [UnityTest] public IEnumerator KingReadinessFullCycleUsesOriginalDungeonProfile() => RoyalCycle(false);

    private IEnumerator RoyalCycle(bool coordinated)
    {
        if (coordinated)
        {
            yield return Launch(3, true);
            ((CombatClockSnapshot)Get(Run.MoveClock, "state")).profile = CombatClockSnapshot.LegacyEffectsProfile;
        }
        else yield return LaunchDungeonHazards();
        var data = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_King.asset");
        Assert.That(Run.Waves.TrySummonEnemy(data, out var king), Is.True);
        yield return Stable(); PreserveRoster();
        Set(Run.Player, "maximumHealth", 10000); Set(Run.Player, "currentHealth", 10000);
        var ability = king.GetComponent<KingEnemyAbility>();
        int commands = 0, sets = 0, lanes = 0, slashes = 0;
        ability.CommandIssued += _ => commands++;
        Run.Board.GemSetMarked += t => { if (t.Owner == king) sets++; };
        Run.Board.LanesMarked += t => { if (t.Owner == king) lanes++; };
        Run.Board.LaneSlash += (row, lane, duration) => slashes++;
        for (int i = 0; i < 4; i++) yield return EnvironmentMove();
        yield return Until(() => sets == 1 && Run.Continuation.CanCapture, "Judgment completes its frame-driven startup");
        Assert.That(sets, Is.EqualTo(1), "Judgment starts after real moves");
        var judgment = (BoardController.GemSetThreat)Get(ability, "judgment");
        for (int i = 0; i < 4; i++) yield return EnvironmentMove();
        yield return Until(() => commands == 1 && !(bool)Get(ability, "pending") && Run.Continuation.CanCapture, "Assault recovery completes");
        Assert.That(judgment.Ended, Is.True);
        Assert.That(commands, Is.EqualTo(1), "Assault cannot remain ready at zero");
        Assert.That(king.IsSpecialReady, Is.False);
        for (int i = 0; i < 4; i++) yield return EnvironmentMove();
        yield return Until(() => lanes == 1 && Run.Continuation.CanCapture, "Bombardment raise completes");
        Assert.That(lanes, Is.EqualTo(1));
        Assert.That(king.GetComponent<EnemyAutoAttack>().IsPausedByAction, Is.True);
        // Resume the raised sword through the production continuation path.
        yield return ResumeRoster();
        king = Enemy(data.EnemyId); ability = king.GetComponent<KingEnemyAbility>();
        Assert.That(king.GetComponent<EnemyAutoAttack>().IsPausedByAction, Is.True);
        slashes = 0; Run.Board.LaneSlash += (row, lane, duration) => slashes++;
        yield return EnvironmentMove(); yield return EnvironmentMove();
        yield return Until(() => slashes == 2 && Run.Continuation.CanCapture, "Bombardment recovery completes");
        Assert.That(slashes, Is.EqualTo(2));
        Assert.That(king.GetComponent<EnemyAutoAttack>().IsPausedByAction, Is.False);
        Assert.That(king.HasAnimationActionInProgress, Is.False);
        Assert.That((int)Get(ability, "cycle"), Is.Zero);
        sets = 0; Run.Board.GemSetMarked += t => { if (t.Owner == king) sets++; };
        yield return EnvironmentMove(); yield return EnvironmentMove();
        yield return Until(() => sets == 1 && Run.Continuation.CanCapture, "next Judgment completes");
        Assert.That(sets, Is.EqualTo(1), "cycle repeats after Bombardment recovery");
    }

    [UnityTest] public IEnumerator KingReadinessReadySaveRecoversAndCommandKeepsSafetyGuards()
    {
        yield return Launch(3, true);
        var data = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_King.asset");
        Assert.That(Run.Waves.TrySummonEnemy(data, out var king), Is.True);
        yield return Stable(); PreserveRoster();
        Set(king.GetComponent<KingEnemyAbility>(), "cycle", 1);
        // Model an already-saved zero-counter stall without altering its schema.
        for (int i = 0; i < king.SpecialTurnRequirement; i++) king.RegisterValidPlayerTurn();
        Assert.That(king.IsSpecialReady, Is.True);
        yield return ResumeRoster(); king = Enemy(data.EnemyId);
        var attack = king.GetComponent<EnemyAutoAttack>();
        var hold = new object();
        Run.GetComponent<RunControlsUI>().OpenSettings();
        Assert.That(attack.TryReserveCommand(hold, true), Is.False, "pause still blocks commands");
        Run.GetComponent<RunControlsUI>().Close();
        attack.SetActionPaused(hold, true);
        Assert.That(attack.TryReserveCommand(hold, true), Is.False, "Bombardment/action holds still block commands");
        attack.SetActionPaused(hold, false);
        var stagger = king.GetComponent<EnemyStagger>();
        Set(stagger, "remainingImmunityTime", 0f); Assert.That(stagger.ApplyStagger(2, 2), Is.GreaterThan(0));
        Assert.That(attack.TryReserveCommand(hold, true), Is.False, "stagger still blocks commands");
        // Let this move-based stagger expire normally.
        for (int i = 0; i < 4 && king.IsSpecialReady; i++) yield return Move();
        Assert.That(king.IsSpecialReady, Is.False);
        Assert.That((int)Get(king.GetComponent<KingEnemyAbility>(), "cycle"), Is.EqualTo(2));
        Assert.That(attack.HasCommandReservation, Is.False);
    }

    [UnityTest] public IEnumerator KingReadinessCaptainCommandWorksWithTimedBasics()
    {
        yield return Launch(3, true);
        var data = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_KnightCaptain.asset");
        Assert.That(Run.Waves.TrySummonEnemy(data, out var captain), Is.True);
        yield return Stable(); PreserveRoster();
        var ability = captain.GetComponent<KnightCaptainEnemyAbility>(); Set(ability, "preferChains", false);
        int hits = 0; captain.GetComponent<EnemyAutoAttack>().AttackResolved += (a, n, applied) => hits++;
        for (int i = 0; i < captain.SpecialTurnRequirement; i++) yield return Move();
        Assert.That(hits, Is.GreaterThan(0)); Assert.That(captain.IsSpecialReady, Is.False);
        Assert.That(captain.GetComponent<EnemyAutoAttack>().HasCommandReservation, Is.False);
    }

    [UnityTest] public IEnumerator KingReadinessVineGrowthUsesHalfSpreadAndCoverage()
    {
        yield return Launch(1, true); QuietKitFixture(); var board = Run.Board;
        var owner = Enemy("orc_rootbinder"); owner.SetSpecialTurnRequirement(100);
        board.QueueEnvironmentalVine(board.GetGem(3,3)); yield return Stable();
        int before = board.VineCount;
        Assert.That(board.QueueVineSurge(owner, null), Is.True); yield return Stable();
        Assert.That(board.VineCount - before, Is.EqualTo(2), "spread reduced from four to two");
        before = board.VineCount;
        yield return board.AdvanceVineNetworks(2); yield return Stable();
        Assert.That(board.VineCount - before, Is.InRange(1,3), "at most two frontier additions plus one edge seed");
        for (int i = 0; i < 10; i++) { board.QueueVineSurge(owner, null); yield return Stable(); }
        Assert.That(board.VineCount, Is.EqualTo(12), "growth cap reduced from 24 to 12");
        var prior = board.CaptureVines(Run.Waves.ContinuationOwnerSlot);
        yield return ResumeRoster();
        Assert.That(Run.Board.VineCount, Is.EqualTo(prior.Count));
        Assert.That(Run.Board.NextVineGrowthMove, Is.EqualTo(4));
    }

    [UnityTest] public IEnumerator KingReadinessAbilityCountersFitAtPortraitSizes()
    {
        yield return Launch(3, true);
        var data = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_King.asset");
        Assert.That(Run.Waves.TrySummonEnemy(data, out var king), Is.True);
        var captainData = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_KnightCaptain.asset");
        Assert.That(Run.Waves.TrySummonEnemy(captainData, out var captain), Is.True);
        yield return Stable(); QuietKitFixture();
        yield return Until(() => !king.GetComponent<EnemyLifecycleVFX>().IsSpawning &&
            !captain.GetComponent<EnemyLifecycleVFX>().IsSpawning, "counters are visible after spawn");
        string output = Path.GetFullPath(".utmp/KingReadiness"); Directory.CreateDirectory(output);
        foreach (var size in new[] {new Vector2Int(720,1280), new Vector2Int(1080,1920), new Vector2Int(1080,2400)})
        {
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic)
                .Invoke(null, new object[] {size});
            yield return Until(() => Screen.width == size.x && Screen.height == size.y, "portrait resize");
            yield return null; yield return null;
            foreach (var actor in new[] {king, captain})
            {
                var counter = actor.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "SpecialAbilityCounter");
                foreach (string value in new[] {"4", "0", "12"})
                {
                    counter.text = value; PixelTextFitter.Apply(counter);
                    yield return null; counter.ForceMeshUpdate(true, true);
                    Assert.That(counter.gameObject.activeInHierarchy, Is.True);
                    Assert.That(counter.enabled, Is.True); Assert.That(counter.font, Is.SameAs(GameUi.TmpFont));
                    Assert.That(counter.preferredWidth, Is.LessThanOrEqualTo(counter.rectTransform.rect.width + .1f));
                    Assert.That(counter.preferredHeight, Is.LessThanOrEqualTo(counter.rectTransform.rect.height + .1f));
                    var glyph = counter.textInfo.characterInfo[0];
                    float pixels = (glyph.topLeft.y - glyph.bottomLeft.y) * counter.canvas.rootCanvas.scaleFactor;
                    if (pixels < 14)
                    {
                        ScreenCapture.CaptureScreenshot(Path.Combine(output, "counter-failure.png"));
                        yield return null; yield return null;
                    }
                    Assert.That(pixels, Is.GreaterThanOrEqualTo(14), $"actual glyph pixels: active={counter.gameObject.activeInHierarchy}, text={counter.text}, fontSize={counter.fontSize}, rect={counter.rectTransform.rect}, visible={glyph.isVisible}");
                    var bounds = RenderedTextBounds(counter);
                    Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0));
                    Assert.That(bounds.xMax, Is.LessThanOrEqualTo(Screen.width));
                    Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0));
                    Assert.That(bounds.yMax, Is.LessThanOrEqualTo(Screen.height));
                    var intent = actor.GetComponentInParent<EnemySlotUI>().GetComponentsInChildren<TMP_Text>()
                        .Single(t => t.name == "MoveIntent");
                    var intentBounds = RenderedTextBounds(intent);
                    if (bounds.Overlaps(intentBounds))
                    {
                        ScreenCapture.CaptureScreenshot(Path.Combine(output, "counter-overlap.png"));
                        yield return null; yield return null;
                    }
                    Assert.That(bounds.Overlaps(intentBounds), Is.False,
                        $"{actor.Definition.EnemyId} at {size}: ability {value} {bounds} must clear countdown {intentBounds}");
                }
                counter.text = "4"; PixelTextFitter.Apply(counter);
            }
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(output, "ability-counter-" + size.x + "x" + size.y + ".png"));
            yield return null; yield return null;
        }
    }

    private static Rect RenderedTextBounds(TMP_Text text)
    {
        text.ForceMeshUpdate(true, true);
        var bounds = text.textBounds;
        Vector2 min = RectTransformUtility.WorldToScreenPoint(null, text.transform.TransformPoint(bounds.min));
        Vector2 max = RectTransformUtility.WorldToScreenPoint(null, text.transform.TransformPoint(bounds.max));
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }
}
