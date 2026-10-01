"""Download explicitly returned PixelLab frame URLs and create review contact sheets/GIFs."""
import concurrent.futures, json, urllib.request
from pathlib import Path
from PIL import Image, ImageDraw
root=Path(__file__).resolve().parents[1]
jobs=json.loads((root/'ArtSource/EnemyAttacks/manifest.json').read_text())['downloads']
def fetch(job):
    out=root/job['directory'];out.mkdir(parents=True,exist_ok=True)
    for i in range(job['frames']):
        dest=out/f'{i:02}.png'
        if not dest.exists():
            with urllib.request.urlopen(job['url']+str(i),timeout=60) as response:
                data=response.read()
            if not data.startswith(b'\x89PNG\r\n\x1a\n'): raise ValueError(f'Invalid PNG: {job["name"]}/{i}')
            dest.write_bytes(data)
    frames=[Image.open(out/f'{i:02}.png').convert('RGBA') for i in range(job['frames'])]
    w,h=frames[0].size
    cols=min(8,len(frames)); rows=(len(frames)+cols-1)//cols
    review=Image.new('RGB',(w*4*cols,(h*4+30)*rows),'#191624');draw=ImageDraw.Draw(review)
    for i,frame in enumerate(frames):
        large=frame.resize((w*4,h*4),Image.Resampling.NEAREST)
        x=(i%cols)*w*4;y=(i//cols)*(h*4+30)
        review.paste(large,(x,y+24),large);draw.text((x+4,y+5),str(i),fill='white')
    review.save(out.parent/(job['name']+'_frames.png'))
    previews=[]
    for frame in frames:
        canvas=Image.new('RGB',(w*4,h*4),'#191624');large=frame.resize((w*4,h*4),Image.Resampling.NEAREST);canvas.paste(large,(0,0),large);previews.append(canvas)
    previews[0].save(out.parent/(job['name']+'_review.gif'),save_all=True,append_images=previews[1:],duration=job.get('durations',[100]*len(frames)),loop=0,disposal=2)
    return job['name'],len(frames),[w,h]
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
    for result in pool.map(fetch,jobs): print(result)
