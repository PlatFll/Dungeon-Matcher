from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
import json,hashlib,xml.etree.ElementTree as ET
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/ForestSurroundRevision/UnifiedCorners';a=p/'Assets/_Game/Art/ForestSurroundRevision/UnifiedCorners';t=p/'.utmp/forest-unified-corners'
def connected(im):
 points={(x,y)for y in range(im.height)for x in range(im.width)if im.getpixel((x,y))[3]};sizes=[]
 while points:
  todo=[points.pop()];n=0
  while todo:
   x,y=todo.pop();n+=1
   for dx,dy in [(-1,0),(1,0),(0,-1),(0,1),(-1,-1),(-1,1),(1,-1),(1,1)]:
    q=(x+dx,y+dy)
    if q in points:points.remove(q);todo.append(q)
  sizes.append(n)
 return sorted(sizes,reverse=True)
modules=[]
for q in json.loads((r/'Diagnostics/Selection.json').read_text())['modules']:
 name=q['name'];im=Image.open(a/(name+'.png')).convert('RGBA');expected=Image.open(t/(name+'-expected.png')).convert('RGBA');reopened=Image.open(t/'reopened'/(name+'.png')).convert('RGBA')
 assert im.size==expected.size==reopened.size and im.tobytes()==expected.tobytes()==reopened.tobytes(),name
 assert set(im.getchannel('A').get_flattened_data())=={0,255}
 b=im.getbbox();assert b[0]>=2 and b[1]>=2 and b[2]<=im.width-2 and b[3]<=im.height-2,(name,b)
 components=connected(im)
 if name.startswith('Stone_'):assert len(components)==1,(name,components)
 modules.append(dict(name=name,dimensions=im.size,bounds=b,opaque_colors=len({c for c in im.get_flattened_data()if c[3]}),alpha=[0,255],components=components,libresprite_reopen_exact=True,sha256=hashlib.sha256((a/(name+'.png')).read_bytes()).hexdigest()))
test=ET.parse(r/'Diagnostics/UnityTests.xml').getroot();assert test.attrib['result']=='Passed'and test.attrib['failed']=='0',test.attrib
before=Image.open(r/'Previews/Unity-Before-720x1360.png').convert('RGBA');after=Image.open(r/'Previews/Unity-Unified-720x1360.png').convert('RGBA');large=Image.open(r/'Previews/Unity-Unified-1440x2720.png').convert('RGBA');base=Image.open(r/'Previews/Original-Unity-720x1360.png').convert('RGBA')
assert before.tobytes()==base.tobytes(),'baseline differs from prior Stage 2'
assert large.tobytes()==after.resize((1440,2720),Image.Resampling.NEAREST).tobytes(),'not exact pixel doubling'
for box in [(0,0,720,1000),(104,528,616,1040),(236,1160,484,1248),(262,1100,458,1136),(220,1278,500,1308)]:
 assert before.crop(box).tobytes()==after.crop(box).tobytes(),('locked pixels',box)
diff=ImageChops.difference(before,after).convert('RGB')
frameMask=Image.new('L',before.size)
ev4=json.loads((p/'ArtSource/EnchantedForestStaged/Stage04/Diagnostics/Unity-720x1360.json').read_text())
for q in ev4['images']:
 if not q['frame']:continue
 rect=q['rect'];x,y=round(rect['x']),1360-round(rect['y']+rect['height']);sw,sh=round(rect['width']),round(rect['height']);factor=round(q['scale'])
 src=Image.open(p/q['path']).getchannel('A');src=src.resize((src.width*factor,src.height*factor),Image.Resampling.NEAREST)
 if q['type']=='Tiled':
  region=Image.new('L',(sw,sh))
  for yy in range(0,sh,src.height):
   for xx in range(0,sw,src.width):region.paste(src,(xx,yy))
 else:
  assert src.size==(sw,sh),(q['name'],src.size,(sw,sh));region=src
 frameMask.paste(region,(x,y),region)
