from pathlib import Path
from PIL import Image,ImageChops
import json,hashlib,xml.etree.ElementTree as ET,sys
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/Stage03'
M=Path('C:/UnityProjects/Dungeon Matcher')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return Image.open(p).convert('RGBA')
protected=json.loads((R/'Diagnostics/ProtectedHashes.json').read_text());changed=[]
for path,h in protected.items():
 if sha(P/path)!=h:changed.append(path)
start=json.loads((R/'Diagnostics/StartingState.json').read_text())
mainchanged=[p for p,h in start['main_user_files'].items() if sha(M/p)!=h]
print('Protected files:',len(protected),'changed:',changed,'main user files changed:',mainchanged)
assert not mainchanged
if '--pre-restore' in sys.argv:sys.exit(0)
assert not changed
tests=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert tests.get('result')=='Passed' and tests.get('passed')=='2'
modules=json.loads((R/'Modules.json').read_text())['parts'];palette=set()
for m in modules:
 im=load(R/'Exports'/f"{m['name']}.png");assert im.size==(m['width'],m['height']);assert sha(P/m['path'])==m['sha256']
 assert sha(R/'Exports'/f"{m['name']}.png")==m['sha256'];assert sha(R/'Native'/f"{m['name']}.aseprite")==m['source_sha256']
 assert set(im.getchannel('A').get_flattened_data())<={0,255};palette.update(v[:3] for v in im.get_flattened_data() if v[3])
captures=[]
for tag,w,h in [('Top',360,240),('BoardHud',576,688)]:
 a=load(R/f'Previews/Unity-{tag}-{w}x{h}.png');b=load(R/f'Previews/Unity-{tag}-{w*2}x{h*2}.png')
 expected=a.resize(b.size,Image.Resampling.NEAREST);diff=ImageChops.difference(expected,b).convert('RGB');box=diff.getbbox()
 captures.append(dict(study=tag,native_size=a.size,double_size=b.size,exact_nearest_neighbor_double=box is None,difference_bounds=box))
 assert box is None,(tag,'integer-zoom capture differs from exact native pixels',box)
# The approved gate region remains source-identical where the new HUD does not overlay it.
before=load(P/'ArtSource/DungeonHybrid/Stage02/Previews/Unity-cast-360x240.png');after=load(R/'Previews/Unity-Top-360x240.png')
gatebox=(173,60,301,178);floorbox=(108,178,360,214)
for box in [gatebox,floorbox]:assert before.crop(box).tobytes()==after.crop(box).tobytes(),box
result=dict(result='PASS',native_modules=len(modules),opaque_colors=len(palette),alpha=[0,255],ase_reopen_export_verified=True,
 protected_files=len(protected),protected_files_unchanged=True,main_user_files=len(start['main_user_files']),main_user_files_unchanged=True,
 approved_gate_and_floor_capture_regions_identical=True,unity_tests_passed=2,captures=captures,
 scope='Art-only preview tests, not production gameplay integration or real-device testing.')
(R/'Diagnostics/Verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result))
