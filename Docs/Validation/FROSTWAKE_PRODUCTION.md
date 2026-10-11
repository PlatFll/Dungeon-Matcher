# Frostwake production validation — 11 October 2026

Windows desktop, Unity **6000.3.19f1**, actual MainMenu/Game scenes, disposable
account profiles, Editor audio muted. Scope: eleven mountain enemies, approved
motion, Minotaur eye correction, mountain gameplay theme and fifth-zone travel.

## Asset evidence

`Tools/Check-FrostwakeAssets.py` passed: 70 imported sheets are byte-identical
to selected exports; all 501 exposures use 128x104 cells, binary alpha, their
approved target palette, Point filtering and a four-pixel floor pivot.
`ArtSource/Frostwake/production/import-verification.json` records their hashes.

The Minotaur's PixelLab exact correction changes 1,372 eye-region pixels across
33 source cels. Alpha and all pixels outside the approved eye regions are
unchanged. Six existing sheets and the portrait use those verified exports.
The source and receipt are in `ArtSource/FrostwakePlayables/eye-revision/`.
Rattlebones, Farmer and the unrelated four-file working changes match the
recorded source hashes. Kobold artwork is unchanged.

Eleven identities were approved for animation. This is distinct from human
approval of finished motion, scenery and audio. Review
`ArtSource/Frostwake/MotionReview.html` for all selected states and native poses.

## Automated integration evidence

Full logs and NUnit XML are under `.utmp/ForestValidation/`.

- `0555b875-a6e2-420c-98ab-c389fd2669f8.xml`: 20/20 passed before final
  presentation refinements. Covers all eleven basics and imported controllers,
  supports, ice lifecycle, channel counterplay, continuation, 100 seeded weighted
  visits, menu zone selection and seven actual handoffs across all five zones.
- `fbf4b3b1-8a0d-4df9-8268-ecfdb51c7874.xml`: 23/24 passed after layout fixes.
  Includes the four existing zones' status/action HUD, leader announcement,
  all 73 definition intervals and three Frostwake/Minotaur portrait sizes.
  The remaining deadline fixture triggered the production emergency-reshuffle
  cleanup on an artificial no-follow-up board. Its correction preserves a real
  legal crystal response; target selection is fixed away from controlled clears.
  Counterplay and emergency cleanup remain enabled in production.

- `a2a7a6a8-44db-45b5-9dd4-357e901b6bc0.xml`: final affected rerun **2/2 passed**:
  production Haldra holds all three real responses, consumes one identity,
  releases its action lock and does not repeat damage; all three portrait
  layouts pass with the final stone floor. Together the latest affected runs
  establish **24 unique passing checks**, without adding overlapping reruns.
- `Tools/Validate-Unity.ps1` **passed / exit 0** on the final runtime/editor
  source. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-3075f5f1-1512-49e3-8074-ed18d19240e9.log`.

## Presentation findings and corrections

- Native frame pieces now match the existing 80x80 corner / 64x16 edge /
  144x32 energy / 128x32 wave contracts. No stretched source borders.
- Player frame center is transparent, with dark timber behind the character.
- The selected 512x384 mountain scenes cover tall views; repeated floor cells
  extend the actual stone ledge beneath the shared enemy footline.
- Larger animation canvases preserve the authored floor when sliced, and
  action counters stay inside the rightmost slot.
- Ability icons refit to whole source pixels after viewport resizing.

Gameplay captures cover 1080x1920, 1080x2400 and 720x1280. Optional presentation
does not own damage, clear counts, ice deadlines or channel completion.
Final inspected captures are in [Frostwake/](Frostwake/):
[mountain pass](Frostwake/scene-1-1080x1920.png),
[wandering hall](Frostwake/scene-2-1080x2400.png),
[glacier gate on a short portrait](Frostwake/scene-3-720x1280.png).

## Limits

This is targeted automated Editor validation and visual inspection, not a
physical-device certification or human pacing study. The 91-second original
temporary music cue still needs listening review. Enemy values, visit duration
and encounter pressure are first-pass tuning. No dynamic player-power scaling,
forced waits, new HP-padding mechanism, menu redesign or other-zone art rebuild
was introduced. Existing board, centralized damage and shield-gating owners
remain authoritative.

PixelLab used **147.5 subscription generations**, including rejected attempts
and revisions; exact edits were free, and no credits were purchased.
