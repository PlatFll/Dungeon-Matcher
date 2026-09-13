# Gameplay audit coverage checkpoint

## Status

Audit chunks 19-20. **Read-only production review; documentation-only changes.**

Pinned main: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after PR #134), rechecked for chunk 20.
No new confirmed gameplay defect was established in the checks recorded below. No production code, serialized asset, prefab, scene, input binding, import setting or balance value was changed. No additional regression suite was authored in these chunks.

This checkpoint is not a Unity test result or a declaration that the full source audit is finished. Compilation, AssetDatabase import/loading, Play Mode and device checks remain deferred to the user's combined validation stage. Earlier percentage estimates were rough system-coverage estimates, not a measured percentage of files read.

## Completed: enemy database membership references

Read `Assets/_Game/Data/Enemies/EnemyDatabase_Main.asset` and each of the 21 referenced definition `.asset.meta` files at the pinned commit. The directory tree also contains the corresponding asset files.

Result: **21 database entries, 21 distinct referenced GUIDs, 21 matching definition metadata files**. There is no duplicate or unresolved GUID among these database membership entries. Each matching metadata file declares main object file ID `11400000`, matching its database reference. The database's own script GUID `02d2b410687169d4cbcc3b360b81b031` matches `Assets/_Game/Scripts/Combat/Enemies/Data/EnemyDatabase.cs.meta`.

These are repository-text comparisons, not results from Unity's AssetDatabase. This does not certify every internal reference inside each definition, all project GUIDs, imported object types, visual sprites, eligibility, balance or successful spawning.

All filenames below are under `Assets/_Game/Data/Enemies/` and use `.asset` plus `.asset.meta`.

| Definition | Matching metadata GUID |
| --- | --- |
| Enemy_Farmer | a76417e22f884fad9fc5f40479f897e7 |
| Enemy_BasketVillager | 655f6b9c480740719f00f56e0169908d |
| Enemy_PanVillager | 36c47a216df94b9fae10c869929010fa |
| Enemy_Miner | 84831a37e3ae4a6aa1147fb4b1f8e6b9 |
| Enemy_CrossbowGuard | 4b6fd7e690b34c219e51f1294a1b301a |
| Enemy_Knight | 6615ee48f5122354fb3bb48ee8cd8b8f |
| Enemy_SpearKnight | 3446e21fb60447829950580946a51eff |
| Enemy_BarricadeVillager | cf3b9a8c35434f72afc9567d6c0835de |
| Enemy_BarricadeGuard | 1b41367eaef540e7b4fbc553bf6d5a28 |
| Enemy_ShieldKnight | 7e5bdc7d5c3140a3a3fe37dc861dd4f0 |
| Enemy_TownMarshal | a8912c080c9d415082ff439864aefd06 |
| Enemy_SpearGuard | 3e9ea0e381044e83b0b982c96c74c769 |
| Enemy_SiegeSergeant | 9e704135ac48432c9bdb148a30da3e8a |
| Enemy_KnightCaptain | cb5b4ce0108940d788ee9a219b2fe946 |
| Enemy_RoyalSwordsman | 103ebefdbf0b40dcbf8f1e7c1537c2a3 |
| Enemy_RoyalLancer | 574ae9bff3c2431fb01239e7b48856f7 |
| Enemy_RoyalArbalist | e0401922f77142049ae15844298a5eb7 |
| Enemy_RoyalStandardBearer | 39f0ae90d3b445afbf8f722502c90a05 |
| Enemy_CourtMage | b2a9fefd5490455182d2f6f2740862f2 |
| Enemy_RoyalArchbishop | 8d684cf2178442b8b6e3d160fb5fac79 |
| Enemy_King | b9de130924cb44f3b77befc8fa1c0438 |

## Completed: enum-to-factory coverage

Compared the complete `EnemySpecialAbilityKind.cs` enum with `EnemySpecialAbilityRuntimeFactory.cs`.

All **11 non-None kinds** have a factory case: Miner, CrossbowGuardBolt, Barricade, ShieldingAllies, TownMarshal, SiegeSergeant, KnightCaptain, RoyalStandardBearer, CourtMage, RoyalArchbishop and King. None deliberately returns no special runtime; unknown values take the existing error path. No missing current factory case was found.

