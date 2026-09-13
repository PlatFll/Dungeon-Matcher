# Cracked chain upgrade reporting audit — 2026-09-13

Gameplay audit chunk 16 / validation backlog group 15.

## Status and dependency

IMPLEMENTED / SOURCE AND DIFF REVIEWED / UNITY VALIDATION DEFERRED / NOT MERGED.
No Unity compilation, NUnit discovery/execution, actual shatter, Play Mode or device testing has run in this GitHub editing pass. Hold for the user's final combined validation stage.

Branch: `fix/cracked-chain-upgrade-scope`.
Incremental base: `4aad2826d315a2f723dbcbdf2aa8c25a80428aa9`, PR #146 (`fix/cracked-center-energy-attribution`).
Dependency order: **#142 -> #146 -> this change**. PR #143 is a separate sibling dependent on #142, not an additional prerequisite here. Integrate the existing fixes; do not replace their versions with main's older files.
Main inspected: `4e2e959186492636d44c91c33514d65aa1f724c1`.

## Confirmed defect

`RunUpgradeResolver.ResolveGemDamage` explicitly admits `Ability` clears with `GrantsSpecialEnergy` to the existing Bombsmith and Chain Reaction calculations. Those calculations depend on the current special-clear counts. Ordinary bomb reporting publishes `RunUpgradeSpecialClearPrepared` and clears that scope with `RunUpgradeSpecialClearFinished`.

`ReportCrackedClearSetToCombat` did not publish that scope. A Row/Column Bomb caught by a Cracked explosion could therefore perform its established blast while ordinary collateral damage missed Bombsmith's configured bonus. Multiple participating bombs similarly failed to supply Chain Reaction's count. These are source-confirmed missing-data paths, not observed Unity reproductions or measurements of frequency.

## Correction and boundaries

Wrap the existing Cracked report body in the same prepare/finish signaling, using its authoritative expanded set. The finish runs in `finally`, including null-center early return and observer exceptions. The original report body is a private helper inside the same BoardController partial class, not a new resolver.

- Ordinary collateral retains `Ability` source and explicit explosion-energy entitlement.
- No duplicate Bomb-source report, extra damage instance, reward dispatcher or utility-bomb commitment is introduced.
- The existing classifier counts Row, Column, Poison, Healing and Shield Bombs only. Cracked centers and Color Crystals do not manufacture additional bomb counts.
- Fixed-center damage retains its existing fixed-damage/ability-upgrade path and PR #146's center marker. This fix does not extend collateral card bonuses onto fixed centers.
- Bombsmith stays +25%; Chain Reaction stays +10% per extra counted bomb, capped at +30%; their existing additive calculation remains unchanged.
- No change to HP/shield, refunds, energy costs/caps, target selection, explosion footprints, chaining, stagger, mastery preferences, scene/prefab data, imports or art/layout.
- Exception coverage verifies scope cleanup, not rollback of damage already applied or general exception recovery for the board.

Only incremental production file: `Assets/_Game/Scripts/Board/BoardController.CrackedGems.cs`.
Two nonfunctional formatting differences (one blank line and EOF newline) are visible in the reviewed diff; there are no other production logic changes outside the report wrapper/helper.

## Regression coverage authored — NOT RUN

`Assets/_Game/Scripts/Editor/CrackedChainUpgradeScopeTests.cs`: **20 NUnit cases** across 11 methods.

Cases cover the five participating bomb types; one/two/four/five-bomb Chain Reaction counts/cap; additive card composition; unchanged fixed-center damage; hidden-crystal exclusion; unchanged no-card damage; no utility activation from reporting; finished-scope isolation; preparation/outcome observer failures; empty/null input; all-special fallback without centers; inactive encounters; and actual expansion followed by reporting.

