# Drowned Court rising water

User request, 2026-11-08: generate and animate a smoother screen flood with gentle
surface waves and bubbles. Show the actual result before merging. This directory
was approved for actual-game integration and merge on 2026-11-09. The checksum repair is separately
merged in PR #224; no approved underwater artwork was changed by that repair.

PixelLab generated four 128x64 still candidates (10 generations); candidate 3
was animated into 16 frames plus its original input (2 generations). Job IDs,
exact reference hash and export identity are in `manifest.json`. Remaining
subscription allowance after these calls: 3745.90 generations. No other provider
was used. The actual unchanged Rattlebones image was the rendering reference;
the Court's teal family sets the effect palette.

`raw/` preserves all received animation frames. `build.py` removes the extra
ending frame, normalizes a quiet teal compositing ramp, eliminates generated
shading flicker, and joins each horizontal repeat with native edge collars.
Small 4px and 8px hollow bubble accents supplement the generated surface bubbles.
No resampling, artwork repaint, blur or global scene recolor occurs.

`WaterSurface.aseprite` is the editable 16-frame LibreSprite source. LibreSprite
batch-saved it and exported `WaterSurface-strip.png`; all exported native pixels
were independently compared with the cleaned frame PNGs. Each frame is 128x64,
and its first/last columns match exactly. Transparency values are deliberate
water-compositing levels (0, 64, 120, 164), not sprite-edge interpolation residue.
The atlas includes opaque utility pixels and native hollow bubble accents.

`CourtFloodGraphic` draws the surface without stretching its native pixels,
over a restrained depth tint, across the complete physical canvas. It renders
above world scenery/gems and below live HUD controls and actors. The 1.8-second
cosmetic rise/drain is independent of the existing .35-second board transition.
No AIR, combat, saves, RNG, character, gem, layout or approved environment files
are modified. The layer cannot intercept pointer input. Pause freezes it;
reduced motion uses a static tint. Missing optional art retains a quiet fill.

## Verification

- Unity compilation: `Tools/Validate-Unity.ps1`.
- Actual Game-scene test: `ForestFoundationPlayTests.CourtFloodPresentationPreviewAndContracts`.
- Runtime captures: 720x1280, 1080x1920, 1080x2400; tall-phone safe area.
- Checks: render geometry is submitted and not culled; full-screen coverage;
  point filtering/no mipmaps; no input interception; pause/reduced motion;
  unchanged move count, AIR and HP while the effect runs; complete drain.
- 210 actual Unity frames at 30fps show dry, rise, underwater hold and drain.
- Numeric checks are accompanied by visual inspection: the original first pass
  was invisible because a custom Graphic lacked CanvasRenderer. That was fixed,
  and the geometry regression assertion now detects it.
- No real-device test. User approved the actual Game preview for integration.

Generate the local review page from captured frames with
`python ArtSource/DrownedCourtWaterFill/publish_preview.py <output-folder>`.
The lossless animated WebP is the full-resolution review; the GIF is a compact
nearest-neighbor preview. `prepare.py` only downloads immutable generated source
images when absent; `build.py` performs the native cleanup and LibreSprite export.
