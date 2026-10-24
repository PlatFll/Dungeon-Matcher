from pathlib import Path
from PIL import Image,ImageChops
import json,hashlib,xml.etree.ElementTree as ET
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/ForestSurroundRevision/Stage02';a=p/'Assets/_Game/Art/ForestSurroundRevision/Stage02';t=p/'.utmp/forest-surround-stage02'
modules=[]
for m in json.loads((r/'Diagnostics/Selection.json').read_text())['modules']:
 name=m['name'];im=Image.open(a/(name+'.png')).convert('RGBA');expected=Image.open(t/(name+'-expected.png')).convert('RGBA');reopened=Image.open(t/'reopened'/(name+'.png')).convert('RGBA')
 assert im.size==expected.size==reopened.size and im.tobytes()==expected.tobytes()==reopened.tobytes(),name
 assert set(im.getchannel('A').get_flattened_data())=={0,255},name
 b=im.getbbox();assert b[0]>=2 and b[1]>=2 and b[2]<=im.width-2 and b[3]<=im.height-2,(name,b)
 raw=Image.open(r/'Raw'/m['raw']).convert('RGBA');ox,oy=m['source_body_offset']
 for y in range(raw.height):
  for x in range(raw.width):
   if raw.getpixel((x,y))[3]:assert im.getpixel((x+ox,y+oy))[3]==255,(name,x,y)
 modules.append(dict(name=name,dimensions=im.size,bounds=b,opaque_colors=len({c for c in im.get_flattened_data()if c[3]}),binary_alpha=True,source_body_silhouette_retained=True,libresprite_reopen_exact=True,sha256=hashlib.sha256((a/(name+'.png')).read_bytes()).hexdigest()))
test=ET.parse(r/'Diagnostics/UnityTests.xml').getroot();assert test.attrib['result']=='Passed'and test.attrib['failed']=='0',test.attrib
before=Image.open(r/'Previews/Unity-Before-720x1360.png').convert('RGBA');after=Image.open(r/'Previews/Unity-Stage02-720x1360.png').convert('RGBA');large=Image.open(r/'Previews/Unity-Stage02-1440x2720.png').convert('RGBA');base=Image.open(r/'Previews/Original-Unity-720x1360.png').convert('RGBA')
assert before.tobytes()==base.tobytes(),'baseline differs from approved Stage 1'
assert large.tobytes()==after.resize((1440,2720),Image.Resampling.NEAREST).tobytes(),'not exact pixel doubling'
for box in [(0,0,720,1080),(110,1160,204,1248),(236,1160,484,1248),(516,1160,610,1248),(262,1100,458,1136),(220,1278,500,1308)]:assert before.crop(box).tobytes()==after.crop(box).tobytes(),('locked pixels',box)
allowed=Image.new('L',before.size);old=json.loads((p/'ArtSource/EnchantedForestStaged/Stage05/Revision02/Scripts/Placements.json').read_text())['pieces']
for q in old:
 if not(q['name'].endswith('bough 6')or q['name'].endswith('lower foot boulder')):continue
 im=Image.open(p/'Assets/_Game/Art/EnchantedForestStaged/Stage05/Revision02'/(q['file']+'.png')).getchannel('A');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);allowed.paste(im,(q['x']*2,q['y']*2),im)
for q in json.loads((r/'Scripts/Placements.json').read_text())['pieces']:
 im=Image.open(a/(q['file']+'.png')).getchannel('A');im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);allowed.paste(im,(q['x']*2,q['y']*2),im)
diff=ImageChops.difference(before,after).convert('RGB');off=Image.eval(allowed,lambda x:255-x);assert ImageChops.multiply(diff,Image.merge('RGB',(off,off,off))).getbbox()is None,'outside authorized mask'
hashes=json.loads((r/'Diagnostics/PreservedFiles.json').read_text());bad=[f for f,h in hashes.items()if hashlib.sha256((p/f).read_bytes()).hexdigest()!=h];assert not bad,bad
for name,im in [('Original-Lower',before),('Revised-Lower',after)]:im.crop((0,1040,720,1360)).save(r/'Previews'/(name+'.png'))
ev=json.loads((r/'Diagnostics/Unity-720x1360.json').read_text());report=dict(unity_compile='Passed',compile_log='C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-3ad1675e-0be2-4aec-a83f-7a54a591438e.log',test_result='Passed',test='ForestSurroundStage02Tests.LowerSideRootReview',resolutions=[[720,1360],[1440,2720]],pixel_doubling_exact=True,top_hud_battle_and_stage1_board_sides_pixel_identical=True,bottom_hud_controls_and_text_pixel_identical=True,unchanged_image_bindings=ev['preservedImages'],protected_source_files=len(hashes),changes_outside_authorized_dressing=0,board=[512,512],cells=[64,64],portrait_native=[98,220],frame=[580,580],native_modules=modules,generations_spent=20,production_integrated=False,real_device_test=False)
(r/'Diagnostics/Validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
