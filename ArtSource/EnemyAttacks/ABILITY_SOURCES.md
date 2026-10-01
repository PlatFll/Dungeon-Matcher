# Approved enemy abilities

The user accepted Royal Gold and the other concepts, with two revisions: the King holds and thrusts the sword with both hands, and the Shield Knight raises his shield before it glows blue. These choices replace the initial one-handed King thrust and frontal shield bubble.

`abilities.json` records every selected source frame, exposure, contact frame, controller state and accent palette. `FinishAbilities.js` and `Tools/Finish-EnemyAbilities.ps1` finish the PixelLab poses in LibreSprite. The `.aseprite` sources remain editable; matching PNG sheets, frame metadata and GIFs are exports. Import through `CombatActionImporter.ImportPixelLabAbilities` or `Tools/Review-EnemyAbilities.ps1`.

## Native finishing

- Preserve the original character palette, native scale, transparent canvas and bottom-center alignment.
- Add restrained gold, blue or green effect colors. Benediction uses a gold version of the Archbishop's accepted green Restoration gesture.
- Omit malformed Shield Knight source frames 6 and 8 and Mage frame 9. Shorten repeated holds and retain anticipation, contact and recovery.
- King Bombardment uses two timed contacts from the revised two-handed source. Its raised warning pose loops separately. The board owns both lane clears and the single final refill.
- Royal Command uses the accepted directional sword gesture; it is distinct from the two-handed ground thrust.
- Barricade Guard keeps the existing single-contact board-animation protocol. Other new casts use named states and numbered contact cues on the same actor/presenter and board queue.

## Revision provenance

| Asset | PixelLab job | Direction |
| --- | --- | --- |
| King two-handed ready pose | `9fa02753-19eb-470c-9a50-be597d47f0cd` | Both gauntlets on the same hilt, vertical blade pointing down; preserve identity and 96 x 80 canvas. |
| King two-handed thrust | `0a97ecfb-1776-4c8f-bdb2-7619873c87b2` | Both hands remain on the hilt during lift, ground thrust, planted hold and extraction. |
| Shield Knight raised shield | `44935141-2841-4c70-bf17-fbe12e19921b` | Raise the decorated shield, then a blue rim pulse; lower to the original stance. |

The original concept gallery is retained as review history. Current runtime behavior and executed checks are recorded in `Docs/Validation/ENDLESS_COMBAT_POLISH.md`.
