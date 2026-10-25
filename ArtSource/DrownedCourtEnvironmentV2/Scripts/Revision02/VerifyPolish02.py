from pathlib import Path
from PIL import Image
import numpy as np,json,hashlib,re
R=Path(__file__).resolve().parents[2];F=R.parents[1];Q=R/'QA/Revision02';P=R/'Preview/Revision02';E=R/'Exports/Revision02'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def ar(p):return np.array(Image.open(p).convert('RGBA'))
saved=json.loads((Q/'PreservedBefore.json').read_text());changed=[s for s,h in saved.items() if not(F/s).exists() or sha(F/s)!=h];assert not changed,changed[:30]
data=json.loads((R/'Native/Revision02/Manifest.json').read_text());patches=json.loads((R/'Native/Revision02/CoralCleanup.json').read_text());patchcount=0
for j in patches:
 a=ar(R/j['source']);expected=a.copy()
 for x,y,before,after in j['patches']:assert list(a[y,x])==before;expected[y,x]=after;patchcount+=1
 assert np.array_equal(expected,ar(E/(j['name']+'.png'))),j['name']+' cleanup scope'
for a in data['assets']:
 n=a['name'];im=ar(E/(n+'.png'));assert im.shape[:2]==(a['height'],a['width']);assert set(np.unique(im[:,:,3]))<={0,255}
 assert np.array_equal(im,ar(Q/(n+'-roundtrip.png'))),n+' ASE roundtrip';assert sha(E/(n+'.png'))==a['sha256'];assert (E/(n+'.png')).read_bytes()==(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02/{n}.png').read_bytes()
 for f in ['filterMode: 0','textureCompression: 0','enableMipMap: 0','spritePixelsToUnits: 64','spriteMeshType: 0','nPOTScale: 0']:assert f in (F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02/{n}.png.meta').read_text(),(n,f)
screens=[];samples=0
for height in [640,800]:
 for scale in [1,2,3]:
  dim=f'{360*scale}x{height*scale}';before=ar(P/f'Unity-Before-{dim}.png');after=ar(P/f'Unity-Revised-{dim}.png');old=ar(R/f'Preview/Revision01/Unity-Revised-{dim}.png')
  assert np.array_equal(before,old),dim+' baseline mismatch'
  mask=ar(R/f'Preview/Revision01/Protected-Revised-{dim}.png')[:,:,3]==255
  diff=mask&np.any(before!=after,axis=2)
  if np.any(diff):
   yy,xx=np.nonzero(diff);raise AssertionError((dim,'protected pixels changed',len(xx),list(zip(xx.tolist(),yy.tolist()))[:24]))
  samples+=int(mask.sum());screens.append(dict(size=dim,protected_pixels=int(mask.sum())))
 assert np.array_equal(ar(P/f'LoopStart-{height}.png'),ar(P/f'LoopEnd-{height}.png')),str(height)+' loop seam'
 probe=ar(P/f'PortraitMaskProbe-{height}.png');pink=np.all(probe[:,:,:3]==[255,0,255],axis=2);yy,xx=np.nonzero(pink);assert xx.size>0 and xx.min()>=20 and xx.max()<=98 and yy.min()>=34 and yy.max()<=218,('portrait leaked',height,xx.min(),xx.max(),yy.min(),yy.max())
native=ar(P/'Unity-Before-360x640.png');mask=ar(R/'Preview/Revision01/Protected-Revised-360x640.png')[:,:,3]==255
frames=list(sorted((P/'Motion').glob('Frame-*.png')));assert len(frames)==288;unique=set();motion={k:0 for k in ['top','left','right','bottom','clam']};prev=None
regions={'top':(105,55,360,200),'left':(0,265,15,538),'right':(344,265,360,538),'bottom':(80,550,290,638),'clam':(58,596,95,634)}
for f in frames:
 a=ar(f);diff=mask&np.any(a!=native,axis=2)
 if np.any(diff):
  yy,xx=np.nonzero(diff);raise AssertionError((f.name,'FX touches protected pixels',len(xx),list(zip(xx.tolist(),yy.tolist()))[:12]))
 unique.add(sha(f))
 if prev is not None:
  for name,(x,y,X,Y) in regions.items():motion[name]+=int(np.any(a[y:Y,x:X]!=prev[y:Y,x:X]))
 prev=a
assert all(v>4 for v in motion.values()),motion
# The wall is an opaque exact tile, with mortar matched on the repeat boundary.
wall=ar(E/'OceanMasonry.png');assert np.all(wall[:,:,3]==255);assert np.array_equal(wall[0,:,0:3],wall[16,:,0:3])
clips=list((F/'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02/Motion').glob('*.anim'));controllers=list((F/'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02/Motion').glob('*.controller'));assert len(clips)==26 and len(controllers)==26
for p in clips:assert 'm_LoopTime: 1' in p.read_text(),p.name
result=dict(status='PASS',preserved_original_files=len(saved),native_assets=len(data['assets']),coral_artifact_pixels_removed=patchcount,opaque_UI_actor_frame_tile_samples_unchanged=samples,screens=screens,motion_frames=288,distinct_motion_frames=len(unique),motion_region_change_counts=motion,native_animation_clips=len(clips),native_animator_controllers=len(controllers),loop_seams_exact=True,portrait_mask_contained=True,pixelLab_generations=5,scope='Review-only Unity scene; sampled actual AnimationClips. No production gameplay, Android or device testing.')
(Q/'Verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result,indent=2))
