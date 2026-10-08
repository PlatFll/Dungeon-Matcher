# Dungeon Matcher Game Design Reference

## Ironvein authorization (2026-10-08)

The approved Ironvein pack expands the cave concept into a fourth region. Its
implementation is staged on a review branch; current phase status is in
[IronveinExcavation/WORKLIST.md](IronveinExcavation/WORKLIST.md). Brittle, Hardened
and Obsidian are structural material stages, separate from remaining durability.
Neglect matures them; damage delays maturity. The small Bore projectile stops
after exactly one durability hit on the first stone, even if it breaks. Shared
environmental drills penetrate and destroy every intended object in their lane.
Neither changes existing zones' clear/shield rules. See
[the implementation contract](IronveinExcavation/IMPLEMENTATION.md).

Ore-Powered strengthens one whole basic sequence without stacking. Packbeetle
defeat grants it to eligible living allies and charges every fixed drill once.
The seven specialist contracts are recorded in that same implementation document.
Rattled halves new Stagger buildup for two accepted moves; stones/drills never
cleanse it. Sapper defusing deals no host damage; expiration deals exactly two
structural hits and one central player damage packet.

## Cross-zone design contract (2026-10-06)

[Shared zone design rules](ZONE_DESIGN_RULES.md) define meaningful mechanic
variation, predictable environmental interactions and the zone/lore checklist.
[Canonical statuses](STATUS_EFFECTS.md) define stable meanings and provisional
tuning. [Future zone locks](FUTURE_ZONE_LOCKS.md) record Dwarven Cave, Snowy Mountain
and the separate Reef Clan boss as documentation-only concepts. Their presence
in these documents does not mean their zone gameplay has been implemented.

[Caster presentation](CASTER_PRESENTATION.md) defines the shared targeting language:
left triangle, middle square, right ring. Successful ability casts announce their
inspection name; readiness and failed casts stay silent. Gem targets follow their
gems, cell targets stay fixed, and row/column targets sit outside the corresponding
board edge. Simultaneous sources remain visible in stable slot order until their
warning or cast ends. Existing countdowns communicate timing.

## Purpose and status

This document records durable, finalized design direction for Dungeon Matcher. It is not a balance sheet and does not make every currently serialized value a permanent rule. Current implementation details may be cited to define an established model, but unapproved details are marked **Needs finalized design input** rather than inferred.

## Game identity and core loop

## Unified move-combat experiment (2026-10-08)

New runs in all four zones use `unified-accepted-moves-v2`. One accepted manual
action advances basics, specials and timed effects once after its full cascades.
Thinking, invalid swaps, menus, free skills and presentation consume no moves.
This is an approved experiment; numerical tuning remains provisional.

Specials resolve Boss → Miniboss → Special → Normal, then left/middle/right.
All ordinary due basics follow, allowing both actions when legal. Commands consume
their participants' opportunity; channels and Stagger suppress it. Existing mine
warnings that depend on the final response drill release after that drill under
the same input hold, in rank/slot order. Birth-action summons receive no free tick.

Haste uses the strongest source: `max(2, ceil(base interval / speed))`. A new buff
leaves at least one response move; expiry never refunds progress. All 62 enemy
definitions have explicit new-profile intervals, from 2 to 6, preserving each
authored sequence. See [the interval table](UNIFIED_MOVE_INTERVALS.md).

Stagger lasts two full future moves, then two immunity moves. Partial meter stays
still while thinking and decays once only on an action without a qualifying hit.
Normal/Special/Miniboss/Boss HP thresholds are 30/35/30/25%; decay removes
10/7.5/5/3 damage-equivalent buildup. Rattled halves new buildup. Poison periodic
damage does not build it; existing feedback-producing ability damage still does.
Royal Decree affects five full future moves; Longer Reign adds one per stack.

New 24px PixelLab status icons use runtime outlined counters: moves, actual stacks,
or a neutral condition mark. Enemy sword and slot-sigil rows replace the large
attack text/radial/ability counter. Channel response moves use the same lower row,
in amber with an underline. Legacy saved profiles retain their original timing.
No old float attack timer is reinterpreted as an integer.

See [implementation and evidence](UNIFIED_MOVE_COMBAT.md). Human pacing and
physical-device acceptance remain separate from automated Unity evidence.

### Forest combat and live crystal travel

New production runs use the unified experiment above. Explicit historical forest
fixtures and existing saves retain their profile. The older compatibility rules
are in [the legacy timing contract](Forest/FOUNDATION_CONTRACT.md).

