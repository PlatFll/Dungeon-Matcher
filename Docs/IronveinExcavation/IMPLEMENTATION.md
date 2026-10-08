# Ironvein implementation contract

## Phase 02 — stone foundation

The additive `ironvein-excavation` definition is available through the temporary
testing picker. `eligibleForTesting` is separate from live crystal eligibility:
the unfinished destination cannot be selected by a live crystal. The original
foundation used labelled shell artwork. Phases 08–09 replace that shell with
native roster and cave candidates for review; live encounter readiness is Phase 10.

BoardController owns mine material history and uses the existing barricade
dictionary for occupancy/durability. `MineStoneState` stores stable stone/source
IDs, stage, birth action, last hit and ignored accepted moves. Brittle/Hardened/
Obsidian use 1/2/3 durability. Three ignored moves advance one tier and restore
that tier's durability. A hit resets age and prevents same-action hardening.
Damage does not downgrade the material. Standard clear-hit deduplication and
existing structural modifiers remain; the future small drill's fixed one-hit
operation must bypass those modifiers explicitly.

Safe placement protects specials and warning responses, respects source/global
caps and retains useful board responses. `maximumMineStones` is six provisionally.
Placement, aging and settlement use the existing mutation queue. The zone state
survives waves and has a versioned continuation payload. Newer payload versions
and malformed stone identities/durability are rejected. Legacy non-mine saves
need no new payload.

Photographs preserve current mine stone cells, durability, stage, age and source
identity; they cannot resurrect broken/extracted stones or erase later placements.
Travel cleans source stones on the detached destination checkpoint, with ordinary
gem restoration through the existing arrival path. The live source remains intact
if the destination write fails. Actor source IDs are independent of photograph
instance keys and continuation slot keys.

The initial presentation used labelled solid-color fallbacks. Phase 09 adds
distinct native materials/damage art, imminent-hardening sparks, fuse overlays
and drill hardware while retaining the existing materialization/hit feedback.

## Phase 03 — fixed drills

Two persistent board-owned drills have stable IDs: 1 enters a row from the left,
2 enters a column from below. The initial lanes are the middle row and column.
Each has four charge pips. A deliberate opening match involving either of the
first two cells inside an intake contributes one charge, at most once per drill
per accepted move. Cascades, specials, free abilities and machine clears do not
feed this rule. Explicit enemy feeds can select one machine or all current
machines. Charge saturates at one launch; excess does not create hidden shots.

The move coordinator drains charged machines after actor opportunities. Off-turn
feeds drain under the mutation queue before its ownership ends. Machines reserve
charge before firing callbacks, launch in stable-ID order, and clear whole lanes.
Intersecting cells clear once. There is no refill between passes; one canonical
environmental settlement follows the batch. No actor damage is dealt by machines.

This environmental source removes crossed special gems without activation.
Its refill cascades use the same resolver with combat reporting, special creation
and special activation suppressed for that transaction. No energy, HP damage,
healing, shield grants or recursive drill fuel can come from the machine clear.
Other environmental operations and all player clears retain their established
semantics. The small Bore projectile is a separate later-phase operation.

Mine snapshot v2 saves machine lanes, charges and the last contributing manual
action. Foundation v1 saves initialize the two machines once. Photos keep the
current network and cannot refund spent charge; zone travel drops the source
network. The existing layout owners reserve optional perimeter space only for
the mine theme. `MineEnvironmentView` observes state and shows fallback machine
shapes, pips and brief sweeps; it owns no charge, targeting, timing or damage.

## Phase 04 — ore network and normals

Pickaxe Delver, Rivet Gunner and Packbeetle have separate data definitions and a
three-actor testing fixture. Existing shell art remains temporary until Phase 08.
Weapon eligibility is declared in EnemyDefinition; eligible weapons receive one
non-stacking 1.3× next-basic token. Reapplying keeps the stronger pending value.
EnemyAutoAttack consumes it once for the whole sequence, including follow-ups;
the existing central damage and five-point rounding paths remain authoritative.
Pending tokens survive Continue and are not refunded by board photographs.

Packbeetle subscribes to authentic defeat, claims its once-only guard before
callbacks, empowers all currently living eligible allies and queues one charge
to every existing fixed drill. No destroy/animation callback triggers gameplay.
A fourth charge can fire the normal penetrating drill and clear player obstacles.
Living-source stone limits also retain their saved slot binding while Continue
restores actor persistent IDs, closing a source-cap bypass.

## Phase 05 — seven specialists

`MineEnemyAbility` selects behavior from definition data and retains immutable
target identities, lanes and accepted-move deadlines. Basics pause during held
warnings; death/Stagger cancels them without a separate recovery timer. Missing
or changed Assay targets fizzle. Source sigils, actor countdowns and optional
board highlights observe the saved intent. Native effects remain Phase 09 work.

