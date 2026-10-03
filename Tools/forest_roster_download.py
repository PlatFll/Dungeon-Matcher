"""Download returned native PixelLab assets, retaining raw bytes and job provenance."""
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
import json, urllib.request
ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'ArtSource/Forest/RosterProduction'
jobs=json.loads((ART/'results.json').read_text(encoding='utf-8'))
tasks=[];seen=set()
for job in jobs:
    key=(job['name'],job['state'])
    duplicate=key in seen
    seen.add(key)
    result=job['data']
    if not isinstance(result,dict) or result.get('status')!='completed':continue
    for i,url in enumerate(result.get('download_urls',[result.get('download_url')])):
        if not url:continue
        folder=ART/'Raw'/job['name']/job['state'] if not duplicate else ART/'UnusedDuplicates'/job['job']
        path=folder/f'{i:02}.png'
        if not path.exists(): tasks.append((url,path))
def download(task):
    url,path=task;path.parent.mkdir(parents=True,exist_ok=True)
    with urllib.request.urlopen(url,timeout=90) as response: data=response.read()
    if not data.startswith(b'\x89PNG'):raise ValueError(f'Non-PNG response: {path}')
    path.write_bytes(data)
with ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(download,tasks))
print(json.dumps(dict(downloaded=len(tasks),completedJobs=sum(isinstance(j['data'],dict) and j['data'].get('status')=='completed' for j in jobs))))
