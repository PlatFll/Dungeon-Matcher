# Balance v1 validation — 14 September 2026

This is targeted implementation evidence for the approved balance/progression brief. It is not a repository-wide audit or evidence of real-player retention. Implementation values and rationale are in [../BALANCE_V1.md](../BALANCE_V1.md).

## Environment and reproducibility

Windows desktop, Unity **6000.3.19f1**, actual `Game` / `MainMenu` scenes. Tests use disposable account JSON and scoped character/mastery settings. They do not clear the user's preferences. The two-process audio check snapshots and restores the original music/SFX keys. Source assets retain Bardley's intentional **energyCost: 1**; only pacing fixtures clone the ability and set that clone to **80**. Runtime test fixtures never save changes to shared player/enemy definitions.

- Compile/import: `powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1`, with Unity closed.
- Focused automated checks: `powershell -ExecutionPolicy Bypass -File Tools/Test-Balance.ps1`, with Unity closed. The script reads the project's editor version, rejects failed/skipped results and writes `.utmp/balance-editmode.xml`.
- Graphics integration: Unity menu **Dungeon Matcher → Validation → Balance v1 Integration**, or launch the editor with `-executeMethod BalancePlayValidation.Run`. Stop Play Mode after inspection to restore the test scopes.
- Engine pacing: **Dungeon Matcher → Validation → Balance v1 Pacing**, or `-executeMethod BalancePacingValidation.Run`. Allow all eight runs to finish, then stop Play Mode.
- Audio restart: separate batch processes executing `AudioPersistenceValidation.Write` then `AudioPersistenceValidation.ReadAndRestore`; `.utmp/audio-persistence.json` preserves the original snapshot until restoration succeeds.
- LibreSprite: `Tools/Build-ConsumableArt.ps1 -LibreSpritePath <installed executable>`. Editable `.ase` documents and redraw scripts are under `ArtSource/Consumables`.

## Compilation and automated assertions

The final `Tools/Validate-Unity.ps1` invocation completed successfully with **Unity 6000.3.19f1**, exit **0**, after all eight final gameplay observations and the final UI pass. [Recorded command/result](BalanceV1/final-unity-validation.txt). Original local log: `DungeonMatcher-UnityValidation-fcd195d7-0ee9-4c85-bad3-1ca56fd622cf.log` in the user's temporary directory. Earlier compilation stages also passed; this final invocation covers the delivered runtime/editor code and imported assets.

The final focused automated invocation completed with Unity exit **0**, **231 passed, 0 failed, 0 skipped**, in **72.8 seconds wall time**. The [NUnit result](BalanceV1/focused-tests.xml) retains all test-case results; verbose console output was removed from this committed copy. Unity's XML aggregate duration spans only the final domain after Play Mode reloads, so its approximately 0.6-second duration is not the test's wall-clock duration.

One of those 231 tests is `BalanceLifecyclePlayTests`: it enters actual Unity Play Mode and invokes **108 existing lifecycle cases**, including every setup, assertion and teardown in the selected fixtures. [Its separate report](BalanceV1/balance-lifecycle-play.txt) records 108 passed / 0 failed. These require Unity `OnEnable`/`OnDisable` callbacks and cannot be validated merely by constructing components in Edit Mode. They cover Cracked center/chaining reward identity, run modifiers, intermission ownership and accepted/rejected/cancelled abilities. Do not add 108 to 231 and call that the NUnit test count. The editor-only ability probe was moved into the runtime compilation assembly behind `UNITY_EDITOR`, retaining its `.meta` identity, so those callbacks execute. No gameplay assertions were disabled to obtain this result.

