"""Stage03 checks: exact source/export/import pixels, edge joins, preservation and Unity render evidence."""
from pathlib import Path
from PIL import Image
import hashlib,json,re
R=Path(__file__).resolve().parents[2];F=R.parents[1];Q=R/'QA/Stage03';E=R/'Exports/Stage03';P=R/'Preview/Stage03'
protected=json.loads((R/'QA/ProtectedBefore.json').read_text());accepted=json.loads((Q/'AcceptedStagesHashes.json').read_text())
changed=[p for p,v in protected.items() if not (F/p).exists() or hashlib.sha256((F/p).read_bytes()).hexdigest()!=v['sha256']]
changed += [p for p,v in accepted.items() if not (F/p).exists() or hashlib.sha256((F/p).read_bytes()).hexdigest()!=v]
assert not changed,changed
data=json.loads((R/'Native/Stage03/Manifest.json').read_text());colors=set()
def img(n):return Image.open(E/(n+'.png')).convert('RGBA')
for a in data['assets']:
 n=a['name'];p=E/(n+'.png');im=img(n);out=Image.open(Q/(n+'-roundtrip.png')).convert('RGBA')
 assert im.tobytes()==out.tobytes() and im.size==out.size,n+' ASE round trip'
 assert p.read_bytes()==(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage03/{n}.png').read_bytes(),n+' Unity PNG'
 assert hashlib.sha256(p.read_bytes()).hexdigest()==a['sha256'],n+' hash'
 assert set(im.getchannel('A').getdata())<={0,255},n+' alpha'
 colors.update(c[:3] for c in im.getdata() if c[3])
raw=Image.open(R/'RawPixelLab/Stage03/PlatformSeed.png').convert('RGBA');assert raw.getchannel('A').tobytes()==img('PlatformSeed').getchannel('A').tobytes()
joins=0
for prefix in ['FloorTop','Fascia']:
 for a in ['A','B','C','Fill','Left']:
  for b in ['A','B','C','Fill','Right']:
   aa=img(prefix+a);bb=img(prefix+b)
   assert aa.crop((aa.width-1,0,aa.width,aa.height)).tobytes()==bb.crop((0,0,1,bb.height)).tobytes(),prefix+a+b
   joins+=1
for p in data['placements']:
 a=next(a for a in data['assets'] if a['name']==p['name']);assert 0<=p['x']<=360-a['width'] and 0<=p['y']<=240-a['height']
floor=Image.open(P/'Module-Join-Native.png').convert('RGBA');assert floor.size==(360,42) and set(floor.getchannel('A').getdata())=={255}
native=Image.open(P/'LayerStudy-Native.png').convert('RGBA');actual=Image.open(P/'Unity-Layers-720x1280.png').convert('RGBA')
expected=Image.new('RGBA',actual.size);expected.paste(native.resize((720,480),Image.Resampling.NEAREST),(0,4))
for area in [(216,150,720,400),(216,400,720,484),(0,456,720,484)]:assert expected.crop(area).tobytes()==actual.crop(area).tobytes(),('Native pixels / Unity mismatch',area)
normal=Image.open(P/'Unity-Actors-720x1280.png');tall=Image.open(P/'Unity-Actors-720x1600.png')
assert normal.crop((0,0,720,484)).tobytes()==tall.crop((0,0,720,484)).tobytes(),'Tall upper drift'
prior=Image.open(R/'Preview/Stage02/Unity-Actors-720x1280.png').convert('RGBA');before=Image.open(P/'Unity-Before-720x1280.png').convert('RGBA')
assert prior.tobytes()==before.tobytes(),'Before differs from accepted Stage02'
assert normal.crop((0,484,720,1280)).tobytes()==prior.crop((0,484,720,1280)).tobytes(),'Lower layout changed'
# RectMask2D's unchanged shader feathers its existing boundary by one pixel.
# The opaque interior must remain exact; only those pre-existing boundary pixels may blend the new floor.
assert normal.crop((22,50,215,454)).tobytes()==prior.crop((22,50,215,454)).tobytes(),'Portrait interior leaked'
for y in range(49,455):
 for x in range(21,216):
  if normal.getpixel((x,y))!=prior.getpixel((x,y)):assert x in (21,215) or y in (49,454),'Mask change beyond existing boundary'
def docs(path):
 t=path.read_text();return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!\d+ &(\d+).*?(?=^--- !u!|\Z)',t,re.M|re.S)}
old=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage02.unity');new=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage03.unity')
allowed=[]
for key,value in old.items():
 assert key in new,'Original scene object removed '+key
 if value!=new[key]:
  # Only the canvas child list and the obsolete guide's active flag may change.
  if 'm_Name: ExistingBattleFloorGuide' in value:assert value.replace('m_IsActive: 1','m_IsActive: 0')==new[key]
  else:
   stripped=lambda x: re.sub(r'  m_Children:\n(?:  - .*\n)*','  m_Children:\n',x)
   assert stripped(value)==stripped(new[key]) and 'RectTransform:' in value,'Unexpected scene edit '+key
  allowed.append(key)
assert len(allowed)==2,allowed
assert json.loads((Q/'UnityPreview.json').read_text())['status']=='PASS'
result=dict(status='PASS',protected_production_files=len(protected),accepted_stage01_02_files=len(accepted),native_roundtrips=len(data['assets']),exact_edge_pairings=joins,combined_opaque_colors=len(colors),native_unity_pixels_exact=True,before_equals_stage02=True,normal_tall_upper_exact=True,unchanged_lower_scene_pixels=True,portrait_interior_unchanged=True,portrait_mask_serialization_unchanged=True,portrait_boundary='Existing one-pixel RectMask2D shader boundary blends new floor; no other portrait changes.',old_scene_object_changes=allowed,full_floor_opaque=True)
(Q/'Verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result,indent=2))