This is dispatch coverage, not proof that all serialized definition values are correct or that each runtime passes its gameplay tests.

## Additional source inspection in chunk 19

The following exact scopes were read/rechecked; none produced a new confirmed defect in that pass:

| Source | Scope |
| --- | --- |
| Combat/Player/Runtime/PlayerAffinityHealing.cs | Full file: board subscription pairing, player/affinity guards, healing calculation and actor handoff |
| Combat/Player/PlayerAbilityMatchEnergyGain.cs | Full file: outcome subscriptions, source/entitlement gating, shape/special rewards, reference resolution |
| Combat/Player/PlayerAbilityEnergy.cs | Full file: storage, cap, spending, reset and changed-event publication |
| Presentation/CharacterAnimationPlayback.cs | Full file: pause/resume bookkeeping and impact event relay |
| Combat/Enemies/Runtime/RoyalStandardBearerEnemyAbility.cs | Full file: readiness, cap, placement completion, aura installation and owner orphaning |
| Board/BoardController.RoyalBanners.cs | Lines 1-360: request acceptance, placement/owner recheck, safe-target simulation and opening notification entry points; not a new complete gravity re-review |
| Combat/Enemies/Runtime/RoyalArchbishopEnemyAbility.cs | Full file: triage, rune scheduling, blessing grants, alternation and cleanup |
| Combat/CombatController.cs at PR #137 head `4b2a0dd2256568f0166f84bff5fcf9d8d59c78c7` | Lines 1-420: damage snapshot/actual recipient reporting, poison application, player utility effects and modifier handoff |

Paths in this table are relative to `Assets/_Game/Scripts/`. Except for the explicitly identified PR #137 read, these reads used the pinned main commit. A no-new-finding review is not a runtime PASS and does not supersede the existing audit fixes or their validation requirements.

## Completed in chunk 20: full run-upgrade catalog mapping

Inspected the complete `Assets/_Game/Resources/RunUpgrades` tree, `PrototypeRunUpgradeCatalog.asset`, all 27 referenced card definitions from their script/identity fields through their final effect fields, and all corresponding `.asset.meta` files. Also checked catalog metadata and the definition/catalog script metadata. All reads used the pinned main above. A local text comparison of the transcribed GUID/ID tables checked the counts and equality; it did not load Unity or execute any gameplay validator.

**27 catalog entries -> 27 distinct card GUIDs -> 27 existing card definitions -> 27 unique nonempty upgrade IDs.** Every card definition in this inspected directory is represented once in the catalog; there is no extra unregistered card definition in that directory. Metadata declares main object ID `11400000` for each referenced card, matching the catalog's references. This is not a project-wide duplicate-GUID scan or imported-object type validation.

All 27 card script references match `RunUpgradeDefinition.cs.meta` (`8a6fd06b25614f26955cabf2c123a002`). The catalog's script reference matches `RunUpgradeCatalog.cs.meta` (`8a6fd06b25614f26955cabf2c123a003`). The apparent gap `...1231007` in the prototype card GUID sequence is the catalog asset's own metadata GUID, not a missing card.

### Complete membership and effect table

Filenames below have `.asset` and `.asset.meta` suffixes under `Assets/_Game/Resources/RunUpgrades/`. Rows retain catalog order. Effects describe the serialized modifier channel/grant, not a new balance specification. Percentage modifiers use AdditivePercent; raw additions use Flat. `M#` means the corresponding typed mechanic in the consumer table below. Stack caps are the actual serialized limits.

