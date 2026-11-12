# Selected bomb idles and yellow blast review — 2026-11-12

Branch: `codex/bomb-readability-and-vfx`. Draft PR #232. Not merged.

## Selected artwork and motion

The user selected **Poison A, Healing C, Directional D and Star A**. The exact
cleaned casing PNGs are the static production fallbacks on this branch. Earlier
raw generations, unselected alternatives and the original review remain intact.

- Poison: 24 × 100 ms, 64×64. Metal body/socket stay fixed. One-pixel liquid
  movement and two staggered bubbles rise, open and pop through a seamless cycle.
- Healing: 32 × 100 ms, 64×64. Body/socket stay fixed; three tiny yellow plus
  accents emerge, rise and disappear with a quiet rest between cycles.
- Directional row/column: eight × 80 ms positions. Whole casing and gem move
  together by at most one native pixel. No arbitrary rotation or scaling.
- Star: 32 × 80 ms, 64×64. Subtle one-pixel sway plus a diagonal white highlight
  through the light facets. Dark silhouette and the selected design are retained.
- Row/Column blast: 64×64 native fronts/trails replace the earlier 64×32 art.
  Broad yellow-white flame cores, restrained amber rims, matching native height,
  bidirectional travel and the existing board mask. Gameplay footprint is unchanged.

The selected loops were finalized as actual multi-frame LibreSprite sources,
re-exported and compared byte-for-byte after transparent RGB normalization.
The runtime uses those exact PNG cels; optional missing sequences fall back to
static art. Pause freezes time; Reduced Motion returns the static designs.

## Verification actually completed

- `Tools/Validate-Unity.ps1` passed, Unity 6000.3.19f1. Log:
  `DungeonMatcher-UnityValidation-67742902-63f7-4ab2-895f-858eba0fef76.log`.
- 83 automated tests passed, zero failures or skips. Exact results:
  `Validation/SelectedIdles-tests.xml` (source run `reference-tests-20261112-081103.xml`).
- Actual Game scene at 720×1280, disposable account/profile. Captured 100 frames
  of selected idles plus actual Row, Column and Color Crystal swaps. Tests check
  all six gem sockets, hidden ordinary gems, board masking, completed moves,
  absence of generic directional bursts, pause, Reduced Motion, special-state
  replacement and star materialization/activation after animated idle frames.
- 88 authored idle PNGs: 64×64, binary alpha, minimum one-pixel canvas clearance;
  Poison/Healing gem cavities unchanged. `SelectedIdles/NativeValidation.json`.
- Shield SHA-256 remains
  `3992f944830f2827dc16fd105695fe4112c0c5c1953d5bd6e467523e8ab49d40`.
  Rattlebones and six full-size gem masters are unchanged. The earlier 12 small
  gem recolors remain exact to their approved/current palette mapping.
- Visual review: body silhouettes stay stable, the sockets hold all six gems,
  bubbles/crosses remain small accents, the star shine crosses its facets, and
  the broader blast reads clearly at native board scale without crossing the mask.
- No real-device or Android build test. New animations/blast await user review.

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
