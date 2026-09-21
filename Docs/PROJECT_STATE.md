# Revised implementation status — 2026-09-15

The user approved implementation of the revised design, retired Bardley's testing override and authorized merging after validation. Yesterday's Balance v1 PR #157 and documentation PR #152 were merged first. See [DESIGN_V2_IMPLEMENTATION.md](DESIGN_V2_IMPLEMENTATION.md) for the exact scope, balance decisions and executed evidence.

Delivered systems include resumable combat with typed snapshots and accepted-input recovery, first-draft Board/Ability/Survival directions, one free persistent refinement, gentle mechanic introductions and post-milestone relief, useful-response checks for interference, paused inspection and learning tips, confirmed Bomb footprint, free practice, two optional post-King challenges, clearer loss/economy recap, independent audio settings and reduced motion. SmallHold's editable 30-frame ident is restored; its final white pose holds 1.5 extra seconds, with the Unity splash disabled.

The newer rules in this status and the owning design/architecture documents supersede the historical Balance v1 observations below. Target fight lengths remain review bands, never minimums; a deliberate early loss is not a design objective. Physical-device and human-comprehension evidence must be distinguished from automated Unity evidence.

# Dungeon Matcher — Canonical Project State

> **Read this first after `AGENTS.md`.**
>
> This is the living, high-level definition of Dungeon Matcher and the current development checkpoint. It exists so a new ChatGPT/Astra/Codex session can understand what the game is, what is already implemented, what is intentionally temporary, and what remains to do **without reconstructing the project from old chats**.
>
> **Update this file whenever a meaningful feature, balance direction, content milestone, art direction, architecture rule, or known-risk status changes.** Replace stale statements rather than accumulating a giant changelog.

---

## 0. Document contract

### Last updated

