# Dungeon Matcher combat idle set

Edited in LibreSprite 1.1-dev on 2026-09-20 using its native pixel/cel API and visually reviewed at native 1× and integer 4×. All exports were produced by LibreSprite.

Each character has an editable `.aseprite`, a transparent horizontal `.png` sprite sheet, an animated `.gif`, and `.json` frame metadata. Every animation is **nine unique 64×64 frames at 130 ms each (1.17 seconds)**. Each sheet is **576×64**, with no trimming, padding, or scaling.

Open `Preview.html` for synchronized playback, frame stepping, 50% speed, dark/light backgrounds, and a frame-nine/frame-one seam view. The HTML also works directly from disk.

| Character | Changes | Opaque palette |
| --- | --- | --- |
| Rattlebones | Removed generated color drift; kept every original pose, mask, frame order, and duration. | 15 colors |
| Farmer | Calmer two-pixel head travel; torso, both grips, and the complete rigid pitchfork move together through a three-pixel stance response. Boots stay fixed. Corrected small straw/wood material leaks. | 14 colors |
| Pan Villager | Restored all material ramps; used upright source poses with a three-pixel stance dip, leg compression, and rigid pan movement. Recovery uses a distinct source pose. | 16 colors |
| Bardley | Preserved the restored materials, body compression and anchored base; retimed the exact existing eyes/catchlights/smile together so the face follows the body with at most a one-pixel step. | 15 colors |

The correction pass addresses the first delivery's tip-only pitchfork movement and premature three-pixel eye drop. Farmer now uses a stable source head/held-tool pose, with whole-pixel articulation below it; the head stays within one pixel of the torso's offset. Bardley's body silhouette and all pixels outside the local face repair remain unchanged. Rattlebones and Pan Villager are byte-for-byte unchanged by this refinement.

## Alignment and source preservation

- Canvas dimensions and native pixel scale are unchanged.
- Farmer and Pan Villager retain their source ground contact at y=63. Rattlebones retains its complete original geometry and timing.
- Bardley's base remains at **y=51**, matching the source animation. The static recolor's unresolved 12-pixel downward offset was not carried into the animation.
- `Originals/` contains snapshots saved directly from all four open LibreSprite documents before editing, including their in-memory edits. The original Downloads files were not overwritten.
- `Originals/FamilyPass1/` preserves the reviewed Farmer/Bardley delivery before this correction, and supplies the correction script's reproducible inputs.
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

`Scripts/validate_refinement.py` and `RefinementValidation.json` additionally check rigid translation of the fork tines, both grips and lower shaft, fixed Farmer boot pixels, reduced head travel, Bardley's exact facial glyph, one-pixel maximum face steps, and preservation of Bardley's non-face pixels. The face-to-body curl offset varies by only one pixel across the loop. The check also pins the unchanged Rattlebones and Pan Villager editable-file hashes.

Visual review covered each frame, material separation, native-size readability, dark/light backgrounds, side-by-side cadence, and the final-to-first transition. Numeric pixel-difference measurements are inspection aids, not a claim that an artistic loop is objectively perfect.

Unity validation/runtime testing was not run: this is an art-only delivery with no Unity serialization or code changes.

## Production helpers

- Run `inspect_idles.py` first, then `build_native_script.js` to assemble the native scripts and palettes. `RestoreIdleMaterials.js` followed by `UnifyIdleFamily.js` reproduces the first pass. `RefineIdleMotion.js` reads the preserved `Originals/FamilyPass1/` inputs and saves the corrected Farmer/Bardley candidates under `Review/`; review and save those as the matching root `_Idle.aseprite` files before exporting.
- `restore_materials_body.js`, `family_motion_body.js` and `refine_motion_body.js` record the native LibreSprite edit operations.
- `export_idles.ps1` exports the final editable files using the local LibreSprite executable.
- `inspect_idles.py`, `validate_idles.py` and `validate_refinement.py` are read-only artwork inspection/validation helpers. They do not recolor or generate the edited sprites. Use `validate_refinement.py --candidate` to check the review files before saving the final delivery.

The local `Review/` directory and temporary UI helpers are working evidence, excluded from the committed delivery. The final files at this directory's root are authoritative for this pass.
