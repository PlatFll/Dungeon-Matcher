from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
import json,hashlib,shutil,xml.etree.ElementTree as ET,re,yaml,struct
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/WaterfallCorrection';T=F/'.utmp/forest-waterfall';A=F/'Assets/_Game/Art/ForestSurroundRevision/WaterfallCorrection'
assert ET.parse(R/'Diagnostics/UnityTests.xml').getroot().attrib['result']=='Passed'
e=json.loads((R/'Diagnostics/UnityEvidence.json').read_text());assert e['removedComets']==2 and e['waterPoses']==24
prior=json.loads((R/'Diagnostics/PreservedFiles.json').read_text());changed=[p for p,h in prior.items()if hashlib.sha256((F/p).read_bytes()).hexdigest()!=h];assert not changed,changed
native=Image.open(R/'Diagnostics/WaterMask.png').convert('L')
oldmask=Image.open(F/'ArtSource/ForestSurroundRevision/LivingForest/Diagnostics/WaterfallAllowedMask.png').convert('L')
water=Image.new('L',(720,1360));water.paste(native.resize((720,400),Image.Resampling.NEAREST),(0,0))
allowed=Image.new('L',(720,1360));allowed.paste(ImageChops.lighter(native,oldmask).resize((720,400),Image.Resampling.NEAREST),(0,0))
d=ImageDraw.Draw(allowed)
for x,y,dx,dy in [(4,540,1,2),(318,615,2,3)]:d.rectangle(((x-dx)*2,(y-dy)*2,(x+38+dx)*2-1,(y+40+dy)*2-1),fill=255)
outside=ImageChops.invert(allowed);outsideRGB=Image.merge('RGB',(outside,)*3);waterRGB=Image.merge('RGB',(water,)*3)
before=sorted((T/'before').glob('Frame-*.png'));after=sorted((T/'after').glob('Frame-*.png'));assert len(before)==len(after)==96
data=[];waterImages=[]
for i,(b,a)in enumerate(zip(before,after)):
 old=Image.open(b).convert('RGB');new=Image.open(a).convert('RGB');diff=ImageChops.difference(old,new)
 assert ImageChops.multiply(diff,outsideRGB).getbbox() is None,('Unexpected difference',i,ImageChops.multiply(diff,outsideRGB).getbbox())
 waterImages.append(ImageChops.multiply(new,waterRGB))
 data.append(sum(max(c)>0 for c in ImageChops.multiply(diff,waterRGB).get_flattened_data()))
assert min(data)>100,min(data)
detail=sorted((T/'water-detail').glob('Frame-*.png'));assert len(detail)==24
detailImages=[Image.open(p).convert('RGB')for p in detail]
assert len({im.tobytes()for im in detailImages})==24
waterOutside=ImageChops.invert(water);waterOutsideRGB=Image.merge('RGB',(waterOutside,)*3)
assert all(ImageChops.multiply(ImageChops.difference(detailImages[0],im),waterOutsideRGB).getbbox()is None for im in detailImages)
medium=Image.open(R/'Previews/Unity-Final-720x1360.png').convert('RGB');large=Image.open(R/'Previews/Unity-Final-1440x2720.png').convert('RGB')
assert medium.tobytes()==Image.open(after[15]).convert('RGB').tobytes()
assert medium.resize(large.size,Image.Resampling.NEAREST).tobytes()==large.tobytes()
assert struct.unpack_from('<H',(R/'Native/Waterfall_Flow.aseprite').read_bytes(),6)[0]==24
for i in range(24):
 im=Image.open(A/f'Waterfall_Flow_{i:02}.png').convert('RGBA');assert im.tobytes()==Image.open(T/f'Waterfall_Flow_{i:02}-plan.png').convert('RGBA').tobytes()
 assert set(im.getchannel('A').get_flattened_data())<={0,255}
 b=im.getbbox();assert b[0]>=2 and b[1]>=2 and b[2]<=38 and b[3]<=142
 assert ImageChops.multiply(im.getchannel('A'),ImageChops.invert(native.crop((174,8,214,152)))).getbbox()is None
def scene(path):
 chunks=re.split(r'^--- !u!\d+ &(\d+)\s*$',path.read_text(encoding='utf-8'),flags=re.M)
 return {k:yaml.safe_load(v)for k,v in zip(chunks[1::2],chunks[2::2])}
old=scene(F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_Living.unity');new=scene(F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_Waterfall.unity');assert set(old)==set(new)
changes=[]
for k,v in old.items():
 if v!=new[k]:
  kind=next(iter(v));body=v[kind];go=body.get('m_GameObject',{}).get('fileID');name=body.get('m_Name')or old.get(str(go),{}).get('GameObject',{}).get('m_Name',kind)
  fields=[f for f in body if body[f]!=new[k][kind].get(f)]
  assert (name in ['Varied forest spirit 4','Varied forest spirit 7'] and fields==['m_IsActive'])or(name=='Living forest lower spirits'and fields==['tracks'])or(name=='Subtle waterfall shimmer'and fields==['m_Sprite']),(name,fields)
  changes.append(dict(id=k,name=name,fields=fields))
assert len(changes)==4
(R/'Diagnostics/SceneAudit.json').write_text(json.dumps(dict(objects_preserved=len(old),new_objects=0,removed_objects=0,changes=changes),indent=2))
shutil.copy2(before[15],R/'Previews/Unity-Before.png')
for i in [0,8,16,24,48,72,95]:shutil.copy2(after[i],R/'Previews'/f'Unity-Motion-{i:03}.png')
medium.crop((0,0,720,480)).save(R/'Previews/Unity-Top.png')
gif=[im.crop((360,86,404,302)).resize((176,864),Image.Resampling.NEAREST)for im in detailImages]
gif[0].save(R/'Previews/Unity-Waterfall.gif',save_all=True,append_images=gif[1:],duration=[80,80,90]*8,loop=0,disposal=2)
allowed.save(R/'Diagnostics/AllowedChanges.png')
result=dict(result='Passed',matched_unity_pairs=96,water_loop_frames=24,water_loop_seconds=2,min_water_pixel_changes=min(data),max_water_pixel_changes=max(data),original_files_unchanged=len(prior),comets_removed=2,changes_outside_water_and_comets=0,exact_pixel_doubling=True,binary_source_alpha=True,libresprite_roundtrip=True,pixellab_generations=0)
(R/'Diagnostics/Validation.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
