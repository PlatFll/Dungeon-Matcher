# Crystal Resonance ownership

Implemented in stages 01–09; objective Stage 10 checks and Game-scene samples
have executed. This file describes ownership. See the separate
[validation report](../Validation/CRYSTAL_RESONANCE_V1_FINAL.md) for measured
results, sample limitations and remaining acceptance work.

| Responsibility | Existing/new owner and boundary |
|---|---|
| Accepted swaps, clear sets, damage/energy entitlement | BoardController and existing combat contexts; unchanged authority |
| Score/mastery observation | BoardController.ProgressionReceipts emits monotonic receipts, never requests a clear |
| Score and local charge | RunSession-owned CrystalResonanceRuntime and version-1 value snapshot |
| Encounter definition selection/spawn | WaveController.ResonanceEncounters inside the existing spawn owner; ZoneDefinition data |
| Card generation/apply | Existing RunUpgradeCoordinator/Generator/Runtime; ordered Resonance notch queue |
| Durability | Existing RunContinuation, replay tape and AccountProgression atomic journal |
| Travel/reset | Existing ZoneTravelController and detached TryCommitZoneTravel snapshot |
| HUD | Existing WaveTrackerUI/layout plus observing CrystalResonanceHud; no gameplay authority |
| Telemetry | Opt-in local event listeners; Editor-only driver/writer, no production network sink |

## Identity, settlement and gates

Encounter identity is run ID + visit + global wave. Original members include
actors killed during entrance; summons do not add base rewards. Board receipts
carry monotonic sequence, accepted action and intentional provenance. Snapshot
lastReceipt prevents replay credit duplication, restored board sequence never
rewinds. Board score is provisionally capped at 600 per encounter. Each mastery
bit earns once: deliberate 4+, special setup/chain, targeted counter. Automatic
cascade/environment/drill clears cannot manufacture new deliberate attribution.

WaveCompleted synchronously sets pendingWave and holds IWaveProgressionGate.
Runtime waits for board busy, wave cleanup, pending deaths/entrance, accepted-move
clock and journal retry barriers. Bank once by activeWave/lastBankedWave; queue
crossed notches and lock the capstone if appropriate. Save pending state holds
progression until a complete checkpoint succeeds. Defeat discards pending charge.

The card coordinator waits for this safe bank, then holds input/gate across the
whole queue. It saves the exact offer before input, journals selection before
applying, consumes the head notch, saves the resulting build and releases only
when the queue is empty. Existing atomic action recovery handles interruption.
PortalReady itself holds spawning until ZoneTravelController claims its gate,
preventing a next-wave race. Existing account victory remains a separate owner.

## Fixed zone cadence

CrystalResonanceTuning.BaseCharge resolves category awards for the existing
banking owner. Shared ordinary/pressure awards are 4/5; optional zero-default
ZoneDefinition overrides inherit those values. Court serializes 5/6 explicitly.
Miniboss/major awards stay 10/14 everywhere. This changes neither enemy stats nor
the per-visit gates and never reads player level, power, elapsed time or hazards.
The whole original formation still contributes only its highest category once.
See the final tuning decision for actual before/after runs and remaining gaps.

## RNG and saved selection

Board refill, encounter composition, card offers and travel retain their saved
streams. A new capstone stream hashes the run ID, samples each visit threshold
78–88 once, and produces a persisted formation seed at approach. Exact capstone
recipe/member IDs, kind, wave, encounter ID and phase are saved. Resume does not
reroll. Depth is global and never resets on travel. Seen mechanics/leaders and
recent capstones carry across visits. Major cooldown is two visits.

Ordinary recipes use current-zone eligibility, introduction safety, whole threat
and role budgets, tide compatibility and recent-recipe exclusions. Bounded
procedural fallback uses weighted members/adjacent families. Capstones select a
legal kind then an authored whole formation. Repeatable same-zone elite reserves
remain after unique leaders are spent. Missing legal data raises an explicit
error instead of silently spawning foreign enemies or skipping a capstone.

## Adding content

Add approved EnemyDefinition references to the zone roster and depth pool;
declare min/max tiers, per-tier weights, family and summon-only status. Add
coherent ordinary teaching/practice recipes and budget the entire capstone with
escorts. Define prerequisites and unique-leader/required-escort rules. Ensure
every tier has ordinary relief and repeatable elite reserves after all named
leaders are seen; test dry/wet Court contexts. Preserve response placement caps.
Do not force a named leader every visit or invent content to fill absent tiers.
Update the encounter matrix, design status and affected validation evidence.

## Source review and limits

No scene, prefab, enemy sprite, player definition, shop, wallet, health-bar or
shield asset is changed. New scripts have .meta files; runtime services attach
through existing RunSession/WaveTracker owners. Five existing ZoneDefinition
assets hold added serialized profiles with existing enemy GUIDs. No second board
resolver or travel implementation was introduced. The approved split-pink gem,
silver plaque and existing audio clips are reused; see HUD_SOURCE_MANIFEST.md.

The required compiler validator, affected regressions, save/travel edge tests,
five-zone portrait layouts and real Game input samples have executed in Stage 10.
These checks do not establish universal fairness or human enjoyment. Pacing and
natural deep coverage remain bounded by the recorded samples; see the validation
report. Historical schedule-dependent diagnostic fixtures now explicitly choose
the actors relevant to their contract, without weakening production rules.
See VALIDATION_PLAN.md and SAVE_MIGRATION.md.
