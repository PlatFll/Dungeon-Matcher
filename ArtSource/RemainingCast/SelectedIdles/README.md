# Grounded enemy idle family

The user's accepted restored guard motion is the target for the nineteen enemy
idles from Miner through King. Rattlebones remains the unchanged reference.
This supersedes the rejected stronger motion in `../Idles/`.

All loops use nine 130 ms exposures (1.17 seconds), a restrained rise, settle,
blink and recovery above anchored feet. Heads translate with the stance; there is
no extra nodding/shearing motion. The King settles less to retain weight. Capes
and the banner respond modestly. Tools remain rigid and connected to their grips.
Sleeve/collar overlap prevents transparent tears between moving selections.

## Authoritative sources

- `ArtSource/LocalEnemies/`: revised Miner, Basket Villager and Barricade Villager
  idles. Earlier versions are preserved in `Originals/BeforeGroundedFamily/`.
  Existing attack/ability drawings, contact events and durations are unchanged.
- `ArtSource/GuardIdles/`: Crossbow Guard's complete lower bow contour and body
  joins are repaired; his accepted preceding file is in `BeforeCrossbowRepair/`.
  Barricade Guard, Spear Guard and Siege Sergeant retain their exact accepted files.
- This directory: Town Marshal, four knights, six royal units and King. The larger
  Royal Lancer/Arbalist helmets are retained. All ready poses match their current
  recolored stills exactly. No material colors were added.

Miner retains 96×80 cells; the others retain 64×64. Native files, horizontal PNGs,
GIF previews and JSON exposures are supplied. Farmer, Pan Villager, Rattlebones,
Bardley and the beyond-King Minotaur are outside this refinement.

`../FamilyPreview.html` shows synchronized group playback beside Rattlebones,
native-size views, frame stepping and light/dark backgrounds.

## Unity and reproduction

`CombatIdleImporter.ImportEnemyFamily` reuses the existing importer, controllers,
fixed full rectangles, bottom-center pivots and definition-driven presentation.
Source names map to historical definitions as follows: SwordKnight → Knight,
RoyalMage → CourtMage, RoyalArcanist → RoyalArchbishop. The last definition already
displays Royal Arcanist. These are art-reference mappings, not gameplay renames.
Existing local-enemy action states and timing remain intact.

`Scripts/assemble_family.ps1` combines the inspected native anatomy helpers with
`refine_family_native.js`. Run the assembled JavaScript inside LibreSprite's GUI;
the installed development build does not expose active cels to batch scripts.
All production pixel editing and native saving occurs inside LibreSprite.
The preserved local idles and static recolors make repeat runs deterministic.
Do not replace the preserved baseline copies when rerunning.

`Scripts/export_family.ps1` performs native LibreSprite PNG/GIF/JSON exports.
`Scripts/verify_family.py --unity` checks all 171 frames, exact ready poses,
palettes, binary alpha, timing, feet, exposed rigid prop pixels, canvas bounds,
unchanged accepted guards and Unity sheet bytes. `../FamilyVerification.json`
records hashes and geometry evidence. Visual review remains necessary.

`CombatIdleValidation.ImportEnemyFamilyAndRun` exercises actual production-scene
playback/pause and all poses at two portrait sizes in a graphics-enabled batch
editor. See `Docs/Validation/GROUNDED_ENEMY_IDLE_FAMILY.md` for executed evidence.
