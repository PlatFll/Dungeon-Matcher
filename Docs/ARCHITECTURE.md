# Dungeon Matcher Architecture Reference

## Purpose

This document describes the current authoritative gameplay architecture and the invariants that future work must preserve. It names the implementation that exists in the repository; it does not convert every serialized prototype value into a permanent design rule.

> When documentation and implementation appear to disagree, do not silently assume either is correct. Identify the discrepancy and report it before changing behavior.

## Authoritative system map

| Area | Authority and principal files |
| --- | --- |
| Board state and resolution | `BoardController` partial class in `Assets/_Game/Scripts/Board/BoardController*.cs` |
| Clear identity and outcome | `BoardClearContext`, `BoardClearSource`, `BoardClearOutcome`, `BoardMatchType` |
| Gem state | `Gem`, `GemType`, `GemSpecialType` |
| Board-clear combat | `CombatController` and `GemDamageContext` |
| Player runtime state | `PlayerActor` |
| Player definitions and abilities | `PlayerDefinition`, `CharacterAbilityDefinition`, `PlayerAbilityController`, `PlayerAbilityEnergy`, `IPlayerAbilityRuntime` |
| Energy generation | `PlayerAbilityMatchEnergyGain` |
| Enemy data and runtime | `EnemyDefinition`, `EnemyDatabase`, `EnemyActor`, `EnemyAutoAttack`, `IEnemySpecialAbilityRuntime` |
| Waves and scaling | `WaveController`, `WaveSpawnProfile`, `DifficultyProfile`, `EnemyRuntimeStats` |
| Presentation | Board VFX controllers, enemy/player presenters, combat-text controllers, and UI components |

## BoardController ownership

`BoardController` is the authority for board rules and mutable grid state. Its partial files divide implementation by concern, but compile into one component and one ownership domain.

It owns:

- the `Gem[,]` grid, playable-cell state, swaps, matching, gravity, refill, and reshuffle;
- special-gem creation, activation, expansion, and chaining;
- mined cells, pinned gems, barricades, and their interaction with clears;
- board-clear contexts and the handoff to combat, healing, energy, and presentation consumers;
- acceptance and serialization of board-changing player abilities and enemy mutations.

`BoardController` must remain character-agnostic. Character and enemy runtimes may request generic board work, but must not mutate the grid, gem coordinates, obstacle dictionaries, or resolution state directly.

Do not create a second resolver, gravity pass, special-chain engine, or obstacle mutation coroutine outside this authority.

## Deterministic board-resolution pipeline

“Deterministic” here means that one accepted board action follows one authoritative, ordered resolution pipeline. It does not claim that random gem generation or random enemy target selection is seeded for replay determinism; those currently use Unity random selection.

For an ordinary player swap, `BoardController.TrySwap` performs this sequence:

1. Validate the two gems and acquire board ownership with `isBusy` before animation or mutation.
2. Swap the grid positions and visuals.
3. Resolve a supported color-crystal interaction, or find matches created by the swap.
4. Reverse an invalid ordinary swap without producing a valid-turn event.
5. For a valid match, call `ResolveCascades` and retain board ownership until it completes.
6. Release `isBusy`, then invoke `ValidPlayerMoveCompleted` exactly once for the accepted player move.

Color-crystal, double-color-crystal, and accepted board-changing ability paths have specialized orchestration, but reuse the same board-owned clear, special expansion, gravity/refill, cascade, and reshuffle operations. They are not independent board resolvers.

## Board busy and ownership rules

- The private `isBusy` flag represents active board ownership.
- Public `BoardController.IsBusy` is broader: it is true when `isBusy` is true or when `HasPendingBoardMutation` reports an active or queued structural mutation.
- Pointer input, selection, hints, forced reshuffles, and board-changing ability acceptance gate on the busy/pending-mutation state.
- Once an action is accepted, ownership must be acquired synchronously and held until all gameplay state, cascades, refill, and required reshuffle are complete.
- Never release the board between substeps of one logical action. That would allow overlapping input, duplicate resolution, or an enemy mutation to enter mid-cascade.
- `ProcessBoardMutations` waits for current board work, sets `isBusy`, drains the shared mutation queue serially, and releases ownership in `finally`.
- External systems should use accepted request methods such as `TryQueueMineRandomCell`, `TryQueuePinRandomGem`, `TryQueuePlaceBarricades`, and the matching cleanup queues. They must not start competing board coroutines.

