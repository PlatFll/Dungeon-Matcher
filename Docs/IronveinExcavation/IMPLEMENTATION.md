# Ironvein implementation contract

## Phase 02 — stone foundation

The additive `ironvein-excavation` definition is available through the temporary
testing picker. `eligibleForTesting` is separate from live crystal eligibility:
the unfinished destination cannot be selected by a live crystal. Its current
existing shell artwork and dungeon scenery are development placeholders;
the native roster and cave are later phases, not approved mine art.

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

Current presentation is a labelled fallback: different solid stage colors, using
the existing materialization/hit feedback. Native shapes, durability/aging display,
drill hardware and full cave art remain pending production.

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
