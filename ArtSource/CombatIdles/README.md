# Dungeon Matcher combat idle set

Edited in LibreSprite 1.1-dev on 2026-09-20 using its native pixel/cel API and visually reviewed at native 1× and integer 4×. All exports were produced by LibreSprite.

Each character has an editable `.aseprite`, a transparent horizontal `.png` sprite sheet, an animated `.gif`, and `.json` frame metadata. Every animation is **nine unique 64×64 frames at 130 ms each (1.17 seconds)**. Each sheet is **576×64**, with no trimming, padding, or scaling.

Open `Preview.html` for synchronized playback, frame stepping, 50% speed, dark/light backgrounds, and a frame-nine/frame-one seam view. The HTML also works directly from disk.

Open `FarmerHeadReview.html` for the original Farmer, the previous static-head revision, and the restored-head correction side by side at native 1× and integer 4×. Each uses its recorded 130 ms durations: the original has 11 frames, and both revisions have nine. The original begins on its raised fifth frame for the initial comparison, then plays its full loop.

| Character | Changes | Opaque palette |
| --- | --- | --- |
| Rattlebones | Removed generated color drift; kept every original pose, mask, frame order, and duration. | 15 colors |
| Farmer | Restored the original raised, dipping, nodding, and recovering head/hat/face drawings, coordinated with the latest stance cycle. Preserved the improved torso, both grips, complete rigid pitchfork, and fixed boots. | 14 colors |
| Pan Villager | Restored all material ramps; used upright source poses with a three-pixel stance dip, leg compression, and rigid pan movement. Recovery uses a distinct source pose. | 16 colors |
| Bardley | Preserved the restored materials, body compression and anchored base; retimed the exact existing eyes/catchlights/smile together so the face follows the body with at most a one-pixel step. | 15 colors |

The first refinement corrected tip-only pitchfork movement and Bardley's premature eye drop. The subsequent Farmer-only correction replaces the overly static head with distinct original head drawings. The deepest nod falls on the low stance, followed by a short recovery lag. The head sequence uses original frames 5, 11, 6, 1, 2, 9, 8, 7, and 4 at the existing nine-frame cadence. No extra nod or lateral translation was added. All Farmer pixels below the lowest chin remain identical to the preceding revision, and the fork and grips retain their rigid three-pixel motion. Rattlebones, Pan Villager, and Bardley are byte-for-byte unchanged by this latest correction.

## Alignment and source preservation

- Canvas dimensions and native pixel scale are unchanged.
- Farmer and Pan Villager retain their source ground contact at y=63. Rattlebones retains its complete original geometry and timing.
- Bardley's base remains at **y=51**, matching the source animation. The static recolor's unresolved 12-pixel downward offset was not carried into the animation.
- `Originals/` contains snapshots saved directly from all four open LibreSprite documents before editing, including their in-memory edits. The original Downloads files were not overwritten.
- `Originals/FamilyPass1/` preserves the reviewed Farmer/Bardley delivery before this correction, and supplies the correction script's reproducible inputs.
- `Originals/BeforeHeadRestore/` preserves the exact latest Farmer body/prop revision before the head restoration. `Comparisons/` contains unmodified reference-sheet exports for the three-way preview.
- These are art-source deliverables, outside Unity's `Assets/` directory. No runtime behavior, animator, sprite import settings, GUIDs, or serialized scene/prefab references were changed.

## Color and shading authority

The supplied `Dungeon_Matcher_Art_Direction.txt` and `Dungeon_Matcher_Art_References.png` were inspected before editing. Exact material palettes are recorded in `Scripts/palettes.json` and the four `.gpl` files. `Scripts/material_map.json` documents deliberate replacements for every source color, grouped by destination material tone; it is not an unrestricted nearest-color quantizer.

Bardley's individual `BardleyFinal.png` supplied the spatial material-shading reference, read in the original animation alignment. An unchanged copy is retained as `References/Bardley_Palette_Reference_Placement_Pending.png`. Every shade remains in the approved palette. Source outlines, eyes, material boundaries and animation shapes were retained during recoloring. The subsequent motion edits use integer pixel placement only.

## Validation performed

`Validation.json` records measurements and SHA-256 checksums. `Scripts/validate_idles.py` verifies all 36 frames:

- 64×64 canvas, nine distinct poses, 130 ms duration, and 1,170 ms loop;
- zero off-palette pixels and only alpha 0/255;
- exact frame-by-frame equivalence between ASE, PNG strip, and decoded GIF;
- infinite GIF looping and matching JSON timing/size metadata;
- unchanged Rattlebones masks and stable Farmer soles / Pan Villager sole row / Bardley base row.

`Scripts/validate_refinement.py` and `RefinementValidation.json` additionally check rigid translation of the fork tines, both grips and lower shaft, fixed Farmer boot pixels, Bardley's exact facial glyph, one-pixel maximum face steps, and preservation of Bardley's non-face pixels. The face-to-body curl offset varies by only one pixel across the loop. The check also pins the unchanged Rattlebones and Pan Villager editable-file hashes.

`Scripts/validate_farmer_head.py` and `FarmerHeadValidation.json` verify that edits stay within the head area, every pixel below the lowest chin is unchanged, and the actual prop/grip pixels retain their rigid movement. The check distinguishes nine different face drawings after removing vertical translation from the previous revision's one repeated face drawing. It also verifies the comparison sheets against their original ASE files and pins all three unchanged characters. These preservation checks complement the visual comparison; they do not determine whether the motion feels natural.

Visual review covered each frame, material separation, native-size readability, dark/light backgrounds, side-by-side cadence, and the final-to-first transition. Numeric pixel-difference measurements are inspection aids, not a claim that an artistic loop is objectively perfect.

Unity validation/runtime testing was not run: this is an art-only delivery with no Unity serialization or code changes.

## Production helpers

- Run `inspect_idles.py` first, then `build_native_script.js` to assemble the native scripts and palettes. `RestoreIdleMaterials.js` followed by `UnifyIdleFamily.js` reproduces the first pass. `RefineIdleMotion.js` reads the preserved `Originals/FamilyPass1/` inputs and saves the corrected Farmer/Bardley candidates under `Review/`; review and save those as the matching root `_Idle.aseprite` files before exporting.
- `RestoreFarmerHead.js` reads the preserved original head poses and `Originals/BeforeHeadRestore/Farmer_Idle.aseprite`, and saves `Review/Farmer_HeadRestored.aseprite`. Review it with `FarmerHeadReview.html?candidate=1`, then save/export only Farmer to the root delivery files.
- `restore_materials_body.js`, `family_motion_body.js`, `refine_motion_body.js`, and `restore_farmer_head_body.js` record the native LibreSprite edit operations.
- `export_idles.ps1` exports the final editable files using the local LibreSprite executable.
- `inspect_idles.py`, `validate_idles.py`, `validate_refinement.py`, and `validate_farmer_head.py` are read-only artwork inspection/validation helpers. They do not recolor or generate the edited sprites. Use `validate_farmer_head.py --candidate` for the latest Farmer working copy; `validate_refinement.py --candidate` checks the preceding Farmer/Bardley review files.

The local `Review/` directory and temporary UI helpers are working evidence, excluded from the committed delivery. The final files at this directory's root are authoritative for this pass.
