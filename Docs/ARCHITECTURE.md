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
| Run upgrades | `RunUpgradeDefinition`, `RunUpgradeCatalog`, `RunUpgradeRuntime`, `RunUpgradeResolver`, `UpgradeDraftGenerator` |
| Presentation | Board VFX controllers, enemy/player presenters, combat-text controllers, and UI components |

## Board and modular frame presentation

- Modular board sprites use Full Rect import meshes. Tight corner triangulation can distort the one-pixel border under pixel snapping and make its join with a rectangular straight strip look stepped, despite matching sprite bounds and transforms.
- `BoardVisuals` creates the board background, cell tiles, rectangular gem mask, and modular frame. It assigns frame sorting once: `Effects / 100`, above board content on `BoardBackground` and `Gems`, below `WorldUI` and screen-space overlay canvases. Frame sprites are unmasked. Board motion, obstacles, and VFX must stay below this border; neither animation nor gameplay code owns frame sorting.
- There is no `BoardFrameSortingGuard` or additional `BoardFrame` sorting layer. An unavailable sorting-layer name can resolve to `Default` in an editor that has not reloaded project settings; a per-frame override must not replace valid initialized sorting.
### Gameplay pixel layout ownership

- `BattleBackgroundTilemapController` owns world-background placement and clipping. Its generated rectangular SpriteMask follows the TopHUD interior inset by the native 16px frame and covers only the authored Default/-100 through -70 background orders. The board mask covers BoardBackground through WorldUI, excluding the background without changing board SpriteRenderer or UI sorting. Tilemaps retain authored unit scale, cell size and 64 PPU. The map origin is snapped to the 1/64 world grid after the Pixel Perfect Camera projection update, and the floor marker consumes that snapped baseline. The screen-space battle frame remains above the world background. `BattleBackgroundViewportValidation.Run` checks oversized-tile leakage, production board-mask isolation, scale and floor alignment at 1080x1920 and 1080x2400 in a graphics-enabled batch editor (omit `-quit`; the validation exits after its frame loop completes).

