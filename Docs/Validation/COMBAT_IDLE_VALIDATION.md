# Combat idle integration validation

Date: 2026-09-20. Unity 6000.3.19f1. Branch: `codex/idle-animation-family-recolor`, PR #159 (unmerged).

## Delivered behavior

Rattlebones, Farmer, Pan Villager and Bardley use the approved recolored battle idles through their existing definition/presenter hooks. Every loop has nine 130 ms frames and a 1.17 second duration. The final art polish changes only Farmer's lower cheeks in frames 1 and 9 and Pan Villager's rear scarf tail. Rattlebones/Bardley pixels and all other Farmer frames are unchanged in that localized pass.

The source/export canvas remains 64×64. Unity's Bardley slices consistently exclude 12 transparent bottom rows; the PNG bytes and artwork placement remain unchanged. Every character uses fixed slices and bottom-center pivots, with no animated transform or frame-dependent recentering.

The existing player HUD owner now spaces and centers the full character/gem/health-bar stack. The modular bar disables its legacy root Image while rendering visible children; the prior visibility check omitted that bar from the stack. Including it and keeping four logical pixels between containers prevents obscured feet and excessive spacing when player artwork changes.

## Checks actually run

- `validate_idles.py`: PASS for all 36 frames, approved palette membership, binary alpha, dimensions, duration, and exact ASE/PNG/GIF frame equivalence. Report: [Validation.json](../../ArtSource/CombatIdles/Validation.json).
- `validate_turn_based.py`: PASS for the retained pose/blink/ground-contact constraints, allowing only the explicitly authorized cheek contour additions. Report: [TurnBasedValidation.json](../../ArtSource/CombatIdles/TurnBasedValidation.json).
- `validate_final_polish.py`: PASS for exactly 13 changed cheek pixels per Farmer endpoint frame, unchanged Farmer frames 2–8, scarf-tail-only Pan changes, and unchanged Rattlebones/Bardley source/export hashes. Report: [FinalPolishValidation.json](../../ArtSource/CombatIdles/FinalPolishValidation.json).
- Native-size and 4× visual review: full-speed family playback, low pose/blink, recovery, dark/light backgrounds, previous/current views and frame 9→1 loop seam. LibreSprite saved the editable files and all image exports.
- `CombatIdleValidation.Run`: PASS in a graphics-enabled batch editor using the production Game scene, temporary player selection and disposable account profiles. Both players were tested with Farmer and Pan Villager spawned through the existing wave system. Real Animator playback was observed before deterministic sampling of all nine poses at **1080×1920 and 1080×2400**. Checks cover imported source bytes, fixed rectangles/pivots, Point/uncompressed/FullRect import, exact clip timing, sprite-only curves, correct selected poses, stable rectangles, integer physical texel scale, actual opaque ground contact, player foot clearance, and the complete player stack remaining within its panel. Final report: 4,928 assertions including polling checks; exit code 0.
- `Tools/Validate-Unity.ps1`: PASS after the final runtime/editor changes. Log: `DungeonMatcher-UnityValidation-5d12e73b-22b7-4e02-a537-25ea386abeb8.log` in the local temp directory.
- Definition YAML review: only sprite/controller references change in the two player and two enemy definitions. No scene/prefab, balance, damage, shield, board or attack-event changes.

Reproduce the engine check with Unity `-batchmode -projectPath <project> -executeMethod CombatIdleValidation.Run -logFile <log>`. Omit `-nographics` and `-quit`: the helper needs rendered frames and exits after Play Mode. Local reports/screenshots go to `.utmp/CombatIdles/`.

## Render review

Screenshots were visually inspected in addition to numeric checks. Original-resolution evidence:

- [Rattlebones, 1080×1920, frame 1](CombatIdles/skeleton-1920-frame1.png)
- [Rattlebones, 1080×2400, frame 6](CombatIdles/skeleton-2400-frame6.png)
- [Bardley, 1080×1920, frame 6](CombatIdles/bardley-1920-frame6.png)
- [Bardley, 1080×2400, frame 1](CombatIdles/bardley-2400-frame1.png)

## Limits and existing diagnostic

The broader layout validator reports a pre-existing fractional Y position on Bardley's bottom-HUD ability icon (`AbilityButton/AbilityIcon`, y=71.60 at 1080×1920). The test reproduced that same warning after replacing the new idle with the old static Bardley artwork. Only that specific diagnostic is allowed; other layout errors fail the run. The ability icon is outside the character-animation placement change and remains unchanged.

This is desktop Unity Play Mode coverage at two portrait sizes, not a physical-phone build or a new full gameplay/balance regression run. Numeric checks establish placement and export integrity; artistic approval remains a visual judgment.
