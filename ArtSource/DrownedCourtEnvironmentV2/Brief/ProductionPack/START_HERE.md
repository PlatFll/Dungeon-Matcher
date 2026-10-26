# Dungeon Matcher — Drowned Court / Sunken Marches PixelLab Production Pack

**This is an execution pack for Astra, not finished art.** It includes the actual user-supplied Drowned Court mockup, current Dungeon and Magical Forest production screenshots, a historical style board, reference-only crops, 13 sequential stage prompts (00–12), production rules, PixelLab-ready description recipes, QA checklists and tracking templates.

## The visual decision
Drowned Court is an **ancient submerged royal court amid coral-covered ruins**, not a generic bright aquarium. Its identity: deep blue/teal underwater space; broken and whole stone columns; a distant trident-bearing monarch statue; arches and lost-city masonry; teal ancient stone frames; kelp, barnacles, coral and shell growth that are *anchored* to stone; slow bubbles and shafts of light. Clear combat silhouettes and bright gems remain primary.

**Division of authority:**
- `references/01_Drowned_Court_Visual_Target.png`: **thematic/compositional target**, NOT tileable native art and NOT authorization to reproduce its character sprites.
- `references/02_Current_Dungeon_Production_Benchmark.png`: benchmark for actual game pixel cleanliness, shared controls/layout, concise masonry.
- `references/03_Current_Magical_Forest_Production_Benchmark.png`: benchmark for coherent themed side architecture, differentiated zone atmosphere and subtle ambience.
- Current repo `Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt`, `Docs/ArtDirection/Golden_Reference_Register.md`, `AGENTS.md`, and `Docs/DROWNED_COURT.md`: implementation/style authority. Read CURRENT files; these may evolve after pack creation.
- Current live Dungeon and Forest game scenes: placement/scaling and verified production readiness standard.

## Start it
1. Attach the ZIP to Astra or extract it into the workspace OUTSIDE `Assets` until reviewed.
2. Paste `SEND_TO_ASTRA_FIRST.txt`.
3. Astra executes `prompts/00_PREFLIGHT_AND_PIXEL_PIPELINE.md` **only**, then stops for review.
4. After you approve, tell Astra `Execute prompts/01_... ONLY` and so on in the order in `STAGE_INDEX.csv`.
5. Each stage concludes with screenshots/native 1× previews and an honest quality checkpoint. Stage 12 is **optional and requires explicit user approval**.

## Hard rules
- Never fabricate exact PixelLab tool results; check installed tool schemas, current balance, model/cost/size restrictions before running jobs.
- Generate new **individual asset families**, not a flattened full-screen image. When reference image is upscaled, prepare native-clean references first (PixelLab's `/unzoom` may help, check results). Actual game art remains the native source of pixel discipline.
- Use PixelLab → exact-cel LibreSprite editing/cleanup → Unity art-only scene → real Unity captures (and only after final art approval, production). All source art stays editable.
- The current Court already has playable content, flooding/AIR and native VFX. This is a **visual rebuild**, not a new Drowned Court gameplay implementation.
- Preserve shared `WAVE` and `GUIDE/settings` interaction, global bottom resonance/potion/bomb controls, gems/board cells, characters/enemies, health bars, shields, AIR mechanics, save/resume and other zones. Zone-specific frame and surrounding material MAY change, shared buttons and their logic must NOT.
- Do not infer global direction changes from isolated experiments; record candidate/approved/rejected status. Do not merge without the user saying so.

## Deliverable
An original **modular Drowned Court zone artwork kit** with native PNG + editable LibreSprite source + source lineage, actual Unity art-only composition, animated ambient study, before/after/gameplay preview plan, credit ledger and review gallery. Future production integration is opt-in.
