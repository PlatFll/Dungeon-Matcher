from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import json,hashlib,re,shutil,difflib
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage06'
u=json.loads((R/'QA/Unity.json').read_text());assert u['status']=='PASS'
locked=json.loads((R/'QA/ApprovedSourceLock.json').read_text())
for p,h in locked.items():assert hashlib.sha256((F/p).read_bytes()).hexdigest()==h,'Protected source changed: '+p
assets=json.loads((R/'QA/Assets.json').read_text());assert assets['status']=='PASS'
for a in assets['assets']:
 src=R/'Exports'/(a['name']+'.png');dst=F/'Assets/_Game/Art/IronveinBelowTopHUD/Stage06'/src.name
 assert src.read_bytes()==dst.read_bytes(),src.name
 assert 'spritePixelsToUnits: 100' in dst.with_suffix('.png.meta').read_text()
records=[]
for w in [360,720,1080]:
 a=Image.open(R/f'Preview/Before-{w}x{w*16//9}.png').convert('RGB');b=Image.open(R/f'Preview/Unity-{w}x{w*16//9}.png').convert('RGB');assert a.size==b.size
 for box in [(0,0,w,570*w//360),(72*w//360,618*w//360,288*w//360,a.height)]:assert a.crop(box).tobytes()==b.crop(box).tobytes(),(w,box)
 mask=Image.open(R/f'Preview/ProtectedMask-{w}x{w*16//9}.png').getchannel('A').point(lambda p:255 if p==255 else 0)
 assert not ImageChops.multiply(ImageChops.difference(a,b),Image.merge('RGB',(mask,mask,mask))).getbbox(),'Approved opaque art changed'
 records.append({'width':w,'difference_bounds':ImageChops.difference(a,b).getbbox(),'top_board_energy_bar_exact':True,'bottom_text_exact':True,'approved_opaque_UI_and_environment_exact':True})
before=Image.open(R/'Preview/Before-360x640.png').convert('RGBA');after=Image.open(R/'Preview/Unity-360x640.png').convert('RGBA')
comparison=Image.new('RGBA',(748,676),'#171b25');d=ImageDraw.Draw(comparison)
d.text((10,8),'Before: approved Stage 05',fill='white');d.text((382,8),'Stage 06: bottom-HUD seating',fill='white')
comparison.alpha_composite(before,(8,28));comparison.alpha_composite(after,(380,28));comparison.save(R/'Preview/Comparison.png')
detail=Image.new('RGBA',(1460,740),'#171b25');d=ImageDraw.Draw(detail)
for n,prefix in enumerate(['Before','Unity']):
 im=Image.open(R/f'Preview/{prefix}-720x1280.png').convert('RGBA')
 d.text((12,n*370+6),'Before' if n==0 else 'Stage 06 - original UI on timber / iron supports',fill='white')
 crop=im.crop((0,1108,720,1280)).resize((1440,344),Image.Resampling.NEAREST);detail.alpha_composite(crop,(10,n*370+22))
detail.save(R/'Preview/HUDDetails.png')
def docs(p):return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!\d+ &(\d+)\n.*?(?=^--- !u!|\Z)','\n'.join(x.rstrip() for x in p.read_text().splitlines())+'\n',re.M|re.S)}
a=docs(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage05.unity');b=docs(F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage06.unity')
changes=[k for k in a.keys()&b.keys() if a[k]!=b[k]];added=b.keys()-a.keys()
assert not a.keys()-b.keys();assert len(added)==42,(len(added),'unexpected new scene records');assert len(changes)==1,changes
for k in changes:
 delta=list(difflib.ndiff(a[k].splitlines(),b[k].splitlines()));removed=[v for v in delta if v.startswith('- ')];inserted=[v[2:] for v in delta if v.startswith('+ ')]
 assert not removed and len(inserted)==1 and 'fileID:' in inserted[0],delta
(R/'QA/SceneDelta.json').write_text(json.dumps({'added_records':len(added),'removed_records':0,'changed_record_ids':changes,'interpretation':'One Canvas child link added for 10 new native support Images. Every existing object record is unchanged.'},indent=2))
(R/'QA/SceneDelta.patch').write_text(''.join(''.join(difflib.unified_diff(a[k].splitlines(True),b[k].splitlines(True),fromfile='Stage05/'+k,tofile='Stage06/'+k)) for k in changes))
report={'status':'PASS','stage':6,'protected_file_count':len(locked),'approved_sources_unchanged':True,'captures':records,'unity_report':'Unity.json','assets_report':'Assets.json','pixel_lab_generations':10,'remaining_generations':192.2,'no_merge':True,'not_live_gameplay_or_device_test':True,'artistic_verdict':'A restrained timber cradle seats the existing controls, with two solid forged-iron corbels and tiny warm rivets. Existing HUD frames, icons, energy bar and instructions remain readable and unchanged. No extra dangling props or scene redesign.'}
(R/'QA/ReviewValidation.json').write_text(json.dumps(report,indent=2))
cards=''.join(f'<article><h3>{a["name"]}</h3><p>{a["size"][0]} × {a["size"][1]} · {a["opaque_colors"]} colors · binary alpha</p><img src="Exports/{a["name"]}.png"><br><img width="{a["size"][0]*4}" src="Exports/{a["name"]}.png"></article>' for a in assets['assets'])
html='''<!doctype html><html lang="en"><meta charset="utf-8"><title>Ironvein · Stage 06 HUD seating</title><style>body{background:#111721;color:#e4e9ef;font:16px system-ui;margin:24px}p{max-width:1050px;color:#b6c8d6}img{image-rendering:pixelated}button,a{color:#f3c985}button{background:#273442;padding:9px;border:1px solid #526474;border-radius:4px;cursor:pointer}section{overflow:auto;margin:20px 0}.pair{display:flex;gap:20px}.cards{display:grid;grid-template-columns:repeat(auto-fill,minmax(240px,1fr));gap:16px}article{padding:16px;background:#202a38;border-radius:6px}h3{font-size:16px}.note{padding:15px;border-left:3px solid #c58826;background:#1c2330}</style><h1>Ironvein · Stage 06 bottom-HUD seating</h1><p>Timber rails and iron corbels sit behind the unchanged potion, resonance and bomb controls. Small warm rivets tie them to the mine frame without adding clutter.</p><p class="note">Actual Unity art-only renders; production Game unchanged. Stage 06 only. All approved source art, top, board, HUD graphics and layout are preserved. No merge. PixelLab: 10 generations spent; 192.20 remain. No image uploads.</p><h2>Bottom HUD · before and after, 2× Unity capture</h2><section><img src="Preview/HUDDetails.png"></section><h2>Full composition · native 360×640</h2><section class="pair"><div><p>Approved Stage 05</p><img src="Preview/Before-360x640.png" width="360"></div><div><p>Stage 06 candidate</p><img src="Preview/Unity-360x640.png" width="360"></div></section><h2>Unity render</h2><p><button onclick="show(360)">360×640</button> <button onclick="show(720)">720×1280</button> <button onclick="show(1080)">1080×1920</button></p><section><img id="scene" src="Preview/Unity-720x1280.png" width="720"></section><h2>Native assets / 4× inspection</h2><div class="cards">__CARDS__</div><details><summary>Retained PixelLab alternatives</summary><section><img src="Preview/BracketCandidates.png"></section></details><p><a href="QA/Assets.json">Native checks</a> · <a href="QA/Unity.json">Unity checks</a> · <a href="QA/Validation.json">Final validation</a> · <a href="Generation.json">Generation receipts and selection</a></p><script>function show(w){let im=document.getElementById('scene');im.src='Preview/Unity-'+w+'x'+(w*16/9)+'.png';im.width=w}</script></html>'''.replace('__CARDS__',cards)
(R/'Review.html').write_text(html,encoding='utf-8')
target=R/'Scripts/ValidateAndReview.py'
if Path(__file__).resolve()!=target.resolve():shutil.copyfile(__file__,target)
print(json.dumps({'status':'PASS','protected_files':len(locked),'added_records':len(added),'sizes':[360,720,1080]}))
