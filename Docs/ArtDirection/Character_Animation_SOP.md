# Dungeon Matcher character animation SOP

Use this procedure when drawing or repairing a character idle, attack, cast, hit, death or transition. The [art direction](Dungeon_Matcher_Art_Direction.txt) and [golden reference register](Golden_Reference_Register.md) own rendering and approval. The [cast source index](Character_Animation_Sources.md) identifies the production sheets and acting history for individual characters. The [frame audit](CastMotionAudit/Review.html) contains examples to inspect while working.

## 1. Identify the drawing the game actually shows

Start at the player or enemy definition. Resolve its controller, state, clip, and **each sprite’s GUID and local file ID**. Check the secondary forms and the code that selects an action. Minotaur’s board charge has a separate face icon. The Grand Delver has additional controllers. A shared animation, portrait copy, overlay or held cel can be intentional.

Open the current first pose, the accepted actor still and the current Rattlebones reference at native scale. Verify the registered reference hashes. For human proportions compare the actor in a floor-aligned lineup with Farmer, Pan or an appropriate armored human. Record the silhouette, eye design, facing, material colors, weapon hand, shield side and floor row. Preserve helmeted heads, pearl faces, tusks, glowing mineral eyes and mechanical lenses according to that actor’s approval.

Keep the current sheet, native file, pivot, sub-sprite rectangles and exposure list as the before version. Read the latest correction before choosing an older source. Farmer’s hand-edited idle supersedes the earlier take. The Puffer Sentinel’s restored face, Ironvein’s west-facing repairs and the later shark-tail edit all have precedence over earlier exports. The expansion eye cleanup applied only to the ten new enemies. The Nacre Halberdier has a flat pearl-white eye inside his visor by explicit character direction. Carry that color through blinking and weapon poses.

## 2. Block the action in native pixels

1. Put the neutral pose on the production floor. Draw the largest anticipation, contact and follow-through silhouettes on separate cels. Include the hat, crest, back foot and the farthest tool tip when deciding the cell bounds.
2. Add transparent pixels wherever the swing needs room. Keep the body at its original pixel scale. Carry the same floor and pivot offset through every frame. The expansion Axeman and Halberdier use 160×144 working cells. A helmet or weapon can legitimately make one member of a lineup taller.
3. Mark the supporting foot and the main hand or socket. On a weight shift, move the pelvis, ribcage and shoulder in sequence. Keep the planted boot’s sole on the floor. A lifted toe should visibly rejoin its leg. A slime compresses around its base, while a machine can hinge around a rivet.
4. For an existing actor, try a local LibreSprite correction first. For genuinely new motion, send the approved still and required actual golden images to PixelLab under the project’s current permissions. Record the source frames, selected indices, rejected poses and their visible defects. A clean pose reused on recovery can read better than an extra malformed drawing.

## 3. Give an idle one readable rhythm

Play Rattlebones alongside the new loop. His nine 130 ms exposures make a useful breathing reference. Heavy armor, a banner, a slime and a tiny slinger have different amounts of travel. Keep the detail that expresses their individual weight.

Animate one complete inhale and settle with the head riding the neck. For the expansion’s left lean, the primary horizontal path is **neutral → one pixel left → neutral**. Track a brow or temple over *every* breath, including the loop boundary. In this pass the old recipe reversed the lean on one cycle, although the first breath looked plausible in isolation. Add secondary hair, beard, ear or crest motion a beat later. Blink on its own exposure. The expansion’s long idles use 40 ms into the blink, 80 ms closed and 40 ms opening, once over several breaths. A visor may hide most of this motion.

Redraw the hidden sleeve, scarf, collar and neck behind a moving part. Check those joins on a contrasting matte at the extreme of the breath. The Miner’s hand remains connected to the overall silhouette even where a transparent row cuts through his pick. A silhouette-component count alone misses this defect.

## 4. Draw what makes the action happen

| Action | What to draw and inspect |
|---|---|
| Swords, maces and axes | Move the shoulder, elbow and hand through anticipation, contact and recovery. Keep the grip closed around the handle. Follow the weapon head, socket and butt through every exposure. Draw the rigid shaft as one piece and put the fingers over it. Allow foreshortening when the pose shows depth. Check the Bulwark’s mace and Axeman’s high guard against their lowered poses. |
| Spears, halberds and rapiers | Establish which hand drives the thrust and which guides it. Keep the blade on its axis and show the weight settling after contact. Review the far tip against both canvas edges. The Royal Lancer’s two-handed thrust and Nacre Duelist’s scallop guard are actor-specific references. |
| Casts and heals | Let the actor look or turn toward the target. As the hand lifts, rotate the shoulder, allow the torso to follow and settle the lower body. Attach the glow to the gem or emitting surface. The Chirurgeon rises into the heal as the staff comes up. Gideon makes a small mechanical shutter movement and holds it until released. |
| Ranged attacks | Show the ready position, release and recovery separately. Trace exactly one object from hand to exit for each intended projectile. The Orc has one stone and a small recovery stumble. Guild Ranger uses a follow-up shot. Kobold’s six rapid discharges belong to an ability sequence. Read the actual event contract for each one. |
| Breathing with a grounded stance | Scrub the shirt hem, belt, pelvis and both thighs together. On a fixed floor registration, compare the bad exposure with the one immediately before and after. Follow which edge belongs to the moving arm or hem and which belongs to the planted leg. Trace the jaw or collar for the moving head. Check a magnified capture on the actual Game canvas before accepting a repaired waist. |
| Hit, death and transforms | Preserve mass as the actor recoils or falls. Check knees, soles, tail, horn, cape and weapon on every cel, including the held end. An interior ruler-straight cut through a body can survive a transparent-padding check. Reconstruct clipped anatomy on the source drawing. Review channel/start/interrupt/release and mounted/pilot transitions consecutively with the next state’s first drawing. |

