# Crystal Resonance V1 refinement — work in progress

November 18, 2026. Existing draft PR #234 only; **not merged**.
The pilot below is completed evidence, not a completed final acceptance pass.
All four planned batches (54 runs) have completed. The focused Court formation
correction, affected reruns and final validation/builds are in progress.
Historical results remain unchanged in CRYSTAL_RESONANCE_V1_FINAL.md.

## A. Implementation

Gameplay commit b689187 makes all fresh standard, Retry, player-facing Practice
and challenge starts Dungeon-first. MainMenu loads Game without a zone picker.
Continue retains its saved zone, board, resources and progression. Explicit
Editor-only zone requests remain available and are consumed once. Menu/Retry
clear stale diagnostic requests. Random eligible travel remains unchanged.

RunLaunchOptions, RunSession, ZoneTravelController and MainMenuController own
these narrow launch changes. No second board resolver or damage path was added.
Enemy stats, shield rules, abilities, rewards and approved sprites remain unchanged.
The focused Court formation correction is described in section D. Design/state/pacing owner docs
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

## B2. Court correction checks

After the formation correction, `Tools/Test-Balance.ps1 -OutputDirectory
.utmp/CrystalResonance/Refinement/Regression/court-correction-20261118` passed
272/272 with Unity exit 0. The focused Court/director/state/continuation suite
passed 45/47 initially. Both failures were obsolete timing fixtures: an immediate
basic outside accepted resolution, and seconds-based Stagger expiry. The runtime
already follows the approved unified move contract. Corrected fixtures retain
inflation/deflation and interruption checks while additionally verifying no
thinking-time attack/recovery, one due attack, and full move-based suppression.

The two-test rerun passed Tribute and exposed Puffer's second legacy assumption:
four setup moves instead of the existing v3 three-move special cadence. The
fixture now uses the actor's authoritative requirement and asserts two full
future responses. Its final rerun passed. Thus **47 distinct affected checks
pass across the latest results**, not one clean 47-test execution. Every failed
XML/log remains in `court-correction-test-results.zip` and its ledger. No runtime
code or enemy value changed to satisfy those failures. The Court contract now
labels its historical seconds profile and links the current move rules.

The final required `Tools/Validate-Unity.ps1` returned exit 0; log `DungeonMatcher-UnityValidation-37d7d691-92cb-4dc6-83d9-022236a0fe81.log`.
Test/build output changes preserve old artifacts using unique paths. Full commands,
failure messages and source paths remain in the ledger/logs. Android/Windows
builds and natural affected reruns still follow; this is not final acceptance.

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

## C2. Completed 20-case matrix (consecutive-seed sample)

Batch 20261118-020131 finished with Unity exit 0, no runtime errors and an independent
audit of 4,664 durable inputs with zero invariant failures. Seventeen attempts
ended in natural defeat; three reached the configured three-visit observation
limit alive. There were 30 completed visits: 20 Dungeon, 7 Forest and 3 Frostwake.
The longest attempt lasted 20.35 minutes but ended in defeat; it is not a claimed
successful deep run.

| Profile | Median completed Dungeon seconds | Median Dungeon swaps | Forest completed/attempted |
|---|---:|---:|---:|
| Novice | 495.5 | 121.5 | 0/4 |
| Typical | 316.9 | 84.5 | 0/4 |
| Skilled | 259.0 | 81.0 | 4/4 |
| Defensive | 378.6 | 74.0 | 2/4 |
| Aggressive | 338.1 | 86.0 | 1/4 |

All profiles use the same four character/level pairs per seed group, so this
table is descriptive across those pairs. Separate exact rows, HP damage, casts,
cards, threat diagnostics, formations, capstones and causes of death remain in
main-summary.json. Do not infer skill solely from elapsed time.

No immediate recipe repeats occurred. Forest completions ranged 220.9–526.3s;
Frostwake completions 255.7–366.8s. There was a Briar Matriarch major-boss capstone,
alongside elites and guardians: major bosses were not mandatory. Completed
capstones ranged 7.314–86.367s in this batch; brief victories remain earned play,
not automatic failures. The historical 3.7-second case still requires comparison
with deep new runs. Forest first-card outliers of 108.3s (novice) and 101.3s
(defensive) persisted; the Frostwake aggressive first card reached 117.2s.
These are flags for encounter/decision review, not grounds for uniform rewards.

### Sampling limitation discovered and corrected prospectively

Every original named seed 19001–19005 chose Forest as its first destination; the
surviving third visits all reached Frostwake. Source inspection found no forced
Forest route. The travel selector sorts the four eligible other zones and uses
the first xorshift draw of encounterSeed XOR 0x36741. These tightly adjacent
experimental seeds yielded fractions 0.8130–0.8135, all selecting the same index.
This is biased coverage from the experiment's seed selection, not evidence that
Court or Ironvein worked naturally. A Court arrival-only row is not a played
Court visit.

