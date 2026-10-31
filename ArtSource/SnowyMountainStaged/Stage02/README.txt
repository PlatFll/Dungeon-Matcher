Snowy Mountain / Frostwake Peaks - Stage 2 review candidate
=========================================================

Scope: top environmental backdrop only. Stop here for user review before Stage 3.
Stage 1 was accepted by the user's Continue instruction.

Selected: AlpinePass, a 384x320 sheltered alpine pass with snow shelves, pines,
a receding rope-fenced path, a stone bridge, layered mountains and ruined towers.
PixelLab Pro generated two text-only original plates. B was selected; A is
retained as an alternate. The supplied mockup was inspected, never uploaded.

LibreSprite cleanup:
- 40 source colors consolidated to 28 intentional opaque colors.
- 29 fully surrounded one-pixel plane flecks removed.
- Canvas and native pixel geometry unchanged; no resampling.
- Editable Native/AlpinePass.ase reopened to Roundtrip/AlpinePass.png.
- Roundtrip and export pixels match exactly. See QA/Native.json.
- Golden reference hashes match Stage01/References.json and the live register.

Actual Unity preview:
- Editor-only explicit test opens the production Game scene in frostwake-peaks
  with a disposable profile, Rattlebones and three actual Frostwake enemies.
- Only the Architecture scenery layer is temporarily hidden and a masked
  candidate SpriteRenderer is added. The test restores the original hierarchy.
- Native plate pixels map uniformly to 1, 2 or 3 physical pixels. Responsive
  overscan frames the bridge/towers; it never stretches the scenery.
- No production theme, prefab, scene, floor, UI, cell, gem or actor is saved/edited.
- Five captures: 720x1280, 1080x1920, 1080x2400, inset-safe 1080x2400, 360x640.
- Shared layout validator and exact before/after geometry comparisons pass.
- Screen differences are confined to the existing background viewport.
  Board, bottom HUD and opaque portrait interior are pixel-identical.
  Background legitimately changes through transparent portrait frame fringes.
- All 8,835 baseline production files and all six preexisting edits retain hashes.

Validation:
Tools/Validate-Unity.ps1 passed after the final framing edit.
Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainStage02BackdropReview -Graphics
passed, 1 test / 0 failures, including all five viewport captures.
QA/UnityChecks.json and QA/UnityTests.xml record execution evidence.
These are Unity Editor renders, not real-device tests or an Android build.

Known legacy limitations:
The old Frostwake outer enclosure, portrait/board materials and lower HUD remain
visible intentionally. Their artwork stages have not begun. The original 360x640
Pixel Perfect Camera warning and crowded labels also remain visible in the
baseline. This backdrop stage does not claim to fix those existing UI issues.

PixelLab: two Pro calls at 25 generations = 50 spent; 4,950 remaining.
All prompts, seeds, receipts and output hashes: Generation.json.
Preview: Review.html; full source-size assets: Raw/ and Exports/.

Reproduce: run the explicit Unity test above; it uses Exports/AlpinePass.png.
LibreSprite cleanup: Scripts/RunLibreSprite.ps1 (working-copy outputs only).
Branch codex/snowy-mountain-pixellab; draft PR #218. Do not merge.