The launch plan is dungeon, magical forest and aquatic, developed one zone at a
time. Crystal travel chooses randomly among eligible destinations other than the
current zone; there is no player route selection. Dungeon, forest and Drowned Court are connected
after their entire apex formations die and any card reward is chosen. The split
pink crystal shakes, smoke covers gameplay, and the destination is restored under
cover. Settings remains usable. The run, board, resources and global depth carry
over; travel does not heal or end a run. See [the travel contract](Forest/CRYSTAL_TRAVEL.md).
For the current testing build only, new runs may start in Dungeon, Magical
Forest or Drowned Court through a temporary menu picker. This changes the starting location;
crystal destinations during the run still follow the existing random rule.
Continue restores its saved location and Retry starts the current live zone.
Remove this testing choice for the full game.
Forest has six starter kits plus seven approved roster additions, modular woodland,
gameplay-only timber UI and an original temporary music cue for review.
The additions are Thornkeeper, Berserker, Bloomcaller, Snapvine, Drummer, Briar
Archer and Ancient Treant (Old Stump variant C). Their finalized behavior and
provisional values are in [the expanded roster contract](Forest/EXPANDED_ROSTER.md).
Approximately ten-minute substantial visits still require human pacing validation.
The aquatic rules, fourteen identities, AIR economy and provisional control values
are defined in [the Drowned Court contract](DROWNED_COURT.md).
The roster revision makes oxygen a finite initial reserve, with emergency supply
only after exhaustion. Thief/Warden coffers capture resources instantly and survive
their caster. Queen's armored pearl moves through a legal short line rotation.
Nacre Tribute converts uncollected physical bubbles into Fortified: one pink pearl
halves one direct incoming hit, consumed once; periodic damage leaves it intact.
All counterplay uses the established accepted moves, caster sigils, central damage
and board-owned queue. These rules replace the retired Crushing Depths attack.
Current forest visits use eighteen local starter
encounters; these bands are temporary content anchors. This implementation adds no
minimum fight time, player-power correction, HP padding or run cap. Opened menu
and settings screens retain their existing presentation.

### Revised forest kits (2026-10-03)

Vines are cell overlays. They obscure some of the gem without blocking swapping,
gravity, matches, bombs or destruction. Clearing the covered gem removes its vine.
Normal growth pulses every two accepted moves, before enemy casts: one edge seed
and at most two frontier additions, with a 12-overlay growth cap. The October 4
tuning halves frontier spread (also for Surge) and maximum growth coverage; the
single edge seed remains, making a full normal pulse three additions instead of
five. Existing saved overlays are retained and must be cleared below the new cap
before growth resumes. A pulse uses
the original frontier, never recursive same-pulse spread. Root placement always
seeds its four orthogonal neighbors, independent of that growth cap. These are
prototype cadence/readability values, not finalized pacing. Real chains retain
their existing six-restriction cap; vines consume none of it.

Rootbinder immediately replaces a safe ordinary interior gem with a structural
1-hit Root at its authored release contact. Four immediate vines never spread.
First clear a vine beside the Root; a later clear through an unvined, opened side
deals one durability hit. The opening clear cannot also damage that Root.
Roots use the existing structural queue, useful-response checks, cap and refill.

Warden first readies after two moves, then immediately plants a 2-hit wooden
shield Root with four spreading vines at its authored release contact. While any
living Warden Root remains, every living ally and summon has **Warded**: one 25%
incoming-damage reduction through central damage resolution. Duplicate sources
do not stack; losing one leaves the others active. The buff is shown beside each
recipient's weakness gem and in inspection. This is not shield HP. Both instant placements use
the structural queue, protected-target checks, cap and useful-response rules.
Neither has a pre-placement warning. Legacy placement warnings fizzle on Continue
without planting or granting Stagger.

Matriarch first readies after three moves. Renew plants two linked 2-hit Heartroots
when none remain, then channels for two future moves. Wooden heart growths and
subtle life pulses distinguish both durability states without adding HP layers.
She heals herself and every
living ally for base 20 plus 20 per Heartroot surviving at resolution. Her cycle
then uses Verdant Surge (immediate existing-vine advance) and Thorn Harvest
(three future moves; base 20 damage plus 5 per currently vine-covered gem).
Harvest consumes all vines, leaving Heartroot occupancy and durability intact.
Surge never resets or delays the normal growth deadline. Both Heartroots fully
destroyed causes ordinary EnemyStagger, respecting its existing immunity rules.
Surviving spreading roots can regrow vines on the next normal pulse.

