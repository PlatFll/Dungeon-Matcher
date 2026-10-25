from pathlib import Path
from PIL import Image
import json,hashlib,re,numpy as np
R=Path(__file__).resolve().parents[2];F=R.parents[1];Q=R/'QA/Revision01';N=R/'Native/Revision01';P=R/'Preview/Revision01';E=R/'Exports/Revision01'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def arr(p):return np.array(Image.open(p).convert('RGBA'))
def rgba(c):return list(bytes.fromhex(c))+[255]
protected=json.loads((R/'QA/ProtectedBefore.json').read_text());accepted=json.loads((Q/'AcceptedStagesHashes.json').read_text());authorities=json.loads((Q/'AuthorityHashes.json').read_text())
changed=[p for p,v in protected.items() if not(F/p).exists() or sha(F/p)!=v['sha256']]
changed += [p for p,v in {**accepted,**authorities}.items() if not(F/p).exists() or sha(F/p)!=v]
assert not changed,changed
data=json.loads((N/'Manifest.json').read_text());recolors=0
for job in data['recolors']:
 before=arr(F/job['source']);after=arr(E/(job['name']+'.png'));expected=before.copy()
 for old,new in job['map'].items():expected[np.all(before==rgba(old),axis=2)]=rgba(new)
 assert np.array_equal(after,expected),job['name']+' changed more than specified palette map'
 assert np.array_equal(after[:,:,3],before[:,:,3]),job['name']+' mask'
 assert sha(F/job['source'])==job['source_sha256'];recolors+=1
count=0
for a in data['assets']:
 n=a['name'];im=arr(E/(n+'.png'));assert im.shape[:2]==(a['height'],a['width'])
 assert set(np.unique(im[:,:,3]))<={0,255};assert np.array_equal(im,arr(Q/(n+'-roundtrip.png'))),n+' roundtrip'
 assert sha(E/(n+'.png'))==a['sha256'];assert (E/(n+'.png')).read_bytes()==(F/f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/{n}.png').read_bytes()
 count+=int((im[:,:,3]>0).sum())
screens=[];fixed=0
for height in [640,800]:
 for scale in [1,2,3]:
  dim=f'{360*scale}x{height*scale}';before=arr(P/f'Unity-Before-{dim}.png');after=arr(P/f'Unity-Revised-{dim}.png')
  assert np.array_equal(before,arr(R/f'Preview/Stage08/Unity-Actors-{dim}.png')),dim+' baseline mismatch'
  pb=arr(P/f'Protected-Before-{dim}.png');pa=arr(P/f'Protected-Revised-{dim}.png');assert np.array_equal(pb,pa),dim+' protected rendering changed'
  mask=pa[:,:,3]==255;assert np.array_equal(before[mask],after[mask]),dim+' protected scene pixels changed';fixed+=int(mask.sum())
  sides=arr(P/f'Unity-SidesOnly-{dim}.png');expected=np.array(Image.open(P/f'SideModules-Native-{height}.png').resize((360*scale,height*scale),Image.Resampling.NEAREST));solid=expected[:,:,3]>0
  assert np.array_equal(sides[:,:,3],expected[:,:,3]) and np.array_equal(sides[solid],expected[solid]),dim+' side pixel mapping'
  assert not np.any(solid&mask),dim+' sides overlap protected art'
  assert np.array_equal(after[solid],expected[solid]),dim+' side occlusion/clipping'
  assert not np.any(solid[:(243+(63 if height==800 else 0))*scale]),dim+' side outside authorized top'
  assert not np.any(solid[(551+(63 if height==800 else 0))*scale:]),dim+' side outside authorized bottom'
  assert not np.any(solid[:,31*scale:329*scale]),dim+' board interior encroachment'
  screens.append(dict(size=dim,protected_pixels=int(mask.sum()),side_pixels=int(solid.sum()),recolored_or_redesigned_pixels=int(np.any(before!=after,axis=2).sum())))
def docs(p):return {m.group(1):'\n'.join(line.rstrip() for line in m.group(0).splitlines())+'\n' for m in re.finditer(r'^--- !u!\d+ &(\d+).*?(?=^--- !u!|\Z)',p.read_text(),re.M|re.S)}
old=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage08.unity');new=docs(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision01.unity')
allowed_guids={re.search(r'guid: (\w+)',(F/(j['oldAsset']+'.meta')).read_text()).group(1) for j in data['recolors']};changes=[]
for k,before in old.items():
 assert k in new,('deleted object',k)
 after=new[k]
 if before==after:continue
 if 'RectTransform:' in before:
  strip=lambda s:re.sub(r'  m_Children:\n(?:  - .*\n)*','  m_Children:\n',s)
 elif 'GameObject:' in before and ('m_Name: Stage06_SideArchitecture' in before or 'm_Name: Stage08_MarineDetails' in before):
  strip=lambda s:re.sub(r'  m_IsActive: [01]','  m_IsActive:',s)
 elif any('guid: '+g in before for g in allowed_guids):
  strip=lambda s:re.sub(r'  m_Sprite: \{[^\n]*\}','  m_Sprite:',s)
 elif 'm_Color:' in before:
  object_id=re.search(r'm_GameObject: \{fileID: (\d+)\}',before).group(1)
  assert any('m_Name: '+n in old[object_id] for n in ['BlankOceanExtent','BlankLowerSurroundings']),('unauthorized color',k)
  strip=lambda s:re.sub(r'  m_Color: \{[^\n]*\}','  m_Color:',s)
 else:raise AssertionError(('unauthorized serialized change',k,before[:120]))
 assert strip(before)==strip(after),('unexpected serialized difference',k);changes.append(k)
result=dict(status='PASS',protected_production_files=len(protected),accepted_stage_files=len(accepted),reference_authorities=len(authorities),exact_recolored_assets=recolors,roundtrips=len(data['assets']),native_asset_pixels=count,unchanged_UI_actor_frame_tile_pixels=fixed,screens=screens,authorized_serialized_object_changes=len(changes),all_prior_scene_objects_retained=True,side_extents='Only x0-30 and329-359, y243-550; established tall mapping +63px.',generation_cost=0,limitations='Art-only Unity review, not gameplay/device test. Production integration and merge not performed.')
(Q/'Verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result,indent=2))