- `GameplayPixelLayoutController`, installed on the gameplay safe root by `RuntimeScenePresentationBootstrap` for every scene load, is the only writer of that root, `TopHUD`, `GameArea`, and `BottomHUD`. `SafeAreaFitter` is disabled on that root; other scenes may continue using it. Device constraints come from `Screen.width`, `Screen.height`, and `Screen.safeArea`.
- The gameplay Canvas uses Constant Pixel Size at the largest positive integer scale fitting the native 448px minimum battle-content width plus safe padding. This minimum derives from the 146px player section, section/content margins, and three 80px enemy-slot allocations. Vertical feasibility (220px battle minimum, 176px bottom, two 6px gaps, and the board) can reduce that integer scale. The old 540x960 reference does not impose arbitrary scale thresholds. Canvas reference PPU remains 64. The safe viewport is rounded inward to this logical grid with four additional logical pixels on each edge. Decorative backgrounds may remain edge-to-edge; no interactive section relies on Android reporting a navigation inset.
- The stack has two gaps of at least six logical pixels. The bottom enclosure is 176 logical pixels: two native 80px corners plus a 16px straight section. A centered 64px ability button has 40px vertical breathing room inside the 16px borders. The battle region has a 220px minimum, 320px preferred height and upper bound; the current policy stops at the preferred height. TopHUD and BottomHUD are anchored to the upper and lower safe viewport edges. The square board is centered between them; surplus height is split between the two gaps with at most one logical pixel difference. This replaces the bottom-packed stack. Use Screen.safeArea rather than a guessed camera-hole height; the existing inward rounding and four logical pixels provide additional clearance.
- `BoardLayoutController` consumes the assigned `GameArea`. Its render callback is registered in Start, after URP Pixel Perfect Camera's OnEnable callback, so it measures the actual updated projection before fitting. It uses the assigned uniform fit ratio and converts that ratio to world scale using the current camera mapping and Assets PPU (64). The board center is snapped in physical screen coordinates. PPC's per-renderer world-grid snapping stays off: that separate 1/64-world-unit grid conflicts with a board scaled by 0.5 under a 2x camera projection. Render scale remains 1 and MSAA remains off.
- **Phone board sizing:** the board fills the available width or remaining height, whichever limits it, using uniform fractional scaling when needed. This supersedes the earlier integer-only / below-552px exception following phone feedback: that policy nearly halved the board width at 1080px. Canvas and HUD art retain integer scaling; board source texels can occupy uneven physical pixel widths under Point filtering. The validator checks the assigned ratio and uniform frame scaling rather than requiring integer board texels.
- `ResponsiveModularFrameFitter` owns only frame internals and keeps native corners/edge density. Tiling may terminate at a whole source-texel boundary; partial texels and arbitrary tile-density scaling are forbidden. `BottomHudModularFrameController` builds the shared frame but never resizes the HUD or positions the ability. Mirrored frame pieces retain unit-magnitude axis signs for reflection.
- `PlayerAreaThreeSliceFrameController` retains the custom player frame and owns its section geometry, including the arena's native 16px border plus a six-pixel interior gap. The player section retains its pre-expansion height: min(290, available TopHUD height) minus the existing 22px top/bottom insets, vertically centered in the battle area with at most half a logical pixel of grid rounding. Its character/base/background floor receives the same centering offset so the player HUD retains its internal arrangement. Only the battle enclosure uses the increased 320px preferred height. The former `PlayerAreaFrameSpacingController` is removed. `TopBattleLayoutController` builds battle content; `TopBattlePresentationController` consumes the assigned height and positions the floor and contents without partitioning the screen.
- `AbilityButtonUI` owns its 176x64 button, native 64x64 energy art, and display-only integer fill steps. It never resizes its enclosing HUD. Pixel-art UI roots use `(1,1,1)`, with native dimensions or source-texel-compatible RectTransform sizes. `GameplayPixelGrid` and `PixelPerfectBattleCharacterUI` use the same physical screen phase; character width and height share one integer physical source-texel ratio rather than independently rounded dimensions. Board gems use native presentation scale; generation, matching, energy, damage, and shield behavior remain independent of this presentation policy.
- Editor/development `GameplayPixelLayoutValidator` reports device/logical/physical rectangles, gaps, clearance, source-texel ratio, board origin, frame piece bounds, scale, missing pieces, and known competing layout components. `GameplayPixelLayoutTests` provides 32 pure geometry cases and an opt-in actual-scene screenshot matrix under `.utmp/PixelLayout`, including deliberate invalid-state checks. Screenshot inspection remains separate from numeric assertions.
- `Dungeon Matcher > Validation > Board Frame Play Mode` exercises production swaps using temporary Play Mode fixtures and saves native Game View screenshots plus a renderer report under `.utmp/FrameVerification`. Passing renderer assertions does not replace visual inspection of the captured frames.

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
- Expansion computes targets without applying bomb effects. Genuine activation paths opt into `ClearMatches` special activation; it commits effects when all gem shell/icon renderers hide at shatter. Environmental removal and double-crystal sweeps retain non-activating removal semantics.

Preserve current chaining and obstacle interaction semantics unless an approved feature explicitly changes them.

## Combat and damage flow

For normal and special board clears:

1. `BoardController` constructs a `BoardClearContext` containing gem type, rewardable gem count, trigger count, cascade depth, clear source, and match type.
2. It invokes `BoardClearResolved` for systems that consume the clear itself, including `PlayerAffinityHealing` and active clear-driven ability effects.
3. It passes the context to `CombatController.ResolveGemClear`, or to `ResolveFixedGemDamage` for the established fixed-damage ability path.
4. `CombatController` creates a `GemDamageContext`, raises `BeforeGemDamage`, and targets each active initialized enemy whose `AssignedGemType` matches the clear's gem type.
5. `EnemyActor.TryTakeDamage` is the final authority for shield-aware enemy damage, HP mutation, and defeat.
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

- **Generation:** `PlayerAbilityMatchEnergyGain` subscribes to `BoardClearOutcomeResolved` and calculates gains from clear source, match shape, cleared count, and whether a matching enemy was damaged. `BoardClearContext.GrantsSpecialEnergy` permits player-owned bomb/crystal clears and explicitly opted-in Cracked explosions during abilities. Other Ability clears remain ineligible and ordinary match energy remains paused during active abilities.
- **Storage:** `PlayerAbilityEnergy` owns current energy, maximum energy, clamping, reset, addition, spending, and `EnergyChanged`.
- **Spending:** `PlayerAbilityController` checks the definition's cost and spends only after runtime acceptance.

