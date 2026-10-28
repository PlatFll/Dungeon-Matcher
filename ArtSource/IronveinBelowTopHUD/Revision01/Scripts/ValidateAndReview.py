from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
import json,hashlib,shutil,re,subprocess
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Revision01'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
report=json.loads((R/'QA/Unity.json').read_text());assert report['status']=='PASS'
locks=json.loads((R/'QA/ApprovedSourceLock.json').read_text())
# Unity rewrites whitespace in three unrelated UI metadata files. Only recover
# those exact files if their content differs solely in trailing whitespace.
for n in ['SlotDisabled','SlotHighlighted','SlotPressed']:
 rel=f'Assets/_Game/Resources/UI/Consumables/{n}.png.meta';p=F/rel
 if rel in locks and sha(p)!=locks[rel]:
  old=subprocess.check_output(['git','show','HEAD:'+rel],cwd=F)
  normal=lambda b:'\n'.join(s.rstrip() for s in b.decode().splitlines())
  assert normal(old)==normal(p.read_bytes()),'Unexpected semantic metadata change: '+rel
  lf=old.replace(b'\r\n',b'\n');forms=[old,lf,lf.replace(b'\n',b'\r\n')]
  original=next((v for v in forms if hashlib.sha256(v).hexdigest()==locks[rel]),None)
  assert original is not None,'Unable to restore exact original newline convention: '+rel
  p.write_bytes(original)
for p,h in locks.items():assert sha(F/p)==h,'Original source changed: '+p
assets=[]
for p in sorted((R/'Exports').glob('*.png')):
 im=Image.open(p).convert('RGBA');rt=Image.open(R/'Roundtrip'/p.name).convert('RGBA')
 assert im.size==rt.size and im.tobytes()==rt.tobytes(),'LibreSprite roundtrip mismatch'
 assert set(im.getchannel('A').get_flattened_data())<={0,255}
 imported=F/'Assets/_Game/Art/IronveinBelowTopHUD/Revision01'/p.name
 assert imported.read_bytes()==p.read_bytes()
 if p.stem.startswith('Floor'):
  assert im.getchannel('A').getextrema()==(255,255)
  assert im.crop((0,0,1,im.height)).tobytes()==im.crop((im.width-1,0,im.width,im.height)).tobytes()
 else:
  b=im.getbbox();assert b[0]>0 and b[1]>0 and b[2]<im.width and b[3]<im.height,'Crystal clipped'
 assets.append(dict(name=p.stem,size=im.size,bounds=im.getbbox(),colors=len({c[:3] for c in im.get_flattened_data() if c[3]}),binary_alpha=True,roundtrip_exact=True,sha256=sha(p)))
for prefix in ['FloorSurface','FloorFace']:
 a=Image.open(R/'Exports'/(prefix+'A.png'));b=Image.open(R/'Exports'/(prefix+'B.png'))
 assert a.crop((0,0,1,a.height)).tobytes()==b.crop((0,0,1,b.height)).tobytes(),'Variant seam mismatch'
assert len(assets)==6
(R/'QA/Assets.json').write_text(json.dumps(dict(status='PASS',assets=assets,repeat_variant_seams='exact',protected_source_files=len(locks)),indent=2))

old=F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage07.unity';new=F/report['scene']
def records(p):return {m.group(1):'\n'.join(s.rstrip() for s in m.group(2).splitlines()) for m in re.finditer(r'^--- !u!\d+ &(\d+)\s*\n(.*?)(?=^--- !u!|\Z)',p.read_text(),re.M|re.S)}
a,b=records(old),records(new);assert a.keys()<=b.keys(),'No original serialized object may be deleted'
changes=[]
for k,v in a.items():
 if v==b[k]:continue
 if v.replace('m_IsActive: 1','m_IsActive: 0')==b[k]:kind='Authorized hidden placement'
 elif v.startswith('RectTransform:') and re.sub(r'  m_Children:.*?(?=  m_Father:)', '  m_Children: preserved plus new groups\n',v,flags=re.S)==re.sub(r'  m_Children:.*?(?=  m_Father:)','  m_Children: preserved plus new groups\n',b[k],flags=re.S):kind='Root child list: add modular revision layers'
 else:raise AssertionError('Unexpected original serialized record change '+k)
 changes.append(dict(id=k,kind=kind))
assert sum(c['kind']=='Authorized hidden placement' for c in changes)==8
(R/'QA/SceneDelta.json').write_text(json.dumps(dict(status='PASS',existing_records_changed=changes,records_added=len(b)-len(a),records_removed=0),indent=2))
g=json.loads((R/'Generation.json').read_text());g['ending_balance']=166.20;g['actual_usage']=26;g['jobs'][0]['charged']=1;g['jobs'][1]['charged']=10;g['jobs'][2]['charged']=15;g['accounting']='192.20 before minus 166.20 after = 26 generations. Includes rejected standard floor.';g['image_uploads']=0
(R/'Generation.json').write_text(json.dumps(g,indent=2))
before=Image.open(R/'Preview/Before-360x640.png').convert('RGBA');after=Image.open(R/'Preview/Unity-360x640.png').convert('RGBA')
board=Image.new('RGBA',(748,676),'#171b25');d=ImageDraw.Draw(board);d.text((10,8),'Before - Stage 07 assembly',fill='white');d.text((382,8),'Revised - floor and grounded amber',fill='white');board.alpha_composite(before,(8,28));board.alpha_composite(after,(380,28));board.save(R/'Preview/Comparison.png')
details=Image.new('RGBA',(1460,760),'#171b25');d=ImageDraw.Draw(details)
for title,box,y in [('Combat floor and cleared top',(230,210,720,475),30),('Grounded lower crystals',(0,1090,720,1280),390)]:
 d.text((8,y-20),title+' - before',fill='white');d.text((738,y-20),title+' - revised',fill='white')
 for x,prefix in [(8,'Before'),(738,'Unity')]:details.alpha_composite(Image.open(R/f'Preview/{prefix}-720x1280.png').convert('RGBA').crop(box),(x,y))
