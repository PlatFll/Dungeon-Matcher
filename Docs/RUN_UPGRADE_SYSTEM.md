# Run Upgrade System

## Runtime flow

`RunUpgradeBootstrap` installs one scene-scoped `RunUpgradeRuntime`, one
`RunUpgradeCoordinator`, and the reusable choice UI when the battle scene loads.
Nothing is stored in `PlayerPrefs`, and a scene reload starts a fresh run.

Every fifth completed wave, the coordinator immediately holds the generic
`IWaveProgressionGate`. It waits for `BoardController.IsBusy` to become false,
acquires a disposable external input block, and opens the draft. Selecting one
legal card applies it once, closes the modal, releases input, and releases the
gate. `WaveController` then increments and spawns through its normal pipeline.
The UI never changes the current wave or calls `SpawnCurrentWave`.

If the player is defeated, required references are absent, or a draft has zero
legal cards, the hold is safely released so the run cannot softlock.

## Architecture

- `RunUpgradeDefinition` is immutable card data: stable ID, text, art, weight,
  stacks, wave bounds, player/ability eligibility, dependencies, exclusions,
  numeric modifiers, and typed mechanic capabilities.
- `RunUpgradeCatalog` is the data-driven list used by a run. The first-pass
  catalog lives at `Resources/RunUpgrades/PrototypeRunUpgradeCatalog`.
- `RunUpgradeRuntime` owns definitions and stack counts for this run and applies
  immediate authoritative consequences such as added maximum HP.
- `UpgradeDraftGenerator` performs weighted selection without replacement and
  returns three unique legal cards when at least three exist.
- `RunUpgradeResolver` is the side-effect-free gameplay read API. With no active
  runtime, every query returns the base value.
- `UpgradeChoiceUI` and `UpgradeCardView` are presentation. They do not contain
  upgrade effects.

Drafting uses a dedicated `System.Random` seeded by a stable mix of the encounter
seed. It never reads or advances `WaveController`'s encounter RNG.

## Numeric modifiers

Create a `RunUpgradeDefinition`, add it to the catalog, and add one or more typed
`RunUpgradeModifier` entries. Current authoritative channels are global gem
damage, maximum HP, healing, shield granted, ability-energy gain, ability-energy
cost, barricade durability damage, and Cracked Gems target count.

Modifier order is deterministic and independent of pick order:

1. base value;
2. summed flat additions;
3. summed additive percentages;
4. multiplicative modifiers in stable upgrade-ID order;
5. clamp and integer rounding.

Stack count multiplies flat and additive-percent values. Multipliers are raised
to the stack count. `maxStacks` excludes a maxed definition from later drafts.

## Mechanic-changing cards

New behavior must use an explicit typed `RunUpgradeMechanic` capability (or a
small purpose-built runtime hook when state is required), queried through the
resolver. Do not disguise new behavior as a number, add card-ID switches, use
reflection/string property paths, or put the behavior in UI code. Numeric tuning
continues to use `RunUpgradeModifier`.

## Eligibility and dependencies

Use the stable `PlayerDefinition.PlayerId` and
`CharacterAbilityDefinition.AbilityId` fields. Empty requirements mean any
player/ability. Prerequisites and exclusions contain stable upgrade IDs;
exclusions are enforced in both directions. `minimumWave`, optional
`maximumWave`, `weight`, and max stacks also participate in eligibility.

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
field. Missing art uses the generated 64x64 transparent pixel-art diamond, so it
never blocks gameplay.

The first catalog intentionally contains prototype/test content only: Gem
Grinder, Thicker Hide, Mana Spark, Siegebreaker, Strong Remedy, and Bardley-only
Horrible Encore, plus Efficient Casting to exercise effective ability cost.

## Scene and asset setup

No manual scene setup is required. The bootstrap follows the project's existing
runtime-installed presentation convention and locates the battle wave, board,
player, root overlay Canvas, and Resources catalog. Production artwork remains
an Inspector assignment on each definition.
