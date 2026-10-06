# Shark-tail revision

Authorized scope: Hammerhead Bruiser and Breakwater Captain only. Direct native
pixels, no generation. Revised tails await user review; original character approval
does not automatically approve this addition.

- 3 still canvases (including Captain wide) and 13 motion sheets / 118 frames.
- Every original opaque body pixel is unchanged, including faces and equipment.
- No new palette colors, intermediate alpha, resampling, canvas or floor changes.
- Tail anchors follow each pose; contact/ready frames keep their original timing.
- Added pixels remain connected to the body. All 13 frame sheets were inspected.
- Needlefin also lacks a clear shark-tail silhouette. Reported only, unchanged.
- Existing isolated hit/weapon spark pixels were preserved; this is a tail edit.

`Before` contains immutable input PNGs. `ApplyTailRevision.py` edits from those
inputs and copies current PNGs into the existing Unity art paths. `BuildReview.py`
updates selected manifests and the offline review. Run these in that order after
any legacy production preparation script; the older scripts alone omit this pass.
Do not run the full theme importer: it can overwrite authored scenery changes.

`TechnicalChecks.json` records exact file dimensions, palette counts, alpha and
before/after hashes. `Review/Review.html` includes animated motion, full native
sheets, before/after stills and the cast at a shared native pixel scale.
The earlier `Production/Review` package is a historical pre-tail submission.
