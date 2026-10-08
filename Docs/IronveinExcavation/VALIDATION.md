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

## Phase 08 — presentation support tranche

- Added active-sequence modifier ownership for selecting the single powered
  attack after its token is consumed, including multi-hit sequences. Existing
  damage calculation is unchanged. The shared importer accepts explicit attack
  clips, and Ironvein uses the existing optional hit/death presenter.
- Initial headless run: 24 passed / 3 failed because Unity remained at 640×480
  instead of the required portrait scene viewport; no test expectations were
  weakened. `.utmp/ForestValidation/cbc0686e-808b-4034-aa84-c654b4d36735.xml/.log`.
- Graphics-enabled affected run: **27 passed**, no failures/skips:
  `.utmp/ForestValidation/cc267c4b-7a8d-48b0-ad67-92d8fd828228.xml/.log`.
- Required `Tools/Validate-Unity.ps1`: **PASS**, muted Unity 6000.3.19f1:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-fc2c033c-2e67-44b4-bbf6-5d98335dccab.log`.
- Native source/candidate work is outside Unity. Fourteen stills have measured
  dimensions, palettes, hashes and binary alpha. Palette-only cleanup preserves
  every source alpha mask. Action inputs add transparent margins by integer
  translation, never resampling. Final clip imports and playback proof remain.

## Phase 08 — first native import

- Fourteen stills and fifteen clips imported with unchanged kit values.
- Five focused checks passed (`117b7954-2c1a-4a5e-ab48-8931dc1e8056.xml` in
  `.utmp/ForestValidation`), including actual sprite-at-contact assertions for
  powered then ordinary Pickaxe attacks: one hit each, exact pose, Idle recovery.
- Final current Ironvein run: **43 passed / 0 failed / 0 skipped**, graphics
  enabled, Editor audio muted. Evidence:
  `.utmp/ForestValidation/85c420a3-3cbf-41c2-bc41-839a9c43e8f5.xml/.log`.
- Mandatory Unity validator **passed**, exit 0:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-9990cc73-9944-4f73-976c-d751ce3bc215.log`.
- Captures reviewed: `.utmp/Ironvein/Captures/phase08-AutoAttack-contact.png`
  and `phase08-OreChargedAutoAttack-contact.png`. Actors grounded; native texels
  and counter spacing pass the layout validator. Dungeon backdrop remains a
  temporary shell; this is not a completed cave composition or user approval.
- All four unrelated starting user-file hashes remain unchanged.

## Phase 08 — complete native motion integration

- Fourteen identities plus optional pilot/reserve forms bind 108 native clips.
  Exact source selections and contact times are in `motion-selections.json`;
  exported frame RGBA equality, bounds, alpha and palette records are retained.
- Whole-roster scene test checks every basic's actual impact sprite, one damage
  event, frozen paused windup and Idle recovery. Phase tests verify ejection,
  one remount and Continue without transition replay.
- Initial complete regression found a genuine Core cleanup race: the board
  released animation ownership one frame before the ability cleared saved intent.
  The move gate now waits for pending kit cleanup. Fixed-delay test assumptions
  were replaced with bounded actual-state waits; expected behavior is unchanged.
- Final affected run: **45 passed / 0 failed / 0 skipped**, graphics enabled and
  Editor audio muted. `.utmp/ForestValidation/0041fd67-d4d0-4f05-a059-31893540070d.xml/.log`.
- Required `Tools/Validate-Unity.ps1`: **PASS**, exit 0, Unity 6000.3.19f1.
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-6659517b-4fc5-48f2-b5a9-46b0b9a00fc5.log`.
- All four starting user-file SHA256 hashes still match. Native motion and scene
  tests do not establish user visual approval, final cave composition or device QA.

## Phase 09 — cave, gameplay UI, mechanisms and temporary audio

- Required `Tools/Validate-Unity.ps1`: **PASS**, exit 0, Unity 6000.3.19f1:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-703ea8ca-c5a3-495a-89df-ebf7c37531a5.log`.
  The first sandbox attempt could not connect to licensing and was stopped;
  this successful run used the local licensing client. All user-file hashes match.

