# Enemy pressure and special swaps — 2026-10-13

## Request and diagnosis

The user found that enemies died before basic attacks or abilities and were
staggered too often. They requested faster pressure, harder stagger buildup,
longer stagger immunity, no damage shake, and activation when swapping any two
special gems.

The merged baseline was `d1099c2c` (menu PR #193 plus board-feel PR #194).
Its 73 definitions had 2–6 move basics, usually 4–5, with equally slow opening
attacks. Stagger took 30/35/30/25% HP by rank, disabled two future moves and
protected only two moves afterward. Non-crystal special pairs still went
through ordinary match validation. Damage also started a half-second random
sprite translation routine.

## Research and choice

- [Puzzle Quest 3's developer explanation of Action Points](https://puzzlequest3.com/new-game-mode-action-points/)
  describes enemies taking turns and attacking even without skull matches,
  with spells visibly charging. The useful principle here is reliable, readable
  enemy opportunities. Its AP system and numerical balance differ from ours.
- [Royal Match's official power-up combinations](https://dreamgames.helpshift.com/hc/en/3-royal-match/faq/7-power-up-combinations/)
  and [activation rules](https://dreamgames.helpshift.com/hc/en/3-royal-match/faq/6-creating-and-using-the-power-ups/)
  support swaps as a direct way to use power-ups. Dungeon Matcher preserves
  its own effects, following the user's request for activation at swapped cells.

These sources support design principles, not transferable damage or stagger
percentages. The following values are a project-specific first tuning pass.

| Control | Before | New runs |
|---|---|---|
| First basic | 2–6 moves | 2–3 moves |
| Repeat basic | 2–6 moves | 2–4 moves |
| Typical special charge | 2–6 moves | 2–4 moves |
| Stagger threshold, Normal/Special/Miniboss/Boss | 30/35/30/25% max HP | 60/70/80/90% |
| Stagger duration | 2 full future moves | 1 full future move |
| Immunity after stagger | 2 full future moves | 5 full future moves |

HP, hit damage, response windows, shields, economy, difficulty curves, formation
budgets and board-feel timings are unchanged. Early weak enemies can still die
to a good match; high output is not punished with adaptive HP or forced survival.
The immunity window allows at least one normal opportunity even for a four-move
attacker, absent another legitimate hold or death. It blocks both buildup and
forced stagger. Poison ticks still do not build stagger; Rattled still halves it.

## Ownership and save compatibility

New runs record `unified-accepted-moves-v3`. Separate serialized definition fields
retain the v2 attack data; old saves keep v2 thresholds, duration, immunity and
special-swap behavior. No mid-fight counter is silently shortened. New runs use
the revised values in all five zones. Existing crystal effects remain intact.

Non-crystal pairs seed the existing deterministic bomb expansion with both
physically swapped gems. One clear set handles overlaps, chained bombs, utility
effects, board blockers and rewards. There is no synthetic three-gem match or
extra accepted turn. Hints, safe placement and reshuffle validation recognize
the same special pair. Pinned endpoints remain illegal; extended movement uses
the selected gem's final destination. Damage retains white flash and numbers;
authored animation and basic-attack lunges keep their existing owners.

## Validation

`Tools/Validate-Unity.ps1` completed successfully with Unity **6000.3.19f1**,
exit 0. BatchValidationAudio muted Editor audio. There are **120 distinct passing
focused checks**, taking the latest result for each case across three recorded
runs. [Evidence and source hashes](EnemyPressure/evidence.json) and the
[case list](EnemyPressure/test-results.csv) record the exact coverage.

- All 36 playable special-pair combinations are legal with equal or different
  colors; pins block them. All 25 non-crystal bomb pairings were actually
  swapped in the production scene, checking swapped origins, one accepted move,
  exact initial footprint and no duplicate clear rewards.
- Extended movement activates the final contact pair at the rotated positions.
  Existing Slippery preview, cancellation, fallback, save and movement tests pass.
- Rank thresholds, one full future stagger move, five immunity moves, blocked
  forced stagger/buildup, usable haste and old-profile Save/Continue pass.
- King command ownership, warning/impact completion, channel priority, summons,
  status expiry, board-feel regressions and portrait HUD checks pass.
- Damage flash remains visible while the enemy visual root stays stationary.
  A legacy-only countdown overlap found by the portrait test was fixed by
  positioning the countdown below the separate ability counter; verified at
  720×1280, 1080×1920 and 1080×2400.
- The data audit verified all 73 definitions retain their prior HP, damage and
  legacy intervals. Only the new cadence fields and neutral Arbalist description
  changed. Five pre-existing local files remain byte-identical and are excluded
  from the PR. No scene, audio, menu asset or board-feel timing change is included.

Earlier failures were retained in the evidence: shortened intervals exposed
ineffective ceiling-rounded haste; old tests assumed the prior King charge or
allowed Conch to recast while testing expiry; the bomb fixture initially allowed
dungeon crumble holes; legacy countdown text overlapped a two-digit counter.
The runtime issues were fixed and fixture assumptions corrected without removing
their assertions. All affected cases were rerun. The final broad gameplay run
remains applicable because subsequent production edits only moved legacy text.

### Limited pacing observation

Each profile ran one seeded, disposable level-1 Skeleton session per zone through
six completed waves, using actual production enemies and combat. The bot selects
greedy swaps, uses its free ability, drinks a potion below 45% HP and takes the
first offered upgrade. It runs at 6× simulation speed with a 2.5 game-second
input interval. HP and damage are unchanged. The baseline selects retained v2
rules; it is not a checkout of the old executable. The same zone seed is used,
but frame scheduling and changed resolution can produce different boards.

| Zone | Basic sequences, v2 → v3 | Specials, v2 → v3 | HP damage received, v2 → v3 |
|---|---:|---:|---:|
| Dungeon | 0 → 1 | 0 → 0 | 0 → 5 |
| Magical Forest | 3 → 15 | 1 → 3 | 35 → 175 |
| Drowned Court | 4 → 16 | 1 → 2 | 80 → 160 |
| Ironvein | 2 → 6 | 0 → 1 | 15 → 60 |
| Frostwake | 5 → 15 | 0 → 1 | 40 → 130 |

Raw per-wave CSVs and summaries are in [EnemyPressure](EnemyPressure).
The stagger-event field in those summaries includes lethal-hit notifications;
it must not be read as the number of enemy turns prevented. Duration/immunity
tests separately measure the actual lost and protected opportunities.

These short samples show increased enemy opportunities, especially beyond the
introductory dungeon waves. They do not establish whole-run balance, a human win
rate or retention. All ten sessions reached the six-wave observation limit.
Human playtesting should start a **new run**, since continued saves deliberately
keep their prior rules. Physical-device validation remains separate.
