# Dungeon Matcher — Progression and Run Pacing

## Purpose and authority

This document defines the finalized high-level progression, encounter-pacing, replayability, and faction-escalation philosophy for Dungeon Matcher.

It is authoritative for how chapters/eras, enemy pools, milestone encounters, card cadence, encounter speed, and post-King progression should be designed.

Where older documentation or prototype data describes chapters as strict exact-wave scripts (for example, fixed wave-8 / wave-16 chapter endpoints or exact per-wave enemy lists), this document supersedes that interpretation. Existing exact-wave numbers may still be used as temporary tuning anchors or guaranteed milestone gates where explicitly justified, but they are not the default chapter model.

This document does not freeze final balance values. Exact spawn weights, threat costs, HP values, card rewards, and encounter durations remain tunable unless explicitly finalized elsewhere.

## Core pacing goal

Dungeon Matcher is a mobile match-3 battler and should feel fast, varied, readable, and replayable.

The run must not become a sequence of slow HP-sponge fights. Difficulty should increasingly come from enemy combinations, behavior, board pressure, prioritization, and interaction complexity rather than simply making every enemy take much longer to kill.

A strong build and skilled play should sometimes allow the player to destroy a wave very quickly. The game should not artificially slow a successful player merely because a wave is meant to be difficult.

The intended rhythm is frequent change: new compositions, new enemy families, new board-pressure patterns, card choices, milestone enemies, and occasional major bosses should keep the run from feeling repetitive.

## Chapters are weighted progression eras, not scripts

A chapter/era means:

> Around this part of the run, these enemies become the dominant population and the narrative focus shifts toward this faction.

A chapter does **not** mean:

> Wave 1 is always enemy A, wave 2 is always enemy B, wave 3 is always enemy C.

Ordinary encounters should be procedurally composed from overlapping weighted pools.

New factions rise in weight as the run progresses. Previous factions decline in weight rather than disappearing immediately. Different runs should therefore produce different enemy sequences and mixed compositions while still communicating a clear escalation.

Eligibility, weights, encounter constraints, category limits, active-slot limits, milestone rules, and future threat-budget rules should shape the composition without turning the run into a fixed encounter script.

## Finalized opening escalation

The opening world progression is:

**LOCALS / VILLAGERS → GUARDS → KNIGHTS → ROYAL FORCES → THE KING → ADVENTURER GUILD → WIDER FANTASY WORLD**

This replaces the older idea that Mercenaries and the Adventurer Guild must appear before the King.

Mercenaries remain a valid faction, but they fit better after the King as part of the wider post-King world rather than as a mandatory pre-King chapter.

### Why this order is locked

The first arc should be easy to read narratively and should reach a major payoff quickly enough for a mobile run.

1. **Locals / Villagers** — ordinary people investigate or stumble into the dungeon.
2. **Guards** — the town responds because the locals cannot handle the threat.
3. **Knights** — the Crown now takes the dungeon seriously.
4. **Royal Forces** — the kingdom commits its elite military.
5. **The King** — the first true major Boss and climax of the opening kingdom arc.
6. **Adventurer Guild** — after the King fails, news spreads and professional monster-fighters begin entering the dungeon.
7. **Wider fantasy world** — Guild parties, mercenaries, monster hunters, rival powers, famous adventurers, cultists, mages, bounty hunters, and other factions can expand the long-term roster.

The King is therefore the boundary between two broad narrative phases:

- **Act 1: the Kingdom reacts to the dungeon.**
- **Act 2: the wider world discovers the dungeon.**

## Overlapping faction pools

Old enemies should remain relevant after their introductory era.

When Guards become dominant, Locals should still appear.

When Knights become dominant, Guards should remain common and Locals should become less frequent.

When Royal Forces become dominant, Knights should remain common, Guards occasional, and Locals rare.

After the King, the dungeon can contain almost anything introduced previously because the world has now become aware of it. Guards, Knights, villagers, failed expeditions, escorts, treasure seekers, lost townsfolk, and Guild parties can plausibly overlap.

This serves both narrative logic and replayability. Existing enemies should not be discarded merely because a newer faction exists.

