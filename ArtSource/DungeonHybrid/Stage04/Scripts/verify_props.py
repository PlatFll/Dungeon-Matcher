from pathlib import Path
from PIL import Image,ImageChops
import json,hashlib,xml.etree.ElementTree as ET,sys
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/Stage04';M=Path('C:/UnityProjects/Dungeon Matcher')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return Image.open(p).convert('RGBA')
protected=json.loads((R/'Diagnostics/ProtectedHashes.json').read_text());changed=[p for p,h in protected.items() if sha(P/p)!=h]
start=json.loads((R/'Diagnostics/StartingState.json').read_text())
mainchanged=[p for p,h in start['main_user_files'].items() if sha(M/p)!=h]
print('Protected:',len(protected),'changed:',changed,'main user files changed:',mainchanged)
assert not mainchanged
if '--pre-restore' in sys.argv:sys.exit(0)
assert not changed
tests=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert tests.get('result')=='Passed' and tests.get('passed')=='1'
mods=json.loads((R/'Modules.json').read_text())['parts']
for m in mods:
 im=load(R/'Exports'/f"{m['name']}.png");assert im.size==(m['width'],m['height'])
 assert sha(P/m['path'])==m['sha256']==sha(R/'Exports'/f"{m['name']}.png")
 assert sha(R/'Native'/f"{m['name']}.aseprite")==m['source_sha256']
 assert set(im.getchannel('A').get_flattened_data())=={0,255}
 x0,y0,x1,y1=im.getbbox();assert x0>=2 and x1<=im.width-2
 if not m['name'].startswith('Chain_'):assert y0>=2 and y1<=im.height-2
families=[]
for n in ['Torch','Chain','Banner','Barrel','Jar','Bones','Rubble']:
 a=load(R/'Sources'/f'{n}_Raw.png');b=load(R/'Previews'/f'{n}_Clean64.png')
 assert a.size==b.size==(64,64) and a.getchannel('A').tobytes()==b.getchannel('A').tobytes()
 families.append(dict(family=n,raw_sha256=sha(R/'Sources'/f'{n}_Raw.png'),clean_sha256=sha(R/'Previews'/f'{n}_Clean64.png'),
  raw_colors=len({v[:3] for v in a.get_flattened_data() if v[3]}),clean_colors=len({v[:3] for v in b.get_flattened_data() if v[3]}),source_silhouette_preserved=True))
# Whole props are integer translations/crops with transparent clearance.
for n,box in [('Torch',(20,8,40,58)),('Banner',(15,8,48,56)),('Barrel',(13,16,49,57)),('Jar',(13,21,49,54)),('Rubble',(10,28,54,53))]:
 clean=load(R/'Previews'/f'{n}_Clean64.png').crop(box);out=load(R/'Exports'/f'{n}.png')
 assert clean.tobytes()==out.crop((2,2,out.width-2,out.height-2)).tobytes()
# Skull and bones are disconnected source components, not rectangular cuts.
recovered=Image.new('RGBA',(64,64))
for n,pos in [('Skull',(3,20)),('Bones',(29,24))]:recovered.alpha_composite(load(R/'Exports'/f'{n}.png'),pos)
assert recovered.tobytes()==load(R/'Previews/Bones_Clean64.png').tobytes()
# Chain endpoints are complete; repeat seams connect and retain empty holes.
anchor=load(R/'Exports/Chain_Anchor.png');repeat=load(R/'Exports/Chain_Repeat.png');end=load(R/'Exports/Chain_End.png')
for count in [0,1,2,6]:
 im=Image.new('RGBA',(14,39+14*count));im.alpha_composite(anchor,(0,0))
 for j in range(count):im.alpha_composite(repeat,(0,25+14*j))
 im.alpha_composite(end,(0,25+14*count));assert im.getbbox()==(2,2,12,im.height-2)
 for y in [25+14*j for j in range(count+1)]:
  assert any(im.getpixel((x,y-1))[3] and im.getpixel((x,y))[3] for x in range(14))
 im.save(R/'Previews'/f'Chain-{count}-repeats.png')
captures=[]
for mode in ['baseline','classic','classic-cast','alternate','alternate-cast']:
 a=load(R/f'Previews/Unity-{mode}-360x240.png')
 for scale in [2,3]:
  b=load(R/f'Previews/Unity-{mode}-{360*scale}x{240*scale}.png')
  box=ImageChops.difference(a.resize(b.size,Image.Resampling.NEAREST),b).convert('RGB').getbbox()
  assert box is None,(mode,scale,box);captures.append(dict(mode=mode,scale=scale,exact_nearest_neighbor=True))
base=load(R/'Previews/Unity-baseline-360x240.png')
assert base.tobytes()==load(P/'ArtSource/DungeonHybrid/Stage03/Previews/Unity-Top-360x240.png').tobytes()
for mode in ['classic-cast','alternate-cast']:
 im=load(R/f'Previews/Unity-{mode}-360x240.png')
 for box in [(108,190,360,214),(181,70,287,156)]:assert im.crop(box).tobytes()==base.crop(box).tobytes(),(mode,box)
 # Head/face rectangles stay exactly as captured before prop dressing.
 for box in [(40,126,75,151),(143,134,169,155),(265,125,289,149)]:
  assert im.crop(box).tobytes()==base.crop(box).tobytes(),(mode,'face clearance',box)
out=dict(result='PASS',native_exports=len(mods),families=families,alpha=[0,255],ase_reopen_exact=True,
 protected_files=len(protected),protected_files_unchanged=True,main_user_files=len(start['main_user_files']),main_user_files_unchanged=True,
 unity_tests_passed=1,unity_game_view_captures=15,captures=captures,baseline_exactly_matches_approved_stage03=True,
 gate_upper_region_and_fascia_pixels_unchanged=True,face_clearance_checks=True,chain_repeat_counts_checked=[0,1,2,6],
 scope='Art-only Unity Game View study with static approved cast. No production combat/device test claim.')
(R/'Diagnostics/Verification.json').write_text(json.dumps(out,indent=2)+'\n');print('PASS: native pixels, margins, chain seams, preserved source silhouettes, 15 Unity captures, approved base and protected files.')
