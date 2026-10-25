from pathlib import Path
import json,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
R=F/'ArtSource/ForestSurroundRevision/PolishStage01'
W=Path(r'C:/Users/USER/.codex/visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a')
v=json.loads((R/'Diagnostics/Validation.json').read_text())
cards=''
for q in v['modules']:
 n=q['name'];cards+=f'<article><h3>{n} · 40×16 · {q["colors"]} colors</h3><p>Native 1×</p><img src="Exports/{n}.png"><p>Nearest-neighbor 6×</p><img width="240" height="96" src="Exports/{n}.png"><p><a href="Native/{n}.aseprite">LibreSprite source</a></p></article>'
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Forest Stage 1 — stone separator</title><style>
body{margin:28px;background:#131c19;color:#e5e9dc;font:16px system-ui;line-height:1.5}h1{font-size:28px}a{color:#b4d893}img{image-rendering:pixelated;max-width:100%}.cards{display:flex;gap:18px;flex-wrap:wrap}article{padding:16px;background:#26322b}.compare{display:flex;gap:20px;flex-wrap:wrap}.compare section{max-width:720px}button{padding:10px 18px;background:#304330;color:inherit;border:1px solid #718454;cursor:pointer}.note{padding:14px;background:#26352a}#screen{width:720px;max-width:100%}p{max-width:1050px}figure{margin:0}</style>
<h1>Stage 1 — a constructed mudstone divider</h1><p>Nine broad blocks use three native tile variations. Restrained moss, connected stone planes and clear joints give the battle platform a substantial front edge.</p>
<p class="note">Review checkpoint: only the 32-pixel-high separator band changes. The board, HUD, actors, approved source art and lower root/rock decorations keep their current state. No merge.</p>
<h2>Actual Unity detail</h2><img src="Previews/Separator-BeforeAfter.png" width="720" height="300">
<h2>Full Unity view</h2><button onclick="show('Unity-Separator-720x1360.png')">Revised</button> <button onclick="show('Unity-Before-720x1360.png')">Before</button><p><img id="screen" src="Previews/Unity-Separator-720x1360.png"></p>
<p><a href="Previews/Unity-BeforeAfter.png">Full side-by-side PNG</a> · <a href="Previews/Unity-Separator-1440x2720.png">1440×2720 capture</a></p>
<h2>Native modular pieces</h2><div class="cards">'''+cards+'''</div><p>Complete native 360×16 assembly:</p><img src="Previews/Native-Assembled-Strip.png" width="360" height="16">
<h2>Validation</h2><p>Unity compilation and the explicit render test passed. Captures at 720×1360 and 1440×2720 match exact 2×/4× native rendering. Every pixel outside the separator band is unchanged. The blocks have fully opaque tile boundaries, so joints cannot reveal an accidental background gap. LibreSprite documents were saved, reopened and compared pixel-for-pixel.</p>
<p>These are actual Unity art-review captures with static approved actors and UI, not a new production combat or physical-device test.</p>
<p>PixelLab balance decreased by 7 subscription generations (697.20 → 690.20), with $0 USD credits used. Two completed Pixen calls cost 1 each. An additional 5-generation debit appeared after the cancelled Pro Flash request; no result or refund was returned. The stalled Pro request produced no result.</p>
<details><summary>PixelLab source studies</summary><p>Selected olive source, before native cleanup:</p><img src="Previews/Mudstone-Study.png"><p>Rejected gray study — too tall and cracked:</p><img src="Previews/Gray-Study.png"></details>
<p><a href="README.md">Report</a> · <a href="Diagnostics/Validation.json">Pixel validation</a> · <a href="Diagnostics/UnityTests.xml">Unity test results</a> · <a href="Diagnostics/Generation.json">Generation lineage</a></p>
<script>function show(n){document.getElementById('screen').src='Previews/'+n}</script></html>'''
(R/'Review.html').write_text(html,encoding='utf-8')
selection=json.loads((R/'Scripts/Selection.json').read_text())
readme=f'''# Forest polish — Stage 1 separator review

Candidate awaiting user review. Stop before Stage 2. No merge or production integration.

The previous ground edge read as an indistinct thin strip. This revision replaces its
front 360×16 native band with nine broad constructed mudstone blocks, using three
40×16 modules. The floor where enemies stand, board position and all UI remain fixed.
The native band is y=224–240 within the existing 360×240 top environment; it renders
at screen y=448–480 in the 720×1360 review. The existing portrait foliage and foreground
continue to render over it. No old asset, scene, prefab or production code is rewritten.

## Art decisions

{selection['judgment']}

Selected raw source: Mudstone_Pixen.png, job a21ef4e6-9058-47dc-807f-f5bf5fcde4cf.
The 64×32 image contained two masonry courses. Native cleanup removes the small
internal divisions and constructs three 40×16 single-block variations from its
outer contours and stone planes. Source rows/columns and every native edit are recorded.
The unselected gray Pixen study was too tall and cracked for this boundary.

PixelLab balance decreased by **7 subscription generations** (697.20 → 690.20),
with zero USD credits used. Two completed Pixen calls explicitly charged 1 each.
An additional 5-generation debit appeared after cancellation of the Pro Flash request,
matching its quoted cost; the service returned no finished image or refund confirmation.
The earlier Pro job remained at 64% for over 15 minutes and was cancelled without
an output. Receipts, cancellation/error responses and observed balance are retained.
Do not count either cancelled request as successfully delivered artwork.

Pro/Pro Flash received the actual verified Rattlebones style PNG but stalled.
Pixen has no image-reference input. Its result was therefore manually inspected
and cleaned against the actual current golden images and forest palette; no
image-conditioned lineage is claimed for the selected Pixen source.

LibreSprite writes the final native pixels, saves editable `.aseprite` sources,
exports PNGs and reopens/re-exports the documents for exact comparison.
`Diagnostics/NativeCleanup.json` records source coordinate selections, exact palette
mappings and every final pixel correction. Exported tiles are native 40×16 with
no image resampling or filtering. Opaque mortar is intentional at repeatable
tile edges; transparent padding would produce gaps in a full-width structural strip.

## Unity / pixel evidence

- `Tools/Validate-Unity.ps1`: passed with Unity 6000.3.19f1.
- `ForestSeparatorReviewTests.SeparatorReview`: passed.
- Actual captures: 720×1360 and 1440×2720, exact nearest-neighbor relationship.
- All {v['preserved_image_bindings']} existing visible Image bindings and RectTransforms preserved.
- Before capture equals the prior unified-corner capture exactly.
- Every pixel above y=448 and below y=480 matches the before capture exactly.
- {v['unchanged_existing_files']} existing art/scene/script/resource/settings files hash-identical.
- All three native sources reopen to exactly the exported RGBA pixels.
- Tiles are fully opaque, point sampled, unmipped, uncompressed; seams verified against exports.
- Board remains 512×512 at (104,528); portrait/frame dimensions unchanged.

Scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_StoneSeparator.unity`.
Menu: Dungeon Matcher / Art Preview / Forest stone separator.
The scene is excluded from player build settings and uses static readability samples.
This is not live production combat, Android validation or real-device testing.

## Scope / next checkpoint

The annotated lower root joins and mushroom/rock attachments remain outstanding.
They belong to later requested stages and are not presented as fixed by this separator.
Stage 2 starts only after review. Branch: feature/forest-zone-side-surround-revision;
draft PR #209 targets feature/forest-zone-staged-rebuild. Main unchanged.
'''
(R/'README.md').write_text(readme,encoding='utf-8')
for name in ['VerifyForestSeparator.py','PackageForestSeparator.py','RunSeparatorNative.ps1','RunSeparatorUnity.ps1']:
 shutil.copy2(W/name,R/'Scripts'/name)
print('Packaged separator gallery and review report.')
