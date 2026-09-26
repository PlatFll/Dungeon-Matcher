# Native tile bursts

The four supplied 48×48 LibreSprite sources are preserved in `Originals/`.
Production files are 64×64, thirteen frames at **30 ms each (390 ms)**. Their
original colors and frame order are retained. Native nearest-neighbor pixel
placement fits each effect's complete footprint into its tile without soft edges.

- `Explosion`: directional bombs, Bardley cracked explosions and Bomb consumable.
- `PoisonExplosion`: Poison Bomb gas.
- `ShieldExplosion`: Shield Bomb's blue protective crest.
- `HealingExplosion`: Healing Bomb's warm restorative ring.

`Scripts/prepare_native.js` performs the cel work inside LibreSprite.
`Scripts/finish_native.js` opens native timing dialogs, set to 30 ms, and restores
ordinary transparent layers. PNG sheets, JSON exposures and preview GIFs are
exported by LibreSprite. Unity imports through `TileBurstArtImporter.Run`.

Each actual cleared cell receives one centered burst. Overlapping families retain
a special's identity at its own cell; collateral selects shield, healing, poison,
then generic in that deterministic priority. Preserved reward cells and protected
crystals do not get a false destruction burst. The effects do not add gameplay
waits; the small dissipating tail can overlap refill. Audio groups each family
instead of playing once for every affected tile.