Successful casts and cancellations consume their sequence before effect callbacks.
After a milestone channel ends, normal three-move readiness begins immediately.
Basics pause during preparation/channel and owned animation, with no additional
post-channel recovery moves. Scout and Trailguard retain their
existing normal attacks. Mender retains her fixed-recipient two-move +20 heal;
recipient invalidation never retargets or auto-staggers her. All true interruptions
use EnemyStagger; no forest-specific stagger or vulnerability state exists.

Root IDs, remaining durability, opened sides, owner, spread policy, overlays,
normal growth deadline and milestone cycle/sequence are saved. Legacy vine pins
migrate to overlays; old nonspreading anchor casts retire with a harmless fizzle.
Real chains remain intact. Photographs keep the present growth deadline and root
durability/open sides, cannot resurrect destroyed roots or dead producers, and
do not rewind channels or consumed effects. Root networks leave with their owner;
environmental vines persist across waves, perk choices, pause and Continue. They
leave when the run transitions to another zone. Optional art never owns effects.

### Dungeon crumbling tiles and Royal Arbalist

Purple Dungeon uses **crumbling tiles**. The starting tuning is a pulse every
four accepted player moves, choosing one or two safe ordinary cells. Tile and
gem shake together for 480 ms, flash fully white and break. The holes skip gravity
and refill until two further accepted moves finish; then they flash white,
materialize and refill through normal board resolution. Invalid swaps, supplies,
skills and cascades do not advance the deadline. Specials, pinned gems, active
warning targets and existing structures are protected. Shared structure limits
and useful-response checks may reduce or skip a pulse. Pause freezes presentation;
reduced motion removes the shake. Hole deadlines persist through waves and saves,
and Gideon's photograph cannot rewind or resurrect them. Cadence is provisional.

Crossbow Guard remains the baseline Special with one chained gem per use. Royal
Arbalist is a Special and board disruptor: retain double-shot basic attacks and
chain up to two ordinary gems every four accepted moves, capped at two owned and
the existing six restrictions globally. His dedicated two-shot ability animation
uses approved native drawings with a separate contact for each chain. Normal
chain counterplay, safe targeting, cancellation and owner cleanup apply.

Travel removes the departing zone's vines, roots and crumbling holes before the
destination is revealed. Ordinary gems and specials carry over. Reopened zone
cells receive ordinary gems without a free match or reward during the atomic
handoff. Failed checkpoint writes leave the source board intact.

The following battle loop describes the production legacy profile.

Dungeon Matcher is a mobile pixel-art match-3 dungeon battler built in Unity.

The established battle loop is:

1. The player chooses a valid gem swap.
2. The board resolves the match, special-gem effects, clears, cascades, refill, and any required reshuffle.
3. Rewardable gem clears drive combat, player affinity healing, and ability energy.
4. Once the accepted move has completely settled, enemies advance turn-counted pressure and may queue board interference. Enemy auto-attacks provide separate real-time pressure.
5. Defeating the active enemies completes the wave; the next wave begins only after the old board resolution has finished.

The approved Balance v1 loop awards a choice of one from up to three legal run cards after waves 2, 5, 9, 13, 17, 21, 25 and 28. Defeating the King formation completes the opening milestone and the same run continues until death or an explicit End Run. The current Normal/Special roster continues in weighted formations; Guild zones remain future content. Further card opportunities occur every four completed waves from wave 32. Completed waves are journaled during combat and pay shared Gold Coins once on death or an explicit End Run/Retry. Suspend preserves the same attempt; legacy journals without a combat snapshot settle once during recovery. Gold buys independent character levels or optional consumables; see [BALANCE_V1.md](BALANCE_V1.md) for editable numerical decisions and measured evidence.

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
- Player-owned bomb destruction generates the existing per-gem special energy even while an ability is active, including Cracked explosions and crystal-driven chains. Ability clears without this explicit explosion entitlement still grant no energy; ordinary match energy remains paused during an active ability. Enemy/environmental removal grants no direct rewards.
- Energy is spent only after the selected ability runtime accepts activation. A rejected activation costs nothing.

Exact gain rates, capacity, and ability costs are tunable data and are not frozen by this reference.

## HP and shield distinction

- HP and shield are separate resources.
- Healing changes HP; shield grants change shield. Neither is a synonym for the other.
- Shield has its own cap, damage handling, events, and presentation.
- For both players and enemies, shield present at the start of a hit gates that entire hit. Apply the existing shield mitigation, consume shield, and discard excess damage. HP is untouched even when shield breaks. Each later hit, including a separate combo hit or poison tick, checks shield again.
- Changes unrelated to shield must not alter shield behavior or presentation.

