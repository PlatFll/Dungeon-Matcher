# Crystal Resonance playtest policy — attention-v3-slide

Editor-only HumanLikePlaytestDriver plays the real Game scene at x1 through normal
journaled input. It never alters HP, energy, cards, charge, enemies or travel.
The runner creates a disposable starting account at the configured permanent
level, with three equipped potions/bombs. Natural cases use genuine new-run
creation in Dungeon. Non-Dungeon starts require diagnosticStart=true and remain
separate from natural progression evidence. Recorded results retain deaths and
duration censoring; no completion is invented.

## Perception and choices

The board supplies visible legal responses, not simulated random futures. An
ordinary profile inspects a spatial subset around a varying focus before scoring;
the best move is not guaranteed a place in that subset. Emergency health/AIR
increases attention and recognition without granting a rescue. Skilled examines
all current visible responses. Recognizing a weakness, special or warning is
sampled once per decision rather than separately for every option.

| Style | Ordinary attention | Priorities and limitations |
|---|---|---|
| Novice | 3–6 responses | Partial weakness/telegraph recognition, occasional lower-ranked noticed match, inconsistent ability use, survival-oriented cards with occasional off-preference pick |
| Typical | 7–12 | Usually recognizes visible threats/specials; occasionally overlooks or prefers a weaker move, holds a cast for an obvious counter or very small remainder |
| Skilled | All visible | Consistent recognition, special proximity setup, visible-only falling-piece followups, owned-build/card synergy, saves casts against nearly finished enemies |
| Defensive | 8–14 | Early potion use, healing proportional to missing HP, stronger ice/oxygen priorities, survival cards and more selective abilities |
| Aggressive | 6–10 | More direct weakness damage, frequent abilities and consumable bombs, ability cards, less consistent warning recognition |

All use only current weaknesses, HP, visible channels/marks and offered cards.
Skilled/typical/defensive/aggressive can recognize one gravity step involving
already-visible ordinary pieces. Unknown refill slots remain empty. This narrow
heuristic deliberately declines special/obstacle/ice states it cannot model.
The gravity heuristic uses each piece's visible final position, including three-piece
underwater/Slippery rotation; the prior two-piece assumption could invent cascades.
It does not read board RNG, simulate random cascades, inspect future formations
or inspect unseen offers. Near-special setup is a proximity heuristic rather
than a multi-turn solver. Decisions wait until the next board is settled;
card reading takes 6–11 seconds. Delays alone are not evidence of skill.

The historical defensive policy awarded the same large affinity preference for
5 missing HP as for critical injury. The revised preference scales with missing
health. This corrects the simulation's tendency to repeatedly chase overhealing;
it does not change healing, enemy HP or Resonance awards in the game.

## Diagnostics, not an oracle

After choosing, a separate deterministic full-visible-board score records a
heuristic gap, missed/noticed/chosen marked responses, support/channel target
opportunities, special setup/use and visible followups. This observer consumes
no decision RNG and cannot influence the submitted action. Ability logs flag
casts against a small remainder and casts when marked answers were available.
Card logs retain the actual offer and off-preference choices. These flags are
not proof of mistakes: an explosion or killing the caster can answer a warning
indirectly, and spending a cast to preserve health may be sensible.

The native durable journal independently verifies acceptance. Compare swaps,
casts, supplies, health loss, mastery, counters and encounters alongside time.
All profiles receive legal candidates and therefore omit invalid-swap discovery costs.
They do not fully plan around ordinary enemy attack countdowns or long-horizon
resource use; this limits conclusions about unavoidable damage and optimal play.
No policy models touch slips, actual reading/comprehension, fatigue, long-term
learning or enjoyment. It cannot certify human fun or device performance.

## Reproduction

Each config records the named seed, character, level, policy and limits. Named
seeds initialize board/encounter/gameplay streams; the production-generated run
ID independently seeds capstones. Initial-account.json and native checkpoints
retain that identity/stream. Matching named seeds alone therefore do not imply
identical whole runs. Retain full source manifests and journals; do not pool
different source versions as a single final dataset. Direct-zone, injected-depth
and presentation fixtures never count toward natural run coverage.
