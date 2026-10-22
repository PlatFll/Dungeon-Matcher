> **Rejected:** the user requested keeping the original side stones and adding only matching bottom masonry. [Current correction](MatchedCorners/Review.html).

# Dungeon side masonry and portrait banner review

The user requested removing the skeleton banner inside the player portrait and generating side masonry inspired by the supplied mockup. The banner is removed from the current hybrid Dungeon only; the portrait recess, frame, character and live UI remain intact. Existing legacy fallback art is preserved.

## Masonry candidate

PixelLab Pro Flash produced one usable native stepped wall from the requested paired source (job `9d2ced30-1336-404f-aca8-2f0718130fe5`, source `457ee932-b079-5518-8311-d314c23c0d04`). Its current authorized Rattlebones image was passed as the rendering reference. The supplied mockup guided the inward-stepped stone silhouette visually; it was not transmitted. Only one source was generated: **6 generations**, balance 931.20 → 925.20.

LibreSprite maps 12 source colors to seven existing Dungeon stone colors, preserving the complete alpha silhouette. Four modules were exported and reopened exactly: left/right 48×86 bases, left/right 24×16 repeats. The opposite side uses a mirrored silhouette with inward-lit edges and shared top-lit planes. Repeat connection edges intentionally touch; the complete bases have two transparent pixels of clearance. All pixels remain native with binary alpha and Point/uncompressed imports. Raw source, request, lineage, editable ASE documents, reproducible cleanup script and measurements are retained.

The candidate is shown inside the actual Game scene by an editor-only review fixture. Existing side masonry is temporarily hidden there; modular repeats and stepped bases are drawn on the existing background canvas beneath the live board and HUD. No alternate gameplay/layout owner is added. The candidate is **not selected in production resources**; approve this appearance before replacing the currently approved masonry. All prior approved source assets remain unchanged.

## Verification

Two existing/reused live-scene capture tests pass: the banner-free current production scene and the new masonry review. Each captures 720×1280, 1080×1920, 1080×2400, a tall safe-area case, and 360×640, with Rattlebones and three live enemies. They check native image density, portrait masking, full base bounds, board/gem visibility, health/intent placement, Guide/Settings controls and an accepted swap. Board, section and health geometry match the pre-integration baseline in both views at every size. The artwork's pixel/alpha/source checks pass independently.

`Diagnostics/UnityTests.xml` and `Verification.json` hold the current evidence; the compiler receipt is recorded separately after final validation. These are graphics-enabled Unity checks, not physical-device testing. No new Android build was performed.

Open `Review.html` for current/candidate screenshots, native exports and raw/clean comparison. Dedicated branch `feature/dungeon-zone-hybrid-pixellab-rebuild`, draft PR #205. Stop for the user's masonry review; do not merge.

Final compiler validation passed with Unity 6000.3.19f1 (exit 0). All 5,298 protected files and eight pre-existing main-checkout edits remain byte-identical. The earlier 69-test regression result belongs to the preceding integration milestone; the two live-scene fixtures were rerun for this change.

The initial candidate bound assertion used an exclusive maximum edge. After correcting the inclusive check, the safe-area case exposed a 0.033-screen-pixel overhang. The final candidate uses floor-rounded dimensions and one native pixel of edge clearance; all original coverage passes. Both failed-run XML records are retained beside the final passing result.
