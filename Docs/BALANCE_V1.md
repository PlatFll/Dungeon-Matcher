# Balance v1 implementation record

Status: connected Balance v1 implemented. Numerical targets remain design hypotheses; executed tests, final engine measurements and remaining tuning limits are recorded in [the validation record](Validation/BALANCE_V1_VALIDATION.md).

## Plan and authority

Implement account persistence and independent character stats, shared mastery, cards and shield balance, hybrid encounters and bounded interference, then consumables and the connected menu/HUD loop. Compile at each stage. Finish with disposable-save regression tests, real Play Mode checks, portrait screenshots, local pacing measurements and a reviewed PR.

The approved September 14 brief supersedes older every-fifth-wave rewards, solo milestone defaults, wave-25 King tuning, and prototype enemy numbers. Weighted overlapping eras and all existing board/damage/lifecycle invariants remain authoritative. The separate pacing attachment referenced by the brief was unavailable; `PROGRESSION_PACING.md` supplies the durable philosophy.

## Targets and economy

| Target | Range |
| --- | --- |
| Beginner failure | 2–4 minutes |
| Ordinary repeat attempt | 4–8 minutes |
| Regular wave | 12–24 seconds |
| Pressure composition | 20–35 seconds |
| Miniboss formation | 30–50 seconds |
| King formation | 45–80 seconds |
| Successful opening arc | 10–14 minutes including transitions and choices |

No time limit, death wave, damage cap or adaptive enemy scaling enforces these targets. Bardley's committed ability cost stays **1**; normal-cost measurements must explicitly use a temporary 80-energy configuration and report that distinction.

| Permanent progression | Bardley | Rattlebones |
| --- | ---: | ---: |
| Level 1 HP | 80 | 100 |
| HP per level | 8 | 10 |
| Level 1 gem damage | 10 | 11 |
| Gem damage per level | 0.65 | 0.55 |
| Ability damage increase per level | 7% of base | 20% of base |
| Shield cap at level 1 | 40 | 45 |
| Shield cap per level | 3 | 3 |
| Level cap | 20 | 20 |

Upgrade from level L costs `25 + 15*(L-1) + 5*(L-1)^2` shared gold. No XP. Stats are recomputed from immutable definitions, a run-start permanent level snapshot, then temporary card modifiers. No multiplication of already modified values.

Shared unlocks use the highest individual character level, never summed levels: directional bombs 2, Poison 3, Healing 4, Shield 5. Fresh five-gem shapes create Color Crystals; locked straight-four shapes clear all four gems with normal rewards. Color Crystals have no levels. Valid explicit legacy selections are grandfathered account-wide.

Cards after completed waves **2, 5, 9, 13, 17, 21, 25, 28**. First choice arrives early, with increasingly long play stretches. Potion price 18, Bomb price 24. Potion heals 35% maximum HP. Each equipped type loads at most three charges once per run; stock is deducted only on accepted use. Independent five-second game-time cooldowns pause with gameplay.

Gold: each completed wave pays `5 + floor((wave-1)/3)`; milestone completion adds 12; defeating the King formation adds 60; each newly reached best wave adds 2; first King clear adds 80. No kill or summon income. Journal completed waves as they happen; finalize once on death, victory, Retry, menu exit or next startup after interruption. Partial current waves pay zero. Early 4-wave failures earn 29 gold, enough for the first upgrade; a new best of 10 waves earns 82 before milestone bonuses (94 with the Marshal). Deeper waves pay more per comparable combat time.

Shield Bomb starts at 25 plus 2 per permanent level. Shield grants add to the cap without duration refresh. Existing 25% mitigation applies once to the entire hit when shield was present; reduced overflow damages HP. Aegis Reservoir raises both grant and cap. HP and shield remain separate.

## Evidence

The focused Unity suite passed 231 tests with no failures/skips, including a Play Mode wrapper exercising 108 existing lifecycle cases. Actual menu/game integration, concurrent disruption, Royal mechanics, fresh special creation, account reload, independent audio persistence across a Unity restart and portrait render checks were executed. See [the validation record](Validation/BALANCE_V1_VALIDATION.md) for separate compilation, functional, visual and pacing evidence; synthetic play is not a claim about human retention.

