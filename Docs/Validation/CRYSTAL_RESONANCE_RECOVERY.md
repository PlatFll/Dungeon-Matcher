# Crystal Resonance recovery and targeted tuning

Status: implementation and validation in progress; PR #234 remains unmerged.
Starting source: 7e306f72b26ad5b9bb2458cf973bb73cc7dcbc07.

## Recovered evidence

The new read-only inventory re-parsed original completion markers, cases, results,
visits, actions and events, and decompressed closed native journals. No prior
Unity, runner, validation shell or completion waiter remained alive. Only unrelated
art-tool servers were present. No old test was restarted, overwritten or deleted.
`CrystalResonanceRecoveryEvidence/recovered-inventory.json` records every case,
duration, outcome, evidence path and SHA-256 provenance. Missing source manifests
are explicit, never reconstructed as a guessed source commit.

| Batch | Cases | Status | Natural defeats / alive limits | Recorded case minutes |
|---|---:|---|---|---:|
| 20261117-014621 | 1 | FAILED Editor exception, despite one result | 0 / 1; not a technical pass | 3.20 |
| 20261117-015703 | 6 | Completed historical | 1 / 5 | 24.19 |
| 20261117-030133 | 15 | Completed historical | 10 / 5 | 179.59 |
| 20261117-062332 | 11 | Completed historical | 2 / 9 | 59.81 |
| 20261117-073354 | 13 | Completed historical | 4 / 9 | 65.92 |
| 20261117-084917 | 4 | Completed historical | 1 / 3 | 17.88 |
| 20261118-010917 pilot | 5 | Completed; old wrapper false failure preserved | 5 / 0 | 48.17 |
| 20261118-020131 matrix | 20 | Completed | 17 / 3 | 264.59 |
| 20261118-063014 deep | 4 | Completed | 4 / 0 | 86.36 |
| 20261118-080102 spread | 25 | Completed | 22 / 3 | 329.52 |
| 20261118-135947 Court | 8 | Completed | 8 / 0 | 85.48 |
| pass1 / probe1 | 2 partials | No completion/result, no live owner | Incomplete; excluded | Unknown |

The older 20261117 records include direct-zone/diagnostic starts and different
tuning; they are not pooled with Dungeon-first refinement or final-source runs.
No expected overnight batch is missing. No current stalled or timed-out process
was found. The historical failed Editor session and unfinished probes remain open
evidence limitations. Original summary/audit archives and job records remain in
`CrystalResonanceRefinementEvidence`; this audit adds no fake successful marker.

## Prioritized findings and changes

1. **Confirmed selector defect:** recipe-label anti-repetition allowed adjacent
   variants of the same special. The final prior Court batch contained seven
   such transitions among 55 encounter pairs; the spread batch had 11/47 Court,
   33/156 Frostwake and 7/102 Forest. These include repeated Learn/Patrol Moray,
   healing and shield kits. Prefer a different special when a legal alternative
   exists; preserve normal escorts, depth/tide budgets and exhaustion fallback.
2. **Confirmed missing pacing safeguard:** the Resonance route bypassed the old
   post-leader relief owner. In the prior spread batch all six post-capstone
   Frostwake encounters were pressure formations, including Shieldmaiden (65 HP
   damage) and combined Rimecarver/Hearthkeeper (60). Restore one normal-only,
   current-depth relief encounter if further charge is required. No forced extra
   fight or timer is added, and rewards remain unchanged.
3. **Confirmed simulation defect:** the visible cascade heuristic applied a
   two-piece swap to three-piece underwater/Slippery rotation. A fixed visible
   board reproduced a phantom three-gem follow-up. Use the existing visible final
   cell map; no hidden RNG, future refill or second board resolver is introduced.
4. **Confirmed measurement gap:** old cumulative HP-damage events lacked source.
   Record the already-calculated last-hit description plus current AIR, remaining
   flood moves, free bubbles and coffer charges. This observer cannot change damage.
