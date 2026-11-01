# Dungeon floor — full-width assembly correction

Review candidate on PR #222; not merged.

Revision 02 stopped the deep floor at native x=106 and kept the portrait pillars on the old floor line. This left a shallow step beside the portrait. A soft RectMask2D edge also prevented that floor from continuing cleanly behind the portrait.

The same 128×32 native floor now repeats horizontally across the physical screen, including behind the portrait and both safe-area margins. No shallow strip remains beneath it. The separator continues across those margins with exact native-size joins. All three pillar bases and the doorway meet the rear edge; only the final repeated shaft segment is trimmed to fit its complete base. Existing sprites are unchanged.

The Dungeon portrait uses the existing hard-clipping UI shader with flash amount zero, through an independent serialized material. Its mask rectangle, overfill, frame and gameplay layout stay unchanged. Floor tiles need no clipping mask.

Two tiny contact shadows (32×6 and 40×6) were authored and reopened/exported in LibreSprite, using only two existing floor colors and binary alpha. These ground the existing skull, bones and barrel without changing their pixels. Source scripts, ASE files and exact roundtrip PNGs are preserved here. No new PixelLab generations; the original floor generation remains documented in Revision02.

[Actual Unity review](Preview/Review.html)

## Verification

The final Unity integration test and required Tools/Validate-Unity.ps1 compilation both passed. Receipts are in QA. Tests exercise the actual Game scene with Rattlebones, three enemies, 64 gems, an accepted swap and Settings. The five Dungeon layouts are 720×1280, 1080×1920, 1080×2400, 1080×2400 with side/top/bottom safe-area insets, and 360×640. Underwater is also captured at two layouts.

Exact frozen comparisons protect portrait pixels and pixels outside the floor/grounding region. Actor feet, health bars, board and HUD geometry remain identical. A bright test-only backing immediately behind the floor and separator detects any visible gap, even one physical pixel at a tile seam or display edge. It is removed after capture.

No Android build or physical-device test is claimed. User visual approval is still pending; no merge.
