# Dungeon Matcher combat idle set

Edited in LibreSprite 1.1-dev on 2026-09-20 using its native pixel/cel API and reviewed at native 1× and integer 4×. All editable files and image exports were saved by LibreSprite.

Each character has an editable `.aseprite`, a transparent horizontal `.png` sprite sheet, an animated `.gif`, and `.json` frame metadata. Every animation has **nine 64×64 frames at 130 ms each (1.17 seconds)**. Each sheet is **576×64**, without trimming, padding, or scaling. Farmer and Pan Villager retain their raised ready pose across the loop seam; both have seven distinct drawings. The production cadence is recorded per character/frame in [the project art-direction guide](../../Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt).

The final localized polish changes **13 lower-cheek pixels in Farmer frames 1 and 9 only**, guided by `References/Farmer_RoundFace.png`. Eyes, mouth, hat, body, tool and frames 2–8 are unchanged. Pan Villager gains a small rear scarf-tail lift on descent and trailing settle on recovery, guided by the old animation saved from the user's open tab in `Originals/FinalPolishReference/`. Only the tail pixels change; head, face, body and pan remain identical. Rattlebones and Bardley source/export files are unchanged. `Originals/BeforeFinalPolish/` preserves both pre-polish animations.

Open `Preview.html` for synchronized playback, frame stepping, half speed, dark/light backgrounds, loop-seam inspection, and a previous/current Farmer and Pan comparison. It also works directly from disk. `FarmerHeadReview.html` compares the original 11-frame Farmer, the preceding nine-frame revision, and the current correction.

| Character | Current motion and materials | Opaque palette |
| --- | --- | --- |
| Rattlebones | Original gold-standard geometry, frame order and timing, with approved bone, cape, crown and cyan-eye ramps. Unchanged in the latest pass. | 15 colors |
| Farmer | Complete connected poses from the Farmer reference open in LibreSprite: head, shoulders, torso, hands and pitchfork retain their relationship. The blink closes at the bottom and opens on recovery. | 14 colors |
| Pan Villager | Head follows the downward/upward phase of Rattlebones, with shoulder and waist compression, planted feet, and a blink at the bottom. The iron pan and gripping arm move together. | 16 colors |
| Bardley | Approved material separation, grounded slime compression and existing eyes/smile retimed to follow the body with at most a one-pixel face step. Unchanged in the latest pass. | 15 colors |

The preceding motion correction addressed Farmer's nod continuing while the body rose. His cycle selects whole reference poses in source-frame order **5, 5, 6, 7, 8, 9, 7, 6, 5** (one-based). There is no independently delayed head cutout or extra tip-only motion. This replaced the preceding revision's body and tool drawings with those from the user-selected reference; it does not preserve that revision's exact rigid tool translation. The reference silhouettes remain intact except for the final authorized cheek rounding in frames 1 and 9. Palette cleanup and the closing/reopening eyelids are applied within those poses.

Farmer and Pan Villager close their eyes on **frame six**, matching Rattlebones' lowest blink phase, and reopen during the upward recovery. Pan Villager's torso shortens from shoulders toward waist rather than translating as a rigid block. Her head remains connected to the collar; her pan and gripping arm translate without deformation. The pan naturally passes in front of part of the left boot at the deepest dip, while the sole contact stays fixed.

## Sources and alignment

- All canvases remain 64×64 with integer pixel placement and binary transparency.
- Rattlebones, Farmer and Pan Villager retain ground contact at y=63. Bardley's base remains at **y=51**, as in its source animation. The static recolor's unresolved 12-pixel offset was not adopted.
- `Originals/TurnBasedReference/Farmer_OpenReference.aseprite` is the exact reference saved from the user's open LibreSprite tab for this pass. The original Downloads file was not overwritten.
- `Originals/BeforeTurnBasedPass/` preserves the Farmer and Pan Villager files immediately before the latest pass. Their unmodified comparison sheets are in `Comparisons/`.
- Earlier snapshots in `Originals/`, `Originals/FamilyPass1/` and `Originals/BeforeHeadRestore/` remain available as production history.
- These editable sources remain outside Unity's `Assets/`. Exact PNG copies are now imported under `Assets/_Game/Art/CombatIdles/`; new UI Image clips/controllers are in `Assets/_Game/Animations/CombatIdles/`. The existing player/enemy definitions select them. The existing player HUD centering component now spaces the complete sprite, affinity gem and health bar before centering them. Scene/prefab files and gameplay rules are unchanged.

## Color and shading authority

The supplied `Dungeon_Matcher_Art_Direction.txt` and `Dungeon_Matcher_Art_References.png` were inspected before editing. Exact palettes are recorded in `Scripts/palettes.json` and the four `.gpl` files. `Scripts/material_map.json` records explicit source-to-material replacements, rather than unrestricted nearest-color quantization.

