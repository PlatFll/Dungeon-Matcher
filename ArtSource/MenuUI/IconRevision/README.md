# Menu icons and button materials

## Scope

Follow-up to the LibreSprite frame pass on PR #196, starting at
`8fc6f036e1db20afb4d4c5ace9b237c8e0a2e7e0`. The user requested all button icons
be extracted from the supplied targets and asked for their richer button shading.
The clean originals in `../references/` supply the pixels. The annotated chat
comparisons identify the scope. Canonical guide v1.16 and the current golden
register remain unchanged; this does not approve a new character reference.

## Changes

- Fourteen separate icons: crowned skull, helmeted skull, potion, mastery gem,
  crossed swords, trophy, music, speaker, reduced motion, vibration, resume,
  suspend, retry and end-run skull. The shared gear also uses a cleaned extract.
- Native canvases remain 24–32 pixels. Shapes have dark outlines, binary alpha
  and internal padding; no labels or surrounding scene are baked into the icons.
- Play, Practice and Challenges retain their dark inset plates. The helmet,
  potion and mastery gem sit directly on the pale button face, as in the target.
- Settings-row icons now clear the left brackets. Their labels retain 16-em
  lettering; the longest row fits a 104px field. Short green/red actions place
  their icon and label together nearer the center, matching the reference group.
- Six button families retain their extracted silver rims and purple jewels.
  Empty reference material patches add worn stone and colored enamel to their
  faces, including hover, pressed and disabled states. Live text remains Thaleah.
- Separate editable `.aseprite` files and PNG exports retain existing Unity
  resource names, native dimensions and import GUIDs.

Hall, wordmark, playable portraits, combat, board feel, button callbacks and
save formats are preserved. Five pre-existing local files remain excluded.
No PixelLab requests, credit purchases or other image generators were used.

## Review

- [Main menu before / after / reference](main-menu-comparison.png)
- [Settings before / after / reference](settings-comparison.png)
- [Icons at native scale and 4× nearest-neighbor](icons-native.png)
- [Button states and shared parts](native-assets.png)
- [Full menu](after/1920-main-menu.png) and [settings](after/1920-settings.png)
- [Tall settings](after/2400-settings.png)

The native files and technical report establish dimensions, palette counts and
alpha. The review sheets are inspection previews. Fine shading is consolidated
onto the game's coarser pixel grid; these are cleaned component extracts, not
pixel-identical high-resolution cutouts. Thaleah lettering, the existing logo,
account information and real gameplay backdrop still differ from the mockup.
Physical-device checks and subjective visual approval remain separate.

## Reproduction and evidence

`Tools/Build-MenuFrames.ps1` runs `BuildFrames.js` and `IconDetails.js` inside
LibreSprite, verifies the written cels, then exports PNGs through its native batch
converter. Each source/export uses a fresh document process because this installed
development build intermittently retained stale pixels across scripted saves.
The failed exports were caught and replaced; final comparisons require exact
ASE/PNG equality. `-ExportOnly` re-exports hand-edited sources. Layer normalization
only repairs ASE metadata. The full menu build finishes with this step, so it
cannot restore the earlier simplified icons. Extraction rectangles and cleanup
are explicit in `IconDetails.js`; material patches are in `BuildFrames.js`.

Import with `Tools/Import-MenuArt.ps1`; inspect with `Tools/Validate-MenuArt.py`.
Rebuild review sheets with `Tools/Review-MenuFrames.ps1 -Revision IconRevision`.
See `evidence.json`, `lineage.json` and `validation/` for final measured results.

Final checks: **54/54 native ASE/PNG/runtime comparisons passed; all three
MenuUiReviewTests passed; Tools/Validate-Unity.ps1 passed with Unity 6000.3.19f1.**
Tests cover 1080×1920, 1080×2400, 720×1280, safe insets, all five playable portraits,
live text fit, button hit areas, preferences, pause and saved-run continuation.
Editor audio was muted and disposable profiles were used. Repeated runs count
as three distinct tests. The 42 revised PNG exports preserve native dimensions;
all state masks agree. Twelve other native assets, all menu import metadata and
the five pre-existing local files are unchanged. No merge was performed.
