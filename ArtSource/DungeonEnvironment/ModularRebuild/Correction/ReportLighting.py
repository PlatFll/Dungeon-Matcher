"""Derived review images and documentation only; never edits production artwork."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import json,hashlib,math,xml.etree.ElementTree as ET
K=Path(__file__).resolve().parents[1];P=K.parents[2];L=K/'Correction/Lighting'
qa=json.loads((L/'Validation.json').read_text());test=ET.parse(K/'Evidence/UnityTests.xml').getroot()
assert test.attrib['result']=='Passed' and test.attrib['passed']=='2'
layouts={}
def upper_bottom(width,height,inset):
 # Measured region boundary from the unchanged layout owner's reference-composition formula.
 sx,sy,sw,sh=(24,80,1032,2200) if inset else (0,0,width,height)
 scale=max(1,math.floor(sw/456));vx=math.ceil(sx/scale)*scale+4*scale;vy=math.ceil(sy/scale)*scale+4*scale
 w=math.floor((sx+sw-4*scale-vx)/(2*scale))*2;h=math.floor((sy+sh-4*scale-vy)/(2*scale))*2
 fit=min(w*.85*scale/568,(h-156-12-320)*scale/568)
 board=math.floor(fit*568/scale);remaining=h-156-board
 battle=min(max(320,min(480,remaining-12)),remaining-12)
 return height-(vy+(h-battle)*scale)
for f in sorted((K/'Review/Unity').glob('*.png')):
 before=Image.open(L/'BeforeUnity'/f.name).convert('RGB');after=Image.open(f).convert('RGB')
 diff=ImageChops.difference(before,after);box=diff.getbbox();boundary=upper_bottom(*after.size,'safe' in f.stem)
 assert box and box[3]<=boundary,(f.name,box,boundary)
 layouts[f.stem]={'change_bounds':box,'upper_scene_bottom':boundary,'lower_scene_byte_pixels_unchanged':True,'capture_sha256':hashlib.sha256(f.read_bytes()).hexdigest()}
 height=690 if after.height<2000 else (1120 if 'safe' not in f.stem else 1100)
 # Unity's 1080 captures use 2x source pixels; this is a labeled review-only nearest-neighbor view.
 for label,img in [('Before',before),('After',after)]:
  crop=img.crop((0,0,img.width,height));crop.save(L/'Review'/f'{f.stem}-{label}-Upper.png')
  if f.stem=='1080x1920':
   native=crop.resize((crop.width//2,crop.height//2),Image.Resampling.NEAREST);native.save(L/'Review'/f'{label}-NativeUpper.png')
ref=Image.open(K/'Brief/01_REFERENCE/full_scene_reference.png');ref.crop((0,0,941,600)).save(L/'Review/Mockup-Upper.png')
board=Image.new('RGB',(1096,383),'#151621');d=ImageDraw.Draw(board)
for x,label in [(0,'Before'),(556,'After')]:
 d.text((x+8,8),label+' - native pixel scale from Unity 2x capture',fill='white');board.paste(Image.open(L/'Review'/f'{label}-NativeUpper.png'),(x,32))
board.save(L/'Review/BeforeAfterUpper.png')
for name,box in {'Floor':(315,407,1080,674),'Walls':(315,0,1080,407),'Props':(315,150,1080,675),'PlayerFrame':(30,70,320,630),'BoardFrame':(80,680,995,1590),'UI':(150,1610,940,1885)}.items():
 Image.open(K/'Review/Unity/1080x1920.png').crop(box).save(K/'Correction/Review'/f'{name}-After.png')
(L/'UnityRenderChecks.json').write_text(json.dumps({'tests':dict(test.attrib),'layouts':layouts},indent=2)+'\n',encoding='utf-8')
style='''<style>body{background:#151621;color:#eee;font:16px system-ui;margin:24px}a{color:#d6bdff}p{max-width:1000px;line-height:1.5}.compare{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:14px}figure{margin:0;overflow:auto;background:#212230}figcaption{padding:10px}img{image-rendering:pixelated;max-width:100%;vertical-align:top}header{position:sticky;top:0;background:#151621ee;padding:12px;z-index:2}button,select{padding:10px;background:#30283e;color:white;border:1px solid #77618d}.exact{display:block}.exact img{max-width:none}.assets{display:flex;flex-wrap:wrap;gap:16px}.assets figure{padding:8px}details{margin:24px 0}@media(max-width:800px){.compare{grid-template-columns:1fr}}</style>'''
page='''<!doctype html><meta charset="utf-8"><title>Dungeon lighting correction</title>'''+style+'''
<h1>Dungeon upper scene - lighting correction</h1>
<p>Same masonry, silhouettes, placements and UI. The far wall is cooler and darker, the standing floor has three clearer depth values, and the existing torch regions use warm brown/orange stone planes with a little reflected warmth on the floor. Direct LibreSprite edits; no generation.</p>
<header><select id="view"><option>1080x1920</option><option>720x1280</option><option>1080x2400</option><option>1080x2400-safe-area</option></select> <button id="zoom">Toggle actual capture pixels</button> <a href="Review.html">Full reconstruction gallery</a></header>
<div class="compare" id="compare"><figure><figcaption>Approved mockup - atmosphere reference</figcaption><img src="../Correction/Lighting/Review/Mockup-Upper.png"></figure><figure><figcaption>Immediately before this lighting pass</figcaption><img id="before" src="../Correction/Lighting/Review/1080x1920-Before-Upper.png"></figure><figure><figcaption>Updated - actual Unity render</figcaption><img id="after" src="../Correction/Lighting/Review/1080x1920-After-Upper.png"></figure></div>
<p>Fit view changes display size only. Actual-pixel mode shows capture pixels. Existing modular stone topology remains more regular than the mockup; this correction intentionally preserves it. The art preview has no combat characters or gems.</p>
<details open><summary>Native 1x pixel comparison</summary><img src="../Correction/Lighting/Review/BeforeAfterUpper.png"></details>
<details><summary>Two-pass review decisions</summary><p>Pass 1 improved depth, stage clarity and stone readability while preserving layout. Broad orange faces were too strong. Pass 2 softened those faces, retained warm lit edges, and tinted only complete existing floor-stone clusters beneath the torches.</p><img src="../Correction/Lighting/PassOneUnity.png" style="width:360px"></details>
<details><summary>19 edited modules - native 1x and nearest-neighbor 2x</summary><div class="assets">'''
for a in qa['changed_assets']:
 n=Path(a['path']).stem;page+=f'<figure><figcaption>{n}</figcaption><img src="../Correction/Lighting/Review/{n}.png"></figure>'
page+='''</div></details><p>Verified: all 150 native cel/export/Unity copies agree; binary alpha; 224 edge pairings; two Unity EditMode tests across four layouts. Scene, placement manifest, layout code, build settings and all six golden-reference/production image hashes are unchanged. User visual approval is pending.</p>
<script>const v=document.querySelector('#view');v.onchange=()=>{for(const id of ['before','after'])document.querySelector('#'+id).src='../Correction/Lighting/Review/'+v.value+'-'+id[0].toUpperCase()+id.slice(1)+'-Upper.png'};document.querySelector('#zoom').onclick=()=>document.querySelector('#compare').classList.toggle('exact');</script>'''
(K/'Review/Lighting.html').write_text(page,encoding='utf-8')
report='''# Dungeon upper-scene lighting correction

Status: native art updated in the isolated modular Unity preview; awaiting user visual review. No merge or production-scene integration.

## Scope and result

Nineteen existing modules received color/shading adjustments in LibreSprite: four BattleWall tiles, twelve floor tiles, both TorchLitMasonry sprites, and RightTopPillar's already-warm stone clusters. The remaining 131 exports are unchanged. No pixels were added, removed, scaled or moved. Doorway, torches, chains, banners, barrels, bones, frames and UI placement are preserved.

- Cooler, slightly darker far-wall palette separates the arch and brightened combat floor.
- Existing torch-lit masonry now has restrained red-brown outer planes and warmer orange highlights nearest the flame.
- The rear/middle/near floor families have distinct value ramps. A few complete existing stone clusters below the torches carry warm reflected color; no extra cracks or detail were added.
- The unchanged foreground lip now reads against the brighter standing surface.

## Visual iteration

Pass 1: depth, central-stage clarity, readable stones and shape preservation passed visual comparison. The broad orange brick faces were too assertive, so localized warmth did not yet pass.

Pass 2: softened broad warm faces, retained orange lit edges, and slightly extended the subtle floor reflection. All five requested visual checks now pass internal review: deeper upper background, warmer localized torch light, clearer combat stage, readable quiet stone faces, and preserved shapes/layout. The modular brick geometry remains more regular than the generated mockup by design; no redesign was authorized. Artistic acceptance remains the user's decision.

## Verification

- Exact canvas, alpha-mask and connected color-cluster boundary equality against the pre-lighting baseline for all 19 edited assets.
- Binary transparency and native LibreSprite cel / exported PNG / Unity-copy equality for all 150 modules.
- 224 horizontal/vertical seam pairings passed across seven tile families.
- Scene YAML, placement manifests, assembly/editor/test C#, build settings and six registered golden-reference/production images remain byte-identical to the starting baseline.
- Two existing Unity EditMode tests passed after the final pass, with fresh camera renders at 720x1280, 1080x1920, 1080x2400 and 1080x2400 with safe-area insets. Integer native placement, portrait/board clipping, all-module usage, save-file isolation and release-build isolation passed.
- Render differences are confined to the upper scene; lower board/HUD pixels are unchanged in all four captures.
- No C# or Unity serialization changed, so the broader compiler-only validator was not rerun for this art-only correction. The prior compiler evidence remains attached to the unchanged source. These captures are EditMode evidence, not live-combat/device tests.
- Zero generation calls or credits for this lighting correction.

## Reproduce and review

`Correction/LightingCorrection.js` reads immutable `Correction/Lighting/Before/` exports and applies native LibreSprite color assignments. `Build.ps1` applies this pass last; it does not generate new art. Use `Correction/ValidateLighting.py --import` to verify locked geometry and copy only verified modified PNGs, then run `DungeonModularPreviewTests` in Unity. `Correction/ReportLighting.py` regenerates the comparison gallery from actual captures.

Review: `Review/Lighting.html`. Technical evidence: `Correction/Lighting/Validation.json`, `UnityRenderChecks.json`, and `Evidence/UnityTests.xml`. The first trial remains in `Correction/Lighting/PassOneUnity.png`.
'''
(L/'REPORT.md').write_text(report,encoding='utf-8')
for f in ['README.md','VALIDATION_REPORT.md']:
 p=K/f;s=p.read_text(encoding='utf-8');s=s.replace('21 changed, 21 new, 108 retained','22 changed, 21 new, 107 retained').replace('21 revised, 21 new, 108 retained','22 revised, 21 new, 107 retained')
 heading='## Current lighting-only follow-up'
 if heading not in s:s+='\n'+heading+'\n\nThe latest 19-module lighting pass preserves the existing composition and every sprite silhouette. See [lighting comparison](Review/Lighting.html) and [scoped report](Correction/Lighting/REPORT.md) for the immediate before/after, exact preservation checks and fresh Unity renders. No additional generation spending.\n'
 p.write_text(s,encoding='utf-8')
p=K/'Review/Review.html';s=p.read_text(encoding='utf-8').replace('21 revised, 21 new, 108 retained','22 revised, 21 new, 107 retained')
if 'Lighting.html' not in s:s=s.replace('<header>','<p><a href="Lighting.html">Latest lighting correction: immediate before / after / mockup</a></p><header>')
p.write_text(s,encoding='utf-8')
p=P/'Docs/PROJECT_STATE.md';s=p.read_text(encoding='utf-8').replace('modules (21 revised, 21 new, 108 retained)','modules (22 revised, 21 new, 107 retained)')
marker='user visual approval and any integration/merge remain pending.'
extra='\nThe subsequent upper-scene lighting correction changes 19 existing modules in\nLibreSprite: cooler far wall, localized warm torch stone and separated floor\nvalues. All masks, cluster boundaries, scene/layout files and 131 other exports\nare unchanged. Fresh four-layout Unity rendering tests pass; see\n`ArtSource/DungeonEnvironment/ModularRebuild/Correction/Lighting/REPORT.md`.'
if extra not in s:s=s.replace(marker,marker+extra,1)
p.write_text(s,encoding='utf-8')
(K/'CHECKPOINT.md').write_text('''# Current checkpoint - PR 200 lighting correction

The upper dungeon lighting-only follow-up is complete on `codex/dungeon-modular-scene-rebuild`. Native LibreSprite edits: 19 existing modules, 131 exports unchanged. Cooler/darker back wall, localized warm stone around torches and clearer floor-depth values. Scene, code, placement manifest, alpha masks and original color-cluster boundaries are unchanged. Zero additional generations.

Two visual passes; final native/export/copy, 224 seam pairings and two Unity tests across four layouts passed. Current evidence: `Correction/Lighting/Validation.json`, `UnityRenderChecks.json`, `REPORT.md`, `Review/Lighting.html` and `Evidence/UnityTests.xml`.

User visual review is next. No merge authorized and no production integration performed. Preserve the four pre-existing incidental native/editor metadata changes outside this work.
''',encoding='utf-8')
print(json.dumps({'report':'Correction/Lighting/REPORT.md','gallery':'Review/Lighting.html','unity_tests':test.attrib['passed'],'render_layouts':len(layouts)}))
