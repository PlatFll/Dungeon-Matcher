# Ironvein — six working row drills

**Current timing revision:** [ContactTiming/README.md](ContactTiming/README.md) and [updated preview](ContactTiming/Review.html). The capture and validation below document the retained original six-drill revision.

Review candidate on `codex/ironvein-below-tophud-pixellab`, draft PR #215. Not merged.

## Result

- Six independently charged launchers, three per side. New runs use rows 1, 4 and 7 on the left and rows 2, 5 and 8 on the right, counted from the top. Muzzles follow the actual row centers, including Rail Switcher lane changes.
- Four charges launch a spinning bit inward. The board owns each 75 ms travel segment and canonical gem-impact hold. The socket stays empty through the final 180 ms burst; the replacement then emerges over 160 ms.
- Simultaneously ready machines fire in stable ID order. A shared visited-cell set prevents repeated clears on overlapping rows. One final canonical refill/cascade follows the full batch.
- Existing stone penetration, no-reward/no-energy environmental clearing, manual intake limits, boss response windows and continuation rules remain intact. Saved two-drill networks migrate to six without losing stored fuel or replaying intake.
- Existing Stage04 mounts/motors/power blocks and approved environment pixels are reused. Board size, frames, floor, separator, actors and HUD geometry were not changed.

## Art provenance

PixelLab generated the detachable still and spinning motion using the approved drill nose as identity and the current Rattlebones golden image as rendering reference. One still job cost 10 generations; two animation jobs cost 1 each. Total **12 generations**, balance **166.2 → 154.2**. Job IDs, prompts and reference SHA-256 hashes are in `Lineage.json`. Rejected/uncleaned source frames are retained.

Eight distinct 40×32 frames, 12 opaque colors, alpha only 0/255. Every frame has the same occupied bounds [6,5,35,27], with transparent margins. Native cleanup locks the collar, tip and silhouette while retaining the generated rotating interior ridges. `Native/RowDrillSpin.ase` was reopened/exported by LibreSprite; its strip matches all cleaned RGBA frame pixels. `NativeCleanup.py`, `ArtQA.json` and `ImportQA.txt` preserve reproduction and checks. Unity uses point filtering, no compression or mipmaps, FullRect sprites. Runtime spin is 24 fps; source-editor loop is 60 ms per frame.

## Review

Open `Review.html` for the actual Unity capture, pause/scrub controls, native sprite preview, alignment comparison and separate production Game screenshots. `Preview/DrillSequence.gif` is the compact animation preview. The 115 recorded frames use the real production board, gems, clears, launch/socket/burst/reload lifecycle against the existing approved Revision04 modular art assembly. Its actors and HUD are the unchanged static art-review snapshot, not live combat actors. The separate `Preview/Game-*.png` screenshots show actual production Game with the new drill system and its still-existing older Ironvein environment. Full replacement of that environment is a later integration step, not silently included here.

## Verification

`Tools/Validate-Unity.ps1` completed successfully with Unity 6000.3.19f1. Latest results for **23 distinct checks passed**: 6 foundation checks, 16 play-mode behavior/layout checks and 1 explicit Unity motion-capture fixture. Coverage includes six-machine firing, overlapping lanes, Obsidian penetration, manual intake deduplication, saves, pause, no-view gameplay fallback, boss interruption windows, Packbeetle/Hauler/Bore/Switcher interactions and row alignment without clipping at 360×640, 720×1280 and 1080×2400. Exact test names/run IDs and the validator log are in `Verification.json`; full local runs remain under `.utmp/ForestValidation/`.

The affected Bore regression was fixed in implementation by retaining its existing feed IDs. Enemy-feed fixtures were isolated from their top-row manual match so they continue asserting the exact single-charge result. All three affected tests were rerun and passed. No failed result is represented as passing without a successful rerun.

No Android build or physical-device test was performed for this revision. User visual approval remains pending. Do not merge yet.
