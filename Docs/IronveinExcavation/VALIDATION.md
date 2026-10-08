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

## Phase 05

- Specialist import/compile: PASS. Initial focused run: **20 passed**, zero
  failures/skips (`46ced193-43f4-4195-b46f-f8b6da2d34df.xml/.log`).
- Final affected run after cancellation/Assay race fixes: **53 passed**, zero
  failures/skips. Includes all Ironvein tests, player statuses, caster sigil
  helpers, shared barricade gravity and combat continuation checks. Result:
  `.utmp/ForestValidation/11664bf1-69a3-4082-af35-8cae1dbe787b.xml` and `.log`.
  The file-name-only `CourtRosterRevision` filter did not select Court scene
  methods; do not treat this run as the complete Court regression.
- Proves first-stone stop even on break, protected crossed specials, exact bomb
  durability on all three materials, safe defusing, saved fuse/one player packet,
  saved Bore warning, support/placement/Assay/track effects, Rattled expiration
  and persistence, actual Stagger/death cancellation and queued interruption.
- Required `Tools/Validate-Unity.ps1`: **PASS**, muted Unity 6000.3.19f1.
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-7a3f23ec-6c12-44be-95c1-5f3146c8f294.log`.
- Four starting user files still match their SHA256 hashes. Unity's automatic
  TimeManager rewrite was reviewed and removed. No paid generation or merge.

## Phase 06

- Seven initial focused scene checks passed, no failures/skips:
  `.utmp/ForestValidation/5cf1049e-bda6-40fb-a04e-4ef6ce6b1001.xml/.log`.
- Final affected checks: **33 passed**, zero failures/skips:
  `.utmp/ForestValidation/3f195399-e0ae-4030-90cf-32fff8b16585.xml/.log`.
  Covers separate builder ownership/full slots, actual-tier extraction with zero
  rewards, Sentinel current material/Continue/lost targets/final-move drill
  interruption, CourtRevision and KingReadiness regressions. The filename-only
  ChannelLifecycle filter did not select its unrelated method names.
- Actual turret/ore-status proof captured and inspected at
  `.utmp/Ironvein/Captures/phase06-turret-proof.png`; shell art is temporary.
- Required `Tools/Validate-Unity.ps1`: **PASS**, muted Unity 6000.3.19f1:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-97d9c7b9-8317-4536-b5df-f20ecf81c4e8.log`.
- All four starting user-file hashes still match. No paid generation or merge.

## Phase 07

- Initial focused tests: **12 passed**, no failures/skips:
  `.utmp/ForestValidation/cefaa272-c87e-4e3c-b4e3-12f9548af841.xml/.log`.
- Final affected tests: **74 passed**, no failures/skips:
  `.utmp/ForestValidation/35d2da62-0e49-4bb4-8fb6-ac163efa9d68.xml/.log`.
  Covers the three-part boss cycle, Core hits/Continue/final-response drill,
  default final death, optional ejection/cancelled Core/early pilot defeat,
  saved pilot HP/deadline, one weaker reserve suit and whole-formation gating.
  Includes Ironvein, damage, continuation and legacy channel recovery checks.
- Required `Tools/Validate-Unity.ps1`: **PASS**, muted Unity 6000.3.19f1:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-7bbd8b44-f1af-451a-b98b-2db1e0461807.log`.
- All four starting user-file hashes still match. Boss motion and native visual
  proof remain Phase 08 work; no claim of visual or physical-device approval.
