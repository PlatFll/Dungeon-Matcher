"""Independent checks of Stage04 files, editable roundtrips, joins and Unity render evidence."""
from pathlib import Path
from PIL import Image
import hashlib,json,re
R=Path(__file__).resolve().parents[2];F=R.parents[1];Q=R/'QA/Stage04';E=R/'Exports/Stage04';P=R/'Preview/Stage04'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
protected=json.loads((R/'QA/ProtectedBefore.json').read_text());accepted=json.loads((Q/'AcceptedStagesHashes.json').read_text());authority=json.loads((Q/'AuthorityHashes.json').read_text())
changed=[p for p,v in protected.items() if not (F/p).exists() or sha(F/p)!=v['sha256']]
changed += [p for p,v in {**accepted,**authority}.items() if not (F/p).exists() or sha(F/p)!=v]
assert not changed,changed
data=json.loads((R/'Native/Stage04/Manifest.json').read_text());colors=set()
def img(path):return Image.open(path).convert('RGBA')
def asset(n):return img(E/(n+'.png'))
for a in data['assets']:
 n=a['name'];im=asset(n);out=img(Q/(n+'-roundtrip.png'))
 assert im.size==(a['width'],a['height']) and im.size==out.size and im.tobytes()==out.tobytes(),n+' roundtrip'
 assert sha(E/(n+'.png'))==a['sha256'],n+' manifest'
 assert (E/(n+'.png')).read_bytes()==(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage04/{n}.png').read_bytes(),n+' imported source'
 assert set(im.getchannel('A').getdata())<={0,255},n+' alpha'
 colors.update(c[:3] for c in im.getdata() if c[3])
assert img(R/'RawPixelLab/Stage04/CornerMotif.png').getchannel('A').tobytes()==asset('CornerMotif').getchannel('A').tobytes()
joins=0
for a in ['RecessA','RecessB']:
 for b in ['RecessA','RecessB']:
  aa,bb=asset(a),asset(b)
  assert aa.crop((63,0,64,64)).tobytes()==bb.crop((0,0,1,64)).tobytes(),a+b+' horizontal'
  assert aa.crop((0,63,64,64)).tobytes()==bb.crop((0,0,64,1)).tobytes(),a+b+' vertical';joins+=2
for n in ['RailTop','RailBottom']:
 a=asset(n);assert a.crop((0,0,1,7)).tobytes()==a.crop((15,0,16,7)).tobytes();joins+=1
for n in ['RailLeft','RailRight']:
 a=asset(n);assert a.crop((0,0,7,1)).tobytes()==a.crop((0,15,7,16)).tobytes();joins+=1
# Every relevant L exit reproduces the same seven-pixel rail profile.
h=asset('RailTop').crop((0,0,1,7)).tobytes();v=asset('RailLeft').crop((0,0,7,1)).tobytes()
for n,right,down in [('CornerTL',True,True),('CornerTR',False,True),('CornerBL',True,False),('CornerBR',False,False)]:
 a=asset(n);x=14 if right else 0;y=14 if down else 0
 assert a.crop((x,4,x+1,11)).tobytes()==h,n+' horizontal exit'
 assert a.crop((4,y,11,y+1)).tobytes()==v,n+' vertical exit';joins+=2
before=img(P/'Unity-Before-720x1280.png');old=img(R/'Preview/Stage03/Unity-Actors-720x1280.png')
assert before.tobytes()==old.tobytes(),'Before differs from accepted Stage03'
normal=img(P/'Unity-Actors-720x1280.png');tall=img(P/'Unity-Actors-720x1600.png')
assert normal.crop((0,0,720,484)).tobytes()==tall.crop((0,0,720,484)).tobytes(),'Tall upper drift'
# Only portrait rectangle plus attached exterior accent bounds may change.
diffbox=(13,49,223,455)
for y in range(1280):
 for x in range(720):
  if not(diffbox[0]<=x<diffbox[2] and diffbox[1]<=y<diffbox[3]):assert before.getpixel((x,y))==normal.getpixel((x,y)),('Unexpected outside-portrait change',x,y)
probe=img(P/'Unity-Mask-Probe-720x1280.png');layers=img(P/'Unity-Layers-720x1280.png');magenta=0;changed_probe=0
for y in range(1280):
 for x in range(720):
  c=probe.getpixel((x,y))
  if c[:3]==(255,0,255):magenta+=1
  if c!=layers.getpixel((x,y)):
   changed_probe+=1;assert 41<=x<196 and 69<=y<435,('Mask leak',x,y)
assert magenta>40000 and changed_probe>40000,(magenta,changed_probe)
# Uncovered TR corner is rendered exactly at source 2x. Alpha pixels reveal existing surroundings.
corner=asset('CornerTR').resize((30,30),Image.Resampling.NEAREST)
for y in range(30):
 for x in range(30):
  c=corner.getpixel((x,y))
  if c[3]:assert layers.getpixel((187+x,49+y))==c,('Native corner mismatch',x,y)
def docs(path):
 return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!\d+ &(\d+).*?(?=^--- !u!|\Z)',path.read_text(),re.M|re.S)}
prior=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage03.unity');new=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage04.unity');allowed=[]
for key,value in prior.items():
 assert key in new,'Old scene object removed '+key
 if value!=new[key]:
  if 'm_Name: OverfilledBlankRecess' in value:assert value.replace('m_IsActive: 1','m_IsActive: 0')==new[key]
  else:
   strip=lambda s:re.sub(r'  m_Children:\n(?:  - .*\n)*','  m_Children:\n',s)
   assert 'RectTransform:' in value and strip(value)==strip(new[key]),'Unexpected old scene mutation '+key
  allowed.append(key)
assert len(allowed)==3,allowed
assert json.loads((Q/'UnityPreview.json').read_text())['status']=='PASS'
result=dict(status='PASS',protected_production_files=len(protected),accepted_stage01_03_files=len(accepted),authorities_unchanged=len(authority),native_roundtrips=16,exact_edge_pairings=joins,combined_opaque_colors=len(colors),source_alpha_preserved=True,before_equals_stage03=True,normal_tall_upper_exact=True,outside_portrait_pixels_unchanged=True,magenta_probe_pixels=magenta,probe_changed_pixels=changed_probe,probe_leaks=0,native_corner_pixels_exact=True,old_scene_object_changes=allowed)
(Q/'Verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result,indent=2))
