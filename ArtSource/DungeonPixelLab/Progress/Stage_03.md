# Stage 03 — DONE; stopped for user review

2026-10-19. Flat side-view floor **top only**, following the user's Stage 02 approval. Draft PR #203. No front-fascia work, production integration or merge. Stage 04 has not started.

## Delivered

- One new PixelLab **sidescroller** set: sixteen 32×32 tiles on an immutable 128×128 source sheet, with original tile metadata.
- Five native LibreSprite surface pieces: `Floor_Left`, `Floor_Mid_A`, `Floor_Mid_B`, `Floor_Mid_C` at **32×12**, and an independently finished **30×12** `Floor_Right` end cap. The narrower right cap fits the actual viewport; no 32-pixel sprite is clipped or squeezed.
- One **254×12** horizontal strip, eight modules from native x106 to x360 at y178. The frame draws above its left edge. No receding tile rows, slopes, cobblestone field or vertical front fascia.
- Actual Unity 1×/3× captures, a temporary floor-contact-marker preview, raw-versus-cleaned native/4× contact sheets, and all sixteen legal neighbor-pair examples.

## Authority, generation and native cleanup

Current art guide v1.17.2, golden register, actual Rattlebones/Farmer/Pan images and applicable UI pixels were rechecked against current hashes. The approved Stage 01 wall and Stage 02 recess are preserved.

The official `create_sidescroller_tileset` endpoint supports native 16/32 tiles and flat platforms. Its inspected cost range was 2–3 generation equivalents, within the remaining pack budget. It has no style-image input: actual goldens and accepted wall were inspected locally, while the request used explicit material, palette, shading and side-view descriptions. No claim is made that images were uploaded to this endpoint. Official documentation consulted: https://api.pixellab.ai/mcp/docs and https://api.pixellab.ai/v2/llms.txt.

Tileset **fd7b1bf8-270d-447c-a135-177884748b06**, base tile **74904d1b-9197-4c89-afd7-c88fd563734b**, seed **310301**. The source metadata identifies `tileset_type: sidescroller`. Its shared `wang_*` tile names do not imply a top-down request; no top-down or isometric tool was called.

LibreSprite takes the native capstone top cluster at **x96–127, y16–22**, excluding the generated deeper support rows. Connected source planes are joined into wider stones by filling the old narrow joints, with no scaling. Source violet-gray shades are assigned five deliberate material roles: back seam `#241F3A`, edge shadow `#382D55`, subdued bevel/joint `#4E3D6C`, broad surface `#67537D`, and restrained light `#846A95`. The three middle variants shift shallow slab joints and alter only a connected bevel or tiny chip. Shared module profiles stay identical; no deep crack repeats at every join.

The 12-pixel strip includes a shadowed back seam, shallow walking plane and thin finishing bevel. It does not contain the deeper vertical support/fascia intended for Stage 04. Left/right ends are explicitly finished at their native dimensions. Transparent corner pixels remain binary. Raw output is untouched under `Sources/Stage03/`; all edits run through LibreSprite's native pixel/cel API and separate PNG exporter. Background-layer flags are normalized using the existing project helper without changing pixel bytes.

## Validation actually performed

- Five native LibreSprite cels equal independent PNG exports exactly; candidate import PNGs are byte-identical to those exports.
- Five meaningful opaque colors per piece. Middle pieces are opaque; end-cap corners use alpha 0/255 only. All pieces have a continuous opaque contact row.
- **16/16 legal joins** have identical edge profiles: all nine ordered middle/middle pairs, three left/middle pairs, three middle/right pairs and left/right. Three middle outputs are distinct.
- The eight placed modules exactly cover the 254×12 rectangle, with zero overlapping sprite rectangles, gaps or clipped sprite bounds. All prior Stage 02 placements are unchanged.
- Unity explicit test `DungeonPixelPipelineStage03Tests.Stage03FloorNativePreview`: **1 passed, 0 failed**, Unity 6000.3.19f1. It checks native sizes, integer positions, 64 PPU, white tint, one consistent horizontal strip, and absence of gameplay/input owners.
- Native contact markers at x154/234/314 all meet **y180**. They are temporary geometric inspection aids; no actor logic is instantiated. They are removed before the restored screenshot and absent from the saved scene.
- Independent expected assembly vs actual Unity normal, marker and restored captures: **zero differing RGBA pixels at both 360×640 and 1080×1920**. Initial marker tint rounded one red-channel value in Unity; the diagnostic was changed to exact endpoint cyan, then the affected test and validator were rerun. Artwork did not change during that correction.
- Point, FullRect, 64 PPU, no compression, mipmaps or NPOT resizing. `Tools/Validate-Unity.ps1`: **SUCCESS**, exit 0, after the final editor test change. Exact tested hashes and log receipt are in `Diagnostics/Stage03/ValidationReceipt.json`.
- **82 approved/prior source, native, export and frame/import files** retain their initial hashes. Goldens unchanged. Production Game scene and build settings unchanged. The user's eight main-checkout edits retain their starting hashes. Three incidental whitespace-only consumable metadata rewrites are restored only in the initially clean art worktree.

## Artistic verdict and limits

**Ready for user review.** One level, shallow violet-gray surface reads clearly against the cooler wall. It keeps broad stone planes, thin clean joints, restrained highlights and sparse connected wear. The native joins are coherent and the contact height is stable. No perspective road, cobblestone noise or broken slab seam was observed.

The surface intentionally has no deep vertical support yet, so the isolated preview is not a finished raised stage. Stage 04 supplies that family after authorization. These are real isolated Unity art captures, not production battle or phone-build verification. The existing production layout/scaling concerns recorded in Stage 00 remain outside this task.

## Spend and stop

One paid sidescroller call, no retry. Account balance **1044.20 → 1041.20**, a measured **3 generation-equivalent** charge. Cumulative pack **13/35**; 22 remain before the soft-budget boundary. Request, results and balance evidence are retained under `Progress/Stage03_PixelLab_*.json`.

Stage 03 is DONE; art remains pending user review. **STOP before `PROMPTS/04_GENERATE_FRONT_PLATFORM_FASCIA.txt`.**
