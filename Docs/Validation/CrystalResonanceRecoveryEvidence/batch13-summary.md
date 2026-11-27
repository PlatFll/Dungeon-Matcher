# Actual Unity proxy measurements

| Zone | Policy | Level | Completed | Seeds | Median seconds | p25 / p75 | Min / max |
|---|---|---|---:|---:|---:|---|---|
| drowned-court | 3 | 10 | 1 | 1 | 404.278 | 404.278 / 404.278 | 404.278 / 404.278 |
| drowned-court | 3 | 5 | 1 | 1 | 415.319 | 415.319 / 415.319 | 415.319 / 415.319 |
| drowned-court | all | all | 2 | 2 | 409.798 | 407.038 / 412.559 | 404.278 / 415.319 |
| dungeon | 0 | 1 | 2 | 2 | 456.665 | 447.724 / 465.606 | 438.782 / 474.548 |
| dungeon | 1 | 1 | 1 | 1 | 265.98 | 265.98 / 265.98 | 265.98 / 265.98 |
| dungeon | 1 | 10 | 2 | 1 | 482.338 | 418.537 / 546.139 | 354.736 / 609.94 |
| dungeon | 1 | 5 | 1 | 1 | 261.177 | 261.177 / 261.177 | 261.177 / 261.177 |
| dungeon | 2 | 1 | 1 | 1 | 225.053 | 225.053 / 225.053 | 225.053 / 225.053 |
| dungeon | 2 | 10 | 1 | 1 | 195.434 | 195.434 / 195.434 | 195.434 / 195.434 |
| dungeon | 2 | 5 | 1 | 1 | 300.532 | 300.532 / 300.532 | 300.532 / 300.532 |
| dungeon | 3 | 1 | 1 | 1 | 394.891 | 394.891 / 394.891 | 394.891 / 394.891 |
| dungeon | 3 | 10 | 1 | 1 | 308.999 | 308.999 / 308.999 | 308.999 / 308.999 |
| dungeon | 3 | 5 | 1 | 1 | 381.658 | 381.658 / 381.658 | 381.658 / 381.658 |
| dungeon | 4 | 1 | 2 | 2 | 329.822 | 325.932 / 333.713 | 322.041 / 337.604 |
| dungeon | all | all | 14 | 9 | 329.822 | 274.618 / 391.583 | 195.434 / 609.94 |
| frostwake-peaks | 1 | 10 | 1 | 1 | 391.367 | 391.367 / 391.367 | 391.367 / 391.367 |
| frostwake-peaks | 2 | 10 | 1 | 1 | 238.208 | 238.208 / 238.208 | 238.208 / 238.208 |
| frostwake-peaks | 3 | 10 | 2 | 1 | 476.853 | 468.738 / 484.968 | 460.623 / 493.083 |
| frostwake-peaks | all | all | 4 | 3 | 425.995 | 353.077 / 468.738 | 238.208 / 493.083 |
| ironvein-excavation | 2 | 10 | 1 | 1 | 202.635 | 202.635 / 202.635 | 202.635 / 202.635 |
| ironvein-excavation | 3 | 10 | 1 | 1 | 533.218 | 533.218 / 533.218 | 533.218 / 533.218 |
| ironvein-excavation | all | all | 2 | 2 | 367.926 | 285.281 / 450.572 | 202.635 / 533.218 |
| magical-forest | 1 | 5 | 1 | 1 | 402.021 | 402.021 / 402.021 | 402.021 / 402.021 |
| magical-forest | 2 | 10 | 1 | 1 | 217.997 | 217.997 / 217.997 | 217.997 / 217.997 |
| magical-forest | 3 | 10 | 1 | 1 | 458.029 | 458.029 / 458.029 | 458.029 / 458.029 |
| magical-forest | all | all | 3 | 3 | 402.021 | 310.009 / 430.025 | 217.997 / 458.029 |

Case outcomes: {"defeat": 12, "censored duration limit": 1}

Policies: 0 novice, 1 typical, 2 skilled, 3 defensive, 4 aggressive.

| Zone | Completed | Death visits | Censored | Independent seeds | Policies | Natural revisits |
|---|---:|---:|---:|---:|---|---:|
| drowned-court | 2 | 7 | 0 | 2 | [3] | 0 |
| dungeon | 14 | 1 | 0 | 9 | [0, 1, 2, 3, 4] | 1 |
| frostwake-peaks | 4 | 0 | 1 | 3 | [1, 2, 3] | 1 |
| ironvein-excavation | 2 | 1 | 0 | 2 | [2, 3] | 0 |
| magical-forest | 3 | 3 | 0 | 3 | [1, 2, 3] | 0 |

Actual Unity Game-scene legal-input proxy, not human enjoyment approval.
Completed wall time includes thinking, card reading and travel; incomplete visits are excluded from timing distributions.
First-card interval is measured from first observation of that visit; incoming reveal may overlap it.
Enemy telegraph response counters cover visible marked-gem responses; they do not certify every attack as fair.
Accepted swaps, abilities and consumables are separate; card picks do not consume a combat action.
Warning-without-immediate-marked-match counts include environmental/cell warnings and alternative counters; they are not proof of unavoidable damage.
Different levels/characters must be separated before interpreting skill effects.
Policy-v2 suboptimal/priority/cast flags are visible heuristic diagnostics, not proof of a tactical mistake; specials or killing a caster can answer threats indirectly.
Policy-v2 attention does not model touch error, memory, reading comprehension or human enjoyment. The skilled policy sees all visible legal responses, never hidden refill or future offers.
Fresh-run IDs and capstone seeds are captured in initial-account/native journals. The named seed initializes board and encounter streams; matching named seeds alone does not prove identical whole runs.
