# Stage 2 foliage correction

The user rejected the sparse foliage and selected the fuller original PixelLab
leaf treatment. This revision reuses that exact saved generation. **No new
generation or credits. No merge. Stop for visual review.**

Four padded native foliage modules now retain the source's broader leaves,
winding side growth and four small flowers. The material planes and leaf colors
come directly from the source. LibreSprite exports the explicit extraction and
localized contour repairs; it does not scale or regenerate the drawing. A sparse
right-side stem band is omitted to fit the unchanged frame, and a few clipped
outer-edge pixels become dark closing contours. Recess and timber are untouched.
The supplied clipboard image has identical alpha and visible pixels to the saved
PixelLab PNG; only hidden RGB beneath transparent pixels differs.

The library has 13 opaque foliage colors and binary alpha.
Foliage module coverage increased from 1381 to
6761 opaque pixels, without enlarging the portrait.
Exact per-module dimensions and source rectangles are in Scripts/Placements.json.

## Verified

- Unity compilation and EnchantedForestStage02FoliageTests.PortraitReview passed.
- Actual Unity review captures at 720×480 and 1440×960; exact 2× pixel doubling.
- All four LibreSprite documents reopen with identical RGBA exports.
- All four sprite canvases fit the review viewport, and visible bounds have padding.
- All nonfoliage image references and positions match the previous review. The
  98×220 frame, 12px inset, backing overfill, shared layout, HP and gem are unchanged.
- Rattlebones's visible source-mask pixels are unchanged in the rendered preview.
- The existing portrait mask contains its backing; no spill. Resize/whole-pixel
  anchor tests pass. Extreme small sizes test frame geometry, not a usable UI.
- The exposed approved forest region remains pixel-identical. All 24256
  previous tracked files matched their starting hashes before the status/doc update.

The correction uses a new art-only scene:
Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage02_Foliage.unity.
It is excluded from build settings. Production gameplay, other zones, board and
lower HUD are untouched. No actual combat, Android or device testing is claimed.
Original Stage 2 art and captures remain preserved for comparison.

Open Review.html for the updated Unity view, previous sparse view and source.
Native/*.aseprite are the editable sources. Scripts/PrepareNative.py prepares
native plans; AuthorNative.js applies them in LibreSprite; ReopenNative.js checks
saved exports. No generation call occurs during reproduction.
