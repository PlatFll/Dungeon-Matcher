# Bomb contacts and Dungeon turn / presentation fixes

Status: implemented on `codex/bomb-chain-dungeon-turn-fixes`, awaiting user review. Not merged.
Base: `d2a3857d83647f06248743a3d52d065bc42f942a` (PR #232).

## Behavior and causes

- Drill impacts formerly used non-activating clears. Both machine and portable drills now use the existing bomb expansion and clear pipeline. Bomb footprints preserve their combat/utility effects; ordinary drill cells and environmental cascades remain unrewarded. Crystal requests are deduplicated and finish after the reserved drill batch exits. Fuel cannot recursively feed from the bomb chain.
- Bomb-pair swaps formerly removed both seeds before finding color matches. The existing cascade resolver now captures those matches first and unions the two activation seeds, with disjoint match/collateral reporting.
- Dungeon tiles formerly waited only 480 ms before breaking in the same move. Warnings now persist until the next accepted move, recheck safety then break. Example: warn 4, break 5, restore 7. Version-2 saves retain warning deadlines; versions 0/1 remain supported. Pause, photographs and travel preserve their intended timing semantics.
- Upper Dungeon pillar shafts now continue beyond the physical top edge. Grounded jar/barrel, rubble and bone groups reuse approved native art at the lower corners. On narrow screens they shift outward only where their vertical extent meets a live control.

UI button dimensions, gameplay layout, enemy floor line, gems, characters and approved artwork files are unchanged. No generation credits spent.

## Automated verification

53 distinct checks passed across these Unity Test Framework runs:

| Results file (`.utmp/ThreeZonePresentation/`) | Passed |
| --- | ---: |
| `reference-tests-20261114-040604.xml` | 39 |
| `reference-tests-20261114-041116.xml` | 12 |
| `reference-tests-20261114-041323.xml` | 3 |

Coverage includes each bomb family, chain deduplication, deferred stars, portable first-stone stopping, ordinary drill reward suppression, contact timing, missing optional drill view, bomb-pair color matches, slippery swaps, crumble pause/Continue/safety/photo/travel and production Dungeon layout captures. Earlier failing iterations were diagnosed and corrected; the files above are successful affected runs. These are focused checks, not the complete project test suite.

The tests run through the editor test runner; gameplay cases enter Play Mode and load the actual `Game` scene with a disposable profile. They do not use the user's save.

Required compilation validator: **passed** via `Tools/Validate-Unity.ps1` with Unity 6000.3.19f1. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-c2faff4a-0ccb-4713-8f94-713564612bf3.log`.

## Visual evidence

[Interactive comparison](../../ArtSource/BombDungeonTurnFixes/Preview/Review.html)

Actual Game scene screenshots: 720Ã—1280, 1080Ã—1920, 1080Ã—2400, 1080Ã—2400 with simulated safe area, and 360Ã—640. Each revised capture has Rattlebones, three enemies, gems and live HUD. Native layer scale, screen-top pillar coverage and decoration/control separation are asserted.

The before view reconstructs only the former decoration placement inside the same paused game frame. It is explicitly labelled, not presented as an older-build capture. At 360Ã—640, existing enlarged controls leave little corner space; artwork peeks off-screen to keep them clear. Safe-area insets are 24 pixels left/right, 96 bottom, 120 top.

Full result/log files remain under `.utmp/ThreeZonePresentation`; the committed `ArtSource/BombDungeonTurnFixes/Verification.json` lists test results and screenshot SHA-256 values. Screenshots are unedited Unity captures.

No Android build or real-device test was performed for this follow-up. No merge to main.
