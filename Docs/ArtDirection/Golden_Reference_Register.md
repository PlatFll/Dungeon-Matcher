# Dungeon Matcher — current golden-reference register

2026-10-11 · canonical guide v1.13.1. This register identifies current images and roles; the canonical art guide owns direction. Enemy images now reflect the user-authorized cohesion/canvas pass. Rattlebones remains unchanged. Puffer's original facial expression has been restored at the user's request; none of the reference images below changed.

Rattlebones is explicitly primary by the current user instruction. Farmer and Pan Villager retain their existing supporting status. Four additional memberships below are proposals for review. Approval of an existing artwork, implementation or merge is not approval of new golden-reference membership.

## Exact current images

Each convenience PNG is a lossless extraction of the currently bound fallback/ready sprite, using its Unity GUID/fileID and imported rectangle. It is not a regenerated interpretation or a new approved replacement. Current motion sources are the linked controller and `ArtSource/EnemyArtCohesion/jobs.json` / `unity-import-manifest.json`. `Audit2026-10/bindings.json` remains the immutable pre-edit ledger, with old rectangles and hashes. Recheck hashes before a future art task; if production/reference authority changes, update this register rather than relying on a dated gallery.

[Original audit comparison board](Audit2026-10/boards/golden-comparison.png) · [Current enemy before/after player](../../ArtSource/EnemyArtCohesion/Review.html)

### RattleBones

**Status:** Primary golden standard — user instruction 2026-10-11.

**Reference for:** Crisp contours, broad connected shading, clear focal face/prop and grounded compact motion.

**Do not copy:** Do not copy skeleton anatomy, cyan eyes, royal colors or exact stance to other species.

- Current native image: [skeleton-still.png](Audit2026-10/media/skeleton-still.png) (64×64).
- Extracted PNG SHA-256: `705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c`.
- Production source: `Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png`; SHA-256 `59b04929e9fdc15ae9f1d70fd3c85c6ea5362bdac479eb12f45cfcbb0efe4306`.
- Unity GUID `ea3e0e0bde8898948aad0f1b15065804`, fileID `-873265099634963608`, sprite `Rattlebones_Idle_00`.
- Extraction rectangle in PNG top-left coordinates: `[0, 0, 64, 64]` (right/bottom exclusive). No scaling, trimming or recentering.
- Current controller: `Assets/_Game/Animations/CombatIdles/Rattlebones_Idle.controller`.

### Farmer

**Status:** Existing supporting reference; retained.

**Reference for:** Warm human skin, quiet workwear, straw and an identifying improvised tool.

**Do not copy:** Do not copy the same hat/face/pose to all locals or force all cloth brown.

- Current native image: [farmer-still.png](EnemyCohesion2026-10/farmer-still.png) (112×88).
- Extracted PNG SHA-256: `6189bc2cfe01bab84bafada24a7b129a3fd8c2225f629eb8580386f461109e2d`.
- Production source: `Assets/_Game/Art/CombatIdles/Farmer_Idle.png`; SHA-256 `f1ee772c518c6106617dcf782de95c431e6d90dd6d079a6a5d1d097ce1f10164`.
- Unity GUID `f9bceeac2ee99d542bf3ca0aa1583b2f`, fileID `99358554080292503`, sprite `Farmer_Idle_00`.
- Extraction rectangle in PNG top-left coordinates: `[0, 0, 112, 88]` (right/bottom exclusive). Full expanded canvas; no scaling/trimming. Unity source-floor pivot: (56, 4).
- Current controller: `Assets/_Game/Animations/CombatIdles/Farmer_Idle.controller`.

### Pan Villager

**Status:** Existing supporting reference; retained.

**Reference for:** Simple human expression, restrained cloth and dull iron with one large prop mass.

**Do not copy:** The pan remains cookware, not a heraldic shield; do not normalize her face/gender presentation.

