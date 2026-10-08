# Unified accepted-move combat — experimental revision

## Authority and starting state

User brief: `a6e1f030-90a1-4d95-b881-6b235484e23f/Pasted text.txt`.
The direct request authorizes merging after implementation and validation.
Base: `b625cbb61d9929a0281edecedda79b0eb3b58ec2` (Ironvein merged).
Branch: `codex/unified-move-combat`. Four pre-existing asset edits are excluded;
their baseline hashes remain in `Docs/IronveinExcavation/READINESS.md`.

## Current-source audit

- BoardController already emits exactly one acceptance/completion pair around
  the entire manual swap. CombatMoveClock already holds input and wave progression
  through environmental and enemy work. Extend this owner, never add a resolver.
- Existing profiles are seconds, seconds with move-coordinated abilities, hybrid
  forest effects, and the `accepted-moves-v1` forest prototype. Their float timing
  values cannot be silently reinterpreted as integer moves.
- Prototype basics subtract fractional haste each move; it orders actors by ID
  and treats special/basic opportunities as mutually exclusive. New requirements
  need integer intervals, all specials before basics, and slot/rank ordering.
- Prototype Stagger lasts one future move and drains 25% after a grace move.
  EnemyStagger consumes feedback-producing actor damage; periodic poison bypasses
  those events. Rattled multiplies only new buildup. Preserve that source boundary.
- Player's eight statuses already use accepted moves and birth-action protection.
  Poison, Marshal rally, Drummer rhythm, supplies and Decree have profile-aware
  paths. Conch rally still runs in seconds. Ore/Benediction consume one sequence,
  Warded depends on living roots, Fortified consumes eligible incoming hits,
  banner haste depends on surviving board standards, and rage lasts until death.
- Channel owners already save target identities/deadlines, cancel through ordinary
  Stagger and have zero extra recovery. Mine final-response warnings must remain
  live through environmental drills before they release.
- Existing player status art is seven 16px icons plus the 32px Rattled glyph.
  The new presentation replaces these with a consistent native 24px family.
  Enemy status presentation is mostly text beside weakness gems. Existing action
  UI combines HIT IN text, a separate ability counter and slot sigil.

## New profile contract

`unified-accepted-moves-v2` applies to new normal/practice runs in all four zones.
Existing records keep their saved profile; explicit old forest test fixtures
retain their chosen prototype profile. No migration converts seconds into moves.
The new basic countdown is saved in a dedicated integer field. Missing/invalid
new-profile state is rejected without altering the durable save.

Basic and special countdowns are independent. All accepted actors advance before
special effects. Specials use Boss, Miniboss, Special, Normal priority, then slot
and persistent identity. Basics use slot order and only the original due set.
Commands consume participants' basic opportunities. New summons wait until the
next acceptance. Every action rechecks living state, holds and reservations.

Haste uses `max(2, ceil(base interval / strongest active speed))`. A stronger buff
can shorten an in-progress countdown by the interval difference, leaving at least
one future move. Expiry never adds back consumed progress. Damage modifiers keep
their existing ownership and composition.

## Status/art manifest

| Timed owner | New-profile duration |
|---|---|
| Enemy basic | Individual first/repeat integers, 2–6 moves |
| Enemy special | Existing definition readiness; response deadlines stay in moves |
| Player Burn/Weakened/Sapped/Wounded/Fear/Frostbite/Slippery | 3 future moves; source/cleanse rules may end them earlier |
| Player Rattled | 2 future moves |
| Enemy poison | 3 future ticks, 4 with Slow Venom; one tick per accepted move |
| Stagger / post-Stagger immunity | 2 / 2 full future moves |
| War Rhythm / Marshal rally / Conch rally | 3 future moves |
| Marshal retreat | 2 future moves, or protector loss |
| Royal Decree | 5 future moves plus 1 per Longer Reign stack |
| Potion and Bomb cooldowns | 2 moves; activation itself is free |

Animation frames, contact/recovery presentation, VFX, music and UI fades still
use seconds. They do not advance combat. Existing environmental counters retain
their accepted-move rules, including flooding/AIR, snare shrink, vine/root growth,
stone hardening, drill charge, shaky tiles and board warnings.

