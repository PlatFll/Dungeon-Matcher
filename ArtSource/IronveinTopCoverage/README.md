# Ironvein upper-screen coverage

Review candidate, 2026-10-31. Not merged.

The original 240×160 cave panel followed the enemy floor vertically but kept
its fixed dimensions. Tall physical screens exposed repeated CaveWall above
the panel and made the vista look like a small rectangular overlay.

`DungeonPresentationArt.Ironvein.cs` now covers the physical screen above that
same lower edge with the original image. Scaling is uniform and integer in
physical pixels, Point sampled, centered horizontally and bottom anchored.
Overflow is cropped at the physical screen edges. This intentionally changes
the vista's display size and framing, not its artwork. Foreground timber,
walking floor, separator, portrait, board, characters and shared UI retain their
prior geometry. No scene/prefab/resource replacement is required.

No PNG, palette, texture import setting or golden reference was modified.
No PixelLab or image-generation credits were spent. A native extension study
was rejected before import; the delivered fix uses no new artwork.

## Evidence

`Preview/Before` and `Preview/After` contain actual Game-scene screenshots and
serialized geometry snapshots. `Preview/Review.html` compares the captures.
`QA/SourceHashes.json` records the exact original and golden-reference lineage.
Both explicit before/after Game-scene capture tests passed, including 12 scene/
viewport cases (eight Ironvein profiles and one in each of the other four zones).
The snapshots match exactly for all UI Images, feet, board cells and existing
scenery other than the intentionally reframed cavern. Coverage assertions check
physical-screen edges, uniform integer scale, pixel-aligned origin, Point filtering,
original sprite identity and overlap behind the unchanged floor. Guide and
Settings open/close checks passed. Repository Unity compilation passed.

Ironvein profiles: 720×1280, 1080×1920, 1080×2340, 1080×2400, 1080×2400 with
inset safe area, 800×1800, 1440×3200 and 360×640. The other four zones were
checked at 1080×2340. Exact results/log locations are in `QA/Validation.json`.
Native source/reference hashes remain exact; `QA/ArtifactChecks.json` records
all snapshot and screenshot dimensions. No new Android build was run for this fix.

The current 360×640 shared-control crowding is outside this focused scenery
fix. Screenshots are from the Unity editor, not a physical Android device.
