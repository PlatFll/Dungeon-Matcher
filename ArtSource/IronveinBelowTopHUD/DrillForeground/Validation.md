# Ironvein drill foreground correction

The Ironvein board rim used an overlay Canvas, which always covered world SpriteRenderers. Drills already rendered above gems on Effects/140, but no sprite sorting order could bring them above that overlay rim. The frame now uses a nested world Canvas at Effects/100; dispenser hardware, loaded bits, projectiles and bursts render above it. The existing live HUD remains on the overlay.

Only render ownership and coordinate mapping changed. No source artwork, scene asset, board cell, gem, drill timing, board mutation or other-zone presentation changed. Native frame dimensions and placement are preserved across the five tested layouts, including integer native piece scale and raycast-disabled artwork.

## Evidence

Actual Game scene captures, not an additive art-only assembly. The still regression places a real native projectile at each frame crossing with time frozen, then compares screenshot pixels with that renderer enabled/disabled. Random gem arrangements differ between baseline and revised captures. The animation runs two real production drill activations; enemies are held at long countdowns to isolate the drill sequence.

| Layout | Visible rim pixels before (L / R) | Visible rim pixels after (L / R) | Frame bounds |
|---|---:|---:|---|
| 720x1280 | 0 / 0 | 700 / 716 | unchanged |
| 1080x1920 | 0 / 0 | 1536 / 1512 | unchanged |
| 1080x2400 | 0 / 0 | 1554 / 1530 | unchanged |
| 1080x2400-safe | 0 / 0 | 1493 / 1517 | unchanged |
| 360x640 | 0 / 0 | 154 / 163 | unchanged |

Passed Unity tests:

- `IronveinDrillForegroundBaseline` — captured the original occlusion at both edges in five layouts.
- `IronveinDrillForegroundVisible` — verifies rendered pixels over both rails, native scaling, exact frame bounds, and gameplay layout validation in five layouts.
- `IronveinDrillForegroundMotionPreview` — actual Game animation, 135 captured frames.
- `IronveinLeftDrillBoresBeforeEntryButLaunchShattersImmediately`.
- `IronveinRightDrillBoresBeforeEntryButLaunchShattersImmediately`.
- `IronveinSixDrillGameplayDoesNotDependOnTheView`.

Results in `.utmp/ForestValidation/`: baseline `417b39e1-3f71-41ef-8a8a-3d795fb3c5b4.xml`; corrected visibility `063949fd-4eab-435a-afeb-033467282fb8.xml`; motion and gameplay checks `bdd65688-b2ee-46db-ae3e-464898031eb0.xml`.

`Tools/Validate-Unity.ps1` completed successfully using Unity 6000.3.19f1. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-683c65c2-8982-40eb-966b-cfbc815f2183.log`.

An initial coordinate-conversion attempt moved the frame by one screen pixel; the strict regression rejected it. Screen-space rounding before world conversion fixed that drift; the unchanged bounds check now passes without relaxing its tolerance.

Desktop Unity runtime and rendered-frame verification only. No new Android build or real-device test for this correction. No PixelLab generations or artwork edits. Not merged.
