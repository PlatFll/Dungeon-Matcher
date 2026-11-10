# PixelLab Marine Net and Hearth Ember

User requested PixelLab replacements alongside production integration and PR #227
merge on 2026-11-10. The three exact input images were explicitly approved for
upload to PixelLab: Rattlebones, the earlier MarineSnare and HearthEmber drafts.
No new golden-reference membership is asserted.

Two PixelLab Pro jobs cost **20 subscription generations total**, 10 each.
Balance changed from 3745.90 to 3725.90; USD credit balance remained $0.00.
`generation-requests.json` retains full prompts, reference roles, palette inputs,
seeds, job IDs and receipts. Rattlebones was passed as an actual style image for
outline/detail/shading, not used as an anatomy or palette template.

Both jobs returned 64 native candidates, preserved in `Raw/` and contact sheets.
Selected zero-based indices: **MarineSnare 51**, **HearthEmber 3**. The net has a
readable knotted perimeter and four attached barbs; the hearth has a cohesive
slate stone ring and a single clear warm flame. These replace only the two effect
PNGs; existing characters, gems, environments and shared controls remain intact.

`finalize.py` reproduces the native cleanup and exports through LibreSprite:

- Net: thin redundant interior black outline pixels to expose the trapped gem.
  Every colored rope/barb pixel and the external silhouette remain unchanged.
- Hearth: translate the entire drawing upward two native pixels to provide
  bottom clearance. No drawing pixels are resampled or removed.
- Both: normalize invisible RGB, save editable `.ase`, export 32x32 PNG, and
  require exact RGBA equality after the LibreSprite round trip.

Net: 6 opaque colors, 55.86% transparent, bounds `[2,2,30,31)`.
Hearth: 7 opaque colors, 52.83% transparent, bounds `[2,2,30,30)`.
Both have binary alpha. The net's final open area is below the prompt's aspirational
65%; the actual Game-scene capture is the readability check, not that request.
The native dimensions, import GUIDs, Point filtering, 64 PPU and existing gameplay
footprints are retained. No additional animation or gameplay timing is introduced.

`manifest.json` records guide/reference hashes, all raw hashes, chosen sources,
exact removed ink coordinates and final export hashes. `Review.png` compares the
historical direct-pixel drafts with the new effects at native size and 4x.
The historical sources in the parent directory remain preserved; do not rerun
the parent's legacy `build.py` to overwrite the current PixelLab exports.

Actual Game screenshots and test evidence are linked in
`Docs/Validation/EnemyRevisions2026-11-10.md` and `EnemyRevisions.html`.
