# Dungeon PixelLab staged asset library

Stages 00 and 01 complete. **Stopped before Stage 02; artwork awaits user review.**
Draft PR: https://github.com/PlatFll/Dungeon-Matcher/pull/203. No merge or production integration.

- `Brief/`: immutable user pack; only prompts 00/01 executed.
- `Sources/Stage01/`: unchanged 64x64 PixelLab wall source.
- `Native/Stage01/`: fourteen editable LibreSprite documents.
- `Exports/Stage01/`: four native 48x48 variants and ten exact edge/corner closures.
- `Previews/Stage01/`: native/4x comparison, 6x4 repeat field and real Unity captures.
- `Progress/`: numbered reports, current state, provenance, asset manifest and actual spending.
- `Diagnostics/Stage01/`: exact PNG/cel/seam/render checks and Unity test/validator receipts.
- `Scripts/`: reproducible native LibreSprite build and independent QA; no generation calls.

Open `Review.html` for Stage 01; `Review_Stage00.html` retains setup diagnostics.
Run `Scripts/BuildWallStage01.ps1` using the installed LibreSprite for native art authoring/export.
Run Foundry Python with `Scripts/verify_stage01.py` for independent native/screen checks.
The explicit Unity test `DungeonPixelPipelineStage01Tests.Stage01WallNativePreview` imports
only this wall family and rebuilds `Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity`.
Do not rerun an earlier stage's scene builder after later assembly without checking its scope.

The isolated scene is wall-only, outside build settings, with no gameplay owners, actors or gems.
Candidate textures use Point, FullRect, 64 PPU, no compression/mipmaps/NPOT resizing.
Production Dungeon art, layout, prefabs and gameplay remain unchanged.
Stage 00's noninteger production-art scaling finding remains unresolved for later integration.

Confirmed cumulative PixelLab charge: 5/35 equivalents; balance 1049.20. No retry.
Next prompt: `PROMPTS/02_GENERATE_SEPARATE_PORTRAIT_RECESS.txt`. Wait for the user.
