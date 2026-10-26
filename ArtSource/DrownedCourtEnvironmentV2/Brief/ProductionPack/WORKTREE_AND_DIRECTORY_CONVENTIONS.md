# Repository hygiene and proposed folder organization

1. Inspect latest repo branch and local worktree state; create a new isolated branch `feature/drowned-court-pixellab-environment-v2` from latest main, or choose a unique suffix if occupied, never reset/stash unrelated user files. Keep existing `DrownedCourt` and presentation work intact.
2. Use project existing art locations if established; don't introduce parallel runtime presentation ownership. Provisional source organization (adjust to real conventions):
```
ArtSource/DrownedCourtEnvironmentV2/
  Brief/             # stage prompt pack copies / reference provenance
  RawPixelLab/       # untouched output/job receipts
  Native/            # .aseprite/.ase editable cels
  Exports/           # final true-native transparent PNGs
  Preview/           # before/after, native, comparative & Unity captures
  Progress/          # stage gates, ledger, review state
  QA/                # imported settings + exact pixel checks
Assets/_Game/Art/DrownedCourtEnvironmentV2/
Assets/_Game/Scenes/ArtPreview_DrownedCourtV2.unity
```
3. Preserve Unity .meta GUID history. Do not replace approved production PNGs by deleting/recreating them. Keep candidates separate and link accepted art through opt-in bindings.
4. Log every asset: source prompt, model/tool, raw file sha, native sha, dimensions, palette, alpha, anchor, material, accepted status, Unity binding.
5. `ArtPreview` scene is excluded from build settings; separate from real gameplay test.
6. Current Court uses zone prefabs/resources and existing native presentation owners; inspect these and update only existing integration points after the user approves Stage12.
