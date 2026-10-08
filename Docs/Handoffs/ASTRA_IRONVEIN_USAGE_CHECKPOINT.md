# Ironvein review checkpoint

2026-10-08. Branch `codex/ironvein-excavation`; all eleven implementation phases
delivered for review in [PR #183](https://github.com/PlatFll/Dungeon-Matcher/pull/183).
**Merge explicitly authorized on 2026-10-08.** Validated implementation/evidence
commit: `5b72270eca8e1b9f558cafd77afbc23cb302e6a7`. Documentation-only review and
integration receipts follow it; use `git log -1 --oneline` for exact HEAD.

## Implemented

Thirteen enemies plus turret, persistent evolving stones, small first-stone
and fixed full-lane drills, ore power, specialist/miniboss/boss kits and saved
optional remount (disabled). Fourteen native stills, 108 clips, three cave
scenes, industrial HUD, mechanism VFX and original temporary audio. Forty-one
weighted formations, saved teaching/relief, four-zone travel and mine Guide.

## Validation

56 current mine/affected cases pass on their latest executions. Final previous-
zone regression: 157 passed, zero failures/skips; result/log
`.utmp/ForestValidation/f7a700b6-bc91-419d-9f31-b58afa15ef50.xml/.log`.
Mandatory `Tools/Validate-Unity.ps1` passed with muted Unity 6000.3.19f1; log
`C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-60755fbb-8d9f-488b-8ef4-bfaa4a695788.log`.
Source/GUID checks and four portrait captures pass. Three synthetic-input visits
reached travel in 465.129–638.667 game seconds without stat/cadence overrides.
Full execution history and remaining limits: `Docs/IronveinExcavation/VALIDATION.md`.
Native evidence and playtest entry: `Docs/Validation/Ironvein/README.md`.

## Review gates / exact next action

Integrate the validated PR #183 under the user's explicit merge authorization.
After integration, continue user playtesting. User visual, listening, human pacing and
physical-device approval remain pending. Helpful drill opportunities were rare
under the greedy automated policy. Do not tune HP to force a visit duration.
No further generation is needed
before this review; any corrections must retain the existing allowance ledger.

The local review is `http://127.0.0.1:8891/Review.html`, served by PID 32096;
state/logs are `.utmp/Ironvein/review-server.json` and `review-server.*.log`.
It actually runs independently of this turn. If closed, restart using
`Tools/Serve-IronveinReview.ps1`. No Unity, generation or download jobs remain.
No automatic implementation continuation is scheduled.

## Preserved state and allowance

Read current instructions and `Docs/IronveinExcavation/READINESS.md`.
The four original user files listed there remain unchanged and outside commits.
Pack: `.utmp/IronveinPack/Dungeon_Matcher_Ironvein_Excavation_Astra_Pack/`.
Source provenance: `ArtSource/Ironvein/README.md` and `Production/`.
Final source review excluded unrelated files; no existing approved art replaced.

PixelLab materials authorized; 400 total / 300 initial / 100 reserve, no purchases.
Quoted usage **295.2 across 185 requests**, including one cancelled request
conservatively. Reserve untouched. Latest balance: 935 used / 1,064 remaining,
$0 purchased credits. `Tools/Record-IronveinProduction.py` rebuilds the ledger.
Last actual Codex quota: **72% used / 28% remaining**. Refresh on resuming.
5%: no new phase; 3%: validation/handoff only; 1%: stop all new work.
