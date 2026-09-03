# Dungeon Matcher Game Design Reference

## Purpose and status

This document records durable, finalized design direction for Dungeon Matcher. It is not a balance sheet and does not make every currently serialized value a permanent rule. Current implementation details may be cited to define an established model, but unapproved details are marked **Needs finalized design input** rather than inferred.

## Game identity and core loop

Dungeon Matcher is a mobile pixel-art match-3 dungeon battler built in Unity.

The established battle loop is:

1. The player chooses a valid gem swap.
2. The board resolves the match, special-gem effects, clears, cascades, refill, and any required reshuffle.
3. Rewardable gem clears drive combat, player affinity healing, and ability energy.
4. Once the accepted move has completely settled, enemies advance turn-counted pressure and may queue board interference. Enemy auto-attacks provide separate real-time pressure.
5. Defeating the active enemies completes the wave; the next wave begins only after the old board resolution has finished.

The wider run structure, end condition, and metagame loop need finalized design input.

## Core match-3 design philosophy

- The match-3 layer must remain satisfying, decision-rich, readable, fast, and responsive.
- Mechanics should create depth without unnecessary complexity.
- Rules should be easy to understand while supporting expert optimization.
- Mechanics must support clear player goals.
- Prefer the simplest fun implementation before adding abstraction or complexity.

## Player agency, readability, and responsiveness principles

- Preserve player agency and perceived control despite randomness.
- Accepted input must receive immediate, legible feedback.
- A move must resolve as one coherent sequence; avoid unexplained pauses, overlapping ownership, or outcomes that appear disconnected from the player's action.
- Random selection, enemy interference, and reshuffles must remain readable and fair.
- The board should clearly communicate legal state, active pressure, cause and effect, and when the player can act again.

## Combat and gem weakness model

- Each active enemy is assigned a gem-type weakness.
- Rewardable cleared gems damage active enemies whose weakness matches the cleared gem type.
- A single gem type may simultaneously be an enemy weakness and the player's affinity; these are independent relationships.
- Clear origin and context matter. Match clears, special clears, ability clears, and cascades must retain their source so damage, healing, energy, and secondary effects can apply the intended rules without duplicate rewards.
- Damage resolution must use the established combat and actor damage flows. A feature must not invent a parallel health mutation path merely because it needs specialized damage.

Exact damage values, cascade multipliers, targeting exceptions, and future damage-source rules remain balance data or need finalized design input.

## Player characters and abilities

- Characters use the generic `PlayerDefinition` model for identity, base health, gem affinity, presentation, active ability, and passive ability references.
- Active abilities use `CharacterAbilityDefinition` data and an ability runtime selected by `PlayerAbilityController`.
- Character-specific behavior belongs in its ability runtime and presentation layers. Board rules remain character-agnostic; abilities request board-owned operations when they affect the grid.
- Ability activation must be explicitly accepted before it consumes energy.

**Needs finalized design input:** the final roster, detailed character kits, passive rules, exact ability behavior, and balance values. Do not reconstruct these specifications from asset names, descriptions, or prototype values alone.

## Energy rules

- Energy generation, storage, activation checks, and spending are separate responsibilities.
- Established match energy distinguishes match shape and whether the clear damaged a matching enemy.
- Established special-gem energy is awarded per rewardable gem cleared and also distinguishes whether a matching enemy was damaged.
- Ability-source board clears grant no energy by default. This prevents self-refunds and energy loops unless a future mechanic explicitly defines otherwise.
- Energy is not generated while an ability is active under the current model.
- Energy is spent only after the selected ability runtime accepts activation. A rejected activation costs nothing.

Exact gain rates, capacity, and ability costs are tunable data and are not frozen by this reference.

## HP and shield distinction

- HP and shield are separate resources.
- Healing changes HP; shield grants change shield. Neither is a synonym for the other.
- Shield has its own cap, damage handling, events, and presentation.
- Damage that exhausts shield may continue into HP according to the shield damage rules owned by `PlayerActor`.
- Changes unrelated to shield must not alter shield behavior or presentation.

Exact shield capacity and reduction values remain balance data.

## Special gems

- Special gems are part of the authoritative board-resolution sequence and may chain into other specials.
- The normal special-gem set contains the established Row Bomb, Column Bomb, Poison Bomb, Healing Bomb, and Shield Bomb types, plus Color Crystal.
- Cracked is not an ordinary special gem. It is a temporary gem state used by Bardley's Cracked Gems ability.
- Straight-four matches currently create directional bombs. Higher-order shape rewards are selected through the Gem Mastery model rather than being hard-coded as one universal mapping.
- Special activation must preserve one clear source, one reward report, and one board mutation for each resolved outcome.
- A special's hidden or preserved gem data must not accidentally create unintended damage, healing, or energy.

**Needs finalized design input:** permanent shape-to-special mappings, activation footprints, combination rules, creation-position rules, individual secondary effects, and final balance. Current prototype mappings must not be promoted to finalized design without approval.