| Area | Executed checks |
| --- | --- |
| Account/levels | Fresh profile, independent character levels, levels 1/5/20, cap and affordability, immutable base growth, highest-individual shared unlocks, explicit valid legacy selections, unsupported/corrupt saves and readable-backup recovery, atomic failure of purchase/level/charge writes |
| Mastery/cards | Actual fresh four-match clears and rewards all four; five-match creates Crystal; level-2 four-match creates directional Bomb; locked menu choices; all 27 card eligibility/stack limits, unavailable-special exclusion, cost-1 energy-card exclusion, early reward cadence |
| Damage/shield | Level/card composition, Glass Cannon HP tradeoff, Aegis grant plus cap, shield overflow/mitigation/break contexts, immutable clear outcomes, crystal attribution and special-chain regressions |
| Encounters | Authored recipe membership/windows/budgets, constrained random formations, 100-seed milestone variation, escorts, unique named leaders, King/Archbishop exception, no player-power correction |
| Board/enemies | Real concurrent Miner and barricade queues, per-owner/global caps, legal responses, persistent walls versus restored mines, shared Crossbow/Captain falling chains and serialized 2/3 caps, manual chain-swap rejection, Bomb break and death cleanup; Royal warnings, triage/blessings, complete commanded attacks and one-time reinforcements |
| Lifecycle/rewards | Death, victory, Retry, menu exit and interrupted recovery; completed waves only; first clear once; no summon income; accepted consumables, cancellations and failed writes; old callback/roster/pointer identities and external input ownership |

Dedicated Play Mode reports: [fresh shapes](BalanceV1/balance-fresh-shapes-play.txt), [concurrent disruption](BalanceV1/balance-disruption-play.txt), [Royal mechanics](BalanceV1/balance-royal-play.txt). Royal isolation cases deliberately boost fixture HP/stop unrelated attacks to inspect mechanics; those are not used as pacing measurements.

## Real scene integration and persistence

[The final integration report](BalanceV1/ui-integration.txt) ends **INTEGRATION PASSED**. It opens actual menu and gameplay scenes, operates their enabled controls, and runs normal swaps/board coroutines. Executed checks include level-up price/wallet updates and shared unlock feedback; buying and equipping both item types; fresh run charge snapshots; full-health potion rejection; Bomb cancellation without spend; independent cooldowns; paused cooldowns and preservation of another input-block owner; ten owned Bombs → three uses → seven owned with no fourth use; Retry restoring three new charges; completed-wave payout once; scene cleanup; account reload; real swaps reaching wave 3; and death payout followed by menu exit without duplication. A forced fatal hit in the death-UI fixture is labeled a functional check, never an ordinary survival result.

Music and SFX were toggled independently, checked across scenes, then checked across **two complete Unity processes**. Both processes exited 0; [the restart report](BalanceV1/audio-persistence-report.txt) confirms persistence and restoration of the original keys. Audio routing was exercised through the real music source and gem-break source pool; this does not claim listening-based sound-quality testing.

A separate manual pointer pass used the visible Unity Game view after the scripted integration driver finished. Play opened the selected level-2 RattleBones run; a visible green match and resulting purple cascade defeated the first Basket Villager; a Potion healed the injured player and reduced its slot from 3 to 2; Settings paused with that cooldown unchanged; Resume restored play; the right Bomb button entered targeting and a clicked board gem cleared its area, reduced Bombs from 3 to 2 and reached the wave-2 card choice. The card was selected through its visible control. [Unedited desktop capture](BalanceV1/12-manual-pointer-play.png). This is a control/playability check, not a human-beginner sample or benchmark.

## Visual and art evidence

LibreSprite actually generated editable Potion, Bomb and Slot sources, then exported **24×24** icons and a **32×32** frame. Imports use Point filtering, no mipmaps/compression and Full Rect sprites at the existing PPU. Runtime icons occupy 48×48 and frames 64×64 logical pixels. Counts, prices, cooldowns, stats and energy are Unity text; no text was painted into the sprites. Existing dungeon frame/gem artwork is preserved. Liberation Sans TTF and TMP SDF are assigned centrally by `Resources/UI/Typography.asset`.

The final rendered captures were inspected for text clipping, overlaps, sprite proportions, slot placement and safe-area containment. Short portrait is **540×960**. Tall portrait is **720×1600** with the injected safe rectangle `(0,54,720,1470)`; control-corner containment is also asserted by the integration driver. These are real Unity renders with simulated cutouts, not physical Android device tests. The editor preview was fit to its panel before manual interaction; a cropped editor zoom is not a device layout failure.

