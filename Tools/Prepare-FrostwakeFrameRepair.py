"""Pack unchanged native UI source and blank output cells for PixelLab exact edits."""
from pathlib import Path
from PIL import Image
import base64,json
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake'
sheet=Image.new('RGBA',(512,128));sheet.paste(Image.open(root/'environment/Button.png'),(0,0))
sheet.save(root/'theme/frame-repair-input.png')
ops=[]
def copy(box,at):
    ops.append(dict(operation='copy_region',layer='image',frames=[1],source_layer='image',source_frame=1,region=box,at=at,reason='Assemble the inspected timber and iron border at native pixel scale into the existing Unity frame dimensions; no resizing.'))
# 80x80 L corner: top/left arms are 16 source pixels thick.
copy([0,0,15,15],[128,0]);copy([16,0,79,15],[144,0])
for y in range(16,80,16):copy([0,12,15,27],[128,y])
copy([16,0,79,15],[128,96])
# 144x32 energy frame with an actual transparent 120x8 track opening.
for sx,dx in [(0,256),(84,388)]:
    copy([sx,0,sx+11,11],[dx,0]);copy([sx,28,sx+11,39],[dx,20]);copy([sx,16,sx+11,23],[dx,12])
for x in range(12,132,24):
    copy([24,0,47,11],[256+x,0]);copy([24,28,47,39],[256+x,20])
(root/'theme/frame-repair-request.json').write_text(json.dumps(dict(image='data:image/png;base64,'+base64.b64encode((root/'theme/frame-repair-input.png').read_bytes()).decode(),changes=ops),indent=2))
print('Packed source and 23 guarded native copy operations for PixelLab.')