## Enemy data

All category and individual stat multipliers are 1. HP and damage multiply by `1 + 0.01*(wave-1)`, round once; attack intervals and special cadence do not accelerate. Player-power correction is disabled. Enemy slots remain three.

| Enemy | Base HP | Hit(s) | Interval | Eligible from | Threat |
| --- | ---: | --- | ---: | ---: | ---: |
| Farmer | 30 | 5 | 10s | 1 | 1 |
| BasketVillager | 35 | 4 | 9s | 1 | 1 |
| PanVillager | 40 | 6 | 10s | 3 | 1 |
| Miner | 50 | 6 | 11s | 4 | 2 |
| BarricadeVillager | 45 | 6 | 11s | 5 | 2 |
| TownMarshal | 120 | 6 | 10s | 7 | 4 |
| SpearGuard | 60 | 6 | 9s | 6 | 1.5 |
| CrossbowGuard | 55 | 6 | 10s | 7 | 2 |
| BarricadeGuard | 60 | 6 | 11s | 8 | 2 |
| SiegeSergeant | 240 | 5 | 11s | 12 | 4 |
| Knight | 100 | 5 | 10s | 11 | 2 |
| SpearKnight | 100 | 3 + 5 | 11s | 12 | 2 |
| ShieldKnight | 90 | 4 | 11s | 13 | 2.5 |
| KnightCaptain | 250 | 7 | 11s | 18 | 4.5 |
| RoyalSwordsman | 130 | 4 + 5 | 11s | 19 | 2.5 |
| RoyalLancer | 140 | 3 + 7 | 12s | 20 | 2.5 |
| RoyalArbalist | 115 | 4 + 4 | 10s | 21 | 2.5 |
| RoyalStandardBearer | 105 | 5 | 12s | 22 | 3 |
| CourtMage | 95 | 5 | 12s | 23 | 3 |
| RoyalArchbishop | 210 | 5 | 12s | 24 | 4 |
| King | 480 | 9 | 11s | 29 | 7 |

Normal pools overlap; a declining weight tail retains older enemies. Milestone windows: Marshal 7–8 with one local escort; Sergeant 12–14 with one guard; Captain 18–20 with two knights; Archbishop 24–26 with one royal; King 29–30 with required Archbishop. King victory ends the supported opening arc. The Archbishop escort is an explicit narrative exception to leader repeat exclusion.

## Authored library

45% authored opportunity, otherwise constrained existing weighted selection. No immediate recipe repeat. Global random cap: two disruptors and one support; a named recipe may explicitly allow three light disruptors within its whole-formation budget.

| Recipe | Waves | Formation | Purpose |
| --- | --- | --- | --- |
| curious-locals | 3–6 | 1 × Farmer, 1 × PanVillager | Practice target priority; threat ≤ 3 |
| first-excavation | 4–6 | 1 × Miner, 1 × Farmer | Learn mining counterplay; threat ≤ 3 |
| two-miners | 6–12 | 2 × Miner | Combine two small mining threats; threat ≤ 4 |
| village-wall | 5–9 | 1 × BarricadeVillager, 1 × BasketVillager | Break the fragile wall; threat ≤ 3 |
| chain-lesson | 7–11 | 1 × CrossbowGuard, 1 × SpearGuard | Clear the chained gem; threat ≤ 3.5 |
| guard-wall | 8–15 | 1 × BarricadeGuard, 1 × SpearGuard | Choose wall breaker or attacker; threat ≤ 3.5 |
| mining-fortification | 10–16 | 1 × Miner, 2 × BarricadeGuard | Test structural priority with capped obstacles; threat ≤ 6 |
| knight-patrol | 11–23 | 2 × Knight | Breathing room against direct attackers; threat ≤ 4 |
| shield-lesson | 13–18 | 1 × ShieldKnight, 1 × Knight | Focus the shield support; threat ≤ 4.5 |
| chains-and-spears | 14–20 | 1 × CrossbowGuard, 1 × SpearKnight, 1 × Knight | Practice chains before the Captain; threat ≤ 6 |
| royal-vanguard | 19–27 | 1 × RoyalSwordsman, 1 × RoyalLancer | Learn two-hit royal attacks; threat ≤ 5 |
| royal-fireline | 21–28 | 1 × RoyalArbalist, 1 × RoyalSwordsman, 1 × Knight | Target ranged pressure; threat ≤ 7 |
| banner-lesson | 22–25 | 1 × RoyalStandardBearer, 1 × RoyalSwordsman | Break a banner before combined pressure; threat ≤ 5.5 |
| ice-lesson | 23–26 | 1 × CourtMage, 1 × Knight | Learn freeze counterplay; threat ≤ 5 |
| court-formation | 26–28 | 1 × CourtMage, 1 × RoyalStandardBearer, 1 × RoyalArbalist | Test freeze and banner priority; threat ≤ 8.5 |
| last-breath | 27–28 | 1 × RoyalLancer, 1 × RoyalSwordsman | A readable attacker patrol before the King; threat ≤ 5 |

