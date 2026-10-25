from pathlib import Path
from PIL import Image
import hashlib,json,re,numpy as np
R=Path(__file__).resolve().parents[2];F=R.parents[1];Q=R/'QA/Stage07';E=R/'Exports/Stage07';P=R/'Preview/Stage07'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def img(p):return Image.open(p).convert('RGBA')
def arr(p):return np.array(img(p))
def asset(n):return img(E/(n+'.png'))
def components(im):
 a=np.array(im)[:,:,3];left={(int(x),int(y)) for y,x in zip(*np.nonzero(a))};groups=[]
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
data=json.loads((R/'Native/Stage07/Manifest.json').read_text());colors=set();connectivity={}
for a in data['assets']:
 n=a['name'];im=asset(n);out=img(Q/(n+'-roundtrip.png'))
 assert im.size==(a['width'],a['height']) and im.size==out.size and im.tobytes()==out.tobytes(),n+' roundtrip'
 assert sha(E/(n+'.png'))==a['sha256'],n+' manifest'
 assert (E/(n+'.png')).read_bytes()==(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage07/{n}.png').read_bytes(),n+' imported file'
 assert set(im.getchannel('A').get_flattened_data())<={0,255},n+' alpha';colors.update(c[:3] for c in im.get_flattened_data() if c[3])
 meta=(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage07/{n}.png.meta').read_text()
 for field in ['filterMode: 0','textureCompression: 0','enableMipMap: 0','spritePixelsToUnits: 64','spriteMeshType: 0','nPOTScale: 0']:assert field in meta,(n,field)
 connectivity[n]=components(im);assert len(connectivity[n])==1,(n,'disconnected components',connectivity[n])
for n in ['LeftReef','RightReef']:assert asset(n).getchannel('A').tobytes()==img(R/f'RawPixelLab/Stage07/{n}.png').getchannel('A').tobytes(),n+' raw silhouette'
assert asset('RuinConnector').tobytes()==img(R/'Exports/Stage06/ColumnShaftA.png').tobytes(),'accepted shaft copy'
for n in ['Seafloor','SeafloorShort']:
 im=asset(n);assert im.crop((0,0,1,6)).tobytes()==im.crop((im.width-1,0,im.width,6)).tobytes(),n+' tiling seam'
screens=[];checked_native=0;frame_samples=0;oldside_samples=0;contacts=[]
for height in (640,800):
 offset=0 if height==640 else 63
 expected=img(P/f'ReefLayers-Native-{height}.png');placements=data['placements' if height==640 else 'placementsTall']
 # Structural join proof uses the accepted stone-only side assembly and the new connectors, omitting unrelated coral.
 base=Image.new('RGBA',(360,height));base.alpha_composite(img(R/'Preview/Stage06/StoneStructure-Native.png'),(0,offset))
 for p in placements:
  if p['name']=='RuinConnector':base.alpha_composite(asset(p['name']),(p['x'],p['y']))
 for name,x in [('LeftReef',0),('RightReef',280)]:
  reef=Image.new('RGBA',base.size);reef.alpha_composite(asset(name),(x,height-(98 if name=='LeftReef' else 95)))
  overlap=int(np.count_nonzero((np.array(reef)[:,:,3]>0)&(np.array(base)[:,:,3]>0)));assert overlap>=8,(height,name,'column join',overlap)
  floor_overlap=int(np.count_nonzero(np.array(reef)[height-6:,:,3]));assert floor_overlap>0,(height,name,'not grounded')
  contacts.append(dict(height=height,name=name,column_overlap_pixels=overlap,seafloor_overlap_pixels=floor_overlap))
  base.alpha_composite(reef)
 assert len(components(base))==2,(height,'not exactly two continuous reef-column structures',components(base))
 for p in placements:
  bb=asset(p['name']).getbbox();assert p['x']+bb[0]>=0 and p['x']+bb[2]<=360 and p['y']+bb[1]>=0 and p['y']+bb[3]<=height,(p,'viewport clipping')
 for scale in (1,2,3):
  w,h=360*scale,height*scale;dim=f'{w}x{h}'
  before=arr(P/f'Unity-Before-{dim}.png');after=arr(P/f'Unity-Actors-{dim}.png');only=arr(P/f'Unity-SidesOnly-{dim}.png');prior=arr(R/f'Preview/Stage06/Unity-Actors-{dim}.png')
  assert np.array_equal(before,prior),dim+' before differs from accepted Stage06'
  ex=np.array(expected.resize((w,h),Image.Resampling.NEAREST));opaque=ex[:,:,3]>0
  assert np.array_equal(ex[:,:,3],only[:,:,3]),dim+' native layer alpha mismatch'
  assert np.array_equal(ex[opaque],only[opaque]),dim+' native pixels mismatch';checked_native+=int(opaque.sum())
  fm=arr(R/f'Preview/Stage05/Unity-FrameOnly-{dim}.png')[:,:,3]>0;side=arr(R/f'Preview/Stage06/Unity-SidesOnly-{dim}.png')[:,:,3]>0
  assert np.array_equal(before[fm],after[fm]),dim+' approved frame changed';frame_samples+=int(fm.sum())
  assert np.array_equal(before[side],after[side]),dim+' accepted side pixels changed';oldside_samples+=int(side.sum())
  assert not np.any(opaque&fm),dim+' reef behind frame'
  assert np.array_equal(before[(height-80)*scale:(height-9)*scale,90*scale:270*scale],after[(height-80)*scale:(height-9)*scale,90*scale:270*scale]),dim+' bottom HUD region changed'
  changed=np.any(before!=after,axis=2);ys,xs=np.nonzero(changed)
  legal=((ys>=(545+offset)*scale)&((xs<104*scale)|(xs>=249*scale)))|(ys>=(height-6)*scale)
  assert np.all(legal),(dim,'out of scope',list(zip(xs[~legal][:8],ys[~legal][:8])))
  screens.append(dict(size=dim,changed_lower_pixels=int(changed.sum()),native_pixels=int(opaque.sum()),frame_overlap_pixels=0,protected_hud_pixels_unchanged=True))
def docs(path):return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!\d+ &(\d+).*?(?=^--- !u!|\Z)',path.read_text(),re.M|re.S)}
prior=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage06.unity');new=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage07.unity');changes=[]
for k,v in prior.items():
 assert k in new
 if v!=new[k]:
  strip=lambda s:re.sub(r'  m_Children:\n(?:  - .*\n)*','  m_Children:\n',s)
  assert 'RectTransform:' in v and strip(v)==strip(new[k]),('old scene mutation',k);changes.append(k)
assert len(changes)==1,changes
unity=json.loads((Q/'UnityPreview.json').read_text());assert unity['status']=='PASS'
result=dict(status='PASS',protected_production_files=len(protected),accepted_stage01_06_files=len(accepted),authorities_unchanged=len(authority),native_roundtrips=len(data['assets']),combined_opaque_colors=len(colors),main_reef_masses=2,continuous_reef_column_structures=2,contacts=contacts,asset_connected_components=connectivity,verified_native_pixels=checked_native,unchanged_frame_pixels=frame_samples,unchanged_stage06_pixels=oldside_samples,screens=screens,old_scene_changes=changes,unity_captures=unity['captures'],scope='Static art-only; no production, gameplay, animation, safe-area or device test. Existing fractional cell mapping remains unchanged.')
(Q/'Verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps({k:v for k,v in result.items() if k not in ['asset_connected_components','screens']},indent=2))
