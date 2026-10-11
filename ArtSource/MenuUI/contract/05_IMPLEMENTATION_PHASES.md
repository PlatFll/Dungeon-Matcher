# IMPLEMENTATION PHASES

## Phase 1 — Audit and plan
- locate existing menu and pause/settings screens in project
- identify current prefabs / scenes / UI structure
- identify what can be replaced safely
- identify which elements should remain functional text vs baked art
- identify risks before editing

## Phase 2 — Main menu art build
- reconstruct main menu UI elements
- create / clean background art
- create button frames
- integrate into main menu scene
- align spacing and composition to reference
- verify at native scale

## Phase 3 — Pause/settings modal build
- reconstruct modal frame
- create settings row modules and toggles
- create action button variants
- integrate overlay and dimmer
- verify readability / spacing / button hierarchy

## Phase 4 — Shared general UI documentation
- document which parts are now the global standard
- note where later battle-zone UI should inherit from this pass
- list reusable assets clearly

## Phase 5 — Validation
- produce before/after screenshots
- compare against references
- confirm no blur / no scaling bugs / no clipping / no anchor issues
- confirm menus still function

## Stop conditions
Astra must stop and report clearly if:
- it cannot identify the correct UI scene/prefab,
- PixelLab output is too poor and needs approval,
- any asset looks blurry or off-style,
- the scene layout cannot match the reference without risky structural changes.