## Obstacles and board interference

- Board interference must remain readable, fair, and compatible with clear player goals.
- Enemies may manipulate the board only after the player's accepted move has fully settled.
- Obstacles must use the authoritative board pipeline and preserve existing special-gem and obstacle interaction semantics unless a mechanic explicitly changes them.
- The current implementation includes mined cells, pinned gems, and barricades. Their board state is owned by `BoardController`, not by VFX.
- Interference should create pressure and interesting prioritization rather than turn every encounter into a rigid puzzle.

**Needs finalized design input:** the permanent rules, limits, lifetimes, counterplay, and encounter use for each obstacle type.

## Enemy design philosophy

- Enemy behavior is data-driven through `EnemyDefinition` and the enemy runtime systems.
- Enemies should create behavior-based pressure and increasingly manipulate the board.
- Enemy identity should come from understandable behavior, cadence, weakness, and counterplay rather than hidden exceptions.
- Shared board logic must not hard-code individual enemies.
- Enemy board actions occur only at safe board-resolution points.

Detailed enemy kits and encounter compositions require explicit finalized specifications; current assets are implementation evidence, not a complete design bible.

## Progression and difficulty philosophy

- Difficulty should deepen decision-making and pressure without sacrificing board readability or perceived fairness.
- Escalation may combine stronger enemies, more demanding behavior, and greater board manipulation.
- Wave composition and numerical scaling are data-driven so pacing and balance can change without rewriting shared gameplay logic.
- Increasing difficulty should not remove meaningful player choices or replace the match-3 game with a sequence of predetermined solutions.

**Needs finalized design input:** run length, progression economy, unlock structure, difficulty milestones, boss cadence, failure/retry rules, and long-term scaling targets.

## Presentation and pixel-art readability principles

- Preserve the project's pixel-art rendering conventions.
- Gameplay state and cause-and-effect must remain legible on a mobile screen.
- Match, special, obstacle, damage, healing, shield, and enemy-action feedback should be visually distinct.
- Presentation may reinforce timing but must not become the authority for gameplay state.
- Missing optional art or VFX must not prevent correct gameplay resolution; use safe presentation fallbacks where appropriate.
- Responsiveness takes priority over decorative delay. Effects should clarify an outcome rather than obscure or postpone it.

## Finalized mechanic specifications

### Spear Knight

- Spear Knight is a late-game Normal enemy eligible starting at wave 8.
- Base maximum HP: 120.
- Its normal auto-attack is a two-hit combo once every 10 seconds: lunge, deal 5 base damage at the first impact, return completely to rest, take a brief recovery/readability beat, then lunge again, deal 7 base damage at the second impact, and return completely to rest again.
- The next 10-second auto-attack cooldown begins only after the second return finishes; there is no normal cooldown between the two lunges.
- The two hits are separate damage instances, so player shield and defeat handling apply independently to each hit.
- Spear Knight has no special ability.

### Shield Knight

- Shield Knight is a late-game Normal enemy eligible starting at wave 8.
- At its wave-8 introduction under the expected-player-power baseline, it has 160 maximum HP and its normal single-hit auto-attack deals 5 damage every 10 seconds. It has no follow-up attack.
- Shielding Allies casts after every 7 valid completed player moves. Invalid swaps and cascades do not advance this counter, and difficulty scaling does not shorten the cadence.
- A cast grants +10 shield to every other living enemy and +15 shield to the caster. Other Shield Knights are allies, but the caster never receives its own ally grant.
- Enemy shield grants stack up to a maximum of 30 shield.
- When an enemy had shield at the start of a damage instance, that entire instance receives the same 25% reduction and ceiling-rounding semantics as the player's shield, even if the hit breaks the shield.
- Reduced damage consumes enemy shield first and any remainder overflows into HP. A later separate hit is unreduced when no shield remains.

### Bardley

#### Identity

- Bardley is a slime bard/musician.
- Maximum HP: 80.
- Affinity: Topaz.
- Active ability: Cracked Gems.
- Energy cost: 80.

#### Cracked Gems targeting

- Target up to 5 gems.
- First priority is ordinary non-special gems whose colors match the weaknesses of currently living enemies.
- Second priority is other ordinary non-special gems.
- Special gems may be targeted only when zero ordinary gems are available.
- If at least one ordinary gem exists but fewer than 5 ordinary gems are available, use fewer than 5 targets. Do not fill the remaining slots with specials.

#### Presentation

- A bubble travels to each selected target from outside the sides of the board.
- It briefly hovers and pops.
- The selected gem becomes visibly Cracked.
- Cracked gems shake for approximately 1 second, slightly enlarge/bulge, flash white, and then explode.
- Presentation assets and procedural fallbacks must not alter gameplay results.

#### Resolution

- Each cracked center produces a 3x3 explosion.
- Each cracked center deals 50 fixed matching-color damage to enemies whose weakness matches that cracked gem's color.
- Ordinary collateral destruction follows the established normal board/combat-clear behavior.
- Existing specials caught in the explosion chain using their established behavior.
- Obstacles use their established interaction semantics.

