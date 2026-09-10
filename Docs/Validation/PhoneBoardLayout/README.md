# Phone board size and bottom anchoring

The phone screenshot exposed two presentation policy problems in PR #116: flooring the board's source-texel ratio reduced a 1080px display to a 544px board, and centering the complete stack lifted BottomHUD away from the bottom.

The board now uses the largest uniform fit inside the width and remaining height, with logical-size rounding and a snapped physical center. At 1080x2400 without insets it is 1064px wide. BottomHUD starts at the bottom of the safe viewport, retaining the existing 4 logical pixel inset and any device navigation inset. Extra height stays above the stack. HUD integer scaling, 176px height and 6px section gaps are unchanged.

This intentionally supersedes the previous integer-only board rule and below-552px exception. Point-filtered board texels can have uneven physical pixel widths at fractional scales; the board remains uniformly scaled without stretching its aspect ratio. Gameplay, combat and ability rules are unchanged. Local Game.unity gemScale and ProjectSettings edits are excluded from this fix.

Validation uses the 32 rendered phone-size/safe-area cases in GameplayPixelLayoutTests, including three enemies, intentional validator violations, Retry and MainMenu reload. New assertions require bottom anchoring, maximum board fit and board containment. The containment assertion also covers floating-point ceiling overflow found at 1440px during the first run. Native screenshots and case results accompany this report. These are desktop GameView tests, not a physical Android build test.

Results: all 32 cases, deliberate violations, Retry and MainMenu-to-Game passed. Tools/Validate-Unity.ps1 succeeded with Unity 6000.3.19f1 after the final correction. Inspected the native 1080x2400 capture. No gameplay code changed; the broader gameplay suites were not rerun for this layout-only follow-up.
