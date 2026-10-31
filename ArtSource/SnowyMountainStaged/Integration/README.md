# Snowy Mountain / Frostwake production integration

The user approved the final mountain look, requested the same board cell background as the other zones, and authorized integration and merge of PR #218.

The actual `Game` scene now selects `SnowyMountainAssembly` for `frostwake-peaks`. This is a runtime zone binding, not an editor-only preview. The cell sprite is the very same `DungeonPixelLab.boardCell64` reference used by Dungeon and Magical Forest; the existing 64-pixel gems and board fit are unchanged.

51 source PNGs are imported byte-for-byte: the mountain vista and sky overscan, snowy floor/separator, muted cobble wall, modular portrait/board frames, connected cliff source pieces, mounted flags, lanterns, and approved snow/glint frames. No new generation, repaint, recolor, or replacement of approved source files. The old environment is bypassed only for Frostwake; historical sources remain available.

The existing presentation owner builds the modular frames and masked portrait backing. Its scenery follows the shared enemy floor and physical screen edges. Each side is one native connected cliff texture, assembled by the same deterministic seam logic as the approved LibreSprite source; resizing changes coverage without stretching source pixels. The five reviewed sizes compare exactly against the approved cliff PNGs. Decorative motion uses independent time and no gameplay RNG; Reduced Motion hides it.

## Executed verification

- Unity 6000.3.19f1 `Tools/Validate-Unity.ps1`: passed. Final compile log: `DungeonMatcher-UnityValidation-93392f99-b4f6-4d16-95b8-ec4e2490a18d.log` in the Windows temporary directory.
- Baseline capture: 1/1 test passed before implementation, 25 actual Game cases (five zones by five profiles).
- Integration/regression: 3/3 tests passed. 25 matching after-captures at 720×1280, 1080×1920, 1080×2400, inset-safe 1080×2400, and 360×640; additional Frostwake checks at 720×1600, 800×1800 and 1440×3200.
- Board/portrait rectangles, enemy feet, health bars, button placement and artwork compare exactly with the pre-integration runtime measurements. Other-zone cell references remain exact. Board sizes and floor lines match across zones.
- Native scale/Point filtering; all ten reviewed cliff textures pixel-exact; no outer gutter gaps or opaque cliff overlap of gems; portrait clipping; no enclosing top/bottom frame; motion loop, RNG independence and Reduced Motion checks passed.
- Actual accepted player swap settles and advances the move clock; Guide and Settings open/close. Existing ice-state save/cleanup test and Minotaur/Frostwake scenery/ice presentation test passed. These are automated Unity checks, not real-device testing.
- Android IL2CPP development APK build succeeded: zero errors, seven warnings. All 51 new artwork PNGs appear in the packed build report. `AndroidBuild.json` records the build outcome. No phone/device installation or manual device test is claimed.

`Before/` captures old production artwork; `After/` captures this production binding. `Review.html` shows the original, approved art review, and actual integrated game. `SourceVerification.json` records immutable artwork and byte-identical imports.

## Existing small-screen limits

360×640 retains the previously approved fractional board fit and shared HUD dimensions. Its existing Wave/Guide crowding and Unity pixel-camera diagnostic overlay in editor/development builds remain; this integration does not redesign the shared controls or camera policy. Normal phone captures at 720 and 1080 widths have no such overlap. Physical-device validation is still required.
