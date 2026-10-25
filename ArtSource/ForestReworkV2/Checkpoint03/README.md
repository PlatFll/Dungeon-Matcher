# Magical Forest v2 - checkpoint 3: portrait frame

Candidate ready for user visual review. No production integration or merge.
The user's Continue approved checkpoint 2 and authorized only the next review stop.

## Result

The existing thin portrait proportions remain. Quiet square mitred timber joints
replace the four small diamond finials. Native 32-pixel rail repeats use one broad
grain groove and top-left lighting; the right and bottom are shaded independently.
Two small joined leaf/vine silhouettes replace the detached leaf clusters. Foliage
is outside the mask, on its own padded canvases. One centered wooden tenon per
vertical side replaces three repeating studs at this portrait size.

The existing ResponsiveModularFrameFitter still owns corners, tiling and centered
connectors. No runtime frame system was added. The 98x220 native target, 12-pixel
mask inset and 32-pixel backing overfill are unchanged. The new art retains the
existing eight-color combined wood/foliage material subset.

## Assets

| Separate PNG module | Native pixels | Opaque colors |
|---|---:|---:|
| Portrait_Corner_BL | 15 x 15 | 5 |
| Portrait_Corner_BR | 15 x 15 | 4 |
| Portrait_Corner_TL | 15 x 15 | 5 |
| Portrait_Corner_TR | 15 x 15 | 5 |
| Portrait_Join | 7 x 7 | 4 |
| Portrait_Rail_Bottom | 32 x 9 | 4 |
| Portrait_Rail_Left | 9 x 32 | 5 |
| Portrait_Rail_Right | 9 x 32 | 4 |
| Portrait_Rail_Top | 32 x 9 | 5 |
| Portrait_Vine_Lower | 25 x 27 | 4 |
| Portrait_Vine_Upper | 30 x 24 | 4 |

Eleven matching editable LibreSprite documents are in Native/. Scripts prepare
explicit pixel plans; LibreSprite authors, saves and exports those plans, then
reopens the documents. Every reopened pixel matches its exported PNG. All eight
corner/rail connection ports and each rail's repeat endpoints match exactly.
Standalone foliage has padding and binary alpha; no filtered/resampled art.
NativeFrameStudy is a display-only assembly study, never a runtime sprite.

PixelLab: zero new calls or spending. The prior Stage02 source has verified
PixelLab/Rattlebones lineage; its existing palette and the checkpoint-one direction
study supply the material basis. New generations add no value to precise frame
joints. LibreSprite handles all eleven final native pieces.

## Preserved and validated

All 23305 existing protected files match their starting hashes, including the
approved checkpoint-2 background, characters, gems, all prior scene/source files,
shared controls and lower HUD. Only a new portrait preview scene/editor fixture
and the new review assets/evidence are added.

Tools/Validate-Unity.ps1 passed using Unity 6000.3.19f1. The explicit
ForestReworkV2Checkpoint03Tests.PortraitReview test passed. Actual Unity captures
at 720x480 and 1440x960 have exact nearest-neighbor 2x equality. Whole-pixel origins,
uniform scale, full standalone sprite extents and untruncated labels pass.

Frame shell cases: 30x30, 74x180, 98x220, 129x257 and 163x287 retain native corners,
centered rails/connectors and the mask/overfill relationship. The undersized 29x29
shell hides safely. This is a shell geometry check, not content reflow validation
at an unusably small size.

Before capture matches the approved checkpoint-2 capture exactly. Only frame and
foliage source masks contain changed pixels. Portrait contents and the whole enemy
side are pixel-identical. Removing the old upper ivy reveals 48 screen pixels of
the exact existing timber backing inside the opening; every other interior pixel
is unchanged. The initial whole-interior comparison flagged this intended removal;
the final check verifies its exact footprint and the exposed source tile pixels.
A magenta overfill probe changes zero pixels outside the
portrait mask. No camera gaps are exposed.

This is an art-only Unity preview with static Rattlebones and three Forest enemies,
not live combat, production safe-area mapping, Android or real-device validation.
Scene: Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint03.unity.
Menu: Dungeon Matcher / Art Preview / Forest rework v2 checkpoint 03.
The scene is excluded from build settings. Existing source scenes are never saved.

## Review and next step

Review.html contains actual Unity before/after captures, a portrait closeup and each
module at 1x/4x. Artistic verdict: calmer square joinery reads as a built timber
frame; two connected foliage accents keep the forest identity without extra props.
Character/gem contrast remains dominant. User approval is still required.

STOP here. The board-frame variant is the next checkpoint; no board or lower HUD
work belongs in this pass. No merge.
