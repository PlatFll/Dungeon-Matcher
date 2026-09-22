# Dungeon backgrounds — working refinement, 2026-09-22

Backgrounds only. Rebuilt in LibreSprite from the supplied dungeon mockup motifs;
no UI, characters, gems or text were extracted from the reference screenshots.
The supplied images are preserved under `References/`. This is a reviewable working
replacement, not a claim of new user approval for every detail.

## Editable sources

Every source below has a matching PNG export at exactly its native dimensions.

| Source | Size | Purpose |
|---|---:|---|
| `Modules/WallA`–`WallD` | 64×64 | Four cool stone variants |
| `Modules/FloorA`–`FloorD` | 64×64 | Walkable slabs and front ledge; matching side joins |
| `Modules/Foundation` | 64×64 | Quiet wall beneath the platform |
| `Modules/Torch` | 32×64 | Small warm wall accent |
| `Modules/BannerSwords`, `BannerSkull` | 48×96 | Muted plum heraldry |
| `Modules/BarredGate` | 112×160 | Recessed barred arch |
| `Modules/BoneShrine` | 64×112 | Subdued alcove statue |
| `Modules/CrateStack` | 80×72 | Warm wooden dressing |
| `Modules/Candles`, `Skulls`, `Chain` | 40×56, 56×40, 16×128 | Small supporting props |
| `BattlegroundWall` | 512×256 | Assembled wall and dressing |
| `BattlegroundFloor` | 512×64 | Eight-cell floor strip |
| `BattlegroundScene` | 512×384 | Complete native assembly for review |
| `GeneralMasonry` | 128×128 | Dark repeat for gutters and panel backgrounds |
| `GeneralBackgroundPreview` | 384×512 | Repeated masonry inspection canvas |

The wall is cooler than the former brown environment. Light comes from sparse
upper-left stone planes and localized warm palette substitutions around torches.
The surroundings use four dark violet shades; they must stay quieter than the
battlefield, characters and gems. No blur, gradients or partial-alpha edges.
Exact per-file pixel colors, dimensions and hashes are in `Validation/AssetManifest.json`.

## Native authoring and Unity

`Scripts/paint_backgrounds.js` paints and assembles native files through LibreSprite's
native JS API. Run it from LibreSprite or with its GUI-enabled `--script` option;
batch-only scripting cannot provide the same active-cel workflow. Its `ROOT` and
`SEED` paths assume this checkout. The seed is read only. Alphabetic variant names
avoid LibreSprite's numbered-file animation-sequence prompt on subsequent runs.
Then run `Scripts/finish_backgrounds.js` from the actual **Scripts menu** to clear
the nine props' temporary background-layer flags. CLI startup commands cannot
perform that UI-context conversion. Run `Export-Backgrounds.ps1 -LibreSprite
"path/to/libresprite.exe"` to export the saved native files with the native batch
exporter. Finally run the read-only verifier. The PNGs and native files are
ordinary editable assets; regenerating the script
overwrites these outputs, so port deliberate hand edits into it or save a variant.

Use **Dungeon Matcher → Art → Import Background Refinement Only**, implemented in
`DungeonBackgroundImporter.cs`. It updates only the existing dungeon environment
prefab and four background texture destinations. Do not use the broader presentation
importer for this pass: it also owns unrelated menu and consumable artwork.

Unity uses 64 PPU, Point filtering, FullRect sprites, no compression and no mipmaps.
The 512×256 wall is sliced into 32 cells; the floor into eight. Separate native
props remain reusable while the selected composition is baked into those wall cells.
Quiet wall overscan fills taller views. Floor row 48 is the existing standing
baseline: its front lip is below the feet. Existing Grid, mask, sort orders and
layout/controller ownership are retained. The old dressing maps are cleared in
this prefab so two sets of background props cannot overlap.

The existing `DungeonBackdropTile` resource GUID is preserved. Its 128×128 repeat
fills world gutters and the existing player/bottom-panel background images. It does
not replace the separate baked menu background.

## Review

See [Unity evidence](../../Docs/Validation/DUNGEON_BACKGROUND_REFINEMENT.md).
Native/PNG identity and exact imported bytes can be checked with
`Scripts/verify_backgrounds.py`; this script only reads raster data and writes
measurements. Actual production-scene screenshots are in `Validation/`.
