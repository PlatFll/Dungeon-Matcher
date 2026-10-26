# Approved Drowned Court production integration

The user approved Revision04 for the actual playable underwater zone and explicitly authorized merging PR #212 on 2026-10-26.

The build-enabled `Game` scene now selects 94 approved separate environment modules and 15 cosmetic motion tracks through `CourtAssembly`. The existing presentation owner handles the native scenery, clipped portrait, responsive frame and floor. The old Court tilemap/reference-kit scenery is bypassed at runtime. Original assets and review scenes remain available.

Clam hinge motion, two subtle anchored coral sways and twelve bubble tracks use the exact approved sampled Unity poses. They obey pause and Reduced Motion. No new generation, recoloring, resampling or production character changes were made; this integration spends zero PixelLab generations. Pack total remains 107.

## Verification

- `Tools/Validate-Unity.ps1`: passed, Unity 6000.3.19f1.
- Actual Game Play Mode: 25 cases across all five zones at 720×1280, 1080×1920, 1080×2400, 1080×2400 with inset safe area, and 360×640; zero layout-validator errors.
- Court before/after board and portrait rectangles, HP rows, floor anchors, shared button rectangles/sprite bindings and seeded decorative cell sources match exactly. Cross-zone board dimensions agree within 0.001 physical pixel (float matrix precision); portrait sizes match exactly.
- Existing `SharedEnemyFloorTests` passed: 75 actual sprite-pivot floor measurements with original Dungeon baseline and full board/portrait geometry checks.
- Accepted gem swap, Guide, Settings, motion, pause and Reduced Motion passed.
- Seven AIR-state unit tests plus dry-first-encounter and final-drain checks passed.
- Focused production wet resume passed: exact board carryover, AIR=2, wetMoves=12, preserved active channel deadline, new art remains active.
- 18 Dungeon/Court/Forest handoffs passed: board, run ID, HP/energy and saved state preserved; hazards cleared; exactly one selected environment; per-zone environment module counts remain constant; account remains below 500 KB.
- Android development APK succeeded: IL2CPP, 0 errors, 25 warnings, 1,441,269,298 bytes. APK path is recorded in `AndroidBuild.json`. No physical-phone execution claimed.
- 5,553 pre-existing art, animation, scene, zone-data and project-settings files match their baseline hashes. Main-checkout local changes are preserved separately during synchronization.

## Existing limitations, not suppressed

Two older tests fail identically with the new Court binding present and disconnected. `CourtWetResumePreservesAirAndChannelDeadline` assumes four moves leave a two-move Moray warning; current behavior has one remaining. `CourtThreeZoneSoakCleansEffectsPreservesRunAndBoundsSaveSize` compares Dungeon's 848 objects to Forest's count plus 250 (814), rather than comparing repeated visits to the same zone. Those tests are unchanged; complete failed results remain in `Gameplay.xml` and `LegacyGameplay.xml`. New focused tests verify the current resume contract and exact environment cleanup without changing gameplay or those legacy assertions.

360×640 retains the established uniformly reduced board and compact shared controls. Its editor/development screenshot includes Unity's pre-existing below-reference-resolution warning (camera reference remains 540×960); normal release builds do not draw that warning. This work does not claim that the shared low-resolution HUD was redesigned.

## Evidence and reproduction

Open `Review.html` for actual Game before/after screens, safe-area cases and flooded resume. `Verification.json`, test XML, native placement ledger and measurement JSON contain the detailed results. `BeforeInitial/` retains the independent original pre-hook capture; `Before/` uses the same gameplay source with the optional Court resource disconnected and a fixed cosmetic seed on both sides. `SharedFloor/` contains the latest source-pivot evidence; older Revision03 evidence remains immutable.

Export with `DrownedCourtProductionExporter.Export`. Run explicit `DrownedCourtProductionTests.VerifyProduction`, `ForestFoundationPlayTests.CourtProductionWetResumePreservesCurrentDeadlineAndArt`, `ForestFoundationPlayTests.CourtProductionTravelPreservesStateAndReleasesEnvironment` and `SharedEnemyFloorTests.AllZonesShareTheApprovedDungeonFloor`. Android build entry point: `DrownedCourtProductionBuild.Android`.