## Complete card review

All 27 catalog entries are retained. Numeric general-purpose stacks are bounded; expensive ability/energy cards are excluded from Bardley's 1-energy development configuration. Special-dependent cards require both an account unlock and a compatible equipped mastery shape (directional bombs need the unlock). Affinity healing remains available on a fresh account. Rarity multipliers are Common 1, Uncommon 0.65, Rare 0.35, Epic 0.20, multiplied by the editable asset weight. Character-specific cards remain Epic. Offers draw up to three distinct eligible cards without replacement using the separate draft RNG. No forced build or hidden pity selection.

| Card | Effect per stack / mechanic | Cap | Rarity | Asset weight | Additional eligibility |
| --- | --- | ---: | --- | ---: | --- |
| Aegis Reservoir | Shield Bombs grant +30% shield. Maximum shield +30%. | 3 | Common | 1 | Shield equipped |
| Arcane Efficiency | Active ability costs 15% less energy. | 2 | Rare | 1 | cost > 1 |
| Bombsmith | Row and Column Bomb chains deal +25% gem damage. | 1 | Common | 1.5 | Directional bombs |
| Boss Hunter | Deal +20% damage to Minibosses and Bosses. | 1 | Uncommon | 1 | wave 5+ |
| Cascade Catalyst | Each cascade depth adds +10% gem damage. | 1 | Common | 1.5 | Any character |
| Chain Reaction | Special chains gain +10% damage per extra detonation, up to +30%. | 1 | Rare | 1 | Any character |
| Chromatic Conductor | Color Crystal clears grant +1 extra ability energy per gem. | 1 | Uncommon | 1.5 | cost > 1 |
| Corrosive Formula | Poison ticks deal +30% damage. | 3 | Common | 1 | Poison equipped |
| Efficient Casting | -10% active ability energy cost | 2 | Common | 1 | cost > 1 |
| Emergency Plating | The first shield break each wave restores 10 shield. | 1 | Rare | 1 | Shield equipped |
| Executioner | Deal +25% damage to enemies below 30% HP. | 1 | Uncommon | 1 | Any character |
| Final Word | Royal Decree deals +25% damage per gem. | 3 | Epic | 1 | skeleton; royal_decree |
| Gem Grinder | +15% gem damage | 3 | Common | 1 | Any character |
| Glass Cannon | +30% gem and ability damage. -15% maximum HP. | 1 | Rare | 1.5 | Any character |
| Horrible Encore | +1 Cracked Gems target | 2 | Epic | 1 | bardley; cracked_gems |
| Longer Reign | Royal Decree lasts +2 seconds. | 3 | Epic | 1 | skeleton; royal_decree |
| Mana Spark | +20% ability energy gained | 3 | Common | 1 | cost > 1 |
| Opening Volley | Deal +25% damage to enemies above 80% HP. | 1 | Uncommon | 1 | Any character |
| Prepared Casting | Begin each wave with +15 ability energy. | 1 | Uncommon | 1.5 | cost > 1 |
| Reinforced Flask | Healing Bombs restore +30% HP. | 3 | Common | 1 | Healing equipped |
| Resonant Cracks | Every third Cracked Gem detonation refunds 5 energy. | 1 | Epic | 1 | bardley; cracked_gems; cost > 1 |
| Siegebreaker | +1 durability damage to barricades | 1 | Common | 1 | wave 5+ |
| Slow Venom | Poison lasts +40% longer, but ticks deal -15% damage. | 1 | Uncommon | 1 | Poison equipped |
| Sour Note | Cracked Gem explosions deal +30% fixed damage. | 3 | Epic | 1 | bardley; cracked_gems |
| Strong Remedy | +20% healing | 3 | Common | 1 | Any character |
| Thicker Hide | +20 maximum HP and current HP | 3 | Common | 1 | Any character |
| Toxic Momentum | While any enemy is poisoned, gain +20% ability energy. | 1 | Uncommon | 1 | Poison equipped; cost > 1 |

