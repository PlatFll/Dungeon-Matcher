# Branch and Execution Rules

## Branch
Create and use a new branch for this zone rebuild. Suggested name:
`feature/ironvein-pixellab-staged-rebuild`

## Execution Rules
- Follow the stage files in numeric order.
- Each stage is a separate focused generation / cleanup / integration pass.
- Do not compress multiple stages into one broad PixelLab attempt.
- Do not present a half-built result as done.
- Keep a running ledger of:
  - prompt used
  - asset family generated
  - what was approved
  - what was rejected
  - cleanup/recolor notes
- If a generated result is weak, regenerate that asset family before moving on.
- If an asset family passes, freeze it and move forward.
- Stop only at the final review stage.
- Do not merge.
