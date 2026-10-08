# Unified move-combat validation — 2026-10-08

## Delivery scope

Feature branch: `codex/unified-move-combat`, based on merged Ironvein
`b625cbb61d9929a0281edecedda79b0eb3b58ec2`. The direct user request authorizes
merging this implementation after validation. Numerical tuning is experimental.

The board's accepted-action pipeline remains authoritative. New runs in every
zone use integer basics and move-based effects; earlier saves keep their explicit
profiles. Specials resolve by rank then slot before remaining due basics. One
enemy may do both legally. Full weapon sequences, shield gating, central damage,
environmental caps and global endless scaling retain their existing owners.

- [Rules, status manifest and timing audit](../../UNIFIED_MOVE_COMBAT.md).
- [All 62 attack intervals and special readiness values](../../UNIFIED_MOVE_INTERVALS.md).
- [Native art review](../../../ArtSource/UnifiedCombat/NativeReview.png).
- [File dimensions, binary alpha, palettes and hashes](../../../ArtSource/UnifiedCombat/FinalChecks.json).
- [Generation ledger](../../../ArtSource/UnifiedCombat/GenerationLedger.json).
- [Palette corrections](../../../ArtSource/UnifiedCombat/Corrections.json).
- [Free sword edit](../../../ArtSource/UnifiedCombat/SwordEdits.json).

## Executed evidence

Focused gameplay coverage includes integer countdown/reset, independent special
readiness, all rank ties, dead actors, slot reuse, one full Lancer command sequence,
no summon birth tick, actual cascades sharing one action, haste application and
expiry, natural/forced Stagger, full future durations, Decree Continue and Conch
expiry. Malformed unified float durations are rejected, while legacy float timers
retain their meaning.

The rendered suite verifies all four zones at 720×1280, 1080×1920, 1080×2400
and a simulated safe-area inset. Additional captures cover the large Grand Delver
with cast copy, and simultaneous healer channels through Continue and independent
interruption. Audio is muted in automated Unity sessions.

The final run inventory and latest result for each unique case are recorded in
`AutomatedTests.json` and `AutomatedTests.csv`. Repeated runs are not summed as
additional unique coverage. Full Unity XML/logs remain under `.utmp/ForestValidation`.
Final latest results: **479 unique cases passed, 0 failed, 0 skipped** across
the broad suite and focused reruns. The first broad run's failures are retained
in the run inventory rather than hidden. The 44-case repair run passed 43 cases;
its remaining ore-power fixture then passed separately using accepted moves.

`powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1` completed
successfully with **Unity 6000.3.19f1**, exit 0. The verified implementation commit
is `86dd73e5cc66b461c542ce56e030cebb9ce98352`; subsequent delivery changes contain
only documentation and captured evidence. See `UnityValidator.json` for the
original full-log path and checksum. Unity audio was muted during validation.

### Legacy fixture corrections

The first broad run reported 436/461 passing. The failing tests exposed obsolete
fixture assumptions as well as checks needing investigation. Tests that directly
prime seconds-profile casts now load an explicit version-one checkpoint through
the real continuation bootstrap. They remain legacy compatibility coverage; the
new King full-cycle test uses real accepted moves in the unified profile.

Quiet kit fixtures now silence the correct integer basic counter, and portrait
tests configure the intended Game view before launch. Player ability lifecycle
fixtures run in PlayMode so Unity actually invokes enable/disable callbacks.
No gameplay guard or assertion was removed to make these fixtures pass. Repaired
cases and affected integration scenarios passed separately; latest per-case
results are the acceptance record. The ore sequence test checks both powered
and ordinary damage through actual accepted moves, preserving all damage assertions.

## Rendered inspection

The `Visual` folder contains actual Unity captures, not composition mockups:

- Four zones × three portrait sizes plus safe-area inset (16 captures).
- Large leader/cast display at three portrait sizes.
- Two concurrent healer warnings with their fixed-target links.

