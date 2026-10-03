# Forest development contract — Phases 5–6

New isolated forest tests use `seconds-basics-move-abilities-v1`: ordinary attacks
count seconds; special abilities count accepted player moves. This follows the
user's Phase 5–6 timing revision. Version-2 `accepted-moves-v1` saves keep their
earlier all-move interpretation. Production dungeon runs and version-1 saves keep
their existing profile. Forest is not eligible for live travel or release.
Six starter kits, battle motion, modular art and temporary audio are now included.

## Work sequence

The Phase 4 foundation remains the action/save owner. Phase 5–6 adds the two
milestone kits and native art through these owners, then stops for art, motion,
kit and music review. No merge or next-phase authorization is implied.

## Action resolution

The board alone accepts legal manual swaps. Each acceptance has the board's next
completed-move ID and a snapshot of the currently living actors. Failed swaps,
individual cascades, free abilities, supplies and environmental board work do not
create clock ticks. The clock holds an external input token from acceptance until
all consequences settle; board mutation remains owned by BoardController.

Complete player clears/resources/damage first. Apply deaths and real stagger
interrupts as they occur. At settlement commit that action once; process existing
poison, then surviving actors in persistent spawn-ID order. Each actor resolves
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
vine records separately retain source, network identity, age and target gem ID.
Gideon's board photograph does not rewind combat clock or proc budgets.

## Forest mechanics

The reviewed starter kit sheet supplies exact forest values. Mender has no
self-targeting: a fixed living ally below 75% receives +20 only after two future
accepted actions. Stagger/kill cancels once, recipient death fizzles, and 2 future
recovery moves follow every outcome. It cannot attack during channel/recovery.

Vines share canonical movable pins: block swapping, fall with gems, remain
matchable and are removed by clear/conversion. Enemy roots die with their owner;
environment roots live until wave/zone cleanup. A network has a two-action growth
age with a visible last-move warning, at most one new vine globally per action,
no same-tick recursion, and shared six-restriction capacity including reservations.
Only ordinary, safe orthogonal neighbors qualify. Player specials and reserved
counterplay routes are protected. Clearing/conversion removes that node and its
pending growth; new growth receives its own full grace.

Resonance multiplies only eligible damage attributed to the current zone gem by
1.15, after generic gem modifiers and before target modifiers/final five-step
rounding. Other colors in a mixed clear are unchanged. Weakness and player
affinity remain independent. A generic current-gem event is a card hook only;
full offer/proc integration remains Phase 7.

Warden: first readiness after 2 moves; warn two safe cells for one future response.
Surviving anchors are nonspreading, share the six-restriction cap, and reduce all
incoming damage by 25% once, regardless of anchor count. Clearing the last anchor
or staggering the preparation grants 2 future moves of +25% weakness damage.
The clearing packet itself does not get the new bonus. Recovery then resets the
special to 3 moves. Basics pause during preparation/exposure/recovery.

Matriarch: first readiness after 3 moves; create up to two nonspreading anchors,
choose the lowest-health-fraction other ally strictly below 75%, or self if none,
and fix its persistent identity for 2 future response moves. At completion heal
10 per surviving anchor, at most 20 and capped to missing HP. Clearing both or
staggering cancels and exposes; losing the recipient fizzles without retargeting.
Every outcome has 2 future exposure/recovery moves; recurrence then takes 3 moves.
Consume the cast sequence before healing callbacks. Removing the owner/encounter
removes its anchors. Photographs cannot resurrect a finished cast.

Eligibility is the attributed Match/Special/Ability damage packet through
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
