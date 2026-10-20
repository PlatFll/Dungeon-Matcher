# Stage 12 — DONE; stopped for user review

Eleven new native components produce the Wave plaque, Guide frame and square Settings frame/gear in the isolated Unity preview. The current portrait correction and all earlier art remain unchanged. No production integration, phone build, Stage13 or merge.

## Art and construction

One new 64×64 PixelLab source supplies a blank riveted metal plaque. LibreSprite retains its rivet, rail and violet-socket vocabulary while regularizing joins and simplifying repeated shade rows. Native row/column selection is followed by direct pixel cleanup; no resampling. The raw source remains immutable. The gear is separately authored in LibreSprite using the same material ramp.

The kit contains four 9×9 riveted corners, 16×5 top/bottom rails, 5×16 side rails, a 7×7 violet socket, a 16×16 dark inset tile and a 15×15 gear. Individual assets use 1–5 opaque colors. Rails repeat without stretched corners, with centered sockets on the wider Wave plaque. The dark inset overfills behind an interior mask. Small square rivets distinguish this family from the portrait's diamond corners.

The native preview derives its rectangles from the existing live HUD's 470-wide reference coordinates: Wave (169,21,135,47), Guide (330,30,79,39), Settings (414,30,45,40). At the established 360-wide native preview, Wave is 103×36 at (130,16), Guide 60×30 at (253,23), and Settings an intact 31×31 square at (319,23), centered inside its unchanged 35×31 equivalent control region. Existing production hit rectangles and control scripts are not edited.

`WAVE 1` and `GUIDE` are live TextMeshPro text using the existing shared Thaleah bitmap font and PixelTextFitter. The preview-only DungeonPreviewPixelText aligns glyph geometry to the native grid after font layout. It changes neither the shared typography owner nor gameplay controls. A separate icon Image displays the gear. The preview contains no Buttons, EventSystem, input owners, enemies or gems.

## Source authority and actual spend

Current guide v1.17.2 and golden register hashes were verified, and current Rattlebones/Farmer/Pan plus applicable UI were inspected. Actual unscaled 64×64 Rattlebones PNG bytes were sent through `style_image.base64` for outline/detail/shading. Palette copying was disabled. The supported create endpoint takes one native style image; larger supporting/UI images were compared locally and were not represented as transmitted inputs.

Job: 7e594107-a72b-453d-9834-0a48b2dd0e2c. Image: 2045e378-119e-5ef2-94f5-530bc76ad52a. Seed: 912120. Exactly one paid job, charged five approved generations. Balance 999.20 → 994.20; cumulative staged spend 60. No paid retry. Exact request, receipt and raw source hashes are preserved.

## Executed verification

- All 11 native ASE cels match PNG exports and Unity copies exactly; only binary alpha. Exact dimensions, bounds and palettes are in NativeChecks.json.
- Unity plaque test: 1 passed, 0 failed. Actual 360×640, 720×1280, 1080×1920 and 1080×2400 safe-area captures. Complete frame geometry, native icon placement, live font identity, readable unclipped glyphs and absence of gameplay/input owners checked.
- `WAVE 999` updates the live label and fits without clipping. Hiding a visual frame changes neither its parent rectangle nor its label.
- Every pixel of the 2× and 3× capture equals nearest-neighbor enlargement of native 1×. Tall-screen content matches the 3× capture exactly.
- All 314 protected previous-art files retain their hashes. All 223,931 screen pixels outside the three new plaque rectangles are identical to the previous staged assembly. Existing portrait, board and environment pixels are preserved.
- Scene review: 162 new serialized objects for the three plaques; no prior objects removed. Existing object identities/order retained. Only the preview's module/manifest bindings and top-UI child list change among prior objects.
- Tools/Validate-Unity.ps1 succeeded with Unity 6000.3.19f1; exact final source/log hashes are in ValidationReceipt.json. Production Game scene/build settings and main-checkout user files preserved.

The first native visual pass exposed a thin gear body and fractional text offsets. The gear hole was reduced from 5×5 to 3×3, and preview glyph positions were snapped to native pixels. Initial screenshots, passed geometry test and failed exact-scaling finding remain under Diagnostics/Stage12/FirstPass; they are superseded by the final exact-pixel checks.

## Artistic verdict and limits

The restrained cool metal reads clearly against the violet wall, with small violet fittings and a quiet recessed face. Wave remains the strongest top label; Guide and the gear stay readable at native size. All source silhouettes and joins fit their canvases. This is a review candidate, not user approval or proof of production phone integration. Stage00's production scaling caveat remains deferred to integration.

Review: ../Review.html. STOP. Next stage only when requested: PROMPTS/13_GENERATE_BOTTOM_HUD_RESONANCE.txt.
