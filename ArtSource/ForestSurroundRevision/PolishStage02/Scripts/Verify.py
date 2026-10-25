from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
import json,hashlib,shutil,xml.etree.ElementTree as ET
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/PolishStage02';A=F/'Assets/_Game/Art/ForestSurroundRevision/PolishStage02';T=F/'.utmp/forest-root-joins'
def parts(im):
 points={(x,y)for y in range(im.height)for x in range(im.width)if im.getpixel((x,y))[3]};sizes=[]
 while points:
  todo=[points.pop()];n=0
  while todo:
   x,y=todo.pop();n+=1
   for dx in [-1,0,1]:
    for dy in [-1,0,1]:
     q=(x+dx,y+dy)
     if q in points:points.remove(q);todo.append(q)
  sizes.append(n)
 return sorted(sizes,reverse=True)
records=[]
for name in ['Root_Left','Root_Right','StoneAnchor_Left','StoneAnchor_Right']:
 im=Image.open(A/(name+'.png')).convert('RGBA')
 assert im.tobytes()==Image.open(T/(name+'-expected.png')).convert('RGBA').tobytes()==Image.open(T/'reopened'/(name+'.png')).convert('RGBA').tobytes(),name
 b=im.getbbox();assert b[0]>=2 and b[1]>=2 and b[2]<=im.width-2 and b[3]<=im.height-2,(name,b)
 assert set(im.getchannel('A').get_flattened_data())=={0,255}
 count=parts(im)
 if name.startswith('Root_'):assert len(count)==1,(name,count)
 shutil.copy2(A/(name+'.png'),R/'Exports'/(name+'.png'))
 records.append(dict(name=name,dimensions=im.size,bounds=b,colors=len({c for c in im.get_flattened_data()if c[3]}),alpha=[0,255],component_sizes=count,libresprite_reopen_exact=True))
stonecolors={tuple(bytes.fromhex(h))+(255,)for h in ['23313a','314651','49616d','739096']}
material=[]
for side,offset in [('Left',(0,79)),('Right',(2,75))]:
 old=Image.open(F/'Assets/_Game/Art/ForestSurroundRevision/UnifiedCorners'/('Stone_'+side+'.png')).convert('RGBA')
 new=Image.open(A/('StoneAnchor_'+side+'.png')).convert('RGBA');root=Image.open(A/('Root_'+side+'.png')).convert('RGBA')
 preserved=sum(1 for c in old.get_flattened_data()if c in stonecolors)
 assert all(old.getpixel((x,y))==new.getpixel((x,y))for y in range(old.height)for x in range(old.width)if old.getpixel((x,y))in stonecolors),'Stone material changed'
 joined=Image.new('RGBA',(88,180));joined.alpha_composite(new,offset);joined.alpha_composite(root)
 joined.save(R/'Previews'/(side+'-Native-Assembly.png'))
 joined.resize((264,540),Image.Resampling.NEAREST).save(R/'Previews'/(side+'-Assembly-3x.png'))
 material.append(dict(side=side,unchanged_original_stone_pixels=preserved,assembled_components=parts(joined)))
print('Native modules',records);print('Stone anchors',material)
if not (R/'Diagnostics/UnityTests.xml').exists():raise SystemExit('Native checks complete; Unity evidence not ready yet.')
tests=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert tests.attrib['result']=='Passed'and tests.attrib['failed']=='0'
b=Image.open(R/'Previews/Unity-Before-720x1360.png').convert('RGB');a=Image.open(R/'Previews/Unity-RootJoins-720x1360.png').convert('RGB')
assert b.tobytes()==Image.open(R/'Previews/Before.png').convert('RGB').tobytes(),'Prior checkpoint changed'
large=Image.open(R/'Previews/Unity-RootJoins-1440x2720.png').convert('RGB');assert large.tobytes()==a.resize(large.size,Image.Resampling.NEAREST).tobytes()
diff=ImageChops.difference(b,a)
for box in [(0,0,720,1000),(104,528,616,1040),(236,1160,484,1248),(262,1100,458,1136),(220,1278,500,1308)]:assert b.crop(box).tobytes()==a.crop(box).tobytes(),('Locked region changed',box)
hud=Image.open(F/'ArtSource/ForestSurroundRevision/UnifiedCorners/Diagnostics/ProtectedHudPixels.png').convert('L');assert ImageChops.multiply(diff,Image.merge('RGB',(hud,)*3)).getbbox()is None,'HUD artwork changed'
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
assert ImageChops.multiply(diff,Image.merge('RGB',(frame,)*3)).getbbox()is None,'Board frame changed'
allowed=Image.new('L',b.size);accents=Image.new('L',b.size)
def add(mask,q,folder):
 im=Image.open(folder/(q['file']+'.png')).getchannel('A');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);mask.paste(im,(q['x']*2,q['y']*2),im)
for q in json.loads((F/'ArtSource/EnchantedForestStaged/Stage05/Revision02/Scripts/Placements.json').read_text())['pieces']:
 if q['name'].endswith('bough 5'):add(allowed,q,F/'Assets/_Game/Art/EnchantedForestStaged/Stage05/Revision02')
for q in json.loads((F/'ArtSource/ForestSurroundRevision/UnifiedCorners/Scripts/Placements.json').read_text())['pieces']:
 add(allowed if q['file'].startswith('Stone_')else accents,q,F/'Assets/_Game/Art/ForestSurroundRevision/UnifiedCorners')
for q in json.loads((R/'Scripts/Placements.json').read_text())['pieces']:add(allowed,q,A)
assert ImageChops.multiply(diff,Image.merge('RGB',(accents,)*3)).getbbox()is None,'Mushroom/moss accent changed before Stage 3'
off=ImageChops.invert(allowed);assert ImageChops.multiply(diff,Image.merge('RGB',(off,)*3)).getbbox()is None,'Outside root/anchor repair scope'
hashes=json.loads((R/'Diagnostics/PreservedFiles.json').read_text());changed=[p for p,h in hashes.items()if hashlib.sha256((F/p).read_bytes()).hexdigest()!=h];assert not changed,changed
evidence=json.loads((R/'Diagnostics/Unity-720x1360.json').read_text())
report=dict(native_modules=records,stone_material_preservation=material,unchanged_existing_files=len(hashes),unchanged_image_bindings=evidence['preservedImages'],unity_render_test='Passed',exact_pixel_doubling=True,top_hud_battle_separator_and_upper_surrounds_identical=True,board_and_painted_frame_identical=True,bottom_hud_controls_text_identical=True,existing_mushroom_moss_accents_identical=True,changes_outside_authorized_scope=0,changed_bounds=diff.getbbox(),production_integrated=False,real_device_test=False)
(R/'Diagnostics/Validation.json').write_text(json.dumps(report,indent=2))
comparison=Image.new('RGB',(1448,412),'#15201b');d=ImageDraw.Draw(comparison);d.text((8,8),'BEFORE - prior Unity scene',fill='white');d.text((736,8),'AFTER - continuous lower roots',fill='white');comparison.paste(b.crop((0,980,720,1360)),(0,32));comparison.paste(a.crop((0,980,720,1360)),(728,32));comparison.save(R/'Previews/Unity-BeforeAfter-Lower.png')
for name,im in [('Before',b),('After',a)]:im.crop((0,980,720,1360)).save(R/'Previews'/(name+'-Lower.png'))
print(json.dumps(report,indent=2))
