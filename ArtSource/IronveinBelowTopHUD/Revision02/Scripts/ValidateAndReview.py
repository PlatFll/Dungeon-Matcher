from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,subprocess,re,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Revision02'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
report=json.loads((R/'QA/Unity.json').read_text());assert report['status']=='PASS'
locks=json.loads((R/'QA/ApprovedSourceLock.json').read_text())
for n in ['SlotDisabled','SlotHighlighted','SlotPressed']:
 rel=f'Assets/_Game/Resources/UI/Consumables/{n}.png.meta';p=F/rel
 if rel in locks and sha(p)!=locks[rel]:
  old=subprocess.check_output(['git','show','HEAD:'+rel],cwd=F);norm=lambda b:'\n'.join(s.rstrip() for s in b.decode().splitlines())
  assert norm(old)==norm(p.read_bytes()),'Unexpected metadata change: '+rel
  lf=old.replace(b'\r\n',b'\n');original=next((b for b in [old,lf,lf.replace(b'\n',b'\r\n')] if hashlib.sha256(b).hexdigest()==locks[rel]),None)
  assert original is not None;p.write_bytes(original)
for p,h in locks.items():assert sha(F/p)==h,'Original changed: '+p
assets=[]
for p in sorted((R/'Exports').glob('*.png')):
 im=Image.open(p).convert('RGBA');rt=Image.open(R/'Roundtrip'/p.name).convert('RGBA')
 assert im.size==rt.size and im.tobytes()==rt.tobytes();assert im.getchannel('A').getextrema()==(255,255)
 assert p.read_bytes()==(F/'Assets/_Game/Art/IronveinBelowTopHUD/Revision02'/p.name).read_bytes()
 assert im.crop((0,0,1,im.height)).tobytes()==im.crop((im.width-1,0,im.width,im.height)).tobytes()
 assets.append(dict(name=p.stem,size=im.size,opaque_colors=len(set(im.get_flattened_data())),binary_alpha=True,roundtrip_exact=True,sha256=sha(p)))
assert len(assets)==4
for prefix in ['FloorSurface','SeparatorBlock']:
 a=Image.open(R/'Exports'/(prefix+'A.png'));b=Image.open(R/'Exports'/(prefix+'B.png'))
 assert a.crop((0,0,1,a.height)).tobytes()==b.crop((0,0,1,b.height)).tobytes()
