# Crystal Resonance serialized encounter matrix

Generated from the five current ZoneDefinition assets during Stage 10. Global depth tiers begin at **1 / 6 / 13 / 25 / 45**. Existing enemy HP curves and kits remain unchanged.

Selection-only Unity tests pass for every zone/tier, including exhausted named pools and empty ordinary recipe libraries. This establishes legality and fallback availability; actual played pacing and fairness remain separately measured in the final validation report.

Authored recipe selection uses `recipe weight × mean(member tier weights)` after eligibility, prerequisite, threat, role and recent-history filters. Capstone kind is drawn first (elite 6, miniboss 4, major 2; major 3 at the deepest tier). A major requires a two-visit gap. Named leaders occur once per run, except the explicitly required King escort. Reserve elites are repeatable.

## drowned-court

| Member | Eligible tiers | Tier weights 0–4 | Family | Summon only |
|---|---|---|---:|---|
| reef_spearman | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| hammerhead_bruiser | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| needlefin_skirmisher | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| shellback_porter | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| pearl_thief | 0–4 | 10 / 7 / 4 / 1 / 1 | 0 | no |
| pearl_cantor | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| conch_marshal | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| moray_siphoner | 2–4 | 0 / 0 / 10 / 7 / 4 | 0 | no |
| reef_netweaver | 0–4 | 10 / 7 / 4 / 1 / 1 | 0 | no |
| puffer_sentinel | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| breakwater_captain | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| lantern_warden | 2–4 | 0 / 0 / 10 / 7 / 4 | 0 | no |
| queen_nacre | 3–4 | 0 / 0 / 0 / 10 / 7 | 0 | no |
| skittercrab | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | yes |

| Recipe | Tiers | Kind | Members | Weight | Whole threat cap | Prerequisites |
|---|---|---|---|---:|---:|---|
| Outer patrol | 0–2 | ordinary | reef_spearman, hammerhead_bruiser | 4 | 4 | — |
| Quiet patrol | 0–4 | ordinary | reef_spearman | 1 | 4 | — |
| Learn pearl_thief | 0–4 | ordinary | pearl_thief, reef_spearman | 3 | 4 | — |
| Patrol pearl_thief | 1–4 | ordinary | pearl_thief, reef_spearman, hammerhead_bruiser | 4 | 5.5 | — |
| Learn reef_netweaver | 0–4 | ordinary | reef_netweaver, reef_spearman | 3 | 4 | — |
| Patrol reef_netweaver | 1–4 | ordinary | reef_netweaver, reef_spearman, hammerhead_bruiser | 4 | 5.5 | — |
| Learn puffer_sentinel | 1–4 | ordinary | puffer_sentinel, reef_spearman | 3 | 4 | — |
| Patrol puffer_sentinel | 1–4 | ordinary | puffer_sentinel, reef_spearman, hammerhead_bruiser | 4 | 5.5 | — |
| Learn pearl_cantor | 1–4 | ordinary | pearl_cantor, reef_spearman | 3 | 4 | — |
| Patrol pearl_cantor | 1–4 | ordinary | pearl_cantor, reef_spearman, hammerhead_bruiser | 4 | 5.5 | — |
| Learn conch_marshal | 1–4 | ordinary | conch_marshal, reef_spearman | 3 | 4 | — |
| Patrol conch_marshal | 1–4 | ordinary | conch_marshal, reef_spearman, hammerhead_bruiser | 4 | 5.5 | — |
| Learn moray_siphoner | 2–4 | ordinary | moray_siphoner, reef_spearman | 3 | 4 | — |
| Patrol moray_siphoner | 2–4 | ordinary | moray_siphoner, reef_spearman, hammerhead_bruiser | 4 | 5.5 | — |
| Combined reef_netweaver | 2–4 | ordinary | reef_netweaver, conch_marshal, reef_spearman | 3 | 6.5 | — |
| Elite reef_netweaver | 1–4 | elite | reef_netweaver, conch_marshal, hammerhead_bruiser | 5 | 6.5 | — |
| Combined pearl_thief | 2–4 | ordinary | pearl_thief, moray_siphoner, reef_spearman | 3 | 6.5 | — |
| Elite pearl_thief | 2–4 | elite | pearl_thief, moray_siphoner, hammerhead_bruiser | 5 | 6.5 | — |
| Veteran patrol | 0–4 | elite | pearl_thief, reef_spearman, hammerhead_bruiser | 4 | 5.5 | — |
| Guardian breakwater_captain | 1–4 | miniboss | breakwater_captain, reef_spearman | 4 | 6.5 | — |
| Guardian lantern_warden | 2–4 | miniboss | lantern_warden, reef_spearman | 4 | 6.5 | — |
| Capstone queen_nacre | 3–4 | major | queen_nacre, hammerhead_bruiser | 2 | 6.5 | pearl_thief |
| Reserve Outer patrol | 0–4 | elite | reef_spearman, hammerhead_bruiser | 1 | 4 | — |

