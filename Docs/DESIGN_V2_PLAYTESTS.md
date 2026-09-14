# Production playthroughs — 2026-09-15

These are eight executed Unity Game-scene runs with production assets, seed 3101, actual combat damage and no invulnerability. Bardley uses the shipped 80-energy cost and 50% refund ceiling. Level 5 has the corresponding mastery unlocks. The runner drives legal moves, uses available abilities and chooses cards; its casual and greedy-special policies are repeatable decision samples, not human players. Simulation runs at six times wall-clock speed; the duration column uses game time and includes a three-second allowance per draft.

Raw measurements: [runs.csv](Validation/DesignV2/runs.csv) and [waves.csv](Validation/DesignV2/waves.csv).

| Character / policy | Level | Supplies | Result / waves | Game seconds | HP lost | Gross gold | Gold/min | Moves | Casts | Special swaps | Potion / Bomb used |
|---|---:|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| RattleBones / casual | 1 | none | Defeat / 8 | 303.13 | 159.00 | 75 | 14.85 | 71 | 2 | 4 | 0 / 0 |
| Bardley / casual | 1 | none | Defeat / 17 | 385.19 | 229.00 | 183 | 28.51 | 78 | 11 | 2 | 0 / 0 |
| RattleBones / greedy-special | 1 | none | Defeat / 26 | 488.80 | 293.00 | 330 | 40.51 | 233 | 12 | 2 | 0 / 0 |
| Bardley / greedy-special | 1 | none | Victory / 29 | 466.12 | 258.00 | 517 | 66.55 | 179 | 24 | 9 | 0 / 0 |
| RattleBones / greedy-special | 5 | none | Victory / 30 | 426.13 | 246.00 | 533 | 75.05 | 187 | 11 | 23 | 0 / 0 |
| Bardley / greedy-special | 5 | none | Victory / 30 | 381.73 | 102.00 | 533 | 83.78 | 129 | 24 | 19 | 0 / 0 |
| RattleBones / greedy-special | 5 | equipped | Victory / 29 | 389.59 | 216.00 | 517 | 79.62 | 157 | 15 | 28 | 2 / 3 |
| Bardley / greedy-special | 5 | equipped | Victory / 29 | 365.46 | 99.00 | 517 | 84.88 | 124 | 21 | 14 | 0 / 3 |

Both level-5 characters defeated the King without supplies. The stronger move policy substantially improved both level-1 outcomes, with Bardley clearing the arc. Bardley remains more powerful in these samples; one shared seed and scripted policies do not establish equal character difficulty or final balance. Keep these values as TEST / TUNE and use human play to assess ability frequency and board comprehension. Do not force a beginner loss or pad successful fights to reach a target duration.

## Repeatable income and replacement value

Fresh profiles include new-best and first-King bonuses. Repeatable income below removes 2 gold per newly completed best wave and the one-time 80-gold King bonus. Supply replacement is Potion × 18 + Bomb × 24; it describes restoring the consumed stock, not a second in-game fee. Wave, milestone and recurring King income remain included.

| Policy | Gross | One-time bonuses | Repeatable | Supply replacement | Repeatable less replacement | Repeatable net/min |
|---|---:|---:|---:|---:|---:|---:|
| skeleton-L1-casual-bare | 75 | 16 | 59 | 0 | 59 | 11.68 |
| bardley-L1-casual-bare | 183 | 34 | 149 | 0 | 149 | 23.21 |
| skeleton-L1-greedy-special-bare | 330 | 52 | 278 | 0 | 278 | 34.12 |
| bardley-L1-greedy-special-bare | 517 | 138 | 379 | 0 | 379 | 48.79 |
| skeleton-L5-greedy-special-bare | 533 | 140 | 393 | 0 | 393 | 55.34 |
| bardley-L5-greedy-special-bare | 533 | 140 | 393 | 0 | 393 | 61.77 |
| skeleton-L5-greedy-special-equipped | 517 | 138 | 379 | 108 | 271 | 41.74 |
| bardley-L5-greedy-special-equipped | 517 | 138 | 379 | 72 | 307 | 50.40 |

Upgrade price from current level L is `25 + 15*(L-1) + 5*(L-1)^2`: levels 1→2, 2→3, 3→4 and 4→5 cost 25, 45, 75 and 115 (260 total). Level 5→6 costs 165. A repeat casual RattleBones attempt here earns 59 gold, enough for the first upgrade but not the whole mastery ladder; the level-5 equipped samples net 271–307 repeatable gold after replacing supplies, enough for one level-5 upgrade. This is affordable optional consumption rather than a required purchase to clear the King. These are sample outcomes, not guaranteed income.

## Execution details and limits

The runner wrote all eight completed rows and `ENGINE PLAYTHROUGHS COMPLETED.` in `.utmp/BalancePacing/report.txt`; the run log recorded no gameplay error. An inherited runner cleanup bug left its editor open after successful completion. The owned, completed process was then closed, so the wrapper returned −1 rather than zero. The completed gameplay data above is valid; this is not reported as a clean wrapper exit. `BalancePacingValidation.Finish` now cleans up and exits batch mode explicitly. The final focused suite and required compiler validator cover that code change.

Separate runtime tests, restart evidence, UI captures and final validator details are recorded in [DESIGN_V2_IMPLEMENTATION.md](DESIGN_V2_IMPLEMENTATION.md). No physical Android device was attached; touch comfort, cutouts, thermal/performance behavior and audio hardware were not validated.