Do not merge these responsibilities. In particular, a board clear should describe its source accurately; changing it to `Match` or `Bomb` to obtain energy would create an unintended refund path.

Enemy white-flash presentation has one material writer, `EnemyCombatFeedback`. Poison requests a timed hit flash from that owner; expiry combines with current stagger state instead of restoring a captured temporary value. Disable and defeat clear temporary state.

Player shield combat numbers consume `PlayerActor.ShieldDamaged`, which contains actual shield loss after mitigation. HP numbers continue to consume actual `DamageTaken`, in a separate display lane. At zero shield, `PlayerPanelUI` deactivates and destroys the complete runtime shield overlay immediately.

## Enemy data and runtime responsibilities

- `EnemyDefinition` is the per-enemy `ScriptableObject` for identity, prefab and presentation, base combat values, spawn eligibility/weight, category, special-ability kind and cadence, and ability-specific data currently represented there.
- `EnemyDatabase` supplies eligible weighted definitions.
- `WaveSpawnProfile` produces a category-based `WaveSpawnPlan`; `DifficultyProfile` converts a definition, wave, category modifiers, and player-power input into `EnemyRuntimeStats`.
- `WaveController` selects definitions, instantiates the configured prefab, initializes `EnemyActor`, assigns a gem weakness, initializes `EnemyAutoAttack`, and asks `EnemySpecialAbilityRuntimeFactory` to install the configured runtime.
- `EnemyActor` owns runtime HP, shield, weakness, scaled stats, defeat, and the valid-player-turn counter that makes a special ready.
- `EnemyAutoAttack` owns continuous attack cadence and sends player damage through `PlayerActor`. Definitions may optionally provide one follow-up auto-attack hit and a non-negative delay after the primary presentation's completed-return acknowledgement. The primary and follow-up are independently scaled and resolved as separate `PlayerActor.TryTakeDamage` calls inside the same attack cadence. For a follow-up sequence, `EnemyAutoAttack` retains action ownership while `EnemyCombatFeedback` acknowledges each generic lunge's impact and completed return using that hit's presentation ID. The next hit cannot begin before the required return acknowledgement and configured follow-up delay, and the cooldown cannot begin before the final return; one-shot guards and real-time fallbacks prevent duplicate damage or presentation-dependent stalls. Definitions with no follow-up retain the established single-hit path.
- `MinerEnemyAbility`, `CrossbowGuardEnemyAbility`, and `BarricadeEnemyAbility` react to runtime events and request BoardController-owned mutations. They do not directly change the grid.
- `ShieldingAlliesEnemyAbility` consumes the authoritative active-enemy roster supplied by `WaveController` and grants shield through each living target's `EnemyActor`. It does not mutate the board, HP, or UI directly.

Shared board code must never switch on a concrete enemy identity. Add enemy behavior through definition data and an enemy runtime that requests generic operations.

## Wave and enemy board-manipulation timing

- `BoardController.NotifyValidPlayerMoveCompleted` is called once after a successful player swap has completely resolved. Cascades never call it independently.
- `WaveController` snapshots active enemies and calls `EnemyActor.RegisterValidPlayerTurn` once per active enemy for that completed move.
- Ready enemy runtimes may immediately queue board work. All mining, pinning, cleanup, and barricade requests share `pendingBoardMutations` and `ProcessBoardMutations`.
- The mutation processor waits until the player's resolution releases `isBusy`, then owns the board until every queued structural change, refill, resulting cascade, and reshuffle has settled.
- Cascades caused by enemy/environmental board changes do not count as additional player turns.
- Some enemy runtimes also listen to `ValidPlayerMoveCompleted` to retry a ready action when an earlier board state had no legal target; the ready state is not permission to mutate outside the queue.
- When a final enemy dies during board resolution, `WaveController.AdvanceToNextWaveWhenReady` waits for `BoardController.IsBusy` to become false before spawning the next wave. This prevents old cascades from damaging new-wave enemies.

Wave transitions also expose generic registered `IWaveProgressionGate`
instances. The run-upgrade coordinator holds this gate after waves divisible by
five, waits for the board to settle, presents the choice, and releases the gate
after one accepted selection. `WaveController` does not know about upgrade UI,
and the UI never increments waves or spawns encounters.

