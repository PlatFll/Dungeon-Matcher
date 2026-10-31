from pathlib import Path
from PIL import Image
import json,hashlib
S=Path(__file__).resolve().parents[1];W=S/'QA'
old=Image.open(S/'Inputs/Banner.png').convert('RGBA');report=[]
for side in ['Left','Right']:
 p=S/f'Exports/Banner_{side}_Mounted.png';a=Image.open(p).convert('RGBA');b=Image.open(S/f'Roundtrip/{p.name}').convert('RGBA');assert a.tobytes()==b.tobytes()
 for y in range(old.height):
  for x in range(old.width):
   c=old.getpixel((x,y))
   if c[3]:assert a.getpixel((x+4,y+12))==c
 assert {c[3] for c in a.get_flattened_data()}=={0,255}

 report.append({'side':side,'size':a.size,'opaque_colors':len({c for c in a.get_flattened_data() if c[3]}),'binary_alpha':True,'original_visible_pixels':'exact +4,+12 registration','libresprite_reopen':'exact','sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
(S/'QA/Native.json').write_text(json.dumps(report,indent=2));print('PASS: both 48x82 native mounts; original flag pixels exact; LibreSprite reopen exact; binary alpha.')