- Hauler powers existing eligible allies and alternates one drill feed.
- Stonewright's floor contact queues two safe Brittle placements immediately.
- Bore scans rows then columns in stable index order, preferring fewer stones.
  Its two-move warned projectile skips specials and stops at its first stone,
  dealing exactly one durability even when that destroys it. Only an unobstructed
  shot reaches the player. Each launch gives the corresponding fixed drill one
  charge. Enemy clears and refill cascades grant no player rewards.
- Surveyor marks one non-Obsidian stone for one move. If another effect already
  changed its stage, the cast fizzles; it does not advance a replacement target.
- Sapper's one attached charge has a two-move deadline. Deliberate adjacent
  matches and special clears defuse without a host hit. Cascade damage can still
  hit the stone normally. Host destruction cancels the charge. Expiration deals
  exactly two durability to the current material and one central player packet.
  Structure-damage upgrades do not multiply either the small drill or explosion.
- Switcher warns one adjacent lane, preserves charge and rechecks the one-step
  move when its queued mutation executes. It cannot retarget a firing batch.
- Smith applies shared Rattled: half new Stagger buildup for two accepted moves.
  Existing meter/duration, forced Stagger, damage and energy are unaffected.
  Neither a stone hit nor a drill shot cleanses it. Continue preserves it.

Enemy and board snapshots retain charge source, fixed stone ID, planned stage,
drill target and due move. Future enemy payload versions are rejected safely.
The Rattled icon temporarily reuses Weakened's glyph pending dedicated mine art;
its name, inspection description and remaining-move count identify the effect.

## Phase 06 — machinery minibosses

Siege Machinist alternates building and priming through the established summon
service. Each builder records up to two living turret persistent IDs. A full
formation with no owned turret defers without replacing anyone or spending its
ready counter. Prime affects only that builder's current living turrets and
feeds one fixed drill. Turrets retain their ordinary actor identity, HP, basic
timer and ore token after builder death and Continue. They remain in the wave
gate; the existing completed-wave economy grants no separate summon income.

Obsidian Sentinel alternates fixed-stone extraction and Hydraulic Slam; with no
stone, it can warn a Slam. Extraction consumes the target's **current** material
through the board queue, with no clear rewards. Provisional next-basic strengths
are 1.3/1.6/2× for Brittle/Hardened/Obsidian. Hardened/Obsidian also grant one
Fortified (shared cap two); Obsidian arms one 1.5× Slam. All pending power saves.
Losing the extraction target fizzles without awarding power or forced Stagger.

Slam waits two accepted moves. An actual full drill firing while warned cancels
it and requests ordinary two-move Stagger through EnemyStagger, respecting its
immunity. Charge increases alone do not interrupt. The coordinator drains drills
before committing a due Slam, so the final response move remains useful; the
same accepted-action hold covers the surviving Slam's impact and recovery. No
independent board resolver or post-channel recovery timer was added.

The mechanics proof gallery is `.utmp/Ironvein/Review.html`. Its existing shell
artwork and dungeon backgrounds are explicitly temporary, not approved mine art.

## Phase 07 — Grand Delver mechanics

The default suit cycles Claim the Vein (two Brittle stones), Full Steam (one
next-basic ore token and one charge to each fixed drill) and Heart of Obsidian.
Heart places a distinct tagged three-durability Core through normal safe
placement. Its fixed cell and three-move deadline survive Continue. Ordinary
structure hits or a full drill through the cell can break it; breaking cancels
the attack and requests ordinary Stagger. The last response move's drills resolve
first. A surviving Core is consumed without player rewards for one centrally
rounded 45-base-damage packet. External interruption releases the ritual tag,
leaving ordinary Obsidian; other stone specialists cannot steal an active Core.
A full board makes the boss use Steam instead of stalling on placement.

`mineEnableRemount` is **false by default**, as the pack calls this experimental.
When enabled before spawn, the first lethal suit hit transitions the same actor
to PilotFoot before its authentic Defeated event. No reward, slot release or wave
advance occurs. Ejection absorbs remaining callbacks in that resolution; the
pilot is targetable at the next settled input boundary. Poison, Stagger and other
existing actor effects retain their own ownership. Any next lethal pilot hit is
final. Surviving three future accepted moves creates one SecondMech, whose death
is always final. The flag, phase, timer and remount guard are saved, independent
of later changes to the development toggle.

Provisional profiles at base depth: first suit 350 HP/15 rounded damage/6.9s;
pilot 20% HP (70), half rounded basic damage (10), 0.7× interval (4.83s);
reserve suit 50% HP (175), 0.8× basic damage (10), original interval. Both derive
from the original globally scaled stats, without player-power correction.
The pilot has ordinary basic pressure and a visible REMOUNT IN counter, no
mechanized specials. Every phase uses one persistent ID. Whole-formation death,
including surviving escorts/turrets, remains the existing wave/travel gate.

The generic actor exposes an optional pre-final-defeat phase hook and phase-stat
application; mine-specific decisions remain in MineEnemyAbility. Missing motion
cannot own phase state. Actual ejection/remount art is still Phase 08 work.

## Phase 08 — native roster motion

