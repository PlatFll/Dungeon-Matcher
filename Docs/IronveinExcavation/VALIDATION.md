# Ironvein validation

Unity 6000.3.19f1; Editor audio muted. These are executed checks, not blanket
approval of the whole region. Art/music review and device testing remain pending.

## Phase 02

- Foundation import/compile: PASS, exit 0.
  `.utmp/Ironvein/foundation-import.log`.
- Focused EditMode + scene tests: **9 passed, 0 failed/skipped**.
  Filter: `IronveinFoundationTests;IronveinStones;IronveinStoneHit;IronveinAccepted;ZonePickerCancelCreatesNoRunAndFitsPortraitScreens`.
  `.utmp/ForestValidation/70c168e3-d155-4b2c-947c-9adc75653233.xml` and `.log`.
  Covers birth/duplicate action/aging/terminal tier, hit reset, detached state,
  future versions, testing-only zone, placement caps/special preservation/useful
  response, hit deduplication, material retention, photograph permanence,
  accepted moves/continue, detached travel cleanup and picker layout.
- Compatibility suite: **28 passed, 0 failed/skipped**.
  Filter: `ChronoShutterTests;ZoneTravelTests;BarricadeBannerGravityTests;DesignV2ContinuationTests`.
  `.utmp/ForestValidation/67af6a04-0c73-475f-8a3f-a99182ddbe8f.xml` and `.log`.
  Includes actual Gideon and combat continuation scene tests, barricade/banner
  gravity, and the `ZoneTravelTests` policy tests. Other travel scene tests belong
  to the shared fixture class and were not selected by this filter.
- Required `powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1`:
  **PASS**, Unity 6000.3.19f1. Log:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-f16060ab-3180-4aec-95f3-6e713ff3e9d0.log`.
- Four original user files match their starting SHA256 hashes. Unity's unrelated
  TimeManager serialization rewrite was inspected and removed after the jobs ended.
- Full region gameplay, native art, all four shapes, pacing and device acceptance:
  NOT EXECUTED at this foundation checkpoint.
