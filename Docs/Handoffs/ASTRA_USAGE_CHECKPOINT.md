# Revision checkpoint — 2026-10-06

Branch: `codex/status-sigils-court-revision`.
Starting HEAD/main: `2783df8eb9107c182db5d9c768e2a36c799cfc35` (merged PR #180).
Task source: `C:/Users/USER/.codex/attachments/2bbe0e9d-d8e9-4dd2-a040-80ab8e06593c/Pasted text.txt`.
No merge authorized for this revision. Single writer; no paid generation needed.

## Tranches

1. Design rules, canonical status contract and future-only locks.
2. Shared status runtime, centralized hooks, UI and persistence; focused tests.
3. Slippery preview and board-owned final-arrangement swaps; focused tests.
4. Cast announcements and slot sigils across existing warnings; focused tests.
5. Court culture taxonomy/recipes, two shark-tail edits and native review.
6. Broad affected regression, portraits, mandatory Unity validator and PR.

Current state: orientation/audit complete; tranche 1 documentation in progress.
No new runtime or art implementation yet. No tests run for this revision yet.
Remaining account usage last checked: 28%; stop starting tranches at 5%, prioritize
validation/handoff at 3%, no new work at 1%. Recheck at each tranche boundary.

## Preserve unrelated starting changes

- `Assets/_Game/Resources/BattleEnvironments/DrownedCourt_Throne.prefab`
- `Assets/_Game/Resources/UI/Consumables/SlotDisabled.png.meta`
- `Assets/_Game/Resources/UI/Consumables/SlotHighlighted.png.meta`
- `Assets/_Game/Resources/UI/Consumables/SlotPressed.png.meta`

These four files were modified on main before work began. Do not stage/reset them.
Starting `git status`: main tracked origin/main; four modified paths above.

## Audit findings and next action

Existing EnemyDefinition has race/faction/role strings; reuse them. BoardController
owns accepted swaps, with CombatMoveClock coordinating complete moves. PlayerActor
owns damage/healing; generated energy is separate from PlayerAbilityEnergy storage.
The current Court docs incorrectly still describe #180 as unmerged; correct that
without treating merge approval as approval of unseen art. Needlefin also lacks a
clear shark-tail silhouette; report it only. Authorized edits are Hammerhead/Captain.

Next: commit tranche 1 after review, then inspect shared status/presentation owners
and implement tranche 2. Preserve exact validated commits and evidence here as work
advances. No running jobs or automatic background continuation are pending.
