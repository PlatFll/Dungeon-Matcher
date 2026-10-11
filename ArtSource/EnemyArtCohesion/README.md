# Enemy art cohesion and west-facing production

User-authorized implementation of the October cast audit, including PixelLab,
LibreSprite, game integration and merge. The user confirmed three-quarter camera
with faces/actions toward screen-left. Rattlebones remains the protected primary
reference. Existing identities, species, equipment and combat timing are retained.

## Current checkpoint

All 395 native sources and PNG sheets are complete and staged into Unity:
64 enemy forms (62 current definitions plus two disabled Grand Delver alternates),
2,951 imported sprites, 353 unchanged clips. Native cels match exported RGBA
exactly; transparency is binary; every full canvas has clear outer margins.
The initial cohesion pass's mandatory Unity validator passed with Unity 6000.3.19f1.
Asset checks passed 17,494 assertions; scene checks passed all 5,902
drawing/portrait combinations.
Ten relevant NUnit tests passed across the recorded runs, including real attack
and ability contact, pause, cancellation and return-to-idle checks. The exact
coverage, corrected fixture failures and limitations are recorded in
[the validation report](../../Docs/Validation/ENEMY_ART_COHESION.md).

Puffer follow-up: the user rejected the moved eye and added mouth. His original
facial pixels are restored across all affected states through the native recipe,
retaining west-facing poses, canvas clearance and artifact cleanup. The gallery
and Highlights show the restored version. Pixel/export checks were rerun; Unity
was not rerun for this art-only correction with unchanged imports and clips.

Material assignments simplify selected cloth, armor, shell and beard clusters.
Specific repairs replace Snapvine's detached-twig
death with a rooted wilt, uncover Court Mage's face, reduce King's opaque swing
wedges, flatten Shield Knight engraving, and complete clipped props and effects.
Puffer/Lantern attacks and Scout/Moray deaths receive complete-pose westward
corrections. Grand Delver's disabled remount has a 288x144 canvas and complete
incoming machinery; its gameplay switch remains disabled.

125 general masked PixelLab results were inspected: 89 selected and 36 rejected.
Nine Smith repairs were also selected. Rejected duplicate props, particles and
geometry changes are retained as provenance, with restrained native repairs.
Both whole-still studies were rejected; no new character identity was substituted.
The 137 paid jobs consumed 815 generations (750 masked general, 54 Smith,
10 rejected studies, one wilt animation); final observed balance was 223.

## Sources and review

- Native/: editable LibreSprite cels, full canvas and original clip exposures.
- Sheets/: exact LibreSprite PNG exports copied to production paths.
- Originals/: immutable baseline PNGs and Unity metadata.
- jobs.json: original GUID/fileID rectangles, native dimensions and registration.
- material_rules.json, native_edit.js, frame_overrides.json: native edit recipe.
- repair_review.json, RepairReceipts/, generation-ledger.json: selection/lineage.
- candidate-validation.json: measured native/export equality and per-frame bounds.
- unity-import-manifest.json: production hashes, retained IDs and unchanged clips.
- [Review.html](Review.html): offline original/updated playback at actual exposures.
- [Highlights](../../Docs/Validation/EnemyArtCohesion/Highlights.png): labeled
  before/after inspection layout; original pixels at nearest-neighbor 3x.

Canvas expansion adds space without resampling characters. Normal expansion is
24 left, 20 top, 24 right, four below; remount uses 88/28/88/4. Unity remains
64 PPU, Point, uncompressed, unmipped, FullRect, with a fixed center/four-pixel
floor pivot. The shared UI fitter compensates for that floor offset.

## Re-export and import

Run prepare_native.py --all (or --actors with comma-separated IDs), then
Tools/Run-EnemyArtNative.ps1 and Tools/Export-EnemyArtNative.ps1. All production
pixel editing runs inside LibreSprite. Python only handles source selection,
read-only inspection/review layouts, metadata and unchanged file copies.
Run verify_candidates.py, inspect affected poses and playback, then
stage_unity.py --apply. Staging preserves GUIDs and signed 64-bit fileIDs and
refuses unknown intervening PNG edits. Do not run historical roster importers
over these assets: their earlier source sheets/rectangles are archived history.
Use this pass's native sources and staging metadata for future enemy art edits.
The edge-planning and receipt helpers retain the original production workflow;
historical checkpoint flags are not instructions to spend more generations.
RepairInputs contains the exact submitted images, independent of temporary files.

Runtime/code edits require Tools/Validate-Unity.ps1 before PR/merge. The opt-in
EnemyArtCohesionValidation.RunAssets checks imports and clip references;
EnemyArtCohesionValidation.Run checks the real Game scene at both portrait
heights. Neither native measurements nor automated playback establish user
visual approval or physical-device acceptance. Preserve unrelated Court prefab
and three consumable metadata edits outside this feature.
