# Magical Forest v2 — checkpoint 2: top background

**Status: Unity art-only blockout ready for visual review. No production integration or merge.**
The user authorized execution after the checkpoint-one direction study. This is the
next review stop, not permission to skip the portrait and board-frame checkpoints.

## Result

The distant woodland now has a clear old stone bridge, a narrow waterfall opening,
and darker layered trees. Its quiet lower third separates the unchanged fighters.
Two small far wisps provide restrained magical accents. The hanging rune plaque
is excluded from this preview; the two existing upper lanterns stay on their tree
supports. No props or extra detail were added to the standing floor.

The existing large trees, foreground canopy, flat ground, character/gem artwork,
portrait and shared controls remain untouched. The lower HUD and all previous
Forest scenes/source modules are unchanged. Only the upper review area is shown;
the board and lower HUD are not rebuilt at this checkpoint.

## Brief asset list

| New separate module | Native size | Opaque colors |
|---|---:|---:|
| Depth_CanopyRepeat | 48×24 | 2 |
| Depth_SideRepeat | 52×160 | 3 |
| Depth_UnderstoryRepeat | 48×16 | 3 |
| Distant_Wisp | 12×16 | 4 |
| Far_EnchantedSanctuary | 256×160 | 11 |

`Far_EnchantedSanctuary` is a distant support layer, not a flattened game screen.
The three depth repeats are scoped palette variants of existing support tiles,
preserving their dimensions, shapes and masks. `Distant_Wisp` has a padded binary
alpha canvas and four opaque colors; it is placed twice above the fighters.

PixelLab: **one source, 9 charged generations**. The actual verified Rattlebones PNG
was supplied as the rendering reference. Request, reference hash, job, receipt and
raw source remain in `Raw/` and `Diagnostics/`. No other generation was requested.

LibreSprite: all five native modules were authored/exported as editable documents.
The far cleanup consolidates 684 pixels of tiny moss/rock/canopy clusters in explicit
regions, groups two bank planes, and adds restrained waterfall strokes. The raw
11-color palette remains 11 colors; no unnecessary global quantization or blur.
Context tiles use explicit material mappings. Native exports match the prepared
pixel plans and reopened LibreSprite documents exactly.

## Actual Unity preview

Scene: `Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint02.unity`.
Editor menu: **Dungeon Matcher / Art Preview / Forest rework v2 checkpoint 02**.
It reads the prior Stage5 upper assembly and adds/replaces only the background
support modules. It never saves the source scene and is excluded from build settings.
The unchanged Rattlebones and three Forest enemies are static readability samples,
with static HP/countdowns. This is not a live combat or phone-build claim.

- `Previews/Unity-Before-720x480.png`: previous background, captured in this same scene.
- `Previews/Unity-After-720x480.png`: revised background with cast and UI.
- `Previews/Unity-After-1440x960.png`: exact nearest-neighbor 2× capture.
- `Previews/Unity-Background-720x480.png`: cast/stat samples hidden for background review.
- `Previews/Unity-MaskProbe-720x480.png`: backing overfill visualization.

## Executed checks

`Tools/Validate-Unity.ps1` **passed** with Unity 6000.3.19f1. The explicit
`ForestReworkV2Checkpoint02Tests.TopBackgroundReview` passed, final process exit 0.
Every visible sprite is Point filtered at uniform integer scale and aligned to
whole pixels. Standalone sprites fit their canvas; repeating masked backing is
tested by visible extent and a separate spill probe. Text remains untruncated.

Independent PNG checks passed: exact 2× rendering; 35,066 unchanged actor/HP/countdown
pixels; 36,236 unchanged opaque frame/control pixels; exact portrait interior and
standing-ground region; zero mask spill and zero exposed camera pixels. Transparent
frame holes correctly show the new forest. The baseline matches the previous full
capture above row 458; its last 22 rows exclude 872 pixels of old board decorations
which intruded into the former top crop. No sprite placement was changed to obtain
that match. All 8,125 protected source/production files match their baseline hashes.

The first test's overfill assertion mistake and one subsequent Unity shutdown anomaly
are retained in diagnostics. The unchanged test rerun passed and exited cleanly.
No production gameplay, Android build, real-device, or adaptive full-screen layout
test was performed for this isolated art blockout.

## Review stop

This checkpoint establishes distant mood, focal depth and readable stage separation.
It does not approve final artwork automatically. The cleaner board direction from
checkpoint 1 remains the target for checkpoint 4; it has not been replaced by the old
decorated board. Stop for user review before checkpoint 3, the portrait frame variant.
Do not merge.
