# Crystal Resonance V1 — Stage 10 evidence and acceptance limits

Implementation and objective integration checks are complete. **Balance acceptance
remains provisional; this is not human-fun, physical-device or merge approval.**
The PR is unmerged. The measured runs below include real losses and short visits;
they do not demonstrate every zone meeting the 5–7 minute target.

## Scope and provenance

Base updated main: `fac476ad15300f68cae48ee68385323782667439` (PR #233).
Branch: `codex/crystal-resonance-zone-director`, dedicated worktree. GitHub main
was rechecked and remained identical to that base before final publication.
Stages 01–09 were committed sequentially without compilation, Unity, validation,
tests, screenshots or playtests. All executed evidence is from Stage 10.

The actual Game scene uses cumulative score separately from a 100-point visit
meter; provisional intentional mastery; safe sequential 12/40/70 drafts; persisted
78–88 approach scheduling; same-zone capstones and global-depth formations;
sealed 100 until the complete formation settles; existing atomic travel and
source-zone cleanup. No fixed King wave, forced Dungeon visit or required major
boss each visit remains in the live director. Exact responsibilities and data:
[implementation](../CrystalResonance/IMPLEMENTATION.md),
[migration](../CrystalResonance/SAVE_MIGRATION.md),
[encounter matrix](../CrystalResonance/ENCOUNTER_MATRIX.md).

Board/combat/shield ownership, character powers, enemy kits, shop/economy and
approved source sprites were preserved. No HP/damage, card delay or adaptive
threat tuning was used to force elapsed time. Constant charge awards were tested
at 4/5/10/14 after the 5/6/10/14 baseline. The current shared data is **4/5/10/14**;
**Drowned Court explicitly retains 5/6 ordinary/pressure awards** through fixed
ZoneDefinition overrides. All zones retain 10/14 miniboss/major awards and at
most three intentional mastery points. The exact changed paths accompany
this report in `CrystalResonanceEvidence/changed-files.txt`.

## Executed correctness checks

Unity **6000.3.19f1**, Windows. Mandatory command:
`powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1`.
Latest successful log:
`%TEMP%/DungeonMatcher-UnityValidation-a3fa965e-8d06-42ee-bf95-fc9ecd443a4a.log`.

| Check | Actual result | Evidence |
|---|---|---|
| Full balance regression | 272 passed, 0 failed, 0 skipped; 222.3s process wall time | `Tools/Test-Balance.ps1`; raw XML archived |
| Earlier focused regression | 37 passed, 0 failed, 0 skipped | Ledger contains exact filter/command |
| Expanded focused/deep/visual/counter execution | 38 passed, 1 failed, 0 skipped | 39 total; fixture failure described below |
| Corrected travel fixture rerun | 1 passed, 0 failed, 0 skipped | Baseline checkpoint was 38 plus 1; later full candidate rerun is separate below |
| Charge-candidate full regression | 272 passed, 0 failed, 0 skipped; 224s process wall time | Repeated after the numerical change |
| Charge-candidate focused regression | 39 passed, 0 failed, 0 skipped in one execution | Repeated save/travel/receipt/selection/visual/deep/counter checks |
| Final Court-cadence full regression | 272 passed, 0 failed, 0 skipped | Required suite repeated after zone-data/runtime correction |
| Final Court-cadence focused regression | 40 passed, 0 failed, 0 skipped | Includes actual production zone-award contract and repeated Game checks |
| Included lifecycle wrapper | 93 internal cases passed | Included within 272, never added as 93 more NUnit tests |
| Ordinary/capstone selection | 12,500 each, all five zones/tiers | Selection only, not played timing |
| Exhausted named/absent authored pools | 2,500 each | Legal same-zone fallback checks |
| Sequential deep capstone sampling | 5,000 selections | Unique leaders and major spacing; not natural revisits |
| Court dry/pending/flooded exhausted-pool sampling | 1,500 ordinary/capstone pairs | Explicit diagnostic phase tests |
| Offline observation/journal audit | 43 cases, 6035 durable accepted inputs, no observed audited invariant failures | Not a full Unity replay execution |

[Exact commands, counts, failures and hashes](CrystalResonanceEvidence/executed-test-ledger.json).
[Original NUnit reports and lifecycle result](CrystalResonanceEvidence/test-results.zip).
NUnit durations may exclude Play Mode/domain reload; they are not process wall time.
Overlapping suites are not summed into a misleading grand total.

Initial 77-check execution had eight failures, all passing in the later 37-check
rerun. Balance initially failed 10/257 headless checks, then 3/257 with graphics;
the final 272 execution passed. Corrections selected explicit diagnostic enemies
where old tests assumed retired fixed waves, used the current shared Ice contract,
expected approved five-point combat quantities, and correctly executed UnitySetUp/TearDown
for ability lifecycle fixtures. The expanded 39-check failure was a zone-hole
travel fixture randomly receiving an enemy-owned barricade; it now explicitly
chooses a Farmer to isolate zone-owned cleanup. Its assertions are unchanged.
No failure was independently reproduced on base, so none is called a confirmed
pre-existing failure. No production shield/rounding/ice/AIR rule was weakened.

## Stage 10 corrections

1. Authored recipes now honor their actual members' depth-fade weights, matching
   the fallback pool rather than retaining early compositions at full weight.
2. Score layout reserves the crystal and ornament; pooled particles stay still
   on their first paused frame. Telemetry removes destroyed actor references.
   Audio aliases use one existing shared resource lookup.
3. Actual Frostwake frames exposed low-contrast white feedback. An opaque native
   dark backing and rendered-bounds checks now preserve readability over snow.
4. The Editor policy missed stored oxygen inside a visible coffer. It now scores
   adjacent clears and prioritizes known urgent AIR responses over non-targeted
   ability use. Production water mechanics are unchanged. A recorded Game board
   was restored separately and normal legal swap `(0,2)->(1,2)` opened the coffer
   and retained AIR 5. This is a diagnostic counter fixture, not a completed visit.

The same Court level-1 seed 9203 originally suffocated at 201.2s / 57 Resonance.
After the policy correction it passed that oxygen failure, then died at 233.1s /
64 Resonance to Puffer Sentinel. That remains a failed visit. The level-5 Court
repeat stayed near 268.5s. These observations do not justify nerfing AIR or
claiming that Court pressure has been solved.

### Constant-charge experiment and final decision

Retain shared ordinary/pressure awards at 4/5, with an explicit fixed Drowned Court override restoring 5/6. Miniboss/major awards stay 10/14 everywhere. This is a provisional balance choice, not certification of the 5–7 minute target.

The uniform 4/5 experiment completed 9/13 cases; all four defeats were Court, including a previously successful level-5 seed 8303 which defeated its capstone but died at 93 charge. Restoring Court 5/6 completed that same typical case in 268.8s. The final source received fresh compilation, 272 balance checks and 40 focused checks before its actual-input follow-up.

In the uniform 4/5 trial, typical level-1 Dungeon median was 298.8s across two seeds, Frostwake 293.0s across two, Mine 313.2s across one, and Forest 229.7s across one. The fresh final-source Forest case completed in 269.1s. Forest and Court therefore remain below the target in these typical-policy observations. These small samples support only provisional cadence decisions. Defensive Forest first-card timing reached 116.3s, so another indiscriminate reward reduction could worsen that delay. No timers, artificial delay, HP padding, hazard nerf or power compensation was introduced. Court level-1/deep attrition and very brief high-power capstones remain unresolved acceptance questions.

The experiment changed only ordinary/pressure awards from 5/6 to 4/5, with
miniboss/major awards 10/14 unchanged. It did not add a duration gate, delay,
HP padding, hidden DPS compensation or mid-run grants. It included same-profile
repeats in every zone and fresh seeds. The full correctness suites ran again
after the change. A duration delta is calculated only for completed/completed
pairs; deaths and fresh profiles remain separate. See the exact per-case
comparison, current-source coverage and remaining issues in
[the tuning decision](CrystalResonanceEvidence/tuning-decision.json).
[Matched comparisons](CrystalResonanceEvidence/20261117-073354-comparison.md)
retain both failures and fresh profiles.
[Court restoration comparisons](CrystalResonanceEvidence/20261117-084917-comparison.md)
show the final repeated and fresh cases separately.

The global 4/5 trial produced four Court defeats and no Court completion. The
level-5 seed 8303 regressed from a 268.5-second success to a defeat at 93 charge
after its capstone died. The next correction therefore restores Court's original
5/6 awards only, through explicit zone data; it never examines level, damage,
player skill, elapsed time or oxygen to alter rewards. Shared fallback awards and
all five production zone bindings are covered by an additional regression test.
The final batch repeats typical/aggressive Court, novice Court and a fresh Forest
seed. Its exact outcomes are recorded below, including any remaining defeats.

## Real Game-scene measurement method

The Editor-only driver plays the production Game scene at **x1**, using normal
journaled swaps, abilities, supplies and card selections. It reads current legal
options, visible weaknesses, telegraphs, ice and oxygen. It never reads future
refill/encounter RNG. Novice/typical/skilled/defensive/aggressive policies differ
in response weights, build themes and natural decision delays; card reading
takes 6–11 seconds. Thinking starts after settlement.

Each seed/character/account level and initial zone is explicit in the config.
These are disposable starting profiles, not earned account progression. Level 10
deep exploration is within the production level-20 cap but is **not typical
level-1 pacing evidence**. There are no mid-run free HP/energy, kills, spawns,
charge writes, forced teleports or accelerated time in these samples. Separate
diagnostic depth/state controls never enter completed-visit timing.

The driver invokes normal accepted command callbacks, not physical touchscreen
gestures. Native journals verify acceptance. Direct pointer hit testing and
physical-device latency are not certified. Sample wall time includes thinking,
card reading, save/reload and travel. Telemetry combat/draft/travel clocks restart
at arrival and may differ from full visit wall time. First-card interval starts
at first visit observation and may overlap incoming reveal.
Editor capture/journal I/O is included rather than subtracted; these timings are
not device benchmarks. Policy speed differences include both choices and their
configured thinking delays, so elapsed time alone cannot prove strategic skill.

Batch 030133 is the pre-coffer-policy baseline; batch 062332 has the corrected
policy and feedback backing at the same 5/6 awards. Batch 073354 tests uniform
4/5. The final batch restores only Court to 5/6 while retaining 4/5 elsewhere,
with unchanged enemy stats/rosters. Tables remain separate;
historical coverage totals are not presented as final-tuning sample sizes.
Timing is not silently pooled. The earlier
015703 pilot is historical and excluded from final comparable distributions.

## Samples and completed-visit pacing

43 cases, **54 completed visits**, 17 natural case defeats,
26 configured visit-limit endings and
0 duration-censored cases.
Arrival-only rows after a visit limit are not failed/attempted visits. Repeated
seeds are not counted as independent seeds. Complete event and input logs retain
every recorded failure; no unsuccessful sample was silently dropped.

| Zone | Completed | Death visits | Censored | Independent completed seeds | Completed policies | Max completed depth |
|---|---|---|---|---|---|---|
| drowned-court | 7 | 8 | 0 | 4 | Typical, Skilled, Defensive, Aggressive | 16 |
| dungeon | 20 | 3 | 0 | 12 | Novice, Typical, Skilled, Defensive, Aggressive | 61 |
| frostwake-peaks | 9 | 3 | 0 | 8 | Novice, Typical, Skilled | 32 |
| ironvein-excavation | 8 | 1 | 0 | 7 | Typical, Skilled, Aggressive | 46 |
| magical-forest | 10 | 2 | 0 | 7 | Typical, Skilled, Defensive | 46 |

### Batch 20261117-030133

15 cases; 29 completed visits. Case outcomes: `{'defeat': 10, 'visit limit': 5}`.

| Zone | Policy | Level | Completed n | Median s | p25 / p75 s | Range s | First card median s |
|---|---|---|---|---|---|---|---|
| drowned-court | Typical | 5 | 1 | 268.4 | 268.4 / 268.4 | 268.4–268.4 | 33.8 |
| dungeon | Novice | 1 | 1 | 351.7 | 351.7 / 351.7 | 351.7–351.7 | 24.4 |
| dungeon | Typical | 1 | 2 | 297.2 | 240.3 / 354.2 | 183.3–411.1 | 33.4 |
| dungeon | Typical | 5 | 4 | 307.1 | 272.9 / 330.2 | 203.8–366.2 | 38.8 |
| dungeon | Skilled | 5 | 1 | 214.8 | 214.8 / 214.8 | 214.8–214.8 | 13.2 |
| dungeon | Defensive | 5 | 1 | 348.4 | 348.4 / 348.4 | 348.4–348.4 | 31.5 |
| dungeon | Aggressive | 5 | 1 | 476.1 | 476.1 / 476.1 | 476.1–476.1 | 60.7 |
| frostwake-peaks | Novice | 1 | 1 | 224.3 | 224.3 / 224.3 | 224.3–224.3 | 17.5 |
| frostwake-peaks | Typical | 1 | 2 | 263.5 | 257.3 / 269.7 | 251.1–275.9 | 41.8 |
| frostwake-peaks | Typical | 5 | 3 | 230.6 | 229.3 / 231.0 | 228.0–231.5 | 26.7 |
| frostwake-peaks | Skilled | 5 | 1 | 265.5 | 265.5 / 265.5 | 265.5–265.5 | 21.3 |
| ironvein-excavation | Typical | 1 | 1 | 316.3 | 316.3 / 316.3 | 316.3–316.3 | 29.4 |
| ironvein-excavation | Typical | 5 | 3 | 209.3 | 189.9 / 254.6 | 170.4–299.9 | 25.6 |
| ironvein-excavation | Aggressive | 5 | 1 | 247.2 | 247.2 / 247.2 | 247.2–247.2 | 19.8 |
| magical-forest | Typical | 1 | 1 | 289.6 | 289.6 / 289.6 | 289.6–289.6 | 33.7 |
| magical-forest | Typical | 5 | 2 | 270.9 | 258.9 / 282.8 | 247.0–294.8 | 28.0 |
| magical-forest | Skilled | 5 | 1 | 294.3 | 294.3 / 294.3 | 294.3–294.3 | 30.1 |
| magical-forest | Defensive | 5 | 2 | 395.6 | 383.3 / 408.0 | 371.0–420.3 | 73.6 |

[Full measurements](CrystalResonanceEvidence/20261117-030133-summary.json), [CSV](CrystalResonanceEvidence/20261117-030133-visits.csv), [configuration](CrystalResonanceEvidence/20261117-030133-config.json), [source hashes](CrystalResonanceEvidence/20261117-030133-source-manifest.json), [observed defeats](CrystalResonanceEvidence/20261117-030133-failure-review.json).

### Batch 20261117-062332

11 cases; 13 completed visits. Case outcomes: `{'visit limit': 9, 'defeat': 2}`.

| Zone | Policy | Level | Completed n | Median s | p25 / p75 s | Range s | First card median s |
|---|---|---|---|---|---|---|---|
| drowned-court | Typical | 5 | 1 | 268.5 | 268.5 / 268.5 | 268.5–268.5 | 33.9 |
| drowned-court | Skilled | 5 | 1 | 287.4 | 287.4 / 287.4 | 287.4–287.4 | 35.1 |
| drowned-court | Defensive | 5 | 1 | 372.1 | 372.1 / 372.1 | 372.1–372.1 | 55.6 |
| drowned-court | Aggressive | 5 | 1 | 272.5 | 272.5 / 272.5 | 272.5–272.5 | 28.7 |
| dungeon | Typical | 1 | 1 | 264.1 | 264.1 / 264.1 | 264.1–264.1 | 20.3 |
| dungeon | Skilled | 1 | 1 | 252.8 | 252.8 / 252.8 | 252.8–252.8 | 17.5 |
| dungeon | Skilled | 10 | 2 | 173.8 | 160.1 / 187.5 | 146.4–201.2 | 18.2 |
| dungeon | Defensive | 1 | 1 | 299.9 | 299.9 / 299.9 | 299.9–299.9 | 26.6 |
| dungeon | Aggressive | 1 | 1 | 228.9 | 228.9 / 228.9 | 228.9–228.9 | 19.6 |
| ironvein-excavation | Typical | 1 | 1 | 273.8 | 273.8 / 273.8 | 273.8–273.8 | 35.3 |
| ironvein-excavation | Skilled | 10 | 1 | 172.2 | 172.2 / 172.2 | 172.2–172.2 | 18.1 |
| magical-forest | Skilled | 10 | 1 | 277.5 | 277.5 / 277.5 | 277.5–277.5 | 37.1 |

[Full measurements](CrystalResonanceEvidence/20261117-062332-summary.json), [CSV](CrystalResonanceEvidence/20261117-062332-visits.csv), [configuration](CrystalResonanceEvidence/20261117-062332-config.json), [source hashes](CrystalResonanceEvidence/20261117-062332-source-manifest.json), [observed defeats](CrystalResonanceEvidence/20261117-062332-failure-review.json).

### Batch 20261117-073354

13 cases; 9 completed visits. Case outcomes: `{'visit limit': 9, 'defeat': 4}`.

| Zone | Policy | Level | Completed n | Median s | p25 / p75 s | Range s | First card median s |
|---|---|---|---|---|---|---|---|
| dungeon | Novice | 1 | 1 | 433.1 | 433.1 / 433.1 | 433.1–433.1 | 43.4 |
| dungeon | Typical | 1 | 2 | 298.8 | 271.3 / 326.4 | 243.7–354.0 | 36.7 |
| dungeon | Skilled | 1 | 1 | 269.3 | 269.3 / 269.3 | 269.3–269.3 | 27.4 |
| frostwake-peaks | Typical | 1 | 2 | 293.0 | 272.7 / 313.2 | 252.5–333.5 | 43.5 |
| ironvein-excavation | Typical | 1 | 1 | 313.2 | 313.2 / 313.2 | 313.2–313.2 | 29.3 |
| magical-forest | Typical | 1 | 1 | 229.7 | 229.7 / 229.7 | 229.7–229.7 | 33.7 |
| magical-forest | Defensive | 5 | 1 | 451.8 | 451.8 / 451.8 | 451.8–451.8 | 116.3 |

[Full measurements](CrystalResonanceEvidence/20261117-073354-summary.json), [CSV](CrystalResonanceEvidence/20261117-073354-visits.csv), [configuration](CrystalResonanceEvidence/20261117-073354-config.json), [source hashes](CrystalResonanceEvidence/20261117-073354-source-manifest.json), [observed defeats](CrystalResonanceEvidence/20261117-073354-failure-review.json).

### Batch 20261117-084917

4 cases; 3 completed visits. Case outcomes: `{'visit limit': 3, 'defeat': 1}`.

| Zone | Policy | Level | Completed n | Median s | p25 / p75 s | Range s | First card median s |
|---|---|---|---|---|---|---|---|
| drowned-court | Typical | 5 | 1 | 268.8 | 268.8 / 268.8 | 268.8–268.8 | 33.7 |
| drowned-court | Aggressive | 5 | 1 | 272.9 | 272.9 / 272.9 | 272.9–272.9 | 28.8 |
| magical-forest | Typical | 1 | 1 | 269.1 | 269.1 / 269.1 | 269.1–269.1 | 23.9 |

[Full measurements](CrystalResonanceEvidence/20261117-084917-summary.json), [CSV](CrystalResonanceEvidence/20261117-084917-visits.csv), [configuration](CrystalResonanceEvidence/20261117-084917-config.json), [source hashes](CrystalResonanceEvidence/20261117-084917-source-manifest.json), [observed defeats](CrystalResonanceEvidence/20261117-084917-failure-review.json).

### Matched policy comparison

Same Skeleton, level 1, Dungeon seed 8201 and initial equipment; novice baseline
comes from 030133, other policies from 062332. The changed coffer policy does not
affect the initial Dungeon. These are one seeded path per policy, not a population
estimate; policies make different subsequent choices and see different boards.

| Policy | Seconds | Banked encounters | Swaps | Mastery | End HP | Cards |
|---|---|---|---|---|---|---|
| Novice | 351.7 | 17 | 84 | 9 | 95 | prototype_strong_remedy, cascade_catalyst, chain_reaction |
| Typical | 264.1 | 16 | 75 | 13 | 95 | cascade_catalyst, prototype_efficient_casting, chain_reaction |
| Skilled | 252.8 | 16 | 84 | 15 | 80 | cascade_catalyst, prototype_efficient_casting, chain_reaction |
| Defensive | 299.9 | 16 | 69 | 10 | 100 | prototype_strong_remedy, cascade_catalyst, chain_reaction |
| Aggressive | 228.9 | 17 | 70 | 10 | 95 | prototype_mana_spark, chromatic_conductor, bombsmith |

Any policy that failed instead of completing remains in the case/visit files and
is not assigned a fictitious completion time. Different characters/levels in the
other samples cannot establish a pure player-skill effect.
On this baseline seed, the skilled heuristic earned 15 mastery versus typical's
13 and novice's 9, but needed 84 swaps versus typical's 75. It is not uniformly
more efficient. Defensive finished with 100 HP and 69 swaps; aggressive used all
three starting bombs. Time, resource use and decisions must be interpreted
together rather than calling the fastest policy objectively best.

### Variety, rewards and depth

Across the explicitly separated historical and candidate sources, completed
visits banked 848 encounters with 4823 base and 727
mastery charge before recorded overflow. Mastery is bounded by encounter, and
defensive play earned it in real runs (Bardley baseline 13/15/13 mastery across
three completed visits). Across completed visits, capstone ranks were
`{1: 30, 2: 22, 3: 2}` (1 elite, 2 miniboss, 3 major); immediate ordinary/formation
recipe repeats counted 3. Exact member identities, recipes, cards,
special executions, capstone seconds and charge sources are retained per visit.
These totals are descriptive, not a proof that every kit was fully experienced.

Descriptive exposure across all recorded batches (mixed account levels/policies; do not
use these aggregates as a pure skill or typical pacing comparison):

| Zone | Completed | Capstone rank counts | Capstone seconds range | Enemy special executions median | Distinct enemies encountered | Marked responses chosen | Marked opportunities |
|---|---|---|---|---|---|---|---|
| drowned-court | 7 | {1: 6, 2: 1} | 20.5–33.9 | 6.0 | 9 | 0 | 0 |
| dungeon | 20 | {1: 12, 2: 7, 3: 1} | 3.7–96.3 | 3.0 | 20 | 5 | 6 |
| frostwake-peaks | 9 | {1: 3, 2: 6} | 7.8–53.6 | 7.0 | 10 | 14 | 17 |
| ironvein-excavation | 8 | {1: 5, 2: 3} | 6.1–41.4 | 4.0 | 11 | 0 | 0 |
| magical-forest | 10 | {2: 5, 3: 1, 1: 4} | 11.7–32.7 | 3.5 | 12 | 7 | 7 |

Natural completed same-zone revisits:

| Batch/case | Zone | Visit | Formation depths | Seconds |
|---|---|---|---|---|
| 20261117-030133/pass2-policy3-bardley-L5-9402 | magical-forest | 2 | 31–45 | 371.0 |
| 20261117-062332/depth-skilled-skeleton-L10-100719 | dungeon | 3 | 47–61 | 201.2 |

The baseline genuinely reached King+Minister at depth 29 and Briar Matriarch at
depth 45 without fixed appointments. Its Forest revisit covered depths 31–45.
No ordinary Dungeon locals after depth 12, repeated once-per-run leaders or
audited full-bar capstone bypasses were observed. Exhausted-unique liveness and
every-zone phase legality also have separate selection fixtures.

## Fairness, challenge and unresolved acceptance

- **Pacing remains provisional.** The per-source tables and tuning decision
  show the actual result, including short visits and natural failures. A single
  fast or slow seeded path is not a population median. Some first drafts arrive
  before the proposed 30-second lower review band. Those bands are not timers.
  No artificial wait or HP padding was added. Current-source gaps remain explicit
  in the tuning decision, rather than borrowing baseline sample sizes.
  The defensive baseline also has a first-card outlier above 90 seconds; one
  cadence adjustment cannot be justified from the faster samples alone.
- **Court pressure needs human review.** Correcting the coffer policy separated
  one missed legal response from later damage attrition. A separate deep Minotaur
  baseline died at depth 49 / 13 charge against a Hammerhead-containing patrol after
  using available AIR responses. Neither case alone proves unavoidable damage.
- **Some capstones are brief.** The baseline includes an approximately 6-second
  Obsidian Sentinel and 9-second reserve patrol. Other capstones last much longer.
  The naturally reached level-10 Dungeon revisit at depths 47–61 defeated its
  royal elite capstone in 3.7 seconds and recorded zero enemy special executions
  across that visit. This is a concrete mechanics-exposure gap for that strong
  profile, not evidence of typical level-1 pacing or a reason for hidden scaling.
  Reserve composition/weight and mechanics exposure deserve focused review before
  locking balance; stronger builds must retain their earned speed.
- **Counter coverage is partial.** Logged immediate marked-gem opportunities,
  selected answers and low-AIR/coffer responses appear in each visit record.
  Environmental warnings, column mechanics, ability counters and killing an
  attacker require separate interpretation. A warning without an immediate
  marked match is not automatically an unanswerable attack. No comprehensive
  unavoidable-damage or crowd-control proof is claimed.
- **Coverage minima are explicit.** The coverage table is the actual result,
  including any zone below six completed visits and missing natural revisits.
  Natural deaths are not replaced by forced successes. Deep diagnostic captures
  cannot fill natural sampling gaps. Follow up sparse Court/deep cells with real
  players and recorded seeds before accepting balance.
  Observed novice first-visit deaths: drowned-court seed 71875 at 241.7s, drowned-court seed 71875 at 261.8s. These remain failures, not successful pacing samples.
- **Human enjoyment and device performance are unapproved.** Heuristic policies
  approximate choices; they cannot certify readability, challenge or fun for a
  real player. No physical Android device was attached during the final check.

## Actual Game visuals and deep diagnostics

The shared HUD test covers **five zones × five phone/safe-area profiles at 70%**.
80%, 100% and six-digit score use the final 1080×2400 profile, not all 25 cases.
Eighteen actual animation frames, pause, reduced motion and missing-sprite checks
are retained. Bounds/native placement checks passed; visual quality still needs
user acceptance. Existing source PNGs remain unchanged. The new composition and
pixel particles are candidates, not newly approved golden artwork.

![Actual Frostwake charged feedback](CrystalResonanceEvidence/frostwake-peaks-100.png)

![Actual Frostwake approach feedback after contrast correction](CrystalResonanceEvidence/frostwake-approach-contrast.png)

The separate deep diagnostic set injects depth 45 / visit 4 and known exposure, then
spawns a legal ordinary formation and highest eligible capstone in each zone;
each accepts a real swap. Ten formations cover Dungeon King+Minister, Forest
Matriarch+Trailguard, Court Nacre+Hammerhead, Mine Grand Delver+Gunner and Frostwake
Haldra+Wolf plus ordinary combinations. These demonstrate Game integration,
**not natural progression or boss victory**. [Exact diagnostic records](CrystalResonanceEvidence/deep-diagnostic-formations.json).
Played arrival captures may show an actor still entering; they are not posed
fully settled lineup evidence. [Native HUD animation frames](CrystalResonanceEvidence/animation).

## Builds and reproduction

Windows development build succeeded with 0 errors / 6 warnings; Android
development build succeeded with 0 errors / 7 warnings. BuildReport Android
`bytes` describes reported build size, not APK file size; the APK is **89,848,421
bytes (~85.7 MiB)**. [Windows report](CrystalResonanceEvidence/build-StandaloneWindows64.json),
[Android report](CrystalResonanceEvidence/build-Android.json). The warnings include obsolete Unity
API uses in unchanged files, unused fields/events and package shader messages;
they did not fail compilation. They were not independently reproduced on base.
[Build/validator provenance and artifact hashes](CrystalResonanceEvidence/build-validation-provenance.json)
records the tested source relationship and any reviewed raw-byte differences.

Local APK: `.utmp/CrystalResonance/Build/DungeonMatcher-CrystalResonance.apk`.
Local desktop: `.utmp/CrystalResonance/Build/Windows/DungeonMatcher.exe`.
Build binaries and full Unity/license logs are not committed.

To reproduce, use the recorded batch config/source hashes with Unity 6000.3.19f1,
copy config to `.utmp/CrystalResonance/playtest-config.json`, and run
`powershell -ExecutionPolicy Bypass -File Tools/Run-UnityScenario.ps1 -Method CrystalResonancePlaytest.Run -Name CrystalResonance-review`.
The runner uses disposable local profiles; start with the project closed.
Analyze each finished batch with `Tools/Analyze-CrystalResonance.py` and audit
recorded invariants with `Tools/Audit-CrystalResonanceEvidence.py`.
The complete native input/event evidence is in
[20261117-030133 archive](CrystalResonanceEvidence/20261117-030133-play-evidence.zip), [20261117-062332 archive](CrystalResonanceEvidence/20261117-062332-play-evidence.zip), [20261117-073354 archive](CrystalResonanceEvidence/20261117-073354-play-evidence.zip), [20261117-084917 archive](CrystalResonanceEvidence/20261117-084917-play-evidence.zip), with
[SHA-256 hashes](CrystalResonanceEvidence/archive-hashes.json). The archives are observations;
their offline audit does not mean every complete tape was replayed in Unity.

## Required human acceptance before balance/art lock

Play a 20–30 minute session on a phone, using Continue once and trying both
survival and offensive cards. Can you predict the next reward? Do intentional
mastery sparks and pending fill communicate what will bank? Can you read and
counter boss warnings? Does the complete capstone feel distinct from ordinary
combat? Are Court oxygen responses understandable? Do card choices affect your
decisions? Do any formations feel incoherent or unfair? Do the crystal particles
stay clear of input, and do you want another visit? Record the seed/profile and
specific encounter for negative answers. Automated timings cannot answer these
questions. **No merge is authorized by this report.**
