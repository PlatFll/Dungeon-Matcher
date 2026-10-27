from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import json,hashlib,re,shutil,subprocess,difflib
F=Path('C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage03'
u=json.loads((R/'QA/Unity.json').read_text());assert u['status']=='PASS'
source=json.loads((F/'ArtSource/IronveinBelowTopHUD/Stage01/SourceLock.json').read_text())
locked={**source['locked_file_hashes'],**source['guide_and_golden_hashes'],**json.loads((R/'QA/ApprovedStage02Lock.json').read_text())}
for path,expected in locked.items(): assert hashlib.sha256((F/path).read_bytes()).hexdigest()==expected,'Protected source changed: '+path
assets=json.loads((R/'QA/Assets.json').read_text())
for a in assets['assets']:
 p=R/'Exports'/(a['name']+'.png');dst=F/'Assets/_Game/Art/IronveinBelowTopHUD/Stage03'/p.name
 assert p.read_bytes()==dst.read_bytes(),p.name+' Unity source copy mismatch'
 metadata=dst.with_suffix('.png.meta').read_text();assert 'spritePixelsToUnits: 100' in metadata
separator=Image.new('L',(360,640))
S=F/'ArtSource/IronveinBelowTopHUD/Stage02/Exports'
for name,x,y in [('BeamCenter',160,245),('EndTransition',47,245),('EndTransitionRight',303,245),('CrystalNode',171,242)]:
 im=Image.open(S/(name+'.png')).convert('RGBA');separator.paste(im.getchannel('A'),(x,y))
for x0,x1 in [(53,160),(200,307)]:
 im=Image.open(S/'BeamRepeat.png').convert('RGBA').getchannel('A')
 for x in range(x0,x1,24): separator.paste(im.crop((0,0,min(24,x1-x),10)),(x,246))
records=[]
for w in [360,720,1080]:
 a=Image.open(R/f'Preview/Before-{w}x{w*16//9}.png').convert('RGB');b=Image.open(R/f'Preview/Unity-{w}x{w*16//9}.png').convert('RGB')
 assert a.size==b.size
 assert a.crop((0,0,w,242*w//360)).tobytes()==b.crop((0,0,w,242*w//360)).tobytes()
 mask=separator.resize(a.size,Image.Resampling.NEAREST);masked=ImageChops.multiply(ImageChops.difference(a,b),Image.merge('RGB',(mask,mask,mask)))
 assert not masked.getbbox(),'Approved separator pixels changed'
 records.append({'width':w,'difference_bounds':ImageChops.difference(a,b).getbbox(),'locked_top_exact':True,'separator_opaque_pixels_exact':True})
before=Image.open(R/'Preview/Before-360x640.png').convert('RGBA');after=Image.open(R/'Preview/Unity-360x640.png').convert('RGBA')
comparison=Image.new('RGBA',(748,676),'#171b25');d=ImageDraw.Draw(comparison);d.text((10,8),'Before: approved Stage 02',fill='white');d.text((382,8),'Stage 03: modular board frame',fill='white');comparison.alpha_composite(before,(8,28));comparison.alpha_composite(after,(380,28));comparison.save(R/'Preview/Comparison.png')
detail=Image.new('RGBA',(988,652),'#171b25');d=ImageDraw.Draw(detail)
for n,im in enumerate([before,after]):
 crop=im.crop((20,242,340,280));crop=crop.resize((960,114),Image.Resampling.NEAREST);d.text((8,n*326+8),('Before' if n==0 else 'Stage 03')+' - top corners / approved separator',fill='white');detail.alpha_composite(crop,(8,n*326+30))
 d.text((8,n*326+164),'Bottom corners / board edge',fill='white');crop=im.crop((20,518,340,556)).resize((960,114),Image.Resampling.NEAREST);detail.alpha_composite(crop,(8,n*326+186))
detail.save(R/'Preview/FrameDetails.png')
def docs(p):return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!\d+ &(\d+)\n.*?(?=^--- !u!|\Z)','\n'.join(line.rstrip() for line in p.read_text().splitlines())+'\n',re.M|re.S)}
a=docs(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage02.unity');b=docs(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage03.unity')
changes=[{'id':k,'before':a[k],'after':b[k]} for k in a.keys()&b.keys() if a[k]!=b[k]]
assert not a.keys()-b.keys();assert len(b.keys()-a.keys())==12;assert len(changes)==16
(R/'QA/SceneDelta.json').write_text(json.dumps({'added_ids':list(b.keys()-a.keys()),'removed_ids':list(a.keys()-b.keys()),'changed_record_ids':[c['id'] for c in changes],'interpretation':'8 frame Images + 4 connector Images + 1 fitter sprite reference + 3 connector child links. Added records are the 3 small amber overlays (4 components each).'},indent=2))
(R/'QA/SceneDelta.patch').write_text(''.join(''.join(difflib.unified_diff(c['before'].splitlines(True),c['after'].splitlines(True),fromfile='Stage02/'+c['id'],tofile='Stage03/'+c['id'])) for c in changes))
report={'status':'PASS','protected_file_count':len(locked),'approved_sources_unchanged':True,'captures':records,'unity_report':'Unity.json','assets_report':'Assets.json','pixel_lab_generations':10,'remaining_generations':236.2,'stage':3,'no_merge':True,'not_live_gameplay_or_device_test':True,'pixel_mapping':'New UI imports use 100 PPU, matching the existing Canvas. The unchanged native fitter renders all frame parts at uniform 2x in 720x1280. Shared Canvas and runtime code unchanged.'}
(R/'QA/ReviewValidation.json').write_text(json.dumps(report,indent=2))
cards=''.join(f'<article><h3>{a["name"]}</h3><p>{a["size"][0]}×{a["size"][1]} · {a["opaque_colors"]} colors · binary alpha</p><img src="Exports/{a["name"]}.png"><br><img width="{a["size"][0]*4}" src="Exports/{a["name"]}.png"></article>' for a in assets['assets'])
html='''<!doctype html><html lang="en"><meta charset="utf-8"><title>Ironvein · Stage 03 board frame</title><style>body{background:#111721;color:#e4e9ef;font:16px system-ui;margin:24px}p{max-width:1000px;color:#b6c8d6}img{image-rendering:pixelated}button,a{color:#f3c985}button{background:#273442;padding:9px;border:1px solid #526474;border-radius:4px;cursor:pointer}section{overflow:auto;margin:20px 0}.pair{display:flex;gap:20px}.cards{display:grid;grid-template-columns:repeat(auto-fill,minmax(230px,1fr));gap:16px}article{padding:16px;background:#202a38;border-radius:6px}h3{font-size:16px}article p{font-size:13px}.note{padding:15px;border-left:3px solid #c58826;background:#1c2330}</style><h1>Ironvein · Stage 03 board frame</h1><p>Ten separate native modules, made from PixelLab candidate 1 and cleaned in LibreSprite. The approved top, separator, board opening, cells, gems, portrait, side supports and bottom HUD are preserved.</p><p class="note">Actual Unity art-only camera renders. This stage is not installed in the production Game scene. No drills yet; no merge. PixelLab: 10 generations (236.20 remaining).</p><h2>Before / after — native 360×640</h2><section class="pair"><div><p>Approved Stage 02</p><img src="Preview/Before-360x640.png" width="360"></div><div><p>Stage 03 candidate</p><img src="Preview/Unity-360x640.png" width="360"></div></section><h2>Unity render</h2><p><button onclick="show(360)">360×640</button> <button onclick="show(720)">720×1280</button> <button onclick="show(1080)">1080×1920</button></p><section><img id="scene" src="Preview/Unity-720x1280.png" width="720"></section><h2>Frame detail — nearest-neighbor 3×</h2><section><img src="Preview/FrameDetails.png"></section><h2>Native assets / 4× inspection</h2><div class="cards">__CARDS__</div><h2>Original PixelLab alternatives</h2><section><img src="Preview/RawCandidates.png"></section><p><a href="QA/Assets.json">Native pixel and palette checks</a> · <a href="QA/Unity.json">Unity geometry and protected-pixel checks</a> · <a href="QA/ReviewValidation.json">Source preservation and usage</a></p><script>function show(w){let im=document.getElementById('scene');im.src='Preview/Unity-'+w+'x'+(w*16/9)+'.png';im.width=w}</script></html>'''.replace('__CARDS__',cards)
(R/'Review.html').write_text(html,encoding='utf-8')
shutil.copyfile(__file__,R/'Scripts/ValidateAndReview.py')
print(json.dumps({'status':'PASS','protected_files':len(locked),'scene_added_records':len(b.keys()-a.keys()),'removed_records':len(a.keys()-b.keys()),'changed_records':len(changes),'captured_sizes':[360,720,1080]}))
