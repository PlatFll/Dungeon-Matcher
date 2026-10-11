# Frostwake Peaks — Zone Brief

## Zone identity

A snowy mountain region inhabited by **Iceborn human clans** with Viking flavor.
They are not generic kingdom soldiers in fur. They are mountain-dwelling, migration-forced people shaped by a dangerous glacier and a culture of mobile survival.

## Core lore

Working lore direction:
- the Iceborn live in moving mountain settlements / wandering halls;
- their halls and supplies are dragged between safer routes because the glacier keeps swallowing older paths;
- they seek the dungeon-moving crystal because if it can move a dungeon, perhaps it can help move or save their people.

This should feel simple, readable and aligned with the project’s general light-but-serious fantasy tone.
No convoluted prophecy needed.

## Zone effect foundation

Keep the established progressive freezing structure:
- **Frost**
- **Heavy Ice**
- **Bifrost**

Neglected freezing worsens over time.
Breaking high-stage ice through a player-made clear is dangerous.
The player-facing debuff remains **Frostbite** (not hard crowd-control “Freeze”).

Mountain ice should read as **encasing a gem / occupying its state** rather than replacing the whole lane with unrelated stone.
Maintain good board readability.

## Roster direction

Use the previously approved/proposed roster direction as the starting point:
- Iceborn Raider
- Harpoon Hunter
- Frostfang Wolf
- Rimecarver
- Shieldmaiden
- Rime Skald
- Hearthkeeper
- Frostblade Reaver
- Glacier Troll
- Iceborn Champion
- Jarl Haldra, the Icebreaker

Preserve the good overall direction unless a targeted implementation issue requires a refinement.
Do not bloat the zone with unnecessary extra enemy systems.

## Jarl Haldra revision — locked

Jarl Haldra’s **Glacier Cleave** is revised as follows:

1. She selects a target gem.
2. She **fully freezes that gem** and marks it.
3. She starts a heavy wind-up.
4. The player has **3 accepted player moves** to respond.
5. If the player manages to **fully unfreeze** that gem before resolution, the ability is canceled/fails according to the existing authoritative ability-failure handling.
6. If not canceled, Haldra **consumes the marked gem** and performs the attack.
7. Damage scales based on the freeze stage remaining on that gem at resolution:
   - lower stage = lower damage
   - higher stage = higher damage
   - if the gem remains at **Bifrost** stage, the attack deals the heaviest damage and additionally applies **Frostbite** to the player.

Important implementation note:
- this means the player’s response can partially mitigate damage even if they fail to fully answer the mechanic;
- Astra should tune the damage bands conservatively first and mark the values as provisional if the user has not given exact numbers.

This ability should visually feel important and threatening, worthy of a zone boss.

## Art direction for the zone

The zone should feel like:
- dark rock,
- deep cold blues,
- pale ice,
- timber structures,
- ropes, hides and furs,
- occasional warm fire/orange brazier contrast.

Avoid turning the whole scene into flat white snow.
Readable board contrast remains critical.

UI/environment should be zone-specific, not just “blue dungeon.”