Fixtures use real Resources assets read-only, the current runtime/hooks and board/combat reporting, plus disposable inactive enemies with seeded stats. One collateral control gem uses a test-only 100-damage base to isolate percentage arithmetic, and a separate enemy verifies the fixed center remains 50 without relevant ability upgrades. The standard energy producer is absent in bonus-isolation fixtures. RNG is restored; no PlayerPrefs, source assets, live singleton or authored scene is modified.

These synchronous tests do not execute actual ability acceptance/spending, shatter, refill, cascades, poison timing or whole-scene progression. Their existence is not a PASS.

## Additional read-only coverage and confirmed testing override

Reviewed the current GameManager/bootstrap paths, EnemyDatabase eligibility/weighting, encounter composition, spawn profile/standard data, run-upgrade bootstrap/catalog, and relevant Game-scene player/wave fields. Traced Game's enemy database, difficulty profile and wave profile GUIDs to their corresponding assets; traced both Resources player definitions' active-ability GUIDs to their ability assets. This is targeted reference coverage, not proof that every repository GUID or file has been checked.

**User decision, 2026-09-14: Bardley's 1-energy cost is intentional. Keep it unchanged for now.**

- The user reduced the cost to speed through waves and test later-wave enemies. It is an authorized testing override, not an activation bug or unresolved configuration question.
- Current referenced `Data/Player Abilities/Ability_CrackedGems.asset` serializes `energyCost: 1`. Preserve that value during this audit, integration and combined validation.
- `CrackedGemsAbilityDefinition` initializes that field to 80, and `Docs/GAME_DESIGN.md` records 80 for Bardley. The temporary override does not replace the documented non-testing balance or authorize changing the C# default.
- The asset at `d65f982752a28e3c7bb8b859e74fe4ccd2dec12f` had 80. Its subsequent path history includes `4a161eb7481f0636ee90ddbffc1f1a4bf40b84b0` ("Adding shield knight"), after which the current serialized value is 1.
- Report the actual tested cost. Test fixtures may exercise controlled costs without saving changes, but do not certify normal 80-energy pacing from accelerated 1-energy playtesting.
- Do not restore 80 until the user explicitly requests removal of the testing override. Royal Decree's cost and the current scene energy capacity remain 100.

This decision supersedes the original PR/backlog request for confirmation. The earlier Aegis Reservoir shield-cap observation remains unchanged.

## Final combined validation backlog

Integrate #142 then #146 then this branch, alongside the other audit groups, before a single combined validation stage.

1. Equip Bombsmith on Bardley and actually chain a Row/Column Bomb through a Cracked blast. Verify ordinary collateral HP damage gains the configured modifier once. Use both no-card and ordinary-bomb-chain controls.
2. Equip Chain Reaction and trigger one/two/four/five counted bombs. Verify its existing cap and additive composition with Bombsmith. Test utility-bomb combinations without granting Bombsmith merely because they are specials.
3. Distinguish fixed centers, normal collateral, caught bombs and a Color Crystal's hidden color. Preserve center damage upgrades, Resonant Cracks classification/refunds and existing per-gem energy exactly once.
4. Confirm the next unrelated clear receives no stale counts. Test a batch without centers and an encounter ending during accepted board work. Do not reopen combat or leak damage to the next wave.
5. Play-test actual shatter-time Healing/Shield/Poison commitment, crystal conversion, refill/cascades, recovery and board locking with the prior fixes. Reporting must not become a second activation point.
6. Run all 20 new cases with PR #146 and #142 suites, existing Bardley/upgrade/board/gameplay regressions and `Tools/Validate-Unity.ps1` in the authorized final combined pass. Investigate skipped/missing tests instead of counting them as passes.
7. Preserve Bardley's user-authorized serialized cost of 1 and record it in the result. No automatic restoration to 80. Do not commit fixture cost overrides or other temporary scene changes.

The source audit remains incomplete: coverage reconciliation and remaining static/serialized-reference checks are separate work. No whole-repository bug-free certification is implied.
