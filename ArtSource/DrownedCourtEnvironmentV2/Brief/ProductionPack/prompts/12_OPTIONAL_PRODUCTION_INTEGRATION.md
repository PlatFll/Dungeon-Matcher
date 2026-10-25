# STAGE 12 — OPTIONAL — bind approved art into playable Drowned Court after user authorization

**Execute this stage ONLY.** Read `START_HERE.md`, `GLOBAL_LOCKS_AND_ART_DIRECTION.md`, `REFERENCE_MAP_AND_MATERIALS.md`, `PIXELLAB_TOOL_SELECTION.md`, `QA_AND_APPROVAL.md` and the current repository authorities first. Reference images are examples for art direction only, NOT native sprites. Work in the dedicated isolated worktree.

**DO NOT EXECUTE UNLESS USER EXPLICITLY APPROVES THE ASSEMBLED ART AND THIS PRODUCTION INTEGRATION STAGE.**

1. Inspect latest main and current Court gameplay scene/zone definitions/prefabs and native presentation owner; identify legacy environmental layers that would duplicate new art. Preserve all runtime actor, wave, board, gem and AIR/flood state logic.
2. Integrate ONLY the approved Court environment/frame/dressing assets using existing presentation ownership, versioned bindings, correct GUIDs and import settings. Reuse shared Wave/Guide/Settings and bottom HUD screens without re-rendering or recoloring.
3. Test actual gameplay on court zone and on return travel/continue: dry/flooded transitions, AIR/hazard overlays, bubble gem gameplay VFX and saved run state; art-only ambient must not change gameplay timing/AI. Capture characters, real gems and health bars in real `Game` scene.
4. Verify device layout matrix, native scaling, safe areas, Reduced Motion, settings/Guide buttons and real swap. Run existing Unity validation and relevant tests, Android build check if available; distinguish editor tests from physical device acceptance.
5. Produce side-by-side before/after gameplay screenshots and a focused draft PR. Leave user approval of merge to the user. Update canonical art guide/reference status/changelog only to record art actually approved, without changing unrelated global rules.

No merge. Stop for final playable review.

## Required stage handoff
Create/update candidate asset files, manifest entries, PixelLab usage ledger, a native 1× contact sheet, nearest-neighbor zoom inspection, reference comparison and actual Unity art-only scene screenshot when applicable. Distinguish objective PASS/FAIL from artistic review. Describe exact preserved files and any unresolved defects. Do NOT merge or proceed to the next stage without explicit user approval.
