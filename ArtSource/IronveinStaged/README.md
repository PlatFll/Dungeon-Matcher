# Ironvein Excavation — selected scenery review

All ten supplied stages completed on `feature/ironvein-pixellab-staged-rebuild`. This is an art-only Unity review; the selected assets below are user-approved to keep. Production integration remains separate. No merge. The prior attempt and PR #213 remain intact.

## Background restoration — 2026-10-27

The user's latest screenshot exactly matches `IronveinEnvironmentV2/Preview/Unity-360x640.png`. Reused that library's **Platform**, **CaveWall** and **CaveRecess** directly through their existing Unity assets, preserving their PNG and LibreSprite sources byte-for-byte. These are distinct from the rejected staged replacements with the same names.

The stone platform retains its original 22-pixel height at the existing combat floor. CaveWall fills the surrounding background at native scale, including tall screens; CaveRecess fills the portrait's existing overfilled mask and the platform underside as in the older assembly. All 33 selected newer modules, actors, board cells/gems, frame dimensions and shared UI remain unchanged. No other earlier scene decorations return.

`Review.html` now compares the selected-only assembly with this restoration. Before-images: `Preview/BackdropRestore01/Before/`; exact sources and preservation: `QA/BackdropRestore01.json`. The updated scene remains art-only and excluded from production builds. Zero new generation usage; no merge.

## Selection revision — 2026-10-27

The user selected the cave depth, wheel, five modular support pieces, timber, lower-left/right rock corners, cart, crate, both lanterns, chain, and existing portrait/board frame pieces. The review scene now uses only those **33 native modules**. The other **14 source assets** remain preserved but have no sprite references in the saved scene.

Unselected wall/recess textures, separate mine arch, rails, platform, ore decorations, barrel, beam/straps, lower wall/ledges, tools and rope are removed from the assembly. Plain dark backing fills cleared areas; no replacement scenery was designed. The crate sits on the retained lower-left rock instead of its removed ledge. Selected art is byte-identical, including the player/board frame pieces. Gameplay actors, gems and shared controls retain their previous placement.

`Preview/SelectionRevision01/Before/` preserves all six earlier Unity captures. `Review.html` compares these directly to the revised captures and displays only the kept asset set. `QA/SelectionRevision01.json` records exact attachment matches, the selection and preservation checks. This revision spent **zero generations**. The complete staged source library described below remains recoverable; this partial selection does not approve the other assets or production integration.

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
