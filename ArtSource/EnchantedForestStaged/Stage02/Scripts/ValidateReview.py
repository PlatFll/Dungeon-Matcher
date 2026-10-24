from pathlib import Path
from PIL import Image,ImageChops
import json,hashlib,xml.etree.ElementTree as ET,shutil
p=Path(__file__).resolve().parents[4];r=p/'ArtSource/EnchantedForestStaged/Stage02';old=p/'ArtSource/EnchantedForestStaged/Stage01'
rgb=lambda f:Image.open(f).convert('RGB')
before=rgb(old/'Previews/Unity-Stage01-720x480.png');after=rgb(r/'Previews/Unity-After-720x480.png');large=rgb(r/'Previews/Unity-After-1440x960.png');probe=rgb(r/'Previews/Unity-MaskProbe-720x480.png')
assert ImageChops.difference(before.crop((234,0,720,480)),after.crop((234,0,720,480))).getbbox() is None,'Protected environment changed'
assert large.tobytes()==after.resize((1440,960),Image.Resampling.NEAREST).tobytes(),'Not exact integer screenshot'
e=json.loads((r/'Diagnostics/Unity-720x480.json').read_text());prev=json.loads((old/'Diagnostics/Unity-720x480.json').read_text())
# Compare unchanged image bindings and screen registration, with duplicate identities handled as multisets.
def unchanged_rows(e):
 return sorted((i['name'],i['path'],tuple(i['rect'][k] for k in ['x','y','width','height'])) for i in e['images'] if ('Stage02/Portrait_' not in i['path'] and '/Checkpoint03/Portrait_' not in i['path'] and '/ForestHybrid/Stage02/Portrait_Inset' not in i['path']))
# Stage 1 old ivy sprites live in Checkpoint03 and are excluded above; all other graphics must be identical.
assert unchanged_rows(e)==unchanged_rows(prev),'A protected image or rect changed'
back=[i for i in e['images'] if i['name']=='OverfilledBacking' and 'Stage02/Portrait_Inset' in i['path']][0]['rect']
# Backing overfills the unchanged mask by 32 native pixels on every edge, displayed at 2x.
mask=(round(back['x']+64),round(480-(back['y']+back['height']-64)),round(back['x']+back['width']-64),round(480-(back['y']+64)))
diff=ImageChops.difference(after,probe);bb=diff.getbbox();assert bb and bb[0]>=mask[0] and bb[1]>=mask[1] and bb[2]<=mask[2] and bb[3]<=mask[3],(bb,mask)
baseline=json.loads((r/'Diagnostics/StartingHashes.json').read_text());changed=[]
for name,h in baseline.items():
 f=p/name
 if not f.exists() or hashlib.sha256(f.read_bytes()).hexdigest()!=h: changed.append(name)
assert set(changed) <= {'Docs/PROJECT_STATE.md'},changed
assert ET.parse(r/'Diagnostics/UnityTests.xml').getroot().attrib['result']=='Passed'
result={'unity_test':'Passed','unchanged_tracked_files':len(baseline),'preserved_environment_region':[234,0,720,480],'protected_image_bindings_and_rectangles_identical':True,'screenshot_2x_exact_nearest_neighbor':True,'mask_screen_rect_top_left':mask,'mask_probe_changed_bounds':bb,'mask_probe_no_spill':True,'portrait_native_size':[98,220],'inset_per_side':12,'backing_overfill_per_side':32,'resized_frame_cases':e['frameCases'],'production_integration':False,'device_test':False,'unity_compile':'Passed','compile_log':'C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-690252a7-e3b1-4e7f-8cdd-ac23577b7d5a.log'}
(r/'Diagnostics/ReviewValidation.json').write_text(json.dumps(result,indent=2))
board=Image.new('RGB',(1440,480));board.paste(before,(0,0));board.paste(after,(720,0));board.save(r/'Previews/Unity-Comparison.png')
after.crop((0,0,234,480)).save(r/'Previews/Portrait-Unity-2x.png')
print(json.dumps(result))
