# Main menu and settings review

## Result and scope

The menu now follows the supplied hall, portal, pedestal and button composition.
The selected or saved hero uses the existing approved idle and native frame size.
Settings uses the silver frame, purple jewels, four toggle rows, skull divider,
and green/gray/red actions. The home gear opens the same persistent preferences.

The existing menu and run-control owners retain navigation, suspension,
continuation, confirmation and pause behavior. Zone art, board rules, combat,
economy, character production assets and save formats are unchanged.
The four pre-existing local modifications remain outside this change.

Status: the user approved the background and authorized merging PR #193 for
testing on 2026-10-12. The requested PixelLab button/frame refinement remains
pending generation replenishment; the current native assets are retained.

## Visual evidence

- [Main menu: before / Unity after / target](review/main-menu-comparison.png)
- [Settings: before / Unity after / target](review/settings-comparison.png)
- [Tall home](review/2400-main-menu.png) and [tall settings](review/2400-settings.png)
- [Home settings](review/menu-settings.png), [saved run](review/saved-run.png)
- [Minotaur](review/hero-dungeon_attender.png), [Kobold](review/hero-kobold_gunner.png)
- [Native asset contact sheet](validation/NativeAssets.png)

Comparison panels are inspection previews resized with nearest-neighbor sampling.
The separate full-resolution screenshots and actual native source files are the
dimension/pixel evidence; a composite does not establish an asset's native size.

## Reusable assets

All 54 PNGs have editable LibreSprite sources in `native/`. Runtime copies live
in `Assets/_Game/Resources/UI/GlobalMenu/`.

| Family | Native size | Use |
| --- | --- | --- |
| Hall / HallTall | 360×640 / 360×800 | Static scene; tall-phone foreground extension |
| Logo | 280×124 | Separate silver/gold wordmark with skull O |
| Play | 224×52 | Primary navigation |
| Secondary / Challenge | 136×44 / 168×44 | Home navigation |
| Affirmative / Neutral / Destructive | 184×38 | Green, gray and red actions |
| Gear | 32×32 | Shared settings control |
| Panel | 48×48, 12px borders | Tiled modal with protected corners |
| SettingsRow / Toggle | 208×32 / 52×24 | Settings rows and explicit on/off states |
| IconPlate / CornerJewel | 32×32 / 22×22 | Reusable inset and purple trim |
| SettingsCrest / Divider | 64×32 / 184×16 | Header and action separator |
| Utility/action icons | 24px to 32px canvases | Separate icons; existing potion reused |

Button families and gear include normal, highlighted, pressed and disabled
exports. Labels are live Thaleah text. The large button construction is documented
for future ability-HUD work; that HUD is not reskinned by this pass.

## Production and technical checks

PixelLab used the supplied main-menu image through its actual reference-image
input to generate the scenery. One Pro request cost **25 subscription
generations**, from 26 available to 1 remaining; **$0 credit spent/purchased**.
No further paid requests were made. `generation.json` stores prompt, image input,
job/asset identifiers and receipt. `references/lineage.json` records source hashes.
The canonical v1.15 guide/register were the input authority; v1.16 adds this menu
direction without changing character approval or reference membership.

LibreSprite authored the controls and wordmark, adjusted the portal's brightest
colors, and extended the foreground for tall screens. Production does not contain
either full target screenshot. The scenery's banners, platform and props are
part of one fixed illustration; interactive controls remain modular.

`Tools/Validate-MenuArt.py` verifies all 54 ASE/PNG dimensions and exact RGBA
pixels, binary/opaque alpha, byte-identical runtime exports, Point filtering,
no mipmaps, no compression, no platform overrides, and unchanged starting user
files. Per-asset native sizes, color counts and hashes: `validation/native-checks.json`.

Rebuild with `Tools/Build-MenuArt.ps1`; import with `Tools/Import-MenuArt.ps1`.
The build uses the established local LibreSprite installation and the existing
source-layer normalization tool. These explicit commands do not run on startup.