External modal gameplay input uses disposable reference-counted tokens owned by
`BoardController`. These block pointer begin/end, selection, drag/swipe, and swap
acceptance without marking an already-running resolution busy or interrupting
it.

## Run upgrade ownership and resolution

`RunUpgradeRuntime` is scene/run scoped and is the authority for selected stable
upgrade IDs and stack counts. It resets on a new battle-scene run and never
persists temporary upgrades through `PlayerPrefs`. Definitions and the catalog
are immutable `ScriptableObject` data; runtime application never mutates them.

`UpgradeDraftGenerator` filters wave bounds, stable `PlayerId`/`AbilityId`, max
stacks, prerequisites, and bidirectional exclusions, then performs weighted
selection without replacement. Its dedicated `System.Random` is seeded from the
encounter seed through a stable mix but never reads or advances the encounter
RNG instance.

Gameplay systems query the side-effect-free `RunUpgradeResolver` only at their
existing authoritative resolution point. Numeric ordering is base, summed flat,
summed additive percentage, stable-ID multiplicative, then clamp/round. Missing
runtime state returns the base value. Current hooks are normal gem-clear damage
(fixed ability damage remains distinct), maximum HP, healing, shield grants,
ability-energy gain/cost, barricade durability damage, and Cracked Gems target
count. Typed mechanic capabilities are the extension boundary for non-numeric
behavior.

## HP and shield system separation

`PlayerActor` owns separate `currentHealth`/`maximumHealth` and `currentShield`/`maximumShield` values, normalized values, and event streams.

- `Heal` affects HP only.
- `GrantShield` affects shield only and enforces the shield cap.
- `TryTakeDamage` applies the configured reduction when shield was active at the start of the attack, consumes shield, and applies any remaining damage to HP.
- Defeat is determined by HP, not shield; revival restores HP and resets shield.
- `PlayerPanelUI` presents shield separately from HP and consumes the separate events.
- `CombatController.HealPlayerFromBomb` and `GrantPlayerShieldFromBomb` call the corresponding distinct actor methods.

Never reuse HP fields/events for shield or change shield rules as a side effect of unrelated combat work.

`EnemyActor` independently owns enemy `currentHealth` and `currentShield` state. Enemy shields are distinct from HP and use their own cap, normalized value, grant API, and change/damage events.

- All established damage sources remain shield-aware by entering through `EnemyActor.TryTakeDamage` or `TryTakeDamageWithoutFeedback`.
- If shield was active at the start of a hit, `EnemyActor` applies the enemy shield reduction and ceiling rounding once to the whole hit, consumes shield first, and sends reduced overflow to HP. Breaking shield does not remove the reduction from that hit; the next separate unshielded hit uses full damage.
- `GrantShield` changes shield only and clamps it to the enemy shield cap. Ability runtimes must use this API rather than changing HP or presentation.
- Enemy defeat remains based on HP reaching zero. Initialization resets shield so it cannot persist between enemy instances or waves.
- `EnemySlotUI` observes enemy shield events and presents shield in place of HP while shield is active. It is presentation-only, and missing shield presentation cannot prevent gameplay resolution.

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

## Enemy summoning, interception, and temporary attack-speed modifiers

The Town Marshal feature adds three reusable enemy-runtime capabilities without moving ownership into `BoardController`:

- `IEnemySummonService` is the narrow contract used by enemy runtimes that need to request a real enemy spawn. `WaveController` implements it because the wave system owns enemy slots, active-enemy membership, weakness assignment, scaled runtime stats, prefab initialization, lifecycle VFX, and wave-completion accounting.
- `WaveController.TrySummonEnemy` may only use a genuinely free configured enemy slot. A successful summon is initialized through the same `CreateEnemy` path as normal wave enemies, added to `activeEnemies`, and therefore remains an independent wave member until defeated. Enemy runtimes must not instantiate enemy prefabs or mutate the active roster themselves.
- `EnemySpecialAbilityRuntimeFactory` may pass the summon service to runtimes that require it. Runtimes that do not summon remain unaware of this capability.
- `EnemyActor` owns optional one-hop damage interception through `SetDamageRedirectTarget` / `ClearDamageRedirectTarget`. Normal `TryTakeDamage` may redirect one incoming damage instance to the designated living enemy, but the redirected hit is resolved with further redirection disabled so interception cannot recurse through a chain. `TryTakeDamageWithoutFeedback` bypasses interception, preserving already-applied damage-over-time behavior.
- Damage interception changes only the destination of the established actor damage call. The receiving actor still owns shield reduction, HP mutation, feedback, and defeat. Combat and board systems do not special-case the Town Marshal.
- `EnemyAutoAttack` owns a runtime attack-speed multiplier in addition to the definition/difficulty-derived base interval. Temporary buffs accelerate the existing countdown rather than creating a second attack loop, changing attack damage, or rewriting serialized base stats. Resetting the multiplier returns the same attack loop to normal speed.
- Town Marshal's retreat visual is presentation-only. The runtime's authoritative protection state is the `EnemyActor` redirect target and its move-limited lifetime; missing or replaced art cannot change whether damage is intercepted.

