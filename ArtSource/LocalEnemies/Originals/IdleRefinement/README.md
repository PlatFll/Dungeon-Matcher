# Supplied idle acting drafts — 2026-09-21

These files preserve the user's new animation inputs before cleanup.

- `Miner_Supplied.png`: copied unchanged from Downloads/MinerIdle.png, twelve 96×80 cells. The first cell is blank; it is not an animation pose.
- `BasketVillager_Supplied.png`: copied unchanged from Downloads/BasketVillager.png, eleven 64×64 cells.
- `BarricadeVillager_Supplied.png`: copied unchanged from Downloads/BarricadeVillager (1).png, eleven 64×64 cells.
- `*_Live.aseprite`: saved from the corresponding already-open LibreSprite tabs, without overwriting the user's originals. The Miner tab was a single wide sheet; the villagers were eleven-frame animations. Their pixels matched the supplied sheets.
- `BarricadeVillager_Ability_Before.aseprite`: exact pre-refinement native ability from commit `061c5eb`, retained so the knee-only correction is reproducible and the unchanged head/tool/plank pixels can be checked.

These are acting inputs, not palette authorities. Production ready poses and `Scripts/palettes.json` retain that authority. `Scripts/refinement_materials.json` records explicit generated-color cleanup. `Manifest.json` fingerprints these captured sources.
