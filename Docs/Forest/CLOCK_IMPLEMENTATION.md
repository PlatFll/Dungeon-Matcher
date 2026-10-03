# Forest timer profiles

New tests use `seconds-basics-move-abilities-v1`. `EnemyAutoAttack` and its HUD use
seconds, with countdown paused during accepted board resolution, stagger and
specialist holds. Attack impact/recovery still use the existing guarded sequence.
No move decrement or reset is applied to that stored seconds progress.

Existing `accepted-moves-v1` saves retain the table below. The remaining forest
buffs, stagger, poison and supply cooldowns also keep these move semantics in new
hybrid tests; the user's change specifically restored ordinary attack timers.
Production/version-1 saves retain their original rules. The implementation
contract supplies exact expiry order.

| Owner | Move profile | Existing seconds retained for |
|---|---|---|
| EnemyAutoAttack / definitions | Explicit first/repeat cadence; one readiness reduction per accepted action; one ordinary sequence; owned speed multiplies progress, clamped 0.1–5 | Attack contact, recovery, follow-up spacing, missing-event timeout |
| EnemyLifecycleVFX | Spawn visuals cannot stop/reset logical readiness | Entrance and death effects |
| EnemyActor / special runtimes | Only current surviving actor may accept a special; stable persistent IDs; newly spawned actors wait for a future tick | Accepted special presentation |
| King / Captain command | Existing reservation and per-hit guards; participants cannot also spend an ordinary action | Sword motion, command spacing; held King keeps basic progress paused |
| Royal warnings | Existing completed-move deadlines; shared scheduling and board queue | Warning flashes and strikes |
| Royal Decree | 3 full actions, Longer Reign adds 1 (cap 4); base bonus 5/gem; expire after final rewards; match energy suppression preserved | Mark/pulse visuals |
| ChronoShutter | Existing five accepted actions; waits for shared combat settlement before photo restore; does not rewind clocks/proc budgets | Cast, rewind, recovery |
| Stagger | Trigger action + 1 future blocked action; 2 future immune actions; 1 future meter-grace action, then −0.25 meter/action | Hit flash, status visuals |
| Poison | 3 future ticks (+1 Slow Venom); refresh keeps next due tick; status damage remains non-staggering | Poison effect visuals |
| Marshal rally | 3 future actions; owned targets/speed saved; expiry after due work | Rally animation |
| Marshal retreat | Existing accepted-move counter and stable reconstructed protector | Retreat travel |
| Banner / Enrage | Source/threshold lifetime; speed enters future move progress | Aura and enrage visuals |
| Benediction | One entire committed attack sequence, including follow-up | Halo |
| Shields | Existing separate resource and whole-hit gate; no timer | Fill and floating numbers |
| Supplies | Two future moves per kind; accepted-use debit; no time recharge | UI feedback |
| Other cards / affinity / energy | Existing event-driven limits, rewards, refund budgets and persistent modifiers | Choice/reward presentation |
| Mender | Fixed persistent recipient, two future responses, exactly-once outcome, two future recovery actions | Authored start/hold/release; separate heal/cancel cue |
| Warden / Matriarch | Nonspreading board anchors, cast sequence, fixed ritual target, future response/exposure/recovery deadlines | Authored motion follows state events; cannot grant effects |
| Vines | Age and reservations in snapshot; warning on first response, at most one spread after second; independent environment owner | Dedicated vine, amber anchor and warning sprites |
| Continuation | Version 2 stores profile, zone, action and next actor identity; owners store timers and pending work; in-flight actions use existing replay journal | Frame deltas reproduce accepted presentation only |

Readiness is calculated for all surviving actors before any actor grants a new
speed buff. Excess progress is discarded when an ordinary sequence starts.
No catch-up burst or wall-time resource generation is introduced.

## Explicit dungeon comparison cadences

These values are used only under the internal move profile. They are authored
role choices, independent of existing seconds intervals and difficulty curves.
Both first and repeat are: Pan Villager 3; Basket Villager 4; Farmer 3;
Barricade Villager 4; Spear Guard 3; Barricade Guard 4; Crossbow Guard 4;
Miner 4; Town Marshal 4; Knight 3; Spear Knight 3; Shield Knight 4;
Siege Sergeant 4; Knight Captain 3; Royal Swordsman 3; Royal Lancer 3;
Royal Arbalist 4; Court Mage 4; Royal Standard Bearer 4; Royal Archbishop 4;
King 4. Existing dungeon HP/damage and live encounter tables remain unchanged.

## Save and photo boundaries

The prototype captures only fully settled action boundaries. An accepted input
in progress remains in the durable replay journal, preserving the original
checkpoint and RNG. Only all-move idle thinking skips empty frames. Hybrid runs
record seconds as the original timed continuation does; stable saves compact it.
Unknown schema/profile/zone combinations are rejected while retaining the run.
Version 1 explicitly means the legacy profile; it is never silently converted.

ChronoShutter restores vine geometry with the board. Surviving nodes retain
present age, reintroduced nodes get full future grace, dead owners are not
resurrected, and environmental sources remain separate. Pending enemy warnings
retain present deadlines and only surviving gem identities. The photograph
does not restore channel targets, consumed procs or combat tick.
Nonspreading anchor restoration additionally requires the current producer and
creation move to match a living cast, preventing old photos from reviving solved
anchors or donating them to a later ritual.
