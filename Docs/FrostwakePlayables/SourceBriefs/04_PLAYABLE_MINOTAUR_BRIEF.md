# Playable Character Brief — Dungeon Attender Minotaur

## Identity

This is an existing designed character concept: the **Dungeon Attender Minotaur**.
Use the attached concept art as the identity inspiration.

Important art revision:
- redesign/refine him to match Dungeon Matcher’s current art style if needed;
- preserve the core identity: horned, heavy, warm-brown/gold, dungeon-denizen attendant flavor;
- make him **bigger and more imposing than the orcs**;
- his silhouette should clearly read as a Minotaur, not just a bulky human with horns.

Do not make him over-detailed or realistic.
He should still fit the game’s whimsical, compact, readable cast.

## Recommended flavor/lore

He is the dungeon’s rough-and-ready maintenance enforcer / attendant.
If something is clogging the dungeon’s workings, he clears it by brute force.
This makes his gameplay ability intuitive: he crashes through problem tiles and objects.

## Ability concept — locked direction

His ability causes him to:
1. jump upward out of frame;
2. a small Minotaur icon/marker appears on a random valid target tile;
3. he lands/breaks the gem or object there;
4. then he runs in straight lines across the board,
   preferring meaningful targets.

## Targeting priorities

Prioritize, in descending importance:
1. barricades / structural blockers / high-value destructibles;
2. special gems if appropriate;
3. affinity gems that heal the player if there is nothing more important;
4. otherwise useful destructibles / default legal targets.

Astra should implement the actual target selector deterministically enough for debugging, while preserving the playful feeling.

## What the Minotaur can damage / break

He is intended to interact broadly with board problems.
He can:
- break vines;
- break freeze/ice states;
- break barricades;
- damage/break other relevant destructible blockers using the game’s normal object-damage rules;
- break ordinary gems he lands on / runs through when appropriate.

## Important rule — does not always one-shot high-tier barricades

He does **not** automatically one-shot a level 3 barricade.
He should deal damage equivalent to a **normal gem clear** against that object, modified by player upgrades/cards.

Example:
- if the player has a powerup/card effect that gives `+2 barricade damage`,
- his collisions should respect that and deal that boosted amount.

In other words, the Minotaur should reuse the authoritative existing destructible-damage rules where possible rather than hardcoding bespoke values disconnected from the rest of the game.

## Repeated collision behavior

If he hits an object that survives (example: a stronger barricade that downgrades but does not fully break),
he should visibly collide, **stumble back slightly**, and continue charging into it again if he still has movement left.

This should be readable and fun, not instant/teleporty.

## Movement count

Prototype default:
- **7 spaces of movement** on activation.

He moves in straight lines only.
Astra should wire the value data-driven so it can be tuned later.

## Presentation

Need:
- character art / idle / attack/ability / hit / death / utility states as needed by the character system;
- board marker/icon for the ability’s landing/run path behavior;
- stomp/crash impact VFX;
- satisfying heavy-footstep / crash SFX.

The tiny moving board icon should still clearly read as the Minotaur.

## Validation focus

Test that he correctly:
- prefers important targets;
- damages blockers using normal authoritative damage rules;
- respects barricade-damage modifiers;
- can interact with vines/freeze/barricades;
- does not break unsupported protected targets;
- behaves correctly if movement runs out mid-obstacle;
- uses stable save/resume state if the ability can exist across action phases.
