from pathlib import Path
from PIL import Image,ImageChops
import json,hashlib,xml.etree.ElementTree as ET
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/ForestSurroundRevision/Stage01';a=p/'Assets/_Game/Art/ForestSurroundRevision/Stage01';t=p/'.utmp/forest-surround-stage01'
sel=json.loads((r/'Diagnostics/Selection.json').read_text());modules=[]
for record in sel['modules']:
 name=record['name'];im=Image.open(a/(name+'.png')).convert('RGBA');expected=Image.open(t/(name+'-expected.png')).convert('RGBA');reopened=Image.open(t/'reopened'/(name+'.png')).convert('RGBA')
 assert im.size==expected.size==reopened.size and im.tobytes()==expected.tobytes()==reopened.tobytes(),name
 assert set(im.getchannel('A').get_flattened_data())=={0,255}
 b=im.getbbox();assert b[0]>=2 and b[1]>=2 and b[2]<=im.width-2 and b[3]<=im.height-2
 raw=Image.open(r/'Raw'/record['raw']).convert('RGBA');ox,oy=record['raw_offset']
 for y in range(144):
  for x in range(64):
   if raw.getpixel((x,y))[3]:assert im.getpixel((x+ox,y+oy))[3]==255,(name,x,y)
 modules.append(dict(name=name,dimensions=im.size,bounds=b,opaque_colors=len({c for c in im.get_flattened_data()if c[3]}),binary_alpha=True,source_body_silhouette_retained=True,libresprite_reopen_exact=True,sha256=hashlib.sha256((a/(name+'.png')).read_bytes()).hexdigest()))
test=ET.parse(r/'Diagnostics/UnityTests.xml').getroot();assert test.attrib['result']=='Passed'and test.attrib['failed']=='0',test.attrib
before=Image.open(r/'Previews/Unity-Before-720x1360.png').convert('RGBA');after=Image.open(r/'Previews/Unity-Stage01-720x1360.png').convert('RGBA');large=Image.open(r/'Previews/Unity-Stage01-1440x2720.png').convert('RGBA');base=Image.open(r/'Previews/Original-Unity-720x1360.png').convert('RGBA')
assert before.tobytes()==base.tobytes(),'current implementation differs from baseline'
assert large.tobytes()==after.resize((1440,2720),Image.Resampling.NEAREST).tobytes(),'noninteger doubling'
for rect in [(0,0,720,480),(0,1080,720,1360),(104,528,616,1040)]:assert before.crop(rect).tobytes()==after.crop(rect).tobytes(),('locked region',rect)
# Every approved opaque frame pixel is protected, including its pre-existing ivy.
mask=Image.new('L',before.size);uses=json.loads((p/'ArtSource/EnchantedForestStaged/Stage04/Diagnostics/Unity-720x1360.json').read_text())['images']
for u in uses:
 if not u['frame']:continue
 im=Image.open(p/u['path']).convert('RGBA').getchannel('A');scale=round(u['scale']);im=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST);rect=u['rect'];rw,rh=round(rect['width']),round(rect['height']);patch=Image.new('L',(rw,rh))
 if u['type']=='Tiled':
  for yy in range(0,rh,im.height):
   for xx in range(0,rw,im.width):patch.paste(im,(xx,yy))
 else:assert im.size==(rw,rh);patch=im
 mask.paste(patch,(round(rect['x']),1360-round(rect['y']+rect['height'])),patch)
diff=ImageChops.difference(before,after).convert('RGB');assert ImageChops.multiply(diff,Image.merge('RGB',(mask,mask,mask))).getbbox()is None,'frame pixels changed'
allowed=Image.new('L',before.size)
old=json.loads((p/'ArtSource/EnchantedForestStaged/Stage05/Revision02/Scripts/Placements.json').read_text())['pieces']
for q in old:
 if not any(q['name'].endswith(' bough '+str(i))for i in range(1,5)):continue
 im=Image.open(p/'Assets/_Game/Art/EnchantedForestStaged/Stage05/Revision02'/(q['file']+'.png')).getchannel('A');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);allowed.paste(im,(q['x']*2,q['y']*2),im)
for q in json.loads((r/'Scripts/Placements.json').read_text())['pieces']:
 im=Image.open(a/(q['file']+'.png')).getchannel('A');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);allowed.paste(im,(q['x']*2,q['y']*2),im)
off=Image.eval(allowed,lambda x:255-x);assert ImageChops.multiply(diff,Image.merge('RGB',(off,off,off))).getbbox()is None,'pixels changed outside permitted dressing'
hashes=json.loads((r/'Diagnostics/PreservedFiles.json').read_text());bad=[f for f,h in hashes.items()if hashlib.sha256((p/f).read_bytes()).hexdigest()!=h];assert not bad,bad
for name,im in [('Original-Board',before),('Revised-Board',after)]:im.crop((0,480,720,1080)).save(r/'Previews'/(name+'.png'))
evidence=json.loads((r/'Diagnostics/Unity-720x1360.json').read_text())
report=dict(test_result='Passed',test='ForestSurroundStage01Tests.BoardSideRootReview',resolutions=[[720,1360],[1440,2720]],pixel_doubling_exact=True,pack_base_matches_current=True,top_hud_and_battle_pixel_identical=True,board_and_gems_pixel_identical=True,approved_frame_pixels_identical=True,lower_hud_and_deferred_surroundings_pixel_identical=True,unchanged_image_bindings=evidence['preservedImages'],protected_source_files=len(hashes),changes_outside_authorized_dressing=0,board=[512,512],cells=[64,64],portrait_native=[98,220],frame=[580,580],native_modules=modules,credits_dollars_spent=0,generations_spent=10,production_integrated=False,real_device_test=False)
(r/'Diagnostics/Validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
