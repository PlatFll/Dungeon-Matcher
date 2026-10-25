# Forest unified lower stone corners
Review candidate, 2026-10-24. This focused revision supersedes the rejected Stage 2 split-stone composition. Stop for user review; no next stage, production integration or merge.

## Change
Each side now contains one main continuous stone/root formation. Both the old board-foot boulders and the prior lower rock/trunk layer are disabled in the new review scene. The existing woody connector remains to join the approved board-side roots naturally. Separate red/pale mushroom clusters and sparse moss accents sit on the formations.

All earlier art and scenes remain intact as source history. Only the two lower environmental corners change in the new art-only Unity scene. Top HUD/battle/portrait, board and frame, gems, shared dimensions, central HUD, potion/bomb controls and resonance text stay unchanged.

## Native assets
| Asset | Canvas (px) | Opaque colors |
|---|---|---|
| Stone_Left | 85 x 101 | 14 |
| Stone_Right | 86 x 105 | 15 |
| Mushrooms_Red | 24 x 24 | 10 |
| Mushrooms_Pale | 24 x 26 | 6 |
| Moss_Sprig | 22 x 22 | 5 |

Exact material edits were authored, saved and reopened in LibreSprite; no scaling or interpolation. Every PNG has binary alpha and at least two transparent pixels of clearance. The main silhouettes are each one connected component. Visual review is still the user's decision; connectivity alone does not establish artistic quality.

## Generation history and usage
53 PixelLab subscription generations; USD credit spending 0. Balance 750.20 to 697.20.
Two first tall studies were rejected as column-like, and the 64px accent studies were too large for the available decoration spaces. The second focused left/right sets use lower, broader stone masses. Three 24px Pixen accents were cleaned into the same material family. Every request, receipt, raw result and selection remains under Diagnostics, Raw and Scripts.

The main Pro generations used actual current Rattlebones for contour/shading/detail and supplied mockup corner crops for composition. Rattlebones's anatomy and palette were not copied. Small Pixen accents used text-directed material roles, followed by native palette cleanup and golden-reference visual review; that endpoint has no independent image-reference input.

## Executed verification
- Tools/Validate-Unity.ps1: passed; log in Diagnostics/Validation.json.
- ForestUnifiedCornerTests.UnifiedCornerReview: passed with actual Unity captures at 720x1360 and 1440x2720. Larger capture is exact nearest-neighbor doubling.
- Original capture matches the previous Stage 2 output byte-for-byte.
- Top 1000 screen rows, board cells, opaque board-frame art, all painted HUD pixels, button interiors and text remain pixel-identical. New scenery can be visible through the controls' transparent outside margins; those background pixels are not UI artwork.
- 290 prior image bindings/rectangles and 1799 existing source files preserved.
- Zero changed pixels outside the authorized old/new lower dressing masks.
- One main formation instance per side; both prior stone groups are disabled. No clipped asset canvas or floating main silhouette.
- Board 512x512 with 64x64 cells, board frame 580x580, native portrait 98x220 unchanged.

These are Unity art-review scene captures, not live combat or real-device testing. The review scene is excluded from build settings. No runtime gameplay or production scene files changed.

## Review/reproduce
Open Review.html. Scene: Assets/_Game/Scenes/ArtPreview_ForestSurround_UnifiedCorners.unity.
Native sources: Native/*.aseprite. Exported PNGs: Exports/.
Regenerate exact pixel plans with Scripts/PrepareNative.py, execute Scripts/RunNative.js in LibreSprite, reopen via ReopenNative.js, run the named Unity test, then Scripts/Verify.py.