Build routes: Gem Grinder / Cascade Catalyst / Chain Reaction reward board setup; Bombsmith supports directional chains; poison cards add sustained damage and energy; Aegis / Emergency Plating add shield capacity and recovery; Prepared Casting and efficiency support repeated casts; the two characters have distinct ability cards. Glass Cannon adds 30% damage while removing 15% maximum HP once. Its negative HP change now takes effect immediately; neither positive nor negative cards mutate the definition. Aegis increases Shield Bomb grants and shield cap together, so a grant increase does not disappear at an unchanged cap.

## Permanent level price table

| Upgrade | Gold | Upgrade | Gold |
| --- | ---: | --- | ---: |
| 1 → 2 | 25 | 11 → 12 | 675 |
| 2 → 3 | 45 | 12 → 13 | 795 |
| 3 → 4 | 75 | 13 → 14 | 925 |
| 4 → 5 | 115 | 14 → 15 | 1065 |
| 5 → 6 | 165 | 15 → 16 | 1215 |
| 6 → 7 | 225 | 16 → 17 | 1375 |
| 7 → 8 | 295 | 17 → 18 | 1545 |
| 8 → 9 | 375 | 18 → 19 | 1725 |
| 9 → 10 | 465 | 19 → 20 | 1915 |
| 10 → 11 | 565 | 20 (cap) | — |

At level 5, Bardley has 112 HP, 12.6 gem damage, 26 damage per cracked center and a 52 shield cap; RattleBones has 140 HP, 13.2 gem damage, 9 Decree damage per gem and a 57 shield cap. At level 20 those become 232 / 22.35 / 47 / 97 and 290 / 21.45 / 24 / 102 respectively. Ability values round at the existing damage resolver. RattleBones gains one full point of base Decree damage each level so every upgrade visibly improves the ability.

## Enemy ability quantities and counterplay

Cadences below count accepted completed matching moves, not invalid swaps, cascades, consumables or environmental clears. Every board action queues behind settlement. A rejected placement adds nothing; readiness/retry remains owned by the existing ability runtime. The global structural limit is **10 mined plus barricaded cells**; standards retain their separate existing lifetime and cap. The movable-chain queue stops at **6 total pinned gems**, including existing freezes when choosing capacity. Normal random formations allow at most two board disruptors and one support; the named three-disruptor recipe is explicitly budgeted.