| Screen | Evidence |
| --- | --- |
| Menu with selected character/wallet | [Main menu](BalanceV1/01-main-menu.png) |
| Independent levels, next stats, cost and unlock feedback | [Characters](BalanceV1/03-characters-upgraded.png) |
| Locked mastery requirements | [Mastery](BalanceV1/04-mastery-locks.png) |
| Only Potion/Bomb purchase and equipment | [Shop](BalanceV1/05-shop.png) |
| Short portrait HUD | [540×960](BalanceV1/07-hud-short.png) |
| Tall portrait/cutout HUD | [720×1600](BalanceV1/08-hud-tall-cutout.png) |
| Progressive slot dimming and live quantities | [Cooldowns](BalanceV1/09-consumable-cooldowns.png) |
| Paused settings and independent mute controls | [Settings](BalanceV1/10-settings.png) |
| Reward breakdown and death exits | [Death](BalanceV1/11-death-rewards.png) |

## Pacing method and tuning history

`BalancePacingValidation` plays production Game scenes at **6× game speed**, using accepted normal swaps, normal ability controllers, normal damage and the real enemy runtimes. Reported durations are **game seconds**, including encounter transitions and **three seconds per card menu**, not editor wall time. A casual policy takes an available hint every four game seconds; a greedy policy checks visible valid swaps every 1.8 seconds, favoring longer matches, specials and enemy weakness/support priority. Neither policy plans multi-move setups like an expert human. Both cast a funded ability when legal. Card choice positions vary; builds are not optimized by a hidden solver.

Eight observations cover both characters: level-1 casual bare, level-1 greedy bare, level-5 greedy bare and level-5 greedy equipped. Character-specific seeds are 3101/3102. Level-1 uses the fresh Crystal loadout; level-5 uses unlocked Poison/Healing/Shield shape selections. Equipped runs begin with ten of each item, use Potions below 55% HP, and use at most three Bombs from wave 6 against multi-enemy encounters. Every run uses a disposable profile, so gold totals include applicable personal-best/first-clear bonuses. Ordinary repeat income must subtract those one-time bonuses. A 20-minute observation limit would be labeled censored, never a forced defeat.

An initial exploratory pair with affinity healing at three HP/gem produced an 849-second RattleBones casual attempt to wave 17 and a 725-second Bardley casual attempt to wave 29. Healing was reduced to one HP/gem and early damage/HP were retuned around a few matching opportunities. An [intermediate eight-run comparison](BalanceV1/intermediate-runs.csv) then showed useful RattleBones level/skill progression but Bardley King fights around 17–23 seconds. Bardley's cracked-center base damage was reduced from 50 to 20 while preserving all chain behavior and the committed energy override. Captain's intended serialized chain cap was also corrected to 3 and explicitly asserted by the real queue tests. The following final measurements use those corrected values; do not present intermediate CSVs as final balance.

## Final engine measurements

[Raw runs](BalanceV1/final-runs.csv), [per-wave formations and damage](BalanceV1/final-waves.csv), [completion report](BalanceV1/final-report.txt). All eight observations finished naturally; none was censored.

| Character / level | Input / items | Result / completed waves | Minutes | HP lost | Gold | Gold/min | Moves / casts / special swaps | Potion / Bomb uses |
| --- | --- | --- | ---: | ---: | ---: | ---: | --- | --- |
| RattleBones L1 | casual, bare | Defeat, 10 | 5.48 | 168 | 94 | 17.17 | 75 / 3 / 3 | 0 / 0 |
| Bardley (80-energy clone) L1 | casual, bare | Defeat, 28 | 13.68 | 461 | 361 | 26.38 | 158 / 35 / 2 | 0 / 0 |
| RattleBones L1 | greedy, bare | Defeat, 28 | 7.91 | 272 | 361 | 45.65 | 209 / 11 / 14 | 0 / 0 |
| Bardley (80-energy clone) L1 | greedy, bare | Victory, 29 | 6.26 | 74 | 517 | 82.55 | 80 / 44 / 13 | 0 / 0 |
| RattleBones L5 | greedy, bare | Victory, 30 | 8.32 | 209 | 533 | 64.04 | 225 / 16 / 12 | 0 / 0 |
| Bardley (80-energy clone) L5 | greedy, bare | Victory, 29 | 6.25 | 32 | 517 | 82.75 | 119 / 24 / 14 | 0 / 0 |
| RattleBones L5 | greedy, equipped | Victory, 30 | 7.96 | 219 | 533 | 66.92 | 203 / 14 / 22 | 0 / 3 |
| Bardley (80-energy clone) L5 | greedy, equipped | Victory, 29 | 5.99 | 43 | 517 | 86.36 | 115 / 22 / 8 | 0 / 3 |

