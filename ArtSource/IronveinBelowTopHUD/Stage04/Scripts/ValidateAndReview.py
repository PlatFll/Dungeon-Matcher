from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import json,hashlib,re,shutil,difflib
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage04'
u=json.loads((R/'QA/Unity.json').read_text());assert u['status']=='PASS'
locked=json.loads((R/'QA/ApprovedSourceLock.json').read_text())
for p,h in locked.items():assert hashlib.sha256((F/p).read_bytes()).hexdigest()==h,'Protected source changed: '+p
assets=json.loads((R/'QA/Assets.json').read_text());assert assets['status']=='PASS'
for a in assets['assets']:
 src=R/'Exports'/(a['name']+'.png');dst=F/'Assets/_Game/Art/IronveinBelowTopHUD/Stage04'/src.name
 assert src.read_bytes()==dst.read_bytes(),src.name
 meta=dst.with_suffix('.png.meta').read_text();assert 'spritePixelsToUnits: 100' in meta
records=[]
for w in [360,720,1080]:
 a=Image.open(R/f'Preview/Before-{w}x{w*16//9}.png').convert('RGB');b=Image.open(R/f'Preview/Unity-{w}x{w*16//9}.png').convert('RGB')
 assert a.size==b.size
 # Full top and separator area, and everything below the last drill, stay exact.
 for box in [(0,0,w,284*w//360),(0,510*w//360,w,a.height)]:assert a.crop(box).tobytes()==b.crop(box).tobytes(),(w,box)
 records.append({'width':w,'difference_bounds':ImageChops.difference(a,b).getbbox(),'top_and_separator_exact':True,'lower_hud_exact':True})
before=Image.open(R/'Preview/Before-360x640.png').convert('RGBA');after=Image.open(R/'Preview/Unity-360x640.png').convert('RGBA')
comparison=Image.new('RGBA',(748,676),'#171b25');d=ImageDraw.Draw(comparison)
d.text((10,8),'Before: approved Stage 03',fill='white');d.text((382,8),'Stage 04: mounted drill modules',fill='white')
comparison.alpha_composite(before,(8,28));comparison.alpha_composite(after,(380,28));comparison.save(R/'Preview/Comparison.png')
detail=Image.new('RGBA',(984,600),'#171b25');d=ImageDraw.Draw(detail)
for n,im in enumerate([before,after]):
 d.text((12+n*492,8),('Before' if n==0 else 'Stage 04')+' - left / right board sides',fill='white')
 for side,(x1,x2) in enumerate([(0,58),(302,360)]):
  crop=im.crop((x1,258,x2,536)).resize((116,556),Image.Resampling.NEAREST)
  detail.alpha_composite(crop,(20+n*492+side*240,30))
detail.save(R/'Preview/DrillDetails.png')
def docs(p):return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!\d+ &(\d+)\n.*?(?=^--- !u!|\Z)','\n'.join(x.rstrip() for x in p.read_text().splitlines())+'\n',re.M|re.S)}
a=docs(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage03.unity');b=docs(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage04.unity')
changes=[k for k in a.keys()&b.keys() if a[k]!=b[k]];added=b.keys()-a.keys()
assert not a.keys()-b.keys();assert len(added)==98,(len(added),'unexpected new scene objects');assert len(changes)==1,changes
old=a[changes[0]].splitlines();new=b[changes[0]].splitlines();delta=list(difflib.ndiff(old,new))
removed=[v for v in delta if v.startswith('- ')];inserted=[v[2:] for v in delta if v.startswith('+ ')]
assert not removed and len(inserted)==1 and 'fileID:' in inserted[0],delta
(R/'QA/SceneDelta.json').write_text(json.dumps({'added_records':len(added),'removed_records':0,'changed_record_ids':changes,'interpretation':'Only the existing Canvas child list gains one group link; 24 new module Images and their parent group. No existing art or transform record changes.'},indent=2))
(R/'QA/SceneDelta.patch').write_text(''.join(difflib.unified_diff(a[changes[0]].splitlines(True),b[changes[0]].splitlines(True),fromfile='Stage03/Canvas',tofile='Stage04/Canvas')))
report={'status':'PASS','stage':4,'protected_file_count':len(locked),'approved_sources_unchanged':True,'captures':records,'unity_report':'Unity.json','assets_report':'Assets.json','pixel_lab_generations':12,'remaining_generations':224.2,'no_merge':True,'not_live_gameplay_or_device_test':True,'artistic_verdict':'Six compact inward drills attach to the rail through top and bottom mounting straps. Broad steel planes and small amber cores preserve readability. No floating fragments or grid overlap. Previous side stone/lantern sources are retained for their separate Stage05 review.'}
(R/'QA/ReviewValidation.json').write_text(json.dumps(report,indent=2))
cards=''.join(f'<article><h3>{a["name"]}</h3><p>{a["size"][0]}×{a["size"][1]} · {a["opaque_colors"]} colors · binary alpha</p><img src="Exports/{a["name"]}.png"><br><img width="{a["size"][0]*4}" src="Exports/{a["name"]}.png"></article>' for a in assets['assets'])
html='''<!doctype html><html lang="en"><meta charset="utf-8"><title>Ironvein · Stage 04 drill modules</title><style>body{background:#111721;color:#e4e9ef;font:16px system-ui;margin:24px}p{max-width:1000px;color:#b6c8d6}img{image-rendering:pixelated}button,a{color:#f3c985}button{background:#273442;padding:9px;border:1px solid #526474;border-radius:4px;cursor:pointer}section{overflow:auto;margin:20px 0}.pair{display:flex;gap:20px}.cards{display:grid;grid-template-columns:repeat(auto-fill,minmax(230px,1fr));gap:16px}article{padding:16px;background:#202a38;border-radius:6px}h3{font-size:16px}article p{font-size:13px}.note{padding:15px;border-left:3px solid #c58826;background:#1c2330}</style><h1>Ironvein · Stage 04 drill modules</h1><p>Three inward-facing drills on each side, mounted to the existing board rails. Seven native pieces combine mounting plates, motors, spiral noses and amber power blocks. The approved top, separator, frame sources, grid and shared HUD are preserved.</p><p class="note">Actual Unity art-only camera renders, not the production Game scene or a device test. Stage 04 only; no merge. PixelLab: 12 subscription generations spent, 224.20 remaining. No image uploads.</p><h2>Before / after — native 360×640</h2><section class="pair"><div><p>Approved Stage 03</p><img src="Preview/Before-360x640.png" width="360"></div><div><p>Stage 04 candidate</p><img src="Preview/Unity-360x640.png" width="360"></div></section><h2>Unity render</h2><p><button onclick="show(360)">360×640</button> <button onclick="show(720)">720×1280</button> <button onclick="show(1080)">1080×1920</button></p><section><img id="scene" src="Preview/Unity-720x1280.png" width="720"></section><h2>Mount assemblies — native and 5×</h2><section><img src="Preview/Assemblies.png"></section><h2>Frame contact and clearance — nearest-neighbor 2×</h2><section><img src="Preview/DrillDetails.png"></section><h2>Native modules / 4× inspection</h2><div class="cards">__CARDS__</div><h2>Raw source and cleanup</h2><section class="pair"><article><h3>Selected PixelLab Pro candidate 17</h3><img src="Raw/ProDrill-17.png"><br><img src="Raw/ProDrill-17.png" width="192"><p>The raw left wraparound fragment was discarded. The clipped bit tip was completed on its separate canvas. No resampling.</p></article><article><h3>Cleaned and mounted</h3><img src="Preview/AssemblyLeft.png"><br><img src="Preview/AssemblyLeft.png" width="240"><p>Native LibreSprite extraction, material cleanup, connected mounting plate and separate amber inset.</p></article></section><details><summary>All retained PixelLab alternatives and rejected first attempts</summary><section><img src="Preview/ProCandidates.png"></section><section><img src="Preview/PixfluxAttempts.png"></section></details><p><a href="QA/Assets.json">Pixel/roundtrip checks</a> · <a href="QA/Unity.json">Unity protected-pixel checks</a> · <a href="QA/Validation.json">Final validation</a> · <a href="Generation.json">Generation receipts</a></p><script>function show(w){let im=document.getElementById('scene');im.src='Preview/Unity-'+w+'x'+(w*16/9)+'.png';im.width=w}</script></html>'''.replace('__CARDS__',cards)
(R/'Review.html').write_text(html,encoding='utf-8');shutil.copyfile(__file__,R/'Scripts/ValidateAndReview.py')
print(json.dumps({'status':'PASS','protected_files':len(locked),'added_records':len(added),'existing_records_changed':len(changes),'sizes':[360,720,1080]}))
