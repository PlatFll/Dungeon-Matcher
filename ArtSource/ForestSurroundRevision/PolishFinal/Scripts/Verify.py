from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
import json,hashlib,shutil,xml.etree.ElementTree as ET,re,difflib
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/PolishFinal';A=F/'Assets/_Game/Art/ForestSurroundRevision/PolishFinal';T=F/'.utmp/forest-polish-final'
def parts(im):
 pts={(x,y)for y in range(im.height)for x in range(im.width)if im.getpixel((x,y))[3]};sizes=[]
 while pts:
  todo=[pts.pop()];n=0
  while todo:
   x,y=todo.pop();n+=1
   for dx in [-1,0,1]:
    for dy in [-1,0,1]:
     q=(x+dx,y+dy)
     if q in pts:pts.remove(q);todo.append(q)
  sizes.append(n)
 return sorted(sizes,reverse=True)
records=[]
for p in sorted(A.glob('*.png')):
 im=Image.open(p).convert('RGBA');name=p.stem;b=im.getbbox()
 assert im.tobytes()==Image.open(T/(name+'-plan.png')).convert('RGBA').tobytes()==Image.open(T/'reopened'/p.name).convert('RGBA').tobytes(),name
 assert b[0]>=2 and b[1]>=2 and b[2]<=im.width-2 and b[3]<=im.height-2,(name,b)
 assert set(im.getchannel('A').get_flattened_data())=={0,255},name
 assert len(parts(im))==1,(name,parts(im))
 shutil.copy2(p,R/'Exports'/p.name)
 records.append(dict(name=name,dimensions=im.size,bounds=b,colors=len({c for c in im.get_flattened_data()if c[3]}),alpha=[0,255],components=parts(im),source_reopen_exact=True,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
print('Native modules:',len(records),'exact LibreSprite round trips, binary alpha, single silhouettes and clear edges.')
if not(R/'Diagnostics/UnityTests.xml').exists():raise SystemExit('Unity render evidence not ready yet.')
t=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert t.attrib['result']=='Passed'and t.attrib['failed']=='0','Unity render test failed'
b=Image.open(R/'Previews/Unity-Before-720x1360.png').convert('RGB');a=Image.open(R/'Previews/Unity-Final-720x1360.png').convert('RGB')
assert b.tobytes()==Image.open(R/'Previews/Before.png').convert('RGB').tobytes(),'Before capture changed'
large=Image.open(R/'Previews/Unity-Final-1440x2720.png').convert('RGB');assert large.tobytes()==a.resize(large.size,Image.Resampling.NEAREST).tobytes(),'Pixel doubling changed'
hud=Image.open(F/'ArtSource/ForestSurroundRevision/UnifiedCorners/Diagnostics/ProtectedHudPixels.png').convert('L')
frame=Image.new('L',b.size)
for q in json.loads((F/'ArtSource/EnchantedForestStaged/Stage04/Diagnostics/Unity-720x1360.json').read_text())['images']:
 if not q['frame']:continue
 rr=q['rect'];x,y=round(rr['x']),1360-round(rr['y']+rr['height']);sw,sh=round(rr['width']),round(rr['height']);factor=round(q['scale'])
 src=Image.open(F/q['path']).getchannel('A');src=src.resize((src.width*factor,src.height*factor),Image.Resampling.NEAREST)
 if q['type']=='Tiled':
  region=Image.new('L',(sw,sh))
  for yy in range(0,sh,src.height):
   for xx in range(0,sw,src.width):region.paste(src,(xx,yy))
 else:assert src.size==(sw,sh);region=src
 frame.paste(region,(x,y),region)
allowed=Image.new('L',b.size);wisps=Image.new('L',b.size)
def add(mask,q,folder):
 im=Image.open(folder/(q['file']+'.png')).getchannel('A');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);mask.paste(im,(q['x']*2,q['y']*2),im)
for q in json.loads((F/'ArtSource/ForestSurroundRevision/PolishStage02/Scripts/Placements.json').read_text())['pieces']:add(allowed,q,F/'Assets/_Game/Art/ForestSurroundRevision/PolishStage02')
for q in json.loads((F/'ArtSource/ForestSurroundRevision/UnifiedCorners/Scripts/Placements.json').read_text())['pieces']:
 if not q['file'].startswith('Stone_'):add(allowed,q,F/'Assets/_Game/Art/ForestSurroundRevision/UnifiedCorners')
placements=json.loads((R/'Scripts/Placements.json').read_text())['pieces']
for q in placements:
 if q['stage']==5:
  for offset in [-1,0,1]:
   qq=dict(q,y=q['y']+offset);add(wisps,qq,A);add(allowed,qq,A)
 else:add(allowed,q,A)
def assert_locked(im):
 diff=ImageChops.difference(b,im)
 for box in [(0,0,720,500),(104,528,616,1040),(236,1160,484,1248),(262,1100,458,1136),(220,1278,500,1308)]:assert b.crop(box).tobytes()==im.crop(box).tobytes(),('Locked region changed',box)
 for title,mask in [('HUD',hud),('board frame',frame)]:assert ImageChops.multiply(diff,Image.merge('RGB',(mask,)*3)).getbbox()is None,title+' pixels changed'
 off=ImageChops.invert(allowed);assert ImageChops.multiply(diff,Image.merge('RGB',(off,)*3)).getbbox()is None,'Outside authorized dressing'
 return diff.getbbox()
stageboxes={}
for stage in [3,4,5]:stageboxes[stage]=assert_locked(Image.open(R/'Previews'/('Unity-Stage'+str(stage)+'-720x1360.png')).convert('RGB'))
finalbox=assert_locked(a);motion=[]
for n in range(12):
 im=Image.open(R/'Previews'/('Unity-Motion-'+str(n).zfill(2)+'.png')).convert('RGB');assert_locked(im)
 diff=ImageChops.difference(a,im);off=ImageChops.invert(wisps);assert ImageChops.multiply(diff,Image.merge('RGB',(off,)*3)).getbbox()is None,'Motion altered scenery or UI';motion.append(im)
assert len({im.tobytes()for im in motion})>1
hashes=json.loads((R/'Diagnostics/PreservedFiles.json').read_text());changed=[p for p,h in hashes.items()if hashlib.sha256((F/p).read_bytes()).hexdigest()!=h];assert not changed,changed
evidence=json.loads((R/'Diagnostics/Unity-720x1360.json').read_text());m=json.loads((R/'Diagnostics/Motion.json').read_text());assert all(m[k]for k in ['reducedMotionStable','loopCloses','animatedStatesDiffer'])
report=dict(native_modules=records,unchanged_existing_files=len(hashes),unchanged_image_bindings=evidence['preservedImages'],unity_compile_and_review='Passed',pixel_doubling_exact=True,top_hud_battle_separator_unchanged=True,board_and_frame_unchanged=True,bottom_hud_and_text_unchanged=True,changes_outside_dressing=0,stage_changed_bounds=stageboxes,final_changed_bounds=finalbox,motion=m,production_integrated=False,real_device_test=False)
(R/'Diagnostics/Validation.json').write_text(json.dumps(report,indent=2))
comparison=Image.new('RGB',(1448,912),'#17221d');d=ImageDraw.Draw(comparison);d.text((8,8),'BEFORE - approved Stage 2',fill='white');d.text((736,8),'FINAL - stages 3 to 6',fill='white');comparison.paste(b.crop((0,480,720,1360)),(0,32));comparison.paste(a.crop((0,480,720,1360)),(728,32));comparison.save(R/'Previews/Unity-BeforeAfter.png')
lower=Image.new('RGB',(1448,412),'#17221d');d=ImageDraw.Draw(lower);d.text((8,8),'BEFORE',fill='white');d.text((736,8),'FINAL',fill='white');lower.paste(b.crop((0,980,720,1360)),(0,32));lower.paste(a.crop((0,980,720,1360)),(728,32));lower.save(R/'Previews/Unity-BeforeAfter-Lower.png')
palette=a.quantize(colors=256,method=Image.Quantize.MEDIANCUT);frames=[im.quantize(palette=palette,dither=Image.Dither.NONE)for im in motion];frames[0].save(R/'Previews/Unity-Wisps.gif',save_all=True,append_images=frames[1:],duration=500,loop=0,optimize=False,disposal=2)
print(json.dumps({k:v for k,v in report.items()if k!='native_modules'},indent=2))
if Path(__file__).resolve()!=(R/'Scripts/Verify.py').resolve():shutil.copy2(__file__,R/'Scripts/Verify.py')