The ownership invariant prevents double resolution, duplicate rewards, duplicate damage, duplicate mutation, and recursive trigger bugs.

## Clear, cascade, and refill flow

`ResolveCascades` is the ordinary resolution loop:

1. Build connected match groups and special-gem creation requests.
2. Expand the initial clear set through bomb/special chaining.
3. Report the authoritative match and additional special clears to combat and VFX with their source and cascade depth.
4. Run `ClearMatches`, preserving any gem selected to become a newly created special.
5. Resolve color crystals triggered by bomb paths according to the protected/queued crystal sequence.
6. Run `CollapseAndRefillBoard` and wait for movement and landing presentation to settle.
7. Scan the settled board for new matches and repeat with the next cascade depth.
8. When no matches remain, reshuffle if no legal move exists.

`ClearMatches` is the board-state removal point. `CollapseAndRefillBoard` compacts movable gems into playable destinations, respects non-playable cells and pinned gems, creates replacements, updates grid coordinates, and waits for their movement.

Environmental mutations call `ResolveEnvironmentalBoardChange`, which reuses collapse/refill, settled-board matching, cascade resolution, and reshuffle. Environmental destruction deliberately bypasses normal clear rewards unless a specific rule reports a clear context.

## Special-gem chaining

- `BuildBombExpandedClearSet` uses a queue plus visited/clear sets to expand row, column, poison, healing, and shield bomb effects without recursive activation or duplicate clears.
- Color crystals reached by bomb expansion are protected from the immediate explosion and represented by `BombTriggeredCrystalRequest`; they activate against the refilled board in the defined sequence.
- Normal color-crystal, bomb-plus-crystal, bomb-triggered-crystal, and double-crystal paths all return to board-owned refill, cascade, and reshuffle operations.
- `SpecialGemCreationRequest` identifies the one matched gem preserved as the new special. Only genuinely destroyed colored gems are rewardable.
- A color crystal's hidden original `GemType` is explicitly excluded from damage, healing, energy, and other color-based rewards.
- Expanded clear sets and visited sets are authoritative. Presentation must not independently discover or add gameplay targets.

Preserve current chaining and obstacle interaction semantics unless an approved feature explicitly changes them.

## Combat and damage flow

For normal and special board clears:

1. `BoardController` constructs a `BoardClearContext` containing gem type, rewardable gem count, trigger count, cascade depth, clear source, and match type.
2. It invokes `BoardClearResolved` for systems that consume the clear itself, including `PlayerAffinityHealing` and active clear-driven ability effects.
3. It passes the context to `CombatController.ResolveGemClear`, or to `ResolveFixedGemDamage` for the established fixed-damage ability path.
4. `CombatController` creates a `GemDamageContext`, raises `BeforeGemDamage`, and targets each active initialized enemy whose `AssignedGemType` matches the clear's gem type.
5. `EnemyActor.TryTakeDamage` is the final authority for enemy HP mutation and defeat.
6. `BoardController` emits `BoardClearOutcomeResolved` with whether a matching enemy was damaged. Energy generation consumes this outcome rather than trying to infer combat success separately.

This ordering keeps clear identity, weakness matching, damage modifiers, actual health mutation, and downstream rewards distinct while preventing duplicate reports.

Player damage follows its own actor authority: `EnemyAutoAttack` calls `PlayerActor.TryTakeDamage`, and `PlayerActor` resolves shield and HP effects. Healing and shield grants also enter through separate `PlayerActor` methods.

Specialized current paths must be understood before modification. `RoyalDecreeRuntime` applies its bonus through `EnemyActor.TryTakeDamage`, and `EnemyPoisonStatus` uses `EnemyActor.TryTakeDamageWithoutFeedback`; those paths keep health mutation in `EnemyActor` but do not pass through the normal `CombatController` gem-clear pipeline. Treat them as explicit existing behavior, not as permission for new features to bypass the appropriate established damage flow.

## Player definitions and ability runtime responsibilities

### `PlayerDefinition`

A `ScriptableObject` containing stable player identity, base maximum health, affinity gem type, presentation references, and active/passive definition references. `PlayerActor` is initialized from the selected or fallback definition and owns runtime health state.

### `CharacterAbilityDefinition`