| Asset stem | Upgrade ID | Matching metadata GUID | Effect / stack cap |
| --- | --- | --- | --- |
| Prototype_GemGrinder | prototype_gem_grinder | 8a6fd06b25614f26955cabf2c1231001 | GemDamage +15%; cap 5 |
| Prototype_ThickerHide | prototype_thicker_hide | 8a6fd06b25614f26955cabf2c1231002 | MaximumHealth +20; cap 5 |
| Prototype_ManaSpark | prototype_mana_spark | 8a6fd06b25614f26955cabf2c1231003 | AbilityEnergyGain +20%; cap 5 |
| Prototype_Siegebreaker | prototype_siegebreaker | 8a6fd06b25614f26955cabf2c1231004 | BarricadeDurabilityDamage +1; cap 3 |
| Prototype_StrongRemedy | prototype_strong_remedy | 8a6fd06b25614f26955cabf2c1231005 | Healing +20%; cap 5 |
| Prototype_HorribleEncore | prototype_horrible_encore | 8a6fd06b25614f26955cabf2c1231006 | CrackedGemsTargetCount +1; cap 3 |
| Prototype_EfficientCasting | prototype_efficient_casting | 8a6fd06b25614f26955cabf2c1231008 | AbilityEnergyCost -10%; cap 3 |
| RunUpgrade_CascadeCatalyst | cascade_catalyst | 03fb1fd85a0646bfb4b8a8cd3fd7c273 | M1; cap 1 |
| RunUpgrade_Bombsmith | bombsmith | d597e532fa3c4c7f88c6cafff0b598b5 | M2; cap 1 |
| RunUpgrade_ChromaticConductor | chromatic_conductor | 9ca4ad0826624878ba42a9ea1ac5a580 | M3; cap 1 |
| RunUpgrade_ChainReaction | chain_reaction | ef716d35e5ae4cefa6ce6417a312fb79 | M4; cap 1 |
| RunUpgrade_CorrosiveFormula | corrosive_formula | 8f589c43bdb34a87b5d24a15c6c3a257 | PoisonTickDamage +30%; cap 3 |
| RunUpgrade_SlowVenom | slow_venom | ccac09cd653246ca869ee551c36f00c0 | PoisonDuration +40%, PoisonTickDamage -15%; cap 1 |
| RunUpgrade_ToxicMomentum | toxic_momentum | f2baf927bc7b482a929410b9101ea8de | M5; cap 1 |
| RunUpgrade_ReinforcedFlask | reinforced_flask | d8786e5412204e928b2fbfbcbf063e38 | HealingBombHealing +30%; cap 3 |
| RunUpgrade_AegisReservoir | aegis_reservoir | c603a5d26d5b49929d419d30e5ea4f16 | ShieldBombShield +30%; cap 3 |
| RunUpgrade_EmergencyPlating | emergency_plating | 207de6bfa7804629b52d325d84ac5d23 | M6; cap 1 |
| RunUpgrade_BossHunter | boss_hunter | 33a7056148054d299bce5adfe40e349b | M7; cap 1 |
| RunUpgrade_Executioner | executioner | 9a9c4c4c01fb4a2e9aed18544bfa93ad | M8; cap 1 |
| RunUpgrade_OpeningVolley | opening_volley | 4f4756f865704d759b3aa3c8a8b4e0da | M9; cap 1 |
| RunUpgrade_GlassCannon | glass_cannon | 4926fdf04c6342618aa985fcfffedd0f | GemDamage +30%, AbilityDamage +30%, MaximumHealth -15%; cap 1 |
| RunUpgrade_PreparedCasting | prepared_casting | b6f4a1030ee446a7a06ec7c8c5016111 | M10; cap 1 |
| RunUpgrade_ArcaneEfficiency | arcane_efficiency | 821560fae49147ed9403e3cc4aaa52e6 | AbilityEnergyCost -15%; cap 2 |
| RunUpgrade_SourNote | sour_note | 191027e8fa1c44ae9d6831e4cc45b174 | CrackedGemDamage +30%; cap 3 |
| RunUpgrade_ResonantCracks | resonant_cracks | 00178e580b784aa2a94e22b8598a3dc9 | M11; cap 1 |
| RunUpgrade_LongerReign | longer_reign | 02d62b810a114bff81c158685ac5bb9d | RoyalDecreeDuration +2; cap 3 |
| RunUpgrade_FinalWord | final_word | 5550c325b1534ef08d84fe13a5b7b2b4 | RoyalDecreeDamage +25%; cap 3 |

