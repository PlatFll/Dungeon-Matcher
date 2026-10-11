"""Download recorded PixelLab frames unchanged and make review-only contact sheets."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,re,hashlib,urllib.request,concurrent.futures

root=Path(__file__).resolve().parents[1]/'ArtSource/FrostwakePlayables'
motion=root/'motion';requests=json.loads((motion/'requests.json').read_text())
completed=json.loads((motion/'completed-results.json').read_text())

def fetch(item):
    name,blocks=item;text=blocks[0]['text'];job=re.search(r'job_id: (\S+)',text)[1]
    count=requests[name]['frame_count']+1;out=motion/name/'raw';out.mkdir(parents=True,exist_ok=True)
    frames=[];records=[]
    for i in range(count):
        path=out/f'{i:02}.png'
        if not path.exists():
            with urllib.request.urlopen(f'https://api.pixellab.ai/mcp/images/{job}/download?index={i}',timeout=90) as r:path.write_bytes(r.read())
        im=Image.open(path).convert('RGBA');frames.append(im)
        records.append(dict(frame=i,sha256=hashlib.sha256(path.read_bytes()).hexdigest(),size=im.size,bounds=im.getbbox(),
            colors=len({p[:3] for p in im.get_flattened_data() if p[3]}),alpha=sorted(set(im.getchannel('A').get_flattened_data()))))
    w,h=frames[0].size;cols=5;scale=3;rows=(count+cols-1)//cols
    sheet=Image.new('RGB',(w*scale*cols,(h*scale+22)*rows),'#303238');d=ImageDraw.Draw(sheet)
    previews=[]
    for i,im in enumerate(frames):
        big=im.resize((w*scale,h*scale),Image.Resampling.NEAREST);x=(i%cols)*w*scale;y=(i//cols)*(h*scale+22)
        sheet.paste(big,(x,y+22),big);d.text((x+8,y+5),f'{name} {i}',fill='white')
        p=Image.new('RGB',(w*scale,h*scale),'#303238');p.paste(big,(0,0),big);previews.append(p)
    sheet.save(motion/name/'raw-review.png')
    previews[0].save(motion/name/'raw-review.gif',save_all=True,append_images=previews[1:],duration=130 if name.endswith('Idle') else 90,loop=0)
    (motion/name/'raw-checks.json').write_text(json.dumps(records,indent=2))
    return dict(name=name,job=job,frames=count,size=[w,h])
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
    results=list(pool.map(fetch,completed.items()))
print(json.dumps(results,indent=2))
