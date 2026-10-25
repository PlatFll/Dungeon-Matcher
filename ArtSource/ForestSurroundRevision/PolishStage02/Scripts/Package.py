from pathlib import Path
from PIL import Image
import json,hashlib,shutil,re,difflib,urllib.request
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/PolishStage02';W=Path(r'C:/Users/USER/.codex/visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a')
v=json.loads((R/'Diagnostics/Validation.json').read_text())
cards=''
for side in ['Left','Right']:
 cards+=f'<article><h3>{side} root · 88×180 · 19 colors</h3><div class="pair"><figure><img src="Exports/Root_{side}.png"><figcaption>Native root layer, 1×</figcaption></figure><figure><img src="Previews/{side}-Native-Assembly.png"><figcaption>Root + preserved stone, 1×</figcaption></figure></div><p><img src="Previews/{side}-Assembly-3x.png" width="264" height="540"></p><a href="Native/Root_{side}.aseprite">LibreSprite root source</a> · <a href="Native/StoneAnchor_{side}.aseprite">Stone layer source</a></article>'
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Forest Stage 2 — continuous root joins</title><style>body{margin:28px;background:#131c19;color:#e5e9dc;font:16px system-ui;line-height:1.5}h1{font-size:28px}a{color:#b4d893}img{image-rendering:pixelated;max-width:100%}.cards,.pair{display:flex;gap:22px;flex-wrap:wrap}article{padding:18px;background:#26322b}figure{margin:0}button{padding:10px 18px;background:#304330;color:inherit;border:1px solid #718454;cursor:pointer}.note{padding:14px;background:#26352a}#screen{width:720px;max-width:100%}p{max-width:1050px}</style>
<h1>Stage 2 — continuous lower roots</h1><p>Two rebuilt bark curves replace the flared, overlapping bough ends. Each root flows from its existing leaf crown into the wrap around its stone anchor.</p>
<p class="note">The approved separator, upper scene, board/frame and HUD remain unchanged. Existing mushroom and moss accent sprites retain their pixels and positions. Their integration pass is Stage 3 and has not started.</p>
<h2>Actual Unity before / after</h2><img src="Previews/Unity-BeforeAfter-Lower.png">
<h2>Full Unity view</h2><button onclick="show('Unity-RootJoins-720x1360.png')">Revised</button> <button onclick="show('Unity-Before-720x1360.png')">Before</button><p><img id="screen" src="Previews/Unity-RootJoins-720x1360.png"></p><p><a href="Previews/Unity-RootJoins-1440x2720.png">1440×2720 capture</a></p>
<h2>Native modules and close-ups</h2><div class="cards">'''+cards+'''</div>
<h2>Verification</h2><p>Unity compilation and review tests passed at 720×1360 and 1440×2720. Rendering doubles exactly. All pixels above the lower repair area, board cells, painted board frame, HUD artwork/text and existing mushroom/moss accents match the prior capture. Native PNGs have binary transparency and at least two pixels of canvas clearance. LibreSprite save/reopen comparisons are exact.</p>
<p>These are actual Unity art-review captures with static approved actors/UI. No production combat or physical-device testing was performed in this stage.</p>
<p>PixelLab: 2 completed image edits, 2 subscription generations, $0 USD credits. Latest polish pass total: 9 generations debited including Stage 1's reported cancelled-request debit.</p>
<details><summary>Raw PixelLab edits</summary><div class="pair"><img src="Previews/Left-Raw-3x.png"><img src="Previews/Right-Raw-3x.png"></div><p>The generated stone repaint was discarded. Only the repaired roots were cleaned and used, with existing stone material pixels preserved underneath.</p></details>
<p><a href="README.md">Report</a> · <a href="Diagnostics/Validation.json">Pixel validation</a> · <a href="Diagnostics/UnityTests.xml">Unity results</a> · <a href="Diagnostics/Costs.json">Usage</a></p><p>Stopped for Stage 2 review. No merge.</p><script>function show(n){document.getElementById('screen').src='Previews/'+n}</script></html>'''
(R/'Review.html').write_text(html,encoding='utf-8')
cost=dict(stage=2,starting_balance=690.20,ending_balance=688.20,debited=2,usd_credits_used=0,polish_pass_total_debited=9,completed_jobs=['deca51dd-5635-4c0f-8fbf-a5de70c06b91','daeb8554-14cd-4b20-8c50-758dacf20c75'])
(R/'Diagnostics/Costs.json').write_text(json.dumps(cost,indent=2))
log=R/'Diagnostics/UnityCompile.log'
(R/'Diagnostics/CompileValidation.json').write_text(json.dumps(dict(command='powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1',exit_code=0,result='Unity validation succeeded with Unity 6000.3.19f1',local_log=str(log),log_sha256=hashlib.sha256(log.read_bytes()).hexdigest(),editor_source_sha256=hashlib.sha256((F/'Assets/_Game/Scripts/Editor/ForestRootJoinReviewTests.cs').read_bytes()).hexdigest()),indent=2))
readme=f'''# Forest polish — Stage 2 root / branch joins

Review candidate; stop before Stage 3. No merge or production integration.

## Problem and correction

The two lower boughs ended in flared triangular tips. Those separate tips overlapped
the pre-existing stone roots with mismatched tangents and exposed old spur outlines.
Two image-conditioned PixelLab edits rebuild the connection as continuous bark curves.
LibreSprite cleanup restores the existing upper leaf crowns, reconciles wood ramps,
removes detached generated scraps and saves the repaired roots as separate native layers.
The old bough-5 instances and combined stone/root instances are disabled only in this
new review scene; every original source and earlier scene is preserved.

Four modular exports:
- Root_Left / Root_Right: 88×180, 19 opaque colors each, one connected silhouette each.
- StoneAnchor_Left: 85×101; StoneAnchor_Right: 86×105; 13 colors each.

The stone layers are separated from the original roots so obsolete wooden spurs do
not survive under the repair. Every original colored stone-plane pixel is unchanged:
1,868 on the left and 3,378 on the right. The small stone recess exposed by removal of
the disconnected right spur is filled with the existing stone ramp. This restores the
surface behind removed wood; it does not move or redesign the corner stone masses.
Each assembled root/stone pair has one connected silhouette. Pixel connectivity is
supporting evidence; the actual Unity comparison is the visual review authority.

## Generation and native lineage

PixelLab Pixen edited two 88×184 transparent native assemblies of the current bough
and stone source files. `References/InputAssembly.json` records exact positions/hashes.
No image scaling was used. PixelLab received the actual target assemblies, not a
flattened screen. Current Rattlebones/Farmer/Pan references were hash-verified and
visually inspected for rendering discipline. The source-image target governs placement.
Both requests completed at 1 generation each: **2 generations**, balance
690.20 → 688.20, zero USD credits. Total debited across this latest polish pass is 9,
including the Stage 1 service/cancellation discrepancy documented there.

Generated stone changes were discarded. Original stone material pixels remain exact.
Editable `.aseprite` files and PNGs were written through LibreSprite, then reopened
and re-exported for exact RGBA comparison. Alpha is binary; all occupied bounds have
at least two pixels of clearance. The two final 88×180 root canvases remove only
empty rows from the 88×184 generation canvas; no rendered pixels are cropped off.

## Verification

- `Tools/Validate-Unity.ps1`: passed with Unity 6000.3.19f1.
- `ForestRootJoinReviewTests.RootJoinReview`: passed.
- Actual Unity screenshots at 720×1360 and 1440×2720, exact nearest-neighbor relationship.
- Before capture matches the approved Stage 1 separator capture exactly.
- Top 1,000 rows, board cells and painted frame, bottom HUD/buttons/text and existing
  mushroom/moss accent pixels remain identical.
- All {v['unchanged_image_bindings']} prior visible Image bindings/RectTransforms preserved.
- {v['unchanged_existing_files']} existing art/scene/script/resource/settings files remain hash-identical.
- No screenshot differences outside the old/new authorized root-and-anchor masks.
- All four LibreSprite source reopen/export comparisons pass.

Scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_RootJoins.unity`.
Menu: Dungeon Matcher / Art Preview / Forest continuous lower root joins.
Excluded from player builds. Captures use static approved character/UI samples;
this is not live production combat, Android testing or real-device evidence.

## Next checkpoint

The red-circled root joins are this stage's revision. Mushroom/greenery attachment
and richer corner integration remain Stage 3 work. No new wisps or animations.
Branch feature/forest-zone-side-surround-revision, draft PR #209. Stop for review.
'''
(R/'README.md').write_text(readme,encoding='utf-8')
for name,dst in [('VerifyForestRootJoins.py','Verify.py'),('PackageForestRootJoins.py','Package.py'),('RunRootJoinNative.ps1','RunNative.ps1'),('RunRootJoinUnity.ps1','RunUnity.ps1')]:shutil.copy2(W/name,R/'Scripts'/dst)
print('Packaged Stage 2 root review and exact usage report.')
