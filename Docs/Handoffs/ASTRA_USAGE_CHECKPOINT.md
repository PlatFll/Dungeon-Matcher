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

Current state: tranche 1 committed as `7c972f1` (design contracts/future locks).
Tranche 2 backend committed as `df8b06f`: seven data definitions, player-owned durations,
central damage/healing/generation hooks, Fear source lifecycle, persistent dungeon
enemy IDs and continuation. Status UI/icons and optional data-selected casting are
implemented and validated. Slippery movement remains.
Focused status/damage/continuation tests passed 38/38 in
`.utmp/ForestValidation/d68008c9-6c36-4714-bf0b-ae8aaca5dd2e.xml`.
An initial 4/7 run exposed isolated test-fixture initialization errors, now fixed.
Complete-action expiration and legacy/forest save-resume passed 2/2 graphics-enabled
live tests: `.utmp/ForestValidation/79c1bdb3-9dbe-446d-993f-36dd2df0fded.xml`.
The first headless live run failed because it could not create the required game
viewport; the graphics-enabled rerun passed. The 8 isolated status tests also pass.
Required validator passed with Unity 6000.3.19f1; log:
`C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-dfe34e86-d837-41c5-a986-660b9ff28064.log`.
UI/casting asset and runtime tests passed 2/2 in
`.utmp/ForestValidation/6df3a9ba-5f29-456e-b051-58f99eb9fb4d.xml`; its third test
caught a 3px status-row overflow. Corrected spacing passed all four viewport
variants in `.utmp/ForestValidation/9534cc12-9e10-4b33-84d6-07b97d4822c7.xml`.
Actual captures: `.utmp/StatusRevision/Visual/statuses-*.png` (720x1280,
1080x1920, 1080x2400, 1080x1920-safe). Native icon proof and checks:
`ArtSource/SharedStatus/NativeIcons.png` and `NativeChecks.json`.
Required validator passed again; log suffix `094aa894-b46d-4f92-a996-e1d2a0e0ec59`.
Remaining account usage last checked: 25%; stop starting tranches at 5%, prioritize
validation/handoff at 3%, no new work at 1%. Recheck at each tranche boundary.

## Preserve unrelated starting changes

- `Assets/_Game/Resources/BattleEnvironments/DrownedCourt_Throne.prefab`
- `Assets/_Game/Resources/UI/Consumables/SlotDisabled.png.meta`
- `Assets/_Game/Resources/UI/Consumables/SlotHighlighted.png.meta`
- `Assets/_Game/Resources/UI/Consumables/SlotPressed.png.meta`

These four files were modified on main before work began. Do not stage/reset them.
Exact copies and SHA-256 hashes: `.utmp/StatusRevision/UserChanges/manifest.json`.
Starting `git status`: main tracked origin/main; four modified paths above.

## Audit findings and next action

Existing EnemyDefinition has race/faction/role strings; reuse them. BoardController
owns accepted swaps, with CombatMoveClock coordinating complete moves. PlayerActor
owns damage/healing; generated energy is separate from PlayerAbilityEnergy storage.
The current Court docs incorrectly still describe #180 as unmerged; correct that
without treating merge approval as approval of unseen art. Needlefin also lacks a
clear shark-tail silhouette; report it only. Authorized edits are Hammerhead/Captain.

Next: implement Slippery preview, board-owned rotation and final-arrangement
legality/hints; no production assignment. Then global announcements/sigils,
Court cultures/recipes, direct shark-tail pixels, final regression/docs/PR.
No automatic background continuation is configured.