| Family | Owner / actual lifetime | Planned glyph |
|---|---|---|
| Weakened | PlayerStatusRuntime / moves | Notched dull sword |
| Burn | PlayerStatusRuntime / moves, periodic player damage | Orange flame |
| Sapped | PlayerStatusRuntime / moves | Dim violet crystal |
| Wounded | PlayerStatusRuntime / moves | Cracked red heart |
| Fear | PlayerStatusRuntime / moves and source cleanup | Spectral mask |
| Frostbite | PlayerStatusRuntime / moves | Ice shard |
| Slippery | PlayerStatusRuntime / moves; currently no enemy source | Aqua drop with motion |
| Rattled | PlayerStatusRuntime / moves | Cracked metal bell |
| Poison | EnemyPoisonStatus / periodic moves in new profile | Green venom drop |
| Warded | EnemyActor / living Warden root predicate | Leaf/wood shield |
| Fortified | EnemyActor / incoming-hit stacks | Pink pearl |
| Ore-Powered | EnemyOrePower / next eligible whole basic | Orange ore housing |
| Bloodrage / Enrage | ForestCombatAbility / King / until defeat | Red snarling mask |
| War Rhythm | ForestCombatAbility / moves | War drum |
| Marshal rally | TownMarshalEnemyAbility / moves | Hand bell |
| Royal standard | RoyalBannerAuraRuntime / surviving banner | Small gold standard |
| Benediction | RoyalArchbishopEnemyAbility / next whole basic | Blessed blade |
| Rallying Conch | AquaticEnemyAbility / moves; damage only | Coral conch |
| Royal Decree | RoyalDecreeRuntime / five future moves (+ card) | Antique crown |
| Stagger | EnemyStagger / two future moves | Dazed stars |
| Stagger immunity | EnemyStagger / post-expiry moves | Guarded star |
| Marshal retreat | TownMarshalEnemyAbility / protector and moves | Figure behind shield |
| Basic counter | EnemyAutoAttack / integer readiness, not a status | Ivory sword |

HP/shield, ore-machine charges, blockers and channel deadlines are not invented
statuses. Native sources will remain separate from imported final PNGs. Runtime
text supplies counters; condition icons use a neutral marker rather than a fake
duration. Actual native dimensions, alpha and palette counts require file checks.

## Phases and evidence

1. Audit/profile; 2. integer basics; 3. global priority; 4. haste.
5. Stagger; 6. remaining timer owners; 7. Royal Decree.
8. two-row enemy UI; 9. manifest; 10. PixelLab icons; 11. status counters.
12. four-zone provisional balance; 13. save/regression/render/pacing evidence.
14. authoritative docs/art register; 15. review PR, validate and merge.

Implementation is in progress. The first focused Unity run passed 11/11 tests
with audio muted: integer countdown/reset, thinking time, haste/expiry and legacy
save interpretation. Results: `.utmp/ForestValidation/99e921ed-3924-4c20-9550-afc228d3b186.xml`.
Global priority is covered by the focused evidence below; broader integration
and pacing evidence are recorded separately. Existing HP/damage curves, encounter counts, shields and drill
clear semantics are preserved until an explicit, evidenced balance decision.

The next focused run passed 17/17 (`95bf901f-cc4f-4cd8-95be-bad7a16a9245.xml`):
natural/forced Stagger, birth protection, two future moves, immunity, no idle
decay, per-action decay deduplication, Decree Continue and existing player-status
unit coverage. A separate 3/3 run (`4e47c43d-0172-41df-8637-c9ddeb80fbdb.xml`)
proved higher-rank specials before all basics, same-actor special plus basic,
no summon birth tick and the old forest timing fixture. The latest 3/3 run
(`52c576fc-67e5-42a8-9a1f-4a9285a03c87.xml`) added Conch move expiry/save coverage.
These are scoped automated checks, not final integration or human pacing evidence.

### Rendered checks and audit corrections

The final presentation suite passed 4/4 (`9e1225c8-ecab-4f32-b581-0f5aa28fe7e5`):
all four zones at 720x1280, 1080x1920, 1080x2400 and inset safe area; a large
Grand Delver formation at three sizes; two simultaneous Mender channels through
Continue and independent interruption; and legacy icon/data compatibility.
Actual PNGs show the player with all eight statuses plus Decree, enemy poison and
Stagger, HP/shield/weakness, source sigils and native counters. White actor/weakness
flashes are the retained Stagger feedback. The 99-move immunity in the leader and
channel stress fixtures is a test survival guard, never production tuning.

Visual review found and corrected the Decree target marker crossing the new rows,
oversized animation canvases placing status/cast copy into the header, and the
retired move view taking healer target links with it. The new view keeps links,
positions the mark alongside the rows and reserves header clearance. A newer
cast announcement replaces fading copy for that actor. Tests allow transparent
animation headroom; actual rendered inspection checks face/weapon clearance.

