# Bomb readability and VFX review — 2026-11-12

Branch: `codex/bomb-readability-and-vfx`. Review-only casing selection; no merge.

## Review choices

- Poison A–D, Healing A–D, Directional A–D (paired row/column orientation studies).
- White Star A–C. Star B is used for the gameplay review; source/prefab GUID retained.
- Socket centers are explicit in `Candidates/selection.json`. Each casing has a 23px dark cavity for a separate unchanged-size 16px gem. Shape/lighting finishing for the selected column counterpart can follow selection.
- ShieldBomb.png remains SHA-256 `3992f944830f2827dc16fd105695fe4112c0c5c1953d5bd6e467523e8ab49d40`.

The other casing PNGs are deliberately not bound to the production prefabs yet. The current casings remain visible in gameplay captures until the user chooses replacements.

## Completed palette and VFX revision

Twelve small gem variants use the corresponding current board-gem facet colors. Original geometry/alpha and weakness gold rims are exact. The six 64px master gems and Rattlebones are unchanged. `Palette/Manifest.json` records every map and preservation hash; original files and exact LibreSprite round trips are retained.

Row/Column presentation uses two animated native flame fronts and tiled fire trails. The existing board SpriteMask clips them at the grid boundary. No generic per-cell explosion is emitted for the directional-only footprint. Utility bombs, consumable/Bardley bursts, damage, rewards, targets and refill timing are preserved. A real-play test caught and fixed the previous tile-library suppression of the dedicated directional event.

Color Crystal stars snapshot the source and destination, travel on the native pixel grid, land before the existing flash, then shrink through native frames. Board resolution still owns every removal. Reduced Motion disables star travel and moving flame fronts. Pools clean up on disable; Time.deltaTime respects pause.

## Evidence

- Mandatory `Tools/Validate-Unity.ps1`: passed Unity 6000.3.19f1 Final log: `DungeonMatcher-UnityValidation-29651482-cdec-4ff9-90fc-c49f78e537bf.log`.
- 81 focused automated checks passed, zero failures/skips: bomb visuals, actual swaps, tile bursts, crystal chaining/validity, Board Feel.
- After the final flame-art polish, all 7 affected native/presentation/play checks passed again; this is repeat coverage, not 7 additional unique tests.
- `.utmp/ThreeZonePresentation/reference-tests-20261112-064514.xml`: 81/81.
- `.utmp/ThreeZonePresentation/reference-tests-20261112-064850.xml`: 7/7 final art capture.
- Actual Game scene at 720×1280, disposable account: 100 frames each for row, column and Color Crystal swaps. The review GIFs are nearest-neighbor half-size and deliberately slowed; full native screenshots are alongside them.
- Native PNG checks: 39 new candidate/effect exports, binary alpha; 12 preserved gem masks; unchanged masters/Shield/Rattlebones.
- LibreSprite: exact PNG round trips for candidate stills and recolors; real multi-frame ASE sources for three VFX sequences, with exact pixel comparison after export.
- No real-device or Android build test for this review. Casing approval remains pending.

## PixelLab usage

Four native 64×64 Pro jobs, 16 raw candidates each, 10 generations each: **40 generations total**. Balance 3667.90 → 3627.90; no cash credits spent. Approved Rattlebones and Shield images were the actual supported image inputs. Job IDs, costs and source hashes are in `Candidates/jobs.json`. No additional provider was used.

## Sources

`Candidates/Raw` is immutable provider output. `Candidates/Editable`, `Palette/Editable` and `Effects/Editable` contain LibreSprite sources. `Scripts` contains native drawing/recolor recipes; update the documented local LibreSprite executable path if running on another machine. New casing PNGs remain candidates, not approved production replacements.
