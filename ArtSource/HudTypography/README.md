# HUD native sources

PixelLab candidates and lineage are preserved in `Candidates/` and `manifest.json`.
`PowerupTile.png` was rejected; `PowerupTileFixed.png` is the accepted tile.

Rebuild the editable sources and PNGs with:

```powershell
Tools/Finish-HudArt.ps1 -LibreSprite "path/to/libresprite.exe"
Tools/Import-HudTypography.ps1
Tools/Review-FinalizedVisuals.ps1
```

`FinishHud.js` runs in LibreSprite. It keeps native dimensions, restricts the
palette, makes alpha binary and derives color-only tile interaction states.
Sources are 32×32 for the tile and 24×24 for each icon. Unity uses Point filtering,
uncompressed textures, no mipmaps, centered pivots and Full Rect meshes.
The HUD presents tiles at 64×64 logical pixels and icons at 48×48.

The importer also creates/binds the Thaleah bitmap atlas. Font provenance and
license are in `Assets/_Game/Fonts/Thaleah/ATTRIBUTION.md`.
