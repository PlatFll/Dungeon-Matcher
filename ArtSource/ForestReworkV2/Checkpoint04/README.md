# Magical Forest v2 - checkpoint 4: board frame

Status: review candidate. The user authorized Continue after the portrait review.
Stop at this board-frame checkpoint. No production integration or merge.

## Result and asset list

The board uses sturdy square mitred timber with sparse connected grain. Spiral
corner plates, repeated connector studs, extra side posts/feet/braces, board lamps,
rune tags and mushrooms are absent from the revised assembly. Two broad connected
leaf/vine accents provide the forest identity. They remain separate sprites with
padded canvases and do not overlap a cell. They can later support gentle ambient
motion; no motion is authored or enabled at this checkpoint.

| Module | Native pixels | Opaque colors |
|---|---:|---:|
| Board_Corner_BL | 23 x 23 | 5 |
| Board_Corner_BR | 23 x 23 | 4 |
| Board_Corner_TL | 23 x 23 | 5 |
| Board_Corner_TR | 23 x 23 | 5 |
| Board_Foliage_LR | 64 x 64 | 4 |
| Board_Foliage_UL | 64 x 64 | 4 |
| Board_Rail_Bottom | 32 x 11 | 4 |
| Board_Rail_Left | 11 x 32 | 5 |
| Board_Rail_Right | 11 x 32 | 4 |
| Board_Rail_Top | 32 x 11 | 5 |

The 304x304 native frame target at 2x retains its prior 608x608 screen footprint,
surrounding the unchanged 512x512 board at a 24-native-pixel inset. Gems and cells
remain their original 64x64 drawings. No resampling, resizing or redraw of board
content occurs. The existing ResponsiveModularFrameFitter owns native corners and
tiled rails. No new runtime frame or layout system is introduced.

PixelLab contributes one 64x64 foliage source, charged 5 generations. Its actual
style image is the verified Rattlebones PNG, for rendering discipline only. The
existing muted woodland palette is supplied independently. Raw prompt, seed,
reference hashes, job ID, receipts and source remain in Raw/ and Diagnostics/.
No full screen or new frame structure is generated. No further calls are made.

LibreSprite writes all ten native modules from explicit pixel plans and reopens
the editable sources for exact export comparison. The rails and joints are direct
native drawings in the portrait's material family. Foliage cleanup is local and
its lower-right shading is authored independently; see Diagnostics/Cleanup.json.
All eight corner/rail ports and repeated rail endpoints match exactly. Standalone
leaves retain transparent padding, binary alpha and zero transparent RGB.

## Unity review and validation

The isolated board scene clones only the previous board contents/frame/props for
comparison. The 64 cells and gems retain their relative positions and exact scale.
The before/after view has a shared unchanged timber backing and a larger review
canvas so both foliage contours fit. Approved background/portrait and lower-HUD
scenes are not rebuilt here. Full-screen integration remains checkpoint 5.

Actual Unity captures: 720x704 and 1440x1408; exact nearest-neighbor 2x equality.
Compilation: Tools/Validate-Unity.ps1 passed, Unity 6000.3.19f1.
Explicit test: ForestReworkV2Checkpoint04Tests.BoardReview passed, process exit 0.
Five shell cases include odd dimensions and non-square extents; native corners,
rail centering and tile pitch pass. A 45x45 undersized shell hides rather than
distorting its parts. These are shell checks, not production phone safe-area tests.

Every pixel in the 512x512 opening matches the previous Unity capture and an
independent composition of the original cell/gem PNGs. Frame-on versus frame-off
captures and a separate alpha/placement test show zero board intrusion. Empty
cells remain exact. Full independent-sprite extents, integer origins, uniform
scale, Point filtering and zero camera gaps pass.

All 23380 pre-existing protected files retain their hashes. Previous candidate
assets remain available. Production scene, mechanics, characters, gems, controls,
approved background/portrait and shared lower HUD are unchanged. No live-combat,
Android or real-device validation is claimed.

Scene: Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint04.unity.
Menu: Dungeon Matcher / Art Preview / Forest rework v2 checkpoint 04.
The scene is excluded from build settings. Review.html shows actual Unity
before/after, empty cells and native modules. New artwork remains a candidate for
user visual approval. Checkpoint 5 is next; do not merge.