## dungeon

| Member | Eligible tiers | Tier weights 0–4 | Family | Summon only |
|---|---|---|---:|---|
| barricade_guard | 1–4 | 0 / 10 / 7 / 4 / 1 | 1 | no |
| barricade_villager | 0–1 | 10 / 7 / 0 / 0 / 0 | 0 | no |
| basket_villager | 0–1 | 10 / 7 / 0 / 0 / 0 | 0 | no |
| court_mage | 3–4 | 0 / 0 / 0 / 10 / 7 | 3 | no |
| crossbow_guard | 1–4 | 0 / 10 / 7 / 4 / 1 | 1 | no |
| farmer | 0–1 | 10 / 7 / 0 / 0 / 0 | 0 | no |
| king | 3–4 | 0 / 0 / 0 / 10 / 7 | 3 | no |
| knight | 2–4 | 0 / 0 / 10 / 7 / 4 | 2 | no |
| knight_captain | 2–4 | 0 / 0 / 10 / 7 / 4 | 2 | no |
| miner | 0–1 | 10 / 7 / 0 / 0 / 0 | 0 | no |
| pan_villager | 0–1 | 10 / 7 / 0 / 0 / 0 | 0 | no |
| royal_arbalist | 3–4 | 0 / 0 / 0 / 10 / 7 | 3 | no |
| royal_arcanist | 3–4 | 0 / 0 / 0 / 10 / 7 | 3 | no |
| royal_lancer | 3–4 | 0 / 0 / 0 / 10 / 7 | 3 | no |
| royal_standard_bearer | 3–4 | 0 / 0 / 0 / 10 / 7 | 3 | no |
| royal_swordsman | 3–4 | 0 / 0 / 0 / 10 / 7 | 3 | no |
| shield_knight | 2–4 | 0 / 0 / 10 / 7 / 4 | 2 | no |
| siege_sergeant | 2–4 | 0 / 0 / 10 / 7 / 4 | 2 | no |
| spear_guard | 1–4 | 0 / 10 / 7 / 4 / 1 | 1 | no |
| spear_knight | 2–4 | 0 / 0 / 10 / 7 / 4 | 2 | no |
| town_marshal | 1–4 | 0 / 10 / 7 / 4 / 1 | 1 | no |