These capabilities are generic infrastructure. Future summoners, bodyguards, or temporary speed buffs may reuse them, but their lifetime, ownership, persistence, targeting, and balance rules must remain explicit per mechanic rather than inferred from Town Marshal.

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

## Siege Sergeant integration

- `SiegeSergeantEnemyAbility` owns the alternating cadence, one pending warning, stagger/action availability, cleanup and conditional defence. Definition data supplies warning moves, base hammer damage and defence reduction alongside the existing barricade fields.
- `BoardController` remains character-agnostic. `TryQueuePlaceBarricades` has opt-in straight-line preference and special-gem protection; existing callers keep their old defaults. Candidate selection is repeated at execution to account for preceding mining or obstacle requests.
- `BoardController.TargetedClears` adds generic gem-pair marking and environmental clearing to the existing mutation queue. `GemPairThreat` retains gem identity, owner, deadline and consumed state. A due strike is queued from valid-move notification where possible and revalidated at execution; refill cannot inherit a warning. The queue holds board ownership through both-target removal and complete settlement. The processor yields initially so instant metadata-only requests cannot leave a stale coroutine handle.
- `EnemyActor.IncomingDamageMultiplier` is an optional runtime provider, reset on initialization. It is evaluated after interception and before shield handling inside the authoritative damage path. The Sergeant supplies a live owner-count-based provider and clears it during cleanup. No board logic knows which enemy uses this hook.
- `EnemyRuntimeStats.DamageMultiplier` carries the unrounded difficulty/category/individual damage scale so special damage does not inherit rounding error from the basic attack. Existing constructor callers default to multiplier 1.
- `BoardHammerThreatVFX` listens to board marking/impact events, procedurally creates its pixel hammer texture and white sweep, tracks warning identities for presentation, and cleans up its sprites/textures/transient objects. Gameplay never waits for VFX completion.
- `ExactWaveRule.fixedEnemies` optionally specifies per-slot identities while preserving category-based rules. `WaveController` validates fixed definitions against category, minimum wave, prefab and database membership. The wave-16 rule requests one Mini-boss and fixes its identity to the Sergeant; no concrete enemy identity is hard-coded into the spawner.
- Unity editor validation is available at `Dungeon Matcher > Validation > Siege Sergeant`, or `-executeMethod SiegeSergeantValidation.Run`. It exercises actual target-selection helpers, gem identity/cancellation, ownership-based defence, direct/DoT damage, asset references, baseline stats and the fixed checkpoint. Run the normal `Tools/Validate-Unity.ps1` compilation gate first; Play Mode VFX/pacing verification remains separate.

## Weighted chapter pools and formation commands

- `WaveController` records successfully spawned Mini-boss/Boss definitions for adjacent-encounter exclusion. All weighted/fallback draws share that exclusion; already seen major Bosses remain excluded. Independent validation run fixtures reset both milestone and encounter history explicitly.