All original runs remain evidence. They are not counted as a representative
five-zone sample. An additional **predeclared five widely spread seeds per policy**
is committed in spread-seed-protocol.json (commit 81c93d8), plus four independently
derived deep seeds. SHA-256 maps fixed labels to 31-bit seeds; no candidate
destination draws were inspected and no seeds rejected to obtain favorable
routes/outcomes. The policy still cannot inspect future RNG. No game RNG,
durable save format, travel weighting or mechanics were changed.

Pilot versus main full source-manifest comparison found exactly one changed
file: the offline analyzer's consistent-metadata correction. Assets, scripts,
zone data and ProjectSettings were byte-identical before the two executions.
Batches remain separate in the report. Completed deep and spread-seed evidence
appears below.

## C3. Existing-run preservation audit (07:38 UTC, November 18)

The user noticed that the detached playtest continued after chat responses.
Before any additional test launch, all Playtest directories and the live process
chain were inspected. No run was deleted, overwritten, restarted or interrupted.
`existing-runs-inspection.json` records the inventory and local evidence paths.

The pilot (5 cases) and main matrix (20 cases) were finished. The one active batch
was deep `20261118-063014`: two finished cases, the typical Minotaur third case
running, and the aggressive Kobold fourth case not started yet. Unity PID23680
belonged to this batch, with two import workers. Wrapper34856/runner22420 and
the existing one-shot completion waiter17540 accounted for its remaining job
processes. No duplicate pilot/main Unity process was found. This is a point-in-time
inventory, not a promise that live process IDs remain current.

Historical batch `20261117-014621` retains its FAILED status and Unity Editor
SearchDatabase ArgumentOutOfRangeException. Its recorded result remains usable
as failure evidence, not a clean technical pass. Five other historical batches
contain 49 finalized cases. Root-level `pass1-novice-dungeon-L1-8201` and
`probe1-novice-dungeon-L1-8201` are old incomplete probes with no live owner or
result.json, not still-running tests. They are excluded from completed timings.
The earliest historical batches lack source/config provenance and are not
pooled with the new attention-v2 runs.

All 77 finalized case records across the inspected directories parsed without
integrity errors; existing closed native journals also passed gzip/JSON and
requested-versus-accepted counts. This does not convert the historical runtime
failure into a pass. Live gzip/checkpoint files were left alone. Complete
append-only text rows showed ongoing third-case progress. Only the two closed
deep cases were copied to a **new** inspection snapshot; their offline audit
passed 777 accepted commands, without changing any original evidence.

These two deep L10 attempts both ended in natural defeat. Skilled Rattlebones
completed Dungeon, Forest, Frostwake and Ironvein before dying on a Dungeon
revisit at depth71 after 877.4s (14.6 minutes). Defensive Bardley completed
Dungeon, Court, Frostwake and Ironvein, then died against the Iceborn Champion
capstone at depth85 after 1980.4s (33.0 minutes). The latter's completed Court
visit took 400.7s with 79 swaps, 15 ability casts and six enemy specials. This
adds genuine natural Court/Ironvein evidence, not a claim that every zone is
balanced. Its 33-minute attempt is a real loss, not a successful survival claim.

The skilled Forest guardian fell in 4.544s; defensive capstones lasted 24.1–41.3s.
The short earned result remains a review flag rather than grounds for adding HP
or timing gates. No balance change is justified from a partially completed batch.
The already-armed completion waiter will resume this chat after the job exits.
After all intended work, verify Unity, import workers, runner, wrapper and
notifier have exited; do not terminate unrelated Unity/Hub processes.

## C4. Completed deep attempts

Batch `20261118-063014` completed all four L10 Dungeon-first attempts. Unity,
analysis and the independent recorded-input audit exited 0. The audit checked
1,438 durable accepted commands with no invariant failures; there are 13
completed zone visits and four natural defeats. The 07:38 inspection snapshot
above is a subset of this batch and must not be counted a second time.

| Profile / character | Run minutes | Completed visits | Death depth | Natural route (last visit failed) |
|---|---:|---:|---:|---|
| Typical / dungeon_attender | 25.08 | 3 | 62 | dungeon → frostwake-peaks → dungeon → magical-forest |
| Skilled / skeleton | 14.62 | 4 | 71 | dungeon → magical-forest → frostwake-peaks → ironvein-excavation → dungeon |
| Defensive / bardley | 33.01 | 4 | 85 | dungeon → drowned-court → frostwake-peaks → ironvein-excavation → frostwake-peaks |
| Aggressive / kobold_gunner | 13.65 | 2 | 41 | dungeon → ironvein-excavation → drowned-court |

