Snowy Mountain / Frostwake Peaks — Stage 4 floor and separator review
===================================================================
Stage 3 was accepted by the user's Continue. This stage supplies only the snowy
battle floor, block separator and their disposable actual-Game Unity preview.
STOP for Stage 4 review. Stage 5 has not begun. No merge or production bindings.

Nine native modules: three 64x32 snow/stone floor variations, three 64x24 block
faces, one 64x32 dark foundation repeat, and two complete snow overhangs
(109x35 and 73x23). Seven meaningful opaque colors; binary transparency.
One text-only PixelLab Pro source, 320x96; 10 subscription generations used.
Balance 4,930. Cumulative staged spend 70. No reference images uploaded.

LibreSprite cleanup: cold material ramps, matching floor repeat boundaries,
completed native stone faces where the generated source was cut, connected
snow silhouettes with canvas margin, editable ASE save/reopen and exact PNG
roundtrip. Raw generation and prompt/receipt remain alongside exports.

Unity: new art follows the existing BattleFloorAnchor. The walking-surface
canvas extends 20 native pixels behind and 12 ahead of the unchanged feet line;
its back snow silhouette has 8-10 native pixels of transparent clearance.
The complete 24px stone face sits below it, with a quiet foundation repeat where
the existing responsive layout leaves more height. A separate ledge mask allows
the whole face to fit; the approved backdrop and portrait masks are unchanged.
The legacy bottom enclosure rail/corners are hidden only in this preview.
Floor remains behind live characters, health bars, portrait and weakness markers.

Tools/Validate-Unity.ps1 passed. The final actual-Game preview test passed at
720x1280, 1080x1920, 1080x2400, inset-safe 1080x2400 and 360x640. Checks retain
shared geometry and common feet alignment, verify native source scale, and
require real snow/stone pixels in each capture. Pixel comparisons show zero
changed pixels outside the floor/replaced lower rail. All 8,974 protected files,
including approved Stages 2/3 and the six preexisting local edits, retain hashes.

Art verdict: the bright snowy standing plane and darker broad block face read
as a frozen mountain ledge, with clear separation from the board. The approved
alpine vista, portrait, characters, gems, controls and gameplay are preserved.
These are Unity Editor captures, not Android or real-device tests. Retained
baseline: legacy camera warning/crowded labels at 360x640; old enclosure, board
and bottom HUD artwork await later stages and are not finished Snowy art.

Open Review.html for the paired screenshots, native assets and source.
Reproduce: Tools/Test-ForestFoundation.ps1 -Filter
ForestFoundationPlayTests.SnowyMountainStage04FloorReview -Graphics
Branch codex/snowy-mountain-pixellab; draft PR #218. Do not merge.
