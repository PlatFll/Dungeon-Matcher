# Dungeon Matcher: global art-direction and full cast audit

2026-10-11 · source baseline `10f667bd` (matched `origin/main` after fetch).
**Recommendations for review; production sprites have not been changed.**

The game already has a recognizable and appealing identity. Its best sprites
make a character readable through a face, one or two large body shapes and an
identifying prop. My recommendation is to bring the busiest assets back toward
that economy, while keeping the considerable variety already present. A global
redraw, universal palette reduction or pose replacement would discard good work.

Rattlebones remains the primary standard. His broad bone planes, tiny cyan eyes,
crown and red cape demonstrate a clear visual hierarchy. The target is the same
clarity in other materials and bodies, not more skeleton-like characters.

## Decisions proposed for review

1. Keep Rattlebones and most of the established Dungeon cast. Preserve approved
   identities throughout all zones. Use internal cleanup before redraw.
2. Add **Royal Swordsman, Elven Mender, Orc Trailguard and Pearl Thief** as four
   narrowly scoped supporting golden references. Retain Farmer and Pan Villager.
   Keep Elven Scout as a specialist reserve. These are membership proposals,
   not a record of new artwork or reference approval.
3. First correct **Puffer's inflated face**, **Snapvine's death progression**,
   **Lantern Warden's competing details**, and **Seismic Smith's beard/chest/hammer
   clutter**. These changes have clearer benefits than broadly reposing humans.
4. Then simplify Shield Knight's ornament, dense bark and royal trim, overlapping
   mining equipment, and excessive opaque action effects. Review each still
   before propagating it across its animation family.
5. Preserve the gem family. Quiet the busiest board surrounds and backgrounds
   instead of dulling the interactive pieces to match the scenery.

No full PixelLab regeneration is recommended. Therefore there are no regeneration
briefs to commission or generation costs in this audit. Localized redraws are
native edits to named regions/states, not a request to generate a new character.
If an approved native correction later fails, prepare an individual replacement
brief from that actor's evidence, protected landmarks and current image inputs;
do not treat that contingency as permission to generate now.

## Scoring rubric and evidence limits

The supplied pasted brief refers to a separately named full specification and
its detailed rubric/usage procedure, but only the pasted request was available.
This audit follows every concrete requirement in that supplied text and uses the
following disclosed rubric. Scores are art-direction judgments, not measured
similarity, mathematical quality or user approval. Half points express relative
judgment, not measurement precision.

- **Style /10:** contour and source-pixel discipline; connected shading; face and
  identifying-prop readability; material separation; controlled visual density
  relative to the actual golden family. Camera, species and canvas size do not
  automatically affect the score.
- **Theme /10:** whimsical dungeon fantasy, memorable personality, understandable
  equipment and appropriate zone/faction identity. An industrial suit or eel can
  score as well as a human when the design belongs in this world.
- **Animation style /10:** identity and material consistency through the inspected
  keys; grounded or anatomically appropriate contact; grip/prop continuity;
  readable preparation, action and recovery; restrained idle; distinct special,
  channel and alternate-state reads. This is a **source-frame/timing score**,
  not certification of live controller transitions or gameplay synchronization.

9–10 means a strong family fit worth preserving; 8–8.5 means good with localized
polish; 7–7.5 means a clear mismatch in a named area; 6–6.5 means a significant
readability/state issue requiring targeted work. Lower bands would justify
progressively larger redesigns, but none of this cast warrants full regeneration
on the evidence inspected. Rattlebones's 10s designate the chosen reference,
not an objective claim of flawless art.

**P1** = first correction batch; **P2** = second batch; **P3** = keep, optional
polish or low-priority conditional work. Priorities are art-production order,
not gameplay bug severities.

## Human stance assessment

The repeated compact ready stance mostly communicates a coherent combat game.
Farmer's diagonal pitchfork, Pan Villager's round low pan, Basket Villager's low
load, the Marshal's open arms, and the upright casters already form distinct
silhouettes. Soldiers' common footing communicates training. Bow users and
crossbow users should share plausible bracing rather than each receiving a novel
camera angle. Rattlebones, Bardley and Gideon already demonstrate very different
anatomy and posture within one rendering family.

Keep the common camera/pixel scale. Keep species proportions. Make optional pose
studies only for a named benefit: a slight load-bearing shoulder lean for Barricade
Villager, a shield-forward quarter turn for Shield Knight, or a small forearm
separation for Netweaver's net. None is essential before internal cleanup. A pose
change affects ready, idle and every action entry/exit; it is considerably more
work than simplifying a shield emblem. Prioritize personality during the action
poses, where the existing hammer, bell, pan, instrument and staff motions pay off.

## Per-zone comparison

| Zone | What works | Main correction direction |
|---|---|---|
| Dungeon | Strongest established shared rendering; readable locals and disciplined soldiers; excellent material hierarchy in Farmer/Pan/Royal Swordsman | Simplify Shield Knight heraldry, Arbalist's tiny reflections and selected gold trim; reduce giant pale sword arcs without changing event timing |
| Magical Forest | Mender/Scout/Trailguard bridge naturally into new species; good variety of plants, ritualists and apex silhouettes | Quiet bark, branch and robe texture; preserve Old Stump and Matriarch's approved anatomy; clarify Snapvine death; avoid turning all olive faces into dark detail clusters |
| Drowned Court | Strongest range of nonhuman shapes; Pearl Thief, Moray and Skittercrab retain charm with economical forms | Keep face landmarks during inflation; simplify Lantern Warden, shell shield subdivisions and fine net mesh; preserve the fan silhouettes and marine colors |
| Ironvein Excavation | Clear equipment-based roles; strong broad machine shapes; expressive Sapper, burdened Hauler and readable Turret | Consolidate beard/metal/strap planes; separate tool from hands and chest; reduce decorative bevels and noisy smoke, keeping native scale and industrial identity |

Ironvein is not uniformly the weakest cast. Turret's functional shapes and
Hauler's clear load read outperform several ornate sprites in other zones.
The relevant distinction is broad readable masses versus competing surface
detail, not human versus dwarf. Conversely, being an earlier approved humanoid
does not exempt Shield Knight or Arbalist from targeted criticism.

## Whole-game presentation recommendations

The four committed 1080×1920 UnifiedCombat captures were inspected as historical
composition evidence. They are test captures with simultaneous white hit flashes
and synthetic status displays; white silhouettes are **not missing sprite art**.
They cannot establish present live animation feel. The local Drowned Court throne
prefab has an unrelated uncommitted edit, so these captures do not establish its
current edited composition. All four images are included in the offline pack.

- **Gems:** keep existing colors and silhouettes. Their brighter faceted treatment
  has a different job from cloth/bone shading. Forest's amber log cells compete
  most with orange/yellow gems; lower cell-interior contrast and ring detail in a
  future approved background pass, leaving gem identity intact.
- **Battle scenery:** Dungeon's dark violet masonry is a useful hierarchy model.
  Keep focal space behind heads and counters quiet. Court's coral/architecture and
  Ironvein's rails/cart/ore compete more with silhouettes. Consolidate distant
  texture into larger dark shapes; preserve the zone landmarks and warm accents.
- **Player/HUD backing:** Court's bright repeated water streaks pull attention away
  from the player and controls. Reduce the backing's local contrast and frequency;
  retain marine colors. Keep UI frames recognizable across zones rather than
  prescribing a single texture for every zone.
- **Icons/effects:** status icons have more micro-detail than nearby large pixel
  numerals. Test simplified icon interiors at their actual displayed size. Keep
  shape-plus-color coding. Avoid adding outlines around every internal feature.
  Short hit flashes can stay; their duration/overlap needs live review, not a
  conclusion from these frozen test moments.
- **Player cast:** keep Bardley's broad friendly shape. Preserve Gideon's approved
  hat, lens, coat and cane; only optional interior-speck cleanup is suggested.
  His earlier rejected simplified redesign must not be revived under this audit.
- **Layout:** Ironvein's smaller board footprint and side/bottom drills are a layout
  review question, not evidence of wrong gem pixels. Compare on a phone before
  changing size. HP, shield and gameplay layout remain outside this art-only pass.

This is exhaustive for the bound character roster, not a claim to have examined
every loose UI, environment tile, effect, menu image or historical concept in the
repository. Non-character observations above are scoped to the named screen
evidence. No fresh device/runtime or complete non-character asset audit is claimed.

## Smallest safe production sequence

1. Review the four proposed golden additions and the priority list. The permanent
   guide now records the user-requested Rattlebones authority and actual-image
   reference workflow; candidate memberships remain explicitly pending.
2. Make one reviewed still/representative state correction for each P1 actor.
   For Puffer, show base → inflation → inflated hold → release; for Snapvine,
   show the complete death sequence. Show before/after at native 1× and integer
   zoom, beside untouched Rattlebones and the relevant supporting image.
3. After still/state approval, propagate the same material decisions through all
   named affected states. Preserve native drawn size, approved identity and fixed
   registration. No blanket color-count cap or automated nearest-palette remap.
4. Work through P2 character groups, then scenery/UI hierarchy. Keep low-priority
   optional reposes separate so they do not force unnecessary animation rework.
5. Validate any later imports/runtime changes under repository rules. Review real
   idle → action → idle transitions, channels, interrupts, powered state seams,
   deaths and full three-actor compositions on light/dark inspection stages and
   the actual zone. Finish with phone readability and human visual approval.

For every actor below, the state list names exactly what was inspected. An
all-state material change affects the fallback still and every listed state.
Targeted effect/death/state corrections affect the named states first, with an
entry/exit consistency review across the remainder. No production edits have
been authorized merely by these recommendations.

## Coverage, reproducibility and exclusions

- 62 current definitions: Dungeon 21, Forest 13, Court 14, Ironvein 14 (including
  Rivet Turret). Summons already represented by these definitions are not double
  counted as new characters. Database_Main alone is insufficient for later zones.
- Three current PlayerDefinitions; two serialized Grand Delver remount forms
  included separately. `mineEnableRemount: 0` keeps these alternates outside the
  default production encounter. Puffer inflation and mine ore-powered actions
  are included in their controllers' state lists.
- 361 bound states, 2,983 inspected drawing entries after consecutive duplicate
  sprite keys were combined into their true holds. A drawing entry is not a claim
  of a unique image. Every original key count, frame source, rectangle, exposure,
  state speed, loop flag and event name is recorded in `bindings.json`.
- Binding chain: definition → selected controller → AnimatorState motion → clip
  sprite GUID/fileID → imported sprite rectangle. All references resolved; no
  filename-based guessing or arbitrary first-cell slicing was used.
- Every actor's fallback/ready image and every bound drawing row was visually
  inspected. All frames are available in labeled light/dark contact sheets and
  the gallery. Large sheets may be scaled by an image viewer; the gallery offers
  exact native-size and integer-zoom frame inspection.
- Serialized exposure tables were read. The gallery reconstructs source playback
  at those timings; it is not Unity's Animator, does not reproduce controller
  transitions, runtime time scaling, overlays, tint flashes, board VFX or shaders.
  No claim is made that every state was watched in real-time playback.
- The Dungeon controllers have no separate Hit/Death sprite states. Their absence
  is recorded, not hidden or scored as a missing full animation set. Shared runtime
  feedback includes `EnemyCombatFeedback` white-flash material; fresh inspection
  of death lifecycle and actual feedback playback remains a runtime follow-up.
- Old galleries, unbound candidates and rejected versions are not current cast.
  The historical Minotaur source is not among the current 62 definitions and is
  excluded from the production score table. No rejected source was promoted.
- Source hashes allow stale evidence to be detected. `build_review.py` only reads
  Assets and writes inspection copies here. `package_review.py` combines the
  authored judgments, guide/register copies and gallery into the portable ZIP.
  These are audit tools, not a second art authoring or gameplay system.

