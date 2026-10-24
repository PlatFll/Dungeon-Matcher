# Enchanted Forest — Stage 2 portrait review

Stage 1 was approved by the user. Stage 2 is a new candidate awaiting review.
**Stop before Stage 3. Do not merge.** Draft PR #208 remains stacked on PR #207.

## Result

Warm timber rails, a dark green recessed plank fill, four restrained ivy accents
and two small ivory flowers. Rattlebones, the gem, pennant and HP bar keep their
existing artwork and positions. The Stage 1 scenery and other UI stay unchanged.

One PixelLab Pro design/material source (128×256, 18 opaque colors, 10 generations)
used the supplied pack reference and verified Rattlebones image. Job:
`af8f94e9-87d1-4f31-ba21-8ec7434b1049`. Request, receipt and source hashes are recorded
in Diagnostics/. Rattlebones guided rendering discipline, not the timber palette.

The raw source had uneven rails and clipped edge foliage. It was not scaled into
the UI. Its timber palette and ivy motifs were reconstructed as exact modular
native pieces through LibreSprite pixel editing, with simplified connected clusters.
Final library: **14 modules, 14 opaque colors, binary alpha**. Every editable
document was reopened and exported again; decoded RGBA matches its intended pixel
plan and final PNG exactly. Raw pixels are not claimed to be preserved.

| Pieces | Count | Native size |
|---|---:|---|
| Independent corners | 4 | 15×15 |
| Top / bottom tiled rails | 2 | 32×9 |
| Left / right tiled rails | 2 | 9×32 |
| Centered timber join | 1 | 7×7 |
| Recess fill | 1 | 32×32 |
| Separate padded ivy accents | 4 | 43×27, 26×50, 25×44, 31×45 |

## Geometry and validation

The same ResponsiveModularFrameFitter assembles the pieces. No shared/runtime
implementation was modified. Portrait: **98×220** native units, **12-pixel inset**,
**32-pixel backing overfill on every edge** behind the existing clipping mask.
Rail thickness is 9 pixels; rails and joins share the same centerlines. The
separate ivy may extend outside the wood boundary, with complete padded silhouettes.
The shared board/player sizing corrections inherited from PR #207 remain intact.

- Tools/Validate-Unity.ps1 passed, Unity 6000.3.19f1.
- EnchantedForestStage02Tests.PortraitReview passed. Actual art-only Unity captures:
  720×480 and 1440×960. The latter equals exact nearest-neighbor 2× of the former.
- Native pieces and all eight corner ports / four repeated rail seams passed.
- Frame cases: 30×30, 74×180, 98×220, 129×257 and 163×287. Impossible 29×29 frame
  hides instead of squashing corners. Ivy anchors stay on whole pixels at odd sizes.
  Small-size cases test assembly geometry, not a usable character/UI composition.
- Mask probe changes only the exact interior rectangle; no background spill.
- All retained image references and screen rectangles match Stage 1. The exposed
  environment beyond the portrait and ivy is pixel-identical to the approved capture.
- All 24,166 pre-existing tracked files matched starting hashes before this status
  documentation update. Automatic whitespace-only importer changes were restored.
- New assets use Point, FullRect, 64 PPU, no mipmaps and no compression.

This is an isolated review scene with static readability samples, not live gameplay
or device testing. No production scene, lower HUD, gem, character, board, shared
layout or build settings changed. No later stage was started.

## Review and reproduction

Open Review.html for before/after Unity captures, a native frame view and all PNGs.
Scene: `Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage02.unity`.
Menu: **Dungeon Matcher / Art Preview / Enchanted Forest staged 02**.
It is excluded from player build settings.

Editable sources: Native/*.aseprite. Run Scripts/PrepareNative.py with the Foundry
Python runtime to prepare blank canvases and explicit native pixel plans, then
execute Scripts/AuthorNative.js in LibreSprite from the repository root. ReopenNative.js
exports saved documents for independent comparison. NativeValidation.json lists
dimensions, bounds, palettes and hashes. ReviewValidation.json records screenshot
and preservation evidence. Generation is not repeated by these scripts.

Artistic verdict: clearer timber construction and a quieter, distinct interior,
with sparse ivy supporting the portrait silhouette. Candidate ready for user review.
