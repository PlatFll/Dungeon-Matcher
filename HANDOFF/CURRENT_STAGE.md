# Crystal Resonance implementation checkpoint

Stage 6; branch `codex/crystal-resonance-zone-director`.
Base updated main: `fac476ad15300f68cae48ee68385323782667439`.
Prior user edits remain untouched in the other checkouts.

Live travel now requires the committed full meter, defeated locked capstone,
settled combat/board, completed cards, living player and no overlapping modal.
Reuses existing durable destination RNG, save-before-smoke, commit and reveal.
Destination validity now depends on actual profile/fallback content, not apexEnemy.
Atomic detached arrival resets visit fields only; score and all existing player,
card, depth and board-identity owners persist. Existing zone cleanup unchanged.
Added nested-schema guards, zero-charge legacy migration and leader history,
with separate already-earned departure versus already-committed arrival policy.
Save failures hold progression; refined offers snapshot their updated exact cards.
Migration details: Docs/CrystalResonance/SAVE_MIGRATION.md. No tests executed.


Tests/compilation/runtime validation: **NOT RUN — deferred to Stage 10**.
No Unity process, compiler, validator, test or screenshot has been invoked for stages 01–09.
Art: existing approved sources retained; no new locked production sprites.
Next: stage 7 in the attached prompt pack. No intermediate PR or merge.
