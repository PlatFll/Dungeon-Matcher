> **Rejected by the user:** insufficient branch/leaf mass and rocks too small. See [Revision 02](Revision02/README.md) for the replacement candidate. Original evidence below is retained as history.

# Enchanted Forest Stage 5 — side life dressing

Review candidate, 2026-10-24. Stages 1–4 are approved. Stop before Stage 6;
no merge or production integration is authorized by this stage.

## Result

Six native reusable sprites in eight placements add rooted side growth, leaf
offshoots, tiny mossy stones and warm woodland mushrooms. All dressing renders
behind the approved board-frame foliage. Continuous lower roots connect the frame
corners to the mushrooms at their feet; side branches enrich the middle margins.
The center remains quiet and all gameplay/UI space remains clear.

| Native module | Canvas |
|---|---|
| Root_Stone_Left / Right | 48×148 each |
| Leaf_Sprig_Left | 32×34 |
| Leaf_Sprig_Right | 33×35 |
| Mushroom_Pair | 50×45 |
| Mushroom_Tall | 40×43 |

PixelLab Pro supplied two source families for 20 generations total: four large
prop studies and sixteen narrow 48×72 candidates. Narrow 00 and 13 supply the
roots/foliage; complete mushroom silhouettes are extracted from large study 00.
Actual Rattlebones and the supplied forest reference were supported image inputs.
The large full rock/root studies were too wide for these locked margins, so they
were not squeezed into the scene. All raw images, requests, receipts and hashes
are retained. No approved sprite was regenerated or recolored.

LibreSprite applies the explicit native pixel plans, saves editable .aseprite
sources and exports transparent PNGs. Cleanup merges near-duplicate colors into
the approved wood/leaf material family, controls mushroom highlights, separates
complete cutouts, removes extraction residue and pads all silhouettes. Native
joining curves extend the two lower roots without stretching their generated
crowns or stone/foot groups. Each source reopens with identical RGBA. No blur,
resampling, partial-alpha contours or destructive edits to approved assets.

## Verification

- Tools/Validate-Unity.ps1 passed with Unity 6000.3.19f1.
- EnchantedForestStage05Tests.SideDressingReview passed. Actual art-scene captures
  are 720×1360 and 1440×2720; the larger image is an exact pixel doubling.
- Hiding the dressing restores the approved Stage 4 screenshot byte-for-byte.
  The approved top, all board tiles/gems, every opaque frame/ivy pixel and the
  lower HUD are unchanged. No pixel changes occur outside the new dressing mask.
- All 280 prior image bindings and their rectangles are preserved. Board remains
  512×512 with 64×64 cells; frame target 580×580; portrait 98×220 native.
- Six padded native sources pass binary-alpha, dimensions and LibreSprite
  reopen checks. Eight placements stay fully inside the view, at integer scale
  and whole-pixel positions, with Point filtering and no UI raycasts.
- 596 protected art/presentation source files are byte-identical. Production
  scenes, shared layout, gameplay, other zones and lower HUD were not edited.

Scene: Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage05.unity, excluded from
player builds. Static Rattlebones and enemies provide scale/readability context.
This is Unity art-review evidence, not live combat, Android or device testing.

## Review and reproduction

Open Review.html for full Unity before/after, side detail, native pieces and raw
studies. Native/ contains all six editable documents. Scripts/PrepareNative.py
prepares local canvases and exact pixel plans; AuthorNative.js runs in LibreSprite,
and ReopenNative.js checks persistence. Scripts/VerifyStage05.py verifies native
pixels and captures. Its protected-file audit uses the local ignored starting
snapshot; reproduction on another checkout should create a fresh baseline first.
The Unity editor menu Dungeon Matcher / Art Preview / Enchanted Forest staged 05
rebuilds the separate review scene. No reproduction script invokes generation.
