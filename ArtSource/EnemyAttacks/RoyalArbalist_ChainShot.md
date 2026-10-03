# Royal Arbalist — Royal Chain Shot

The Special-rank chain ability reuses the approved native drawings in
`RoyalArbalist_AutoAttack.png`, without changing those pixels or the basic clip.
`CombatActionImporter.ImportRoyalArbalistChainShot` builds the dedicated Ability
state on the existing idle controller.

- Pose indices: 0, 1, 2, 3, 4, 5, 2, 3, 4, 5, 6, 7, 8.
- Exposures in milliseconds: 120, 80, 100, 80, 80, 80, 80, 80, 80, 80, 80, 100, 120.
- Aim, first recoil, re-aim, second recoil, recovery; total 1.16 seconds.
- Chain contacts at 380 and 700 ms; completion at 1150 ms.
- The existing board queue applies each chain once and keeps action ownership
  through recovery. Pause, death and stale-contact guards remain authoritative.
- Native canvas, bottom-center pivot, material palette and Point filtering are
  inherited from the approved sprites.

No generation requests or subscription allowance were used for this clip.
