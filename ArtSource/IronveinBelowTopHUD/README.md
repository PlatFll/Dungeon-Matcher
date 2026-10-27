# Ironvein below-top-HUD rebuild — Stage 06 bottom-HUD seating

Branch: `codex/ironvein-below-tophud-pixellab`.
Base: `feature/ironvein-pixellab-staged-rebuild` at `4a790aa0` (PR #214), including the user's three restored older backgrounds. Main and the earlier branch remain unchanged.

## Current result and stop point — Stage 06

Scene: `Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage06.unity`.
Review: `Stage06/Review.html`, `Stage06/Preview/Comparison.png` and `HUDDetails.png`.

A shallow timber cradle and two iron-capped wooden corbels seat the unchanged
potion, resonance and bomb controls inside the mine. Four small warm rivets
connect the support rails to the outer posts. New art renders behind the
existing controls and lower props. The energy bar and both instruction lines
remain clear; no controls were moved, replaced or disabled.

Five separate native assets: paired corbels (32x32), rail (24x7), post (7x24),
and iron/amber rivet (5x5). One PixelLab Pro call produced 64 bracket candidates;
candidate 0 was selected for its simple connected shapes. LibreSprite preserved
the silhouette, produced the mirrored partner, adapted an unchanged copy of
Stage02 timber into shallow supports, and repaired exact repeat seams.
The set uses 11 opaque colors, binary alpha, and no resampling. Editable ASEs,
native exports, exact reopen roundtrips, source alternatives and scripts remain.

Validation: `Tools/Validate-Unity.ps1` passed on Unity 6000.3.19f1. Actual art-only
Unity renders at 360x640, 720x1280 and 1080x1920 preserve the entire top and board,
energy bar and instruction pixels. Existing opaque UI, frames, drills, separator,
side dressing and lower props match. All existing Graphic geometry, references
and active states remain exact. 761 protected files retain their hashes; five
Unity PNG copies and LibreSprite roundtrips match. The new scene adds 10 Images
in one group, with only one Canvas child link changed. It is excluded from builds.
These are art-only preview checks, not live gameplay or real-device testing.

PixelLab: **10 subscription generations** spent; **192.20 remain**. Cumulative
pack usage: **74**. No reference-image uploads and no pending generation jobs.

**Stop for Stage 06 visual approval. Do not start Stage 07 or merge.**
Draft PR #215 remains on the dedicated branch, based on PR #214's Ironvein branch.
Production Game, gameplay systems, approved art, characters/gems, shared frame
dimensions and all other zones remain unchanged.

## Approved Stage 05 history

Scene: `Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage05.unity`.
Review: `Stage05/Review.html` and `Stage05/Preview/Comparison.png`.

The board-side purple support placements are replaced by brown timber posts,
iron collars and darker irregular mine rock. The approved drills now sit over
timber supports. Two restrained amber deposits anchor the lower sides, and a
quiet native rock course continues beneath the board. Existing lanterns,
chains, lower-corner props, crate, general background and functional HUD remain.
Bottom-HUD seating is Stage 06 and has not started.

Nine separate native modules: timber cap/repeat/foot (20x18, 20x24, 20x15), iron
collar (22x8), two rock outcrops (22x46, 22x48), a rock footing (28x17), under-board
rock course (64x16), and amber deposit (24x30). PixelLab timber candidate 0 and
rock candidates 5/9 were selected from two 25-candidate calls; two focused
Pixflux calls supplied the crystal and rock course. LibreSprite cleanup repaired
repeat edges, simplified the noisy course into broad native planes, consolidated
material colors, and tightened collar clearance without scaling. Raw alternatives,
ASE sources, PNGs, exact roundtrips and scripts remain separately editable.
The complete set uses 17 meaningful opaque colors and binary alpha.

Validation: `Tools/Validate-Unity.ps1` passed on Unity 6000.3.19f1. Actual art-only
Unity renders at 360x640, 720x1280 and 1080x1920 preserve the top, playable grid,
HUD and every opaque frame/drill/separator pixel. Existing geometry and asset
references are exact; 614 protected source files retain their hashes. The new
scene only disables 14 old support placements and adds one group of 31 Images.
No existing assets or prior scenes are deleted or overwritten. Nine Unity PNG
copies and LibreSprite roundtrips match, and both repeat seams match. See
`Stage05/QA/Validation.json`, `Unity.json`, `Assets.json` and `SceneDelta.patch`.
These are automated/visual preview checks, not live gameplay or device tests;
the scene remains excluded from builds.

PixelLab: **22 subscription generations** spent (two Pro calls at 10, two
Pixflux calls at 1); **202.20 remain**. Cumulative pack usage: **64**. No reference
images uploaded; no pending generation jobs.

The user approved continuation to Stage 06. Preserve the Stage05 source and scene unchanged.
Draft PR #215 stays on the dedicated branch, based on PR #214's Ironvein branch.
Production Game, gameplay systems, approved top, shared frame dimensions,
characters/gems and all other zones remain untouched.

## Approved Stage 04 history

Scene: `Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage04.unity`.
Review: `Stage04/Review.html` and `Stage04/Preview/Comparison.png`.

Six inward-facing drill mechanisms attach to the board rails at three paired
stations. Seven separate native assets form the assemblies: left/right mounting
plates (38x36), motors (18x27), drill noses (16x17), and one amber power block
(9x9). The bolts and mounting straps contact the existing rail centerlines;
the drill tips remain outside the playable opening. No frame source or layout
value is changed. Existing side stones, lamps and chains remain intact for
their separate Stage 05 review.

PixelLab Pro candidate 17 supplied the selected side-profile motor and spiral
bit. Two earlier Pixflux results were rejected for unclear profile. LibreSprite
native editing removed the raw wraparound fragment, completed the clipped tip
on a separate canvas, cleaned material planes, and built separate mounting
plates and the amber inset. Paired silhouettes match; shading keeps upper-left
light. All 66 raw alternatives, 7 editable ASE files, exports, roundtrips and
reproducible scripts are retained. The finished set uses 13 opaque colors total
and binary alpha. Nothing was resized or blurred.

Validation: `Tools/Validate-Unity.ps1` passed on Unity 6000.3.19f1. Actual art-only
Unity captures at 360x640, 720x1280 and 1080x1920 verify that every changed pixel
lies in a drill station and outside the grid. Top, separator and lower HUD match
exactly. All existing Graphic geometry and sprite references are unchanged;
471 protected source files retain their hashes. The new scene only adds one
Canvas child group and its 24 Images. All seven Unity PNG copies and LibreSprite
roundtrips match. See `Stage04/QA/Validation.json`, `Unity.json`, `Assets.json`
and `SceneDelta.patch`. This is an art-only preview, excluded from builds;
live gameplay and real-device behavior were not tested.

PixelLab: **12 subscription generations** spent (two Pixflux calls at 1 each,
one Pro call at 10); **224.20 remaining**. Total for Stages 02–04: **42**.
No image uploads and no generation jobs pending.

The user approved continuation to Stage 05. Preserve this source and scene unchanged.
Draft PR #215 remains based on the current Ironvein parent branch. Production
Game, runtime presentation, characters, shared controls and other zones remain
unchanged.

## Approved Stage 03 history

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

The user approved continuation to Stage 04. Preserve this source and scene unchanged. Draft PR #215
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
