# Approved Ironvein production integration — 2026-10-28

The approved Revision04 environment and six row drills now run in the build-enabled Game scene. The user authorized integration and merge of PR #214/#215. The review scene itself is never instantiated in production.

## Actual-game evidence

- [Before/after, underwater verification and playback](Review.html)
- [Three live enemies and the approved mine environment](Game-ThreeEnemies.png)
- [Recorded drill playback](Game-DrillPlayback.gif); timings and scene provenance: [Motion/Evidence.json](Motion/Evidence.json)
- [Measurements for all 25 final cases](After/Measurements.json)
- [Machine-readable verification and tested source hashes](Verification.json)

All captures are from Unity's actual Game scene with real actors, gems and controls. GIF is a palette-limited viewing copy; PNGs preserve full captured color. Raw 149-frame motion captures remain locally under Motion, excluded from Git; the GIF and timing record are committed.

## Production binding

`IronveinBattleAssembly` references 74 approved modular environment placements, responsive native frame pieces, the masked cave portrait recess and approved shared control art. Existing presentation owners place them around the unchanged live board and common enemy floor. The walking surface spans behind/in front of the feet and the separator is below it. Legacy scenery/enclosures are bypassed; no old source artwork is deleted. Six real MineEnvironmentView dispensers replace the static review mounts.

The first row cell breaks on launch. Subsequent occupied cells hold the spinning bit at contact, break, then allow entry. The socket remains empty through the terminal burst and then reloads. BoardController retains lock, charge, stone, mutation, unrewarded clear, deduplication and final-settlement authority. Saved legacy mine charges migrate without loss.

Source artwork, cells, gems and review scenes are preserved: **4,641 protected existing files hash-identical**. No PixelLab generations or image credits spent on integration. The approved shared control family and player rendering size replace old Ironvein visual bindings. Compact health labels are single-line and the optional compact caption directs the player to Guide without covering the buttons.

## Executed verification

- Unity 6000.3.19f1: `Tools/Validate-Unity.ps1` passed. Log recorded in Verification.json.
- Actual Game: five zones × 720×1280, 1080×1920, 1080×2400, 1080×2400 safe inset and 360×640; **25 cases passed**, no layout errors. Exact before/after board/portrait rectangles, HP positions, enemy floor lines and seeded cell art retained. Other-zone controls retained. Native source pixel scale, masks, six muzzle rows, Guide/Settings pause and accepted swaps verified.
- Fifteen affected tests cover integration, six-drill migration/intake, both contact directions, immediate first-cell shatter, stationary drilling before destruction/entry, stone, pause/reload, no-view fallback, overlaps/Obsidian, boss response windows and seven actual zone handoffs. Initial batch: 15 passed. After the compact-text correction, 14 mechanics/presentation cases passed; a camera-reference experiment failed the strict float-exact geometry check and was reverted. The final integration test **0009d679-7d80-4208-ba2c-e072b75cae6e** passes with the original assertions, plus single-line/no-clipping caption checks. All XML records are retained in Validation.
- Android **IL2CPP development APK succeeded**, 0 errors, 7 warnings. [Build report](AndroidBuild.json). APK is an uncommitted local validation artifact.
- Drowned Court was already merged through **PR #212**, commit `10445532`. Its approved 94-piece art and 15 ambient tracks are confirmed in the real Game scene; no underwater art replacement was needed.

No physical-device execution is claimed. The pre-existing 360×640 below-reference-resolution editor/development warning remains; board fit is the established uniform responsive policy. Earlier art-only reviews are historical and do not describe the current production bindings.
