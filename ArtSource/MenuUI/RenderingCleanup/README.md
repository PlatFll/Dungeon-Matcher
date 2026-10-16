# Controlled main-menu background cleanup

Historical V1 evidence. The user judged this pass too subtle; the current
[V2 follow-up](../RenderingCleanupV2/README.md) preserves it as the baseline.
Use V2's native sources and install recipe for the current review candidate.

## Scope and result

The user requested a selective rendering cleanup of the **current implemented
menu**, preserving its identity, geometry and layout. This work starts from
merged main `a0aafe53e22f2efe7cbf0319b9ce4a0de1e8fe3b` on
`codex/menu-background-cleanup`. It does not include the separate battle-art PR.

LibreSprite applies 693 exact pixel replacements (0.24% of the 360×800 hall):

- Small disconnected chips inside stair faces, paving, and a few wall/arch
  surfaces join adjacent existing shading clusters.
- Scattered carpet speckles are reduced inside the carpet, away from its edges.
- The existing 38-color palette, top-left lighting and bright edge accents remain.

No major shapes, silhouettes, composition, lighting direction or placement
changed. The skull/portal, both statues, banners, torch flames, treasure/props,
pedestal and tall foreground continuation are pixel-identical in the recorded
locked regions. Deep masonry seams remain unchanged. Wood and metal already
read clearly at this scale and were left intact. The logo, Rattlebones, buttons,
icons, all layout code, Unity scenes and import metadata are unchanged.

This is deliberately a small cleanup, not a new rendering of the scene. No
generator, paid service, blur, resampling, palette reduction or new color was used.

## Review

- [Actual Unity before/after](review/UnityComparison.png)
- [Cleaned menu capture](after/runtime/1920-main-menu.png)
- [Tall cleaned menu](after/runtime/2400-main-menu.png)
- [Native hall beside Rattlebones, before/after](review/NativeComparison.png)
- [4× nearest-neighbor detail comparison](review/DetailComparison.png)
- [Exact edited-pixel map](review/EditMap.png)

The native comparison places both 360×640 hall crops and the actual 64×64 golden
sprite at 1× on a neutral background. It is visual evidence, not a substitute
for inspecting the individual PNGs. The separate logo and live controls are
visible in the Unity captures. Native dimensions remain 360×640 (`Hall`) and
360×800 (`HallTall`). The latter's top 640 rows exactly match `Hall`.
Rattlebones is live in these captures, so his idle animation frame differs;
his artwork and transform were not edited.

## Authority and provenance

The current canonical guide and golden register were read before editing.
The real Rattlebones golden PNG was inspected and hash-verified:
`Docs/ArtDirection/Audit2026-10/media/skeleton-still.png`, SHA-256
`705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c`.
`../references/MainMenuTarget.png` is the approved composition reference.
The current production hall is the edit target; no pixels were extracted from
that mockup during this pass. Existing reference approval/history is unchanged.

`before/` preserves the original PNGs and editable documents. `baseline.json`
records input and protected-file hashes. `cleanup-plan.json` records every exact
old/new RGB value and the explicit interior material regions. Only fragments of
at most 14 pixels were selected; original dark seams and material boundaries
are protected. The actual editor output was visually reviewed before installation.

## Reproduction and validation

Use Python with Pillow to run `PlanCleanup.py`; it selects local existing-color
replacements and writes a native editor script under ignored `logs/`.
Run `RunLibreSprite.ps1` to edit the copied original cels, save editable documents,
reopen, and export through LibreSprite. `Verify.py` independently checks every
changed pixel, unchanged regions, palette, alpha and protected files. Its
`--install` flag copies only the two hall exports and native sources into the
existing menu asset locations. `Prepare.py` is baseline capture only: **do not
rerun it after installation**. `Tools/Build-MenuArt.ps1` finishes by restoring
these finalized cleanup sources, so the older hall constructor cannot undo them.

Pixel checks are in `evidence/pixel-checks.json`; menu import/source checks and
muted Unity test results are also retained under `evidence/`. Capture scripts
use disposable test profiles. Runtime coverage and final results are recorded
there; physical-device review is separate. No C# or Unity serialization changed.

Final measured results: **54/54 native ASE/PNG/runtime pairs and import settings
passed; all 3 existing MenuUiReviewTests passed in Unity 6000.3.19f1 with audio
muted.** These cover 1080×1920, 1080×2400, 720×1280, safe areas, five playable
portraits, text fit, button hit areas, settings, pause and saved-run continuation.
The independent pixel audit confirms all 263 other protected files unchanged.
The broad `Validate-Unity.ps1` compile gate was not needed for this art-only pass;
the actual Unity menu suite ran successfully. The full 54-asset rebuild was not
rerun; its final preservation step copies the independently verified sources.

The five pre-existing local user files recorded in `baseline.json` remain
unchanged and excluded from this work. No merge is part of this request.
