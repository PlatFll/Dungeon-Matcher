# Dungeon Matcher Encounter Pacing

Chapters are weighted progression/spawn eras. This supersedes the old fixed Chapter 2 teaching script; ordinary waves do not prescribe exact enemy identities. Current tunable data lives in `WaveSpawnProfile_Standard.asset` and the enemy definitions.

| Era | First-pass progression | Pool and capacity |
| --- | --- | --- |
| Locals | 1–7 | Farmer, Pan/Basket Villagers and existing local Specials. One enemy initially, two by wave 3, two or three from 6. At most one Special. Wave 5 retains a category-only introduction to Specials. |
| Town Calls for Help | 9–15 | Spear Guard, Crossbow Guard and Barricade Guard become eligible at 9; older locals fade. Two or three enemies, at least one Normal, at most one Special. |
| Crown / Knights | 17–20 | Sword and Spear Knights are Normal; Shield Knight is Special. Crown weights rise while returning Guards decline. Two or three enemies, at least one Normal, at most one Special. |
| Crown formations | 21 onward | Knight Captain becomes eligible as a Mini-boss. Two or three enemies; at most one Mini-boss and one Special. Guards leave after 24. |

The solo Town Marshal checkpoint at 8 and solo Siege Sergeant checkpoint at 16 remain encounter exceptions. Their eligibility ends at their respective checkpoints. No ordinary Chapter 2 or Chapter 3 wave has a fixed enemy composition.

Each definition has minimum/optional maximum eligibility and an age-relative weight curve multiplied by its spawn weight. Locals fade from 1 to 0.1 over 15 progression steps after their unlock; Guards do the same across waves 9–24. Crown soldiers rise from 0.6 to 2 over waves 17–24. These weights are relative within a category, not spawn probabilities. Category weights separately control how often Normal/Special/Mini-boss slots are requested.

A Captain selection constrains the entire remaining encounter to his configured Crown escort pool. Escorts are weighted draws with replacement: Sword/Spear duplicates are legal, and at most one Shield Knight is allowed. Two-slot encounters contain Captain plus one escort; three-slot encounters contain two. The spawner respects configured slot capacity. Escort choice never depends on entrance-animation timing or deaths during spawning.

`WaveController.encounterSeed` accepts a nonzero seed for repeatable encounter generation. Zero chooses and records a fresh seed at run initialization. Composition and weakness shuffling share that private RNG; presentation and board randomness cannot perturb encounter draws. Repeatability assumes the same initial progression and sequence of generation calls.

Manual Unity testing should cover pool variation across seeds, boundary waves 8/9/16/17/21/25, Captain with one/two escorts, command cancellation during wind-up and between spear hits, cooldown restart, Shield Knight seven-move casts, and chain gravity/destruction/replacement/legal-move safety.
