# Crystal Resonance implementation checkpoint

Stage 4; branch `codex/crystal-resonance-zone-director`.
Base updated main: `fac476ad15300f68cae48ee68385323782667439`.
Prior user edits remain untouched in the other checkouts.

Extended ZoneDefinition/ZoneTestEncounter with serialized per-zone depth pools,
five tiers, strict min/max, per-tier weights, family compatibility, authored
ordinary/elite/leader recipes, prerequisites and whole-formation budgets.
WaveController's existing spawn routine selects these recipes for Resonance
runs; legacy fixed/milestone RNG is bypassed. Actors still use existing slots,
stat curves, summons and death completion. Same-zone constrained weighted patrol
and legal relief replace cross-zone fallthrough. First unseen special gets a
normal escort, at most two actors; no multi-unseen-special introductory formation.
Dungeon locals end after tier 1; guards/knights/royals overlap. All five zone
assets now have explicit pools and recipes. Named formations are reserved for
stage 05. Source matrix: Docs/CrystalResonance/ENCOUNTER_MATRIX.md.
Unchanged: enemy stats/abilities/art, global HP curve, hazards and GUID identities.
Content gap: non-Dungeon deep tiers reuse strongest existing legal kits, not new
Guild/late enemies. No runtime/data validator has executed.


Tests/compilation/runtime validation: **NOT RUN — deferred to Stage 10**.
No Unity process, compiler, validator, test or screenshot has been invoked for stages 01–09.
Art: existing approved sources retained; no new locked production sprites.
Next: stage 5 in the attached prompt pack. No intermediate PR or merge.
