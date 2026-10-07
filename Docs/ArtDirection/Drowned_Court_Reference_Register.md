# Drowned Court reference register — 2026-10-05

The existing art-direction guide and approved humanoid reference board remain
authoritative for native pixel density, material ramps and broad shading clusters.
Marine bodies may have distinct silhouettes; a wider animation canvas does not
authorize enlarging the body pixels.

## Approval and lineage

- The user approved the first four Phase 2 designs and requested solid-color eyes.
  `ArtSource/DrownedCourt/Approved/APPROVED_DESIGNS.json` records exact PNG hashes,
  dimensions and the sixteen changed eye pixels. Other pixels and alpha are unchanged.
- The ten remaining designs are director-selected under the instruction to finish
  implementation. Their palettes reuse the current cast's restrained material ramps.
  They are **not recorded as user-approved**. See `Production/Selected/StillManifest.json`.
- The 104 motion roles reuse selected paid clips where suitable. Native preparation
  excludes unsolicited impact-flash drawings, maps to each selected material palette,
  pads canvases and returns attacks to the exact ready pose. Frame indices, durations,
  alpha, colors and hashes are in `Production/Selected/Motion/NativeChecks.json`.
- Spearman and Captain use 128×80 motion canvases for horizontal weapons. Other
  ordinary actors use 80×80; Warden and Queen use 96×96. The production source stills
  and individual PNG metadata establish dimensions; a composite alone does not.
- Marine panels and modular scenes are selected production candidates. Reefgate,
  Coral Cloister and throne approach are three compositions, not three independently
  generated scenes. Semantic gem identities, rank badges and approved player art remain.
- Drowned Court music is an original locally composed temporary cue. Numerical loop
  checks do not establish human listening approval.

## Review and budget

The 2026-10-06 lore revision classifies the approved appearances without redesign:
Reef Clans use scavenged physical gear; Nacre Court preserves ceremonial shell,
pearl craft and magic. Exact membership is in `Docs/DROWNED_COURT_ENCOUNTERS.md`.
Culture does not change species, rank, approved faces or equipment. Only Hammerhead
and Captain are authorized for compact shark-tail edits in this revision.

Those tail edits are now applied to the selected sources and existing Unity PNGs:
three still canvases and all thirteen motion roles (118 frames). The original
opaque pixels, palette, scale, floor and dimensions are unchanged. Native comparison,
full motion sheets, hashes and connected-tail checks are in
`ArtSource/DrownedCourt/TailRevision`. Revised tails await the user's review.
Needlefin also lacks a clear tail silhouette; reported only and left unchanged.
The older `Production/Review` remains the historical pre-tail package.

`ArtSource/DrownedCourt/Production/Review/Review.html` works offline and embeds real
PNG/GIF files. The neighboring ZIP contains the media and manifests. Runtime images
are identified separately from native sheets. `Hashes.json` verifies package files.

The production ledger reconciles 82 accepted jobs/182 quoted generations with the
actual balance decrease 1542 → 1360. This is separate from Phase 2's 39 generations.
The user approved 300 total with 220 initial and 80 reserved; thirty-eight initial units
and the entire reserve remain unused. No credit purchases occurred.

Unselected attempts remain in `Production/MotionRaw` and job records for provenance.
Only `Production/Selected` is an import source. Later art changes must retain this
distinction and update hashes rather than relabeling an unseen revision approved.

## Roster revision pearl candidates

`ArtSource/DrownedCourt/RosterRevision` holds native direct-pixel candidates, not
user-approved replacements. The 32×32 one-hit pearl and armored two-hit variant
share an exact bright pink core and marine shell ramp. Exposed pearl is 32×32;
orbit pearl/pop are 12×12, shell fragment 8×8 and microbubble 6×6. `pearls.json`
records each PNG's actual dimensions, opaque palette count (2–9), binary alpha and
SHA-256. `PearlReview.png` displays actual 1× and nearest-neighbor 3× comparisons.
The pink magical ramp is recorded explicitly; shell colors come from the existing
Court coffer. No character redesign or PixelLab generation was used.

`Tools/author_court_pearls.py` preserves the editable construction. Run the scoped
`CourtRevisionImporter.Run` after a historical Court rebuild; it sets FullRect,
64 PPU, Point, uncompressed, unmipped sprite imports and only the pearl theme
fields. It does not overwrite environment prefabs. Shell fracture is a short
presentation-only burst using the same native shell fragment.
