from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,subprocess,re,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Revision03'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
report=json.loads((R/'QA/Unity.json').read_text());assert report['status']=='PASS'
locks=json.loads((R/'QA/ApprovedSourceLock.json').read_text())
# Unity rewrites trailing whitespace in three existing importer metadata files.
# Restore only when semantic equality AND the exact starting hash are established.
for n in ['SlotDisabled','SlotHighlighted','SlotPressed']:
 rel=f'Assets/_Game/Resources/UI/Consumables/{n}.png.meta';p=F/rel
 if rel in locks and sha(p)!=locks[rel]:
  old=subprocess.check_output(['git','show','HEAD:'+rel],cwd=F);norm=lambda b:'\n'.join(s.rstrip() for s in b.decode().splitlines())
  assert norm(old)==norm(p.read_bytes()),'Unexpected metadata change: '+rel
  lf=old.replace(b'\r\n',b'\n');original=next((b for b in [old,lf,lf.replace(b'\n',b'\r\n')] if hashlib.sha256(b).hexdigest()==locks[rel]),None)
  assert original is not None;p.write_bytes(original)
for p,h in locks.items():assert sha(F/p)==h,'Original changed: '+p
assets=[];images=[]
for p in sorted((R/'Exports').glob('*.png')):
 im=Image.open(p).convert('RGBA');rt=Image.open(R/'Roundtrip'/p.name).convert('RGBA')
 assert im.size==(64,32) and im.tobytes()==rt.tobytes();assert im.getchannel('A').getextrema()==(255,255)
 assert p.read_bytes()==(F/'Assets/_Game/Art/IronveinBelowTopHUD/Revision03'/p.name).read_bytes()
 assert im.crop((0,0,1,32)).tobytes()==im.crop((63,0,64,32)).tobytes()
 images.append(im)
 assets.append(dict(name=p.stem,size=im.size,opaque_colors=len(set(im.get_flattened_data())),binary_alpha=True,roundtrip_exact=True,sha256=sha(p)))
