# Stage 5 revision 02 — rich woodland side dressing

The user rejected the first Stage 5: branches were too thin, leaf coverage sparse
and stones too small relative to the supplied reference. That art remains in its
original folder as rejected history. This revision is a separate review candidate;
Stages 1–4 stay approved and unchanged. Stop here; no Stage 6 or merge.

## Revised result

Four native bough modules form dense, overlapping growth down both board margins
and the lower sides. Four distinct substantial mossy boulder modules anchor the board-foot
and bottom corners. Eight editable assets are placed sixteen times, with varied
curves, large connected foliage fans and a few small flowers. The sources retain
their native silhouettes; no procedural thin-stem extensions, stretching or
resampling are used. Overlap conceals module joining ends.

The side dressing renders behind the existing board and HUD, allowing generous
branch/rock silhouettes to extend naturally underneath them. UI remains on top;
the board, portrait and frame dimensions are unchanged. The old sparse roots and
standalone mushroom cutouts are not used in this revision scene.

## PixelLab and LibreSprite

The user's new image and focused side crops were actual PixelLab image inputs.
Rattlebones was supplied for outline/detail/shading discipline, not anatomy or
palette. Three requests cost 30 generations in this correction: a tall bough
study, sixteen boulder variants and nine broader bough variants. The tall study
was rejected as too fine/narrow and cut at its canvas ends. Selected sources are
Broad 01/04/06/08 and Rock 04/08/09/12. Earlier Stage 5 cost 20 generations separately.

LibreSprite applies explicit per-material native recolors, merges near-duplicate
deep contours and shades, saves eight editable .aseprite sources and exports PNGs.
Actual source masks are retained exactly after transparent trimming and a new
two-pixel margin. Wood/green ramps relate to the approved forest family; the large
stones keep cool broad planes. Raw studies, requests, hashes and the complete
user reference remain available in this folder.

## Verification and limits

- Tools/Validate-Unity.ps1 passed with Unity 6000.3.19f1.
- EnchantedForestStage05RevisionTests.SideDressingReview passed at 720×1360 and
  1440×2720. The larger render is an exact integer doubling.
- Hiding the new dressing restores approved Stage 4 exactly. Every opaque frame
  and ivy pixel, all board cells/gems, top battle art, HUD controls and HUD text
  remain pixel-identical. Surrounding side background is intentionally richer.
- All 280 prior image bindings/rectangles remain unchanged. Board: 512×512 with
  64×64 cells; outer frame target: 580×580; portrait: 98×220 native.
- Eight sources reopen from LibreSprite with identical RGBA, binary alpha and
  padded canvases. Full view bounds and integer scale/placement pass. No sprite
  is clipped by the screen. Board/HUD underlap is intentional and tested through
  actual render comparison, rather than forbidding all rectangular overlap.
- Existing approved art/presentation source hashes are unchanged. The review
  scene is excluded from builds; production scenes, gameplay and other zones
  are untouched. These are static Unity art-review captures, not combat/device
  testing or production integration.

Open Review.html to compare revised Unity, rejected Stage 5, approved Stage 4 and
the supplied reference. Native assets, source choices and verification JSON are
linked there. Scripts/PrepareNative.py and AuthorNative.js reproduce the exact
LibreSprite pixels without generation. The verifier's local source-hash audit
uses the ignored starting snapshot. Unity menu: Dungeon Matcher / Art Preview /
Enchanted Forest staged 05 rich revision.
