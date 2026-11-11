# Verification â€” 2026-11-12

Passed `Tools/Validate-Unity.ps1` using Unity 6000.3.19f1 on the final runtime code.
Final compile log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-bad9013e-c75f-493d-be88-dfb308891294.log`.

Passed `Tools/Test-ReferencePresentation.ps1 -Filter SilverRoyalRelicProductionTests`:
one automated Unity test, zero failures or skipped tests. The test runs the actual
production Game scene, without an editor skin-substitution adapter.

- 20 cross-zone layout captures: Dungeon, Forest, Drowned Court, Ironvein and
  Frostwake at 360Ã—640, 720Ã—1280, 1080Ã—1920 and 1080Ã—2400 with a simulated safe area.
- Three additional character captures: Minotaur, Kobold and Bardley at 720Ã—1280.
- Separate centered character icons; identical approved frame resources across zones.
- Point filtering, native control pixels, integer placement, board clearance,
  Settings/wave separation and potion/ability/bomb spacing.
- Live wave values and long-label fit; visual QA caught and corrected small text
  caused by TMP's line-height metrics before the final passing capture run.
- Actual Settings open/close and pause, bomb target/cancel without spending,
  potion activation/stock decrement and ability activation/energy spending.
- Ready lift/glow, pause freeze and Reduced Motion's stationary ready cue.

`UnityTests.xml` records the final run. `Preview` contains 23 normal Game screenshots
and eight ready-state screenshots. `AllZones.png` is a nearest-neighbor contact
sheet of five actual 720Ã—1280 screenshots, not a repainted mockup.

`SourceIntegrity.json` verifies byte-exact copies of all five supplied PNGs,
lossless native wave-strip reconstruction, uniform repeating rails and the three
existing icon LibreSprite round trips. PixelLab usage for this integration: **0**.

The first test exposed an invisible legacy parent Image being treated as resized
art; the runtime now disables that renderer and uses only the five native child
strips. No layout validator or test assertion was weakened to accept it.

Unity's three unrelated consumable-importer whitespace rewrites and TimeManager
format upgrade were restored after validation. User-edited ASE files, untracked
UserEdits, and unrelated main-checkout changes were preserved.

No Android build or physical-device test was run. The safe-area evidence is simulated
in Unity. Existing energy-bar artwork remains unchanged.

Baseline integrity: of 9,182 pre-existing Unity/configuration files, only the four shared UI scripts and Rattlebones ability definition changed. Every pre-existing artwork, scene, prefab and project configuration file is byte-identical to the starting baseline.
