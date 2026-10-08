"""Fetch recorded PixelLab outputs and retain unchanged native frames for review."""
import argparse
import hashlib
import json
import urllib.request
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
p = argparse.ArgumentParser()
p.add_argument('manifest', type=Path)
args = p.parse_args()
jobs = json.loads(args.manifest.read_text(encoding='utf-8'))
for job in jobs:
    output = ROOT / job['output']
    output.mkdir(parents=True, exist_ok=True)
    frames, records = [], []
    for i, url in enumerate(job['urls']):
        if not url.startswith('https://api.pixellab.ai/mcp/images/'):
            raise ValueError('Only recorded PixelLab image outputs are accepted')
        path = output / f'{i:02}.png'
        if not path.exists():
            with urllib.request.urlopen(url, timeout=90) as response:
                path.write_bytes(response.read())
        im = Image.open(path).convert('RGBA')
        frames.append(im)
        records.append(dict(file=path.name, size=im.size, bounds=im.getbbox(),
                            colors=len(set(c[:3] for c in im.getdata() if c[3])),
                            alpha=sorted(set(im.getchannel('A').getdata())),
                            sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    w, h = frames[0].size
    sheet = Image.new('RGBA',(w*len(frames),h))
    cols = min(7,len(frames)); rows=(len(frames)+cols-1)//cols
    contact = Image.new('RGB',(cols*w,rows*(h+12)),'#343941')
    d = ImageDraw.Draw(contact)
    for i, im in enumerate(frames):
        if im.size != (w,h): raise ValueError('Inconsistent native canvas')
        sheet.paste(im,(w*i,0))
        x,y=(i%cols)*w,(i//cols)*(h+12)
        contact.paste(im,(x,y),im);d.text((x,y+h),str(i),fill='white')
    sheet.save(output/'sheet.png')
    contact.resize((contact.width*3,contact.height*3),Image.Resampling.NEAREST).save(output/'contact.png')
    (output/'technical.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(output=str(output),frames=len(frames),size=[w,h],
                         colors=[r['colors'] for r in records])))
