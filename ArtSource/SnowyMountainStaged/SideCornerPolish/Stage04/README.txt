SNOWY MOUNTAIN SIDE/CORNER POLISH — STAGE 4 UNITY ASSEMBLY
2026-10-31 — review checkpoint; no production integration or merge.

RESULT
The approved Stage 2 cliff/timber side modules and Stage 3 corner kits are assembled in a disposable instance of the actual Game scene. The left has broad snowy rock and buried fencing; the right has exposed bracing and a taller supported cargo ledge. Narrow overlapping cliff connectors bridge the board footings to the larger lower masses. The existing lanterns and banners remain attached to the supports.

Preserved: exact source PNG/ASE artwork, upper scene, portrait, characters, actor floor roots, gems, 8×8 opening, frame geometry, controls, labels and gameplay. No regeneration, recoloring or resampling. Physical environment pixel scales are 2× at 720, 3× at 1080, and 1× at 360. Only scenery placement changes. Glints are repositioned onto the new footing/ice; the upper atmosphere is unchanged.

REVIEW
Open Review.html for paired actual Unity screenshots at 720×1280, 1080×1920, 1080×2400, inset-safe 1080×2400 and 360×640. Native assets and editable LibreSprite sources link to the approved Stage 2/3 files, avoiding duplicate art. The assembled scene is not a flattened replacement texture.

VERIFICATION
Tools/Validate-Unity.ps1 passed on Unity 6000.3.19f1.
SnowyMountainSideCornerStage04AssemblyReview passed: one explicit test with five viewports, no failures/skips.
The fixture checks full opaque sprite clearance, binary alpha, Point filtering, uniform whole-pixel scenery scale, no gem-opening overlap, fixed actor/UI geometry, unchanged gem and cell renderer state, and unchanged bottom controls and text. It adds no input targets and uses a disposable save profile.
Before/after comparisons show zero changed pixels throughout the upper scene and playable gem opening at every tested size. Approved source hashes and six preexisting local modifications remain exact. See QA/Validation.json, QA/UnityTestResults.xml, QA/ProtectedSourceHashes.json and QA/Assembly-*.json.

LIMITS
360×640 retains preexisting shared HUD crowding and the Unity development/editor Pixel Perfect Camera warning; the scenery renders behind controls at that size. This stage does not change shared UI dimensions. No Android build or physical-device test. Modules are overlapping courses, not newly authored seamless terrain. Stage 5 is the separate approval-gated overlap, contact-shadow and snow-layering polish pass.

REPRODUCE
powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1
powershell -ExecutionPolicy Bypass -File Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainSideCornerStage04AssemblyReview -Graphics
Then run Scripts/Validate.py using the Foundry Python with Pillow.

PixelLab usage this stage: 0 generations; no uploads.
Branch: codex/snowy-mountain-pixellab. Draft PR #218.
STOP after Stage 4 for review. Do not merge.
