# STAGE 09 — Bubbles, slow kelp sway and light-shaft ambience

**Execute this stage ONLY.** Read `START_HERE.md`, `GLOBAL_LOCKS_AND_ART_DIRECTION.md`, `REFERENCE_MAP_AND_MATERIALS.md`, `PIXELLAB_TOOL_SELECTION.md`, `QA_AND_APPROVAL.md` and the current repository authorities first. Reference images are examples for art direction only, NOT native sprites. Work in the dedicated isolated worktree.

**Use** bubble/kelp reference from mockup and current Court native effects from `Docs/DROWNED_COURT.md`. This stage is **ambient scenery only**, not AIR/gem gameplay VFX or enemy animations.

## Separate effects and timing
1. **Air bubble streams**: few discrete clusters rising from rock crevices on the OUTER board gutters; deliberately stagger sizes, emitters and timing. Hollow rings/transparent interior, not an opaque wall of blue circles.
2. **Seaweed tips**: anchor roots and stone bases entirely still; gently sway loose blades by 1px around fixed attachments, asynchronous slow rhythm.
3. **Far light rays**: subtle small native-frame brightness cycling in the water only; do not apply a filter over UI/gems.
4. **Distant fish silhouettes**: optional 1–2 slow isolated background motions, never past character faces or countdown UI.
5. **Suspended water particles**: rare, tiny low-value motes; no uniform glittering texture.

### PixelLab description A — bubble cluster
> Four to six small distinct hollow cyan underwater air bubbles, staggered vertically, crisp hard pixel rim rings with dark transparent centers, isolated sprite set for quiet ambient rise from a stone vent. Vary sizes but keep small. No painterly glow bloom, no game gem, no text, no background, no opaque water filter.

### PixelLab description B — kelp still for motion
> Short rooted ribbon of two underwater green kelp fronds with connected holdfast and long tapering leaves, fixed base suitable for 1-source-pixel gentle underwater sway, dark teal/olive colors and sparse upper-left lighting, crisp native pixel sprite, no decorative knots or random blobs.

## Animation technology
Prefer controlled native LibreSprite 4–8-frame cels or explicit discrete offset timeline. Use PixelLab animation only if it keeps form and roots connected. Preserve source pixel grid, alpha, fixed pivots. Looping bubbles may be spawned at very low frequency; leave gaps in temporal schedule. Handle pause, Reduced Motion and flood-state presentation consistent with current project code—do not make these animations grant AIR or replace existing gameplay VFX.

## Validation
Show one real Unity 6–12 second sample at normal speed; prove UI/gems/actors unchanged frame to frame except approved ambient regions. Verify reduced-motion static state and multiple phone scales. Stop.

## Required stage handoff
Create/update candidate asset files, manifest entries, PixelLab usage ledger, a native 1× contact sheet, nearest-neighbor zoom inspection, reference comparison and actual Unity art-only scene screenshot when applicable. Distinguish objective PASS/FAIL from artistic review. Describe exact preserved files and any unresolved defects. Do NOT merge or proceed to the next stage without explicit user approval.
