# Gameplay audit coverage checkpoint

## Status

Audit chunk 19. **Read-only production review; documentation-only change.**

Pinned main: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after PR #134).
No new confirmed gameplay defect was established in the checks recorded below. No production code, serialized asset, prefab, scene, input binding, import setting or balance value was changed. No additional regression suite was authored in this chunk.

This checkpoint is not a Unity test result or a declaration that the full source audit is finished. Compilation, AssetDatabase import/loading, Play Mode and device checks remain deferred to the user's combined validation stage. Earlier percentage estimates were rough system-coverage estimates, not a measured percentage of files read.

## Completed: enemy database membership references

Read `Assets/_Game/Data/Enemies/EnemyDatabase_Main.asset` and each of the 21 referenced definition `.asset.meta` files at the pinned commit. The directory tree also contains the corresponding asset files.

Result: **21 database entries, 21 distinct referenced GUIDs, 21 matching definition metadata files**. There is no duplicate or unresolved GUID among these database membership entries. Each matching metadata file declares main object file ID `11400000`, matching its database reference. The database's own script GUID `02d2b410687169d4cbcc3b360b81b031` matches `Assets/_Game/Scripts/Combat/Enemies/Data/EnemyDatabase.cs.meta`.

These are repository-text comparisons, not results from Unity's AssetDatabase. This does not certify every internal reference inside each definition, all project GUIDs, imported object types, visual sprites, eligibility, balance or successful spawning.

All filenames below are under `Assets/_Game/Data/Enemies/` and use `.asset` plus `.asset.meta`.

| Definition | Matching metadata GUID |
| --- | --- |
| Enemy_Farmer | a76417e22f884fad9fc5f40479f897e7 |
| Enemy_BasketVillager | 655f6b9c480740719f00f56e0169908d |
| Enemy_PanVillager | 36c47a216df94b9fae10c869929010fa |
| Enemy_Miner | 84831a37e3ae4a6aa1147fb4b1f8e6b9 |
| Enemy_CrossbowGuard | 4b6fd7e690b34c219e51f1294a1b301a |
| Enemy_Knight | 6615ee48f5122354fb3bb48ee8cd8b8f |
| Enemy_SpearKnight | 3446e21fb60447829950580946a51eff |
| Enemy_BarricadeVillager | cf3b9a8c35434f72afc9567d6c0835de |
| Enemy_BarricadeGuard | 1b41367eaef540e7b4fbc553bf6d5a28 |
| Enemy_ShieldKnight | 7e5bdc7d5c3140a3a3fe37dc861dd4f0 |
| Enemy_TownMarshal | a8912c080c9d415082ff439864aefd06 |
| Enemy_SpearGuard | 3e9ea0e381044e83b0b982c96c74c769 |
| Enemy_SiegeSergeant | 9e704135ac48432c9bdb148a30da3e8a |
| Enemy_KnightCaptain | cb5b4ce0108940d788ee9a219b2fe946 |
| Enemy_RoyalSwordsman | 103ebefdbf0b40dcbf8f1e7c1537c2a3 |
| Enemy_RoyalLancer | 574ae9bff3c2431fb01239e7b48856f7 |
| Enemy_RoyalArbalist | e0401922f77142049ae15844298a5eb7 |
| Enemy_RoyalStandardBearer | 39f0ae90d3b445afbf8f722502c90a05 |
| Enemy_CourtMage | b2a9fefd5490455182d2f6f2740862f2 |
| Enemy_RoyalArchbishop | 8d684cf2178442b8b6e3d160fb5fac79 |
| Enemy_King | b9de130924cb44f3b77befc8fa1c0438 |

## Completed: enum-to-factory coverage

Compared the complete `EnemySpecialAbilityKind.cs` enum with `EnemySpecialAbilityRuntimeFactory.cs`.

All **11 non-None kinds** have a factory case: Miner, CrossbowGuardBolt, Barricade, ShieldingAllies, TownMarshal, SiegeSergeant, KnightCaptain, RoyalStandardBearer, CourtMage, RoyalArchbishop and King. None deliberately returns no special runtime; unknown values take the existing error path. No missing current factory case was found.

This is dispatch coverage, not proof that all serialized definition values are correct or that each runtime passes its gameplay tests.

## Additional source inspection in this chunk

