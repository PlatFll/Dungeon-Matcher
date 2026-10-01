# Finalized visual targets v2 — production candidate

This pass follows `Dungeon_Matcher_Finalized_Visual_Targets_v2_PixelLab.zip` and its `03_ASTRA_BRIEF/ASTRA_IMPLEMENTATION_PROMPT.txt`. The supplied mockups are visual references, not source atlases. PixelLab produces the new native candidates; LibreSprite supplies exact cleanup and editable sources. Final visual approval remains with the user after actual Unity screenshot review.

## Scope and ownership

- Base: main `651fe50`; branch `codex/finalized-visual-targets` in an isolated checkout. Existing local background edits and the separate Gideon branch are preserved.
- Environment: native 512×384, 8×6 cells at 64×64. Wall rows 0–3, floor/ledge row 4, repeatable foundation row 5. Preserve the central horned gate, pillars, banners, chains, locks, torches and side furniture.
- Duplicate the valid environment prefab and preserve the existing grid, 64 PPU, Tilemap layer roles, sorting, projection, mask and floor anchor.
- UI: stepped purple frames, amethyst jewels, text-free control surfaces, left-side universal heart/player and four enemy-rank badges. HP stays red; energy stays teal. Preserve character sprites, animation, gems, VFX and locked frame colors.
- Extend the existing health-bar and UI presentation owners. Combat, board rules, actor placement and enemy selection are outside this art pass.
- All source PNGs use integer native pixels, Point filtering, FullRect, no mipmaps/compression and binary alpha. Fill width is cropped or masked, never rescaled.

## Production gates

1. Review the first PixelLab environment candidate in the actual Game scene with unchanged characters/UI at 1080×1920 and 1080×2400.
2. Generate coherent modular wall/floor/foundation, architecture and prop batches. Review seams, quiet areas, palette, native cast scale and Unity composition after each batch.
3. Establish one UI base with `create_ui_asset`, then derive matching native components and interaction states. Review real screens before expanding the family.
4. Finish editable LibreSprite sources, imports, prefab/palette, rank mapping and screen presentation. Record dimensions, pivots, slice borders, hashes and PixelLab lineage in `AssetManifest.json`.
5. Validate assets, exact composed/baked reconstruction, required Unity compile, focused presentation tests and all changed screens. Open one focused PR; do not merge.

## Current checkpoint

127 native PNGs and matching editable LibreSprite sources are complete. PixelLab balance moved from 823 to 663 generations (160 used; $0 credits). All art remains a review candidate pending user approval of the Unity screenshots.

`AssetManifest.json` indexes every export, palette, hash, native size, pivot, PPU, border and generation lineage. `Validation/NativeInspection.html` displays the original PNGs at 1× and nearest-neighbor enlargement on light/dark backgrounds. The native review sheets place the environment beside the current 64×64 cast.

## Rebuild and review

Set `LIBRESPRITE_EXE` to a LibreSprite build with JavaScript scripting, or pass `-LibreSprite` to `Tools/Finish-FinalizedArt.ps1`. The wrapper resolves `__ART_ROOT__` for this checkout and normalizes ASE layer flags after each script. This metadata repair preserves RGBA cels and prevents transparent pixels from flattening on PNG export.

Run native scripts through the wrapper in this order: `FinishTiles.js`, `ComposeEnvironment.js`, `ExportEnvironment.js`, `FinishUI.js`, `ExportUI.js`. `FinishButtonBase.js` records the original palette-finishing step; the already-finished base is retained for repeatable family builds. Alphabetic tile candidate copies avoid LibreSprite's numbered-frame import prompt.

Run `python ArtSource/FinalizedVisuals/ValidateAssets.py` to verify source/PNG pixel equality, required transparency, state silhouettes, exact 48-cell reconstruction and all 61 intended tile-neighbor pairs. `BuildReviewSheets.py` creates inspection sheets only.

Run `Tools/Import-FinalizedVisuals.ps1` for imports, category style assets, the focused environment variant and `FinalizedDungeonPalette`. Run `Tools/Review-FinalizedVisuals.ps1` for actual production-scene captures and presentation checks at both portrait sizes. It uses disposable account/character-selection state and writes results under `.utmp/FinalizedVisualReview`. Run `Tools/Validate-Unity.ps1` after code/serialization changes.

The committed evidence is under `Validation/Screens/`. Full validation details and reference comparisons are in `Docs/Validation/FINALIZED_VISUAL_TARGETS_V2.md`. Earlier candidate images remain provenance, not alternate production assignments.
