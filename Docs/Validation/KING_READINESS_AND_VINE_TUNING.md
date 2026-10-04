# King readiness, vine pressure and ability counters

Date: 2026-10-04. User authorized implementation and merge.

## Changes and causes

- King's Assault could not reserve its commanded attack during the move
  coordinator's pause of autonomous timed basics. The counter stayed ready at
  zero, so the cycle never reached Bombardment. Explicit command reservations now
  pass that particular pause while retaining pause-menu, stagger, action-hold,
  death and occupied-action guards. The Captain uses the same corrected path.
- Commands retain their reservations through inter-strike recovery. The move
  coordinator waits for reservations, preventing input/save capture between the
  final strike and the special counter/cycle reset.
- Forest frontier spread is two additions per pulse instead of four; the growth
  cap is twelve instead of twenty-four. The edge seed and two-move cadence remain,
  so a full normal pulse adds at most three instead of five. Root placement still
  seeds four neighboring cells. Existing saved vines are not deleted by tuning.
- Ability counter containers and Thaleah text are enlarged in the shared enemy
  prefab. The tests inspect actual rendered glyph height, not just the serialized
  font-size setting.
- Editor batch runs set `EditorUtility.audioMasterMute`. This silences automated
  Unity testing without changing in-game sound settings; a regression assertion
  verifies the mute is active.

## Executed evidence

The original Assault bug was reproduced on the unchanged runtime after four real
accepted swaps: the command event count remained zero. Raw result:
`.utmp/ForestValidation/99623250-fced-4746-9bf9-9483f74c91c7.xml`.

Six focused gameplay checks passed after the command fixes. These cover the full
Judgment → Assault → Bombardment → Judgment cycle in both seconds-based profiles,
save/resume with a raised sword, recovery from a saved zero-counter state,
reservation safety guards, Captain commands, reduced vine spread/cap and resume.
Result: `.utmp/ForestValidation/79ff1dac-dcb9-4410-8302-23dbdc53ae9d.xml`.
That run's separate counter-rendering test failed, correctly exposing the old
container height shrinking the enlarged font. It is not final visual evidence.

The final gameplay regression suite passed **111/111**, with no skips:

| Fixture | Passed |
| --- | ---: |
| AcceptedMoveStateTests | 2 |
| EnemyAbilityPlayTests | 1 |
| EnemySpecialExecutionGuardTests | 19 |
| ForestFoundationPlayTests | 76 |
| RoyalAssaultParticipantLifetimeTests | 13 |

Command: `Tools/Test-ForestFoundation.ps1 -Graphics -Filter
"ForestFoundationPlayTests;EnemyAbilityPlayTests;EnemySpecialExecutionGuardTests;RoyalAssaultParticipantLifetimeTests;AcceptedMoveStateTests"`.
Raw results/log: `.utmp/ForestValidation/08218322-d9dc-4401-bcdb-4cc4b3e3a209.*`.
[Committed results](KingReadiness/regressions.xml) omit verbose test output only;
test names, outcomes and durations are retained.

The counter placement was then raised to clear the separate attack intent label.
Its expanded portrait test passed **1/1** on the final prefab at 720×1280,
1080×1920 and 1080×2400, checking King and Captain in adjacent occupied slots.
Values `4`, `0` and `12` fit their containers, use Thaleah, render at least fourteen
physical pixels high, stay on-screen and do not overlap the attack countdown.
This reruns one of the 111 checks; it is not an additional unique test.
Raw results/log: `.utmp/ForestValidation/d382ddcb-e865-4db4-a4d7-2dcf20ad611a.*`.
[Final counter results](KingReadiness/counters.xml).

Inspected actual Unity captures:

- [720×1280](KingReadiness/ability-counter-720x1280.png)
- [1080×1920](KingReadiness/ability-counter-1080x1920.png)
- [1080×2400](KingReadiness/ability-counter-1080x2400.png)

The scout's long basic timer in those captures is a fixture hold that keeps the
board stable while resizing; production timing is unchanged.

`powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1` completed
successfully with Unity **6000.3.19f1** after the final changes. Full log:
`C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-5412d1eb-056d-4697-a550-2708a070bf82.log`.
Only generated import metadata and Unity's equivalent TimeManager serialization
upgrade were restored in the isolated test worktree. The three pre-existing
consumable metadata edits in the primary workspace are preserved.

This record does not claim a repository-wide test pass or physical-device
verification. Earlier unrelated lifecycle/audio baseline failures remain
documented separately; the selected suite here has no failures.
