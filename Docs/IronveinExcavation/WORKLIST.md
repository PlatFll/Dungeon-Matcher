# Ironvein implementation worklist

All numerical tuning is provisional. Preserve existing zones and user edits.
Each implementation tranche gets focused tests, actual Unity validation and a
reviewed commit. Final output is a PR; no merge without new approval.

| Phase | Deliverable / dependency | State |
| --- | --- | --- |
| 01 | Current-source audit, branch, authority/tool/allowance record | Complete |
| 02 | Additive zone/theme stub; saved 1/2/3-hit stones, safe caps, 3-move aging | Complete; 37 affected tests and validator pass |
| 03 | Two charged perimeter drills; full-lane clears, dedupe and one settlement | Complete; 37 affected tests and validator pass; proof captured |
| 04 | Ore-Powered sequence token; Pickaxe, Gunner, Packbeetle death network | Complete; six focused tests and validator pass |
| 05 | Hauler, Stonewright, Bore, Surveyor, Sapper, Switcher, Smith/Rattled | Complete; 53 affected tests and validator evidence in VALIDATION |
| 06 | Machinist, independent turret, Sentinel extraction/slam | Complete; focused/compatibility checks and validator pass |
| 07 | Grand Delver cycle/Core; optional one-remount phase behind data flag | Complete; 74 affected tests and validator pass |
| 08 | Four native art pilots, powered pilot, then 14 identities and motion | Complete for review; 14 stills/108 clips; 45 current Ironvein checks and mandatory validator pass |
| 09 | Modular cave compositions, industrial HUD, mechanics/VFX/audio | Integrated for review; 49 affected checks pass; four actual screen layouts captured |
| 10 | Weighted encounters, teaching/relief, four-zone travel and continuation | Pending mechanics; enable only ready destinations |
| 11 | Regression, captures, native gallery, final docs and review PR | Pending all affected work |

## Explicit decisions

- Small drill: first stone, exactly one hit, always stop. Big drills: full lane.
- Ore tokens refresh and apply to one entire basic sequence, not each hit.
- Stone hit resets age and prevents same-action hardening; stage never downgrades
  merely because remaining durability fell.
- Sapper defusing removes the charge without structural damage; expiration is
  two durability plus one actor damage packet.
- Boss phase experiment is labelled and configurable; one persistent actor,
  one possible reserve suit, no rewards until final defeat.
- No forced visit duration, HP padding, player-power correction or wave cap.

## Evidence

Phase 01: pack manifest 85/85 verified, clean new branch atop current main,
zero open PRs, current native references inspected, PixelLab balance confirmed.
No gameplay changes, generation, Unity tests or runtime claims in the audit.
