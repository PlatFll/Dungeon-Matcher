# User-edited larger Silver controls

On 2026-11-12 the user completed the enlarged wave, Settings and power-up
holder in LibreSprite, requested actual-game integration and explicitly
authorized merge. Their exact saved `.ase` documents and exports are in Inputs.
Previous PNGs are preserved in Previous. No new drawing, recoloring or generation.

| Artwork | Native canvas | Visible edits versus opened copy |
| --- | --- | --- |
| Wave plaque | 264 x 99 | 1,980 pixels |
| Settings | 84 x 84 | 652 pixels |
| Shared potion/bomb holder | 105 x 105 | 1,814 pixels |

The opened potion and bomb icon copies have zero visible changes; their production
icons remain unchanged. The user's previously edited energy frame and all four
ability icon sources are preserved exactly.

The three runtime PNGs are byte-identical to the LibreSprite exports. Wave strips
are exact column crops [0,36), [36,114), [114,150), [150,216), [216,264); reassembly
matches every RGBA pixel. Only uniform straight rails shorten on narrow screens.
All importers and GUIDs remain unchanged. See Manifest.json for exact hashes.

Shared layout keeps the approved on-screen sizes, using the larger source at
native scale on normal layouts rather than multiplying it by 1.5 a second time.
The existing 360x640 compact layout uses a uniform two-thirds fit with Point
filtering; all outer bounds and wave joins remain on physical pixel boundaries.
Gameplay, board dimensions, actors, environments, ability behavior and energy
authority are unchanged. No generation credits spent.

See Validation.md and Preview for actual Game-scene results and screenshots.
Historical generators do not supersede these manual edits.
