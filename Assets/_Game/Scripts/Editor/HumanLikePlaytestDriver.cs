#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ResonancePlaystyle { Novice, Typical, Skilled, Defensive, Aggressive }

/// <summary>Editor-only visible-state policy, with bounded attention. Never reads refill/encounter RNG.</summary>
public sealed class HumanLikePlaytestDriver
{
    public const string PolicyVersion = "attention-v2";
    public readonly ResonancePlaystyle Style;
    private readonly System.Random decisions;
    private float nextAction, cardReady = -1;
    private bool waitForSettledBoard = true;
    public int Swaps { get; private set; }
    public int Abilities { get; private set; }
    public int CardPicks { get; private set; }
    public Action<string> ActionLog;
    public HumanLikePlaytestDriver(ResonancePlaystyle style, int seed)
    { Style = style; decisions = new System.Random(seed ^ 0x591a); }
    private bool Roll(float probability) => decisions.NextDouble() < probability;
    private bool Novice => Style == ResonancePlaystyle.Novice;
    private bool Skilled => Style == ResonancePlaystyle.Skilled;
    private bool Defensive => Style == ResonancePlaystyle.Defensive;
    private bool Aggressive => Style == ResonancePlaystyle.Aggressive;

    public void Step(RunSession run)
    {
        if (run == null || !run.InitialStateReady || run.IsFinished || run.Continuation.IsRestoring || Time.timeScale <= 0) return;
        var draft = run.Waves.GetComponent<RunUpgradeCoordinator>();
        if (draft != null && draft.IsBlockingWaveProgression && run.Continuation.CanCapture)
        { PickCard(run, draft); return; }
        cardReady = -1;
        if (!run.Waves.IsWaveActive || !run.Continuation.CanCapture || run.Board.IsExternalInputBlocked) return;
        // Thinking begins only after the next board is actually visible and settled.
        if (waitForSettledBoard) { waitForSettledBoard = false; nextAction = Time.time + Delay(); return; }
        if (Time.time < nextAction) return;
        float health = (float)run.Player.CurrentHealth / run.Player.MaximumHealth;
        if (health < (Defensive ? .65f : Novice ? .38f : .45f) && run.TryUsePotion())
        { ActionLog?.Invoke("potion"); waitForSettledBoard = true; return; }

        var options = run.Board.GetImmediateResponses();
        if (options.Count == 0) return;
        var warnings = run.Board.CaptureContinuation(run.Waves.ContinuationOwnerSlot).warnings;
        bool emergency = health < .3f || (run.Board.IsFlooded && run.Board.Aquatic.air <= 2);
        // Visibility is not comprehension. Recognition is sampled once per decision,
        // not once per candidate (which would reward scanning more alternatives).
        bool knowsWeakness = Skilled || Roll(Novice ? .55f : Aggressive ? .83f : .9f);
        bool seesThreat = Skilled || Roll(emergency ? .95f : Novice ? .48f : Aggressive ? .75f : Defensive ? .97f : .9f);
        bool seesSpecial = Skilled || Roll(Novice ? .65f : .96f);
        var noticed = Notice(options, emergency);
        var ranked = noticed.Select(o => new Rated(o, Score(run, o, warnings, knowsWeakness, seesThreat, seesSpecial)))
            .OrderByDescending(p => p.Score).ToArray();
        int rank = Roll(Novice ? .22f : Skilled ? 0 : Aggressive ? .12f : .07f)
            ? decisions.Next(Math.Min(3, ranked.Length)) : 0;
        var chosen = ranked[rank].Option;
        bool urgentAir = run.Board.IsFlooded && run.Board.Aquatic.air <= 2;
        bool noticedAir = seesThreat && noticed.Any(o => Bubbles(run, o) || HitsAirCoffer(run, o));
        // Safety is a priority, not a forced death mechanism: an understood available
        // oxygen answer overrides an attractive special at critically low AIR.
        if (urgentAir && noticedAir)
            chosen = ranked.First(p => Bubbles(run, p.Option) || HitsAirCoffer(run, p.Option)).Option;

        var ability = run.Player.GetComponent<PlayerAbilityController>();
        float remainingHp = run.Waves.ActiveEnemies.Where(e => e != null && !e.IsDefeated).Sum(e => e.CurrentHealth);
        bool visibleChannel = run.Waves.ActiveEnemies.Any(e => e != null && e.GetComponent<EnemyChannelRuntime>()?.IsChanneling == true);
        bool smallRemainder = remainingHp <= 25 && !visibleChannel;
        bool urgentCounter = seesThreat && noticed.Any(o => Answers(run, o, warnings));
        bool cast = ability != null && ability.CanActivate && !(urgentAir && noticedAir) &&
            (Novice ? Roll(.4f) : Skilled ? !smallRemainder && !urgentCounter :
             Defensive ? !smallRemainder && (health < .8f || visibleChannel || Roll(.4f)) :
             Aggressive ? Roll(.95f) : !smallRemainder && !urgentCounter && Roll(.8f));
        if (cast && ability.TryActivate())
        {
            Abilities++;
            // Diagnostic heuristics, not a claim that a cast could not kill the threat.
            ActionLog?.Invoke("abilityDecision:smallRemainder=" + smallRemainder + ";counterAvailable=" +
                options.Any(o => Answers(run, o, warnings)) + ";urgentAir=" + urgentAir + ";remainingHp=" + remainingHp);
            ActionLog?.Invoke("ability"); waitForSettledBoard = true; return;
        }
        if (!run.Board.IsHintMoveStillValid(chosen.Source, chosen.Target)) return;
        if ((Aggressive || health < .3f) && run.Waves.ActiveEnemies.Count >= 2 &&
            run.CanUse(ConsumableKind.Bomb) && Roll(Aggressive ? .16f : .2f) && run.TryUseBomb(chosen.Target))
        { ActionLog?.Invoke("bomb:" + chosen.Target.Column + "," + chosen.Target.Row); waitForSettledBoard = true; return; }

        // Full-board diagnostic scoring happens AFTER the choice. It cannot influence
        // the policy or its RNG. 'Suboptimal' means this visible heuristic only.
        var diagnostic = options.Select(o => new Rated(o, Score(run, o, warnings, true, true, true, true))).ToArray();
        float best = diagnostic.Max(p => p.Score), selected = diagnostic.First(p => p.Option == chosen).Score;
        var priority = run.Waves.ActiveEnemies.Where(e => e != null && !e.IsDefeated)
            .OrderByDescending(e => e.GetComponent<EnemyChannelRuntime>()?.IsChanneling == true ? 2 : e.Definition.IsSupport ? 1 : 0).FirstOrDefault();
        bool HitsPriority(BoardController.ResponseOption o) => priority != null && o.Clears.Any(g => g != null && g.Type == priority.AssignedGemType);
        bool priorityThreat = priority != null && (priority.Definition.IsSupport || priority.GetComponent<EnemyChannelRuntime>()?.IsChanneling == true);
        ActionLog?.Invoke("decision:policy=" + PolicyVersion + ";legal=" + options.Count + ";noticed=" + noticed.Count +
            ";warnings=" + warnings.Count + ";knowsWeakness=" + knowsWeakness + ";recognizesThreat=" + seesThreat +
            ";counterAvailable=" + options.Any(o => Answers(run, o, warnings)) + ";counterNoticed=" + noticed.Any(o => Answers(run, o, warnings)) +
            ";counterChosen=" + Answers(run, chosen, warnings) + ";air=" + (run.Board.IsFlooded ? run.Board.Aquatic.air : -1) +
            ";bubbleAvailable=" + options.Any(o => Bubbles(run, o)) + ";bubbleChosen=" + Bubbles(run, chosen) +
            ";cofferAvailable=" + options.Any(o => HitsAirCoffer(run, o)) + ";cofferChosen=" + HitsAirCoffer(run, chosen) +
            ";cofferCharges=" + (run.Board.Aquatic?.coffer?.charges ?? 0) + ";chosenRank=" + rank +
            ";heuristicGap=" + (best - selected).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) +
            ";priorityAvailable=" + (priorityThreat && options.Any(HitsPriority)) + ";priorityChosen=" + HitsPriority(chosen) +
            ";priorityEnemy=" + (priority?.Definition.EnemyId ?? "") + ";createsSpecial=" + chosen.CreatesSpecial +
            ";usesSpecial=" + chosen.UsesSpecial + ";visibleFollowups=" + VisibleFollowups(run.Board, chosen));
        int x = chosen.Source.Column, y = chosen.Source.Row, tx = chosen.Target.Column, ty = chosen.Target.Row;
        run.Board.ReplayPlayerSwap(x, y, tx, ty); Swaps++;
        ActionLog?.Invoke("swap:" + x + "," + y + ">" + tx + "," + ty); waitForSettledBoard = true;
    }

    private sealed class Rated
    {
        public readonly BoardController.ResponseOption Option; public readonly float Score;
        public Rated(BoardController.ResponseOption option, float score) { Option = option; Score = score; }
    }
    private List<BoardController.ResponseOption> Notice(List<BoardController.ResponseOption> options, bool emergency)
    {
        if (Skilled) return options;
        int count = Novice ? decisions.Next(3, 7) : Defensive ? decisions.Next(8, 15) : Aggressive ? decisions.Next(6, 11) : decisions.Next(7, 13);
        if (emergency) count += 3;
        // Spatial scan around a varying attention focus; not a ranking of every
        // move's reward. No guaranteed best candidate is smuggled into this subset.
        var focus = options[decisions.Next(options.Count)].Source;
        return options.Select(o => new Rated(o, Mathf.Abs(o.Source.Column - focus.Column) +
                Mathf.Abs(o.Source.Row - focus.Row) + (float)decisions.NextDouble() * 5))
            .OrderBy(p => p.Score).Take(Math.Min(count, options.Count)).Select(p => p.Option).ToList();
    }
    private static bool Answers(RunSession run, BoardController.ResponseOption option, List<BoardWarningSnapshot> warnings)
        => option.Clears.Any(g => g != null && warnings.Any(w => !w.environmental && w.kind != 2 &&
            w.targets.Contains(option.FinalCell(g).y * run.Board.Width + option.FinalCell(g).x)));
    private static bool Bubbles(RunSession run, BoardController.ResponseOption option)
        => run.Board.IsFlooded && option.Clears.Any(g => g != null && run.Board.Aquatic.bubbles.Contains(g.BoardIdentity));
    private static bool HitsAirCoffer(RunSession run, BoardController.ResponseOption option)
    {
        var coffer = run.Board.IsFlooded ? run.Board.Aquatic.coffer : null;
        return coffer != null && coffer.charges > 0 && option.Clears.Any(g => g != null &&
            Mathf.Abs(option.FinalCell(g).x - coffer.x) + Mathf.Abs(option.FinalCell(g).y - coffer.y) == 1);
    }
    private float Delay()
    {
        float low = Novice ? 1 : Defensive ? 1.5f : Skilled ? .4f : Aggressive ? .5f : .7f;
        float high = Novice || Defensive ? 3 : Skilled ? 1.5f : 2;
        return Mathf.Lerp(low, high, (float)decisions.NextDouble());
    }
    private float Score(RunSession run, BoardController.ResponseOption option, List<BoardWarningSnapshot> warnings,
        bool knowsWeakness, bool seesThreat, bool seesSpecial, bool observer = false)
    {
        bool skilled = Skilled || observer, defensive = Defensive && !observer, novice = Novice && !observer;
        float value = option.Clears.Count;
        if (seesSpecial) value += (option.CreatesSpecial ? novice ? 8 : skilled ? 23 : 18 : 0) +
                                 (option.UsesSpecial ? novice ? 10 : skilled ? 30 : 24 : 0);
        if (skilled && option.CreatesSpecial)
            for (int x = 0; x < run.Board.Width; x++) for (int y = 0; y < run.Board.Height; y++)
            {
                var nearby = run.Board.GetGem(x, y);
                if (nearby != null && nearby.SpecialType != GemSpecialType.None &&
                    Mathf.Abs(x - option.Target.Column) + Mathf.Abs(y - option.Target.Row) <= 2) value += 6;
            }
        // Only already-visible falling pieces; no refill or subsequent random cascade.
        if (skilled || (!novice && seesSpecial)) value += VisibleFollowups(run.Board, option) * (skilled ? 5 : 2);
        if (option.BreaksObstacle) value += seesThreat ? 12 : 1;
        if (seesThreat && HitsAirCoffer(run, option)) value += run.Board.Aquatic.air <= 2 ? 120 :
            run.Board.Aquatic.bubbles.Count == 0 ? 50 : defensive ? 18 : 10;
        foreach (var gem in option.Clears)
        {
            if (gem == null) continue;
            if (seesThreat && run.Board.IsFlooded && run.Board.Aquatic.bubbles.Contains(gem.BoardIdentity))
                value += run.Board.Aquatic.air <= 2 ? 80 : defensive ? 12 : 5;
            if (seesThreat && run.Board.IceStage(gem) != FrostStage.None) value += defensive ? 12 : 8;
            float missing = 1 - (float)run.Player.CurrentHealth / run.Player.MaximumHealth;
            if (gem.Type == run.Player.Definition.AffinityGemType && missing > 0)
                value += missing * (defensive ? 18 : novice ? 7 : 10);
            if (knowsWeakness) foreach (var enemy in run.Waves.ActiveEnemies)
            {
                if (enemy == null || enemy.IsDefeated || enemy.AssignedGemType != gem.Type) continue;
                value += novice ? 3 : enemy.Definition.IsSupport ? 8 : Aggressive && !observer ? 7 : 5;
                if (seesThreat && enemy.GetComponent<EnemyChannelRuntime>()?.IsChanneling == true) value += 10;
                if (!novice && enemy.CurrentHealth <= option.Clears.Count * 10) value += 2; // visible finish opportunity, approximate
            }
            if (seesThreat)
            {
                var cell = option.FinalCell(gem); int index = cell.y * run.Board.Width + cell.x;
                foreach (var warning in warnings)
                    if (!warning.environmental && warning.kind != 2 && warning.targets.Contains(index)) value += 24;
            }
        }
        return value;
    }

    // Conservative visible-only one-gravity-step analysis. Unknown refill slots
    // remain empty. Skip specials/obstacles whose outcomes this proxy cannot model.
    public static int VisibleFollowups(BoardController board, BoardController.ResponseOption option)
    {
        if (option.UsesSpecial || option.CreatesSpecial) return 0;
        int[,] types = new int[board.Width, board.Height];
        for (int x = 0; x < board.Width; x++) for (int y = 0; y < board.Height; y++) types[x, y] = -1;
        for (int x = 0; x < board.Width; x++)
        {
            int destination = 0;
            for (int y = 0; y < board.Height; y++)
            {
                var original = board.GetGem(x, y);
                if (original == null || board.IsGemPinned(original) || board.IceStage(original) != FrostStage.None) return 0;
                var gem = original == option.Source ? option.Target : original == option.Target ? option.Source : original;
                if (option.Clears.Contains(gem)) continue;
                if (gem.SpecialType != GemSpecialType.None) return 0;
                types[x, destination++] = (int)gem.Type;
            }
        }
        var matched = new HashSet<int>();
        for (int x = 0; x < board.Width; x++) for (int y = 0; y < board.Height; y++)
        {
            if (types[x, y] < 0) continue;
            if (x + 2 < board.Width && types[x, y] == types[x + 1, y] && types[x, y] == types[x + 2, y])
                for (int i = 0; i < 3; i++) matched.Add(y * board.Width + x + i);
            if (y + 2 < board.Height && types[x, y] == types[x, y + 1] && types[x, y] == types[x, y + 2])
                for (int i = 0; i < 3; i++) matched.Add((y + i) * board.Width + x);
        }
        return matched.Count;
    }
    private void PickCard(RunSession run, RunUpgradeCoordinator draft)
    {
        var snapshot = run.Continuation.Capture();
        if (snapshot.draft.Count == 0) return;
        if (cardReady < 0) cardReady = Time.time + 6 + (float)decisions.NextDouble() * 5;
        if (Time.time < cardReady) return;
        var catalog = RunUpgradeRuntime.Current.Catalog;
        var offered = snapshot.draft.Select(id => catalog.Upgrades.First(d => d.UpgradeId == id)).ToArray();
        float Value(RunUpgradeDefinition card)
        {
            var preferred = Defensive || Novice ? RunUpgradeTheme.Survival : Aggressive ? RunUpgradeTheme.Ability : RunUpgradeTheme.Board;
            float score = card.Theme == preferred ? 10 : 0;
            if (!Novice && run.Player.CurrentHealth < run.Player.MaximumHealth * .45f && card.Theme == RunUpgradeTheme.Survival) score += 14;
            if (Skilled)
            {
                // Only the owned build and currently shown offer are inspected.
                if (!string.IsNullOrEmpty(card.RequiredAbilityId)) score += 8;
                int sameTheme = snapshot.cards.Sum(c => catalog.Upgrades.Any(d => d.UpgradeId == c.id && d.Theme == card.Theme) ? c.stacks : 0);
                score += Math.Min(6, sameTheme * 2);
                if (card.RequiresEnergyBudget && Abilities > 0) score += 5;
            }
            return score;
        }
        var sorted = offered.OrderByDescending(Value).ToArray();
        var pick = Roll(Novice ? .3f : Skilled ? 0 : .08f) ? offered[decisions.Next(offered.Length)] : sorted[0];
        if (draft.SelectRecordedCard(pick.UpgradeId))
        {
            CardPicks++; ActionLog?.Invoke("cardDecision:offered=" + string.Join(",", snapshot.draft) +
                ";preferred=" + sorted[0].UpgradeId + ";offPreference=" + (Value(pick) < Value(sorted[0])));
            ActionLog?.Invoke("card:" + pick.UpgradeId); cardReady = -1; waitForSettledBoard = true;
        }
    }
}
#endif
