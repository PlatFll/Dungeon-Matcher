# Dungeon Matcher Encounter Pacing

Chapters are weighted, overlapping progression eras. The approved September 2026 Balance v1 brief replaces the old exact-wave/solo checkpoints. [BALANCE_V1.md](BALANCE_V1.md) contains the complete editable roster, sixteen authored formations, budgets and timing targets. [PROGRESSION_PACING.md](PROGRESSION_PACING.md) remains authoritative for the durable progression philosophy.

| Era | Introduction anchors | Pool and capacity |
| --- | --- | --- |
| Locals | 1–5 | Farmer and Basket from 1, Pan from 3, Miner from 4, Barricade Villager from 5. One enemy at 1–2, two at 3–5, two or three afterward. |
| Town calls for help | 6–10 | Spear Guard 6, chain Crossbow Guard 7, Barricade Guard 8. Returning locals decline in relative weight. |
| Knights | 11–18 | Sword Knight 11, Spear Knight 12, Shield Knight 13. Guards remain eligible at lower weight. |
| Royal formations | 19 onward | Royal Swordsman 19, Lancer 20, Arbalist 21, Standard Bearer 22, Court Mage 23. Earlier ranks remain occasional lower-weight members. |

Milestone opportunities occur once per run: Marshal 7–8 with one local escort, Siege Sergeant 12–14 with one Guard, Captain 18–20 with two Knights, Archbishop 24–26 with one Royal, King 29–30 with the required Archbishop. Each window starts with a 35% opportunity and guarantees the remaining encounter at its end. A seen milestone cannot recur as a random leader. The King's required Archbishop escort is the explicit story exception. All formations respect the actual three slots and whole-formation threat budget.

Ordinary encounters have a 45% authored-recipe opportunity. Eligible recipes express members, window, weight, purpose, budget and disruption/support limits; the immediately previous recipe is excluded when possible. Remaining encounters use weighted category/member draws with bounded retries and a cheapest-legal fallback. Named milestones stay out of ordinary random pools. Old ranks fade rather than disappearing at rigid chapter boundaries. No ordinary wave has a guaranteed identity script.

Concurrent disruption is bounded by six pins, four mines and ten mined/barricaded cells. Usual random formations allow two disruptors and one support. The explicit one-Miner/two-Barricade-Guard recipe permits three disruptors under the same board limits and legal-response checks. Two Miners, attackers protecting supports, and escorted leaders provide practice and pressure combinations. Escort selection never depends on entrance-animation timing or deaths during spawning.

`WaveController.encounterSeed` accepts a nonzero seed for repeatable encounter generation. Zero chooses and records a fresh seed at run initialization. Composition and weakness shuffling share that private RNG; presentation and board randomness cannot perturb encounter draws. Repeatability assumes the same initial progression and sequence of generation calls.

Focused validation covers weighted pool variation, all opportunity windows, recipe legality and escorts, bounded random fallback, concurrent Miner/barricade placement, shared Crossbow/Captain falling chains, Shield Knight six-move casts, royal commands and owner-death cleanup. Actual engine measurements and portrait captures are recorded separately in [Validation/BALANCE_V1_VALIDATION.md](Validation/BALANCE_V1_VALIDATION.md). There is no hidden player-level/build correction, enforced death wave or minimum fight duration.