| Enemy / action | Quantity and cadence | Targeting / cap | Warning, counterplay and cleanup |
| --- | --- | --- | --- |
| Miner | One hole every 5 moves | Random legal ordinary unpinned cell; 3 per owner, 4 mines globally; shared structural limit 10 | Pickaxe impact/flash. Candidate removal must preserve a legal move. Kill the Miner to queue restoration of its holes; refills/cascades finish before control returns. |
| Barricade Villager | One wooden, one-hit wall every 4 moves | Controlled random legal cells; 3 per owner | Placement materialization. Adjacent clears, bombs and abilities deal existing obstacle damage. Walls persist after death with ownership removed. |
| Barricade Guard | Two stone, two-hit walls every 4 moves | Controlled random legal cells; 4 per owner | First hit downgrades stone to wood. Same counterplay and orphan lifetime; earlier queued owners count toward the shared limit when each cast executes. |
| Siege Sergeant | Alternates three wooden walls and Hammer Time every 4 moves | Prefers a complete straight run, otherwise random distinct legal cells; 6 walls per owner; specials protected | Hammer marks one adjacent ordinary pair for 2 moves. Remove/convert/pin either identity to cancel the entire strike. Failed warning deals base 10 through the shield-aware path, removes both targets environmentally and settles. One warning per owner. Owned walls grant the established 20% damage reduction. Death cancels warning/passive; walls persist orphaned. |
| Crossbow Guard | Adds one chain every 3 moves, up to 2 owned | Ordinary unpinned gems; each placement must preserve a legal move | Same falling-chain rules as Captain: no manual swap, gravity allowed, matching/special destruction removes chain, adjacency alone does not. Death/disable queues release; emergency reshuffle can release pins. |
| Knight Captain | Alternates top-up to 3 chains and On My Mark every 4 moves | Shared chain queue; command snapshots eligible available Crown allies in roster order | Command wind-up and spaced complete normal sequences. Kill/stagger an ally to interrupt participation; killing the Captain cancels outstanding commands and releases reservations/chains. No immunity or forced escort-first order. |
| Shield Knight | Every 6 moves: +10 shield to each living ally, +12 to self | Enemy shield cap 30; caster never receives its ally grant | Grants add up to the cap, with no timed expiration or refresh. Shields live on the recipient and remain if the Knight dies. Shield VFX; focus support or break shields with damage. Existing 25% hit mitigation and reduced overflow remain centralized. |
| Town Marshal | Alternates one summon and a rally opportunity every 4 moves | Farmer/Pan/Basket pool; only free slots within 3 total; roster-based local eligibility | Summon becomes protector for 2 moves; killing it ends interception and resets a held-ready special. Rally is +40% attack speed for 5 seconds, non-stacking. Summons survive Marshal death; no summon income. |
| Royal Standard Bearer | One standard every 5 moves, at most one owned | Legal ordinary top-row cell; one non-gem occupant | Existing standard falls with actual gravity openings and leaves at the bottom. Clear below it to end its shared non-stacking +20% Crown attack-speed aura. It cannot be directly matched/bombed away; death orphans the standard rather than erasing it. |
| Court Mage | One freeze every 5 moves; 3 per owner | Legal ordinary unpinned cell, preserving a move | Ice remains fixed under gravity and cannot be manually swapped. Matching/destroying that gem breaks the ice; adjacent clears do not. Owner death queues release and normal settlement. This is deliberately distinct from chains. |
| Archbishop | Alternates Restoration and Benediction every 4 moves | Three distinct runes, 3-move warning; blessings on at most 2 other allies | Destroy/convert rune identities to cancel individual heals. Each survivor removes its gem environmentally and heals 3.3% of a newly chosen triage target's max HP. Need × rank weights select targets (1 / 1.15 / 1.35 / 1.6); meaningful wounded allies halve self-priority. Blessing multiplies the next accepted whole normal sequence by 1.4. Death cancels marks and unspent blessings. |
| King | Judgment → Assault → Bombardment every 4 moves; 3 when enraged | Judgment: three distinct gems for 3 moves; Bombardment: one row plus one column for 2 moves; command snapshots explicitly eligible Royals | Judgment deals base 12 per surviving mark; remove individual identities to prevent hits. Bombardment lanes cannot be cancelled but preserve specials/structures and deal base 6 once. Commands use complete normal sequences at ×1.1 with 0.6s wind-up and 0.12s spacing. Surviving 50%/25% crossings each fill free slots once, never with an Archbishop; first crossing adds ×1.2 normal damage and ×1.25 attack speed. Death cancels outstanding actions; independent reinforcements remain. |

Teaching order: Miner and local walls precede multiple-owner recipes; Crossbow chains precede Captain chains/commands; two-hit Knights precede Royal two-hit commands; separate banner and ice recipes precede the combined court. Sergeant teaches identity-following warnings before Archbishop/King marks. Older low-threat opponents remain possible breathing room through declining spawn weights. Pressure derives from role combinations, not player-level compensation.

## Healing, ability and economy reasoning

Routine affinity healing is **1 HP per genuinely destroyed affinity gem**, retaining the existing +15% per cascade-depth multiplier and global healing-card channel. The initial trial at 3 HP allowed a casual policy to repair most low-rank damage; reducing the number preserves color-based defensive agency while making healing builds and optional potions useful.