| Recipe | Tiers | Kind | Members | Weight | Whole threat cap | Prerequisites |
|---|---|---|---|---:|---:|---|
| Local search | 0–1 | ordinary | farmer, basket_villager | 3 | 4 | — |
| Village watch | 0–1 | ordinary | pan_villager, farmer | 3 | 4 | — |
| Excavation lesson | 0–1 | ordinary | miner, farmer | 3 | 4 | — |
| Timber lesson | 0–1 | ordinary | barricade_villager, basket_villager | 3 | 4 | — |
| Town boundary | 1–1 | ordinary | spear_guard, pan_villager | 3 | 4 | — |
| Chain patrol | 1–2 | ordinary | crossbow_guard, spear_guard | 3 | 4 | — |
| Guard fortification | 1–2 | ordinary | barricade_guard, spear_guard | 3 | 4 | — |
| Knight patrol | 2–4 | ordinary | knight, spear_knight | 3 | 4 | — |
| Shield lesson | 2–4 | ordinary | shield_knight, knight | 3 | 4.5 | — |
| Knight advance | 2–3 | ordinary | crossbow_guard, knight, spear_knight | 3 | 6.0 | — |
| Royal vanguard | 3–4 | ordinary | royal_swordsman, royal_lancer | 3 | 5.0 | — |
| Royal chain lesson | 3–4 | ordinary | royal_arbalist, royal_swordsman | 3 | 5.0 | — |
| Court ice lesson | 3–4 | ordinary | court_mage, royal_lancer | 3 | 5.5 | — |
| Standard guard | 3–4 | ordinary | royal_standard_bearer, royal_swordsman | 3 | 5.5 | — |
| Court pressure | 3–4 | ordinary | court_mage, royal_standard_bearer, royal_swordsman | 3 | 8.5 | — |
| Royal fireline | 3–4 | ordinary | royal_arbalist, royal_lancer, royal_swordsman | 3 | 7.5 | — |
| Town elite | 1–2 | elite | crossbow_guard, spear_guard, barricade_guard | 3 | 5.5 | — |
| Knight elite | 2–4 | elite | shield_knight, knight, spear_knight | 3 | 6.5 | — |
| Royal elite | 3–4 | elite | court_mage, royal_lancer, royal_swordsman | 3 | 8.0 | — |
| Marshal escort | 1–2 | miniboss | town_marshal, spear_guard | 3 | 5.5 | — |
| Siege escort | 2–3 | miniboss | siege_sergeant, spear_guard | 3 | 5.5 | — |
| Captain escort | 2–4 | miniboss | knight_captain, knight, spear_knight | 3 | 8.5 | — |
| Minister escort | 3–4 | miniboss | royal_arcanist, royal_swordsman | 3 | 6.5 | — |
| The King and Minister | 3–4 | major | king, royal_arcanist | 2 | 11.0 | royal_swordsman, court_mage |
| Reserve Local search | 0–1 | elite | farmer, basket_villager | 1 | 4 | — |
| Reserve Knight patrol | 2–4 | elite | knight, spear_knight | 1 | 4 | — |
| Reserve Royal vanguard | 3–4 | elite | royal_swordsman, royal_lancer | 1 | 5.0 | — |

## frostwake-peaks

| Member | Eligible tiers | Tier weights 0–4 | Family | Summon only |
|---|---|---|---:|---|
| iceborn_raider | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| harpoon_hunter | 0–4 | 10 / 7 / 4 / 1 / 1 | 0 | no |
| frostfang_wolf | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| rimecarver | 0–4 | 10 / 7 / 4 / 1 / 1 | 0 | no |
| shieldmaiden | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| rime_skald | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| hearthkeeper | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| frostblade_reaver | 2–4 | 0 / 0 / 10 / 7 / 4 | 0 | no |
| glacier_troll | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| iceborn_champion | 2–4 | 0 / 0 / 10 / 7 / 4 | 0 | no |
| jarl_haldra | 3–4 | 0 / 0 / 0 / 10 / 7 | 0 | no |

