# Ironvein below-top-HUD rebuild — Stage 01 foundation

Branch: `codex/ironvein-below-tophud-pixellab`.
Base: `feature/ironvein-pixellab-staged-rebuild` at `4a790aa0` (PR #214), including the user's three restored older backgrounds. Main and the earlier branch remain unchanged.

## Current result and stop point

Stage 01 only. The current approved scene is duplicated into `Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage01.unity`. No artwork is generated, removed, recolored, resized or replaced. Original scene: `Assets/_Game/Scenes/ArtPreview_IronveinStaged_Final.unity`.

The working scene has two non-rendering hierarchy markers: `LOCKED_TOP_KeepCurrentArtwork` and `LOWER_ONLY_StageBoundary_NoArtwork`. The latter is an empty rectangular mask for future environment additions, below existing UI. This is a safe art-only scene, excluded from build settings. The image target remains a reference file, never a game texture.

**STOP HERE for user approval of Stage 01. Stage 02 is not started. PixelLab usage: 0. No merge or production integration.**

## Exact boundary

At **720 × 1280**, screen rows **0–483** are locked; the first editable lower-environment row is **484**. At native **360 × 640**, the first editable row is **242**. The boundary is computed from the lowest actual upper-scene Graphic plus a one-native-pixel safety row, not guessed from a percentage of the screen.

At the current camera scale, the editable world rectangle is `(0, 0, 720, 796)`. It begins below the restored stone combat platform and recessed underside, below the portrait frame and enemy indicators, and above the existing board-side caps. The existing playable grid is entirely protected inside the lower region. All lower decorative work must stay outside the grid and clear of functional HUD elements.

For future tall-phone/safe-area assemblies, derive the boundary again from the approved upper visuals after the existing layout mapping. Do not hard-code the 484/242 row values into a gameplay layout or assume a fixed screen percentage. This stage rendered 360 × 640, 720 × 1280 and 1080 × 1920; it does not claim new tall-phone or device validation.

## Locked elements

- Entire approved top: cavern vista, title/WAVE treatment, Guide/settings, portrait frame and dark backing, player and enemy graphics, lanterns, timber, cart/wheel, restored combat platform and underside, HP bars and indicators. Preserve the actual current top; the target mockup's different characters, title and perspective do not replace it.
- Board opening, position, 8 × 8 cells, gem artwork and gameplay. The reference image uses a different grid representation; it does not authorize changing ours.
- Current shared dimensions for board/player frames, actor floor registration, all character art/animations, health and shield displays, and shared controls.
- Bottom HUD center panel, energy track, potion/bomb artwork, labels, buttons, and functionality. Future environmental seating goes behind/around them.
- Production Game, gameplay code, themes, saves, other zones, source assets and previous review history.

## Lower-half asset checklist and planned stages

| Stage | Asset family | Modular deliverables / constraints | Existing sources |
|---|---|---|---|
| 02 | Separator beam / ledge | Center, repeat segment, mounted amber node, end/transition caps; native tiled timber and forged-iron joins | New focused PixelLab pieces below the locked boundary; keep combat platform above |
| 03 | Board frame | Top/bottom/left/right rails, four corner plates, centered joints/braces, amber inset plate | Replace visible staged board-frame placements only after review; retain source files and shared opening dimensions |
| 04 | Integrated drill sides | Housing/mount, short inward nose, crystal connector, backplate; paired left/right assemblies at separated stations | New focused generation; mount to Stage 03 supports, never float beside them or cover playable cells |
| 05 | Lower board surrounds | Rock modules, timber/support stacks, sparse crystal clusters, optional attached lantern brackets and restrained mining props | Current SupportCap/Shaft/Brace/Rune/Base, chains/lamps and restored CaveWall remain until this stage's approved replacement; reuse suitable cart/crate/rock art |
| 06 | Bottom-HUD seating | Timber/metal supports, small anchored amber nodes, corner rock/prop continuation; preserve center controls | Current LowerLeft/LowerRight/Crate remain until reviewed alternatives; reuse if they serve the composition |
| 07 | Assembly and polish | Join/centering checks, material grouping, silhouette cleanup, source-pixel and clipping QA | Combine only approved modules; no full-scene raster replacement |

The index mentions Stage 08 cleanup, but the supplied pack has no separate Stage 08 prompt. Its QA instructions are in `11_QA_AND_APPROVAL.md`; this does not change the Stage 01 stop requirement.

## Generation and composition plan

Use PixelLab text-description generation only when each later stage is authorized. Describe one asset family at a time: dark sturdy timber, quiet forged iron, deliberate rivets, small amber-orange crystal accents. Use the current Dungeon Matcher guide and actual Rattlebones render language to judge edge/shading quality. Do not send the mockup as a shortcut to whole-scene copying. Clean working copies in LibreSprite; retain raw generations, native sources, exports and cost receipts.

The target is a mining structure around the board. Large wood/metal shapes establish structure; accents remain secondary. Drill housings attach to braces. Inward noses end at the board's outside edge, keeping gem faces and touch areas unobstructed. Native dimensions and occupancy are decided against the unchanged board opening before paid generation. Do not enlarge/reposition the board to fit decorative machinery. Existing pixels remain recoverable.

The mockup guides below-boundary composition, not its exact detail density. Keep readable empty space around the board and HUD text. Corners, rails and connectors use the existing responsive modular frame system; no stretched pixels or disconnected plates.

## Stage 01 evidence

- Actual Unity source-versus-copy captures match pixel-for-pixel over the **entire image** at three sizes, including all locked top pixels.
- Existing board bounds: `{'serializedVersion': '2', 'x': 87.45048522949219, 'y': 213.45050048828125, 'width': 545.0989990234375, 'height': 545.0989990234375}`. Portrait bounds: `{'serializedVersion': '2', 'x': 21.0, 'y': 825.0, 'width': 195.0, 'height': 406.0}`. No layout values changed.
- `Stage01/SourceLock.json` records current source and reference hashes; `Stage01/QA/Foundation.json` records measured boundary/geometry and Unity assertions.
- `Stage01/Preview/Boundary.png` labels the exact edit boundary. `Unity-*.png` are unannotated actual Unity renders; `Source-*.png` are the live source-scene comparison captures.
- Required Unity compilation and source preservation are recorded separately in `Stage01/QA/Validation.json` after completion. No phone test or gameplay integration claim.

Run the foundation once using `Scripts/RunStage01.ps1` or the Unity menu **Dungeon Matcher → Art → Ironvein Below Top HUD → Stage 01 Foundation**. It refuses to overwrite an existing working scene. Subsequent stages should edit a new stage scene/copy and keep this baseline intact.
