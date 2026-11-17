# Crystal Resonance implementation checkpoint

Stage 5; branch `codex/crystal-resonance-zone-director`.
Base updated main: `fac476ad15300f68cae48ee68385323782667439`.
Prior user edits remain untouched in the other checkouts.

Added independent persisted threshold/formation RNG, current-zone weighted
elite/miniboss/major selection, cross-visit major cooldown and recent-formation
avoidance. Scheduled ID/members/seed/wave/run-visit encounter identity are locked;
the existing next spawn uses that exact formation. Completion requires the normal
WaveController full-formation/summon/death-effect barrier and matching identity.
100% is sealed until that capstone is defeated; early capstone victory continues
ordinary encounters until full. Explicit same-zone reserve formations cover
exhausted named pools without restoring forced King waves. King retains Royal
prerequisites and mandatory Minister. Neutral anticipation event is ready for HUD.
Authored seed/tier/uniqueness tests; NOT RUN. Travel trigger is the next stage.


Tests/compilation/runtime validation: **NOT RUN — deferred to Stage 10**.
No Unity process, compiler, validator, test or screenshot has been invoked for stages 01–09.
Art: existing approved sources retained; no new locked production sprites.
Next: stage 6 in the attached prompt pack. No intermediate PR or merge.
