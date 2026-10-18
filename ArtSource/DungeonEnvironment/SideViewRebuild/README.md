# Dungeon side-view modular reference scene

Approved native library, merged in PR #201. The isolated preview below remains
available. The subsequent production battle integration is documented separately
in [BattleIntegration](../BattleIntegration/README.md); its review is still pending.

The supplied reference is the composition authority. The screenshot mat is excluded using source rectangle `[178,18,938,1370]`. The 760×1352 artwork is sampled with nearest-neighbor into a **360×640 native coordinate grid**. Final artwork is authored in LibreSprite on that grid; review enlargements use integer nearest-neighbor scaling.

## User-requested clipping and wall correction

The first version's banner mask inverted the pointed tails, and narrow wall/stone crops retained broken prop fragments or cut across natural stone edges. These defects are corrected in the current version:

- Complete pointed tails on all five banners; separately editable decorations remain above the wall.
- One global-phase wall field behind the board, side gutters and lower HUD. Removed the five sampled support fragments, two patched side strips and redundant offset lower-wall field.
- Both lower corner sprites expand from narrow 31/32×105 crops to **68×164** canvases. Natural stone contours replace rectangular slabs, source UI fragments are excluded, and the bottom silhouettes are completed.
- Standalone props and corners have **two transparent pixels of clearance**. Border props move inward by the minimum needed to keep the entire padded sprite in view; native pixels are never scaled.
- UI interiors/cells sit above scenery, and frames/labels retain their existing higher order. Background pieces may extend under those UI surfaces.

No PixelLab generations were spent on this correction. Total generation usage remains 10. Rejected patches are removed from the active library; their sources and the marked feedback remain in `Evidence/WallCorrection/` for review history.

## Contents

- `Editable/`: 119 individual LibreSprite documents, organized by material/role.
- `Exports/`: matching native PNGs with binary alpha.
- `AssetManifest.json`: dimensions, per-module lineage, and every placement/clip rectangle.
- `INVENTORY.md`: complete dimensions, palette counts and repeat/join/standalone classification.
- `References/`: exact supplied reference, nearest preparation, reference crops, original request and lineage.
- `PixelLab_Source/`: the request, receipt and all 16 returned candidates, retained separately from final assets.
- `Review/Index.html`: reference, modular layer toggles, actual Unity capture, and the asset library.
- `Evidence/`: native source/export checks, reference hashes, usage, Unity tests and final validation summary.

## Artistic decisions

The arch, barred door, banners, torches, barrels, chain bends, frames and their anchors follow the supplied image. The upper floor remains a **12px shallow standing strip** over a **19px frontal ledge**. It has not been converted into a receding perspective floor. The board cells and portrait interior render above all environment layers, so extended stone pieces cannot cover them. The player background keeps a cooler blue material, distinct from the plum dungeon wall. Warm light is represented by discrete brick clusters behind separate torch sprites.

The original screenshot contains soft shading and sampling residue. Native cleanup uses material-specific color ramps and connected pixel clusters. Quiet wall fills and board cells were reconstructed; incidental cracks, speckles, duplicated prop fragments and blurred tiny text were removed. The small resonance label is a native 3×5 glyph reconstruction. Wave lettering is a separate PNG; Guide lettering and the gear remain part of their button shells. These are art preview labels, not localized or gameplay-controlled UI text.

This is a visual interpretation on a fixed native grid, **not a pixel-identical copy of the high-resolution screenshot**. The requested 90% art threshold is treated as a visual review criterion; no objective similarity percentage is claimed. See the side-by-side page for the actual result.

## Modularity and joins

Wall, portrait and cell fills repeat. Frame corners and their matching edge spans, platform course A–D pieces, and arch sections are independent native assets. Those reference-specific spans are **matching join pieces**, not arbitrary interchangeable terrain tiles. Standalone props and icons have transparent silhouettes. Local lighting is a separate replaceable layer. Moving props exposes a complete background rather than duplicated prop fragments.

The Unity hierarchy has six ordered groups: continuous base wall, shallow platform, structural masonry/light, props, top UI shells, bottom HUD shells. Integer scale and centered letterboxing preserve reference proportions at taller phone sizes and within safe areas. The test scene has no units, board gems, input/event system, combat, wave or run owners. Decorative frame gems and HUD icons are retained from the reference.

## Unity preview

- Scene: `Assets/_Game/Scenes/ArtPreview_DungeonSideView.unity`
- Prefab: `Assets/_Game/Prefabs/ArtPreview/DungeonSideView.prefab`
- Imported modules: `Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/`
- Editor menu: **Dungeon Matcher → Art Preview → Open Reference Side-View Scene**
- Rebuild menu: **Dungeon Matcher → Art Preview → Build Reference Side-View Scene**

Imports use Point filtering, no mipmaps, uncompressed textures, Full Rect mesh, 64 PPU and no non-power-of-two resize. The scene is not added to the playable build list. The preview's build menu asks Unity to preserve unsaved scene work before switching scenes.

## Reproduce

From the project root, using the installed LibreSprite path recorded in `Build.ps1`:

```powershell
powershell -ExecutionPolicy Bypass -File ArtSource/DungeonEnvironment/SideViewRebuild/Build.ps1
python ArtSource/DungeonEnvironment/SideViewRebuild/ValidateNative.py
powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1
powershell -ExecutionPolicy Bypass -File ArtSource/DungeonEnvironment/SideViewRebuild/ValidateUnity.ps1 -Mode Build
powershell -ExecutionPolicy Bypass -File ArtSource/DungeonEnvironment/SideViewRebuild/ValidateUnity.ps1 -Mode Test
python ArtSource/DungeonEnvironment/SideViewRebuild/ValidateFinal.py
python ArtSource/DungeonEnvironment/SideViewRebuild/BuildReview.py
```

Python verification/review scripts require Pillow. They only inspect native files and derive review images; production pixel edits occur in `BuildNative.js` inside LibreSprite. `Build.ps1` repairs only LibreSprite layer flags via the existing repository helper before native exports. No cel pixels are modified by that metadata repair. Unity must be closed for the batch commands on this checkout.

## Generation usage

**10 PixelLab subscription generations**, one completed job `59bc82c9-7774-4084-b362-2a407edbc3c5`. Balance 1064.20 → 1054.20; no cash credits spent. The service returned 16 actual 64×32 images despite four requested categories and inconsistent size metadata in its retrieval response. Actual PNG dimensions are recorded.

Tile A supplied the staggered wall baseline. Its overly broad mortar and flat values were reconstructed in LibreSprite against the reference. Generated platform alternatives changed the reference's ledge profile, so they were rejected; the final platform follows the supplied silhouette. All unused proposals remain clearly separated in `PixelLab_Source/`. No additional generations were used.

## Validation record

`Evidence/ValidationSummary.json` records the executed gates and capture comparisons. Source/export integrity, rendering/alignment tests and artistic inspection are separate kinds of evidence. No gameplay validation or production art approval is implied by this isolated scene.
