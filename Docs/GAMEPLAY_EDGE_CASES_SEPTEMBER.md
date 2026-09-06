# September gameplay edge-case regression

## Bug matrix

| Bug | Root cause and fix | Validation actually performed |
| --- | --- | --- |
| Enemy stays white | Poison captured a temporary flash value and restored it later, racing stagger. `EnemyCombatFeedback` now composes timed hit, stagger and lifecycle flash state; poison and spawn/death presentation request state from that owner. | Play Mode: eight overlapping direct/poison presentation hits, stagger expiry, disable, lethal hit and actor destruction. |
| Shield damage has no number | Combat text subscribed only to HP damage. It now consumes the actor's actual `ShieldDamaged` amount in a separate shield-number lane. Damage/mitigation calculations remain unchanged. | Play Mode: absorption, exact break, overflow, zero damage and regrant; actual shield/HP event totals asserted. |
| Empty shield artwork | Zero-shield handling retained a full white overlay through a break coroutine. Zero now deactivates the complete overlay immediately, destroys it and restores HP presentation. | Play Mode: initialization, break, regrant and orphan-overlay checks passed. The reported initial thin line was not reproduced in the inspected Game view; no serialized shield frame exists in `Game.unity`. |
| Crystal + mastery bomb | Conversion dispatch accepted only Row/Column. It now accepts every current chain-reactive bomb and preserves the exact mastery type across eligible color targets. | Real Play Mode crystal swaps for Row, Column, Poison, Healing and Shield: every target observed with the expected type, every target removed, board playable afterward. Crit/Damage bombs do not exist in this version. |
| Missing explosion energy | The active-ability gate blocked all energy; Cracked clears were ineligible Ability contexts. An explicit `GrantsSpecialEnergy` entitlement permits player explosions while preserving Ability attribution. | Play Mode: exact per-clear energy during active ability, one reward per physical gem, ordinary Ability exclusion, zero-physical-clear exclusion, environmental non-rewarding removal, actual Bardley activation/refund timing. |
| Mastery effects fire before detonation | Expansion helpers applied effects while computing future targets. They now compute targets only. Genuine activation paths opt into effect commit at the shared `ClearMatches` shatter point. | Play Mode: repeated planning causes zero effects; two chained bombs of each mastery type produce exactly two commits, with all bomb renderers already hidden. Bardley preparation grants no early shield. |
| Bardley wastes crystal | Cracked crystals already expanded the triggering color, but had no crystal activation presentation. Conversion could also overwrite existing bombs, and the all-special targeting fallback overwrote directly selected crystals. These now present activation and preserve special behavior. | Play Mode: cracked-color expansion, all five bomb-to-remote-crystal paths, direct-crystal fallback, actual Bardley ability, missing target and duplicate cracked seeds. |
| Captain chains bottom row | Top-up selected the first legal ordinary gem in row order. It now samples a legal candidate with Unity RNG and recalculates safety after each pin. | Play Mode: 60 casts / 180 pins, exactly three ordinary pins and a legal move after every cast. Row counts: 22, 26, 19, 28, 16, 19, 23, 27. |
| Consecutive Captain encounters | Composition had no preceding-encounter leader exclusions. Successfully spawned Mini-boss identities now exclude the next encounter; seen major Bosses stay excluded. Weighted selection and fallback share the guard. | 600 production weighted compositions in Play Mode: Captain excluded after himself, fallback nonempty, eligible after another encounter (45 selections in 300 eligible draws). |

## Additional related fixes

- **Bomb shell/icon linger:** a mastery bomb retained child sprite artwork after the base renderer vanished. All gem sprite renderers now hide at shatter. The effect-commit tests assert this ordering for Poison, Healing and Shield.
- **Special overwrite during cracked conversion:** put an existing bomb on the color targeted by a cracked-triggered crystal, or supply a special as the all-special targeting fallback. Replacing it with Cracked erased its original behavior. Existing bombs/crystals are now retained and activated. This resolves the conflict between broad conversion wording and the approved requirement that caught specials use their established behavior.
- **Lifecycle flash ownership:** spawn/death also wrote the hit-flash material. It now supplies a separate lifecycle channel to the same owner, preserving the intentional death flash and clearing it on disable.
- **Required escort bypassed repeat protection:** selecting the King could reintroduce the preceding encounter's Bishop through the required escort. The full pairing is now deferred for that encounter; the milestone remains eligible afterward with its required escort intact.

## Repeating the checks

Open `Assets/_Game/Scenes/Game.unity` outside Play Mode. Use **Dungeon Matcher > Validation > Gameplay Edge Cases** and **Gameplay Supplementary Cases**. Both use real production coroutines and runtime-only fixtures, then exit Play Mode. Results go to `Logs/GameplayEdgeCaseValidation.log`; each run replaces that log. The main run additionally exercises six real player swaps and their cascades/refill/reshuffle.

The existing Royal Play Mode suite resets encounter history when constructing each independent run fixture; this does not relax runtime repeat protection.

## Validation results

- `GameplayEdgeCaseValidation`: passed the main and supplementary Play Mode suites described above.
- `RoyalMilestonePlayValidation`: passed real swaps, simultaneous warnings, Standard Bearer/Court Mage interactions, banner/freeze bombardment, blessed Lancer multi-hit commands, death cancellation and reinforcement-slot scenarios.
- `RoyalMilestoneValidation.Run` and `SiegeSergeantValidation.Run`: passed the existing editor validation suites.
- `GameplayEdgeCaseValidation.RunEncounterHistory`: passed required-escort exclusion, nonempty fallback, deferred milestone eligibility and seen-Boss exclusion after the final encounter fix.
- `Tools/Validate-Unity.ps1`: passed with Unity 6000.3.19f1 after closing the editor and using the required local permissions. The initial sandbox launch failed on Unity cache/OS access before compilation; it was not counted as successful validation.
- `git diff --check` for the changed scripts and docs: passed. The pre-existing user asset edits contain trailing whitespace and remain excluded from this PR.

## Scope and remaining visual verification

No enemy balance, shield mitigation, centralized damage calculations, affinity healing, board locking, obstacle semantics, attack sequencing or weighted-era tuning was redesigned. Existing King/Archbishop asset edits and new enemy artwork in the working tree are excluded from this change.

Ali should still check perceived timing and combat-number readability on the target mobile resolution, especially the originally reported one-pixel line. Automated state assertions and the inspected desktop Game view cannot establish pixel-perfect appearance on every device. Royal Decree's exact rapid-hit presentation and enemy death partway through a Bardley chain were not separately reproduced by the new suite; generic overlap/death and accepted-board completion paths were exercised.