- Current native image: [pan_villager-still.png](EnemyCohesion2026-10/pan_villager-still.png) (112×88).
- Extracted PNG SHA-256: `e1be89f8231483df5db6739dd2cae7bef83e11aaa9b028e4cb1b60ea55aba0ca`.
- Production source: `Assets/_Game/Art/CombatIdles/PanVillager_Idle.png`; SHA-256 `cf2f8bcb3d7066e5837e933e105ed9ff70cd50780bf48d26cb8df9185833e51d`.
- Unity GUID `3c27279675a73fa4483e90748a589ab2`, fileID `2638720417487856195`, sprite `PanVillager_Idle_00`.
- Extraction rectangle in PNG top-left coordinates: `[0, 0, 112, 88]` (right/bottom exclusive). Full expanded canvas; no scaling/trimming. Unity source-floor pivot: (56, 4).
- Current controller: `Assets/_Game/Animations/CombatIdles/PanVillager_Idle.controller`.

### Royal Swordsman

**Status:** PROPOSED addition — membership pending user review.

**Reference for:** Large clean armor planes, dark visor and disciplined red/gold accents.

**Do not copy:** Not universal white armor, closed faces, or a universal melee pose.

- Current native image: [royal_swordsman-still.png](EnemyCohesion2026-10/royal_swordsman-still.png) (112×88).
- Extracted PNG SHA-256: `f7d4896847862dd507afefcf3853854b5115d5578d4235012564cc0e05f46b77`.
- Production source: `Assets/_Game/Art/CombatIdles/RoyalSwordsman_Idle.png`; SHA-256 `c9bfa06b8a06eb4bb1348e4b6d0bd8e47112db7c2abc29e170816f8838495ae9`.
- Unity GUID `515d6081feed5644fb014b27bfe1c414`, fileID `-4154602047249404397`, sprite `RoyalSwordsman_Idle_00`.
- Extraction rectangle in PNG top-left coordinates: `[0, 0, 112, 88]` (right/bottom exclusive). Full expanded canvas; no scaling/trimming. Unity source-floor pivot: (56, 4).
- Current controller: `Assets/_Game/Animations/CombatIdles/RoyalSwordsman_Idle.controller`.

### Elven Mender

**Status:** PROPOSED addition — membership pending user review.

**Reference for:** Quiet fabric, exposed readable face, simple support equipment and graceful open-hand posture.

**Do not copy:** Not mandatory elf anatomy, cream/rose costume or identical casting hands.

- Current native image: [elven_mender-still.png](EnemyCohesion2026-10/elven_mender-still.png) (112×88).
- Extracted PNG SHA-256: `f5b3d67796b1b65fdbf4d0d8043cfbeaa08371c2cd1b03fcf58f9b82d46348fa`.
- Production source: `Assets/_Game/Art/Forest/Elven_Mender.png`; SHA-256 `80e0a4bba8993b617aff9d186eb1542565de5afbcb584d6112dad2329cdfaae2`.
- Unity GUID `1849556a95e20914ab7cf00cb59fab9f`, fileID `21300000`, sprite `Elven_Mender`.
- Extraction rectangle in PNG top-left coordinates: `[0, 0, 112, 88]` (right/bottom exclusive). Full expanded canvas; no scaling/trimming. Unity source-floor pivot: (56, 4).
- Current controller: `Assets/_Game/Animations/Forest/Elven_Mender.controller`.

### Orc Trailguard

**Status:** PROPOSED addition — membership pending user review.

**Reference for:** Broad nonhuman anatomy with clear skin/leather/iron separation and readable tool acting.

**Do not copy:** Not mandatory olive skin, muscular width or axe equipment.

- Current native image: [orc_trailguard-still.png](EnemyCohesion2026-10/orc_trailguard-still.png) (112×88).
- Extracted PNG SHA-256: `e8621e70149e13c19c1db958c6a190fa40cf76b6b9e2f69ad4dcba56f02e295d`.
- Production source: `Assets/_Game/Art/Forest/Orc_Trailguard.png`; SHA-256 `710a1316bc9c40506368a45aa7032a62535d0cb45482c597f99503fe290f7622`.
- Unity GUID `3f27ba57600c5ee449d4481892e799a0`, fileID `21300000`, sprite `Orc_Trailguard`.
- Extraction rectangle in PNG top-left coordinates: `[0, 0, 112, 88]` (right/bottom exclusive). Full expanded canvas; no scaling/trimming. Unity source-floor pivot: (56, 4).
- Current controller: `Assets/_Game/Animations/Forest/Orc_Trailguard.controller`.