## 5. Finish the pixels in LibreSprite

Keep the working file at 1×. Use nearest-neighbour, integer translation and an explicit local selection for a face or hair ramp. Protect the outline, face, straps and weapon highlights where colors are shared. Keep highlights on glass, teeth, metal, scales and ore distinct from eye whites. For resizing a head, pin the jaw at the collar, preserve the brow, nose and mouth, then restore foreground blades and gloves on top. Review every pose that passes the weapon in front of the face.

Inspect on light and dark mattes and on a saturated contrasting matte where a slit is suspected. Clean a detached pixel only after identifying its source part and checking its neighbors in time. Dust, a muzzle flash and an intentional drifting spark can be detached. Palette membership, binary alpha, a continuous outer silhouette and edge clearance are useful measurements. They cannot establish that a wrist looks attached or that a cut-off foot looks whole.

### Repairing a single bad exposure

Play the approved loop at gameplay size. Pause where the movement catches, then step **previous → suspect → next**, including the last-to-first boundary. Keep the floor and pivot registered. Use a bright matte for holes and a dark matte for leftover light or skin pixels. Toggle an onion skin over the two drawings and follow a specific edge from its owner, such as sleeve to glove, or pick handle through hand. A line that stays behind while its owner rises can read as a rip even when the entire silhouette is connected.

The Dungeon Miner is the reference case. As his torso lifted, a thin band remained across his trousers and pixels hung beneath his gloves. His breath, head, pick and planted feet already told the right story. Restore the accepted source first. Select only the faulty pixels in frame 2, clear the leftover edge, and borrow the exposed trouser or glove color from the neighboring pose at the matching joint. The accepted correction changed 26 native pixels. The other nine timed drawings matched their originals exactly. [Step through the Miner’s Game capture](AnimationSweep/Review.html).

Before exporting, compare the full RGBA sheet to the approved baseline. List the changed cel numbers, pixel counts and bounding boxes. Blink the difference mask over the edit and verify every changed pixel belongs to the joint being repaired. Compare every other cel, the exposure sequence, pivots, events and loop length. Reopen the editable master and compare its rendered pixels to the bound Unity sprites. Keep both versions on the same playback clock for review.

### Distinguishing flat shading from blur

Look at the actor at native size beside Rattlebones and the closest approved material reference. A broad, uniform patch gives the eye a readable plane. Look for several almost-identical values following the same edge, a one-pixel fringe outside an outline, or scattered highlight islands that crawl over one material during a hold. Compare a lit base, a shadow and a selective highlight on the same surface. Verify point filtering, mipmaps and integer presentation before attributing softness to the drawing.

Trace the material across idle, contact, hit, death, fallback and form changes. Pin the approved face, outline, silhouette and identifying accents. For a confirmed noisy ramp, choose existing swatches from that material, mask shared colors, and merge only the intermediate values that obscure its shape. Keep a distinct highlight where it describes a tooth, hardware, pearl or an emitting core. Recheck a complete loop and the adjoining state at game size. A color count or a close-color edge percentage can locate a candidate. The visual comparison determines which pixels to edit. [Synchronized material and seam examples](AnimationSweep/MaterialContinuity/Review.html).

## 6. Preserve timing and check the result

- Record native width/height, visible bounds, floor, pivot, per-cel duration, contact/release time, loop flag, source hash and exported sheet hash. Keep full native editable cels with the review. Round-trip the exported pixels and durations through LibreSprite.
- Read the existing attack or ability event in Unity before retiming. The expansion mace and axe contacts are at 320 ms in a 680 ms attack. The heal releases at 360 ms in a 780 ms cast. Keep the impact drawing on the actual event. Distinguish a terminal Unity key that holds the last drawing from an additional hit.
- Play at authored speed at native size beside the reference. Step the extremes, examine the neighboring cels, then play across several loop boundaries. Scrub both versions on the **same elapsed clock**. Give the reviewer a whole-character loop, numbered problem frames, a short diagnosis and an editable source.
- When approved for integration, preserve the sprite IDs, point filtering, full rectangles, pixel scale and existing event ownership. Compare imported sub-sprite RGBA with the accepted native cels at the registered offset. In the Game scene, check both portrait sizes and the true action trigger. For a single hit, exercise a legal action with and without shield, observe the contact and count the actual damage events. Keep clip playback, Game-scene footage and device testing separately identified.

## Production precedents

The [cast source index](Character_Animation_Sources.md) covers the five player definitions, all enemy definitions and the Grand Delver’s additional forms. It connects the original workflows to current bound artwork. The live conversation provides specific approvals and corrections, including Snowcap’s blink and solid eyes, the restored Puffer face, west-facing repairs, the accepted single Orc shot and the expansion weapon changes. Earlier actor work also survives in native files, selection manifests and integration notes. Where historical material records only accepted cels, use those cels as evidence and leave their unrecorded drawing steps open.
