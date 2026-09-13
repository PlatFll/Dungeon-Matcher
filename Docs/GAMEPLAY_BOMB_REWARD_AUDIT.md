# Gameplay and bomb-reward source audit — 2026-09-13

Base reviewed: `57fabb2d612feb7d1bc4beacc4f596c4a470b694` (`main`, after PR #133).
Branch: `fix/gameplay-bomb-reward-audit`.

## Status and scope

This is a source-level audit with proposed fixes. Unity compilation, execution of the new validator, existing validators, visual playtesting and physical-device testing have **NOT RUN** in the GitHub-only editing environment. Do not treat the existence of a validator as a passing result. Keep this work in a draft PR until local validation is recorded.

The repository tree was inventoried. Detailed review focused on the gameplay paths involved in the reported symptom: ordinary match/cascade resolution, special creation, bomb expansion and shatter-time commitment, single/double/remote crystal paths, Cracked Gems, mining/targeted environmental removals, clear reporting, player HP/shield and affinity healing, ability activation/energy, enemy damage/poison, wave completion and intermission gates, Gem Mastery loadouts/settings/resolution, run-upgrade ownership/resolvers/hooks/drafting, existing regression fixtures, and relevant serialized Game-scene/upgrade values.

This is not certification that every file, enemy-specific ability, visual asset, device, or gameplay combination is bug-free. A complete all-platform execution sweep and deeper review of the remaining enemy-specific/presentation paths are still outstanding. No speculative fix was made solely because a path looked suspicious.

## Confirmed defects and fixes

### 1. Healing and Shield Bomb effects rejected after the wave ends

**Files:** `CombatController.cs`, callers in `BoardController.cs` / `BoardController.SpecialGems.cs` / `BoardController.PoisonBombs.cs`.

Clear damage is reported before the board-owned shatter moment. The old encounter can finish while a long resolution is still in progress. Both utility-bomb methods previously called `CanResolveCombat`, which rejected the reward when `WaveController.IsWaveActive` became false. The visual explosion and clear could therefore occur without the promised player utility.

**Fix:** separate player-effect eligibility from enemy-combat eligibility. A living initialized player can receive an already-activated Healing/Shield Bomb reward without a living encounter. Enemy damage and poison still require an active wave. Raw `PlayerActor` healing/shield ownership, caps and defeat checks remain intact. No reward is queued for future enemies and bombs do not revive a defeated player.

### 2. Existing bomb overwritten at a newly earned special's cell

**File:** `BoardController.cs`, `ClearMatches`.

The special-creation selector may preserve an existing bomb as the reward's location. The old bomb is included in expansion, but the preserved cell is excluded from destruction visuals. Committing utility effects only from `visuals` skipped that old bomb before `SetSpecialType` replaced it.

**Fix:** at the same shatter moment, visit the unique authoritative clear-set gems in grid order, including preserved seeds, before assigning replacement types. Each old utility bomb commits once; the newly earned special survives at the selected location and does not activate during creation. Preserved-cell ordinary gem rewards remain excluded. `activateSpecials: false` remains non-activating for environmental removals and double-crystal sweeps.

### 3. Global healing upgrades bypassed by affinity healing

**File:** `PlayerAffinityHealing.cs`, `CalculateHealing`.

The global `Healing` modifier (for example Strong Remedy) is supported by `RunUpgradeResolver`, but affinity healing returned the rounded base/cascade value directly. `PlayerActor.Heal` is a storage/capping API, not the missing upgrade calculation.

**Fix:** resolve the calculated affinity amount through `RunUpgradeResolver.ResolveHealing` once, at the existing computation point. Do not put a second modifier in `PlayerActor.Heal`; Healing Bombs already resolve global and bomb-specific modifiers separately.

### 4. Cross matches fell through to unclassified energy

**File:** `PlayerAbilityMatchEnergyGain.cs`, `CalculateMatchEnergy`.

`ReportMatchesToCombat` now distinguishes `CrossShape` for Gem Mastery. Energy handled T, L and straight-five, but omitted Cross, so the Cross result fell through to the lower Other reward.

**Fix:** Cross uses the existing T-family configured energy values, retaining the energy treatment it had before the shape distinction. No new balance constants, source reclassification or active-ability entitlement changes were introduced.

### 5. Next-wave spawning could miss a newly busy board

**File:** `WaveController.cs`, `AdvanceToNextWaveWhenReady`.

The coroutine checked board ownership before its transition delay, then waited only for progression gates. A new action could acquire the board during the delay or before gate release. The next encounter could then spawn into an unresolved action.

**Fix:** after the delay, require both an idle board and released progression gates in the same frame immediately before advancing. Preserve existing transition delay, wave identity guards, player-defeat checks and progression ownership.

## Gem Mastery conclusion

The reviewed settings/loadout resolver selects the special awarded for a higher-order shape; it does not gate the utility when that bomb detonates. No confirmed persistence/selection failure was found in that path. The preserved-bomb defect does interact with special creation, which can make the failure appear related to mastery. Cross classification also exposed a separate energy regression.

Do not reset the user's mastery preferences or alter their selected rewards to fix activation bugs.

## Content/balance discrepancy left for a decision

`Game.unity` currently configures Shield Bomb grant = 30 and player maximum shield = 30. The Aegis Reservoir asset promises +30% Shield Bomb grant. Under that unchanged cap, an ordinary Shield Bomb already fills the meter from empty; raising its grant above 30 cannot provide additional usable shield. This is a content/balance conflict, not evidence that the detonation was skipped.

The audit does **not** raise the shield cap, lower the base grant, change the card's mechanic or remove it from the draft catalog. Those are separate design choices. The actual scene Healing Bomb amount is 20 even though the component initializer is 30; neither was rebalanced here.

Full HP/full shield legitimately produce zero actual healing/shield gain. Poison requires a surviving eligible enemy. These cases must not be counted as lost activations.

## Regression validation added

Open a fresh Game scene outside Play Mode and run:

`Dungeon Matcher > Validation > Gameplay Bomb Rewards`

The opt-in fixture uses disposable Play Mode state and requires scene reload. It reads the current mastery selection without writing PlayerPrefs. Production board coroutines execute through Unity rather than manually skipping animation waits.

Coverage includes:

- real special-creation selection retaining an old bomb's coordinate;
- Healing/Shield/Poison seeds, each paired with Row/Column/Crystal/Healing/Shield/Poison replacement rewards;
- repeated expansion remaining effect-free;
- two overlapping bombs committing twice total, not once or more than twice;
- old effect before replacement assignment, correct shatter timing for destroyed chained bombs, replacement survival;
- an ordinary preserved gem not activating its newly created Healing Bomb;
- explicit non-activation paths;
- utility commitments after the real encounter's damage/death completion;
- inactive-wave enemy damage/poison rejection, living-player utility acceptance, caps, defeat and initialization guards;
- Cross energy in damaging/non-damaging cases and ordinary Ability-source exclusion;
- Strong Remedy affecting calculated and actual affinity HP reward once, with baseline restoration;
- transition yield-boundary checks for zero/nonzero delay and held/released gates when the board becomes busy again.

The transition fixture drives the existing IEnumerator at yield boundaries to test the missing condition; it is not a real-time duration test. Also manually test an actual next-wave spawn after release, ordinary gameplay swaps during the transition window, and the wave-5 choice UI.

## Required local merge gate

1. Compile the branch in the project's pinned Unity version with no new errors.
2. Run the new Gameplay Bomb Rewards validator and existing Gameplay Edge Cases, Enemy Stagger Meter, relevant upgrade/EditMode tests and presentation regression checks.
3. Run the repository-required `Tools/Validate-Unity.ps1` following its normal Unity-closed workflow.
4. Visually test actual mastery-created Healing/Shield/Poison bombs, crystal conversions, chained bombs, and Bardley collateral. Confirm one visible detonation produces the expected actual effect, preserved new specials remain visible, and a wave-ending clear still heals/grants shield when eligible.
5. Confirm no old resolution attacks newly spawned next-wave enemies; verify the next wave does resume once the board settles and any choice is accepted.
6. Preserve local user art and scene edits. Do not commit temporary fixtures, changed PlayerPrefs, generated logs or test paint. Report existing unrelated shader warnings separately.
7. Record exact tested commit, outcomes and any fixes in the PR. Do not merge without review/approval.
