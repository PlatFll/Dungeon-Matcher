# Dungeon PixelLab staged asset library

Stages 00–02 complete. Stage 01 is user-approved. **Stopped before Stage 03; Stage 02 awaits user review.**
Draft PR: https://github.com/PlatFll/Dungeon-Matcher/pull/203. No merge or production integration.

`Review.html` shows the separate portrait recess, actual Unity captures and clipping controls.
`Review_Stage01.html` and `Review_Stage00.html` preserve previous stage reviews.

- `Sources/Stage01/` and `Sources/Stage02/`: immutable new PixelLab source images.
- `Native/` and `Exports/`: 14 wall documents/PNGs and two recess documents/PNGs.
- `Previews/`: native and enlarged inspection sheets; real Unity screenshots.
- `Progress/`: reports, state, provenance, manifest and measured spending.
- `Diagnostics/`: native pixel, clipping and render checks; Unity test/validator receipts.
- `Scripts/`: reproducible native LibreSprite builds and independent QA; no generation calls.

The isolated `ArtPreview_DungeonPixelLab` scene now combines the approved wall with the separate
masked recess and unchanged existing portrait rails as reference placeholders. No actors,
gems, input or gameplay owners; the scene remains outside build settings.
Rattlebones appears only on external inspection sheets, unchanged and unimported.

Run `Scripts/BuildRecessStage02.ps1` for native LibreSprite authoring/export.
Run Foundry Python with `Scripts/verify_stage02.py` after the explicit Unity test
`DungeonPixelPipelineStage02Tests.Stage02RecessMaskPreview` for independent QA.
Do not rerun earlier stage scene builders after later assembly without checking their scope.

Point, FullRect, 64 PPU, no compression/mipmaps/NPOT resizing. Production Dungeon art,
layout, prefabs and gameplay remain unchanged. Stage 00's production-art scaling finding
remains a later integration concern; this isolated scene is tested at native 1× and 3×.

Confirmed cumulative PixelLab charge: **10/35 equivalents**; account balance **1044.20**.
Next: `PROMPTS/03_GENERATE_FLAT_SIDESCROLLER_FLOOR.txt`. Wait for the user.
