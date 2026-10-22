# Approved hybrid dungeon - playable integration review

The user approved the complete Stage 5 design and authorized integration into the actual Dungeon battle scene, with a three-enemy preview before merging. This implementation is on `feature/dungeon-zone-hybrid-pixellab-rebuild`, draft PR #205. **No merge has been performed.**

[Actual in-game gallery](Review.html) | [720x1280 screenshot](Previews/Final/Dungeon-720x1280.png) | [Approved Stage 5](../Stage05/Review.html)

## Bottom corner bases removed — 2026-10-22

Removed the two decorative pillar bases circled by the user at the bottom screen
corners. The existing quiet wall-edge tiles now continue to the bottom viewport
boundary. Upper pillars, the extended floor, portrait, board and HUD remain intact;
no sprite artwork or gameplay code changed.

The existing actual-game integration test passed again at all five screen/safe-area
profiles, and `Tools/Validate-Unity.ps1` passed. Final screenshots now show this
revision. Fresh evidence is `PillarBaseRemovalTests.xml` and
`PillarBaseRemovalValidation.json`. Protected-art, user-file and live-layout checks
were refreshed. Earlier test receipts remain historical. PR #205 is unmerged.

## Floor continuity follow-up — 2026-10-22

The user requested a complete floor behind the portrait. The existing top-floor
strip began at native x=106, leaving wall/support surfaces visible in the gutters
beside the portrait. Four additional repeats of the unchanged approved floor tiles
now continue the same tile grid to the left viewport edge beneath the opaque masked
portrait. Existing enemy-side tiles, floor height, fascia, portrait, HUD and actor
positions are unchanged. No source artwork was edited.

The existing actual-game integration test was rerun after this four-line rendering
change: **one test passed**, covering all five screen/safe-area profiles with three
live enemies, Guide/Settings and an accepted swap. `Tools/Validate-Unity.ps1` passed
again. Final screenshot files now show this correction; earlier captures remain in
Git history. `FloorExtensionTests.xml` and `FloorExtensionValidation.json` record
the fresh checks. The 69-test result below describes the preceding integration run;
unaffected gameplay checks were not repeated. Preservation/layout/pixel checks
were refreshed and passed. PR #205 remains unmerged.

## Delivered

- The production `Game` scene now consumes the approved hybrid gate, architecture, props and HUD pieces through the existing Dungeon presentation resource. The ten gate pieces, 35 UI modules and ten prop exports remain unchanged; the existing 102-asset library remains available beneath this optional binding.
- `DungeonPresentationArt` still owns the background canvas, camera mapping and live enemy-floor alignment. Modular wall, support, floor and fascia pieces repeat at whole physical pixels; the larger approved gate follows the existing floor. Complete chains, banners, torches, skull, bones and barrel remain separate sprites behind live actors and controls.
- The portrait keeps its overfilled masked recess and approved frame. Wave, Guide, Settings, ability and supply frames use the approved native families and separate ornaments. Labels, ability symbols, energy fill, stock, cooldowns, input callbacks and state-based dimming keep their existing owners.
- The board retains its 64x64 cell and gem sources and previous responsive footprint. The approved frame still builds around the live board. Its dark backing lives on the world background canvas below the SpriteRenderer gems; only the frame is in overlay UI.
- No gameplay rules, character/gem/animation files, encounter definitions, save systems, production Game-scene serialization or other-zone art were changed. No generation or recoloring occurred.

## Actual Unity verification

**69 tests passed, zero failed** in the final affected regression run. This includes the new actual-game fixture and existing three-zone presentation, ability-symbol, accepted-move, pointer, crystal, unified-combat and continuation tests. `Tools/Validate-Unity.ps1` passed after the final code change using Unity 6000.3.19f1, exit 0.

Five actual Game captures show Rattlebones and three live enemies at 720x1280, 1080x1920, 1080x2400, 1080x2400 with safe rectangle (0,84,1080,2200), and 360x640. A disposable profile fills the two remaining slots with Pan Villager and Knight through the normal summoning API. The first enemy and weaknesses remain live game assignments; no production encounter was edited. Idle motion continues normally.

The checks verify native environment texel density, complete props, all ten gate pieces, 64 cells/gems, visible gem pixels in the captured board, active HP/intent views, portrait clipping and safe-area placement. Guide opens/resumes at every size; Settings opens/closes; an accepted swap advances exactly one move. The existing suite also checks other-zone presentation and playable ability symbols. Sections, board ratio and all three health-row rectangles match the fresh pre-integration baseline exactly at every captured size.

