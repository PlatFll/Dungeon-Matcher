from pathlib import Path
import json,shutil,html
from PIL import Image,ImageDraw
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');S=F/'ArtSource/SnowyMountainStaged/Stage03'
scratch=Path(__file__).parent
for name in ['extract.py','native_qa.py','final_qa.py','build_handoff.py']:
 if (scratch/name).resolve()!=(S/'Scripts'/name).resolve():shutil.copy2(scratch/name,S/'Scripts'/name)
src=F/'.utmp/ForestValidation'
for name,run in [('UnityPortraitTests','788af815-21c9-442a-ae78-af88812b3eae'),('UnityCrossZoneTests','c956f68b-dab5-4741-bdd5-4dc8ef3ae19a')]:
 shutil.copy2(src/(run+'.xml'),S/'QA'/(name+'.xml'))
proof=dict(compilation={'command':'powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1','result':'passed','unity':'6000.3.19f1','log':'C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-fa4ea000-4eab-493f-9e2e-fd3c71826003.log'},portrait={'test':'ForestFoundationPlayTests.SnowyMountainStage03PortraitReview','result':'passed','viewports':5,'negativeScaleAndDensityProbes':True,'xml':'UnityPortraitTests.xml'},crossZone={'test':'ForestFoundationPlayTests.SnowyMountainNativeFrameRegression','result':'passed','zones':5,'viewportsPerZone':3,'xml':'UnityCrossZoneTests.xml'},realDeviceTest=False,androidBuild=False)
(S/'QA/UnityChecks.json').write_text(json.dumps(proof,indent=2))
(S/'QA/LibreSpriteReceipt.txt').write_text('LibreSprite native cleanup: PASS\n15 editable ASE files saved, reopened and re-exported.\nAll 15 exports match their reopened roundtrips byte-for-byte at the pixel level.\nSource script: Scripts/Cleanup.js\nCommand: powershell -ExecutionPolicy Bypass -File Scripts/RunLibreSprite.ps1\n')
g=json.loads((S/'Generation.json').read_text());g['finalBalance']=4940;g['stageGenerationsSpent']=10;g['cycleGenerationsUsed']=60;(S/'Generation.json').write_text(json.dumps(g,indent=2))
readme='''Snowy Mountain / Frostwake Peaks — Stage 3 portrait frame review
===============================================================

Stage 2 backdrop was accepted by the user's Continue instruction. This stage
changes only the portrait decoration in a disposable actual-Game Unity preview.
STOP here for review; Stage 4 has not begun. No merge or production art binding.

Artwork: cold timber rails, square centered steel corner/clamp plates, dark-blue
staggered masonry, broad snow caps, short icicles and small foot deposits.
15 separate native modules, 12 meaningful opaque colors across the kit, binary
alpha. Structural rails/backing are fully opaque. Raw source retained unchanged.
PixelLab Pro text-only source (128x256): one call, 10 generations; balance 4,940.
No mockup or golden image was uploaded. Prompt and receipt: Generation.json.

Native cleanup was executed in LibreSprite, not a generative replacement:
- Native crops separated into corners, beams, clamp, repeatable masonry and snow.
- Repaired transparent gaps in generated timber; simplified near-duplicate shades.
- Centered the square steel plates/bolts and unified beam edge separation.
- Rebuilt the masonry repeat boundaries into two quiet staggered brick courses.
- Removed two detached icicle fragments; retained connected snow silhouettes.
- Saved and reopened all 15 ASE files; exact export/roundtrip pixel equality.

Unity review:
Existing modular frame and RectMask2D architecture, existing player bounds.
Masonry overfills the mask and stays behind live player/health/gem controls.
Corners use 1x/2x/3x integer physical pixels. Rails tile at matching density.
No changes to shared board dimensions, portrait bounds, characters, gems, enemy
floor line, health bars, controls, approved backdrop or other artwork.

Two shared native-frame corrections were needed by this preview:
1. The validator now recognizes the fitter's explicit native-pixel mode, instead
   of treating every new Snow frame as the legacy fixed-thickness skin. It also
   validates tiled texel density; negative probes reject distorted source pixels.
2. Native rail joins/connectors snap to whole physical pixels at odd scales.
   Hidden joins otherwise landed on half pixels. Artwork and frame size stay fixed.

Evidence:
Tools/Validate-Unity.ps1 passed on Unity 6000.3.19f1.
Portrait review test passed at 720x1280, 1080x1920, 1080x2400, inset-safe 1080x2400
and 360x640, including exact before/after shared geometry and mask coverage.
Cross-zone native frame regression passed for all five zones at 360x640,
720x1280 and 1080x1920 (15 cases). See QA/UnityChecks.json and test XML.
These are actual Unity Editor Game-scene captures; not Android or device tests.
QA/Preservation.json checks all 8,835 protected production files, Stage 2 files,
and six preexisting local edits. Of the production files, 8,833 are unchanged;
only the two reviewed frame/validator code files differ. All artwork is preserved.
QA/ScreenDifferences.json records zero changed pixels outside the portrait and
its snow cap in all five paired Unity captures.

Known retained baseline: old Frostwake outer enclosure, board and bottom HUD are
still present until their stages. At 360x640, the preexisting Pixel Perfect Camera
warning/crowded labels remain. No claim that this portrait stage fixes those.

Review.html includes raw source, native modules, assembled portrait and all five
before/after captures. Reproduce preview with:
Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainStage03PortraitReview -Graphics
Branch: codex/snowy-mountain-pixellab. Draft PR #218. Do not merge.
'''
(S/'README.txt').write_text(readme,encoding='utf-8')
# A convenient side-by-side crop from actual Unity renders, with no repainting.
imgs=[Image.open(S/f'Preview/{phase}/720x1280.png').convert('RGB').crop((0,0,720,486)) for phase in ['Before','After']]
board=Image.new('RGB',(1464,528),'#0a141f');d=ImageDraw.Draw(board)
for i,(im,label) in enumerate(zip(imgs,['Before — approved backdrop, previous portrait','Stage 3 — snowy modular portrait'])):
 x=8+i*732;d.text((x,8),label,fill='#d4ebf3');board.paste(im,(x,32))
