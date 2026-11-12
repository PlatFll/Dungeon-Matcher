# Final integration verification

- `Tools/Validate-Unity.ps1`: passed Unity 6000.3.19f1 after the final source-size
  mapping changes. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-ae1229f3-7b61-4019-8365-d69f76399288.log`.
- `Tools/Test-ReferencePresentation.ps1 -Filter 'SilverRoyalRelicProductionTests;AbilityReadinessTests'`:
  **4 passed, 0 failed, 0 skipped**. Result preserved in QA/UnityTests.xml;
  original result/log `.utmp/ThreeZonePresentation/reference-tests-20261112-050956.*`.
- Real Game scene, five zones at 360x640, 720x1280, 1080x1920 and 1080x2400
  with simulated safe insets; all four characters, WAVE 1/WAVE 99999 labels,
  centered icons/counts, square Settings, caption/board clearance and joined rails.
- Settings/pause, potion use, bomb selection/cancel, ability activation/spending,
  white charging/color-ready pop-out, Reduced Motion, visible fill at charge
  checkpoints and ordinary-match energy gain all passed.
- Updated source dimensions are asserted explicitly. Existing logical-size
  checks are retained; full-size art is native and compact art uniformly fits
  at two thirds. The old half-pixel wave-vertex allowance is removed: all joins
  now require whole physical vertices. No tests skipped or weakened.
- Exact user PNG/export hashes, unchanged imports/GUIDs, eleven other shared
  PNG hashes and lossless wave-strip reassembly checked after Unity execution.
- No PixelLab generation, Android build or physical-device testing this task.

Preview contains actual Game screenshots for all five zones at 720x1280.
AllFiveZones is a labeled half-size overview; individual PNGs retain the full
capture resolution. ControlsDetail contains unscaled screenshot crops.
The complete 20-layout review is available locally at
`http://127.0.0.1:57126/FinalUserEdits/Review.html`.

Earlier source/evidence remains historical. This revision is the latest user
approval and source authority; approved for merge on 2026-11-12.
