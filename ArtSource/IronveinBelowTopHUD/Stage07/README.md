# Stage 07 — complete Ironvein art-only assembly

The user approved Stage06 by requesting continuation. The approved components
were already combined incrementally, so Stage07 retains their exact artwork and
placements in a dedicated review scene. No new artwork or generations were needed.

`Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage07.unity` references the
existing 36 modules: 5 separator, 10 frame, 7 drill, 9 surround and 5 HUD assets.
All source assets and editable LibreSprite files remain in their original stage
folders. `QA/Assets.json` indexes exact hashes, dimensions, colors and source paths.

Actual Unity renders at 360x640, 720x1280 and 1080x1920 match approved Stage06
pixel-for-pixel. The top and grid also match the initial Stage01 foundation.
Frame connectors align to rail centerlines, the three paired drill stations have
equal spacing, opaque drill tips stay outside the grid, and native images use
uniform scale, binary alpha and Point/uncompressed/unmipped imports. HUD support
art renders behind the existing UI. All 36 native exports equal their Unity copies
and existing LibreSprite roundtrips. 891 protected files retain their hashes.
No serialized object record differs from Stage06; the new scene has its own GUID.
`Tools/Validate-Unity.ps1` passed; see `QA/Validation.json` and `QA/Assembly.json`.

The clearance check measures opaque pixels: drill canvases deliberately retain a
transparent tip column. The initial canvas-box assertion was corrected; the final
test still forbids every visible drill pixel from overlapping the grid. The full
grid image comparison provides an independent preservation check.

This is a static art-only Unity preview, excluded from builds. It does not establish
production integration, live gameplay, safe-area behavior or real-device results.
Existing shared grid/UI geometry and its fractional edge rounding remain intact;
new native modules preserve their integer pixel scale. Production Game, gameplay,
characters, gems, shared controls and all other zones remain unchanged.

Review `Review.html`, `Preview/Unity-720x1280.png`, `Preview/Comparison.png` and
`Preview/AssemblyDetails.png`. Stage07 spends zero PixelLab generations; cumulative
pack usage remains 74. No new jobs or image uploads. Stop for review; do not merge
or continue to Stage08 automatically.
