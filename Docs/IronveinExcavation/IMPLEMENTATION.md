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
