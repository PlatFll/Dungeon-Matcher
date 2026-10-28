# ASTRA — Ironvein Excavation: Bottom Corner Environmental Rebuild

## Main objective

Improve ONLY the **bottom-left and bottom-right environmental decorations** of our existing Ironvein Excavation zone.

Use the attached mockup as the visual composition target.

I want the corners to look like actual functional areas of a dwarven mining operation, with mining equipment, sturdy timber construction, rock formations and glowing ore.

**Do not regenerate the entire screen. Do not redesign the board or HUD.**

Use PixelLab to generate the necessary assets individually through **text descriptions**, then clean and recolor them in LibreSprite using our established art direction.

You are authorized to use **as many PixelLab generations as necessary** to produce high-quality results. Track the generations spent, reject poor results and revise until the assets look coherent.

## 1. Mandatory art preparation

Before generating:

- Read `AGENTS.md`.
- Read the latest `Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt`.
- Read `Docs/ArtDirection/Golden_Reference_Register.md`.
- Inspect Rattlebones and the current approved Dungeon, Forest and Ironvein artwork.
- Inspect the attached mockup and the current playable Ironvein scene.

The mockup is a **design and composition reference**, not a source image to crop into production sprites.

Use PixelLab's text-generation tools to build each asset separately. Choose suitable native sizes based on the actual scene and maintain consistent source-pixel density.

## 2. Everything that must remain unchanged

Lock the following:

- Entire top HUD and mine background.
- Player portrait frame and characters.
- Gem sprites and board cells.
- Board frame, including the integrated drills.
- Board dimensions and position.
- Bottom resonance meter and ability panel.
- Potion and bomb button artwork.
- Text, health bars and UI placement.
- All combat and gameplay systems.
- Other zones.

**Only the environmental dressing around the bottom-left and bottom-right corners may change.**

The central bottom HUD background must remain relatively quiet.

---

# STAGE 1 — Bottom-left mining station

## Design goal

Create a small, believable **dwarven ore collection station**, integrated into the lower-left cavern wall and floor.

The visual reference shows an ore container surrounded by amber crystals, mine timber and dark rocks.

### Generate these assets separately using PixelLab

**Asset A — Ore collection crate**

PixelLab text prompt:

"Create a compact wooden dwarven mining ore collection crate for a dark fantasy pixel-art game. The crate is reinforced with dark forged iron straps and rivets, filled with a few large angular glowing amber-orange crystals. Heavy practical timber construction, strong silhouette, clean connected pixel shading, dark brown wood, charcoal iron, warm orange highlights, isolated transparent sprite, no background, no painterly texture, no excessive tiny details."

**Asset B — Mining rock foundation**

PixelLab text prompt:

"Create a low, wide pile of dark bluish-purple underground cavern rocks forming the base of a dwarven ore collection station. Several large connected angular stones, deep gaps, restrained shading, subtle embedded amber ore deposits. Designed to connect naturally to an existing stone wall and floor. Crisp retro fantasy pixel art, clean large clusters, no disconnected rocks, no excessive cracks, isolated transparent game asset."

**Asset C — Short mine rail and timber supports**

PixelLab text prompt:

"Create a small section of old dwarven mining track embedded into a rocky underground cavern floor. Heavy wooden sleepers and dull iron rail segments, rugged but orderly construction, grounded perspective suitable for a 2D side-view fantasy game. Clean chunky native pixel art, dark wood and iron shading, no floating components, no background."

**Asset D — Small crystal fragments**

PixelLab text prompt:

"Create a compact set of amber-orange ore crystals partially embedded in dark mining rocks. One or two dominant faceted crystal tips with a few smaller fragments. Warm glow, dark stone shadows, restrained pixel shading, crisp intentional clusters, isolated fantasy game prop, no background."

### Assembly requirements

Build ONE coherent left-corner formation.

- The crate must physically rest on the rock or rail foundation.
- The rocks must connect naturally to the surrounding cave masonry.
- The crystals should emerge from stone or sit inside the crate.
- No floating crystal clusters.
- No disconnected pieces.
- Do not add an oversized mine cart or unnecessary wheels.

The formation should feel like a small ore-loading station built into the lower-left corner.

After cleanup, assemble it in Unity and validate the result.

---

# STAGE 2 — Bottom-right excavation support

## Design goal

Create a **dwarven excavation equipment station** on the bottom-right.

This side should complement the left, but it should NOT be a mirrored copy.

The mockup uses sturdy timber construction, mining tools, stone foundations and a smaller amount of glowing ore.

### Generate these assets separately using PixelLab

**Asset A — Reinforced timber support**

PixelLab text prompt:

"Create a sturdy diagonal wooden support beam attached to a dark stone mining foundation, reinforced with iron brackets, bolts and metal plates. Functional dwarven underground excavation construction, heavy timber, dark brown wood, charcoal-gray forged iron, clear structural connections. Crisp fantasy pixel art, strong readable silhouette, restrained top-left highlights, no isolated floating parts, transparent background."