### Example weighting philosophy

These numbers are examples for tuning direction, not permanent balance values:

| Current era | New/dominant faction | Previous faction | Older factions |
| --- | ---: | ---: | ---: |
| Guards | ~65% Guards | ~35% Locals | — |
| Knights | ~60% Knights | ~30% Guards | ~10% Locals |
| Royal Forces | ~55% Royals | ~30% Knights | ~10% Guards, ~5% Locals |
| Guild | ~50% Guild | ~20% Royals | ~15% Knights, ~10% Guards, ~5% Locals |

Weights should also shift gradually inside an era rather than changing abruptly at one exact wave.

For example, early Royal progression might still be mostly Knights, while later Royal progression becomes mostly Royal units.

## First-pass run-position anchors

Approximate wave positions are useful for pacing and tuning, but they are **not exact chapter boundaries or deterministic scripts**.

A good first-pass target is:

| Era | Approximate run position | Dominant content |
| --- | ---: | --- |
| Locals | ~1–6 | Farmers, villagers, Miner and local troublemakers |
| Guards | ~5–12 | Spear Guard, Crossbow Guard, Barricade Guard and local carryover |
| Knights | ~10–18 | Sword Knight, Spear Knight, Shield Knight and Guard carryover |
| Royal Forces | ~17–25 | Elite Royal units with Knight carryover |
| The King | ~24–26 | First major Boss and opening-arc climax |
| Guild era | ~26+ | Adventurer parties plus the full legacy pool |

The overlap is intentional. These are pacing anchors for spawn weights and encounter-generation logic, not instructions that wave 12 must contain one exact composition.

## Milestone mini-bosses

Town Marshal, Siege Sergeant, Knight Captain, and similar enemies should be treated as milestone encounters or strongly biased milestone opportunities rather than ordinary chapter endpoints that force every surrounding wave to be identical.

The generator may strongly increase the chance of an appropriate mini-boss near the end of its faction-heavy era, or guarantee that the player eventually sees it within an acceptable window, while still allowing different surrounding encounters in different runs.

The King is different because he is a major narrative Boss and progression gate. A guaranteed King encounter around the first major run milestone is appropriate.

## The King target

The first-pass pacing target is for the King to appear around **wave 25**, with a small tuning window around that point.

This creates a major payoff before the opening arc becomes overlong and gives the player enough time to form a meaningful run build.

The King should not be delayed deep into a long sequence of Mercenary and Guild chapters before the first true Boss.

## Cards and run rhythm

A card choice approximately every **5 waves** is the preferred first-pass cadence.

This is also a tuning anchor rather than a rule that cannot change, but the design goal is frequent build evolution.

A representative opening rhythm is:

- around wave 5: card choice
- around wave 10: card choice
- around wave 15: card choice
- around wave 20: card choice
- around wave 25: King / major boss reward moment

By the first major Boss, the player should usually have made several meaningful build decisions so the current run already feels distinct.

The desired pacing loop is roughly:

**combat → combat → interesting composition → combat → card/build decision → repeat**

The player should not spend a long uninterrupted stretch doing nothing except matching against similar enemies.

## Encounter duration philosophy

Dungeon Matcher should avoid slow, repetitive HP-sponge combat.

Approximate experience targets are:

| Encounter type | Desired first-pass feel |
| --- | --- |
| Easy/basic wave | ~15–30 seconds |
| Typical wave | ~25–45 seconds |
| Difficult composition | ~40–60 seconds |
| Mini-boss | ~45–75 seconds |
| Major Boss | ~60–120 seconds |

These are not hard timers and should not be enforced mechanically. They are pacing targets for balance review.

A strong build may clear a wave far faster, including occasional ~10-second clears. That is desirable feedback for successful play.

If encounters routinely exceed these ranges because enemies simply have too much HP, the preferred fix is not automatically to make everything deal more damage. Re-evaluate health, composition, ability cadence, threat density, and encounter structure.

## Difficulty should come from composition, not only stats

Increasing difficulty should prioritize:

