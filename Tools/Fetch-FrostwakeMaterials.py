"""Retrieve free PixelLab material correction and extract native cells."""
from pathlib import Path
from PIL import Image
import urllib.request,json,hashlib
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake/theme'
job='79b74f3f-6da1-43a5-bac8-44bd5e6c0e8f'
path=root/'material-repair-output.png'
if not path.exists():
    urllib.request.urlretrieve(f'https://api.pixellab.ai/mcp/pixel-tools/{job}/image.png',path)
im=Image.open(path)
boxes={'QuietRock':(0,0,128,128),'QuietTimber':(142,12,242,116),'PlayerMiddle':(256,0,402,4)}
for name,box in boxes.items(): im.crop(box).save(root/(name+'.png'))
(root/'material-repair-receipt.json').write_text(json.dumps(dict(image_id=job,cost=0,
    native_extractions=boxes,sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
    decisions='Four-shade inspected material maps and 488 guarded center-alpha edits; see material-repair-decisions.json.'),indent=2))
print('Three native material assets extracted without scaling.')