The following exact scopes were read/rechecked; none produced a new confirmed defect in this pass:

| Source | Scope |
| --- | --- |
| Combat/Player/Runtime/PlayerAffinityHealing.cs | Full file: board subscription pairing, player/affinity guards, healing calculation and actor handoff |
| Combat/Player/PlayerAbilityMatchEnergyGain.cs | Full file: outcome subscriptions, source/entitlement gating, shape/special rewards, reference resolution |
| Combat/Player/PlayerAbilityEnergy.cs | Full file: storage, cap, spending, reset and changed-event publication |
| Presentation/CharacterAnimationPlayback.cs | Full file: pause/resume bookkeeping and impact event relay |
| Combat/Enemies/Runtime/RoyalStandardBearerEnemyAbility.cs | Full file: readiness, cap, placement completion, aura installation and owner orphaning |
| Board/BoardController.RoyalBanners.cs | Lines 1-360: request acceptance, placement/owner recheck, safe-target simulation and opening notification entry points; not a new complete gravity re-review |
| Combat/Enemies/Runtime/RoyalArchbishopEnemyAbility.cs | Full file: triage, rune scheduling, blessing grants, alternation and cleanup |
| Combat/CombatController.cs at PR #137 head `4b2a0dd2256568f0166f84bff5fcf9d8d59c78c7` | Lines 1-420: damage snapshot/actual recipient reporting, poison application, player utility effects and modifier handoff |

Paths in this table are relative to `Assets/_Game/Scripts/`. Except for the explicitly identified PR #137 read, these reads used the pinned main commit. A no-new-finding review is not a runtime PASS and does not supersede the existing audit fixes or their validation requirements.

## Remaining source/reference closure work

Keep these items explicit; do not mark them complete merely because a directory was listed or a validator exists.

### A. Remaining gameplay-data reference tables

- Resolve the complete `Resources/RunUpgrades/PrototypeRunUpgradeCatalog.asset` membership against card metadata, then inspect identity/eligibility/modifier/mechanic references for inconsistencies. The catalog directory was inventoried in this pass, but its complete mapping was NOT checked here.
- Finish definition-internal script/prefab/escort/reinforcement references beyond the targeted links already inspected in earlier chunks. The 21-entry database membership table above does not cover these nested links.
- Record required versus optional null references. Preserve approved testing overrides and report ambiguous design/balance values rather than silently changing them.

### B. Remaining scene/prefab wiring

- Reconcile gameplay-relevant components and local file-ID links in MainMenu/Game, the shared enemy prefab and the gem prefab actually referenced by the scene.
- Finish required Resources and script-GUID links, including input-module package references. A package-owned GUID absent from Assets alone is not evidence of a broken reference.
- Separate legacy/disabled or optional presentation fields from required runtime dependencies. Do not redesign layout or reserialize art during this audit.

### C. Remaining presentation-to-gameplay boundary coverage

- Reconcile the script inventory with the earlier system passes, especially presentation coroutines that gameplay yields to and impact/return callback forwarding.
- Focus on missing/interrupted presentation preventing authoritative completion, rather than opening unrelated cosmetic refactors.
- Record any genuinely unread gameplay-relevant source paths in the closure report. Do not represent every source line, binary asset or third-party package as reviewed without evidence.

These are remaining scopes, not a promise of exactly three more chunks. Do not start the combined Unity validation until the source-audit closure state is explicitly recorded and the user authorizes that next stage.

## Combined validation handoff

This checkpoint introduces **no new bug issue group**. Keep the existing gameplay validation groups 1-17; do not rerun them individually now.

Recorded dependency relationships from the prior audit work:

- `#142 -> #143`
- `#142 -> #146 -> #149`
- Integrate the other focused fixes with their recorded baselines and review the final combined diff; independent files do not imply absence of behavioral integration risks.

During the final shared validation, load the actual database and catalog through Unity, verify required references/runtime installations, and execute the accumulated automated and manual gameplay checks. Textually matching GUIDs alone do not replace those tests. No merge is authorized by this document.

**Bardley cost=1 remains the user's intentional testing override to accelerate waves. Keep it at 1.** Do not restore 80 or modify the documented non-testing default. The earlier Aegis Reservoir shield-cap observation remains a separate unchanged balance question.
