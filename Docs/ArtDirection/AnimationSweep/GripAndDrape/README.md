# Enemy grips, armor and loose cloth

[Compare the 13 repaired clips](Review.html) · [Step through the entire enemy roster](Roster.html) · [Watch the Guild heal in the Game scene](guild-heal.webp)

The inspection followed 4,189 timed exposures in 445 bound animation clips across 85 enemies and forms. It included special holds, interrupts, deaths, the turret and both additional Grand Delver forms. There were 2,597 distinct original drawings. Repeated drawings and the final hold were inspected in their playback context. The [inventory](coverage.csv) identifies every clip, its exposure count, its checksum and the physical features inspected on its enemy.

## Production repairs

| Enemy | Action | Edited exposures, starting at 1 | Observation |
|---|---|---|---|
| Elven Guild Chirurgeon | Ability | 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 | Her staff and shoulder rose while the cape hem stayed pinned in place. The long panel beside the staff opens first, and the short panel behind the satchel follows. Both settle after the arm comes down. |
| Elven Guild Chirurgeon | Idle | 3, 4, 5, 6, 10, 11, 12, 13, 16, 17, 21, 22, 23, 24 | The split blue cape kept the same lower outline through the entire breathing cycle. Its free panel now trails the shoulder by a beat and settles into the original resting shape. |
| Elven Pathblade | Idle | 3, 4, 5, 6, 10, 11, 12, 13, 16, 17, 20, 21, 22, 23 | The long green cloak held its tip rigid behind a shifting shoulder. A delayed two-pixel lift travels through the last part of the fold. |
| Guild Ranger | AutoAttack | 3 | The forward hand met the musket on both sides of frame 3, but the black stock edge above the palm opened for that one exposure. |
| Guild Ranger | Idle | 3 | A two-pixel opening flashed between the cheek and the metal receiver on the third drawing. The steel contact matches the adjacent exposures. |
| Guild Swordsman | AutoAttack | 7 | On the recovery drawing, the room showed through the upper half of the steel shoulder plate. The plate now carries its steel field and highlight through to its intact gold rim. |
| Iceborn Axeman | AutoAttack | 6 | Two background pixels showed inside the dark crease of the raised leather forearm on the change of direction. The crease now reads as one piece. |
| Puffer Sentinel | InflatedAttack | 11, 12, 13 | Background flashed through the solid cream belly on the three drawings where the staff comes up. The holes are filled with the surrounding belly color. |
| Royal Standard Bearer | Idle | 2, 3, 4, 5, 6, 8 | The gloved hand briefly showed background where it meets the forearm. The free end of the standard now eases up and down while its pole and sewn edge hold steady. |
| Seismic Smith | Idle | 3, 4 | Two pinholes appeared on consecutive drawings, one in the handle and one at the dark inner gauntlet. Both now use the matching local material. |
| Siege Sergeant | Idle | 2 | A one-pixel hole flashed through the wooden crosspiece beside the raised arm on frame 2. |
| The Grand Delver | OreChargedAutoAttack | 5 | A two-pixel insert in the drill collar became transparent only during the extended thrust. The inset and its highlight now continue across the hold. |
| The King | Idle | 4 | A pinhole interrupted the gold edge where the mantle overlaps the sword on frame 4. |

The Guild Chirurgeon’s shoulder and wrist lead the gesture. The free panel beside the staff now opens up to four native pixels, and the shorter panel behind the satchel follows by up to two. The Pathblade’s long fold and the standard’s point bend progressively away from their attachments. Each clip returns to its original resting drawing.

The Swordsman’s recovery had a transparent patch inside a closed steel outline. Thirteen texels restore the shoulder plate and its highlight above the intact gold trim. On the Axeman, isolating the six native layers located the two missing texels precisely at the overlap of the far forearm and the rigid axe. The repair lives on the far forearm layer.

## How the drawings were inspected

Each contact sheet used the production sprite IDs in chronological order, including repeated exposures. Contrasting mattes and enlarged previous/current/next crops exposed wrist gaps and one-frame material changes that were hard to see on a dark stage. The additional screening examined 748 local candidates and 218 examples of small openings surrounded predominantly by one material. These sets overlap. A flag triggered visual inspection before any edit.

Examples of openings that carry the motion include the Elven Mender’s bent elbow, the Crossbow Guard’s bowstring, the Reef Netweaver’s hanging mesh and the space around the Puffer’s lifted staff. The Hearthkeeper’s loose warm pixels travel away from her brazier in order. The Iceborn Champion’s heavy pelt already changes shape after his hammer turns. These drawings retain their original movement.

## Verification

Baseline `045936ea62cb5e9477659d6d656fd6a66b15f357`. Unity and the capture comparison used production art commit `191c473a867bd459049a2364bc0a5dc63c5c66bb`. LibreSprite reopened and exported all 13 editable masters. The before/after sheets compare the bound sprite rectangles, and the edits changed 56 cels on 11 enemies. The other 4,133 timed exposures matched their baseline RGBA data exactly. Every bound animation file retained its original checksum. The Puffer master splits its last 45 ms hold into 35 and 10 ms. That split is identical to the original.

Unity 6000.3.19f1 passed 17,494 import assertions and 5,902 production Game-scene pose and viewport cases. Eight focused regression tests passed, including the Guild heal, Guild front line, Forest and Frostwake formations, inflated Puffer, Slinghunter contact and Ironvein behavior. The 212 timestamped Game captures show four formations at 1080 × 1920. In the seven revised expansion clips, every changed opaque texel could be matched to a captured pose within one RGB level. Cleared pixels and changes outside those four formations were verified against the native sheets. The screen comparison measures crop correspondence for the saved frames, including overlapping graphics and the final hold.

The roster viewer draws directly from Unity’s production textures. The before/after viewer stores paired strips so later source changes cannot replace the original comparison. Physical-device playback has yet to be checked. [Test receipt and capture hashes](validation.json) · [Per-exposure changes and editable masters](repairs.json)
