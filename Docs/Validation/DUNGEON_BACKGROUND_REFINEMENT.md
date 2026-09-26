# Background refinement — 2026-09-22

Scope: top battleground and surrounding game background only. Based on the three
supplied mockup crops and the existing Dungeon Matcher art guide. No character,
animation, gem, UI frame, menu, powerup, VFX or audio asset was edited.

## Result

Rebuilt cool violet stone, floor slabs and a ledge; added a recessed gate, shrine,
muted banners, torchlight, crates, candles, skulls and chains as reusable native
modules. A four-shade dark masonry repeat fills the existing gutters and panel
backgrounds. Sources are in [ArtSource/Backgrounds](../../ArtSource/Backgrounds/README.md).

The existing dungeon prefab and background resource are the integration points.
Runtime scene layout, gameplay and background controllers are unchanged. The scoped
editor importer retains the existing Grid, sorting and mask, replaces tile content,
and clears the old dressing maps to prevent duplicated props. No scene file changed.

## Executed checks

- `Tools/Validate-Unity.ps1` completed successfully with Unity **6000.3.19f1** after
  the final editor-code change. Local log:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-2d8f93ca-1fb8-44cb-a314-7b1f4a37a7cc.log`.
- Graphics-enabled Unity batch editor ran
  `DungeonBackgroundValidation.ImportAndRun` against the actual **Game** scene at
  **1080×1920** and **1080×2400**, using disposable account data and temporary
  character selection. Both cases passed the production layout validator, viewport
  coverage, tile membership, mask, sort order, background panel placement and
  native texture checks. The resumed-run guide was closed via its existing API
  before capture. Local job log: `.utmp/Backgrounds/unity-background.log`.
- `verify_backgrounds.py` passed: **23** exact native/PNG pairs, binary alpha,
  assembled wall/floor/foundation identity, exact repeating surround, matching
  floor side joins, and all **four** Unity texture copies identical to their exports.
- Reviewed the serialized prefab changes and scoped asset diff. Original object
  identities and renderer configuration are preserved. Other tracked art files
  are absent from this pass's diff.

## Visual review and corrections

Inspected the native scene and both final Unity captures. The first shorter-screen
capture hid most banners/flames behind the existing HUD; these accents were moved
into the lower visible wall band. A crate/torch overlap was separated, and candles
were placed at floor level. The source audit also caught black exported prop
backgrounds; the nine native prop layers were converted in LibreSprite and exported
again, with exact alpha/color comparisons passing afterward. The assembled Unity
textures were already opaque and were unchanged by that export correction.

Final review: warm accents are small, combatants remain readable, the floor joins
cleanly, and the outer masonry stays darker than gems and frames. Tall screens
reveal additional quiet upper wall. The existing short-screen HUD still covers the
upper gate arch and some wall courses; this is background cropping, not a moved or
redesigned HUD. This is a working art refinement pending the user's visual judgment.

Evidence:

- [1080×1920 game](../../ArtSource/Backgrounds/Validation/Unity-1080x1920.png)
- [1080×2400 game](../../ArtSource/Backgrounds/Validation/Unity-1080x2400.png)
- [Unity result](../../ArtSource/Backgrounds/Validation/Unity-Result.txt)
- [1920 layout report](../../ArtSource/Backgrounds/Validation/Layout-1080x1920.txt)
- [2400 layout report](../../ArtSource/Backgrounds/Validation/Layout-1080x2400.txt)
- [Native/export measurements](../../ArtSource/Backgrounds/Validation/AssetManifest.json)

No phone/device run was performed. These are actual Unity Editor renders and
automated scene checks; they do not imply user art approval or device validation.
