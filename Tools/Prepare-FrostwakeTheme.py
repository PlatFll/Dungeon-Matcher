"""Slice/repeat native generated materials into the existing modular UI contract."""
from pathlib import Path
from PIL import Image
import json,hashlib
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake';out=root/'theme';out.mkdir(exist_ok=True);records=[]
def crop(name,source,box):
    im=Image.open(root/'environment'/(source+'.png')).convert('RGBA');im=im.crop(box);im.save(out/(name+'.png'))
    records.append(dict(name=name,source=source,rect=box,size=im.size,operation='lossless crop'))
crop('PanelInset','PanelTimber',(14,12,114,116))
# FrameCorner (80x80), FrameEdge (64x16), EnergyFrame (144x32) come
# from PixelLab's exact native assembly; never overwrite with button crops.
crop('Floor','TallMountainPass',(160,224,224,256))
button=Image.open(root/'environment/Button.png').convert('RGBA')
def wide(name,y0,y1):
    # Existing player-frame contract is exactly 146 source pixels wide.
    im=Image.new('RGBA',(146,y1-y0));im.paste(button.crop((0,y0,12,y1)),(0,0));im.paste(button.crop((84,y0,96,y1)),(134,0))
    band=button.crop((12,y0,84,y1))
    for x in range(12,134,72):im.paste(band.crop((0,0,min(72,134-x),y1-y0)),(x,0))
    im.save(out/(name+'.png'));records.append(dict(name=name,source='Button',operation='native end caps and repeated center',size=im.size))
wide('PlayerTop',0,12);wide('PlayerBottom',28,40)
# PlayerMiddle is the reviewed PixelLab alpha correction; do not refill its opening.
for path in out.glob('*.png'):
    im=Image.open(path).convert('RGBA');records.append(dict(file=path.name,size=im.size,colors=len({p for p in im.get_flattened_data() if p[3]}),alpha=sorted(set(im.getchannel('A').get_flattened_data())),sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
(out/'manifest.json').write_text(json.dumps(records,indent=2));print('Native modular slices prepared; no pixel painting or resampling.')
