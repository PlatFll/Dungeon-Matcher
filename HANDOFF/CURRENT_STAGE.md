# Crystal Resonance refinement — active coverage batch; no merge

Branch codex/crystal-resonance-zone-director; existing draft PR #234 only.
Task-start cae70c1d; origin fetched then, clean start. Local changes are not yet
pushed. User brief: attachment 11ffe724-9eb9-4683-92c7-aefe1d567607/Pasted text.txt.
Current report: Docs/Validation/CRYSTAL_RESONANCE_REFINEMENT.md (A–F, WIP).

## User constraints
Preserve every existing test/log/result, including failures and inactive probes.
Never delete, overwrite or restart old tests. New tests need distinct output
paths. Do not interrupt a running intended batch. Analyze failures as evidence.
After all intended work, verify owned Unity, import workers, runner, wrapper and
completion notifier have exited. Do not terminate unrelated processes. No merge.

## Implemented and validated
b689187: permanent Dungeon-first new/Retry/Practice/challenge; exact saved-zone
Continue; explicit Editor-only diagnostics. attention-v2 five-profile policy
uses limited attention and imperfect visible-only decisions, no future RNG.
Fresh genuine BeginRun/account; recorded initial run IDs/capstone seeds.
cdfae32: offline analyzer accepts identical shared config/result metadata and
rejects conflicts. No gameplay numeric/formation/art change in this refinement.
Owner docs reflect Dungeon-first and flexible pacing; no timers/HP padding.

Validate-Unity.ps1 PASS (Temp79f66a36 log). Balance272/272 PASS. Across latest
results all49 affected checks pass, NOT one49-test execution: initial47/48;
incorrect new test assumed no bomb before swap, corrected to journaled all
accepted commands; rerun+newfeedback2/2. Earlier cascade/clock fixture failures
also corrected/rerun2/2. Ledger and raw failures preserved in Refinement/Regression
and curated initial-test-results.zip. Final validator/builds remain.
Actual HUD5zones*5phones,70/80/100,largeScore,motion18frames,reducedmotion,plus
normal/excellent/pending feedback checked. Approved sprites unchanged. No phone
test or human-enjoyment certification.

## Completed actual-game evidence
All source-separated evidence/configs/journals are curated in
Docs/Validation/CrystalResonanceRefinementEvidence.
- Pilot20261118-010917:5 cases,5Dungeon completions,5Forest deaths;876durable inputs.
  Outer wrapper nullExitCode falsely failed; original preserved, actualUnity0.
  Offline duplicate-key error fixed separately; recovered analysis/audit pass.
- Main20261118-020131:20 cases,17deaths,3alive at3visit limit;30completed visits
  (20Dungeon,7Forest,3Frostwake);4664durableinputs,audit0fail. TypicalForest0/4,
  skilled4/4. No immediate recipe repeats. First-card outliers108/101/117seconds.
- Deep20261118-063014:4L10cases,ALL NATURAL DEFEATS;13completedvisits,1438durable
  inputs,audit0fail. TypicalMinotaur25.08min(3visits),skilledRattle14.62min(4),
  defensiveBard33.01min(4),aggressiveKobold13.65min(2). All5zones played naturally;
  Dungeon return completed, Dungeon/Frost returns also failed. Not successful
  45min survival. Court defensive400.7s completion vs aggressive112.1sdeath;
  low-air immediate bubble answers14/16 vs4/4, not proof of oxygen unfairness.
  Caps4.544–73.360s;2/13under10s,GrandDelvermajor plus elites/guardians. Typical
  lateDungeon682.4s/154swaps;Royal formations15–20swaps each. Review flag, not HP tuning.
  Main/deep manifests identical; pilot/main differs ONLY offlineanalyzer.
  Previous Unity23680,workers10744/38456,runner22420,rtk16348,wrapper34856,
  notifier17540 ALL gone at07:57:59UTC. deep-shutdown.json records clean exit.

Audit38711fe: all Playtestdirs/processes inspected BEFORE new tests. Historical
20261117-014621 FAILED(EditorSearchDatabase exception) retained;5otherhistorical
batches49closedcases;rootpass1/probe1inactivepartials excluded. At07:38deep2closed
snapshot audited777inputs; this is SUBSET of full deep, never count twice.

## Sampling and active job
Original consecutive seeds19001–19005 all traveledForest thenFrost because first
xorshift draws clustered(.8130–.8135), not forced gameplayrouting. Preserve those
25cases asForest-heavy evidence, not five-zone representativecoverage.
Commit81c93d8 predeclared SHA256-spread seeds BEFORE destination inspection.
Wide25 newruns =5spreadseeds*5policies,4chars,L1/L5,3visits/1800s cap,naturaldeaths.
Previous staginginputs copied into Inspection/pre-wide-inputs; originalbatchfiles
untouched. No priorwidejob/log/config/cases existed; noUnityprocess beforelaunch.

ACTIVE WIDE batch20261118-080102, jobwide-20261118-080039, wrapper6848,
verified one-shot notifier21384. Job path:
.utmp/CrystalResonance/Refinement/jobs/wide-20261118-080039/job.json
Launch check confirms exact25 predeclaredcases. Source differs fromdeep ONLY
threeknownConsumablemeta trailingwhitespace/lineending rewrites; hashes and
normalizedfilecontent verified. All otherfiles byteidentical. Strictinitial
manifestassert caughtthis; not silently ignored. See Inspection/wide-launch-check.json.
Do NOT start duplicatejobs, editAssets whileactive, or arm anothernotifier.
The existing local waiter queues completion into this same chat; no modelpolling.

## Next actions on completion
Read job/analysis/audit ONCE and verify ownedprocess exit. Preserve failures.
Review all intended completed evidence: Court attrition, Forest firstcards,
lateDungeon/shortcapstones, composition/mechanics/strategic counters. Tune only
small justifiedissues, then affected regressions AND new natural affected reruns.
Keep sourceversion/batches separate. Do not manufacture successfuldeep outcomes.
Complete final required validator + relevant tests + Android/Windows builds.
IMPORTANT existing CrystalResonanceBuild.cs hardcodes .utmp/CrystalResonance/Build;
old buildhelper would overwrite. Afterwide, allow a unique outputfolder (or other
safe approach) before final builds, keeping old artifacts intact. Existing
Test-Balance helper also uses fixedXML: use newoutput location, not overwrite.
Old regression failures remain evidence, not to be erased.

Review full relevantdiff includingnewfiles, update ownerstate/report A–F and
existing draftPR234. Push localcommits onlyafter actual finalvalidation/report.
No newPR, no merge. Verify finalclean shutdown and report limitations honestly.

Helpers in scratch /visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a:
cr_refine_background.ps1,cr_refine_notify.ps1,cr_refine_launch_group.ps1.
Do not rerun existing collection/report scripts: many deliberately refuse
existing targets; originals must remain intact. ThreeConsumable PNGmeta files
have reviewed whitespace-only Unity importnoise; leave until Unityexits then
guardedcleanup. No userasset changes. Gitwrites/worktreewrites requireescalation.
Every shell command rtk-prefixed. No subagents authorized.
