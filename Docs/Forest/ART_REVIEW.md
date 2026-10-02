# Forest art approval and guide check

The user approved the two Phase 3 milestone designs on 2026-10-02 and requested
a check against the current coloring and shading guide before Phase 4.

All six stills are retained byte-for-byte from the guide-colored Phase 3 files.
The guide does not impose one universal palette: olive orc skin, cool scout
cloth, muted rose healer cloth and wine apex cloth remain distinct. The shared
near-black outline, warm human skin, matte wood/leather, cool iron and pale linen
use established material ramps. Highlights are concentrated on upper/left
planes with hard shadow clusters, no gradient, resampling or alpha fringe.
The Warden's bark and Matriarch's trim carry more detail than ordinary enemies;
they remain readable at native scale and do not require a redesign.

`ArtSource/Forest/Approved/manifest.json` records exact PNG hashes, native sizes,
bounds, palette counts and source paths. The separate approved forest board uses
only these actual PNGs at a common integer scale. The historical dungeon board
and all existing dungeon sprites are preserved.

Stills: Scout 64x64, Trailguard 64x64, Mender 64x64, Rootbinder 64x64,
Warden 64x64, Matriarch 96x96 (declared apex footprint). All have binary alpha,
shared bottom-center pivots and fixed floor alignment. Phase 4 uses stills;
this records design approval without silently approving unseen animation.

No new PixelLab requests or image generation were made for this check.
