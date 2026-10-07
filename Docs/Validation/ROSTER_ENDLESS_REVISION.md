# Roster / endless revision evidence — 2026-10-07

Branch `codex/roster-endless-revision`, based on merged main
`e572a88227d036af765e057831c46a2cc067630d`. This report covers completed
slices of [the approved worklist](../ROSTER_ENDLESS_REVISION.md), not the
whole requested revision. No merge is authorized.

## Channel lifecycle

Unity 6000.3.19f1, actual Game scene, disposable account saves, Editor audio muted.

- Focused success/fizzle/Stagger/legacy recovery continuation: **10 passed**,
  zero failed/skipped. `.utmp/ForestValidation/bbf334b6-afad-423d-a361-67b6e8e964c9.xml`.
- Broader affected combat, sigils, announcements, continuation and eighteen-zone-handoff
  regression: **15 passed, 3 failed** initially.
  `.utmp/ForestValidation/1064d3cc-7859-4fa0-b2b3-e71d6946a1a9.xml`.
  Queen and Captain tests still expected the retired two-move wait. They now
  assert the unchanged three-move cadence and full response window, then immediate
  release. The photograph fixture supplied only an edge match, whereas root sites
  require four neighbors; it now supplies an explicit interior match before asking
  for two warnings. Gameplay placement safety was not changed.
- All three affected reruns **passed**, zero failed/skipped:
  `.utmp/ForestValidation/c333c219-e8a9-4bd9-9081-dca84a464b03.xml`.
  Across the focused and broader sets, **28 unique tests passed**.
- Required `Tools/Validate-Unity.ps1` **passed**, exit 0, Unity 6000.3.19f1.
  Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-a99b67aa-795c-425c-a8c9-f63a5f5204a4.log`.
- The four original user modifications still match their starting SHA-256 hashes.

No visual assets changed in this slice. Final four-viewport review and whole-revision
regression remain pending. Earlier PR #181 evidence is a baseline, not a claim that
this new revision has passed those same checks.

## Audit discrepancy

`PROGRESSION_PACING.md`'s historical “Initial connected forest visits (2026-10-03)”
paragraph still describes two destinations and unavailable aquatic travel. Actual
main has three eligible connected zones and the Court contract records that newer
behavior. Do not revert implementation to that older paragraph; reconcile it when
updating the final progression documentation.
