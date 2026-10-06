# Drowned Court formations — 2026-10-06 revision

The older sophisticated ruins now house two cooperating/rival sea-folk cultures.
Reef Clans are the majority: strength earns authority; they scavenge wrecks and
ruins, using rough coral, rope, hooks, trophies and broken ship metal. Nacre Court
is the minority: disciplined fish-folk preserve the old civilization's traditions,
shell/pearl craft, refined weapons and magic. Queen Nacre remains their boss.

## Production grouping

Existing `EnemyDefinition.faction` is the taxonomy owner; there is no new engine.

- **Reef Clans:** Spearman, Hammerhead, Needlefin, Porter, Thief, Moray, Netweaver,
  Puffer, Breakwater Captain and Skittercrab.
- **Nacre Court:** Pearl Cantor, Conch Marshal, Lantern Warden and Queen Nacre.

The 32 recipe templates now comprise **24 Reef**, **6 Court**, **2 mixed**.
These are template counts, not measured encounter probabilities. Eligibility
bands, tide conditions, recent-formation discouragement and weights also matter.
The two mixed recipes retain weight 1: R06's early Cantor lesson with a Spearman,
and R27's later Puffer/Conch cooperation. Their shared enemy is the dungeon monster.
These are deliberate exceptions, not the default composition.

## Changes and teaching intent

| Recipes | Current members / purpose |
| --- | --- |
| R14 | Porter, Hammerhead, Spearman; coherent salvage convoy |
| R15 | Conch, Cantor; first damage-rally lesson, keeps an eligible other ally |
| R16 | Hammerhead, Porter; physical pressure without another mixed support group |
| R21 / R22 | Warden with Cantor / Conch; prior lessons support the new pressure mechanic |
| R24 / R25 | Reef sentries / Netweaver with Porter and Spearman |
| R30 (weight 3) | Queen, Cantor, Conch; Court apex, no additional miniboss escort |
| R31 / R32 | Queen with Conch / Cantor; lighter two-actor return alternatives |

All recipe bands, tide requirements and weights are retained. No enemy HP,
damage or kit changed. The two-culture roster has only two non-milestone Court
supports, so Court group variety is intentionally limited. Queen summons remain
the existing independent Reef Skittercrab; it does not change her starting escorts.
Development fixtures remain isolated mechanic tests and are not production recipes.

`ArtSource/DrownedCourt/Production/EncounterRecipes.csv` is the editable current
source; `PrepareEncounters.py` deterministically exports `Selected/encounters.json`.
Unity's current serialized recipes must match that export. Do not regenerate from
the old inspection pack and silently restore its mixed formations.

Mixed frequency, Court support combinations and Queen escort pressure remain
**provisional balance**, requiring human play. Counts alone do not establish pacing.
