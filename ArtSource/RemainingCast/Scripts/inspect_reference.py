from pathlib import Path
import sys,json
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase, sheet
fs,ds=read_ase(ROOT/'Originals/RattleBones_FluidIdle.aseprite')
sheet(fs,ds,'UNCHANGED open Rattlebones reference',ROOT/'Review/Rattlebones_reference_frames.png')
print('Rattlebones timings:',ds)
manifest=json.loads((ROOT/'SourceManifest.json').read_text())
for name,v in manifest.items():
 if name.startswith('Rattle'):continue
 print(name, ' '.join(f'{c}:{n}' for c,n in v['colors'].items() if n>=10))
