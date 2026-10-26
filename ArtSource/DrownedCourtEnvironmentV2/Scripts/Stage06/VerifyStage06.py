from pathlib import Path
from PIL import Image
import hashlib,json,re
R=Path(__file__).resolve().parents[2];F=R.parents[1];Q=R/'QA/Stage06';E=R/'Exports/Stage06';P=R/'Preview/Stage06'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def img(p):return Image.open(p).convert('RGBA')
def asset(n):return img(E/(n+'.png'))
def components(im):
 pix=im.getchannel('A');left={(x,y) for y in range(im.height) for x in range(im.width) if pix.getpixel((x,y))};groups=[]
 while left:
  seed=left.pop();todo=[seed];group={seed}
  while todo:
   x,y=todo.pop()
   for dx,dy in [(1,0),(-1,0),(0,1),(0,-1),(1,1),(-1,-1),(-1,1),(1,-1)]:
    p=(x+dx,y+dy)
    if p in left:left.remove(p);todo.append(p);group.add(p)
  groups.append(len(group))
 return sorted(groups,reverse=True)
protected=json.loads((R/'QA/ProtectedBefore.json').read_text());accepted=json.loads((Q/'AcceptedStagesHashes.json').read_text());authority=json.loads((Q/'AuthorityHashes.json').read_text())
changed=[p for p,v in protected.items() if not(F/p).exists() or sha(F/p)!=v['sha256']]
changed += [p for p,v in {**accepted,**authority}.items() if not(F/p).exists() or sha(F/p)!=v]
assert not changed,changed
data=json.loads((R/'Native/Stage06/Manifest.json').read_text());colors=set();connectivity={}
for a in data['assets']:
 n=a['name'];im=asset(n);out=img(Q/(n+'-roundtrip.png'))
 assert im.size==(a['width'],a['height']) and im.size==out.size and im.tobytes()==out.tobytes(),n+' roundtrip'
 assert sha(E/(n+'.png'))==a['sha256'],n+' manifest'
 assert (E/(n+'.png')).read_bytes()==(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage06/{n}.png').read_bytes(),n+' imported file'
 assert set(im.getchannel('A').get_flattened_data())<={0,255},n+' alpha';colors.update(c[:3] for c in im.get_flattened_data() if c[3])
 meta=(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage06/{n}.png.meta').read_text()
 for field in ['filterMode: 0','textureCompression: 0','enableMipMap: 0','spritePixelsToUnits: 64','spriteMeshType: 0','nPOTScale: 0']:assert field in meta,(n,field)
 connectivity[n]=components(im)
 # Source-only studies may document raw defects. Final props must be single connected masses.
 if n not in ['PillarSeed','KelpSeed','CoralSeed','KelpTip','KelpStem']:assert len(connectivity[n])==1,(n,'disconnected components',connectivity[n])
for n in ['PillarSeed','KelpSeed','CoralSeed']:assert asset(n).getchannel('A').tobytes()==img(R/f'RawPixelLab/Stage06/{n}.png').getchannel('A').tobytes(),n+' source alpha'
split=Image.new('RGBA',(31,68));split.alpha_composite(asset('KelpStem'),(0,20));split.alpha_composite(asset('KelpTip'),(0,0));assert split.tobytes()==asset('KelpTall').tobytes(),'Kelp split registration'
shafts=['ColumnShaft'+v for v in ['A','B','C','Short']];joins=0
for a in ['ColumnCap']+shafts:
 for b in shafts+['ColumnBase']:
  aa,bb=asset(a),asset(b);assert aa.crop((0,aa.height-1,31,aa.height)).tobytes()==bb.crop((0,0,31,1)).tobytes(),(a,b,'seam');joins+=1
whole=img(P/'SideLayers-Native.png');stone=img(P/'StoneStructure-Native.png')
assert len(components(stone))==2,('continuous side columns',components(stone))
attachments=[]
for p in data['placements']:
 a=asset(p['name']);x,y=p['x'],p['y'];bb=a.getbbox()
 assert x+bb[0]>0 and x+bb[2]<360 and y+bb[1]>=243 and y+bb[3]<=551,(p,'clipped exterior')
 assert x+bb[2]<=43 or x+bb[0]>=317,(p,'board intrusion')
 if p['attachment']:
  px,py=p['attachment'];assert stone.getpixel((px,py))[3],(p,'holdfast lacks stone')
  overlap=sum(1 for ay in range(a.height) for ax in range(a.width) if a.getpixel((ax,ay))[3] and stone.getpixel((x+ax,y+ay))[3])
  assert overlap>=8,(p,'floating accent',overlap);attachments.append(dict(name=p['name'],point=p['attachment'],stone_overlap_pixels=overlap))
screens=[];checked_native=0;frame_samples=0
for height in (640,800):
 offset=0 if height==640 else 63
 expected=Image.new('RGBA',(360,height));expected.alpha_composite(whole,(0,offset))
 for scale in (1,2,3):
  w,h=360*scale,height*scale;dim=f'{w}x{h}'
  before=img(P/f'Unity-Before-{dim}.png');after=img(P/f'Unity-Actors-{dim}.png');only=img(P/f'Unity-SidesOnly-{dim}.png');prior=img(R/f'Preview/Stage05/Unity-Actors-{dim}.png')
  assert before.tobytes()==prior.tobytes(),dim+' before differs from accepted Stage05'
  ex=expected.resize((w,h),Image.Resampling.NEAREST)
  assert ex.getchannel('A').tobytes()==only.getchannel('A').tobytes(),dim+' side alpha mismatch'
  opaque=0
  for c,e in zip(only.get_flattened_data(),ex.get_flattened_data()):
   if e[3]:assert c==e,(dim,'native pixel mismatch');opaque+=1
  checked_native+=opaque
  fm=img(R/f'Preview/Stage05/Unity-FrameOnly-{dim}.png');changed_pixels=0;hidden=0
  for i,(b,a,o,f) in enumerate(zip(before.get_flattened_data(),after.get_flattened_data(),only.get_flattened_data(),fm.get_flattened_data())):
   x=i%w;y=i//w
   if b!=a:
    changed_pixels+=1;assert (x<43*scale or x>=317*scale) and (243+offset)*scale<=y<(551+offset)*scale,(dim,'outside gutters',x,y)
   if o[3] and f[3]:hidden+=1
   if f[3]:assert b==a,(dim,'approved frame changed',x,y);frame_samples+=1
  assert hidden==0,(dim,'native side pixels hidden behind frame',hidden)
  screens.append(dict(size=dim,changed_gutter_pixels=changed_pixels,native_side_pixels=opaque,hidden_by_frame=hidden))
def docs(path):return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!\d+ &(\d+).*?(?=^--- !u!|\Z)',path.read_text(),re.M|re.S)}
prior=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage05.unity');new=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage06.unity');changes=[]
for k,v in prior.items():
 assert k in new
 if v!=new[k]:
  strip=lambda s:re.sub(r'  m_Children:\n(?:  - .*\n)*','  m_Children:\n',s)
  assert 'RectTransform:' in v and strip(v)==strip(new[k]),('old scene mutation',k);changes.append(k)
assert len(changes)==1,changes
unity=json.loads((Q/'UnityPreview.json').read_text());assert unity['status']=='PASS'
result=dict(status='PASS',protected_production_files=len(protected),accepted_stage01_05_files=len(accepted),authorities_unchanged=len(authority),native_roundtrips=len(data['assets']),combined_opaque_colors=len(colors),exact_modular_seams=joins,continuous_stone_masses=2,attachments=attachments,kelp_split_recomposes_exactly=True,asset_connected_components=connectivity,verified_native_side_pixels=checked_native,unchanged_frame_pixels=frame_samples,screens=screens,old_scene_changes=changes,unity_captures=unity['captures'],scope='Static art-only; no production, gameplay, animation or device test. Existing fractional cell mapping remains unchanged.')
(Q/'Verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps({k:v for k,v in result.items() if k not in ['asset_connected_components','screens']},indent=2))
