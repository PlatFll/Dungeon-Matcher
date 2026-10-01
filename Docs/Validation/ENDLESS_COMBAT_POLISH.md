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

## Approved ability direction; integration in progress

The user reviewed all eleven PixelLab studies in `ArtSource/EnemyAttacks/AbilityConcepts.html`. Royal Gold was selected for the King, with a required two-handed grip throughout the lift and both ground thrusts. The Shield Knight must raise his shield before it glows blue and applies shields. All other concepts were accepted. Revised King/Shield Knight art and actual ability integration are the remaining work; the initial gallery is a concept preview, not proof of Unity ability behavior.
