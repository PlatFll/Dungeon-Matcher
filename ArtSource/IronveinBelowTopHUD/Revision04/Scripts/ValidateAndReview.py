from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,subprocess,re,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Revision04'
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
def records(p):return {m.group(1):'\n'.join(x.rstrip() for x in m.group(2).splitlines()) for m in re.finditer(r'^--- !u!\d+ &(\d+)\s*\n(.*?)(?=^--- !u!|\Z)',p.read_text(),re.M|re.S)}
a=records(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision03.unity');b=records(F/report['scene']);assert a.keys()==b.keys();changes=[]
shift=report['shiftedNativePixels'];assert shift>0
for k,v in a.items():
 if v==b[k]:continue
 assert v.startswith('RectTransform:'),'Unexpected non-transform record changed '+k
 go=re.search(r'm_GameObject: \{fileID: (\d+)\}',v).group(1);name=re.search(r'm_Name: (.+)',a[go]).group(1)
 assert re.fullmatch(r'(WalkingSlab[0-5]|StoneBlock[0-8])',name),name
 expected=v
 # Unity serializes UI translation in anchoredPosition; localPosition remains unchanged.
 for field in ['m_AnchoredPosition']:
  pattern=rf'({field}: \{{x: [^,]+, y: )([^,}}]+)';old=float(re.search(pattern,v).group(2));new=float(re.search(pattern,b[k]).group(2))
  assert new==old-shift,(name,field,old,new)
  expected=re.sub(pattern,lambda m:m.group(1)+re.search(pattern,b[k]).group(2),expected)
 assert expected==b[k],'Additional transform change '+name
 changes.append(dict(id=k,name=name,native_y_delta=-shift))
assert len(changes)==15
(R/'QA/SceneDelta.json').write_text(json.dumps(dict(status='PASS',original_records_changed=changes,records_added=0,records_removed=0,actor_and_UI_records_exact=True),indent=2))
top=174;bottom=int(report['separatorBottom']);checks=[]
assert abs((report['floorTop']+report['floorBottom'])/2-report['enemyFloorNative'])<=.5
for w in [360,720,1080]:
 x=Image.open(R/f'Preview/Before-{w}x{w*16//9}.png').convert('RGBA');y=Image.open(R/f'Preview/Unity-{w}x{w*16//9}.png').convert('RGBA')
 for box in [(0,0,w,top*w//360),(0,bottom*w//360,w,x.height)]:assert x.crop(box).tobytes()==y.crop(box).tobytes(),'Pixels outside alignment band changed'
 checks.append(dict(width=w,outside_alignment_band_exact=True))
(R/'QA/Independent.json').write_text(json.dumps(dict(status='PASS',protected_files=len(locks),source_art_exact=True,feet_centered=True,checks=checks,pixellab_generations=0),indent=2))
before=Image.open(R/'Preview/Before-720x1280.png').convert('RGBA');after=Image.open(R/'Preview/Unity-720x1280.png').convert('RGBA')
close=Image.new('RGBA',(1030,282),'#171b25');d=ImageDraw.Draw(close)
for px,label,im in [(10,'Before - feet at the front edge',before),(525,'Aligned - feet centered / separator below',after)]:
 d.text((px,8),label,fill='white');close.alpha_composite(im.crop((220,280,720,490)),(px,30))
 d.text((px,256),'Unchanged artwork / actors / health bars / board',fill='#bdcbd6')
close.save(R/'Preview/PlatformComparison.png')
full=Image.new('RGBA',(748,676),'#171b25');d=ImageDraw.Draw(full);d.text((10,8),'Before - Revision03',fill='white');d.text((382,8),'Aligned - floor and separator only',fill='white');full.alpha_composite(Image.open(R/'Preview/Before-360x640.png').convert('RGBA'),(8,28));full.alpha_composite(Image.open(R/'Preview/Unity-360x640.png').convert('RGBA'),(380,28));full.save(R/'Preview/Comparison.png')
ref=Image.new('RGBA',(510,528),'#171b25');d=ImageDraw.Draw(ref)
for i,(name,im) in enumerate([('Dungeon - existing production capture',Image.open(F/'ArtSource/DrownedCourtEnvironmentV2/Integration/After/dungeon-720x1280.png').convert('RGBA')),('Court - existing production capture',Image.open(F/'ArtSource/DrownedCourtEnvironmentV2/Integration/After/drowned-court-720x1280.png').convert('RGBA')),('Ironvein - new Unity art preview',after)]):
 y=i*176;d.text((5,y+3),name,fill='white');ref.alpha_composite(im.crop((220,340,720,490)),(5,y+22))
ref.save(R/'Preview/ZoneLayoutComparison.png')
html='''<!doctype html><meta charset="utf-8"><title>Ironvein - centered combat floor</title><style>body{background:#111721;color:#e4e9ef;font:16px system-ui;margin:24px}p{max-width:1100px;color:#bdcbd6}img{image-rendering:pixelated}section{overflow:auto;margin:22px 0}button,a{color:#f3c985}button{background:#273442;padding:10px;cursor:pointer;border:1px solid #526474}</style><h1>Ironvein - centered combat floor</h1><p>The approved 32-pixel walking floor is now centered on the existing enemy standing line. The approved block separator sits directly below the floor. This follows the other zones: scenery is positioned around the shared enemy/health-bar baseline. Characters, health bars, countdowns, weakness gems, board, portrait and all source artwork remain unchanged.</p><h2>Actual Unity before / after</h2><section><img src="Preview/PlatformComparison.png"></section><h2>Full Unity preview</h2><p><button onclick="show(360)">360 x 640</button> <button onclick="show(720)">720 x 1280</button> <button onclick="show(1080)">1080 x 1920</button></p><section><img id="scene" src="Preview/Unity-720x1280.png" width="720"></section><h2>Other-zone layout references</h2><p>Dungeon and Court are existing production captures; Ironvein is the new static art-only Unity render.</p><section><img src="Preview/ZoneLayoutComparison.png"></section><h2>Full before / after</h2><section><img src="Preview/Comparison.png"></section><p>No artwork edits, new generations, scaling or interpolation. Only six floor and nine separator rectangles moved by __SHIFT__ whole native pixels. Original source scenes and production Game remain untouched. No merge.</p><p>Validation checks all original sprite/color/state bindings, exact actor/UI geometry and opaque pixels, centered floor registration, a gap-free floor-to-separator join, and clearance above the board rim. Three render sizes: 360x640, 720x1280, 1080x1920.</p><p><a href="QA/Unity.json">Unity checks</a> / <a href="QA/Independent.json">Independent checks</a> / <a href="QA/SceneDelta.json">Scene delta</a> / <a href="QA/Validation.json">Validation</a> / <a href="../Revision03/Review.html">Previous review</a></p><script>function show(w){let im=document.getElementById('scene');im.src='Preview/Unity-'+w+'x'+w*16/9+'.png';im.width=w}</script>'''.replace('__SHIFT__',str(shift))
(R/'Review.html').write_text(html,encoding='utf-8');(R.parent/'Review.html').write_text('<!doctype html><meta http-equiv="refresh" content="0;url=Revision04/Review.html"><a href="Revision04/Review.html">Current Ironvein review</a>')
(R/'README.md').write_text(f'''# Revision04 - centered walking floor and separator alignment

The previous floor ended only a few native pixels in front of the enemies, leaving their feet at its edge. The current user request authorizes correcting that alignment. Dungeon, Forest and Court bind scenery around the common actor/health-bar floor; see QA/LayoutReferences.json. This revision applies that ownership to the Ironvein review.

The approved floor and separator placements move down {shift} native pixels as complete sprites. Walking floor: y{report['floorTop']}..{report['floorBottom']}, measured enemy floor: y{report['enemyFloorNative']}, separator: y{report['floorBottom']}..{report['separatorBottom']}. The actor feet are centered, and the separator meets the floor below. No artwork, colors, scale, sprite reference or original active state changes. Actors, health bars, countdowns, weakness icons, board and all controls retain exact geometry.

Only fifteen existing RectTransforms move in the copied art-only scene. No objects added/removed; source scenes, approved art and production Game remain untouched. Zero generation spending. This stage performs layout only, so it reuses the prior native LibreSprite sources without editing them.

Actual static Unity captures at 360x640, 720x1280 and 1080x1920; independent source hashes and serialized comparison; required compiler result in QA/Validation.json. These are not live combat/device tests or production integration. Review.html contains the before/after and other-zone layout comparisons. Stop for approval; do not merge.
''',encoding='utf-8')
shutil.copyfile(__file__,R/'Scripts/ValidateAndReview.py')
print(json.dumps(dict(status='PASS',protected_files=len(locks),changed_rects=15,shift=shift,floor=[report['floorTop'],report['floorBottom']],feet=report['enemyFloorNative'],separator=[report['floorBottom'],report['separatorBottom']],artwork_changes=0,generations=0)))
