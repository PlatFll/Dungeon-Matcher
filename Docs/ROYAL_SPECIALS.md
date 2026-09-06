# Royal Special Enemies

This document records the Royal Special enemies and the Royal Archbishop / King milestone mechanics. Numerical combat values, milestone windows, and spawn weights below are first-pass serialized tuning and remain adjustable; the mechanic rules are the durable part of this implementation.

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

## Royal Archbishop — Mini-boss

The Archbishop uses one normal attack and alternates Restoration then Benediction on one accepted-valid-completed-move cadence. Invalid swaps, cascades, refill and settling do not count. A failed cast holds readiness and retries after another valid move; Benediction is the fallback when runes cannot be placed. Specials and due warnings wait for board idle, stagger release and enemy action availability without claiming the board while waiting.

**Sacred Triage:** choose only living damaged enemies, including the caster. Score missing-HP fraction multiplied by rank importance. Ties retain authoritative roster order. When another ally is meaningfully wounded, reduce the caster's own score. A heavily injured Special can outrank a nearly full Boss. Recalculate independently before every heal; healing uses `EnemyActor.RestoreHealth` and never grants shield.

**Runes of Restoration:** mark up to three ordinary unpinned gems, avoiding other Restoration/Judgment marks. Marking does not restrict swaps or matches, so it cannot remove a legal move. Each rune follows its gem identity through swaps/falls. Destruction or conversion into a special permanently breaks that rune. At expiry, each surviving ordinary gem is environmentally removed, then one heal goes to the newly selected triage target. After all surviving runes, settle once using the existing board pipeline. Cleared/replaced targets never heal. Caster defeat/disable cancels surviving runes and pending work.

**Benediction:** bless up to two other living allies in roster order, excluding targets already holding this caster's blessing. Blessing affects their next accepted normal attack sequence; both hits of a two-hit Royal receive it. It composes multiplicatively with command and persistent normal-damage modifiers, is consumed once at sequence acceptance, and never affects special damage or attack speed. Caster cleanup removes its unspent blessings. A blessing granted during an existing attack applies to the following sequence.

First-pass tuning (serialized on `Enemy_RoyalArchbishop`):

- Eligibility starts at 21, with elevated Mini-boss weight and a milestone opportunity window of 21–23. Existing older eligibility is unchanged.
- Approximately 350 HP at introduction, one approximately 6-damage attack per 10 seconds through the existing difficulty pipeline.
- Shared special cadence: 4 valid completed moves. Three runes last 3 complete subsequent moves.
- Each surviving rune restores 3.3% of the selected target's maximum HP, rounded to the nearest whole HP (minimum 1). Three King-targeted pulses restore approximately 9.9% before missing-HP clamping.
- Rank weights: Normal 1.00, Special 1.15, Mini-boss 1.35, Boss 1.60. An ally missing at least 10% HP makes self-priority use a 0.5 multiplier.
- Benediction: 1.40x normal-sequence damage, maximum two allies.

## The King — Boss

The opening composition is exactly **King + Archbishop**. Both are always directly damageable; there is no immunity, damage interception or required target order. Temporary character art uses the existing Captain and Court Mage sprites.

**The Crown's Last Stand:** centralized surviving-health-damage notifications detect downward crossings strictly below 50% and 25%. Both trigger only once, including damage-over-time. Healing cannot rearm them. A surviving 60% → 20% hit queues 50% then 25%; a lethal crossing queues nothing. Pending batches are cancelled by death/disable and execute at safe action points.

Each batch fills currently available slots through `IEnemySummonService`. Its explicit pool is Royal Swordsman, Royal Lancer, Royal Arbalist, Royal Standard Bearer and Court Mage. Never summon an Archbishop. At most one Special is added per batch, and a Special definition already alive is excluded. Normal duplicates are allowed. Slots still occupied by death presentation follow the existing summon service's availability rules. Reinforcements are independent wave members and survive the King's death.

The first crossing also enters Enrage immediately: a persistent normal-damage multiplier and normal-timer speed multiplier compose with the banner aura and temporary blessings. The special cadence shortens without resetting accumulated moves. The quarter-health crossing adds no second permanent stat buff.

One deterministic special cycle is **Royal Judgment → United Royal Assault → Royal Bombardment → repeat**. Successful casts advance the cycle and reset the shared counter; failed casts retain readiness. Due warnings resolve before new casts. Their lifetime is independent of the shared cast counter.

**Royal Judgment:** independently mark three ordinary gems for three subsequent valid moves. White pulsing `!` warnings follow gem identities, avoid existing rune/mark targets, and break individually on destruction/special conversion. At expiry: destroy one surviving gem environmentally, apply one direct shield-aware King strike, then repeat. Zero/one/two/three survivors produce exactly zero/one/two/three separate damage instances. Settle the board once after the sequence.

