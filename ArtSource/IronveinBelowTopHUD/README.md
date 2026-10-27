# Ironvein below-top-HUD rebuild — Stage 03 board frame

Branch: `codex/ironvein-below-tophud-pixellab`.
Base: `feature/ironvein-pixellab-staged-rebuild` at `4a790aa0` (PR #214), including the user's three restored older backgrounds. Main and the earlier branch remain unchanged.

## Current result and stop point — Stage 03

Scene: `Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage03.unity`.
Review: `Stage03/Review.html` and `Stage03/Preview/Comparison.png`.

The board now has ten separate native modules: four 27x27 corner plates, four
9px-thick timber rails, a 9x9 riveted connector and a 9x9 amber inset. PixelLab
Pro candidate 1 supplied the socket/crystal and material direction. LibreSprite
cleanup rebuilt the source into the existing frame footprints, consolidated
planes, aligned rails and connectors, and retained the original inner corner
pixels beneath the rounded board-cell edges. Editable ASE files, raw alternatives,
exact exports and reproducible native-edit scripts remain separate.

The existing ResponsiveModularFrameFitter still owns dimensions and centering.
New UI PNGs import at 100 PPU to match the unchanged Canvas reference PPU, giving
their tiled texels the same native scale as corners. Existing 64-PPU assets,
runtime code and shared Canvas settings are untouched. All new pieces render
at uniform 2x source scale in the 720x1280 preview.

Validation: required Tools/Validate-Unity.ps1 passed. Actual art-only Unity
captures at 360x640, 720x1280 and 1080x1920 keep every changed pixel inside the
original rendered board-frame footprint and outside the unchanged board opening.
The top HUD and opaque separator pixels match exactly. All existing Graphic
bounds, 371 protected source files, the portrait, characters, cells/gems, other
modules and shared controls remain unchanged. All ten LibreSprite roundtrips
and Unity PNG copies match, binary alpha is retained, and rail repeat edges match.
Evidence: `Stage03/QA/Validation.json`, `Unity.json`, `Assets.json` and the compact
`SceneDelta.patch`. These are preview/automated checks, not live gameplay or a
phone test; the scene is excluded from builds.

PixelLab: **10 subscription generations** for four alternatives; **236.20 remain**.
No images uploaded. Stage02 + Stage03 cumulative spend: 30 generations.

**Stop for Stage 03 review. Do not start Stage 04 drills or merge.** Draft PR #215
remains based on the current Ironvein parent branch; production Game is unchanged.

## Approved Stage 02 history

The current review scene is `Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage02.unity`. Five new native modules form four component families: repeat timber beam, reinforced center beam, mounted amber diamond, and separate left/right end brackets. Their PNGs and editable LibreSprite ASE sources are in `Stage02/Exports` and `Stage02/Native`.

Only the separator above the board is assembled. The brackets meet the existing corner plates without covering their crystals. Repeat sections use native tiling, and the new components retain uniform 2x source-pixel scale in the 720x1280 scene. No old frame asset, top-HUD asset, cell, gem, character, bottom control or production scene was edited.

Actual Unity captures at 360x640, 720x1280 and 1080x1920 verify that **every pixel outside the separator rectangle remains exact**, and all existing Graphic geometry/asset references are unchanged. Source imports and LibreSprite reopen/export roundtrips also match. The scene remains art-only, excluded from builds; no phone or live combat test is claimed. Final compilation/source evidence: `Stage02/QA/Validation.json`.

PixelLab: **20 subscription generations** for two Pro calls / eight alternatives, no reference-image uploads. First-call candidate 0 supplies the selected design. The second, short-canvas attempt lost the timber and is rejected. Native cleanup condensed redundant wood rows, restored complete socket contours, consolidated material ramps and made repeat seams/brackets consistent without interpolation. See `Stage02/Generation*.json`, `Raw/`, and `QA/Assets.json`.

Review: `Stage02/Review.html`, `Stage02/Preview/Comparison.png`, native components and unannotated Unity renders. Draft PR #215 remains based on PR #214's branch. The user approved continuation to Stage 03. Preserve this source and scene unchanged.

## Stage 01 foundation history

Stage 01 only. The current approved scene is duplicated into `Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage01.unity`. No artwork is generated, removed, recolored, resized or replaced. Original scene: `Assets/_Game/Scenes/ArtPreview_IronveinStaged_Final.unity`.

The working scene has two non-rendering hierarchy markers: `LOCKED_TOP_KeepCurrentArtwork` and `LOWER_ONLY_StageBoundary_NoArtwork`. The latter is an empty rectangular mask for future environment additions, below existing UI. This is a safe art-only scene, excluded from build settings. The image target remains a reference file, never a game texture.

Stage 01 was reviewed and the user requested continuation to Stage 02. The Stage 01 scene, screenshot baseline and source hashes remain immutable.

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
