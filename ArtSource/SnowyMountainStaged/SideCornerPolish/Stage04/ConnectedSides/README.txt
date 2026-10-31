SNOWY MOUNTAIN — CONNECTED CLIFF SIDES
2026-10-31. Focused review correction. No production registration or merge.

The previous placement-only correction did not fix the source joins: overlapping 60px modules at 40px intervals repeated snow ledges and exposed gaps. Those renderers are removed in this review. Each side is now ONE continuous native cliff/timber/lower-corner sprite. Lanterns and banners remain separate, unchanged decorations in their previous positions.

LibreSprite builds the pieces from copies of the approved native cliff planes, joins matching material boundaries without alpha overlaps, extends the same rock surfaces through the screen edge, and joins the original corner into the continuous body. No full-scene repaint, mirrored lighting, resampling, new palette or additional backing layer. Every visible original lower-corner pixel retains its native position and color. The broad lower formation is preserved; the redundant apron renderer is gone. All original PNG/ASE files remain unchanged.

Ten PNGs and ten editable ASEs cover the five tested viewport layouts: one Left and one Right per viewport. Width is 108 native pixels; height matches the physical distance from board-frame top to screen bottom. The shaft's native width follows the actual board gutter, so it meets the frame while overscanning the physical screen edge by four native pixels. This is still a review fixture; arbitrary device-size production binding is a later integration task.

Unity 6000.3.19f1 compilation and SnowyMountainConnectedSidesReview passed: 720x1280, 1080x1920, 1080x2400, inset-safe 1080x2400 and 360x640. The fixture checks exactly two structure renderers plus two original lamps and banners, native scale, binary alpha, Point filtering, no gem overlap, no scenery input targets, and opaque coverage at each screen edge and board-frame join on every relevant scanline. Shared actor/UI geometry, feet, bars, gems and control state stay checked by the existing assembly fixture.

Paired screenshots preserve the entire upper scene, gem opening and center bottom HUD pixel-for-pixel. All ten ASEs reopened in LibreSprite with exact PNG pixel equality; all ten native silhouettes are one connected component. Existing 360x640 HUD crowding and Unity's development/editor pixel-camera warning remain. No Android build or physical phone test.

Native reproducibility: Scripts/BuildNative.ps1 -OutputDirectory <fresh-directory>, then Scripts/ValidateNative.py in this review for saved-source verification. Unity capture: Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainConnectedSidesReview -Graphics, after Tools/Validate-Unity.ps1.

PixelLab usage: 0. No generation or reference upload was needed for this native join correction. Draft PR #218. Stop for visual review; no merge.
