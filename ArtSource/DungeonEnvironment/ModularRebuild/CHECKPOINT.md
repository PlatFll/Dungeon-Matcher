# Dungeon modular rebuild checkpoint

Branch: `codex/dungeon-modular-scene-rebuild`, isolated from main `b82bf80a`.
Recoverable art commits: `33e207b` (brief/floor) and `a44619e` (native kit).
The preview implementation and final evidence follow in the branch history;
`Evidence/ReviewProvenance.json` fingerprints the exact tested code, scene,
modules and captures independently of commit-message/report updates.

Completed: 129 native LibreSprite modules and exports; per-asset source/1×/3×
comparisons; seamless floor/wall families and rails; isolated Unity preview;
four actual portrait/safe-area captures; clipping/native scale/label/module-use
checks; no gameplay owners, saves or release-scene mutations in the tests.

No art generation services used. Existing approved production assets and the
user's main-worktree changes are preserved. Read VALIDATION_REPORT.md and the
final compiler receipt for verification details and the honest review boundary.

Next action: user visual review in Review/Review.html or the dedicated Unity
scene. Do not integrate or merge without new explicit approval. No background
authoring or Unity validation work remains after the final handoff.

PixelLab-enabled follow-up (2026-10-18): completed on the same branch. Existing
129-module artwork and Unity assembly retained; added full library catalog,
category tool decisions, explicit zero-generation ledger and fresh byte/seam
verification. Earlier Unity evidence is reused only after matching exact hashes;
no Unity rerun or new art generation. PR #200 remains the review destination.
Next action remains user visual review; do not merge.
