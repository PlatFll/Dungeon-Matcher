# Enchanted Forest Stage 4 — board frame

Stage 4 review candidate, 2026-10-24. The user approved Stages 1–3.
This stage stops for review before Stage 5. No merge or production binding.

## Result

Sturdy mitred timber, hanging corner ivy and connected side growth surround the
unchanged board. Thirteen native pieces: four 23×23 corners, four repeatable
32×11 / 11×32 rails, four padded PixelLab-derived corner growth overlays and one
44×202 approved ivy swag used along both sides. Artwork stays out of every cell.

One PixelLab Pro 128×128 request returned four concepts for 10 generations total.
Candidate 02 supplies the hanging-ivy and winding-vine direction. Actual
Rattlebones and the supplied forest mockup were the supported style/reference
inputs. Source PNGs, prompt, receipt, hashes and material remapping are preserved.
Rattlebones guides rendering discipline, not the wood/leaf palette or anatomy.

LibreSprite applies explicit native pixel plans and saves editable .aseprite
documents plus PNGs. The timber uses native broad shading and restrained grain
at the existing frame ports. Generated corner growth has unified material ramps,
removed inward tendrils and disconnected extraction specks, and padded canvases.
The approved side ivy is copied with identical RGBA; original assets are untouched.
An optional center accent was omitted because the fixed floor-to-board gap could
not contain it without covering approved pixels. No resampling or art stretching.

## Fixed geometry and verification

- Board remains 512×512 at its existing review position (104, 528 from top-left),
  containing the same 64×64 cells and native gem artwork.
- Native frame target remains 290×290, rendered 580×580 at 2×. Corners are 23px,
  rails 11px and inset 17px, exactly the existing snug-frame geometry.
- Existing ResponsiveModularFrameFitter.FollowOpening owns the layout. Corners
  keep their native size and rails tile. No runtime/shared-zone code is changed.
- Tools/Validate-Unity.ps1 passed with Unity 6000.3.19f1.
- EnchantedForestStage04Tests.BoardFrameReview passed at 720×1360 and 1440×2720.
  The larger capture is an exact nearest-neighbor doubling. All four rail-to-tile
  gaps are zero; full-canvas bounds, integer placement and untruncated text pass.
- Five frame-geometry resize probes passed, including non-square and odd-size
  openings. These probe the fitter, not usable gameplay at miniature sizes.
- The approved Stage 3 top, all tiles/gems and the lower HUD remain pixel-identical
  in actual Unity captures. Portrait dimensions remain 98×220 with its mask intact.
- All thirteen sources reopen in LibreSprite with identical RGBA; alpha is binary.
  Foliage has transparent padding. Rail endpoints and corner ports intentionally
  reach their canvas edges for seamless joins. No foliage pixel covers the board.

The separate scene Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage04.unity is
excluded from player builds. Static Rattlebones and three enemies provide context;
this is an art-only scene, not live combat, Android or real-device testing.
Production art, other zones, gameplay, board sizing and lower HUD remain unchanged.

## Files and reproduction

Open Review.html for Unity before/after, full context, native pieces and the raw
PixelLab selection. Native/*.aseprite are editable sources. Scripts/PrepareNative.py
prepares blank canvases/pixel plans locally, AuthorNative.js applies them through
LibreSprite, and ReopenNative.js plus VerifyNative.py check the saved data.
No reproduction script generates or spends credits. The editor menu Dungeon
Matcher / Art Preview / Enchanted Forest staged 04 rebuilds this review scene.
