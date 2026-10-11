# Playable Character Brief — Kobold Gunner (Redesign Required)

## High-level requirement

The current Kobold Gunner concept should be **redesigned**.
The user feels the current version is too goofy in the wrong way.

New direction:
- still whimsical,
- still distinctly Dungeon Matcher,
- but more appealing and coherent.

## Design requirements

He should:
- use **one revolver**, not two;
- **not** read like a cowboy;
- not look like a human in a hat;
- clearly read as a **kobold**;
- have personality and charm;
- fit the art style using current golden references.

## Lore/personality direction

Suggested direction the user liked:
- he is **blind or half-blind** / visually impaired;
- he compensates through instinct, ricochet tricks, listening and chaotic confidence;
- this gives him whimsy and explains why his attacks are erratic but effective.

Possible flavor line:
He doesn’t aim where the enemy is; he aims where the bullet *will end up*.

Astra may refine the exact lore, but keep this general character fantasy.

## Ability concept — locked direction

On ability use, he fires a sequence of ricocheting shots.

Prototype behavior:
- randomly selects **6 targets** among legal board-interaction targets;
- shots can hit:
  - gems,
  - barricades,
  - other valid destructibles,
  - and also ricochet to hit enemies randomly.

This should feel lively and chaotic, but not impossible to follow.

Astra should use existing board/damage systems where possible rather than creating isolated logic.

## Passive — locked direction

If the **same enemy** gets hit by his ability multiple times in the same ability sequence,
then each subsequent hit on that same enemy deals more damage.

Example direction:
- 1st hit = base
- 2nd consecutive hit on same enemy during that ability = bonus
- 3rd = bigger bonus
- etc.

Astra should implement this data-driven and document the formula.
Avoid absurd runaway values in the first pass.

## Presentation requirements

### Art
Use PixelLab for the redesign if needed.
Must match the game’s style.
Use actual golden references where supported.

### VFX
- bullet impact VFX on hit gems/objects;
- readable ricochet feeling;
- impacts should feel snappy, not modern realistic muzzle-flash overload.

### SFX
The user explicitly wants:
- quick, satisfying gunshot SFX;
- revolver spin SFX during/around ability execution.

These should feel punchy and playful, not gritty realistic firearms audio.

## Board readability

Even though the ability is random, the sequence must remain readable.
Astra should present it as a short authored burst with visible shot order and impacts,
not as six impossible-to-follow simultaneous explosions.

## Animation direction

He should feel clever, scrappy, reactive and proud of his trick shots.
Avoid cowboy swagger.
Possible posture ideas:
- one-eye squint / listening pose;
- cocked head as if tracking sounds;
- compact tail/body balance;
- a flourish on the revolver that feels tricky rather than gunslinger-western.

## Validation focus

Test:
- random target legality;
- special gem / blocker interactions;
- enemy ricochet hit legality;
- passive multi-hit bonus on same enemy;
- no infinite ricochet loops;
- readable event order;
- SFX/VFX sync;
- save/resume correctness if needed.