- `WaveSpawnProfile` remains the category/count planner; `EnemyDatabase` filters eligible definitions and evaluates their age-relative weight curves. `WaveController.BuildEncounter` resolves the complete composition before spawning through `CreateEnemy`. Definition-owned escort pools constrain all other slots when a Mini-boss is selected; they never create a second spawn path. Ordinary fixed overrides are removed from the standard profile, while the existing solo checkpoints remain.
- `WaveController` owns a private `System.Random` initialized from its recorded encounter seed. Category planning, definition/escort draws and weakness shuffling use that same instance. Existing board randomness remains Unity-based; full-run replay determinism is not claimed.
- `KnightCaptainEnemyAbility` owns preference, one shared four-move special opportunity, participant snapshot, wind-up/sequencing and cancellation. It uses `EnemyActor` action ownership and `EnemyAutoAttack.TryReserveCommand` / `PerformCommandStrike` / `ReleaseCommand`. Reservation suspends the existing timer; release preserves an unspent cooldown or restarts a consumed attack. All hits still use the existing auto-attack sequence and player damage API, including Spear Knight's two-hit lifecycle.
- `TryQueueTopUpMovablePins` extends the single board mutation queue. It selects ordinary gems at execution, uses existing pin ownership and `HasAvailableMove`, and tops up only to the cap. A separate movable-pin set distinguishes swap-only chains from fixed bolts. Gravity skips fixed pins only; input/legal-move checks reject both. Adjacent clears release bolts only. Gem destruction, special conversion, owner cleanup and emergency reshuffles release chains through established ownership cleanup. No board code identifies a Captain.
- Shield Knight's category is Special; its asset compensates category/wave multipliers for the wave-17 introduction baseline. Shield grants and shield-aware damage remain owned by the existing runtime and `EnemyActor`; player shields are unchanged.

## Royal milestone integration

- `RoyalArchbishopEnemyAbility` and `KingEnemyAbility` are installed through the existing factory. They own their shared cast cycles, warning handles, retries, cleanup and rank/roster decisions. `EnemySpecialActionAvailability` and explicit idle/action checks gate new casts; due effects use the same stagger/idle conditions without holding board ownership while waiting.
- `EnemyActor.SurvivedHealthDamage` publishes before/after HP after a nonlethal centralized damage instance, including DoT. The King records crossing latches here, queues ordered threshold batches and applies Enrage; it never polls HP or mutates health. `SetSpecialTurnRequirement` changes the effective requirement without discarding accumulated valid moves. Restoration uses the existing `RestoreHealth` API.
- `EnemyAutoAttack` owns source-keyed persistent normal-damage/speed modifiers and next-sequence damage modifiers. Accepted attacks capture one product for primary/follow-up damage and consume next-sequence modifiers once. Optional `PerformCommandStrike` scaling is scoped to that command; existing Captain calls retain 1x. Enrage multiplies the existing timer countdown independently of the legacy banner/rally speed channel. Removing one source cannot erase another source's modifier.
- `BoardController.TelegraphedClears` extends `pendingBoardMutations` with mark-set and lane requests. Gem sets track owner, identities, deadline and one-shot state. `Gem.SetSpecialType` cancels marks on replacement. Marking changes metadata only, preserving legal moves. Environmental clearability includes frozen ordinary gems but excludes specials and all non-playable structural cells. Clear requests use `ClearMatches(..., null)` without reward reporting and settle once via `ResolveEnvironmentalBoardChange`; no alternate resolver or special chain path exists. Due requests recheck stagger at execution and release their queued flag if deferred.
- `BoardTelegraphVFX`, `EnemyBlessingView` and `EnemyRoyalPhaseView` only observe state/events. Board art slots serialize on the existing `Game.unity` BoardController; missing assets use procedural/presentation fallbacks. Per-lane gameplay pacing is independent of VFX availability and uses the existing clear timings.
- `WaveSpawnProfile.SelectMilestone` draws from serialized rising-probability opportunities using the existing encounter RNG. `WaveController` records successfully spawned definitions as seen, preventing repeat milestone injection. `BuildEncounter` supports Boss leaders and their required escort while preserving the existing normal spawning path. First-pass King milestones request two slots and open with King + Archbishop. Threshold summons use the existing free-slot service; no separate summon ownership or targeting system is introduced.
- The menu/batch editor suite `RoyalMilestoneValidation.Run` covers production data and core invariants. It does not stand in for required Unity compilation or Play Mode VFX/timing/composition checks.
- `RoyalMilestonePlayValidation` is an opt-in editor test driver. Its Play Mode fixtures use real scene services and production coroutines, with temporary HP/counter arrangements and real hint-validated swaps. It exits Play Mode after success or failure and never saves fixture changes to the scene or definitions.