- more meaningful enemy combinations;
- competing target priorities;
- readable board pressure;
- interacting abilities;
- higher-risk compositions;
- more demanding but understandable timing;
- stronger use of previously learned mechanics.

Numerical scaling still matters, but it should support these systems rather than replace them.

Do not simultaneously inflate enemy HP, enemy count, action frequency, and board interference without accounting for the total encounter burden. That creates slow, oppressive waves rather than interesting difficulty.

## Encounter threat budget direction

The long-term encounter generator should use a **threat budget** or equivalent composition-cost system in addition to active-slot and category constraints.

Each enemy should eventually consume a threat cost based on how much pressure it contributes. A simple Normal enemy may cost little; a high-impact Special or Mini-boss costs more.

Illustrative only:

- Farmer: threat 1
- basic Guard: threat 2
- Shield Knight: threat 3
- dangerous caster: threat 4
- Mini-boss: threat 7

If an encounter has a budget of 7, several different valid compositions might be generated rather than always producing the same trio.

The threat budget must account for **behavioral and mechanical pressure**, not just HP. A defensive unit, board manipulator, healer, or coordination enemy may deserve a higher cost even if its raw health is modest.

This prevents the generator from accidentally creating combinations such as three high-HP defensive enemies that turn one mobile wave into a long grind.

The threat budget should rise over the run, but complexity and dangerous synergies should consume budget too.

Until a dedicated threat-budget implementation exists, current slot/category constraints remain valid temporary encounter-safety mechanisms. Future work should not interpret that temporary state as a rejection of the threat-budget design.

## Replayability rules

Replayability is a core pacing requirement, not a secondary bonus.

Ordinary runs should differ in:

- exact enemy sequence;
- mixed faction composition;
- which older enemies reappear;
- timing of some milestone encounters inside their allowed windows;
- card/build choices;
- resulting tactical priorities.

The run should still communicate progression even when two players see different exact encounters.

Avoid exact-wave override tables for ordinary chapter progression unless a specific narrative/tutorial/boss reason requires a guaranteed encounter.

## Post-King world design

After the King loses, the game should deliberately become broader rather than simply introducing a stronger linear replacement faction.

The Adventurer Guild becomes the first major post-King pillar because professional parties now have a reason to investigate the dungeon.

Guild content can introduce much broader combat roles such as:

- Warrior / Fighter
- Paladin
- Rogue
- Archer / Ranger
- Mage
- Cleric / Healer
- Alchemist
- Bard
- Summoner
- other specialized adventurer classes

Older enemies remain eligible in mixed encounters.

Mercenaries also fit naturally in this phase as hired specialists, bounty hunters, or competing expeditions rather than as a mandatory chapter before the King.

Later content can expand into famous adventurers, rival kingdoms, monster hunters, cults, powerful mages, and other world factions without undermining the clean opening kingdom arc.

## Design review checklist

When adding or balancing a chapter, enemy pool, milestone, card cadence, or encounter generator, ask:

1. Does this keep the match-3 layer fast and responsive?
2. Does it create new decisions rather than only adding HP?
3. Can different runs produce meaningfully different compositions?
4. Do older enemies still have useful opportunities to reappear?
5. Does the current faction feel dominant without requiring deterministic wave scripts?
6. Is the total encounter burden appropriate for a mobile session?
7. Does a difficult enemy combination consume enough encounter budget/capacity?
8. Are cards or other build decisions arriving frequently enough to change how the run feels?
9. Is the next major narrative payoff close enough that the run does not feel padded?
10. Does the escalation still read clearly as Locals → Guards → Knights → Royal Forces → King → Guild / wider world?

## Locked summary

The finalized progression philosophy is:

**weighted overlapping spawn eras + procedural encounter composition + milestone mini-bosses + cards roughly every five waves + fast combat + a first major King boss around wave 25 + persistent eligibility for older factions + post-King expansion into the Adventurer Guild and wider fantasy world.**

The finalized opening narrative order is:

**LOCALS / VILLAGERS → GUARDS → KNIGHTS → ROYAL FORCES → THE KING → ADVENTURER GUILD → WIDER FANTASY WORLD.**

Future progression work should preserve this structure unless the user explicitly approves a new direction.