5. **Potential balance concerns:** Court low-level attrition, heavy escorts,
   Forest long first-card cases and late Dungeon returns require the fresh batch.
   No HP, damage, oxygen, ability, shield, economy or charge number is changed.
6. **Acceptable power expression:** the 3.693-second historical encounter was
   Dungeon's depth-59 Royal elite, not Court (the earlier report misnamed it).
   It ended after one accepted move, no incoming damage and no special cast.
   Source-separated action/build evidence supports an earned strong combo; no
   minimum duration or compulsory signature cast is warranted.

The Game Design summary also retained superseded global 5/6 base awards. It now
matches the authoritative pacing/data 4/5 shared, 5/6 Court values without any
reward change. See baseline-compositions.json for raw-derived encounter details.

## Regression evidence so far

The first four-case fixture attempt passed 2/4; its new test setup used clustered
xorshift seeds and captured before a movement-rule change settled. Correcting
the setup (not assertions or gameplay) produced the intended three reproduced
failures and one fallback pass against unchanged production code. Both original
XMLs/logs are retained. Final-source results will be recorded after correction.

## Fresh final-source protocol

The committed final-protocol.json predeclares 13 natural Dungeon-first attempts:
five profiles on the previously affected Court-entry named seed; five independently
derived seeds across characters/levels and profiles; three level-10 deep attempts.
The source commit and full manifest will accompany every run. Existing data is
never overwritten; natural failures and observation limits count separately.
No timing target or successful-zone quota is used to force outcomes.

## Remaining acceptance limits

Automated accepted-input policies cannot establish human learning, touch comfort,
reading, fatigue or enjoyment. Queen/late-visit coverage is not guaranteed by a
natural route. Retain unsuccessful attempts and report those gaps. Physical
Android touch/audio/performance remains human acceptance work. Do not merge.

## Completed thirteen-case pass and final focused tuning

Source `90bc53b9a872c3cd895ed4e4fdc3ee71f277ba60` completed all 13 natural cases:
12 defeats, one 45-minute living limit, 25 completed visits and 3,270 durable
commands. The native audit found no failures. Required Unity validation, 56
focused tests, 272 distinct balance tests and Android/Windows builds passed.
Original results, full source hashes, checkpoints and input journals are archived
as `batch13-*`.

Across all five zones, incoming ordinary encounters repeated the previous special
zero times in 485 pairs; all 22 observed post-capstone follow-ups were normal-only.
Two capstone transitions reused a previously taught special; those are explicitly
outside the ordinary preference. The selection changes are retained.

The five targeted L1 Court attempts all died. Their 49 Court encounters included
12 three-actor ordinary patrols, all using Spearman/Hammerhead. Those two escorts
accounted for 1,075 of 1,525 reported HP damage; suffocation accounted for five.
A second seed/character (novice Bardley) entered the Moray patrol with 85 HP and
died there. Stronger defensive Rattlebones and Bardley builds completed Court.
This supports varying the pressure composition, not globally weakening oxygen
or enemy stats.

The first-flood regression failed with only one legal escort pair when Moray/Conch
were the known flood-safe mechanics. Only `Patrol moray_siphoner` now exchanges
its Hammerhead for the existing Needlefin. It keeps Moray, Spearman, three actors,
tier eligibility, weight and threat budget. Conch's heavy patrol and capstones
are unchanged. `escort-decision.json` predeclares the retention criteria; all nine
Court-playing cases, including successes and failures, will be rerun naturally.
The other four never visited Court and remain separate unchanged-route evidence.

Recorded damage is cumulative reported HP damage (including possible overkill),
not net HP loss; some environmental hits still use the generic label `Damage`.
No policy models full ordinary-attack countdown planning or invalid-swap discovery.
These are explicit limits on claims of unfairness and human enjoyment.

## November 27 integration

The user stopped the original escort pass after its compiler validator had completed. Its focused tests yielded no completion XML, and the nine planned natural runs had not started. The source and the completed 13-case balance pass both entered the later expansion integration. For the separate final-source tests, builds and Game runs, see the [integration report](CRYSTAL_RESONANCE_MAIN_INTEGRATION.md).