assert len(assets)==3
for im in images:assert im.crop((0,0,1,32)).tobytes()==images[0].crop((0,0,1,32)).tobytes()
(R/'QA/Assets.json').write_text(json.dumps(dict(status='PASS',assets=assets,variant_seams='exact',protected_files=len(locks)),indent=2))
def records(p):return {m.group(1):'\n'.join(x.rstrip() for x in m.group(2).splitlines()) for m in re.finditer(r'^--- !u!\d+ &(\d+)\s*\n(.*?)(?=^--- !u!|\Z)',p.read_text(),re.M|re.S)}
a=records(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision02.unity');b=records(F/report['scene']);assert a.keys()<=b.keys();changes=[]
for k,v in a.items():
 if v==b[k]:continue
 if v.replace('m_IsActive: 1','m_IsActive: 0')==b[k] and re.search(r'm_Name: WalkingSurface\d+\n',v):kind='Hide prior walking tile only'
 elif v.startswith('RectTransform:') and re.sub(r'  m_Children:.*?(?=  m_Father:)','children\n',v,flags=re.S)==re.sub(r'  m_Children:.*?(?=  m_Father:)','children\n',b[k],flags=re.S):kind='Append native floor group'
 else:raise AssertionError('Unexpected existing record change '+k)
 changes.append(dict(id=k,kind=kind))
assert len(changes)==13 and sum(c['kind']=='Hide prior walking tile only' for c in changes)==12
(R/'QA/SceneDelta.json').write_text(json.dumps(dict(status='PASS',original_records_changed=changes,records_added=len(b)-len(a),records_removed=0),indent=2))
pixelchecks=[]
for w in [360,720,1080]:
 x=Image.open(R/f'Preview/Before-{w}x{w*16//9}.png').convert('RGBA');y=Image.open(R/f'Preview/Unity-{w}x{w*16//9}.png').convert('RGBA')
 for box in [(0,0,w,174*w//360),(0,206*w//360,w,x.height)]:assert x.crop(box).tobytes()==y.crop(box).tobytes(),'Pixels outside walking surface changed'
 box=(0,206*w//360,w,225*w//360);assert x.crop(box).tobytes()==y.crop(box).tobytes(),'Approved separator changed'
 pixelchecks.append(dict(width=w,outside_walking_surface_exact=True,approved_separator_exact=True))
(R/'QA/PixelScope.json').write_text(json.dumps(dict(status='PASS',checks=pixelchecks),indent=2))
before=Image.open(R/'Preview/Before-720x1280.png').convert('RGBA');after=Image.open(R/'Preview/Unity-720x1280.png').convert('RGBA')
close=Image.new('RGBA',(1030,260),'#171b25');d=ImageDraw.Draw(close)
for px,label,im in [(10,'Before',before),(525,'Revised - broad cave-stone walking surface',after)]:
 d.text((px,8),label,fill='white');close.alpha_composite(im.crop((220,280,720,475)),(px,30))
 d.text((px,238),'Exact same enemy floor line / separator / health bars',fill='#bdcbd6')
close.save(R/'Preview/PlatformComparison.png')
full=Image.new('RGBA',(748,676),'#171b25');d=ImageDraw.Draw(full);d.text((10,8),'Before - Revision02',fill='white');d.text((382,8),'Revised - walking surface only',fill='white');full.alpha_composite(Image.open(R/'Preview/Before-360x640.png').convert('RGBA'),(8,28));full.alpha_composite(Image.open(R/'Preview/Unity-360x640.png').convert('RGBA'),(380,28));full.save(R/'Preview/Comparison.png')
cards=''.join(f'<article><h3>{a["name"]}</h3><p>64 x 32 / {a["opaque_colors"]} colors</p><img src="Exports/{a["name"]}.png"><br><img width="256" src="Exports/{a["name"]}.png"></article>' for a in assets)
html='''<!doctype html><meta charset="utf-8"><title>Ironvein - broad walking floor</title><style>body{background:#111721;color:#e4e9ef;font:16px system-ui;margin:24px}p{max-width:1100px;color:#bdcbd6}img{image-rendering:pixelated}section{overflow:auto;margin:22px 0}button,a{color:#f3c985}button{background:#273442;padding:10px;cursor:pointer;border:1px solid #526474}.cards{display:flex;gap:18px;flex-wrap:wrap}article{background:#202a38;padding:16px}</style><h1>Ironvein - broad walking floor</h1><p>Three 64 x 32 cave-stone slab tiles use the approved underwater floor's broad plane and restrained joints. The surface extends farther behind the enemies. Their exact floor line, the approved separator and all UI remain fixed.</p><h2>Actual Unity close-up</h2><section><img src="Preview/PlatformComparison.png"></section><h2>Actual Unity preview</h2><p><button onclick="show(360)">360 x 640</button> <button onclick="show(720)">720 x 1280</button> <button onclick="show(1080)">1080 x 1920</button></p><section><img id="scene" src="Preview/Unity-720x1280.png" width="720"></section><h2>Full before / after</h2><section><img src="Preview/Comparison.png"></section><h2>Native modules - 1x and 4x</h2><div class="cards">__CARDS__</div><p>Native LibreSprite edits on copies of the approved PixelLab-derived Drowned Court floor. Zero new generations. The original underwater artwork, Ironvein separator and prior scenes are preserved. No scaling or interpolation in these tiles.</p><p>Unity checks at three sizes preserve original Graphic geometry and sprite references, protected opaque actor/UI pixels, the entire separator and every pixel outside the walking-surface band. This is an art-only Unity preview, not production integration or phone testing. No merge.</p><p><a href="QA/Unity.json">Unity checks</a> / <a href="QA/Assets.json">Native checks</a> / <a href="QA/PixelScope.json">Pixel scope</a> / <a href="QA/SceneDelta.json">Scene comparison</a> / <a href="QA/Validation.json">Validation</a> / <a href="../Revision02/Review.html">Previous revision</a></p><script>function show(w){let im=document.getElementById('scene');im.src='Preview/Unity-'+w+'x'+w*16/9+'.png';im.width=w}</script>'''.replace('__CARDS__',cards)
(R/'Review.html').write_text(html,encoding='utf-8');(R.parent/'Review.html').write_text('<!doctype html><meta http-equiv="refresh" content="0;url=Revision03/Review.html"><a href="Revision03/Review.html">Current Ironvein review</a>')
(R/'README.md').write_text('''# Revision03 - broad walking surface

Only the walking floor is revised. The user explicitly chose to keep the exact enemy floor line. The separator at native y206..225, actors, health bars, board and every other original placement remain unchanged. The walking plane extends backward from y186 to y174 and remains fixed at y206 in front. There are approximately three native pixels between the current feet and separator; this revision deliberately preserves that constraint.

Three 64x32 modular slabs join the approved Drowned Court rear/front floor courses, map their five material colors to cave slate, and normalize the outer repeat seams in LibreSprite. Actual input PNG hashes and palette edits are recorded in Lineage.json and Scripts/Cleanup.js. No fresh PixelLab generation and no resampling. ASE sources, exported PNGs and reopened roundtrips are included.

The new art-only Unity scene copies Revision02, disables only its twelve old walking tiles, and adds six tile placements. The last tile clips to the screen width without stretching. The original separator group stays active. Source scenes, source art and production Game remain untouched. Preview scene is excluded from builds.

Executed checks: QA/Unity.json, Assets.json, SceneDelta.json and PixelScope.json. Captures at 360x640, 720x1280 and 1080x1920 are actual static Unity camera renders, not live combat or device testing. Required validator result is recorded separately in QA/Validation.json. Stop for visual review; do not merge.
''',encoding='utf-8')
shutil.copyfile(__file__,R/'Scripts/ValidateAndReview.py')
print(json.dumps(dict(status='PASS',protected_files=len(locks),assets=3,original_scene_records_changed=len(changes),separator_pixels='exact',enemy_floor_line='exact',pixellab_generations=0)))