| Recipe | Tiers | Kind | Members | Weight | Whole threat cap | Prerequisites |
|---|---|---|---|---:|---:|---|
| Outer patrol | 0–2 | ordinary | iceborn_raider, frostfang_wolf | 4 | 4 | — |
| Quiet patrol | 0–4 | ordinary | iceborn_raider | 1 | 4 | — |
| Learn harpoon_hunter | 0–4 | ordinary | harpoon_hunter, iceborn_raider | 3 | 4 | — |
| Patrol harpoon_hunter | 1–4 | ordinary | harpoon_hunter, iceborn_raider, frostfang_wolf | 4 | 5.0 | — |
| Learn rimecarver | 0–4 | ordinary | rimecarver, iceborn_raider | 3 | 4 | — |
| Patrol rimecarver | 1–4 | ordinary | rimecarver, iceborn_raider, frostfang_wolf | 4 | 5.0 | — |
| Learn shieldmaiden | 1–4 | ordinary | shieldmaiden, iceborn_raider | 3 | 4 | — |
| Patrol shieldmaiden | 1–4 | ordinary | shieldmaiden, iceborn_raider, frostfang_wolf | 4 | 5.5 | — |
| Learn rime_skald | 1–4 | ordinary | rime_skald, iceborn_raider | 3 | 4 | — |
| Patrol rime_skald | 1–4 | ordinary | rime_skald, iceborn_raider, frostfang_wolf | 4 | 5.5 | — |
| Learn hearthkeeper | 1–4 | ordinary | hearthkeeper, iceborn_raider | 3 | 4 | — |
| Patrol hearthkeeper | 1–4 | ordinary | hearthkeeper, iceborn_raider, frostfang_wolf | 4 | 5.5 | — |
| Learn frostblade_reaver | 2–4 | ordinary | frostblade_reaver, iceborn_raider | 3 | 4 | — |
| Patrol frostblade_reaver | 2–4 | ordinary | frostblade_reaver, iceborn_raider, frostfang_wolf | 4 | 5.0 | — |
| Combined rimecarver | 2–4 | ordinary | rimecarver, hearthkeeper, iceborn_raider | 3 | 6.0 | — |
| Elite rimecarver | 1–4 | elite | rimecarver, hearthkeeper, frostfang_wolf | 5 | 6.0 | — |
| Combined frostblade_reaver | 2–4 | ordinary | frostblade_reaver, rime_skald, iceborn_raider | 3 | 6.0 | — |
| Elite frostblade_reaver | 2–4 | elite | frostblade_reaver, rime_skald, frostfang_wolf | 5 | 6.0 | — |
| Veteran patrol | 0–4 | elite | harpoon_hunter, iceborn_raider, frostfang_wolf | 4 | 5.0 | — |
| Guardian glacier_troll | 1–4 | miniboss | glacier_troll, iceborn_raider | 4 | 6.5 | — |
| Guardian iceborn_champion | 2–4 | miniboss | iceborn_champion, iceborn_raider | 4 | 6.5 | — |
| Capstone jarl_haldra | 3–4 | major | jarl_haldra, frostfang_wolf | 2 | 8.5 | harpoon_hunter |
| Reserve Outer patrol | 0–4 | elite | iceborn_raider, frostfang_wolf | 1 | 4 | — |

## ironvein-excavation

| Member | Eligible tiers | Tier weights 0–4 | Family | Summon only |
|---|---|---|---:|---|
| pickaxe_delver | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| rivet_gunner | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| packbeetle | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| ore_hauler | 0–4 | 10 / 7 / 4 / 1 / 1 | 0 | no |
| stonewright | 0–4 | 10 / 7 / 4 / 1 / 1 | 0 | no |
| bore_engineer | 2–4 | 0 / 0 / 10 / 7 / 4 | 0 | no |
| vein_surveyor | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| powder_sapper | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| rail_switcher | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| seismic_smith | 2–4 | 0 / 0 / 10 / 7 / 4 | 0 | no |
| siege_machinist | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| obsidian_sentinel | 2–4 | 0 / 0 / 10 / 7 / 4 | 0 | no |
| rivet_turret | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | yes |
| grand_delver | 3–4 | 0 / 0 / 0 / 10 / 7 | 0 | no |

