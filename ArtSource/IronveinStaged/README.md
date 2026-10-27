# Ironvein Excavation staged rebuild — final review candidate

All ten supplied stages completed on `feature/ironvein-pixellab-staged-rebuild`. This is an art-only Unity review, not production integration or user approval. No merge. The prior attempt and PR #213 remain intact.

## Review

Open `Review.html` for current production versus the revised Unity scene, short/tall captures, and every native asset at 1× and 3×. `Assets/_Game/Scenes/ArtPreview_IronveinStaged_Final.unity` is the editable modular assembly. It is excluded from build settings. These are actual Unity camera renders, not generated full-screen replacements.

The candidate uses 47 native PNG modules and 47 editable LibreSprite documents: cave/recess tiles and depth, stone tunnel, timber, lamps, rails/level floor, ore, portrait and board frame pieces, pillar/chain hardware, six mining props, and grounded lower corners. Corners/rails are driven by the existing responsive frame fitter; the portrait backing is overfilled inside its mask. Artwork is independent sprites/tiles, not a flattened game texture.

## Source and cleanup

22 focused text-only PixelLab requests used **193 subscription generations**. Balance: **459.2 → 266.2**; purchased credits remain $0. No image references were uploaded. The supplied concept was viewed only for composition. Current Rattlebones, Farmer and Pan reference hashes were verified and their actual native images inspected; no character pixels changed.

`Raw/`, `Sources.json` and `Receipts/GenerationJobs.json` preserve raw outputs, selected indices, prompts and receipts. Pebbly CaveWall and looping Tracks candidates were rejected. The simpler prior PixelLab track source was reused losslessly. Portrait sockets/rails and continuous board rails were compacted/reconstructed directly in LibreSprite to fit existing openings; generated board sockets retain native pixels. Material-specific color maps, localized light on existing cave clusters, connected floor cleanup and exact modular edge joins are recorded in `Scripts/Selections.json` and `Scripts/NativeCleanup.js`. This is internal selection, not a new art approval.

## Preservation and validation

The production Game scene, all gameplay owners, enemy definitions/animations, gems, shared HUD assets, other zones and original candidate files are unchanged. The review copies live Ironvein cells/gems/enemies and current Dungeon shared player/HUD presentation from disposable play sessions. Existing source artwork and gameplay geometry are retained. The older Ironvein theme's smaller HUD/player rendering was not used as the new shared-style authority.

- Unity preflight: Dungeon, Forest and Ironvein layout validation at 720×1280 and 720×1600; common board/portrait dimensions verified.
- Live-content capture: 64 cells, 64 gems and three distinct existing Ironvein enemies; disposable profile only.
- Native import checks: exact source pixels, Point, uncompressed, no mipmaps, 64 PPU, FullRect. Simple environment sprites retain their native dimensions.
- Responsive frame centering and fitting probes pass; portrait mask retained; all **87 shared HUD graphic rectangles** match their live capture.
- Six art-scene renders: 360×640, 720×1280, 1080×1920, 360×800, 720×1600, 1080×2400.
- Reopened LibreSprite documents export exactly to the PNGs (`Roundtrip/`; measured in `QA/NativeAssets.json`). Alpha is binary.
- Required repository Unity validator result and preservation result are in `QA/FinalValidation.json` and `QA/Preservation.json`.

New environment uses 1×/2×/3× integer source scale. Existing gem and shared UI mapping is preserved, including inherited fractional layout mapping; this candidate does not change global gameplay scaling. It is not an Android build, phone test, production integration, or visual approval. The native source-pixel import assertions and screenshot review are distinct evidence.

## Reproduce

Use the configured Foundry Python with Pillow to run `Scripts/PrepareCleanup.py`, then `Scripts/RunLibre.ps1`. In Unity run **Dungeon Matcher → Art → Ironvein Staged → Build Review**, or `Scripts/RunUnity.ps1`. Baselines/content can be regenerated through the explicit `IronveinStagedPreflight` and `IronveinStagedContentCapture` fixtures. The captured cropped shared icon is a lossless copy of its original texture region, never a regenerated icon.

Local full Unity logs remain under `QA/` and are excluded from Git; their compact results are retained. The complete starting-file hash list remains local. No old art or scenes were deleted. Review this candidate before any production integration or merge.
