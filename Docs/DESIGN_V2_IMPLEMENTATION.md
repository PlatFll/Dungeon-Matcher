# Revised design implementation — 2026-09-15

The user approved implementation of the revised design, explicitly retired Bardley's 1-energy testing override, requested restoration of SmallHold startup with 1.5 extra seconds on white, and authorized merging after validation. Earlier updates were merged first: PR #157 (Balance v1) and #152 (historical audit documentation). The implementation branch began from updated main `6ec402a`. The startup stage is commit `33f7af8`; the remaining runtime/data stage is `5bef5fe`. The implementation and evidence are in [PR #158](https://github.com/PlatFll/Dungeon-Matcher/pull/158).

The user’s current instructions supersede the older attachment’s preserve-cost-1 instruction and the research handoff’s design-only wording. Balance v1’s stat growth, prices, unlocks, shields, damage attribution and earlier gameplay fixes remain the base. The changes below implement the approved revised proposal through the existing gameplay owners.

## Implemented decisions

| Area | Delivered behavior / control values |
|---|---|
| Bardley | 80 energy; five cracked centers, 20 base matching-color damage each. Complete cast refunds, including chained explosions and cards, share a ceiling of 50% of effective accepted cost. Base ceiling 40; two Efficient Casting stacks give cost 64 / ceiling 32. Cancellation does not release an accepted board action’s refund budget. |
| Opening charge | Both characters start at 20% of base cost: Bardley 16, RattleBones 20. Grant happens after player initialization, once per fresh scene/run, and never on Continue. |
| Draft direction | First choice after wave 2 offers Board, Ability and Survival when eligible. Later drafts remain weighted. Existing eight-choice cadence and rarity/stack/character/mastery eligibility remain. |
| Refine | One free use per run. Redraw eligible unoffered cards in the chosen theme; a small remaining pool can yield fewer than three. No eligible cards means no use spent. Result and allowance persist. |
| Combat continuation | Suspend preserves the attempt. Continue restores original character/level/mastery, board and restrictions, enemy roster/ownership/HP/shield/status/timers, build, Royal Decree, supplies/cooldowns, draft and random positions. No offline combat time. |
| Accepted actions | Quiescent snapshots plus a short frame journal recover in-flight swaps, abilities, items and card decisions through the same gameplay pipeline. Item debit and its accepted action are one atomic write. Replay uses an isolated account and publishes only after success; failure preserves the original. |
| End Run | Explicit abandonment, Retry, death and victory settle completed-wave rewards once. Partial waves pay nothing. Current-version interruption is suspension; legacy journals without combat state retain one-time settlement on load. |
| Inspection | Tap portraits to pause and inspect HP/shield, weakness, normal damage/timer, special move cadence, owned restrictions and active warning deadlines. Guide explains combat clocks, counters, ability/refund rules and the current build. |
| Onboarding | Brief first-exposure tips point to enemy inspection or Guide. Learned-tip flags persist; practice does not write them to the real account. No additional mandatory tutorial modal. |
| Bomb targeting | A 3x3 footprint, explicit confirmation in the ability area, cancellation without spending, and text explaining barricade hits/special extension. Guide explains chains, ice, banners and mined-cell exceptions. |
| Practice | Copies the current account's progression into a disposable in-memory profile, gives three free charges of each supply, and leaves real stock, wallet, records and active attempt unchanged. |
| Challenges | After the King: No Supplies (abilities allowed), Board Only (no abilities or supplies, no Ability-theme cards). Separate best-wave/win records, normal rewards, no expiration or fees. |
| Failure/economy | Ending wave, actual last damage and shield absorption, scrollable build, rewards, remaining owned supplies, Retry and Change Build. Replacement value of used supplies is informational; it is never another fee. Repeatable wave/milestone income stays distinct from new-best/first-clear bonuses. |
| Presentation | Independent persistent Music and SFX settings retained. Reduced motion removes selected shake/overshoot and restrains player flashes without changing combat speed or input locks. Pause also gates ready enemy actions and poison ticks. |
| Startup | Original editable 30-frame SmallHold animation and GUID references restored; LibreSprite exported the atlas. The held final frame is white with the original dark silhouette/wordmark. Original 2.5 seconds + 1.5 extra final hold. Native Unity splash/logo disabled; music starts at MainMenu. |

