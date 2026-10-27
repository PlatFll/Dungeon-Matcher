from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import json,hashlib,re,shutil,difflib
def copy_script(source,target):
 if Path(source).resolve()!=Path(target).resolve():shutil.copyfile(source,target)
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage05'
u=json.loads((R/'QA/Unity.json').read_text());assert u['status']=='PASS'
locked=json.loads((R/'QA/ApprovedSourceLock.json').read_text())
for p,h in locked.items():assert hashlib.sha256((F/p).read_bytes()).hexdigest()==h,'Protected source changed: '+p
assets=json.loads((R/'QA/Assets.json').read_text());assert assets['status']=='PASS'
for a in assets['assets']:
 src=R/'Exports'/(a['name']+'.png');dst=F/'Assets/_Game/Art/IronveinBelowTopHUD/Stage05'/src.name
 assert src.read_bytes()==dst.read_bytes(),src.name
 assert 'spritePixelsToUnits: 100' in dst.with_suffix('.png.meta').read_text()
records=[]
for w in [360,720,1080]:
 a=Image.open(R/f'Preview/Before-{w}x{w*16//9}.png').convert('RGB');b=Image.open(R/f'Preview/Unity-{w}x{w*16//9}.png').convert('RGB')
 assert a.size==b.size
 for box in [(0,0,w,242*w//360),(34*w//360,557*w//360,326*w//360,a.height),(0,574*w//360,w,a.height)]:assert a.crop(box).tobytes()==b.crop(box).tobytes(),(w,box)
 mask=Image.open(R/f'Preview/ProtectedMask-{w}x{w*16//9}.png').getchannel('A').point(lambda p:255 if p==255 else 0)
 assert not ImageChops.multiply(ImageChops.difference(a,b),Image.merge('RGB',(mask,mask,mask))).getbbox(),'Approved opaque art changed'
 records.append({'width':w,'difference_bounds':ImageChops.difference(a,b).getbbox(),'locked_top_exact':True,'lower_hud_exact':True,'approved_opaque_frame_drill_separator_exact':True})
before=Image.open(R/'Preview/Before-360x640.png').convert('RGBA');after=Image.open(R/'Preview/Unity-360x640.png').convert('RGBA')
comparison=Image.new('RGBA',(748,676),'#171b25');d=ImageDraw.Draw(comparison)
d.text((10,8),'Before: approved Stage 04',fill='white');d.text((382,8),'Stage 05: mine supports and rock',fill='white')
comparison.alpha_composite(before,(8,28));comparison.alpha_composite(after,(380,28));comparison.save(R/'Preview/Comparison.png')
detail=Image.new('RGBA',(600,700),'#171b25');d=ImageDraw.Draw(detail)
for n,im in enumerate([before,after]):
 d.text((12+n*300,8),('Before' if n==0 else 'Stage 05')+' - left / right surrounds',fill='white')
 for side,(x1,x2) in enumerate([(0,58),(302,360)]):
  crop=im.crop((x1,242,x2,574)).resize((116,664),Image.Resampling.NEAREST);detail.alpha_composite(crop,(10+n*300+side*144,28))
detail.save(R/'Preview/SupportDetails.png')
def docs(p):return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!\d+ &(\d+)\n.*?(?=^--- !u!|\Z)','\n'.join(x.rstrip() for x in p.read_text().splitlines())+'\n',re.M|re.S)}
a=docs(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage04.unity');b=docs(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage05.unity')
changes=[k for k in a.keys()&b.keys() if a[k]!=b[k]];added=b.keys()-a.keys()
assert not a.keys()-b.keys();assert len(added)==126,(len(added),'unexpected new scene objects');assert len(changes)==15,changes
active=0;canvas=0
for k in changes:
 if b[k]==a[k].replace('  m_IsActive: 1\n','  m_IsActive: 0\n') and '  m_Name: Support' in a[k]:active+=1
 else:
  delta=list(difflib.ndiff(a[k].splitlines(),b[k].splitlines()));removed=[v for v in delta if v.startswith('- ')];inserted=[v[2:] for v in delta if v.startswith('+ ')]
  assert not removed and len(inserted)==1 and 'fileID:' in inserted[0],delta;canvas+=1
assert active==14 and canvas==1
(R/'QA/SceneDelta.json').write_text(json.dumps({'added_records':len(added),'removed_records':0,'hidden_old_support_placements':active,'changed_record_ids':changes,'interpretation':'14 old support placements disabled only in the new scene. Canvas gains one group; 31 separate native Images. All other existing asset/geometry records unchanged.'},indent=2))
(R/'QA/SceneDelta.patch').write_text(''.join(''.join(difflib.unified_diff(a[k].splitlines(True),b[k].splitlines(True),fromfile='Stage04/'+k,tofile='Stage05/'+k)) for k in changes))
report={'status':'PASS','stage':5,'protected_file_count':len(locked),'approved_sources_unchanged':True,'captures':records,'unity_report':'Unity.json','assets_report':'Assets.json','pixel_lab_generations':22,'remaining_generations':202.2,'no_merge':True,'not_live_gameplay_or_device_test':True,'artistic_verdict':'Straight timber carries the approved drill assemblies, with dark irregular mine rock behind it. Two small amber deposits anchor the lower sides. The under-board rock course uses quiet broad planes. Lanterns/chains and existing lower props are retained; bottom-HUD seating awaits Stage06.'}
(R/'QA/ReviewValidation.json').write_text(json.dumps(report,indent=2))
cards=''.join(f'<article><h3>{a["name"]}</h3><p>{a["size"][0]}×{a["size"][1]} · {a["opaque_colors"]} colors · binary alpha</p><img src="Exports/{a["name"]}.png"><br><img width="{a["size"][0]*3}" src="Exports/{a["name"]}.png"></article>' for a in assets['assets'])
html='''<!doctype html><html lang="en"><meta charset="utf-8"><title>Ironvein · Stage 05 surroundings</title><style>body{background:#111721;color:#e4e9ef;font:16px system-ui;margin:24px}p{max-width:1000px;color:#b6c8d6}img{image-rendering:pixelated}button,a{color:#f3c985}button{background:#273442;padding:9px;border:1px solid #526474;border-radius:4px;cursor:pointer}section{overflow:auto;margin:20px 0}.pair{display:flex;gap:20px}.cards{display:grid;grid-template-columns:repeat(auto-fill,minmax(260px,1fr));gap:16px}article{padding:16px;background:#202a38;border-radius:6px}h3{font-size:16px}article p{font-size:13px}.note{padding:15px;border-left:3px solid #c58826;background:#1c2330}</style><h1>Ironvein · Stage 05 mine surroundings</h1><p>Timber posts and iron collars carry the approved drills, with dark mine rock behind them and a low rock course beneath the board. Two small amber deposits anchor the lower sides. Nine separate native pieces; earlier artwork remains recoverable.</p><p class="note">Actual Unity art-only renders. Production Game is unchanged. Top, frame, drills, separator, board and shared HUD are preserved. Stage 05 only; no merge. PixelLab: 22 generations spent; 202.20 remain. No reference-image uploads.</p><h2>Before / after — native 360×640</h2><section class="pair"><div><p>Approved Stage 04</p><img src="Preview/Before-360x640.png" width="360"></div><div><p>Stage 05 candidate</p><img src="Preview/Unity-360x640.png" width="360"></div></section><h2>Unity render</h2><p><button onclick="show(360)">360×640</button> <button onclick="show(720)">720×1280</button> <button onclick="show(1080)">1080×1920</button></p><section><img id="scene" src="Preview/Unity-720x1280.png" width="720"></section><h2>Support detail — nearest-neighbor 2×</h2><section><img src="Preview/SupportDetails.png"></section><h2>Native assets / 3× inspection</h2><div class="cards">__CARDS__</div><details><summary>Retained PixelLab source alternatives</summary><p>Timber candidate 0; rock candidates 5 and 9; single crystal and rock-course sources. The noisy course was simplified into four broad native rock planes in LibreSprite.</p><section><img src="Preview/TimberCandidates.png"></section><section><img src="Preview/RockCandidates.png"></section><section><img src="Preview/CrystalCandidates.png"></section><img src="Raw/Course-00.png" width="384"></details><p><a href="QA/Assets.json">Native pixel and repeat checks</a> · <a href="QA/Unity.json">Unity checks</a> · <a href="QA/Validation.json">Final validation</a> · <a href="Generation.json">Generation receipts</a></p><script>function show(w){let im=document.getElementById('scene');im.src='Preview/Unity-'+w+'x'+(w*16/9)+'.png';im.width=w}</script></html>'''.replace('__CARDS__',cards)
(R/'Review.html').write_text(html,encoding='utf-8');copy_script(__file__,R/'Scripts/ValidateAndReview.py')
print(json.dumps({'status':'PASS','protected_files':len(locked),'added_records':len(added),'old_supports_hidden':active,'sizes':[360,720,1080]}))
