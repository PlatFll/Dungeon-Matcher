# Crystal Resonance V1 refinement — work in progress

November 18, 2026. Existing draft PR #234 only; **not merged**.
The pilot below is completed evidence, not a completed final acceptance pass.
The 20-case independent matrix, deep runs and final builds remain outstanding.
Historical results remain unchanged in CRYSTAL_RESONANCE_V1_FINAL.md.

## A. Implementation

Gameplay commit b689187 makes all fresh standard, Retry, player-facing Practice
and challenge starts Dungeon-first. MainMenu loads Game without a zone picker.
Continue retains its saved zone, board, resources and progression. Explicit
Editor-only zone requests remain available and are consumed once. Menu/Retry
clear stale diagnostic requests. Random eligible travel remains unchanged.

RunLaunchOptions, RunSession, ZoneTravelController and MainMenuController own
these narrow launch changes. No second board resolver or damage path was added.
Enemy stats, shield rules, abilities, rewards, zone recipes and approved sprites
have not been adjusted during this refinement. Design/state/pacing owner docs
now describe permanent Dungeon-first starts and flexible visit duration.

HumanLikePlaytestDriver attention-v2 varies attention, weakness/telegraph
recognition, special setup, visible-only falling-piece recognition, casts,
supplies and card preferences. The fresh-account harness uses genuine BeginRun,
not a fabricated Continue. See ../CrystalResonance/PLAYTEST_POLICY.md for limits.
Diagnostic scores cannot influence the submitted action or consume policy RNG.

## B. Checks completed so far

- Tools/Validate-Unity.ps1 passed on Unity 6000.3.19f1, log
  DungeonMatcher-UnityValidation-79f66a36-76d0-4520-9f6d-3c6e2a02ffa6.log.
- Tools/Test-Balance.ps1: 272 passed, zero failed/skipped.
- Affected graphics suite: 47/48 initially passed. The new aggressive-profile
  test incorrectly assumed no consumable could precede its first swap. The
  corrected test compares every submitted command kind with durable acceptance,
  and still requires exactly one accepted swap.
- Corrected command test plus new five-zone feedback fixture: 2/2 passed.
  Across latest results all 49 distinct affected checks pass; this is not a
  single 49-test execution. Raw failed and corrected XMLs are retained in
  CrystalResonanceRefinementEvidence/initial-test-results.zip and the ledger.
- Earlier new fixtures incorrectly modeled a visible cascade and compared clocks
  across a resumed frame. The corrected actual-visible board passed, and Continue
  now checks advancing clocks plus exact non-clock state. These failures remain
  in the ledger, not erased.
- Actual Game captures cover five zones and five mobile profiles at 70/80/100,
  six-digit score, pause/reduced motion and 18 animation frames. New controlled
  legal-input fixtures cover normal three, deliberate four/spark, pending fill,
  notch anticipation and non-blocking images in every zone.
- Representative full frames were visually inspected for every zone. The dark
  message backing remains readable over snow; feedback does not overlap HP or
  board controls. Approved crystal/plaque SHA-256 values remain unchanged.
  These are Editor captures, not physical phone or human listening approval.