details.save(R/'Preview/Details.png')
cards=''.join(f'<article><h3>{a["name"]}</h3><p>{a["size"][0]} x {a["size"][1]} / {a["colors"]} colors</p><img src="Exports/{a["name"]}.png"><br><img width="{a["size"][0]*3}" src="Exports/{a["name"]}.png"></article>' for a in assets)
html='''<!doctype html><meta charset="utf-8"><title>Ironvein - focused floor and crystal revision</title><style>body{margin:24px;background:#111721;color:#e4e9ef;font:16px system-ui}p{max-width:1100px;color:#bdcbd6}img{image-rendering:pixelated}section{overflow:auto;margin:22px 0}button,a{color:#f3c985}button{background:#273442;padding:9px;border:1px solid #526474;cursor:pointer}.cards{display:grid;grid-template-columns:repeat(auto-fill,minmax(250px,1fr));gap:14px}article{background:#202a38;padding:16px}.note{padding:15px;border-left:3px solid #c58826;background:#1c2330}</style><h1>Ironvein - focused revision</h1><p>New PixelLab mine-floor tiles, cleaned in LibreSprite. The three upper lanterns, wheel and cart are hidden. Loose amber deposits beside the board are removed from this revision; two distinct large crystal formations grow from the lower corner rocks. Previous source artwork and scenes remain intact.</p><p class="note">Actual Unity art-only renders. Original board, frame, characters, floor alignment and shared UI geometry remain unchanged. Production Game has not been changed or merged.</p><h2>Revised Unity scene</h2><p><button onclick="show(360)">Native 360 x 640</button> <button onclick="show(720)">720 x 1280</button> <button onclick="show(1080)">1080 x 1920</button></p><section><img id="scene" src="Preview/Unity-720x1280.png" width="720"></section><h2>Before / after</h2><section><img src="Preview/Comparison.png"></section><section><img src="Preview/Details.png"></section><h2>New native assets - 1x and 3x</h2><div class="cards">__CARDS__</div><h2>PixelLab lineage</h2><p>26 generations: rejected standard tileset (1), selected Pro tileset (15), crystal source with 16 alternatives (10). Balance 166.20 after the jobs; no uploaded reference images. The final modules retain editable LibreSprite sources and exact PNG roundtrips.</p><details><summary>Raw generation review</summary><h3>Selected floor source</h3><img width="512" src="Raw/FloorProTileset.png"><h3>Crystal alternatives</h3><img src="Preview/CrystalCandidates.png"></details><h2>Verification</h2><p>Three Unity capture sizes; original object geometry/references unchanged; eight specifically authorized placements hidden. Opaque UI, character and grid pixels match the original. Six native assets have binary alpha, clean repeat seams, Point filtering and uncompressed imports. Original source files retain their hashes. These are static Unity preview checks, not live combat or phone testing.</p><p><a href="QA/Unity.json">Unity checks</a> / <a href="QA/Assets.json">Native asset checks</a> / <a href="QA/SceneDelta.json">Serialized scene changes</a> / <a href="../Stage07/Review.html">Previous assembly</a></p><script>function show(w){let im=document.getElementById('scene');im.src='Preview/Unity-'+w+'x'+w*16/9+'.png';im.width=w}</script>'''.replace('__CARDS__',cards)
(R/'Review.html').write_text(html,encoding='utf-8')
(R.parent/'Review.html').write_text('<!doctype html><meta charset="utf-8"><meta http-equiv="refresh" content="0;url=Revision01/Review.html"><a href="Revision01/Review.html">Open the current Ironvein revision</a>')
(R/'README.md').write_text('''# Focused revision - mine floor and lower crystals

User-authorized changes to the Stage07 art-only assembly: replace the upper gray brick platform with a flat native PixelLab mine-floor tileset; hide three upper lanterns, wheel and cart; hide the two loose board-side amber deposits; add two large distinct grounded crystal formations beside the bottom HUD. This explicitly revises the prior top-area lock only for these items.

The cave background, timber, portrait, board frame, actors, gems, cells and shared controls retain their source artwork, placement and geometry. Original scenes and assets are preserved. The new scene is excluded from builds and does not integrate into production Game.

Six native assets retain editable LibreSprite files, exports, exact reopened roundtrips and raw generation alternatives. The floor has separate 32x8 walking-surface and 32x19 bedrock variants. The two crystal assets are 56x64, with transparent clearance. Native placement uses uniform 2x scale in the existing 720-unit reference canvas; no image resampling. Surface begins behind the unchanged enemy feet and retains the previous platform's lower edge.

Unity checks compare before/after at 360x640, 720x1280 and 1080x1920, enforce the restricted change regions, preserve original Graphic geometry/references, and compare protected opaque UI/actors/grid pixels exactly. Source hashes, alpha, tile seams, imported pixels and LibreSprite roundtrips are checked independently. See QA/Unity.json, Assets.json, SceneDelta.json and Validation.json. These are static art-only checks, not live gameplay/device validation.

PixelLab: 26 generations (1 rejected standard floor + 15 selected Pro floor + 10 crystal source). No reference uploads. Review Review.html and Preview/Comparison.png. Stop for visual approval; do not merge.
''',encoding='utf-8')
shutil.copyfile(__file__,R/'Scripts/ValidateAndReview.py')
print(json.dumps(dict(status='PASS',protected_files=len(locks),assets=len(assets),scene_delta=changes,generations=26)))