Exact shield capacity and reduction values remain balance data. HP, shield, final damage, healing and grants use five-point steps: nearest five with positive effects at least five, after the relevant modifiers. Zero remains zero. Resource caps, revival and legacy continuation restoration enforce the same rule. Fractions used internally for scaling are not rounded per gem. Energy, gold, timing, counts and obstacle durability retain their own units.

## Special gems

- Special gems are part of the authoritative board-resolution sequence and may chain into other specials.
- The normal special-gem set contains the established Row Bomb, Column Bomb, Poison Bomb, Healing Bomb, and Shield Bomb types, plus Color Crystal.
- Cracked is not an ordinary special gem. It is a temporary gem state used by Bardley's Cracked Gems ability.
- Row and Column bombs are available from level 1, without a progression gate. Fresh five-gem shapes create Color Crystals (the classic star), also available from the start. Reaching level 3 on any one character permanently unlocks Poison account-wide; level 5 unlocks Healing and level 7 unlocks Shield. Levels are never summed. These rewards become selectable in Gem Mastery for every character. Higher-order shape rewards use the chosen unlocked Gem Mastery loadout. Color Crystals and bombs have no separate upgrade levels. Previously earned unlocks and valid explicit legacy selections are preserved through migration.
- Special activation must preserve one clear source, one reward report, and one board mutation for each resolved outcome.
- A special's hidden or preserved gem data must not accidentally create unintended damage, healing, or energy.
- Color Crystal + a mastery bomb converts eligible gems of the partner's color into that exact bomb type, then detonates every converted bomb. Directional combinations retain their existing treatment of pre-existing mastery bombs.
- Mastery-bomb effects commit at the board-owned shatter moment, after preparation, once per detonated bomb. Scheduling a clear does not activate it.

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
- The shared provisional endless HP anchors are global waves 1/15/30/50/70/100
  at 1/1.4/2/3/4/6×, with moderate linear continuation to 9× at 150. Damage and
  attack/special timing retain their existing tuning; player-power correction
  stays off. See [BALANCE_V1.md](BALANCE_V1.md) for the serialized curve contract.
- Increasing difficulty should not remove meaningful player choices or replace the match-3 game with a sequence of predetermined solutions.

Run upgrades are temporary, stackable build choices owned only by the current
run. Numerical cards use deterministic typed modifier channels; genuinely new
behavior uses explicit mechanic capabilities. Draft eligibility may depend on
stable player and active-ability IDs, but never display names. Card drafting has
its own deterministic random stream and cannot perturb encounter generation.

Balance v1 authorizes independent gold-purchased character levels (cap 20), shared one-time mastery unlocks, optional equipped consumables, hybrid authored/weighted formations, escorted milestones and a King opportunity at waves 29–30. No XP, hidden level gate or adaptive player-power scaling is used. Permanent levels set starting strength; run cards reset each run. The economy and pacing targets remain adjustable balance data in [BALANCE_V1.md](BALANCE_V1.md). Post-King content remains future work.

## Presentation and pixel-art readability principles

- Preserve the project's pixel-art rendering conventions.
- Gameplay state and cause-and-effect must remain legible on a mobile screen.
- Match, special, obstacle, damage, healing, shield, and enemy-action feedback should be visually distinct.
- Presentation may reinforce timing but must not become the authority for gameplay state.
- Missing optional art or VFX must not prevent correct gameplay resolution; use safe presentation fallbacks where appropriate.
- Responsiveness takes priority over decorative delay. Effects should clarify an outcome rather than obscure or postpone it.

## Finalized mechanic specifications

### The Minister and the King

The Minister retains the `royal_arcanist` save ID and Restoration/Benediction kit.
Judgment consumes surviving physical-gem marks in saved order, with one authored
strike per survivor and no intermediate gravity/refill. Only three survivors
enable the unique empowered third swing. Normal damage remains base 12; the
finisher's separate base 18 is provisional. After the final recovery the board
settles once. Zero survivors cause no Judgment attack.

The approved Royal milestone kits are specified in [ROYAL_SPECIALS.md](ROYAL_SPECIALS.md): Minister's missing-health/rank-based Sacred Triage, individually breakable Restoration runes and one-normal-sequence Benediction; King's one-time surviving 50%/25% reinforcement crossings, Enrage and deterministic Judgment → United Royal Assault → Bombardment cycle. King + Minister is the opening Boss composition, with both targets always damageable. Three-move gem warnings follow identities; Bombardment's two-move row/column warning cannot be cancelled by clearing gems and preserves player specials and structural occupants. Every countdown uses accepted valid completed player moves. Numeric HP, damage, cadence, weights and milestone windows remain first-pass serialized tuning.