### Pearl Thief

**Status:** PROPOSED addition — membership pending user review; broader art approval remains separate.

**Reference for:** Economical whimsical marine anatomy, large quiet head, tiny face and one bright treasure accent.

**Do not copy:** Not a purple palette, tentacles or a generic cute-face template for every creature.

- Current native image: [pearl_thief-still.png](EnemyCohesion2026-10/pearl_thief-still.png) (128×104).
- Extracted PNG SHA-256: `0ef10c489e66d242d5d2e083bf1ae9adb1d84fd25f053e0cea79aef7e0701d5c`.
- Production source: `Assets/_Game/Art/DrownedCourt/pearl_thief.png`; SHA-256 `c63c63ec82d9bdb2acb85280ad58237d6853065037a58a241ed6a9b3bc726229`.
- Unity GUID `ee4e03895a7a87449b53ae7f3e4a0752`, fileID `21300000`, sprite `pearl_thief`.
- Extraction rectangle in PNG top-left coordinates: `[0, 0, 128, 104]` (right/bottom exclusive). Full expanded canvas; no scaling/trimming. Unity source-floor pivot: (64, 4).
- Current controller: `Assets/_Game/Animations/DrownedCourt/pearl_thief.controller`.

### Elven Scout

**Status:** Specialist reserve — not proposed for core membership.

**Reference for:** Bowstring/hand relationship and readable lean archer silhouette.

**Do not copy:** Do not copy its wider idle movement to the whole cast; Mender already covers the core elf/cloth role.

- Current native image: [elven_scout-still.png](EnemyCohesion2026-10/elven_scout-still.png) (112×88).
- Extracted PNG SHA-256: `0ef5ab68f5f455c6b4ccee46bda19cc7be5b35d84a389930f26ae5980b8098ac`.
- Production source: `Assets/_Game/Art/Forest/Elven_Scout.png`; SHA-256 `937a7b3be1443365d2594e061ef374b0f283c6ef9430ccc9cdc5f9a470e0839e`.
- Unity GUID `e51d5254d34cf2140a590edb8774a2ac`, fileID `21300000`, sprite `Elven_Scout`.
- Extraction rectangle in PNG top-left coordinates: `[0, 0, 112, 88]` (right/bottom exclusive). Full expanded canvas; no scaling/trimming. Unity source-floor pivot: (56, 4).
- Current controller: `Assets/_Game/Animations/Forest/Elven_Scout.controller`.

## Approval provenance and historical references

- Farmer/Pan/Rattlebones historical working statuses, exact original hashes and later hand-adjusted animation decisions remain in sections 5, 11 and 14 of the canonical guide. The current reference authority uses the actual bound production versions shown here.
- Mender/Scout/Trailguard still identity approval is recorded in `Docs/Forest/ART_REVIEW.md` and the Forest approved manifests. It does not automatically approve all later animations or these proposed reference memberships.
- Royal Swordsman belongs to the accepted grounded cast family documented in `ArtSource/RemainingCast/SelectedIdles/README.md`; this audit proposes a specific additional reference role.
- Court and Ironvein source-selection/approval records retain their original distinctions. Do not infer approval of all Court art or proposed Pearl Thief membership from a merged implementation.
- Historical `references/characters/*.png` entries in the original guide are not present in this checkout. The historical composite exists and was inspected; its original provenance is retained. This register does not fabricate or silently recreate the missing source-pack files.
- Bardley and Gideon keep their existing approved/working identity authority. They are not replaced by the enemy shortlist. Bardley historical static placement notes must not be applied blindly to the current grounded animation.

## Future generation lineage

Follow the canonical guide’s actual-image input requirements. Use the referenced PNG bytes in a supported style/reference field; paths or textual mentions alone are insufficient. Record guide version/hash, image hashes, input roles, target approval, endpoint/settings/job lineage and output hashes. The approved target remains identity authority during animation. Proposed memberships above must remain marked pending until the user decides.

## 2026-10-11 production update

The user authorized cleanup, west-facing animation repairs, canvas expansion and integration. Current extracted images above follow those bound exports; original references and approval history remain in the immutable audit. This implementation does not approve the four proposed golden memberships. All player art, including Rattlebones, is byte-identical.
