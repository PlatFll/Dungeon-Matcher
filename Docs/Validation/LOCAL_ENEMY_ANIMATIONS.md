# Miner and local-villager animation validation — 2026-09-21

Scope: Miner idle/side-swing attack/mining ability; Basket Villager (berries farmer) idle/berry-release attack; Barricade Villager idle/axe attack/kneeling build. The original four-character art is unchanged.

## Follow-up movement refinement — 2026-09-21

The first draft's largely translated ready poses did not supply enough head, eye, shoulder and torso articulation. The three current idles use cleaned poses from the user's newer LibreSprite drafts. Builder attack anticipation/travel/follow-through was revised; the build ability correction is restricted to lower-leg pixels in frames 3–7. First/last ready poses, palettes, exposures, dimensions and Unity event/controller/import settings remain unchanged. No runtime C# changed in this refinement.

Executed on the revised art:

- `Scripts/validate.py`: **PASS, 71 frames**, exact native/PNG/GIF pixels and durations, approved palettes, binary alpha, margins, ground contact and ready-pose continuity.
- `Scripts/check_refinement.py`: **PASS**. Idle bottom two rows are pixel-identical throughout each loop; first poses match the earlier reference PNGs; all eight Unity sheets exactly match exports. Builder ability differences stay within x=42–63, y=58–63, only frames 3–7; upper-body, timber and left contact pixels remain intact. Detailed per-frame bounds are in `ArtSource/LocalEnemies/RefinementValidation.json`.
- `LocalEnemyAnimationValidation.Run`: **PASS / exit 0**, including actual impact damage, mining/build effects, simultaneous readiness, pause/fallback/cancellation/recovery and the 14 attack lifecycle cases. Log: `.utmp/LocalEnemies/refinement-play.log`; report preserved as `refinement-full-validation.txt`.
- After the final knee-boundary cleanup, `RunAlignmentOnly`: **PASS / exit 0**, all 71 final poses at 1080×1920 and 1080×2400. Log: `.utmp/LocalEnemies/refinement-alignment-final.log`; report: `alignment-validation.txt`. This run validates final artwork placement; it does not rerun the combat cases above. An earlier attempt failed because the editor started with a transient 321×531 Game view; it is preserved in `refinement-alignment-startup-viewport.log`. The successful rerun specified `-screen-width 1080 -screen-height 1920`, and the harness exercised both required portrait sizes without changing or suppressing checks.
- Side-by-side native-size playback against Rattlebones/Farmer/Pan, enlarged contact sheets and final Unity renders were inspected. The final source captures and their hashes live in `Originals/IdleRefinement/`. Earlier supplied source hashes remain intact.

The required validator, 68-test code regression and original-four idle checks listed below belong to the initial runtime integration. They were not repeated for this art-only refinement, which changes no C#, serialized settings, clips or controllers. Physical Android testing remains unperformed. The current guide is v1.3.1.

## Sources and timing

All eight editable LibreSprite files, exact PNG sheets, GIFs, JSON exposures, ready-pose references and palette data are in `ArtSource/LocalEnemies/`. Supplied originals are preserved separately. `AssetManifest.json` and `Originals/Manifest.json` record SHA-256 provenance. The original Miner attack PNG and ASE represent different drawing revisions; the native ASE supplied the motion used in this pass.

| Action | Frames | Exposures in milliseconds | Contact |
| --- | --- | --- | --- |
| Idle | 9 | 130 × 9 | None; 1170 ms loop |
| Auto attack | 8 | 80, 80, 120, 40, 120, 80, 80, 80 | Frame 5, 320 ms |
| Enemy ability | 10 | 80, 80, 120, 80, 120, 80, 80, 80, 80, 80 | Frame 5, 360 ms |

Attack recovery completes at 670 ms and the clip ends at 680 ms. Ability recovery completes at 870 ms and the clip ends at 880 ms. Miner uses 96×80 in all states; villagers use 64×64 idles and 96×64 actions. The Miner deliberately leaves the floor in ability frame 4, with tool contact on frame 5. Basket Villager's berry stays in the hand before release and disappears; no travelling projectile is authored.

## Executed art review

`Scripts/validate.py`: **PASS, all 71 frames**. Native cels and PNG/GIF pixels agree exactly, as do durations. Checks cover approved palette membership, binary alpha, fixed dimensions, side clearance, ground contact (with the deliberate airborne exception), and matching action/idle ready poses. All eight Unity PNG sheets are byte-identical to their native exports. The three reference PNGs match idle frame 1 exactly. Original hashes remain intact.

