# Approved Forest / Dungeon production binding

The previous Forest merge contained approved art and editor review scenes, but
the playable `Game` scene still selected the older Forest presentation. Its new
ambient motion also lived behind `UNITY_EDITOR`. This explains why a phone build
could remain on the old look despite the assets being present on main.

## Production changes

- `ForestAssembly.asset` references the final approved separate modules, frames,
  and motion drawings from `ArtPreview_ForestSurround_Fireflies`. Sample actors,
  gems, labels and controls are excluded from the export.
- `ForestPixelLab.asset` uses the existing native presentation architecture and
  approved shared control family. The live board, portrait, floor, enemies,
  health bars and interaction owners continue to determine layout and behavior.
- `ForestAmbientMotion` runs all 27 approved tracks in player builds, including
  the 24-pose waterfall, varied small spirits, foliage gusts and eight lower warm
  fireflies. The rejected long comet and right-corner sprig remain absent.
- `Game.unity` no longer binds `Dungeon_Finalized`. Unused legacy tilemap scenery
  is not instantiated for the modern Dungeon/Forest presentations. Historical
  source assets and review scenes remain preserved, outside the build scene list.
- Shared enemy-slot geometry now refreshes after viewport changes in every zone.
  The compact Forest caption fits below its buttons. Lower formations remain
  complete, and the Forest canopy extends into tall-display/cutout space.

## Executed verification

- `Tools/Validate-Unity.ps1` passed with Unity 6000.3.19f1.
  Log: `DungeonMatcher-UnityValidation-b23709df-3bda-44d5-ad14-eb91a4abedb3.log`
  in the user's Windows Temp directory.
- `ForestProductionIntegrationTests.PhoneMatrix` passed: all five zones at
  720×1280, 1080×1920, 1080×2400, 1080×2400 with safe insets, and 360×640.
  All 25 cases have zero layout errors. Board and portrait dimensions match
  between zones at each size. Actual Game-scene screenshots include Rattlebones,
  three live enemies, gems and health bars.
- The same run verifies an accepted gem swap and settled continuation state,
  Guide/Settings interaction, native Forest image scale, masked portrait,
  absence of old Forest layers, the 27 runtime tracks, changing water poses,
  stable Reduced Motion, and the existing Dungeon native-art checks.
- A subsequent `Quick` run with additional assertions verifies that water
  advances automatically across real play-mode updates, the environment is not
  rebuilt each frame, and Settings pauses/resumes ambient motion. Its result is
  `AutomaticMotionPassed.xml`; production runtime code is unchanged from the
  25-case matrix and Android build.
- The first matrix exposed a pre-existing resize guard limited to Dungeon;
  after fixing that shared refresh, the complete matrix passed. Earlier failing
  diagnostics remain local history and are not presented as successful evidence.
- PNG source bytes are unchanged. `PreservedSources.json` records hashes and
  provenance; `Pieces.tsv` maps the exported placements. No PixelLab generations
  or other image-generation credits were used.
- Android IL2CPP development APK build succeeded: zero errors, 25 warnings,
  282 seconds. `AndroidBuild.json` records the result. The installable APK is
  85,818,519 bytes (81.8 MiB); Unity's total build-data size is recorded separately.
  No physical Android device has been tested by the agent.

Native environment sprites use integer physical pixel scale. The existing
64×64 gem/cell artwork and user-approved responsive **uniform fractional board
fit** remain unchanged; integer environment rendering is not a claim that the
512-pixel board fits every narrow phone at 1:1.

## Review / build

Open [the actual gameplay gallery](Review.html). Measurements and screenshots
are in `PhoneMatrix/`; `PhoneMatrixPassed.xml` is the complete passing test result.
The three enabled production scenes remain `Startup`, `MainMenu`, and `Game`.
Art-preview scenes are not added to Android builds. Use the normal zone picker
to enter Magical Forest or Dungeon when testing on the phone.

The local debug APK is emitted to
`.utmp/ForestProduction/DungeonMatcher-ApprovedZones.apk` in the integration
checkout. The build retains the existing Android backend and architectures,
using debug signing without reading or changing signing credentials.
