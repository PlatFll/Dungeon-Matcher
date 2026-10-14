# Three-zone battle presentation

Implemented review candidate, 14 October 2026. The pack requires a PR and user
review before merge. [Actual Unity comparisons and ambient previews](Review.html).

## Scope and reference authority

The complete user pack is retained in `brief/`. Its three battle mockups govern
composition and material direction. The current canonical guide and golden
register govern pixel rendering; Rattlebones, Farmer and Pan were inspected and
their exact hashes verified. The older `(2)` filenames are absent; the user
previously authorized the canonical equivalents. Exact hashes and source roles
are recorded in `provenance.json`. New art remains a candidate for visual review.

This pass changes presentation for Dungeon, Magical Forest and Drowned Court.
Characters, enemies, gems, board cell art, HP bars, blockers, damage, ability
timers, stagger, special combinations, board feel and run progression are
unchanged. Main menu, opened settings, Frostwake and Ironvein retain their art.
The existing shared gear icon is reused. Five pre-existing local modifications
are excluded from this PR and checked against their starting hashes.

## Assets and native workflow

There are **57 production PNGs**, each with a matching editable `.aseprite`:

| Family | Native dimensions | Purpose |
|---|---|---|
| Shared Guide | 88×40, four states | Live Thaleah label on reference-derived silver rim |
| Shared supply | 64×64, four states | Existing potion/bomb art remains a separate icon |
| Shared ability | 176×64, four states | Silver/purple shell around existing player symbol |
| Energy | 144×32 frame, 8×8 fill | Transparent track window, existing live charge |
| Shared / zone beams | 64×16 edges, 80×80 corners | Repeated native edges with protected corners |
| Zone player frame | 146×25 top/bottom, 146×32 middle | Current three-slice layout profile |
| Zone plaque / banner | 128×32 / 44×64 | Live wave text and separate emblem |
| Zone panel / general material | 128×128 | Tiled backgrounds |
| Side dressing | 36×384 per zone | Reference-derived border architecture and growth |
| Zone scenery | 512×384 each | Reconstructed architecture sliced into 48 tiles |
| Foliage / kelp / statue | 56×80 / 48×88 / 128×160 | Native environment modules |
| Ambient sheets | Torch: 6×24×40; bubble: 6×12×12; flow: 6×64×96 | Discrete native frames |
| Lantern / rune | 30×46 / 32×40 | Restrained movement / opacity pulse |

`Build.js` reconstructs the assets in LibreSprite from selected material patches,
traced cutout silhouettes and direct pixel drawing. It recomposes compatible
existing woodland and court environment modules without rescaling those assets.
No complete target image is imported as a runtime screen. Sampled high-resolution
reference fragments are rebuilt at declared native canvas sizes; they are not
claims that the supplied mockups themselves have those native dimensions.

LibreSprite sources are reopened and compared byte-for-byte before export.
`Normalize-FinalizedSourceLayers.ps1` clears erroneous background-layer flags
without changing any pixel data, so cutout alpha survives the PNG export. The
independent native checker compares decompressed ASE cels with PNG RGBA bytes and
the imported Unity files. All exports use binary alpha, Point filtering, 64 PPU,
FullRect, no mipmaps, no texture compression and no platform overrides.

Rebuild / import / check:

```powershell
rtk powershell -ExecutionPolicy Bypass -File Tools/Build-BattlePresentation.ps1
rtk powershell -ExecutionPolicy Bypass -File Tools/Import-BattlePresentation.ps1
rtk python Tools/Validate-BattlePresentation.py
```

`-Only` supports focused native rebuilds; `-ExportOnly` re-exports editable
sources. The importer only writes the new `UI/BattlePresentation` resources and
the three `Presentation_*` environment prefabs. Historical art importers are not
used. The presentation Resources paths make all new assets build-reachable.

## Runtime integration

`BattlePresentationSkin` selects optional assets for the three authorized zones
and falls back to the existing theme for all other zones. Existing BoardVisuals,
top/bottom frame controllers, player frame and background owners apply the art.
The board's cells and resolution rules remain with their existing owners.

`BattleAmbientPresentation` adds two top-area dungeon flames; forest lanterns,
rune glow, leaves and waterfall strokes; Court bubble/pop loops and kelp sway.
Native source-pixel movement is rounded, small and independent of gameplay.
Pause stops its clock, Reduced Motion uses a stable pose, and scene unload
destroys its objects/materials. Cosmetic code consumes no Unity random values.
Separate per-sheet unlit materials avoid the shared URP texture binding artifact
caught in actual Unity captures. Border decorations have no input targets and
hide on screens with insufficient board gutter.

All five existing ability symbols remain visible. Only the known 176×64 legacy
whole-button images receive a runtime interior sprite rectangle; their texture
pixels remain untouched. The shared rim is then rendered independently.

## PixelLab usage

**One generation charged; zero generated outputs selected.** The service had no
regular generations and five daily bonus generations. One reference-conditioned
Pixflux Court statue study was rejected for retained UI marks and poor form.
Its exact prompt, input, job, receipt and rejected image are in `generation/`.
The higher-quality Pro Flash endpoint quoted five or six generations, exceeding
the four daily generations left. No credit purchase or provider substitution
was made. The selected statue was reconstructed directly in LibreSprite.

## Executed validation

- Exact ASE/PNG/Unity comparison passed for all 57 assets, including required
  cutout alpha and import settings. `native-checks.json` records dimensions,
  occupied colors, PNG hashes and the five preserved local-file hashes.
- Final Unity test / validator evidence is recorded in `validation.json`.
- The focused suite covers all three zones at 1080×1920, 720×1280 and
  1080×2400 with a safe-area inset; pause and Reduced Motion; unchanged idle
  board snapshot/refill RNG; accepted move resolution; scene cleanup; and all
  five playable ability symbols. Existing board-feel checks run alongside it.
- Captures are actual Unity output; audio was muted. `review/validation/` retains
  layout reports and bound image/material paths. Preview sequences are 24 real
  screenshots sampled at 150 ms, not fabricated animation renders.
- This is desktop Unity evidence. Physical-device performance and human visual
  approval remain untested.

## Remaining differences from the targets

The current eight-by-eight board, battle proportions, HP bars, counters,
Thaleah labels and combat hint are retained. Court displays its real wave
counter. The target's character, gem and cell drawings were outside this scope.

Dungeon floor/stone repeats are more visible than in the mockup. Forest keeps
the existing approved woodland forms and has a narrower, simpler waterfall.
Court includes the reconstructed king statue and reused recessed court masonry;
its lighting, stone palette and distant architecture are simpler than the target.
The three new environments each use one composed vista, so older Court scenery
variants are preserved as files but not selected by this presentation override.

The shared outer HUD beam is simpler than the mockup's ornaments. Existing
button availability dimming remains functional. Exact composition changes are
shown openly in the comparison page; numerical validation does not certify an
exact visual match.
