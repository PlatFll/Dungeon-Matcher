from pathlib import Path
from PIL import Image
import json,hashlib,collections
S=Path(__file__).resolve().parents[1]
W=S/'QA'
report=[]
for spec in json.loads((S/'Inputs/Layouts.json').read_text()):
 name=spec['name'];a=Image.open(S/f'Exports/{name}.png').convert('RGBA');b=Image.open(S/f'Roundtrip/{name}.png').convert('RGBA')
 assert a.tobytes()==b.tobytes(),name
 assert {c[3] for c in a.get_flattened_data()}<={0,255},name
 x=4 if spec['side']=='Left' else 103
 assert all(a.getpixel((x,y))[3]==255 for y in range(a.height)),name
 pts={(i%a.width,i//a.width) for i,c in enumerate(a.get_flattened_data()) if c[3]};start=next(iter(pts));seen={start};q=[start]
 for xx,yy in q:
  for dx,dy in [(1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1)]:
   v=(xx+dx,yy+dy)
   if v in pts and v not in seen:seen.add(v);q.append(v)
 assert len(seen)==len(pts),(name,'Disconnected pixels',len(pts)-len(seen))
 report.append({'name':name,'size':a.size,'opaque_colors':len({c for c in a.get_flattened_data() if c[3]}),'binary_alpha':True,'connected_components':1,'edge_coverage':'full height','libresprite_reopen_exact':True,'sha256':hashlib.sha256((S/f'Exports/{name}.png').read_bytes()).hexdigest()})

(S/'QA/Native.json').write_text(json.dumps(report,indent=2));print('PASS: 10 native LibreSprite assets, exact reopen, binary alpha, one connected silhouette each, full-height edge coverage.')
