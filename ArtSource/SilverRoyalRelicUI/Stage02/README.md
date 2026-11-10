# Silver Royal Relic — Stage 2 top HUD

User selection: **A / Royal bevel**, 2026-11-11. This is the Stage 2 review
checkpoint under the [original brief](../Stage01/Brief.txt). No merge, bottom-HUD
production or final runtime deployment is authorized by this selection.

## What changed

The approved PixelLab A wave and Settings sources were refined in LibreSprite.
The wave has clean repeating straight rails, its original chamfered ends and
small gold side diamonds. The top diamond and skull are separate native pieces,
so resizing the existing control cannot stretch these ornaments. The pale gear
is separated from its square frame and centered with a one-pixel adjustment.
All opaque colors come from the selected A sources; no new palette or image
generation was introduced.

| Export | Native canvas | Opaque colors | Use |
|---|---:|---:|---|
| WaveBody | 176 × 48 | 12 | Tiled frame, border L/B/R/T 20/10/20/10 |
| WaveDiamond | 22 × 20 | 12 | Independent top-center ornament |
| WavePendant | 28 × 22 | 8 | Independent lower skull ornament |
| SettingsFrame | 56 × 56 | 11 | Existing square touch target |
| SettingsGear | 32 × 32 | 7 | Separate centered pale gear |
| Settings | 56 × 56 | 11 | Combined inspection export |

Each export has binary alpha and transparent clearance. The `.ase` sources were
saved, reopened and exported again with exact visible RGBA equality. Transparent
RGB is ignored because it is not visible and the editor may normalize it.
Native artwork was not resampled; enlarged review images use nearest neighbor.

## Actual Unity preview

Open [Review.html](Review.html). Every before/after pair comes from the real
`Game` scene, running combat with a disposable profile, approved Rattlebones,
live enemies, gems and the existing bottom HUD. They are not scene composites.
The profile is isolated; no production scene or asset routing is saved.

`SilverRoyalRelicTopHudPreview` lives in the Editor assembly. It temporarily
substitutes the new art on the real wave and Settings Images after the existing
layout runs, retaining their rectangles, anchors, touch targets and callbacks.
Disposing it restores the production sprites. The new sprites are additive
under `Assets/_Game/Art/UI/SilverRoyalRelic/Stage02`, outside Resources.

The gallery's overview scales captures to fit the page. Use **Open after at
physical pixels** or the native asset cards to judge exact rendering.

## Executed validation

- `Tools/Validate-Unity.ps1`: Unity 6000.3.19f1 compilation.
- `Tools/Test-ReferencePresentation.ps1 -Filter SilverRoyalRelicTopHudTests.Stage02`:
  one automated Editor Play Mode test covering Dungeon at 360×640, 720×1280,
  1080×1920 and 1080×2400, plus Drowned Court at 1080×2340.
- The 1080×2400 case simulates a safe rectangle `(24, 96, 1032, 2184)`.
- Checks retain the existing board, floor line, bottom controls and top-control
  rectangles exactly. Settings opens the existing modal, pauses and closes.
  Dynamic labels `WAVE 1` and `WAVE 99999` fit the current text area using native
  font steps. Settings and ornaments use uniform whole-pixel rendering and do
  not overlap enemy intent indicators.
- The preview is disposed after each case and the original Stone wave sprite
  is verified restored. Editor audio mute is restored after the test.
- `verify.py`: source/reference preservation, import conventions, exact
  LibreSprite roundtrips, approved palette membership, capture dimensions and
  successful Unity test evidence.

These are automated Unity checks and visual screenshot review. No Android build
or physical-device test was performed. Full five-zone acceptance remains Stage 5.
Characters may be at different idle frames between before/after captures; their
art and layout are unchanged.

## Source trail and reproduction

- [Manifest.json](Manifest.json): hashes, dimensions, cost and selected source.
- `Inputs`: exact A PNG copies; Stage 1 raw PixelLab generations are retained.
- `Editable`, `PNG`, `Roundtrip`: native LibreSprite sources and checked exports.
- `QA/NativeProduction.js`, `QA/LibreSprite.log`: native pixel writing and export.
- `QA/StartingProduction.json`: preservation baseline before additive preview files.
- `QA/UnityTests.xml`, `QA/UnityCompilation.log`, `QA/UnityTests.log`: executed evidence.
- `QA/PreservationValidation.json`: independent preservation results.
- `produce.py`: repeatable source separation and LibreSprite editing. It retains
  existing import GUIDs. `review.py` builds the review page from Unity captures.

**PixelLab cost this stage: 0 generations.** Stage 1 used 29. A was already
generated and selected, so a redundant generation would not improve this step.

Stop here for user review. After approval, Stage 3 produces the energy casing,
ability frame and shared item-slot frame. Production skin integration remains
Stage 4. The current light-gray stone controls stay active in builds meanwhile.
