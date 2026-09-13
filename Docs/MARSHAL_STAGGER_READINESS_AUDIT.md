# Enemy-special readiness audit — Town Marshal

Audit chunk 6; combined-validation backlog group 5.
Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after PR #134).
Branch: `fix/marshal-stagger-readiness`.

## Status

IMPLEMENTED / SOURCE AND DIFF REVIEWED / UNITY VALIDATION DEFERRED / NOT MERGED.
The user requested one combined validation stage after the repository scan.
Compilation, NUnit execution, Play Mode and the required Unity validator have NOT RUN for this change. Do not initiate a separate validation or merge now.

## Review scope

Reviewed shared `EnemySpecialActionAvailability`, Miner, Crossbow Guard, Barricade, Shielding Allies and Town Marshal readiness callers; inspected the mining mutation processor and pin reservation/release paths. Compared the Marshal with the established special-startup stagger gate and its actual `Data/Enemies/Enemy_TownMarshal.asset` (stagger duration multiplier 1, three-move cadence). No serialized data changes are required.

This pass confirms the issue below; it is not certification of every obstacle, enemy special, interruption or cleanup combination. Remaining structural/telegraph cleanup and game-over lifecycle work still belongs to the larger audit.

## Confirmed defect

The Marshal implements its own ready-action polling instead of passing through the shared stagger-aware helper. Neither `TryUseReadyAbility` nor `WaitUntilReadyAbilityCanExecute` checked `EnemyStagger.IsStaggered`. His counter-ready event, an auto-attack action release, a polling retry or a rally-expiration callback could start Ring the Bell or Citizens, Seize Him! during an active stagger.

The attack timer being paused by EnemyAutoAttack did not protect these separate special entry paths. A concrete reproduction is a move that both fills the Marshal's stagger meter and reaches his three-move special cadence: after the board settles, readiness can summon a protector while he is still staggered.

## Small correction

Add one private stagger query and use it at both existing gates:
- before claiming a special animation action or applying a summon/rally;
- before the existing ready coroutine retries that entry point.

The second check matters: checking only the immediate method would allow the old poll to immediately call the blocked method again and recursively restart its own retry.

Retain the ready counter while blocked. The existing poll retries once stagger ends, the board is idle, the actor has no other action and a legal ability exists. No additional player move is required solely because stagger ended. No new timer, board resolver, general-purpose scheduler or event subscription was introduced.

Only ACTIVE stagger blocks startup. Partial buildup and the five-second post-stagger immunity do not. Already-applied rally and retreat keep their existing lifetime rules. Stagger does not revoke existing independent summons or cancel their damage interception by itself.

The final production diff changes only TownMarshalEnemyAbility.cs. Normal ability alternation/fallback, three-move cadence, summon candidates/slot service, two-move retreat, protector-defeat breathing-room reset, rally strength/duration, death cleanup, and every balance/art/scene/prefab field remain unchanged. Diff review caught and restored an accidentally omitted protector subscription before opening the PR; no net change to that subscription remains.

## Authored regression coverage — NOT RUN

`TownMarshalStaggerTests` contains 9 EditMode cases:
1. A real counter-ready event while staggered retains charge without summoning.
2. Auto-attack action release cannot bypass stagger.
3. The ready IEnumerator waits through stagger, busy board and another actor action, then requests exactly one summon and resets once.
4. Partial buildup does not block a cast.
5. Post-stagger immunity does not block a cast.
6. An otherwise legal rally cannot start while staggered.
7. Existing rally expiration removes its old buff without starting a ready new cast during stagger.
8. Marshal death stops a deferred retry and ends retreat without defeating the independent local.
9. Protector death still clears retreat and held readiness during stagger.

These are branch/ordering checks on inactive disposable actors with injected stats, a summon-service double and explicitly advanced coroutine yield boundaries. They invoke production methods/events, but do not simulate elapsed stagger/rally time, Unity's coroutine scheduler, actual wave spawning or rendering. They do not write PlayerPrefs or mutate authored assets, and restore Unity RNG state. Real-time and visual checks remain required in the combined stage.

## Final combined-validation checklist

- Include this branch with #135, #136, #137 and #138. Its production file does not overlap those branches.
- Discover/run all 9 new cases plus existing stagger, Marshal, gameplay and earlier audit regressions.
- In the real Marshal encounter, make him ready while staggered. Test both summon and rally eligibility, action-release retries and an old rally ending during stagger.
- Verify no special begins or consumes its charge while staggered; no early enemy action/board lock; automatic retry occurs after all gates clear, not after an extra player move.
- Test stagger ending during an ongoing cascade and while another actor action remains in progress; no premature summon/rally.
- Test partial buildup and post-stagger immunity: the enemy may act normally.
- Confirm an existing rally expires normally, retreat lasts its normal moves, protector death still removes retreat/resets held readiness, Marshal death cancels retries, and summoned locals remain independent wave members.
- Inspect white-blink feedback against actual ability start timing; do not infer gameplay correctness from VFX alone.
- Run required Tools/Validate-Unity.ps1 on the final integrated state. Do not weaken assertions or adjust tuning to obtain PASS.

Full repository audit remains incomplete. No individual validation or merge is requested at this stage.
