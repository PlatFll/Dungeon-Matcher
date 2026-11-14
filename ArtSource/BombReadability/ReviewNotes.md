# Selected bomb idles and yellow blast review — 2026-11-12

Branch: `codex/bomb-readability-and-vfx`. PR #232. User approved production integration and merge on 2026-11-14.

## Selected artwork and motion

The user selected **Poison A, Healing C, Directional D and Star A**. The exact
cleaned casing PNGs are the static production fallbacks on this branch. Earlier
raw generations, unselected alternatives and the original review remain intact.

- Poison: 24 × 100 ms, 64×64. Metal body/socket stay fixed. One-pixel liquid
  movement and two staggered bubbles rise, open and pop through a seamless cycle.
- Healing: 32 × 100 ms, 64×64. Body/socket stay fixed; three tiny yellow plus
  accents emerge, rise and disappear with a quiet rest between cycles.
- Directional row/column: eight × 80 ms native cels. The complete rigid bomb rocks ±3 degrees
  around its socket. The center stays fixed and the whole casing/plate turns
  together. Nearest-pixel native cels preserve hard edges and source colors.
  The rejected separate-side motion is superseded.
- Star: 32 × 80 ms, 64×64. Stationary silhouette with a diagonal white highlight
  through all the light facets, reaching the opposite tip before disappearing.
  The 2026-11-14 correction derives travel from the full reflective footprint,
  replacing the shortened sweep. Loop timing and every alpha pixel are unchanged.
- Row/Column blast: 64×64 native fronts/trails replace the earlier 64×32 art.
  Broad yellow-white flame cores, restrained amber rims, matching native height,
  bidirectional travel and the existing board mask. Gameplay footprint is unchanged.

The selected loops were finalized as actual multi-frame LibreSprite sources,
re-exported and compared byte-for-byte after transparent RGB normalization.
The runtime uses those exact PNG cels; optional missing sequences fall back to
static art. Pause freezes time; Reduced Motion returns the static designs.

## Verification actually completed

- Production approval pass (2026-11-14): `Tools/Validate-Unity.ps1` passed again;
  all 23 bomb/tile-presentation tests passed with no failures or skipped checks.
  Actual Game tests exercised idle playback, pause/replacement, row and column
  detonation, and Color Crystal activation. Game/GemPrefab GUIDs and approved
  still pixel equality were independently checked. Evidence:
  `Validation/ProductionApproval.json` and `ProductionApproval-tests.xml`.

- 2026-11-14 full-star-glint correction: the existing actual Game idle/lifecycle
  test passed again (1/1), including 100 fresh frames at 720×1280.
  `Validation/FullStarGlint-tests.xml` and `FullStarGlint.json` record this run.
  Native checks verify every reflective nonwhite pixel catches the light and
  that the glint reaches the far tip, without altering alpha or dark contours.
  Runtime C#, directional/poison/healing loops and all blast art are unchanged;
  their preceding validation remains applicable. No new device/build test.

- `Tools/Validate-Unity.ps1` passed, Unity 6000.3.19f1. Log:
  `DungeonMatcher-UnityValidation-d107b7a9-fc44-4191-a7ee-9addd67d2ca9.log`.
- After the whole-bomb shake correction, all 9 affected bomb tests passed,
  zero failures/skips: `Validation/WholeBombShake-tests.xml` and
  `WholeBombShake.json`. Runtime C# is unchanged from the preceding validated
  correction; the native art was imported and exercised in this fresh Unity run. The earlier broader 83-test run remains historical evidence in
  `Validation/SelectedIdles-tests.xml`; it was not rerun for this localized change.
- Actual Game scene at 720×1280, disposable account/profile. Captured 100 frames
  of selected idles plus actual Row, Column and Color Crystal swaps. Tests check
  all six gem sockets, hidden ordinary gems, board masking, completed moves,
  absence of generic directional bursts, pause, Reduced Motion, special-state
  replacement and star materialization/activation after animated idle frames.
- 104 authored idle PNGs: 64×64, binary alpha, minimum two-pixel canvas clearance.
  Poison/Healing cavities are unchanged; each row/column cel is an exact
  rigid turn of the full selected casing. All six gem composites match turning
  the complete bomb as one unit. Star alpha masks remain identical throughout.
  Each family retains actual animated pixel changes. `SelectedIdles/NativeValidation.json`.
- Runtime tests assert fixed row/column root and socket positions on all 100
  captured Game frames while checking that their casing sprites keep animating.
- Shield SHA-256 remains
  `3992f944830f2827dc16fd105695fe4112c0c5c1953d5bd6e467523e8ab49d40`.
  Rattlebones and six full-size gem masters are unchanged. The earlier 12 small
  gem recolors remain exact to their approved/current palette mapping.
- Visual review: body silhouettes stay stable, the sockets hold all six gems,
  bubbles/crosses remain small accents, the star shine crosses its facets, and
  the broader blast reads clearly at native board scale without crossing the mask.
- No real-device or Android build test. New animations/blast were approved by the user for production and merge.

## PixelLab and editable sources

Original candidate production used 40 generations (four 16-candidate jobs).
This idle and blast revision used **zero additional generations**. Existing
PixelLab outputs were the approved source; motion was authored at native pixel
resolution and exported through LibreSprite. Receipts: `Candidates/jobs.json`.

`SelectedIdles/Editable` contains five animation sources, native GIF imports and
`Sheets` contains verified strips. `Effects/Editable` contains the revised fire
loops. The regeneration recipes live in `Scripts`; no generative service is
called by them. Review: `SelectedIdles/Preview/Review.html`, with 1×/2× native
playback, all six gem choices, pause/step controls and real Unity GIFs/screenshots.

Gameplay, utility-bomb effects, damage/rewards, UI, characters, environments and
unrelated user edits are unchanged. Main and the primary Unity checkout are untouched.
