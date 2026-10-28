# Ironvein corner stations — review candidate

Only the lower-left and lower-right environment dressing is replaced. The actual Game scene continues to use `IronveinBattleAssembly` and `DungeonPresentationArt`. No production C# changed.

Left: a large ore-storage crate rests on a short track over a connected rock foundation, with a small embedded ore fragment at its foot. Right: an iron-braced timber support, leaning pickaxe, closed supply crate and restrained ore crevice share one rock base. The upright has a native LibreSprite extension and anchor to connect it to the side structure. Nine separate sprites are placed; ten cleaned sources include the original shorter brace.

## Preservation

- Exactly five previous lower-corner placements were replaced; 69 other modular placements are byte-identical.
- 2,905 other protected production files match the starting SHA-256 snapshot. Existing art, characters, gems, scenes, other zones and shared HUD assets are unchanged.
- No generation changed the top mine vista, portrait, board, drills, buttons, resonance panel or text. No gameplay, combat, save or layout runtime code changed.
- The pending PR #216 drill foreground fix is retained as this branch's base. This revision does not merge it or change its behavior.
- Prior approved sprite files and every raw candidate remain intact. These new corners are awaiting user visual approval.

## Native art validation

PixelLab generated each modular prop from text alone. The supplied mockup was used for composition review, never cropped into production art. Nine Pro calls used **90 subscription generations**, from **154.20 to 64.20**, with **$0.00 paid credit spend** and no reference-image uploads. See [the ledger](GenerationLedger.md).

LibreSprite opened all selected candidates, consolidated their material ramps and removed isolated interior texture flecks. Exports have 7–12 opaque colors per original part, binary alpha, Point filtering, no mipmaps, no compression and full-rect sprites. Native dimensions range from 32×32 to 64×56, with the support expanded from 48×72 to 48×96 through new native pixels, not scaling. All `.ase` files were reopened and their PNG round trips match exactly. The unextended sources retain their original alpha masks. One foundation intentionally reaches its canvas edge to join the cavern base; no source pixels were cropped or resampled.

## Actual Unity verification

Two automated Play Mode capture checks launched the production **Game** scene using a disposable save and three live Ironvein enemies. They passed at **720×1280, 1080×1920, 1080×2400, 1080×2400 with safe-area insets, and 360×640**. They verify identical board and portrait rectangles, enemy floor lines and health bars, shared control sprites/positions, six retained drills and foreground sorting, plus native uniform scale, screen bounds and non-interactive decoration.

The before/after images are separate game launches. Idle frames, assigned weakness gems and ambient animation may differ; these are not art replacements. Protected-file hashes and layout assertions are the preservation evidence. Final captures use ordinary production countdowns, not the earlier mechanic-isolation fixture.

The required `Tools/Validate-Unity.ps1` result is recorded in `QA/Compilation.txt`. No Android build or physical-device test is claimed for this art-only placement revision.

## Review and editable files

- [Interactive gallery](Review.html): before/after crops, all five phone captures and native assets.
- `Native/*.ase`: editable LibreSprite files.
- `Exports/*.png`: exact imported native PNGs.
- `Raw/`: all PixelLab candidate PNGs and receipts.
- `Selected/Selection.json`: chosen indices and rejection reasons.
- `Scripts/`: native cleanup, timber extension, scoped import and gallery reproduction.
- `QA/`: reference hashes, palette/alpha checks, preservation proof and Unity test reports.

Work remains on `codex/ironvein-bottom-corner-stations`. **Do not merge until the user approves the corner visuals.**
