# Royal Special Enemies

This document records the implemented Royal Standard Bearer and Court Mage behavior. Numerical combat values and spawn weights below are first-pass serialized tuning and remain adjustable; the mechanic rules are the durable part of this implementation.

## Royal Standard Bearer

- Category: Special.
- Uses a single-hit normal auto-attack; Royal Special enemies do not use the two-hit normal attack pattern of the current Royal Normal roster.
- Its special places one Royal Standard into a legal ordinary gem cell on the top row.
- The standard is a board object, not a gem. It cannot be swapped, matched, or cleared by gem/special-gem destruction.
- The standard participates in board gravity rather than moving on a turn timer.
- During one board clear/resolution, every real gravity opening created below a standard contributes to that same gravity fall. The standard consumes the full batch at once instead of being capped to one row per clear. Example: if a column bomb clears the playable cells beneath a top-row standard, the standard falls all the way to the bottom in that resolution and is removed.
- Normal movable gems are not blocked vertically by the standard. Gems above it can compact through the standard's height and fill valid gravity destinations below it; the standard's own cell remains unavailable as a gem destination while the standard is active.
- Multiple standards never pass through one another.
- The standard is removed only after it reaches the bottom row.
- While at least one standard is active, living enemies whose `EnemyDefinition.CrownSoldier` flag is enabled receive a non-stacking 1.20x auto-attack-speed multiplier.
- Multiple standards keep one shared non-stacking aura alive; removing one standard cannot erase another active standard's aura.
- A bearer may own only one active standard. Reaching the cap consumes that special cadence instead of banking an instant replacement cast.
- A planted standard persists after its bearer dies. Ownership is orphaned, while the standard and its aura remain until the standard reaches the bottom.
- Placement uses the shared board-mutation queue and legal-move check. It cannot begin while the owner is staggered.
- `BoardController.royalBannerBoardSprite` is optional presentation data. Missing artwork does not change gameplay.

First-pass serialized tuning:
- minimum eligible wave: 23;
- base HP target before the normal difficulty pipeline normalization: 200;
- single-hit base damage: 6;
- base attack interval: 10 seconds;
- special cadence: 5 valid completed player moves, locked against global special-turn reduction;
- relative spawn weight: 0.8.

## Court Mage

- Category: Special.
- Uses a single-hit normal auto-attack.
- Its special freezes one ordinary, non-special gem at a time, up to three frozen gems owned by that Mage.
- A frozen gem cannot be manually swapped and does not fall under gravity.
- Other movable gems in the same column can pass through the frozen gem's height and fill valid spaces below it. This reuses the authoritative fixed-pin gravity behavior; there is no second gravity simulation.
- Frozen gems remain real gems in the match grid. A match or special effect that actually destroys the frozen gem removes the freeze with it.
- Merely matching next to a frozen gem does not thaw it.
- When the Mage is defeated or destroyed, its surviving frozen gems are released and the board resolves any resulting gravity/cascades through the normal environmental-board pipeline.
- Freeze target selection reuses the board's legal-move safety check so a new freeze is not allowed to remove the final legal move.
- Freeze requests use the shared board-mutation queue and cannot begin while the Mage is staggered.
- `BoardController.frozenGemOverlaySprite` is the assignable frozen-gem artwork slot. Missing artwork logs a warning but the gameplay freeze still functions.

First-pass serialized tuning:
- minimum eligible wave: 24;
- base HP target before the normal difficulty pipeline normalization: 180;
- single-hit base damage: 6;
- base attack interval: 10 seconds;
- special cadence: 5 valid completed player moves, locked against global special-turn reduction;
- relative spawn weight: 0.75.

## Architecture notes

Both mechanics preserve the existing ownership model:

- `EnemySpecialActionAvailability` gates special startup around stagger and waits for board idle without claiming board ownership early.
- `BoardController` remains the sole authority for structural board mutation, gravity, legal-move checks, refill, cascades and emergency reshuffle.
- Court Mage freeze is represented as a distinct frozen tag layered on the existing pin ownership system. Frozen gems use fixed-pin gravity semantics but have their own overlay and do not use Crossbow Guard adjacency-break behavior.
- Royal standards are non-gem occupants tracked by `BoardController.RoyalBanners`. Physical gem-destruction notifications are batched for the current clear before the normal collapse resumes, so a standard consumes all gravity openings created beneath it in that resolution. Presentation never owns the gameplay lifetime.
- `RoyalBannerAuraRuntime` is a board-level coordinator rather than one independent runtime per standard, so duplicate Standard Bearers cannot incorrectly clear each other's aura.
- Both new enemy definitions are registered in `EnemyDatabase_Main` and use weighted eligibility rather than exact-wave scripted encounters.