### Spear Guard

- Spear Guard is a Normal enemy and the basic military frontline for Chapter 2, The Town Calls for Help. Balance v1 eligibility starts at wave 6.
- His normal auto-attack is one spear thrust. He has no follow-up hit, signature ability, or board manipulation, and uses normal stagger rules.
- Balance v1 base stats are 60 HP, 6 damage and a 9-second attack interval. HP uses the shared global endless curve; damage retains its existing slower curve. Attack and special cadence do not accelerate. Player-power correction and category multipliers are disabled.
- His relative Normal-category spawn weight is 1.5. Registration in the enemy database makes him eligible; it does not guarantee a particular wave composition.
- His definition now uses the restored Spear Guard idle and its matching ready-frame fallback. The shared single-lunge action presentation and gameplay timing are unchanged; the idle contains no combat events.
- Chapter pools now use weighted progression eras; the former wave-8 Knight unlocks were legacy implementation order and are superseded by Chapter 3 eligibility.

### Spear Knight

- Spear Knight is a Chapter 3 Normal enemy eligible starting at wave 12.
- Balance v1 base maximum HP: 100.
- Its normal auto-attack is a two-hit combo once every 11 seconds: lunge, deal 3 base damage at the first impact, return completely to rest, take a brief recovery/readability beat, then lunge again, deal 5 base damage at the second impact, and return completely to rest again.
- The next 11-second auto-attack cooldown begins only after the second return finishes; there is no normal cooldown between the two lunges.
- The two hits are separate damage instances, so player shield and defeat handling apply independently to each hit.
- Spear Knight has no special ability.

### Shield Knight

- Shield Knight is a Chapter 3 Special enemy eligible starting at wave 13.
- Balance v1 base stats: 90 HP, one 4-damage hit every 11 seconds. Shared global HP/damage scaling applies; it has no follow-up attack.
- Shielding Allies casts after every 6 valid completed player moves. Invalid swaps and cascades do not advance this counter, and difficulty scaling does not shorten the cadence.
- A cast grants +10 shield to every other living enemy and +12 shield to the caster. Other Shield Knights are allies, but the caster never receives its own ally grant.
- Enemy shield grants stack up to a maximum of 30 shield.
- When an enemy had shield at the start of a damage instance, that entire instance receives the same 25% reduction and ceiling-rounding semantics as the player's shield, even if the hit breaks the shield.
- Reduced damage consumes shield and excess is discarded. The shield-breaking hit never damages HP. A later separate hit is unreduced when no shield remains.

### Gideon Glass — ChronoShutter

Gideon is a theatrical brass automaton whose blue camera lens is his eye. His right-facing 64×64 design follows the user's selected hat, feather, split coat and cane reference, with no torso camera. Level-one starting values are 90 HP, 10.5 gem damage, 40 shield capacity and Sapphire affinity. These are initial balance values.

- ChronoShutter costs 100 energy. Activation requires a settled board and enemy-action queue in a live encounter. Energy is spent only after acceptance; recasting while active is rejected.
- Activation photographs the grid and its refill generator state. After the 850 ms cast, Gideon keeps a dedicated hold pose with a small countdown above his head. Five accepted valid manual swaps count down to zero. Invalid swaps, cascades, automatic special chains and consumables do not count.
- The fifth move finishes its complete cascade, refill and queued enemy board effects before the photograph returns. A 300 ms board pulse and 370 ms recovery lead back to idle. Presentation callbacks never own the restore.
- Restore replaces gem colors, positions, special types, pins/freezes, mines, barricades (including durability), standards and board refill RNG. Restoring state does not clear gems, resolve matches or emit clear rewards.
- Damage, healing, shield, energy, statuses, enemy deaths, rewards, elapsed combat time and completed enemy-turn counters persist. Normal matching still earns energy during the photograph. Enemy warnings retain their current deadlines and surviving logical gem targets; consumed warnings do not return.
- Dead owners cannot regain their pins/freezes/mines. Reopened mine cells receive safe ordinary gems from the restored board stream; barricades and standards retain their existing orphan rules. Ending the encounter or run, player defeat, cancellation and scene unload discard the photograph.
- Repeating the same swaps reproduces the same refill/cascade future when other board interference is unchanged. Unrelated enemy, encounter, draft and presentation randomness is not rewound. Owner-death cleanup or different intervening board effects can legitimately change that future.
- Pause freezes the phase timer. Continue preserves the photograph, move count, owner references, presentation phase and timer; interrupted fifth-move resolution replays through the existing continuation journal exactly once.

