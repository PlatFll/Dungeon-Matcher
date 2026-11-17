# Crystal Resonance implementation checkpoint

Stage 3; branch `codex/crystal-resonance-zone-director`.
Base updated main: `fac476ad15300f68cae48ee68385323782667439`.
Prior user edits remain untouched in the other checkouts.

Connected committed 12/40/70 notches to existing RunUpgradeCoordinator.
Resonance runs bypass wave-date drafts. Runtime holds progression across every
queued notch; coordinator holds board input for each offer. Same-wave multiple
offers use activeNotch identity, recorded selections and the existing draft RNG.
Offer plus notch are checkpointed before presentation. Empty legal pools consume
only that notch safely; partial offers retain existing honest behavior.
Temporarily hidden offers reopen without rerolling; persisted offers are reused.
First-run first notch retains Board/Ability/Survival directions; refinement remains
once per run. Legacy saved visible offers restore with notch zero and finish once.
Source reviewed only; stage 08 will add lifecycle/Continue integration cases.


Tests/compilation/runtime validation: **NOT RUN — deferred to Stage 10**.
No Unity process, compiler, validator, test or screenshot has been invoked for stages 01–09.
Art: existing approved sources retained; no new locked production sprites.
Next: stage 4 in the attached prompt pack. No intermediate PR or merge.
