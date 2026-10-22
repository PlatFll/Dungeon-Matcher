# Full-screen Dungeon brick coverage and flush masonry

The background used the same safe-area clip as the gameplay viewport. That excluded the top cutout region, bottom inset and outer margins. The prior corner preview also positioned the padded PNG canvas rather than its visible bottom row. Android `androidRenderOutsideSafeArea` was already enabled; no player/platform setting needed changing.

The hybrid background retains its existing world canvas, native pixel scale, tile phase and live actor-floor registration. Its broad wall now overfills the physical screen; only the safe-area background clip is removed. Portrait and board-local masks remain. The original 360px assembly contains a 24px half tile at x336: the matching right half now completes that cell when it repeats into the margins. Original side courses continue to the physical bottom. No scenery, frame, character or gem pixels were edited.

The unchanged matching corner candidates now anchor their opaque bottom at screen y=0 and their outside edges at the physical screen sides. Their two transparent padding rows extend beyond the display; no visible artwork is clipped. They remain editor-only review additions until the user approves their appearance. Production full-screen wall correction is on the feature branch; no merge.

## Verification

Three Unity tests passed: current Dungeon production capture, matching-corner capture, and the existing Dungeon/Forest/Court presentation regression. Both Dungeon views cover six cases: 720x1280, 1080x1920, 1080x2400, 360x640, a tall top/bottom safe area, and a safe area with left/right insets. Each keeps three live enemies, Rattlebones, gems, health/intents and controls; Guide/Settings and a normal accepted move are exercised.

The coverage fixture uses a distinctive camera-clear color and rejects any uncovered pixels in the captures. Independent image QA verifies all four complete screen edges match the approved wall/stone/floor palette in all 12 screenshots, allowing one byte of measured GPU color-channel rounding. The opaque corner bottom rows are checked exactly. The five existing layout profiles match the pre-integration geometry exactly; the added side-inset profile matches between current and candidate views. Corner positions, source hashes, masks and native scale are checked. The first failed run exposed the half-tile gap and is retained beside the final passing XML.

This is Unity automation and visual screenshot review, not a real-device test. No new Android build or image generation was performed. Compiler and preservation receipts are recorded separately after final validation. Keep draft PR #205 unmerged for user review.

The second failed run was a capture-fixture race: simulated insets for a larger screen were applied before GameView finished resizing. The fixture now waits for the requested actual resolution before applying those insets; no runtime error is ignored. Both initial failures remain recorded.

Final `Tools/Validate-Unity.ps1` passed with Unity 6000.3.19f1 (exit 0). All 5,298 protected files and eight pre-existing primary-checkout edits remain unchanged. Earlier regression counts are historical; three tests were rerun for this correction.