Two Gideon-only, one-stack Epic cards use the Ability theme:

| Card | Effect |
| --- | --- |
| Long Exposure | One extra accepted manual move before rewind: six total. |
| Developing Fluid | Grant 10 shield once on a successful rewind, with normal shield-grant modifiers and capacity. Cancellation and resumed recovery grant nothing. |

### Bardley

#### Identity

- Bardley is a slime bard/musician.
- Maximum HP: 80.
- Affinity: Topaz.
- Active ability: Cracked Gems.
- Production energy cost: **80**. The earlier one-energy testing override is retired.

#### Cracked Gems targeting

- Target up to 3 gems.
- First priority is ordinary non-special gems whose colors match the weaknesses of currently living enemies.
- Second priority is other ordinary non-special gems.
- Special gems may be targeted only when zero ordinary gems are available.
- If at least one ordinary gem exists but fewer than 3 ordinary gems are available, use fewer than 3 targets. Do not fill the remaining slots with specials.

#### Presentation

- The selected gem becomes visibly Cracked.
- Cracked gems briefly shake, flash white and explode in three sequential beats.
- Bardley uses no travelling or popping bubbles. Aquatic AIR bubbles are separate.
- One board-owned resolution collects all effects once. The board remains locked
  through all three beats and their chains, then refills once the sequence ends.
- Presentation assets and procedural fallbacks must not alter gameplay results.

#### Resolution

- Each cracked center produces a 3x3 explosion.
- Each cracked center deals 20 base matching-color damage to enemies whose weakness matches that cracked gem's color, scaled by permanent ability growth and current run modifiers. The three selected centers retain established special chaining. Cracked Gems costs 80 energy. One cast's complete explosions, chains and card effects share a refund ceiling of 50% of its effective accepted cost (40 at base cost, 32 at cost 64). Cost and ceiling are production starting values for further human tuning. Both characters start a new run at 20% ability charge; Continue never grants this again.
- Ordinary collateral destruction follows the established normal board/combat-clear behavior.
- Existing specials caught in the explosion chain using their established behavior.
- Obstacles use their established interaction semantics.

#### Color Crystal interaction

- When a Cracked explosion triggers a Color Crystal, eligible ordinary gems of the triggering color become Cracked as part of that same ability resolution; existing bombs activate with their own effects rather than being overwritten. If the all-special targeting fallback selects a crystal directly, it uses the existing protected remote-crystal sequence with a board-RNG-selected available color (any valid color if only crystals remain).
- Existing ordinary bomb-to-Color-Crystal behavior remains unchanged.

#### Energy and affinity

- Cracked explosions are explicitly player-owned bomb destruction and generate per-gem special energy once per destroyed colored gem. Their source remains Ability for damage and affinity consumers. The crystal itself grants no hidden-color rewards.
- Genuine Topaz destruction may still trigger Bardley's normal affinity healing through the established affinity-healing system.

These are finalized gameplay rules. Timing and presentation numeric values not listed above remain tunable unless separately documented.

### Town Marshal

#### Identity and encounter role

- Town Marshal is the first Mini-boss of Chapter 1, The Locals. Balance v1 gives him one Farmer/Pan escort in a weighted wave-7–8 opportunity; surrounding waves vary.
- He is a pompous, cowardly local authority figure whose danger comes from rallying townsfolk rather than from personal combat strength.
- He deliberately does not manipulate the match-3 board. Miner owns Chapter 1's board-interference lesson; Town Marshal teaches summoning, enemy-slot pressure, coordination, and target priority.
- His presentation uses the supplied stout town-official art with facial hair and an oversized hand bell. His recolored, grounded idle and matching ready-frame fallback are now assigned through the existing definition; his combat rules are unchanged.

#### First-pass combat balance

- Balance v1 base stats: 120 HP, 6 damage, 10-second normal attack interval and no follow-up hit. Shared global HP/damage scaling applies.
- The whole formation, including its local escort, fits the shared threat budget. Summons fill only the remaining three-slot capacity.

#### Shared special cadence and ability selection

- The Marshal receives one special-action opportunity every 4 valid completed player moves.
- Invalid swaps and cascades do not advance this cadence, and the four-move requirement is locked against global special-turn shortening.
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

### Siege Sergeant

