"""Independent native, preservation, and rendered-board evidence checks."""
from pathlib import Path
from PIL import Image
import hashlib,json,re,math
R=Path(__file__).resolve().parents[2];F=R.parents[1];Q=R/'QA/Stage05';E=R/'Exports/Stage05';P=R/'Preview/Stage05'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def img(p):return Image.open(p).convert('RGBA')
def asset(n):return img(E/(n+'.png'))
protected=json.loads((R/'QA/ProtectedBefore.json').read_text());accepted=json.loads((Q/'AcceptedStagesHashes.json').read_text());authority=json.loads((Q/'AuthorityHashes.json').read_text())
changed=[p for p,v in protected.items() if not (F/p).exists() or sha(F/p)!=v['sha256']]
changed += [p for p,v in {**accepted,**authority}.items() if not (F/p).exists() or sha(F/p)!=v]
assert not changed,changed
data=json.loads((R/'Native/Stage05/Manifest.json').read_text());colors=set()
for a in data['assets']:
 n=a['name'];im=asset(n);out=img(Q/(n+'-roundtrip.png'))
 assert im.size==(a['width'],a['height']) and im.size==out.size and im.tobytes()==out.tobytes(),n+' roundtrip'
 assert sha(E/(n+'.png'))==a['sha256'],n+' manifest'
 assert (E/(n+'.png')).read_bytes()==(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage05/{n}.png').read_bytes(),n+' imported source'
 assert set(im.getchannel('A').getdata())<={0,255},n+' alpha'
 colors.update(c[:3] for c in im.getdata() if c[3])
 meta=(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage05/{n}.png.meta').read_text()
 for field in ['filterMode: 0','textureCompression: 0','enableMipMap: 0','spritePixelsToUnits: 64','spriteMeshType: 0','nPOTScale: 0']:
  assert field in meta,(n,field)
assert img(R/'RawPixelLab/Stage05/BoardStoneSeed.png').getchannel('A').tobytes()==asset('BoardStoneSeed').getchannel('A').tobytes()
joins=0
for names,vertical in [(['RailTop','RailBottom','RailHorizontalWorn'],False),(['RailLeft','RailRight','RailVerticalWorn'],True)]:
 for a in names:
  for b in names:
   aa,bb=asset(a),asset(b)
   first=(0,0,11,1) if vertical else (0,0,1,11)
   last=(0,31,11,32) if vertical else (31,0,32,11)
   assert aa.crop(last).tobytes()==bb.crop(first).tobytes(),a+b+' join';joins+=1
h=asset('RailTop').crop((0,0,1,11)).tobytes();v=asset('RailLeft').crop((0,0,11,1)).tobytes()
for n,right,down in [('CornerTL',True,True),('CornerTR',False,True),('CornerBL',True,False),('CornerBR',False,False)]:
 a=asset(n);x=24 if right else 0;y=24 if down else 0
 assert a.crop((x,7,x+1,18)).tobytes()==h,n+' horizontal exit'
 assert a.crop((7,y,18,y+1)).tobytes()==v,n+' vertical exit';joins+=2
 box=(18 if right else 0,18 if down else 0,25 if right else 7,25 if down else 7)
 assert not a.crop(box).getchannel('A').getbbox(),n+' opening intrusion'
assert not img(P/'Frame-Native.png').crop((18,18,291,291)).getchannel('A').getbbox(),'Native opening obstructed'
old=img(R/'Preview/Stage04/Unity-Actors-720x1280.png');before=img(P/'Unity-Stage04-Before-720x1280.png')
assert old.tobytes()==before.tobytes(),'Before differs from accepted Stage04'
b=json.loads((Q/'CellSnapshot.json').read_text())['board'];edges=[];cell_pixels=0;native_corner_pixels=0
for height in (1280,1600):
 offset=0 if height==1280 else 194
 for width in (360,720,1080):
  scale=width/720;actual_h=int(height*scale);dim=f'{width}x{actual_h}'
  after=img(P/f'Unity-Actors-{dim}.png');before=img(P/f'Unity-Cells-Before-{dim}.png');frame=img(P/f'Unity-FrameOnly-{dim}.png')
  edge=lambda n:math.ceil(n*scale-.5)
  l=edge(b['x']);r=edge(b['x']+b['width']);top=edge(height-b['y']-offset-b['height']);bottom=edge(height-b['y']-offset)
  box=(l,top,r,bottom)
  assert before.crop(box).tobytes()==after.crop(box).tobytes(),dim+' changed cell pixels'
  cell_pixels+=(r-l)*(bottom-top)
  alpha=frame.getchannel('A')
  samples=[alpha.getpixel((x,y)) for x in range(l,r) for y in (top-1,bottom)]
  samples += [alpha.getpixel((x,y)) for y in range(top,bottom) for x in (l-1,r)]
  assert min(samples)==255,(dim,'gap immediately outside grid',samples.count(0))
  edges.append(dict(size=dim,grid_pixel_bounds=box,edge_samples=len(samples),uncovered=0))
  # The candidate scene cannot change a single existing upper/bottom UI pixel.
  prior=img(R/f'Preview/Stage04/Unity-Actors-{dim}.png')
  for region in [(0,0,width,int(484*scale)),(0,int((1102 if height==1280 else 1228)*scale),width,actual_h)]:
   assert prior.crop(region).tobytes()==after.crop(region).tobytes(),(dim,'upper/lower UI changed',region)
  corner=asset('CornerTL').resize((25*width//360,25*width//360),Image.Resampling.NEAREST)
  ox=edge(b['x']-36);oy=edge(height-(b['y']+offset+b['height']+36))
  for y in range(corner.height):
   for x in range(corner.width):
    c=corner.getpixel((x,y))
    if c[3]:assert frame.getpixel((ox+x,oy+y))==c,(dim,'native corner pixel mismatch',x,y);native_corner_pixels+=1
def docs(path):return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!\d+ &(\d+).*?(?=^--- !u!|\Z)',path.read_text(),re.M|re.S)}
prior=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage04.unity');new=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage05.unity');allowed=[]
for key,value in prior.items():
 assert key in new,'Old object removed '+key
 if value!=new[key]:
  if 'm_Name: GridGuide' in value:assert value.replace('m_IsActive: 1','m_IsActive: 0')==new[key]
  else:
   strip=lambda s:re.sub(r'  m_Children:\n(?:  - .*\n)*','  m_Children:\n',s)
   assert 'RectTransform:' in value and strip(value)==strip(new[key]),'Unexpected old object mutation '+key
  allowed.append(key)
assert len(allowed)==19,allowed
unity=json.loads((Q/'UnityPreview.json').read_text());assert unity['status']=='PASS' and unity['lockedCells']==64
result=dict(status='PASS',protected_production_files=len(protected),accepted_stage01_04_files=len(accepted),authorities_unchanged=len(authority),native_roundtrips=len(data['assets']),exact_edge_pairings=joins,combined_opaque_colors=len(colors),source_alpha_preserved=True,before_equals_stage04=True,unchanged_cell_pixels_compared=cell_pixels,upper_and_lower_ui_pixels_unchanged=True,native_corner_pixels_exact=native_corner_pixels,rendered_edge_checks=edges,old_scene_object_changes=allowed,unity_captures=unity['captures'],resize_cases=unity['resizeCases'],limitation='Existing fractional cell mapping preserved; only candidate frame native scale verified. Static art-only scene, not live combat or device testing.')
(Q/'Verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result,indent=2))
