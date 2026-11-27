# Crystal Resonance implementation checkpoint

Stage 1; branch `codex/crystal-resonance-zone-director`.
Base updated main: `fac476ad15300f68cae48ee68385323782667439`.
Prior user edits remain untouched in the other checkouts.

Repository audit and design reconciliation complete (source inspection only).

Sources: START_HERE, master, stage 01, V3 brief, boundaries; current AGENTS/RTK;
entire PROJECT_STATE; relevant GAME_DESIGN, PROGRESSION_PACING, ARCHITECTURE,
BALANCE_V1 and historical validation, RUN_UPGRADE_SYSTEM, ZONE_DESIGN_RULES,
Forest/CRYSTAL_TRAVEL. Inspected production Game bindings and BalanceV1 resource.

## Owners and integration decisions

- WaveController retains construction and full-formation completion including escorts,
  summons, entrance and death cleanup. A run-owned CrystalResonanceRuntime holds
  IWaveProgressionGate until board, combat move coordinator and journal settle;
  only then publishes a banked result into existing draft/travel gates.
- Result identity: run ID + visit + monotonic global depth; save active identity,
  last banked depth and receipt cursor. Begin before first spawn; WaveStarted currently
  occurs after entrance. BoardController emits deterministic receipts from genuine
  clear reporting, with explicit deliberate flags and physical counter evidence.
- RunSession owns the component; typed nested snapshot version zero means legacy,
  preserving outer v1/v2 clock schemas. Existing draft/travel completes without
  retroactive rewards. Score persists; visit charge resets only in the detached
  destination snapshot committed by RunContinuation.
- RunUpgradeCoordinator remains card owner. Saved ordered notch queue and active
  notch ID replace wave cadence, including several offers at one completed depth.
- WaveController uses current-zone, global-depth legal recipes through its existing
  spawning path. Capstone RNG is separate from encounter/draft/destination/board;
  selected formation IDs and seed remain locked across Continue.
- ZoneTravelController retains pending/cover/commit/restore/reveal and hazard cleanup;
  trigger becomes full committed charge AND defeated capstone AND closed drafts.
- Shared WaveTrackerUI/silver presentation remains the HUD owner. Inspect crystal
  provenance and art guide before stage 07; preserve approved source pixels.

## Explicit reconciliation

The user supersedes wave card dates, guaranteed King windows, local-wave apex
loops/rematches and forced Dungeon returns. King remains a high-depth Dungeon
option with authored Minister escort. Kingdom progression remains thematic.
Guild content is absent and will not be invented. Economy, HP/shield, ability
rules, saved timing units and source-zone hazard semantics remain intact.
Revise conflicting owner-doc guarantees in stage 09.

## Risks

WaveCompleted precedes board settlement. Draft hide callbacks are synchronous;
one selection must not erase a queued offer. JsonUtility absent nested objects
need version-based migration. Local zone recipe windows require explicit global
depth metadata. No code was changed in stage 01.


Tests/compilation/runtime validation: **NOT RUN — deferred to Stage 10**.
No Unity process, compiler, validator, test or screenshot has been invoked for stages 01–09.
Art: existing approved sources retained; no new locked production sprites.
Next: stage 2 in the attached prompt pack. No intermediate PR or merge.
