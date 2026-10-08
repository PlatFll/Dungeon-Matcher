# Unified combat delivery checkpoint — 2026-10-08

## Scope and source

Implement the approved unified accepted-move combat experiment. The direct user
request explicitly authorizes merging after validation.

- Branch: `codex/unified-move-combat`.
- Validated implementation HEAD: `86dd73e5cc66b461c542ce56e030cebb9ce98352`.
- Base/main at validation: `b625cbb61d9929a0281edecedda79b0eb3b58ec2` (PR #183).
- `f1f5d434`: versioned integer clock and deterministic action phases.
- `b3af257d`: full future Stagger/effect durations.
- `84a48a3e`: 23 native PixelLab status/sword assets and provenance.
- `86dd73e5`: final UI, 62 roster intervals, runtime guards and regression coverage.

All implementation phases are complete. Remaining delivery work at this recorded
checkpoint is committing these docs/evidence, creating the PR, and the authorized
merge. No Unity or generation job remains running. Later docs-only commits do
not change the validated runtime source; use Git for the exact delivery/merge HEAD.

## Executed verification

- 479 unique automated cases: latest result all passed, zero skipped.
- Broad run `ae0ad0b5-979b-491a-a06b-b41c42f0638a`: 436/461. All 25 failures
  were repaired through explicit legacy fixtures, correct quiet basic counters,
  continuation readiness and actual PlayMode lifecycle callbacks.
- Repair run `33b60066-5065-4252-a0c7-9d5802a65246`: 43/44. The extra ore-power
  case passed in `cbcc931a-25cd-4e47-bbd1-023c9362faf8` after using real moves.
- Final visual run `9e1225c8-ecab-4f32-b581-0f5aa28fe7e5`: all four cases passed,
  covering 16 zone/portrait combinations, large leader and two healer channels.
- Required `Tools/Validate-Unity.ps1`: passed, Unity 6000.3.19f1, exit 0.
  Full log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-8091e075-1a50-4c9b-adb8-090813d0ad7b.log`.
- Actual captures, case inventory, pacing samples and 496 Stagger calculations:
  [validation record](../Validation/UnifiedCombat/README.md).

Original failed attempts remain in the evidence inventory. No known failing
case is accepted as passing. Numerical tuning is experimental. Human elapsed
pacing, physical-device reading and sustained late-run balance remain unmeasured.
No wave-count, HP/damage, blocker-cap or encounter-weight change was made.

## PixelLab and usage

Approved: 60 subscription units total / 30 initial / 30 reserve, no credit purchase.
Used: 26.3 units (24 creations + 23 palette reductions); reserve untouched.
Final reported balance: 1038 generations, $0 credits. All 23 final native icons
are integrated and technically checked. No art jobs or correction requests remain.
Sources, prompts, job IDs and checks are in `ArtSource/UnifiedCombat`.

Latest measured Astra usage: 91% used / 9% remaining. This is a measurement at
delivery preparation, not an estimate of later remaining usage. The 5/3/1% user
stop gates remain applicable to any continuation.

## Preserved user changes

These four starting edits remain outside the implementation commits, unchanged
according to `.utmp/StatusRevision/UserChanges/manifest.json`:

- `Assets/_Game/Resources/BattleEnvironments/DrownedCourt_Throne.prefab`
- `Assets/_Game/Resources/UI/Consumables/SlotDisabled.png.meta`
- `Assets/_Game/Resources/UI/Consumables/SlotHighlighted.png.meta`
- `Assets/_Game/Resources/UI/Consumables/SlotPressed.png.meta`

The unrelated Unity TimeManager serialization rewrite was removed. Runtime and
art changes are committed; only this delivery documentation/evidence is pending
at this checkpoint. Before resuming, inspect actual Git/PR state rather than
repeating production or tests whose source has not changed.