These numerical controls are TEST / TUNE values, not proof of final human balance. No enemy HP inflation or mandatory early defeat was introduced. The supported story arc still ends at the King. No map, new currency, attendance system or post-King faction content was added.

## Encounter teaching and counterplay

| Mechanic | Eligible from | Teaching deadline |
|---|---:|---:|
| Miner | 4 | 6 |
| Crossbow chains | 7 | 10 |
| Guard barricades | 8 | 11 |
| Shield Knight | 13 | 16 |
| Royal Standard Bearer | 22 | 22 |
| Court Mage | 23 | 23 |

An unseen eligible lesson has a 55% opportunity before its deadline; the earliest pending deadline wins. Milestones take priority. A teaching formation has at most two members and a normal escort within the existing threat budget. The encounter following a milestone prefers a normal relief patrol unless a lesson deadline is due. These guarantees retain weighted-era generation and the 29–30 King window.

Mines, pins, barricades and banner placements must retain an immediate useful response as well as a legal move: weakness damage, needed affinity healing, special use/creation, or obstacle removal. This is a bounded current-board query, not a future-board solver. Separate simultaneous three-matches are not mistaken for a created special; banner counterplay requires clearing beneath it. Interruptible marks require immediately clearable targets when placed. Hammer/set/lane warnings share deadline reservation so later warnings receive their own minimum response window. Inspection displays the resulting deadline. Bombardment remains non-cancellable by clearing ordinary lane gems.

## Card theme catalog

- **Board (11):** Gem Grinder, Siegebreaker, Bombsmith, Boss Hunter, Cascade Catalyst, Chain Reaction, Corrosive Formula, Executioner, Glass Cannon, Opening Volley, Slow Venom.
- **Ability (11):** Efficient Casting, Horrible Encore, Mana Spark, Arcane Efficiency, Chromatic Conductor, Final Word, Longer Reign, Prepared Casting, Resonant Cracks, Sour Note, Toxic Momentum.
- **Survival (5):** Strong Remedy, Thicker Hide, Aegis Reservoir, Emergency Plating, Reinforced Flask.

## Executed validation

The required validator was run repeatedly through compiling stages. The final `Tools/Validate-Unity.ps1` completed successfully with Unity 6000.3.19f1: `DungeonMatcher-UnityValidation-416a055f-79c1-49a8-af56-b854edc9a65e.log`. The final focused suite also exited zero: **243 passed, zero failed/skipped**, 140.8 seconds wall time. [Execution record](Validation/DesignV2/execution-record.txt) and [test XML](Validation/DesignV2/focused-tests.xml) preserve the results.