Preserved XMLs identify every executed test and failure. Commands:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1
powershell -ExecutionPolicy Bypass -File Tools/Test-Balance.ps1
powershell -ExecutionPolicy Bypass -File Tools/Test-ForestFoundation.ps1 -Graphics -Filter 'BalanceDisruptionPlayTests.RoyalMechanicsResolveWithEscortsInActualPlayMode;CombatAudioTests;ForestFoundationPlayTests.EnvironmentTravelSnapshotRestoresZoneHolesWithoutChangingSourceOnWriteFailure;ForestFoundationPlayTests.IronveinNaturalLessonAndEncounterHistoryContinueOnce;ForestFoundationPlayTests.LegacyDungeonFearKeepsItsSourceThroughContinue;CrystalResonanceStateTests;CrystalResonanceContinuationTests;CrystalResonanceDirectorTests;CrystalResonanceSelectionTests;ForestFoundationPlayTests.CrystalResonance;ForestFoundationPlayTests.DungeonFirst;GameplayPixelLayoutTests'
powershell -ExecutionPolicy Bypass -File Tools/Test-ForestFoundation.ps1 -Graphics -Filter 'ForestFoundationPlayTests.CrystalResonancePolicyUsesJournaledLegalInputForEveryStyle;ForestFoundationPlayTests.CrystalResonanceFeedbackShowsNormalExcellentAndPendingInAllZones'
powershell -ExecutionPolicy Bypass -File Tools/Run-UnityScenario.ps1 -Method CrystalResonancePlaytest.Run -Name CrystalResonance-refine-pilot
```

Shell invocations used the required rtk proxy prefix. The final validator and
Windows/Android builds must follow the remaining work.

## C. Completed pilot evidence

Batch 20261118-010917, gameplay source b689187, five independent policies on the
same named initial seed 19001, Rattlebones level 1. Each starts in Dungeon naturally.
Each production-generated run ID/capstone stream is also recorded; named seed
alone is not the full reproducibility key. Input/config/source snapshots and
checksummed native journals are archived in CrystalResonanceRefinementEvidence.
No HP/energy/cards/charge or forced travel was granted during play.

Unity exited 0 and all five cases completed. The outer PowerShell helper falsely
reported failure because its process object exposed a null ExitCode. Separately,
the analyzer rejected duplicated identical diagnosticStart metadata. Commit
cdfae32 fixes only that offline analyzer. Original failed records are preserved
in pilot-recovery.json. Analysis was rerun against untouched evidence.
The independent audit passed all **876 durable accepted inputs**, with no
duplicate banks, skipped capstones, repeat unique leaders or illegal travel found.

| Profile | Dungeon seconds | Dungeon swaps | Dungeon HP damage | Dungeon first card s | Forest death depth | Forest first card s |
|---|---:|---:|---:|---:|---:|---:|
| Novice | 486.8 | 119 | 315 | 43.2 | 27 | 75.6 |
| Typical | 285.5 | 80 | 145 | 17.5 | 33 | 43.6 |
| Skilled | 230.5 | 71 | 140 | 14.8 | 33 | 34.9 |
| Defensive | 307.3 | 64 | 105 | 22.7 | 30 | 65.4 |
| Aggressive | 317.1 | 96 | 170 | 19.9 | 32 | 30.7 |

All five completed Dungeon and naturally traveled to Forest for this shared
initial travel seed. All died in Forest, total run duration 7.4–11.7 minutes.
This is one initial seed per profile, not the requested five-seed coverage.
Dungeon had 19–20 victories, 11 distinct recipes, 11–12 unique enemy types and
zero immediate recipe repeats. Capstones were current-zone Knight/Town elites,
Marshal escort or Captain escort (13.9–75.3 seconds); no guaranteed major boss.

Skill is not inferred from delay alone: skilled used 71 Dungeon swaps and lost 140
HP cumulatively versus novice 119/315. Their capstones/builds differ, so these are
descriptive observations, not a controlled causal estimate. Defensive used 64
swaps/105 damage but took longer. Aggressive used three bombs and had two casts
flagged against a small remainder. Heuristic flags are not proven tactical errors.

Forest's six to seven encountered enemy types included Mender, Thornkeeper,
Briar Archer, Drummer, Berserker, Scout and Trailguard. Recipes did not immediately
repeat. The novice overlooked six visible marked responses and did not choose
eleven; it died at depth 27. Stronger policies reached depth 30–33. Drummer-led patrols
and Berserker formations caused sustained damage; the skilled final formation
lasted 15 swaps, rather than an unexplained instant kill. All profiles consumed
their available potions naturally. One seed at L1 is insufficient to classify
this as an unfair zone or justify a blanket nerf. The larger matrix is required.

## D. Adjustments and historical outliers

No gameplay numeric or formation adjustments have been made in this refinement.
The historical defensive policy overvalued healing even with only 5 HP missing;
attention-v2 scales that preference by missing health. The pilot Forest first
cards arrived 30.7–75.6 seconds. Different seeds/starts mean this does not prove a
controlled fix for the historical 116-second direct-Forest outlier.

The historical 3.693-second depth 59 Court elite was defeated by one accepted
color-crystal/column-bomb combination with a strong accumulated card build.
That is evidence of earned combo power, not a reason to add HP padding. Whether
deep capstones routinely fail to expose mechanics remains for the new deep runs.

## E. Remaining concerns

Court coverage, other natural destinations, more seeds/characters/levels, late
revisits and 20–45-minute natural attempts remain outstanding. Forest attrition
needs cross-seed review. Marked-response heuristics do not capture every indirect
counter; special explosions or killing a caster can solve threats. No policy
models human reading, fatigue, learning, touch slips or enjoyment.

## F. Current recommendation

Implementation and initial technical checks support continuing the actual-game
evaluation. This is **not yet the final human-acceptance recommendation**.
Complete the matrix, investigate supported concerns, rerun affected checks after
any tuning, build both targets, then update this report and the existing draft PR.
Never merge without explicit authorization.