The native sword uses only charcoal and ivory; counter text is independent
Thaleah UI with a one-logical-pixel outline. Each status has its own 24px cell.
Player rows wrap without shrinking, and enemy rows retain authoritative slot
sigils. The Decree target mark sits beside the action rows. Large animation
canvases use transparent headroom with reserved clearance below the wave plaque.

Bright white actors and weakness gems in some captures are intentional retained
Stagger flashes. The fixture's 99-move immunity is a test guard. Production
Stagger and immunity each last two future accepted moves.

## Stagger and balance findings

| Rank | Threshold (% maximum HP) | Decay per unanswered move | Fraction of a 35-damage hit |
|---|---:|---:|---:|
| Normal | 30% | 10 | 28.6% |
| Special | 35% | 7.5 | 21.4% |
| Miniboss | 30% | 5 | 14.3% |
| Boss | 25% | 3 | 8.6% |

`StaggerBalance.csv` contains 496 runtime-derived scenarios: all 62 enemies,
waves 30/70/100/150, and player levels 1/5. Each tested rank can gain net meter by
alternating qualifying and off-color moves. This calculation preserves the actual
HP curve and damage calculation; it is not a complete simulated high-wave run.

First/repeat basic intervals are 2–6 moves. Fast summons/skirmishers retain fast
cadence, while support and heavy sequences allow more board responses. Haste uses
`max(2, ceil(base / strongest speed))`, never stacks multiplicatively, and leaves
at least one future response when applied. No HP, damage, encounter weights,
blocker caps or wave counts were changed from this small pacing sample.

## Pacing method and limits

`Pacing` contains accepted moves per encounter plus separate simulated game time
and actual automated wall time. Four disposable level-1 Skeleton runs use the
production stats and encounter planner, a greedy swap policy, free abilities,
potions below 45% HP, the first offered card, and a 2.5 game-second input interval.
Simulation is accelerated 6× and stops after six completed encounters. Zero new
moves in a wave is possible when a free skill finishes it.

Latest captured opening sample (six encounters per zone):

| Zone | Accepted moves by encounter | Total | Mean moves | Simulated seconds | Automated wall seconds | Ending HP |
|---|---|---:|---:|---:|---:|---:|
| Dungeon | 2, 1, 3, 2, 3, 3 | 14 | 2.33 | 46.17 | 7.68 | 100 |
| Forest | 10, 4, 3, 6, 4, 6 | 33 | 5.50 | 98.24 | 16.36 | 100 |
| Drowned Court | 6, 8, 6, 6, 11, 5 | 42 | 7.00 | 115.21 | 19.19 | 85 |
| Ironvein | 1, 10, 8, 3, 1, 4 | 27 | 4.50 | 74.51 | 12.41 | 100 |

The Dungeon sample clears quickly. The other openings require more responses,
particularly the Court support formations. This is one sample per zone, not a
statistical comparison; initial board randomness, greedy choices and accelerated
presentation affect it. Acceptance counts include the final lethal action even
when its completion callback has not yet run at the observation boundary.

These are short automated observations. **Human elapsed time is not measured.**
Human/device reading, build diversity, long-run difficulty and sustained zone
visits remain acceptance work. No ten-minute visit or final balance claim follows
from these opening samples. Longer Reign, status stacking and late-wave Stagger
numbers remain provisional pending playtests.

## Preservation and review

The original Drowned Court throne prefab edit and three consumable import edits
are excluded and checked against their starting SHA-256 hashes. No enemy designs,
weapon animations, main menu layout or settings screen were redesigned.
PixelLab used **26.3 subscription generation units**: 24 creations and 23 palette
reductions at 0.1 each. The approved allowance is 60 total / 30 initial; no reserve
or credits were used. Final reported balance: 1038 generations, $0 credits.

Implementation commits, final validator evidence and PR outcome are summarized
in the delivery checkpoint and final user report.
