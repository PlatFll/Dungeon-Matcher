# Master Contract — Frostwake Peaks + New Playables

## 1) Read-first and repo safety

Before any change:
- read `AGENTS.md` and any applicable local project instructions;
- read current:
  - `Docs/PROJECT_STATE.md`
  - `Docs/GAME_DESIGN.md`
  - `Docs/ARCHITECTURE.md`
  - `Docs/FUTURE_ZONE_LOCKS.md`
  - `Docs/STATUS_EFFECTS.md`
  - `Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt`
  - `Docs/ArtDirection/Golden_Reference_Register.md`
- inspect current Mountain/Frostwake docs or placeholders if already present;
- inspect current playable-character architecture and character definitions;
- inspect current VFX/SFX asset organization.

Use a dedicated branch/worktree.
Preserve unrelated user changes.
Do not merge without approval.

## 2) Scope

This task covers:
- revised **Frostwake Peaks** direction with boss details;
- new playable **Dungeon Attender Minotaur**;
- redesigned/new playable **Kobold Gunner**;
- all required art, animation, VFX, SFX, and docs;
- gameplay hooks and validation.

This task does **not** authorize unrelated combat-system redesign beyond what is necessary to support these additions.

## 3) PixelLab / art workflow rules

For any new or replacement enemy/playable sprite generation:
- read the canonical art guide first;
- use actual golden-reference imagery where the endpoint supports style/reference inputs;
- default primary style anchor: **Rattlebones**;
- supporting anchors: current approved/registered supporting references as appropriate;
- preserve target identity/anatomy/equipment while using references for outline/shading/detail discipline.

Do not rely on text-only “Rattlebones style” if the endpoint supports actual image references.
Record generation lineage, prompts, settings, selected/rejected outputs, and hashes.

The user explicitly allows broad PixelLab use here.
Still, keep artifacts organized and select deliberately rather than replacing entire casts casually.

## 4) Deliverable structure

Astra should produce:
- implementation commits in coherent tranches;
- updated docs and manifests;
- generated/edited art assets;
- review galleries/comparison sheets;
- screenshots/GIF previews where appropriate;
- final report with branch/HEAD/tests/results.

## 5) Review philosophy

When a new design is required:
- preserve the game’s whimsical nostalgic fantasy identity;
- prioritize readability over fussy detail;
- preserve strong character identity;
- prefer the smallest effective redesign.

## 6) Usage safety

Monitor usage if measurable.
At low usage, checkpoint safely instead of forcing completion.
Do not burn the final usage margin on broad generation sprees without documenting the stopping point.
