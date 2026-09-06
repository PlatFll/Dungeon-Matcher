# Run Upgrade System

## Runtime flow

`RunUpgradeBootstrap` installs one scene-scoped `RunUpgradeRuntime`, one
`RunUpgradeCoordinator`, the gameplay upgrade hooks, and the reusable choice UI
when the battle scene loads. Nothing is stored in `PlayerPrefs`, and a scene
reload starts a fresh run.

Every fifth completed wave, the coordinator immediately holds the generic
`IWaveProgressionGate`. It waits for `BoardController.IsBusy` to become false,
acquires a disposable external input block, and opens the draft. Selecting one
legal card applies it once, closes the modal, releases input, and releases the
gate. `WaveController` then increments and spawns through its normal pipeline.
The UI never changes the current wave or calls `SpawnCurrentWave`.

If the player is defeated, required references are absent, or a draft has zero
legal cards, the hold is safely released so the run cannot softlock.

## Architecture

- `RunUpgradeDefinition` is immutable card data: stable ID, text, art, rarity,
  weight, stacks, wave bounds, player/ability eligibility, dependencies,
  exclusions, numeric modifiers, and typed mechanic capabilities.
- `RunUpgradeCatalog` is the data-driven list used by a run. The current
  first-pass catalog lives at `Resources/RunUpgrades/PrototypeRunUpgradeCatalog`.
- `RunUpgradeRuntime` owns definitions and stack counts for this run and applies
  immediate authoritative consequences such as added maximum HP.
- `UpgradeDraftGenerator` performs rarity-weighted selection without replacement
  and returns three unique legal cards when at least three exist.
- `RunUpgradeResolver` is the side-effect-free gameplay read API. With no active
  runtime, every query returns the base value.
- `RunUpgradeGameplayHooks` owns explicit run-only behavior that needs gameplay
  events, such as wave-start energy and first-shield-break recovery.
- `UpgradeChoiceUI` and `UpgradeCardView` are presentation. They do not contain
  gameplay upgrade effects.

Drafting uses a dedicated `System.Random` seeded by a stable mix of the encounter
seed. It never reads or advances `WaveController`'s encounter RNG.

## Rarity

The first-pass rarity tiers are:

- **Common** — white, draft multiplier `1.00`
- **Uncommon** — green, draft multiplier `0.65`
- **Rare** — blue, draft multiplier `0.35`
- **Epic** — purple, draft multiplier `0.20`

The effective draft weight is `definition.Weight * rarity multiplier`. This
keeps the existing per-card `Weight` useful for fine tuning inside a rarity.

Cards that require a specific active ability are treated as character/ability
modifiers and are always Epic. Their serialized assets should also explicitly
store Epic so their color and rarity are correct before any Inspector edit.

Rarity changes presentation and draft frequency; it does not automatically
multiply a card's effect strength.

## Numeric modifiers

Create a `RunUpgradeDefinition`, add it to the catalog, and add one or more typed
`RunUpgradeModifier` entries. The system supports the established global
channels plus current card-pack channels for ability damage, poison duration and
tick damage, Healing/Shield Bomb strength, Cracked Gem damage, and Royal Decree
duration/damage.

Modifier order is deterministic and independent of pick order:

1. base value;
2. summed flat additions;
3. summed additive percentages;
4. multiplicative modifiers in stable upgrade-ID order;
5. clamp and integer rounding where the target value is integral.

Stack count multiplies flat and additive-percent values. Multipliers are raised
to the stack count. `maxStacks` excludes a maxed definition from later drafts.

## Mechanic-changing cards

New behavior uses an explicit typed `RunUpgradeMechanic` capability (or a small
purpose-built runtime hook when state is required). Do not disguise new behavior
as a number, add card-ID switches, use reflection/string property paths, or put
the behavior in UI code. Numeric tuning continues to use
`RunUpgradeModifier`.

The current pack includes explicit hooks for cascade bonuses, directional-bomb
chains, special-chain scaling, Color Crystal bonus energy, poisoned-enemy energy,
first shield-break recovery, enemy-health/rank conditional damage, wave-start
energy, and Bardley's Resonant Cracks.

`Chain Reaction` operates on the authoritative expanded special clear: each
additional special participating in the same reported special chain adds 10%
damage, capped at 30%. This preserves the board's single deterministic clear and
combat-report pipeline rather than resolving duplicate per-bomb combat passes.

## Eligibility and dependencies

Use the stable `PlayerDefinition.PlayerId` and
`CharacterAbilityDefinition.AbilityId` fields. Empty requirements mean any
player/ability. Prerequisites and exclusions contain stable upgrade IDs;
exclusions are enforced in both directions. `minimumWave`, optional
`maximumWave`, rarity-adjusted `weight`, and max stacks also participate in
eligibility.

Current character-specific IDs used by the card pack are:

- Bardley: `playerId = bardley`, `abilityId = cracked_gems`
- Sir Rattlebone: `playerId = skeleton`, `abilityId = royal_decree`

## Card layout and artwork

The runtime hierarchy uses the existing CanvasScaler and 540x960 reference
resolution. It does not read `Screen.width`.

- card: 152x280 reference pixels;
- three-card gap: 12 pixels;
- total three-card width: 480 pixels (30-pixel margins at reference width);
- title: 136x38;
- artwork section: 136x92;
- artwork image: 64x64 at native size;
- description: 136x96;
- heading: 420x52;
- full-screen dark-purple dimmer: approximately 71% opacity.

Card art must be a **64x64 transparent PNG** imported as a Sprite with Point
filtering, no compression, and no mipmaps. Assign it to the definition's Artwork
field. Missing art uses the generated 64x64 fallback, so missing production art
never blocks gameplay.

Card rarity colors are applied to the whole card theme:
Common white/light neutral, Uncommon green, Rare blue, and Epic purple.

## Current first-pass card pack

The catalog retains the original prototype cards and adds these twenty build
cards:

- Cascade Catalyst
- Bombsmith
- Chromatic Conductor
- Chain Reaction
- Corrosive Formula
- Slow Venom
- Toxic Momentum
- Reinforced Flask
- Aegis Reservoir
- Emergency Plating
- Boss Hunter
- Executioner
- Opening Volley
- Glass Cannon
- Prepared Casting
- Arcane Efficiency
- Sour Note
- Resonant Cracks
- Longer Reign
- Final Word

Sour Note, Resonant Cracks, Longer Reign, Final Word, and the pre-existing
Horrible Encore are Epic because they directly modify a specific character
ability.

These values and rarity probabilities are first-pass balance data and can be
tuned without replacing the card architecture.

## Scene and asset setup

No manual scene setup is required. The bootstrap follows the project's existing
runtime-installed presentation convention and locates the battle wave, board,
combat controller, player, root overlay Canvas, and Resources catalog.
Production artwork remains an Inspector assignment on each definition.
