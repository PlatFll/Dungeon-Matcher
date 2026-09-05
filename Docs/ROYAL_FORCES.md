# Dungeon Matcher — Royal Forces

## Scope

This document records the approved Chapter 4 Royal Forces direction that has been finalized so far. It supplements `GAME_DESIGN.md` and `PROGRESSION_PACING.md`.

Chapter 4 remains an overlapping weighted spawn era, not a fixed per-wave script. The Royal normals below are introduced gradually while Knights and some earlier enemies remain eligible.

The Royal faction combat language is:

- Normal Royal soldiers have no signature ability and attack twice during one normal attack sequence.
- Special Royal units use a simpler single normal attack because their identity comes from their special mechanic.
- Royal Forces should increase danger through disciplined attack patterns and later tactical mechanics rather than becoming HP sponges.

Exact numerical values below are first-pass tuning targets and remain playtestable balance data.

## Royal Swordsman

- Rank: Normal.
- Stable ID: `royal_swordsman`.
- First-pass eligibility anchor: wave 20. This is a weighted-era introduction, not a guaranteed exact-wave spawn.
- Role: durable, balanced Royal melee baseline.
- Normal attack: two disciplined sword hits, 6 damage then 7 damage at the introduction baseline.
- Follow-up readability delay: approximately 0.18 seconds after the first attack has fully returned to rest.
- Attack cadence: approximately 9 seconds at introduction; the next cooldown begins after the second hit sequence finishes.
- Introduction HP target: approximately 230.
- Relative spawn weight begins modestly and rises through the late Royal window so Royal units gradually become dominant rather than replacing Knights instantly.
- Uses the existing Knight animation controller as temporary presentation until Royal Swordsman art/animation is wired.

## Royal Lancer

- Rank: Normal.
- Stable ID: `royal_lancer`.
- First-pass eligibility anchor: wave 21. This is a weighted-era introduction, not a guaranteed exact-wave spawn.
- Role: deliberate two-hit burst enemy.
- Normal attack: light probing thrust followed by a much heavier second thrust, 4 damage then 10 damage at the introduction baseline.
- Follow-up readability delay: approximately 0.45 seconds after the first attack has fully returned to rest, making the dangerous second thrust easy to read.
- Attack cadence: approximately 10 seconds at introduction; cooldown begins after the second return.
- Introduction HP target: approximately 220.
- Uses Spear Knight art as temporary fallback until Royal Lancer art is wired.

## Royal Arbalist

- Rank: Normal.
- Stable ID: `royal_arbalist`.
- First-pass eligibility anchor: wave 22. This is a weighted-era introduction, not a guaranteed exact-wave spawn.
- Role: lower-durability, fast ranged burst.
- Normal attack: two rapid shots, 5 damage then 5 damage at the introduction baseline.
- Follow-up readability delay: approximately 0.12 seconds after the first attack presentation returns.
- Attack cadence: approximately 8 seconds at introduction; cooldown begins after the second shot sequence finishes.
- Introduction HP target: approximately 175.
- Uses Crossbow Guard art as temporary fallback until Royal Arbalist art is wired.

## Weighted Royal introduction

The three Normal Royals use `EnemyDefinition.minimumWave`, `spawnWeight`, and `progressionWeight` rather than exact-wave encounter overrides.

The intended first-pass shape is:

- around wave 20, Royal Swordsman acts as an early Royal teaser while Knights are still common;
- around wave 21, Royal Lancer joins the pool;
- around wave 22, Royal Arbalist completes the first Royal Normal roster;
- by the late Royal window around wave 24, the combined Royal Normal weight should be strong enough for Royals to feel like the dominant new faction while Knights and declining Guards still appear.

All three remain eligible after their introduction instead of disappearing at the King. Their later post-King weight can be retuned when the Guild-era pool is implemented.

All three are `CrownSoldier` enemies so shared Crown coordination systems can recognize them where appropriate.

## Presentation direction

Final Royal Guard art should read as the same kingdom at a higher status tier:

- fully armored;
- bright polished silver/white steel as the dominant material;
- crimson red as the main kingdom accent;
- gold trim;
- purple used sparingly as a secondary royal-status accent.

Final sprites are not part of this implementation and temporary existing-enemy art is intentionally used until the approved Royal sprites are imported.