### Timing and compatibility detail

Poison ticks and normal zone environment work precede action opportunities.
Mine warnings countered by a final-response drill release after full-lane drill
firing, under the same board hold, ordered by rank and slot. This preserves the
existing explicit environment exception to ordinary special-before-basic order.
Actual channel owners pause readiness. A hold that blocks only basics does not
silently freeze the separate special countdown.

Unified snapshots require the dedicated integer attack field and whole, finite,
nonnegative move durations. Rejection leaves the stored record intact. Older
seconds/hybrid/prototype profiles keep their original meanings. Restore rebuilds
source-owned buffs without shortening a saved countdown again. The six-handoff
travel check retains five Decree moves across all four zones without spending,
adding or replaying a move, while source zone effects are cleaned.

### Provisional balance and measurement limits

The first pass changes individual attack intervals and Stagger rather than adding
HP or blocker pressure. All 62 intervals and unchanged special readiness values
are listed in `UNIFIED_MOVE_INTERVALS.md`. Bloodrage's serialized 999 is a passive
sentinel, not a player-facing special countdown.

Runtime-derived Stagger analysis covers 62 enemies at waves 30, 70, 100 and 150,
at player levels 1 and 5 (496 rows). Typical three-gem damage was 35 and 40.
An unanswered move removes 28.6/21.4/14.3/8.6% of the level-1 typical hit for
Normal/Special/Miniboss/Boss. These are damage-equivalent amounts, not a fixed
percentage of each enemy's meter. For example, a wave-30 King reaches Stagger
in 15 alternating weakness/off-color moves at level 1; wave-100 Queen Nacre in
29. This establishes mathematical reachability under the stated scenario, not
the quality of late-wave human play or every possible build.

Opening pacing samples use actual production stats, one fresh level-1 Skeleton
per zone, greedy swaps, free skills, low-health potions, first offered cards and a
2.5 simulated-second think interval. They are bounded at six completed encounters
and accelerated 6x. Per-wave counters subscribe to acceptance, including the
lethal final action; an ability-only finish can legitimately use zero new moves.
Human elapsed time has **not** been measured. Automated game/wall seconds are
reported separately and must not be presented as human timing. No wave counts,
HP/damage, endless scaling, blocker caps or encounter weights are changed on this
limited evidence. Physical-device reading and longer human runs remain the next
balance review. Numerical values are experimental throughout.

New-profile Stagger thresholds are Normal/Special/Miniboss/Boss = 30/35/30/25%
of current maximum HP. An off-color move removes 10/7.5/5/3 damage-equivalent
buildup, respectively. The shared enemy prefab serializes these provisional
values, two full future Stagger moves and two post-expiry immunity moves. This
reduces the old 90%-HP boss threshold without changing HP or damage. A qualifying
hit anywhere in the complete action prevents that action's decay. Forced Stagger
respects immunity and uses the same two-move duration. Existing feedback-producing
damage eligibility (including Royal Decree) is retained; periodic poison does not
build Stagger. Rattled affects new buildup only.

Royal Decree is five future complete moves. Longer Reign adds one move per stack
through the new typed `RoyalDecreeMoves` channel; its separate legacy seconds
modifier remains for old saves. New-profile Decree retains production cascade
damage scaling rather than inheriting the earlier forest prototype's flat 5 rule.
Conch rally uses three future moves and remains a damage buff. Marshal retreat
now expires on accepted moves even when the Marshal is staggered. Source-owned
Marshal/banner haste cannot erase another source when one expires.

## Art allowance

Approved: 60 subscription generations total, 30 initial and 30 corrections.
No credit purchase. Opening balance: 1064 generations, $0 credit. PixelLab help
confirmed native 24px Pixen output at one generation per image. Production used
24 creations (23 families and one Weakened revision) plus 23 palette reductions
at 0.1 each: **26.3 generation units**. The 30-unit initial cap and 60-unit total
are respected; the reserve is untouched. Sword recoloring used free exact PixelLab
operations. All sources, job IDs, prompts and corrections are in
`ArtSource/UnifiedCombat/`. `FinalChecks.json` records actual native dimensions,
alpha, palette counts and hashes; `NativeReview.png` shows 1×/4× comparisons.
Finals use 2–12 opaque colors and binary alpha, Point import, no mipmaps/compression.
The 24px files are copied unchanged into `Resources/UI/CombatStatuses`.