The first render exposed a board-backing ordering mistake despite valid board state; it was corrected before handoff and the actual-pixel visibility assertion now guards it. A shared regression assertion originally assumed every background Image was a single native sprite; it now checks the one repeated backing's texel density independently, retaining the original simple-sprite checks. The initial regression XML remains in `Diagnostics/Final/RegressionTests-initial-tiled-density-failure.xml`.

## Preservation and visual review

All **5,298 protected files** and the **eight pre-existing main-checkout edits** remain byte-identical. The original library, approved hybrid PNGs/ASE sources, imports, character art, gems, animations, gameplay source and scenes remain intact. Unity's incidental metadata/settings rewrites were excluded from this change.

Compared with Stage 5, the approved materials, focal gate, complete props and modular frame construction are preserved. The playable screen retains its established responsive geometry, live affinity/weakness icons, attack counters and health rows. Tall screens expose more quiet wall. The normal first-run teaching hint occupies the lower caption area, and an uncharged ability icon is dimmed by its existing availability state. These are live UI states rather than changes to source art. The board retains the user-approved uniform fractional fit when required; this does not distort X/Y proportions or imply integer board texels at every phone size.

Final screenshot review found the three enemy silhouettes grounded on the platform, readable faces and HP, visible complete gem board, intact frame joins and clipped portrait recess. Production Game does not load the art-only preview or a flattened screenshot.

This is graphics-enabled Unity automation and visual screenshot review. **No physical-device test or new Android build was performed for this integration.** Final gameplay visual approval remains with the user.

## Evidence and reproduction

- `Diagnostics/Final/RegressionTests.xml`: final executed test results.
- `Diagnostics/Final/UnityValidation.json`: compiler validation receipt and source hashes; full log retained locally.
- `Diagnostics/PreservationAndPixels.json`: protected sources, unchanged live geometry and visible-gem measurements.
- `Diagnostics/Baseline/` and `Previews/Baseline/`: fresh actual-scene baseline from this task.
- `Diagnostics/Final/` and `Previews/Final/`: final layout evidence and actual screenshots.
- `Scripts/verify_hybrid_integration.py`: independent preservation, geometry and screenshot checks.
- Explicit Unity test `DungeonHybridIntegrationTests.DungeonCombatAndPhoneScreens` reproduces the live fixture using a disposable account. The older historical baseline-capture tests are not run.

Stop for final gameplay visual approval. Keep PR #205 unmerged.

## Portrait banner and masonry follow-up

Removed the banner inside the current hybrid player portrait, preserving its masked wall, frame, character and UI. The refreshed `Previews/Final` screenshots reflect this change. The portrait and new masonry candidate fixtures both pass at all five phone/safe-area profiles, with unchanged board/section/health geometry. `Tools/Validate-Unity.ps1` passes again; all protected files and existing main edits are unchanged.

The separate [SideMasonry review](../SideMasonry/Review.html) contains one newly requested PixelLab source (6 generations), four LibreSprite-cleaned native modules, and actual Game captures from an editor-only candidate preview. These new pieces are not selected in production resources and await user review. Prior approved art remains unchanged. See [the masonry report](../SideMasonry/Report.md) for current evidence; no merge.

## Full-screen background correction

The current hybrid wall now covers the physical display beyond the gameplay safe area, with complementary half tiles completing the repeat. The original scene layout, portrait mask, art pixels and other-zone paths remain unchanged. Current `Previews/Final` captures include six screen/safe-area cases. The corrected corner candidate reaches the physical screen bottom; see [full-screen review](../SideMasonry/ScreenFill/Review.html). Three current Unity fixtures, the compiler validator, image-edge checks and preservation audit passed. No physical-device claim and no merge.

## Aligned edge masonry

The side courses now share a physical-bottom alignment and physical left/right anchors with the corner review pieces. The corner fixture replaces the lower three repeats per side instead of superimposing them. Artwork, board/UI geometry and gameplay are unchanged. Three current Unity fixtures and compiler validation passed. Native source-pixel comparisons cover the complete visible side stones and corner joins in all 12 captures. See [current masonry review](../SideMasonry/CourseAlignment/Review.html) and its report. Remains unmerged; corner additions remain review-only.

## Final visual approval and production binding

The user approved the aligned masonry and merging PR #205. Both exact corner sprites are now included in the production resource and runtime scene. The final verification fixture adds no editor decorations. See [approved production handoff](../SideMasonry/ApprovedIntegration/Report.md) and [actual Game captures](../SideMasonry/ApprovedIntegration/Review.html). Earlier pending-review statements above describe their historical checkpoints.
