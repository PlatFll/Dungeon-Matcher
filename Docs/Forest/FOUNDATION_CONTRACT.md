# Forest development contract — Phases 5–6

New isolated forest tests use `seconds-basics-move-abilities-v1`: ordinary attacks
count seconds; special abilities count accepted player moves. This follows the
user's Phase 5–6 timing revision. Version-2 `accepted-moves-v1` saves keep their
earlier all-move interpretation. Production dungeon runs and version-1 saves keep
their existing effect units. Dungeon and forest are now eligible for live travel
after apex formations; isolated tests retain their own loop. The additive
`seconds-effects-move-abilities-v1` travel profile keeps seconds effects and adds
move-based forest ability coordination. See [crystal travel](CRYSTAL_TRAVEL.md).
Six starter kits, battle motion, modular art and temporary audio are now included.

## Work sequence

The Phase 4 foundation remains the action/save owner. Phase 5–6 adds the two
milestone kits and native art through these owners. The user subsequently approved
Treant C, root/vine art and motion, connected travel and merging this work.
The seven additional stills remain unanimated and without kits; final music,
full roster and later-zone production still require their own review.

## Action resolution

The board alone accepts legal manual swaps. Each acceptance has the board's next
completed-move ID and a snapshot of the currently living actors. Failed swaps,
individual cascades, free abilities, supplies and environmental board work do not
create clock ticks. The clock holds an external input token from acceptance until
all consequences settle; board mutation remains owned by BoardController.

Complete player clears/resources/damage first. Apply deaths and real stagger
interrupts as they occur. At settlement commit that action once; process existing
poison, the due normal vine-growth pulse, then surviving actors in persistent
spawn-ID order. Each actor resolves
its channel or due special. Under the retained all-move profile, at most one
ordinary action is also offered per actor per tick. In new hybrid tests, ordinary
attacks count seconds between moves; board acceptance pauses new countdowns until
settlement. Previously accepted attacks drain before move-owned special work.
A committed multi-hit or command sequence remains one action
with separately shield-gated hits. Wait for authoritative board mutations and
accepted presentation sequences before advancing to the next actor. New actors
receive no readiness on their spawning action. Dead actors/targets are rechecked.

After due work, expire move durations and release the input token. Death stops
later offensive work. The wave gate waits for the entire action. Free actions can
damage, heal, interrupt or clear threats but cannot advance deadlines/recharge.

## Explicit initial timer settings (tunable)

- New forest basic seconds: Scout 3, Trailguard 4.5, Mender 5, Rootbinder 5.5,
  Warden 4.5, Matriarch 5. These are authored test intervals, not a conversion from
  an assumed matching speed. Held progress resumes without resetting/catching up.
- Retained all-move saves use the existing explicit first/repeat move fields.
- Royal Decree: 3 full manual actions, +1 for Longer Reign (maximum 4). Base
  bonus 5 damage per eligible gem, no cascade multiplier in this prototype;
  existing damage cards still apply. Normal match energy remains suppressed
  during the full action, including its cascades; explicit explosion entitlement
  and refund caps remain. Expire only after the final action's rewards.
- Stagger: rank thresholds unchanged; block the triggering action and one full
  future action. Then 2 future actions of immunity. Partial meter has one full
  future-action grace after a hit and loses 25 percentage points per subsequent
  unaddressed action. Real time changes none of these values.
- Poison: 3 future ticks, one per manual action, +1 for Slow Venom. Reapplication
  refreshes remaining ticks without a second tick or postponing an existing tick.
  Existing status damage and non-staggering semantics remain.
- Supplies: independent 2-future-move cooldowns, starting only on accepted use.
- Marshal rally: 3 future moves; speed multiplies readiness progress per move,
  with at most one ordinary sequence per tick and no excess progress banked.
  Banner and enrage retain their owner/threshold lifetimes.
- Shields have no clock expiry. Benediction lasts one whole attack sequence.
  Persistent cards, per-wave/per-cast limits, energy and affinity retain their
  existing event-driven owners. No passive recharge is introduced.
- New move-timed effects receive their full future allowance; expiry runs after
  due work. Animation, VFX, death recovery and scene transitions use seconds.

## Save representation

Version 1 keeps the legacy seconds interpretation. Version 2 declares profile,
zone ID, completed action, next persistent actor ID, per-actor move state and
owned pending work. Stable snapshots remain outside an action; interruption
inside an action recovers through the existing accepted-input replay journal.
All-move thinking skips idle journal frames. Hybrid runs retain frame deltas
because seconds attacks can occur while the player thinks. Stable checkpoints
use the existing journal compaction. Unknown
profiles/versions retain the durable original and report incompatibility.

