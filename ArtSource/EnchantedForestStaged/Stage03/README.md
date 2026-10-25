# Enchanted Forest Stage 3 — magical decorations

Review candidate, 2026-10-24. Stages 1 and 2 (including fuller foliage) are
approved. This completes Stage 3 only. Stop for review before Stage 4; no merge.

## Art

Five accent designs are assembled from nine separate native modules:
- Branch-mounted amber lantern: fixed mount, body and light, each on a padded
  48×64 canvas. The branch keeps its generated shape; duplicate near-black/brown
  shades were consolidated into the forest wood ramp. One leaf uses the approved
  olive colors. The source silhouette and warm glass are preserved.
- Carved rune plaque: dark timber plus a separate cyan glyph, 32×48. Wood
  highlights are restrained; source shape and rune are preserved.
- Friendly cyan wisp: unchanged approved Stage 1 core plus a hard-pixel two-tone
  emission skirt, 18×22. No blur or partial-alpha edge pixels.
- Tiny warm firefly: native 8×8 drawing using the lantern's warm light ramp.
- Hanging leaf strand: 38×47 padded native extraction from approved portrait
  foliage, retaining the complete lower leaves and removing the cropped remnant
  of the preceding leaf. The source portrait art is unchanged.

PixelLab produced the lantern and rune families in two requests costing 10
generations each (20 total). Selected candidates are Lantern_02 and Rune_15;
all raw candidates and request receipts are retained. Actual Rattlebones and
the supplied forest reference were passed through supported style-image inputs.
Guide, register, current golden and source hashes are in Diagnostics/Lineage.json.
Rattlebones governs rendering quality, not the prop palette or anatomy.

LibreSprite applied the explicit native pixel plans, saved nine editable
.aseprite files, exported nine PNGs and reopened every saved document. Exports
match reopened RGBA exactly, use binary alpha and 1–11 opaque colors per module,
and have transparent edge clearance. The two generated silhouettes are exact.
Only hidden RGB under transparent pixels was normalized during native export.

## Unity review and verification

Scene: Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage03.unity. It opens as
a separate art-only review scene, excluded from player build settings. The
decoration layer sits above scenery and below the portrait, characters and UI.
All additions stay above the enemy lane; original image placements are retained.

- Tools/Validate-Unity.ps1 passed with Unity 6000.3.19f1.
- EnchantedForestStage03Tests.DecorationReview passed in Unity, capturing
  720×480 and 1440×960. The latter is an exact nearest-neighbor doubling.
- Point sampling, no mipmaps, full native canvases, uniform integer scaling,
  whole-pixel placement, full viewport bounds and untruncated labels passed.
- Hiding only the new decoration layer recreates the approved Stage 2 screenshot
  pixel for pixel. All 84 original image bindings and rectangles match.
- Only the canvas label and top-layer child list differ among existing serialized
  scene blocks. 759 prior blocks are unchanged; 38 new blocks form the new layer.
- All 24,299 previous tracked files were byte-identical before this status update.

The 98×220 portrait, its mask/overfill, shared frame dimensions, approved
background and production assets are unchanged. The board and lower HUD are
outside this top-area review and remain untouched. Rattlebones and the three
enemies are static review actors; no live combat, Android or device test is claimed.
Lights are separated for later animation; this stage adds no animation.

## Review and reproduction

Open Review.html for the Unity before/after, native pieces, raw selections and
enlarged views. Previews/Unity-Stage03-720x480.png is the actual Unity capture.
Native/*.aseprite are the editable sources. Scripts/PrepareNative.py builds
canvases and explicit pixel plans without network access; run AuthorNative.js
through LibreSprite, then ReopenNative.js and VerifyNative.py. The preparation
script never calls PixelLab. The editor menu Dungeon Matcher / Art Preview /
Enchanted Forest staged 03 rebuilds the separate review scene.
