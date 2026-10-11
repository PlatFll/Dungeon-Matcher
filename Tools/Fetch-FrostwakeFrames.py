"""Retrieve PixelLab native frame assembly and extract its unchanged cells."""
from pathlib import Path
from PIL import Image
import urllib.request,json,hashlib
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake/theme'
job='88d7714d-570a-4dae-b01c-cb3193938ef0'
urllib.request.urlretrieve(f'https://api.pixellab.ai/mcp/pixel-tools/{job}/image.png',root/'frame-repair-output.png')
im=Image.open(root/'frame-repair-output.png')
boxes={'FrameCorner':(128,0,208,80),'FrameEdge':(128,96,192,112),'EnergyFrame':(256,0,400,32),'WavePlaque':(256,64,384,96)}
for name,box in boxes.items():im.crop(box).save(root/(name+'.png'))
(root/'frame-repair-receipt.json').write_text(json.dumps(dict(image_id=job,parent_id='d91014f6-c73e-4851-a052-975c53543f11',cost=0,native_extractions=boxes,sha256=hashlib.sha256((root/'frame-repair-output.png').read_bytes()).hexdigest()),indent=2))
print('Four native frame assets extracted:',boxes)