HP lost is cumulative actual HP damage, so healing can make it exceed maximum HP. Special swaps count accepted moves involving an existing special, not every special created or detonated. Runs use the real board and random timing; a seed does not make accelerated visual-frame scheduling bit-identical across separate executions.

| Completed encounter category | Samples | Median seconds | Observed min–max |
| --- | ---: | ---: | --- |
| Ordinary formation | 179 | 8.84 | 1.10–47.98 |
| Miniboss formation | 29 | 18.88 | 4.10–63.33 |
| King formation | 5 | 29.53 | 17.90–47.50 |

These are completed-encounter durations between captured wave events; failed current fights are excluded from that table. The run table includes time spent in the losing fight and transitions. Fast carry-over cascades/abilities can legitimately complete an entering formation with zero new swaps.

| King clear | Fight seconds | Final HP |
| --- | ---: | ---: |
| bardley-L1-greedy-special-bare | 17.90 | 80 |
| skeleton-L5-greedy-special-bare | 47.50 | 60 |
| bardley-L5-greedy-special-bare | 35.86 | 122 |
| skeleton-L5-greedy-special-equipped | 28.02 | 94 |
| bardley-L5-greedy-special-equipped | 29.53 | 132 |

### Interpretation and affordability

The casual RattleBones observation ended after ten completed waves at 5.48 minutes, beyond the 2–4 minute beginner target, and earned 94 gold: enough for levels 2 and 3 (25 + 45), leaving 24. The greedy level-1 run reached 28 completed waves at 7.91 minutes. Level-5 bare RattleBones cleared the King, showing useful permanent growth without making items mandatory. Bardley remains substantially stronger under a policy that always recognizes legal matches and immediately casts; its casual observation lasted 13.68 minutes to wave 28. This does **not** establish the intended early beginner wall for Bardley. Human timing/learning and broader build/seed samples remain the next balance test, particularly Bardley energy refunds and encounter-time variance.

A repeated ten-wave result without a new best pays 74, or about 13.5 gold/min at the observed casual time. The level-5 bare 30-wave King result pays 393 on repeat, about 47.2 gold/min at its observed time, versus 533 for a first clear. Deeper progress therefore pays better in both total and rate in these samples. The first level costs 25; cumulative level 5 costs 260. A full three-Potion/three-Bomb kit costs 126. Equipped comparisons measure assistance, not a guarantee of a better outcome: boards, offers and resulting choices diverge, and unused items remain owned.

The proposed fight/session ranges remain targets rather than enforced limits. Several strong clears beat the successful-arc target, and some miniboss/King fights fall outside their target ranges. No death wave, minimum fight length, hidden player-power scaling or forced use of items was introduced to make the table fit.


## Limits and release follow-up

- Synthetic policies and eight final observations are useful first-pass evidence, not distributions of human beginner skill, expert setup, learning, retention or long-term economy.
- Timing includes a fixed three-second card decision. Human reading, touch accuracy and device performance can lengthen runs. Physical Android safe-area, touch and audio-device testing remains outstanding.
- Bardley's committed 1-energy override is intentional development configuration. Release cleanup must deliberately select the normal energy cost, re-enable useful energy-card offers through eligibility, and rerun the same production-cost comparisons. Do not rebalance enemy HP around that override.
- Supported content ends after the King formation. Guild/post-King content and monetization were not added.

This change retains the authoritative board/damage pipelines, existing cracked/crystal ownership fixes, separate energy generation/storage, safe enemy mutation queues, shield overflow semantics and scene cleanup. Compilation, automated assertions, functional Play Mode, manual pointer interaction and synthetic balance measurements are reported separately above.
