# Dungeon Matcher — Approved Enemy Ability Revisions

**Approved:** 2026-11-10

**Status:** Implemented on `codex/approved-enemy-ability-redesigns`; the user approved actual-game integration and merging PR #227 on 2026-11-10. The requested PixelLab Marine Net and Hearth Ember replacements use the existing runtime bindings. The approval record below is preserved. Executed evidence and remaining balance limitations are recorded in [the verification report](Validation/EnemyRevisions2026-11-10.md).

**Scope:** Exactly two previously approved Dungeon revisions, four approved Frostwake/Drowned Court gameplay redesigns, and one Drowned Court presentation-only adjustment.

## Dungeon — two earlier approvals carried forward

### Knight Captain — Advance the Ranks (working name)
- **Keep:** Chain Volley and its established mechanics.
- **Replace:** Royal Command's immediate group attack with **Advance the Ranks**. The King's Royal Assault retains the immediate coordinated-attack role.
- On cast, advance **both ordinary-attack and special-cast readiness** of **every other living allied enemy**, **including minibosses and bosses**, by the equivalent of **one accepted player move**.
- Do **not** accelerate the Captain, fire any action immediately, or reduce a countdown below **1** (2 → 1; 1 → 1).
- Do **not** shorten an ongoing warning, channel, telegraph, ice stage timer, or other response window. Affect only not-yet-started action readiness.
- This is one cast's countdown advance, **not** a lasting haste buff. Preserve the Captain's normal special alternation/eligibility.
- Do not add forced Captain-and-boss formations merely to showcase this interaction.

### Court Mage / Royal Mage — True Ice escalation (lore/name pending)
- The existing Dungeon `court_mage` uses a shard of Frostwake's magical ice; do not rename the character without approval.
- Each cast fixes **one eligible ordinary gem** at its position, unswappable directly, with **up to three Mage-owned gems** and existing safe-placement restrictions. Other gems still fall around the fixed gem.
- Ignored encasement advances **Frost → Heavy Ice → Bifrost**, sharing Frostwake's authoritative stage cadence, appearance and break/thaw rules. No fourth stage.
- An eligible adjacent clear thaws a layer; supported direct clears/specials/cascades remain viable as defined by shared ice rules.
- **At Bifrost**, deliberate player-made **ordinary clearing that actually breaks the encasement** triggers the same **Frostbite and break retaliation** as Frostwake. A partial adjacent thaw, an applicable safer special/cascade exception, or automatic owner-death release is **not** such a break.
- Killing a Mage immediately releases **only that Mage's** encasements at all stages **without Frostbite, retaliation, or a thaw reward**.
- **No ambient Frostwake ice seeding in the Dungeon.** The Mage's existing encasements progress there independently of the Frostwake zone effect. No extra passive or stage-three attack.
- Share one ice-state implementation with correct owner/gem identity and restriction counting; Knight Captain readiness acceleration never speeds active ice-stage counters.

## Frostwake — approved redesigns

### Glacier Troll — Frostwake
- **Keep:** Glacial Armor.
- **Replace:** Marked-gem Avalanche Blow with a **two-accepted-move column warning**.
- **Any player clear in that column** cancels the warned attack, including clears from specials or player abilities.
- If unanswered, apply **Frost to at most two eligible ordinary gems** in the warned column.
- Never upgrade ice already present, apply the old direct hit as an additional effect, or force invalid targets. Invalid placement **fizzles**.
- Purpose: a readable positional ice puzzle, not a second marked-gem damage strike.

### Iceborn Champion — Frostwake
- Replace Hallbreaker's duplicate marks with **Shieldbreaker Charge**.
- Telegraph/channel for **two accepted moves**, then strike with a **modest first impact** followed by the **main impact**. Each hit obeys existing ordinary shield and HP damage rules.
- Counter through weakness-based **Stagger**, defeating the Champion, or sufficient defense; give it a **long, visible recovery** afterward.
- **Keep its normal basic attack unchanged.** No shield bypass and no hidden additional Frostbite multiplier.
- Actual damage numbers and recovery timing remain balance/implementation tuning, not approved exact values.

### Hearthkeeper — Frostwake
- Replace repeated burst healing with **one Hearth Ember structure**, **one hit to destroy**, **maximum one active**, surviving for **three future accepted player moves**.
- On each of those moves, heal the **one fixed wounded ally** for **5 HP** (up to **15 HP total**).
- A player destroying the Ember **stops all future healing** and removes **one ice layer from at most two adjacent iced gems**.
- Defeating Hearthkeeper extinguishes the Ember **without** the thaw reward.
- The Ember is an intentional dual-purpose objective: stop healing and open a local ice route.

