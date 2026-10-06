# Shared zone design rules

Approved revision: 2026-10-06. These are durable design rules, not an instruction
to redesign every existing enemy.

## Reuse systems; vary the decision

Reuse underlying systems, but do not copy a signature ability into another zone
unchanged. This is a soft rule: familiar mechanics should return with a meaningful
variation in the player's decision. Reuse the implementation grammar; vary the
gameplay decision.

Useful differences include duration, expiry, retaliation, cost, clearing method,
movement, ownership, targets, clearing rewards, environmental interactions,
linked objects, spreading, strengthening/weakening and response timing.
Dungeon chains restrict movement; Court thorn snares also expire after three
accepted moves and charge AIR for deliberate wet matching, while indirect clears
remain safe. HP, shields, Stagger, ordinary attacks/damage and shared clocks remain
universal mechanics.

Environmental effects should be predictable problems with exploitable interactions.
An enemy may accidentally help the player when believable world logic explains it.
Debuffs may change available decisions but must never secretly falsify input.
Not every enemy should interact with its zone's environment. Support roles should
vary among healing, damage, speed, regeneration, protection, cleansing and leeching.
Named statuses retain stable meanings across zones; see [STATUS_EFFECTS.md](STATUS_EFFECTS.md).

Board target language is **WHO = slot sigil; WHAT = cast announcement and enemy
inspection; WHEN = existing countdown/telegraph**. No effect-type pictogram or
enemy portrait is required on every marked target.

## Zone design checklist

1. What was this place originally?
2. Who lives here now?
3. Why do its inhabitants look this way?
4. Why do they fight this way?
5. Why does the environmental mechanic exist in-world?
6. Which enemies interact with it?
7. Which enemies deliberately do not?
8. Which parts of the shared mechanic grammar are reused?
9. What changes the decision compared with previous zones?
10. How does the boss express the culture beyond having more HP?

Leave explicit gaps where lore is not approved. Do not fill them with invented
canon. Future concepts belong in [FUTURE_ZONE_LOCKS.md](FUTURE_ZONE_LOCKS.md).

## Current overlap audit

The existing Crossbow Guard, Royal Arbalist and Knight Captain use the same
falling-chain behavior with different counts, cadence and ownership caps. They
are within the dungeon, and this revision does not redesign them. Town Marshal
and Orc Drummer both grant non-stacking temporary haste; their eligibility,
summon context and recast behavior differ, but this is the closest cross-zone
support overlap to revisit in a separately authorized kit pass. Conch Marshal
already changes this decision to a damage buff, and Netweaver uses thorn snares.
Ordinary shielding/healing is shared grammar; no retroactive kit changes are
authorized merely by this audit.
