# Production cast animation repairs

[Open the synchronized before-and-after player](Review.html). Choose an actor, play at game speed or jump to a cited frame. The Swordsman, Halberdier and Axeman follow-ups open beside their last reviewed drawing. You can also select the previous game version for those clips. **Unity Game scene** opens the rendered footage for each of the 17 revised expansion clips.

Production source commit `9ba420e884765b9e33bcebc734ba121dcb352b10` integrates 40 animation states through the existing production controllers. The original audit and review were recorded before integration. [Original audit](../CastMotionAudit/Review.html) · [Production paths and file hashes](production.json)

### Expansion characters

- The Guild Swordsman's black hair and reduced head remain. The front cheek steps inward and the reflected color turns around the chin. The eyes, brow, mouth, collar and foreground sword keep their places as he attacks.
- The Nacre Halberdier uses the light pearl color already in his original eye. It fills the exposed eye without an iris or glint. The lid closes over the same shape during the idle and attack.
- The Iceborn Axeman raises and lowers one rigid axe. In the previous review the front wrist-to-socket distance varied from approximately 9 to 25 native pixels. In production it measures 10–11 pixels and the rear hand stays approximately 37 pixels from the socket. The blade follows the hands through the 320 ms contact and reaches its lowest position during follow-through. [Per-cel landmarks](axe-geometry.json)
- The approved left-leaning idles, Guild hair colors, healer body motion, Bulwark mace and other scoped eye work are in the production sheets. The 17 updated clips retain their original animation keys, durations, pivots, references and combat events.

### Audit repairs

| Original findings | Production repair |
|---|---|
| Ten Dungeon idles | Closed the Miner’s pick and glove and the identified joins on the standard, shoulders, sleeves, gauntlets, hammer and crossbows. Review the marked breathing cels and their return poses. |
| Two playable deaths | Finished the Minotaur’s folded legs, hooves and overlapping hand and the Kobold’s crouched legs, boots and tail. Both settle on the original floor. |
| Bore Engineer and Shellback Porter attacks | The drill holds its reach through contact. The Porter’s mallet reaches in front of him before returning to guard. |
| Three Frostwake clips | Removed detached Reaver pixels, restored the Harpoon Hunter’s complete barbed tip, and fitted the Rimecarver’s frost burst inside its contact frame. |
| Ancient Treant hit and Orc Drummer death | The Treant’s crown stays visible through recoil. The Drummer falls with the strapped drum and the loose mallet settles on the floor. |
| Forest cast interruptions | The Briar Matriarch, Barkhide Warden, Orc Rootbinder and Elven Mender each interrupt the gesture and return to neutral. Their successful releases keep their original drawings. |

The 20 audit findings cover 23 bound clips because the interruption finding spans four actors. The revised native masters and runtime PNGs are linked from the player. The proposed Royal Swordsman reference still is pixel-identical. Its parent sheet hash changed when two other idle cels were repaired.

### Verification

Unity 6000.3.19f1 passed the repository validator. All **25 focused tests passed** on the production source revision. The six expansion formation cases loaded the Game scene, and the Slinghunter single-hit tests exercised shield and health separately. Five formations supplied footage for the 17 changed expansion states.

LibreSprite reopened all **40 editable production sources**. Across **495 native cels and 524 Unity sprite keys**, the comparison checked every visible color and alpha value at each exposure boundary. All 40 prior clip and sprite-import metadata hashes match. The timestamped Game-scene captures sample the actual Unity animators with events suppressed while stepping. The separate combat tests exercise normal event delivery. These checks used the desktop Editor. [Test names, capture provenance and native checks](validation.json)