- Chapter 2 Mini-boss with a Spear/Crossbow Guard escort, appearing once in a weighted wave-12–14 opportunity.
- Balance v1 base stats: 240 HP, one 5-damage hit every 11 seconds, normal stagger, and 10 damage for a failed hammer warning. Shared global HP/damage scaling applies.
- One special opportunity every 4 valid completed player moves, locked against difficulty shortening. Start with Hold the Line, then alternate successful fortification and hammer-warning casts. At the six-block cap, use the hammer instead of banking an instant replacement wall. With no legal targets, retry after another valid move rather than consume a no-op cast or loop every frame.
- **Hold the Line:** place three one-hit wooden blockades as a contiguous horizontal or vertical run. Enumerate legal full runs and choose one randomly. If none fits, choose three distinct random legal cells; if capacity or available cells permit fewer, place only that many. Cap at six blocks owned by this Sergeant. Holes, existing blockades, pinned gems and special gems are excluded. Other barricade enemies retain their existing placement semantics.
- **Hammer Time:** mark two orthogonally adjacent ordinary unpinned gems after prior board mutations settle. Give two full valid moves after marking; invalid swaps and cascades do not advance the warning. Markers follow gem identities through movement, gravity and reshuffles, never replacement gems in the same cells. If either gem is removed, pinned or becomes special, cancel the entire strike. A moved pair may no longer be adjacent at impact; it still targets those same two gems and the sweep connects their current positions.
- A surviving warning resolves after its displayed deadline settles and the Sergeant is free to act. It gives at least two valid moves; shared warning scheduling extends later warnings so each receives its response window. At least one marked target must be clearable by an immediate legal board answer when placed. Stagger or another enemy animation action may delay impact, giving additional opportunity to interrupt. Only one warning per Sergeant may be pending.
- A failed warning makes one shield-aware player damage call, removes exactly the two targets with no direct damage/healing/energy rewards or special activation, then reuses ordinary environmental refill/cascade/reshuffle resolution. Subsequent genuine cascades keep existing reward semantics.
- **Behind the Barricades:** while at least one blockade owned by this Sergeant remains, incoming damage is multiplied by 0.8 and rounded up, including damage-over-time. This reduction is fixed, never multiplied by block count, and is applied before the existing enemy-shield calculation. Other owners' and orphaned blockades do not grant defence. The block count is checked at each hit so breaking the last blockade immediately removes the passive.
- Defeat/disable cancels the warning and releases the passive. Surviving blockades persist and their ownership is orphaned, consistent with existing barricade lifetime rules.
- Presentation uses the supplied 64x64 sprite. White pixel hammer icons have dark outlines and two countdown pips; a faster pulse signals the last move. The strike is a short, broad, squared white sweep between both current gem positions, timed to their clear flash, with a small body tilt. These visuals are non-authoritative and do not require external VFX assets.

### Chapters and Crown escalation

Chapters are weighted enemy spawn eras, not fixed wave-by-wave encounter scripts. Ordinary compositions vary across runs. Eligibility thresholds, declining older-enemy weights, rising Crown weights, category caps and two-to-three active slots govern selection. The same encounter seed and generation calls produce the same compositions. This does not promise replay determinism for the entire board or combat timeline.

Balance v1 uses overlapping pools: Locals from wave 1, Guards from 6, Knights from 11, Royals from 19, and King around 29–30. Older enemies retain a declining weight tail. Marshal, Sergeant, Captain and Minister appear once in escorted opportunity windows. Sixteen editable recipes alternate teaching, practice, combinations and breathing room with constrained random formations. Whole-formation threat and disruption/support limits apply. See [BALANCE_V1.md](BALANCE_V1.md) for the complete schedule.

Sword Knight reuses `Enemy_Knight` and its stable ID. He is the Normal Crown melee baseline, with no signature ability. Spear Knight remains Normal with his existing two-hit normal attack. Shield Knight is Special. Knight Captain is a Mini-boss who owns professional formation coordination, distinct from Marshal summoning/interception and Sergeant fortification/siege pressure.

### Knight Captain

