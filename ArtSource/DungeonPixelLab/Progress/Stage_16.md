# Stage 16 — playable Dungeon integration review

The user approved the Stage15 library and visual design for integration testing on 2026-10-20. The implementation remains on `feature/dungeon-zone-pixellab-asset-library`, draft PR #203. **No merge or production visual acceptance is implied.**

## Delivered

- The real `Game` scene selects the 102-asset PixelLab library through a Dungeon-only resource binding and existing presentation owners. It does not instantiate the art-only scene or draw a flattened mockup. The previous integration and all original GUIDs remain available.
- Continuous walls, extended supports, flat platform/fascia, gate, props, masked portrait backing and dynamic corner/rail/clamp frames use the approved native PNGs. Repeated surfaces extend to fit the viewport rather than being stretched. The extra right corbel removed in Stage15 stays absent.
- Original 64×64 gems remain unchanged. Following the user's correction, LibreSprite produced an additional **64×64 board-cell variant** with the same six colors, corner pixels and bevel widths as the approved 35px preview cell. Its editable source, export and build script are supplied; no source resampling or generation was used.
- The board retains the existing responsive uniform fit and gameplay coordinates. Its frame assembles around the live board. At physical 360×640, the existing uniform board downsampling policy is retained by explicit user instruction: 512 native board pixels cannot be shown one-for-one in 360 physical pixels. This exception applies to board rendering, not unequal X/Y scaling of scenery.
- Environment and frame source pixels are integer-scaled (1×/2×/3× at the requested widths). Native characters retain their authored floor registration and are positioned above their live health bars. The compact Dungeon layout fits where the earlier layout calculation failed. Plaque text, Guide modal, compact HP labels and resized enemy-slot geometry were corrected without changing gameplay.
- Existing buttons, ability icons, energy, stock/cooldowns, player shield, enemy intents/weaknesses and run ownership remain authoritative. Gameplay, damage, RNG, saves and other-zone art are not replaced.

## Actual verification

- `FinalTestResults.xml`: **71 passed, 0 failed, 0 skipped**. Includes real Dungeon gameplay captures, three-zone geometry/input checks, every playable ability symbol, accepted moves, pointer identity, crystal validity, unified combat, save/resume, Bomb/Bardley recovery, and zone environment tests. Logs were not suppressed in the new Stage16 fixture.
- Actual Game-scene screenshots: **720×1280, 1080×1920, 1080×2400, 1080×2400 with safe rect (0,84,1080,2200), and 360×640**. Each has Rattlebones, 64 original gems, three live enemies, health bars, intent counters and new environment. The disposable test fixture adds Pan Villager and Knight through the existing wave summoning API to exercise all slots; it does not change encounter data. Random weakness types are live game assignments.
- Every capture passes the layout validator: native environment/frame ratios, section separation, pixel bounds, portrait clipping, active HP/intent presentation and one-line HP labels. Guide opens and Resume remains inside the safe area at every size. An accepted swap increments exactly one move; Settings opens/closes. The first-run lesson temporarily occupies the resonance-caption area in these real gameplay captures, as intended by existing UI behavior.
- `Tools/Validate-Unity.ps1`: **passed**, Unity 6000.3.19f1. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-f0c992ed-0b5f-43ef-b638-8b464e52d873.log`.
- Android build: **passed**: IL2CPP development APK, existing enabled production scenes, local debug signing, zero build errors and seven warnings (six existing obsolete/unused C# warnings plus Android diagnostics-symbol configuration). APK is about 1.44 GB; this is a build smoke check, not a release-size optimization or phone test. The local APK path and build duration are recorded in AndroidBuild.json.
- Automated editor testing and a local Android build are distinct from real-device testing. **No physical phone test was performed.**

## Preservation and visual assessment

`Preservation.json` verifies all **456 approved source/export/import/metadata files** unchanged; the user's eight existing main-checkout changes remain intact. The Game scene and build-scene list retain their serialized bindings. No gems, character sprites, animations or approved art were regenerated/recolored. Stage16 spent zero PixelLab credits; the pack's 65-generation total is unchanged.

Compared with the Stage15 art-only assembly, the same cool masonry, warm localized torch accents, native metal frames, separate portrait recess, gate and flat platform remain recognizable. Live responsive section dimensions intentionally differ from the small art-only composition. Tall screens reveal more repeated wall; the board keeps its familiar physical footprint. The frame fits rather than stretching its corners. Native review found no new clipped props or gaps in the assembled platform/frame. Final artistic and gameplay approval belongs to the user.

## Evidence and reproduction

- [Actual gameplay gallery](../Stage16Review.html), with the approved art-only composition alongside.
- [720×1280](../Previews/Stage16/Dungeon-720x1280.png), [1080×1920](../Previews/Stage16/Dungeon-1080x1920.png), [1080×2400](../Previews/Stage16/Dungeon-1080x2400.png), [safe area](../Previews/Stage16/Dungeon-safe-1080x2400.png), [360×640](../Previews/Stage16/Dungeon-360x640.png).
- [Final test result](../Diagnostics/Stage16/FinalTestResults.xml), [preservation audit](../Diagnostics/Stage16/Preservation.json), [Android report](../Diagnostics/Stage16/AndroidBuild.json).
- Unity edit-mode test filter `DungeonPixelPipelineStage16Tests.DungeonCombatAndPhoneScreens` reproduces the live capture fixture. The suite also runs the named existing tests above. The historical `CaptureProductionBaseline` test must remain excluded: it requires the pre-integration revision and must never overwrite approved baselines.
- `Scripts/BuildCellStage16.ps1` uses the installed LibreSprite native scripting API and reopens/exports the saved 64px tile. Old iteration logs remain local under `Diagnostics/Stage16`, excluded from the PR; final XML, screenshots and concise reports are included.

**Stopped for final gameplay visual approval. Do not merge without a new explicit user instruction.**
