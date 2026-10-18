# Visual review record

Reviewed actual native assembly, integer 2× assembly, family boards and Unity 360×640 capture against the supplied image. Review page shows the evidence without hiding the reference.

| Family | Inspection and correction |
|---|---|
| Walls / portrait fill | Removed sampled chain/UI fragments; rebuilt continuous staggered courses and narrowed mortar. Blue portrait background remains distinct. Repetition is intentional and simpler than the source's unique bricks. |
| Platform / ledge | Preserved the source course profiles, anchors and shallow side view. Reduced tiny shading fragments; no added perspective field or large cracks. |
| Door / structure | Retained the barred opening, arch contour, column and lower-side silhouettes. Separate opening and crown/piers. |
| Torches / lighting | Separate flame fixtures and warmed masonry; controlled native clusters. No blurred alpha overlay. |
| Banners / chains / barrels / skulls | Reference silhouettes and relative locations retained; material ramps cleaned; props remain separately editable. |
| Player / board / top / lower frames | Matching corner and edge parts. Removed surrounding wall/cell residue from rails and corners, keeping dark contour pixels. |
| Board cells | Rebuilt quiet bevels with clean joins and no gems. 64 cells at the original board rectangle. |
| HUD / labels | Retained icon identities and shell rectangles. Cleaned fills; tiny footer text rebuilt with native glyphs. Labels are artwork, not functional/localized UI. |

The first assembly was rejected for duplicated prop fragments and noisy palette boundaries; its comparison is retained as `RejectedInitialAssembly.png`. Subsequent passes corrected those issues. The subsequent user review identified clipped banner tails, patched side masonry and narrow lower corner crops. The correction below supersedes that earlier assessment; user approval remains pending.


## Wall and clipping correction — 2026-10-18

Reviewed the supplied marked feedback, full native reference, before/after assembly, expanded corner source masks, individual banner/structure sheets and actual Unity 360×640 capture at native and integer zoom.

- The old banner masks removed the central tips. All five masks now retain their full pointed tails; the player crest is unchanged.
- Removed eight redundant or fragmentary wall/support modules. One globally aligned background brick field now continues behind the board and around its edges, with independently placed decorations.
- Replaced the two narrow rectangular corner crops with natural stone contours on 68×164 transparent canvases. Excluded sampled UI pixels and closed the bottom contours. The broader stepped masonry integrates with the continuous wall.
- Added two native pixels of transparent clearance to 24 standalone prop/corner modules. Props at the outer edge moved only enough to fit the complete canvas. Intentional arch/frame/ledge joins remain modular joins.
- Board cells and portrait backing now render above environment layers. All frame, icon and label anchors remain unchanged.

Evidence: `WallCorrection/CorrectionChecks.json` verifies 112 unchanged asset cores, all UI anchor instances, the player crest and complete point, padding, and golden production hashes. The seven art changes are the five banner masks and two corner masks. Retired editable sources and the exact previous assembly are preserved under `WallCorrection/`.

Validation passed: 119 LibreSprite sources match PNG exports and Unity copies; alpha is binary; all standalone placements fit. `Tools/Validate-Unity.ps1` passed, all three editor tests passed, and five actual Unity captures match the corrected assembly pixel for pixel, including tall portrait and safe-area cases. No additional generation spending. Production scenes and characters remain unchanged; the isolated preview is unmerged and awaits user review.
