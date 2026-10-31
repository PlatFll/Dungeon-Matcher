REJECTED / SUPERSEDED — the user rejected this duplicated backing layer. Open ../SidePeek/Review.html for the corrected original-side placement.

Historical evidence follows:

SNOWY MOUNTAIN — SCREEN-EDGE REVISION
Focused follow-up to Stage 4, 2026-10-31. Review only; no merge.

The transparent outside silhouettes and safe-area gutters exposed thin strips of masonry. Matching rock continuation now fills both outer sides from the board-frame top to the physical bottom. It extends three native pixels beyond the left/right viewport edges as requested. Foreground side structures, lanterns, banners and corner formations retain their exact placement and artwork.

No generated or edited PNGs. The continuation uses native rectangular views of the approved Left_MiddleA (source X=9, width up to 22) and Right_MiddleA (source X=14, width up to 20), with top-origin source Y=10 and height up to 40. Whole source pixels repeat beneath the foreground; final courses use smaller source rectangles. No stretching, mirroring, recoloring, soft filtering or new image credit spending.

Unity compilation and SnowyMountainSideCornerEdgeReview passed at 720×1280, 1080×1920, 1080×2400, inset-safe 1080×2400 and 360×640. Continuous opaque coverage at both screen-edge columns is asserted for every scanline. Only designated continuation courses may cross the screen edge, within the requested three-pixel native allowance. All existing foreground artwork retains full opaque clearance. Native scale, binary alpha, unchanged gem opening, no input targets, UI geometry and shared control state remain checked.

Paired screenshots preserve the entire upper scene and center board/lower HUD pixel-for-pixel. See Review.html and QA/Validation.json. Existing 360×640 HUD crowding and the Unity editor warning remain; this is not a device test or production integration. Prior Stage 4 screenshots and all art sources are retained.

Reproduce with Tools/Validate-Unity.ps1, then Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainSideCornerEdgeReview -Graphics. Run Scripts/Validate.py with Foundry Python/Pillow to recheck saved evidence.

PixelLab spend: 0. Draft PR #218. Stop for review; do not merge.
