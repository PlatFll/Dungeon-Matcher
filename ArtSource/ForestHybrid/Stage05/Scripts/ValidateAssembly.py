from pathlib import Path
from PIL import Image,ImageChops
import json,hashlib,xml.etree.ElementTree as ET,shutil
F=Path('C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestHybrid/Stage05'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
assets=[]
for stage in range(1,5):
 for p in sorted((F/('Assets/_Game/Art/ForestHybrid/Stage0'+str(stage))).glob('*.png')):
  im=Image.open(p).convert('RGBA')
  assets.append({'stage':stage,'path':p.relative_to(F).as_posix(),'size':list(im.size),'opaque_colors':len({x[:3] for x in im.get_flattened_data() if x[3]}),'sha256':sha(p)})
assert len(assets)==46,len(assets)
(R/'AssetSummary.json').write_text(json.dumps({'approved_assets':assets,'new_artworks':0,'stage5_generations':0,'forest_total_generations':26},indent=2))
native=Image.open(R/'Previews/Unity-Full-720x1360.png').convert('RGB');big=Image.open(R/'Previews/Unity-Full-1440x2720.png').convert('RGB')
assert native.size==(720,1360) and big.size==(1440,2720)
assert native.resize(big.size,Image.Resampling.NEAREST).tobytes()==big.tobytes(),'2x differs'
assert (255,0,255) not in set(native.get_flattened_data()),'uncovered camera background'
oldboard=Image.open(F/'ArtSource/ForestHybrid/Stage03/Previews/Unity-board-608x608.png').convert('RGB').crop((48,48,560,560))
assert native.crop((104,528,616,1040)).tobytes()==oldboard.tobytes(),'board pixels changed'
probe=Image.open(R/'Previews/Unity-PortraitMaskProbe-720x1360.png').convert('RGB')
diff=ImageChops.difference(probe,native);bounds=diff.getbbox()
assert bounds and bounds[0]>=40 and bounds[1]>=40 and bounds[2]<=188 and bounds[3]<=432,bounds
protected=json.loads((R/'Diagnostics/StartingState.json').read_text())['protected']
changed=[p for p,h in protected.items() if sha(F/p)!=h]
assert not changed,changed
test=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert test.attrib['result']=='Passed',test.attrib
uses=json.loads((R/'Diagnostics/Unity-720x1360.json').read_text())['images']
forest_paths={u['path'] for u in uses if '/ForestHybrid/' in u['path']}
assert forest_paths.issubset({a['path'] for a in assets})
summary={'approved_asset_library':46,'forest_modules_used':len(forest_paths),'total_images':len(uses),'new_artworks':0,'new_generation_charge':0,'cumulative_generations':26,'unity_test':{'passed':test.attrib['passed'],'failed':test.attrib['failed']},'exact_2x':True,'no_uncovered_camera_pixels':True,'board_all_512x512_pixels_preserved':True,'portrait_probe_change_bounds':list(bounds),'protected_hashes':len(protected),'changed_protected_files':changed,'limits':'Art-only assembly; no gameplay integration, Android build, real-device, or adaptive production screen testing.'}
(R/'Diagnostics/Validation.json').write_text(json.dumps(summary,indent=2))
native.crop((0,0,720,480)).save(R/'Previews/UpperDetail.png')
native.crop((0,472,720,1104)).save(R/'Previews/BoardDetail.png')
native.crop((80,1090,640,1320)).save(R/'Previews/SharedHudDetail.png')
shutil.copyfile(__file__,R/'Scripts/ValidateAssembly.py')
print(json.dumps(summary,indent=2))
