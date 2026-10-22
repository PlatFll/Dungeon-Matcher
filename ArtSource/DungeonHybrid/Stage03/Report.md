# Hybrid dungeon Stage 03 — UI hero pieces

Status: complete for user visual review. Stages 01 and 02 are approved; their artwork is unchanged. Stage 04 has not started. No merge or production integration.

## Delivered

- Three PixelLab sources, cleaned and split in LibreSprite into **35 native PNG modules and 35 editable ASE files**.
- Wave frame with small hanging skull; shared Guide/settings/consumable corner-and-rail family; resonance frame with paired violet ornaments.
- Approved portrait and board frames retained byte-for-byte, including centered connectors and scalable rails. The board study uses the actual approved `Stage16/Board_Cell64.png` and six unchanged 64×64 gems.
- Labels, gear, potion, bomb and crowned skull remain separate existing assets. No character, gate, floor, wall or gameplay changes.
- Two isolated Unity art-only scenes: top HUD over the approved gate study; separate board/lower-HUD study. The latter is a native-size piece review, **not a replacement production layout**.

## Verification

Two explicit Unity Play Mode capture tests pass via the Edit Mode runner. Four actual Game View screenshots cover 360×240 and 720×480 top studies, plus 576×688 and 1152×1376 board/HUD studies. The 2× captures match nearest-neighbor enlargement of the 1× captures exactly. They are not composited mockups.

Frame checks cover native corner sizes, tiled rails, matched rail/connector centerlines, odd/even sizes, minimum-size hide/recovery, complete ornaments, no clipped text, no input/gameplay owners, and no build-settings registration. Insets overfill behind interior masks. The retained portrait recess remains clipped as in Stage 02.

The first test attempts caught a test-runner domain-reload variable issue and an obsolete 35px art-study cell reference; both were corrected. Pixel comparison then found that a World Space Canvas's reference PPU reverted to 100 after reload, while the assets use 64 PPU. The new preview scenes now serialize CanvasScaler at 64 reference PPU and use the existing native text snapper. This fixes tiled-rail sampling without changing art or production presentation code. Earlier failure XML is preserved under Diagnostics.

`Tools/Validate-Unity.ps1` passed with Unity 6000.3.19f1. `Diagnostics/UnityValidation.json` records the tested source and log hashes. Independent native checks confirm 13 opaque colors across the new library, binary alpha, unchanged dimensions, exact ASE reopen/export pixels, all 580 protected file hashes unchanged, and all eight pre-existing main-checkout edits unchanged. Approved gate and floor regions in the actual top capture remain pixel-identical to Stage 02.

This is automated art-only editor/runtime preview validation. No production combat integration, Android build or real-device test was performed in Stage 03.

## Generation and lineage

17 generations used: wave 6, shared controls 5, resonance 6. Balance changed from 983.20 to 966.20. Reused the actual approved Rattlebones upload as the rendering reference; no new image uploads. Raw sources, exact requests, job IDs and hashes are under Sources. No regeneration of approved structural or gate art.

## Review / reproduce

Open `Review.html`. Exports are native PNGs; Native contains editable LibreSprite ASE sources. `Modules.json` gives dimensions, palette counts, source hashes and Unity paths. `AssetLedger.csv` records retain/new decisions.

With the installed Foundry Python, run `Scripts/build_assets.py` to execute the native LibreSprite cleanup/export (isolated Stage03 files only), then run the two explicit `DungeonHybridStage03Tests` in Unity. Run `Scripts/build_review.py` and `Scripts/verify.py` for the review and independent checks. Both Unity scenes have Editor menu entries under Dungeon Matcher / Art Preview.

Stop for visual approval before Stage 04. Keep draft PR #205 unmerged.
