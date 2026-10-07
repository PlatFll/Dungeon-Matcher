# The Drowned Court — implementation contract

The user approved the first four designs, solid-color eyes, continued production,
and a separate ceiling of 300 subscription generations (220 initially, 80 reserved).
The remaining ten designs and motion are director-selected for review. This does
not imply that the user has reviewed every later asset. This baseline was merged
in PR #180. The 2026-10-06 status/sigil/culture revision was merged in PR #181.
The separate roster/endless revision is in progress; see [its worklist](ROSTER_ENDLESS_REVISION.md).

## Environment and timing

The Sunken Marches use coral ruins, marine stone, shell details and distinct
gameplay panel materials. Reefgate, Coral Cloister and the throne approach are
three compositions of the same native modular art. Main menu and opened options
retain their existing presentation. Original music is temporary pending listening
review. Flooding changes its mix without restarting the track.

Basic attacks count seconds. Abilities count accepted manual moves. Failed swaps,
cascades and free abilities/supplies do not spend response moves. Travelling runs
retain their recorded units for buffs, poison, stagger and supply cooldowns.

The first complete formation is dry. Six accepted dry moves request flooding;
the request waits until the entire current formation, including living summons,
can fight underwater. Global enemies default to dry-only. The first flood also
waits for a formation without an air thief, snare, inflation or pressure lesson.
Each flood lasts a saved random 16–18 accepted moves. After draining, the next new
complete formation remains dry. Waves and perk screens never reset the tide.

Flooding starts with five AIR blocks and up to five reachable bubbled gems. For an
accepted wet move, AIR is computed once after player clears:

`clamp(max(0, openingAIR - 1 - manualSnareClears) + 2*bubblesPopped + cofferAIR, 0, 5)`

Free player actions may recover AIR without spending a move. Bubbles belong to
physical gem identities: conversion into a special preserves the bubble; actual
player destruction collects it once. Environmental capture and cleanup give no
collection reward. These bubbles are the initial finite reserve: free bubbles plus
captured coffer charges count, so theft cannot cause a replacement supply. After
all reserve oxygen is resolved, a three-move emergency timer begins. Each pulse
tops up to one bubble, or two at AIR ≤1, never more than two. All values are
provisional fields on the zone definition. Low AIR may relocate an existing free
unmarked bubble to an immediately usable ordinary gem, without adding oxygen.
The AIR display presents the same receipt as accounting: debit first, then each
actual +2 bubble or coffer payout. It never changes a bubble into +3.
Legacy saves preserve remaining flood time and current resources; new reserve
rules take over without resetting the flood or minting oxygen.
After enemy actions, zero AIR causes one five-damage shield-gated suffocation hit.
The last wet move drains before this check, so it does not suffocate.

Only one hostile Air Coffer may be active. A cast announces its fixed capture gems
and footprint; placement is revalidated without removing the remaining AIR answer.
Ordinary damage breaks its one/two durability. Break or owner death returns its
stored charges once; drain and zone exit discard it without AIR or damage rewards.
Board photographs preserve current AIR, consumed charges and deadlines.

## Roster and initial control values

These numbers are starting values for human tuning, not pacing guarantees.

