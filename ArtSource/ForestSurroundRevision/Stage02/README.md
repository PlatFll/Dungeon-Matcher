# Forest surround adjustment — Stage 2 lower HUD sides

Review candidate, 2026-10-24. The user's “continue” approves proceeding from the
Stage 1 checkpoint. **Stop here for Stage 2 review. No Stage 3, wisps, animation,
production integration or merge.** Branch: `feature/forest-zone-side-surround-revision`,
draft PR #209; starting commit `0b3da0af`.

## What changed

Two distinct native root-and-stone clusters replace the lowest bough and stone
group on each side. The left has a broad curling woody buttress, two mossy rocks,
red/pale mushrooms and a small flower. The right has an organic crooked trunk,
large stone foot, a prominent red mushroom, smaller pale/brown mushrooms, flowers
and short vines. Exposed wood and rock remain the main forms, with sparse foliage.

The existing board-foot stones and the fifth bough per side cross the Stage 1
boundary, so they stay unchanged. New trunk tips tuck behind these connecting
pieces, avoiding a pasted-on join. The original lowest bough and boulder are only
disabled in the new scene; all source art remains available. New sprites sit behind
the original HUD controls and keep their natural native shape and full canvas.

## PixelLab and native cleanup

Two Pro calls, four 96×128 candidates per side: **20 subscription generations,
$0 credit balance spent**. Balance 770.20 → 750.20. The calls used the supplied
lower-left/right reference crops and verified current Rattlebones for rendering
discipline. A 25-color forest material palette was supplied; selected sources use
19 colors each. Stage 1 usage was 10 generations separately.

Selected Left 0 and Right 2. The other six studies are retained, including two
with artificial pale top caps that were not selected. LibreSprite cleanup removes
154 isolated interior bark speckles, retains the original body silhouettes, and
adds complete tapered/hooked trunk ends where the generated studies ended flat.
Two-pixel transparent clearance protects every final silhouette. No resampling,
gradient, semi-transparent edge or global character/palette changes.

| Asset | Native canvas | Opaque colors |
|---|---|---|
| Lower_Left | 83×124 | 19 |
| Lower_Right | 83×120 | 19 |

## Validation

- `Tools/Validate-Unity.ps1`: passed after final draw-order change, Unity 6000.3.19f1.
- `ForestSurroundStage02Tests.LowerSideRootReview`: passed with actual Unity
  screenshots at 720×1360 and 1440×2720; exact integer pixel doubling.
- Entire image above row 1080 is pixel-identical to approved Stage 1: top HUD,
  portrait, battle environment, board/gems/frame and new board-side roots.
- Potion/bomb controls, resonance panel, energy bar and important text rectangles
  are pixel-identical. No changed pixel lies outside the authorized old/new dressing.
- All 292 original visible image bindings/rectangles and 1,744 protected source
  files remain unchanged. No gameplay, runtime presentation or production scene edits.
- Both actual LibreSprite documents reopen to expected RGBA. Binary transparency,
  preserved source-body silhouettes, padding, integer placement, full screen bounds,
  Point/uncompressed/unmipped imports and nonblocking UI Images pass.
- Shared board 512×512 / cells 64×64; frame 580×580; portrait 98×220 native unchanged.

These are **static Unity art-review captures**, not production combat or device
testing. The scene is excluded from builds. Still-art review precedes any extra
magical accents or animation. The center HUD and background remain intentionally
clear; this stage does not add decoration over the text or controls.

Open `Review.html` for original/revised/mockup comparisons, complete Unity views,
all raw native candidates and cleaned source assets. Editable sources: `Native/`.
Reproduce with `Scripts/PrepareNative.py`, LibreSprite `AuthorNative.js`, then
`ReopenNative.js`; run the Unity test and `Scripts/Verify.py` for evidence.
Unity menu: Dungeon Matcher / Art Preview / Forest surrounds stage 02 lower sides.
