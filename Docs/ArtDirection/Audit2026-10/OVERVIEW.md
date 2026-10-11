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