Channel targets use persistent actor IDs, never recycled slots. Captured board
owners still use the established slot reconstruction within a single snapshot;
vine records retain source, root identity and cell coordinates. Root state and the
normal growth deadline are saved by the board.
Gideon's board photograph does not rewind combat clock or proc budgets.

## Forest mechanics

The reviewed starter kit sheet supplies exact forest values. Mender has no
self-targeting: a fixed living ally below 75% receives +20 only after two future
accepted actions. Stagger/kill cancels once and recipient death fizzles. Each
terminal outcome returns directly to normal readiness. It cannot attack during
the channel; ordinary Stagger alone controls genuine interruption, without an
additional recovery penalty. Old recovery saves migrate to idle without healing.

Vines are cell overlays. They obscure some of the gem without blocking swapping,
gravity, matches, bombs or destruction. Clearing the covered gem removes its vine.
Normal growth pulses every two accepted moves, before enemy casts: one edge seed
and at most two frontier additions, with a 12-overlay growth cap (October 4
temporary tuning). Surge shares the two-addition limit. Existing saved overlays
remain until cleared; growth pauses while at or above the new cap. A pulse uses
the original frontier, never recursive same-pulse spread. Root placement always
seeds its four orthogonal neighbors, independent of that growth cap. These are
prototype cadence/readability values, not finalized pacing. Real chains retain
their existing six-restriction cap; vines consume none of it.

Rootbinder warns one ordinary, reachable interior gem for a future response,
then replaces it with a structural 1-hit Root. Four immediate vines never spread.
First clear a vine beside the Root; a later clear through an unvined, opened side
deals one durability hit. The opening clear cannot also damage that Root.
Roots use the existing structural queue, useful-response checks, cap and refill.

Warden first readies after two moves, warns one cell, then plants a 2-hit Root
with four spreading vines. While it lives, every ally including Warden receives
one 25% incoming-damage reduction through central damage resolution. Multiple
Wardens do not multiply this shared reduction. Breaking the Root removes the aura.
Clearing its warning or normal stagger interrupts preparation. Invalid targets
fizzle without granting stagger. There is no exposure state or weakness bonus.

Matriarch first readies after three moves. Renew plants two linked 2-hit Heartroots
when none remain, then channels for two future moves. She heals herself and every
living ally for base 20 plus 20 per Heartroot surviving at resolution. Her cycle
then uses Verdant Surge (immediate existing-vine advance) and Thorn Harvest
(three future moves; base 20 damage plus 5 per currently vine-covered gem).
Harvest consumes all vines, leaving Heartroot occupancy and durability intact.
Surge never resets or delays the normal growth deadline. Both Heartroots fully
destroyed causes ordinary EnemyStagger, respecting its existing immunity rules.
Surviving spreading roots can regrow vines on the next normal pulse.

Successful casts and cancellations consume their sequence before effect callbacks.
After a milestone channel ends, normal three-move readiness begins immediately.
Basics pause during preparation/channel and owned animation, with no additional
post-channel recovery moves. Scout and Trailguard retain their
existing normal attacks. Mender retains her fixed-recipient two-move +20 heal;
recipient invalidation never retargets or auto-staggers her. All true interruptions
use EnemyStagger; no forest-specific stagger or vulnerability state exists.

Root IDs, remaining durability, opened sides, owner, spread policy, overlays,
normal growth deadline and milestone cycle/sequence are saved. Legacy vine pins
migrate to overlays; old nonspreading anchor casts retire with a harmless fizzle.
Real chains remain intact. Photographs keep the present growth deadline and root
durability/open sides, cannot resurrect destroyed roots or dead producers, and
do not rewind channels or consumed effects. Root networks leave with their owner;
environmental vines leave at wave/zone cleanup. Optional art never owns effects.

Forest resonance multiplies Emerald damage by 1.15 before target modifiers and
final five-step rounding. Eligibility is the attributed Match/Special/Ability damage packet through
CombatController. Separate Royal Decree proc hits, poison ticks and fixed enemy
damage are excluded; they do not silently inherit the color of a mixed clear.
Existing integer rounding in CombatController precedes actor-level rounding to
five, so a 30-damage Emerald packet resolves to 35; a Ruby packet remains 30.

## Evidence required before completion

Failed and special swaps; long cascades; free skill chains; no-move wall time;
deadline lethal/stagger; target/caster death and slot reuse; simultaneous due
work; shared cap saturation; producer/environment cleanup; pause/resume and
mid-action replay; legacy dungeon regression; missing optional art fallback.
Automated behavior and rendered checks do not establish human fun or balance.
