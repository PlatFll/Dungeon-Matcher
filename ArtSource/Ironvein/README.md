# Ironvein native art source

All artwork here is a production candidate for the user's review. No generated
image, numerical check or internal selection is labelled as user approval.
Existing approved player, gem and other-zone assets remain independent.

## Source and selection

- `Candidates`: original PixelLab stills for thirteen enemies and Rivet Turret.
- `Selected`: exact palette-only material cleanup; original silhouettes retained.
- `Inputs`: transparent action margins, integer placement and fixed boot baselines.
  Ordinary canvases are 80x80, large machines 112x112. The drawn sprite was not
  resampled. The repaired Grand Delver drill tip and Smith hammer edge have exact
  unmasked-pixel checks and separate source receipts.
- `RawMotion`: every retrieved original frame, contact sheet and measurements.
  Rejected takes remain provenance, not production input.
- `Motion`: reviewed native frame selections and timing, consumed by Unity.
- `Variants`: separate Grand Delver pilot/reserve candidates. The remount game
  experiment stays disabled by default; phase art does not enable it.
- `Production`: exact request receipts, download manifests, palette decisions,
  generation accounting, frame selections and technical checks.

`Tools/Export-IronveinMotion.py` assembles PNG sheets without resampling or
repainting; it verifies each exported frame against its selected source RGBA.
The technical register records individual dimensions, occupied bounds, alpha,
palette counts, source/sheet hashes, durations and actual contact frame.
The gallery shows the real PNGs at 1x and 3x on a neutral background. A composite
does not establish native dimensions or technical compliance by itself.

## Review decisions

Keep distinct dwarven silhouettes and equipment; broad material shading follows
the current art-direction guide. Idles remain grounded. Hold states are used only
for actual kit response windows. Powered weapons extend/glow for one basic and
then retract. Damage remains in the existing owner/event path.

Oversized generated flares, clipped weapon extremes, wrong weapon sides and
identity-changing samples are rejected or excluded from selected frame lists.
The initial powered Pickaxe lift-only sample and Grand Delver rear-flare/wrong-arm
takes are not imported. Source frames omitted for clipping are documented in
`motion-selections.json`; they are not silently painted over.
Surveyor's green assay sparks were mapped to the orange ore ramp in PixelLab's
free workbench. Exact mapping and unchanged alpha/other pixels were verified.

## Reproduction and approval boundary

Run the preparation/export scripts, then invoke
`IronveinArtImporter.ImportMotion` in Unity. This importer updates only additive
Ironvein art bindings and optional phase presentation, not combat tuning.
The native galleries are `.utmp/Ironvein/ArtReview/Review.html` and
`.utmp/Ironvein/MotionReview/Review.html`; scripts regenerate them.

Paid scope: 400 subscription generations total, up to 300 initially and 100
reserved. The live ledger is `Docs/IronveinExcavation/GENERATION_LEDGER.csv`.
Unused allowance is not a target. No credit purchases. Gameplay validation and
actual Unity proof are recorded in `Docs/IronveinExcavation/VALIDATION.md`.

## Cave, HUD and sound production

`Scene/` retains independent native cave, floor, tunnel, timber, rail, cart,
ore, lamp, winch, pump and banner modules. The selected tunnel is `cave_depth`;
the quiet floor is `cave_floor_v2`. The first floor and masonry tunnel are
unselected source provenance. Three Unity prefabs compose the same modules at
their original 64 PPU; character floor anchors remain owned by the existing UI.

`UI/Prepared` contains free PixelLab workbench corrections with their recipes
and exact palette/alpha checks. Frames use 80x80 L corners and 64x16 rails;
the energy frame is 144x32, wave plaque 128x32 and player slices 146x16.
These dimensions match the existing native layout instead of stretching art.
`BackdropQuiet` reduces the surround's contrast; `InsetSteel` distinguishes
the player and bottom panels. The selected cell/button/panel/supply sources
are their `_v2` variants. Earlier candidates remain unbound.

`Mechanics` contains distinct materials and damaged states, a separate tagged
Core, small and fixed drills, fuse, Rattled and native impact frames. The last
two `ore_burst` frames contain residual flat masks and are not imported.
All views are optional; board receipts, actor contacts and saved state remain
the gameplay authority. Reduced motion suppresses spinning/pulsing.

`Audio` is an original locally synthesized 120-second, 96 BPM temporary cue
plus twelve short mono mechanism sounds. The score and signal measurements
are reproducible with `Tools/Render-IronveinAudio.py`. No recorded samples or
external music service were used. Listening approval is still required.

Reproduce this phase with `IronveinThemeImporter.ImportTheme`. Source dimensions,
alpha, visible palette counts and hashes are beside each family; source copies
are never silently upscaled. `Tools/Review-IronveinScreen.py` assembles the
real Unity captures and native material gallery. Comparison with the supplied
mockup is a composition assessment, not a claim of source-pixel equivalence.
