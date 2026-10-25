from pathlib import Path
from PIL import Image
import json,hashlib,shutil,re
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/PolishFinal';W=Path(__file__).parent
v=json.loads((R/'Diagnostics/Validation.json').read_text());cards=''
for a in v['native_modules']:
 n=a['name'];w,h=a['dimensions'];im=Image.open(R/'Exports'/(n+'.png'));im.resize((w*3,h*3),Image.Resampling.NEAREST).save(R/'Previews'/(n+'-3x.png'))
 cards+=f'<article><h3>{n.replace("_"," ")}</h3><p>{w}×{h} native · {a["colors"]} opaque colors</p><div class="pair"><img src="Exports/{n}.png" width="{w}" height="{h}"><img src="Previews/{n}-3x.png" width="{w*3}" height="{h*3}"></div><p><a href="Exports/{n}.png">Native PNG</a> · <a href="Native/{n}.aseprite">LibreSprite source</a></p></article>'
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Magical Forest — final surround polish</title><style>body{margin:26px;background:#14201c;color:#e5eadc;font:16px system-ui;line-height:1.5}h1{font-size:28px}a{color:#b6d99a}img{image-rendering:pixelated;max-width:100%}.pair,.cards{display:flex;gap:18px;flex-wrap:wrap;align-items:flex-start}article{padding:18px;background:#26352c}button{padding:10px 14px;margin:3px;background:#354b38;color:inherit;border:1px solid #738a68;cursor:pointer}p{max-width:1100px}.note{background:#273a30;padding:15px}#screen{width:720px}#status{font-weight:bold}.raw img{max-height:300px}</style>
<h1>Magical Forest — Stages 3–6 complete</h1><p>Connected stone/root corners with embedded mushrooms and moss, lower sweeping roots, hanging vines, flower growth and four restrained spirit lights. All source modules remain separate and editable.</p>
<p class="note">The approved upper scene, separator, portrait, characters, playable board, board frame and bottom HUD are unchanged. These are actual Unity art-review captures with static actor/UI samples. No production gameplay integration or merge.</p>
<h2>Before / final — lower surroundings</h2><img src="Previews/Unity-BeforeAfter-Lower.png">
<h2>Actual Unity captures and wisp preview</h2><div><button onclick="still('Unity-Before-720x1360.png','Before: approved Stage 2')">Before</button><button onclick="still('Unity-Stage3-720x1360.png','Stage 3: rooted mushrooms and moss')">Stage 3</button><button onclick="still('Unity-Stage4-720x1360.png','Stage 4: lower-area enrichment')">Stage 4</button><button onclick="still('Unity-Final-720x1360.png','Final: all stages')">Final</button><button onclick="play()">Play spirit lights</button><button onclick="stop()">Pause</button></div><p id="status">Final: all stages</p><img id="screen" src="Previews/Unity-Final-720x1360.png"><p><a href="Previews/Unity-Final-1440x2720.png">1440×2720 PNG</a> · <a href="Previews/Unity-Wisps.gif">GIF preview</a></p>
<details><summary>Supplied lower-area target</summary><img src="References/LowerTarget.png"><p>The target guides root flow, attached growth and framing. Existing shared HUD pixels and dimensions remain authoritative.</p></details>
<h2>Native assets — 1× and 3×</h2><div class="cards">'''+cards+'''</div>
<h2>Verification</h2><p>Unity compile and staged render tests passed. 720×1360 and 1440×2720 renders match exact pixel doubling. All nine LibreSprite sources reopen to identical PNGs; hard alpha and canvas clearance pass. Screenshot masks verify the locked upper area, board/frame and bottom controls/text. The six-second wisp loop closes; hovering is integer-native-pixel only and reduced motion is static.</p>
<p>PixelLab: 11 generations for this continuation; 20 debited across the latest six-stage polish pass, including the previously documented Stage 1 cancelled-job debit. No USD credits used.</p>
<p><a href="README.md">Full report</a> · <a href="Diagnostics/Validation.json">Validation</a> · <a href="Diagnostics/UnityTests.xml">Unity test results</a> · <a href="Diagnostics/Costs.json">Usage ledger</a></p><p>Finished and stopped for final visual review. Nothing merged.</p>
<script>let timer=null,i=0;function stop(){clearInterval(timer);timer=null}function still(file,label){stop();document.getElementById('screen').src='Previews/'+file;document.getElementById('status').textContent=label}function play(){stop();document.getElementById('status').textContent='Actual Unity frames: 6-second spirit-light loop';i=0;function next(){document.getElementById('screen').src='Previews/Unity-Motion-'+String(i).padStart(2,'0')+'.png';i=(i+1)%12}next();timer=setInterval(next,500)}for(let n=0;n<12;n++){let im=new Image;im.src='Previews/Unity-Motion-'+String(n).padStart(2,'0')+'.png'}</script></html>'''
rawcards=''
for name in ['LeftCorner','RightCorner','SweepRightV2','VineV2','GrowthLeft','GrowthRight','Wisp']:
 im=Image.open(R/'Raw'/(name+'.png'));w,h=im.size
 rawcards+=f'<article><h3>{name}</h3><p>{w}×{h} native PixelLab output</p><img src="Raw/{name}.png" width="{w}" height="{h}"><p><a href="Raw/{name}.png">Raw PNG</a></p></article>'
html=html.replace('<h2>Verification</h2>','<h2>Selected raw PixelLab sources — native resolution</h2><p>These are generation outputs before the LibreSprite cleanup shown above. Rejected studies remain in Raw/ for provenance.</p><div class="cards raw">'+rawcards+'</div><h2>Verification</h2>')
(R/'Review.html').write_text(html,encoding='utf-8')
jobs={}
for p in (R/'Diagnostics').glob('*-Generation.json'):jobs[p.stem.removesuffix('-Generation')]=json.loads(p.read_text())
cost=dict(starting_balance=688.20,ending_balance=677.20,debited_this_continuation=11,completed_calls=len(jobs),usd_credits_used=0,latest_polish_pass_total_debited=20,selected_raw_sources=['LeftCorner','RightCorner','SweepRightV2','VineV2','Wisp','GrowthLeft','GrowthRight'],rejected_studies=['SweepLeft','SweepRight','SweepLeftV2','Vine'],jobs={n:q['job_id']for n,q in jobs.items()})
(R/'Diagnostics/Costs.json').write_text(json.dumps(cost,indent=2))
log=R/'Diagnostics/UnityCompile.log';sourcepaths=['Assets/_Game/Scripts/Editor/ForestFinalPolishReviewTests.cs','Assets/_Game/Scripts/ArtPreview/ForestPolishPreviewMotion.cs']
(R/'Diagnostics/CompileValidation.json').write_text(json.dumps(dict(command='powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1',result='Passed',exit_code=0,unity='6000.3.19f1',log_sha256=hashlib.sha256(log.read_bytes()).hexdigest(),source_hashes={p:hashlib.sha256((F/p).read_bytes()).hexdigest()for p in sourcepaths}),indent=2))
readme=f'''# Magical Forest — final surround polish, Stages 3–6

Final review candidate. User authorized completion of every remaining stage in one
run; this supersedes the earlier per-stage stop instruction. **No merge.**

## Completed work

- Stage 3: two 88×180 corner modules integrate pale/red mushrooms, stems, soil
  contacts, moss and small flowers into the existing stone/root formations. The
  approved upper root/crown section is retained at its exact native registration.
  Old combined layers and separate mushroom accents remain preserved in earlier
  scenes, and are disabled only in this final review variant.
- Stage 4: a paired set of sweeping roots, two hanging-vine modules and two
  attached growth patches enrich the lower surroundings. One main stone mass
  remains per side. Wood roots carry the composition; foliage supports them.
  The weaker left branch studies were rejected. The stronger right S-curve study
  supplies the paired root variants, with native end completion, cleanup, and
  different flower accents. Nothing is resampled. Broad top-lit planes remain
  consistent on the opposite-side variant.
- Stage 5: one 24×24 four-color spirit sprite appears at four separate placements.
  Preview motion uses a six-second staggered fade and one native pixel of hover.
  Reduced motion holds a stable position and opacity. No rotation, scaling,
  texture blur or subpixel movement is used.
- Stage 6: art review, native integrity, source preservation, layering, pixel
  scale, locked-region comparison and Unity compilation/render/motion checks.

## Sources and workflow

PixelLab produced eleven completed image jobs. The two corner edits were conditioned
on exact native assemblies of the current root/stone/attachment assets. New
branch, vine, growth and spirit studies were generated individually; no full
scene repaint. The current Rattlebones, Farmer and Pan PNGs were hash-verified and
inspected for rendering discipline, not copied as environment palettes.

Nine modular PNGs and nine editable `.aseprite` files were written through
LibreSprite native pixel scripts. Each was reopened and re-exported, then compared
exactly. Raw generations, edit inputs, request receipts, selected/rejected-source
ledger, cleanup plans and scripts are retained alongside the exports. Controlled
material ramps are used; the larger combined corner sprites include the retained
leaf-crown colors and multiple stone/wood/plant/mushroom materials.

PixelLab balance: **688.20 → 677.20**, 11 subscription generations this continuation,
zero USD credits. Latest six-stage polish total: **20 debited**, including Stage 1's
previously reported cancelled-request debit. No generation remains in progress.

## Executed validation

- `Tools/Validate-Unity.ps1`: passed, Unity 6000.3.19f1.
- `ForestFinalPolishReviewTests.FinalReview`: passed.
- Actual Unity captures for Stages 3, 4 and 5/final at 720×1360, plus final 1440×2720.
- Final large capture is exact nearest-neighbor doubling of the smaller capture.
- Before capture matches the approved Stage 2 capture exactly.
- Upper HUD/battle/separator, board cells/painted frame and bottom controls/text
  remain pixel-identical; no changed pixel falls outside authorized dressing.
- All {v['unchanged_image_bindings']} existing visible Image bindings/rectangles remain intact.
- All {v['unchanged_existing_files']} existing art/scene/script/resource/settings files remain hash-identical.
- Every new native sprite has binary alpha, one connected silhouette and clear
  canvas margins. All nine LibreSprite save/reopen/export comparisons are exact.
- Twelve sampled Unity motion frames change only the new spirit-light pixels;
  loop closure and static reduced-motion behavior pass.

`Assets/_Game/Scenes/ArtPreview_ForestSurround_FinalPolish.unity` is a separate
review scene, excluded from player builds. The motion helper is guarded by
`UNITY_EDITOR`; it is not a new production ambient system. Captures contain static
approved actor/UI samples. No production combat, Android build or physical-device
test is claimed for this art-review stage.

## Review

Open `Review.html` for before/final comparisons, intermediate stage captures,
actual Unity motion playback, raw target reference, native assets and editable
sources. Prior scenes remain available. Draft PR #209 on
`feature/forest-zone-side-surround-revision` is updated for final visual review.
Stop here; no merge or next task is authorized by this report.
'''
(R/'README.md').write_text(readme,encoding='utf-8')
for fn,dst in [('VerifyForestFinal.py','Verify.py'),('PackageForestFinal.py','Package.py'),('DownloadForestFinal.py','Download.py'),('AuditForestFinal.py','Audit.py')]:
 if (W/fn).exists() and (W/fn).resolve()!=(R/'Scripts'/dst).resolve():shutil.copy2(W/fn,R/'Scripts'/dst)
print('Packaged final review, native modules, animation and 11-generation usage ledger.')
