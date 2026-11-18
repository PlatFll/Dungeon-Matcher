# Crystal Resonance refinement — in progress, no merge

Existing branch codex/crystal-resonance-zone-director / draft PR #234.
Task start cae70c1d; clean at start and origin fetched. Other checkouts preserved.
Implementation checkpoint committed locally as b689187; not pushed yet.
User brief: November 18 attachment 11ffe724-9eb9-4683-92c7-aefe1d567607/Pasted text.txt.

Implemented: permanent Dungeon-first menu/Retry/Practice/challenges; Editor-only
isolated starts; exact checkpoint precedence. Limited-attention five-profile
policy attention-v2, visible-only cascade heuristic, cast/card choices, diagnostic
logs. Natural runner creates genuine new runs. Non-Dungeon cases require
diagnosticStart=true. Initial run IDs/capstone seeds are recorded, not fabricated.
Owner docs now treat visit duration as a soft reference. No gameplay balance
numbers, enemy formations/stats or approved art changed in this refinement.

Validation: required Validate-Unity.ps1 passed (79f66a36 log in system Temp).
Full balance 272/272 passed. Affected suite 47/48 passed; failed new test assumed
aggressive could not use a bomb before its first swap. Corrected it to compare
every submitted command kind with durable acceptance, still exactly one swap.
Rerun of that test plus new five-zone normal/excellent/pending/reduced-motion
fixture: 2/2 passed (8692ef89 XML). Earlier cascade/clock fixture failures were
corrected and rerun (09f0f8e7 XML). Ledger preserves every failure/correction:
.utmp/CrystalResonance/Refinement/Regression/ledger.json.
Across latest results all 49 distinct affected checks pass; this is not one
single 49-test run. Final required validator/builds remain after playtesting.

Fresh HUD captures: .utmp/CrystalResonance/Visual (five zones/five phone profiles,
70/80/100, reduced motion, 18 motion frames), Refinement/Feedback (15 states).
Viewed representative actual full frames in every zone, large score, snowy
approach, mastery and pending. No health/input overlap observed. Crystal/plaque
source hashes unchanged. No physical phone or human enjoyment certification.

Historical outliers: Refinement/baseline-findings.md and baseline-outliers.json.
3.693s Court elite was one earned color-crystal/column-bomb combo in a strong
depth-59 build, not evidence for HP padding. Prior Forest defensive 116s first
card used flat healing priority even with 5 HP missing; policy now scales it.

Pilot batch 20261118-010917 completed all five natural L1 skeleton/seed19001
cases, each cleared Dungeon then died in Forest. Unity exit0/no errors. The
outer helper falsely failed on a null process ExitCode; original failure kept.
Analyzer also had duplicate identical diagnosticStart keyword metadata; fixed
in offline-only commit cdfae32. Re-analysis and native audit pass:876 accepted
inputs, no invariant failures. Do NOT repeat the pilot because of helper failure.
See Docs/Validation/CRYSTAL_RESONANCE_REFINEMENT.md and its new evidence directory.
Pilot Dungeon swaps novice119/skilled71, damage315/140; Forest losses atdepth27–33.
No gameplay balance change justified from one seed; inspect main/deep next.

ACTIVE main matrix: batch20261118-020131, 20 natural cases, seeds19002–19005,
four characters atL1/L5, all five profiles, max3 visits/30min each. Root PID17724.
Job state .utmp/CrystalResonance/Refinement/jobs/main-20261118-020113/job.json.
One-shot completion waiter PID30432 is VERIFIED waiting for that process.
Queue wakeup was verified twice (probe and completed pilot); it posts to chat
01a127fb-8a65-7d73-9061-df9dc2d9843a. No model polling required.
The corrected wrapper invokes a native child and reads LASTEXITCODE; explicit
exit0/7 probes passed. It analyzes/audits after Unity exits and notifies once.
A claim file prevents duplicate notifications; failed notifications do not retry.
Do not start another Unity job or edit Assets while the current job is running.
Known serialized import noise must be inspected/restored only after owned exit.

Next on notification: read job + analysis/audit once, check all20 cases and
failures. Compare sources explicitly: pilot b689187 vs main cdfae32 differs only
in offline analyzer, not gameplay/zone/policy source. Full source manifests
include Assets/Packages/ProjectSettings/Tools. Keep batches separate in reports.
Then run four deep L10 Dungeon-first attempts (max45min, natural deaths retained):
scratch cr_refine_launch_group.ps1 -Group deep. Its wrapper auto-arms a verified
single completion waiter. Assess sparse natural zones and extend if warranted.
Never substitute direct-zone fixtures for natural progression evidence.

After evidence: investigate Court/Forest/capstones, only targeted justified
tuning + affected retests, final validator, Android/Windows builds, curated A–F
report. Push/update existing draft PR234; never merge or create another PR.
