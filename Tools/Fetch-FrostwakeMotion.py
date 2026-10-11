"""Fetch completed PixelLab motion into immutable raw frame folders."""
from pathlib import Path
import json,urllib.request,concurrent.futures
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake'
tasks=[]
for clip in json.loads((root/'motion-queue.json').read_text()):
    if not clip.get('completed'):continue
    folder=root/'motion'/clip['id']/clip['state']/'raw';folder.mkdir(parents=True,exist_ok=True)
    for i,url in enumerate(clip['completed']['download_urls']):
        assert url.startswith('https://api.pixellab.ai/')
        p=folder/f'{i:02}.png'
        if not p.exists():tasks.append((url,p))
def fetch(task):
    url,p=task;urllib.request.urlretrieve(url,p);return p
with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool:
    for p in pool.map(fetch,tasks):pass
print(f'Downloaded {len(tasks)} exact frame PNGs.')
