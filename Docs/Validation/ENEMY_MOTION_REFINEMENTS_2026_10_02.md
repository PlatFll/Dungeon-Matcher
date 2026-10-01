# Enemy motion refinements — October 2, 2026

## Delivered behavior

- Spear Guard, Spear Knight and Royal Lancer use two-handed thrust poses. Their 192 × 80 canvases retain the native pixel scale; whole-pixel finishing aligns the boots, draws the weapon back and drives it forward. The 680 ms auto-attack sequence and existing follow-up-hit rules are preserved.
- Shield Knight attacks with a shield bash. His shield ability moves the knees, torso and shield arm, then applies shields on the cyan contact drawing and lowers the shield.
- King Bombardment commits from the accepted raise through warning, both contacts, extraction and board settlement. Its owner pauses the existing attack countdown and rejects immediate/commanded attacks. Other King actions wait. A restored warning reconstructs the pause; cleanup releases only its own hold.
- The King's new preparation ends in the approved ready pose. The second ground thrust now flows through extraction and recovery into the original idle drawing.
- Weakness gems reserve a shield lane below the HP frame. Granting or breaking shields does not move the gem; its materialization and death overshoot clear the track.

The original King bug was a presentation-only warning pose: it did not hold the combat countdown or block other King actions. The shield overlap came from positioning the weakness icon using only the HP frame gap.

## Art production

PixelLab was confirmed and its balance checked before production. This revision used 24 subscription generations (47 → 71 used; 1,929 remaining; $0 paid-credit balance). `ArtSource/EnemyAttacks/Refinements/jobs.json` and `pose-jobs.json` retain prompts and job IDs. Raw accepted PNGs have download/hash records; editable `.aseprite` files, production PNG/JSON/GIF exports and the native finishing script are included. First-pass swinging spear loops were rejected.

Seven changed clips passed checks for native canvas dimensions, binary alpha and distinct poses. Production selection and timing are recorded in `selected.json`, `abilities.json` and `manifest.json`. The local animation gallery is `ArtSource/EnemyAttacks/Refinements/Review.html`.

## Verification

- **279 regression tests passed**, zero failed/skipped: combat, damage/shields, balance, board resolution, continuation, Royal commands and Gideon. Evidence: `.utmp/motion-final-regression.xml` and `.log`.
- **Four rendered checks passed**, zero failed/skipped, at 1080 × 1920 and 1080 × 2400. One checks all 16 enemy auto-attacks, including their contact sprite, per-strike damage, pause, death cancellation, return to idle and every frame's scale/visible bounds. Three cover ability metadata/events and real casts, shield/gem clearance, missing-art fallback, both King contacts, no intermediate refill, deferred reinforcements and cancellation. Evidence: `.utmp/motion-final-visual-tests.xml` and `.log`; captures in `.utmp/EnemyAttacksReview` and `.utmp/EnemyAbilitiesReview`.
- The new continuation regression saves during the King's raised warning, reloads the production scene, checks the unchanged countdown after 1.7 seconds and checks owner-specific cleanup.
- **`Tools/Validate-Unity.ps1` passed** using Unity 6000.3.19f1. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-d30c8790-a8bc-4aee-978b-59bf7649f9b3.log`.
- Imported PNG bytes match the production exports. All revised attacks and the Shield Knight ability finish on their exact first idle drawing; the King finishes on the supplied original idle. Expanded canvases retain binary alpha and distinct native poses.

Desktop Editor and automated production-scene checks do not establish Android/device testing.

The animation fixtures retain a real color-crystal response so the game's counterplay guard can accept interference on random openings. Viewport checks inspect every nontransparent texel of expanded canvases while still checking native scale, fixed floor and center. Empty margins may extend outside the viewport; visible artwork may not.

## Equipment correction after visual review

The user approved the motion and identified generated equipment that does not belong to the characters. Royal Lancer and Spear Knight now have no back shields; Shield Knight's bash has no sword. Spear Knight's original narrow helmet slit and brow are copied at the head's existing position in every frame. Armor, hands, spear shafts and cape edges were inspected after removal.

PixelLab's exact pixel-edit workbench performed the repairs without regenerating the motion. This used zero additional generations. `ArtSource/EnemyAttacks/Refinements/EquipmentCleanup/` retains the original loops, both edit passes, tool IDs, source/output hashes and corrected frames. `Tools/Stage-EquipmentCorrections.ps1` converts the reviewed GIFs to RGBA in LibreSprite and recreates the editable sources and game exports.

All three clips retain 12 frames, their original per-frame exposures, 680 ms duration and exact idle endpoints. Every visible pixel in the production PNGs matches the edited PixelLab output. Native sources are RGBA; Unity PNGs match their source exports. Existing animation clips, importer settings and gameplay code were unchanged by this follow-up.

The fresh `PixelLabAttackPlayTests` rendered check passed (1 passed, zero failed/skipped), exercising all 16 attacks at both portrait sizes. Results: `.utmp/equipment-visual-tests.xml` and `.log`; refreshed captures: `.utmp/EnemyAttacksReview/`. The earlier gameplay regression and required Unity validator evidence above remains applicable to the unchanged code.
