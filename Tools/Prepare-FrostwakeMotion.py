"""Pad approved native drawings for motion, without scaling or repainting."""
from pathlib import Path
from PIL import Image
import json,hashlib
root=Path(__file__).resolve().parents[1];art=root/'ArtSource/Frostwake';out=art/'approved';out.mkdir(exist_ok=True)
checks=json.loads((art/'technical-checks.json').read_text());manifest=[]
for row in checks:
    name=row['id'];p=art/'palette-clean'/(name+'.png')
    if not p.exists():p=art/'candidates'/(name+'.png')
    assert hashlib.sha256(p.read_bytes()).hexdigest()==row['sha256']
    im=Image.open(p).convert('RGBA');b=im.getbbox();crop=im.crop(b)
    canvas=Image.new('RGBA',(128,104));offset=((128-crop.width)//2,100-crop.height);canvas.paste(crop,offset);canvas.save(out/(name+'.png'))
    colors=sorted({p for p in im.get_flattened_data() if p[3]});pal=Image.new('RGBA',(len(colors),1));pal.putdata(colors);pal.save(out/(name+'-palette.png'))
    manifest.append(dict(id=name,approved_source=str(p.relative_to(root)),approved_sha256=row['sha256'],approval='User explicitly approved all eleven for animation, 2026-10-11',motion_canvas=[128,104],source_crop=b,offset=offset,scaling=1,sha256=hashlib.sha256((out/(name+'.png')).read_bytes()).hexdigest()))
(out/'manifest.json').write_text(json.dumps(manifest,indent=2));print('11 approved stills padded losslessly to 128x104 with a common four-pixel floor.')