Fourteen identities now have separate original stills and material-cleaned
working selections under `ArtSource/Ironvein`. These are internal production
candidates, not user-approved replacements. The review/technical register and
generation receipts retain originals, rejected corrections and exact maps.
Small action inputs use 80×80; the two large machines use 112×112. Padding adds
headroom without resampling or changing drawn pixel scale.

The existing auto-attack owner retains modifier-source identity during one active
sequence, after consuming its saved token. Presentation can therefore select
`OreChargedAutoAttack` for both hits of that sequence, then return to normal.
This bookkeeping neither adds nor repeats damage. The shared motion importer
accepts an explicit attack flag for normal event timing on named alternate clips.

Fourteen padded stills are imported with Point filtering, full rectangles,
bottom-center pivots and the same native texel scale. All fourteen identities
have Idle/basic/Hit/Death; separate kit gestures, held warnings and ore-powered
attacks bring the set to 108 clips including the optional pilot/reserve forms.
Grand Delver's wide attacks use 160×112 canvases with the same center/floor.
`Production/motion-selections.json` records the exact original
frame sequence/timing; the exporter verifies unchanged RGBA bytes per frame.
The native gallery pairs 1x and 3x presentation and records dimensions, occupied
bounds, alpha, palette counts and hashes independently from the preview.

Multi-action kits select separate optional gesture state names. The same saved
Grand Delver actor selects the imported pilot/reserve controllers; Continue binds
the saved form without replaying ejection/remount. Terminal phase motions return
to Idle. Missing presentation leaves gameplay intact.

The move coordinator waits for a mine kit's pending coroutine as well as its
actor/board action. The board can finish its animation before the kit resumes on
the next frame to clear its saved warning and basic hold. Keeping this brief
cleanup inside the accepted action prevents a false stable-save window.

## Phase 09 — native cave and presentation

Three additive battle compositions use a receding rail tunnel, separate timber,
ore, rail/cart, lamps and machinery modules. The existing unit Grid, Tilemap
roles, viewport mask and battle-floor anchor remain authoritative. A subdued
charcoal surround and a separate steel inset theme the gameplay screen. Native
80-pixel L corners, 64x16 edge strips, 128x32 plaque, 144x32 energy frame and
146-pixel player slices match the existing layout. The original player, skills,
gems, neutral HP family, ranks and opened settings/menu assets remain bound.
The theme reserves .8 world units outside its frame for the two drill housings.

Small drill presentation receives the board's actual stop cell; it cannot choose
or change collision. Fixed drills show a full-lane travelling head/shaft and
retraction, alongside their native spinning housing and four charge pips. Amber
intake brackets identify their first charging cell. Damaged Hardened/Obsidian
and Core art retains each material. Imminent maturation, fuse and Core break
effects observe board state. Pausing freezes animation time, disabling the view
cleans its transient objects, and missing art never blocks a board operation.

Actor presentation observes ore grant, actual basic contact and boss phase
events. All sounds use the existing bounded six-voice mixer, pause/restore
suppression and independent SFX setting. The original temporary 120-second cue
uses the current zone music owner. Source score, signal checks and all PixelLab
receipts are retained. Human art/listening approval remains pending.

## Phase 10 — weighted visits and continuity

`ZoneRuntimeContext` asks `MineEncounterSelector` for a legal authored formation.
The existing WaveController still spawns actors, assigns weaknesses, scales from
global depth and owns rewards. Forty-one recipes use the existing
`WaveSpawnProfile.ThreatBudget` evaluator with a mine-local budget asset; maximum
three slots, two disruptors, one support and one leader. Turret is summon-only.
New kits receive solo or normal-escort lessons; mixed formations require their
members to have appeared. Recent four labels reduce repetition. Actual spawned
identities are recorded on WaveStarted, not merely when a candidate is picked.

Provisional teaching windows: Stonewright 3–4, Hauler 5–6, Machinist 8–10,
Surveyor 11–12, Sapper 13–14, Switcher 15–16, Bore 17–18, Smith 19–20,
Sentinel 22–24, Grand Delver 28–30. Deadlines ensure an unseen mechanic appears;
ordinary waves remain weighted and leaders appear once per visit. The next
encounter after a miniboss is normal-only. This connected-visit rule follows the
existing apex rematch model; it does not change the dungeon's opening leader rules.
No elapsed-time gate, player-power correction or additional HP multiplier exists.

The first accepted action from local encounter two can place one natural Brittle
stone per visit. BoardController executes it inside its existing environmental
mutation, using the same capacity, special protection and useful-response checks
as enemy stones. It has stable board ownership zero and is never a hidden actor.
Failed placement retries at a later safe point. The successful-introduction flag,
recipe history and seen identities persist through Continue.

Ironvein is a fourth live crystal destination. Selection remains random and
excludes the source. The entire apex formation must die before rewards and travel.
The detached destination checkpoint clears mine structures, charge and source
warnings, resets the new visit history, and preserves run/player/card/gem state.
Failed writes leave the source unchanged. Returning to the mine starts a fresh
local teaching arc at the current global difficulty. Cave variants advance from
railhead to pumpworks to deep shaft at local 10 and 20.