LibreSprite native editing and final transparent-layer conversion were followed by contact-sheet inspection, synchronized native-size and enlarged browser playback, and Unity scene renders. Review covered faces following the body, shared compact bop, cool iron/wood separation, hand/tool contact, berry release, kneeling plank contact, silhouette clearance and recovery. Palette membership alone does not prove good material placement; the rendered review is separate evidence.

## Executed Unity checks

Unity **6000.3.19f1**, graphics-enabled production `Game` scene, disposable account profiles:

- `LocalEnemyAnimationValidation.Run`: **PASS**. Actual attack damage and mining/build effects observe frame 5, not the starting pose. Duplicate events do not resolve twice; recovery remains owned and returns to idle.
- Pause longer than the three-second fallback threshold freezes presentation, impact and timeout. Missing animation resolves through the bounded board/attack fallback. Disabling and re-enabling an owner cancels pending contact without a delayed structure or leaked lock.
- Miner and builder becoming ready in the same update: **PASS**. The second retains its charge until the board settles, then plays its own ability and applies its effect on its own frame 5. Each resolves once.
- The existing **14 enemy attack lifecycle scenarios** pass inside the production-scene run.
- `CombatIdleValidation.Run`: **PASS** after the shared UI height fix. The original Rattlebones, Bardley, Farmer and Pan Villager loops retain live playback, all nine poses, stable rectangles, integer pixels and grounded contacts at both portrait sizes. Log: `.utmp/LocalEnemies/original-idles-regression.log`; report: `.utmp/CombatIdles/play-validation.txt`.
- All **71 poses** at **1080×1920 and 1080×2400** pass fixed horizontal center, unchanged integer source-texel scale, fixed floor through state changes, portrait/mask containment, and health-bar clearance. Cross-character floor differences are below 0.1 physical pixel at each size. The final three floors are 1577 px at 1920 height and 1729 px at 2400 height (Unity bottom-origin coordinates).
- Focused NUnit Edit Mode regression: **68 passed, 0 failed, 0 skipped** across `EnemySpecialExecutionGuardTests`, `BarricadeBannerGravityTests`, `EnemyDamageResultTests`, and `RoyalAssaultParticipantLifetimeTests`.
- Required `Tools/Validate-Unity.ps1`: **PASS / exit 0** after the final runtime change. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-3aa9f3d6-45e5-4685-a8fc-dfc316ecc6b9.log`.

The screen review exposed a real taller-canvas problem: bottom-center sprite metadata did not prevent a center-pivoted UI Image from growing below its established floor. The final fix compensates height on the actor root, preserving the separate spawn-anchor/layout and VisualRoot/feedback owners. Earlier attempted offsets failed the tall-portrait check or health-bar review; they were replaced, and the complete production run passed afterward. The final checks explicitly cover both shared floor and visible foot clearance.

Final scene examples: [standard portrait](LocalEnemyAnimations/family-1920.png) and [tall portrait](LocalEnemyAnimations/family-2400.png). They show the existing Rattlebones presentation beside the new enemies; his panel retains its established independent layout.

## Reproduction and limits

Run sequentially with the project closed in other Unity processes:

```powershell
rtk powershell -ExecutionPolicy Bypass -File ArtSource/LocalEnemies/Scripts/export.ps1
rtk python ArtSource/LocalEnemies/Scripts/validate.py
rtk proxy '<Unity 6000.3.19f1>/Editor/Unity.exe' -batchmode -projectPath '<project>' -executeMethod LocalEnemyAnimationValidation.Run -logFile '<log>'
rtk powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1
```

Use `CombatActionImporter.ImportLocalEnemies` when importing changed art. `LocalEnemyAnimationValidation.ImportAndRun` imports and performs the same full scene check. `RunAlignmentOnly` is available for targeted layout iteration. Graphics runs omit `-quit`/`-nographics`; the harness exits after its checks. The focused NUnit command uses `-runTests -testPlatform EditMode` with the four fixture names above separated by semicolons, plus `-testResults` and `-logFile` paths.

Local full logs, XML and all contact renders remain in `.utmp/LocalEnemies/` (`unity-play-final.log`, `play-validation.txt`, `asset-validation.txt`, `ground-coordinates.txt`, `editmode.log`, `editmode.xml`). The source validation report is `ArtSource/LocalEnemies/Validation.json`. The preview can be served locally from `ArtSource/LocalEnemies/Preview.html`.

These are automated Unity Editor and rendered-art checks, not physical Android/device testing or human playtest approval. Pre-existing startup pixel-layout diagnostics, including the fractional AbilityIcon warning, remain in the local diagnostic log; the new character checks run after layout settles. Production captures disable optional damage-flash feedback after combat tests to show the base palettes. HP, shield, damage values, special cadence, target legality, structure caps and environmental clear/reward rules are unchanged.
