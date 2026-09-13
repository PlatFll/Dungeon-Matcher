# Gameplay audit integration handoff — 2026-09-14

## Status and scope

**INTEGRATION PREPARED / SOURCE-PRESERVATION REVIEWED / UNITY VALIDATION NOT RUN / DO NOT MERGE.**

Branch: `integration/gameplay-audit-validation`.
Baseline main: `4e2e959186492636d44c91c33514d65aa1f724c1` (validated PR #134).
Combined payload commit: `39e29570b11c348eb8fc33b5ce5cd7bde9d8d58b`.
Combined payload tree: `a77f3a5204c308c11895ebda3e7f46e189a32fa9`.
This handoff is the only integration-specific file added on top of that payload. Record the actual branch-head SHA when Unity validation eventually starts, including any later corrections.

The user stopped the broad source audit after chunk 20 and authorized combining the collected fixes. This freezes the current audit scope; it does NOT certify that every repository file/reference was inspected. Remaining nested references, scene/prefab wiring and presentation-completion coverage are deferred to a future audit. Historical instructions to wait for the entire source audit are superseded by this scoped handoff, but Unity testing and merging still require their next authorization.

All original gameplay PRs #135–#151 remain separate and unmerged. PR #152 is documentation-only coverage and was not included in this gameplay integration. No changes were made to main or the original fix branches.

## Pinned source heads

| PR / issue group | Included source commit |
| --- | --- |
| #135 / 1 | `62f68d82261401b901836b714963344ae038d35c` |
| #136 / 2 | `a516d39b7f0ed3af0dd8e602f51c00890d9db9c0` |
| #137 / 3 | `4b2a0dd2256568f0166f84bff5fcf9d8d59c78c7` |
| #138 / 4 | `a7ad79804c636bbf2e82f8591dd2f6b4429cfb22` |
| #139 / 5 | `cd9f41eaf3e1ba71aa3239b2a17742dc1096b23b` |
| #140 / 6 | `893353eae787528c9c5b66baf1e51edfcbc32231` |
| #141 / 7 | `2cd240bc6e5dc32d24cdbb1d59b98871e3352c2a` |
| #142 / 8 | `93d0582ab4fadde089da606d9690ba0cc03063b9` |
| #143 / 9 | `219275d48eb7b444902b8e970b314a526c384179` |
| #144 / 10 | `de9b3a6cd71c69c4661acf463ab02c5254797d5d` |
| #145 / 11 | `114c7f0e5f02070ef7d954c260d5fdb5ddf4a734` |
| #146 / 12 | `4aad2826d315a2f723dbcbdf2aa8c25a80428aa9` |
| #147 / 13 | `945a84e6f68086309174ba3a63045cc17a077dac` |
| #148 / 14 | `1538bf0ec2ef9973246575d21a0edb6f45a3c543` |
| #149 / 15 | `3ebbb9c08f916efdf5160df714938677e9a24de5` |
| #150 / 16 | `8539b42f08c01c659cecda47fd20826278065c3d` |
| #151 / 17 | `fc57605a88ab2d36c5528fc220c91bf8b55b6f3b` |

Dependency chains are **#142 -> #143** and **#142 -> #146 -> #149**. The included #149 head contains the later documentation confirmation that Bardley's cost=1 is intentional, rather than only its original opening head.

## Construction and source-preservation checks

This is a GitHub Git Data API integration, using existing file blobs by SHA rather than reconstructing source text. The payload commit has baseline main as its first parent and all 17 pinned source heads as additional parents. Original histories are retained; no source branch was rebased, force-pushed, retargeted or merged into its PR base.

The starting tree was #149's `3179d86d320a4f80cd6d4a4f3cb095ab1de03b1e`, which already contains #142 and #146. Its comparison against baseline main contains exactly 13 changed paths. The other 14 PRs contribute 71 unique incremental paths, including #143's five-file delta against #142. Those paths are mutually disjoint and do not overlap the 13-path inherited stack. No manual text-conflict resolution or new production fix was required.

In particular:
- `RunUpgradeGameplayHooks.cs` retains #142's lifecycle changes AND #146's center rewards.
- `BoardController.CrackedGems.cs` retains #146's center classification AND #149's chain scope.
- `RunUpgradeRuntime.RunReset` is present for #143's coordinator subscription.
- #137's actor result APIs, result type, combat/poison reporting and Royal Decree caller changes are included together.
- #144's encounter identity and summoning changes are included together; existing wave-event interfaces remain available to the upgrade systems.
- #145's Game Over caller and feedback cleanup method are included together.
- #141's board-validity changes do not overwrite #150's pin cancellation or #151's pointer wrappers/Gem forwarding.

Reviewed the production patches and the combined per-file diff scope. The payload comparison contains **84 paths**: 30 production C# files (29 modified and one new result type), 18 added editor C# files (16 NUnit suites, one opt-in scenario validator, one test probe), 19 added script metadata files, and 17 audit documents. There are no deleted/renamed files or serialized scene, prefab, ability/card asset, import-setting, package, art or layout-file changes. Adding this handoff makes 85 paths relative to baseline main.

All added metadata was preserved from its source PR. The 19 new metadata GUIDs are distinct within this added set. This is NOT an all-project duplicate-GUID check or Unity import certification. Compiler-level symbol/API validation and test-fixture execution remain unrun.

## Deferred validation manifest

All source suites are under `Assets/_Game/Scripts/Editor/`. Counts below are the audit's authored expectations, NOT discovered or passed test results. There are **306 expected NUnit cases across 16 suites**, plus **14 scenarios in one separate opt-in Play Mode validator**. Do not report these as 320 discovered NUnit tests.

| Group / PR | Suite or validator | Authored count | Detailed audit document under Docs/ |
| --- | --- | --- | --- |
| 1 / #135 | CrystalChainAttributionTests | 15 NUnit | CRYSTAL_CHAIN_AUDIT.md |
| 2 / #136 | PlayerAbilityLifecycleTests | 15 NUnit | PLAYER_ABILITY_LIFECYCLE_AUDIT.md |
| 3 / #137 | EnemyDamageResultTests | 24 NUnit | ENEMY_DAMAGE_RESULT_AUDIT.md |
| 4 / #138 | EnemyAttackLifecycleValidation | 14 scenarios, NOT NUnit | ENEMY_ATTACK_LIFECYCLE_AUDIT.md |
| 5 / #139 | TownMarshalStaggerTests | 9 NUnit | MARSHAL_STAGGER_READINESS_AUDIT.md |
| 6 / #140 | BarricadeBannerGravityTests | 12 NUnit | BARRICADE_BANNER_GRAVITY_AUDIT.md |
| 7 / #141 | CrystalBoardValidityTests | 24 NUnit | CRYSTAL_BOARD_VALIDITY_AUDIT.md |
| 8 / #142 | RunUpgradeLifecycleTests | 20 NUnit | RUN_UPGRADE_LIFECYCLE_AUDIT.md |
| 9 / #143 | UpgradeIntermissionLifecycleTests | 31 NUnit | UPGRADE_INTERMISSION_LIFECYCLE_AUDIT.md |
| 10 / #144 | WaveDeathLifecycleTests | 24 NUnit | WAVE_DEATH_SPAWN_LIFECYCLE_AUDIT.md |
| 11 / #145 | GameOverRecoveryTests | 28 NUnit | GAME_OVER_RECOVERY_AUDIT.md |
| 12 / #146 | CrackedCenterRewardTests | 22 NUnit | CRACKED_CENTER_REWARD_AUDIT.md |
| 13 / #147 | EnemySpecialExecutionGuardTests | 19 NUnit | ENEMY_SPECIAL_EXECUTION_GUARDS_AUDIT.md |
| 14 / #148 | RoyalAssaultParticipantLifetimeTests | 13 NUnit | ROYAL_ASSAULT_PARTICIPANT_LIFETIME_AUDIT.md |
| 15 / #149 | CrackedChainUpgradeScopeTests | 20 NUnit | CRACKED_CHAIN_UPGRADE_SCOPE_AUDIT.md |
| 16 / #150 | ReshufflePinReservationTests | 12 NUnit | RESHUFFLE_PIN_RESERVATION_AUDIT.md |
| 17 / #151 | BoardPointerIdentityTests | 18 NUnit | BOARD_POINTER_IDENTITY_AUDIT.md |

For group 4, use the existing `Dungeon Matcher > Validation > Enemy Attack Lifecycle` entry and record its scenario results separately. A normal NUnit run does not cover that validator automatically.

When the user assigns the combined validation stage, use this index and the uploaded validation backlog to cover every group. Run shared existing suites once per relevant combined state, rather than once per original PR: Gameplay Bomb Rewards, Gameplay Edge Cases, Gameplay Supplementary Cases, Enemy Stagger Meter, RunUpgradeSystemTests and relevant board/ability/Royal/Captain/wave regressions. Follow AGENTS.md for the pinned Unity version and `Tools/Validate-Unity.ps1`; close the project before batch validation if required.

Play Mode must cover the accumulated reproductions, not merely synthetic reflection fixtures. Cluster them into board/bomb/crystal/energy interactions; enemy damage/stagger/commands and obstacles; wave/spawn/summon/death ownership; upgrades/intermissions/recovery/Retry; and actual InputSystem multi-touch routing. Check real prefab/scene loading, player selections/mastery persistence, late-wave encounters and unchanged pixel presentation. Preserve the detailed edge cases in each linked audit; batching is not permission to skip them.

Classify each failure as production, integration, fixture, or intentional configuration before editing. Fix the root cause only, record the correction and affected group, rerun affected checks and the final shared regression gate. Never count missing, skipped, blocked or unrun cases as passes, weaken assertions to hide failures, or certify device rendering from synthetic fixtures alone.

## Invariants and authorization

**Bardley's serialized Cracked Gems cost MUST stay at 1.** The user intentionally uses this override to accelerate waves and inspect upcoming enemies. Do not restore 80 or silently change the documented non-testing default. Percentage-cost arithmetic may use disposable test definitions without editing production assets. The Aegis Reservoir 30-grant/30-cap question remains a separate unchanged balance issue.

Preserve PR #134, card values, shield behavior, ability costs/refunds, obstacle lifetimes, counter cadence, target rules, source entitlements, mastery/preferences, art/layout and pixel rendering. An accepted board sequence keeps ownership through shatter, refill and cascades; no stale work may damage a replacement wave.

**Validation at preparation time:** no Unity compilation, NUnit discovery/execution, repository Unity validator, Play Mode, Simulator or physical-device test was run. This is readiness for combined validation, NOT readiness for main merge or proof of bug-free gameplay.

Do not merge this integration PR or the original PRs merely because this branch exists. After validation and explicit merge authorization, reconcile any integration-only corrections into the chosen release/PR path. The eventual main production/test contents must match the tested state; recheck after merging. Keep test evidence tied to an exact commit, and preserve unrelated local work when checking out this branch.