board.save(S/'Preview/Comparison.png')
qa=json.loads((S/'QA/Native.json').read_text())
cards=''.join(f'<article><h3>{html.escape(m["name"].replace("Portrait_",""))}</h3><p>{m["size"][0]}×{m["size"][1]} · {m["colors"]} colors</p><div class="asset"><img src="Exports/{m["name"]}.png" alt="{m["name"]}"><img src="Exports/{m["name"]}.png" style="width:{m["size"][0]*3}px;height:{m["size"][1]*3}px" alt="3× enlargement"></div><a href="Native/{m["name"]}.ase">LibreSprite source</a></article>' for m in qa['modules'])
page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Snowy Mountain · Stage 3 portrait review</title>
<style>:root{color-scheme:dark;font:16px system-ui;background:#09131d;color:#d8e9f1}body{margin:0 auto;padding:32px;max-width:1500px}h1{font-size:32px;margin:0 0 8px}h2{margin-top:40px}p{color:#a9c0cf;max-width:1000px;line-height:1.6}a{color:#8edbfa}button,select{font:inherit;padding:9px 14px;border:1px solid #456073;background:#172c3c;color:#e8f4fa;border-radius:5px}img{image-rendering:pixelated}header{border-bottom:1px solid #294150;padding-bottom:20px}.badge{display:inline-block;background:#18384a;padding:5px 10px;border-radius:4px}.pair{display:grid;grid-template-columns:1fr 1fr;gap:16px;align-items:start}.shot{max-width:100%;height:auto}.shots{overflow:auto}.shots.native img{max-width:none}.assets{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:12px}article{background:#12222f;border:1px solid #263d4e;padding:16px;border-radius:6px;overflow:auto}h3{margin:0;font-size:17px}.asset{display:flex;gap:25px;align-items:center;min-height:115px}.asset img{flex:none}.standalones{display:flex;gap:30px;align-items:start;overflow:auto}.checks{padding:15px;background:#12222f;border-left:3px solid #78b9d6}label{display:inline-block;margin:10px 16px 10px 0}@media(max-width:700px){body{padding:16px}.pair{grid-template-columns:1fr}}</style>
<header><span class="badge">Stage 3 · review candidate · not merged</span><h1>Snowy portrait frame</h1><p>Cold timber, centered steel plates, dark masonry and connected snow caps. The approved alpine backdrop and shared board / portrait bounds stay unchanged.</p></header>
<h2>Actual Unity Game scene</h2><p>Rattlebones and three Frostwake enemies. Only the portrait decoration changes between each pair. Existing board and lower HUD art remain until their own stages.</p>
<label>Viewport <select id="size"><option>720x1280</option><option>1080x1920</option><option>1080x2400</option><option>1080x2400-safe</option><option>360x640</option></select></label><button id="scale">Show screenshots at 1:1</button><p id="sizeNote">Fit-to-window view. Open an image for its full capture.</p>
<div class="pair shots" id="shots"><section><h3>Before · previous portrait</h3><a id="beforeLink"><img id="before" class="shot" alt="Before actual Unity screenshot"></a></section><section><h3>Stage 3 · snowy portrait</h3><a id="afterLink"><img id="after" class="shot" alt="Revised actual Unity screenshot"></a></section></div>
<h2>Native artwork</h2><p>Raw PixelLab source → cleaned modular portrait. These are asset views; the panel mock assembly is not a gameplay screenshot. Every enlarged asset uses nearest-neighbor display.</p>
<div class="standalones"><article><h3>Raw generated source · 128×256</h3><img src="Raw/PortraitSource.png" width="128" height="256"><a href="Raw/PortraitSource.png">Original PNG</a></article><article><h3>Cleaned assembly · native 1×</h3><img src="Preview/Portrait-native.png"><a href="Preview/Portrait-4x.png">4× inspection view</a></article><article><h3>Cleanup</h3><p>15 modules · 12 opaque colors<br>Binary transparency<br>Opaque timber and tiled masonry<br>Exact LibreSprite reopen/export match</p><p>One PixelLab call: 10 generations<br>Balance: 4,940 generations</p></article></div>
<h2>Separate modules · 1× and 3×</h2><div class="assets">CARDS</div>
<h2>Verification</h2><div class="checks"><p>Unity compilation passed. Five portrait viewport checks passed, including inset safe area, clipping, centered clamps and unchanged shared geometry. Existing native frames also passed across five zones × three sizes. Negative probes reject incorrect corner scale and tiled density.</p><p>Unity Editor evidence only; no Android build or phone test in this stage. The existing 360×640 camera warning and crowded labels remain visible in both versions.</p><a href="README.txt">Handoff notes</a> · <a href="QA/Native.json">Native QA</a> · <a href="QA/UnityChecks.json">Unity checks</a> · <a href="QA/Preservation.json">Preservation hashes</a> · <a href="QA/ScreenDifferences.json">Screen differences</a></div>
<script>const size=document.querySelector('#size'),shots=document.querySelector('#shots');function update(){for(const phase of ['before','after']){let url='Preview/'+(phase==='before'?'Before':'After')+'/'+size.value+'.png';document.querySelector('#'+phase).src=url;document.querySelector('#'+phase+'Link').href=url;}}size.onchange=update;document.querySelector('#scale').onclick=()=>{const native=shots.classList.toggle('native');document.querySelector('#scale').textContent=native?'Fit screenshots to window':'Show screenshots at 1:1';document.querySelector('#sizeNote').textContent=native?'1 image pixel = 1 CSS pixel; scroll to inspect.':'Fit-to-window view. Open an image for its full capture.';};update();</script></html>'''.replace('CARDS',cards)
(S/'Review.html').write_text(page,encoding='utf-8')
print('Stage 3 review gallery and handoff prepared')
