# Magical Forest v2 — checkpoint one

Status: **direction study for review**, not a final asset set or a new Unity capture.
Branch: `feature/magical-forest-zone-rework-v2`, based on Forest PR #206 commit
`2087e49905a4db1db1dddbdcd995419115715ebe`. The old branch and all prior source art remain intact.

## Why the assembled Forest felt noisy

The board repeated several unrelated motifs: spiral medallions, connector studs,
side braces, lanterns, dangling rune tags, and mushrooms. Their separate silhouettes
competed with the gems. The board structure also appeared heavier than the portrait.
The large central background was pale and visually uniform, despite its distant layers.

## Proposed direction: an old woodland sanctuary

Let the fighters and gems carry the brightest contrasts. Keep large tree silhouettes,
a quiet flat standing strip, and a cool distant clearing. Two small magical lights sit
above the fighters, with warm lanterns confined to the upper tree supports. The board
uses solid timber beams, plain mitred joints, and two connected leaf/vine sweeps.
No mushrooms, lanterns, rune tags, spiral medals or extra braces around the board.

The direction study demonstrates that hierarchy with existing native art and native
pixel motif sketches. It does not establish final wood carving or leaf shapes.
The lower-right motif currently rotates the upper-left study for composition only;
the final asset pass must author its top-left light independently.

## Asset plan and reuse

| Area | Keep | Planned focused work | Review gate |
|---|---|---|---|
| Far forest | Existing ruin/waterfall silhouette and layered trees | Cooler value groups, localized magical light; avoid random particles | 2: top blockout |
| Ground | Existing flat Ground_TopA/B and Ground_FaceA/B, current baseline | Check tile phase and joins only; no added debris in standing area | 2 |
| Large trees | Tree_Left/Right and broad canopy arrangement | Inspect connected canopy masses; preserve open middle | 2 |
| Portrait | Inset mask, character/gem/HP placement, existing slim frame proportions | Simplify tiny clamps only if needed; use the same restrained wood/leaf motif | 3 |
| Board structure | Board rectangle, 64px cells, all gem art, existing modular fitter | Replace spiral corners with quiet joints; reuse rail dimensions and simplify material clusters; remove extra posts/feet/braces | 4 |
| Board foliage | Separate overlay architecture | Two broad connected sweeps, complete padded canvases, no opaque pixels inside grid; independent leaves may later move | 4 |
| Upper lights | Existing hanging lantern silhouette/material family | Retain two attached to branches; no board lamps; small far light accent only | 2 |
| Old optional props | Preserve source files | Exclude mushrooms and loose rune plaques from this composition | 5 |
| Shared UI | Wave/Guide/Settings, HP, progress, potion/bomb, resonance and caption | **No redesign or recolor** | all |
| Characters/gems | Approved Rattlebones and Forest cast, current 64px gems | **No changes** | all |

## Tool use

PixelLab: **zero new calls / zero generations** at this checkpoint. The retained Forest
modules already have PixelLab lineage in `ArtSource/ForestHybrid`. Selective generation
is deferred until the direction is accepted and a specific asset needs it; no full-scene generation.

LibreSprite: executed the exact native-pixel direction plan, authored broader connected
leaf studies and plain timber-joint study, exported PNG and editable ASEPRITE files,
then reopened all four documents. Existing imports were not edited. Python prepares
deterministic pixel plans, composes the review study, and performs independent byte checks.

## Evidence and limits

`Previews/DirectionStudy.png` is a **720×1360 composition study**, based on the previous
Unity capture. It is not a fresh Unity or gameplay screenshot and must never be used
as a runtime background. Environment sketches display at exact 2×; all board/gem pixels
stay at the previous capture's exact size. `Native/` retains editable study documents.

Verified: exact LibreSprite plan/export/reopen agreement; binary alpha; zero foliage
pixels in the 512×512 board; unchanged portrait, shared controls, actor/HP/countdown
pixels, and the full lower HUD region. All 8,125 protected files match their baseline.
No Unity code/import/serialization changes, Unity test run, or device test occurred.
See `Diagnostics/Validation.json` for measured results.

Artistic verdict: the removal of competing props and consolidation into two plant
silhouettes is visibly calmer. Final material polish, independent leaf lighting,
modular joints, responsive Unity behavior and full-screen safe-area coverage remain
future gates. No numerical style-match percentage is asserted.

## Stop and next checkpoint

Stopped at the attachment's first review gate: **art-direction board and asset plan**.
After the user approves this direction, proceed only to **checkpoint 2: top background
blockout**. Portrait, board, integrated Unity scene, and final validation each have
separate subsequent review stops. Do not integrate into Game or merge yet.