## Validation record

Tests use disposable profiles and mute Editor audio without muting player preferences.
Screenshots use an isolated level-1 account so no personal save is changed.

### Integration with Board Feel 2 (2026-10-12)

Integrated main `961469baef422c7f5428f7f1a62efc4d1c59b5bf` (PR #194) into this
branch. Only the two documentation headings conflicted; both sections survive.
The board, combat/core, audio, game scene, Board Feel 2 tests and related evidence
are unchanged from that main revision. The four pre-existing local edits and
the uncommitted future PixelLab revision plan remain outside this PR.

The combined menu/policy/travel run passed 24 cases and the nine board-feel
gameplay cases passed separately: **33 unique passing cases**. Actual screenshot
inspection then caught the tall capture showing the intentional Continue guide.
The capture test now dismisses it and asserts that Settings is present. Its
affected rerun passed; this repeats one of the 33 cases rather than adding a new
case. The updated tall settings image shows the correct panel. Runtime menu
and board code did not change during this integration.

Final run IDs, results, source hashes and validator log are recorded in
[the integration evidence](validation/merge-193/evidence.json). All 54 native
asset checks passed again. These targeted results do not replace physical-device
or subjective board-feel testing.

### Original implementation checks

- `MenuUiReviewTests` plus three existing travel checks: **6 passed, 0 failed**.
  [Result XML](validation/menu-and-travel.xml). Captures cover 1080×1920 and
  1080×2400, with layout checks also at 720×1280 and synthetic safe insets.
  All five playables retain their native sprite rectangle. Checks cover text fit,
  integer physical art scale, Point sampling, actual pointer hit areas, navigation,
  preference toggles, paused move clock, confirmed-exit cancellation, suspension,
  and Continue (including the intentionally paused continue guide).
- After the final home-dimmer parent fix, reran the affected navigation/continue
  test: **1 passed, 0 failed**. [Result XML](validation/home-modal-final.xml).
  It verifies a screen-corner raycast hits the full-screen modal input shield.
- Existing travel checks cover settings through smoke and both pending/committed
  transition recovery. No new gameplay or save schema was introduced.
- All four settings labels retain 16-em native sizing; [settings fit record](validation/settings-text-fit.txt).
  [Home fit record](validation/home-text-fit.txt) includes the live label bounds.
- Native ASE/export/import and preservation checks: **54 passed**.
- Required `Tools/Validate-Unity.ps1`: **passed** with Unity **6000.3.19f1**
  after the final runtime change. [Validator record](validation/unity-validation.txt).

This is targeted menu/integration coverage, not a full game-suite claim. An
earlier exploratory `DesignV2ContinuationTests` graphics run failed because its
fixtures launched at 640×480; that suite remains unverified here. The relevant
menu/continuation paths above use explicit portrait fixtures and pass. Earlier
menu test iterations also caught the continue guide's intentional pause and the
need to wait a frame for re-enabled UI raycasters; the final tests exercise both.

## Differences from the mockups

- The wordmark uses reconstructed, more geometric native lettering. Its wear,
  skew, frame ornament and skull shape are simpler than the reference.
- Hall props follow the composition, but individual masonry, flames, treasure
  and statues differ. Flames/portal are static; the selected character animates.
- Frame wear, utility icons and toggle facets are simplified for one native grid.
  Thaleah letter shapes and fit differ from the baked lettering in the mockup.
- Home retains functional account/level/gold and saved-wave information, Continue,
  and End Saved Run. These additions are absent from the reference screenshot.
- Settings overlays the actual current battlefield. The reference's older battle
  HUD and character health values were not copied into the game.
- Existing shop, character selection, guides, confirmations and zone-picker
  screens retain their previous layout/skin; this task covers home and settings.
- Tall phones expose additional foreground carpet. Physical-device touch,
  cutout behavior and subjective visual approval remain to be checked by the user.
