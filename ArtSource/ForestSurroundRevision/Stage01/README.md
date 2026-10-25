# Forest surround revision — Stage 1 board-side roots

Review candidate, 2026-10-24. **Stop for user approval. No Stage 2–4, animation, production integration or merge.**

The current forest implementation is the starting authority: commit `783e86bf`
from `feature/forest-zone-staged-rebuild` (draft PR #208). The new branch is
`feature/forest-zone-side-surround-revision`. The pack's current reference is
pixel-identical to that implementation's Unity capture. Original assets/scenes
remain untouched; this variant is a separate art-only scene, excluded from builds.

## Stage 1 change

Replaced the first four bushy bough placements on each board side with four
placements of three new PixelLab root sprites. Broad curling wooden forms,
visible bark planes and open hollows now provide the structure. Existing frame
ivy supplies much of the decorative greenery; new leaves are deliberately sparse.
The pre-existing board-foot stones, lower boughs and lower corner stones remain.
Root overlap and draw order keep existing frame and grid pixels intact.

This stage changes only the board-side environmental dressing. The top HUD,
portrait, upper forest/battle area, WAVE/GUIDE/settings, board/gems, shared sizes,
bottom HUD and all lower surroundings remain unchanged. Lower-area improvements,
additional mushrooms/flowers, wisps and animation belong to later approval gates.

## Assets and PixelLab usage

One Pro call, four 64×144 candidates, **10 subscription generations; $0 credit
balance spent**. Balance: 780.20 → 770.20. Actual reference inputs were the two
board-side crops of the supplied target, with the current verified Rattlebones
PNG for outline/detail/shading. No upper environment was regenerated.

Selected Root 0/2/3. Root 1 was rejected for its busy junctions and clipped lateral
offshoot. LibreSprite explicitly merged near-duplicate browns into coherent bark
ramps and saved native editable sources. The raw vertical tips touched the canvas:
small native tapered cap extensions repair these, then two-pixel transparent
padding protects the complete silhouettes. No resizing or resampling of drawing
pixels. Source bodies retain their geometry; every tip repair is recorded.

| Asset | Native canvas | Opaque colors |
|---|---|---|
| Root_A | 60×155 | 10 |
| Root_B | 59×157 | 9 |
| Root_C | 59×156 | 10 |

## Verification

- `Tools/Validate-Unity.ps1`: passed using Unity 6000.3.19f1.
- `ForestSurroundStage01Tests.BoardSideRootReview`: passed; actual Unity
  captures at 720×1360 and 1440×2720; the larger image is exact pixel doubling.
- Entire top 480 rows, board contents, every opaque approved frame pixel, and
  the entire region below row 1080 are pixel-identical to the original Unity view.
- All 296 original image bindings/rectangles and 1,680 guarded source files
  remain exact. Only eight explicitly selected old board-bough instances are
  disabled in the new scene. No changes outside old/new dressing masks.
- Shared board 512×512 / cells 64×64; frame target 580×580; portrait 98×220
  native. Point, uncompressed, unmipped imports, integer placement, complete
  screen bounds and nonblocking UI Images. No new clipping masks.
- All three real LibreSprite documents reopen to exact expected RGBA; binary
  alpha, complete padded silhouettes and original source-body masks pass.

These are **static Unity art-review captures**, not production gameplay or
real-device tests. The current lower sides remain visibly leafy because their
revision is deferred to Stage 2. Visual acceptance remains the user's decision.

Open `Review.html`. It includes original/revised Unity, target crop, full-screen
views, raw native PixelLab studies and cleaned native sources. Reproduce editing
with `Scripts/PrepareNative.py`, then `AuthorNative.js` in LibreSprite; reopen with
`ReopenNative.js`. Unity menu: Dungeon Matcher / Art Preview / Forest surrounds
stage 01 roots. `Scripts/Verify.py` checks local native/run evidence and protected
source hashes. Its validation JSON is supplemented with the actual compile result.
