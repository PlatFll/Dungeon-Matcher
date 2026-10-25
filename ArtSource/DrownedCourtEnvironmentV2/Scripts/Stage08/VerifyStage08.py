from pathlib import Path
from PIL import Image
import hashlib,json,re,numpy as np
R=Path(__file__).resolve().parents[2];F=R.parents[1];Q=R/'QA/Stage08';E=R/'Exports/Stage08';P=R/'Preview/Stage08'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def img(p):return Image.open(p).convert('RGBA')
def arr(p):return np.array(img(p))
def asset(n):return img(E/(n+'.png'))
def components(im):
 a=np.array(im)[:,:,3];left={(int(x),int(y)) for y,x in zip(*np.nonzero(a))};groups=[]
 while left:
  seed=left.pop();todo=[seed];count=1
  while todo:
   x,y=todo.pop()
   for dx,dy in [(1,0),(-1,0),(0,1),(0,-1),(1,1),(-1,-1),(-1,1),(1,-1)]:
    p=(x+dx,y+dy)
    if p in left:left.remove(p);todo.append(p);count+=1
  groups.append(count)
 return sorted(groups,reverse=True)
protected=json.loads((R/'QA/ProtectedBefore.json').read_text());accepted=json.loads((Q/'AcceptedStagesHashes.json').read_text());authority=json.loads((Q/'AuthorityHashes.json').read_text())
changed=[p for p,v in protected.items() if not(F/p).exists() or sha(F/p)!=v['sha256']]
changed += [p for p,v in {**accepted,**authority}.items() if not(F/p).exists() or sha(F/p)!=v]
assert not changed,changed
data=json.loads((R/'Native/Stage08/Manifest.json').read_text());colors=set();connectivity={}
for a in data['assets']:
 n=a['name'];im=asset(n);out=img(Q/(n+'-roundtrip.png'))
 assert im.size==(a['width'],a['height']) and im.size==out.size and im.tobytes()==out.tobytes(),n+' roundtrip'
 assert sha(E/(n+'.png'))==a['sha256'],n+' manifest'
 assert (E/(n+'.png')).read_bytes()==(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage08/{n}.png').read_bytes(),n+' imported file'
 assert set(im.getchannel('A').get_flattened_data())<={0,255},n+' alpha';colors.update(c[:3] for c in im.get_flattened_data() if c[3])
 meta=(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage08/{n}.png.meta').read_text()
 for field in ['filterMode: 0','textureCompression: 0','enableMipMap: 0','spritePixelsToUnits: 64','spriteMeshType: 0','nPOTScale: 0']:assert field in meta,(n,field)
 connectivity[n]=components(im);assert len(connectivity[n])==1,(n,'disconnected components',connectivity[n])
assert asset('SpongePocketSeed').getchannel('A').tobytes()==img(R/'RawPixelLab/Stage08/SpongePocketSeed.png').getchannel('A').tobytes(),'raw silhouette'
assert asset('SpongePocket').crop((2,2,26,31)).tobytes()==asset('SpongePocketSeed').crop((20,21,44,50)).tobytes(),'native padded extraction'
assert asset('SpongePocket').getbbox()==(2,2,26,31),'padding'
assert all(p['name']=='SpongePocket' for p in data['placements']+data['placementsTall']),'rejected trial placed'
screens=[];contacts=[];checked=0;frame_samples=0
for height in (640,800):
 offset=0 if height==640 else 63;expected=img(P/f'DetailLayers-Native-{height}.png');placements=data['placements' if height==640 else 'placementsTall']
 support=Image.new('RGBA',(360,height));support.alpha_composite(img(R/'Preview/Stage06/StoneStructure-Native.png'),(0,offset));sm=np.array(support)[:,:,3]>0;em=np.array(expected)[:,:,3]>0
 overlap=int(np.count_nonzero(sm&em));assert overlap>=20,(height,'not attached',overlap)
 # The bottom three native rows of the stone foot must meet accepted masonry, not just the sponge tubes.
 foot=em.copy();foot[:376+offset]=False;fo=int(np.count_nonzero(foot&sm));assert fo>=10,(height,'unsupported foot',fo)
 contacts.append(dict(height=height,stone_overlap_pixels=overlap,foot_overlap_pixels=fo))
 for scale in (1,2,3):
  w,h=360*scale,height*scale;dim=f'{w}x{h}';before=arr(P/f'Unity-Before-{dim}.png');after=arr(P/f'Unity-Actors-{dim}.png');only=arr(P/f'Unity-SidesOnly-{dim}.png')
  assert np.array_equal(before,arr(R/f'Preview/Stage07/Unity-Actors-{dim}.png')),dim+' accepted scene mismatch'
  ex=np.array(expected.resize((w,h),Image.Resampling.NEAREST));op=ex[:,:,3]>0
  assert np.array_equal(ex[:,:,3],only[:,:,3]) and np.array_equal(ex[op],only[op]),dim+' native mapping'
  # The selected sprite is completely visible, including its contact foot; no hidden/cut pixels.
  assert np.array_equal(after[op],ex[op]),dim+' hidden sprite pixels';checked+=int(op.sum())
  fm=arr(R/f'Preview/Stage05/Unity-FrameOnly-{dim}.png')[:,:,3]>0
  assert not np.any(fm&op) and np.array_equal(before[fm],after[fm]),dim+' frame touched';frame_samples+=int(fm.sum())
  diff=np.any(before!=after,axis=2);assert not np.any(diff&~op),dim+' unrelated changes'
  assert not np.any(diff[:(348+offset)*scale]),dim+' upper layout changed'
  assert not np.any(diff[(height-98)*scale:]),dim+' bottom HUD/reefs changed'
  assert not np.any(diff[:,0:329*scale]),dim+' board or left side changed'
  screens.append(dict(size=dim,changed_pixels=int(diff.sum()),native_pixels=int(op.sum()),hidden_pixels=0,frame_overlap=0))
def docs(path):return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!\d+ &(\d+).*?(?=^--- !u!|\Z)',path.read_text(),re.M|re.S)}
prior=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage07.unity');new=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage08.unity');changes=[]
for k,v in prior.items():
 assert k in new
 if v!=new[k]:
  strip=lambda s:re.sub(r'  m_Children:\n(?:  - .*\n)*','  m_Children:\n',s)
  assert 'RectTransform:' in v and strip(v)==strip(new[k]),('old object mutation',k);changes.append(k)
assert len(changes)==1,changes
unity=json.loads((Q/'UnityPreview.json').read_text());assert unity['status']=='PASS'
result=dict(status='PASS',protected_production_files=len(protected),accepted_stage01_07_files=len(accepted),authorities_unchanged=len(authority),native_roundtrips=len(data['assets']),selected_assets=2,rejected_unplaced_trials=1,placed_details=1,combined_opaque_colors=len(colors),attachment_contacts=contacts,asset_connected_components=connectivity,verified_native_pixels=checked,unchanged_frame_pixels=frame_samples,screens=screens,old_scene_changes=changes,unity_captures=unity['captures'],scope='Static art-only candidate; no animation, production, gameplay, safe-area or device testing. Existing board mapping unchanged.')
(Q/'Verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps({k:v for k,v in result.items() if k not in ['asset_connected_components','screens']},indent=2))