An abstract `ScriptableObject` containing stable ability identity, UI data, energy cost, and an optional `RuntimeType`. Concrete definition types own ability-specific tunable data, not runtime state.

### `PlayerAbilityController`

The character-agnostic activation coordinator. It:

- reads the active definition from `PlayerActor`;
- locates a supporting `IPlayerAbilityRuntime`, adding the declared `RuntimeType` when necessary;
- checks player state, runtime state, runtime support, activation legality, and available energy;
- calls the runtime first and spends energy only after `TryActivate` succeeds;
- cancels the runtime if the post-acceptance spend unexpectedly fails;
- exposes state for the ability UI.

Runtimes may be serialized scene components or definition-declared components. The current scene serializes `RoyalDecreeRuntime`; `CrackedGemsAbilityDefinition` declares `CrackedGemsRuntime` for generic installation.

### Ability runtimes

`IPlayerAbilityRuntime` defines support, activation checks, accepted activation, active state, cancellation, and state-change notification. A concrete runtime owns character-specific target selection, duration/state, subscriptions, and presentation coordination. Board-changing runtimes must ask `BoardController` to accept and own the mutation.

Known implementation boundary: `BoardController.CrackedGems.cs` currently contains the board-owned resolution required by `CrackedGemsRuntime`, exposed through ability-named methods. The grid mutation still follows board ownership and the standard clear/combat/refill flow, but the API is less generic than the intended character-agnostic boundary. Report this discrepancy before any refactor or behavior change; do not use it as precedent for adding more character rules to `BoardController`.

## Ability energy separation

Energy has three separate owners:

- **Generation:** `PlayerAbilityMatchEnergyGain` subscribes to `BoardClearOutcomeResolved` and calculates gains from clear source, match shape, cleared count, and whether a matching enemy was damaged. It awards no energy for `BoardClearSource.Ability` and currently pauses generation while an ability is active.
- **Storage:** `PlayerAbilityEnergy` owns current energy, maximum energy, clamping, reset, addition, spending, and `EnergyChanged`.
- **Spending:** `PlayerAbilityController` checks the definition's cost and spends only after runtime acceptance.

Do not merge these responsibilities. In particular, a board clear should describe its source accurately; changing it to `Match` or `Bomb` to obtain energy would create an unintended refund path.

## Enemy data and runtime responsibilities

- `EnemyDefinition` is the per-enemy `ScriptableObject` for identity, prefab and presentation, base combat values, spawn eligibility/weight, category, special-ability kind and cadence, and ability-specific data currently represented there.
- `EnemyDatabase` supplies eligible weighted definitions.
- `WaveSpawnProfile` produces a category-based `WaveSpawnPlan`; `DifficultyProfile` converts a definition, wave, category modifiers, and player-power input into `EnemyRuntimeStats`.
- `WaveController` selects definitions, instantiates the configured prefab, initializes `EnemyActor`, assigns a gem weakness, initializes `EnemyAutoAttack`, and asks `EnemySpecialAbilityRuntimeFactory` to install the configured runtime.
- `EnemyActor` owns runtime HP, weakness, scaled stats, defeat, and the valid-player-turn counter that makes a special ready.
- `EnemyAutoAttack` owns continuous attack cadence and sends player damage through `PlayerActor`. Definitions may optionally provide one follow-up auto-attack hit; the primary and follow-up are independently scaled and resolved as separate `PlayerActor.TryTakeDamage` calls inside the same attack cadence.
- `MinerEnemyAbility`, `CrossbowGuardEnemyAbility`, and `BarricadeEnemyAbility` react to runtime events and request BoardController-owned mutations. They do not directly change the grid.

Shared board code must never switch on a concrete enemy identity. Add enemy behavior through definition data and an enemy runtime that requests generic operations.

## Wave and enemy board-manipulation timing

