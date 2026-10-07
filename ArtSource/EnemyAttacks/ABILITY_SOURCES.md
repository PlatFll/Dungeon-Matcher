# Approved enemy abilities

## Judgment revision — review candidate, 2026-10-07

`judgment.json` adds `JudgmentStrike1`, `JudgmentStrike2` and `JudgmentFinisher`.
The first two deliberately re-time the approved native King sword poses. The
finisher directly edits those pixels into a higher held windup, committed torso
lean over planted boots, broader royal-gold cut and stepped recovery. It expands
to 112×80 for lateral clearance, retaining source pixel scale and bottom-center alignment.
The other two clips use 96×80. Faces, armor, crown and equipment are retained.
`Tools/author_king_judgment.py` reproduces sheets, exact timing JSON, review GIFs
and SHA-256 records. Import only these states with
`CombatActionImporter.ImportKingJudgment`; existing Bombardment remains intact.
No PixelLab calls or credits were used. These new motions await visual review.

The user accepted Royal Gold and the other concepts, with two revisions: the King holds and thrusts the sword with both hands, and the Shield Knight raises his shield before it glows blue. These choices replace the initial one-handed King thrust and frontal shield bubble.

`abilities.json` records every selected source frame, exposure, contact frame, controller state and accent palette. `FinishAbilities.js` and `Tools/Finish-EnemyAbilities.ps1` finish the PixelLab poses in LibreSprite. The `.aseprite` sources remain editable; matching PNG sheets, frame metadata and GIFs are exports. Import through `CombatActionImporter.ImportPixelLabAbilities` or `Tools/Review-EnemyAbilities.ps1`.

## Native finishing

- Preserve the original character palette, native scale, transparent canvas and bottom-center alignment.
- Add restrained gold, blue or green effect colors. Benediction uses a gold version of the Archbishop's accepted green Restoration gesture.
- The October 2 Shield Knight revision replaces the older static raise. Its selected poses include a torso shift, lifted shield, cyan contact pulse and a complete lowering motion. Mage frame 9 remains omitted.
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

## October 2 motion refinements

The equipment correction in `Refinements/EquipmentCleanup/` is the final production revision for Spear Knight, Royal Lancer and Shield Knight auto-attacks. It removes the invented back shields and sword, and keeps Spear Knight's original narrow helmet slit throughout the motion. `selected.json` points to these corrected frames. The Shield Knight ability and all motion timings remain the approved versions.

`Refinements/jobs.json` and `pose-jobs.json` record PixelLab prompts and job IDs. The accepted source folders retain original downloaded PNGs and SHA-256 records. `selected.json` and `abilities.json` identify the production exposures. First-pass spear loops are rejected because they swing the weapon; the pinned `*Thrust` variants are authoritative.

The three spear attacks use 192 × 80 production canvases. `StageThrusts.js` keeps the boots aligned and adds a short draw-back and straight upper-body extension in whole pixels before palette finishing. No character pixels are scaled. Every auto-attack still lasts 680 ms and applies damage at its authored contact.

Shield Knight uses `ShieldKnightBash` for his attack and `ShieldKnightGuard` for his ability. King preparation uses `KingRaise`; `KingBombardmentPrepared/sources.json` combines the approved two-handed contacts with selected `KingRecovery` frames. Recovery ends on the original idle pose, avoiding the previous abrupt return.