| Executed check | Result and scope |
|---|---|
| Pre-merge baseline | Exact earlier head `9f9f1c3`: required validator and 231 focused tests passed before PR #157 merged. |
| Expanded focused suite | Final run: 243 tests passed, zero failed/skipped, 140.8 seconds wall time. Covers prior board/combat/cascade/shield/ability/purchase/reward regressions plus first-draft/refinement eligibility, 100-seed teaching deadlines, practice/challenge records, production refund cap and opening charge. Includes ready-special/attack and poison pause guards. |
| Scene/account restart | HP, shield, energy, board, poison, stagger and attack countdown restored; accepted partial Bomb and delayed Bardley cast produce matching settled board/energy and spend once across repeated Continue. |
| Draft/build continuation | Real waves 1–2 reach a choice; exact offer survives reload, refinement persists and cannot be reused, accepted card stack restores once, next wave proceeds. |
| Owner reconstruction | Real Miner/Crossbow, Barricade Guard/Mage and Standard Bearer/Archbishop states restore cells, restrictions, owner slots, Restoration warning and active Royal Decree without another purchase, heal or energy spend. |
| Atomic account failures | Locked temporary file rejects accepted-item transaction with unchanged durable JSON/stock. Aborted replay preserves the accepted durable original. |
| Separate Unity processes | `DesignV2PlayValidation.Write` (22.5s) saved an accepted Bardley cast during a Spear Knight two-hit sequence in a real late formation, then recorded an uninterrupted oracle. `Read` in a new process (20.6s) reproduced board/specials, HP/shield, energy and gameplay random position exactly. The reader used the captured in-flight file, not the writer's later settled file. |
| Rendered UI / real scene flow | `DesignV2PlayValidation.UI` passed (31.2s): MainMenu, Shop, practice Potion, settings pause, Guide, Bomb preview, Board Only restriction, defeat/reward and real draft. Captured 1080x1920 and 720x1280 views. Practice left the real disposable account file byte-identical. Visual review prompted moving Bomb confirmation off the board and fixing a missed opening charge; the revised graphics run passed and its final captures were inspected. |
| Startup | Graphics-enabled `StudioIdentValidation.Run` passed (22.9s): 30 frames, eight layout calculations, application pause, white final pose held at least 1.5 extra seconds, one menu handoff, deferred music, reentry and missing-art fallback. LibreSprite preparation and export executed successfully. |
| Android availability | `adb devices -l` returned no attached device. Physical touch, cutouts, audio hardware and mobile performance were not tested. |

Failures found during implementation were fixed and rerun: inspection UI accidentally occupying summon anchors; opening charge arriving before the player definition; Bomb confirmation covering board cells; poison component being lazily created during restoration; draft/random-state continuation; persistence of exactly-once accepted supply use. Some early validation fixtures also required correction (an instantaneous attack has no in-flight window; special readiness must actually commit before asserting warning placement).

## Final production and native-player checks

Eight real Game-scene playthroughs completed with production assets. Both level-5 characters defeated the King without supplies. The level-1 stronger policy reached wave 26 with RattleBones and cleared with Bardley. [Full measurements and economy calculations](DESIGN_V2_PLAYTESTS.md) distinguish repeatable gold from first-clear bonuses and deduct actual supply replacement value. Bardley remains stronger in these samples; this is a human tuning item, not proof of equal difficulty.

The Windows development build completed successfully in 113.7 seconds. In the actual 600x1000 player, a manually entered match dealt damage, healed and charged energy. Suspend, process exit, restart and Continue preserved the visible board, enemy HP (2), player HP (79), energy (26) and paused attack countdown (5.3 seconds). Music Muted, SFX On and Reduced motion On persisted independently. The validation build uses a separate company namespace so its account and preferences do not touch the user's real profile; the build helper restores project company settings afterward.

Timed native observations showed the original night/day castle animation, then the dark SmallHold silhouette and name on white, held across multiple observations roughly 0.7 seconds apart before the menu. No Unity splash appeared. The graphics validator separately measures the required extra hold, all frames, pause and handoff. Native logs contained no managed exceptions/gameplay errors; a D3D12 diagnostic-interface message and shutdown ComputeBuffer disposal warning did not prevent the checks.

## Reviewable UI evidence

- [Bomb footprint and clear board](Validation/DesignV2/bomb-preview.png)
- [Portrait settings](Validation/DesignV2/settings.png) and [720-wide settings](Validation/DesignV2/settings-720.png)
- [First themed draft and refinement](Validation/DesignV2/draft-720.png)
- [Loss and economy recap](Validation/DesignV2/failure.png)
- [Scrollable combat guide](Validation/DesignV2/guide.png)
- [Restored final SmallHold pose](Validation/DesignV2/smallhold-white-hold.png)

## Remaining limits

Automated policies do not establish human comprehension, retention or touch comfort. No Android device was attached, so physical touch, cutouts, hardware audio and mobile performance remain untested. The exact recovery tests cover representative real actions and obstacle owners, not every possible seed/timing combination. The production runner completed all eight runs but needed manual closure because its former finish handler did not exit batch mode; the handler is now fixed, and this limitation is documented with the measurements. Existing unrelated consumable metadata edits remain outside the commits.