**United Royal Assault:** snapshot the King first, then eligible allies in authoritative roster order. The explicit `RoyalAssaultParticipant` flag is true for King, Royal Swordsman, Royal Lancer and Royal Arbalist. It is false for Standard Bearer, Court Mage, Archbishop and legacy Guards/Knights/Captain. Broad `CrownSoldier` membership alone is insufficient. Reserve each eligible available participant through `EnemyAutoAttack`, telegraph the command, then perform its existing full normal sequence. Busy/staggered participants cannot be reserved. A reserved participant newly staggered during windup waits before beginning; dead participants are skipped. Player board resolution finishes before the next commanded sequence starts. Commands consume the next normal attack and restart its normal cooldown after completion, with no immediately following stored ready attack. Owner death/disable cancels outstanding commands and stale impact callbacks. Unspent reservations retain their old cooldown.

**Royal Bombardment:** warn one row and one column for two subsequent valid moves. Clearing gems cannot cancel the lanes. On expiry a white slash travels along the row, then the column, as ordinary gems are environmentally removed. The intersection is processed once. Player-created specials, mined holes, barricades and the Royal Standard survive. Frozen ordinary gems can be removed and use the existing physical-destruction ownership cleanup. Apply one moderate direct hit after both lanes, then settle once. Presentation uses translucent white warnings; no VFX object owns the countdown or grid mutation.

Environmental removal itself reports no player clear rewards, combat damage, healing or energy. Genuine resulting cascades retain the established environmental-settlement cascade semantics, as with Siege Sergeant.

First-pass tuning (serialized on `Enemy_King`):

- Variable milestone window 24–26, centered around the approximate wave-25 narrative anchor; no exact-wave King override.
- Approximately 750 HP and 10 normal damage per roughly 10 seconds near wave 25, normalized through standard difficulty/category scaling rather than multiplying a full Boss-sized HP target again.
- Special cadence 4 moves, shortened to 3 in Enrage.
- Enrage: normal damage 1.20x, cooldown progress speed 1.25x. Banner speed remains a separate multiplier.
- Judgment: 3 targets, 3-move countdown, base 12 damage per survivor scaled by the existing unrounded difficulty damage multiplier.
- Assault: 1.10x for the commanded normal sequence; 0.6-second windup and 0.12-second inter-participant spacing.
- Bombardment: 2-move warning, base 6 damage once after the lanes; set base damage to zero to disable that hit.

These values do not enforce encounter duration or minimum survival time. Strong builds can kill either encounter quickly.

## Inspector art assignment

On the `BoardController` in `Game.unity`, under **Royal Telegraph Art (optional)**:

- `archbishopRestorationRuneOverlay`: pulsing holy circle; procedural golden rune fallback.
- `kingRoyalJudgmentExclamationOverlay`: pulsing white exclamation; procedural white `!` fallback.
- `royalBombardmentRowWarning` / `royalBombardmentColumnWarning`: low-opacity lane sprites; translucent white fallback.
- `royalBombardmentRowSlash` / `royalBombardmentColumnSlash`: travelling white slash sprites; white sweep fallback.
- `royalBombardmentWarningAlpha`: initial 0.12; intersecting warnings remain translucent.

On `Enemy_RoyalArchbishop`, `benedictionHaloSprite` supplies the gold blessing icon; an unobtrusive gold bar is the current unassigned-art fallback. Board sprites use the existing Gems sorting layer and board mask. The King's presentation observer displays a phase indicator, command callout and strike callout without owning gameplay timing.

## Verification

`RoyalMilestoneValidation.Run` (menu **Dungeon Matcher → Validation → Royal Milestones**) audits data links, eligibility, variable milestone windows across 100 seeds, triage, threshold ordering/rearm/lethal handling, modifier composition and gem identity/exclusions. Compile validation and this editor suite are distinct from Play Mode.

**Dungeon Matcher → Validation → Royal Play Mode** runs `RoyalMilestonePlayValidation` against the loaded Game scene, then exits Play Mode. It uses real production swaps, board mutation coroutines, enemy runtimes and the shared summon service. Test fixtures temporarily boost HP, stop unrelated automatic attacks, prime special counters, and accelerate the isolated lane-footprint expiry; these are runtime-only arrangements, not serialized balance changes or a replacement gameplay pipeline. The suite exercises King + Archbishop + Bearer, simultaneous rune/Judgment warnings over three real completed moves, King + Archbishop + Mage, frozen-gem and Standard bombardment, both blessed Lancer hits in Assault, timer consumption, King-death command cancellation, and reinforcement batches with 0/1/2 free slots followed by a second refill. It does not measure encounter duration or replace final-art/mobile readability review.

Play Mode acceptance cases: King + Archbishop + Standard Bearer; King + Archbishop + Court Mage; blessed Lancer in Assault; simultaneous Judgment/Restoration; Bombardment across frozen gems and a Standard; 50% crossings with 0/1/2 free slots; 25% refill after earlier reinforcements die; owner death during warnings/command; both-hit blessing consumption and normal cooldown restart. Also inspect pulse/slash readability, player shield handling, stagger waiting and encounter duration on a mobile-sized view.