- Final affected regression: **49 passed / 0 failed / 0 skipped**, graphics
  enabled and Editor audio muted. Evidence:
  `.utmp/ForestValidation/77602c94-cdc0-4333-bdfd-34e9df2ec316.xml/.log`.
- Actual captures at 720×1280, 1080×1920, 1080×2400 and safe insets are in
  `.utmp/Ironvein/Captures/phase09-*.png`; screen/material gallery is
  `.utmp/Ironvein/ScreenReview/Review.html`. Reviewed grounded actors, full
  drill housing bounds, native frame dimensions, stone damage and counter space.
- Initial failures identified incorrect native border/plaque sizes and one
  clipped housing at 720px. Corrected source slices and board padding. The
  capture now waits for the actual reveal shader to settle before pausing.
- Projectile tests verify first-stone stop, one durability damage, frozen
  paused presentation, cleanup and correct gameplay with optional VFX disabled.
- All production sound bindings and the 120-second temporary cue pass import
  checks. Music has not received human listening approval; testing was muted.
- Three unrelated pre-existing broad CombatAudio test failures reproduced:
  EnemyGainAndHeal and the two Poison parameter cases; baseline evidence is
  `Docs/Validation/ForestPhase5_6/baseline-lifecycle.xml`. The final affected
  run includes the relevant production-cue validation, not these stale cases.
- Quoted PixelLab usage: 295.2/300 initial, 185 requests; 100-generation reserve
  untouched. No pending generation jobs or purchases. All art remains candidate
  production for review; this record does not establish user approval.

## Phase 10 — initial focused evidence

- Deterministic data tests: **2 passed**, including 120 complete seeded visit
  selections, all thirteen identities, leader uniqueness, gentle introductions,
  budgets, relief, saved random-state equivalence and fourth-zone eligibility.
  `.utmp/ForestValidation/96520383-6b46-4a09-aa64-1b549557c97d.xml/.log`.
- Actual scene checks: **4 passed**, covering natural stone + encounter history
  through Continue, six handoffs across all four zones, first-stone cancellation
  and original stone safety. `.utmp/ForestValidation/8d7312c6-c365-45b2-b091-553f846758f7.xml/.log`.
  Six-hop durable account maximum: **133,653 bytes**. Source stones/charge,
  vines, flooding and caster marks clean up; unaffected gems/run/resources persist.
- Initial pacing harness compiled after correcting its health-property name.
  Its first multi-run execution had two counter assertions fail after successful
  travel because it sampled the newly loaded destination. Source move counts
  are now retained before handoff; final measured evidence follows separately.
  The apparently passing zero-assertion seed 3101 case had no measurement file
  and is not accepted as pacing evidence; it is rerun independently.

## Phase 10 — measured visits and affected regression

