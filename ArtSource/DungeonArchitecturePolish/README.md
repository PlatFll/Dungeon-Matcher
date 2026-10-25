# Dungeon side architecture — review handoff

Branch: `feature/dungeon-side-architecture-polish`. Based on main `52c0f657`. **Not merged; awaiting visual approval.**

The actual playable Dungeon uses continuous modular stone columns around the board/lower HUD. Upper battle, portrait, board geometry, gems, character art, HUD and gameplay retain their existing owners and artwork.

## Assets and construction

- 14 new native modules: capital, 15px repeatable shaft, worn shaft, heavy base, ledge, recessed panel, carved skull block, three unlit candle groups, two torch-lit stone variants, two chain endings.
- Four approved DungeonHybrid torch/chain parts reused byte-for-byte. Three PixelLab jobs cost **30 subscription generations** (672.20 → 642.20); no dollar credits spent. Exact requests and lineage remain in `Sources/` and `GenerationUsage.json`.
- Separate raw sources, immutable selections, editable `.aseprite` documents, PNG exports and LibreSprite scripts retained. No flattened scene is used. `Scripts/Rebuild.py --libresprite <path>` reproduces the kit without generation.
- Stone uses the established seven-color Dungeon ramp. Torch bounce uses restrained warm planes on the exact shaft grid. Candles use five/six wax colors and dark wicks, with binary alpha and no flames/glow/effects. Source scale unchanged.
- The optional `DungeonPixelLabBattleArt.sideArchitectureModules` binding is composed by `DungeonPresentationArt` on its existing world canvas below gems/UI. Capitals/bases remain full native sprites; the shaft tiles, and ornaments follow live board bounds. Physical edges stay filled independently of safe-area UI. Other zones retain current presentation.

## Executed validation

- Unity **6000.3.19f1**, actual build-enabled `Game` scene, disposable account, Rattlebones, live Farmer/Pan Villager/Knight wave and 64 live gems.
- `DungeonArchitectureReviewTests.After`: **1 passed integration coroutine covering 6 profiles**: 720×1280, 1080×1920, 1080×2400, top/bottom safe area, 360×640 and side-inset safe area.
- Existing layout/readability checks, integer physical scale for each new sprite, Point sampling, camera coverage, no playable-grid overlap, continuous chains, column/base joins, candle contact, visible HP/intents/gems, Guide safe-area access, Settings, accepted gem swap pass.
- Six frozen before/after image pairs: **zero changed pixels outside the side architecture**. Existing upper HUD/battle, board and bottom controls are pixel-identical.
- 18 editable/native pairs match, including opaque pixels and binary alpha. Production PNGs match exports. Shaft variants have identical alpha and seam rows. No hidden opaque pixels outside native canvases.
- 6,780 protected files match the pre-task snapshot. Sole intentional protected change: Dungeon kit binding. Original artwork and other-zone assets unchanged.
- Required `Tools/Validate-Unity.ps1` **passed**. Test XML and complete validator log in `Diagnostics/`.
- Automated desktop Unity evidence only. **No physical-device test or new Android build for this revision.**

## Review files

- `Review.html`: six gameplay pairs, close-ups and native assets.
- `Previews/BeforeAfter.png`: same-frame Unity comparison.
- `Previews/SideCloseups.png`: left/right gameplay columns.
- `Exports/`, `Native/`: PNGs and editable documents.
- `Diagnostics/After/*-protected-pixels.txt`: exact image counts.
- `Diagnostics/ProtectedAfter.json`, `AssetValidation.json`: preservation/native checks.

Artistic verdict: stronger geometric construction and controlled focal details; the colorful board remains dominant. Original art and fallback presentation preserved. Stop for user review; do not merge automatically.
