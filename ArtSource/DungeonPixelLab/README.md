# Dungeon PixelLab staged asset library

Stage 00 only. New original environment art starts only after the next explicit prompt. No earlier dungeon module is accepted as new art by this reset.

- `Brief/`: immutable extracted user pack, including later prompts that have NOT been executed.
- `Sources/`: future unchanged PixelLab responses/images (empty in Stage 00).
- `Native/`: editable LibreSprite cels. Stage 00 contains only a diagnostic probe.
- `Exports/`: native PNG exports. No environment source has been generated yet.
- `Previews/Stage00/`: references at 1x/nearest-neighbor 4x, real Unity baseline and empty-scaffold/test captures.
- `Diagnostics/`: source/import/screen measurements; Unity evidence. Historical production assets appear ONLY as diagnostics.
- `Progress/`: status, asset manifest, zero-spend ledger, service evidence and numbered handoff.
- `Scripts/`: repeatable local QA; no generation endpoint is called by any script here.

Isolated Unity preview: `Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity`. It reuses only the existing `DungeonSideViewArtPreview` camera/canvas/layer infrastructure with an EMPTY module list and EMPTY placement list. It is not in build settings. The diagnostic probe and old wall sample are injected at test runtime and are NOT saved into the scene. No actors, gems or gameplay owners exist in this scaffold.

Production candidate namespace: `Assets/_Game/Art/DungeonPixelLab/`. Point, FullRect, 64 PPU, no mipmaps, no compression, no NPOT resizing. A larger canvas adds native pixels/clearance; it never justifies changing source pixel pitch.

Branch: `feature/dungeon-zone-pixellab-asset-library`, based on main `1ee1fdb02e409c9d6b17785d84a76d36bab6342c`. The previously attached, clean Unity worktree was reused to preserve its library/cache. The main checkout and its eight local changes are separate and preserved.

Review `Progress/Stage_00.md`, then wait for `PROMPTS/01_GENERATE_DUNGEON_WALL_TILES.txt`. No merge or integration is authorized by this checkpoint.