The typical and defensive attempts lasted 25.08 and 33.01 minutes, respectively,
but both died. There is no successful 45-minute survival claim. All five zones
were played naturally; a typical Dungeon revisit completed, while a skilled
Dungeon revisit and a defensive Frostwake revisit ended in defeat.

Court evidence now includes one defensive Bardley completion (400.7s, 79 swaps,
15 casts, 480 cumulative HP damage) and one aggressive Kobold death after arriving
with 85 HP (112.1s, 30 swaps, 280 cumulative damage). The defensive policy chose
14/16 immediately visible low-air bubble answers; aggressive chose 4/4. The
Kobold's last hit was Hammerhead Bruiser after sustained formation damage, not
proof that oxygen alone caused the loss. These are different characters/seeds
and only two visits: they do not establish low-level Court fairness.

Completed capstones ranged 4.544–73.360s. Two of thirteen lasted under ten
seconds. The Grand Delver major boss appeared in Ironvein alongside other visits'
elites/guardians, so a major boss was not guaranteed. No immediate recipe repeats
were observed. The typical late Dungeon revisit took 682.4s and 154 swaps:
Royal chain lesson/vanguard formations took 15–20 swaps each. This is an
encounter/decision review flag, not a reason to impose duration targets.

All previous deep-owned Unity, import-worker, runner, wrapper and notifier PIDs
were absent at 07:57:59 UTC; no Unity process remained. The completion notifier
queued exactly once and exited. `deep-shutdown.json` preserves that check.
The original batch, failed records and earlier inspection copies are unchanged.

The predeclared five spread seeds per policy are still needed to broaden
low-level natural destinations. No gameplay numbers or formations have been
changed on the basis of this small deep sample. Final balance decisions follow
that remaining matrix, including all of its failures.

The main and deep source manifests are byte-identical across Assets, ProjectSettings, Packages and Tools.

The remaining matrix started as batch `20261118-080102`, job
`wide-20261118-080039`, after confirming the previous processes were gone and no
wide batch/log/config already existed. Its 25 cases exactly match the previously
committed spread-seed protocol. Prior staging inputs were additionally preserved
in a new inspection folder. `wide-launch-check.json` records the three source
hash differences: Unity converted the three Consumable sprite metadata files
from CRLF to LF and added trailing spaces to empty fields. Exact prior/current
hashes and normalized contents were verified; every other source file matches
deep byte-for-byte. Gameplay and automated regression assertions are unchanged.
The batch completed and its one-shot notifier exited; see section C5.

The actual Court capstone screenshot `deep-court-capstone.png` was inspected;
the score/Resonance plaque and approach message remain separate from HP and
board controls. An Ironvein end-visit capture was a transition frame and is not
presented as proof of a visible boss. Recorded event timestamps use the Unity
process clock; action timestamps are case-relative. Use encounter/depth and
journal identity for per-encounter analysis rather than comparing unlike clocks.

## C5. Completed spread-seed matrix and causal review

Batch `20261118-080102` completed all 25 predeclared cases with Unity/analysis/audit
exit 0. The audit checked 5,731 durable commands without invariant failures.
Twenty-two runs ended in natural defeat; three reached the three-visit limit
alive. There were 39 completed visits: 25 Dungeon, nine Frostwake, three Ironvein
and two Forest. Seven Court visits all ended in defeat. Failed visits remain in
the summary and are excluded only from **completed-visit timing** distributions.

| Profile | Median Dungeon seconds | Median Dungeon swaps | Runs ending in defeat |
|---|---:|---:|---:|
| Novice | 469.3 | 118.0 | 5/5 |
| Typical | 307.0 | 88.0 | 5/5 |
| Skilled | 222.9 | 68.0 | 4/5 |
| Defensive | 382.3 | 83.0 | 4/5 |
| Aggressive | 314.8 | 94.0 | 4/5 |

The defensive L5 Minotaur completed Dungeon/Frostwake/Ironvein and remained
alive at the 26.62-minute observation limit. Skilled L5 Minotaur and aggressive
L1 Bardley likewise completed three visits alive after 17.60 and 16.88 minutes.
This supplies a successful observed continuation inside the 20–45-minute target,
not a claim of a complete endless-run victory or human enjoyment. All 54 fresh
refinement attempts are retained across four batches: 48 deaths, six living
observation-limit endpoints and 87 completed zone visits. Source differences
are explicitly recorded rather than silently pooled as one balance version.

