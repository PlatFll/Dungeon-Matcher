from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import json,hashlib,re,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage07';base=R.parent
report=json.loads((R/'QA/Assembly.json').read_text());assert report['status']=='PASS'
locks=json.loads((R/'QA/ApprovedSourceLock.json').read_text())
for p,h in locks.items():assert hashlib.sha256((F/p).read_bytes()).hexdigest()==h,'Protected source changed: '+p
assets=[];groups={};allcolors=set()
for stage in range(2,7):
 name=f'Stage{stage:02}';rows=[]
 for p in sorted((base/name/'Exports').glob('*.png')):
  im=Image.open(p).convert('RGBA');alpha=set(im.getchannel('A').get_flattened_data());assert alpha<={0,255}
  rt=Image.open(base/name/'Roundtrip'/p.name).convert('RGBA');assert im.size==rt.size and im.tobytes()==rt.tobytes()
  asset=F/'Assets/_Game/Art/IronveinBelowTopHUD'/name/p.name;assert asset.read_bytes()==p.read_bytes()
  cols={c[:3] for c in im.get_flattened_data() if c[3]};allcolors|=cols
  row={'stage':stage,'name':p.stem,'size':im.size,'bounds':im.getbbox(),'opaque_colors':len(cols),'binary_alpha':True,'source':p.relative_to(F).as_posix(),'unity_asset':asset.relative_to(F).as_posix(),'ase':(base/name/'Native'/(p.stem+'.ase')).relative_to(F).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'roundtrip_exact':True}
  assert (F/row['ase']).exists();assets.append(row);rows.append(row)
 groups[name]=rows
assert len(assets)==36
seams=[]
for stage,name,axis in [(2,'BeamRepeat','x'),(5,'TimberRepeat','y'),(5,'RockCourse','x'),(6,'SeatRail','x'),(6,'SeatPost','y')]:
 im=Image.open(base/f'Stage{stage:02}'/'Exports'/(name+'.png')).convert('RGBA');w,h=im.size
 first=im.crop((0,0,1,h) if axis=='x' else (0,0,w,1));last=im.crop((w-1,0,w,h) if axis=='x' else (0,h-1,w,h))
 assert first.tobytes()==last.tobytes(),name+' repeat seam'
 seams.append({'stage':stage,'name':name,'axis':axis,'edge_pixels_exact':True})
