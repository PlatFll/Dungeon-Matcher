# Stage 10 validation plan — not yet executed

Stages 01–09: no compilation, Unity launch, tests, validation commands,
screenshots or playtests. This file is preparation, not evidence.

1. Run Tools/Validate-Unity.ps1, preserve exact command/source commit and log.
2. Run CrystalResonanceStateTests/DirectorTests plus affected board attribution,
   card lifecycle, continuation/travel, all five zone and shared UI regressions.
3. Add/execute explicit Game-scene edge fixtures: whole formation surviving
   leader death; sealed 100; three queued drafts; restore/refine/empty catalog;
   duplicate receipts; failed checkpoint; old migration; all source cleanup;
   missing VFX/audio. Label forced states as diagnostics, never pacing evidence.
4. Use CrystalResonancePlaytest.Run with .utmp/CrystalResonance/playtest-config.json.
   Five policies in HumanLikePlaytestDriver read visible legal moves/telegraphs
   and submit normal journaled swaps, ability, consumable and card commands.
   Disposable baseline profiles and initial zone selection are recorded. No
   direct HP/board/charge mutation, free energy, kill or teleport in this runner.
   Run at x1; 6–11 seconds for cards, visible-state decision delays per policy.
   Output: actual Game screenshots, per-encounter checkpoints, events, action
   logs, complete visits and separate deaths/censored outcomes under .utmp.
5. First pass: one completed visit per zone if survival permits and novice early
   failure; report failures honestly. Final target six completed visits per zone
   across multiple seeds/policies, natural deep Dungeon + other-zone revisits.
   Additional deep-tier fixtures do not count as naturally reached depth.
6. Summarize medians/quartiles/ranges only for completed visits; count failures
   separately. Target 300–420s typical successful visits including card reading
   and actual cover/reveal; no timer, HP padding or power-based compensation.
7. Compare same seeds after smallest causal changes; rerun all affected tests
   and required validator after final tuning. Five phone/safe-area layouts,
   paused/reduced-motion/missing-reference screenshots and real-device test if
   available. Distinguish observed screen behavior from untested audio/device.

Telemetry is opt-in local event observation, not a network service. No large
per-frame event allocations. Counters separate live combat/draft/travel/pause;
the runner's completed-visit wall clock includes reload and incoming reveal.
Snapshot clocks alone restart on arrival and are not the completed-visit wall
clock authority. JSON checkpoints include replay RNG only for reproduction;
the decision policy never reads future RNG or future formations.

Real-human acceptance questions: Was the next reward predictable? Could you
answer the capstone's warnings? Did card choices affect your play? Did any
formation feel incoherent or unfair? Did you want another visit? Bot outcomes
cannot answer whether the game is enjoyable.
