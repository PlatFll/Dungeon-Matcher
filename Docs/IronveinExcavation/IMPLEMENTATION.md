# Ironvein implementation contract

## Phase 02 — stone foundation

The additive `ironvein-excavation` definition is available through the temporary
testing picker. `eligibleForTesting` is separate from live crystal eligibility:
the unfinished destination cannot be selected by a live crystal. Its current
Farmer and dungeon scenery are explicitly labelled development placeholders;
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
