# Dungeon modular environment review kit

This is an additive art preview on `codex/dungeon-modular-scene-rebuild`.
It does not replace the live Dungeon environment or enter the release scene list.

## Open and inspect

Open this branch's Unity project, then choose **Dungeon Matcher → Art Preview →
Open Dungeon Modular Scene**. The scene is
`Assets/_Game/Scenes/ArtPreview_DungeonModular.unity`.
The inspector on **Dungeon Modular Art Preview** exposes the module sprite list,
editable preview labels, viewport width/height, and safe area. After changing a
sprite or dimensions, use **Rebuild art preview from modules** in its component
context menu. The generated hierarchy is saved and can be inspected without
entering Play Mode. Use the Game view at the configured aspect ratio.

The preview deliberately has no player, enemy, gems, input, or gameplay owners.
Wave and Guide are editable typography samples; they do not start a run or open
production UI. Lower HUD recesses are empty structural frames.

## Sources and construction

- `Brief/`: immutable supplied extraction package and branch instructions.
- `Sources/`: byte-identical working copies of numbered editable cell sources,
  renamed to avoid LibreSprite's sequence-import dialog.
- `Editable/<group>/`: independent native LibreSprite documents.
- `Exports/<group>/`: PNG exports from LibreSprite's native batch exporter.
- `AssetManifest.json`: source crop, scope of changes, and reference placement.
- `Review/Assets.html`: source/native comparisons and integer enlargements.
- `Review/Unity/`: actual Unity camera captures, not external scene composites.
- `Evidence/`: exact pixel checks and Unity test results.

Source priority follows the user's brief: approved current production/native
art first. The supplied full screenshot is identical to the approved mockup;
several supplied crop coordinates are offset, so they are not used as native
geometry. Existing native art defines the pixel grid. No screenshot downsampling
or image generation is part of this kit.

Floor variants preserve the source's violet perspective slabs, with explicitly
reconstructed boundary bands. Wall variants retain the blue portrait material
and use scoped violet ramps for outer/battle masonry. Corners and distinctive
native props are retained separately; only straight frame sections repeat.
Newly isolated decorations retain their source pixels within the explicit
selection. The review records distinguish isolation from exact alpha reuse.

The preview reuses `GameplayPixelLayoutController.Calculate`, its rectangle
helper, and `GameUi` typography. It does not attach the live gameplay layout
controller, which depends on run/board owners. Native sprites and corners use
integer physical scale; rails tile rather than stretching. Portrait and board
fills live in separate `RectMask2D` interior containers behind their borders.

## Rebuild and verify

`Build.ps1 -Phase Floor|Kit|All` authors through the installed native LibreSprite
JavaScript API, then normalizes source layer flags and exports through LibreSprite.
It preserves existing sources. The executable path is declared near the top of
the script. Python validation only reads pixels and creates review evidence.

Run `ValidateFloor.py` and `ValidateKit.py` with the Foundry art Python. Build the
Unity scene using the menu above or `-executeMethod DungeonModularPreviewTools.BuildBatch`.
Run EditMode tests filtered to `DungeonModularPreviewTests`. These capture four
portrait/safe-area configurations and render oversized fill probes to prove
portrait/board containment. They also verify native import settings, absence of
gameplay owners, unchanged save files, and unchanged release scene settings.

Run `Tools/Validate-Unity.ps1` after C# changes. See the final validation report
for executed results and remaining issues; this README is not a test receipt.
All art remains a review candidate until the user approves it. Do not merge.