| Enemy | HP | Basic damage / seconds | Ability |
| --- | ---: | --- | --- |
| Reef Spearman | 60 | 10 / 4.8 | Trident thrust |
| Hammerhead Bruiser | 110 | 20 / 6 | Heavy club strike |
| Needlefin Skirmisher | 50 | 5 + 5 / 4.2 | Two independently shield-gated darts |
| Shellback Porter | 120 | 10 / 6.5 | Mallet; no hidden armor |
| Pearl Thief | 65 | 5 / 6 | Ready in four moves; two-move theft warning, up to two bubbles, one-hit coffer; waits while dry |
| Pearl Cantor | 75 | 5 / 5.5 | Ready in two moves; two-move fixed-ally heal for 20, other ally below 75%; stagger interrupts |
| Conch Marshal | 90 | 10 / 6 | Ready in four moves; other living allies gain 30% basic damage for five seconds; refreshes, strongest lease only, no speed buff |
| Moray Siphoner | 80 | 5 / 6 | Ready in four moves; two-move 20-damage siphon; heals actual HP lost only |
| Reef Netweaver | 85 | 5 / 6 | Ready in four moves; up to two thorny snares, shrink and expire after three future accepted moves |
| Puffer Sentinel | 100 | 10 / 5.8 | Ready in four moves; inflated for two moves, keeps timed basics; opening manual-match damage retaliates for five once per action |
| Breakwater Captain | 180 | 20 / 7 | Alternates 25-shield Shellguard and a two-move command for one fixed ally's complete basic sequence |
| Lantern Warden | 210 | 15 / 6.5 | Alternates two-hit Air Levy (dry: 25 shield) and two-move Pressure Lance, 25 damage reduced to 10 by its answer |
| Queen Nacre | 320 | 15 / 6.5 | Royal Seizure, Crushing Depths, Court Muster; details below |
| Skittercrab | 25 | 5 / 2.8 | Independent summon, no repeat farming reward |

Netweaver uses thorny snares, never ordinary chains. An opening manual match of a
snared gem while flooded costs one AIR; cascades, abilities and specials clear
safely. Snares prevent directly swapping the bound gem, follow it through gravity,
and use the existing shared six-restriction budget. Placement preserves available
AIR routes. The owner may have at most two.

Milestones first ready after three moves and require three fresh moves after a
cast ends. All Court channels return directly to normal readiness on success or
fizzle. Genuine interruption uses ordinary Stagger without an extra recovery
penalty. Old recovery saves resume idle without replaying effects or rotation.
Death/stagger cancels held actions;
missing optional animation still resolves through the existing guarded fallback.

Queen Nacre stays damageable throughout. Seizure warns for two moves, captures
bubbles in a two-hit coffer, and preserves a reachable rescue; while dry it grants
20 shield. Depths warns for three moves and deals 40/25/10 according to zero/one/two
answers. Two fixed cell marks are shown; while wet, bubble recovery also answers,
and a coffer answers both. One physical gem cannot count twice. Muster warns for
two moves, names an empty slot and summons at most one owned living crab. A full
formation announces a fixed ally's 20-shield Royal Guard instead. It never replaces
an actor or secretly retargets a lost slot. Killing the Queen leaves her crab alive.
The entire apex formation must die before travel.

## Encounters, travel and continuation

The approved two-culture revision uses existing faction data: ten Reef Clan
identities and four Nacre Court identities. Production groups are predominantly
coherent, with two deliberate mixed templates. Queen Nacre retains Court escorts.
See [the encounter record](DROWNED_COURT_ENCOUNTERS.md) for source tables,
teaching changes, lore and provisional balance limits.

Thirty-two weighted recipes cover ordinary, specialist, mixed-pressure and return
formations. Recent formations are discouraged. Local bands and the twenty-wave
apex anchor are temporary content controls, not finalized deterministic scripts.
No forced wait, player-power correction, HP padding or run-ending wave cap is added.
Approximately ten-minute substantial visits still require human pacing evidence.

The testing picker includes Drowned Court. In-run crystal travel chooses randomly
among eligible other zones. Dungeon, forest and Court share the existing atomic
handoff: clean source effects on the detached destination snapshot, commit it, then
reveal. A failed write leaves the source unchanged. AIR, snares, coffers, pressure
marks and water do not leak into another zone. Continue restores the recorded zone,
tide, gem identities, counters, targets, rally leases and response deadlines.

## Art and evidence

`ArtSource/DrownedCourt/Approved` preserves the four corrected references and exact
eye edits. `Production/Selected` contains the selected stills, native motion sheets,
GIF previews and technical manifests. Raw inputs, generation job records and
selection scripts remain separate. Native preparation uses crops, palette mapping,
frame selection and canvas padding; it does not resample production sprites.

Executed tests, captures, budget reconciliation and remaining human/device checks
belong in `Docs/Validation/DROWNED_COURT.md`.