| Recipe | Tiers | Kind | Members | Weight | Whole threat cap | Prerequisites |
|---|---|---|---|---:|---:|---|
| Outer patrol | 0–2 | ordinary | pickaxe_delver, rivet_gunner | 4 | 4 | — |
| Quiet patrol | 0–4 | ordinary | pickaxe_delver | 1 | 4 | — |
| Learn stonewright | 0–4 | ordinary | stonewright, pickaxe_delver | 3 | 4 | — |
| Patrol stonewright | 1–4 | ordinary | stonewright, pickaxe_delver, rivet_gunner | 4 | 5.5 | — |
| Learn ore_hauler | 0–4 | ordinary | ore_hauler, pickaxe_delver | 3 | 4 | — |
| Patrol ore_hauler | 1–4 | ordinary | ore_hauler, pickaxe_delver, rivet_gunner | 4 | 5.5 | — |
| Learn vein_surveyor | 1–4 | ordinary | vein_surveyor, pickaxe_delver | 3 | 4 | — |
| Patrol vein_surveyor | 1–4 | ordinary | vein_surveyor, pickaxe_delver, rivet_gunner | 4 | 5.5 | — |
| Learn powder_sapper | 1–4 | ordinary | powder_sapper, pickaxe_delver | 3 | 4.5 | — |
| Patrol powder_sapper | 1–4 | ordinary | powder_sapper, pickaxe_delver, rivet_gunner | 4 | 6.0 | — |
| Learn rail_switcher | 1–4 | ordinary | rail_switcher, pickaxe_delver | 3 | 4 | — |
| Patrol rail_switcher | 1–4 | ordinary | rail_switcher, pickaxe_delver, rivet_gunner | 4 | 5.5 | — |
| Learn bore_engineer | 2–4 | ordinary | bore_engineer, pickaxe_delver | 3 | 4.5 | — |
| Patrol bore_engineer | 2–4 | ordinary | bore_engineer, pickaxe_delver, rivet_gunner | 4 | 6.0 | — |
| Learn seismic_smith | 2–4 | ordinary | seismic_smith, pickaxe_delver | 3 | 4.5 | — |
| Patrol seismic_smith | 2–4 | ordinary | seismic_smith, pickaxe_delver, rivet_gunner | 4 | 6.0 | — |
| Combined stonewright | 2–4 | ordinary | stonewright, ore_hauler, pickaxe_delver | 3 | 6.5 | — |
| Elite stonewright | 1–4 | elite | stonewright, ore_hauler, rivet_gunner | 5 | 6.5 | — |
| Combined bore_engineer | 2–4 | ordinary | bore_engineer, seismic_smith, pickaxe_delver | 3 | 7.5 | — |
| Elite bore_engineer | 2–4 | elite | bore_engineer, seismic_smith, rivet_gunner | 5 | 7.5 | — |
| Veteran patrol | 0–4 | elite | stonewright, pickaxe_delver, rivet_gunner | 4 | 5.5 | — |
| Guardian siege_machinist | 1–4 | miniboss | siege_machinist, pickaxe_delver | 4 | 6.5 | — |
| Guardian obsidian_sentinel | 2–4 | miniboss | obsidian_sentinel, pickaxe_delver | 4 | 6.5 | — |
| Capstone grand_delver | 3–4 | major | grand_delver, rivet_gunner | 2 | 8.5 | stonewright |
| Reserve Outer patrol | 0–4 | elite | pickaxe_delver, rivet_gunner | 1 | 4 | — |

## magical-forest

| Member | Eligible tiers | Tier weights 0–4 | Family | Summon only |
|---|---|---|---:|---|
| elven_scout | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| orc_trailguard | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | no |
| elven_mender | 0–4 | 10 / 7 / 4 / 1 / 1 | 0 | no |
| orc_rootbinder | 0–4 | 10 / 7 / 4 / 1 / 1 | 0 | no |
| barkhide_warden | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| briar_matriarch | 3–4 | 0 / 0 / 0 / 10 / 7 | 0 | no |
| elven_thornkeeper | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| orc_berserker | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| orc_bloomcaller | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| snapvine | 0–4 | 8 / 7 / 6 / 5 / 4 | 0 | yes |
| orc_drummer | 1–4 | 0 / 10 / 7 / 4 / 1 | 0 | no |
| briar_archer | 2–4 | 0 / 0 / 10 / 7 / 4 | 0 | no |
| ancient_treant | 2–4 | 0 / 0 / 10 / 7 / 4 | 0 | no |

