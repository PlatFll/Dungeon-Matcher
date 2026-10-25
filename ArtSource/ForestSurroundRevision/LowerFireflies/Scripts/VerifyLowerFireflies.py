from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
import json,hashlib,shutil,xml.etree.ElementTree as ET,re,yaml
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
R=F/'ArtSource/ForestSurroundRevision/LowerFireflies'; T=F/'.utmp/forest-fireflies'
assert ET.parse(R/'Diagnostics/UnityTests.xml').getroot().attrib['result']=='Passed'
e=json.loads((R/'Diagnostics/UnityEvidence.json').read_text()); assert e['addedFireflies']==8
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
prior=json.loads((R/'Diagnostics/PreservedFiles.json').read_text())
changed=[p for p,h in prior.items() if not (F/p).is_file() or sha(F/p)!=h]; assert not changed,changed
positions=[(22,300),(337,345),(10,423),(329,482),(118,539),(249,557),(15,646),(335,633)]
mask=Image.new('L',(720,1360)); d=ImageDraw.Draw(mask)
for x,y in positions: d.rectangle(((x-2)*2,(y-2)*2,(x+10)*2-1,(y+10)*2-1),fill=255)
outside=ImageChops.invert(mask); off=Image.merge('RGB',(outside,)*3)
before=sorted((T/'before').glob('Frame-*.png')); after=sorted((T/'after').glob('Frame-*.png'))
assert len(before)==len(after)==96
pixelCounts=[]; visibleSamples=[0]*8
for i,(b,a) in enumerate(zip(before,after)):
 old=Image.open(b).convert('RGB'); new=Image.open(a).convert('RGB'); diff=ImageChops.difference(old,new)
 assert ImageChops.multiply(diff,off).getbbox() is None,('out-of-scope pixels',i)
 pixelCounts.append(sum(max(px)>0 for px in diff.get_flattened_data()))
 for j,(x,y) in enumerate(positions):
  visibleSamples[j]+=bool(diff.crop(((x-2)*2,(y-2)*2,(x+10)*2,(y+10)*2)).getbbox())
assert all(20<n<80 for n in visibleSamples),visibleSamples
medium=Image.open(R/'Previews/Unity-Final-720x1360.png').convert('RGB')
large=Image.open(R/'Previews/Unity-Final-1440x2720.png').convert('RGB')
assert medium.tobytes()==Image.open(after[15]).convert('RGB').tobytes()
assert medium.resize(large.size,Image.Resampling.NEAREST).tobytes()==large.tobytes()
def scene(path):
 chunks=re.split(r'^--- !u!\d+ &(\d+)\s*$',path.read_text(encoding='utf-8'),flags=re.M)
 return {k:yaml.safe_load(v) for k,v in zip(chunks[1::2],chunks[2::2])}
old=scene(F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_Waterfall.unity')
new=scene(F/'Assets/_Game/Scenes/ArtPreview_ForestSurround_Fireflies.unity')
assert set(old)<=set(new)
changes=[]
for k,v in old.items():
 if v!=new[k]:
  kind=next(iter(v)); body=v[kind]; fields=[f for f in body if body[f]!=new[k][kind].get(f)]
  go=body.get('m_GameObject',{}).get('fileID'); name=body.get('m_Name')or old.get(str(go),{}).get('GameObject',{}).get('m_Name',kind)
  assert fields==['m_Children'] or fields==['tracks'],(name,fields)
  if fields==['tracks']:
   assert new[k][kind]['tracks'][:19]==body['tracks']
   assert len(new[k][kind]['tracks'])==27
  changes.append(dict(id=k,name=name,fields=fields))
assert len(changes)==2,changes
(R/'Diagnostics/SceneAudit.json').write_text(json.dumps(dict(objects_preserved=len(old),new_objects=len(set(new)-set(old)),removed_objects=0,changes=changes),indent=2))
shutil.copy2(before[15],R/'Previews/Unity-Before.png')
medium.crop((0,480,720,1360)).save(R/'Previews/Unity-Lower.png')
mask.save(R/'Diagnostics/AllowedChanges.png')
result=dict(result='Passed',added_fireflies=8,original_tracks_preserved=19,matched_unity_pairs=96,
 original_files_unchanged=len(prior),changes_outside_new_fireflies=0,top_hud_and_existing_motion_unchanged=True,
 board_and_shared_hud_unchanged=True,exact_pixel_doubling=True,visible_samples_per_firefly=visibleSamples,
 changed_pixels_per_frame_range=[min(pixelCounts),max(pixelCounts)],pixellab_generations=0)
(R/'Diagnostics/Validation.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
