# Runtime verification — 2026-11-12

`Tools/Test-ReferencePresentation.ps1 -Filter AbilityReadinessTests`
completed successfully in Unity 6000.3.19f1: **3 passed, 0 failed, 0 skipped**.
The unmodified tests use the actual Game scene and disposable profiles.

- All four characters: white unavailable icons, colored ready icons, stepped
  pop-out, accepted activation/spending, pause and Reduced Motion.
- Rendered energy pixels at 0%, 25%, 50%, 75%, 100% and after spending;
  an ordinary board swap/match also generates visible charge.
- All five zones at 360x640, 720x1280, 1080x1920 and 1080x2400 with simulated
  safe insets. Existing layout and native-pixel validation remains strict.
- Exact edited PNG hash, unchanged importer/GUID and 18 other shared PNG hashes
  verified again after the Unity run.

[Live rendered fill states](LiveFillStates.png) · [Actual Game scene](Game_Dungeon.png).
These are fresh screenshots using the saved user-edited bar, without compositing
replacement UI. The state strip crops five actual captured gameplay frames.

Raw result: `.utmp/ThreeZonePresentation/reference-tests-20261112-042743.xml`;
the complete result is copied as `UnityTests.xml` here. The matching `.log`
contains the Editor execution log. All twenty layout captures and four-character
motion frames are retained locally in `.utmp/AbilityReadiness/`.

No C# or scene serialization changed in this asset-only follow-up, so the separate
compilation-only validator was not rerun; Unity compiled/imported and executed
the three tests. No Android build or physical-device test was performed.

Five editing copies were visibly verified open in LibreSprite: wave 264x99,
Settings 84x84, holder 105x105, and potion/bomb icons each 72x72. Original sources
and existing editor documents were preserved. Copies use hard binary alpha and
introduce no visible colors. No generations were spent. No merge performed.
