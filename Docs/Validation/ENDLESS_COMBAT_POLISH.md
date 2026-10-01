# Endless combat and enemy animation checkpoint

## Core changes verified on 2026-10-01

- Replaced the legacy full-size shield overlay with a separate six-pixel track below the modular HP frame. HP and the rank badge remain visible.
- A shield present at hit start absorbs that whole hit. Overflow is discarded; later independent hits can reach HP. Existing mitigation still applies.
- HP, shields, damage and healing resolve in multiples of five after modifiers. Positive values have a minimum of five; zero stays zero. Legacy continuation amounts are normalized on restore. Energy, currency, board durability and internal scaling coefficients keep their own units.
- Normal, Special and Miniboss badges use lime, cyan and amber. The Boss badge is retained.
- The King formation records its milestone and continues into the existing weighted roster. Cards continue every four waves from 32. Rewards settle once on death or explicit End Run/Retry.
- Sixteen missing basic attacks were generated with PixelLab, reviewed, finished in LibreSprite and connected to the existing controllers and actor events. The five prior authored attacks remain.

## Executed evidence

- `Tools/Test-CombatPolish.ps1 -Full`: **268 passed, zero failed/skipped**. Includes whole-hit gates, rounded stats through wave 1000, board/lifecycle regressions and an actual Game-scene King formation followed by wave 31, save/resume and death settlement without duplicate payment.
- `Tools/Review-EnemyAttacks.ps1`: **16 production attacks passed**. Every pose checked at 1080 x 1920 and 1080 x 2400. Actual damage matched the authored contact sprites; pause, separate follow-up hits, duplicate protection, idle return and death cancellation passed. Evidence: `.utmp/EnemyAttacksReview/` and `.utmp/enemy-attacks-tests.xml` in the implementation checkout.
- `Tools/Review-FinalizedVisuals.ps1`: **38 actual scene captures passed** across both portrait sizes, including shield lifecycle, all ranks, menus, typography and combat feedback. Evidence: `.utmp/FinalizedVisualReview/`.
- `Tools/Validate-Unity.ps1` passed in the combined main Unity checkout after installing 722 files with SHA-256 verification and preserving the existing Gideon edits. Unity 6000.3.19f1, log suffix `f4592e3e-e3a1-4cb6-9239-70c4255daa06`.
- Local installation backups and reviewed three-way merges: `.utmp/visual-targets-v2/.utmp/CombatPolishIntegration/`. No merge to main was authorized or performed.

The disruption cap fixture now uses durable walls so random refill cascades cannot destroy them before the cap assertion. It retains the exact cap, legal-response and ownership-cleanup checks. Older opt-in shield/audio/stagger assertions were updated for the new rules; those complete historical manual harnesses were not rerun. The focused current runtime checks above are the executed evidence.

These are Editor/Play Mode checks, not Android-device or human pacing tests. Nearest-five rounding intentionally creates upgrade plateaus and can increase small hits to five; the resulting pacing still needs human play review.

## Approved ability production

The user reviewed all eleven PixelLab studies in `ArtSource/EnemyAttacks/AbilityConcepts.html`. Royal Gold was selected for the King, with a two-handed grip throughout the lift and both ground thrusts. The revised Shield Knight raises his shield before the blue pulse grants shields. All other concepts were accepted.

Fourteen native animation states now cover the ten remaining ability users. The existing controllers, actor action identities and shared board queue own playback and effects. The King holds his raised sword during the warning, strikes the column and row on distinct contact frames, recovers, then allows one refill. Royal Standard gravity is deferred across the whole sequence too. Protected specials and structures retain their existing rules. Death suppresses later strikes and settles prior holes.

Sources and exact timing: `ArtSource/EnemyAttacks/abilities.json`, editable ASE files and `ABILITY_SOURCES.md`. `ApprovedAbilities.html` is a self-contained preview of the actual imported sprite exports. Four actual Unity captures are retained in `ArtSource/EnemyAttacks/Validation/`; the full pose/layout evidence is in `.utmp/EnemyAbilitiesReview/`.

### Ability verification

- Native audit: **14 sheets passed**, exact dimensions/frame exposures, binary transparency and source hashes recorded in the manifest.
- `Tools/Review-EnemyAbilities.ps1 -SkipImport`: **3 checks passed after the final Royal Standard gravity correction**. Covers production metadata, stale/duplicate/paused cues, actual casts, shields on the blue frame, commands, healing, blessings, hammer strikes, missing-art fallback, disable/death cleanup, King column/row contact and all poses at both portrait sizes. Thirty-two actual Unity captures record contact/hold poses and the live lane strikes.
- Basic-attack regression after the presenter extension: **16 attacks passed** through `Tools/Review-EnemyAttacks.ps1 -SkipImport`.
- The broader regression run exposed Royal Standard gravity advancing between the two strikes. The fix preserves its accumulated gravity openings until final settlement; the focused Royal production scenario then passed. The mine-cap fixture now waits for the first stable board and asserts request acceptance, retaining its exact cap assertions.
- A focused run accidentally included rendered layout checks with `-nographics`; those checks failed because Unity reported a zero-size viewport. The dedicated ability runner enables graphics and is used for final visual verification. No layout assertion was removed.
- PixelLab: **47 subscription generations used, 1953 remaining** after all attacks, concepts and revisions. No paid credit balance was used.

- Final full-suite rerun after the ability and Royal Standard gravity changes: **268 passed, zero failed/skipped** through `Tools/Test-CombatPolish.ps1 -Full`. Evidence: `.utmp/combat-polish-tests.xml` and `.utmp/combat-polish-tests.log`.

- Installed and SHA-256 verified **220 ability files** in the combined main Unity checkout. Reviewed three-way merges preserved the local Gideon architecture and checkpoint. Backups: `.utmp/visual-targets-v2/.utmp/AbilityIntegration/`.
- Final `Tools/Validate-Unity.ps1` **passed** in that combined checkout with Unity 6000.3.19f1. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-70366be2-d9ed-4927-a964-1d7d67c9ef66.log`.

No Android or physical-device test was performed. The implementation branch and local installation remain separate from a merge to main.