| Recipe | Tiers | Kind | Members | Weight | Whole threat cap | Prerequisites |
|---|---|---|---|---:|---:|---|
| Outer patrol | 0–2 | ordinary | elven_scout, orc_trailguard | 4 | 4 | — |
| Quiet patrol | 0–4 | ordinary | elven_scout | 1 | 4 | — |
| Learn elven_mender | 0–4 | ordinary | elven_mender, elven_scout | 3 | 4 | — |
| Patrol elven_mender | 1–4 | ordinary | elven_mender, elven_scout, orc_trailguard | 4 | 6.0 | — |
| Learn orc_rootbinder | 0–4 | ordinary | orc_rootbinder, elven_scout | 3 | 4.5 | — |
| Patrol orc_rootbinder | 1–4 | ordinary | orc_rootbinder, elven_scout, orc_trailguard | 4 | 6.5 | — |
| Learn elven_thornkeeper | 1–4 | ordinary | elven_thornkeeper, elven_scout | 3 | 4 | — |
| Patrol elven_thornkeeper | 1–4 | ordinary | elven_thornkeeper, elven_scout, orc_trailguard | 4 | 6.0 | — |
| Learn orc_berserker | 1–4 | ordinary | orc_berserker, elven_scout | 3 | 4 | — |
| Patrol orc_berserker | 1–4 | ordinary | orc_berserker, elven_scout, orc_trailguard | 4 | 6.0 | — |
| Learn orc_bloomcaller | 1–4 | ordinary | orc_bloomcaller, elven_scout | 3 | 4 | — |
| Patrol orc_bloomcaller | 1–4 | ordinary | orc_bloomcaller, elven_scout, orc_trailguard | 4 | 6.0 | — |
| Learn orc_drummer | 1–4 | ordinary | orc_drummer, elven_scout | 3 | 4 | — |
| Patrol orc_drummer | 1–4 | ordinary | orc_drummer, elven_scout, orc_trailguard | 4 | 6.0 | — |
| Learn briar_archer | 2–4 | ordinary | briar_archer, elven_scout | 3 | 4 | — |
| Patrol briar_archer | 2–4 | ordinary | briar_archer, elven_scout, orc_trailguard | 4 | 6.0 | — |
| Combined orc_rootbinder | 2–4 | ordinary | orc_rootbinder, elven_mender, elven_scout | 3 | 7.0 | — |
| Elite orc_rootbinder | 1–4 | elite | orc_rootbinder, elven_mender, orc_trailguard | 5 | 7.5 | — |
| Combined briar_archer | 2–4 | ordinary | briar_archer, orc_berserker, elven_scout | 3 | 6.5 | — |
| Elite briar_archer | 2–4 | elite | briar_archer, orc_berserker, orc_trailguard | 5 | 7.0 | — |
| Veteran patrol | 0–4 | elite | elven_mender, elven_scout, orc_trailguard | 4 | 6.0 | — |
| Guardian barkhide_warden | 1–4 | miniboss | barkhide_warden, elven_scout | 4 | 6.5 | — |
| Guardian ancient_treant | 2–4 | miniboss | ancient_treant, elven_scout | 4 | 6.5 | — |
| Capstone briar_matriarch | 3–4 | major | briar_matriarch, orc_trailguard | 2 | 9.0 | elven_mender |
| Reserve Outer patrol | 0–4 | elite | elven_scout, orc_trailguard | 1 | 4 | — |

Recipe caps do not override lower ordinary zone-tier budgets. Support/disruption limits, Court tide safety and one-new-special teaching constraints still apply. Members with zero tier weight or summon-only status cannot enter ordinary/capstone random selection. Fallbacks remain same-zone and bounded.

Dungeon locals stop appearing in ordinary selection at tier 2 (global depth 13). Forest, Court, Ironvein and Frostwake reuse their existing ordinary attackers at depth with stronger legal special/role combinations; those zones do not yet have separate deep ordinary attacker art/definitions. No unavailable Guild or foreign bosses are invented, and HP is not padded to fill this content gap.
