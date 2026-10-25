from pathlib import Path
from PIL import Image,ImageChops
import json,hashlib
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/LivingForest';A=F/'Assets/_Game/Art/ForestSurroundRevision/LivingForest';T=F/'.utmp/living-forest'
records=[]
for spec in json.loads((R/'Diagnostics/NativeManifest.json').read_text()):
 frames=[]
 for i in range(spec['frames']):
  tag=spec['name']+'_'+str(i).zfill(2);p=A/(tag+'.png');im=Image.open(p).convert('RGBA');plan=Image.open(T/(tag+'-plan.png')).convert('RGBA');reopened=Image.open(T/'reopened'/(tag+'.png')).convert('RGBA')
  assert im.size==plan.size and im.tobytes()==plan.tobytes()==reopened.tobytes(),tag
  assert set(im.getchannel('A').get_flattened_data())<={0,255},tag
  if spec['name']!='Grove_Motion_Copy':
   b=im.getbbox();assert b[0]>=2 and b[1]>=2 and b[2]<=im.width-2 and b[3]<=im.height-2,(tag,b)
  frames.append(im);records.append(dict(file=p.relative_to(F).as_posix(),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),alpha_binary=True,libresprite_exact=True))
 if spec['name'] in ['Vine_Left','Vine_Right','Upper_Leaves']:
  source=Image.open(F/spec['source']).convert('RGBA');assert frames[0].crop((4,4,4+source.width,4+source.height)).tobytes()==source.tobytes()
  pixels=[sum(c[3]>0 for c in im.get_flattened_data())for im in frames];assert len(set(pixels))==1
  assert len({im.tobytes()for im in frames})==5
  order=[0,0,1,1,2,2,1,1,0,0,3,3,4,3,0,0]
 else:order=list(range(len(frames)))
 if len(frames)>1:
  view=[frames[i].resize((frames[i].width*4,frames[i].height*4),Image.Resampling.NEAREST)for i in order]
  view[0].save(R/'Previews'/(spec['name']+'-Motion.gif'),save_all=True,append_images=view[1:],duration=125,loop=0,disposal=2)
original=Image.open(F/'Assets/_Game/Art/ForestSurroundRevision/PolishFinal/Corner_Right.png').convert('RGBA');copy=Image.open(A/'Corner_Right_NoSprig_00.png').convert('RGBA');mask=Image.open(R/'Diagnostics/RemovedSprigMask.png').convert('L')
for y in range(original.height):
 for x in range(original.width):
  if not mask.getpixel((x,y)):assert original.getpixel((x,y))==copy.getpixel((x,y))
  else:assert copy.getpixel((x,y))[3]==0
background=Image.open(F/'Assets/_Game/Art/EnchantedForestStaged/Stage01/Grove_Background.png').convert('RGBA');backcopy=Image.open(A/'Grove_Motion_Copy_00.png').convert('RGBA');mask=Image.open(R/'Diagnostics/SeparatedTopMotesMask.png').convert('L')
diff=ImageChops.difference(background.convert('RGB'),backcopy.convert('RGB'));off=ImageChops.invert(mask);assert ImageChops.multiply(diff,Image.merge('RGB',(off,)*3)).getbbox()is None
water=Image.open(R/'Diagnostics/WaterfallAllowedMask.png').convert('L')
for i in range(12):
 im=Image.open(A/('Waterfall_'+str(i).zfill(2)+'.png')).convert('RGBA')
 for y in range(im.height):
  for x in range(im.width):
   if im.getpixel((x,y))[3]:assert water.getpixel((x+174,y+8)),(i,x,y)
(R/'Diagnostics/NativeVerification.json').write_text(json.dumps(dict(poses=len(records),exact_roundtrip=True,binary_alpha=True,leaf_pixel_counts_preserved=True,corner_changes_only_in_removal_mask=True,background_changes_only_in_extracted_light_mask=True,water_changes_confined_to_water=True,files=records),indent=2))
print('PASS:',len(records),'native poses; exact LibreSprite round trips; source masks and native motion bounds.')
