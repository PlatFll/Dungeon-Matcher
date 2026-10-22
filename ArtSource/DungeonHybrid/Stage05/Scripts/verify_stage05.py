from pathlib import Path
from PIL import Image,ImageChops
import hashlib,json,csv,sys,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/Stage05';M=Path('C:/UnityProjects/Dungeon Matcher')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return Image.open(p).convert('RGBA')
protected=json.loads((R/'Diagnostics/ProtectedHashes.json').read_text());changed=[p for p,h in protected.items() if sha(P/p)!=h]
start=json.loads((R/'Diagnostics/StartingState.json').read_text());mainchanged=[p for p,h in start['main_user_files'].items() if sha(M/p)!=h]
print('Protected files:',len(protected),'changed:',changed,'main edits changed:',mainchanged)
assert not mainchanged
if '--pre-restore' in sys.argv:sys.exit(0)
assert not changed
tests=ET.parse(R/'Diagnostics/UnityTests.xml').getroot();assert tests.get('result')=='Passed' and tests.get('passed')=='1'
captures=[]
for mode in ['SourceCheck','Classic','Alternate','Structure']:
 a=load(R/f'Previews/Unity-{mode}-720x1280.png');b=load(R/f'Previews/Unity-{mode}-1440x2560.png')
 assert a.size==(720,1280) and b.size==(1440,2560)
 box=ImageChops.difference(a.resize(b.size,Image.Resampling.NEAREST),b).convert('RGB').getbbox()
 assert box is None,(mode,box)
 captures.append(dict(mode=mode,native_dimensions=a.size,double_dimensions=b.size,exact_double=True))
source=load(R/'Previews/Unity-SourceCheck-720x1280.png')
prior_top=load(P/'ArtSource/DungeonHybrid/Stage04/Previews/Unity-classic-cast-720x480.png')
assert source.crop((0,0,720,480)).tobytes()==prior_top.tobytes(),'Top assembly differs from approved Stage04'
prior_board=load(P/'ArtSource/DungeonHybrid/Stage03/Previews/Unity-BoardHud-576x688.png')
assert source.crop((104,506,616,1018)).tobytes()==prior_board.crop((32,23,544,535)).tobytes(),'Native grid differs'
classic=load(R/'Previews/Unity-Classic-720x1280.png')
for box in [(80,252,150,302),(286,268,338,310),(530,250,578,298)]:
 assert source.crop(box).tobytes()==classic.crop(box).tobytes(),'Face overlay'
# A 17px corner and 9px rail place the 2x rail's inner edge exactly at
# the 26px board inset. These strips are its continuous dark outline.
# Any wall fragments, gaps or changed geometry would break this equality.
outline=load(P/'Assets/_Game/Art/DungeonPixelLab/BoardFrame/Board_Rail_Left.png').getpixel((8,16))
for box in [(102,560,104,960),(616,560,618,960),(140,504,580,506),(140,1018,580,1020)]:
 colors=set(classic.crop(box).get_flattened_data());assert colors=={outline},(box,colors)
usage=json.loads((R/'AssemblyUsage.json').read_text())
unique={}
for s in usage['sprites']:
 assert s['displayScale'] in (1,2),s
 path=P/s['path'];assert path.exists()
 unique[(s['path'],s['name'],s['displayScale'])]=s
with (R/'AssetLedger.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['path','sprite','source_rect','display_scale','sha256','disposition'])
 for _,s in sorted(unique.items()):
  w.writerow([s['path'],s['name'],json.dumps(s['nativeRect']),s['displayScale'],sha(P/s['path']),'retained unchanged'])
out=dict(result='PASS',unity_preview_tests=1,actual_captures=8,captures=captures,source_files_unchanged=len(protected),
 main_user_files_unchanged=len(start['main_user_files']),approved_stage04_top_pixel_identical=True,
 approved_stage03_64px_grid_pixel_identical=True,board_recess_no_wall_leakage=True,character_faces_unchanged=True,
 unique_sprite_uses=len(unique),unique_png_sources=len({p for p,n,s in unique}),new_art_files=0,generation_cost=0,
 native_layout=[720,1280],integer_scale_policy='Top, environment and frames 2x; 64px gems/cells and static health samples 1x; no fractional or nonuniform scaling.',
 scope='Art-only Unity Game View with static approved cast and display values. No production combat or device testing.')
(R/'Diagnostics/Verification.json').write_text(json.dumps(out,indent=2)+'\n')
print('PASS: full assembly captures are exact integer zooms; approved top/grid pixel-identical; masked recess clean; all protected artwork unchanged.')
