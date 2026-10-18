# Approved dungeon art in the live battle

The approved side-view kit from PR #201 now decorates the production `Game` scene
on `codex/dungeon-approved-battle-integration`. **User approved PR #202 for merge on 2026-10-19.**
Open [Review.html](Review.html) for real before/after Game View captures.

## What changed

The prior scene still selected the older perspective kit. A resource binding now
references all 119 original approved modules by GUID. The existing presentation
owners consume the barred doorway, shallow platform, separate warm lighting and
props, cooler portrait backing, player/wave/board frames and expanded corners.
One continuous tiled wall fills the board gutters and lower HUD. Full padded side
art renders beneath the board's sorting layers; overlay UI stays above scenery.
The shallow platform follows the existing spawn/foot baseline. Its alignment uses
the stable 64px reference fit, so changing animation canvas sizes cannot move it.

The existing Game scene/bootstrap consumes this binding automatically. No scene,
prefab, build-list, character, gem, shared-control, cell, combat, board-rule or save
data changes are included. Forest/Court and the other zone selections retain their
existing paths. All imported source PNGs and their metadata remain unchanged.

The 360×640 isolated art preview was a fixed composition. Live battle fitting
preserves existing UI reservations, character sizes and touch targets. Scenery
uses the established responsive Point-filtered fitting, so physical pixel widths
may be uneven, especially in taller viewports. No review screenshot is used as a
runtime texture. Shared Guide/gear, supplies, ability, health and cell art remain
their previously approved family rather than baked mockup labels.

## Executed validation

- Unity 6000.3.19f1: `Tools/Validate-Unity.ps1` **passed**.
- Six focused tests **passed**. See `Evidence/UnityTests.xml` and
  `ValidationSummary.json` for names and exact source hashes.
- Production Game scene, three zones × three sizes: 1080×1920, 720×1280,
  1080×2400 with safe area `(0,80,1080,2200)` for the tall case.
- All nine before/after layout records are exactly equal: board transform and
  scale, Canvas scale, TopHUD/GameArea/BottomHUD, player/enemy sections, actor
  container, health container, floor anchor, wave label, slots and buttons.
- Real accepted swaps advance one move in every zone. Settings open/close, ambient
  pause and Reduced Motion, and all five playable ability-symbol fits pass.
- All 119 original PNGs/importers are unchanged, with unique verified GUID bindings,
  Point filtering, 64 PPU, uncompressed textures and no mipmaps.
- Layer assertions verify eight distinct board-rim pieces, no obsolete overlay,
  non-raycast scenery, one tiled wall and unmasked surrounding props below the board.
- Actual captures visually inspected for readable actors/UI, complete banners,
  continuous gutters, warm torch placement and corner masonry behind controls.

These are editor runtime/rendering checks, not physical-device display/touch
validation. Encounter choices, idle poses and weakness colors can differ between captures.
No claim is made that the live layout is pixel-identical to the isolated mockup.

## Evidence and reproduction

`Before/` was captured on main `cc2ff70b4048fb7eef10be779b0f5d5a20368b85` before
adding the new resource binding. `After/` is the current production-scene capture.
Each contains PNGs, measured layout JSON and validator output. The baseline
capture test is explicit and rejects an integrated Dungeon; do not overwrite it.

Run Unity with graphics enabled, `-batchmode -projectPath <checkout> -runTests
-testPlatform EditMode -testFilter
"DungeonApprovedBattleTests.ApprovedArtKeepsLiveBattleLayoutAndInput;DungeonApprovedBattleTests.BindingReusesEveryApprovedNativeModule;ThreeZonePresentationTests"
-testResults <xml> -logFile <log>`. Do not add `-quit` to the test run.
Then run `powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1`.
Use `ApprovedDungeonBattleImporter.Build` only to rebuild the original GUID binding.

The art authority remains the live canonical guide and golden register. Their
source hashes were checked, including the Rattlebones/Farmer/Pan production sheets.
This pass changes placement bindings only; it does not change golden membership,
characters or any approved native artwork.
