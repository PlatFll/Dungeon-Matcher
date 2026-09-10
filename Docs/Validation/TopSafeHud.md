# Top-safe HUD layout

The previous bottom-packed stack left all spare height above TopHUD. TopHUD now ends at the upper safe viewport edge, BottomHUD starts at its lower edge, and the largest fitting square board is centered between them. Both gaps are at least six logical pixels and differ by at most one logical pixel for grid alignment. Battle art remains bounded instead of being stretched to absorb tall displays. Existing integer HUD scaling, native frames and the uniform board fit remain unchanged.

The safe viewport still comes from Screen.safeArea with inward grid rounding and four logical pixels of additional clearance. This uses actual notch/punch-hole/system-bar insets rather than an invented universal camera-hole height. The safe area is relative to the Unity player window, so a window already inset by the OS is not padded a second time by a guessed status-bar height.

Research consulted:

- [Android guidance for Unity games on different form factors](https://developer.android.com/games/engines/unity/unity-large-screen?hl=en): query Unity safeArea and adjust game UI, especially interactive elements.
- [Android display cutouts](https://developer.android.com/develop/ui/views/layout/display-cutout): cutout/system-bar behavior varies with OS and target SDK; use reported insets and test simulated cutouts.
- [Unity Screen.safeArea](https://docs.unity3d.com/ScriptReference/Screen-safeArea.html): coordinates are relative to the player window.

The geometry and rendered validators assert both edge anchors, minimum balanced gaps, containment, board fit, native HUD art and physical alignment. The existing 32-case matrix covers eight portrait sizes (540x960 through 1440x3200) with full, top-cutout, top/bottom and asymmetric safe areas. Retry, scene reload and intentional validator violations are included. Physical Android testing remains separate from desktop GameView validation.

Validation completed: all 32 rendered cases, intentional violations, Retry and MainMenu reload passed. Tools/Validate-Unity.ps1 passed with Unity 6000.3.19f1. The 1080x2400 native capture was visually inspected; screenshots and case summaries are in TopSafeHud/. Gameplay code was not changed and the broader gameplay suites were not rerun. Local scene/settings edits remain excluded.
