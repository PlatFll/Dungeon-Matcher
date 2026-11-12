# Ability readiness and visible silver meter

Requested follow-up, 2026-11-12; pending visual approval. No merge.

The four separate user-edited icons remain byte-identical. While unavailable,
the existing white symbol mask overlays the icon. Ready reveals its unchanged
colors while the plate rises one native pixel left and three up over 160 ms.
Pause freezes the state; Reduced Motion preserves the color cue without travel.

The old SharedEnergy art had an opaque painted center drawn over the live fill.
Charge events and the mask updated, but the artwork concealed the changes. The
matching existing Stage03 silver casing is now used at its original 147x24 size,
with a transparent 117x8 window. Its bevels and silhouette are unchanged. An 8x8
empty track uses the approved #1C2438 inset and #171622 upper shadow; the existing
purple fill is retained. Both art files have native LibreSprite sources and
pixel-identical round trips. The rim, track and fill use one native pixel scale.
No energy generation, costs, refunds, activation rules or gameplay were changed.
In particular, Bardley's existing ability can refund charge; the preview shows
that real remaining amount after spending rather than forcing an empty bar.

## Evidence

- `Preview/ChargingReady.png`: four actual Game charging/ready comparisons.
- `Preview/AllFourMotion.gif`: actual 30 fps capture, empty → 25% → 50% → 75%
  → full/ready → actual accepted activation and resulting energy state.
- Eight full 720x1280 character screenshots and an ordinary-match screenshot.
- `QA/UnityTests.xml`: three passing automated Unity Game tests. They check
  rendered fill pixels, not just stored energy or mask widths; white/colored
  states, four native pop steps, accepted activation/spending, pause, Reduced
  Motion, and a real ordinary swap/match granting visible charge.
- Twenty additional captures verify the same live bar in all five zones at
  360x640, 720x1280, 1080x1920 and 1080x2400 (simulated safe area).
  Full raw capture history remains in `.utmp/AbilityReadiness/` locally.
- `QA/PreservedUserArtwork.json`: all existing Silver PNGs byte-identical.
- Native dimensions, exact palette, lineage and LibreSprite checks are in QA.

The first test run exposed a fixture assumption: Rattlebones remains active
after casting, so refilling energy alone cannot make it ready. Pause/Reduced
Motion assertions were moved before activation, preserving the real runtime
lifetime. Final tests pass; no gameplay or acceptance checks were weakened.

This is automated Editor Game-scene validation, not a physical-device test.
Zero PixelLab generations. Earlier sources and the user's unsaved/uncommitted
LibreSprite work are preserved. Rebuild the art with `build_meter.py` and the
installed Foundry Python/LibreSprite; the script does not alter icon pixels.
