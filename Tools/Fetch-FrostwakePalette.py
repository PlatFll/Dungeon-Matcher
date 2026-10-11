"""Retrieve shared-palette native cels; validate size, alpha and palette membership."""
from pathlib import Path
from urllib.request import urlopen
from concurrent.futures import ThreadPoolExecutor
from PIL import Image
import json,hashlib
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake';jobs=json.loads((root/'motion-palette-queue.json').read_text());tasks=[]
for job in jobs:
    result=job.get('completed')
    if not result or result.get('status')!='completed':continue
    urls=result['download_urls'];assert len(urls)==len(job['paths']),(job['key'],len(urls),len(job['paths']))
    for name,url in zip(job['paths'],urls):
        source=root/name;target=source.parent.parent/'palette'/source.name
        tasks.append((source,target,url))
def fetch(item):
    source,target,url=item;target.parent.mkdir(exist_ok=True)
    if not target.exists():
        with urlopen(url,timeout=120) as response:target.write_bytes(response.read())
    a=Image.open(source).convert('RGBA');b=Image.open(target).convert('RGBA');assert a.size==b.size==(128,104)
    assert a.getchannel('A').tobytes()==b.getchannel('A').tobytes(),str(target)+' alpha changed'
    palette=Image.open(root/'approved'/(source.parents[2].name+'-palette.png')).convert('RGBA')
    colors={p for p in palette.get_flattened_data() if p[3]};actual={p for p in b.get_flattened_data() if p[3]}
    assert actual<=colors,(str(target),actual-colors)
    return dict(path=str(target.relative_to(root)),size=b.size,colors=len(actual),bounds=b.getbbox(),alpha=sorted(set(b.getchannel('A').get_flattened_data())),sha256=hashlib.sha256(target.read_bytes()).hexdigest())
with ThreadPoolExecutor(max_workers=6) as pool:checks=list(pool.map(fetch,tasks))
(root/'motion-palette-checks.json').write_text(json.dumps(checks,indent=2));print(f'{len(checks)} frames verified: exact alpha, 128x104, approved palettes.')