(R/'QA/Assets.json').write_text(json.dumps({'status':'PASS','assets':assets,'asset_count':36,'no_asset_edits':True,'repeat_seams':seams,'palette_union':['#%02x%02x%02x'%c for c in sorted(allcolors)]},indent=2))
def normalize(p):return '\n'.join(x.rstrip() for x in p.read_text().splitlines())+'\n'
scene=F/report['scene'];old=F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage06.unity'
assert normalize(scene)==normalize(old),'Stage07 must preserve every approved scene record'
(R/'QA/SceneDelta.json').write_text(json.dumps({'status':'PASS','existing_records_changed':0,'records_added':0,'records_removed':0,'new_scene_uses_same_approved_serialized_assembly':True,'note':'Dedicated review scene receives its own Unity asset GUID; all object records match Stage06.'},indent=2))
captures=[]
for w in [360,720,1080]:
 initial=Image.open(R/f'Preview/Initial-{w}x{w*16//9}.png').convert('RGBA');approved=Image.open(R/f'Preview/Approved-{w}x{w*16//9}.png').convert('RGBA');final=Image.open(R/f'Preview/Unity-{w}x{w*16//9}.png').convert('RGBA')
 assert approved.tobytes()==final.tobytes()
 assert initial.crop((0,0,w,242*w//360)).tobytes()==final.crop((0,0,w,242*w//360)).tobytes()
 captures.append({'width':w,'height':final.height,'approved_stage06_pixels_exact':True,'initial_top_exact':True})
before=Image.open(R/'Preview/Initial-360x640.png').convert('RGBA');after=Image.open(R/'Preview/Unity-360x640.png').convert('RGBA')
comparison=Image.new('RGBA',(748,676),'#171b25');d=ImageDraw.Draw(comparison)
d.text((10,8),'Initial lower-half foundation',fill='white');d.text((382,8),'Stage 07 - approved modules assembled',fill='white')
comparison.alpha_composite(before,(8,28));comparison.alpha_composite(after,(380,28));comparison.save(R/'Preview/Comparison.png')
full=Image.open(R/'Preview/Unity-720x1280.png').convert('RGBA')
detail=Image.new('RGBA',(1030,700),'#171b25');d=ImageDraw.Draw(detail)
for title,box,pos in [('Separator / upper join',(40,480,680,550),(12,28)),('Left drill rail',(0,566,90,1020),(12,200)),('Right drill rail',(630,566,720,1020),(116,200)),('Bottom frame / rock course',(40,1048,680,1118),(12,118)),('HUD seating',(90,1136,630,1272),(250,240))]:
 d.text((pos[0],pos[1]-16),title,fill='white');detail.alpha_composite(full.crop(box),pos)
detail.save(R/'Preview/AssemblyDetails.png')
cards=[]
titles={'Stage02':'Separator', 'Stage03':'Board frame','Stage04':'Drill modules','Stage05':'Surroundings','Stage06':'HUD supports'}
for group,rows in groups.items():
 cards.append('<h3>'+titles[group]+'</h3><div class="cards">')
 for a in rows:cards.append(f'<article><h4>{a["name"]}</h4><p>{a["size"][0]} × {a["size"][1]} · {a["opaque_colors"]} colors</p><img src="../{group}/Exports/{a["name"]}.png"><br><img width="{a["size"][0]*3}" src="../{group}/Exports/{a["name"]}.png"></article>')
 cards.append('</div>')
html='''<!doctype html><html lang="en"><meta charset="utf-8"><title>Ironvein · Stage 07 full assembly</title><style>body{background:#111721;color:#e4e9ef;font:16px system-ui;margin:24px}p{max-width:1050px;color:#bdcbd6}img{image-rendering:pixelated}button,a{color:#f3c985}button{background:#273442;padding:9px;border:1px solid #526474;border-radius:4px;cursor:pointer}section{overflow:auto;margin:20px 0}.pair{display:flex;gap:20px}.cards{display:grid;grid-template-columns:repeat(auto-fill,minmax(250px,1fr));gap:14px}article{padding:16px;background:#202a38;border-radius:6px}h4{margin:0}.note{padding:15px;border-left:3px solid #c58826;background:#1c2330}</style><h1>Ironvein · Stage 07 full assembly</h1><p>All 36 approved native modules assembled: reinforced separator, timber-and-iron board frame, six inward-facing drills, mine-rock surroundings and the bottom-HUD timber cradle.</p><p class="note">Actual Unity art-only renders. Stage 07 reuses the approved Stage 06 composition exactly; no approved artwork or placements were changed. Top HUD, 64 gems/cells, characters and shared controls remain untouched. No new generations, no production integration, no merge.</p><h2>Full Unity preview</h2><p><button onclick="show(360)">360×640</button> <button onclick="show(720)">720×1280</button> <button onclick="show(1080)">1080×1920</button></p><section><img id="scene" src="Preview/Unity-720x1280.png" width="720"></section><h2>Starting foundation versus complete assembly</h2><section><img src="Preview/Comparison.png"></section><h2>Assembly close-ups</h2><section><img src="Preview/AssemblyDetails.png"></section><h2>Assembly checks</h2><p>Frame joints share their rail centerlines. Three evenly spaced drill pairs meet the rails; occupied drill pixels clear the gem grid. All native modules use uniform pixel scale and Point/uncompressed/unmipped imports. HUD supports render behind the original UI. All 36 native exports match their Unity copies and LibreSprite roundtrips. The existing shared layout and its sub-native edge rounding are preserved.</p><p>All rendered pixels match approved Stage 06 at each resolution. The top and gem grid also match the original Stage 01 foundation. These checks cover art-only Unity rendering, not live gameplay, safe areas or a phone build.</p><details><summary>All 36 native assets · 1× and 3×</summary>__CARDS__</details><p><a href="QA/Assembly.json">Unity assembly measurements</a> · <a href="QA/Assets.json">Native asset register</a> · <a href="QA/Validation.json">Verification report</a> · <a href="../Stage06/Review.html">Approved Stage 06</a></p><script>function show(w){let im=document.getElementById('scene');im.src='Preview/Unity-'+w+'x'+(w*16/9)+'.png';im.width=w}</script></html>'''.replace('__CARDS__',''.join(cards))
(R/'Review.html').write_text(html,encoding='utf-8')
(R/'QA/ReviewValidation.json').write_text(json.dumps({'status':'PASS','stage':7,'protected_files':len(locks),'assets':36,'scene_records_unchanged':True,'captures':captures,'new_generations':0,'cumulative_pack_generations':74,'artistic_verdict':'The approved lower families form one restrained mine assembly. Wood carries the frame and drills, iron reads as mounting hardware, amber stays an accent, and dark rock supports the sides. The HUD cradle ties the lower controls into the setting. No additional art or placement changes were needed.','limitations':['Art-only static preview, not production Game or device testing.','Existing shared board/UI layout is retained, including its fractional geometry. New native environment pieces preserve their uniform pixel scale.'],'stop':'Stage07 review; do not merge or start Stage08 automatically'},indent=2))
target=R/'Scripts/ValidateAndReview.py'
if Path(__file__).resolve()!=target.resolve():shutil.copyfile(__file__,target)
print(json.dumps({'status':'PASS','protected_files':len(locks),'assets':len(assets),'scene_records_changed':0,'new_generations':0}))
