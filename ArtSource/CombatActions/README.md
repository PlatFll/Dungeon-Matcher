# Dungeon Matcher combat actions

Current production art-direction pass, 2026-09-20. Edited and retimed in LibreSprite 1.1-dev using native cels, pixel operations and Frame Properties; exported by LibreSprite. The supplied originals are preserved in `Originals/`.

| Clip | Frames | Canvas | Frame durations (ms) | Total |
|---|---:|---|---|---:|
| Farmer AutoAttack | 8 | 96×64 | 80, 80, 120, 40, 120, 80, 80, 80 | 680 ms |
| Pan Villager AutoAttack | 8 | 96×64 | 80, 80, 120, 40, 120, 80, 80, 80 | 680 ms |
| Rattlebones Ability | 10 | 64×64 | 80, 80, 120, 80, 120, 80, 80, 80, 80, 80 | 880 ms |
| Bardley Ability | 10 | 64×64 | 80, 80, 120, 80, 120, 80, 80, 80, 80, 80 | 880 ms |

Each root clip has an editable `.aseprite`, untrimmed horizontal PNG sheet, GIF and JSON timing. Auto-attack impact is **frame 5 at 320 ms**. The ability release pose starts at 360 ms. These actions play once in Unity and return to idle; GIFs repeat for review.

Farmer's new attack is a short pitchfork thrust: both hands and the rigid tool move together, shoulders lean and boots stay grounded. Pan Villager keeps the supplied overhead swing, with a completed pan edge and cool iron material. Rattlebones uses the supplied royal gesture; Bardley plays his pipe and returns to his original smile. Approved palettes, outline and material ramps remain unchanged. Sources with color drift were corrected across all selected frames. Native skull material cleanup removes gray-to-gold ambiguity inside Rattlebones' skull while preserving the gold pauldron.

The user-adjusted Farmer idle is copied into Unity exactly. Its saved native source is preserved as `Originals/Farmer_Idle.aseprite`; all nine current idle drawings and durations match. Bardley's idle moves down exactly 12 pixels, with no rescaling or lost pixels. Both his idle and ability now stand at y=63 on the same 64×64 canvas. Other idle drawings are unchanged. The previous idle pass is archived in `../CombatIdles/Originals/BeforeCombatActions/`.

Enemy attack canvases expand by 16 pixels on each side. Unity imports fixed 96×64 rectangles with bottom-center pivots, preserving the 64-pixel reference body scale. Player clips and all idles use fixed 64×64 rectangles. No per-frame trimming, recentering, transform animation, blur, filtering or palette additions.

## Review and validation

Serve `ArtSource/` locally and open `CombatActions/Preview.html` for synchronized idle/action playback, game speed, slow motion, native 1× and integer 3× views. The page loads exported timing metadata. Native files are the editing authority; sheets are the Unity import source.

- `Scripts/validate_actions.py` checks all 36 action frames, palette membership, binary alpha, side margins, bottom contact, native/PNG/GIF equivalence, timing and exact ready-pose continuity. It also proves the user Farmer idle is unchanged and Bardley's idle differs only by the 12-pixel translation.
- `../CombatIdles/Scripts/validate_idles.py` verifies all 36 current idle frames and their exports. Prior motion/cheek/scarf validators target archived revisions.
- `Validation.json` records current art checks; `AssetManifest.json` records source/output hashes and provenance.
- [Unity evidence](../../Docs/Validation/COMBAT_ACTION_VALIDATION.md) covers actual hit timing, ability activation, return to idle, pause/cancellation/fallback and every pose at both tested portrait sizes.

## Production helpers

`Scripts/build_actions_body.js`, `material_map.json` and `palettes.json` record the native draft construction and material choices. `assemble_native.py` assembles the text-only LibreSprite helper. It never edits images itself. `retime_native.js` opens native duration dialogs; enter the documented durations and verify the saved files. `finish_rattle_native.js` records the final material-only skull correction. Verify that every source layer is transparent (Layer → Layer from Background when needed), then save before export; checking RGBA pixels alone does not detect a background-layer flag that can flatten a GIF.

Run `Scripts/export_actions.ps1` for native exports, then both pixel validators. `CombatActionImporter.Run` imports exact sheets and extends existing Unity controllers. `CombatActionValidation.ImportAndRun` imports and runs the graphics-enabled production-scene checks. The helper construction is provenance, not an instruction to overwrite a later hand-edited native file.

The persistent [art-direction guide](../../Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt), v1.2, defines the shared idle, attack and cast family and exact timing defaults. No board rules, ability effects, HP/shield rules or damage amounts are changed by this art pass.
