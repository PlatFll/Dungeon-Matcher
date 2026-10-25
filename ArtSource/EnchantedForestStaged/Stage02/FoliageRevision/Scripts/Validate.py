from pathlib import Path
from PIL import Image,ImageChops
import json,hashlib,xml.etree.ElementTree as ET
p=Path(__file__).resolve().parents[5];s=p/'ArtSource/EnchantedForestStaged/Stage02';r=s/'FoliageRevision';a=p/'Assets/_Game/Art/EnchantedForestStaged/Stage02/FoliageRevision';t=p/'.utmp/enchanted-foliage-revision'
rows=[];colors=set();new_pixels=0;old_pixels=0
for f in sorted(a.glob('*.png')):
 im=Image.open(f).convert('RGBA');expected=Image.open(t/(f.stem+'-expected.png')).convert('RGBA');reopened=Image.open(t/'reopened'/f.name).convert('RGBA')
 assert im.tobytes()==expected.tobytes()==reopened.tobytes(),f
 c={v for v in im.getdata() if v[3]};colors|=c;alpha={v[3] for v in im.getdata()};assert alpha<={0,255}
 bb=im.getbbox();assert bb[0]>0 and bb[1]>0 and bb[2]<im.width and bb[3]<im.height,(f,bb,im.size)
 new_pixels+=sum(1 for v in im.getdata() if v[3]);rows.append({'name':f.stem,'dimensions':im.size,'occupied_bounds':bb,'colors':len(c),'alpha':sorted(alpha),'sha256':hashlib.sha256(f.read_bytes()).hexdigest()})
for f in (p/'Assets/_Game/Art/EnchantedForestStaged/Stage02').glob('Portrait_Ivy_*.png'):old_pixels+=sum(1 for v in Image.open(f).convert('RGBA').getdata() if v[3])
assert len(rows)==4
rgb=lambda f:Image.open(f).convert('RGB')
before=rgb(s/'Previews/Unity-After-720x480.png');after=rgb(r/'Previews/Unity-After-720x480.png');large=rgb(r/'Previews/Unity-After-1440x960.png');probe=rgb(r/'Previews/Unity-MaskProbe-720x480.png')
assert after.resize((1440,960),Image.Resampling.NEAREST).tobytes()==large.tobytes()
assert ImageChops.difference(before.crop((234,0,720,480)),after.crop((234,0,720,480))).getbbox() is None
prev=json.loads((s/'Diagnostics/Unity-720x480.json').read_text());curr=json.loads((r/'Diagnostics/Unity-720x480.json').read_text())
def protected(e):return sorted((i['name'],i['path'],tuple(i['rect'][k] for k in ['x','y','width','height'])) for i in e['images'] if not i['name'].startswith(('Portrait_Ivy_','Ivy_')))
assert protected(prev)==protected(curr),'Nonfoliage sprite or rectangle changed'
# Mask probe must still stay inside the exact old portrait opening.
diff=ImageChops.difference(after,probe);bb=diff.getbbox();assert bb and bb[0]>=40 and bb[1]>=40 and bb[2]<=188 and bb[3]<=432,bb
for i in curr['images']:
 if 'Rattlebones' in i['path']:
  box=i['rect'];x,y=round(box['x']),round(480-box['y']-box['height']);im=Image.open(p/i['path']).convert('RGBA').crop((0,0,64,64));mask=im.getchannel('A').resize((round(box['width']),round(box['height'])),Image.Resampling.NEAREST)
  d=ImageChops.difference(before.crop((x,y,x+mask.width,y+mask.height)),after.crop((x,y,x+mask.width,y+mask.height)))
  assert ImageChops.multiply(d.convert('L'),mask).getbbox() is None,'Foliage obscures player pixels'
baseline=json.loads((r/'Diagnostics/StartingHashes.json').read_text());changed=[n for n,h in baseline.items() if not (p/n).exists() or hashlib.sha256((p/n).read_bytes()).hexdigest()!=h]
assert set(changed)<={'Docs/PROJECT_STATE.md','ArtSource/EnchantedForestStaged/Stage02/README.md'},changed
test=ET.parse(r/'Diagnostics/UnityTests.xml').getroot();assert test.attrib['result']=='Passed'
result={'module_count':4,'opaque_palette_count':len(colors),'foliage_opaque_pixels_before':old_pixels,'foliage_opaque_pixels_after':new_pixels,'new_generations':0,'native_reopened_identically':True,'all_alpha_binary':True,'all_module_bounds_padded':True,'unchanged_tracked_files':len(baseline),'protected_sprite_refs_and_rectangles_unchanged':True,'rattlebones_opaque_pixels_unchanged':True,'mask_probe_within_unchanged_opening':True,'integer_capture_exact':True,'unity_test':'Passed','unity_compile':'Passed','compile_log':'C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-9deae311-c115-40f4-b7c6-31a24445d0b9.log','modules':rows}
(r/'Diagnostics/Validation.json').write_text(json.dumps(result,indent=2));board=Image.new('RGB',(1440,480));board.paste(before,(0,0));board.paste(after,(720,0));board.save(r/'Previews/Comparison.png');after.crop((0,0,234,480)).save(r/'Previews/Portrait-Unity.png')
print(json.dumps({k:v for k,v in result.items() if k!='modules'}))
