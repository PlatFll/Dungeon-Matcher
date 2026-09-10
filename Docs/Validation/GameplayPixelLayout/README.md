# Gameplay pixel layout validation

## Diagnosis and ownership

The previous presentation mixed independently computed screen partitions, responsive character scaling, frame corner shrinking, and a non-uniformly scaled energy bar. Safe-area, HUD and board controllers could write overlapping geometry. World-board placement also needed the final PixelPerfectCamera projection before snapping.

GameplayPixelLayoutController now owns the safe gameplay viewport and TopHUD, GameArea and BottomHUD rectangles. GameplayPixelGrid supplies shared UI texel fitting and physical-origin snapping. BoardLayoutController fits and snaps the world board after PixelPerfectCamera updates its projection. Existing presentation controllers arrange content inside assigned regions; runtime bootstrap installs them again on scene entry. The redundant PlayerAreaFrameSpacingController and duplicate startup hooks were removed.

Changed files comprise the new layout/grid/validator and editor runners; existing board layout, top/bottom/player frame, character, health, affinity, ability and background presentation scripts; Game.unity; TopBattlePresentationProfile and its resource; and ARCHITECTURE.md. Board resolution, combat, shield rules, ability acceptance/spending and encounter progression implementations are unchanged. The pre-existing local ProjectSettings.asset edit is excluded.

## Exact geometry policy

- Canvas uses ConstantPixelSize, reference PPU 64, and integer scale. Start at floor(safe width / 456), clamped to at least one, then reduce until native content and vertical requirements fit. The 448 logical pixel minimum viewport accommodates the player and three enemy slots.
- Safe area is clipped to the display, rounded inward onto the logical grid, and inset by 4 logical pixels on each edge. Viewport dimensions are even logical integers.
- BottomHUD is 176 logical pixels high; section gaps are 6 logical pixels. Shared frame corners remain native 80 pixels and borders 16 pixels. The ability box is 176 by 64 and energy art is 64 by 64 with unit transform scale.
- TopHUD is at least 220 and normally capped at the preferred 290 logical pixels (absolute configured bound 320). Surplus screen height becomes background around the centered stack.
- Board scale is the largest fitting integer physical pixels per source texel, with its physical origin snapped to an integer pixel after camera projection. The current board needs 544 physical pixels at 1x. Only safe widths below 552 pixels permit the user-approved fractional downscale; source texels cannot all be preserved in that exception.
- A 1080-pixel display cannot fit the board at 2x (1088 pixels before insets), so the board stays 544 pixels wide. A 1440-pixel display fits 2x. This deliberate unused space preserves the approved integer policy. Mirrored frame pieces retain unit-magnitude scale signs. Transient VFX are outside the static pixel-art assertions.

## Validation performed

Unity 6000.3.19f1 batch compilation was run with Tools/Validate-Unity.ps1. Native rendered GameView scene validation passed all 32 combinations of these display sizes: 540x960, 720x1280, 1080x1920, 1080x2160, 1080x2340, 1080x2400, 1080x2460 and 1440x3200.

Each size used safe insets (left, right, top, bottom) of (0,0,0,0), (0,0,73,0), (0,0,97,41) and (3,5,99,43). Assertions cover containment, gaps, scale, competing layout components, source-texel ratios, physical origins, frame corners/edges/thickness, missing sprites and UI vertices. Deliberate corruptions were detected and restored successfully. Three enemies, actual Retry and MainMenu-to-Game reload also passed. See layout-cases.txt.

Existing Royal milestone, Siege Sergeant, encounter history, primary GameplayEdgeCaseValidation and supplementary gameplay suites passed. These exercise real swaps/cascades, specials, shields, energy ownership, encounter sampling and Bardley activation. NUnit EditMode results: 6 passed, 0 failed (editmode-results.xml).

The art audit covered 31 runtime textures: Point filtering, no mipmaps, PPU 64, effective uncompressed imports and no Android override. Source dimensions do not exceed the import limit. Shared frame sprites use FullRect; UI-only Tight imports still use Image rectangle geometry. See art-imports.txt and source-sizes.txt.

## Visual evidence and limits

The four native screenshots show representative resolutions and safe areas. bottom-frame-4x.png, board-corner-4x.png and energy-bar-4x.png are exact nearest-neighbor enlargements from 1080x2400-safe2.png, inspected for uniform pixels and joins.

Validation used rendered desktop Unity GameViews with simulated phone resolutions and safe areas. No physical Android device validation was performed. Screens below the minimum supported content dimensions report a layout error rather than silently introducing additional fractional policies.
