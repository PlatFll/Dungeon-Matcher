Snowy Mountain side/corner polish — Stage 2 complete, awaiting user review

SCOPE
Board-side native asset generation and LibreSprite cleanup only. No bottom-corner generation, scene assembly, production import or merge. All earlier sources and approved art remain intact. Top HUD, battle environment, player portrait, characters, gems, board/frame dimensions and central bottom HUD are unchanged.

ART DELIVERED
Two separately generated cliff/support designs. Left: snow-heavy rock buttress with an embedded upright. Right: exposed diagonal timber bracing, layered snow and short icicles. Each complete cleaned source is 44x142, accompanied by four overlapping modules: upper, middle A, middle B (44x60), and footing (44x52). Ten editable ASE files and ten native transparent PNGs total. Both complete designs use 14 opaque colors; modules use 13–14.
Raw PixelLab input is 40x128. Native editing adds 2px registration padding, completes the cut bottom contours, unifies material ramps, removes the small unrelated left faceted ornament and connects the right cliff shadows. No source-pixel resampling. The native mask is intentionally changed for these localized structural fixes; this is not a geometry-locked recolor.
Left candidate 0 was selected for its broad snow/rock shapes. Right candidate 1 was selected for its complete cap and strong timber bracing. All eight raw candidates remain saved. Other candidates are unselected, not approvals.

MODULAR ASSEMBLY CONTRACT
Upper at y=0, MiddleA at y=40, MiddleB at y=56 and Foot at y=90 reconstruct each complete source byte-for-byte. These are overlapping structural slices, not edge-to-edge seamless terrain tiles. Keep complete source pixels at integer native scale; do not stretch to fill height. Stage 4 must establish variable-height overlap placement, test repetition, and hide only structural underlap behind the opaque board rim. The 42px occupied width exceeds the common 30–31px external gutter; do not shrink the board or intrude into the grid. Approved lantern/banner pieces remain available unchanged for later supported placements.

VERIFICATION
All ten native sources were saved in LibreSprite, reopened in LibreSprite batch mode, and exported pixel-identically. Every PNG has only alpha 0/255, transparent outer canvas clearance and one connected 8-neighbor opaque component. Both complete sources reconstruct exactly from their native modules. Exact dimensions, bounds, palettes, file hashes and component evidence are in QA/Manifest.json. Pixel art was visually reviewed at 1x and integer nearest-neighbor enlargement.
No fresh Unity compilation or runtime capture was required or run for this ArtSource-only checkpoint. Existing Stage 9 captures are explicitly linked as the unchanged Unity baseline, not proof of new asset integration. Stage 4 owns the actual viewport, UI overlap and scene-placement checks.

PIXELLAB
Two create_image_pro jobs, four candidates per job, 10 generations each: 20 total. Balance 4,888 -> 4,868; USD credits unchanged at $0.00. Actual Rattlebones rendering reference and the existing SnowRock/Support_Left PNGs were sent as labeled style/material inputs. Requests, receipts, seeds and hashes remain in Raw/ and QA/Lineage.json. No extra generation was used for cleanup.

NATIVE TOOL RECOVERY
The first command-line attempts stopped at a hidden sequence-import prompt caused by numbered candidate filenames; batch-plus-script was unsupported by this build. The selected PNGs now use non-sequential temporary aliases. The final normal --script run exits 0; --batch is used only for exact reopen/export. Earlier unsuccessful working passes remain outside the repository and are not passing evidence.
Scripts/BuildNative.ps1 reproduces the native work into a new temporary directory and never overwrites approved art. Raw blank canvases are empty export canvases. BuildNative.js performs all material edits, contour completion and slicing inside LibreSprite.

ARTISTIC VERDICT
Suitable board-side candidates for user review: substantial connected snow/rock masses, clearer timber support logic, quieter native material colors. The left and right differ in structure without mirrored lighting. Final composition approval depends on the later Unity assembly with the larger corner kits.

STOP
Wait for approval before Stage 3 bottom-corner generation. No merge.
