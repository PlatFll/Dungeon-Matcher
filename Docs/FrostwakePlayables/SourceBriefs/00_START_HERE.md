# Dungeon Matcher — Frostwake Peaks + New Playables Pack

This pack briefs Astra to implement:

1. **Frostwake Peaks** (Snowy Mountain zone) production kickoff direction,
2. a **boss revision** for Jarl Haldra,
3. two **new playable characters**:
   - **Dungeon Attender Minotaur**
   - **Kobold Gunner** (redesign required)
4. all related art, VFX, SFX, gameplay hooks, and validation.

## Attachments to give Astra alongside this pack

Attach this ZIP **and** the two concept images from chat:
- **Minotaur concept** (the horned brown/gold minotaur image)
- **Existing kobold concept** (the small red-hatted gunner image)

The Minotaur concept should be treated as the identity inspiration and silhouette direction.
The Kobold concept should be treated as a replaceable placeholder identity source only; its final art should be redesigned.

## Deliverables Astra should produce

- branch/worktree with implementation in safe commits;
- PixelLab-backed sprite generation / revision where required;
- integrated gameplay changes;
- implemented art/VFX/SFX;
- updated docs and manifests;
- review galleries / screenshots / exported comparisons;
- a final report.

## Important art rule

Astra must read the **current** canonical art direction and golden-reference register before doing art:
- `Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt`
- `Docs/ArtDirection/Golden_Reference_Register.md`

Rattlebones is the **primary** golden standard for rendering discipline.
Supporting reference family remains the established human/approved cast.
This does **not** mean every sprite should copy Rattlebones anatomy or stance.

## Scope note

This pack authorizes implementation work for these additions.
If Astra discovers a serious architectural conflict, it should stop and report clearly rather than improvising a dangerous rewrite.
