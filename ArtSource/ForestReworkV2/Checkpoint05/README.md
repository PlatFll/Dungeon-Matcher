# Magical Forest v2 - checkpoint 5: assembled Unity preview

The approved top background and portrait, board frame and shared lower HUD now
appear together in a new art-only Unity scene. Rattlebones and three existing
Forest enemies are static readability samples, with existing gems, health bars,
countdowns and shared controls. No production or gameplay binding changes.

## Reused artwork

- Checkpoint 2 enchanted woodland, layered depth and distant lights.
- Checkpoint 3 portrait: mitred timber, connected vines, centered tenons and clipped backing.
- Checkpoint 4 board: four corners, four repeating rails and two ivy overlays.
- Earlier approved ground, near trees, hanging lanterns, current characters and UI.
- Original 64x64 gems/cells and 512x512 board; shared lower HUD and continuous backing.

No new PNGs or editable art documents are created or modified. PixelLab: zero
generations this stage. LibreSprite: exact approved exports reused without edits.
The previous stages' generation and cleanup lineage remains in their own folders.
The full scene retains modular, independent images rather than a flattened asset.
The builder copies only the approved scene hierarchies and remaps the board review
by 432 screen pixels to the established full layout. It never invokes earlier
builders or saves earlier scene paths. Preview text grids are rebound to this scene.

## Executed checks

- Tools/Validate-Unity.ps1 passed, Unity 6000.3.19f1.
- Explicit FullAssemblyReview test passed; process exit 0.
- Actual Unity captures at 720x1360 and 1440x2720; exact nearest-neighbor 2x equality.
- Entire 720x480 top region matches the approved checkpoint 3 render exactly.
- Entire 512x512 board opening matches the approved checkpoint 4 and original renders.
- Every opaque approved board-frame pixel matches checkpoint 4 after placement.
- All shared lower HUD pixels match the original full review. The only difference
  below y=1104 is 476 pixels of the old board ivy beside the energy bar, already
  removed by approved checkpoint 4. Those pixels reveal the exact original backing;
  all other pixels in the lower region match. No HUD drawing or layout changes.
- Portrait mask probe changes only pixels inside its 148x392 screen rectangle.
- Empty cells remain exact. No camera gaps, clipped independent sprites, stretched
  pixels or truncated text. Native Point filtering and integer uniform scale pass.
- All 23456 pre-existing protected files retain their SHA-256 hashes.

This is an art-only Unity Play Mode capture with static samples, not live combat.
Production phone safe-area integration, gameplay behavior, Android and real-device
testing are outside this checkpoint. Checkpoint 6 final validation has not started.

## Review

Open Review.html for full before/after, structure-only, top detail and lower HUD.
Scene: Assets/_Game/Scenes/ArtPreview_ForestReworkV2_Checkpoint05.unity.
Menu: Dungeon Matcher / Art Preview / Forest rework v2 checkpoint 05 full assembly.
The review scene is excluded from build settings. Diagnostics/AssemblySources.json
records all 110 reused image sources and current hashes. REPORT.json and pixel/Unity
evidence distinguish performed checks from untested production behavior.

Stop for visual review. Do not merge.
