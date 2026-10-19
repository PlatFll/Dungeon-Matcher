# Dungeon PixelLab staged asset library

Stages 00–03 complete. Stages 01/02 are user-approved. **Stopped before Stage 04; Stage 03 awaits review.**
Draft PR: https://github.com/PlatFll/Dungeon-Matcher/pull/203. No merge or production integration.

`Review.html` shows the floor-top family, actual Unity contact-marker proof and native source comparison.
`Review_Stage02.html`, `Review_Stage01.html` and `Review_Stage00.html` preserve earlier galleries.

- `Sources/`: immutable PixelLab wall, recess and sidescroller outputs with provenance.
- `Native/` and `Exports/`: 14 wall pieces, 2 recess pieces and 5 floor-top pieces.
- `Previews/`: native/enlarged inspection sheets and real Unity screenshots.
- `Progress/`: stage reports, state, manifest, actual tool receipts and spending ledger.
- `Diagnostics/`: native cels, seams, masks and render checks; Unity test/validator receipts.
- `Scripts/`: LibreSprite native authoring/export and independent QA; no generation calls.

The isolated `ArtPreview_DungeonPixelLab` scene contains the approved wall/recess, unchanged
reference portrait rails, and one shallow 254×12 floor-top strip. No front fascia yet.
It remains outside build settings with no actors, gems, input or gameplay owners.
Temporary floor-contact markers are removed after runtime capture and never saved.

Reproduce Stage 03 native sources with `Scripts/BuildFloorStage03.ps1`, using the installed
LibreSprite. Its existing project layer-metadata helper preserves transparent corner pixels.
The explicit Unity test `DungeonPixelPipelineStage03Tests.Stage03FloorNativePreview`
rebuilds only the isolated art scene. Then use Foundry Python to run `Scripts/verify_stage03.py`
and `Scripts/build_stage03_review.py`. Do not run earlier stage scene builders over later assembly.

Point, FullRect, 64 PPU, no compression/mipmaps/NPOT resizing. Production Dungeon art,
UI layout, gameplay and the user's main checkout remain unchanged. Stage 00's production
scaling finding remains a later integration concern; isolated art is verified at 1× and 3×.

Confirmed cumulative PixelLab charge: **13/35 equivalents**; account remaining **1041.20**.
Next: `PROMPTS/04_GENERATE_FRONT_PLATFORM_FASCIA.txt`. Wait for user authorization.
