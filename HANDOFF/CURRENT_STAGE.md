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

Main matrix batch20261118-020131 COMPLETE:20 cases,17 natural defeats,3 alive
at3-visit limit. Unity/analysis/audit all exit0;4,664 accepted inputs audited.
30 completed visits (20Dungeon,7Forest,3Frostwake); no immediate recipe repeats.
Main report/artifacts now curated in Docs/Validation/CrystalResonanceRefinementEvidence.
The source delta against pilot is ONLY the offline analyzer; runtime/data unchanged.
Typical Forest0/4 vs skilled4/4; all20 Dungeon cleared. Do not infer fairness
solely from win rate or thinking time; examine decisions, supports and counters.

Sampling flaw found: all original consecutive seeds19001–19005 picked Forest
first and surviving third visits Frostwake. Travel first xorshift fractions all
0.8130–0.8135: tightly clustered experimental seeds, NOT a forced Forest route.
Do not present the25 original runs as representative five-zone coverage.
Commit81c93d8 predeclared spread-seed-protocol.json BEFORE testing any new route.
SHA256(fixed prefix|label) to positive31bit seeds; no desired-outcome selection.
New wide group:5 spread seeds*5 policies=25 natural runs, same character/level
pairs, max3visits/1800s each. Not started yet. Preserve original sample separately.

ACTIVE DEEP batch20261118-063014, four L10 natural Dungeon-first attempts,
spread seeds per committed protocol. Max10 visits/2700s, real deaths retained.
Wrapper PID34856; verified one-shot completion waiter PID17540.
Job state .utmp/CrystalResonance/Refinement/jobs/deep-20261118-062955/job.json.
No model polling required; completion queues to this same chat. Do not launch
another Unity job or edit Assets while active. Notify claim prevents duplicates.
Known import noise must be reviewed/restored only after owned Unity exits.
Scratch cr_refine_background.ps1 uses native child LASTEXITCODE (exit0/7 verified)
and automatically analyzes/audits/notifies once; no retry or model change.
Waiter cr_refine_notify.ps1 observes only own wrapper, never kills processes.
Launch tool: scratch/cr_refine_launch_group.ps1 -Group deep|wide|main|pilot.

NEXT ON DEEP COMPLETION: inspect job+analysis/audit once, deep survival,
Court/Ironvein/return coverage, capstone mechanics and pressure. Then launch
-Group wide (predeclared25 cases) unless actual bug requires targeted correction.
Do not silently substitute direct-zone fixtures for missing natural evidence.
Keep source-separated batches; current production code/data remain b689187,
offline analyzer cdfae32; later commits only reports and seed protocol.
After wide: measure fairness/strategic metrics, firstcards and caps, any smallest
justified tuning + affected reruns; no timers/HP padding or manufactured success.

After evidence: investigate Court/Forest/capstones, only targeted justified
tuning + affected retests, final validator, Android/Windows builds, curated A–F
report. Push/update existing draft PR234; never merge or create another PR.

LATEST USER STEERING / PRESERVATION AUDIT (November18,07:38UTC):
Before new playtests, inspected every Playtest directory and process chain.
No tests started/deleted/overwritten/restarted/interrupted this turn.
Deep remains the only active batch:2closed,typical Minotaur third active,fourth
Kobold pending at inspection. Snapshot safe: Inspection/audit-20261118-073806-51e6f8.
New offline copied-case audit passed777durableinputs,8completedvisits,2realdeaths.
Skilled14.6min diedDungeonrevisitdepth71;defBard33min diedFrostcapdepth85 after
successfulCourt/Ironveinvisits. Do not pool partial snapshot into final batch
again or claim wholedeepcomplete. Final fulljobanalysis will supersede snapshot.
Historicalfailed20261117-014621 retained(EditorSearchDatabaseexception);oldroot
pass1/probe1 are inactive incompleteprobes,notrunning. All77closedrecordsinventory
parsed;nointegrityerrors. See C3report/existing-runs-inspection.json.
Userrequires clean Unity/test-process shutdown AFTER work; verify owned process
exit includingimportworkers,wrapper,notifier. Do not kill activebatch or unrelated
processes. Existing notifier17540 already armed; do not create another waiter.
Future tests/builds must use NEW output paths and preserve existinglogs/results.
Do not launch any new test until currentbatch ends and itsresult is inspected.
No tuning until intended natural evidence is reviewed including failures.