- A straightforward sword attack, stronger than Sword Knight, at a medium cadence. Balance v1 base values are 250 HP, 7 damage and an 11-second interval, modified by the normal difficulty/category pipeline. These are tunable balance data, not finalized runtime targets. Existing Knight animation is temporary presentation until Captain art is available.
- One special opportunity every 4 valid completed player moves, locked against difficulty shortening. Invalid swaps, cascades and settling do not count. Prefer Hold Fast first, then On My Mark, alternating after successful casts. If the preferred command cannot execute, try the other. If neither can execute, retain readiness and retry after another completed move.
- **Hold Fast!** tops up to 3 owned chains on ordinary, unpinned gems. Chained gems cannot be manually swapped but can fall with gravity and be cleared by matches, specials or abilities. Chains follow gem identity, disappear on destruction/replacement, and do not break from adjacent clears. Each placement passes the authoritative legal-move check with earlier placements included. No legal placement means no chain is added. Existing pin overlay/dimming is the presentation fallback. Captain defeat releases his chains through queued board cleanup; emergency reshuffles also release them.
- **On My Mark!** reserves the Captain and eligible living Crown soldiers present when the command starts. Enemies already performing an action or staggered cannot join. The Captain telegraphs, then participants execute their existing normal attack sequences in roster order, Captain first, with a brief gap. Spear Knight retains both separate hits and complete returns. This consumes each participant's next normal attack: its cooldown restarts after its command sequence. Reserved allies cannot start another normal or special attack during the wind-up.
- Allies defeated during the wind-up are skipped. Captain defeat/despawn cancels unfinished command attacks and releases surviving participants; unspent reservations retain their stored cooldown, while participants that already struck restart theirs. No stale damage callbacks may survive cancellation.
- No passive immunity, protector interception, forced target order, or escort-first rule. Matching the Captain's weakness damages him through the normal pipeline. Burning down the Captain and dismantling escorts are both valid strategies.
- Balance v1 Captain encounters choose two escorts from Sword/Spear Knights within the whole-formation threat budget and three-slot limit. Their shared falling-chain behavior is taught earlier by Crossbow Guard: cap 2 for the Guard, 3 for the Captain, 6 globally. Mage freezes remain distinct.

## Account, consumables and menus

Characters includes a confirmed **Reset to Lv 1** action for the selected character. It resets only that character's permanent level between runs, retaining other characters, wallet, inventory, equipment, records and earned account-wide bomb unlocks. Upgrade gold is not refunded. Reset is disabled at level 1 and while a saved run exists.

Gold is shared; each character level is independent. Shop purchases and equip state persist. Each equipped potion/Bomb loads `min(3, owned)` charges at run start, independently. Inventory is spent exactly once only on accepted use; unused stock remains owned and the same run never reloads charges. Potion heals 35% max HP and cannot be used at full HP. Bomb targets a gem and clears a 3×3 footprint through existing bomb chains, obstacle damage and cascades. Cancellation/invalid use spends nothing. Each slot has its own five-second paused-game-time cooldown.

The main menu provides Play/Continue, Characters, Shop, Gem Mastery, free practice and post-King challenges. Settings provides Resume, Suspend to Menu, confirmed End Run/Retry, independent persistent music/SFX/vibration controls and reduced motion. Suspend preserves this attempt without offline combat time. End Run pays completed waves once; unfinished waves pay nothing. Continued attempts retain their original character, level, mastery, build, charges, board, enemies and pending accepted actions. Account upgrades and supply changes are unavailable during an active run. Legacy journals without combat snapshots settle once on load.

The first draft after wave 2 offers legal Board, Ability and Survival directions. Later drafts remain weighted. One free Refine per run replaces an offer with eligible unoffered cards in the selected theme; exhausted themes spend nothing. Offers, choices and refinement survive Continue. Eight choices remain the control cadence. Practice copies the current profile, provides three free charges of each supply, and never writes gold, stock, records or progression to the real profile. No Supplies and Board Only unlock after the King, use separate best-wave/win records and pay normal rewards. Board Only also disables ability activation and Ability-theme cards.

Tap portraits for paused inspection of HP/shield, weakness, seconds until attack, move counters, owned restrictions and warning deadlines. Guide also explains board rules, ability refunds and the current build. Brief first-exposure tips point to inspection and can be revisited through Guide. Bomb requires footprint preview then confirmation; obstacle exceptions are explained before spending. Death shows the ending wave, actual last damage, scrollable build, earned rewards, supply replacement value, remaining owned stock, Retry and Change Build. Supply replacement value is informational, never an extra deduction. All runtime text and numbers use centrally configured Thaleah bitmap typography, sized to their containers. Sprites and the font atlas retain Point filtering. Healing feedback is green, shield gains bright blue, normal damage white and poison dark green; floating numbers show actual amounts and fade above actors.

The startup scene plays the original 30-frame SmallHold Games ident, followed by an additional 1.5-second final pose on white before MainMenu. Unity's native splash and logo are disabled. Music starts in MainMenu; the ident remains silent. Reduced motion changes presentation amplitudes, not gameplay clocks or resolution order.