- Mandatory `Tools/Validate-Unity.ps1`: **PASS**, Unity 6000.3.19f1, exit 0:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-94108512-6aec-459e-abf7-460fbce2e05c.log`.
  Editor testing remained muted. All four original user-file hashes still match.

- Combined affected run: **54 passed / 1 failed**, no skips:
  `.utmp/ForestValidation/85c585b9-5c98-4592-9a2f-d50c54a19472.xml/.log`.
  The sole failure was the historical foundation assertion that the zone must
  remain testing-only. Phase 10 intentionally enables completed live travel;
  its replacement also requires all fourteen definitions, 41 recipes, the
  budget asset, Grand Delver apex and three native environments.
- Final readiness/teaching rerun: **8 passed / 0 failed / 0 skipped**:
  `.utmp/ForestValidation/6cee9ee9-113a-4894-9060-febdf91d3ddd.xml/.log`.
  All 55 combined cases pass on their latest affected execution; independently
  measured seed 3101 adds one case. Earlier counts overlap and are not additive.
- All mechanics, motion, four screen captures, actual six-hop travel, natural
  introduction/history and both corrected level-five pacing runs passed.
- The independent level-one run passed with its required measurement artifacts:
  `.utmp/ForestValidation/8a997135-d849-454d-ab62-e4ab0e0523c0.xml/.log`.

### Actual engine measurements

Disposable Skeleton profiles; greedy existing match policy, ability when ready,
potion below 45% HP, first offered card and 2.5 game-second think interval.
Engine ran at 6× speed with audio muted. No enemy/player stat or attack cadence
overrides. These are synthetic input observations, not manual play or guarantees.

| Seed / level | Encounters | Visit game seconds | Accepted moves | Fight seconds min / median / max | Fight moves min / median / max |
| --- | ---: | ---: | ---: | --- | --- |
| 3101 / 1 | 29 | 638.667 | 225 | 4.300 / 18.887 / 63.460 | 1 / 6 / 23 |
| 10101 / 5 | 29 | 465.129 | 157 | 2.136 / 11.044 / 63.477 | 0 / 3 / 22 |
| 9121 / 5 | 29 | 632.188 | 226 | 1.605 / 16.398 / 69.825 | 0 / 5 / 23 |

All three reached random travel, not a run ending. Ability/periodic kills can
finish a fight with zero new accepted moves. Mean sampled structural occupancy
was 0.7%, 0.6% and 1.0%; peak live stones 4, 3 and 6. Fixed drills fired 18, 16
and 18 times; 0, 2 and 0 fired through a live stone. First observed Obsidian was
at local wave 4, 28 and 26. Helpful-drill opportunities were uncommon with this
policy; human targeting/readability review remains useful before numerical tuning.
Raw per-wave CSV and summaries: `.utmp/Ironvein/Pacing/`. Different seeds and
real-time attack scheduling are not paired character-power experiments.

## Phase 11 — final review evidence

- **PASS — mandatory final validator:**
  `powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1` completed
  with exit 0 using **Unity 6000.3.19f1**. Log:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-60755fbb-8d9f-488b-8ef4-bfaa4a695788.log`.
  The four starting user files still match their exact hashes. Unity's unrelated
  TimeManager format rewrite was reviewed and removed after Unity exited.
- **PASS — previous-zone regression: 157 passed, 0 failed, 0 skipped.**
  Exact prior milestone cases plus shared barricade gravity, continuation,
  Bardley, zone picker and crystal/settings checks. Graphics-enabled Unity
  6000.3.19f1, muted Editor audio. Full result/log:
  `.utmp/ForestValidation/f7a700b6-bc91-419d-9f31-b58afa15ef50.xml/.log`.
  Covers King/forest/Court kits, channel cancellation, status and counter
  behavior, saves and prior travel. The compact per-case record is committed
  under `Docs/Validation/Ironvein/`. This also compiles the mine Guide update.
- **PASS — static native delivery:** fourteen stills and 108 motion sheets
  match their recorded source hashes and individual size/alpha/palette records;
  122 hashes checked, 2,404 Unity GUID references resolved, zero duplicate
  Ironvein GUIDs. `Tools/Verify-IronveinDelivery.py` writes
  `.utmp/Ironvein/native-delivery-check.json`; a durable copy is in
  `Docs/Validation/Ironvein/`.
- **PASS — review delivery:** the local gallery responds on
  `http://127.0.0.1:8891/Review.html`. Its actual-time motion page was opened and
  visually checked in the browser. It serves copied native sheets, real Unity
  screen captures and temporary music without autoplay. Source scripts,
  reference status and per-request generation receipts are retained.
- **PASS — scoped diff checks:** C#, Python, PowerShell and Markdown/text changes
  pass `git diff --check`. The unrestricted check reports Unity-generated
  trailing spaces on empty serialized YAML fields; those serialized files
  retain the editor's formatting. No unrelated user file was reformatted.
- **VISUAL REVIEW PENDING:** all newly generated Ironvein art and motion, cave
  composition and gameplay UI. The same-native-scale reference composite is a
  visual aid; individual technical measurements remain separate.
- **LISTENING REVIEW PENDING:** temporary original cue and effect mix. All Unity
  batch tests use the existing `BatchValidationAudio` Editor mute.
- **NOT EXECUTED:** manual human seeded sessions, human comprehension/pacing,
  physical touch testing or Android profiler capture. Three actual automated
  seeded sessions and automated previous-zone scenarios are recorded separately.
- **DEVICE TEST PENDING:** Android performance, safe cutouts, touch and sound.
  No result here establishes final balance or user approval to merge.