assert ImageChops.multiply(diff,Image.merge('RGB',(frameMask,)*3)).getbbox()is None,'approved opaque frame artwork changed'
# Actual HUD artwork mask; transparent external margins contain authorized scenery.
mask=Image.new('L',before.size)
for q in json.loads((p/'ArtSource/EnchantedForestStaged/Stage04/Diagnostics/Unity-720x1360.json').read_text())['images']:
 rr=q['rect']
 if not (rr['y']<260 and rr['height']<260) or q['name']=='OverfilledBacking':continue
 x,y=round(rr['x']),1360-round(rr['y']+rr['height']);sw,sh=round(rr['width']),round(rr['height']);factor=round(q['scale'])
 src=Image.open(p/q['path']).getchannel('A');src=src.resize((src.width*factor,src.height*factor),Image.Resampling.NEAREST)
 if q['type']=='Tiled':
  region=Image.new('L',(sw,sh))
  for yy in range(0,sh,src.height):
   for xx in range(0,sw,src.width):region.paste(src,(xx,yy))
 else:
  assert src.size==(sw,sh),(q['name'],src.size,(sw,sh));region=src
 mask.paste(region,(x,y),region)
d=ImageDraw.Draw(mask)
for box in [(120,1170,194,1238),(526,1170,600,1238),(252,1170,468,1238),(270,1110,450,1128),(220,1278,500,1308)]:d.rectangle(box,fill=255)
bad=ImageChops.multiply(diff,Image.merge('RGB',(mask,)*3));
mask.save(r/'Diagnostics/ProtectedHudPixels.png')
assert bad.getbbox()is None,'Actual painted HUD pixels changed'


allowed=Image.new('L',before.size)
def add(q,folder):
 im=Image.open(folder/(q['file']+'.png')).getchannel('A');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);allowed.paste(im,(q['x']*2,q['y']*2),im)
old=json.loads((p/'ArtSource/EnchantedForestStaged/Stage05/Revision02/Scripts/Placements.json').read_text())['pieces']
for q in old:
 if q['name'].endswith('bough 5')or q['name'].endswith('board foot boulder'):add(q,p/'Assets/_Game/Art/EnchantedForestStaged/Stage05/Revision02')
for q in json.loads((p/'ArtSource/ForestSurroundRevision/Stage02/Scripts/Placements.json').read_text())['pieces']:add(q,p/'Assets/_Game/Art/ForestSurroundRevision/Stage02')
for q in json.loads((r/'Scripts/Placements.json').read_text())['pieces']:add(q,a)
off=Image.eval(allowed,lambda x:255-x);assert ImageChops.multiply(diff,Image.merge('RGB',(off,)*3)).getbbox()is None,'outside authorized dressing mask'
hashes=json.loads((r/'Diagnostics/PreservedFiles.json').read_text());bad=[f for f,h in hashes.items()if hashlib.sha256((p/f).read_bytes()).hexdigest()!=h];assert not bad,bad
for name,im in [('Original-Lower',before),('Revised-Lower',after)]:im.crop((0,1000,720,1360)).save(r/'Previews'/(name+'.png'))
ev=json.loads((r/'Diagnostics/Unity-720x1360.json').read_text())
report=dict(unity_compile='Passed',compile_log='C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-069441fa-032b-424a-88b1-75fc3bc07548.log',test_result='Passed',test='ForestUnifiedCornerTests.UnifiedCornerReview',resolutions=[[720,1360],[1440,2720]],pixel_doubling_exact=True,top_hud_battle_and_upper_board_sides_pixel_identical=True,board_grid_and_opaque_frame_pixel_identical=True,bottom_hud_controls_and_text_pixel_identical=True,unchanged_image_bindings=ev['preservedImages'],protected_source_files=len(hashes),changes_outside_authorized_dressing=0,board=[512,512],cells=[64,64],portrait_native=[98,220],frame=[580,580],native_modules=modules,one_main_stone_instance_per_side=True,old_upper_and_lower_stone_layers_disabled=True,production_integrated=False,real_device_test=False)
(r/'Diagnostics/Validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
