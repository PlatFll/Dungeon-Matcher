# Combat-action validation — 2026-09-20

Scope: the user's exact adjusted Farmer idle; Farmer/Pan auto-attacks; Bardley/Rattlebones player casts; shared grounding and the associated existing-controller integration.

## Art and native exports

LibreSprite produced the editable sources, PNG sheets, GIFs and JSON timings. `ArtSource/CombatActions/Scripts/validate_actions.py` checks all 36 action frames; the idle validator checks all 36 current idle frames. The checks cover approved character palettes, binary transparency, fixed canvases, complete side margins, bottom-row contact, native/export equivalence and exact durations. Farmer's user-edited idle pixels/timing are unchanged. Bardley's idle is an exact 12-pixel downward translation, without pixel loss. Current reports and hashes are beside the sources.

Visual review includes contact sheets, native and enlarged playback, rigid Farmer hands/tool motion, completed iron-pan silhouette, skull/bone versus gold separation, Bardley's face/pipe acting and idle/action ready-pose continuity. A native background-layer flag was corrected before the final transparent Bardley GIF export.

## Unity checks

`CombatActionValidation.ImportAndRun` / `Run` use the production Game scene, disposable account profiles, both selected players and the real Farmer/Pan actors. They exercise:

- The actual accepted enemy action, exact fifth-pose damage, no start-pose damage, duplicate-event rejection, complete recovery and automatic idle return.
- A pause longer than the fallback deadline, cancellation/restart without inheriting an old impact, and missing-animation fallback without leaked ownership.
- Actual accepted player ability activation through `PlayerAbilityController`, one cast cue, multiple rendered cast poses and automatic idle return. Ability gameplay remains in its existing runtime.
- Every action pose at 1080×1920 and 1080×2400: stable ground/center across idle/action, unchanged source-texel scale, integer physical pixels, fixed rectangles/pivots and portrait containment.
- The existing 14 enemy lifecycle scenarios, including disable/re-enable, command reservations, stop, death and revive.
- All 15 existing ability lifecycle test methods run with real MonoBehaviour callbacks in Play Mode, including the new accepted/rejected presentation-cue assertions.

`CombatIdleValidation.Run` separately checks live idle playback, all nine idle poses for both players and villagers, fixed rectangles, ground contact, the complete player HUD stack and integer pixels at the same portrait sizes.

`PlayerAbilityLifecycleTests` covers accepted/rejected casts, energy and coordinator lifecycle, including one presentation cue on acceptance and none on rejection. `Tools/Validate-Unity.ps1` is the required compilation validation.

The runtime test found that Unity emits animation events before applying the current Image sprite curve. Authored impacts now resolve in the same frame's LateUpdate, carrying the accepted presentation ID, so damage observes the visible contact pose. The ability presenter also installs when Game is loaded after the menu. The importer explicitly adds timing flags to legacy enemy assets where those fields were absent.

## Evidence and limits

Local test logs, reports and production screenshots live under `.utmp/CombatActions/` and `.utmp/CombatIdles/`. Test scenes use temporary health on visual fixtures during real ability effects so the subjects survive for pose review; production enemy stats are unchanged. Desktop input is disabled in the action test scene to prevent unrelated mouse clicks from opening settings while it captures poses.

This is Unity Editor evidence, not physical Android device testing. The pre-existing fractional AbilityIcon layout warning is tracked separately from character placement in the idle validator; source palette checks do not imply that every transient gameplay flash uses the base palette. Runtime damage flashes remain intentional effects.

## Recorded results

- Required compilation validation: **PASS**, Tools/Validate-Unity.ps1 with Unity 6000.3.19f1. Log: C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-2c079753-9114-4b57-96de-26d8cc00b6a4.log.

- Action production-scene run: **PASS, 8,372 checks**, including 14 enemy lifecycle scenarios and all 15 ability lifecycle cases in Play Mode. Every action pose was checked at 1080×1920 and 1080×2400.
- Idle production-scene run: **PASS, 4,162 checks**, all four idle loops at both portrait sizes.
- Art validation: **PASS**, all 36 action frames and all 36 idle frames, including exact native/PNG/GIF pixels and timings.
- The separate NUnit Edit Mode invocation returned 11 passed / 4 failed. Its four failures require ordinary OnEnable/OnDisable callbacks that Edit Mode does not supply. The same 15 fixture methods subsequently passed in the graphics-enabled Play Mode run above; the Edit Mode report is retained as a test-runner limitation, not reported as a pass.

Action evidence: .utmp/CombatActions/play-validation.txt, asset-validation.txt, player-ability-tests.xml, and the native Game View impact screenshots. Idle evidence: .utmp/CombatIdles/play-validation.txt. Full action log: .utmp/CombatActions-play.log. Final scene captures were reviewed with optional enemy flash feedback disabled after the actual combat tests, so they show base art instead of a paused intentional white damage flash.
