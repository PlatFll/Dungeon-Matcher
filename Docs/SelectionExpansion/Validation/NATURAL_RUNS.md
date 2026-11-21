# Natural Dungeon-first Game runs

The production Game scene ran ten attention-v2 cases at normal speed on f3ed12bd8806b7874749d4f05f3d523f14398266. The five styles used four characters and four permanent levels. Each pair used two independent, predeclared seeds. The driver sent legal player swaps, ability activations, supply uses and card choices. The [configuration](natural-case-config.json) and [source manifest](natural-source.json) identify the batch.

## Outcomes

On 21 completed visits, elapsed time ranged from 3:27 to 11:18, with a median of 5:19. Eight runs reached their assigned observation limit alive. Two ended in defeat. Eight arrival-only rows record the next destination without any decisions in that visit. All ten began in Dungeon.

| Style and seed | Character and level | Played route | Completed visits | Deepest observed | Elapsed | End |
|---|---|---|---:|---:|---:|---|
| Novice 41339 | Rattlebones L1 | Dungeon → Frostwake | 2 | 37 | 17:28 | Observation limit |
| Novice 904681 | Rattlebones L1 | Dungeon → Frostwake | 1 | 33 | 14:43 | Defeat |
| Typical 58439 | Kobold L4 | Dungeon → Frostwake | 2 | 35 | 10:49 | Observation limit |
| Typical 960931 | Kobold L4 | Dungeon → Ironvein | 2 | 35 | 10:17 | Observation limit |
| Skilled 14281 | Dungeon Attender L8 | Dungeon → Court → Ironvein → Dungeon | 4 | 69 | 26:46 | Observation limit |
| Skilled 456217 | Dungeon Attender L8 | Dungeon → Forest | 2 | 37 | 9:58 | Observation limit |
| Defensive 121909 | Bardley L3 | Dungeon → Ironvein | 2 | 36 | 15:07 | Observation limit |
| Defensive 972283 | Bardley L3 | Dungeon → Frostwake | 2 | 34 | 13:34 | Observation limit |
| Aggressive 209311 | Rattlebones L8 | Dungeon → Frostwake → Forest | 2 | 47 | 11:38 | Defeat |
| Aggressive 717229 | Rattlebones L8 | Dungeon → Frostwake | 2 | 38 | 8:26 | Observation limit |

The first Novice sample completed two visits in 17:27. The other died at Frostwake depth 33 to an Iceborn Axeman. The Aggressive deep attempt died at Forest depth 47 to an Orc Trailguard after two teleports. Both Typical samples reached two teleports in 10:17 and 10:49. Characters and levels differ between styles, so the elapsed differences cannot isolate player skill.

The durable journals matched 2,442 accepted actions, including 2,139 swaps, 199 player ability activations and 68 card picks. The audit found no duplicate banks, illegal travel, repeated unique leaders, skipped capstones or receipt mismatches. The sample included 401 formations. Across 380 ordinary draws, none repeated the multiset of either of the previous two actual formations in the same visit. One additional lookback that ignores capstones finds the same shield lesson on opposite sides of an intervening elite and patrol. The production rule counts the intervening elite.

## Mechanics in the recorded encounters

- **Guild.** A Skilled return to Dungeon at depths 52–69 produced Guild Outrider at 58 and 64, then Guild Trail Party at 69. The Ranger and Swordsman appeared naturally. The run first met the Swordsman on its last fight. The Bulwark and Chirurgeon have deterministic selection and real-Game diagnostic evidence, with natural special timing still open.
- **Ironvein.** Packbeetle appeared in five natural formations. On all five recorded disappearance boundaries, every fixed drill advanced one charge, including wraparound on firing. A surviving Rivet Gunner and Pickaxe Delver acquired the 1.3× ore attack on two same-encounter boundaries. This is a death effect. It never needed an active cast. [Native boundary details](ore-boundaries.json)
- **Court.** One played Court visit ended in a successful teleport after 5:15. The Skilled player answered 13 of 13 low-air opportunities. Two Moray formations were separated by three other encounters, and one Moray special fired. Three further Court arrivals had no subsequent inputs. Beginner oxygen pressure needs human and lower-level coverage.
- **Forest.** Four Mender formations produced one recorded special. Forest first-card offers arrived after 40 and 59 seconds. The earlier base branch had recorded a 116-second Novice first card with repeated healing. The new Forest visits were Skilled and Aggressive, so the earlier Novice case remains an open comparison. [Base-branch observations](../../Validation/CRYSTAL_RESONANCE_REFINEMENT.md)
- **Late Dungeon.** The depth-52 return took 11:18, required 182 swaps across 18 victories, and offered its first card after 1:53. The Royal Chain lesson took seven swaps. A Court Ice lesson and a Knight Patrol took 20 and 21. The older branch recorded 15–20 swaps for some royal chain formations. One deep return does not establish a general pacing improvement.
- **New art in natural play.** Eight of the ten new identities appeared in legal-input runs. All ten appeared in the separately labelled Game-scene gallery. Review the shield-call, facet shatter and healing cue at phone size with a person. The automated profiles cannot judge whether the signals are easy to read or enjoyable.

The observed range supports flexible visit lengths. No enemy HP, reward delay, timer or player strength compensation was added during validation. Further balance changes should start from more beginner Court and deep Dungeon-return evidence.

[Per-visit timing, supplies and HP](natural-visits.csv) · [Every formation, its duration and player damage](natural-formations.csv) · [Native action and mechanic summary](natural-summary.json)

Original case files, account snapshots, PNGs, actions, events and compressed native journals are in the worktree at .utmp/CrystalResonance/Playtest/20261119-082420. The offline audit and full per-case analysis are in .utmp/SelectionExpansion/Audit-20261119-082420. All ten result files and the completion marker were checked.
