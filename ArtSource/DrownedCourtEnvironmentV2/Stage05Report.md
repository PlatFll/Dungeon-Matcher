# Stage 05 — ancient teal stone board surround

Candidate complete; awaiting user review. Stage04 is accepted by the user's Continue. Stage06 has not started. Nothing merged or bound into production.

## Result

Four separate 25×25 stone corners, four 11-pixel-thick repeatable rails, a 9×9 centered metal joint and a 1×1 tiled dark overlap form the new board surround. Two worn rail variants, an 11×11 attachment socket and the cleaned 128×64 motif seed are included as optional/study pieces. **14 native assets; 10 distinct assets used in the scene.** The entire family uses 11 opaque colors with binary transparency.

The quiet teal stone has broad planes, top-left bevels and dull silver socket centers. All four corners are upright with consistent lighting. The socket modules leave useful attachment positions for the later gutter stage; no coral wallpaper or unrelated props were added.

The existing ResponsiveModularFrameFitter follows the original **545.099×545.099** opening at **(87.4505, 213.4505)** in the 720×1280 reference layout. It is unchanged. Repeatable rails fill the length, while corners remain uniformly scaled. A narrow two-reference-pixel dark overlap sits underneath the cells so fractional layout bounds cannot reveal hairline gaps. The whole cell layer renders above the frame and overlap. Cell dimensions, tile pitch and all 64 positions remain exact.

## Production cell evidence and limits

An explicit Unity play-mode snapshot test loaded the current Game scene with a disposable profile and captured all 64 existing Court cell sprite bindings, colors and screen rectangles. It passed **1/1**. These exact read-only bindings are used in this static preview; it does not contain a second board controller or new puzzle logic.

The currently bound Court cells are the existing ReferenceDungeon EmptyCell PNGs, **45/46×46 source pixels**, mapped to approximately 68.137 reference-pixel cells. This pre-existing fractional/nonuniform cell mapping is deliberately preserved by this art-only stage. The 64×64 gem assets are untouched. New frame artwork uses uniform **1×, 2× and 3×** source pixels at 360, 720 and 1080 widths. This is not a claim that the existing production board scaling has been fixed.

## Executed verification

- Tools/Validate-Unity.ps1 **passed**, Unity 6000.3.19f1; full log in QA/Stage05/UnityValidation.log.
- The candidate-only Unity builder produced **20 actual camera captures**, with cells before/after and transparent frame isolation at 360×640, 360×800, 720×1280, 720×1600, 1080×1920 and 1080×2400. Three independent frame-opening sizes passed without changing the captured board.
- **14 LibreSprite .ase sources** reopened/exported with exact RGBA equality. Unity copies are byte-identical; imported opaque pixels and alpha match. Point, FullRect, 64 PPU, no compression/mipmaps/NPOT scaling.
- **26 legal seam pairings** pass, including all corner exits and normal/worn rail neighbors. Corner inner quadrants are transparent; the native assembled opening is clear.
- **2,082,448 board pixels** compared across the six render sizes: unchanged before/after. **13,088 rendered edge samples**: zero gaps immediately outside the grid. **12,040 corner pixels** match exact native 1×/2×/3× source colors.
- Upper scene/portrait and lower HUD pixels match Stage04 at every render size. The before capture exactly reproduces the accepted Stage04 image.
- **435 accepted Stage01–04 files**, **7,839 protected production/settings files**, and five guide/reference files remain byte-identical. The new scene preserves all previous objects except hiding its 18 layout-guide lines and extending its canvas child list. Production scenes/build settings are unchanged.

No live combat validation, new safe-area behavior, flooding/AIR/pause/resume tests, Android build or physical-device test was performed. No new ambient behavior exists in this stage. Those remain later integration checks. The screenshots are actual Unity **art-only** renders with read-only character contrasts, not a production integration.

## PixelLab and native editing

One Pro Flash 128×64 stone motif source: **6 generations**, balance **590.20 → 584.20**, **58 cumulative**. Actual current Rattlebones PNG supplied as rendering reference; anatomy/palette not copied. Raw source, complete request, receipt, job ID, hashes and charged usage are in RawPixelLab/Stage05/Lineage.json. Native cleanup/module construction was performed in LibreSprite; no additional generation or resampling.

## Artistic review — subjective

**8/10 for the structural frame stage.** The restrained stone planes and centered silver joints support the existing blue depths and leave the cell opening uncluttered. The frame is deliberately calmer than the reference's surrounding reef growth; the richer side architecture belongs to Stage06. Approval remains the user's decision.

Review: Preview/Stage05/Review.html. Native manifest: Native/Stage05/Manifest.json. Verification: QA/Stage05/Verification.json. Scene: Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage05.unity.
