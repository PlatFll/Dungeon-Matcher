# Lower forest fireflies — review only

Adds eight yellow fireflies around the board-side roots and lower corner formations. All reuse the approved 8×8 `EnchantedForestStaged/Stage03/Firefly_Mote.png` with no pixel edits, recoloring or generation. Independent fade periods span 2.6–5.6 seconds, with one- to two-native-pixel drift. The original 19 motion tracks are unchanged.

New copied scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_Fireflies.unity`.
Previous scene: `ArtPreview_ForestSurround_Waterfall.unity`, preserved.
Builder/test: `Assets/_Game/Scripts/Editor/ForestFireflyReviewTests.cs`.

## Verified

- `Tools/Validate-Unity.ps1`: passed with Unity 6000.3.19f1.
- `ForestFireflyReviewTests.FireflyReview`: passed; 96 matching-time captures of each version over eight seconds. Every new light moves, becomes invisible and returns to its intended brightness.
- Pixel comparison: no changes outside the eight bounded firefly regions. The entire top HUD and previous motion, board/gems/frame, buttons, center panel and resonance text match at every sampled time.
- Uniform 2× native scale, integer positioning, Point filtering, no mipmaps, viewport clearance, UI clearance, no raycast interception and Reduced Motion checks pass.
- 1440×2720 is exact nearest-neighbor doubling of the 720×1360 capture.
- All 7,102 tracked baseline files remain hash-identical. Scene audit preserves every old object and track; only the root child list and motion track list grow.
- The first test run caught exact floating-point Vector2 comparison noise at 16×16. The final test checks dimensions with the established 0.02 screen-pixel tolerance and separately verifies exact rendered pixel doubling. Initial failure evidence is retained as Attempt01.
- The first pixel comparison caught one firefly occluded by the board-frame foliage. The final layer draws above the decorative frame and below the shared HUD; the matched-pixel check confirms all eight appear and disappear in the rendered scene.

`Review.html` contains actual Unity recordings. Raw paired frames are retained locally under `.utmp/forest-fireflies/`. Existing artwork, prior sources and scenes are preserved. No production gameplay or physical-device test is claimed. No PixelLab calls or credits used.

Draft PR #209 remains unmerged. Stop for visual review.
