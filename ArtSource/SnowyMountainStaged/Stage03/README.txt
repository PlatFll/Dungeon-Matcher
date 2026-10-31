Snowy Mountain / Frostwake Peaks — Stage 3 portrait frame review
===============================================================

Stage 2 backdrop was accepted by the user's Continue instruction. This stage
changes only the portrait decoration in a disposable actual-Game Unity preview.
STOP here for review; Stage 4 has not begun. No merge or production art binding.

Artwork: cold timber rails, square centered steel corner/clamp plates, dark-blue
staggered masonry, broad snow caps, short icicles and small foot deposits.
15 separate native modules, 12 meaningful opaque colors across the kit, binary
alpha. Structural rails/backing are fully opaque. Raw source retained unchanged.
PixelLab Pro text-only source (128x256): one call, 10 generations; balance 4,940.
No mockup or golden image was uploaded. Prompt and receipt: Generation.json.

Native cleanup was executed in LibreSprite, not a generative replacement:
- Native crops separated into corners, beams, clamp, repeatable masonry and snow.
- Repaired transparent gaps in generated timber; simplified near-duplicate shades.
- Centered the square steel plates/bolts and unified beam edge separation.
- Rebuilt the masonry repeat boundaries into two quiet staggered brick courses.
- Removed two detached icicle fragments; retained connected snow silhouettes.
- Saved and reopened all 15 ASE files; exact export/roundtrip pixel equality.

Unity review:
Existing modular frame and RectMask2D architecture, existing player bounds.
Masonry overfills the mask and stays behind live player/health/gem controls.
Corners use 1x/2x/3x integer physical pixels. Rails tile at matching density.
No changes to shared board dimensions, portrait bounds, characters, gems, enemy
floor line, health bars, controls, approved backdrop or other artwork.

Two shared native-frame corrections were needed by this preview:
1. The validator now recognizes the fitter's explicit native-pixel mode, instead
   of treating every new Snow frame as the legacy fixed-thickness skin. It also
   validates tiled texel density; negative probes reject distorted source pixels.
2. Native rail joins/connectors snap to whole physical pixels at odd scales.
   Hidden joins otherwise landed on half pixels. Artwork and frame size stay fixed.

Evidence:
Tools/Validate-Unity.ps1 passed on Unity 6000.3.19f1.
Portrait review test passed at 720x1280, 1080x1920, 1080x2400, inset-safe 1080x2400
and 360x640, including exact before/after shared geometry and mask coverage.
Cross-zone native frame regression passed for all five zones at 360x640,
720x1280 and 1080x1920 (15 cases). See QA/UnityChecks.json and test XML.
These are actual Unity Editor Game-scene captures; not Android or device tests.
QA/Preservation.json checks all 8,835 protected production files, Stage 2 files,
and six preexisting local edits. Of the production files, 8,833 are unchanged;
only the two reviewed frame/validator code files differ. All artwork is preserved.
QA/ScreenDifferences.json records zero changed pixels outside the portrait and
its snow cap in all five paired Unity captures.

Known retained baseline: old Frostwake outer enclosure, board and bottom HUD are
still present until their stages. At 360x640, the preexisting Pixel Perfect Camera
warning/crowded labels remain. No claim that this portrait stage fixes those.

Review.html includes raw source, native modules, assembled portrait and all five
before/after captures. Reproduce preview with:
Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainStage03PortraitReview -Graphics
Branch: codex/snowy-mountain-pixellab. Draft PR #218. Do not merge.
