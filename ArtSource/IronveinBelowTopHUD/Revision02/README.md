# Revision02 - cave-dark floor and charcoal block separator

User-directed native edit of Revision01. The walking plane is 20 native pixels deep instead of 8, extending its rear edge from y=198 to y=186. The existing feet, front edge at y=206 and separator bottom at y=225 stay fixed. Its cool near-black colors are sampled from the current CavernDepth source. Two 40x19 charcoal block variants make the front separator clearly readable. No scaling or interpolation.

All prior source artwork and scenes remain intact. Only the old floor group is hidden in the new art-only Unity scene. Existing characters, UI, board, lower crystal formations, other assets and gameplay remain unchanged. The review scene is excluded from builds. No new PixelLab spending.

Native editing occurred in LibreSprite, with ASE sources and exact reopened PNG roundtrips. Unity checks at 360x640, 720x1280 and 1080x1920 preserve original Graphic geometry and references, all protected opaque UI/actors, and all pixels outside the authorized platform band. Independent checks cover binary alpha, exact variant seams, original file hashes and serialized changes. See QA/Validation.json for the required Unity validator result. No live combat or device test is claimed.

Open Review.html or Preview/PlatformComparison.png. Stop for visual review. Do not merge.
