# Dungeon mockup dissection — LibreSprite revision

The user supplied a new three-enemy Dungeon composition and requested direct
LibreSprite dissection, cleanup and game integration. Sprite Fusion was cancelled;
no generation service or image credit was used for this artwork. This is
the Dungeon revision approved for merging on 2026-10-18 (PR #199), not a newly
approved global reference.

`reference/RequestedDungeon.png` is the immutable supplied mockup. Native work
uses one 470×836 grid sampled at source `(2*x+1, 2*y+1)` from the 941×1672 image.
This deliberately excludes the final source column. The mockup is a generated
image, so that grid is an authored working scale rather than a claim about its
original pixel resolution. All production exports use hard native pixels.

## Editable delivery

- `native/DungeonLayered.aseprite`: 21 named editable layers, registered on the
  full canvas. Its flattened pixels exactly match `DungeonArtOnly.png`.
- Separate rear-wall, arch, floor, player masonry/banner, foreground props,
  player frame, wave plaque, board corners/beams, side and bottom modules.
- `ForegroundProps` assembles the torch pair, barrels and bones. Seven additional
  prop ASE/PNG files preserve their individual cutouts and source positions.
- `native/TopHudBackground.aseprite`: isolated top scene for further editing.
- `Review.html`: supplied target, art-only result, switchable layer inspector,
  asset library and fresh actual Unity captures when validation is packaged.

All drawing, extraction, color cleanup, document construction and PNG exports
execute in LibreSprite. Python reads native files for independent verification
and packages the review page; it does not paint production artwork.

The cleanup uses 84 authored environment/material colors. The runtime palette
limit is not a character rule. Near-duplicate generated shades collapse into
controlled violet masonry, silver, cloth, wood/bone, jewel and flame ramps.
Transparent silhouettes remain hard. The three actor/weapon/HP regions are
removed; the floor is reconstructed as one continuous perspective surface,
avoiding visible patch seams and disconnected sword/bow fragments. The concealed
counter areas are repaired as masonry. These repairs are authored replacements
for missing background, not recovered original pixels.

## Integration boundary

Existing `DungeonPresentationArt` owns the registered backdrop placement. The
arch uses the new source rectangle `(193,59,115,121)` and the foreground uses
the full top extent `(0,0,470,298)`. Both render behind live actors. No actor,
encounter, board, input, damage or progression system is changed.

`BattlePresentationImporter.ImportApprovedDungeon` selects this revision's
matching Dungeon module names while preserving the original kit as history.
It adds only `ForegroundProps` to the production resource set. Shared controls
and board-cell images still use their prior approved exports across all zones;
their new editable studies here are not imported. Whole-screen assemblies,
the complete empty board well, and top-scene review composites are not imported.
Forest, Court, Ironvein, Frostwake, menu, characters, gems and existing user
changes are preserved. `provenance.json` records 2,683 protected file hashes.

## Rebuild and verification

Run `Build.ps1` with Windows PowerShell to construct, reopen and export through
the installed LibreSprite. `Tools/Normalize-FinalizedSourceLayers.ps1` clears
background flags in source metadata before native export; it changes no pixels.
Run `Validate.py` with the Foundry art Python. It independently decodes every
ASE cel, checks export equality, master-layer reconstruction, binary alpha,
palette membership and protected file hashes.

Run `Import.ps1` for the Dungeon-only Unity import, then
`Tools/Validate-Unity.ps1` and `Tools/Test-ReferencePresentation.ps1 -Filter
ThreeZonePresentationTests`. The tests capture Dungeon/Forest/Court at three
portrait sizes, exercise the real board, validate control layout and verify
native import bytes. `Review.py` packages only screenshots newer than this
revision's native build. Full logs stay under `logs/` or `.utmp/`.

Executed validation is recorded in `checks.json` and `evidence/validation.json`.
The user approved this Dungeon look for merging on 2026-10-18. Physical-device
appearance remains untested; packaged validation preserves the original review-stage evidence.
Responsive Point-filtered fitting can still produce uneven physical pixel
widths; this revision does not change the established layout scaling policy.