### Identity, eligibility and serialized-value checks

- All entries have positive weight `1`, minimum wave `1`, no maximum wave (`0`), positive stack caps, and recognized rarity values. No card is unavailable solely because of a zero weight or invalid wave window in this catalog.
- Rarity totals: 11 Common, 7 Uncommon, 4 Rare, 5 Epic. These are catalog counts, not equal per-card draw probabilities. `UpgradeDraftGenerator` also applies the existing rarity multipliers.
- All prerequisite/exclusion arrays are empty. There are no dangling prerequisite or exclusion IDs in this catalog; this does not test arbitrary future dependency graphs.
- 22 cards have no character/ability restriction. Horrible Encore, Sour Note and Resonant Cracks require both `bardley` and `cracked_gems`. Longer Reign and Final Word require both `skeleton` and `royal_decree`. These literals match the two Resources player definitions and their linked ability assets. All five restricted cards are Epic.
- `RunUpgradeRuntime.IsEligible` compares the stable player/ability IDs ordinally and enforces owned stack limits. Names such as SirRattlebone and RoyalDecree do not replace their stable IDs in these checks.
- For an initialized character with no owned cards at a covered wave, the inspected data/rules imply 25 eligible definitions for Bardley and 24 for Skeleton. These are source-derived fixture expectations, NOT a Unity draft-test result or guaranteed draw distribution.
- 16 cards contain numeric modifiers (19 modifier entries across 15 distinct stat channels); 11 cards contain mechanic grants. All serialized stat, operation and mechanic values map to current enum members, and the numeric channels have corresponding resolver methods. No effectless card, unknown enum value or mismatched serialized numeric channel was found in this catalog.
- All 27 artwork fields are null. `RunUpgradeDefinition` explicitly declares artwork optional; these are not missing gameplay dependencies. No art was fabricated or assigned.

### Mechanic consumer mapping

| Grant | Named mechanic | Existing consumer |
| --- | --- | --- |
| M1 | CascadeCatalyst | RunUpgradeResolver.ResolveGemDamage: cascade-depth contribution |
| M2 | Bombsmith | RunUpgradeResolver.ResolveGemDamage: participating directional-bomb count |
| M3 | ChromaticConductor | RunUpgradeGameplayHooks.HandleBoardClearOutcomeResolved: ColorCrystal-source per-gem bonus |
| M4 | ChainReaction | RunUpgradeResolver.ResolveGemDamage: extra participating-special count and cap |
| M5 | ToxicMomentum | RunUpgradeResolver.ResolveAbilityEnergyGainInternal and hooks' poisoned-roster query |
| M6 | EmergencyPlating | RunUpgradeGameplayHooks.HandleShieldChanged: first qualifying break in an active wave |
| M7 | BossHunter | RunUpgradeResolver.ResolveEnemyDamage: target category |
| M8 | Executioner | RunUpgradeResolver.ResolveEnemyDamage: target below the current HP threshold |
| M9 | OpeningVolley | RunUpgradeResolver.ResolveEnemyDamage: target above the current HP threshold |
| M10 | PreparedCasting | RunUpgradeGameplayHooks.HandleWaveStarted: fixed wave-start energy grant |
| M11 | ResonantCracks | PR #146 hooks' board-outcome center classification and detonation counter |

This mapping establishes that the current grants have code consumers; it does not certify every source interaction. The existing resolver also exposes a contextual ChromaticConductor overload. The current standard board-energy producer uses the non-contextual overload, with the separate hook supplying the card bonus. Do not switch that producer to the contextual overload without reviewing duplicate-bonus ownership. Fixed mechanic grants are not silently rescaled by this audit.

Previously found defects remain deferred, not cleared by these metadata checks: #142 event/reset lifecycle, #143 intermission selection, #146 center refunds and #149 Cracked collateral special-count scope. Numeric target bonuses retain their existing pre-interception calculation. No new interpretation of broad card-description wording was used to expand damage-source eligibility.

### Source boundaries for chunk 20