Bardley's individual `BardleyFinal.png` supplied the spatial shading reference, read in the original animation alignment. An unchanged copy is retained as `References/Bardley_Palette_Reference_Placement_Pending.png`. All characters use the approved `#0A0D11` outline and existing material ramps. No new colors, antialiasing or blur were introduced.

## Validation

`Scripts/validate_idles.py` and `Validation.json` cover all 36 delivered frames:

- 64×64 canvases, nine frames at 130 ms and a 1,170 ms loop;
- zero off-palette pixels and only alpha 0/255;
- exact frame-by-frame equivalence between ASE, PNG strip and decoded GIF;
- infinite GIF looping and matching JSON frame sizes/timing;
- original Rattlebones masks and stable Farmer soles, Pan Villager soles and Bardley base.

`Scripts/validate_turn_based.py` and `TurnBasedValidation.json` additionally verify Farmer's full reference-pose silhouettes, downward/upward head order, both villagers' blink phase, rigid Pan Villager pan and grip pixels, visible boot preservation, and unchanged Rattlebones/Bardley file hashes. The source frame selection and measured head/eye positions are included in the report.

Visual review covered all poses, full-speed side-by-side playback at 1×/4×, the closed-eye low pose, upward recovery, previous/current comparisons, dark/light backgrounds and the final-to-first transition. Pixel checks establish asset integrity; they do not determine whether an artistic loop feels natural.

`RefinementValidation.json` and `FarmerHeadValidation.json` are explicitly **historical** reports for the preserved preceding revision. Their scripts read the archived Farmer/Pan files. The former also checks the still-current Bardley facial correction. They do not claim the new Farmer keeps the superseded body's pixels.

`Scripts/validate_final_polish.py` and `FinalPolishValidation.json` verify the exact authorized cheek coordinates and scarf-tail region, unchanged other frames/features, unchanged Rattlebones/Bardley files, and identical first/last poses for both villagers.

## Unity integration

`CombatIdleImporter.Run` imports the source PNGs unchanged, reads JSON timing and assigns the existing `PlayerDefinition`/`EnemyDefinition` visual fields. Clips animate only `Image.m_Sprite`. Their 100 Hz authoring clock represents each 130 ms exposure exactly; a final held sample ends at 1.17 seconds without adding a first-frame tick. Existing combat triggers, damage/impact timing and UI layout retain their owners.

All imports use Point filtering, uncompressed textures, no mipmaps, 64 PPU and Full Rect meshes with fixed bottom-center pivots. Rattlebones/Farmer/Pan use 64×64 import rectangles. Bardley uses one fixed 64×52 rectangle at texture y=12 across every frame, excluding its 12 empty bottom rows. The PNG remains a 576×64 sheet; its art pixels and source placement are unchanged. The existing UI floor anchoring aligns the actual contact without a new positioning component or animated transform.

`PlayerHudCenteringController` keeps four logical pixels between the character, affinity gem and health-bar containers, then centers their combined bounds. It includes the modular health bar's visible children even though its legacy root Image is disabled. This prevents hidden feet and excessive gaps when the selected player's fixed sprite rectangle changes.

Engine checks and portrait playback evidence are recorded in [the Unity validation record](../../Docs/Validation/COMBAT_IDLE_VALIDATION.md).

## Production helpers

- Run `inspect_idles.py`, then `build_native_script.js` to assemble the native LibreSprite scripts and palettes.
- `TuneBattleIdles.js`, assembled from `turn_based_motion_body.js`, reads the saved open Farmer reference and pre-pass Pan Villager. Run it in LibreSprite to save `Review/Farmer_TurnBased.aseprite` and `Review/PanVillager_TurnBased.aseprite`. After exporting matching candidate sheets, inspect them with `Preview.html?candidate=1` and run `validate_turn_based.py --candidate`. Save approved editable files to the root and export through LibreSprite.
- `export_idles.ps1` exports the root editable files using LibreSprite. `validate_idles.py` checks all final exports; `validate_turn_based.py` checks the current motion correction.
- `RestoreIdleMaterials.js`, `UnifyIdleFamily.js`, `RefineIdleMotion.js` and `RestoreFarmerHead.js` reproduce earlier stages from their retained inputs. Their body scripts and historical validators remain for traceability.
- Python helpers inspect artwork and write reports/contact sheets; they do not edit the delivered sprites.
- `final_polish_body.js` applies the final scoped cheek/scarf edits through LibreSprite. `validate_final_polish.py` checks their pixel boundaries. `CombatIdleImporter` and `CombatIdleValidation` are Unity editor-only import and integration helpers.

The local `Review/` directory and generated UI scripts are excluded from the committed delivery. The root `_Idle` files are the current deliverables.
