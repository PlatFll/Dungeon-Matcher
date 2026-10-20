# Portrait pipe correction — ready for review

The previous fix aligned canvas centers, but the visible pipe still lay one pixel off-axis. The user's green example is the original Stage10 left rail: its five-pixel outlined pipe is centered at pixel 3 within the seven-pixel connector.

LibreSprite now uses that exact pipe profile on all four edges. The unchanged 7×7 square extends equally by one pixel on either side. Vertical rail canvases remain 7×20, horizontal rails 24×7; corners, connector, colors, HP shell and heart are unchanged. The 99×190 outer footprint stays at (9,20). The interior mask expands one pixel from inset 10 to 9 (81×172 at 18,29), with the existing overfilled tiled backing, so the narrower visible pipe leaves no empty strip.

The shared frame fitter already supplies responsive corners, repeating rails and matching connector rows. This correction changes native rail pixels and the preview mask, not production runtime code or gameplay bindings.

## Verification

- Four editable ASE sources decode exactly to their PNG exports and Unity copies. Binary transparency; four unchanged opaque pipe colors across five pixels. Every pipe cross-section matches the green-reference pixels.
- Both affected portrait Unity regressions passed: 2 tests, 0 failures. Actual Unity render/resize probes cover 99×190, 100×191, 119×231 and 60×90 at 1× and 3×, with undersize recovery. Mask coverage is checked with decorative corner occluders hidden; normal complete-frame captures verify the assembly. The initial occlusion-related probe failure is retained in diagnostics.
- Native enlargement equals the complete 3× render exactly. All 64 board cells are unchanged. Every rendered pixel outside the portrait footprint remains identical.
- 298 protected art files and all current golden reference files retain their hashes.
- Mandatory Unity validator and test receipts are in Diagnostics/PortraitPipeCorrection. Production Game scene/build settings and main-checkout user files remain untouched.

The current visual verdict is centered metal pipes through larger square fittings on every edge, preserving the original green example. User review remains required. The old Stage11 portrait verdict is superseded; its gallery and captures are retained as history.

Stage12 preflight has started. Its dedicated PixelLab source is quoted at five additional generations; no Stage12 paid call has been made while the budget-extension question awaits a reply. No production import or merge.
