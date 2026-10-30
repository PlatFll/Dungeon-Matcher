SNOWY MOUNTAIN SIDE/CORNER POLISH — STAGE 3
Bottom-corner asset generation complete; awaiting user visual review.
No Unity assembly, production import or merge in this stage.

DELIVERY
Two larger connected snow/rock corner cores, two lossless approved cliff connectors,
and two separately authored snow aprons. Six native PNGs and editable LibreSprite ASEs.
Corner_Left: 104x88, 14 opaque colors; broad drift, buried fence and embedded ice.
Corner_Right: 108x92, 14 opaque colors; stepped rocky ledge, small snow-covered
supply crate and timber support. The generated clipped right/bottom contours are
completed in added native canvas space. No resampling of source artwork.
Connector_Left/Right: 44x60, exact RGBA copies of approved Stage 2 MiddleA courses.
SnowApron_Left/Right: 64x24, four colors, separate overlap pieces for lower drift joins.

SELECTION / EDITS
Left_3 selected for strong angular rock and a low inward snow slope.
RightCargo_0 selected for a substantial coherent stone mass and visibly supported cargo.
First right job rejected: it repeated the narrow support motif and missed the cargo ledge.
All twelve raw candidates remain immutable in Raw. Left/right are separately generated;
lighting was not mirrored. Material mappings in BuildNative.js remove near-duplicate
generated colors, using the approved snow, rock, timber and ice ramps.
Left mask and source registration are exact at +4,+4. Right edits are limited to
the final four source columns and newly added canvas; the remaining source positions
match the explicit material remap exactly. Source references and their hashes are in QA.

MEASURED CHECKS
All six native sources reopened and exported exact RGBA pixels.
All six PNGs: binary alpha, one 8-connected opaque component, clear canvas margins.
Both connectors preserve approved pixels exactly. No blur or artwork resizing.
Scripts/BuildNative.ps1 recreates native sources/exports in a fresh output folder.
Scripts/ValidateAndReview.py checks reopened pixels and source registration, then
creates labelled native / nearest-neighbor inspection layouts, not replacement art.

ASSEMBLY LIMITS / NEXT STAGE
These are modular overlap pieces, not seamless terrain tiles. At Stage 4, place a
fixed-size core below the HUD controls, continue a narrow cliff behind it, and overlap
the snow apron with the core. Native connector crops/repetition must fit the measured
90-171px lower-region heights without stretching. Verify actual potion/bomb/text bounds,
screen-edge coverage, source contour continuity and all protected UI at every viewport.
The new cores have not been fitted or tested in Unity yet. Existing Stage 9 captures
are historical unchanged baseline evidence, not screenshots of these new assets.

COST / SCOPE
PixelLab: three Pro calls, four candidates each, 30 subscription generations total.
Balance 4868 -> 4838; USD balance remains $0.00. No dollar credits spent.
Only additive ArtSource candidate files and the staged-art README checkpoint changed.
Approved Stage 2 side kit, upper HUD, characters, gems, board/frame, shared UI,
production assets, runtime code and unrelated local work are preserved.
Stop before Stage 4 Unity assembly. Do not merge.