## Drowned Court — approved revisions

### Pearl Cantor — flooded-mode heal only
- **Keep:** existing recipient lock, two-move channel, interruption rules and **all dry-mode behavior**.
- While flooded, begin the channel with a **20-HP intended heal** on the existing locked recipient.
- Each bubble actually popped **during that channel** reduces the heal by **10 HP**, clamped at zero (20 → 10 → 0).
- Display the **remaining heal** after each pop.
- No other part of the Cantor's kit changes.

### Reef Netweaver — presentation only
- Present existing **Thorny Snare** as **barbed sea-netting or an urchin-line trap** so it reads as underwater reef equipment, **not forest thorn growth**.
- **Final new name is NOT approved yet.** Keep `Thorny Snare` as a working/mechanical identifier until naming approval.
- **No mechanical changes:** the snare follows a falling gem, restricts direct manual swapping, expires after its established duration, costs AIR when deliberately cleared by a flooded manual match, and remains safely removable by qualifying indirect/special/cascade clears. Preserve established placement safety, ownership, cap and expiry.
- This is an **art/presentation direction**, not approval of a finished sprite, palette, asset replacement or altered hitbox. Any production sprite must follow the current art-direction guide and be reviewed at native pixel scale.

## What has *not* been approved

All **other** ability changes in the broad enemy-and-zone review remain **unselected suggestions**. Do not implement, rename, rebalance or recategorize them merely because the report proposed them. Preserve their existing established kits unless the user separately approves a change.

## Implementation gates and ownership

1. Before implementing, inspect current `AGENTS.md`, `PROJECT_STATE.md`, enemy definitions, zone rules, board/ice/pin mutation paths, HUD warnings, and the actual affected scripts. Confirm which parts (if any) already exist.
2. Keep gameplay deterministic under authoritative board resolution; no direct enemy-owned grid mutation, duplicate damage, stale warnings, impossible response states or cross-zone effects lingering after leaving a zone.
3. Validate warning cancellation, stagger, ownership/death cleanup, safe targets, save/Continue, exit/zone transitions, multiple enemies, simultaneous board hazards, readability and balance.
4. Keep numerical values not specified here **tunable** and distinguish authored design from tested implementation. Update the owning rules, project state, and this approval status only after the changes actually land.

## Change record

| Date | Decision | Status |
| --- | --- | --- |
| 2026-11-02 | Knight Captain **Advance the Ranks** and Court Mage shared **True Ice/Bifrost/Frostbite** | Previously approved; reaffirmed |
| 2026-11-10 | Glacier Troll column-warning Frost; Iceborn Champion Shieldbreaker Charge; Hearthkeeper Hearth Ember; Pearl Cantor flooded bubble-counter heal | **Approved design**, implementation unverified |
| 2026-11-10 | Reef Netweaver's snare **sea-net/urchin-line look** only; final name pending | **Approved presentation direction**, art asset not yet approved |
| 2026-11-10 | Implement the six redesigns and marine-net presentation on a feature branch | User authorized code/data/VFX/LibreSprite work and a PR; separate approval still required to merge |

## Implementation handoff

Review PR: [#227](https://github.com/PlatFll/Dungeon-Matcher/pull/227).
Implementation commit: `2a9d5a60fe467af54194dad149b3c75c9e7e4a77`.
Final gate: **38/38 passed, no skips**, and `Tools/Validate-Unity.ps1` passed
with Unity 6000.3.19f1. Not merged; physical-device and human balance review
remain outstanding.

All seven scoped revisions are implemented using the existing board queue, damage
flow, accepted-move clock and continuation architecture. King's Royal Assault and
unrelated encounters remain unchanged. Mage ice shares the Frostwake rules and
records caster ownership; leaving a zone uses the existing arrival cleanup.
Champion's initial **10/25** impacts and **three-move** recovery are provisional,
editable values, not newly approved balance constants.

The [gameplay gallery](Validation/EnemyRevisions.html) contains actual Unity
Game-scene captures from controlled automated fixtures. Marine net and Hearth
Ember have editable LibreSprite sources and native 32×32 PNGs. They are review
candidates; this implementation does not declare them new golden references.

The [verification report](Validation/EnemyRevisions2026-11-10.md) distinguishes
automated Play Mode, focused rule checks and untested device/balance behavior.

**Maintainer rule:** When these designs are implemented, update this file to indicate the exact PR/commit and validation status and update the relevant game/zone documentation. Do not upgrade other review suggestions into approvals by inference.