Bardley's five-target, 3×3 cracked-chain mechanics and energy entitlements are unchanged. Base fixed matching-color damage per cracked center is now **20**, plus level and existing ability/card modifiers. The initial 50-damage trial let normal-cost automated casts defeat the King formation in roughly 17–23 seconds. This change adjusts ability damage, never enemy HP to compensate for the development energy override. RattleBones retains a 100-energy, seven-second Royal Decree with base five bonus damage per gem; ability growth adds one base damage per permanent level.

An early matching opportunity deals roughly 30–33 matching-color damage before cascades/cards: early 30–40 HP enemies usually take one or two productive matches, while mixed weaknesses reward choosing the useful colors. Low-rank attacks now deal 4–6 rather than the initial 2–3, so inefficient color choices have a visible cost. Regular Knight attacks can be less damaging than a Guard's; higher HP, two-hit variants and support combinations supply their distinct pressure. Enemy HP/damage scale only 29% from wave 1 to 30, keeping composition dominant.

First best at wave 4 pays 29 gold versus the first 25-gold level; level 5 costs 260 total gold from level 1. A first King completion on wave 29 pays 517 (271 progress + 48 milestones + 60 King + 58 best + 80 first clear); repeating it pays 379. Wave 30 totals are 533 first / 393 repeat. A full three-potion/three-Bomb kit costs 126, so supplies are an optional spending tradeoff. Level 20 costs 13,585 gold cumulatively and is unnecessary for the supported opening arc; baseline level-5 clears without items are part of validation.

## Persistence and failure policy

`account-v1.json` stores the wallet, per-character level list, shared unlock list, two inventory counts, two equip flags, best progress, first-King flag, one active journal and last reward. Unity JsonUtility's empty-object representation of null journals is normalized. Transactions write/flush a temporary snapshot, replace the current file atomically and retain a backup before publishing state. Purchases, leveling, charge deductions and payouts cannot partly update memory when the disk write fails. Interrupted recovery settles only the journal's completed waves once; abandoned partial waves never pay. A readable backup recovers a complete earlier transaction; corrupt originals/unsupported versions are preserved rather than silently reset.

Only explicitly saved valid legacy mastery selections count as grandfathered unlocks. No migration rewrites existing preference keys. Scene Retry resets temporary cards, board actors and per-run charge snapshots while preserving permanent levels, shared unlocks, inventory and wallet. Audio uses separate persistent music/SFX keys. Automated and Play Mode tests use disposable account files and temporary character/mastery scopes; audio restart validation restores the original keys and values.

## Editable implementation locations

- Economy, shared unlock schedule, shield-bomb growth, consumable values, shared interference caps and card cadence: `Assets/_Game/Resources/Balance/BalanceV1.json`.
- Immutable player growth: `Resources/Players/Player_Bardley.asset` and `Player_Skeleton.asset`; ability data under `Data/Player Abilities`.
- Individual enemy stats, cadence, roles and escorts: `Data/Enemies/Enemy_*.asset`; wave bands, opportunity windows, threat curve and recipes: `Data/Balance/WaveSpawnProfile_Standard.asset`; scaling: `DifficultyProfile_Standard.asset`.
- Card effects, rarity, weights, caps and eligibility: `Resources/RunUpgrades/*.asset`, with existing typed mechanic implementations in `RunUpgradeResolver`/`RunUpgradeGameplayHooks`.
- Typography family: `Resources/UI/Typography.asset` (existing Liberation Sans TTF and TMP SDF references). Art: `ArtSource/Consumables/*.ase`, redraw scripts, source crop and `Tools/Build-ConsumableArt.ps1`; imported runtime sprites in `Resources/UI/Consumables`.
- Repeatable checks: `Tools/Test-Balance.ps1`, `Tools/Validate-Unity.ps1`, and Unity's **Dungeon Matcher → Validation → Balance v1 Integration / Pacing** menu actions. The detailed final evidence is in [Validation/BALANCE_V1_VALIDATION.md](Validation/BALANCE_V1_VALIDATION.md).
