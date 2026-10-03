# Phase 4 development contract

This is an internal accepted-move prototype. Production dungeon runs and version 1
saves keep their real-time profile. Forest is not eligible for live travel.
No public mode selector, full animation production or release is part of this phase.

## Work sequence

1. Preserve approved stills and verify material palettes/shading.
2. Add and test the action identity, profile and save contract.
3. Migrate each gameplay timer under the isolated profile, retaining legacy behavior.
4. Add the zone context, attributed resonance, channels and canonical vines.
5. Provide the direct development forest fixture and readable intent presentation.
6. Run focused tests, dungeon regressions, Unity validation and rendered scene checks.
7. Open a focused PR and stop for technical/playable review. No merge.

## Action resolution

The board alone accepts legal manual swaps. Each acceptance has the board's next
completed-move ID and a snapshot of the currently living actors. Failed swaps,
individual cascades, free abilities, supplies and environmental board work do not
create clock ticks. The clock holds an external input token from acceptance until
all consequences settle; board mutation remains owned by BoardController.

Complete player clears/resources/damage first. Apply deaths and real stagger
interrupts as they occur. At settlement commit that action once; process existing
poison, then surviving actors in persistent spawn-ID order. Each actor resolves
its channel or due special before an ordinary basic. At most one ordinary action
per actor per tick; a committed multi-hit or command sequence remains one action
with separately shield-gated hits. Wait for authoritative board mutations and
accepted presentation sequences before advancing to the next actor. New actors
receive no readiness on their spawning action. Dead actors/targets are rechecked.

After due work, expire move durations and release the input token. Death stops
later offensive work. The wave gate waits for the entire action. Free actions can
damage, heal, interrupt or clear threats but cannot advance deadlines/recharge.

## Explicit initial timer settings (tunable)

- Every definition has explicit first/basic move cadence; no seconds conversion.
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
The journal must not grow indefinitely while the player merely thinks. Unknown
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