Read the complete pinned-main `RunUpgradeTypes.cs`, `RunUpgradeDefinition.cs`, `RunUpgradeCatalog.cs`, `UpgradeDraftGenerator.cs` and `RunUpgradeResolver.cs`, plus the definition/catalog script metadata. Read `RunUpgradeRuntime.cs` lines 1-400 at #142's `93d0582ab4fadde089da606d9690ba0cc03063b9` for eligibility, application, ownership and draft state. Read complete `RunUpgradeGameplayHooks.cs` at #146's `4aad2826d315a2f723dbcbdf2aa8c25a80428aa9` for current corrected consumer mapping. Rechecked both Resources player identity/active-ability fields and both active-ability identity/cost fields at pinned main; their GUID links were traced in the preceding reference pass. This did not integrate or execute those branches.

### Preserved configuration and shared validation expectations

Bardley remains at the user-approved **1-energy testing cost**. Efficient Casting and Arcane Efficiency cannot reduce an already-minimum cost below the resolver's floor of 1. That is a consequence of the intentional override, not a newly discovered broken card. Keep the production asset unchanged; a disposable test definition can exercise percentage-cost arithmetic without committing a balance change.

Aegis Reservoir's previously recorded 30-shield-grant versus 30-cap observation remains a separate balance question. Efficient Casting and Arcane Efficiency are distinct current card IDs with different values/stack caps, not duplicate-ID corruption; no merging or removal was performed.

During the one final combined validation stage, load the actual catalog with Unity, confirm 27 non-null unique definitions, and test the derived character eligibility, stack caps, real card acquisition/effects, source distinctions and missing-art presentation alongside existing suites. Do not create a separate validation run for this documentation. No new numbered issue group is introduced.

## Remaining source/reference closure work

Keep these items explicit; do not mark them complete merely because a directory was listed or a validator exists.

### A. Remaining gameplay-data reference tables

- COMPLETE: full run-upgrade catalog membership and current identity/eligibility/modifier/mechanic mapping, recorded above in chunk 20. Unity import/drafting/effect execution remains deferred.
- Finish definition-internal script/prefab/escort/reinforcement references beyond the targeted links already inspected in earlier chunks. The 21-entry database membership table above does not cover these nested links.
- Record required versus optional null references. Preserve approved testing overrides and report ambiguous design/balance values rather than silently changing them.

### B. Remaining scene/prefab wiring

- Reconcile gameplay-relevant components and local file-ID links in MainMenu/Game, the shared enemy prefab and the gem prefab actually referenced by the scene.
- Finish required Resources and script-GUID links, including input-module package references. A package-owned GUID absent from Assets alone is not evidence of a broken reference.
- Separate legacy/disabled or optional presentation fields from required runtime dependencies. Do not redesign layout or reserialize art during this audit.

### C. Remaining presentation-to-gameplay boundary coverage

- Reconcile the script inventory with the earlier system passes, especially presentation coroutines that gameplay yields to and impact/return callback forwarding.
- Focus on missing/interrupted presentation preventing authoritative completion, rather than opening unrelated cosmetic refactors.
- Record any genuinely unread gameplay-relevant source paths in the closure report. Do not represent every source line, binary asset or third-party package as reviewed without evidence.

These are remaining scopes, not a promise of exactly three more chunks. Do not start the combined Unity validation until the source-audit closure state is explicitly recorded and the user authorizes that next stage.

## Combined validation handoff

This checkpoint introduces **no new bug issue group**. Keep the existing gameplay validation groups 1-17; do not rerun them individually now.

Recorded dependency relationships from the prior audit work:

- `#142 -> #143`
- `#142 -> #146 -> #149`
- Integrate the other focused fixes with their recorded baselines and review the final combined diff; independent files do not imply absence of behavioral integration risks.

During the final shared validation, load the actual database and catalog through Unity, verify required references/runtime installations, and execute the accumulated automated and manual gameplay checks. Textually matching GUIDs alone do not replace those tests. No merge is authorized by this document.

**Bardley cost=1 remains the user's intentional testing override to accelerate waves. Keep it at 1.** Do not restore 80 or modify the documented non-testing default. The earlier Aegis Reservoir shield-cap observation remains a separate unchanged balance question.
