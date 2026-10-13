# LibreSprite menu frame revision

## Scope and source authority

The user approved the hall and requested closer button/frame matching through
LibreSprite, explicitly allowing extraction from the supplied mockups. This
supersedes the earlier pending PixelLab frame plan for this task. No generation,
credit purchase, character edit or background regeneration was performed.

Base: main `264255d7e2eca1f484dc3bc7f66ba39c1d722f90` (PR #195).
Guide v1.16 and the current golden register were read; the Rattlebones source,
production idle and clean original UI targets were inspected and hash-verified.
The clean 941×1672 originals in `../references/` supplied the actual pixels; the
annotated chat comparison images identify the requested scope.

The originals are unchanged. `lineage.json` records the current authorization,
reference hashes, extraction rectangles and native cleanup. The older reference
lineage remains a record of the original menu pass, not of this extraction pass.

## Result

- Six button families with normal, hover, pressed and disabled states: 24 exports.
- Nine shared pieces: panel, header rule, settings row, two toggles, icon inset,
  purple corner jewel, skull crest and divider.
- **33 revised assets**, each with an editable LibreSprite `.aseprite` source and
  a separate PNG in `../native/`. Existing runtime resource paths/GUIDs are kept.
- Hard alpha silhouettes remove surrounding scene pixels. Baked labels/icons
  are removed from the button faces. Faces are repainted with connected shades;
  sampled rim detail is reduced to a controlled palette on the native grid.
- Reference-derived silver brackets and bolts, faceted purple diamonds, quieter
  pale navigation faces and dark inset rows replace the original simple bevels.
- The 48×48 panel retains protected 12px corners and a repeatable middle. It is
  tiled at native size instead of stretching a complete reference panel.
- Green/red action labels use 24-em Thaleah; long neutral labels keep their
  verified 16-em fit. No baked text is imported into an interactive control.

The approved Hall/HallTall, logo, character sprites, gameplay code, board feel,
combat balance, save formats and button callbacks are preserved. The importer
now avoids copying byte-identical PNGs to prevent an observed Windows mapped-file
lock, while still applying the configured import settings.

## Visual review

- [Main menu: before / Unity after / reference](main-menu-comparison.png)
- [Settings: before / Unity after / reference](settings-comparison.png)
- [Native pieces and all four button states](native-assets.png)
- [Full-size menu](after/1920-main-menu.png) and [settings](after/1920-settings.png)
- [Tall menu](after/2400-main-menu.png) and [settings](after/2400-settings.png)
- [Saved run](after/saved-run.png) and [home settings](after/menu-settings.png)

Composites are nearest-neighbor inspection previews. The separate native PNGs,
ASE files and technical report establish actual dimensions, colors and alpha.

### Remaining differences

Face texture is simplified into broad native shades rather than reproducing
every tiny mark in the high-resolution mockup. Existing utility icons, wordmark
and Thaleah letterforms retain their approved implementation; they are not exact
copies of the mockup. The pause backdrop is the real current game, and the home
screen retains its account/Continue information. Device touch/display testing
and subjective visual approval remain with the user.

## Rebuild and validation

`Tools/Build-MenuFrames.ps1` runs `../BuildFrames.js` **inside LibreSprite** for
extraction, palette reduction, masking, native cleanup and ASE authoring. It then
exports PNGs through LibreSprite. Layer normalization only fixes ASE metadata.
No other image editor/generator writes production pixels.

`Tools/Import-MenuArt.ps1` imports the PNGs into Unity. `Tools/Validate-MenuArt.py`
reads and compares all 54 existing ASE/PNG pairs, runtime exports and import
settings. `Tools/Review-MenuFrames.ps1` assembles review previews in LibreSprite.
The full menu build ends with the new frame step so it cannot revert these rims
to the original simplified construction.

See `evidence.json` and `validation/` for exact test results and source hashes.
Final checks: **54 native ASE/export/import checks passed; all three menu tests
passed; `Tools/Validate-Unity.ps1` passed with Unity 6000.3.19f1.** Menu tests cover
1080×1920, 1080×2400, 720×1280, synthetic safe insets, all five playable portraits,
live text fit, actual button hit areas, settings toggles, pause and saved-run
continuation. All four state silhouettes agree within each button family.
Tests mute Unity Editor audio and use disposable profiles. Repeated executions
of the same three menu tests count as three distinct checks, not additional tests.
Five pre-existing local files are preserved byte-for-byte and excluded from the PR.
