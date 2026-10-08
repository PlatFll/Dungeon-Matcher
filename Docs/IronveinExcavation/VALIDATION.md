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

## Phase 03

- Initial import/compile: PASS.
- **11 focused tests passed**, no failures/skips, including the eight foundation
  cases and three drill scene cases. Result:
  `.utmp/ForestValidation/9bb9b67e-c6bb-43cb-a0b2-c2c16f3be013.xml` and `.log`.
  Covers simultaneous ordered crossing passes, zero clear/cascade rewards,
  destroyed intersected Obsidian, manual-intake deduplication, saved charge,
  photograph non-refund and paused state.
- Actual 1080×1920 proof captured and visually inspected:
  `.utmp/Ironvein/Captures/phase03-drill-proof.png`. Both machines and charge pips
  fit outside the board, clear of gem hitboxes, right/top sigils and bottom HUD.
  This is a mechanics proof using the explicitly temporary dungeon/Farmer assets,
  not the final cave or a visual approval request.
- Two-stone straight-lane and real accepted-move ordering checks, affected crystal
  and layout regressions: **26 passed, zero failures/skips**.
  `.utmp/ForestValidation/56301d1a-13f0-4245-bdd3-990d3ce82630.xml` and `.log`.
  The real move test confirms actor opportunities precede launch, input ownership
  remains held, and post-launch cascades issue no combat report.
- Required `Tools/Validate-Unity.ps1`: **PASS**, Unity 6000.3.19f1.
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-c5d1e485-af2a-4dd5-925c-ef7ca1fbd6cf.log`.
- Original four user-file hashes still match. No PixelLab generations used.

## Phase 04

- Additive normal-definition import/compile: PASS.
- **Six focused scene tests passed**, zero failures/skips:
  ore refresh/Continue/consumption, complete multi-hit sequence, Packbeetle death
  network and saved non-replay, source-cap restoration, two-stone penetration and
  actor-before-drill ordering. Results:
  `.utmp/ForestValidation/6a3d0be7-5171-41c8-a585-90441d415fcb.xml` and `.log`.
- Required `Tools/Validate-Unity.ps1`: **PASS**, muted Unity 6000.3.19f1.
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-659a0f7a-ad57-43e3-8493-cc9206990e4a.log`.
- PixelLab usage remains zero. Native enemy art and full region acceptance are pending.
