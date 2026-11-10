# Enemy revision effect candidates

Two hand-authored 32×32 RGBA effects, exported through LibreSprite. These are
review candidates, not additions to the golden-reference register. Existing
character sprites and the old snare source remain untouched.

| Effect | Opaque colors | Alpha | Purpose |
| --- | ---: | --- | --- |
| MarineSnare | 6 | 0 or 255 only | Barbed sea-net overlay; existing Thorny Snare rules/name |
| HearthEmber | 8 | 0 or 255 only | One-hit healing structure, stone support and warm ember |

`build.py` places pixels directly, creates editable `.ase` documents with
LibreSprite, exports the runtime PNGs, and verifies exact RGBA equality. It uses
no generative service or credits. `manifest.json` records source hashes, bounds,
palette counts and the actual Rattlebones/Farmer/Pan reference hashes inspected.
`Review.png` shows native-size and nearest-neighbor enlarged views.

Rendering follows the canonical outline, clustered shading and native-pixel
guidance. Rattlebones is the rendering authority, not a palette or anatomy
template. Runtime textures use Point filtering and uncompressed source imports.
The gameplay board retains its existing responsive display scale.