**Asset B — Mining pickaxe**

PixelLab text prompt:

"Create a dwarven mining pickaxe with a sturdy dark wooden handle and a dull steel head, designed to lean naturally against a wooden support beam in a fantasy underground mining scene. Compact readable pixel-art prop, limited clean material shading, strong silhouette, isolated transparent background."

**Asset C — Excavation crate and equipment**

PixelLab text prompt:

"Create a small reinforced dwarven mining supply crate, dark brown wood with forged iron corners and bolts, suitable for resting beside an excavation support in a cave. Heavy functional construction, low compact profile, readable pixel-art shading, slight worn edges, clean native pixels, isolated sprite, no background."

**Asset D — Embedded ore formation**

PixelLab text prompt:

"Create a small amber-orange crystal deposit growing naturally from dark bluish-purple cavern rock at the base of a reinforced wooden mining support. Large connected rock forms, only a few bright orange crystal points, restrained warm highlights, clean retro fantasy pixel art, no disconnected fragments, transparent background."

### Assembly requirements

- Timber must connect to the rocky foundation.
- The pickaxe must visibly lean against the support or rest on the ground.
- The crate must have a believable resting surface.
- Ore must emerge from the rock rather than float beside it.
- The entire formation must feel physically connected.

Do not add unrelated machinery, giant wheels, or new hanging lanterns.

---

# STAGE 3 — Connect both corners to the environment

This stage is especially important.

The previous attempts sometimes looked like isolated props pasted onto an existing background.

**That must not happen again.**

Integrate the structures using:

- Continuous dark cave masonry.
- Short, connected rock foundations.
- Timber braces attached to the existing mine structures.
- Small iron brackets or anchors.
- Subtle contact shadows.
- Limited warm light reflecting off nearby surfaces.

The two corners should share the same material family without having identical shapes.

### Composition

**Left:** ore storage and collection.

**Right:** excavation tools and structural support.

Both should visually connect to the existing lower cavern and board-side architecture.

Keep the area directly behind the resonance panel clear.

---

# STAGE 4 — LibreSprite color and shading cleanup

For each selected PixelLab asset:

1. Preserve the original generated source.
2. Open the candidate in LibreSprite.
3. Clean accidental isolated pixels.
4. Simplify excessive texture noise.
5. Normalize the material ramps.
6. Repair broken silhouettes.
7. Remove unnecessary colors.
8. Verify binary alpha.
9. Verify the native pixel dimensions and alignment.
10. Export without blur or resampling.

### Material direction

- **Stone:** dark slate-blue and muted purple.
- **Timber:** warm dark browns with restrained highlights.
- **Iron:** charcoal-gray, dull silver and dark separations.
- **Ore:** amber-orange, with only limited bright yellow highlights.

Do not apply the bright gem palette across the entire environment.

Rattlebones is the rendering-quality reference, not a requirement to reuse his exact palette or canvas dimensions.

Keep the material shading simple and readable.

---

# STAGE 5 — Unity assembly and visual QA

Integrate both completed corner formations into the existing Ironvein scene.

Use separate reusable sprites and modular parts wherever practical.

### Validate the following

- Rocks are connected to the actual cavern floor and wall.
- Wooden supports look structurally plausible.
- Props have believable contact points.
- No objects float.
- No large unnatural gaps between asset components.
- Ore deposits look embedded.
- Both sides feel balanced but asymmetrical.
- No unnecessary bright orange clutter.
- No blocking of HUD text or buttons.
- No changes to gameplay geometry or protected artwork.
- No blurred or distorted pixels at supported screen sizes.

Inspect the actual playable Game scene at standard and tall-phone aspect ratios, not just the isolated art preview.

Compare with the attached mockup at native display size.

If the result still looks scattered, regenerate or reconstruct the specific failing assets rather than adding more decorations.

## Final deliverables

Provide:

1. Finished bottom-left mining station.
2. Finished bottom-right excavation support.
3. Individual cleaned native sprites and editable LibreSprite sources.
4. Actual Unity screenshots showing both corners in the full game.
5. Before-and-after close-ups.
6. PixelLab generation ledger.
7. Confirmation that protected UI, characters, gems and other zones remain unchanged.

## Execution permissions

You have permission to proceed through every stage without requesting routine approval.

Use as many PixelLab generations as needed to achieve the desired quality, but avoid spending credits on duplicates or changes that LibreSprite can handle directly.

Work on a dedicated branch. Preserve all existing approved assets and unrelated changes.

**Do not merge anything into main.**

Stop when both corners are fully implemented and visually validated, and present them for my approval.

## Final artistic target

The lower-left and lower-right corners should look like **two connected parts of a working dwarven excavation**, not isolated glowing crystals decorating the edges of the screen.

I want the existing Ironvein zone to feel more constructed, grounded and complete while preserving its current gameplay readability.