#### Color Crystal interaction

- When a Cracked explosion triggers a Color Crystal, every eligible non-crystal gem of the triggering color becomes Cracked as part of that same ability resolution.
- Existing ordinary bomb-to-Color-Crystal behavior remains unchanged.

#### Energy and affinity

- Board clears whose source is the ability do not generate ability energy.
- Genuine Topaz destruction may still trigger Bardley's normal affinity healing through the established affinity-healing system.

These are finalized gameplay rules. Timing and presentation numeric values not listed above remain tunable unless separately documented.

### Town Marshal

#### Identity and encounter role

- Town Marshal is the first Mini-boss of Chapter 1, The Locals, and is introduced as a solo Mini-boss encounter on wave 8 in the current first-pass progression.
- He is a pompous, cowardly local authority figure whose danger comes from rallying townsfolk rather than from personal combat strength.
- He deliberately does not manipulate the match-3 board. Miner owns Chapter 1's board-interference lesson; Town Marshal teaches summoning, enemy-slot pressure, coordination, and target priority.
- His presentation direction is a short/fat town official with a huge moustache and oversized hand bell. Final sprite/animation art is not yet wired into the current definition.

#### First-pass combat balance

- Target runtime maximum HP at the wave-8 introduction is approximately 400 under the expected-player-power baseline. The serialized base/scaling values are chosen to reach that target through the normal `DifficultyProfile` pipeline rather than bypassing global scaling.
- His personal auto-attack is intentionally pathetic: approximately 1 damage at introduction with a very slow roughly 12-13 second runtime cadence.
- He has no follow-up auto-attack hit.

These numeric values are first-pass balance and should be playtested rather than treated as immutable final balance.

#### Shared special cadence and ability selection

- The Marshal receives one special-action opportunity every 3 valid completed player moves.
- Invalid swaps and cascades do not advance this cadence, and the three-move requirement is locked against global special-turn shortening.
- Ability choice is deterministic rather than random so the introductory Mini-boss remains learnable and readable.
- His initial preference is `Ring the Bell`. After a successful Ring cast, his next preference is `Citizens, Seize Him!`; after a successful Citizens cast, his next preference returns to Ring.
- If the preferred ability is currently invalid, he may use the other valid ability instead.
- If neither ability is currently legal, the ready special is held until an ability becomes legal rather than consuming the action on a no-op.

#### Passive — Big Man in Town

- `Ring the Bell` designates the newly summoned local as the Marshal's protector.
- The Marshal visibly retreats behind that protector for up to 2 valid completed player moves.
- Retreat ends early if that specific protector is defeated.
- While retreated, ordinary direct/clear damage that would normally hit the Marshal is fully intercepted by the protector. The damage is not discarded; it enters the protector's normal `EnemyActor` damage path.
- Damage-over-time already applied to the Marshal is not redirected.
- Retreat presentation is non-authoritative: the current first-pass fallback moves him slightly back/up, scales him down, and dims him. Gameplay must remain correct if final retreat art/animation is missing.
- If a special was being held ready because all enemy slots were full when the protector dies, the Marshal's shared special counter resets. This prevents an immediate replacement summon and guarantees a real opening after the player removes his meat shield.

#### Ability 1 — Ring the Bell

- Ring the Bell requires a free enemy spawn slot and summons exactly one local per successful cast.
- There are only three active enemy slots total; the ability can never create an invisible or fourth active enemy.
- Current implementation candidates are Farmer, Pan Villager, and Basket Villager because those are the existing Chapter-1 local assets. The newer roster concept may later replace Basket Villager with Torch Villager; that content/naming change is deliberately not folded into the Marshal feature.
- The candidate list is data-driven in the Marshal's `EnemyDefinition` so the roster can change without rewriting the runtime.
- Summoned townsfolk are real independent enemies. They are added to the authoritative active-wave roster, count toward wave completion, and remain alive if the Marshal dies.

#### Ability 2 — Citizens, Seize Him!

- Citizens, Seize Him! affects all currently living local allies matching the Marshal's configured local candidate set, whether they were part of the original encounter or were summoned by him.
- It increases those allies' real-time auto-attack speed by 40% for 5 seconds in the first-pass balance.
- It does not increase attack damage and does not buff the Marshal himself.
- The buff does not stack with itself. The ability is invalid while its current rally is active.
- The ability is invalid when no qualifying local ally is alive.
- A local summoned after an already-running rally begins does not retroactively receive that existing rally; a future valid cast may include it.

#### Summon lifetime rule established by this encounter

- Summon persistence is a property of the summon fiction/mechanic, not a universal rule that all summoned entities vanish with their owner.
- Town Marshal's rallied townsfolk are independent physical enemies and persist after his death.
- Future owner-bound magical summons, such as a spirit familiar, may explicitly despawn when their summoner dies.
