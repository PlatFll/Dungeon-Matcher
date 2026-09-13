# Gameplay and bomb-reward source audit — 2026-09-13

Base reviewed: `57fabb2d612feb7d1bc4beacc4f596c4a470b694` (`main`, after PR #133).
Branch: `fix/gameplay-bomb-reward-audit`.

## Status and scope

The original GitHub-only source audit was followed by local Unity 6000.3.19f1 compilation, automated validation, and pointer-driven Game-scene playtesting on 2026-09-13. Local results and fixture corrections are recorded below. Physical-device testing was not performed.

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

- real L/T/Cross/Straight-5 classification and special-creation selection retaining an old bomb's coordinate, using the player's saved mastery choices read-only;
- Healing/Shield/Poison seeds, each paired with Row/Column/Crystal/Healing/Shield/Poison replacement rewards;
- repeated expansion remaining effect-free;
- two overlapping bombs committing twice total, not once or more than twice;
- old effect before replacement assignment, correct shatter timing for destroyed chained bombs, replacement survival;
- an ordinary preserved gem not activating its newly created special for each saved high-order mastery choice;
- explicit non-activation paths;
- utility commitments after the real encounter's damage/death completion;
- inactive-wave enemy damage/poison rejection, living-player utility acceptance, caps, defeat and initialization guards;
- Cross energy in damaging/non-damaging cases and ordinary Ability-source exclusion;
- Strong Remedy affecting calculated and actual affinity HP reward once, with baseline restoration; combined Strong Remedy/Reinforced Flask retaining the existing global and bomb-specific Healing Bomb modifier path;
- real-time transition checks for zero/nonzero delay and held/released gates when the board becomes busy again, followed by exactly one successful next-wave start with fresh enemy HP and poison state.

The transition fixture runs the production coroutine on Unity and acquires board ownership after its initial delay begins. It verifies both a held gate with an idle board and a released gate with a busy board, then releases ownership and observes successful spawning. The actual wave-5 choice UI is covered separately by the manual session.

## Local validation record

The original validator's HP fixture was invalidated by upgrade callbacks restoring the player's normal maximum HP. The large temporary HP fixture now starts after affinity-upgrade validation. Actual affinity healing first asserts sufficient maximum HP, then starts exactly the expected healing amount below that cap. Production HP, healing values, shield capacity, and upgrade balance were not changed.

The expanded Gameplay Bomb Rewards suite passed 46 scenarios. Gameplay Edge Cases, Gameplay Supplementary Cases, and Enemy Stagger Meter passed. Unity Test Runner passed all six RunUpgradeSystemTests with zero failures or skipped tests. The other existing NUnit suite is for pixel layout, outside this gameplay change; board, crystal, ability, and wave regressions are exercised by the named Play Mode validators.

Saved mastery remained Straight-5 = Color Crystal, L = Healing Bomb, T = Shield Bomb, Cross = Poison Bomb. No PlayerPrefs writes or preference resets were used.

Pointer-driven playtests used disposable runtime board fixtures in the actual Game scene and observed visual effects alongside actor events, energy totals, move completion, and wave events:

- Normal Poison, Healing, and Shield Bomb activation; two-bomb chains for each type. Healing granted 20 HP per bomb, poison applied once per bomb, and shield stopped at the existing 30 cap.
- Row Bomb collateral into Healing Bomb, Column Bomb collateral into Poison Bomb, and Shield Bomb collateral into a protected/refilled Color Crystal. Broader directional/crystal permutations also passed the existing automated validator.
- Color Crystal combined with each mastery bomb: the fixture's 14 Ruby gems converted and detonated; Healing and Poison produced 14 utility events, while Shield correctly saturated its cap.
- Bardley's actual ability button triggered Healing, Poison, and Shield collateral. The two Healing/Poison bombs each committed once; shield respected its cap. Ability energy matched entitled clears.
- Each old utility bomb participated in a real T-shaped swap, applied its effect once, and survived as the newly earned Shield Bomb without immediate self-activation.
- Wave-ending Healing and Shield Bombs were tested with a disposable longer shatter hold to ensure the encounter ended before utility commitment. Logs showed wave completion while the board was busy, the utility event while the wave was inactive, board settlement, and only then the next-wave spawn. The new enemies retained full HP.
- A runtime wave-5 completion fixture displayed the actual three-card choice and held progression for over 20 seconds. One pointer selection released it and wave 6 started once.

Every completed manual board action settled; recorded energy matched expected clear rewards. No new production defect was found. Temporary execution adapters, runtime fixtures, logs, and screenshots are excluded from the PR. The repository-required Unity-closed compilation result is recorded in the final PR validation report.

## Required local merge gate

1. Compile the branch in the project's pinned Unity version with no new errors.
2. Run the new Gameplay Bomb Rewards validator and existing Gameplay Edge Cases, Enemy Stagger Meter, relevant upgrade/EditMode tests and presentation regression checks.
3. Run the repository-required `Tools/Validate-Unity.ps1` following its normal Unity-closed workflow.
4. Visually test actual mastery-created Healing/Shield/Poison bombs, crystal conversions, chained bombs, and Bardley collateral. Confirm one visible detonation produces the expected actual effect, preserved new specials remain visible, and a wave-ending clear still heals/grants shield when eligible.
5. Confirm no old resolution attacks newly spawned next-wave enemies; verify the next wave does resume once the board settles and any choice is accepted.
6. Preserve local user art and scene edits. Do not commit temporary fixtures, changed PlayerPrefs, generated logs or test paint. Report existing unrelated shader warnings separately.
7. Record exact tested commit, outcomes and any fixes in the PR. Do not merge without review/approval.