- `BoardController.NotifyValidPlayerMoveCompleted` is called once after a successful player swap has completely resolved. Cascades never call it independently.
- `WaveController` snapshots active enemies and calls `EnemyActor.RegisterValidPlayerTurn` once per active enemy for that completed move.
- Ready enemy runtimes may immediately queue board work. All mining, pinning, cleanup, and barricade requests share `pendingBoardMutations` and `ProcessBoardMutations`.
- The mutation processor waits until the player's resolution releases `isBusy`, then owns the board until every queued structural change, refill, resulting cascade, and reshuffle has settled.
- Cascades caused by enemy/environmental board changes do not count as additional player turns.
- Some enemy runtimes also listen to `ValidPlayerMoveCompleted` to retry a ready action when an earlier board state had no legal target; the ready state is not permission to mutate outside the queue.
- When a final enemy dies during board resolution, `WaveController.AdvanceToNextWaveWhenReady` waits for `BoardController.IsBusy` to become false before spawning the next wave. This prevents old cascades from damaging new-wave enemies.

## HP and shield system separation

`PlayerActor` owns separate `currentHealth`/`maximumHealth` and `currentShield`/`maximumShield` values, normalized values, and event streams.

- `Heal` affects HP only.
- `GrantShield` affects shield only and enforces the shield cap.
- `TryTakeDamage` applies the configured reduction when shield was active at the start of the attack, consumes shield, and applies any remaining damage to HP.
- Defeat is determined by HP, not shield; revival restores HP and resets shield.
- `PlayerPanelUI` presents shield separately from HP and consumes the separate events.
- `CombatController.HealPlayerFromBomb` and `GrantPlayerShieldFromBomb` call the corresponding distinct actor methods.

Never reuse HP fields/events for shield or change shield rules as a side effect of unrelated combat work.

## Gameplay and VFX/presentation separation

- Gameplay code computes authoritative targets, clear sets, damage, and state transitions before presentation consumes them.
- Board presentation is driven through contexts/events such as `GemMatchVFXRequested`, `BombVFXRequested`, `CrackedGemTargetsSelected`, and the color-crystal VFX context. VFX controllers must not add gameplay targets or mutate board state.
- `ClearMatches` and movement coroutines may wait for presentation timing, but the grid and clear sets remain authoritative.
- Missing optional sprites, presenters, or VFX components must fall back safely or skip presentation without changing the gameplay result.
- Animation Events may coordinate an enemy impact frame, but gameplay has timeout/fallback paths so a missing event or interrupted clip cannot leave an attack or board mutation soft-locked.
- Presenters such as enemy lifecycle/combat feedback, board VFX controllers, combat text, and player UI observe runtime events. They do not own HP, shield, weakness, wave, or board state.

## ScriptableObject and Unity serialization considerations

Unity serialization is part of the architecture, not an incidental editor detail.

- `PlayerDefinition`, concrete `CharacterAbilityDefinition` assets, `EnemyDefinition`, `EnemyDatabase`, `DifficultyProfile`, and `WaveSpawnProfile` carry gameplay configuration through serialized assets.
- `Assets/_Game/Scenes/Game.unity` serializes the live component graph and many tuned fields, including references among `BoardController`, `CombatController`, `WaveController`, `PlayerActor`, energy/ability components, and their data assets.
- `EnemyPrefab_General.prefab` provides the current common enemy runtime component base; `EnemyDefinition` assets reference the prefab and per-enemy data.
- Serialized scene, prefab, and asset values override C# field initializers. Changing a C# default alone does not update an existing serialized value.
- Script, asset, sprite, scene, and prefab links rely on GUIDs in `.meta` files. Preserve `.meta` files and inspect GUID references when moving or replacing serialized content.
- Renaming or changing the type of a serialized field can discard data unless migration is handled deliberately. Inspect all affected scenes, prefabs, and assets rather than assuming compilation proves the wiring is intact.
- All `BoardController` partial fields serialize onto the single `BoardController` component in the scene.
- Runtime-added components are not a substitute for required serialized data unless the architecture explicitly provides safe discovery/default behavior.

## Validation workflow

For C# gameplay, runtime, or editor changes, run from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1
```

The script reads the exact editor version from `ProjectSettings/ProjectVersion.txt`, locates that editor (or uses `UNITY_EXE`), launches Unity in batch mode, and fails on a nonzero exit or recognized compilation/project-open errors.

- Diagnose and fix failures caused by the change before treating implementation as complete.
- If Unity cannot validate because the project is open elsewhere, report validation as blocked.
- Run relevant automated tests when they exist.
- Distinguish static/code review from Unity runtime verification.
- Never claim Unity validation passed unless the script actually completed successfully.
- Documentation-only changes do not require Unity validation. Art-only changes that do not affect Unity serialization or runtime code also do not require it.
