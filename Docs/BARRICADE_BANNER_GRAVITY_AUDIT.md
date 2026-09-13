# Barricade / Royal Standard gravity audit

Gameplay audit chunk 7; validation backlog group 6.
Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after validated #134).
Branch: `fix/barricade-banner-gravity`.

**IMPLEMENTED / SOURCE-DIFF REVIEWED / UNITY VALIDATION DEFERRED / NOT MERGED.**
The user requested one combined validation pass after the source audit, not an individual run for this issue. Compilation, EditMode execution, actual clear/fall timing and visual inspection have NOT RUN here.

## Confirmed missing interaction

`ROYAL_SPECIALS.md` requires every real gravity opening below a standard to contribute to the current fall. Gem destruction already notifies banner gravity through `Gem.OnDestroy -> NotifyGemDestroyedForPins -> NotifyGemDestroyedForRoyalBanners`. Restoring a Miner hole explicitly calls `NotifyRoyalBannerSpaceOpened`.

Destroying a barricade, however, only removed its entry from `barricadeCells` and started its optional hit visual. No opening was reported to banner gravity. The destroyed barricade is not a Gem, so it cannot supply the missing physical-gem notification later.

Reproduction geometry: place a standard at (2,5), a one-hit barricade at (2,3), and make a legal match in column 1 touching that barricade at (1,3). Clear no gems in column 2. The barricade disappears and its cell becomes refillable, but previously the standard receives zero gravity steps. In mixed clears it may instead fall too few steps. This can prolong its existing enemy-speed aura; it is not a new or stronger aura.

These are code-level findings, not a claimed local Play Mode reproduction. Persistent barricades from earlier waves can coexist with a later standard; no spawn/balance modification is required.

## Minimal correction and ordering

- Immediately after authoritative barricade removal, add exactly one opening through the existing `QueueRoyalBannerGravityOpening` method.
- Do not count a nonlethal durability hit or change once-per-clear barricade hit deduplication.
- Barricade damage is reported BEFORE the matched gems finish their clear animation. Consume the pending gravity batch in Update only after the existing physical-gem-destruction signal has arrived. This keeps the barricade openings and actual gem openings at the same original banner position until the full clear batch is collected.
- Without that gate, a newly queued barricade opening could move a standard early during the flash, then a later same-clear gem destruction could appear at/above the new standard position and be missed.
- Miner restoration retains its existing immediate flush directly before environmental collapse. There is no second gravity resolver, board scan, new destruction reward or VFX-driven gameplay signal.

Production files:
- `Assets/_Game/Scripts/Board/BoardController.Barricades.cs`
- `Assets/_Game/Scripts/Board/BoardController.RoyalBanners.cs`

No fields renamed, art/import/scene/prefab changes, new balance values, or changes to obstacle durability/caps, standard placement, one-standard-per-bearer, non-stacking aura strength, orphan lifetime, fixed-pin skipping, lower-first standard ordering, removal-at-bottom or enemy damage rules. Barricades.cs has a nonfunctional final-newline difference which can be restored during local cleanup.

## Authored tests, NOT RUN

`BarricadeBannerGravityTests`: **12 EditMode cases** using disposable inactive BoardController/Gem fixtures, injected obstacle states and the actual barricade-damage/opening methods. They require no live run-upgrade singleton, do not change PlayerPrefs/assets, do not run coroutine presentation, and do not constitute a falling/refill/aura test.

Coverage:
- Side-only clear: broken obstacle queues one opening even with no cleared gem in the standard column.
- No early Update-time movement during the clear flash; the physical-destruction signal marks the batch ready.
- Surviving stone hit queues nothing; a second distinct hit queues one opening.
- Multiple adjacent gems count once; ignored/preserved seeds count none.
- Repeating a hit after the obstacle has already been removed cannot count it again.
- Multiple broken obstacles and a destroyed gem accumulate in the same batch.
- Openings above a standard or in another column do not affect it.
- Orphaned standard and orphaned barricade each preserve the interaction.
- Multiple standards above one opening each receive the existing accounting.
- No-standard boards retain ordinary barricade removal behavior.

## Final combined-validation backlog

1. Compile/discover/run all 12 tests on the integrated audit state with #135-#139. When practical, verify the new opening regression fails against the original baseline.
2. Play the reproduction geometry through a REAL legal swap, not just direct method calls. Confirm the standard falls one eligible slot and every playable cell refills. Repeat with a stone barricade: first hit no fall, breaking hit one contribution.
3. Clear gems AND break barricades in the standard's column in one match/bomb/Cracked resolution. Verify the full count, no early mid-flash remapping, no lost gem, no duplicate/missing opening and no movement past another standard.
4. Verify normal match, bomb, crystal, Cracked and environmental gem-destruction paths still release their batch; test a column bomb taking a standard to the bottom in one clear.
5. Test multiple standards, fixed pins/freezes, mined cells and Miner restoration, orphaned obstacles, missing optional art, bottom removal, one remaining standard retaining the aura and last-standard removal clearing it once.
6. Check exactly-once damage/healing/energy and next-wave isolation. Run existing Royal Milestones / Royal Play Mode, Siege Sergeant, shared gameplay/previous-audit tests and `Tools/Validate-Unity.ps1` at the final authorized stage.
7. Diagnose production vs fixture failures; do not weaken assertions, reset mastery or rebalance to pass. No merge before combined validation and approval.

## Other paths inspected in this chunk

Read the queue/execution and caller boundaries for targeted hammer pairs, Restoration/Judgment gem sets and Bombardment lanes; Siege Sergeant and Archbishop callbacks; barricade placement/durability/orphaning; pin reservations/release and physical-destruction forwarding; Royal Standard placement, fall batching and removal; Miner restoration. Owner/target validation already exists in the reviewed warning paths, so no speculative warning rewrite was included.

This is not certification of every cancellation timing, every enemy runtime or the full repository. The move-availability/reshuffle path with pinned specials and the game-over/revival UI lifecycle remain later audit areas. The broader source scan is still incomplete.