- **Date:** 2026-09-22
- **Current milestone:** Seventeen remaining-cast recolors and matching idle sources, with the prior dungeon presentation/audio pass preserved
- **Current implementation branch:** `codex/remaining-cast-recolors-idles`
- **Latest gameplay PR:** [#158 — Balance Bardley, add resumable combat and restore SmallHold startup](https://github.com/PlatFll/Dungeon-Matcher/pull/158)
- **Current art-pass base:** `codex/dungeon-presentation-polish` at `454cd5a` (PR #162).
- **Earlier updates:** PR #157 (Balance v1) and #152 (historical audit documents) merged before this implementation.
- **Release status:** The original idles and first combat-action pass are on unmerged [PR #159](https://github.com/PlatFll/Dungeon-Matcher/pull/159) and [PR #160](https://github.com/PlatFll/Dungeon-Matcher/pull/160). The local-enemy pass on [PR #161](https://github.com/PlatFll/Dungeon-Matcher/pull/161), presentation pass on [PR #162](https://github.com/PlatFll/Dungeon-Matcher/pull/162), and current art-source pass extend that stack; merge requires a new explicit instruction. Sources live under `ArtSource/`; the durable guide is `Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt` (v1.5). Current source-art verification is in `Docs/Validation/REMAINING_CAST_ART.md`; prior Unity presentation evidence remains in `Docs/Validation/DUNGEON_PRESENTATION_POLISH.md`.

### What this document is

This file is:

- the quickest authoritative **project orientation and current-state map**;
- a summary of the game fantasy, gameplay loop, pacing, balance model, visual identity, implemented systems, current risks, and next work;
- a pointer to the detailed design/architecture/balance documents that own exact rules and tables;
- the place to record intentional temporary development overrides so future work does not accidentally “fix” them.

### What this document is not

This file is **not**:

- a replacement for `GAME_DESIGN.md`, `PROGRESSION_PACING.md`, `ARCHITECTURE.md`, or detailed balance tables;
- a complete changelog;
- proof that an implementation is correct merely because it is listed here;
- permission to silently resolve a conflict between code, serialized Unity data, and design documentation.

If this summary disagrees with a detailed authoritative document or the current implementation, inspect the discrepancy and report it before changing behavior.

### Update rule for future assistants

After any meaningful project change:

1. Update **Last updated / current milestone / active PR or branch**.
2. Move finished work into **Implemented now**.
3. Remove or rewrite stale descriptions rather than preserving obsolete wording.
4. Update **Intentional temporary overrides**.
5. Update **Known risks / unresolved tuning**.
6. Update **Next priorities**.
7. If the detailed mechanic itself changed, also update its owning design/architecture/balance document.

Do not make this file a diary. It should always describe **the game as it exists and is currently intended**.

---

# 1. One-paragraph game definition

**Dungeon Matcher** is a portrait mobile pixel-art **match-3 dungeon battler / run-based progression game** built in Unity. The player is not the adventurer: the player is one of the dungeon’s monsters/denizens defending a powerful magical artifact while increasingly serious outsiders invade the dungeon. The player matches gems to damage enemies by weakness, heal through affinity, charge character abilities, create special gems, and manipulate the board. Enemies attack in real time and increasingly interfere with the board. Runs are shaped primarily by temporary card choices, while permanent character levels provide a stronger starting floor. Gold earned from runs funds character progression and optional consumables. The opening narrative escalates from locals to guards, knights, royal forces, and finally the King; later content is intended to expand into the Adventurer Guild and the wider fantasy world.

---

# 2. Core player fantasy and tone

## Fantasy

The core reversal is:

> **You are what adventurers normally fight.**

The dungeon contains something powerful enough that the outside world gradually takes the threat more seriously. The enemy roster should therefore feel like an escalating response to the player’s continued survival rather than an arbitrary list of stronger stat blocks.

The opening escalation is locked at a high level:

**LOCALS / VILLAGERS → GUARDS → KNIGHTS → ROYAL FORCES → THE KING → ADVENTURER GUILD → WIDER FANTASY WORLD**

The King is the climax of the current supported opening arc. Guild/post-King content is future work.

## Tone

The game should be:

- charming, readable, slightly spooky and magical rather than grim;
- whimsical enough for memorable enemies and characters;
- visually cohesive with a retro/indie pixel-art feel;
- mechanically clear even when enemy combinations become chaotic;
- satisfying and snappy rather than slow or over-animated.

Humor and personality are welcome, but they should not make threats unreadable.

---

# 3. Visual identity and UI direction

## Pixel-art identity

Current visual direction:

- portrait mobile presentation;
- crisp pixel-art characters, gems, frames, obstacles, icons and dungeon scenery;
- dark purple / violet / indigo dungeon-stone framing and masonry;
- very dark backgrounds with warm orange fire/torch accents;
- jewel-tone gems with strong color separation and readable silhouettes;
- cute/stylized monster proportions rather than realistic anatomy;
- short, snappy animation cycles instead of highly fluid animation for its own sake;
- gameplay readability takes priority over decorative noise.

The current player cast includes the crowned skeleton **Sir RattleBones / RattleBones** and the green slime bard **Bardley**. Their silhouettes and personalities should remain distinct.

## Combat-animation art checkpoint

The prior four-idle integration is on unmerged [PR #159](https://github.com/PlatFll/Dungeon-Matcher/pull/159). The current action pass is on unmerged [PR #160](https://github.com/PlatFll/Dungeon-Matcher/pull/160), stacked on that branch. Idle sources remain nine 64×64 frames at 130 ms (1.17 seconds). Farmer now uses the user's own subsequent LibreSprite adjustment, preserved exactly. Pan Villager's approved head/body rhythm and scarf-tail polish and Rattlebones' idle are unchanged.

`ArtSource/CombatActions/` adds Farmer's rigid pitchfork thrust, the supplied Pan Villager overhead pan strike with its clipped disk completed, and polished/recolored Bardley and Rattlebones ability casts. Auto-attacks use eight frames / 680 ms with the visible hit at 320 ms (frame five); player casts use ten frames / 880 ms. The persistent source guide is [ArtDirection/Dungeon_Matcher_Art_Direction.txt](ArtDirection/Dungeon_Matcher_Art_Direction.txt), now v1.5. The same approved palettes and pixel material treatment apply across every frame.

All four idles and both player casts now stand at source row 63 in full 64×64 rectangles. Bardley's idle moves down exactly 12 pixels, without changing any drawing or losing pixels; this supersedes the previous idle-only 64×52 Unity crop. Farmer/Pan attack canvases expand symmetrically to 96×64 while retaining the same 64-pixel body scale and fixed floor/center.

The existing definition-selected controllers own idle/action playback. Accepted player activations emit one cast cue; persistent effects and rejected casts do not repeat it. Farmer/Pan use the existing enemy impact/damage flow with the clip's hit event, protected against duplicates, pause and cancellation. Their authored sprite movement replaces the generic UI lunge. Ability gameplay effects, board resolution, HP/shield rules and balance numbers are unchanged. See [the action source notes](../ArtSource/CombatActions/README.md) and [validation record](Validation/COMBAT_ACTION_VALIDATION.md). Earlier idle-pass evidence remains historical and does not describe the later user-edited Farmer or the new Bardley baseline.

The 2026-09-21 local-enemy pass extends the animation checkpoint above: Miner, Basket Villager (berries farmer), and Barricade Villager now have matching material palettes and nine-frame idles. All three have eight-frame attacks with damage on frame five; Miner and Barricade Villager have ten-frame abilities with board contact at 360 ms. The Miner swings sideways to attack and leaps into a ground strike to mine. Basket Villager gathers/releases a berry with no travelling projectile. Barricade Villager uses an axe attack and a compact kneeling plank-set ability. Sources and exact exports are in `ArtSource/LocalEnemies/`; the current guide is v1.5. Fixed 96×80 Miner canvases and 64×64/96×64 villager canvases preserve source scale and bottom-center alignment. The pixel UI presenter compensates taller canvases without moving the shared battle-floor anchor; every pose is checked for health-bar clearance after portrait resizing. Earlier four-character art remains unchanged. Ability contact/recovery extends the existing actor action ownership and authoritative board queue; HP/shield, special cadence, caps and balance are unchanged. See [local-enemy verification](Validation/LOCAL_ENEMY_ANIMATIONS.md).

The follow-up refinement on PR #161 uses the newly supplied LibreSprite idle drafts for connected head pitch, eye closure, shoulder changes and torso compression in all three local-enemy idles. It also improves the builder axe windup/follow-through and corrects the kneeling leg locally. The current presentation pass repairs disconnected candle/boot pixels, preserves the basket as a rigid carried prop, articulates the berry throw and rounds the builder's folded rear boot. Ready poses, palettes, dimensions and impact times remain unchanged. Earlier four-character art and the Miner's action drawings are preserved.

## Current presentation and feedback checkpoint

The working guide is v1.5. `ArtSource/TileVfx/` preserves the four supplied LibreSprite effects and exports thirteen-frame, 390 ms tile bursts. Actual cleared tiles receive centered generic, poison, healing or shield effects at shatter; the existing board pipeline still owns gameplay and refill timing. `ArtSource/Presentation/` contains native dungeon props, quiet masonry gutters/panel fills, polished consumables, a menu doorway and the pink-gem title logo. The existing battle tilemap prefab and layout owners remain authoritative.

Thirteen original short combat sounds cover grouped gem matches/landings, explosions, poison, hits, healing, shields and abilities. A bounded six-voice mix prioritizes impacts and player casts. Android/iOS haptics provide short grouped landing/impact/cast pulses with independent persistent vibration control. Feedback is suppressed during pause, focus loss and snapshot restore/replay. Physical-device vibration feel and speaker/headphone mix approval remain outstanding. Current validation evidence is recorded in `Docs/Validation/DUNGEON_PRESENTATION_POLISH.md`; previous pass evidence remains historical.

## Remaining-cast source checkpoint

`ArtSource/RemainingCast/` now preserves the 18 supplied LibreSprite tabs and adds
17 recolored stills plus 17 nine-frame, 130 ms idles for the guards, knights, royal
cast and Minotaur. Recolors preserve original masks and reuse guide colors; idles
coordinate head/eyes, shoulders and body over planted soles, with rigid tools and
subtle cloth follow-through. The supplied Rattlebones idle is unchanged. Native
files, exact PNG/GIF exports, per-character palettes and a synchronized native-size
preview are included. The guide's current version is v1.5.

This checkpoint is source artwork only: none of these seventeen new idles has been
assigned to Unity definitions in this pass. Visual approval and later integration
remain next art steps; the Minotaur source does not imply a new playable definition.
See [source notes](../ArtSource/RemainingCast/README.md) and
[executed art checks](Validation/REMAINING_CAST_ART.md).

## Locked/preserved art direction

- Existing gem identity and major frame language should be treated as established unless the user explicitly requests a redesign.
- Pixel art must remain sharp under the current Point-filtering / pixel-grid presentation rules.
- Do not stretch source sprites simply to make layout fit.
- Gameplay VFX should explain cause/effect, not hide the board.

## Text

For now, game text and numbers use a **readable non-pixel runtime font**. Text must be rendered by Unity UI rather than painted into sprites. The final global font is intentionally not decided yet and should remain centrally replaceable.

## Current HUD direction

The battle screen uses:

- chapter/wave information in the top HUD;
- player and enemy combat presentation above the board;
- a square match-3 board as the visual/gameplay center;
- the active ability in the bottom-center HUD;
- **Health Potion on the left** of the ability button;
- **Bomb consumable on the right**;
- safe-area-aware framing for portrait devices and cutouts.

Detailed layout ownership lives in `Docs/ARCHITECTURE.md`.

---

# 4. Core gameplay loop

## Meta loop

1. From the main menu, select a character / inspect character progression.
2. Spend shared Gold Coins on that character’s permanent level if desired.
3. Configure unlocked Gem Mastery choices.
4. Buy/equip optional Potion and Bomb consumables in the Shop.
5. Start a run.
6. Progress through waves, choosing run cards at milestones.
7. Die or defeat the King formation.
8. Receive Gold based on completed progress and milestone rewards.
9. Spend Gold, change build preparation, and run again.

The desired emotional loop is:

> “I know why I lost, I earned something useful, and one more upgrade, better decision, or different build could get me farther.”

## Battle loop

1. Player performs a legal gem swap.
2. The board resolves matches, created specials, special chains, clears, cascades, refill, and reshuffle.
3. Rewardable clears drive weakness damage, affinity healing, and ability energy according to their clear source/context.
4. Only after the player’s accepted board action fully settles may turn-counted enemy pressure queue board mutations.
5. Enemy auto-attacks provide separate real-time pressure.
6. Defeating all active enemies completes the wave.
7. The next wave cannot damage or mutate through stale callbacks from the old encounter.
8. At specified milestones, the run pauses for a card choice before progression continues.

One accepted board action must always resolve as one coherent authoritative sequence.

---

# 5. The four contributors to run performance

Dungeon Matcher deliberately separates four sources of success:

1. **Permanent character level = power floor.**
2. **Run cards = build trajectory / run-to-run identity.**
3. **Player skill = efficiency.**
4. **RNG = the situation the player adapts to.**

These must complement rather than replace each other.

Permanent progression should help a casual player overcome an earlier soft wall, but it must never become a hidden permission gate. A skilled low-level player with strong planning, special-gem setup, targeting and card choices must be allowed to outperform expectations.

Do not dynamically inflate enemies merely because the player is strong or clearing quickly. Strong builds are allowed to feel strong.

---

# 6. Pacing philosophy

The game should be **brisk, replayable, and suitable for multiple attempts in a session**, without forcing every run to be equally short.

Balance v1 experience targets are currently:

| Experience | Target |
| --- | --- |
| Beginner failure | ~2–4 minutes |
| Ordinary repeat attempt | ~4–8 minutes |
| Regular wave | ~12–24 seconds |
| Pressure formation | ~20–35 seconds |
| Miniboss formation | ~30–50 seconds |
| King formation | ~45–80 seconds |
| Successful opening arc including choices/transitions | ~10–14 minutes |

These are **review targets, never forced timers**.

Difficulty should rise in pulses:

**introduce → practice → combine → spike → breathe → checkpoint**

Later waves should become difficult through:

- stronger enemy combinations;
- target-priority conflicts;
- board manipulation;
- interacting abilities;
- timing pressure;
- learned mechanics being recombined;

—not primarily through giant HP pools.

Detailed progression authority: `Docs/PROGRESSION_PACING.md`.

---

# 7. Characters and permanent progression

## Current playable characters

### RattleBones

- Skeleton dungeon defender.
- Current Balance v1 Level 1 HP: **100**.
- Current Balance v1 Level 1 gem damage: **11**.
- Active ability: **Royal Decree**.
- Intended to be a more traditional/directly readable combat baseline than Bardley.

### Bardley

- Green slime bard/musician.
- Current Balance v1 Level 1 HP: **80**.
- Current Balance v1 Level 1 gem damage: **10**.
- Active ability: **Cracked Gems**.
- Cracked Gems targets ordinary gems first, cracks them, then resolves 3×3 explosions and established special/crystal interactions.
- Current Balance v1 cracked-center base damage: **20** before permanent/run modifiers.

## Permanent levels

- Each character levels **independently**.
- Shared Gold Coins pay for level-ups.
- There is no XP gate in Balance v1.
- Current level cap: **20**.
- Permanent levels raise health, gem damage, ability damage and shield capacity according to character-specific growth.
- Permanent stats must be recomputed from immutable base data; never repeatedly multiply already-modified runtime values.
- Run cards remain temporary and must not become permanent character data.

Current exact tables and cost formula live in `Docs/BALANCE_V1.md`.

---

# 8. Intentional temporary development overrides

These are **not bugs** unless the user explicitly changes direction.

## Bardley production balance (testing override retired)

The user explicitly retired the 1-energy testing override on 2026-09-15. The committed Cracked Gems cost is now **80**, with five 20-damage cracked centers and a **50% shared cast-refund ceiling**. At effective cost 64, the complete cast returns at most 32 energy. New runs start with 20% charge. These are production control values for human tuning, not a reason to force an early defeat.

Energy cards are eligible at normal costs. Tests may use isolated low-cost fixtures but must not restore a testing override to the production asset.

## Temporary global font

The current non-pixel font is a placeholder global typography choice. Preserve centralized runtime text and replace the font later rather than baking text into pixel assets.

## Supported run endpoint

The implemented run currently ends after the **King formation**. Post-King Guild/wider-world progression is designed directionally but not yet implemented as supported gameplay.

---

# 9. Gem weakness, affinity, energy and abilities

## Enemy weakness

Each active enemy has an assigned gem-type weakness. Rewardable cleared gems damage enemies whose weakness matches that gem color.

## Player affinity

The player also has a gem affinity. Affinity is independent from enemy weakness and can trigger character-side effects such as healing.

## Energy

Energy generation, storage and spending are separate systems.

- Ordinary rewardable match/special clears can generate energy according to their context.
- Ability-source clears do not automatically generate energy.
- Explicitly player-owned explosion paths may retain special-energy entitlement where designed.
- Energy is spent only after the ability runtime accepts activation.
- Rejected casts cost nothing.

## Ability architecture

Abilities use generic definitions/runtimes. Character-specific rules must not be hard-coded into the shared board controller. Abilities request board-owned work when they manipulate gems.

Detailed ownership rules: `Docs/ARCHITECTURE.md`.

---

# 10. Gem Mastery and special-gem progression

Gem Mastery is an account-wide progression layer, not a per-character grind.

Fresh-profile state:

- Color Crystal / classic star crystal is available from the start.
- It has no separate levels.

Current Balance v1 first-time account unlocks use the **highest individual character level** (levels are never summed):

- Level 2: directional bombs
- Level 3: Poison Bomb
- Level 4: Healing Bomb
- Level 5: Shield Bomb

Reaching the threshold on **any one character** unlocks it account-wide for every character.

Locked shapes must still behave safely and reward ordinary matching rather than becoming dead inputs.

Valid explicit legacy mastery selections are preserved through migration.

---

# 11. Run cards

Cards are intended to be the **main reason two runs with the same character and permanent level feel different**.

Current implementation:

- 27 card definitions retained in the catalog.
- Draft offers up to three distinct eligible cards.
- Draft RNG is separate from encounter RNG.
- Cards reset with the run.
- Card eligibility can depend on character, ability, permanent shared unlocks, equipped mastery and meaningful mechanic availability.
- Cost/energy cards are eligible for production Bardley at cost 80. Generic eligibility still excludes dead energy choices in isolated low-cost test fixtures.

Current card milestones after completed waves:

**2, 5, 9, 13, 17, 21, 25, 28**

Desired build identities include:

- gem/cascade offense;
- directional bomb chains;
- poison/sustained pressure;
- shield/survival;
- ability/energy specialization;
- boss specialization;
- character-specific ability builds;
- high-risk/high-output builds such as Glass Cannon.

Strong builds are allowed to clear faster. Do not secretly normalize them through adaptive enemy scaling.

Exact current card values/rarities/caps/eligibility: `Docs/BALANCE_V1.md`.

---

# 12. Shield system

HP and shield are separate resources.

Current Balance v1 direction:

- Player Level 1 shield cap: Bardley **40**, RattleBones **45**.
- Shield cap gains **+3 per permanent level**.
- Shield Bomb grants **25 + 2 per permanent level** before run-card modifiers.
- Aegis Reservoir increases both Shield Bomb grant and shield cap so its benefit is not lost against an unchanged cap.
- Existing shield mitigation applies once to the damage instance when shield was present, with overflow continuing into HP according to actor damage rules.
- Enemy shield users have their own grants/caps and must remain counterable; shielding must not create indefinite stalemates.

Exact numbers remain balance data and should be tuned from real play evidence.

---

# 13. Gold economy

Gold Coins are the shared account currency.

Gold currently pays for:

- permanent character level-ups;
- Health Potions;
- Bomb consumables.

There is no premium currency or XP requirement in Balance v1.

Current reward model pays for completed run progress rather than enemy farming:

- completed waves;
- milestone completion;
- King completion;
- personal-best progress;
- first King clear.

No ordinary kill or summon farming income is used.

The run journal settles rewards exactly once on death, victory or explicit End Run/Retry. Suspend and current-version interruption preserve the same attempt; only legacy journals without combat state settle on load. Partial unfinished waves pay nothing.

The economy goal is:

- early failed runs usually fund a meaningful early improvement;
- mid progression takes several decent runs;
- deep runs pay better in total and generally in gold/minute than deliberately farming shallow content.

Exact formula and level costs: `Docs/BALANCE_V1.md`.

---

# 14. Consumables and Shop

Balance v1 Shop sells only:

1. **Health Potion**
2. **Bomb**

Current prices:

- Potion: **18 Gold**
- Bomb: **24 Gold**

Rules:

- Inventory is persistent account state.
- Each item type has its own Equip/Unequip state.
- Both may be equipped simultaneously.
- At run start, each equipped item receives `min(3, owned quantity)` usable run charges.
- The rest of the persistent inventory does **not** refill the slot mid-run.
- Inventory is deducted only when an item use is actually accepted.
- Rejected/cancelled uses spend nothing.
- Each slot has an independent **5-second game-time cooldown**.
- Cooldowns pause with gameplay.
- Running out of run charges disables further use even if persistent inventory still exists.

Current Potion effect: heal **35% maximum HP**.

The Bomb consumable uses board-owned targeting/resolution and must not create a parallel clear/damage pipeline.

---

# 15. Enemy taxonomy and encounter philosophy

## Taxonomy

Do not conflate these concepts:

- **Rank** = mechanical complexity/importance (Normal, Special, Mini-boss, Boss, future Hero).
- **Faction** = locals, guards, knights, royals, Guild, etc.
- **Race/species** = human, dwarf, slime, etc.
- **Combat role** = attacker, support, disruptor, summoner, defender, etc.

Rank is about encounter importance and mechanical load, not species or faction.

## Enemy pressure

Enemies should increasingly create behavior-based pressure and board manipulation.

Some waves should feel temporarily puzzle-like because the board has meaningful constraints, but Dungeon Matcher remains a dynamic battler rather than a sequence of fixed handcrafted puzzles.

Enemy interference must remain:

- readable;
- reversible/counterable where appropriate;
- bounded by placement/cap rules;
- queued only at safe board-resolution points;
- unable to create permanent softlocks through ordinary legal behavior.

Mini-bosses and bosses should generally use their mechanics more deliberately and powerfully than introductory Specials. “Smarter” means bounded, understandable targeting heuristics—not omniscient cheating.

---

# 16. Teaching-before-testing philosophy

Important boss mechanics should usually be foreshadowed by simpler enemies before the boss combines or escalates them.

The preferred learning rhythm is:

1. Show a simple mechanic.
2. Let the player practice against it.
3. Combine it with another pressure source.
4. Let a miniboss/boss test mastery with a stronger or more deliberate version.

This is inspired by good action/platformer encounter teaching: the player should recognize the language of a mechanic before a boss weaponizes it.

It is a guiding rule, not a requirement that every boss move must appear identically beforehand.

---

# 17. Current encounter generation and opening arc

The current opening run uses a hybrid system:

- weighted overlapping faction eras;
- constrained random compositions;
- authored encounter recipes;
- threat budgets;
- active-slot/category limits;
- once-per-run milestone leaders;
- milestone windows instead of deterministic surrounding wave scripts.

Current enemy slots: **3**.

Current authored-recipe opportunity: **45%**, otherwise constrained weighted generation.

Current faction anchors:

- Locals from wave 1
- Guards become available around wave 6
- Knights around wave 11
- Royals around wave 19

Current milestone windows:

| Leader | Window | Escort model |
| --- | --- | --- |
| Town Marshal | 7–8 | one local escort |
| Siege Sergeant | 12–14 | one guard escort |
| Knight Captain | 18–20 | two knight escorts |
| Royal Archbishop | 24–26 | one royal escort |
| King | 29–30 | required Archbishop |

Mini-bosses should not routinely be isolated just because they are milestone enemies. The **whole formation** must be budgeted.

Old factions decline gradually rather than disappearing immediately. Named leaders generally do not respawn in the same run; the King’s required Archbishop pairing is the current explicit narrative exception.

Full recipes, enemy base stats and threat values: `Docs/BALANCE_V1.md`.

---

# 18. Current implemented opening enemy set

Balance v1 currently tunes **21 implemented enemy definitions** through the King:

- Farmer
- Basket Villager
- Pan Villager
- Miner
- Barricade Villager
- Town Marshal
- Spear Guard
- Crossbow Guard
- Barricade Guard
- Siege Sergeant
- Knight
- Spear Knight
- Shield Knight
- Knight Captain
- Royal Swordsman
- Royal Lancer
- Royal Arbalist
- Royal Standard Bearer
- Court Mage
- Royal Archbishop
- King

The larger post-King roster/faction plan is future content and should not be mistaken for currently supported gameplay.

---

# 19. Board-pressure mechanics and current rules of thumb

The current game includes or supports:

- mined holes;
- barricades;
- movable chains/pins;
- Court Mage freeze-style pins;
- Royal Standards;
- warning markers and delayed royal attacks;
- summons/escorts;
- enemy shielding;
- poison and other special-gem effects.

Crossbow Guard and Knight Captain now share the same falling/movable chain family instead of behaving like unrelated versions of the same idea. They may differ in quantity/cadence by rank.

Current Balance v1 structural safety direction includes:

- bounded mined/barricaded cells;
- bounded total pinned/frozen capacity;
- board legality checks before structural placements;
- queued mutations after board settlement;
- cleanup tied to correct ownership/lifetime.

Do not introduce direct obstacle dictionary mutations from enemy scripts.

---

# 20. Menu, settings and audio state

Current connected UI flow includes:

## Main menu

- Play / Continue with the saved character and run recap
- Characters
- Gem Mastery
- Shop
- free Practice
- post-King No Supplies / Board Only challenges
- selected or saved character presentation
- shared Gold display where relevant

## Characters

- choose character;
- independent level display;
- current stats;
- next-level improvements;
- level-up Gold price;
- affordability/max-level handling;
- small level-up feedback/VFX.

## Shop

- Potion and Bomb only for now;
- purchase quantities;
- owned stock;
- Equip/Unequip state.

## In-run settings

Top-right settings control with:

- Resume / close
- Suspend to Menu (preserves the exact attempt)
- End & Retry / End Run (settles once)
- Music mute
- SFX mute
- vibration on/off
- reduced motion

Music, SFX and vibration preferences persist independently.

## Death/end-of-run

Death UI shows ending wave, last actual damage, earned gold, supply replacement value, remaining owned stock, scrollable build and Retry / Change Build. Reward settlement remains exactly-once.

---

# 21. Architecture rules that must survive future work

These are non-negotiable unless the user explicitly approves an architecture redesign:

1. **BoardController owns board state/rules/resolution.**
2. Use **one authoritative board-resolution pipeline**.
3. BoardController remains **character-agnostic**.
4. Accepted board work keeps ownership until fully settled.
5. Enemy board mutations queue behind player resolution.
6. Damage stays centralized through established combat/actor flows.
7. HP and shield remain separate resources.
8. Energy generation/storage/spending remain separate responsibilities.
9. Ability energy is spent only after accepted activation.
10. VFX/presentation never becomes gameplay authority.
11. Clear contexts/results carry explicit source/identity; do not infer rules from mutable presentation or coincident numbers.
12. Prevent duplicate damage, rewards, clear reports, board mutation, callbacks and next-wave leakage.
13. Persistent account state and per-run temporary state remain distinct.
14. Card draft RNG stays separate from encounter RNG.
15. Enemy behavior stays data-driven where practical.

Read `Docs/ARCHITECTURE.md` before implementation work that touches these systems.

---

# 22. What Balance v1 currently implemented

Merged PR #157 supplied the first connected meta/balance layer, extended by the revised implementation above. Its base systems include:

- persistent shared account save/progression;
- shared Gold Coins;
- independent character levels 1–20;
- permanent stat growth;
- account-wide Gem Mastery unlock milestones;
- migration/preservation rules for existing selections;
- run reward journal and exactly-once settlement paths;
- 27-card rebalance/eligibility pass;
- player shield-cap / Shield Bomb / Aegis interaction pass;
- enemy HP/damage/cadence first-pass tuning;
- disabled player-power enemy correction;
- hybrid authored + weighted encounter generation;
- 16 authored encounter recipes;
- threat-budget and disruption-cap rules;
- escorted milestone formations;
- King around waves 29–30;
- Crossbow/Captain shared falling-chain behavior;
- obstacle/interference caps and legality checks;
- Potion/Bomb Shop;
- inventory/equipment;
- max 3 run charges per equipped item type;
- accepted-use inventory deduction;
- independent 5-second cooldowns;
- Characters and Shop menu flows;
- run/death reward presentation and exit handling;
- paused in-run settings;
- independent persistent Music/SFX controls;
- consumable HUD slots and pixel-art sources;
- centralized temporary non-pixel typography;
- focused Balance v1 validation tooling and evidence.

Detailed values: `Docs/BALANCE_V1.md`.

Executed validation evidence: `Docs/Validation/BALANCE_V1_VALIDATION.md`.

---

# 23. Validation status of Balance v1

Astra/Codex reports the following on Unity **6000.3.19f1** for PR #157:

- `Tools/Validate-Unity.ps1`: success / exit 0.
- `Tools/Test-Balance.ps1`: **231 passed, 0 failed, 0 skipped**.
- One included Play Mode wrapper exercised **108 existing lifecycle cases**; do not add these to 231 as if they were extra NUnit tests.
- Actual MainMenu/Game integration was exercised.
- Fresh/existing save behavior, failure atomicity, shared unlocks, card/shield interactions, concurrent disruptors and Royal mechanics were exercised.
- Manual visible pointer play was performed.
- Short/tall portrait renders with simulated cutouts were checked.
- Independent audio persistence was checked across two Unity processes.
- Eight final engine playthroughs were run across both characters / levels / bare-equipped policies.

Important limitation:

> This is targeted implementation evidence, **not a repo-wide audit and not human retention proof**.

Physical Android touch/cutout/audio testing and broader human/build/seed balance testing remain outstanding.

---

# 24. Current known balance/tuning risks

These are the most important unresolved Balance v1 questions right now.

## Bardley is substantially stronger in synthetic testing

Historical Balance v1 measurements used an uncapped normal-cost clone, before the current refund ceiling and revised encounter teaching. They do not measure the current implementation. A casual Bardley sample reached approximately wave 28 in ~13.7 minutes, while casual RattleBones died after 10 completed waves around ~5.5 minutes.

Do **not** “fix” this by secretly scaling enemy HP based on character/player power.

Next balance work should investigate:

- Bardley target count / Cracked damage / chain leverage;
- card interactions and refund engines;
- ability energy at intended production cost;
- human planning versus the synthetic policies;
- whether RattleBones is underpowered, Bardley overpowered, or both;
- encounter pressure differences caused by their kits.

## Encounter duration variance

Several ordinary/miniboss/King formations currently resolve faster or slower than target ranges depending on board/build state. This is expected in a run-based game, but extreme outliers should be reviewed for whether they are exciting power expression or broken pacing.

## Human first-run wall is not proven

Synthetic policies are not beginner humans. The intended “readable early soft wall → useful Gold → stronger retry” loop still needs human playtest evidence.

## Current Bardley control values

The 80-energy production cost and 50% cast-refund ceiling are implemented. Further tuning should compare meaningful cast timing, board decisions and both characters at matched progression. Synthetic performance alone does not establish human enjoyment or justify a mandatory beginner loss.

---

# 25. Current next priorities

Unless the user explicitly changes direction, the most sensible next work after Balance v1 is:

1. **User play and iteration on the merged revised-design implementation.** PR #157 was merged before this work.
2. **Human-feel balance passes**, especially Bardley vs RattleBones and early soft-wall pacing.
3. **Tune encounter outliers** using real player behavior rather than only synthetic runs.
4. **Physical mobile validation**: Android touch, cutouts/safe areas, audio behavior and performance.
5. **Polish the progression/menu UX** after the user has lived with the first implementation.
6. **Finalize typography** later; current font is intentionally temporary.
7. **Tune production abilities** using the current 80-energy Bardley cost and shared refund ceiling as the control.
8. **Build post-King content**: Adventurer Guild, wider fantasy factions, later bosses, additional authored formations and cards as needed.
9. Near feature completion, perform another **repo-wide Astra audit/validation** rather than repeatedly re-auditing the evolving prototype after every feature.

---

# 26. Deferred / future content

Not currently complete as supported gameplay:

- Adventurer Guild era after the King;
- wider-world factions and long-run content;
- final release roster beyond the current 21 implemented opening enemies;
- final global font;
- further human tuning of Bardley and RattleBones;
- broad real-player balance telemetry/distributions;
- monetization systems (none are required for the current game loop);
- any future difficulty modes / starting-depth acceleration for very high permanent progression.

Do not infer missing future systems from old brainstorms as if they were approved implementation requirements.

---

# 27. Detailed documents and their authority

Future assistants should use this order:

1. **`AGENTS.md`** — workflow, repository safety, validation, architecture-preservation rules.
2. **`Docs/PROJECT_STATE.md`** — this file: current game definition, current milestone, implemented/missing/risks/overrides.
3. **`Docs/GAME_DESIGN.md`** — finalized mechanic behavior and durable gameplay rules.
4. **`Docs/PROGRESSION_PACING.md`** — encounter eras, run pacing, replayability, faction escalation and milestone philosophy.
5. **`Docs/ARCHITECTURE.md`** — technical ownership and invariants.
6. **`Docs/BALANCE_V1.md`** — current Balance v1 numbers, tables, card values, enemy stats, recipes, economy and first-pass rationale.
7. **`Docs/Validation/BALANCE_V1_VALIDATION.md`** — what Balance v1 was actually tested, with limits/evidence.
8. Mechanic-specific documents such as `Docs/ROYAL_SPECIALS.md` and audit documents when touching those systems.

This file summarizes; the detailed owner document wins within its domain unless the user has explicitly approved a newer direction that has not yet been documented.

---

# 28. Fast onboarding checklist for a fresh ChatGPT/Astra/Codex session

Before answering a broad project question or implementing a feature:

1. Read `AGENTS.md`.
2. Read `Docs/PROJECT_STATE.md` completely.
3. If the question is about gameplay rules, read the relevant `GAME_DESIGN.md` section.
4. If it is about waves/balance/progression, read `PROGRESSION_PACING.md` and `BALANCE_V1.md`.
5. If it changes runtime ownership/flow, read `ARCHITECTURE.md`.
6. Inspect the current code/assets/branch instead of trusting this summary blindly.
7. Check **Intentional temporary development overrides** before “fixing” suspicious values.
8. Do not ask the user to restate project history that is already captured here.
9. After the work, update this file if the project state materially changed.

---

# 29. Short identity card

If only a very short refresher is needed:

**Dungeon Matcher** = portrait mobile pixel-art match-3 dungeon battler where the player is the dungeon monster defending an artifact. Match gems → exploit enemy weaknesses → heal via affinity → charge character abilities → create/chains specials → survive real-time attacks and turn-counted board interference → choose temporary run cards → defeat increasingly serious invaders. Permanent per-character levels bought with shared Gold raise the starting floor; account-wide Gem Mastery unlocks and optional Potion/Bomb consumables support progression. Runs use weighted overlapping factions plus authored formations, with miniboss escorts and the King around wave 30. Cards are the primary run-to-run differentiator. Skill must let strong players exceed the expected progression curve; permanent power creates soft walls, never hard gates. Visual identity is crisp dark-purple dungeon pixel art with charming monster characters, jewel gems and warm torch accents. Current supported arc ends at the King. Balance v1 exists but still needs human tuning, especially Bardley vs RattleBones.
