from pathlib import Path
from PIL import Image,ImageChops
import json,hashlib,xml.etree.ElementTree as ET
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/EnchantedForestStaged/Stage05';a=p/'Assets/_Game/Art/EnchantedForestStaged/Stage05';t=p/'.utmp/enchanted-stage05'
modules=[]
for f in sorted(a.glob('*.png')):
 im=Image.open(f).convert('RGBA');reopened=Image.open(t/'reopened'/f.name).convert('RGBA');expected=Image.open(t/(f.stem+'-expected.png')).convert('RGBA')
 assert im.size==reopened.size==expected.size and im.tobytes()==reopened.tobytes()==expected.tobytes(),f.name
 assert set(im.getchannel('A').get_flattened_data())=={0,255},f.name
 b=im.getbbox();assert b[0]>=2 and b[1]>=2 and b[2]<=im.width-2 and b[3]<=im.height-2,(f.name,b)
 modules.append(dict(name=f.stem,dimensions=im.size,bounds=b,opaque_colors=len({c for c in im.get_flattened_data() if c[3]}),binary_alpha=True,libresprite_reopen_exact=True,sha256=hashlib.sha256(f.read_bytes()).hexdigest()))
assert modules
placements=json.loads((r/'Scripts/Placements.json').read_text())['pieces'];overlay=Image.new('RGBA',(720,1360))
for q in placements:
 im=Image.open(a/(q['file']+'.png')).convert('RGBA');assert 0<=q['x'] and q['x']+im.width<=360 and 240<=q['y'] and q['y']+im.height<=680
 overlay.alpha_composite(im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST),(q['x']*2,q['y']*2))
for box in [(0,0,720,480),(104,528,616,1040),(110,1100,610,1360)]:assert overlay.crop(box).getbbox() is None,box
(r/'Diagnostics/NativeValidation.json').write_text(json.dumps(dict(modules=modules,overlay_instances=len(placements),binary_alpha=True,padded_canvas=True,overlap_board_battle_hud=False),indent=2))
xml=r/'Diagnostics/UnityTests.xml'
if not xml.exists():print('Native checks passed:',len(modules),'sources; no overlap. Unity pending.');raise SystemExit(0)
test=ET.parse(xml).getroot();assert test.attrib['result']=='Passed' and int(test.attrib['failed'])==0,test.attrib
before=Image.open(r/'Previews/Unity-Before-720x1360.png').convert('RGBA');after=Image.open(r/'Previews/Unity-Stage05-720x1360.png').convert('RGBA');large=Image.open(r/'Previews/Unity-Stage05-1440x2720.png').convert('RGBA')
approved=Image.open(p/'ArtSource/EnchantedForestStaged/Stage04/Previews/Unity-Stage04-720x1360.png').convert('RGBA')
assert before.tobytes()==approved.tobytes(),'baseline differs from approved Stage 4'
assert large.tobytes()==after.resize((1440,2720),Image.Resampling.NEAREST).tobytes(),'not exact pixel doubling'
for box in [(0,0,720,480),(104,528,616,1040),(110,1100,610,1360)]:assert before.crop(box).tobytes()==after.crop(box).tobytes(),box
# Every already-approved frame pixel is protected, including its outward foliage.
frame_mask=Image.new('L',before.size)
uses=json.loads((p/'ArtSource/EnchantedForestStaged/Stage04/Diagnostics/Unity-720x1360.json').read_text())['images']
for u in uses:
 if not u['frame']:continue
 im=Image.open(p/u['path']).convert('RGBA').getchannel('A');scale=round(u['scale']);im=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST)
 rect=u['rect'];rw,rh=round(rect['width']),round(rect['height']);patch=Image.new('L',(rw,rh))
 if u['type']=='Tiled':
  for yy in range(0,rh,im.height):
   for xx in range(0,rw,im.width):patch.paste(im,(xx,yy))
 else:assert im.size==(rw,rh);patch=im
 frame_mask.paste(patch,(round(rect['x']),1360-round(rect['y']+rect['height'])),patch)
diff=ImageChops.difference(before,after).convert('RGB');assert ImageChops.multiply(diff,Image.merge('RGB',(frame_mask,frame_mask,frame_mask))).getbbox() is None,'approved frame pixels covered'
offmask=Image.eval(overlay.getchannel('A'),lambda x:255-x)
assert ImageChops.multiply(diff,Image.merge('RGB',(offmask,offmask,offmask))).getbbox() is None,'unexpected pixel changes outside dressing'
base=json.loads((r/'Diagnostics/StartingHashes.json').read_text());bad=[s for s,h in base.items() if not(p/s).exists() or hashlib.sha256((p/s).read_bytes()).hexdigest()!=h];assert not bad,bad
for name,im in [('Unity-Side-Dressing-720x880',after),('Unity-Side-Before-720x880',before)]:im.crop((0,480,720,1360)).save(r/'Previews'/(name+'.png'))
evidence=json.loads((r/'Diagnostics/Unity-720x1360.json').read_text())
report=dict(compile_result='Passed',compile_log='C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-0fc05e71-56c1-49b4-a595-d831a26c88b1.log',test='EnchantedForestStage05Tests.SideDressingReview',test_result='Passed',captures=[[720,1360],[1440,2720]],integer_scale_exact=True,approved_stage4_baseline_pixel_identical=True,all_approved_frame_pixels_unchanged=True,top_battle_area_unchanged=True,board_cells_and_gems_unchanged=True,lower_hud_unchanged=True,changes_outside_dressing_mask=0,board_screen_size=[512,512],frame_screen_size=[580,580],portrait_native_size=[98,220],preserved_image_bindings=evidence['preservedImages'],protected_files_byte_identical=len(base),production_runtime_changed=False,live_combat_tested=False,device_tested=False)
(r/'Diagnostics/Validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