No Unity validator was run: this delivery changes documentation and inspection
artifacts only. No C#, Unity serialization, production sprite, gameplay or audio
file was edited. No PixelLab request or generation was made. Existing unrelated
local changes were preserved and excluded from the audit commit/PR.

## Individual assessments

The generated report continues below with every actor's scores, source bindings,
stance decision, preservation constraints, motion notes and golden-reference
recommendation. Scores remain proposed professional judgments for the user's review.


### Drowned Court

| Character | Style | Theme | Animation style | Priority | Intervention |
|---|---:|---:|---:|---|---|
| [Breakwater Captain](Review.html#breakwater_captain) | 8 | 10 | 8.5 | P2 | Internal shading/palette cleanup |
| [Conch Marshal](Review.html#conch_marshal) | 8 | 10 | 8.5 | P2 | Internal shading/palette cleanup |
| [Hammerhead Bruiser](Review.html#hammerhead_bruiser) | 8.5 | 10 | 8.5 | P3 | Internal shading/palette cleanup |
| [Lantern Warden](Review.html#lantern_warden) | 7 | 10 | 8 | P1 | Partial redraw/simplification |
| [Moray Siphoner](Review.html#moray_siphoner) | 9 | 10 | 8.5 | P3 | Keep |
| [Needlefin Skirmisher](Review.html#needlefin_skirmisher) | 8.5 | 10 | 8.5 | P3 | Keep |
| [Pearl Cantor](Review.html#pearl_cantor) | 8.5 | 10 | 8.5 | P3 | Internal shading/palette cleanup |
| [Pearl Thief](Review.html#pearl_thief) | 9.5 | 10 | 9 | P3 | Keep |
| [Puffer Sentinel](Review.html#puffer_sentinel) | 8.5 | 10 | 6.5 | P1 | Partial redraw/simplification |
| [Queen Nacre](Review.html#queen_nacre) | 8 | 10 | 8.5 | P2 | Internal shading/palette cleanup |
| [Reef Netweaver](Review.html#reef_netweaver) | 8.5 | 10 | 8.5 | P2 | Internal shading/palette cleanup |
| [Reef Spearman](Review.html#reef_spearman) | 9 | 10 | 8.5 | P3 | Keep |
| [Shellback Porter](Review.html#shellback_porter) | 8 | 10 | 8.5 | P2 | Internal shading/palette cleanup |
| [Skittercrab](Review.html#skittercrab) | 9 | 10 | 8.5 | P3 | Keep |

#### Breakwater Captain

**Style 8/10 · Theme 10/10 · Animation style 8.5/10 · P2**

**Internal shading/palette cleanup.** Shark head, coral trident and shell shield provide strong identity. Shield's many bright checkered segments pull attention from the face; use broader shell planes.

**Preserve:** Shark profile, shield silhouette, red sash, trident and broad captain stance.

**Stance decision:** Keep shield-bearing authority; it need not resemble Shield Knight's anatomy.

**Animation evidence/correction:** Raised shield and channel hold are legible. Apply shell cleanup across every angle; preserve the trident's clean thrust.

**Affected states:** Fallback/ready still and all bound states: Interrupt, AutoAttack, Ability, Hit, Death, Release, Idle, ChannelStart, ChannelHold. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Role/silhouette reference only; not core rendering.

**Inspected:** fallback/ready plus Interrupt, AutoAttack, Ability, Hit, Death, Release, Idle, ChannelStart, ChannelHold.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/breakwater_captain.asset` → `Assets/_Game/Animations/DrownedCourt/breakwater_captain.controller`. [All bound drawings](boards/breakwater_captain-frames.png) · [Native ready image](media/breakwater_captain-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Conch Marshal

**Style 8/10 · Theme 10/10 · Animation style 8.5/10 · P2**

**Internal shading/palette cleanup.** Red/cream crest and conch are distinctive but share many alternating small stripes. Quiet lower costume marks and keep the conch opening clear against the head.

**Preserve:** Fan crest, conch instrument, coral-red palette and narrow body.

**Stance decision:** Keep upright horn-player stance; do not overpose the legs.

**Animation evidence/correction:** ChannelStart brings conch to mouth convincingly. Preserve mouth contact and head shape through Release/Interrupt.

**Affected states:** Fallback/ready still and all bound states: Interrupt, ChannelStart, Idle, Death, Release, AutoAttack, ChannelHold, Hit, Ability. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist musical signaling, not core.

**Inspected:** fallback/ready plus Interrupt, ChannelStart, Idle, Death, Release, AutoAttack, ChannelHold, Hit, Ability.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/conch_marshal.asset` → `Assets/_Game/Animations/DrownedCourt/conch_marshal.controller`. [All bound drawings](boards/conch_marshal-frames.png) · [Native ready image](media/conch_marshal-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Hammerhead Bruiser

**Style 8.5/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Hammer-shaped head and compact mallet give an immediate chunky silhouette. Reduce tiny leather-buckle and abdominal contour noise.

**Preserve:** Distinct wide head, pale throat, blue-gray skin, mallet and broad body.

**Stance decision:** Keep the grounded heavy stance; species shape already supplies variety.

**Animation evidence/correction:** Overhead mallet swing is readable; no need to inflate idle motion or add broad flashes.

**Affected states:** Fallback/ready still and all bound states: Hit, Death, AutoAttack, Idle. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Potential heavy marine reference, but Pearl Thief adds a more economical nonhuman core example.

**Inspected:** fallback/ready plus Hit, Death, AutoAttack, Idle.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/hammerhead_bruiser.asset` → `Assets/_Game/Animations/DrownedCourt/hammerhead_bruiser.controller`. [All bound drawings](boards/hammerhead_bruiser-frames.png) · [Native ready image](media/hammerhead_bruiser-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Lantern Warden

**Style 7/10 · Theme 10/10 · Animation style 8/10 · P1**

**Partial redraw/simplification.** Lure, toothed face, spined back, shell disks and carried coffer all compete. Preserve all major identity parts but simplify shell interiors, dorsal ornaments and coffer trim; make lure/face the first read.

**Preserve:** Angler lure, purple body, coffer, recognizable teeth and bulky bent silhouette.

**Stance decision:** Keep hunched coffer carrier; reposition the coffer slightly only if hand/face separation remains poor after internal cleanup.

**Animation evidence/correction:** Coffer opens clearly in ChannelHold. Preserve hinge/grip across opening and attack; avoid independent ornament flicker.

**Affected states:** Fallback/ready still and all bound states: Idle, Interrupt, ChannelHold, ChannelStart, Hit, Death, Release, AutoAttack, Ability. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; the main Court density outlier.

**Inspected:** fallback/ready plus Idle, Interrupt, ChannelHold, ChannelStart, Hit, Death, Release, AutoAttack, Ability.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/lantern_warden.asset` → `Assets/_Game/Animations/DrownedCourt/lantern_warden.controller`. [All bound drawings](boards/lantern_warden-frames.png) · [Native ready image](media/lantern_warden-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Moray Siphoner

**Style 9/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Keep.** Long pale throat, green coiled body and coral staff create clear large shapes. Good proof that a nonhuman silhouette need not be heavily textured.

**Preserve:** Eel head, pale underside, curled floor contact, burgundy cloth and coral staff.

**Stance decision:** Keep the coiled upright serpent pose.

**Animation evidence/correction:** Body flex and mouth opening are coherent; ensure future changes retain the floor contact and do not turn the idle into floating.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Hit, ChannelHold, Death, AutoAttack, ChannelStart, Release, Ability, Idle, Interrupt.

**Golden-reference recommendation:** Strong reserve nonhuman reference; core shortlist kept smaller with Pearl Thief.

**Inspected:** fallback/ready plus Hit, ChannelHold, Death, AutoAttack, ChannelStart, Release, Ability, Idle, Interrupt.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/moray_siphoner.asset` → `Assets/_Game/Animations/DrownedCourt/moray_siphoner.controller`. [All bound drawings](boards/moray_siphoner-frames.png) · [Native ready image](media/moray_siphoner-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Needlefin Skirmisher

**Style 8.5/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Keep.** Long snout, sharp fins and spare equipment create a fast, recognizable silhouette. Cool planes remain mostly connected.

**Preserve:** Needle snout, small orange fin accents, narrow legs and blue-gray skin.

**Stance decision:** Keep the light forward stance; this is appropriate anatomy, not off-style camera work.

**Animation evidence/correction:** Forward attack extension reads well; preserve snout length and ankle contacts.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Hit, Death, Idle, AutoAttack.

**Golden-reference recommendation:** Specialist agile marine body; not core.

**Inspected:** fallback/ready plus Hit, Death, Idle, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/needlefin_skirmisher.asset` → `Assets/_Game/Animations/DrownedCourt/needlefin_skirmisher.controller`. [All bound drawings](boards/needlefin_skirmisher-frames.png) · [Native ready image](media/needlefin_skirmisher-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Pearl Cantor

**Style 8.5/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Golden seahorse head and dark teal robe are a strong two-mass design. Thin bright robe piping can be consolidated around the central fold.

**Preserve:** Seahorse snout, curled tail, crook staff, pale head and teal robe.

**Stance decision:** Keep the stately vertical caster; it contrasts with crawling/coiled Court enemies.

**Animation evidence/correction:** Curled appendage lifts distinctly in casting; preserve its identity through channel and collapse.

**Affected states:** Fallback/ready still and all bound states: Ability, ChannelStart, Death, Release, Interrupt, ChannelHold, Idle, AutoAttack, Hit. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Strong caster reserve; Mender fills core cloth and Pearl Thief fills marine simplicity.

**Inspected:** fallback/ready plus Ability, ChannelStart, Death, Release, Interrupt, ChannelHold, Idle, AutoAttack, Hit.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/pearl_cantor.asset` → `Assets/_Game/Animations/DrownedCourt/pearl_cantor.controller`. [All bound drawings](boards/pearl_cantor-frames.png) · [Native ready image](media/pearl_cantor-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Pearl Thief

**Style 9.5/10 · Theme 10/10 · Animation style 9/10 · P3**

**Keep.** Large quiet purple head, tiny mischievous face, bright pearl and curling limbs feel closest to the playful monster identity. Very little decoration is wasted.

**Preserve:** Head silhouette, minimal face, pearl/coffer relationship, hook and tentacle shapes.

**Stance decision:** Keep low asymmetric tentacle stance; do not add humanoid legs or a skeleton face.

**Animation evidence/correction:** Casting and death retain the same identity; little accents remain subordinate to the large head.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Death, Release, Hit, Ability, Idle, Interrupt, AutoAttack, ChannelStart, ChannelHold.

**Golden-reference recommendation:** PROPOSE core addition for whimsical marine/nonhuman simplicity; do not copy purple color, eye placement or tentacles globally. Membership and broader artwork approval remain separate.

**Inspected:** fallback/ready plus Death, Release, Hit, Ability, Idle, Interrupt, AutoAttack, ChannelStart, ChannelHold.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/pearl_thief.asset` → `Assets/_Game/Animations/DrownedCourt/pearl_thief.controller`. [All bound drawings](boards/pearl_thief-frames.png) · [Native ready image](media/pearl_thief-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Puffer Sentinel

**Style 8.5/10 · Theme 10/10 · Animation style 6.5/10 · P1**

**Partial redraw/simplification.** Base pose is appealing, but inflation becomes a dominant pale disk and the face nearly disappears into its upper rim. Restore a clearly readable eye/mouth cluster and dark contour separation while retaining the expanded silhouette.

**Preserve:** Orange/cream puffer identity, spines, staff, normal face, deliberate inflated size and gameplay meaning.

**Stance decision:** Keep base stance and inflated round body; do not replace the intentional state change with ordinary breathing.

**Animation evidence/correction:** Prioritize InflatedIdle, InflatedAttack, ChannelStart/Hold, Ability, Release and Interrupt. Keep face registration through growth/shrink; do not make it jump between unrelated positions.

**Affected states:** InflatedIdle, InflatedAttack, ChannelStart, ChannelHold, Ability, Release and Interrupt. Keep the normal still; verify normal/inflated entry and exit against Idle, AutoAttack, Hit and Death.

**Golden-reference recommendation:** Not core until alternate-state identity reads clearly.

**Inspected:** fallback/ready plus Idle, Release, InflatedAttack, Death, Interrupt, Ability, InflatedIdle, AutoAttack, Hit, ChannelHold, ChannelStart.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/puffer_sentinel.asset` → `Assets/_Game/Animations/DrownedCourt/puffer_sentinel.controller`. [All bound drawings](boards/puffer_sentinel-frames.png) · [Native ready image](media/puffer_sentinel-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Queen Nacre

**Style 8/10 · Theme 10/10 · Animation style 8.5/10 · P2**

**Internal shading/palette cleanup.** Fan crown/fins, trident and wine-red gown establish a marine queen. Fine gold edges and repeated fin stripes compete with the small face; reduce lower-gown trim before touching the iconic crown.

**Preserve:** Pale fish face, royal fan silhouette, trident, red/cream/gold scheme and tall authority.

**Stance decision:** Keep upright courtly stance; crown/fan already creates a distinct apex silhouette.

**Animation evidence/correction:** DepthsRelease and thrust keep the trident readable. Carry cleaner robe/fins through channel turns and prone Death.

**Affected states:** Fallback/ready still and all bound states: DepthsRelease, ChannelHold, Ability, Hit, Idle, Death, AutoAttack, ChannelStart, Interrupt, Release. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Apex marine silhouette reference; not global detail standard.

**Inspected:** fallback/ready plus DepthsRelease, ChannelHold, Ability, Hit, Idle, Death, AutoAttack, ChannelStart, Interrupt, Release.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/queen_nacre.asset` → `Assets/_Game/Animations/DrownedCourt/queen_nacre.controller`. [All bound drawings](boards/queen_nacre-frames.png) · [Native ready image](media/queen_nacre-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Reef Netweaver

**Style 8.5/10 · Theme 10/10 · Animation style 8.5/10 · P2**

**Internal shading/palette cleanup.** Simple shark body is strong; numerous tiny net knots are the noisy component. Use fewer readable mesh cells and a clear top grip.

**Preserve:** Shark head, carried hanging net, pale throat and slim blue-gray body.

**Stance decision:** Keep net arm slightly clear of torso; a local forearm separation is enough if needed, not a whole repose.

**Animation evidence/correction:** Channel extension and retrieval are readable; simplified mesh must maintain consistent knot topology between frames.

**Affected states:** Fallback/ready still and all bound states: Death, Idle, Interrupt, Hit, ChannelStart, ChannelHold, Ability, AutoAttack, Release. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist net/prop example after cleanup; not core.

**Inspected:** fallback/ready plus Death, Idle, Interrupt, Hit, ChannelStart, ChannelHold, Ability, AutoAttack, Release.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/reef_netweaver.asset` → `Assets/_Game/Animations/DrownedCourt/reef_netweaver.controller`. [All bound drawings](boards/reef_netweaver-frames.png) · [Native ready image](media/reef_netweaver-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Reef Spearman

**Style 9/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Keep.** Orange crest against green skin and burgundy cloth creates a readable ordinary marine soldier. Equipment is economical.

**Preserve:** Crest, fish face, coral trident and slim stance.

**Stance decision:** Keep modest diagonal spear stance; faction and anatomy do the differentiating.

**Animation evidence/correction:** Long thrust stays attached to the hands and returns to the same ready pose.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Death, Hit, AutoAttack, Idle.

**Golden-reference recommendation:** Good marine infantry reserve; not core expansion.

**Inspected:** fallback/ready plus Death, Hit, AutoAttack, Idle.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/reef_spearman.asset` → `Assets/_Game/Animations/DrownedCourt/reef_spearman.controller`. [All bound drawings](boards/reef_spearman-frames.png) · [Native ready image](media/reef_spearman-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Shellback Porter

**Style 8/10 · Theme 10/10 · Animation style 8.5/10 · P2**

**Internal shading/palette cleanup.** Big shell is an excellent load-bearing silhouette; near-equal warm tones in shell, arm and face reduce separation. Darken overlap divisions and simplify repeated shell highlights.

**Preserve:** Shell dome, forward face, mallet, orange shell ramp and bowed body.

**Stance decision:** Keep the forward load-bearing lean; one of the most purposeful poses.

**Animation evidence/correction:** Shell and body stay coherent through swing and fall. Preserve shell mass instead of smoothing it into a backpack.

**Affected states:** Fallback/ready still and all bound states: AutoAttack, Hit, Idle, Death. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist shell/material reference after cleanup; not core.

**Inspected:** fallback/ready plus AutoAttack, Hit, Idle, Death.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/shellback_porter.asset` → `Assets/_Game/Animations/DrownedCourt/shellback_porter.controller`. [All bound drawings](boards/shellback_porter-frames.png) · [Native ready image](media/shellback_porter-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Skittercrab

**Style 9/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Keep.** Broad claws and small low body read instantly; warm shell planes are connected and playful.

**Preserve:** Low footprint, two clear claws, eye stalks and sand/coral colors.

**Stance decision:** Keep the low crab stance; never force it onto the humanoid floor silhouette.

**Animation evidence/correction:** Claw attack is readable; final Death is subtle, so a more visibly lowered claw is an optional later terminal-pose polish.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Death, AutoAttack, Hit, Idle.

**Golden-reference recommendation:** Strong reserve small-creature reference; no need to enlarge the core set further.

**Inspected:** fallback/ready plus Death, AutoAttack, Hit, Idle.

**Binding:** `Assets/_Game/Data/Enemies/DrownedCourt/skittercrab.asset` → `Assets/_Game/Animations/DrownedCourt/skittercrab.controller`. [All bound drawings](boards/skittercrab-frames.png) · [Native ready image](media/skittercrab-still.png). Exact clip/sprite paths and hashes: `bindings.json`.


### Dungeon

| Character | Style | Theme | Animation style | Priority | Intervention |
|---|---:|---:|---:|---|---|
| [Barricade Guard](Review.html#barricade_guard) | 8.5 | 9 | 8.5 | P3 | Internal shading/palette cleanup |
| [Barricade Villager](Review.html#barricade_villager) | 9 | 10 | 9 | P3 | Internal shading/palette cleanup |
| [Basket Villager](Review.html#basket_villager) | 9.5 | 10 | 9 | P3 | Keep |
| [Court Mage](Review.html#court_mage) | 8.5 | 9.5 | 8 | P2 | Internal shading/palette cleanup |
| [Crossbow Guard](Review.html#crossbow_guard) | 9 | 9.5 | 9 | P3 | Keep |
| [Farmer](Review.html#farmer) | 9.5 | 10 | 9.5 | P3 | Keep |
| [The King](Review.html#king) | 8.5 | 10 | 8 | P2 | Internal shading/palette cleanup |
| [Sword Knight](Review.html#knight) | 9 | 9.5 | 9 | P3 | Keep |
| [Knight Captain](Review.html#knight_captain) | 9 | 9.5 | 8.5 | P3 | Keep |
| [Miner](Review.html#miner) | 9 | 10 | 8.5 | P3 | Keep |
| [Pan Villager](Review.html#pan_villager) | 9.5 | 10 | 9.5 | P3 | Keep |
| [Royal Arbalist](Review.html#royal_arbalist) | 8 | 9.5 | 8.5 | P2 | Internal shading/palette cleanup |
| [The Minister](Review.html#royal_arcanist) | 9 | 10 | 8.5 | P3 | Internal shading/palette cleanup |
| [Royal Lancer](Review.html#royal_lancer) | 8.5 | 9.5 | 8.5 | P3 | Internal shading/palette cleanup |
| [Royal Standard Bearer](Review.html#royal_standard_bearer) | 8.5 | 10 | 8.5 | P3 | Internal shading/palette cleanup |
| [Royal Swordsman](Review.html#royal_swordsman) | 9.5 | 9.5 | 9 | P3 | Keep |
| [Shield Knight](Review.html#shield_knight) | 7.5 | 9.5 | 8.5 | P2 | Partial redraw/simplification |
| [Siege Sergeant](Review.html#siege_sergeant) | 8.5 | 9.5 | 8.5 | P3 | Internal shading/palette cleanup |
| [Spear Guard](Review.html#spear_guard) | 9 | 9.5 | 9 | P3 | Keep |
| [Spear Knight](Review.html#spear_knight) | 8 | 9.5 | 8.5 | P3 | Internal shading/palette cleanup |
| [Town Marshal](Review.html#town_marshal) | 9 | 10 | 9 | P3 | Keep |

#### Barricade Guard

**Style 8.5/10 · Theme 9/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Helmet silhouette and hammer read immediately; remove a few scattered visor/helmet specks and consolidate the shield's small metal patches.

**Preserve:** Closed helmet, short hammer, small round shield, red cloth and compact scale.

**Stance decision:** Keep the compact defensive stance; equipment already distinguishes him from villagers.

**Animation evidence/correction:** Ground-strike Ability frames 6-12 are busy at the feet; retain contact but reduce loose sparks if polishing.

**Affected states:** Fallback/ready still and all bound states: Idle, AutoAttack, Ability. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Useful secondary dull-iron reference; not a core addition.

**Inspected:** fallback/ready plus Idle, AutoAttack, Ability.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_BarricadeGuard.asset` → `Assets/_Game/Animations/CombatIdles/BarricadeGuard_Idle.controller`. [All bound drawings](boards/barricade_guard-frames.png) · [Native ready image](media/barricade_guard-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Barricade Villager

**Style 9/10 · Theme 10/10 · Animation style 9/10 · P3**

**Internal shading/palette cleanup.** Face and cap are clean; the carried timber's many bright parallel cuts compete with the face. Combine internal log highlights while retaining log-end separation.

**Preserve:** Cap, axe, armful of logs, rustic palette, original face and ready silhouette.

**Stance decision:** Keep now. A slight load-bearing shoulder lean is an optional later pose study, not required for cohesion.

**Animation evidence/correction:** Kneeling Ability is characterful and clearly separate from the axe attack; preserve folded leg and hand/tool contact.

**Affected states:** Fallback/ready still and all bound states: Ability, Idle, AutoAttack. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Local workwear/prop reference only; Farmer already fills the core rustic role.

**Inspected:** fallback/ready plus Ability, Idle, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_BarricadeVillager.asset` → `Assets/_Game/Animations/CombatIdles/BarricadeVillager_Idle.controller`. [All bound drawings](boards/barricade_villager-frames.png) · [Native ready image](media/barricade_villager-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Basket Villager

**Style 9.5/10 · Theme 10/10 · Animation style 9/10 · P3**

**Keep.** Broad hat, small warm face and low basket communicate an ordinary local instantly. Berry accent is restrained.

**Preserve:** Hat, basket volume, subdued clothing, berry cluster and hand-adjusted family style.

**Stance decision:** Keep; lowered basket already differentiates this silhouette from the pitchfork Farmer.

**Animation evidence/correction:** Keep the compact berry gather/release and rigid basket; do not add a noisy projectile trail.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Idle, AutoAttack.

**Golden-reference recommendation:** Strong supporting villager reference; redundant with Farmer for core membership.

**Inspected:** fallback/ready plus Idle, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_BasketVillager.asset` → `Assets/_Game/Animations/CombatIdles/BasketVillager_Idle.controller`. [All bound drawings](boards/basket_villager-frames.png) · [Native ready image](media/basket_villager-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Court Mage

**Style 8.5/10 · Theme 9.5/10 · Animation style 8/10 · P2**

**Internal shading/palette cleanup.** Pale hood and cyan staff have a clear hierarchy; gold trim and bright garment edges could use fewer competing interior accents.

**Preserve:** Pale hood, visible bearded face, cyan crystal staff, red lower fabric.

**Stance decision:** Keep the upright caster; no need to copy the Mender's open palm.

**Animation evidence/correction:** Ability frames 6-11 place cyan streaks across the torso and face; narrow their coverage while keeping the release cue.

**Affected states:** Fallback/ready still and all bound states: AutoAttack, Ability, Idle. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist cloth/magic reference after effects cleanup; not core.

**Inspected:** fallback/ready plus AutoAttack, Ability, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_CourtMage.asset` → `Assets/_Game/Animations/CombatIdles/RoyalMage_Idle.controller`. [All bound drawings](boards/court_mage-frames.png) · [Native ready image](media/court_mage-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Crossbow Guard

**Style 9/10 · Theme 9.5/10 · Animation style 9/10 · P3**

**Keep.** Kettle helmet, open face and horizontal crossbow form three clean masses. The wood/string contour remains readable across the poses.

**Preserve:** Complete bow/string, open face, helmet and blue-gray uniform.

**Stance decision:** Keep the two-handed low guard; shared ranged discipline is appropriate.

**Animation evidence/correction:** Retain the restrained idle and visible weapon recoil; preserve the repaired lower bow edge.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Idle, AutoAttack, Ability.

**Golden-reference recommendation:** Supporting rigid-prop reference; not an additional core anchor.

**Inspected:** fallback/ready plus Idle, AutoAttack, Ability.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_CrossbowGuard.asset` → `Assets/_Game/Animations/CombatIdles/CrossbowGuard_Idle.controller`. [All bound drawings](boards/crossbow_guard-frames.png) · [Native ready image](media/crossbow_guard-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Farmer

**Style 9.5/10 · Theme 10/10 · Animation style 9.5/10 · P3**

**Keep.** Current ready pose has a large straw shape, clean face and one clear diagonal tool; cloth stays subordinate. An excellent expression of the game's ordinary-person charm.

**Preserve:** User-adjusted face/idle, straw hat, exact pitchfork/prongs, skin and rustic materials.

**Stance decision:** Keep. The diagonal fork already supplies motion and personality.

**Animation evidence/correction:** Nine 130 ms idle exposures and compact 680 ms thrust sit naturally beside Rattlebones.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Idle, AutoAttack.

**Golden-reference recommendation:** Retain existing supporting reference; formalize rustic clothing/skin and improvised equipment, not universal anatomy.

**Inspected:** fallback/ready plus Idle, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_Farmer.asset` → `Assets/_Game/Animations/CombatIdles/Farmer_Idle.controller`. [All bound drawings](boards/farmer-frames.png) · [Native ready image](media/farmer-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### The King

**Style 8.5/10 · Theme 10/10 · Animation style 8/10 · P2**

**Internal shading/palette cleanup.** Crown, beard, red cape and broad armor establish authority; small gold/white plate accents compete at the waist. Merge minor accents without reducing rank.

**Preserve:** Crown, gray beard, heavy body, royal red/gold, large sword and approved two-handed Bombardment.

**Stance decision:** Keep the upright planted authority; no exaggerated crouch.

**Animation evidence/correction:** AutoAttack/JudgmentStrike1 frame 5 and Strike2 frame 4 have large pale arcs. Keep force but open negative space around the head; preserve attack timing and both-hand contact.

**Affected states:** Fallback/ready still and all bound states: JudgmentStrike1, Bombardment, BombardmentRaise, AutoAttack, JudgmentStrike2, BombardmentReady, RoyalCommand, JudgmentFinisher, Idle. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Boss hierarchy reference, not the universal detail budget or attack-flash standard.

**Inspected:** fallback/ready plus JudgmentStrike1, Bombardment, BombardmentRaise, AutoAttack, JudgmentStrike2, BombardmentReady, RoyalCommand, JudgmentFinisher, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_King.asset` → `Assets/_Game/Animations/CombatIdles/King_Idle.controller`. [All bound drawings](boards/king-frames.png) · [Native ready image](media/king-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Sword Knight

**Style 9/10 · Theme 9.5/10 · Animation style 9/10 · P3**

**Keep.** Large subdued helmet and red tabard are clear; sword identifies the role with very little texture.

**Preserve:** Closed rounded helmet, simple plates, tabard and short sword.

**Stance decision:** Keep the disciplined stance; the simpler silhouette usefully precedes royal troops.

**Animation evidence/correction:** Compact slash and return remain consistent with the restrained idle.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Idle, AutoAttack.

**Golden-reference recommendation:** Supporting common-armor example; Royal Swordsman is a more useful core royal example.

**Inspected:** fallback/ready plus Idle, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_Knight.asset` → `Assets/_Game/Animations/CombatIdles/SwordKnight_Idle.controller`. [All bound drawings](boards/knight-frames.png) · [Native ready image](media/knight-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Knight Captain

**Style 9/10 · Theme 9.5/10 · Animation style 8.5/10 · P3**

**Keep.** Exposed face and tall plume distinguish the captain without excessive ornament. Face remains a useful focal patch.

**Preserve:** Visible face, gray plume, red cape and sword.

**Stance decision:** Keep; face and plume already distinguish leadership. A commanding off-hand gesture can stay in Ability.

**Animation evidence/correction:** AutoAttack frame 5 has a broad bright arc; slight thinning is optional, not grounds to redraw the character.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Idle, AutoAttack, Ability.

**Golden-reference recommendation:** Strong face/helmet combination; secondary only.

**Inspected:** fallback/ready plus Idle, AutoAttack, Ability.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_KnightCaptain.asset` → `Assets/_Game/Animations/CombatIdles/KnightCaptain_Idle.controller`. [All bound drawings](boards/knight_captain-frames.png) · [Native ready image](media/knight_captain-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Miner

**Style 9/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Keep.** Cap candle, readable pick and simple face are a charming direct bridge to the mining zone; this is a better material anchor than highly textured fantasy miners.

**Preserve:** Candle attachment, cap, pick, warm face, existing 96x80 canvas and drawn scale.

**Stance decision:** Keep the modest worker stance; do not imitate the dwarves' broader anatomy.

**Animation evidence/correction:** Preserve the intentional airborne Ability frame 4. Attack/Ability eyes become rounder; keep this as acting, avoiding further facial inflation.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Ability, Idle, AutoAttack.

**Golden-reference recommendation:** Supporting mine-material reference, not a requirement that dwarves adopt human proportions.

**Inspected:** fallback/ready plus Ability, Idle, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_Miner.asset` → `Assets/_Game/Animations/CombatIdles/Miner_Idle.controller`. [All bound drawings](boards/miner-frames.png) · [Native ready image](media/miner-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Pan Villager

**Style 9.5/10 · Theme 10/10 · Animation style 9.5/10 · P3**

**Keep.** Distinct headscarf, simple expression and one large cool pan shape work beautifully. Existing flat pan center is readable cookware.

**Preserve:** Original face and female identity, scarf shape, pan size/grip, iron ramp and planted boots.

**Stance decision:** Keep. The round low pan makes the silhouette distinct without an unusual pose.

**Animation evidence/correction:** Overhead attack carries the pan with the hand and returns cleanly; retain scarf restraint.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Idle, AutoAttack.

**Golden-reference recommendation:** Retain existing supporting reference for cloth, faces and dull metal; never copy the pan as a shield.

**Inspected:** fallback/ready plus Idle, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_PanVillager.asset` → `Assets/_Game/Animations/CombatIdles/PanVillager_Idle.controller`. [All bound drawings](boards/pan_villager-frames.png) · [Native ready image](media/pan_villager-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Royal Arbalist

**Style 8/10 · Theme 9.5/10 · Animation style 8.5/10 · P2**

**Internal shading/palette cleanup.** White armor, visor bars, plume, crossbow and gold highlights create more small bright fragments than Royal Swordsman. Simplify helmet/gauntlet interior reflections first.

**Preserve:** Enlarged approved helmet, red plume, full crossbow/string and royal identity.

**Stance decision:** Keep the ranged stance; it need not be dramatically different from Crossbow Guard.

**Animation evidence/correction:** Recoil remains clear; apply material cleanup consistently across Idle, AutoAttack and Ability, preserving the lower bow contour.

**Affected states:** Fallback/ready still and all bound states: AutoAttack, Idle, Ability. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; Royal Swordsman demonstrates royal armor more cleanly.

**Inspected:** fallback/ready plus AutoAttack, Idle, Ability.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_RoyalArbalist.asset` → `Assets/_Game/Animations/CombatIdles/RoyalArbalist_Idle.controller`. [All bound drawings](boards/royal_arbalist-frames.png) · [Native ready image](media/royal_arbalist-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### The Minister

**Style 9/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** The Minister's warm face, red robe and single gold staff read well. Keep robe highlights subordinate to face and staff.

**Preserve:** Current display name, existing face/hair, red/cream vestment and gold staff.

**Stance decision:** Keep the calm upright official's stance; expressive hand belongs in casting.

**Animation evidence/correction:** Restoration and Benediction share a strong gesture with distinct green/gold effects. Reduce overlapping rings only if they obscure hand/staff.

**Affected states:** Fallback/ready still and all bound states: AutoAttack, Ability, Idle, Benediction. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Secondary human caster; Mender adds more complementary simplicity to the core set.

**Inspected:** fallback/ready plus AutoAttack, Ability, Idle, Benediction.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_RoyalArchbishop.asset` → `Assets/_Game/Animations/CombatIdles/RoyalArcanist_Idle.controller`. [All bound drawings](boards/royal_arcanist-frames.png) · [Native ready image](media/royal_arcanist-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Royal Lancer

**Style 8.5/10 · Theme 9.5/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Strong lance and white helmet read clearly, but segmented shin/forearm highlights are busier than the torso. Merge small reflections.

**Preserve:** Enlarged approved helmet, lance length, plume and compact royal body.

**Stance decision:** Keep upright reach specialist. Optional small shoulder turn only if side-by-side playtests confuse him with Spear Knight.

**Animation evidence/correction:** Wide attack canvas is necessary clearance, not a smaller character. Preserve full reach, contact and recovery.

**Affected states:** Fallback/ready still and all bound states: AutoAttack, Idle. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist reach/weapon reference; not core.

**Inspected:** fallback/ready plus AutoAttack, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_RoyalLancer.asset` → `Assets/_Game/Animations/CombatIdles/RoyalLancer_Idle.controller`. [All bound drawings](boards/royal_lancer-frames.png) · [Native ready image](media/royal_lancer-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Royal Standard Bearer

**Style 8.5/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Banner gives an excellent role silhouette. Tiny armor spots and banner embroidery should remain less prominent than the pole, face plate and cloth triangle.

**Preserve:** Banner emblem/shape, tall pole, white helmet, red cape and faction colors.

**Stance decision:** Keep the vertical standard; do not add a wide combat pose just for variety.

**Animation evidence/correction:** The flag's controlled tilt is readable; keep pole rigid and grip connected through Ability.

**Affected states:** Fallback/ready still and all bound states: AutoAttack, Idle, Ability. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist banner/cloth reference, not a general stance model.

**Inspected:** fallback/ready plus AutoAttack, Idle, Ability.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_RoyalStandardBearer.asset` → `Assets/_Game/Animations/CombatIdles/RoyalStandardBearer_Idle.controller`. [All bound drawings](boards/royal_standard_bearer-frames.png) · [Native ready image](media/royal_standard_bearer-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Royal Swordsman

**Style 9.5/10 · Theme 9.5/10 · Animation style 9/10 · P3**

**Keep.** Large white helmet planes, clean dark visor and restrained red cape show how richer equipment can remain as readable as the villagers.

**Preserve:** Simple helmet planes, visor, sword, cape and existing royal palette.

**Stance decision:** Keep the economical three-quarter ready pose.

**Animation evidence/correction:** Attack anticipation, extension and recovery are easy to follow; preserve arm/weapon continuity.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Idle, AutoAttack.

**Golden-reference recommendation:** PROPOSE core addition for clean metal and royal hierarchy; do not copy white armor, closed face or pose to everyone.

**Inspected:** fallback/ready plus Idle, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_RoyalSwordsman.asset` → `Assets/_Game/Animations/CombatIdles/RoyalSwordsman_Idle.controller`. [All bound drawings](boards/royal_swordsman-frames.png) · [Native ready image](media/royal_swordsman-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Shield Knight

**Style 7.5/10 · Theme 9.5/10 · Animation style 8.5/10 · P2**

**Partial redraw/simplification.** Gold shield filigree and many trim fragments are the Dungeon's clearest internal-detail outlier. Redraw the emblem as a few connected gold masses with quiet steel around it.

**Preserve:** Recognizable decorated shield, cross-like helmet trim, dark steel/red cloth and gold identity.

**Stance decision:** Keep initially. An optional shield-forward quarter turn could clarify defense, but is a separate all-state repose, not needed for cleanup.

**Animation evidence/correction:** Raised-shield Ability is good acting; retain the blue grant pulse and shield outline. Carry simplified ornament through every shield rotation.

**Affected states:** Fallback/ready still and all bound states: Ability, AutoAttack, Idle. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core until ornament simplification; useful defensive-action study.

**Inspected:** fallback/ready plus Ability, AutoAttack, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_ShieldKnight.asset` → `Assets/_Game/Animations/CombatIdles/ShieldKnight_Idle.controller`. [All bound drawings](boards/shield_knight-frames.png) · [Native ready image](media/shield_knight-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Siege Sergeant

**Style 8.5/10 · Theme 9.5/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Beard, gloves and dark chest compress into one busy area. Separate beard from collar with a larger quiet plane and reduce small chest highlights.

**Preserve:** Bearded face, red crest, heavy hammer and workmanlike blue/brown equipment.

**Stance decision:** Keep weight over the hammer; broader stance fits the heavy strike.

**Animation evidence/correction:** Keep the deliberate hammer lift/contact; reduce a few peripheral ground sparks if they cover the boots.

**Affected states:** Fallback/ready still and all bound states: Idle, Ability, AutoAttack. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Secondary heavy-tool reference; not core.

**Inspected:** fallback/ready plus Idle, Ability, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_SiegeSergeant.asset` → `Assets/_Game/Animations/CombatIdles/SiegeSergeant_Idle.controller`. [All bound drawings](boards/siege_sergeant-frames.png) · [Native ready image](media/siege_sergeant-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Spear Guard

**Style 9/10 · Theme 9.5/10 · Animation style 9/10 · P3**

**Keep.** Simple open face, long spear and wooden round shield are clearly separated. A successful low-rank soldier.

**Preserve:** Kettle helmet, wood shield, long spear and blue-gray cloth.

**Stance decision:** Keep the vertical guard position; reach is expressed during attack.

**Animation evidence/correction:** Long horizontal thrust stays readable in its padded canvas. Preserve the shaft as one rigid line.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Idle, AutoAttack.

**Golden-reference recommendation:** Useful basic-spear example; not a core addition.

**Inspected:** fallback/ready plus Idle, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_SpearGuard.asset` → `Assets/_Game/Animations/CombatIdles/SpearGuard_Idle.controller`. [All bound drawings](boards/spear_guard-frames.png) · [Native ready image](media/spear_guard-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Spear Knight

**Style 8/10 · Theme 9.5/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Gold edge lines cover helmet, spear, torso and legs. Reduce uninterrupted bright edging on nonfocal lower plates while retaining the faction vocabulary.

**Preserve:** Red crest, gold-trimmed dark helmet, spear and knight proportions.

**Stance decision:** Keep the tall spear stance; same combat family does not demand a unique camera angle.

**Animation evidence/correction:** Retain the full thrust and attached plume; simplify trim consistently across rotations.

**Affected states:** Fallback/ready still and all bound states: Idle, AutoAttack. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; current detail density is less economical than Royal Swordsman.

**Inspected:** fallback/ready plus Idle, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_SpearKnight.asset` → `Assets/_Game/Animations/CombatIdles/SpearKnight_Idle.controller`. [All bound drawings](boards/spear_knight-frames.png) · [Native ready image](media/spear_knight-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Town Marshal

**Style 9/10 · Theme 10/10 · Animation style 9/10 · P3**

**Keep.** Broad belly, bald head, beard and handbell make a memorable civilian authority. Large areas are clean and expressive.

**Preserve:** Bald head, beard, belly, bell and secondary weapon.

**Stance decision:** Keep; his open arms and round body already provide valuable variety.

**Animation evidence/correction:** Bell lift and small circular ringing accents communicate the ability without losing the face.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and AutoAttack, Ability, Idle.

**Golden-reference recommendation:** Excellent personality example; reserve rather than expanding the core set.

**Inspected:** fallback/ready plus AutoAttack, Ability, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Enemy_TownMarshal.asset` → `Assets/_Game/Animations/CombatIdles/TownMarshal_Idle.controller`. [All bound drawings](boards/town_marshal-frames.png) · [Native ready image](media/town_marshal-still.png). Exact clip/sprite paths and hashes: `bindings.json`.


### Magical Forest

| Character | Style | Theme | Animation style | Priority | Intervention |
|---|---:|---:|---:|---|---|
| [Ancient Treant](Review.html#ancient_treant) | 7.5 | 10 | 7.5 | P2 | Internal shading/palette cleanup |
| [Barkhide Warden](Review.html#barkhide_warden) | 7.5 | 9.5 | 8 | P2 | Internal shading/palette cleanup |
| [Briar Archer](Review.html#briar_archer) | 8.5 | 9.5 | 8.5 | P3 | Internal shading/palette cleanup |
| [Briar Matriarch](Review.html#briar_matriarch) | 7.5 | 10 | 8 | P2 | Partial redraw/simplification |
| [Elven Mender](Review.html#elven_mender) | 9.5 | 10 | 9 | P3 | Keep |
| [Elven Scout](Review.html#elven_scout) | 9 | 9.5 | 8.5 | P3 | Keep |
| [Elven Thornkeeper](Review.html#elven_thornkeeper) | 8.5 | 9.5 | 8.5 | P3 | Internal shading/palette cleanup |
| [Orc Berserker](Review.html#orc_berserker) | 8.5 | 9.5 | 8.5 | P3 | Internal shading/palette cleanup |
| [Orc Bloomcaller](Review.html#orc_bloomcaller) | 8 | 10 | 8.5 | P2 | Internal shading/palette cleanup |
| [Orc Drummer](Review.html#orc_drummer) | 8.5 | 10 | 8 | P3 | Internal shading/palette cleanup |
| [Orc Rootbinder](Review.html#orc_rootbinder) | 8.5 | 9.5 | 8.5 | P3 | Internal shading/palette cleanup |
| [Orc Trailguard](Review.html#orc_trailguard) | 9 | 9.5 | 9 | P3 | Keep |
| [Snapvine](Review.html#snapvine) | 8.5 | 10 | 6.5 | P1 | Partial redraw/simplification |

#### Ancient Treant

**Style 7.5/10 · Theme 10/10 · Animation style 7.5/10 · P2**

**Internal shading/palette cleanup.** Approved Old Stump has an excellent cut-trunk silhouette, but repeated bark striations compete with the face and limb masses. Merge bark into broad turning planes; keep only selected grooves.

**Preserve:** Approved C identity, stump rings, branch crown, layered fungi, root arms and large footprint.

**Stance decision:** Keep the heavy crouched tree anatomy; no human stance.

**Animation evidence/correction:** Death frames 1-5 remain relatively upright and similar to a flinch. A later localized slumping/branch-settle ending would clarify defeat. Hit dust should not erase the stump face.

**Affected states:** Fallback/ready still and all bound states: Death, ChannelHold, AutoAttack, Release, ChannelStart, Ability, Idle, Hit. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist nonhuman anatomy only; not a core rendering anchor yet.

**Inspected:** fallback/ready plus Death, ChannelHold, AutoAttack, Release, ChannelStart, Ability, Idle, Hit.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Ancient_Treant.asset` → `Assets/_Game/Animations/Forest/Roster/Ancient_Treant.controller`. [All bound drawings](boards/ancient_treant-frames.png) · [Native ready image](media/ancient_treant-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Barkhide Warden

**Style 7.5/10 · Theme 9.5/10 · Animation style 8/10 · P2**

**Internal shading/palette cleanup.** Face, bark shoulder layers and shield grain all have similar busy contrast. Quiet interior bark lines and keep a stronger separation around the olive face.

**Preserve:** Broad orc body, large wooden shield, leaf/bark armor and earth palette.

**Stance decision:** Keep asymmetric shield-first mass; it already says protector.

**Animation evidence/correction:** Channel shield movement is readable; keep wood/arm contact. Simplification must follow shield turns, including fallen Death poses.

**Affected states:** Fallback/ready still and all bound states: ChannelStart, Death, ChannelHold, Release, Idle, Hit, Interrupt, AutoAttack. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; Trailguard gives a cleaner orc/material anchor.

**Inspected:** fallback/ready plus ChannelStart, Death, ChannelHold, Release, Idle, Hit, Interrupt, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Barkhide_Warden.asset` → `Assets/_Game/Animations/Forest/Barkhide_Warden.controller`. [All bound drawings](boards/barkhide_warden-frames.png) · [Native ready image](media/barkhide_warden-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Briar Archer

**Style 8.5/10 · Theme 9.5/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Pale hair against dark wine cloth is a good focal arrangement; a few pointed cloth/strap fragments crowd the waist and bow hand.

**Preserve:** Silver hair, dark bow, wine-red hood/cape and lean elf anatomy.

**Stance decision:** Keep the aimed stance; distinguish from Scout through costume and attack character, not a new camera.

**Animation evidence/correction:** ChannelStart and Release show a clear draw/release. Preserve the bowstring and hand at all keys; simplify only interior folds.

**Affected states:** Fallback/ready still and all bound states: Release, Idle, AutoAttack, Hit, ChannelHold, ChannelStart, Death. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist dark archer example; Scout is the cleaner optional bow reference.

**Inspected:** fallback/ready plus Release, Idle, AutoAttack, Hit, ChannelHold, ChannelStart, Death.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Briar_Archer.asset` → `Assets/_Game/Animations/Forest/Roster/Briar_Archer.controller`. [All bound drawings](boards/briar_archer-frames.png) · [Native ready image](media/briar_archer-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Briar Matriarch

**Style 7.5/10 · Theme 10/10 · Animation style 8/10 · P2**

**Partial redraw/simplification.** Crown, hair, fingers and many long robe accents make an elegant but more illustrative figure. Consolidate robe into three or four major panels and simplify internal hair strands; retain tall proportions.

**Preserve:** Approved queenly identity, thorn crown, pale hair/face, staff, long gown and 96x96 footprint.

**Stance decision:** Keep tall open-handed authority; do not shorten her into a villager.

**Animation evidence/correction:** Channel/Release gestures are clear; carry simpler robe planes through Hit, Death and casting. Do not enlarge effects to compensate for a small face.

**Affected states:** Fallback/ready still and all bound states: Hit, Death, AutoAttack, Interrupt, ChannelHold, ChannelStart, Release, Idle. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Apex silhouette reference only; not the global shading standard.

**Inspected:** fallback/ready plus Hit, Death, AutoAttack, Interrupt, ChannelHold, ChannelStart, Release, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Briar_Matriarch.asset` → `Assets/_Game/Animations/Forest/Briar_Matriarch.controller`. [All bound drawings](boards/briar_matriarch-frames.png) · [Native ready image](media/briar_matriarch-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Elven Mender

**Style 9.5/10 · Theme 10/10 · Animation style 9/10 · P3**

**Keep.** Open face, cream shoulder cloth, muted rose robe and simple staff separate cleanly. Calm friendly fantasy survives at native size.

**Preserve:** Approved face, ears, staff, cream/rose materials and slim anatomy.

**Stance decision:** Keep the welcoming free hand and upright staff; an excellent purposeful alternative to weapon-ready humans.

**Animation evidence/correction:** All eight states maintain recognizable face/costume; compact channel and recovery are well suited to the family.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Hit, Interrupt, ChannelHold, Idle, Death, AutoAttack, Release, ChannelStart.

**Golden-reference recommendation:** PROPOSE core addition for readable support/caster design and quiet cloth; do not copy elf anatomy or cream clothing globally.

**Inspected:** fallback/ready plus Hit, Interrupt, ChannelHold, Idle, Death, AutoAttack, Release, ChannelStart.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Elven_Mender.asset` → `Assets/_Game/Animations/Forest/Elven_Mender.controller`. [All bound drawings](boards/elven_mender-frames.png) · [Native ready image](media/elven_mender-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Elven Scout

**Style 9/10 · Theme 9.5/10 · Animation style 8.5/10 · P3**

**Keep.** Hair, ear, drawn bow and blue-gray clothing read cleanly with little surface noise. Bow/string legitimately add fine lines.

**Preserve:** Approved face/hair, long ear, bow, quiver and light equipment.

**Stance decision:** Keep the poised archer stance; useful contrast to standing melee troops.

**Animation evidence/correction:** Idle opens the bow/body more than the tiny Rattlebones settle; acceptable specialist acting, but do not amplify it further.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and AutoAttack, Hit, Death, Idle.

**Golden-reference recommendation:** Strong reserve reference for bow/hand relationship; omit from core to avoid redundancy with Mender and Farmer.

**Inspected:** fallback/ready plus AutoAttack, Hit, Death, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Elven_Scout.asset` → `Assets/_Game/Animations/Forest/Elven_Scout.controller`. [All bound drawings](boards/elven_scout-frames.png) · [Native ready image](media/elven_scout-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Elven Thornkeeper

**Style 8.5/10 · Theme 9.5/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Curved thorn growth identifies the kit, but thin branch tips and small waist details can become tangled. Group branch interiors while retaining the outer hook silhouette.

**Preserve:** Approved hair, ears, green tunic, thorn-loop prop and slim body.

**Stance decision:** Keep the guarded three-quarter stance; prop carries the distinction.

**Animation evidence/correction:** Ability spreads the thorn silhouette clearly. Preserve face and hand connections while simplifying branch texture across all states.

**Affected states:** Fallback/ready still and all bound states: AutoAttack, Hit, Death, Ability, Idle. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Secondary organic-tool reference; not core.

**Inspected:** fallback/ready plus AutoAttack, Hit, Death, Ability, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Elven_Thornkeeper.asset` → `Assets/_Game/Animations/Forest/Roster/Elven_Thornkeeper.controller`. [All bound drawings](boards/elven_thornkeeper-frames.png) · [Native ready image](media/elven_thornkeeper-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Orc Berserker

**Style 8.5/10 · Theme 9.5/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Large jaw, muscular shoulders and heavy axe communicate force. Small muscle/strap accents can be joined into larger skin and leather planes.

**Preserve:** Approved olive skin, tusked face, axe, red loincloth and broad build.

**Stance decision:** Keep the forward-ready weight; anatomical difference is intentional.

**Animation evidence/correction:** Ability's lifted chest/shout distinguishes rage without requiring a new identity. Keep grounded return and axe grip.

**Affected states:** Fallback/ready still and all bound states: Hit, Idle, AutoAttack, Death, Ability. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist broad-body acting; Trailguard is the more restrained core anchor.

**Inspected:** fallback/ready plus Hit, Idle, AutoAttack, Death, Ability.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Orc_Berserker.asset` → `Assets/_Game/Animations/Forest/Roster/Orc_Berserker.controller`. [All bound drawings](boards/orc_berserker-frames.png) · [Native ready image](media/orc_berserker-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Orc Bloomcaller

**Style 8/10 · Theme 10/10 · Animation style 8.5/10 · P2**

**Internal shading/palette cleanup.** Strong pale hood/robe and flower staff make a charming ritualist. Numerous garment folds, bead-like details and floral colors divide attention.

**Preserve:** Approved hood, face, cream/rose robe, flower staff and orc anatomy.

**Stance decision:** Keep open casting hand; no need to mimic Mender exactly.

**Animation evidence/correction:** Hand-to-mouth attack and raised-flower Ability are characterful; preserve these, simplifying repeated cloth accents throughout.

**Affected states:** Fallback/ready still and all bound states: Death, Hit, Ability, AutoAttack, Idle. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; useful thematic contrast, with less restrained surface treatment.

**Inspected:** fallback/ready plus Death, Hit, Ability, AutoAttack, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Orc_Bloomcaller.asset` → `Assets/_Game/Animations/Forest/Roster/Orc_Bloomcaller.controller`. [All bound drawings](boards/orc_bloomcaller-frames.png) · [Native ready image](media/orc_bloomcaller-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Orc Drummer

**Style 8.5/10 · Theme 10/10 · Animation style 8/10 · P3**

**Internal shading/palette cleanup.** Big drumhead and paired sticks explain the role immediately. Simplify drum lacing to a few clear diagonals and quiet chest straps.

**Preserve:** Approved drum size, two sticks, hair tie, olive face and leather palette.

**Stance decision:** Keep planted performer stance; slight torso angle already creates personality.

**Animation evidence/correction:** Ability's raised-stick preparation reads well. Death settles into a crouch that could use a clearer final head/drop beat if revised.

**Affected states:** Fallback/ready still and all bound states: Ability, AutoAttack, Idle, Hit, Death. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist instrument acting; not core.

**Inspected:** fallback/ready plus Ability, AutoAttack, Idle, Hit, Death.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Orc_Drummer.asset` → `Assets/_Game/Animations/Forest/Roster/Orc_Drummer.controller`. [All bound drawings](boards/orc_drummer-frames.png) · [Native ready image](media/orc_drummer-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Orc Rootbinder

**Style 8.5/10 · Theme 9.5/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Blue robe and pale fur shoulder separate well, but twig fingers/staff and small face shadows could be quieter. Group fur into a few broad tufts.

**Preserve:** Approved olive face, blue robe, pale shoulder fur and branching staff.

**Stance decision:** Keep the hunched shaman stance; useful contrast to Trailguard.

**Animation evidence/correction:** Staff raise and channel continuity read clearly; preserve the staff's full outline rather than attaching isolated new twigs.

**Affected states:** Fallback/ready still and all bound states: Death, ChannelHold, Hit, ChannelStart, Interrupt, Idle, Release, AutoAttack. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Supporting shaman anatomy/materials; not core.

**Inspected:** fallback/ready plus Death, ChannelHold, Hit, ChannelStart, Interrupt, Idle, Release, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Orc_Rootbinder.asset` → `Assets/_Game/Animations/Forest/Orc_Rootbinder.controller`. [All bound drawings](boards/orc_rootbinder-frames.png) · [Native ready image](media/orc_rootbinder-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Orc Trailguard

**Style 9/10 · Theme 9.5/10 · Animation style 9/10 · P3**

**Keep.** Broad jaw, simple leather mass, cool shoulder plate and pale axe head stay distinct. Shows that an orc can match the rendering without copying human proportions.

**Preserve:** Approved tusks, olive skin, broad build, leather armor and two-handed axe.

**Stance decision:** Keep the stable wide stance; it expresses weight rather than pose repetition.

**Animation evidence/correction:** Clear overhead anticipation, strike and compact recovery; Death resolves into an unmistakable prone silhouette.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Death, Idle, AutoAttack, Hit.

**Golden-reference recommendation:** PROPOSE core addition for broad nonhuman bodies and leather/iron separation; do not copy green skin, axe or stance to all monsters.

**Inspected:** fallback/ready plus Death, Idle, AutoAttack, Hit.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Orc_Trailguard.asset` → `Assets/_Game/Animations/Forest/Orc_Trailguard.controller`. [All bound drawings](boards/orc_trailguard-frames.png) · [Native ready image](media/orc_trailguard-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Snapvine

**Style 8.5/10 · Theme 10/10 · Animation style 6.5/10 · P1**

**Partial redraw/simplification.** The curved stalk and toothed mouth are a strong simple design. Preserve the still; concentrate work on Death's detached root-like piece above the head in frames 2-6.

**Preserve:** Approved mouth, teeth rhythm, curved green stalk, leaves and grounded root footprint.

**Stance decision:** Keep plant anatomy; no humanoid ready pose.

**Animation evidence/correction:** Replace the confusing airborne root cluster with an attached wilt/collapse progression or clearly separated small debris. Inspect source slicing first; do not assume a generation defect from names.

**Affected states:** Death first (especially drawings 2-6); keep the still, Idle, Hit and AutoAttack. Check sprite rectangles and source sheet before editing.

**Golden-reference recommendation:** Useful plant silhouette, not a motion reference until Death is corrected.

**Inspected:** fallback/ready plus AutoAttack, Hit, Death, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Forest/Snapvine.asset` → `Assets/_Game/Animations/Forest/Roster/Snapvine.controller`. [All bound drawings](boards/snapvine-frames.png) · [Native ready image](media/snapvine-still.png). Exact clip/sprite paths and hashes: `bindings.json`.


### Ironvein Excavation

| Character | Style | Theme | Animation style | Priority | Intervention |
|---|---:|---:|---:|---|---|
| [Bore Engineer](Review.html#bore_engineer) | 7.5 | 10 | 8.5 | P2 | Internal shading/palette cleanup |
| [The Grand Delver](Review.html#grand_delver) | 8 | 10 | 8 | P2 | Internal shading/palette cleanup |
| [Obsidian Sentinel](Review.html#obsidian_sentinel) | 7.5 | 9.5 | 8 | P2 | Partial redraw/simplification |
| [Ore Hauler](Review.html#ore_hauler) | 8.5 | 10 | 8.5 | P3 | Internal shading/palette cleanup |
| [Packbeetle](Review.html#packbeetle) | 8 | 10 | 8 | P2 | Internal shading/palette cleanup |
| [Pickaxe Delver](Review.html#pickaxe_delver) | 8.5 | 10 | 8.5 | P3 | Internal shading/palette cleanup |
| [Powder Sapper](Review.html#powder_sapper) | 8.5 | 10 | 8 | P3 | Internal shading/palette cleanup |
| [Rail Switcher](Review.html#rail_switcher) | 8 | 10 | 8 | P2 | Internal shading/palette cleanup |
| [Rivet Gunner](Review.html#rivet_gunner) | 7.5 | 9.5 | 8.5 | P2 | Internal shading/palette cleanup |
| [Rivet Turret](Review.html#rivet_turret) | 8.5 | 10 | 8.5 | P3 | Keep |
| [Seismic Smith](Review.html#seismic_smith) | 6.5 | 9.5 | 8 | P1 | Partial redraw/simplification |
| [Siege Machinist](Review.html#siege_machinist) | 7 | 10 | 8 | P2 | Partial redraw/simplification |
| [Stonewright](Review.html#stonewright) | 8 | 10 | 8 | P2 | Internal shading/palette cleanup |
| [Vein Surveyor](Review.html#vein_surveyor) | 8.5 | 10 | 8.5 | P3 | Internal shading/palette cleanup |

#### Bore Engineer

**Style 7.5/10 · Theme 10/10 · Animation style 8.5/10 · P2**

**Internal shading/palette cleanup.** Drill tip and orange power chamber identify the job, but goggles, beard and machinery carry many tiny equally sharp lines. Simplify beard strands and glove/weapon overlaps.

**Preserve:** Dwarf proportions, goggles, gray beard, drill spiral, warm power cell and two-hand grip.

**Stance decision:** Keep braced tool-forward stance; do not slim him toward the villagers.

**Animation evidence/correction:** Normal and ore-charged thrusts retain a clear tip. Keep powered accents concentrated at the chamber/contact, not across the face.

**Affected states:** Fallback/ready still and all bound states: OreChargedAutoAttack, ChannelStart, Hit, Idle, Death, Release, ChannelHold, AutoAttack. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; study Miner for material economy and Trailguard for broad-body grouping.

**Inspected:** fallback/ready plus OreChargedAutoAttack, ChannelStart, Hit, Idle, Death, Release, ChannelHold, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/bore_engineer.asset` → `Assets/_Game/Animations/Ironvein/bore_engineer.controller`. [All bound drawings](boards/bore_engineer-frames.png) · [Native ready image](media/bore_engineer-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### The Grand Delver

**Style 8/10 · Theme 10/10 · Animation style 8/10 · P2**

**Internal shading/palette cleanup.** The suit already has broad useful chest planes; repeated joint/rivet highlights and dark round idle smoke distract from the small pilot face. Simplify joint glints and smoke clusters.

**Preserve:** Huge drill, asymmetric claw, exposed dwarf face, orange power accents and mechanical mass.

**Stance decision:** Keep heavy broad stance; no anatomy normalization.

**Animation evidence/correction:** Idle smoke is more prominent than the body's settle; reduce its contrast/occupied area. Preserve powered reach, claw claim and Core poses; never alter contact timing through art cleanup.

**Affected states:** Fallback/ready still and all bound states: Idle, CoreRelease, Hit, OreChargedAutoAttack, SteamAbility, CoreChannelHold, AutoAttack, Death, ClaimAbility, CoreChannelStart. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Useful large-machine silhouette reference; not core character rendering.

**Inspected:** fallback/ready plus Idle, CoreRelease, Hit, OreChargedAutoAttack, SteamAbility, CoreChannelHold, AutoAttack, Death, ClaimAbility, CoreChannelStart.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/grand_delver.asset` → `Assets/_Game/Animations/Ironvein/grand_delver.controller`. [All bound drawings](boards/grand_delver-frames.png) · [Native ready image](media/grand_delver-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Obsidian Sentinel

**Style 7.5/10 · Theme 9.5/10 · Animation style 8/10 · P2**

**Partial redraw/simplification.** Large shoulder rocks, round plates and piston joints create a strong golem but too many small bevels. Flatten nonfocal armor planes and clarify stone versus metal joints.

**Preserve:** Massive dark body, angular shoulders, orange slit/core, piston limbs and heavy fists.

**Stance decision:** Keep square weight-bearing stance; the absence of a human face is intentional.

**Animation evidence/correction:** Slam and Devour poses have distinct silhouettes. Preserve their difference; remove only excess micro-bevels across all rotations.

**Affected states:** Fallback/ready still and all bound states: Death, AutoAttack, SlamChannelStart, Hit, Idle, OreChargedAutoAttack, DevourChannelStart, DevourRelease, DevourChannelHold, SlamRelease, SlamChannelHold. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; machinery/stone specialist after cleanup.

**Inspected:** fallback/ready plus Death, AutoAttack, SlamChannelStart, Hit, Idle, OreChargedAutoAttack, DevourChannelStart, DevourRelease, DevourChannelHold, SlamRelease, SlamChannelHold.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/obsidian_sentinel.asset` → `Assets/_Game/Animations/Ironvein/obsidian_sentinel.controller`. [All bound drawings](boards/obsidian_sentinel-frames.png) · [Native ready image](media/obsidian_sentinel-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Ore Hauler

**Style 8.5/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Pale beard, rounded load and orange ore tell the role well. Quiet the basket straps and small alternating ore chips around the main chunks.

**Preserve:** Large carried load, pale beard, cap, orange ore and stocky posture.

**Stance decision:** Keep forward burdened lean; strong differentiation from armed dwarves.

**Animation evidence/correction:** Ore scattering in Death is readable. Keep the load attached and simplify straps consistently during reaching.

**Affected states:** Fallback/ready still and all bound states: Death, Idle, Hit, AutoAttack, Ability. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** One of Ironvein's stronger candidates; reserve load/prop reference, pending art approval.

**Inspected:** fallback/ready plus Death, Idle, Hit, AutoAttack, Ability.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/ore_hauler.asset` → `Assets/_Game/Animations/Ironvein/ore_hauler.controller`. [All bound drawings](boards/ore_hauler-frames.png) · [Native ready image](media/ore_hauler-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Packbeetle

**Style 8/10 · Theme 10/10 · Animation style 8/10 · P2**

**Internal shading/palette cleanup.** Low beetle body and ore panniers are recognizable, but legs and dark shell edges merge against dark backgrounds. Create a few broader upper shell planes and simplify pack fixtures.

**Preserve:** Six-legged beetle read, low silhouette, dark carapace, ore load and orange eyes.

**Stance decision:** Keep quadrupedal/insect support footprint; do not give it a humanoid stance.

**Animation evidence/correction:** Attack is deliberately compact; make leg compression readable without lifting the whole load. Death collapse is clear enough to preserve.

**Affected states:** Fallback/ready still and all bound states: Idle, Hit, AutoAttack, Death. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Specialist pack-creature reference after value separation; not core.

**Inspected:** fallback/ready plus Idle, Hit, AutoAttack, Death.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/packbeetle.asset` → `Assets/_Game/Animations/Ironvein/packbeetle.controller`. [All bound drawings](boards/packbeetle-frames.png) · [Native ready image](media/packbeetle-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Pickaxe Delver

**Style 8.5/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Large beard, simple pick and broad workwear mostly fit the family. Quiet little chest/strap facets and keep the face distinct from the beard.

**Preserve:** Pick length/head, lamp helmet, brown beard and short broad body.

**Stance decision:** Keep worker's planted swing stance.

**Animation evidence/correction:** OreChargedAutoAttack frames 5-7 are much brighter than ordinary attack; reduce the arc to a compact connected sweep while preserving the powered cue.

**Affected states:** Fallback/ready still and all bound states: AutoAttack, Death, OreChargedAutoAttack, Hit, Idle. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Strong supporting dwarf baseline; not core until art selection receives approval.

**Inspected:** fallback/ready plus AutoAttack, Death, OreChargedAutoAttack, Hit, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/pickaxe_delver.asset` → `Assets/_Game/Animations/Ironvein/pickaxe_delver.controller`. [All bound drawings](boards/pickaxe_delver-frames.png) · [Native ready image](media/pickaxe_delver-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Powder Sapper

**Style 8.5/10 · Theme 10/10 · Animation style 8/10 · P3**

**Internal shading/palette cleanup.** Wild red hair, round goggles and paired explosives have strong personality. Simplify small belt rectangles and beard hatching; protect the fuse silhouette.

**Preserve:** Wild hair, goggles, two bombs, fuse and compact dwarf body.

**Stance decision:** Keep outward hands and eager lean; good purposeful variety.

**Animation evidence/correction:** Channel pose brings hands together clearly. Death ends in a brief crouch; a stronger head drop would better separate defeat from preparation.

**Affected states:** Fallback/ready still and all bound states: Death, Hit, AutoAttack, ChannelStart, Release, ChannelHold, Idle. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Strong Ironvein personality reserve; not core rendering standard.

**Inspected:** fallback/ready plus Death, Hit, AutoAttack, ChannelStart, Release, ChannelHold, Idle.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/powder_sapper.asset` → `Assets/_Game/Animations/Ironvein/powder_sapper.controller`. [All bound drawings](boards/powder_sapper-frames.png) · [Native ready image](media/powder_sapper-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Rail Switcher

**Style 8/10 · Theme 10/10 · Animation style 8/10 · P2**

**Internal shading/palette cleanup.** Cap and green goggles distinguish the operator, but the chest device, gloves and beard compress into a busy dark oval. Separate the hand/device boundary and quiet minor fittings.

**Preserve:** Rail cap, green lenses, controller/device, dark coat and stout legs.

**Stance decision:** Keep operator stance; if polishing pose, turn only the control panel outward enough to reveal its function.

**Animation evidence/correction:** Release device gesture is subtle; reinforce hand-to-control contact rather than making the whole body swing.

**Affected states:** Fallback/ready still and all bound states: ChannelStart, Hit, Release, ChannelHold, Idle, AutoAttack, Death. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; useful specialist prop concept.

**Inspected:** fallback/ready plus ChannelStart, Hit, Release, ChannelHold, Idle, AutoAttack, Death.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/rail_switcher.asset` → `Assets/_Game/Animations/Ironvein/rail_switcher.controller`. [All bound drawings](boards/rail_switcher-frames.png) · [Native ready image](media/rail_switcher-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Rivet Gunner

**Style 7.5/10 · Theme 9.5/10 · Animation style 8.5/10 · P2**

**Internal shading/palette cleanup.** Weapon, gloves, beard and chest overlap in one gold-brown cluster. Broaden the gun's main metal plane and separate it from the supporting forearm; remove tiny beard specks.

**Preserve:** Goggles, orange-brown beard, rivet weapon, blue-gray sleeves and two-hand grip.

**Stance decision:** Keep horizontal braced firing stance; plausible shared posture with Bore Engineer.

**Animation evidence/correction:** Short recoil and directional muzzle flash work. Keep normal/charged distinction without a larger explosion.

**Affected states:** Fallback/ready still and all bound states: Death, OreChargedAutoAttack, Idle, AutoAttack, Hit. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; the turret demonstrates the machinery more economically.

**Inspected:** fallback/ready plus Death, OreChargedAutoAttack, Idle, AutoAttack, Hit.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/rivet_gunner.asset` → `Assets/_Game/Animations/Ironvein/rivet_gunner.controller`. [All bound drawings](boards/rivet_gunner-frames.png) · [Native ready image](media/rivet_gunner-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Rivet Turret

**Style 8.5/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Keep.** Barrel, rectangular warm power housing and stable legs read at a glance. One of the cleaner machine designs in the cast.

**Preserve:** Mechanical barrel, amber housing, leg footprint and machine-only identity.

**Stance decision:** Keep planted tripod-like support; no character pose required.

**Animation evidence/correction:** Recoil, powered flash and mechanical collapse are easy to distinguish. Preserve the muzzle as the flash origin.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and OreChargedAutoAttack, Hit, Idle, Death, AutoAttack.

**Golden-reference recommendation:** Best current Ironvein machine reserve; proposed core membership deferred pending artwork review and to keep shortlist small.

**Inspected:** fallback/ready plus OreChargedAutoAttack, Hit, Idle, Death, AutoAttack.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/rivet_turret.asset` → `Assets/_Game/Animations/Ironvein/rivet_turret.controller`. [All bound drawings](boards/rivet_turret-frames.png) · [Native ready image](media/rivet_turret-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Seismic Smith

**Style 6.5/10 · Theme 9.5/10 · Animation style 8/10 · P1**

**Partial redraw/simplification.** Face sits inside dense dark beard/chest texture; chain-like torso marks and heavily beveled hammer dilute the strong arm/hammer silhouette. Redraw interior beard and shirt into broad masses; isolate eyes/nose against one skin plane.

**Preserve:** Powerful arms, black beard, heavy rune hammer, orange focal inset, dwarf width and worker character.

**Stance decision:** Keep broad hammer-bearing stance. Do not solve shading noise by changing species proportions.

**Animation evidence/correction:** Hammer lift/contact already reads clearly. Propagate simpler beard/chest/hammer planes through Ability and charged attack; preserve the hand grip and repaired edge.

**Affected states:** Fallback/ready still and all bound states: Ability, Death, OreChargedAutoAttack, Idle, AutoAttack, Hit. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core in current state; strongest Ironvein candidate for a targeted internal redraw.

**Inspected:** fallback/ready plus Ability, Death, OreChargedAutoAttack, Idle, AutoAttack, Hit.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/seismic_smith.asset` → `Assets/_Game/Animations/Ironvein/seismic_smith.controller`. [All bound drawings](boards/seismic_smith-frames.png) · [Native ready image](media/seismic_smith-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Siege Machinist

**Style 7/10 · Theme 10/10 · Animation style 8/10 · P2**

**Partial redraw/simplification.** Wrench silhouette is excellent; toolbelt, backpack tubes, goggles and beard all carry tiny edge accents. Redraw the belt/backpack interiors with fewer larger parts and a quiet torso.

**Preserve:** Large wrench, goggles, gray beard, blue machinery pack, stocky build and assembler identity.

**Stance decision:** Keep wrench-up ready stance; distinguish from gunner through the prop, not an exaggerated lean.

**Animation evidence/correction:** Assemble and Prime gestures are close in body read; emphasize distinct wrench/hand key poses in a later local action pass, preserving timings.

**Affected states:** Fallback/ready still and all bound states: AssembleAbility, Idle, PrimeAbility, AutoAttack, Hit, Death. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; strong concept needing restrained execution.

**Inspected:** fallback/ready plus AssembleAbility, Idle, PrimeAbility, AutoAttack, Hit, Death.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/siege_machinist.asset` → `Assets/_Game/Animations/Ironvein/siege_machinist.controller`. [All bound drawings](boards/siege_machinist-frames.png) · [Native ready image](media/siege_machinist-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Stonewright

**Style 8/10 · Theme 10/10 · Animation style 8/10 · P2**

**Internal shading/palette cleanup.** Wide helmet and small hammer provide a good silhouette. Twin braids and repeated armor borders can be simplified to larger connected clusters.

**Preserve:** Wide-brim helmet, orange braids, compact hammer, dark apron and broad stance.

**Stance decision:** Keep sturdy craftsperson stance; do not copy Seismic Smith's huge hammer.

**Animation evidence/correction:** Idle has three exposures totaling 790 ms, unlike nearby dwarf idles. Review whether the short settle feels rushed in playback; do not retime automatically from frame count.

**Affected states:** Fallback/ready still and all bound states: AutoAttack, Ability, Hit, OreChargedAutoAttack, Idle, Death. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Supporting dwarf variation; not core.

**Inspected:** fallback/ready plus AutoAttack, Ability, Hit, OreChargedAutoAttack, Idle, Death.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/stonewright.asset` → `Assets/_Game/Animations/Ironvein/stonewright.controller`. [All bound drawings](boards/stonewright-frames.png) · [Native ready image](media/stonewright-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Vein Surveyor

**Style 8.5/10 · Theme 10/10 · Animation style 8.5/10 · P3**

**Internal shading/palette cleanup.** Pale beard and gold lens stand out against the blue hood. Rolled map and satchel are clear; simplify little belt/strap crossings.

**Preserve:** Blue hood, white beard, gold inspection lens, map and dwarf anatomy.

**Stance decision:** Keep the inspecting forward lean; purposeful role distinction.

**Animation evidence/correction:** Release presents the map/lens clearly; preserve small measured gestures and prop continuity.

**Affected states:** Fallback/ready still and all bound states: ChannelHold, Idle, AutoAttack, Release, Hit, Death, ChannelStart. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Good intellectual/utility dwarf reserve; not core expansion.

**Inspected:** fallback/ready plus ChannelHold, Idle, AutoAttack, Release, Hit, Death, ChannelStart.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/vein_surveyor.asset` → `Assets/_Game/Animations/Ironvein/vein_surveyor.controller`. [All bound drawings](boards/vein_surveyor-frames.png) · [Native ready image](media/vein_surveyor-still.png). Exact clip/sprite paths and hashes: `bindings.json`.


### Players

| Character | Style | Theme | Animation style | Priority | Intervention |
|---|---:|---:|---:|---|---|
| [Bardley](Review.html#bardley) | 9.5 | 10 | 9.5 | P3 | Keep |
| [Gideon Glass](Review.html#gideon_glass) | 8.5 | 10 | 9 | P3 | Internal shading/palette cleanup |
| [RattleBones](Review.html#skeleton) | 10 | 10 | 10 | P3 | Keep |

#### Bardley

**Style 9.5/10 · Theme 10/10 · Animation style 9.5/10 · P3**

**Keep.** Asymmetric lime slime, plum hat/cape and little instrument are bold, cheerful and extremely readable. Large body areas remain quiet.

**Preserve:** Exact slime contour, hat/feather, instrument, face, saturated body and current grounded production baseline.

**Stance decision:** Keep asymmetry and low body; never make a human pose template.

**Animation evidence/correction:** Preserve compact grounded idle and musical cast. Historical static placement-pending file is not the current animation alignment authority.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Idle, Ability.

**Golden-reference recommendation:** Existing palette/identity reference; useful alongside Rattlebones, without expanding enemy core shortlist.

**Inspected:** fallback/ready plus Idle, Ability.

**Binding:** `Assets/_Game/Resources/Players/Player_Bardley.asset` → `Assets/_Game/Animations/CombatIdles/Bardley_Idle.controller`. [All bound drawings](boards/bardley-frames.png) · [Native ready image](media/bardley-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Gideon Glass

**Style 8.5/10 · Theme 10/10 · Animation style 9/10 · P3**

**Internal shading/palette cleanup.** Large hat, pale feather and blue lens create a memorable automaton. Thin coat/cane and small brass body details are less readable than Rattlebones; reduce only optional coat specks if needed.

**Preserve:** Approved detailed design, hat/feather silhouette, lens, brass face, split coat and thin cane. The rejected simplified redesign stays rejected.

**Stance decision:** Keep elegant narrow side-facing stance; do not force the large-head villager proportions.

**Animation evidence/correction:** Cast and recovery stay controlled. Preserve the intentional still Hold pose and avoid inventing breathing or camera motion.

**Affected states:** Fallback/ready still and all bound states: Ability, Recovery, Idle, Hold. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Player specialist only; not a universal anatomy/detail standard.

**Inspected:** fallback/ready plus Ability, Recovery, Idle, Hold.

**Binding:** `Assets/_Game/Resources/Players/Player_GideonGlass.asset` → `Assets/_Game/Animations/GideonGlass/Gideon.controller`. [All bound drawings](boards/gideon_glass-frames.png) · [Native ready image](media/gideon_glass-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### RattleBones

**Style 10/10 · Theme 10/10 · Animation style 10/10 · P3**

**Keep.** Primary golden standard: clear skull, cyan focal eyes, tiny crown and red cape; broad bone planes and sparse accents create instant identity.

**Preserve:** All approved Rattlebones anatomy, palette, face, crown, cape, scale and motion.

**Stance decision:** Keep exactly; never redesign him to justify mismatched cast art.

**Animation evidence/correction:** Use the current nine-frame grounded idle and compact royal command as motion anchors, not universal frame-count laws.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Idle, Ability.

**Golden-reference recommendation:** PRIMARY GOLDEN STANDARD, explicitly established by this task.

**Inspected:** fallback/ready plus Idle, Ability.

**Binding:** `Assets/_Game/Resources/Players/Player_Skeleton.asset` → `Assets/_Game/Animations/CombatIdles/Rattlebones_Idle.controller`. [All bound drawings](boards/skeleton-frames.png) · [Native ready image](media/skeleton-still.png). Exact clip/sprite paths and hashes: `bindings.json`.


### Ironvein alternates (disabled)

| Character | Style | Theme | Animation style | Priority | Intervention |
|---|---:|---:|---:|---|---|
| [Grand Delver — pilot](Review.html#grand_delver_pilot) | 8 | 9.5 | 8 | P3 | Keep |
| [Grand Delver — reserve suit](Review.html#grand_delver_reserve) | 7.5 | 9.5 | 8 | P3 | Internal shading/palette cleanup |

#### Grand Delver — pilot

**Style 8/10 · Theme 9.5/10 · Animation style 8/10 · P3**

**Keep.** Small pilot in a large 112x112 transition canvas is intentional. Judge drawn bounds at shared pixel scale; do not enlarge him merely to fill the canvas.

**Preserve:** Orange beard, helmet, pilot identity, small scale relative to suit and continuous ejection placement.

**Stance decision:** Keep small braced pilot stance. Inspect runtime floor/HP spacing before any pose work; correct layout instead of redrawing if a placement defect is later confirmed.

**Animation evidence/correction:** Eject frames 5-14 coherently reveal the pilot and falling suit; verify optional phase continuity only if remount is enabled later. No import defect is established here.

**Affected states:** None required. Any optional polish named above is a separate proposal; preserve the current still and Eject, AutoAttack, Death, Idle, Hit.

**Golden-reference recommendation:** Not core; disabled experimental alternate, lower remediation priority.

**Inspected:** fallback/ready plus Eject, AutoAttack, Death, Idle, Hit.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/grand_delver.asset` → `Assets/_Game/Animations/Ironvein/grand_delver_pilot.controller`. [All bound drawings](boards/grand_delver_pilot-frames.png) · [Native ready image](media/grand_delver_pilot-still.png). Exact clip/sprite paths and hashes: `bindings.json`.

#### Grand Delver — reserve suit

**Style 7.5/10 · Theme 9.5/10 · Animation style 8/10 · P3**

**Internal shading/palette cleanup.** Reserve suit preserves the boss identity but has more fine contour highlights and smoke. Use the same simplified joint/metal scheme as the primary suit.

**Preserve:** Distinct reserve machine, orange beard, drill/claw, smoke-stack and transition silhouette.

**Stance decision:** Keep heavy stance; do not change relative pilot scale.

**Animation evidence/correction:** Remount deliberately brings the suit in from off-canvas; edge entry is not automatically clipping damage. Review idle/action seams before enabling the experiment.

**Affected states:** Fallback/ready still and all bound states: OreChargedAutoAttack, Remount, Death, ClaimAbility, AutoAttack, Hit, CoreChannelStart, Idle, CoreRelease, SteamAbility, CoreChannelHold. Review original timing and entry/exit registration after any approved edit.

**Golden-reference recommendation:** Not core; disabled alternate, review after the active boss.

**Inspected:** fallback/ready plus OreChargedAutoAttack, Remount, Death, ClaimAbility, AutoAttack, Hit, CoreChannelStart, Idle, CoreRelease, SteamAbility, CoreChannelHold.

**Binding:** `Assets/_Game/Data/Enemies/Ironvein/grand_delver.asset` → `Assets/_Game/Animations/Ironvein/grand_delver_reserve.controller`. [All bound drawings](boards/grand_delver_reserve-frames.png) · [Native ready image](media/grand_delver_reserve-still.png). Exact clip/sprite paths and hashes: `bindings.json`.
