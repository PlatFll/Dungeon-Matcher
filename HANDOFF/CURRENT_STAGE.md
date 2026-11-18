# Crystal Resonance refinement — in progress, no merge

Existing branch codex/crystal-resonance-zone-director / draft PR #234.
Task start cae70c1d; clean at start and origin fetched. Other checkouts preserved.
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

Next: pilot (5 profiles, skeleton L1 seed19001), review actual outcomes, then
20 more natural cases (seeds19002–19005, four characters, L1/L5) and four deep
natural L10 attempts capped45min. Keep deaths/censoring, never force progress.
Scratch cr_refine_matrix.py config + cr_refine_background.ps1 waiter can run groups.
Do not start overlapping Unity jobs. The waiter records PID, exit, logs/artifacts
and attempts one same-chat codex queue notification, without retries/model changes.
Queue probe accepted ID01a1ec2d-1311-71a2-8fbb-586f15872bd6; wakeup not yet observed.
Check .utmp/CrystalResonance/Refinement/jobs for live/completed job state.

After evidence: investigate Court/Forest/capstones, only targeted justified
tuning + affected retests, final validator, Android/Windows builds, curated A–F
report. Push/update existing draft PR234; never merge or create another PR.