No immediate recipe repeats occurred. That label-level check did not detect
repeated escort structure: Court's ordinary library used the same Spearman and
Hammerhead pair in all six three-actor routine patrols. Needlefin Skirmisher and
Shellback Porter both had positive ordinary pool weights but appeared in **zero**
authored live recipes; the always-available authored choices prevented fallback
from supplying them. Across the nine played Court visits in deep/wide, neither
appeared. This is concrete lost roster variety, independent of run win rate.

Court's seven new failed visits ended with enemy basics as their final recorded
hit. Low-air immediate bubble answers were chosen in 38/44 opportunities. The
telemetry's cumulative player_damage records omit damage-source detail, so a
last-hit label cannot establish the full suffocation contribution. All five L1
Rattlebones cases reached Court with 45–90 HP; late L5 Kobolds had 115 HP and
no potions left in that visit. Defensive missed four bubble opportunities,
whereas skilled answered all available ones. These findings justify examining
composition and entry resources; they do not justify a blanket oxygen nerf.

Forest's new 116.1-second novice first draft involved a 17-swap Mender lesson,
then an eight-swap Thornkeeper lesson and another Mender patrol. The Mender
healed repeatedly. Other new first-card intervals were under 90s except a
95.3s novice Ironvein visit. Together with prior outliers this warrants human
priority/clarity review, not globally accelerating charge or every card notch.

Wide capstones under ten seconds numbered four: three Dungeon and one Ironvein.
The deep sample's short victories were a minority, while other caps exposed
longer pressure. No timer, HP padding or forced boss mechanic was added.
The wide Unity, runner, wrapper and notifier were gone at 13:32:11 UTC;
`wide-shutdown.json` records the clean process inventory.

## D. Adjustments and historical outliers

The sole gameplay balance correction in this refinement changes Court formation
data: add Reef scouts (Spearman + Needlefin, weight 3) and Salvage escort
(Needlefin + Shellback, weight 2), both legal across existing depth tiers. Replace
the Hammerhead escort with Needlefin in Thief/Netweaver patrols and Shellback in
Puffer patrols. Heavy Marshal/Siphoner/Cantor patrols, member stats,
oxygen, charge awards and all other zones remain unchanged. The objective is
restoring eligible roster variety and varying repeated heavy pressure, not
forcing a time or success rate. A production-selector regression checks all four
ordinary identities and varied escort pairs at every tier.

Capstone contract review also found Queen Nacre paired with a Reef Hammerhead,
contradicting the approved Nacre Court-only escort contract. Restore the existing
Queen + Conch Marshal pairing and its complete 7.5 threat cost (previously 6.5
for Queen + Hammerhead). This is an authored formation budget, not an enemy-stat
multiplier. Other capstones, kind weights, uniqueness and prerequisites remain
unchanged. A regression checks faction identity and full escort legality.

Eight new natural reruns are declared in `court-correction-protocol.json`: all
seven failed wide Court scenarios plus the successful deep defensive control.
They use new names and directories; original results are untouched. Named seeds
alone do not pair production run IDs/capstone streams, and recipe changes alter
later RNG consumption, so survival differences will be descriptive, not proof
that the correction alone caused each outcome. Retest results are pending.

Editor build outputs, balance XML/logs and selection-distribution files now use
unique destinations or reject existing targets, preserving the user's evidence.
The historical defensive policy overvalued healing even with only 5 HP missing;
attention-v2 scales that preference by missing health. The pilot Forest first
cards arrived 30.7–75.6 seconds. Different seeds/starts mean this does not prove a
controlled fix for the historical 116-second direct-Forest outlier.

The historical 3.693-second depth 59 Court elite was defeated by one accepted
color-crystal/column-bomb combination with a strong accumulated card build.
That is evidence of earned combo power, not a reason to add HP padding. In the
new deep batch, two of thirteen completed capstones lasted under ten seconds;
others included a 64.6-second Grand Delver. The completed spread sample also contains a minority of short capstones; this
does not establish a systematic composition failure.

## E. Remaining concerns

Low-level Court attrition remains a concern after the baseline spread matrix;
the focused formation correction still needs its new natural reruns. Forest
first-card outliers and the longer late-Dungeon return need human priority and
clarity review. One defensive L5 run remained alive at its 26.62-minute limit;
the L10 deep attempts ending after 25 and 33 minutes were losses. These outcomes
are retained separately rather than relabeling losses as successful runs.

Marked-response heuristics do not capture every indirect counter; special
explosions or killing a caster can solve threats. Damage telemetry lacks source
detail for cumulative HP loss. No policy models human reading, fatigue, learning,
touch slips or enjoyment. No physical-device acceptance has occurred.

## F. Current recommendation

Implementation and initial technical checks support continuing the actual-game
evaluation. This is **not yet the final human-acceptance recommendation**.
Complete the affected Court reruns and final checks/builds, then update this
report and the existing draft PR. Never merge without explicit authorization.