(R/'QA/Assets.json').write_text(json.dumps(dict(status='PASS',assets=assets,variant_seams='exact',protected_files=len(locks)),indent=2))
def records(p):return {m.group(1):'\n'.join(x.rstrip() for x in m.group(2).splitlines()) for m in re.finditer(r'^--- !u!\d+ &(\d+)\s*\n(.*?)(?=^--- !u!|\Z)',p.read_text(),re.M|re.S)}
a=records(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision01.unity');b=records(F/report['scene']);assert a.keys()<=b.keys();changes=[]
for k,v in a.items():
 if v==b[k]:continue
 if v.replace('m_IsActive: 1','m_IsActive: 0')==b[k]:kind='Hide old floor group only'
 elif v.startswith('RectTransform:') and re.sub(r'  m_Children:.*?(?=  m_Father:)','children\n',v,flags=re.S)==re.sub(r'  m_Children:.*?(?=  m_Father:)','children\n',b[k],flags=re.S):kind='Append native floor revision group'
 else:raise AssertionError('Unexpected existing record change '+k)
 changes.append(dict(id=k,kind=kind))
assert len(changes)==2 and sum(c['kind']=='Hide old floor group only' for c in changes)==1
(R/'QA/SceneDelta.json').write_text(json.dumps(dict(status='PASS',original_records_changed=changes,records_added=len(b)-len(a),records_removed=0),indent=2))
before=Image.open(R/'Preview/Before-720x1280.png').convert('RGBA');after=Image.open(R/'Preview/Unity-720x1280.png').convert('RGBA')
# Restriction check is independent of the Unity rectangle assertion.
for w in [360,720,1080]:
 x=Image.open(R/f'Preview/Before-{w}x{w*16//9}.png').convert('RGBA');y=Image.open(R/f'Preview/Unity-{w}x{w*16//9}.png').convert('RGBA')
 for box in [(0,0,w,186*w//360),(0,225*w//360,w,x.height)]:assert x.crop(box).tobytes()==y.crop(box).tobytes(),'Pixels outside platform changed'
close=Image.new('RGBA',(1020,330),'#171b25');d=ImageDraw.Draw(close)
for px,label,im in [(10,'Before',before),(520,'Revised - deeper dark floor / block separator',after)]:
 d.text((px,8),label,fill='white');close.alpha_composite(im.crop((220,280,720,475)),(px,30))
 d.text((px,240),'Actors / HP / board remain fixed',fill='#bdcbd6')
close.save(R/'Preview/PlatformComparison.png')
full=Image.new('RGBA',(748,676),'#171b25');d=ImageDraw.Draw(full);d.text((10,8),'Before - Revision01',fill='white');d.text((382,8),'Revised - cave floor and stone blocks',fill='white');full.alpha_composite(Image.open(R/'Preview/Before-360x640.png').convert('RGBA'),(8,28));full.alpha_composite(Image.open(R/'Preview/Unity-360x640.png').convert('RGBA'),(380,28));full.save(R/'Preview/Comparison.png')
cards=''.join(f'<article><h3>{a["name"]}</h3><p>{a["size"][0]} x {a["size"][1]} / {a["opaque_colors"]} colors</p><img src="Exports/{a["name"]}.png"><br><img width="{a["size"][0]*4}" src="Exports/{a["name"]}.png"></article>' for a in assets)
html='''<!doctype html><meta charset="utf-8"><title>Ironvein - cave floor and block separator</title><style>body{background:#111721;color:#e4e9ef;font:16px system-ui;margin:24px}p{max-width:1100px;color:#bdcbd6}img{image-rendering:pixelated}section{overflow:auto;margin:22px 0}button,a{color:#f3c985}button{background:#273442;padding:10px;cursor:pointer;border:1px solid #526474}.cards{display:flex;gap:18px;flex-wrap:wrap}article{background:#202a38;padding:16px}</style><h1>Ironvein - cave floor and block separator</h1><p>The standing surface now reaches farther back into the cave and uses the cave's near-black slate colors. The front separator is a row of broad, lighter charcoal stone blocks with clear joints and restrained top edges. Enemy feet, health bars, board and other artwork stay fixed.</p><h2>Platform close-up</h2><section><img src="Preview/PlatformComparison.png"></section><h2>Actual Unity preview</h2><p><button onclick="show(360)">360 x 640</button> <button onclick="show(720)">720 x 1280</button> <button onclick="show(1080)">1080 x 1920</button></p><section><img id="scene" src="Preview/Unity-720x1280.png" width="720"></section><h2>Full before / after</h2><section><img src="Preview/Comparison.png"></section><h2>Native modules - 1x and 4x</h2><div class="cards">__CARDS__</div><p>Direct LibreSprite editing of the existing PixelLab floor source plus modular native stone blocks. No new PixelLab generations. Prior scenes/assets remain intact. Four native exports match their reopened ASE files and Unity imports; binary alpha, exact repeat edges, Point filtering and no compression.</p><p>Unity image checks at three sizes preserve every pixel outside the platform band and all protected opaque actor/UI pixels. Original Graphic geometry and sprite references are unchanged. This is an art-only Unity preview, not production integration or phone testing. No merge.</p><p><a href="QA/Unity.json">Unity checks</a> / <a href="QA/Assets.json">Native checks</a> / <a href="QA/SceneDelta.json">Scene comparison</a> / <a href="QA/Validation.json">Validation</a> / <a href="../Revision01/Review.html">Previous revision</a></p><script>function show(w){let im=document.getElementById('scene');im.src='Preview/Unity-'+w+'x'+w*16/9+'.png';im.width=w}</script>'''.replace('__CARDS__',cards)
(R/'Review.html').write_text(html,encoding='utf-8');(R.parent/'Review.html').write_text('<!doctype html><meta http-equiv="refresh" content="0;url=Revision02/Review.html"><a href="Revision02/Review.html">Current Ironvein review</a>')
(R/'README.md').write_text('''# Revision02 - cave-dark floor and charcoal block separator

User-directed native edit of Revision01. The walking plane is 20 native pixels deep instead of 8, extending its rear edge from y=198 to y=186. The existing feet, front edge at y=206 and separator bottom at y=225 stay fixed. Its cool near-black colors are sampled from the current CavernDepth source. Two 40x19 charcoal block variants make the front separator clearly readable. No scaling or interpolation.

All prior source artwork and scenes remain intact. Only the old floor group is hidden in the new art-only Unity scene. Existing characters, UI, board, lower crystal formations, other assets and gameplay remain unchanged. The review scene is excluded from builds. No new PixelLab spending.

Native editing occurred in LibreSprite, with ASE sources and exact reopened PNG roundtrips. Unity checks at 360x640, 720x1280 and 1080x1920 preserve original Graphic geometry and references, all protected opaque UI/actors, and all pixels outside the authorized platform band. Independent checks cover binary alpha, exact variant seams, original file hashes and serialized changes. See QA/Validation.json for the required Unity validator result. No live combat or device test is claimed.

Open Review.html or Preview/PlatformComparison.png. Stop for visual review. Do not merge.
''',encoding='utf-8')
shutil.copyfile(__file__,R/'Scripts/ValidateAndReview.py')
print(json.dumps(dict(status='PASS',protected_files=len(locks),assets=4,scene_changes=changes,pixellab_generations=0)))
